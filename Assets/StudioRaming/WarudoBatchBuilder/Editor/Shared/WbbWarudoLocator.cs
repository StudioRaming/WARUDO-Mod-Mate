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
        // The last Steam search and the generation it was made for. Invalidate moves the generation on instead of clearing the
        // result, so the pipeline badge's worker thread and the main thread can both search: each search builds its own result
        // and only swaps the reference in, and a result made for an older generation is searched again.
        private static SteamSearch steamSearch;
        private static int searchGeneration;
        private static readonly Regex PathEntry = new Regex("\"path\"\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase);
        private static readonly Regex LegacyEntry = new Regex("^\\s*\"\\d+\"\\s+\"([^\"]+)\"\\s*$", RegexOptions.Multiline);
        private static readonly Regex InstallDirEntry = new Regex("\"installdir\"\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase);
        public static string StreamingAssetsPath
        {
            get
            {
                if (DisableSteamLookupForTests)
                    return "";
                return Searched().streamingAssets;
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
            return Searched().libraries;
        }

        public static void Invalidate()
        {
            System.Threading.Interlocked.Increment(ref searchGeneration);
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
                root = SettingRoot(configured);
                if (root.Length > 0)
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

        // The WARUDO folder setting's StreamingAssets folder, or "" when the setting is empty or has no Managed folder next to it.
        private static string SettingRoot(string configured)
        {
            if (string.IsNullOrEmpty(configured))
                return "";
            string root = NormalizeStreamingAssets(configured);
            return !string.IsNullOrEmpty(root) && ManagedExists(root) ? root : "";
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

        // What AmbientStreamingAssets decides from, taken without touching the disk: the running batch's root, the WARUDO
        // folder setting, and whether and which Steam search applies. Main thread only (EditorPrefs and the batch state).
        public sealed class AmbientInputs
        {
            public readonly string batch;
            public readonly string setting;
            public readonly bool steamDisabled;
            public readonly int steamGeneration;

            public AmbientInputs(string batch, string setting, bool steamDisabled, int steamGeneration)
            {
                this.batch = batch ?? "";
                this.setting = setting ?? "";
                this.steamDisabled = steamDisabled;
                this.steamGeneration = steamGeneration;
            }

            public bool SameAs(AmbientInputs other)
            {
                return other != null && string.Equals(batch, other.batch, System.StringComparison.Ordinal)
                    && string.Equals(setting, other.setting, System.StringComparison.Ordinal)
                    && steamDisabled == other.steamDisabled && steamGeneration == other.steamGeneration;
            }
        }

        public static AmbientInputs CaptureAmbient()
        {
            return new AmbientInputs(WbbBuildRunner.AnyRunning ? BatchStreamingAssets : "", WbbPrefs.GetString(RootPrefKey, ""),
                DisableSteamLookupForTests, System.Threading.Volatile.Read(ref searchGeneration));
        }

        // The disk half of AmbientStreamingAssets, in the same order: Steam is searched only when the batch root and the setting
        // give nothing. Safe off the main thread: it checks folders, and the Steam search it may run swaps its result in whole.
        // It does not report an unusable setting; the preflight does, through ResolveForOutput.
        public static string ResolveAmbient(AmbientInputs inputs)
        {
            if (inputs.batch.Length > 0)
                return inputs.batch;
            string root = SettingRoot(inputs.setting);
            if (root.Length > 0)
                return root;
            return inputs.steamDisabled ? "" : Searched().streamingAssets;
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

        // One finished Steam search. Never changed after it is made.
        private sealed class SteamSearch
        {
            public readonly int generation;
            public readonly string streamingAssets;
            public readonly IList<string> libraries;

            public SteamSearch(int generation, string streamingAssets, List<string> libraries)
            {
                this.generation = generation;
                this.streamingAssets = streamingAssets;
                this.libraries = libraries.AsReadOnly();
            }
        }

        private static SteamSearch Searched()
        {
            int generation = System.Threading.Volatile.Read(ref searchGeneration);
            SteamSearch search = System.Threading.Volatile.Read(ref steamSearch);
            if (search != null && search.generation == generation)
                return search;
            search = Search(generation);
            System.Threading.Volatile.Write(ref steamSearch, search);
            return search;
        }

        private static SteamSearch Search(int generation)
        {
            string found = "";
            var libraries = new List<string>();
            try
            {
                string steam = SteamRoot();
                if (!string.IsNullOrEmpty(steam))
                {
                    AddLibrary(libraries, steam);
                    string vdf = steam + "/steamapps/libraryfolders.vdf";
                    if (File.Exists(vdf))
                        ParseLibraryFolders(libraries, File.ReadAllText(vdf));
                    for (int i = 0; i < libraries.Count; i++)
                    {
                        string candidate = CandidateFor(libraries[i]);
                        if (!string.IsNullOrEmpty(candidate) && Directory.Exists(candidate))
                        {
                            found = candidate;
                            break;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("WARUDO install lookup failed: " + e.Message);
            }

            return new SteamSearch(generation, found, libraries);
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

        private static void ParseLibraryFolders(List<string> libraries, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            foreach (Match match in PathEntry.Matches(text))
                AddLibrary(libraries, Unescape(match.Groups[1].Value));
            foreach (Match match in LegacyEntry.Matches(text))
            {
                string value = Unescape(match.Groups[1].Value);
                if (value.IndexOf(':') > 0 || value.StartsWith("/"))
                    AddLibrary(libraries, value);
            }
        }

        private static string Unescape(string vdfValue)
        {
            return vdfValue == null ? "" : vdfValue.Replace("\\\\", "\\");
        }

        private static void AddLibrary(List<string> libraries, string path)
        {
            string normalized = WbbUtil.NormalizePath(path);
            if (string.IsNullOrEmpty(normalized))
                return;
            for (int i = 0; i < libraries.Count; i++)
            {
                if (string.Equals(libraries[i], normalized, System.StringComparison.OrdinalIgnoreCase))
                    return;
            }

            libraries.Add(normalized);
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
