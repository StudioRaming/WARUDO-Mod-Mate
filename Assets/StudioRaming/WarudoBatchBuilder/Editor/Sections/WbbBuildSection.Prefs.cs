using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private static string OutputDirKey(WbbExportType type)
        {
            return WbbUtil.PrefPrefix + "OutputDir." + type;
        }

        private WbbExportType CurrentType
        {
            get
            {
                return (WbbExportType)Mathf.Clamp(job.exportType, 0, TypeCount - 1);
            }
        }

        private bool IsPackageMode
        {
            get
            {
                return job.outputMode == (int)WbbOutputMode.UnityPackage;
            }
        }

        private void LoadPrefs()
        {
            activeJobId = WbbPrefs.GetString(Pfx + "ActiveJob", "");
            WbbJob active = WbbDatabase.GetJob(activeJobId);
            job.id = active != null ? active.id : "";
            job.name = active != null ? active.name : "";
            job.prefabFolderGuid = WbbPrefs.GetString(Pfx + "PrefabGuid", "");
            job.prefabFolderPath = WbbPrefs.GetString(Pfx + "PrefabPath", "");
            job.exportType = Mathf.Clamp(WbbPrefs.GetInt(Pfx + "Type", job.exportType), 0, TypeCount - 1);
            job.modVersion = WbbPrefs.GetString(Pfx + "Version", "1.0.0");
            job.modAuthor = WbbPrefs.GetString(Pfx + "Author", "");
            job.modDescription = WbbPrefs.GetString(Pfx + "Description", "");
            job.outputDirectory = WbbPrefs.GetString(OutputDirKey(CurrentType), WbbUtil.DefaultOutputPath(CurrentType));
            job.appendDateSuffix = WbbPrefs.GetBool(Pfx + "AppendDate", false);
            job.overwriteExisting = WbbPrefs.GetBool(Pfx + "Overwrite", true);
            job.outputMode = Mathf.Clamp(WbbPrefs.GetInt(Pfx + "OutputMode", 0), 0, OutputModeCount - 1);
            NoteOutputDirSeen();
            job.stripVrcComponents = WbbPrefs.GetBool(Pfx + "StripVrc", true);
            job.manualScriptFolder = WbbListSettings.ManualScriptFolder;
            LoadManualNames();
            LoadScriptFolders(job);
            job.useFolderNamePrefix = WbbPrefs.GetBool(Pfx + "UseFolderPrefix", true);
            job.namePrefix = WbbPrefs.GetString(Pfx + "NamePrefix", "");
            job.recurseSubFolders = true;
            SyncHiddenToJob();
            view = Mathf.Clamp(WbbPrefs.GetInt(Pfx + "View", ViewList), ViewGrid, ViewList);
            sortMode = Mathf.Clamp(WbbPrefs.GetInt(Pfx + "Sort", SortName), SortName, SortState);
            showHidden = WbbPrefs.GetBool(Pfx + "ShowHidden", false);
            treeWidth = Mathf.Clamp(WbbPrefs.GetFloat(Pfx + "TreeWidth", treeWidth), MinTreeWidth, MaxTreeWidth);
            treeVisible = WbbPrefs.GetBool(Pfx + "TreeVisible", true);
            settingsOpen = WbbPrefs.GetBool(Pfx + "SettingsOpen", false);
            advancedOpen = WbbPrefs.GetBool(Pfx + "AdvancedOpen", false);
            string scope = WbbUtil.ResolveFolder(job.prefabFolderGuid, job.prefabFolderPath);
            scopeFolder = WbbUtil.IsValidProjectFolder(scope) ? scope : "Assets";
            job.prefabFolderPath = scopeFolder;
            job.prefabFolderGuid = WbbUtil.FolderGuid(scopeFolder);
            collapsed.Clear();
            string raw = WbbPrefs.GetString(Pfx + "Collapsed", "");
            if (!string.IsNullOrEmpty(raw))
            {
                string[] parts = raw.Split('\n');
                for (int i = 0; i < parts.Length; i++)
                    if (!string.IsNullOrEmpty(parts[i]))
                        collapsed.Add(parts[i]);
            }
        }

        private void SavePrefs()
        {
            WbbPrefs.SetString(Pfx + "ActiveJob", activeJobId == null ? "" : activeJobId);
            SaveScriptFolders(job);
            WbbListSettings.ManualScriptFolder = job.manualScriptFolder;
            WbbPrefs.SetString(Pfx + "PrefabGuid", job.prefabFolderGuid == null ? "" : job.prefabFolderGuid);
            WbbPrefs.SetString(Pfx + "PrefabPath", job.prefabFolderPath == null ? "" : job.prefabFolderPath);
            // While a rebuild borrows the record's settings, the borrowed values are not the user's defaults.
            if (rebuildBorrow == null)
            {
                WbbPrefs.SetInt(Pfx + "Type", Mathf.Clamp(job.exportType, 0, TypeCount - 1));
                WbbPrefs.SetString(Pfx + "Version", job.modVersion == null ? "" : job.modVersion);
                WbbPrefs.SetString(Pfx + "Author", job.modAuthor == null ? "" : job.modAuthor);
                WbbPrefs.SetInt(Pfx + "OutputMode", job.outputMode);
            }

            WbbPrefs.SetString(Pfx + "Description", job.modDescription == null ? "" : job.modDescription);
            PersistOutputDirIfChanged();
            WbbPrefs.SetBool(Pfx + "AppendDate", job.appendDateSuffix);
            WbbPrefs.SetBool(Pfx + "Overwrite", job.overwriteExisting);
            WbbPrefs.SetBool(Pfx + "StripVrc", job.stripVrcComponents);
            WbbPrefs.SetBool(Pfx + "UseFolderPrefix", job.useFolderNamePrefix);
            WbbPrefs.SetString(Pfx + "NamePrefix", job.namePrefix == null ? "" : job.namePrefix);
            WbbPrefs.SetInt(Pfx + "View", view);
            WbbPrefs.SetInt(Pfx + "Sort", sortMode);
            WbbPrefs.SetBool(Pfx + "ShowHidden", showHidden);
            WbbPrefs.SetBool(Pfx + "TreeVisible", treeVisible);
            WbbPrefs.SetBool(Pfx + "SettingsOpen", settingsOpen);
            WbbPrefs.SetBool(Pfx + "AdvancedOpen", advancedOpen);
            SaveCollapsed();
        }

        private void SaveCollapsed()
        {
            WbbPrefs.SetString(Pfx + "Collapsed", string.Join("\n", new List<string>(collapsed).ToArray()));
        }

        public void FlushPrefs()
        {
            SavePrefs();
        }

        internal static WbbJob LoadSavedJob(WbbExportType type)
        {
            var saved = new WbbJob();
            saved.exportType = (int)type;
            saved.outputMode = (int)WbbOutputMode.WarudoMod;
            saved.modVersion = WbbPrefs.GetString(Pfx + "Version", "1.0.0");
            saved.modAuthor = WbbPrefs.GetString(Pfx + "Author", "");
            saved.modDescription = WbbPrefs.GetString(Pfx + "Description", "");
            saved.outputDirectory = WbbPrefs.GetString(OutputDirKey(type), WbbUtil.DefaultOutputPath(type));
            saved.stripVrcComponents = WbbPrefs.GetBool(Pfx + "StripVrc", true);
            saved.manualScriptFolder = WbbListSettings.ManualScriptFolder;
            LoadScriptFolders(saved);
            return saved;
        }

        private static void LoadScriptFolders(WbbJob target)
        {
            string shared = WbbListSettings.GetScriptFolder(WbbListSettings.SharedScriptFolder);
            target.buildFolderPath = shared;
            target.buildFolderGuid = WbbUtil.FolderGuid(shared);
            for (int t = 0; t < TypeCount; t++)
            {
                var type = (WbbExportType)t;
                WbbUtil.SetBuildFolderForType(target, type, WbbListSettings.GetScriptFolder(type.ToString()));
            }
        }

        private static void SaveScriptFolders(WbbJob source)
        {
            WbbListSettings.SetScriptFolder(WbbListSettings.SharedScriptFolder, source.buildFolderGuid, source.buildFolderPath);
            for (int t = 0; t < TypeCount; t++)
            {
                WbbListSettings.SetScriptFolder(((WbbExportType)t).ToString(), ListItem(source.buildFolderGuidsByType, t), ListItem(source.buildFolderPathsByType, t));
            }
        }

        private static string ListItem(List<string> list, int index)
        {
            if (list == null || index < 0 || index >= list.Count || list[index] == null)
                return "";
            return list[index];
        }

        private void SyncHiddenToJob()
        {
            job.excludedPrefabGuids = WbbListSettings.HiddenGuids;
        }

        [System.Serializable]
        private class ManualNameStore
        {
            public List<string> guids = new List<string>();
            public List<string> names = new List<string>();
        }

        private void SaveManualNames()
        {
            var store = new ManualNameStore();
            if (job.manualNameGuids != null)
                store.guids = new List<string>(job.manualNameGuids);
            if (job.manualNames != null)
                store.names = new List<string>(job.manualNames);
            try
            {
                WbbPrefs.SetString(Pfx + "ManualNames", JsonUtility.ToJson(store));
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Failed to save manual names: " + e.Message);
            }
        }

        private void LoadManualNames()
        {
            job.manualNameGuids = new List<string>();
            job.manualNames = new List<string>();
            string json = WbbPrefs.GetString(Pfx + "ManualNames", "");
            if (string.IsNullOrEmpty(json))
                return;
            try
            {
                var store = JsonUtility.FromJson<ManualNameStore>(json);
                if (store == null)
                    return;
                if (store.guids != null)
                    job.manualNameGuids = new List<string>(store.guids);
                if (store.names != null)
                    job.manualNames = new List<string>(store.names);
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Manual name store is corrupt, ignoring: " + e.Message);
            }
        }
    }
}
