using System.Collections.Generic;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
        private List<WbbRecord> DedupeByPrefab(List<Row> source)
        {
            var result = new List<WbbRecord>();
            if (source == null || source.Count == 0)
                return result;
            var slot = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < source.Count; i++)
            {
                WbbRecord record = source[i].record;
                string key = string.IsNullOrEmpty(record.prefabGuid) ? "name:" + record.modName : record.prefabGuid;
                if (string.IsNullOrEmpty(key))
                    continue;
                int index;
                if (!slot.TryGetValue(key, out index))
                {
                    slot[key] = result.Count;
                    result.Add(record);
                    continue;
                }

                string kept = result[index].startedIso == null ? "" : result[index].startedIso;
                string candidate = record.startedIso == null ? "" : record.startedIso;
                if (string.CompareOrdinal(candidate, kept) > 0)
                    result[index] = record;
            }

            return result;
        }

        // The largest group of targets that share one rebuild token; SendRebuild queues only this group.
        private List<WbbRecord> PickRebuildGroup(List<WbbRecord> targets)
        {
            var byGroup = new Dictionary<string, List<WbbRecord>>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < targets.Count; i++)
            {
                string key = host.RebuildGroupKey(targets[i]);
                List<WbbRecord> bucket;
                if (!byGroup.TryGetValue(key, out bucket))
                {
                    bucket = new List<WbbRecord>();
                    byGroup[key] = bucket;
                }

                bucket.Add(targets[i]);
            }

            List<WbbRecord> chosen = null;
            foreach (var kv in byGroup)
            {
                if (chosen == null || kv.Value.Count > chosen.Count)
                    chosen = kv.Value;
            }

            return chosen == null ? targets : chosen;
        }

        // Extra line for the rebuild confirm: how many targets the single-group rule leaves out of this run.
        private string RebuildDropNote(List<WbbRecord> targets)
        {
            if (host == null || targets == null || targets.Count < 2)
                return "";
            int dropped = targets.Count - PickRebuildGroup(targets).Count;
            return dropped > 0 ? "\n\n" + L("History.RebuildDropNote", dropped) : "";
        }

        private void SendRebuild(List<WbbRecord> targets)
        {
            if (host == null || targets == null || targets.Count == 0)
                return;
            if (host.NoticeBuildBusy())
                return;
            List<WbbRecord> chosen = PickRebuildGroup(targets);
            for (int i = 0; i < chosen.Count; i++)
                host.RequestRebuild(chosen[i], false);
            int dropped = targets.Count - chosen.Count;
            if (dropped > 0)
            {
                ConfirmDialog(L("Common.ProductName"), L("History.RebuildMultiNotice", chosen.Count, chosen[0].modName, dropped), L("Common.Ok"), "");
            }
        }

        private void RebuildSelected()
        {
            var picked = CollectSelected();
            if (picked.Count == 0)
                return;
            if (host != null && host.NoticeBuildBusy())
                return;
            var targets = DedupeByPrefab(picked);
            if (targets.Count == 0)
                return;
            if (targets.Count == 1)
            {
                WbbRecord one = targets[0];
                string prefab = string.IsNullOrEmpty(one.prefabPath) ? L("Common.None") : one.prefabPath;
                if (!ConfirmDialog(L("Common.ProductName"), L("History.RebuildConfirm", one.modName, prefab), L("History.RebuildSend"), L("Common.Cancel")))
                    return;
            }
            else
            {
                if (!ConfirmDialog(L("Common.ProductName"), L("History.RebuildSelectedConfirm", targets.Count, targets[0].modName, RebuildDropNote(targets)), L("History.RebuildSend"), L("Common.Cancel")))
                    return;
            }

            SendRebuild(targets);
        }

        private void RebuildFailedOnly()
        {
            var tokens = ParseTokens(search);
            var failed = new List<Row>();
            for (int i = 0; i < rows.Count; i++)
            {
                Row row = rows[i];
                if (row.statusBucket != 1)
                    continue;
                if (!PassesFilter(row, tokens))
                    continue;
                failed.Add(row);
            }

            if (failed.Count == 0)
                return;
            if (host != null && host.NoticeBuildBusy())
                return;
            var targets = DedupeByPrefab(failed);
            if (targets.Count == 0)
                return;
            if (!ConfirmDialog(L("Common.ProductName"), L("History.RebuildFailedConfirm", targets.Count, targets[0].modName, RebuildDropNote(targets)), L("History.RebuildSend"), L("Common.Cancel")))
                return;
            SendRebuild(targets);
        }
    }
}
