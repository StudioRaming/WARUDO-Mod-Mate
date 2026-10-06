using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbWarudoLocator
    {
        public const string AppId = "2079120";
        private const string DataSuffix = "Warudo_Data/StreamingAssets";
        private static bool searched;
        private static string cachedStreamingAssets = "";
        private static readonly List<string> cachedLibraries = new List<string>();
        private static readonly Regex PathEntry = new Regex("\"path\"\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase);
        private static readonly Regex LegacyEntry = new Regex("^\\s*\"\\d+\"\\s+\"([^\"]+)\"\\s*$", RegexOptions.Multiline);
        private static readonly Regex InstallDirEntry = new Regex("\"installdir\"\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase);
        public static string StreamingAssetsPath
        {
            get
            {
                EnsureSearched();
                return cachedStreamingAssets;
            }
        }

        public static bool Found
        {
            get
            {
                return !string.IsNullOrEmpty(StreamingAssetsPath);
            }
        }

        public static string TypeFolder(WbbExportType type)
        {
            string root = StreamingAssetsPath;
            if (string.IsNullOrEmpty(root))
                return "";
            switch (type)
            {
                case WbbExportType.Character:
                    return root + "/Characters";
                case WbbExportType.Environment:
                    return root + "/Environments";
                default:
                    return root + "/Props";
            }
        }

        public static IList<string> SteamLibraries()
        {
            EnsureSearched();
            return cachedLibraries.AsReadOnly();
        }

        public static void Invalidate()
        {
            searched = false;
            cachedStreamingAssets = "";
            cachedLibraries.Clear();
        }

        public static bool IsInsideWarudoData(string path, out string streamingAssetsRoot)
        {
            streamingAssetsRoot = "";
            string normalized = WbbUtil.NormalizePath(path);
            if (string.IsNullOrEmpty(normalized))
                return false;
            int index = normalized.IndexOf("/" + DataSuffix, System.StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return false;
            streamingAssetsRoot = normalized.Substring(0, index + 1 + DataSuffix.Length);
            return true;
        }

        private static void EnsureSearched()
        {
            if (searched)
                return;
            searched = true;
            cachedStreamingAssets = "";
            cachedLibraries.Clear();
            try
            {
                string steam = SteamRoot();
                if (string.IsNullOrEmpty(steam))
                    return;
                AddLibrary(steam);
                string vdf = steam + "/steamapps/libraryfolders.vdf";
                if (File.Exists(vdf))
                    ParseLibraryFolders(File.ReadAllText(vdf));
                for (int i = 0; i < cachedLibraries.Count; i++)
                {
                    string candidate = CandidateFor(cachedLibraries[i]);
                    if (!string.IsNullOrEmpty(candidate) && Directory.Exists(candidate))
                    {
                        cachedStreamingAssets = candidate;
                        return;
                    }
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("WARUDO install lookup failed: " + e.Message);
            }
        }

        private static string SteamRoot()
        {
            string value = ReadRegistry("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath");
            if (string.IsNullOrEmpty(value))
                value = ReadRegistry("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath");
            if (string.IsNullOrEmpty(value))
                value = ReadRegistry("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath");
            return WbbUtil.NormalizePath(value);
        }

        private static string ReadRegistry(string key, string name)
        {
            try
            {
                object value = Microsoft.Win32.Registry.GetValue(key, name, null);
                return value as string ?? "";
            }
            catch (System.Exception)
            {
                return "";
            }
        }

        private static void ParseLibraryFolders(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            foreach (Match match in PathEntry.Matches(text))
                AddLibrary(Unescape(match.Groups[1].Value));
            foreach (Match match in LegacyEntry.Matches(text))
            {
                string value = Unescape(match.Groups[1].Value);
                if (value.IndexOf(':') > 0 || value.StartsWith("/"))
                    AddLibrary(value);
            }
        }

        private static string Unescape(string vdfValue)
        {
            return vdfValue == null ? "" : vdfValue.Replace("\\\\", "\\");
        }

        private static void AddLibrary(string path)
        {
            string normalized = WbbUtil.NormalizePath(path);
            if (string.IsNullOrEmpty(normalized))
                return;
            for (int i = 0; i < cachedLibraries.Count; i++)
            {
                if (string.Equals(cachedLibraries[i], normalized, System.StringComparison.OrdinalIgnoreCase))
                    return;
            }

            cachedLibraries.Add(normalized);
        }

        private static string CandidateFor(string library)
        {
            string installDir = "Warudo";
            try
            {
                string manifest = library + "/steamapps/appmanifest_" + AppId + ".acf";
                if (File.Exists(manifest))
                {
                    Match match = InstallDirEntry.Match(File.ReadAllText(manifest));
                    if (match.Success && match.Groups[1].Value.Trim().Length > 0)
                        installDir = Unescape(match.Groups[1].Value.Trim());
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Could not read the WARUDO app manifest in " + library + ": " + e.Message);
            }

            return library + "/steamapps/common/" + installDir + "/" + DataSuffix;
        }
    }
}
