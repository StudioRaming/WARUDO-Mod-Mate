using System;
using System.Collections.Generic;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbPackageExporter
    {
        private const string RiskIncludedKey = WbbUtil.PrefPrefix + "Package.RiskIncluded";
        private static readonly HashSet<string> CodeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs",
            ".dll",
            ".asmdef",
            ".asmref",
            ".shader",
            ".cginc",
            ".hlsl",
            ".glslinc",
            ".compute",
            ".shadergraph",
            ".shadersubgraph",
        };
        private static HashSet<string> riskIncluded;
        public static bool IsCodeLike(string assetPath)
        {
            string ext = GetExtension(assetPath);
            return ext.Length > 0 && CodeExtensions.Contains(ext);
        }

        public static List<string> ScriptFolders()
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int t = 0; t < 3; t++)
            {
                try
                {
                    WbbJob saved = WbbBuildSection.LoadSavedJob((WbbExportType)t);
                    AddFolder(result, seen, WbbUtil.ResolveBuildFolder(saved, (WbbExportType)t));
                }
                catch (Exception e)
                {
                    WbbUtil.LogWarning("Could not read the saved script folder: " + e.Message);
                }
            }

            try
            {
                IList<WbbJob> jobs = WbbDatabase.Jobs;
                for (int i = 0; i < jobs.Count; i++)
                {
                    for (int t = 0; t < 3; t++)
                        AddFolder(result, seen, WbbUtil.ResolveBuildFolder(jobs[i], (WbbExportType)t));
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not read preset script folders: " + e.Message);
            }

            return result;
        }

        private static void AddFolder(List<string> list, HashSet<string> seen, string folder)
        {
            string normalized = WbbUtil.NormalizePath(folder);
            if (string.IsNullOrEmpty(normalized) || !seen.Add(normalized))
                return;
            list.Add(normalized);
        }

        public static bool IsInScriptFolder(string assetPath, IList<string> scriptFolders)
        {
            if (scriptFolders == null)
                return false;
            string path = WbbUtil.NormalizePath(assetPath);
            for (int i = 0; i < scriptFolders.Count; i++)
            {
                if (WbbJournal.IsUnder(path, scriptFolders[i]))
                    return true;
            }

            return false;
        }

        public static bool IsRedistributionRisk(string assetPath, IList<string> scriptFolders)
        {
            return IsCodeLike(assetPath) && !IsInScriptFolder(assetPath, scriptFolders);
        }

        public static void ClassifyRedistribution(IList<WbbPackageAsset> assets, IList<string> scriptFolders = null)
        {
            if (assets == null)
                return;
            if (scriptFolders == null)
                scriptFolders = ScriptFolders();
            for (int i = 0; i < assets.Count; i++)
            {
                WbbPackageAsset asset = assets[i];
                if (asset == null)
                    continue;
                bool code = IsCodeLike(asset.assetPath);
                bool inScripts = code && IsInScriptFolder(asset.assetPath, scriptFolders);
                asset.customScript = inScripts;
                asset.redistributionRisk = code && !inScripts && !asset.isRoot;
            }
        }

        private static HashSet<string> RiskIncluded
        {
            get
            {
                if (riskIncluded != null)
                    return riskIncluded;
                riskIncluded = new HashSet<string>(StringComparer.Ordinal);
                // Not migrated: re-including a risky file is a choice for one project. Copying the shared value from 0.1.0 would
                // include the same paid code by default in every other project that imported the same package (same GUIDs).
                string raw = WbbPrefs.GetString(RiskIncludedKey, "", false);
                if (!string.IsNullOrEmpty(raw))
                {
                    string[] parts = raw.Split(';');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        string guid = parts[i] == null ? "" : parts[i].Trim();
                        if (guid.Length > 0)
                            riskIncluded.Add(guid);
                    }
                }

                return riskIncluded;
            }
        }

        public static bool IsRiskReincluded(string guid)
        {
            return !string.IsNullOrEmpty(guid) && RiskIncluded.Contains(guid);
        }

        public static void SetRiskReincluded(string guid, bool on)
        {
            if (string.IsNullOrEmpty(guid))
                return;
            bool changed = on ? RiskIncluded.Add(guid) : RiskIncluded.Remove(guid);
            if (!changed)
                return;
            WbbPrefs.SetString(RiskIncludedKey, string.Join(";", new List<string>(RiskIncluded).ToArray()));
        }

        public static int RiskReincludedCount
        {
            get
            {
                return RiskIncluded.Count;
            }
        }

        public static int ClearRiskReincluded()
        {
            int count = RiskIncluded.Count;
            if (count == 0)
                return 0;
            RiskIncluded.Clear();
            WbbPrefs.SetString(RiskIncludedKey, "");
            return count;
        }

        public static bool ResolveIncluded(WbbPackageAsset asset, ICollection<string> excludedGuids)
        {
            if (asset == null)
                return false;
            if (asset.isRoot)
                return true;
            if (excludedGuids != null && excludedGuids.Contains(asset.guid))
                return false;
            if (asset.redistributionRisk)
                return IsRiskReincluded(asset.guid);
            return true;
        }

        public static bool SetIncluded(WbbPackageAsset asset, bool include, ICollection<string> excludedGuids)
        {
            if (asset == null || asset.isRoot || string.IsNullOrEmpty(asset.guid))
                return false;
            bool before = ResolveIncluded(asset, excludedGuids);
            if (excludedGuids != null)
            {
                if (include)
                    excludedGuids.Remove(asset.guid);
                else if (!excludedGuids.Contains(asset.guid))
                    excludedGuids.Add(asset.guid);
            }

            if (asset.redistributionRisk)
                SetRiskReincluded(asset.guid, include);
            return before != ResolveIncluded(asset, excludedGuids);
        }
    }
}
