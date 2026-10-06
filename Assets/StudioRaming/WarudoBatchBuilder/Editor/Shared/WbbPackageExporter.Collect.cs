using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbPackageExporter
    {
        public static List<WbbPackageAsset> CollectDependencies(IList<string> rootPrefabPaths)
        {
            var result = new List<WbbPackageAsset>();
            if (rootPrefabPaths == null || rootPrefabPaths.Count == 0)
                return result;
            var roots = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < rootPrefabPaths.Count; i++)
            {
                string path = WbbUtil.NormalizePath(rootPrefabPaths[i]);
                if (!string.IsNullOrEmpty(path))
                    roots.Add(path);
            }

            if (roots.Count == 0)
                return result;
            var byGuid = new Dictionary<string, WbbPackageAsset>(StringComparer.Ordinal);
            foreach (string rootPath in roots)
            {
                string[] dependencies;
                try
                {
                    dependencies = AssetDatabase.GetDependencies(rootPath, true);
                }
                catch (Exception e)
                {
                    WbbUtil.LogWarning("Package: dependency scan failed for '" + rootPath + "' (" + e.Message + ")");
                    continue;
                }

                if (dependencies == null)
                    continue;
                Add(byGuid, rootPath, roots);
                for (int i = 0; i < dependencies.Length; i++)
                    Add(byGuid, dependencies[i], roots);
            }

            result.AddRange(byGuid.Values);
            result.Sort(CompareAssets);
            ClassifyRedistribution(result);
            return result;
        }

        private static void Add(Dictionary<string, WbbPackageAsset> byGuid, string rawPath, HashSet<string> roots)
        {
            string path = WbbUtil.NormalizePath(rawPath);
            if (!IsPackable(path))
                return;
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
                return;
            bool isRoot = roots.Contains(path);
            WbbPackageAsset existing;
            if (byGuid.TryGetValue(guid, out existing))
            {
                if (isRoot)
                    existing.isRoot = true;
                return;
            }

            byGuid[guid] = new WbbPackageAsset
            {
                guid = guid,
                assetPath = path,
                fileName = FileNameOf(path),
                extension = GetExtension(path),
                category = Classify(path),
                sizeBytes = FileSizeOf(path),
                isRoot = isRoot,
            };
        }

        public static bool IsPackable(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
            string path = WbbUtil.NormalizePath(assetPath);
            if (!path.StartsWith("Assets/", StringComparison.Ordinal))
                return false;
            if (path.IndexOf("unity_builtin_extra", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
            if (path.IndexOf("unity default resources", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
            if (path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                return false;
            try
            {
                if (AssetDatabase.IsValidFolder(path))
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static int CompareAssets(WbbPackageAsset a, WbbPackageAsset b)
        {
            if (a == null)
                return b == null ? 0 : 1;
            if (b == null)
                return -1;
            int order = ((int)a.category).CompareTo((int)b.category);
            if (order != 0)
                return order;
            order = string.Compare(a.fileName, b.fileName, StringComparison.OrdinalIgnoreCase);
            if (order != 0)
                return order;
            return string.Compare(a.assetPath, b.assetPath, StringComparison.Ordinal);
        }

        private static string FileNameOf(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return string.Empty;
            string path = WbbUtil.NormalizePath(assetPath);
            int index = path.LastIndexOf('/');
            return index >= 0 ? path.Substring(index + 1) : path;
        }

        private static long FileSizeOf(string assetPath)
        {
            try
            {
                string absolute = WbbUtil.ToAbsolutePath(assetPath);
                if (string.IsNullOrEmpty(absolute) || !File.Exists(absolute))
                    return 0L;
                return new FileInfo(absolute).Length;
            }
            catch
            {
                return 0L;
            }
        }
    }
}
