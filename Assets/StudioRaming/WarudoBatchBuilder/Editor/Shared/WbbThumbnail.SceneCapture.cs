using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbThumbnail
    {
        private const int CaptureSupersample = 2;
        private const int ProbeLongSide = 512;
        private const float CropPadding = 0.06f;
        private const int CoverageThreshold = 4;
        private const float AutoFov = 40f;
        private const float AutoPitch = 15f;
        private const float AutoPadding = 1.25f;
        public static Texture2D CaptureSceneObject(GameObject target, int size, out bool usedSceneView, out string error)
        {
            usedSceneView = false;
            error = "";
            if (target == null)
            {
                error = "Target is null.";
                return null;
            }

            size = Mathf.Clamp(size, 64, 2048);
            int renderSize = size * CaptureSupersample;
            GameObject subject = target;
            GameObject tempClone = null;
            GameObject camGO = null;
            GameObject lightGO = null;
            RenderTexture rt = null;
            Texture2D readback = null;
            Dictionary<GameObject, int> originalLayers = null;
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                if (!target.activeInHierarchy)
                {
                    tempClone = Object.Instantiate(target, target.transform.position, target.transform.rotation);
                    tempClone.hideFlags = HideFlags.HideAndDontSave;
                    tempClone.transform.localScale = target.transform.lossyScale;
                    if (!tempClone.activeSelf)
                        tempClone.SetActive(true);
                    subject = tempClone;
                }

                camGO = EditorUtility.CreateGameObjectWithHideFlags("WbbTempSceneThumbCam", HideFlags.HideAndDontSave, typeof(Camera));
                Camera cam = camGO.GetComponent<Camera>();
                cam.enabled = false;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.allowHDR = false;
                cam.allowMSAA = false;
                if (!AnySceneLightReaches(IsolationLayer))
                {
                    lightGO = EditorUtility.CreateGameObjectWithHideFlags("WbbTempSceneThumbLight", HideFlags.HideAndDontSave, typeof(Light));
                    Light light = lightGO.GetComponent<Light>();
                    light.type = LightType.Directional;
                    light.intensity = 1.2f;
                    light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                    light.cullingMask = 1 << IsolationLayer;
                }

                originalLayers = IsolateLayers(subject);
                if (originalLayers == null || originalLayers.Count == 0)
                {
                    error = "Could not isolate the object for rendering.";
                    return null;
                }

                cam.cullingMask = 1 << IsolationLayer;
                bool linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
                float aspect = 1f;
                Rect? coverage = null;
                SceneView sceneView = SceneView.lastActiveSceneView;
                Camera sceneCamera = sceneView != null ? sceneView.camera : null;
                if (sceneCamera != null)
                {
                    cam.transform.position = sceneCamera.transform.position;
                    cam.transform.rotation = sceneCamera.transform.rotation;
                    cam.fieldOfView = sceneCamera.fieldOfView;
                    cam.orthographic = sceneCamera.orthographic;
                    cam.orthographicSize = sceneCamera.orthographicSize;
                    cam.nearClipPlane = sceneCamera.nearClipPlane;
                    cam.farClipPlane = sceneCamera.farClipPlane;
                    aspect = sceneCamera.aspect > 0.01f ? sceneCamera.aspect : 1f;
                    coverage = FindCoverage(cam, aspect);
                    usedSceneView = coverage.HasValue;
                }

                if (!coverage.HasValue)
                {
                    FrameFront(cam, subject.transform, CalculateBounds(subject));
                    aspect = 1f;
                    coverage = FindCoverage(cam, aspect);
                    if (!coverage.HasValue)
                    {
                        error = "Nothing was rendered (no visible renderers).";
                        return null;
                    }
                }

                rt = new RenderTexture(renderSize, renderSize, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                rt.Create();
                cam.targetTexture = rt;
                cam.ResetProjectionMatrix();
                cam.aspect = aspect;
                cam.projectionMatrix = CropMatrix(SquareAround(coverage.Value, aspect)) * cam.projectionMatrix;
                readback = new Texture2D(renderSize, renderSize, TextureFormat.RGBA32, false);
                Color32[] onBlack = RenderOnce(cam, rt, readback, Color.black);
                Color32[] onWhite = RenderOnce(cam, rt, readback, Color.white);
                cam.targetTexture = null;
                var result = new Texture2D(size, size, TextureFormat.RGBA32, false);
                result.hideFlags = HideFlags.HideAndDontSave;
                result.SetPixels32(ExtractAlpha(onBlack, onWhite, renderSize, size, linear));
                result.Apply(false);
                return result;
            }
            catch (System.Exception e)
            {
                error = e.Message;
                WbbUtil.LogError("Scene thumbnail capture failed for " + target.name + ": " + e.Message + "\n" + e.StackTrace);
                return null;
            }
            finally
            {
                RestoreLayers(originalLayers);
                RenderTexture.active = previousActive;
                if (readback != null)
                    Object.DestroyImmediate(readback);
                if (rt != null)
                {
                    rt.Release();
                    Object.DestroyImmediate(rt);
                }

                if (camGO != null)
                    Object.DestroyImmediate(camGO);
                if (lightGO != null)
                    Object.DestroyImmediate(lightGO);
                if (tempClone != null)
                    Object.DestroyImmediate(tempClone);
            }
        }

        public static Texture2D SavePngAsset(Texture2D texture, string assetPath, out string error)
        {
            error = "";
            if (texture == null)
            {
                error = "Texture is null.";
                return null;
            }

            string normalized = WbbUtil.NormalizePath(assetPath);
            if (!normalized.StartsWith("Assets/"))
            {
                error = "Thumbnail path must be under Assets/: " + assetPath;
                return null;
            }

            try
            {
                byte[] png = texture.EncodeToPNG();
                if (png == null || png.Length == 0)
                {
                    error = "Failed to encode thumbnail PNG.";
                    return null;
                }

                string absolutePath = WbbUtil.ToAbsolutePath(normalized);
                string absoluteFolder = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(absoluteFolder) && !Directory.Exists(absoluteFolder))
                    Directory.CreateDirectory(absoluteFolder);
                File.WriteAllBytes(absolutePath, png);
                AssetDatabase.ImportAsset(normalized, ImportAssetOptions.ForceSynchronousImport);
                ApplyImporterSettings(normalized);
                var saved = AssetDatabase.LoadAssetAtPath<Texture2D>(normalized);
                if (saved == null)
                    error = "Thumbnail was written but could not be imported: " + normalized;
                return saved;
            }
            catch (System.Exception e)
            {
                error = e.Message;
                WbbUtil.LogError("Saving thumbnail failed (" + normalized + "): " + e.Message);
                return null;
            }
        }

        private static Color32[] RenderOnce(Camera cam, RenderTexture rt, Texture2D readback, Color background)
        {
            cam.backgroundColor = background;
            cam.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            readback.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            RenderTexture.active = previous;
            return readback.GetPixels32();
        }

        private static Rect? FindCoverage(Camera cam, float aspect)
        {
            int width = aspect >= 1f ? ProbeLongSide : Mathf.Max(16, Mathf.RoundToInt(ProbeLongSide * aspect));
            int height = aspect >= 1f ? Mathf.Max(16, Mathf.RoundToInt(ProbeLongSide / aspect)) : ProbeLongSide;
            RenderTexture probe = null;
            Texture2D readback = null;
            try
            {
                probe = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                probe.Create();
                cam.targetTexture = probe;
                cam.ResetProjectionMatrix();
                cam.aspect = aspect;
                readback = new Texture2D(width, height, TextureFormat.RGBA32, false);
                Color32[] onBlack = RenderOnce(cam, probe, readback, Color.black);
                Color32[] onWhite = RenderOnce(cam, probe, readback, Color.white);
                int minX = width, minY = height, maxX = -1, maxY = -1;
                for (int y = 0; y < height; y++)
                {
                    int row = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        Color32 b = onBlack[row + x];
                        Color32 w = onWhite[row + x];
                        int diff = ((w.r - b.r) + (w.g - b.g) + (w.b - b.b)) / 3;
                        if (255 - diff <= CoverageThreshold)
                            continue;
                        if (x < minX)
                            minX = x;
                        if (x > maxX)
                            maxX = x;
                        if (y < minY)
                            minY = y;
                        if (y > maxY)
                            maxY = y;
                    }
                }

                if (maxX < 0)
                    return null;
                return Rect.MinMaxRect((float)minX / width, (float)minY / height, (float)(maxX + 1) / width, (float)(maxY + 1) / height);
            }
            finally
            {
                cam.targetTexture = null;
                if (readback != null)
                    Object.DestroyImmediate(readback);
                if (probe != null)
                {
                    probe.Release();
                    Object.DestroyImmediate(probe);
                }
            }
        }

        private static Rect SquareAround(Rect viewport, float aspect)
        {
            float widthUnits = viewport.width * aspect;
            float heightUnits = viewport.height;
            float side = Mathf.Max(widthUnits, heightUnits) * (1f + CropPadding * 2f);
            if (side < 0.0001f)
                side = 0.0001f;
            float du = side / aspect;
            float dv = side;
            Vector2 center = viewport.center;
            return new Rect(center.x - du * 0.5f, center.y - dv * 0.5f, du, dv);
        }

        private static Matrix4x4 CropMatrix(Rect viewport)
        {
            float x0 = viewport.xMin * 2f - 1f, x1 = viewport.xMax * 2f - 1f;
            float y0 = viewport.yMin * 2f - 1f, y1 = viewport.yMax * 2f - 1f;
            Matrix4x4 m = Matrix4x4.identity;
            m.m00 = 2f / (x1 - x0);
            m.m03 = -(x1 + x0) / (x1 - x0);
            m.m11 = 2f / (y1 - y0);
            m.m13 = -(y1 + y0) / (y1 - y0);
            return m;
        }

        private static void FrameFront(Camera cam, Transform subject, Bounds bounds)
        {
            float radius = bounds.extents.magnitude;
            if (radius < 0.0001f)
                radius = 0.5f;
            float sin = Mathf.Sin(AutoFov * 0.5f * Mathf.Deg2Rad);
            float distance = radius / Mathf.Max(0.0001f, sin) * AutoPadding;
            Vector3 facing = Vector3.ProjectOnPlane(subject.forward, Vector3.up);
            if (facing.sqrMagnitude < 0.0001f)
                facing = Vector3.forward;
            facing.Normalize();
            Quaternion rotation = Quaternion.LookRotation(-facing, Vector3.up) * Quaternion.Euler(AutoPitch, 0f, 0f);
            cam.transform.rotation = rotation;
            cam.transform.position = bounds.center - (rotation * Vector3.forward) * distance;
            cam.orthographic = false;
            cam.fieldOfView = AutoFov;
            cam.orthographicSize = radius * AutoPadding;
            cam.nearClipPlane = Mathf.Max(0.01f, distance * 0.01f);
            cam.farClipPlane = distance + radius * 4f + 10f;
        }

        private static bool AnySceneLightReaches(int layer)
        {
            int bit = 1 << layer;
            Light[] lights = Object.FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || !light.isActiveAndEnabled)
                    continue;
                if (light.intensity <= 0f)
                    continue;
                if ((light.cullingMask & bit) != 0)
                    return true;
            }

            return false;
        }

        private static Color32[] ExtractAlpha(Color32[] onBlack, Color32[] onWhite, int srcSize, int dstSize, bool linear)
        {
            int factor = Mathf.Max(1, srcSize / dstSize);
            int samples = factor * factor;
            var toLinear = new float[256];
            for (int i = 0; i < 256; i++)
            {
                float v = i / 255f;
                toLinear[i] = linear ? Mathf.GammaToLinearSpace(v) : v;
            }

            var output = new Color32[dstSize * dstSize];
            for (int y = 0; y < dstSize; y++)
            {
                for (int x = 0; x < dstSize; x++)
                {
                    float sumR = 0f, sumG = 0f, sumB = 0f, sumA = 0f;
                    for (int dy = 0; dy < factor; dy++)
                    {
                        int row = (y * factor + dy) * srcSize;
                        for (int dx = 0; dx < factor; dx++)
                        {
                            int index = row + x * factor + dx;
                            Color32 b = onBlack[index];
                            Color32 w = onWhite[index];
                            float br = toLinear[b.r], bg = toLinear[b.g], bb = toLinear[b.b];
                            float diff = ((toLinear[w.r] - br) + (toLinear[w.g] - bg) + (toLinear[w.b] - bb)) / 3f;
                            float alpha = Mathf.Clamp01(1f - diff);
                            sumR += br;
                            sumG += bg;
                            sumB += bb;
                            sumA += alpha;
                        }
                    }

                    float a = sumA / samples;
                    if (a <= 0.5f / 255f)
                    {
                        output[y * dstSize + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float r = Mathf.Clamp01(sumR / samples / a);
                    float g = Mathf.Clamp01(sumG / samples / a);
                    float bl = Mathf.Clamp01(sumB / samples / a);
                    if (linear)
                    {
                        r = Mathf.LinearToGammaSpace(r);
                        g = Mathf.LinearToGammaSpace(g);
                        bl = Mathf.LinearToGammaSpace(bl);
                    }

                    output[y * dstSize + x] = new Color32(ToByte(r), ToByte(g), ToByte(bl), ToByte(a));
                }
            }

            return output;
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt(value * 255f), 0, 255);
        }
    }
}
