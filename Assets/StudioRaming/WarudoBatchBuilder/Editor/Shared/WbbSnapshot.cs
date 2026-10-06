using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbSnapshot
    {
        public static bool SaveFromSceneObject(GameObject source, string assetPath, WbbFixupOptions options, out WbbFixupReport report, out string error)
        {
            report = new WbbFixupReport();
            error = "";
            if (source == null)
            {
                error = "Source object is missing.";
                return false;
            }

            Vector3 lossyScale = source.transform.lossyScale;
            string name = source.name;
            return SaveClone(holder =>
            {
                GameObject clone = Object.Instantiate(source, holder, false);
                clone.name = name;
                return clone;
            }, clone =>
            {
                Transform root = clone.transform;
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;
                root.localScale = lossyScale;
                if (!clone.activeSelf)
                    clone.SetActive(true);
            }, assetPath, options, name, out report, out error);
        }

        public static bool SaveFromPrefabAsset(string prefabPath, string assetPath, WbbFixupOptions options, out WbbFixupReport report, out string error)
        {
            report = new WbbFixupReport();
            error = "";
            GameObject prefab = string.IsNullOrEmpty(prefabPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                error = "Prefab asset could not be loaded: " + prefabPath;
                return false;
            }

            return SaveClone(holder => PrefabUtility.InstantiatePrefab(prefab, holder) as GameObject, null, assetPath, options, prefab.name, out report, out error);
        }

        public static int CountMissingScripts(GameObject root)
        {
            if (root == null)
                return 0;
            int count = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                count += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transforms[i].gameObject);
            return count;
        }

        private static bool SaveClone(Func<Transform, GameObject> makeClone, Action<GameObject> adjustRoot, string assetPath, WbbFixupOptions options, string label, out WbbFixupReport report, out string error)
        {
            report = new WbbFixupReport();
            error = "";
            Scene preview = default(Scene);
            bool previewOpen = false;
            GameObject holder = null;
            try
            {
                preview = EditorSceneManager.NewPreviewScene();
                previewOpen = true;
                holder = EditorUtility.CreateGameObjectWithHideFlags("WbbSnapshotHolder", HideFlags.HideAndDontSave);
                holder.SetActive(false);
                SceneManager.MoveGameObjectToScene(holder, preview);
                GameObject clone = makeClone(holder.transform);
                if (clone == null)
                {
                    error = "Could not clone: " + label;
                    return false;
                }

                UnpackAllPrefabInstances(clone);
                if (adjustRoot != null)
                    adjustRoot(clone);
                report = WbbBuildFixups.Apply(clone, options);
                bool success;
                PrefabUtility.SaveAsPrefabAsset(clone, assetPath, out success);
                if (!success)
                {
                    error = "SaveAsPrefabAsset failed: " + assetPath;
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                WbbUtil.LogError("Snapshot failed for " + label + ": " + e.Message + "\n" + e.StackTrace);
                return false;
            }
            finally
            {
                if (holder != null)
                    Object.DestroyImmediate(holder);
                if (previewOpen)
                    EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static void UnpackAllPrefabInstances(GameObject root)
        {
            var roots = new System.Collections.Generic.List<GameObject>();
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObject go = transforms[i].gameObject;
                if (PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                    roots.Add(go);
            }

            for (int i = 0; i < roots.Count; i++)
            {
                if (roots[i] == null || !PrefabUtility.IsPartOfPrefabInstance(roots[i]))
                    continue;
                try
                {
                    PrefabUtility.UnpackPrefabInstance(roots[i], PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
                catch (Exception e)
                {
                    WbbUtil.LogWarning("Could not unpack a nested prefab in the temporary copy (" + roots[i].name + "): " + e.Message);
                }
            }
        }
    }
}
