using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbQuickBuild
    {
        public struct SourceKey
        {
            public string objectId;
            public string scenePath;
            public string hierarchyPath;
            public string prefabGuid;
            public static SourceKey Of(GameObject go)
            {
                var key = new SourceKey
                {
                    objectId = "",
                    scenePath = "",
                    hierarchyPath = "",
                    prefabGuid = ""
                };
                if (go == null)
                    return key;
                try
                {
                    GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(go);
                    if (id.identifierType != 0 && !id.assetGUID.Empty())
                        key.objectId = id.ToString();
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("GlobalObjectId lookup failed for " + go.name + ": " + e.Message);
                }

                key.scenePath = go.scene.IsValid() && go.scene.path != null ? go.scene.path : "";
                key.hierarchyPath = HierarchyPath(go.transform);
                if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
                {
                    GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(go);
                    string prefabPath = prefabRoot != null ? AssetDatabase.GetAssetPath(prefabRoot) : "";
                    if (!string.IsNullOrEmpty(prefabPath))
                        key.prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
                }

                return key;
            }
        }

        private static string HierarchyPath(Transform transform)
        {
            if (transform == null)
                return "";
            var names = new List<string>();
            for (Transform t = transform; t != null; t = t.parent)
                names.Add(t.name);
            names.Reverse();
            return string.Join("/", names.ToArray());
        }

        public static GameObject ResolveSource(string objectId, string scenePath, string hierarchyPath)
        {
            if (!string.IsNullOrEmpty(objectId))
            {
                GlobalObjectId id;
                if (GlobalObjectId.TryParse(objectId, out id))
                {
                    GameObject found = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                    if (found != null)
                        return found;
                }
            }

            if (string.IsNullOrEmpty(hierarchyPath))
                return null;
            string wantedScene = scenePath == null ? "" : scenePath;
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded)
                    continue;
                if (!string.Equals(scene.path == null ? "" : scene.path, wantedScene, System.StringComparison.OrdinalIgnoreCase))
                    continue;
                GameObject found = FindByHierarchyPath(scene, hierarchyPath);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static GameObject FindByHierarchyPath(Scene scene, string hierarchyPath)
        {
            string[] parts = hierarchyPath.Split('/');
            if (parts.Length == 0)
                return null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                if (roots[r] == null || roots[r].name != parts[0])
                    continue;
                Transform current = roots[r].transform;
                for (int p = 1; p < parts.Length && current != null; p++)
                {
                    Transform next = null;
                    for (int c = 0; c < current.childCount; c++)
                    {
                        Transform child = current.GetChild(c);
                        if (child.name == parts[p])
                        {
                            next = child;
                            break;
                        }
                    }

                    current = next;
                }

                if (current != null)
                    return current.gameObject;
            }

            return null;
        }

        public static string DescribeSource(WbbRecord record)
        {
            if (record == null)
                return "";
            string scene = string.IsNullOrEmpty(record.sourceScenePath) ? L("Quick.UnsavedScene") : record.sourceScenePath;
            return L("Quick.TipSource", scene, record.sourceHierarchyPath);
        }

        public static void PingSource(WbbRecord record)
        {
            if (record == null)
                return;
            GameObject go = ResolveSource(record.sourceObjectId, record.sourceScenePath, record.sourceHierarchyPath);
            if (go == null)
            {
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("Quick.SourceNotLoaded", DescribeSource(record)), L("Common.Ok"));
                return;
            }

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }

        private static readonly List<WbbRecord> pendingRebuild = new List<WbbRecord>();
        public static void QueueRebuild(WbbRecord record)
        {
            if (record == null || !record.IsSceneSource)
                return;
            if (pendingRebuild.Count == 0)
                EditorApplication.delayCall += FlushRebuild;
            pendingRebuild.Add(record);
        }

        private static void FlushRebuild()
        {
            var records = new List<WbbRecord>(pendingRebuild);
            pendingRebuild.Clear();
            if (records.Count == 0)
                return;
            if (WbbBuildRunner.AnyRunning)
            {
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("Runner.OtherBuildRunning"), L("Common.Ok"));
                return;
            }

            WbbExportType type = records[0].ExportType == WbbExportType.Character ? WbbExportType.Character : WbbExportType.Prop;
            var objects = new List<GameObject>();
            var names = new Dictionary<GameObject, string>();
            int notLoaded = 0;
            int otherType = 0;
            string firstMissing = "";
            for (int i = 0; i < records.Count; i++)
            {
                WbbRecord record = records[i];
                WbbExportType recordType = record.ExportType == WbbExportType.Character ? WbbExportType.Character : WbbExportType.Prop;
                if (recordType != type)
                {
                    otherType++;
                    continue;
                }

                GameObject go = ResolveSource(record.sourceObjectId, record.sourceScenePath, record.sourceHierarchyPath);
                if (go == null)
                {
                    notLoaded++;
                    if (firstMissing.Length == 0)
                        firstMissing = DescribeSource(record);
                    continue;
                }

                if (names.ContainsKey(go))
                    continue;
                objects.Add(go);
                names[go] = record.modName;
            }

            if (objects.Count == 0)
            {
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("Quick.SourceNotLoaded", firstMissing), L("Common.Ok"));
                return;
            }

            string note = notLoaded > 0 || otherType > 0 ? L("Quick.RebuildSkipped", notLoaded, otherType) : "";
            WbbQuickBuildWindow.Open(type, objects, names, note);
        }
    }
}
