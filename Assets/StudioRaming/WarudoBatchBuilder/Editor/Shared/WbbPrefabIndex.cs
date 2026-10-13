using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbPrefabIndex
    {
        [Flags]
        public enum Signal
        {
            None = 0,
            Physics = 1,
        }

        public sealed class Entry
        {
            public string guid = "";
            public string path = "";
            public string folder = "";
            public string name = "";
            public long size;
            public long ticks;
            public Signal signals;
            public bool inCode;
            public bool computed;
            public List<string> sources = new List<string>();
            public bool HasPhysics
            {
                get
                {
                    return (signals & Signal.Physics) != 0;
                }
            }
        }

        [Serializable]
        private class StoredEntry
        {
            public string guid = "";
            public string path = "";
            public long size;
            public long ticks;
            public int signals;
            public List<string> sources = new List<string>();
        }

        [Serializable]
        private class Store
        {
            public int version = StoreVersion;
            public List<StoredEntry> items = new List<StoredEntry>();
        }

        private const int StoreVersion = 2;
        private const string CacheRelative = "Library/StudioRaming/WarudoBatchBuilder/PrefabIndex.json";
        private static readonly List<Entry> entries = new List<Entry>();
        private static readonly Dictionary<string, Entry> byGuid = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private static readonly Dictionary<string, int> folderCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly Queue<Entry> pending = new Queue<Entry>();
        private static readonly HashSet<string> dirtyPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, bool> scriptPhysics = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static bool cacheLoaded;
        private static bool needsEnumerate = true;
        private static bool saveDue;
        private static int totalToCompute;
        public static int Version { get; private set; }

        public static IList<Entry> Entries
        {
            get
            {
                return entries;
            }
        }

        public static bool Busy
        {
            get
            {
                return needsEnumerate || pending.Count > 0;
            }
        }

        public static float Progress
        {
            get
            {
                return totalToCompute <= 0 ? 1f : Mathf.Clamp01(1f - pending.Count / (float)totalToCompute);
            }
        }

        public static int PendingCount
        {
            get
            {
                return pending.Count;
            }
        }

        public static Entry Find(string guid)
        {
            Entry e;
            return !string.IsNullOrEmpty(guid) && byGuid.TryGetValue(guid, out e) ? e : null;
        }

        public static int FolderPrefabCount(string folder)
        {
            int n;
            return !string.IsNullOrEmpty(folder) && folderCounts.TryGetValue(folder, out n) ? n : 0;
        }

        public static void RequestRefresh(bool force)
        {
            needsEnumerate = true;
            if (force)
            {
                for (int i = 0; i < entries.Count; i++)
                    entries[i].computed = false;
                scriptPhysics.Clear();
            }
        }

        public static bool Tick(double budgetSeconds)
        {
            int before = Version;
            double start = EditorApplication.timeSinceStartup;
            if (!cacheLoaded)
            {
                cacheLoaded = true;
                LoadCache();
            }

            if (dirtyPaths.Count > 0)
                ApplyDirtyPaths();
            if (needsEnumerate)
            {
                needsEnumerate = false;
                Enumerate();
            }

            while (pending.Count > 0)
            {
                Entry e = pending.Dequeue();
                if (e.computed)
                    continue;
                Compute(e);
                if (EditorApplication.timeSinceStartup - start > budgetSeconds)
                    break;
            }

            if (pending.Count == 0)
            {
                if (totalToCompute > 0)
                {
                    totalToCompute = 0;
                    Version++;
                }

                if (saveDue)
                {
                    saveDue = false;
                    SaveCache();
                }
            }
            else if (EditorApplication.timeSinceStartup - start > budgetSeconds)
            {
                if ((pending.Count % 64) == 0)
                    Version++;
            }

            return Version != before;
        }

        private static void Enumerate()
        {
            string[] guids;
            try
            {
                guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Prefab index scan failed: " + e.Message);
                return;
            }

            HashSet<string> codeRoots = FindCodeRoots();
            var alive = new HashSet<string>(StringComparer.Ordinal);
            folderCounts.Clear();
            bool changed = false;
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(guid));
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!alive.Add(guid))
                    continue;
                long size, ticks;
                StatFile(path, out size, out ticks);
                Entry e;
                if (!byGuid.TryGetValue(guid, out e))
                {
                    e = new Entry
                    {
                        guid = guid
                    };
                    byGuid[guid] = e;
                    entries.Add(e);
                    changed = true;
                }

                if (!string.Equals(e.path, path, StringComparison.Ordinal))
                {
                    e.path = path;
                    int slash = path.LastIndexOf('/');
                    e.folder = slash > 0 ? path.Substring(0, slash) : "Assets";
                    e.name = Path.GetFileNameWithoutExtension(path);
                    changed = true;
                }

                if (e.size != size || e.ticks != ticks)
                {
                    e.size = size;
                    e.ticks = ticks;
                    e.computed = false;
                }

                bool inCode = IsUnderAny(e.folder, codeRoots);
                if (inCode != e.inCode)
                {
                    e.inCode = inCode;
                    changed = true;
                }

                int n;
                folderCounts.TryGetValue(e.folder, out n);
                folderCounts[e.folder] = n + 1;
            }

            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (alive.Contains(entries[i].guid))
                    continue;
                byGuid.Remove(entries[i].guid);
                entries.RemoveAt(i);
                changed = true;
            }

            pending.Clear();
            for (int i = 0; i < entries.Count; i++)
            {
                if (!entries[i].computed && !entries[i].inCode)
                    pending.Enqueue(entries[i]);
            }

            totalToCompute = pending.Count;
            if (pending.Count > 0)
                saveDue = true;
            if (changed)
            {
                saveDue = true;
                Version++;
            }
        }

        private static void Compute(Entry e)
        {
            Signal signals = Signal.None;
            e.sources.Clear();
            try
            {
                string[] deps = AssetDatabase.GetDependencies(e.path, true);
                for (int i = 0; i < deps.Length; i++)
                {
                    string dep = deps[i];
                    if (dep.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsPhysicsScript(dep))
                            signals |= Signal.Physics;
                    }
                }

                string[] direct = AssetDatabase.GetDependencies(e.path, false);
                for (int i = 0; i < direct.Length; i++)
                {
                    if (direct[i].EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) && !string.Equals(direct[i], e.path, StringComparison.OrdinalIgnoreCase))
                        e.sources.Add(WbbUtil.NormalizePath(direct[i]));
                }
            }
            catch (Exception ex)
            {
                WbbUtil.LogWarning("Prefab index could not inspect " + e.path + ": " + ex.Message);
            }

            if (signals != e.signals)
                Version++;
            e.signals = signals;
            e.computed = true;
            saveDue = true;
        }

        private static bool IsPhysicsScript(string scriptPath)
        {
            bool result;
            if (scriptPhysics.TryGetValue(scriptPath, out result))
                return result;
            result = false;
            try
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                Type type = script != null ? script.GetClass() : null;
                if (type != null && typeof(Component).IsAssignableFrom(type))
                {
                    string ns = type.Namespace ?? "";
                    result = ns.StartsWith("MagicaCloth", StringComparison.Ordinal) || type.Name.IndexOf("SpringBone", StringComparison.Ordinal) >= 0 || string.Equals(ns, "StudioRaming.Physics", StringComparison.Ordinal);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            scriptPhysics[scriptPath] = result;
            return result;
        }

        public static List<string> SuggestWorkFolders()
        {
            var found = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < entries.Count; i++)
            {
                Entry e = entries[i];
                if (e.inCode)
                    continue;
                string[] parts = e.folder.Split('/');
                string acc = parts[0];
                for (int p = 1; p < parts.Length; p++)
                {
                    acc = acc + "/" + parts[p];
                    if (!LooksLikeModFolder(parts[p]))
                        continue;
                    if (seen.Add(acc))
                        found.Add(acc);
                    break;
                }
            }

            var result = new List<string>();
            for (int i = 0; i < found.Count; i++)
            {
                if (!WbbListSettings.IsWorkFolder(found[i]))
                    result.Add(found[i]);
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        public static bool LooksLikeModFolder(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            if (name.IndexOf("모드", StringComparison.Ordinal) >= 0)
                return true;
            string lower = name.ToLowerInvariant();
            int idx = 0;
            while ((idx = lower.IndexOf("mod", idx, StringComparison.Ordinal)) >= 0)
            {
                bool leftOk = idx == 0 || !char.IsLetter(lower[idx - 1]);
                int after = idx + 3;
                if (after < lower.Length && lower[after] == 's')
                    after++;
                bool rightOk = after >= lower.Length || !char.IsLetter(lower[after]);
                if (leftOk && rightOk)
                    return true;
                idx += 3;
            }

            return false;
        }

        internal static void NotifyAssetsChanged(string[] imported, string[] deleted, string[] moved)
        {
            bool any = false;
            any |= MarkDirty(imported);
            any |= MarkDirty(deleted);
            any |= MarkDirty(moved);
            if (any)
                needsEnumerate = true;
        }

        private static bool MarkDirty(string[] paths)
        {
            if (paths == null)
                return false;
            bool any = false;
            for (int i = 0; i < paths.Length; i++)
            {
                string p = paths[i];
                if (string.IsNullOrEmpty(p))
                    continue;
                if (p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase) || AssetDatabase.IsValidFolder(p))
                {
                    dirtyPaths.Add(WbbUtil.NormalizePath(p));
                    any = true;
                }
            }

            return any;
        }

        private static void ApplyDirtyPaths()
        {
            // The signals come from every nested level (GetDependencies recursive) but sources lists only the direct nested
            // prefabs, so a prefab that goes stale makes the prefabs holding it stale too, until no new path is added: in
            // A > B > C a change to C has to reach A, not only B.
            bool added = true;
            while (added)
            {
                added = false;
                for (int i = 0; i < entries.Count; i++)
                {
                    Entry e = entries[i];
                    if (dirtyPaths.Contains(e.path))
                    {
                        e.computed = false;
                        continue;
                    }

                    for (int s = 0; s < e.sources.Count; s++)
                    {
                        if (dirtyPaths.Contains(e.sources[s]))
                        {
                            e.computed = false;
                            dirtyPaths.Add(e.path);
                            added = true;
                            break;
                        }
                    }
                }
            }

            dirtyPaths.Clear();
            needsEnumerate = true;
        }

        private static string CachePath
        {
            get
            {
                string data = WbbUtil.NormalizePath(Application.dataPath);
                int idx = data.LastIndexOf('/');
                return (idx > 0 ? data.Substring(0, idx) : data) + "/" + CacheRelative;
            }
        }

        private static void LoadCache()
        {
            try
            {
                string path = CachePath;
                if (!File.Exists(path))
                    return;
                var store = JsonUtility.FromJson<Store>(File.ReadAllText(path));
                if (store == null || store.version != StoreVersion || store.items == null)
                    return;
                for (int i = 0; i < store.items.Count; i++)
                {
                    StoredEntry s = store.items[i];
                    if (s == null || string.IsNullOrEmpty(s.guid) || byGuid.ContainsKey(s.guid))
                        continue;
                    var e = new Entry
                    {
                        guid = s.guid,
                        path = s.path ?? "",
                        size = s.size,
                        ticks = s.ticks,
                        signals = (Signal)s.signals,
                        computed = true,
                        sources = s.sources ?? new List<string>(),
                    };
                    int slash = e.path.LastIndexOf('/');
                    e.folder = slash > 0 ? e.path.Substring(0, slash) : "Assets";
                    e.name = Path.GetFileNameWithoutExtension(e.path);
                    byGuid[e.guid] = e;
                    entries.Add(e);
                }
            }
            catch (Exception ex)
            {
                WbbUtil.LogWarning("Prefab index cache ignored: " + ex.Message);
            }
        }

        private static void SaveCache()
        {
            try
            {
                var store = new Store();
                for (int i = 0; i < entries.Count; i++)
                {
                    Entry e = entries[i];
                    if (!e.computed)
                        continue;
                    store.items.Add(new StoredEntry { guid = e.guid, path = e.path, size = e.size, ticks = e.ticks, signals = (int)e.signals, sources = e.sources, });
                }

                string path = CachePath;
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(store));
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(tmp, path);
            }
            catch (Exception ex)
            {
                WbbUtil.LogWarning("Prefab index cache could not be saved: " + ex.Message);
            }
        }

        private static HashSet<string> FindCodeRoots()
        {
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string[] guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets" });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(guids[i]));
                    int slash = path.LastIndexOf('/');
                    if (slash > 0)
                        roots.Add(path.Substring(0, slash));
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("asmdef scan failed: " + e.Message);
            }

            return roots;
        }

        private static bool IsUnderAny(string folder, HashSet<string> roots)
        {
            if (roots.Count == 0 || string.IsNullOrEmpty(folder))
                return false;
            string walk = folder;
            while (!string.IsNullOrEmpty(walk))
            {
                if (roots.Contains(walk))
                    return true;
                int cut = walk.LastIndexOf('/');
                if (cut <= 0)
                    break;
                walk = walk.Substring(0, cut);
            }

            return false;
        }

        private static void StatFile(string assetPath, out long size, out long ticks)
        {
            size = 0L;
            ticks = 0L;
            try
            {
                var info = new FileInfo(WbbUtil.ToAbsolutePath(assetPath));
                if (!info.Exists)
                    return;
                size = info.Length;
                ticks = info.LastWriteTimeUtc.Ticks;
            }
            catch (Exception)
            {
            }
        }
    }

    internal sealed class WbbPrefabIndexPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            WbbPrefabIndex.NotifyAssetsChanged(imported, deleted, moved);
            if (movedFrom != null && movedFrom.Length > 0)
                WbbPrefabIndex.NotifyAssetsChanged(null, movedFrom, null);
        }
    }
}
