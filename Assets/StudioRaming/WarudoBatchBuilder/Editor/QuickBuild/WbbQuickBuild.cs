using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    [InitializeOnLoad]
    public static partial class WbbQuickBuild
    {
        public const string JobId = "wbb-scene-quick-build";
        public const int ThumbnailSize = 512;
        public const int KeptThumbnailSize = 256;
        private const int KeptThumbnailLimit = 500;
        private const string TempFolderPrefix = "_WBB_Temp_do_not_edit_QB_";
        private const string LegacyTempFolderPrefix = "WbbQuickBuild_";
        private const string PendingCleanupKey = WbbUtil.PrefPrefix + "Quick.PendingCleanup";
        private const double ProgressInterval = 0.1d;
        private const int ReportPreviewCount = 8;
        private static string L(string key)
        {
            return WbbLang.T(key);
        }

        private static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }

        public class Target
        {
            public GameObject source;
            public string modName = "";
            public string rememberName = "";
            public Texture2D thumbnail;
        }

        private static WbbBuildRunner runner;
        private static string tempFolder = "";
        private static readonly List<string> tempGuids = new List<string>();
        private static readonly List<WbbRecord> finishedRecords = new List<WbbRecord>();
        private static readonly Dictionary<string, string> thumbByModName = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        private static string outputDirectory = "";
        private static bool updateHooked;
        private static bool quietFinish;
        private static double nextProgressTime;
        private static double batchStartTime;
        private static double lastItemFinishTime;
        static WbbQuickBuild()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.delayCall += CleanupAfterCrash;
        }

        public static bool IsRunning
        {
            get
            {
                return runner != null && runner.IsRunning;
            }
        }

        public static bool Start(IList<Target> targets, WbbExportType type, out string error)
        {
            error = "";
            if (WbbBuildRunner.AnyRunning)
            {
                error = L("Runner.OtherBuildRunning");
                return false;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                error = L("Quick.EditorBusy");
                return false;
            }

            var valid = new List<Target>();
            if (targets != null)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] != null && targets[i].source != null)
                        valid.Add(targets[i]);
                }
            }

            if (valid.Count == 0)
            {
                error = L("Quick.NoTargets");
                return false;
            }

            WbbWindow.FlushOpenWindowPrefs();
            WbbJob job = WbbBuildSection.LoadSavedJob(type);
            if (!CreateTempFolder())
            {
                error = L("Quick.FolderFailed");
                return false;
            }

            var items = new List<WbbBuildItem>();
            WbbFixupOptions fixups = WbbFixupOptions.FromJob(job);
            thumbByModName.Clear();
            try
            {
                for (int i = 0; i < valid.Count; i++)
                {
                    Target target = valid[i];
                    string label = target.source.name;
                    EditorUtility.DisplayProgressBar(L("Quick.WindowTitle"), L("Quick.Preparing", label), (float)i / valid.Count);
                    string fileBase = "WbbQB_" + i.ToString("00") + "_" + SafeFilePart(label);
                    string prefabPath = tempFolder + "/" + fileBase + ".prefab";
                    WbbFixupReport fixupReport;
                    string snapshotError;
                    if (!SaveSnapshot(target.source, prefabPath, fixups, out fixupReport, out snapshotError))
                    {
                        error = L("Quick.SnapshotFailed", label, snapshotError);
                        CleanupTemp();
                        return false;
                    }

                    string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
                    TrackTemp(prefabGuid);
                    string thumbPath = tempFolder + "/" + fileBase + "_thumb.png";
                    Texture2D icon = SaveThumbnail(target, thumbPath);
                    string modName = WbbUtil.SanitizeModName(target.modName == null ? "" : target.modName.Trim());
                    SourceKey key = SourceKey.Of(target.source);
                    if (!string.IsNullOrEmpty(target.rememberName))
                        WbbQuickBuildNames.Remember(key, WbbUtil.SanitizeModName(target.rememberName.Trim()), type);
                    var item = new WbbBuildItem();
                    item.prefabGuid = prefabGuid;
                    item.prefabPath = prefabPath;
                    item.prefabName = label;
                    item.modName = modName;
                    item.icon = icon;
                    item.sourceObjectId = key.objectId;
                    item.sourceScenePath = key.scenePath;
                    item.sourceHierarchyPath = key.hierarchyPath;
                    item.disposableCopy = true;
                    item.fixupReport = fixupReport == null ? "" : fixupReport.ToText();
                    items.Add(item);
                    if (icon != null && modName.Length > 0)
                        thumbByModName[modName] = thumbPath;
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogError("Scene quick build preparation failed: " + e.Message + "\n" + e.StackTrace);
                error = L("Quick.StartFailed") + "\n" + e.Message;
                CleanupTemp();
                return false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            job.id = JobId;
            job.name = L("Quick.JobName");
            job.prefabFolderPath = tempFolder;
            job.prefabFolderGuid = WbbUtil.FolderGuid(tempFolder);
            job.overwriteExisting = true;
            job.appendDateSuffix = false;
            job.useFolderNamePrefix = false;
            job.lastUsedIso = WbbUtil.NowIso();
            List<WbbPreflightIssue> issues;
            try
            {
                issues = WbbBuildRunner.Preflight(items, job);
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Preflight failed: " + e.Message);
                issues = new List<WbbPreflightIssue>();
            }

            string blocking = FormatBlocking(issues);
            if (blocking.Length > 0)
            {
                error = L("Quick.Blocked") + "\n\n" + blocking;
                CleanupTemp();
                return false;
            }

            outputDirectory = string.IsNullOrEmpty(job.outputDirectory) ? WbbUtil.DefaultOutputPath(type) : WbbUtil.NormalizePath(job.outputDirectory);
            job.outputDirectory = outputDirectory;
            runner = new WbbBuildRunner();
            runner.RecordFinished += OnRecordFinished;
            runner.Finished += OnFinished;
            finishedRecords.Clear();
            quietFinish = false;
            if (!runner.Start(items, job))
            {
                error = string.IsNullOrEmpty(runner.StatusMessage) ? L("Quick.StartFailed") : runner.StatusMessage;
                DetachRunner();
                CleanupTemp();
                return false;
            }

            nextProgressTime = 0d;
            batchStartTime = EditorApplication.timeSinceStartup;
            lastItemFinishTime = batchStartTime;
            HookUpdate(true);
            WbbUtil.Log("Scene quick build started: " + items.Count + " item(s), type=" + type + ", output=" + outputDirectory);
            return true;
        }

        private static Texture2D SaveThumbnail(Target target, string assetPath)
        {
            Texture2D source = target.thumbnail;
            Texture2D captured = null;
            string error;
            try
            {
                if (source == null)
                {
                    bool usedSceneView;
                    captured = WbbThumbnail.CaptureSceneObject(target.source, ThumbnailSize, out usedSceneView, out error);
                    if (captured == null)
                    {
                        WbbUtil.LogWarning("Thumbnail capture failed for " + target.source.name + ": " + error);
                        return null;
                    }

                    source = captured;
                }

                Texture2D saved = WbbThumbnail.SavePngAsset(source, assetPath, out error);
                if (saved == null)
                {
                    WbbUtil.LogWarning("Thumbnail save failed for " + target.source.name + ": " + error);
                    return null;
                }

                TrackTemp(AssetDatabase.AssetPathToGUID(assetPath));
                return saved;
            }
            finally
            {
                if (captured != null)
                    Object.DestroyImmediate(captured);
            }
        }

        private static string FormatBlocking(List<WbbPreflightIssue> issues)
        {
            var sb = new System.Text.StringBuilder();
            if (issues == null)
                return "";
            for (int i = 0; i < issues.Count; i++)
            {
                WbbPreflightIssue issue = issues[i];
                if (issue == null || !issue.blocking)
                    continue;
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append("· ").Append(issue.message);
                if (!string.IsNullOrEmpty(issue.detail))
                    sb.Append("\n   ").Append(issue.detail);
            }

            return sb.ToString();
        }

        private static string SafeFilePart(string name)
        {
            string clean = WbbUtil.SanitizeModName(name == null ? "" : name);
            if (clean.Length > 40)
                clean = clean.Substring(0, 40).TrimEnd('.', ' ');
            return clean.Length == 0 ? "Object" : clean;
        }

        private static void HookUpdate(bool on)
        {
            if (on == updateHooked)
                return;
            if (on)
                EditorApplication.update += Tick;
            else
                EditorApplication.update -= Tick;
            updateHooked = on;
        }

        private static void Tick()
        {
            if (runner == null)
            {
                HookUpdate(false);
                return;
            }

            if (!runner.IsRunning)
                return;
            double now = EditorApplication.timeSinceStartup;
            if (now < nextProgressTime)
                return;
            nextProgressTime = now + ProgressInterval;
            int total = runner.TotalCount;
            int finished = runner.FinishedCount;
            string title = L("Quick.ProgressTitle", Mathf.Min(finished + 1, Mathf.Max(1, total)), total);
            string status = runner.StatusMessage;
            string info = string.IsNullOrEmpty(status) ? L("Runner.PreparingBatch") : status;
            string eta = RemainingText(now, finished, total);
            if (eta.Length > 0)
                info += "  ·  " + L("Quick.Eta", eta);
            if (EditorUtility.DisplayCancelableProgressBar(title, info, runner.Progress))
                runner.Cancel();
        }

        private static string RemainingText(double now, int finished, int total)
        {
            if (finished <= 0 || finished >= total)
                return "";
            double average = (lastItemFinishTime - batchStartTime) / finished;
            double remaining = average * (total - finished) - (now - lastItemFinishTime);
            return remaining < 1d ? "" : WbbUtil.FormatDuration(remaining);
        }

        private static void OnRecordFinished(WbbRecord record)
        {
            if (record != null)
            {
                finishedRecords.Add(record);
                string thumbPath;
                if (record.Status == WbbBuildStatus.Success && record.IsSceneSource && thumbByModName.TryGetValue(record.modName ?? "", out thumbPath))
                    KeepThumbnail(record.id, thumbPath);
            }

            lastItemFinishTime = EditorApplication.timeSinceStartup;
        }

        public static string KeptThumbnailDirectory
        {
            get
            {
                string dir = WbbBuildLog.LibraryDirectory + "/Thumbs";
                try
                {
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("Could not create the thumbnail folder: " + e.Message);
                }

                return dir;
            }
        }

        public static string KeptThumbnailPath(string recordId)
        {
            if (string.IsNullOrEmpty(recordId))
                return "";
            string path = KeptThumbnailDirectory + "/" + recordId + ".png";
            return File.Exists(path) ? path : "";
        }

        private static void KeepThumbnail(string recordId, string pngAssetPath)
        {
            if (string.IsNullOrEmpty(recordId) || string.IsNullOrEmpty(pngAssetPath))
                return;
            Texture2D source = null;
            Texture2D scaled = null;
            RenderTexture target = null;
            RenderTexture previous = RenderTexture.active;
            try
            {
                string absolute = WbbUtil.ToAbsolutePath(pngAssetPath);
                if (!File.Exists(absolute))
                    return;
                byte[] bytes = File.ReadAllBytes(absolute);
                source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!source.LoadImage(bytes))
                    return;
                string destination = KeptThumbnailDirectory + "/" + recordId + ".png";
                int longest = Mathf.Max(source.width, source.height);
                if (longest <= KeptThumbnailSize)
                {
                    File.WriteAllBytes(destination, bytes);
                }
                else
                {
                    float scale = (float)KeptThumbnailSize / longest;
                    int width = Mathf.Max(1, Mathf.RoundToInt(source.width * scale));
                    int height = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));
                    target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                    Graphics.Blit(source, target);
                    RenderTexture.active = target;
                    scaled = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    scaled.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    scaled.Apply();
                    File.WriteAllBytes(destination, scaled.EncodeToPNG());
                }

                PruneKeptThumbnails();
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Could not keep a thumbnail for the build history: " + e.Message);
            }
            finally
            {
                RenderTexture.active = previous;
                if (target != null)
                    RenderTexture.ReleaseTemporary(target);
                if (source != null)
                    Object.DestroyImmediate(source);
                if (scaled != null)
                    Object.DestroyImmediate(scaled);
            }
        }

        private static void PruneKeptThumbnails()
        {
            try
            {
                var files = new List<FileInfo>(new DirectoryInfo(KeptThumbnailDirectory).GetFiles("*.png"));
                if (files.Count <= KeptThumbnailLimit)
                    return;
                files.Sort((a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));
                for (int i = KeptThumbnailLimit; i < files.Count; i++)
                {
                    try
                    {
                        files[i].Delete();
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Thumbnail pruning failed: " + e.Message);
            }
        }

        private static void OnFinished(bool canceled)
        {
            EditorUtility.ClearProgressBar();
            HookUpdate(false);
            int success = runner != null ? runner.SuccessCount : 0;
            int failed = runner != null ? runner.FailedCount : 0;
            DetachRunner();
            CleanupTemp();
            WbbWindow.RepaintOpenWindows();
            if (quietFinish || WbbBuildRunner.AbortingSilently)
            {
                WbbUtil.LogWarning("Scene quick build interrupted (domain reload or play mode change): " + success + " succeeded, " + failed + " failed.");
                return;
            }

            Report(canceled, success, failed);
        }

        private static void DetachRunner()
        {
            if (runner == null)
                return;
            runner.RecordFinished -= OnRecordFinished;
            runner.Finished -= OnFinished;
            runner = null;
        }

        private static void Report(bool canceled, int success, int failed)
        {
            string elapsed = WbbUtil.FormatDuration(EditorApplication.timeSinceStartup - batchStartTime);
            if (!canceled && failed == 0 && success > 0)
            {
                string text = success == 1 && finishedRecords.Count > 0 ? L("Quick.DoneOne", finishedRecords[0].modName + WbbUtil.ModExtension) : L("Quick.DoneMany", success, elapsed);
                for (int i = 0; i < finishedRecords.Count; i++)
                    WbbUtil.Log("Built " + finishedRecords[i].outputFilePath);
                SceneView sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                    sceneView.ShowNotification(new GUIContent(text));
                return;
            }

            var body = new System.Text.StringBuilder();
            body.Append(canceled ? L("Quick.ResultCanceled", success, failed) : L("Quick.ResultDone", success, failed));
            body.Append("\n").Append(L("Quick.Elapsed", elapsed));
            for (int i = 0; i < finishedRecords.Count && i < ReportPreviewCount; i++)
            {
                WbbRecord record = finishedRecords[i];
                body.Append("\n\n").Append(WbbLang.StatusName(record.Status)).Append(": ").Append(record.modName).Append(WbbUtil.ModExtension);
                if (record.Status != WbbBuildStatus.Success && !string.IsNullOrEmpty(record.message))
                    body.Append("\n").Append(record.message);
            }

            if (finishedRecords.Count > ReportPreviewCount)
                body.Append("\n\n").Append(L("Build.ConfirmMore", finishedRecords.Count - ReportPreviewCount));
            int choice = EditorUtility.DisplayDialogComplex(L("Quick.WindowTitle"), body.ToString(), L("Build.OpenHistory"), L("Common.Close"), L("Build.OpenOutput"));
            if (choice == 0)
            {
                WbbWindow window = WbbWindow.Open();
                if (window != null)
                    window.SwitchTab(WbbWindow.TabHistory);
            }
            else if (choice == 2)
            {
                WbbUtil.RevealFolder(outputDirectory);
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            AbortQuietly(L("Runner.AbortReload"));
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            AbortQuietly(L("Runner.AbortPlayMode"));
        }

        private static void AbortQuietly(string reason)
        {
            if (!IsRunning)
                return;
            quietFinish = true;
            try
            {
                runner.AbortAndRestore(reason);
            }
            catch (System.Exception e)
            {
                WbbUtil.LogError("Scene quick build abort failed: " + e.Message);
            }

            EditorUtility.ClearProgressBar();
        }

        private static bool CreateTempFolder()
        {
            tempFolder = "";
            tempGuids.Clear();
            try
            {
                string name = TempFolderPrefix + System.DateTime.Now.ToString("HHmmss") + "_" + Random.Range(1000, 9999);
                string guid = AssetDatabase.CreateFolder("Assets", name);
                if (string.IsNullOrEmpty(guid))
                    return false;
                string path = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(guid));
                if (!WbbUtil.IsValidProjectFolder(path))
                    return false;
                tempFolder = path;
                SavePendingCleanup();
                return true;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogError("Failed to create the quick build folder: " + e.Message);
                return false;
            }
        }

        private static void TrackTemp(string guid)
        {
            if (string.IsNullOrEmpty(guid) || tempGuids.Contains(guid))
                return;
            tempGuids.Add(guid);
            SavePendingCleanup();
        }

        private static void SavePendingCleanup()
        {
            WbbPrefs.SetString(PendingCleanupKey, tempFolder + "|" + string.Join(";", tempGuids.ToArray()));
        }

        private static void CleanupTemp()
        {
            for (int i = 0; i < tempGuids.Count; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(tempGuids[i]);
                if (string.IsNullOrEmpty(path))
                    continue;
                try
                {
                    if (!File.Exists(WbbUtil.ToAbsolutePath(path)))
                        continue;
                    if (!AssetDatabase.DeleteAsset(path))
                        WbbUtil.LogWarning("Could not delete quick build temp asset: " + path);
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("Quick build temp asset cleanup failed (" + path + "): " + e.Message);
                }
            }

            tempGuids.Clear();
            string folder = tempFolder;
            tempFolder = "";
            if (IsOurTempFolder(folder))
            {
                try
                {
                    string absolute = WbbUtil.ToAbsolutePath(folder);
                    if (Directory.Exists(absolute))
                    {
                        string[] files = Directory.GetFiles(absolute, "*", SearchOption.AllDirectories);
                        int remaining = 0;
                        for (int i = 0; i < files.Length; i++)
                        {
                            if (!files[i].EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
                                remaining++;
                        }

                        if (remaining > 0)
                            WbbUtil.LogWarning("Quick build folder is not empty, leaving it in place: " + folder);
                        else if (!AssetDatabase.DeleteAsset(folder))
                            WbbUtil.LogWarning("Could not remove quick build folder: " + folder);
                    }
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("Quick build folder cleanup failed: " + e.Message);
                }
            }

            WbbPrefs.DeleteKey(PendingCleanupKey);
        }

        private static bool IsOurTempFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
                return false;
            string normalized = WbbUtil.NormalizePath(folder);
            bool prefixed = normalized.StartsWith("Assets/" + TempFolderPrefix, System.StringComparison.Ordinal) || normalized.StartsWith("Assets/" + LegacyTempFolderPrefix, System.StringComparison.Ordinal);
            return prefixed && normalized.IndexOf('/', "Assets/".Length) < 0;
        }

        private static void CleanupAfterCrash()
        {
            string pending = WbbPrefs.GetString(PendingCleanupKey, "", false);
            if (string.IsNullOrEmpty(pending) || IsRunning || WbbBuildRunner.AnyRunning)
                return;
            int split = pending.IndexOf('|');
            string folder = split >= 0 ? pending.Substring(0, split) : pending;
            string guidList = split >= 0 ? pending.Substring(split + 1) : "";
            tempFolder = folder;
            tempGuids.Clear();
            string[] guids = guidList.Split(';');
            for (int i = 0; i < guids.Length; i++)
            {
                if (!string.IsNullOrEmpty(guids[i]))
                    tempGuids.Add(guids[i]);
            }

            WbbUtil.LogWarning("Cleaning up files left by an interrupted scene quick build: " + folder);
            CleanupTemp();
        }
    }
}
