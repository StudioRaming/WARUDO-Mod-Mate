using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbDiagnostics
    {
        private const int RecentProblemCount = 5;
        private static string L(string key)
        {
            return WbbLang.T(key);
        }

        private static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }

        public static void ShowAbout()
        {
            string body = L("Ui.About.Body", WbbProduct.Version, Application.unityVersion, WbbProduct.RequiredUnity, ModToolVersion(), WbbProduct.SupportEmail);
            EditorUtility.DisplayDialog(WbbProduct.Name, body, L("Common.Ok"));
        }

        public static void OpenRepository()
        {
            Application.OpenURL(WbbProduct.RepositoryUrl);
        }

        public static void OpenManual()
        {
            string anchor = WbbLang.Current == WbbLanguage.Korean ? "한국어" : WbbLang.Current == WbbLanguage.Japanese ? "日本語" : "english";
            Application.OpenURL(WbbProduct.ManualUrl + "#" + System.Uri.EscapeDataString(anchor));
        }

        public static void OpenDownloads()
        {
            Application.OpenURL(WbbProduct.DownloadUrl);
        }

        public static void CopySupportEmail()
        {
            EditorGUIUtility.systemCopyBuffer = WbbProduct.SupportEmail;
            Notify(L("Ui.Help.EmailCopied", WbbProduct.SupportEmail));
        }

        public static void CopyToClipboard()
        {
            EditorGUIUtility.systemCopyBuffer = Build();
            Notify(L("Ui.Help.DiagnosticsCopied"));
        }

        public static string Build()
        {
            var sb = new StringBuilder(1024);
            sb.AppendLine(WbbProduct.Name + " " + WbbProduct.Version);
            sb.AppendLine("Unity: " + Application.unityVersion + " (required " + WbbProduct.RequiredUnity + ")");
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            sb.AppendLine("Render pipeline: " + (pipeline == null ? "Built-in" : pipeline.GetType().Name));
            string warudoSource;
            WbbUtil.PipelineKind? warudoKind = WbbWarudoPipeline.ReadNow(out warudoSource);
            sb.AppendLine("WARUDO pipeline: " + (warudoKind.HasValue ? WbbUtil.PipelineName(warudoKind.Value, "") : "unknown") + " (" + warudoSource + ")");
            sb.AppendLine("Color space: " + PlayerSettings.colorSpace);
            sb.AppendLine("Warudo Mod Tool: " + ModToolVersion());
            sb.AppendLine("OS: " + SystemInfo.operatingSystem);
            sb.AppendLine("Editor language: " + WbbLang.Current);
            sb.AppendLine("Skin: " + (EditorGUIUtility.isProSkin ? "Pro(dark)" : "Personal(light)") + " · DPI scale " + EditorGUIUtility.pixelsPerPoint.ToString("0.##", CultureInfo.InvariantCulture));
            string error;
            bool umod = WbbExportSettings.ValidateAvailable(out error);
            sb.AppendLine("Build API available: " + umod + (umod ? "" : " (" + error + ")"));
            sb.AppendLine("Build settings asset found: " + (WbbExportSettings.Locate(out error) != null));
            var records = WbbDatabase.Records;
            int success = 0, failed = 0, other = 0;
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].status == (int)WbbBuildStatus.Success)
                    success++;
                else if (records[i].status == (int)WbbBuildStatus.Failed)
                    failed++;
                else
                    other++;
            }

            sb.AppendLine("History: " + records.Count + " (success " + success + ", failed " + failed + ", other " + other + ")");
            int shown = 0;
            for (int i = records.Count - 1; i >= 0 && shown < RecentProblemCount; i--)
            {
                WbbRecord r = records[i];
                if (r.status == (int)WbbBuildStatus.Success)
                    continue;
                shown++;
                sb.AppendLine("  - " + r.finishedIso + " " + (WbbBuildStatus)r.status + " [" + (WbbExportType)r.exportType + "] " + Mask(r.modName) + ": " + Mask(r.message));
            }

            return sb.ToString();
        }

        private static string ModToolVersion()
        {
            try
            {
                UnityEditor.PackageManager.PackageInfo info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/app.warudo.modtool");
                if (info != null)
                    return info.version;
            }
            catch (System.Exception)
            {
            }

            return "unknown";
        }

        private static string Mask(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            string home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(home))
            {
                text = text.Replace(home, "%USERPROFILE%");
                text = text.Replace(home.Replace('\\', '/'), "%USERPROFILE%");
            }

            return text.Length > 300 ? text.Substring(0, 300) + "…" : text;
        }

        private static void Notify(string message)
        {
            EditorWindow window = EditorWindow.focusedWindow;
            if (window != null)
                window.ShowNotification(new GUIContent(message), 2.0);
            else
                WbbUtil.Log(message);
        }
    }
}
