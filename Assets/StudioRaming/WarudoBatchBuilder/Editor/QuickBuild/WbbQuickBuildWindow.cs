using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public class WbbQuickBuildWindow : EditorWindow
    {
        private const float WindowWidth = WbbSize.Quick.WindowWidth;
        private const float RowHeightEstimate = WbbSize.Quick.RowHeightEstimate;
        private const float ThumbSize = WbbSize.Quick.ThumbSize;
        private const int EnterDelayRepaints = 2;
        private enum RowState
        {
            Ok,
            Missing,
            Empty,
            Duplicate,
        }

        private class Row
        {
            public GameObject source;
            public string controlName = "";
            public string input = "";
            public string rememberedName = "";
            public bool exactName;
            public Texture2D thumb;
            public bool thumbPending = true;
            public bool thumbFromSceneView;
            public string thumbError = "";
            public WbbQuickBuild.SourceReport report;
            public bool inactive;
            public string sceneName = "";
            public string clean = "";
            public string finalName = "";
            public RowState state = RowState.Ok;
            public bool outputExists;
        }

        private static string L(string key)
        {
            return WbbLang.T(key);
        }

        private static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }

        private static GUIContent LC(string key)
        {
            return WbbLang.C(key);
        }

        private readonly List<Row> rows = new List<Row>();
        private WbbExportType type = WbbExportType.Prop;
        private WbbJob settings;
        private string outputDir = "";
        private string stagingFolder = "";
        private string note = "";
        private string blocking = "";
        private bool prependScene;
        private bool appendDate;
        private bool autoNumber;
        private string sceneExample = "";
        private readonly Dictionary<string, bool> existsCache = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
        private Vector2 scroll;
        private bool validationDirty = true;
        private bool canBuild;
        private bool focusPending = true;
        private bool pendingCapture;
        private bool pendingBuild;
        private int buildDelayRepaints;
        private GUIStyle statusStyle;
        private GUIStyle nameButtonStyle;
        private bool stylesDark;
        private GUIContent[] typeContents;
        private int typeContentsVersion = -1;
        private GUIContent prependSceneContent;
        private GUIContent appendDateContent;
        private GUIContent autoNumberContent;
        private string toggleContentKey = "";
        public static void Open(WbbExportType type, IList<GameObject> objects, IDictionary<GameObject, string> presetNames, string note)
        {
            var alive = new List<GameObject>();
            if (objects != null)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] != null && !alive.Contains(objects[i]))
                        alive.Add(objects[i]);
                }
            }

            objects = alive;
            if (objects.Count == 0)
            {
                EditorUtility.DisplayDialog(L("Quick.WindowTitle"), L("Quick.NoTargets"), L("Common.Ok"));
                return;
            }

            if (WbbBuildRunner.AnyRunning)
            {
                EditorUtility.DisplayDialog(L("Quick.WindowTitle"), L("Runner.OtherBuildRunning"), L("Common.Ok"));
                return;
            }

            WbbQuickBuildWindow[] open = Resources.FindObjectsOfTypeAll<WbbQuickBuildWindow>();
            for (int i = 0; i < open.Length; i++)
            {
                if (open[i] != null)
                    open[i].Close();
            }

            var window = CreateInstance<WbbQuickBuildWindow>();
            window.Init(type, objects, presetNames, note);
            window.minSize = new Vector2(WbbSize.Quick.MinWidth, WbbSize.Quick.MinHeight);
            window.ShowUtility();
            float height = Mathf.Clamp(WbbSize.Quick.ChromeHeightEstimate + window.rows.Count * RowHeightEstimate, WbbSize.Quick.OpenMinHeight, WbbSize.Quick.OpenMaxHeight);
#if UNITY_2020_1_OR_NEWER
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            window.position = new Rect(main.x + (main.width - WindowWidth) * 0.5f,
                main.y + Mathf.Max(0f, (main.height - height) * WbbSize.Quick.OpenTopRatio), WindowWidth, height);
#else
            Rect current = window.position;
            window.position = new Rect(current.x, current.y, WindowWidth, height);
