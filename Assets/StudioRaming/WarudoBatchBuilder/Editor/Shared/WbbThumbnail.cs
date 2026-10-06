using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbThumbnail
    {
        private const int IsolationLayer = 31;
        private const string PrimarySuffix = "_thumb.png";
        private static readonly string[] LookupSuffixes =
        {
            "_thumb.png",
            "_thumbnail.png",
            "_icon.png",
            ".png"
        };
        private static readonly Color BackgroundColor = new Color(0.20f, 0.20f, 0.20f, 1f);
        public static string ThumbnailAssetPath(string prefabPath, string prefabName)
        {
            string folder = FolderOf(prefabPath);
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(prefabName))
                return "";
            return folder + "/" + prefabName + PrimarySuffix;
        }

        public static Texture2D FindExisting(string prefabPath, string prefabName)
        {
            string folder = FolderOf(prefabPath);
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(prefabName))
                return null;
            for (int i = 0; i < LookupSuffixes.Length; i++)
            {
                string candidate = folder + "/" + prefabName + LookupSuffixes[i];
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(candidate);
                if (texture != null)
                    return texture;
            }

            return null;
        }

        public static Texture2D Generate(GameObject prefab, string prefabPath, string prefabName, out string error, int size = 1024)
        {
            error = "";
            if (prefab == null)
            {
                error = "Prefab is null.";
                return null;
            }

            if (string.IsNullOrEmpty(prefabPath))
                prefabPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(prefabName))
            {
                prefabName = string.IsNullOrEmpty(prefabPath) ? prefab.name : Path.GetFileNameWithoutExtension(prefabPath);
            }

            string assetPath = ThumbnailAssetPath(prefabPath, prefabName);
            if (string.IsNullOrEmpty(assetPath))
            {
                error = "Cannot resolve thumbnail path for: " + prefab.name;
                return null;
            }

            size = Mathf.Clamp(size, 64, 4096);
            GameObject instance = null;
            bool createdTempInstance = false;
            GameObject camGO = null;
            GameObject lightGO = null;
            RenderTexture rt = null;
            Texture2D shot = null;
            Dictionary<GameObject, int> originalLayers = null;
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                instance = ResolveInstance(prefab, out createdTempInstance);
                if (instance == null)
                {
                    error = "Failed to instantiate prefab: " + prefabName;
                    return null;
                }

                Vector3 cameraPosition;
                Quaternion cameraRotation;
                float cameraFov = 40f;
                bool cameraOrtho = false;
                float cameraOrthoSize = 5f;
                float cameraNear = 0.3f;
                float cameraFar = 1000f;
                SceneView sceneView = createdTempInstance ? null : SceneView.lastActiveSceneView;
                Camera sceneCamera = sceneView != null ? sceneView.camera : null;
                if (sceneCamera != null)
                {
                    cameraPosition = sceneCamera.transform.position;
                    cameraRotation = sceneCamera.transform.rotation;
                    cameraFov = sceneCamera.fieldOfView;
                    cameraOrtho = sceneCamera.orthographic;
                    cameraOrthoSize = sceneCamera.orthographicSize;
                    cameraNear = sceneCamera.nearClipPlane;
                    cameraFar = sceneCamera.farClipPlane;
                }
                else
                {
                    Bounds bounds = CalculateBounds(instance);
                    float radius = bounds.extents.magnitude;
                    if (radius < 0.0001f)
                        radius = 0.5f;
                    cameraFov = 40f;
                    float halfFovRad = cameraFov * 0.5f * Mathf.Deg2Rad;
                    float sin = Mathf.Sin(halfFovRad);
                    if (sin < 0.0001f)
                        sin = 0.0001f;
                    float distance = radius / sin * 1.25f;
                    cameraRotation = Quaternion.Euler(15f, 180f, 0f);
                    cameraPosition = bounds.center - (cameraRotation * Vector3.forward) * distance;
                    cameraNear = Mathf.Max(0.01f, distance * 0.01f);
                    cameraFar = distance + radius * 4f + 10f;
                    cameraOrthoSize = radius * 1.25f;
                }

                camGO = new GameObject("WbbTempThumbnailCam");
                camGO.hideFlags = HideFlags.HideAndDontSave;
                Camera renderCam = camGO.AddComponent<Camera>();
                renderCam.enabled = false;
                renderCam.transform.position = cameraPosition;
                renderCam.transform.rotation = cameraRotation;
                renderCam.fieldOfView = cameraFov;
                renderCam.orthographic = cameraOrtho;
                renderCam.orthographicSize = cameraOrthoSize;
                renderCam.nearClipPlane = cameraNear;
                renderCam.farClipPlane = cameraFar;
                renderCam.clearFlags = CameraClearFlags.SolidColor;
                renderCam.backgroundColor = BackgroundColor;
                lightGO = new GameObject("WbbTempThumbnailLight");
                lightGO.hideFlags = HideFlags.HideAndDontSave;
                Light mainLight = lightGO.AddComponent<Light>();
                mainLight.type = LightType.Directional;
                mainLight.intensity = 1.2f;
                mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                originalLayers = IsolateLayers(instance);
                if (originalLayers != null && originalLayers.Count > 0)
                {
                    renderCam.cullingMask = 1 << IsolationLayer;
                    mainLight.cullingMask = 1 << IsolationLayer;
                }
                else
                {
                    renderCam.cullingMask = ~0;
                    mainLight.cullingMask = ~0;
                }

                rt = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                rt.Create();
                renderCam.targetTexture = rt;
                renderCam.aspect = 1f;
                renderCam.Render();
                RenderTexture.active = rt;
                shot = new Texture2D(size, size, TextureFormat.RGBA32, false);
                shot.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                shot.Apply();
                RenderTexture.active = previousActive;
                renderCam.targetTexture = null;
                byte[] png = shot.EncodeToPNG();
                if (png == null || png.Length == 0)
                {
                    error = "Failed to encode thumbnail PNG: " + prefabName;
                    return null;
                }

                string absolutePath = WbbUtil.ToAbsolutePath(assetPath);
                string absoluteFolder = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(absoluteFolder) && !Directory.Exists(absoluteFolder))
                    Directory.CreateDirectory(absoluteFolder);
                File.WriteAllBytes(absolutePath, png);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
                ApplyImporterSettings(assetPath);
                var saved = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (saved == null)
                {
                    error = "Thumbnail was written but could not be imported: " + assetPath;
                    return null;
                }

                return saved;
            }
            catch (System.Exception e)
            {
                error = e.Message;
                WbbUtil.LogError("Thumbnail generation failed for " + prefabName + ": " + e.Message + "\n" + e.StackTrace);
                return null;
            }
            finally
            {
                RestoreLayers(originalLayers);
                if (RenderTexture.active == rt)
                    RenderTexture.active = previousActive;
                if (shot != null)
                    UnityEngine.Object.DestroyImmediate(shot);
                if (rt != null)
                {
                    rt.Release();
                    UnityEngine.Object.DestroyImmediate(rt);
                }

                if (camGO != null)
                    UnityEngine.Object.DestroyImmediate(camGO);
                if (lightGO != null)
                    UnityEngine.Object.DestroyImmediate(lightGO);
                if (createdTempInstance && instance != null)
                    UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        public static int GenerateBatch(IList<GameObject> prefabs, IList<string> prefabPaths, System.Func<float, string, bool> progress)
        {
            if (prefabs == null || prefabs.Count == 0)
                return 0;
            int count = prefabs.Count;
            if (prefabPaths != null && prefabPaths.Count < count)
                count = prefabPaths.Count;
            int created = 0;
            for (int i = 0; i < count; i++)
            {
                var prefab = prefabs[i];
                if (prefab == null)
                    continue;
                string path = prefabPaths != null ? prefabPaths[i] : null;
                if (string.IsNullOrEmpty(path))
                    path = AssetDatabase.GetAssetPath(prefab);
                string name = string.IsNullOrEmpty(path) ? prefab.name : Path.GetFileNameWithoutExtension(path);
                if (progress != null && !progress(count > 0 ? (float)i / count : 1f, name))
                    return created;
                string error;
                var texture = Generate(prefab, path, name, out error);
                if (texture != null)
                    created++;
                else
                    WbbUtil.LogWarning("Thumbnail skipped: " + name + (string.IsNullOrEmpty(error) ? "" : " (" + error + ")"));
            }

            return created;
        }

        private static string FolderOf(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return "";
            string normalized = WbbUtil.NormalizePath(assetPath);
            int index = normalized.LastIndexOf('/');
            return index > 0 ? normalized.Substring(0, index) : "";
        }

        private static GameObject ResolveInstance(GameObject prefab, out bool createdTemp)
        {
            createdTemp = false;
            var sceneInstance = FindSceneInstanceOfPrefab(prefab);
            if (sceneInstance != null)
                return sceneInstance;
#if UNITY_2021_2_OR_NEWER
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
            var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif
            if (stage != null && stage.prefabContentsRoot != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(prefab);
#if UNITY_2020_3_OR_NEWER
                string stageAssetPath = stage.assetPath;
#else
                string stageAssetPath = stage.prefabAssetPath;
#endif
                if (!string.IsNullOrEmpty(assetPath) && string.Equals(WbbUtil.NormalizePath(stageAssetPath), WbbUtil.NormalizePath(assetPath), System.StringComparison.OrdinalIgnoreCase))
                {
                    return stage.prefabContentsRoot;
                }
            }

            var temp = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (temp == null)
                return null;
            temp.transform.position = Vector3.zero;
            temp.transform.rotation = Quaternion.identity;
            if (!temp.activeSelf)
                temp.SetActive(true);
            temp.hideFlags = HideFlags.HideAndDontSave;
            createdTemp = true;
            return temp;
        }

        private static GameObject FindSceneInstanceOfPrefab(GameObject prefab)
        {
            if (prefab == null)
                return null;
            try
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (!scene.IsValid() || !scene.isLoaded)
                    return null;
                var roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                {
                    var root = roots[r];
                    if (root == null)
                        continue;
                    if (root.activeInHierarchy && PrefabUtility.GetCorrespondingObjectFromSource(root) == prefab)
                        return root;
                    var transforms = root.GetComponentsInChildren<Transform>(true);
                    for (int t = 0; t < transforms.Length; t++)
                    {
                        var go = transforms[t].gameObject;
                        if (!go.activeInHierarchy)
                            continue;
                        if (PrefabUtility.GetCorrespondingObjectFromSource(go) == prefab)
                            return go;
                    }
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Scene instance lookup failed: " + e.Message);
            }

            return null;
        }

        private static Bounds CalculateBounds(GameObject obj)
        {
            if (obj == null)
                return new Bounds(Vector3.zero, Vector3.one);
            var renderers = obj.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds(obj.transform.position, Vector3.one);
            bool initialized = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    continue;
                if (!initialized)
                {
                    bounds = renderer.bounds;
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
        }

        private static Dictionary<GameObject, int> IsolateLayers(GameObject instance)
        {
            if (instance == null)
                return null;
            var original = new Dictionary<GameObject, int>();
            try
            {
                var transforms = instance.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    var go = transforms[i].gameObject;
                    if (go == null || original.ContainsKey(go))
                        continue;
                    original[go] = go.layer;
                    go.layer = IsolationLayer;
                }

                return original;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Layer isolation failed, rendering without isolation: " + e.Message);
                RestoreLayers(original);
                return null;
            }
        }

        private static void RestoreLayers(Dictionary<GameObject, int> original)
        {
            if (original == null)
                return;
            foreach (var pair in original)
            {
                if (pair.Key != null)
                    pair.Key.layer = pair.Value;
            }

            original.Clear();
        }

        private static void ApplyImporterSettings(string assetPath)
        {
            try
            {
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    return;
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Default)
                {
                    importer.textureType = TextureImporterType.Default;
                    dirty = true;
                }

                if (!importer.sRGBTexture)
                {
                    importer.sRGBTexture = true;
                    dirty = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    dirty = true;
                }

                if (dirty)
                    importer.SaveAndReimport();
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Failed to apply thumbnail importer settings for " + assetPath + ": " + e.Message);
            }
        }
    }
}
