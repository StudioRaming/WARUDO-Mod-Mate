using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        public void RefreshScan()
        {
            pendingRescan = true;
            RequestRepaint();
        }

        public string RebuildToken(WbbRecord record)
        {
            string version, author, outputDir;
            return RebuildSettings(record, out version, out author, out outputDir);
        }

        private string RebuildSettings(WbbRecord record, out string version, out string author, out string outputDir)
        {
            int type = Mathf.Clamp(record.exportType, 0, TypeCount - 1);
            version = string.IsNullOrEmpty(record.modVersion) ? (rebuildBorrow != null ? rebuildBorrow.modVersion : job.modVersion) : record.modVersion;
            author = string.IsNullOrEmpty(record.modAuthor) ? (rebuildBorrow != null ? rebuildBorrow.modAuthor : job.modAuthor) : record.modAuthor;
            outputDir = string.IsNullOrEmpty(record.outputDirectory) ? WbbPrefs.GetString(OutputDirKey((WbbExportType)type), WbbUtil.DefaultOutputPath((WbbExportType)type)) : WbbUtil.NormalizePath(record.outputDirectory);
            return type + "|" + version + "|" + author + "|" + outputDir;
        }

        public void QueueRebuild(WbbRecord record)
        {
            if (record == null)
                return;
            if (IsBusy)
            {
                SetStatus(L("Win.BusyRebuild"), MessageType.Warning);
                RequestRepaint();
                return;
            }

            string path = "";
            if (!string.IsNullOrEmpty(record.prefabGuid))
                path = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(record.prefabGuid));
            if (string.IsNullOrEmpty(path))
                path = WbbUtil.NormalizePath(record.prefabPath);
            int type = Mathf.Clamp(record.exportType, 0, TypeCount - 1);
            string version, author, outputDir;
            string token = RebuildSettings(record, out version, out author, out outputDir);
            bool accumulate = string.Equals(token, rebuildToken, System.StringComparison.Ordinal) && pendingSelectGuids.Count > 0;
            string shown = string.IsNullOrEmpty(record.prefabName) ? (string.IsNullOrEmpty(record.modName) ? path : record.modName) : record.prefabName;
            if (!accumulate)
            {
                if (rebuildQueuedCount > 0)
                    SetStatus(L("Build.RebuildSwitched", rebuildQueuedCount, shown), MessageType.Warning);
                pendingSelectGuids.Clear();
                pendingSelectNames.Clear();
                pendingRebuildModNames.Clear();
                pendingSelectReplace = true;
                rebuildQueuedCount = 0;
                rebuildUnhiddenCount = 0;
            }

            rebuildToken = token;
            // The record's settings are borrowed for this run only; the saved Build tab settings come back afterwards.
            BorrowForRebuild(type, version, author, outputDir);
            string guid = record.prefabGuid;
            if (string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(path))
                guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid) && !pendingSelectGuids.Contains(guid))
            {
                pendingSelectGuids.Add(guid);
                pendingSelectNames.Add(shown);
                if (!string.IsNullOrEmpty(record.modName))
                    pendingRebuildModNames[guid] = record.modName;
                if (WbbListSettings.IsHidden(guid))
                {
                    WbbListSettings.SetHidden(guid, false);
                    rebuildUnhiddenCount++;
                }

                if (!string.IsNullOrEmpty(path) && !WbbListSettings.IsUnder(path, scopeFolder))
                    pendingScope = "Assets";
            }

            pendingRescan = true;
            namesDirty = true;
            RequestRepaint();
        }

        private void RebuildItems()
        {
            itemsDirty = false;
            indexVersionSeen = WbbPrefabIndex.Version;
            listVersionSeen = WbbListSettings.Version;
            WbbDatabase.EnsureLoaded();
            var latest = new Dictionary<string, WbbRecord>(System.StringComparer.Ordinal);
            var counts = new Dictionary<string, int>(System.StringComparer.Ordinal);
            var sceneLatest = new Dictionary<string, WbbRecord>(System.StringComparer.OrdinalIgnoreCase);
            var sceneCounts = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            var sceneOrder = new List<string>();
            IList<WbbRecord> records = WbbDatabase.Records;
            for (int i = 0; i < records.Count; i++)
            {
                WbbRecord r = records[i];
                if (r == null)
                    continue;
                if (r.IsSceneSource)
                {
                    string key = SceneKeyOf(r);
                    if (key.Length == 0)
                        continue;
                    int n;
                    sceneCounts.TryGetValue(key, out n);
                    sceneCounts[key] = n + 1;
                    if (!sceneLatest.ContainsKey(key))
                    {
                        sceneLatest[key] = r;
                        sceneOrder.Add(key);
                    }

                    continue;
                }

                if (string.IsNullOrEmpty(r.prefabGuid))
                    continue;
                int c;
                counts.TryGetValue(r.prefabGuid, out c);
                counts[r.prefabGuid] = c + 1;
                if (!latest.ContainsKey(r.prefabGuid))
                    latest[r.prefabGuid] = r;
            }

            items.Clear();
            guidIndex.Clear();
            hoverIndex = -1;
            rawCount = 0;
            userHiddenCount = 0;
            otherScopeCount = 0;
            bool wholeProject = string.Equals(scopeFolder, "Assets", System.StringComparison.OrdinalIgnoreCase);
            IList<WbbPrefabIndex.Entry> entries = WbbPrefabIndex.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                WbbPrefabIndex.Entry e = entries[i];
                if (e == null || e.inCode)
                    continue;
                // A selection made in another folder stays and builds; the scope only decides what the list shows.
                bool otherScope = !wholeProject && !WbbListSettings.IsUnder(e.path, scopeFolder);
                if (otherScope && !selected.Contains(e.guid))
                    continue;
                WbbRecord record;
                latest.TryGetValue(e.guid, out record);
                ShowReason reason;
                if (WbbListSettings.IsHidden(e.guid))
                    reason = ShowReason.Hidden;
                else if (WbbListSettings.IsPinned(e.guid))
                    reason = ShowReason.Pinned;
                else if (WbbListSettings.IsInWorkFolder(e.path))
                    reason = ShowReason.WorkFolder;
                else if (record != null)
                    reason = ShowReason.Built;
                else if (e.HasPhysics)
                    reason = ShowReason.Physics;
                else
                    reason = ShowReason.Raw;
                if (!otherScope && reason == ShowReason.Raw)
                    rawCount++;
                else if (!otherScope && reason == ShowReason.Hidden)
                    userHiddenCount++;
                bool dimmed = reason == ShowReason.Raw || reason == ShowReason.Hidden;
                if (dimmed && !otherScope && !showHidden && !IsAutomationTarget(e.guid))
                    continue;
                if (otherScope)
                    otherScopeCount++;
                var item = new Item();
                item.otherScope = otherScope;
                item.guid = e.guid;
                item.path = e.path;
                item.folder = e.folder;
                item.folderName = WbbUtil.FolderName(e.folder);
                item.folderPrefabs = WbbPrefabIndex.FolderPrefabCount(e.folder);
                item.name = e.name;
                item.reason = reason;
                item.dimmed = dimmed;
                item.record = record;
                int built;
                counts.TryGetValue(e.guid, out built);
                item.buildCount = built;
                if (record != null)
                    item.signature = WbbUtil.SourceSignature(e.path);
                string keepName;
                if (rebuildModNames.TryGetValue(item.guid, out keepName))
                    item.modNameOverride = keepName;
                ComputeState(item);
                guidIndex[item.guid] = items.Count;
                items.Add(item);
            }

            if (wholeProject)
            {
                for (int i = 0; i < sceneOrder.Count; i++)
                {
                    string key = sceneOrder[i];
                    bool hidden = WbbListSettings.IsSceneHidden(key);
                    if (hidden)
                        userHiddenCount++;
                    if (hidden && !showHidden)
                        continue;
                    WbbRecord r = sceneLatest[key];
                    var item = new Item();
                    item.isScene = true;
                    item.sceneKey = key;
                    item.guid = ScenePrefix + key;
                    item.name = string.IsNullOrEmpty(r.modName) ? Path.GetFileNameWithoutExtension(key) : r.modName;
                    item.modName = item.name;
                    item.record = r;
                    item.reason = hidden ? ShowReason.Hidden : ShowReason.Built;
                    item.dimmed = hidden;
                    int n;
                    sceneCounts.TryGetValue(key, out n);
                    item.buildCount = n;
                    item.sceneLabel = SceneLabelOf(r);
                    ComputeState(item);
                    guidIndex[item.guid] = items.Count;
                    items.Add(item);
                }
            }

            PruneSelection();
            namesDirty = true;
            displayDirty = true;
            filterDirty = true;
            summaryDirty = true;
            preflightDirty = true;
        }

        private static string SceneKeyOf(WbbRecord r)
        {
            string key = WbbUtil.NormalizePath(r.outputFilePath);
            if (string.IsNullOrEmpty(key))
                key = r.modName ?? "";
            return key;
        }

        private static string SceneLabelOf(WbbRecord r)
        {
            string scene = string.IsNullOrEmpty(r.sourceScenePath) ? "" : Path.GetFileNameWithoutExtension(r.sourceScenePath);
            string obj = r.sourceHierarchyPath ?? "";
            if (scene.Length == 0)
                return obj;
            return scene + " · " + obj;
        }

        private static int CompareByName(Item a, Item b)
        {
            int result = string.Compare(a.modName, b.modName, System.StringComparison.OrdinalIgnoreCase);
            if (result != 0)
                return result;
            return string.Compare(a.path, b.path, System.StringComparison.OrdinalIgnoreCase);
        }

        private void PruneSelection()
        {
            if (selected.Count == 0)
                return;
            var drop = new List<string>();
            foreach (string guid in selected)
            {
                int index;
                if (!guidIndex.TryGetValue(guid, out index) || items[index].isScene)
                    drop.Add(guid);
            }

            for (int i = 0; i < drop.Count; i++)
                selected.Remove(drop[i]);
        }

        private static string DatabaseFingerprint()
        {
            WbbDatabase.EnsureLoaded();
            var records = WbbDatabase.Records;
            int count = records == null ? 0 : records.Count;
            string top = count > 0 && records[0] != null ? records[0].id : "";
            return count + "|" + top;
        }

        private void ComputeState(Item item)
        {
            WbbRecord record = item.record;
            item.outputExists = false;
            item.sortTicks = 0L;
            if (record == null)
            {
                item.state = BadgeState.NotBuilt;
                return;
            }

            System.DateTime when;
            string stamp = string.IsNullOrEmpty(record.finishedIso) ? record.startedIso : record.finishedIso;
            if (WbbUtil.TryParseIso(stamp, out when))
                item.sortTicks = when.Ticks;
            if (record.Status == WbbBuildStatus.Failed)
            {
                item.state = BadgeState.Failed;
                return;
            }

            if (record.Status != WbbBuildStatus.Success)
            {
                item.state = BadgeState.Other;
                return;
            }

            item.outputExists = FileExistsSafe(record.outputFilePath);
            if (!item.outputExists)
            {
                item.state = BadgeState.MissingOutput;
                return;
            }

            bool changed = !item.isScene && !string.IsNullOrEmpty(record.sourceSignature) && !string.IsNullOrEmpty(item.signature) && !string.Equals(record.sourceSignature, item.signature, System.StringComparison.Ordinal);
            item.state = changed ? BadgeState.SourceChanged : BadgeState.Built;
        }

        private int IndexOfGuid(string guid)
        {
            int index;
            return !string.IsNullOrEmpty(guid) && guidIndex.TryGetValue(guid, out index) ? index : -1;
        }
    }
}
