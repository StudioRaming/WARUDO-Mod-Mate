using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbUtil
    {
        public const string PrefPrefix = "StudioRaming.WarudoBatchBuilder.";
        public const string ModExtension = ".warudo";
        private static readonly char[] InvalidNameChars = BuildInvalidNameChars();
        private static char[] BuildInvalidNameChars()
        {
            var set = new HashSet<char>(Path.GetInvalidFileNameChars());
            set.Add('/');
            set.Add('\\');
            set.Add(':');
            set.Add('*');
            set.Add('?');
            set.Add('"');
            set.Add('<');
            set.Add('>');
            set.Add('|');
            var arr = new char[set.Count];
            set.CopyTo(arr);
            return arr;
        }

        public static string NowIso()
        {
            return System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string ToIso(System.DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static bool TryParseIso(string text, out System.DateTime value)
        {
            return System.DateTime.TryParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
        }

        public static string DateSuffix()
        {
            return "_" + System.DateTime.Now.ToString("MM-dd", CultureInfo.InvariantCulture);
        }

        public static string FormatDuration(double seconds)
        {
            if (seconds < 0d)
                seconds = 0d;
            if (seconds < 60d)
                return seconds.ToString("0.0", CultureInfo.InvariantCulture) + "s";
            int m = (int)(seconds / 60d);
            int s = (int)(seconds - m * 60);
            return m + "m " + s + "s";
        }

        public static string FormatSize(long bytes)
        {
            if (bytes <= 0L)
                return "0 B";
            if (bytes < 1024L)
                return bytes + " B";
            double kb = bytes / 1024d;
            if (kb < 1024d)
                return kb.ToString("0.#", CultureInfo.InvariantCulture) + " KB";
            double mb = kb / 1024d;
            if (mb < 1024d)
                return mb.ToString("0.#", CultureInfo.InvariantCulture) + " MB";
            double gb = mb / 1024d;
            return gb.ToString("0.##", CultureInfo.InvariantCulture) + " GB";
        }

        public static string SanitizeModName(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "";
            var sb = new System.Text.StringBuilder(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                bool bad = c < 32;
                if (!bad)
                {
                    for (int k = 0; k < InvalidNameChars.Length; k++)
                    {
                        if (InvalidNameChars[k] == c)
                        {
                            bad = true;
                            break;
                        }
                    }
                }

                sb.Append(bad ? '_' : c);
            }

            string result = sb.ToString().Trim();
            result = result.TrimEnd('.', ' ');
            return AvoidReservedName(result);
        }

        private static readonly string[] ReservedDeviceNames =
        {
            "CON",
            "PRN",
            "AUX",
            "NUL",
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "LPT1",
            "LPT2",
            "LPT3",
            "LPT4",
            "LPT5",
            "LPT6",
            "LPT7",
            "LPT8",
            "LPT9",
        };
        private static string AvoidReservedName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            int dot = name.IndexOf('.');
            string stem = (dot >= 0 ? name.Substring(0, dot) : name).TrimEnd(' ');
            for (int i = 0; i < ReservedDeviceNames.Length; i++)
            {
                if (string.Equals(stem, ReservedDeviceNames[i], System.StringComparison.OrdinalIgnoreCase))
                    return dot >= 0 ? stem + "_" + name.Substring(dot) : stem + "_";
            }

            return name;
        }

        public static bool IsSafeModName(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return false;
            return SanitizeModName(raw) == raw;
        }

        public static string TimeSuffix()
        {
            return "_" + System.DateTime.Now.ToString("MM-dd-HHmmss", CultureInfo.InvariantCulture);
        }

        public static string ComposeModName(WbbJob job, string prefabFolderName, string prefabName)
        {
            string prefix;
            if (job != null && !job.useFolderNamePrefix)
                prefix = job.namePrefix == null ? "" : job.namePrefix.Trim();
            else
                prefix = prefabFolderName == null ? "" : prefabFolderName.Trim();
            string core = string.IsNullOrEmpty(prefix) ? prefabName : prefix + "_" + prefabName;
            // The time suffix already starts with the date, so both options together add it once.
            if (job != null && !job.overwriteExisting)
                core += TimeSuffix();
            else if (job != null && job.appendDateSuffix)
                core += DateSuffix();
            return SanitizeModName(core);
        }

        public static string GetManualName(WbbJob job, string prefabGuid)
        {
            if (job == null || string.IsNullOrEmpty(prefabGuid))
                return "";
            if (job.manualNameGuids == null || job.manualNames == null)
                return "";
            int count = Mathf.Min(job.manualNameGuids.Count, job.manualNames.Count);
            for (int i = 0; i < count; i++)
            {
                if (string.Equals(job.manualNameGuids[i], prefabGuid, System.StringComparison.Ordinal))
                    return job.manualNames[i] == null ? "" : job.manualNames[i];
            }

            return "";
        }

        public static void SetManualName(WbbJob job, string prefabGuid, string name)
        {
            if (job == null || string.IsNullOrEmpty(prefabGuid))
                return;
            if (job.manualNameGuids == null)
                job.manualNameGuids = new List<string>();
            if (job.manualNames == null)
                job.manualNames = new List<string>();
            while (job.manualNames.Count < job.manualNameGuids.Count)
                job.manualNames.Add("");
            while (job.manualNameGuids.Count < job.manualNames.Count)
                job.manualNames.RemoveAt(job.manualNames.Count - 1);
            string clean = string.IsNullOrEmpty(name) ? "" : SanitizeModName(name.Trim());
            for (int i = 0; i < job.manualNameGuids.Count; i++)
            {
                if (!string.Equals(job.manualNameGuids[i], prefabGuid, System.StringComparison.Ordinal))
                    continue;
                if (clean.Length == 0)
                {
                    job.manualNameGuids.RemoveAt(i);
                    job.manualNames.RemoveAt(i);
                }
                else
                {
                    job.manualNames[i] = clean;
                }

                return;
            }

            if (clean.Length == 0)
                return;
            job.manualNameGuids.Add(prefabGuid);
            job.manualNames.Add(clean);
        }

        public static string ResolveBuildFolder(WbbJob job, WbbExportType type)
        {
            if (job == null || !job.manualScriptFolder)
                return "";
            int index = (int)type;
            string guid = ListValue(job.buildFolderGuidsByType, index);
            string path = ListValue(job.buildFolderPathsByType, index);
            string resolved = ResolveFolder(guid, path);
            if (!string.IsNullOrEmpty(resolved))
                return resolved;
            return ResolveFolder(job.buildFolderGuid, job.buildFolderPath);
        }

        public static void SetBuildFolderForType(WbbJob job, WbbExportType type, string folderPath)
        {
            if (job == null)
                return;
            int index = (int)type;
            EnsureListSize(job.buildFolderGuidsByType, index + 1);
            EnsureListSize(job.buildFolderPathsByType, index + 1);
            if (string.IsNullOrEmpty(folderPath))
            {
                job.buildFolderGuidsByType[index] = "";
                job.buildFolderPathsByType[index] = "";
                return;
            }

            string normalized = NormalizePath(folderPath);
            job.buildFolderGuidsByType[index] = FolderGuid(normalized);
            job.buildFolderPathsByType[index] = normalized;
        }

        private static string ListValue(List<string> list, int index)
        {
            if (list == null || index < 0 || index >= list.Count)
                return "";
            return list[index] == null ? "" : list[index];
        }

        private static void EnsureListSize(List<string> list, int size)
        {
            if (list == null)
                return;
            while (list.Count < size)
                list.Add("");
        }

        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";
            return path.Replace('\\', '/').TrimEnd('/');
        }

        public static string Combine(string a, string b)
        {
            return NormalizePath(Path.Combine(a == null ? "" : a, b == null ? "" : b));
        }

        public static string ToAbsolutePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return "";
            string p = NormalizePath(assetPath);
            if (p.StartsWith("Assets/") || p == "Assets")
            {
                string dataPath = NormalizePath(Application.dataPath);
                string rest = p.Length > "Assets".Length ? p.Substring("Assets".Length).TrimStart('/') : "";
                return string.IsNullOrEmpty(rest) ? dataPath : dataPath + "/" + rest;
            }

            return p;
        }

        public static bool IsValidProjectFolder(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) && AssetDatabase.IsValidFolder(NormalizePath(assetPath));
        }

        public static bool IsFolderAsset(Object obj)
        {
            if (obj == null)
                return false;
            string path = AssetDatabase.GetAssetPath(obj);
            return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path);
        }

        public static string FolderName(string assetPath)
        {
            string p = NormalizePath(assetPath);
            if (string.IsNullOrEmpty(p))
                return "";
            int idx = p.LastIndexOf('/');
            return idx >= 0 ? p.Substring(idx + 1) : p;
        }

        public static string ResolveFolder(string guid, string fallbackPath)
        {
            if (!string.IsNullOrEmpty(guid))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (IsValidProjectFolder(p))
                    return NormalizePath(p);
            }

            string fb = NormalizePath(fallbackPath);
            return IsValidProjectFolder(fb) ? fb : "";
        }

        public static string FolderGuid(string assetPath)
        {
            if (!IsValidProjectFolder(assetPath))
                return "";
            return AssetDatabase.AssetPathToGUID(NormalizePath(assetPath));
        }

        public static string SourceSignature(string assetPath)
        {
            try
            {
                string abs = ToAbsolutePath(assetPath);
                if (string.IsNullOrEmpty(abs) || !File.Exists(abs))
                    return "";
                var fi = new FileInfo(abs);
                return fi.Length.ToString(CultureInfo.InvariantCulture) + ":" + fi.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                return "";
            }
        }

        public enum PipelineKind
        {
            BuiltIn,
            Urp,
            Hdrp,
            Custom,
        }

        private static System.Type pipelineTypeSeen;
        private static PipelineKind pipelineKindSeen;
        public static PipelineKind CurrentPipeline(out string typeName)
        {
            UnityEngine.Rendering.RenderPipelineAsset asset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (asset == null)
            {
                typeName = "";
                return PipelineKind.BuiltIn;
            }

            System.Type type = asset.GetType();
            typeName = type.Name;
            if (type == pipelineTypeSeen)
                return pipelineKindSeen;
            PipelineKind kind = PipelineKind.Custom;
            for (System.Type t = type; t != null; t = t.BaseType)
            {
                string fullName = t.FullName ?? t.Name;
                if (fullName.IndexOf("Universal", System.StringComparison.Ordinal) >= 0)
                {
                    kind = PipelineKind.Urp;
                    break;
                }

                if (fullName.IndexOf("HighDefinition", System.StringComparison.Ordinal) >= 0)
                {
                    kind = PipelineKind.Hdrp;
                    break;
                }
            }

            pipelineTypeSeen = type;
            pipelineKindSeen = kind;
            return kind;
        }

        public static string PipelineName(PipelineKind kind, string typeName)
        {
            switch (kind)
            {
                case PipelineKind.Urp:
                    return "URP";
                case PipelineKind.Hdrp:
                    return "HDRP";
                case PipelineKind.Custom:
                    return string.IsNullOrEmpty(typeName) ? "Custom" : typeName;
                default:
                    return "Built-in";
            }
        }

        public static string PipelineLogText()
        {
            string typeName;
            PipelineKind kind = CurrentPipeline(out typeName);
            string name = PipelineName(kind, typeName);
            return kind == PipelineKind.Urp || kind == PipelineKind.Hdrp ? name + " (" + typeName + ")" : name;
        }

        public static string NewId()
        {
            return System.Guid.NewGuid().ToString("N");
        }

        public static string DefaultOutputPath(WbbExportType type)
        {
            return WbbWarudoLocator.TypeFolder(type);
        }

        // The output folder stored under `key`, or the type's default when none is stored. The default is worked out only
        // then: it checks the WARUDO folder setting's folders, and a share that does not answer blocks that check for about
        // 20 seconds, on every window focus and tab entry that reads the stored folder.
        public static string StoredOutputPath(string key, WbbExportType type)
        {
            return WbbPrefs.HasKey(key) ? WbbPrefs.GetString(key, "") : DefaultOutputPath(type);
        }

        public static void RevealFolder(string absolutePath)
        {
            try
            {
                if (string.IsNullOrEmpty(absolutePath))
                    return;
                if (Directory.Exists(absolutePath))
                    EditorUtility.RevealInFinder(absolutePath + "/");
                else if (File.Exists(absolutePath))
                    EditorUtility.RevealInFinder(absolutePath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[WarudoBatchBuilder] Failed to reveal path: " + absolutePath + " (" + e.Message + ")");
            }
        }

        public static void Log(string message)
        {
            Debug.Log("[WarudoBatchBuilder] " + message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning("[WarudoBatchBuilder] " + message);
        }

        public static void LogError(string message)
        {
            Debug.LogError("[WarudoBatchBuilder] " + message);
        }
    }
}