#endif
            window.Focus();
        }

        private void Init(WbbExportType newType, IList<GameObject> objects, IDictionary<GameObject, string> presetNames, string newNote)
        {
            type = newType == WbbExportType.Character ? WbbExportType.Character : WbbExportType.Prop;
            note = newNote == null ? "" : newNote;
            rows.Clear();
            try
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    GameObject go = objects[i];
                    if (go == null)
                        continue;
                    EditorUtility.DisplayProgressBar(L("Quick.WindowTitle"), L("Quick.Inspecting", go.name), (float)i / objects.Count);
                    var row = new Row();
                    row.source = go;
                    row.controlName = "WbbQuickName" + i;
                    string preset;
                    if (presetNames != null && presetNames.TryGetValue(go, out preset) && !string.IsNullOrEmpty(preset))
                    {
                        row.rememberedName = WbbUtil.SanitizeModName(preset);
                        row.exactName = true;
                    }
                    else
                    {
                        row.rememberedName = WbbQuickBuildNames.Find(WbbQuickBuild.SourceKey.Of(go));
                    }

                    row.input = row.rememberedName.Length > 0 ? row.rememberedName : WbbUtil.SanitizeModName(go.name);
                    row.report = WbbQuickBuild.InspectSource(go);
                    row.inactive = !go.activeInHierarchy;
                    rows.Add(row);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            prependScene = EditorPrefs.GetBool(WbbQuickBuild.PrependScenePrefKey, false);
            appendDate = EditorPrefs.GetBool(WbbQuickBuild.AppendDatePrefKey, false);
            autoNumber = EditorPrefs.GetBool(WbbQuickBuild.AutoNumberPrefKey, false);
            ReloadSettings();
            UpdateTitle();
            pendingCapture = true;
            focusPending = true;
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            EditorApplication.delayCall += CloseIfEmpty;
        }

        private void CloseIfEmpty()
        {
            if (this != null && rows.Count == 0)
                Close();
        }

        private void OnDisable()
        {
            for (int i = 0; i < rows.Count; i++)
                DestroyThumb(rows[i]);
        }

        private void OnFocus()
        {
            WbbLang.OnWindowFocus();
            if (rows.Count > 0)
                ReloadSettings();
        }

        private void OnHierarchyChange()
        {
            validationDirty = true;
            Repaint();
        }

        private void Update()
        {
            if (pendingCapture)
            {
                pendingCapture = false;
                CaptureAll();
                Repaint();
            }

            if (pendingBuild)
            {
                pendingBuild = false;
                existsCache.Clear();
                Validate();
                if (canBuild)
                    DoBuild();
                else
                    Repaint();
            }
        }

        private void ReloadSettings()
        {
            WbbWindow.FlushOpenWindowPrefs();
            settings = WbbBuildSection.LoadSavedJob(type);
            outputDir = string.IsNullOrEmpty(settings.outputDirectory) ? WbbUtil.DefaultOutputPath(type) : WbbUtil.NormalizePath(settings.outputDirectory);
            stagingFolder = WbbUtil.ResolveBuildFolder(settings, type);
            string error;
            blocking = "";
            if (!WbbExportSettings.ValidateAvailable(out error))
                blocking = L("Runner.NoUMod") + "\n" + L("Runner.NoUMod.hint");
            else if (WbbExportSettings.Locate(out error) == null)
                blocking = L("Runner.NoExportSettings") + "\n" + L("Runner.NoExportSettings.hint");
            existsCache.Clear();
            validationDirty = true;
            Repaint();
        }

        private void Validate()
        {
            validationDirty = false;
            var requests = new List<WbbQuickBuild.NameRequest>(rows.Count);
            sceneExample = null;
            for (int i = 0; i < rows.Count; i++)
            {
                Row row = rows[i];
                row.clean = WbbUtil.SanitizeModName(row.input == null ? "" : row.input.Trim());
                row.sceneName = SceneNameOf(row.source);
                if (row.source != null)
                {
                    if (sceneExample == null)
                        sceneExample = row.sceneName;
                    else if (!string.Equals(sceneExample, row.sceneName, System.StringComparison.Ordinal))
                        sceneExample = "";
                }

                requests.Add(new WbbQuickBuild.NameRequest { baseName = row.source == null ? "" : row.clean, exact = row.exactName, sceneName = row.sceneName, });
            }

            if (sceneExample == null)
                sceneExample = "";
            WbbQuickBuild.NameResult[] results = WbbQuickBuild.ResolveNames(requests, prependScene, appendDate, autoNumber, WbbUtil.DateSuffix(), OutputFileExists);
            canBuild = rows.Count > 0 && blocking.Length == 0 && !WbbBuildRunner.AnyRunning;
            for (int i = 0; i < rows.Count; i++)
            {
                Row row = rows[i];
                row.finalName = results[i].finalName;
                row.outputExists = results[i].overwrites;
                if (row.source == null)
                    row.state = RowState.Missing;
                else if (row.clean.Length == 0)
                    row.state = RowState.Empty;
                else if (results[i].duplicate)
                    row.state = RowState.Duplicate;
                else
                    row.state = RowState.Ok;
                if (row.state != RowState.Ok)
                    canBuild = false;
            }
        }

        private static string SceneNameOf(GameObject go)
        {
            if (go == null)
                return "";
            UnityEngine.SceneManagement.Scene scene = go.scene;
            if (!scene.IsValid() || UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(scene))
                return "";
            return string.IsNullOrEmpty(scene.path) ? "" : (scene.name ?? "");
        }

        private bool OutputFileExists(string modName)
        {
            bool exists;
            if (existsCache.TryGetValue(modName, out exists))
                return exists;
            try
            {
                exists = File.Exists(WbbUtil.ToAbsolutePath(OutputPath(modName)));
            }
            catch (System.Exception)
            {
                exists = false;
            }

            existsCache[modName] = exists;
            return exists;
        }

        private string OutputPath(string modName)
        {
            return WbbUtil.Combine(outputDir, modName + WbbUtil.ModExtension);
        }

        private void UpdateTitle()
        {
            titleContent = new GUIContent(L("Quick.WindowTitle") + " · " + WbbLang.TypeName(type));
        }

        private void CaptureAll()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                Row row = rows[i];
                DestroyThumb(row);
                row.thumbPending = false;
                row.thumbError = "";
                if (row.source == null)
                    continue;
                bool usedSceneView;
                string error;
                row.thumb = WbbThumbnail.CaptureSceneObject(row.source, WbbQuickBuild.ThumbnailSize, out usedSceneView, out error);
                row.thumbFromSceneView = usedSceneView;
                row.thumbError = row.thumb == null ? (string.IsNullOrEmpty(error) ? "?" : error) : "";
            }
        }

        private static void DestroyThumb(Row row)
        {
            if (row.thumb != null)
                DestroyImmediate(row.thumb);
            row.thumb = null;
        }

        private void RequestBuild()
        {
            if (buildDelayRepaints > 0 || pendingBuild)
                return;
            buildDelayRepaints = EnterDelayRepaints;
            Repaint();
        }

        private void DoBuild()
        {
            var targets = new List<WbbQuickBuild.Target>();
            for (int i = 0; i < rows.Count; i++)
            {
                Row row = rows[i];
                targets.Add(new WbbQuickBuild.Target { source = row.source, modName = row.finalName, rememberName = row.exactName ? "" : row.clean, thumbnail = row.thumb, });
            }

            string error;
            if (WbbQuickBuild.Start(targets, type, out error))
            {
                Close();
                return;
            }

            EditorUtility.DisplayDialog(L("Quick.WindowTitle"), error, L("Common.Ok"));
            ReloadSettings();
        }

        private void OnGUI()
        {
            if (rows.Count == 0)
                return;
            EnsureStyles();
            WbbStyles.WindowBackground(this);
            Event e = Event.current;
            if (e.type == EventType.MouseMove)
                Repaint();
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    e.Use();
                    Close();
                    GUIUtility.ExitGUI();
                }
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    RequestBuild();
                }
            }

            if (e.type == EventType.Layout && validationDirty)
                Validate();
            if (e.type == EventType.Repaint && buildDelayRepaints > 0)
            {
                buildDelayRepaints--;
                if (buildDelayRepaints == 0)
                    pendingBuild = true;
                else
                    Repaint();
            }

            WbbStyles.BeginWindowBody();
            DrawTypeBar();
            DrawNameRules();
            WbbStyles.StatusLine(note, MessageType.Info);
            WbbStyles.StatusLine(blocking, MessageType.Error);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < rows.Count; i++)
                DrawRow(rows[i]);
            EditorGUILayout.EndScrollView();
            WbbStyles.Separator();
            DrawSettingsLine();
            DrawButtons();
            WbbStyles.EndWindowBody();
            if (focusPending && e.type == EventType.Repaint)
            {
                focusPending = false;
                EditorGUI.FocusTextInControl(rows[0].controlName);
                Repaint();
            }
        }

        private void DrawTypeBar()
        {
            if (typeContents == null || typeContentsVersion != WbbLang.Version)
            {
                typeContentsVersion = WbbLang.Version;
                typeContents = new[]
                {
                    new GUIContent(WbbLang.TypeName(WbbExportType.Prop)),
                    new GUIContent(WbbLang.TypeName(WbbExportType.Character)),
                };
            }

            GUILayout.Space(WbbSize.SpaceSmall);
            int current = type == WbbExportType.Character ? 1 : 0;
            int next = WbbStyles.Segmented(current, typeContents);
            if (next != current)
            {
                type = next == 1 ? WbbExportType.Character : WbbExportType.Prop;
                UpdateTitle();
                ReloadSettings();
            }

            GUILayout.Space(WbbSize.SpaceHair);
        }

        private void DrawNameRules()
        {
            string dateSuffix = WbbUtil.DateSuffix();
            string key = WbbLang.Version + "|" + dateSuffix + "|" + sceneExample;
            if (appendDateContent == null || toggleContentKey != key)
            {
                toggleContentKey = key;
                string scene = sceneExample.Length > 0 ? sceneExample : L("Quick.SceneNameGeneric");
                prependSceneContent = new GUIContent(L("Quick.PrependScene", scene + "_"), L("Quick.PrependScene.tip"));
                appendDateContent = new GUIContent(L("Quick.AppendDate", dateSuffix), L("Quick.AppendDate.tip"));
                autoNumberContent = new GUIContent(L("Quick.AutoNumber"), L("Quick.AutoNumber.tip"));
            }

            bool nextScene = WbbStyles.ToggleLeft(prependSceneContent, prependScene);
            bool nextDate = WbbStyles.ToggleLeft(appendDateContent, appendDate);
            bool nextNumber = WbbStyles.ToggleLeft(autoNumberContent, autoNumber);
            if (nextScene != prependScene)
            {
                prependScene = nextScene;
                EditorPrefs.SetBool(WbbQuickBuild.PrependScenePrefKey, prependScene);
                validationDirty = true;
            }

            if (nextDate != appendDate)
            {
                appendDate = nextDate;
                EditorPrefs.SetBool(WbbQuickBuild.AppendDatePrefKey, appendDate);
                validationDirty = true;
            }

            if (nextNumber != autoNumber)
            {
                autoNumber = nextNumber;
                EditorPrefs.SetBool(WbbQuickBuild.AutoNumberPrefKey, autoNumber);
                validationDirty = true;
            }

            GUILayout.Space(WbbSize.SpaceHair);
        }

        private void DrawRow(Row row)
        {
            EditorGUILayout.BeginHorizontal(WbbStyles.Card);
            Rect thumbRect = GUILayoutUtility.GetRect(ThumbSize, ThumbSize, GUILayout.Width(ThumbSize), GUILayout.Height(ThumbSize));
            if (Event.current.type == EventType.Repaint)
            {
                if (row.thumb != null)
                {
                    EditorGUI.DrawTextureTransparent(thumbRect, row.thumb, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.DrawRect(thumbRect, WbbTheme.P.ThumbWell);
                    if (row.thumbPending)
                        GUI.Label(thumbRect, "…", WbbStyles.CenteredMini);
                    else
                        WbbStyles.Glyph(new Rect(thumbRect.x, thumbRect.y, thumbRect.width, WbbSize.LineHeight), UI.SRGlyph.Close);
                }
            }

            GUILayout.Space(WbbSize.SpaceMedium);
            EditorGUILayout.BeginVertical();
            string title = row.source != null ? row.source.name : L("Quick.ObjectMissing");
            if (GUILayout.Button(new GUIContent(title, L("Quick.PingTip")), nameButtonStyle) && row.source != null)
                EditorGUIUtility.PingObject(row.source);
            string next = WbbStyles.LabeledTextField(LC("Quick.ModName"), row.input, row.controlName);
            if (!string.Equals(next, row.input, System.StringComparison.Ordinal))
            {
                row.input = next;
                row.exactName = false;
                validationDirty = true;
            }

            DrawStatus(row);
            DrawWarnings(row);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatus(Row row)
        {
            switch (row.state)
            {
                case RowState.Missing:
                    ColoredLine(L("Quick.ObjectMissing"), WbbStyles.Danger);
                    return;
                case RowState.Empty:
                    ColoredLine(L("Quick.NameEmpty"), WbbStyles.Danger);
                    return;
                case RowState.Duplicate:
                    ColoredLine(L("Quick.NameDuplicate"), WbbStyles.Danger);
                    return;
            }

            bool remembered = !row.exactName && row.rememberedName.Length > 0 && string.Equals(row.clean, row.rememberedName, System.StringComparison.OrdinalIgnoreCase);
            string text = L("Quick.OutputFile", row.finalName + WbbUtil.ModExtension);
            if (row.exactName)
                text += " · " + L("Quick.NameFromHistory");
            else if (remembered)
                text += " · " + L("Quick.NameRemembered");
            text += " · " + (row.outputExists ? L("Quick.FileOverwrite") : L("Quick.FileNew"));
            Color color = row.outputExists ? WbbStyles.Warning : (remembered || row.exactName ? WbbStyles.Success : WbbStyles.Muted);
            ColoredLine(text, color, OutputPath(row.finalName));
            string typed = row.input == null ? "" : row.input.Trim();
            if (!string.Equals(typed, row.clean, System.StringComparison.Ordinal))
                ColoredLine(L("Quick.NameSanitized", row.clean), WbbStyles.Muted);
            if (prependScene && !row.exactName && row.sceneName.Length == 0)
                ColoredLine(L("Quick.SceneNameUnavailable"), WbbStyles.Muted);
        }

        private void DrawWarnings(Row row)
        {
            if (row.source == null)
                return;
            if (row.thumbPending)
                ColoredLine(L("Quick.ThumbPending"), WbbStyles.Muted);
            else if (row.thumb == null)
                ColoredLine(L("Quick.ThumbFailed", row.thumbError), WbbStyles.Danger);
            else if (!row.thumbFromSceneView)
                ColoredLine(L("Quick.ThumbAuto"), WbbStyles.Warning);
            if (row.inactive)
                ColoredLine(L("Quick.Inactive"), WbbStyles.Warning);
            WbbQuickBuild.SourceReport report = row.report;
            if (report == null)
                return;
            if (report.externalReferences > 0)
                ColoredLine(L("Quick.ExternalRefs", report.externalReferences, report.externalSample), WbbStyles.Warning);
            if (report.missingScripts > 0)
                ColoredLine(L("Quick.MissingScripts", report.missingScripts), WbbStyles.Warning);
            if (report.truncated)
                ColoredLine(L("Quick.InspectTruncated"), WbbStyles.Muted);
        }

        private void DrawSettingsLine()
        {
            string version = string.IsNullOrEmpty(settings.modVersion) ? "1.0.0" : settings.modVersion;
            string author = string.IsNullOrEmpty(settings.modAuthor) ? L("Build.NoAuthorUnset") : settings.modAuthor;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            ColoredLine(L("Quick.Settings", outputDir, version, author), WbbStyles.Muted);
            if (!string.IsNullOrEmpty(stagingFolder))
                ColoredLine(L("Quick.Staging", stagingFolder), WbbStyles.Muted);
            EditorGUILayout.EndVertical();
            if (WbbStyles.PlainButton(LC("Quick.OpenSettings"), GUILayout.Width(WbbSize.Quick.OpenSettingsButton)))
            {
                WbbWindow window = WbbWindow.Open();
                if (window != null)
                    window.SwitchTab(WbbWindow.TabBuild);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawButtons()
        {
            GUILayout.Space(WbbSize.SpaceSmall);
            EditorGUILayout.BeginHorizontal();
            if (WbbStyles.PlainButton(LC("Quick.Recapture"), GUILayout.Height(WbbSize.ActionButton)))
            {
                for (int i = 0; i < rows.Count; i++)
                    rows[i].thumbPending = true;
                pendingCapture = true;
            }

            GUILayout.FlexibleSpace();
            if (WbbStyles.PlainButton(L("Common.Cancel"), GUILayout.Width(WbbSize.Quick.CancelButton), GUILayout.Height(WbbSize.ActionButton)))
            {
                Close();
                GUIUtility.ExitGUI();
            }

            using (new EditorGUI.DisabledScope(!canBuild || buildDelayRepaints > 0 || pendingBuild))
            {
                string label = rows.Count > 1 ? L("Quick.BuildMany", rows.Count) : L("Quick.Build");
                if (WbbStyles.PrimaryButton(new GUIContent(label, L("Quick.Build.tip")), GUILayout.Width(WbbSize.Quick.BuildButton), GUILayout.Height(WbbSize.ActionButton)))
                    RequestBuild();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(WbbSize.SpaceMedium);
        }

        private readonly GUIContent lineContent = new GUIContent();
        private void ColoredLine(string text, Color color, string tooltip = "")
        {
            statusStyle.normal.textColor = color;
            lineContent.text = text;
            lineContent.tooltip = tooltip;
            GUILayout.Label(lineContent, statusStyle);
        }

        private void EnsureStyles()
        {
            bool dark = EditorGUIUtility.isProSkin;
            if (statusStyle != null && stylesDark == dark)
                return;
            stylesDark = dark;
            statusStyle = new GUIStyle(WbbStyles.MiniInfo)
            {
                richText = false
            };
            nameButtonStyle = new GUIStyle(WbbStyles.RowLabelBold)
            {
                clipping = TextClipping.Clip
            };
        }
    }
}
