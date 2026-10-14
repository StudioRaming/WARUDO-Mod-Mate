using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbListSettings
    {
        private const string FileName = "WarudoBatchBuilder.List.json";
        private const string LegacyExcludedKey = WbbUtil.PrefPrefix + "Build.Excluded";
        private const string LegacyScriptFolderPrefix = WbbUtil.PrefPrefix + "StagingDir.";
        private const string LegacySharedGuidKey = WbbUtil.PrefPrefix + "Build.BuildGuid";
        private const string LegacySharedPathKey = WbbUtil.PrefPrefix + "Build.BuildPath";
        public const string SharedScriptFolder = "Shared";
        [Serializable]
        private class Data
        {
            public int version = 1;
            public bool manualScriptFolder;
            public List<string> workFolderGuids = new List<string>();
            public List<string> workFolderPaths = new List<string>();
            public List<string> pinned = new List<string>();
            public List<string> hidden = new List<string>();
            public List<string> hiddenScene = new List<string>();
            public bool suggestionAnswered;
            public bool legacyMigrated;
            public List<string> scriptFolderSlots = new List<string>();
            public List<string> scriptFolderGuids = new List<string>();
            public List<string> scriptFolderPaths = new List<string>();
        }

        private static Data data;
        private static readonly HashSet<string> pinnedSet = new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> hiddenSet = new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> hiddenSceneSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<string> resolvedWorkFolders = new List<string>();
        private static bool resolvedDirty = true;
        public static int Version { get; private set; }

        public static string FilePath
        {
            get
            {
                return WbbDatabase.StorageDirectory + "/" + FileName;
            }
        }

        public static bool ManualScriptFolder
        {
            get
            {
                EnsureLoaded();
                return data.manualScriptFolder;
            }

            set
            {
                EnsureLoaded();
                if (data.manualScriptFolder == value)
                    return;
                data.manualScriptFolder = value;
                Changed();
            }
        }

        public static IList<string> WorkFolders
        {
            get
            {
                EnsureLoaded();
                if (resolvedDirty)
                {
                    resolvedDirty = false;
                    resolvedWorkFolders.Clear();
                    int count = Mathf.Min(data.workFolderGuids.Count, data.workFolderPaths.Count);
                    for (int i = 0; i < count; i++)
                    {
                        string path = WbbUtil.ResolveFolder(data.workFolderGuids[i], data.workFolderPaths[i]);
                        if (!string.IsNullOrEmpty(path) && !Contains(resolvedWorkFolders, path))
                            resolvedWorkFolders.Add(path);
                    }
                }

                return resolvedWorkFolders;
            }
        }

        public static bool SuggestionAnswered
        {
            get
            {
                EnsureLoaded();
                return data.suggestionAnswered;
            }

            set
            {
                EnsureLoaded();
                if (data.suggestionAnswered == value)
                    return;
                data.suggestionAnswered = value;
                Changed();
            }
        }

        public static bool IsInWorkFolder(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
            IList<string> folders = WorkFolders;
            for (int i = 0; i < folders.Count; i++)
            {
                if (IsUnder(assetPath, folders[i]))
                    return true;
            }

            return false;
        }

        public static bool IsWorkFolder(string folderPath)
        {
            return Contains(WorkFolders, WbbUtil.NormalizePath(folderPath));
        }

        public static void AddWorkFolder(string folderPath)
        {
            folderPath = WbbUtil.NormalizePath(folderPath);
            if (!WbbUtil.IsValidProjectFolder(folderPath) || IsWorkFolder(folderPath))
                return;
            EnsureLoaded();
            data.workFolderGuids.Add(WbbUtil.FolderGuid(folderPath));
            data.workFolderPaths.Add(folderPath);
            Changed();
        }

        public static void RemoveWorkFolder(string folderPath)
        {
            folderPath = WbbUtil.NormalizePath(folderPath);
            EnsureLoaded();
            int count = Mathf.Min(data.workFolderGuids.Count, data.workFolderPaths.Count);
            for (int i = count - 1; i >= 0; i--)
            {
                // Match the resolved path only. The stored path is not updated when a folder is renamed, so it can
                // name a new folder created later at the old path while the GUID points to the renamed one.
                string resolved = WbbUtil.ResolveFolder(data.workFolderGuids[i], data.workFolderPaths[i]);
                if (string.Equals(resolved, folderPath, StringComparison.OrdinalIgnoreCase))
                {
                    data.workFolderGuids.RemoveAt(i);
                    data.workFolderPaths.RemoveAt(i);
                }
            }

            Changed();
        }

        public static bool IsPinned(string guid)
        {
            EnsureLoaded();
            return !string.IsNullOrEmpty(guid) && pinnedSet.Contains(guid);
        }

        public static bool IsHidden(string guid)
        {
            EnsureLoaded();
            return !string.IsNullOrEmpty(guid) && hiddenSet.Contains(guid);
        }

        public static bool IsSceneHidden(string key)
        {
            EnsureLoaded();
            return !string.IsNullOrEmpty(key) && hiddenSceneSet.Contains(key);
        }

        public static List<string> HiddenGuids
        {
            get
            {
                EnsureLoaded();
                return new List<string>(data.hidden);
            }
        }

        public static void SetPinned(string guid, bool value)
        {
            if (string.IsNullOrEmpty(guid))
                return;
            EnsureLoaded();
            if (value && hiddenSet.Remove(guid))
                data.hidden.Remove(guid);
            if (!SetMember(pinnedSet, data.pinned, guid, value))
                return;
            Changed();
        }

        public static void SetHidden(string guid, bool value)
        {
            if (string.IsNullOrEmpty(guid))
                return;
            EnsureLoaded();
            if (value && pinnedSet.Remove(guid))
                data.pinned.Remove(guid);
            if (!SetMember(hiddenSet, data.hidden, guid, value))
                return;
            Changed();
        }

        public static void AddHidden(IList<string> guids)
        {
            if (guids == null || guids.Count == 0)
                return;
            EnsureLoaded();
            bool changed = false;
            for (int i = 0; i < guids.Count; i++)
            {
                if (string.IsNullOrEmpty(guids[i]))
                    continue;
                if (pinnedSet.Contains(guids[i]))
                    continue;
                changed |= SetMember(hiddenSet, data.hidden, guids[i], true);
            }

            if (changed)
                Changed();
        }

        public static void SetSceneHidden(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
                return;
            EnsureLoaded();
            if (!SetMember(hiddenSceneSet, data.hiddenScene, key, value))
                return;
            Changed();
        }

        public static string GetScriptFolder(string slot)
        {
            if (string.IsNullOrEmpty(slot))
                return "";
            EnsureLoaded();
            int index = data.scriptFolderSlots.IndexOf(slot);
            if (index < 0)
                index = MigrateScriptFolder(slot);
            return WbbUtil.ResolveFolder(data.scriptFolderGuids[index], data.scriptFolderPaths[index]);
        }

        public static void SetScriptFolder(string slot, string guid, string folderPath)
        {
            if (string.IsNullOrEmpty(slot))
                return;
            EnsureLoaded();
            string path = WbbUtil.NormalizePath(folderPath);
            if (path.Length == 0)
                guid = "";
            if (guid == null)
                guid = "";
            int index = data.scriptFolderSlots.IndexOf(slot);
            if (index < 0)
            {
                data.scriptFolderSlots.Add(slot);
                data.scriptFolderGuids.Add(guid);
                data.scriptFolderPaths.Add(path);
            }
            else
            {
                if (string.Equals(data.scriptFolderGuids[index], guid, StringComparison.Ordinal) && string.Equals(data.scriptFolderPaths[index], path, StringComparison.Ordinal))
                    return;
                data.scriptFolderGuids[index] = guid;
                data.scriptFolderPaths[index] = path;
            }

            Save();
        }

        private static int MigrateScriptFolder(string slot)
        {
            string legacy = slot == SharedScriptFolder ? WbbUtil.ResolveFolder(EditorPrefs.GetString(LegacySharedGuidKey, ""), EditorPrefs.GetString(LegacySharedPathKey, "")) : WbbUtil.ResolveFolder("", EditorPrefs.GetString(LegacyScriptFolderPrefix + slot, ""));
            data.scriptFolderSlots.Add(slot);
            data.scriptFolderGuids.Add(WbbUtil.FolderGuid(legacy));
            data.scriptFolderPaths.Add(legacy);
            Save();
            if (legacy.Length > 0)
                WbbUtil.Log("Script folder (" + slot + ") was carried over to this project's settings from the previous shared setting: " + legacy);
            return data.scriptFolderSlots.Count - 1;
        }

        public static void EnsureLoaded()
        {
            if (data != null)
                return;
            data = Load();
            pinnedSet.Clear();
            hiddenSet.Clear();
            hiddenSceneSet.Clear();
            for (int i = 0; i < data.pinned.Count; i++)
                if (!string.IsNullOrEmpty(data.pinned[i]))
                    pinnedSet.Add(data.pinned[i]);
            for (int i = 0; i < data.hidden.Count; i++)
                if (!string.IsNullOrEmpty(data.hidden[i]))
                    hiddenSet.Add(data.hidden[i]);
            for (int i = 0; i < data.hiddenScene.Count; i++)
                if (!string.IsNullOrEmpty(data.hiddenScene[i]))
                    hiddenSceneSet.Add(data.hiddenScene[i]);
            resolvedDirty = true;
            MigrateLegacyExcluded();
        }

        private static Data Load()
        {
            string path = FilePath;
            try
            {
                if (!File.Exists(path))
                    return new Data();
                var loaded = JsonUtility.FromJson<Data>(File.ReadAllText(path));
                return Normalize(loaded);
            }
            catch (Exception e)
            {
                try
                {
                    File.Copy(path, path + ".corrupt-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json", true);
                }
                catch (Exception)
                {
                }

                WbbUtil.LogWarning("List settings could not be read; starting empty (a .corrupt copy was kept): " + e.Message);
                return new Data();
            }
        }

        private static Data Normalize(Data d)
        {
            if (d == null)
                d = new Data();
            if (d.workFolderGuids == null)
                d.workFolderGuids = new List<string>();
            if (d.workFolderPaths == null)
                d.workFolderPaths = new List<string>();
            while (d.workFolderGuids.Count < d.workFolderPaths.Count)
                d.workFolderGuids.Add("");
            while (d.workFolderPaths.Count < d.workFolderGuids.Count)
                d.workFolderPaths.Add("");
            if (d.pinned == null)
                d.pinned = new List<string>();
            if (d.hidden == null)
                d.hidden = new List<string>();
            if (d.hiddenScene == null)
                d.hiddenScene = new List<string>();
            if (d.scriptFolderSlots == null)
                d.scriptFolderSlots = new List<string>();
            if (d.scriptFolderGuids == null)
                d.scriptFolderGuids = new List<string>();
            if (d.scriptFolderPaths == null)
                d.scriptFolderPaths = new List<string>();
            while (d.scriptFolderGuids.Count < d.scriptFolderSlots.Count)
                d.scriptFolderGuids.Add("");
            while (d.scriptFolderPaths.Count < d.scriptFolderSlots.Count)
                d.scriptFolderPaths.Add("");
            return d;
        }

        private static void Save()
        {
            try
            {
                WbbDatabase.WriteAtomic(FilePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Failed to save list settings: " + e.Message);
            }
        }

        private static void MigrateLegacyExcluded()
        {
            if (data.legacyMigrated)
                return;
            data.legacyMigrated = true;
            string raw = EditorPrefs.GetString(LegacyExcludedKey, "");
            if (!string.IsNullOrEmpty(raw))
            {
                string[] parts = raw.Split(';');
                for (int i = 0; i < parts.Length; i++)
                {
                    string guid = parts[i];
                    if (string.IsNullOrEmpty(guid))
                        continue;
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                        continue;
                    SetMember(hiddenSet, data.hidden, guid, true);
                }
            }

            Save();
            Version++;
        }

        private static void Changed()
        {
            resolvedDirty = true;
            Version++;
            Save();
        }

        private static bool SetMember(HashSet<string> set, List<string> list, string value, bool member)
        {
            if (member)
            {
                if (!set.Add(value))
                    return false;
                list.Add(value);
                return true;
            }

            if (!set.Remove(value))
                return false;
            list.RemoveAll(delegate (string s)
            {
                return string.Equals(s, value, StringComparison.OrdinalIgnoreCase);
            });
            return true;
        }

        private static bool Contains(IList<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], value, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool IsUnder(string path, string folder)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(folder))
                return false;
            if (string.Equals(path, folder, StringComparison.OrdinalIgnoreCase))
                return true;
            return path.Length > folder.Length && path[folder.Length] == '/' && path.StartsWith(folder, StringComparison.OrdinalIgnoreCase);
        }
    }
}
