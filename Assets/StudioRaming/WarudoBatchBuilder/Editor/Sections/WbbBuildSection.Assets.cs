using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private static readonly Dictionary<string, Texture2D> keptThumbs = new Dictionary<string, Texture2D>();
        private Texture2D EnsureThumb(Item item)
        {
            if (item.thumbLooked)
                return item.thumb;
            item.thumbLooked = true;
            if (item.isScene)
            {
                if (item.record != null)
                    item.thumb = LoadKeptThumb(item.record.id);
            }
            else
            {
                item.thumb = WbbThumbnail.FindExisting(item.path, item.name);
            }

            return item.thumb;
        }

        private static Texture2D LoadKeptThumb(string recordId)
        {
            if (string.IsNullOrEmpty(recordId))
                return null;
            Texture2D cached;
            if (keptThumbs.TryGetValue(recordId, out cached) && cached != null)
                return cached;
            try
            {
                string path = WbbQuickBuild.KeptThumbnailPath(recordId);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return null;
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.hideFlags = HideFlags.HideAndDontSave;
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                {
                    Object.DestroyImmediate(texture);
                    return null;
                }

                keptThumbs[recordId] = texture;
                return texture;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Kept thumbnail could not be read: " + e.Message);
                return null;
            }
        }

        internal static void ReleaseKeptThumbs()
        {
            foreach (var pair in keptThumbs)
            {
                if (pair.Value != null)
                    Object.DestroyImmediate(pair.Value);
            }

            keptThumbs.Clear();
        }

        private void SyncPackageTargets()
        {
            if (packageSection == null)
                return;
            var paths = new List<string>();
            var key = new System.Text.StringBuilder();
            List<int> order = SelectedInBuildOrder();
            for (int i = 0; i < order.Count; i++)
            {
                Item item = items[order[i]];
                if (string.IsNullOrEmpty(item.path))
                    continue;
                paths.Add(item.path);
                key.Append(item.guid).Append('|');
            }

            string signature = key.ToString();
            if (string.Equals(signature, packageTargetsKey, System.StringComparison.Ordinal))
                return;
            packageTargetsKey = signature;
            packageSection.SetTargets(paths);
        }

        private void SendSelectionToPackageTab()
        {
            if (selectedCount == 0)
            {
                SetStatus(L("Runner.NoSelection"), MessageType.Warning);
                RequestRepaint();
                return;
            }

            pendingOutputMode = (int)WbbOutputMode.UnityPackage;
            RequestRepaint();
        }

        private void PickStagingFolder()
        {
            string current = WbbUtil.ResolveBuildFolder(job, CurrentType);
            string start = WbbUtil.ToAbsolutePath(string.IsNullOrEmpty(current) ? "Assets" : current);
            string picked = EditorUtility.OpenFolderPanel(L("Build.StagingFolder"), start, "");
            if (string.IsNullOrEmpty(picked))
                return;
            string normalized = WbbUtil.NormalizePath(picked);
            string dataPath = WbbUtil.NormalizePath(Application.dataPath);
            if (!normalized.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("Build.StagingMustBeInProject"), L("Common.Ok"));
                return;
            }

            string assetPath = "Assets" + normalized.Substring(dataPath.Length);
            if (!WbbUtil.IsValidProjectFolder(assetPath))
            {
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("Build.StagingMustBeInProject"), L("Common.Ok"));
                return;
            }

            pendingBuildFolder = assetPath;
            RequestRepaint();
        }

        private void PickOutputFolder()
        {
            string start = job.outputDirectory;
            if (string.IsNullOrEmpty(start))
                start = WbbUtil.DefaultOutputPath(CurrentType);
            if (string.IsNullOrEmpty(start))
                start = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string picked = EditorUtility.OpenFolderPanel(L("Build.Output"), start, "");
            if (string.IsNullOrEmpty(picked))
                return;
            job.outputDirectory = WbbUtil.NormalizePath(picked);
            WbbPrefs.SetString(OutputDirKey(CurrentType), job.outputDirectory);
            NoteOutputDirSeen();
            namesDirty = true;
            preflightDirty = true;
            RequestRepaint();
        }

        private void OpenThumbnail(string guid)
        {
            int index = IndexOfGuid(guid);
            if (index < 0)
                return;
            Item item = items[index];
            Texture2D texture = item.isScene ? null : WbbThumbnail.FindExisting(item.path, item.name);
            if (texture == null)
            {
                SetStatus(L("Build.ThumbMissing"), MessageType.Warning);
                RequestRepaint();
                return;
            }

            AssetDatabase.OpenAsset(texture);
        }

        private void OpenItemOutputFolder(string guid)
        {
            int index = IndexOfGuid(guid);
            if (index < 0)
                return;
            WbbRecord record = items[index].record;
            if (record == null)
                return;
            string target = record.outputFilePath;
            if (string.IsNullOrEmpty(target) || !FileExistsSafe(target))
                target = record.outputDirectory;
            if (string.IsNullOrEmpty(target))
                return;
            WbbUtil.RevealFolder(WbbUtil.NormalizePath(target));
        }

        private void GenerateThumbnail(string guid)
        {
            int index = IndexOfGuid(guid);
            if (index < 0 || items[index].isScene)
                return;
            Item item = items[index];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(item.path);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog(L("Build.ThumbConfirmTitle"), L("Build.PrefabLoadFail", item.name), L("Common.Ok"));
                return;
            }

            if (EnsureThumb(item) != null && !EditorUtility.DisplayDialog(L("Build.ThumbConfirmTitle"), L("Build.ThumbRegenConfirm", item.name), L("Build.ThumbRegenOk"), L("Common.Cancel")))
                return;
            string error;
            Texture2D texture = WbbThumbnail.Generate(prefab, item.path, item.name, out error);
            if (texture == null)
            {
                SetStatus(L("Build.ThumbFail", string.IsNullOrEmpty(error) ? L("Common.None") : error), MessageType.Error);
            }
            else
            {
                item.thumb = texture;
                item.thumbLooked = true;
                SetStatus(L("Build.ThumbDone", 1), MessageType.Info);
            }

            RequestRepaint();
        }

        private void GenerateThumbnailBatch()
        {
            bool onlySelected = selectedCount > 0;
            var targets = new List<Item>();
            for (int i = 0; i < filtered.Count; i++)
            {
                Item item = items[filtered[i]];
                if (item.isScene || item.dimmed)
                    continue;
                if (onlySelected && !selected.Contains(item.guid))
                    continue;
                if (EnsureThumb(item) != null)
                    continue;
                targets.Add(item);
            }

            if (targets.Count == 0)
            {
                EditorUtility.DisplayDialog(L("Build.ThumbConfirmTitle"), L("Build.ThumbNoTarget"), L("Common.Ok"));
                return;
            }

            if (!EditorUtility.DisplayDialog(L("Build.ThumbConfirmTitle"), L("Build.ThumbConfirmBody", targets.Count), L("Build.ThumbCreate"), L("Common.Cancel")))
                return;
            var prefabs = new List<GameObject>(targets.Count);
            var paths = new List<string>(targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(targets[i].path);
                if (prefab == null)
                {
                    WbbUtil.LogWarning(L("Build.PrefabLoadFail", targets[i].path));
                    continue;
                }

                prefabs.Add(prefab);
                paths.Add(targets[i].path);
            }

            if (prefabs.Count == 0)
            {
                EditorUtility.DisplayDialog(L("Build.ThumbConfirmTitle"), L("Build.PrefabLoadFail", targets[0].name), L("Common.Ok"));
                return;
            }

            string title = L("Build.ThumbProgressTitle");
            int created = 0;
            try
            {
                created = WbbThumbnail.GenerateBatch(prefabs, paths, delegate (float progress, string name)
                {
                    return !EditorUtility.DisplayCancelableProgressBar(title, name, Mathf.Clamp01(progress));
                });
            }
            catch (System.Exception e)
            {
                WbbUtil.LogError("Thumbnail batch failed: " + e.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            SetStatus(L("Build.ThumbDone", created), MessageType.Info);
            for (int i = 0; i < targets.Count; i++)
                targets[i].thumbLooked = false;
            RequestRepaint();
        }
    }
}
