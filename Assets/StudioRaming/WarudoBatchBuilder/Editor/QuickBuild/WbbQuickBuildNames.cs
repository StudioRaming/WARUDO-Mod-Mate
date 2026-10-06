using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    internal static class WbbQuickBuildNames
    {
        private const string FileName = "WarudoBatchBuilder.QuickNames.json";
        private const int MaxEntries = 1000;
        [Serializable]
        private class Entry
        {
            public string objectId = "";
            public string scenePath = "";
            public string hierarchyPath = "";
            public string prefabGuid = "";
            public string modName = "";
            public int exportType = (int)WbbExportType.Prop;
            public string updatedIso = "";
        }

        [Serializable]
        private class Store
        {
            public List<Entry> items = new List<Entry>();
        }

        private static Store store;
        private static string FilePath
        {
            get
            {
                return WbbDatabase.StorageDirectory + "/" + FileName;
            }
        }

        public static string Find(WbbQuickBuild.SourceKey key)
        {
            EnsureLoaded();
            List<Entry> items = store.items;
            if (!string.IsNullOrEmpty(key.objectId))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.Equals(items[i].objectId, key.objectId, StringComparison.Ordinal))
                        return items[i].modName;
                }
            }

            if (!string.IsNullOrEmpty(key.hierarchyPath))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (SamePlace(items[i], key))
                        return items[i].modName;
                }
            }

            if (!string.IsNullOrEmpty(key.prefabGuid))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (string.Equals(items[i].prefabGuid, key.prefabGuid, StringComparison.Ordinal))
                        return items[i].modName;
                }
            }

            return "";
        }

        public static void Remember(WbbQuickBuild.SourceKey key, string modName, WbbExportType type)
        {
            if (string.IsNullOrEmpty(modName))
                return;
            if (string.IsNullOrEmpty(key.objectId) && string.IsNullOrEmpty(key.hierarchyPath))
                return;
            EnsureLoaded();
            Entry entry = FindSameObject(key);
            if (entry != null)
                store.items.Remove(entry);
            else
                entry = new Entry();
            entry.objectId = key.objectId ?? "";
            entry.scenePath = key.scenePath ?? "";
            entry.hierarchyPath = key.hierarchyPath ?? "";
            entry.prefabGuid = key.prefabGuid ?? "";
            entry.modName = modName;
            entry.exportType = (int)type;
            entry.updatedIso = WbbUtil.NowIso();
            store.items.Insert(0, entry);
            while (store.items.Count > MaxEntries)
                store.items.RemoveAt(store.items.Count - 1);
            Save();
        }

        private static Entry FindSameObject(WbbQuickBuild.SourceKey key)
        {
            List<Entry> items = store.items;
            for (int i = 0; i < items.Count; i++)
            {
                Entry item = items[i];
                bool bothHaveId = !string.IsNullOrEmpty(item.objectId) && !string.IsNullOrEmpty(key.objectId);
                if (bothHaveId)
                {
                    if (string.Equals(item.objectId, key.objectId, StringComparison.Ordinal))
                        return item;
                    continue;
                }

                if (SamePlace(item, key))
                    return item;
            }

            return null;
        }

        private static bool SamePlace(Entry item, WbbQuickBuild.SourceKey key)
        {
            return !string.IsNullOrEmpty(key.hierarchyPath) && string.Equals(item.hierarchyPath, key.hierarchyPath, StringComparison.Ordinal) && string.Equals(item.scenePath ?? "", key.scenePath ?? "", StringComparison.OrdinalIgnoreCase);
        }

        private static void EnsureLoaded()
        {
            if (store != null)
                return;
            store = new Store();
            string path = FilePath;
            try
            {
                if (!File.Exists(path))
                    return;
                var loaded = JsonUtility.FromJson<Store>(File.ReadAllText(path, Encoding.UTF8));
                if (loaded == null || loaded.items == null)
                    return;
                for (int i = loaded.items.Count - 1; i >= 0; i--)
                {
                    Entry item = loaded.items[i];
                    if (item == null || string.IsNullOrEmpty(item.modName))
                    {
                        loaded.items.RemoveAt(i);
                        continue;
                    }

                    if (item.objectId == null)
                        item.objectId = "";
                    if (item.scenePath == null)
                        item.scenePath = "";
                    if (item.hierarchyPath == null)
                        item.hierarchyPath = "";
                    if (item.prefabGuid == null)
                        item.prefabGuid = "";
                    if (item.updatedIso == null)
                        item.updatedIso = "";
                }

                store = loaded;
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Quick build name store is unreadable, starting empty: " + path + " (" + e.Message + ")");
            }
        }

        private static void Save()
        {
            try
            {
                WbbDatabase.WriteAtomic(FilePath, JsonUtility.ToJson(store, true));
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Failed to serialize quick build names: " + e.Message);
            }
        }
    }
}
