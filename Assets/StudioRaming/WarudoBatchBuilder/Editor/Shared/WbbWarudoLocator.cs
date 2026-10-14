using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbWarudoLocator
    {
        public const string AppId = "2079120";
        // The per-project "WARUDO folder" setting (the install root or its Warudo_Data/StreamingAssets).
        public const string RootPrefKey = WbbUtil.PrefPrefix + "WarudoRoot";
        private const string DataSuffix = "Warudo_Data/StreamingAssets";
        // Tests only: makes the Steam search find nothing, so the output path and the WARUDO folder setting decide. Not shown in the UI.
        // Public rather than internal because the Unity bridge snippets that set it compile into their own assembly. Not saved:
        // a domain reload turns it off again.
        public static bool DisableSteamLookupForTests;
        // The root the running batch decided on. Code that has no output path of its own reads it while a batch runs.
        public static string BatchStreamingAssets = "";
        // The last WARUDO folder setting reported as unusable, so a bad value is reported once and not on every preflight.
        private static string warnedSetting = "";
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
                if (DisableSteamLookupForTests)
                    return "";
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

        // The type folder of the WARUDO that code without an output path works with (AmbientStreamingAssets): the running
        // batch's, otherwise the WARUDO folder setting, otherwise Steam. With Steam alone, a WARUDO outside Steam that the
        // setting names would get no default output folder, and the Props reload notice would never match it.
        public static string TypeFolder(WbbExportType type)
        {
            string root = AmbientStreamingAssets;
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
            if (DisableSteamLookupForTests)
                return new List<string>().AsReadOnly();
            EnsureSearched();
            return cachedLibraries.AsReadOnly();
        }

        public static void Invalidate()
        {
            searched = false;
            cachedStreamingAssets = "";
            cachedLibraries.Clear();
        }

        // The one place that decides which WARUDO a build is checked against: (a) the WARUDO the output path is inside,
        // (b) the "WARUDO folder" setting of this project, (c) the Steam install. Returns its Warudo_Data/StreamingAssets
        // folder, or "" when none has a Managed folder next to it (c is the exception: it only has to exist, as before).
        public static string ResolveForOutput(string outputDirectory, out string source)
        {
            source = "";
            string root;
            if (IsInsideWarudoData(outputDirectory, out root) && ManagedExists(root))
            {
                source = "a";
                return root;
            }

            string configured = WbbPrefs.GetString(RootPrefKey, "");
            if (!string.IsNullOrEmpty(configured))
            {
                root = NormalizeStreamingAssets(configured);
                if (!string.IsNullOrEmpty(root) && ManagedExists(root))
                {
                    source = "b";
                    return root;
                }

                if (!string.Equals(warnedSetting, configured, System.StringComparison.Ordinal))
                {
                    warnedSetting = configured;
                    WbbUtil.LogWarning("The WARUDO folder setting '" + configured + "' has no Warudo_Data/Managed folder, so it is ignored.");
                }
            }

            root = SteamStreamingAssets();
            if (!string.IsNullOrEmpty(root))
                source = "c";
            return root;
        }

        // Accepts the install root, its Warudo_Data folder, Warudo_Data/Managed or its StreamingAssets folder. Returns "" when it is
        // none of them.
        public static string NormalizeStreamingAssets(string path)
        {
            string normalized = WbbUtil.NormalizePath(path);
            if (string.IsNullOrEmpty(normalized))
                return "";
            normalized = normalized.TrimEnd('/');
            string root;
            if (IsInsideWarudoData(normalized + "/", out root) && Directory.Exists(root))
                return root;
            if (normalized.EndsWith("/Warudo_Data/Managed", System.StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(0, normalized.Length - "/Managed".Length);
            if (normalized.EndsWith("/Warudo_Data", System.StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(0, normalized.Length - "/Warudo_Data".Length);
            string candidate = normalized + "/" + DataSuffix;
            return Directory.Exists(candidate) ? candidate : "";
        }

        public static bool ManagedExists(string streamingAssetsRoot)
        {
            if (string.IsNullOrEmpty(streamingAssetsRoot))
                return false;
            try
            {
                string parent = Path.GetDirectoryName(streamingAssetsRoot);
                return !string.IsNullOrEmpty(parent) && Directory.Exists(Path.Combine(parent, "Managed"));
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        // The root for code that has no output path: the running batch's, otherwise the setting, otherwise Steam.
        public static string AmbientStreamingAssets
        {
            get
            {
                if (WbbBuildRunner.AnyRunning && !string.IsNullOrEmpty(BatchStreamingAssets))
                    return BatchStreamingAssets;
                string source;
                return ResolveForOutput("", out source);
            }
        }

        public static bool IsInsideWarudoData(string path, out string streamingAssetsRoot)
        {
            streamingAssetsRoot = "";
            string normalized = WbbUtil.NormalizePath(path);
            if (string.IsNullOrEmpty(normalized))
                return false;
            // The match must end at a folder boundary: ".../Warudo_Data/StreamingAssetsX" is not inside StreamingAssets.
            int index = normalized.IndexOf("/" + DataSuffix, System.StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                int end = index + 1 + DataSuffix.Length;
                if (end == normalized.Length || normalized[end] == '/')
                {
                    streamingAssetsRoot = normalized.Substring(0, end);
                    return true;
                }

                index = normalized.IndexOf("/" + DataSuffix, index + 1, System.StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static string SteamStreamingAssets()
        {
            return StreamingAssetsPath;
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
