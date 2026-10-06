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
            activeJobId = EditorPrefs.GetString(Pfx + "ActiveJob", "");
            WbbJob active = WbbDatabase.GetJob(activeJobId);
            job.id = active != null ? active.id : "";
            job.name = active != null ? active.name : "";
            job.prefabFolderGuid = EditorPrefs.GetString(Pfx + "PrefabGuid", "");
            job.prefabFolderPath = EditorPrefs.GetString(Pfx + "PrefabPath", "");
            job.exportType = Mathf.Clamp(EditorPrefs.GetInt(Pfx + "Type", job.exportType), 0, TypeCount - 1);
            job.modVersion = EditorPrefs.GetString(Pfx + "Version", "1.0.0");
            job.modAuthor = EditorPrefs.GetString(Pfx + "Author", "");
            job.modDescription = EditorPrefs.GetString(Pfx + "Description", "");
            job.outputDirectory = EditorPrefs.GetString(OutputDirKey(CurrentType), WbbUtil.DefaultOutputPath(CurrentType));
            job.appendDateSuffix = EditorPrefs.GetBool(Pfx + "AppendDate", false);
            job.overwriteExisting = EditorPrefs.GetBool(Pfx + "Overwrite", true);
            job.outputMode = Mathf.Clamp(EditorPrefs.GetInt(Pfx + "OutputMode", 0), 0, OutputModeCount - 1);
            job.stripVrcComponents = EditorPrefs.GetBool(Pfx + "StripVrc", true);
            job.manualScriptFolder = WbbListSettings.ManualScriptFolder;
            LoadManualNames();
            LoadScriptFolders(job);
            job.useFolderNamePrefix = EditorPrefs.GetBool(Pfx + "UseFolderPrefix", true);
            job.namePrefix = EditorPrefs.GetString(Pfx + "NamePrefix", "");
            job.recurseSubFolders = true;
            SyncHiddenToJob();
            view = Mathf.Clamp(EditorPrefs.GetInt(Pfx + "View", ViewList), ViewGrid, ViewList);
            sortMode = Mathf.Clamp(EditorPrefs.GetInt(Pfx + "Sort", SortName), SortName, SortState);
            showHidden = EditorPrefs.GetBool(Pfx + "ShowHidden", false);
            treeWidth = Mathf.Clamp(EditorPrefs.GetFloat(Pfx + "TreeWidth", treeWidth), MinTreeWidth, MaxTreeWidth);
            treeVisible = EditorPrefs.GetBool(Pfx + "TreeVisible", true);
            settingsOpen = EditorPrefs.GetBool(Pfx + "SettingsOpen", false);
            advancedOpen = EditorPrefs.GetBool(Pfx + "AdvancedOpen", false);
            string scope = WbbUtil.ResolveFolder(job.prefabFolderGuid, job.prefabFolderPath);
            scopeFolder = WbbUtil.IsValidProjectFolder(scope) ? scope : "Assets";
            job.prefabFolderPath = scopeFolder;
            job.prefabFolderGuid = WbbUtil.FolderGuid(scopeFolder);
            collapsed.Clear();
            string raw = EditorPrefs.GetString(Pfx + "Collapsed", "");
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
            EditorPrefs.SetString(Pfx + "ActiveJob", activeJobId == null ? "" : activeJobId);
            SaveScriptFolders(job);
            WbbListSettings.ManualScriptFolder = job.manualScriptFolder;
            EditorPrefs.SetString(Pfx + "PrefabGuid", job.prefabFolderGuid == null ? "" : job.prefabFolderGuid);
            EditorPrefs.SetString(Pfx + "PrefabPath", job.prefabFolderPath == null ? "" : job.prefabFolderPath);
            EditorPrefs.SetInt(Pfx + "Type", Mathf.Clamp(job.exportType, 0, TypeCount - 1));
            EditorPrefs.SetString(Pfx + "Version", job.modVersion == null ? "" : job.modVersion);
            EditorPrefs.SetString(Pfx + "Author", job.modAuthor == null ? "" : job.modAuthor);
            EditorPrefs.SetString(Pfx + "Description", job.modDescription == null ? "" : job.modDescription);
            if (!string.IsNullOrEmpty(job.outputDirectory))
                EditorPrefs.SetString(OutputDirKey(CurrentType), job.outputDirectory);
            EditorPrefs.SetBool(Pfx + "AppendDate", job.appendDateSuffix);
            EditorPrefs.SetBool(Pfx + "Overwrite", job.overwriteExisting);
            EditorPrefs.SetInt(Pfx + "OutputMode", job.outputMode);
            EditorPrefs.SetBool(Pfx + "StripVrc", job.stripVrcComponents);
            EditorPrefs.SetBool(Pfx + "UseFolderPrefix", job.useFolderNamePrefix);
            EditorPrefs.SetString(Pfx + "NamePrefix", job.namePrefix == null ? "" : job.namePrefix);
            EditorPrefs.SetInt(Pfx + "View", view);
            EditorPrefs.SetInt(Pfx + "Sort", sortMode);
            EditorPrefs.SetBool(Pfx + "ShowHidden", showHidden);
            EditorPrefs.SetBool(Pfx + "TreeVisible", treeVisible);
            EditorPrefs.SetBool(Pfx + "SettingsOpen", settingsOpen);
            EditorPrefs.SetBool(Pfx + "AdvancedOpen", advancedOpen);
            SaveCollapsed();
        }

        private void SaveCollapsed()
        {
            EditorPrefs.SetString(Pfx + "Collapsed", string.Join("\n", new List<string>(collapsed).ToArray()));
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
            saved.modVersion = EditorPrefs.GetString(Pfx + "Version", "1.0.0");
            saved.modAuthor = EditorPrefs.GetString(Pfx + "Author", "");
            saved.modDescription = EditorPrefs.GetString(Pfx + "Description", "");
            saved.outputDirectory = EditorPrefs.GetString(OutputDirKey(type), WbbUtil.DefaultOutputPath(type));
            saved.stripVrcComponents = EditorPrefs.GetBool(Pfx + "StripVrc", true);
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
                EditorPrefs.SetString(Pfx + "ManualNames", JsonUtility.ToJson(store));
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
            string json = EditorPrefs.GetString(Pfx + "ManualNames", "");
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
