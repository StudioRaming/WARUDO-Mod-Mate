using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildRunner
    {
        private WbbBuildItem CurrentItem()
        {
            if (items == null || index < 0 || index >= items.Count)
                return null;
            return items[index];
        }

        private void ResetItemState()
        {
            originalPrefabPath = "";
            copyPath = "";
            copyGuid = "";
            restoreFailed = false;
            sourceSignature = "";
            outputFilePath = "";
            outputSizeBytes = 0L;
            lastSize = -1L;
            stableSamples = 0;
            buildStartUtc = System.DateTime.MinValue;
            waitStartTime = 0d;
            itemRecordId = "";
            itemFixupReport = "";
            itemBuildLogPath = "";
            invokeResult = null;
            outputGuard = null;
            if (capture != null)
                capture.Stop();
            capture = null;
        }

        private void FlagCleanupFailure(string currentPath)
        {
            restoreFailed = true;
            string message = L("Safety.CopyCleanupFailed", string.IsNullOrEmpty(currentPath) ? copyPath : currentPath);
            AppendMessage(message);
            SetStatus(message);
            WbbUtil.LogError(message);
            if (!cancelRequested)
            {
                cancelRequested = true;
                WbbUtil.LogError("Stopping the remaining items because a temporary build copy could not be removed.");
            }
        }

        private void MarkCurrent(WbbBuildStatus status, string message)
        {
            var item = CurrentItem();
            if (item == null)
                return;
            item.status = status;
            if (!string.IsNullOrEmpty(message))
                AppendMessage(message);
        }

        private void Fail(string message)
        {
            var item = CurrentItem();
            if (item != null)
            {
                item.status = WbbBuildStatus.Failed;
                AppendMessage(message);
            }

            SetStatus(message);
            WbbUtil.LogError((string.IsNullOrEmpty(currentModName) ? "" : currentModName + ": ") + message);
        }

        private void AppendMessage(string message)
        {
            var item = CurrentItem();
            if (item == null || string.IsNullOrEmpty(message))
                return;
            item.message = string.IsNullOrEmpty(item.message) ? message : item.message + " / " + message;
        }

        private void SetStatus(string message)
        {
            statusMessage = message == null ? "" : message;
        }

        private void Delay(double seconds)
        {
            waitUntil = EditorApplication.timeSinceStartup + seconds;
        }

        private void RequestRepaint()
        {
            var handler = RepaintRequested;
            if (handler == null)
                return;
            try
            {
                handler();
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("RepaintRequested handler threw: " + e.Message);
            }
        }

        private static WbbExportType ResolveExportType(WbbJob source)
        {
            var type = source == null ? WbbExportType.Prop : (WbbExportType)Mathf.Clamp(source.exportType, 0, 2);
            if (type != WbbExportType.Character && type != WbbExportType.Environment)
                type = WbbExportType.Prop;
            return type;
        }

        private static string ResolveOutputDirectory(WbbJob source)
        {
            string directory = source == null || source.outputDirectory == null ? "" : source.outputDirectory.Trim();
            if (string.IsNullOrEmpty(directory))
                directory = WbbUtil.DefaultOutputPath(ResolveExportType(source));
            return WbbUtil.NormalizePath(directory);
        }

        public static string NormalizeModVersion(string raw, out bool autoFixed)
        {
            autoFixed = false;
            string version = raw == null ? "" : raw.Trim();
            if (version.Length == 0)
            {
                autoFixed = true;
                return "1.0.0";
            }

            string[] parts = version.Split('.');
            int ignored;
            int check = parts.Length >= 3 ? 3 : parts.Length;
            for (int i = 0; i < check; i++)
            {
                if (!int.TryParse(parts[i].Trim(), out ignored))
                    return "";
            }

            if (parts.Length >= 3)
                return version;
            autoFixed = true;
            return parts.Length == 1 ? version + ".0.0" : version + ".0";
        }

        private static bool EnsureDirectory(string directory)
        {
            if (!IsUsableOutputPath(directory))
                return false;
            try
            {
                string absolute = WbbUtil.ToAbsolutePath(directory);
                if (Directory.Exists(absolute))
                    return true;
                string warudoRoot;
                if (WbbWarudoLocator.IsInsideWarudoData(absolute, out warudoRoot) && !Directory.Exists(warudoRoot))
                {
                    WbbUtil.LogWarning("Not creating '" + directory + "': the WARUDO folder " + warudoRoot + " does not exist.");
                    return false;
                }

                Directory.CreateDirectory(absolute);
                return Directory.Exists(absolute);
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Cannot create output directory '" + directory + "': " + e.Message);
                return false;
            }
        }

        private static bool IsUsableOutputPath(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                return false;
            try
            {
                string normalized = WbbUtil.NormalizePath(directory);
                if (normalized.Length == 0)
                    return false;
                if (normalized.StartsWith("Assets/") || normalized == "Assets")
                    return true;
                return Path.IsPathRooted(normalized);
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool OutputDirectoryExists(string directory)
        {
            if (!IsUsableOutputPath(directory))
                return false;
            try
            {
                return Directory.Exists(WbbUtil.ToAbsolutePath(directory));
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
