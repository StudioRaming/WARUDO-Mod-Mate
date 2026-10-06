using System.Collections.Generic;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
        private void ExportCsv()
        {
            string defaultName = "WarudoBatchBuilder_History_" + System.DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + ".csv";
            string path = EditorUtility.SaveFilePanel(L("History.ExportTitle"), "", defaultName, "csv");
            if (string.IsNullOrEmpty(path))
                return;
            string error;
            if (WbbDatabase.ExportCsv(path, out error))
            {
                SetStatus(L("History.ExportDone", WbbDatabase.Records.Count, path), WbbTone.Success);
            }
            else
            {
                string failed = L("History.ExportFailed", string.IsNullOrEmpty(error) ? L("Common.None") : error);
                SetStatus(failed, WbbTone.Error);
                EditorUtility.DisplayDialog(L("Common.ProductName"), failed, L("Common.Ok"));
            }
        }

        private void DeleteSingle(string id)
        {
            Row row = null;
            for (int i = 0; i < rows.Count; i++)
            {
                if (string.Equals(rows[i].record.id, id, System.StringComparison.Ordinal))
                {
                    row = rows[i];
                    break;
                }
            }

            if (row == null)
                return;
            string when = string.IsNullOrEmpty(row.record.startedIso) ? L("Common.None") : row.record.startedIso;
            if (!EditorUtility.DisplayDialog(L("Common.ProductName"), L("History.DeleteOneConfirm", row.nameText, when), L("Common.Delete"), L("Common.Cancel")))
                return;
            WbbDatabase.DeleteRecord(id);
            selected.Remove(id);
            SetStatus(L("History.DeleteDone", 1), WbbTone.Success);
            pendingForceRefresh = true;
            if (host != null)
                host.RequestRepaint();
        }

        private void DeleteSelected()
        {
            var picked = CollectSelected();
            if (picked.Count == 0)
                return;
            if (!EditorUtility.DisplayDialog(L("Common.ProductName"), L("History.DeleteSelectedConfirm", picked.Count, picked[0].nameText), L("Common.Delete"), L("Common.Cancel")))
                return;
            var ids = new List<string>(picked.Count);
            for (int i = 0; i < picked.Count; i++)
                ids.Add(picked[i].record.id);
            WbbDatabase.DeleteRecords(ids);
            selected.Clear();
            SetStatus(L("History.DeleteDone", ids.Count), WbbTone.Success);
            pendingForceRefresh = true;
            if (host != null)
                host.RequestRepaint();
        }

        private void CleanOldRecords()
        {
            System.DateTime cutoff = System.DateTime.Now.AddDays(-CleanDays);
            var ids = new List<string>();
            string oldestName = "";
            string oldestWhen = "";
            for (int i = 0; i < rows.Count; i++)
            {
                WbbRecord record = rows[i].record;
                System.DateTime started;
                if (!WbbUtil.TryParseIso(record.startedIso, out started))
                    continue;
                if (started >= cutoff)
                    continue;
                ids.Add(record.id);
                oldestName = rows[i].nameText;
                oldestWhen = record.startedIso;
            }

            if (ids.Count == 0)
                return;
            string oldest = string.IsNullOrEmpty(oldestName) ? L("Common.None") : oldestName + "  " + oldestWhen;
            if (!EditorUtility.DisplayDialog(L("Common.ProductName"), L("History.CleanOldConfirm", CleanDays, ids.Count, oldest), L("History.CleanOldRun"), L("Common.Cancel")))
                return;
            WbbDatabase.DeleteRecords(ids);
            for (int i = 0; i < ids.Count; i++)
                selected.Remove(ids[i]);
            SetStatus(L("History.DeleteDone", ids.Count), WbbTone.Success);
            pendingForceRefresh = true;
            if (host != null)
                host.RequestRepaint();
        }

        private void ClearAllRecords()
        {
            int count = rows.Count;
            if (count == 0)
                return;
            if (!EditorUtility.DisplayDialog(L("Common.ProductName"), L("History.ClearAllConfirm1", count), L("Common.Delete"), L("Common.Cancel")))
                return;
            if (!EditorUtility.DisplayDialog(L("Common.ProductName"), L("History.ClearAllConfirm2", count), L("Common.Delete"), L("Common.Cancel")))
                return;
            WbbDatabase.ClearRecords();
            selected.Clear();
            collapsed.Clear();
            SetStatus(L("History.DeleteDone", count), WbbTone.Success);
            pendingForceRefresh = true;
            if (host != null)
                host.RequestRepaint();
        }
    }
}
