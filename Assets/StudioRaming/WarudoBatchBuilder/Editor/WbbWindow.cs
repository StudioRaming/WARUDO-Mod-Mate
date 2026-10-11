using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public class WbbWindow : EditorWindow
    {
        public const int TabBuild = 0;
        public const int TabLibrary = 1;
        public const int TabHistory = 2;
        private const string WinPfx = WbbUtil.PrefPrefix + "Win.";
        private WbbBuildSection build;
        private WbbLibrarySection library;
        private WbbHistorySection history;
        private int tab;
        private int pendingTab = -1;
        private string tabError = "";
        private int tabErrorTab = -1;
        private GUIContent[] tabContents;
        private int tabContentsVersion = -1;
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

        [MenuItem("StudioRaming/" + WbbProduct.Name)]
        public static WbbWindow Open()
        {
            var window = GetWindow<WbbWindow>(WbbProduct.Name);
            window.minSize = new Vector2(WbbSize.Window.MinWidth, WbbSize.Window.MinHeight);
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            tabContents = null;
            tabContentsVersion = -1;
            titleContent = new GUIContent(WbbProduct.Name);
            minSize = new Vector2(WbbSize.Window.MinWidth, WbbSize.Window.MinHeight);
            wantsMouseMove = true;
            WbbDatabase.EnsureLoaded();
            build = new WbbBuildSection(this);
            library = new WbbLibrarySection(this);
            history = new WbbHistorySection(this);
            build.OnEnable();
            library.OnEnable();
            history.OnEnable();
            tab = Mathf.Clamp(EditorPrefs.GetInt(WinPfx + "tab", tab), 0, 2);
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorPrefs.SetInt(WinPfx + "tab", tab);
            if (build != null)
                build.OnDisable();
            if (library != null)
                library.OnDisable();
            if (history != null)
                history.OnDisable();
            WbbStyles.Cleanup();
        }

        private void OnDestroy()
        {
            if (build != null)
                build.OnDisable();
        }

        private void OnBeforeAssemblyReload()
        {
            if (build != null)
                build.OnDisable();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (build != null && build.IsBusy)
                build.OnDisable();
        }

        private void Update()
        {
            if (build != null)
                build.Update();
        }

        private void OnFocus()
        {
            WbbLang.OnWindowFocus();
            if (tab == TabLibrary && library != null)
                library.Refresh();
            else if (tab == TabHistory && history != null)
                history.Refresh();
        }

        public void RequestRepaint()
        {
            Repaint();
        }

        public static void FlushOpenWindowPrefs()
        {
            var windows = Resources.FindObjectsOfTypeAll<WbbWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                var window = windows[i];
                if (window == null || window.build == null)
                    continue;
                try
                {
                    window.build.FlushPrefs();
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("Failed to flush build tab settings: " + e.Message);
                }
            }
        }

        public static void RepaintOpenWindows()
        {
            var windows = Resources.FindObjectsOfTypeAll<WbbWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                if (windows[i] != null)
                    windows[i].Repaint();
            }
        }

        public void SwitchTab(int newTab)
        {
            newTab = Mathf.Clamp(newTab, 0, 2);
            if (newTab == tab)
                return;
            tab = newTab;
            tabError = "";
            tabErrorTab = -1;
            EditorPrefs.SetInt(WinPfx + "tab", tab);
            if (tab == TabLibrary && library != null)
                library.Refresh();
            else if (tab == TabHistory && history != null)
                history.Refresh();
            GUI.FocusControl(null);
            Repaint();
        }

        public static bool SelectForBuild(string[] prefabPaths, WbbExportType type, out string error)
        {
            WbbWindow window = FindOrOpen();
            window.SwitchTab(TabBuild);
            bool ok = window.build.SelectForBuild(prefabPaths, type, out error);
            window.Repaint();
            return ok;
        }

        public static bool StartBuildUnattended(out string error)
        {
            WbbWindow window = FindOrOpen();
            bool ok = window.build.StartBuildUnattended(out error);
            window.Repaint();
            return ok;
        }

        private static WbbWindow FindOrOpen()
        {
            WbbWindow[] open = Resources.FindObjectsOfTypeAll<WbbWindow>();
            if (open != null && open.Length > 0 && open[0].build != null)
                return open[0];
            return Open();
        }

        public string RebuildGroupKey(WbbRecord record)
        {
            if (record == null)
                return "";
            if (record.IsSceneSource)
                return "scene";
            return build != null ? "prefab|" + build.RebuildToken(record) : "prefab";
        }

        public void RequestRebuild(WbbRecord record)
        {
            if (record == null)
                return;
            if (record.IsSceneSource)
            {
                WbbQuickBuild.QueueRebuild(record);
                return;
            }

            if (build == null)
                return;
            if (build.IsBusy)
            {
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("Win.BusyRebuild"), L("Common.Ok"));
                return;
            }

            SwitchTab(TabBuild);
            build.QueueRebuild(record);
            Repaint();
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Layout && pendingTab >= 0)
            {
                int next = pendingTab;
                pendingTab = -1;
                SwitchTab(next);
            }

            WbbStyles.WindowBackground(this);
            WbbStyles.DrawHeader();
            DrawTabBar();
            WbbStyles.DrawGuide(GuidePrefKey(), "Guide." + GuidePrefKey());
            WbbStyles.BeginWindowBody();
            string taskKey = tab == TabLibrary ? "Ui.Task.Library" : tab == TabHistory ? "Ui.Task.History" : "Ui.Task.Build";
            GUILayout.Label(L(taskKey), WbbStyles.BodyText);
            GUILayout.Space(WbbSize.SpaceSmall);
            if (tabError.Length > 0 && tabErrorTab == tab)
                DrawTabError();
            else
                DrawSection();
            WbbStyles.EndWindowBody();
        }

        // An exception in a section used to stop the frame in the middle of its layout groups. It is caught here, shown
        // once in the window with a Retry button, and ExitGUI makes Unity drop the half-built layout of this frame.
        private void DrawSection()
        {
            try
            {
                switch (tab)
                {
                    case TabLibrary:
                        library.OnGUI();
                        break;
                    case TabHistory:
                        history.OnGUI();
                        break;
                    default:
                        build.OnGUI();
                        break;
                }
            }
            catch (ExitGUIException)
            {
                throw;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogError("The " + (tab == TabLibrary ? "Library" : tab == TabHistory ? "History" : "Build") + " tab stopped with an error: " + e);
                tabError = string.IsNullOrEmpty(e.Message) ? e.GetType().Name : e.Message;
                tabErrorTab = tab;
                GUIUtility.ExitGUI();
            }
        }

        private void DrawTabError()
        {
            WbbStyles.Callout(L("Win.TabError", tabError), WbbTone.Error);
            GUILayout.Space(WbbSize.SpaceSmall);
            if (WbbStyles.Button(LC("Win.TabRetry"), WbbButtonKind.Secondary, GUILayout.Height(WbbSize.Control)))
            {
                tabError = "";
                tabErrorTab = -1;
                Repaint();
            }
        }

        private void DrawTabBar()
        {
            if (tabContents == null || tabContentsVersion != WbbLang.Version)
            {
                tabContentsVersion = WbbLang.Version;
                tabContents = new[]
                {
                    LC("Tab.Build"),
                    LC("Tab.Library"),
                    LC("Tab.History")
                };
                string[] icons =
                {
                    "Prefab Icon",
                    "Folder Icon",
                    "UnityEditor.ConsoleWindow"
                };
                for (int i = 0; i < tabContents.Length; i++)
                {
                    tabContents[i] = new GUIContent(tabContents[i]);
                    tabContents[i].image = EditorGUIUtility.IconContent(icons[i]).image;
                }
            }

            int next = WbbStyles.TabBar(tab, tabContents, null);
            if (next != tab)
            {
                pendingTab = next;
                Repaint();
            }
        }

        private string GuidePrefKey()
        {
            switch (tab)
            {
                case TabLibrary:
                    return "Library";
                case TabHistory:
                    return "History";
                default:
                    return "Build";
            }
        }
    }
}
