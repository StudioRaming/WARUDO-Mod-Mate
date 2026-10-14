using System.Collections.Generic;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
        private string IdAt(int entryIndex)
        {
            if (entryIndex < 0 || entryIndex >= entries.Count)
                return "";
            Entry entry = entries[entryIndex];
            if (entry.header)
                return "";
            if (entry.row < 0 || entry.row >= rows.Count)
                return "";
            return rows[entry.row].record.id;
        }

        private int EntryIndexOfId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return -1;
            for (int i = 0; i < entries.Count; i++)
            {
                if (string.Equals(IdAt(i), id, System.StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        private void SetSelected(string id, bool value)
        {
            if (string.IsNullOrEmpty(id))
                return;
            if (value)
                selected.Add(id);
            else
                selected.Remove(id);
            selectionDirty = true;
        }

        private void SelectOnly(int entryIndex)
        {
            string id = IdAt(entryIndex);
            selected.Clear();
            if (!string.IsNullOrEmpty(id))
                selected.Add(id);
            lastClickedId = id;
            selectionDirty = true;
        }

        private void ApplyRangeSelection(int entryIndex, bool value)
        {
            int anchor = EntryIndexOfId(lastClickedId);
            if (anchor < 0)
            {
                if (value)
                    SelectOnly(entryIndex);
                else
                    SetSelected(IdAt(entryIndex), false);
                return;
            }

            int min = Mathf.Min(anchor, entryIndex);
            int max = Mathf.Max(anchor, entryIndex);
            for (int i = min; i <= max; i++)
                SetSelected(IdAt(i), value);
        }

        private void SetSelectionForVisible(bool value)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].header)
                    continue;
                SetSelected(IdAt(i), value);
            }

            selectionDirty = true;
            if (host != null)
                host.RequestRepaint();
        }

        // Clear Selection drops every selected record, including ones a search or filter hides.
        private void ClearAllSelection()
        {
            selected.Clear();
            selectionDirty = true;
            if (host != null)
                host.RequestRepaint();
        }

        private List<Row> CollectSelected()
        {
            var result = new List<Row>();
            if (selected.Count == 0)
                return result;
            var seen = new HashSet<string>(System.StringComparer.Ordinal);
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry.header)
                    continue;
                if (entry.row < 0 || entry.row >= rows.Count)
                    continue;
                Row row = rows[entry.row];
                if (!selected.Contains(row.record.id))
                    continue;
                if (!seen.Add(row.record.id))
                    continue;
                result.Add(row);
            }

            hiddenSelectedCount = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                Row row = rows[i];
                if (!selected.Contains(row.record.id))
                    continue;
                if (!seen.Add(row.record.id))
                    continue;
                result.Add(row);
                hiddenSelectedCount++;
            }

            return result;
        }

        private void RecomputeSelection()
        {
            selectionDirty = false;
            selectionVersion = WbbLang.Version;
            int count = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                if (selected.Contains(rows[i].record.id))
                    count++;
            }

            selectionCount = count;
            selectionText = L("Common.Selected", count);
        }
    }
}
