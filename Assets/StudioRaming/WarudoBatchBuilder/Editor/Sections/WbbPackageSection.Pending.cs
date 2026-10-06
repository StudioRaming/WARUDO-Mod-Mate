using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private void ProcessPending()
        {
            if (Event.current.type == EventType.Layout)
                return;
            if (!string.IsNullOrEmpty(pendingPingPath))
            {
                string path = pendingPingPath;
                pendingPingPath = "";
                PingAsset(path);
            }

            if (pendingDropPaths.Count > 0)
            {
                var dropped = new List<string>(pendingDropPaths);
                pendingDropPaths.Clear();
                AddFromPaths(dropped);
            }

            if (pendingAddSelection)
            {
                pendingAddSelection = false;
                AddFromProjectSelection();
            }

            if (pendingBrowseOutput == 1)
            {
                pendingBrowseOutput = 2;
                if (host != null)
                    host.RequestRepaint();
            }
            else if (pendingBrowseOutput == 2)
            {
                pendingBrowseOutput = 0;
                BrowseOutput();
            }

            if (pendingClearExcluded)
            {
                pendingClearExcluded = false;
                ClearExcluded();
            }

            if (pendingExport)
            {
                pendingExport = false;
                RunExport();
            }
        }

        private void PingAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return;
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset == null)
            {
                SetStatus(L("Package.PingMissing", assetPath), MessageType.Warning);
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private void AddFromPaths(List<string> paths)
        {
            var found = new List<string>();
            for (int i = 0; i < paths.Count; i++)
                CollectPrefabsFrom(paths[i], found);
            if (found.Count == 0)
            {
                SetStatus(L("Package.SelectionNoPrefab"), MessageType.Warning);
                return;
            }

            pendingAddPaths.AddRange(found);
            SetStatus(L("Package.AddedCount", found.Count), MessageType.Info);
            if (host != null)
                host.RequestRepaint();
        }

        private void AddFromProjectSelection()
        {
            var picked = new List<string>();
            string[] guids = Selection.assetGUIDs;
            if (guids != null)
            {
                for (int i = 0; i < guids.Length; i++)
                    picked.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
            }

            AddFromPaths(picked);
        }

        private static void CollectPrefabsFrom(string rawPath, List<string> result)
        {
            string path = WbbUtil.NormalizePath(rawPath);
            if (string.IsNullOrEmpty(path))
                return;
            if (path != "Assets" && !path.StartsWith(AssetsPrefix, System.StringComparison.Ordinal))
                return;
            if (AssetDatabase.IsValidFolder(path))
            {
                string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { path });
                if (guids == null)
                    return;
                for (int i = 0; i < guids.Length; i++)
                {
                    string child = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(guids[i]));
                    if (child.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                        result.Add(child);
                }

                return;
            }

            if (path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                result.Add(path);
        }

        private void BrowseOutput()
        {
            string start = outputDir;
            if (string.IsNullOrEmpty(start) || !DirectoryExists(start))
                start = DefaultOutputDir();
            string picked = EditorUtility.OpenFolderPanel(L("Package.BrowseTitle"), start, "");
            if (string.IsNullOrEmpty(picked))
                return;
            GUI.FocusControl(null);
            outputDir = WbbUtil.NormalizePath(picked);
            EditorPrefs.SetString(OutputDirKey, outputDir);
            if (host != null)
                host.RequestRepaint();
        }

        private void ClearExcluded()
        {
            int riskCount = WbbPackageExporter.RiskReincludedCount;
            if (excluded.Count == 0 && riskCount == 0)
            {
                EditorUtility.DisplayDialog(L("Package.ClearExcludedTitle"), L("Package.ClearExcludedNone"), L("Common.Ok"));
                return;
            }

            if (!EditorUtility.DisplayDialog(L("Package.ClearExcludedTitle"), L("Package.ClearExcludedConfirm", excluded.Count, riskCount), L("Package.ClearExcludedYes"), L("Common.Cancel")))
                return;
            int count = excluded.Count;
            excluded.Clear();
            SaveExcluded();
            int riskCleared = WbbPackageExporter.ClearRiskReincluded();
            RecomputeCounts();
            SetStatus(L("Package.ClearExcludedDone", count, riskCleared), MessageType.Info);
            if (host != null)
                host.RequestRepaint();
        }

        private static bool DirectoryExists(string path)
        {
            try
            {
                return !string.IsNullOrEmpty(path) && Directory.Exists(path);
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private void SetStatus(string message, MessageType type)
        {
            SetResultStatus(message, type, "");
        }

        private void SetResultStatus(string message, MessageType type, string folder)
        {
            pendingStatusSet = true;
            pendingStatus = message == null ? "" : message;
            pendingStatusType = type;
            pendingStatusFolder = folder == null ? "" : folder;
        }
    }
}
