using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbSceneAutoSave
    {
        public static bool HasDirtyUntitledScene(out string sceneName)
        {
            sceneName = "";
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded)
                    continue;
                if (scene.isDirty && string.IsNullOrEmpty(scene.path))
                {
                    sceneName = string.IsNullOrEmpty(scene.name) ? "Untitled" : scene.name;
                    return true;
                }
            }

            return false;
        }

        public static bool HasDirtyPrefabStage(out string assetPath)
        {
            assetPath = "";
            try
            {
                PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (stage == null || !stage.scene.IsValid() || !stage.scene.isDirty)
                    return false;
                assetPath = stage.assetPath ?? "";
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int SaveDirtyScenes(out string error)
        {
            error = "";
            int saved = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded || !scene.isDirty)
                    continue;
                if (string.IsNullOrEmpty(scene.path))
                    continue;
                bool ok;
                try
                {
                    ok = EditorSceneManager.SaveScene(scene);
                }
                catch (Exception e)
                {
                    ok = false;
                    WbbUtil.LogWarning("Auto-save failed for scene " + scene.path + ": " + e.Message);
                }

                if (ok)
                {
                    saved++;
                    WbbUtil.Log("Auto-saved scene before the build: " + scene.path);
                }
                else if (error.Length == 0)
                {
                    error = scene.path;
                }
            }

            return saved;
        }

        public static SceneSetup[] CaptureSetup()
        {
            try
            {
                SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
                if (setup == null || setup.Length == 0)
                    return null;
                for (int i = 0; i < setup.Length; i++)
                {
                    if (string.IsNullOrEmpty(setup[i].path))
                        return null;
                }

                return setup;
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not capture the open scene setup: " + e.Message);
                return null;
            }
        }

        public static void RestoreSetupIfChanged(SceneSetup[] before)
        {
            if (before == null || before.Length == 0)
                return;
            try
            {
                SceneSetup[] now = EditorSceneManager.GetSceneManagerSetup();
                if (SameSetup(before, now))
                    return;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.IsValid() && scene.isDirty)
                    {
                        WbbUtil.LogWarning("Open scenes were changed during the build and have unsaved edits; " + "not restoring the previous scene setup.");
                        return;
                    }
                }

                for (int i = 0; i < before.Length; i++)
                {
                    if (!System.IO.File.Exists(WbbUtil.ToAbsolutePath(before[i].path)))
                    {
                        WbbUtil.LogWarning("A scene from before the build no longer exists; not restoring: " + before[i].path);
                        return;
                    }
                }

                EditorSceneManager.RestoreSceneManagerSetup(before);
                WbbUtil.Log("Restored the scene setup that was open before the build (" + before.Length + " scene(s)).");
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not restore the scene setup after the build: " + e.Message);
            }
        }

        private static bool SameSetup(SceneSetup[] a, SceneSetup[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;
            var index = new Dictionary<string, SceneSetup>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < b.Length; i++)
            {
                if (!string.IsNullOrEmpty(b[i].path))
                    index[b[i].path] = b[i];
            }

            for (int i = 0; i < a.Length; i++)
            {
                SceneSetup other;
                if (!index.TryGetValue(a[i].path ?? "", out other))
                    return false;
                if (other.isLoaded != a[i].isLoaded || other.isActive != a[i].isActive)
                    return false;
            }

            return true;
        }
    }
}
