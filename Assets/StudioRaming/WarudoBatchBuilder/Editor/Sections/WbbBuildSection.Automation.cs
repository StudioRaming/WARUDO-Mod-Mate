using System.Collections.Generic;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private HashSet<string> automationGuids;
        private bool IsAutomationTarget(string guid)
        {
            return automationGuids != null && automationGuids.Contains(guid);
        }

        public bool SelectForBuild(IList<string> prefabPaths, WbbExportType type, out string error)
        {
            error = "";
            if (IsBusy)
            {
                error = L("Win.BusyRebuild");
                return false;
            }

            if (prefabPaths == null || prefabPaths.Count == 0)
            {
                error = "No prefab path was given.";
                return false;
            }

            if (WbbPrefabIndex.Busy)
            {
                error = "The prefab index is still scanning. Try again when WbbPrefabIndex.Busy is false.";
                return false;
            }

            var guids = new List<string>();
            var names = new List<string>();
            for (int i = 0; i < prefabPaths.Count; i++)
            {
                string path = WbbUtil.NormalizePath(prefabPaths[i]);
                string guid = string.IsNullOrEmpty(path) ? "" : AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid) || !path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase) || AssetDatabase.LoadMainAssetAtPath(path) == null)
                {
                    error = "Not a prefab asset in this project: " + prefabPaths[i];
                    return false;
                }

                if (guids.Contains(guid))
                    continue;
                guids.Add(guid);
                names.Add(System.IO.Path.GetFileNameWithoutExtension(path));
            }

            if ((int)type != job.exportType)
                ChangeType((int)type);
            job.outputMode = (int)WbbOutputMode.WarudoMod;
            SavePrefs();
            pendingSelectGuids.Clear();
            pendingSelectNames.Clear();
            pendingRebuildModNames.Clear();
            pendingSelectReplace = true;
            rebuildQueuedCount = 0;
            rebuildToken = "";
            for (int i = 0; i < guids.Count; i++)
            {
                pendingSelectGuids.Add(guids[i]);
                pendingSelectNames.Add(names[i]);
                if (WbbListSettings.IsHidden(guids[i]))
                    WbbListSettings.SetHidden(guids[i], false);
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!WbbListSettings.IsUnder(path, scopeFolder))
                    pendingScope = "Assets";
            }

            automationGuids = new HashSet<string>(guids, System.StringComparer.Ordinal);
            pendingRescan = true;
            namesDirty = true;
            RequestRepaint();
            return true;
        }

        public bool StartBuildUnattended(out string error)
        {
            error = "";
            if (automationGuids == null)
            {
                error = "Call SelectForBuild first.";
                return false;
            }

            if (pendingSelectGuids.Count > 0 || pendingRescan || itemsDirty || WbbPrefabIndex.Busy)
            {
                error = "The selection is not applied yet. Let the window repaint once, then call again.";
                return false;
            }

            List<int> order = SelectedInBuildOrder();
            var chosen = new HashSet<string>(System.StringComparer.Ordinal);
            for (int i = 0; i < order.Count; i++)
                chosen.Add(items[order[i]].guid);
            if (!chosen.SetEquals(automationGuids))
            {
                var missing = new List<string>();
                foreach (string guid in automationGuids)
                {
                    if (!chosen.Contains(guid))
                        missing.Add(AssetDatabase.GUIDToAssetPath(guid));
                }

                error = "The Build tab selection does not match the request (selected " + chosen.Count + ", requested " + automationGuids.Count + ")." + (missing.Count > 0 ? " Not in the list: " + string.Join(", ", missing.ToArray()) : "");
                return false;
            }

            if (!TryStartBuild(false, out error))
                return false;
            automationGuids = null;
            return true;
        }
    }
}
