using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbLibrarySection
    {
        private string PathAt(int filteredIndex)
        {
            if (filteredIndex < 0 || filteredIndex >= filtered.Count)
                return "";
            int rowIndex = filtered[filteredIndex];
            if (rowIndex < 0 || rowIndex >= rows.Count)
                return "";
            return rows[rowIndex].file.filePath;
        }

        private int FilteredIndexOfPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return -1;
            for (int i = 0; i < filtered.Count; i++)
            {
                if (string.Equals(PathAt(i), path, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private void SetSelected(string path, bool value)
        {
            if (string.IsNullOrEmpty(path))
                return;
            if (value)
                selected.Add(path);
            else
                selected.Remove(path);
            selectionDirty = true;
        }

        private void SelectOnly(int filteredIndex)
        {
            string path = PathAt(filteredIndex);
            selected.Clear();
            if (!string.IsNullOrEmpty(path))
                selected.Add(path);
            lastClickedPath = path;
            selectionDirty = true;
        }

        private void ApplyRangeSelection(int filteredIndex, bool value)
        {
            int anchor = FilteredIndexOfPath(lastClickedPath);
            if (anchor < 0)
            {
                if (value)
                    SelectOnly(filteredIndex);
                else
                    SetSelected(PathAt(filteredIndex), false);
                return;
            }

            int min = Mathf.Min(anchor, filteredIndex);
            int max = Mathf.Max(anchor, filteredIndex);
            for (int i = min; i <= max; i++)
                SetSelected(PathAt(i), value);
        }

        private void SetSelectionForFiltered(bool value)
        {
            for (int i = 0; i < filtered.Count; i++)
                SetSelected(PathAt(i), value);
            selectionDirty = true;
            if (host != null)
                host.RequestRepaint();
        }

        private List<Row> CollectSelected()
        {
            var result = new List<Row>();
            if (selected.Count == 0)
                return result;
            var seen = new HashSet<string>();
            for (int i = 0; i < filtered.Count; i++)
            {
                int rowIndex = filtered[i];
                if (rowIndex < 0 || rowIndex >= rows.Count)
                    continue;
                Row row = rows[rowIndex];
                if (!selected.Contains(row.file.filePath))
                    continue;
                if (!seen.Add(row.file.filePath))
                    continue;
                result.Add(row);
            }

            hiddenSelectedCount = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                Row row = rows[i];
                if (!selected.Contains(row.file.filePath))
                    continue;
                if (!seen.Add(row.file.filePath))
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
            int tracked = 0;
            long size = 0L;
            for (int i = 0; i < rows.Count; i++)
            {
                if (!selected.Contains(rows[i].file.filePath))
                    continue;
                count++;
                if (rows[i].record != null)
                    tracked++;
                size += rows[i].file.sizeBytes;
            }

            selectionCount = count;
            selectionTrackedCount = tracked;
            selectionText = L("Library.SelectionSummary", count, WbbUtil.FormatSize(size));
            string rebuildTip = count == 0 ? L("Library.RebuildSelectedNone.tip") : tracked == 0 ? L("Library.RebuildSelectedUntracked.tip") : L("Library.RebuildSelected.tip");
            rebuildButtonContent = new GUIContent(L("Library.RebuildSelected"), rebuildTip);
            deleteButtonContent = new GUIContent(L("Library.DeleteSelected"), count == 0 ? L("Library.DeleteSelectedNone.tip") : L("Library.DeleteSelected.tip"));
        }

        private void DrawBottomBar()
        {
            if (selectionDirty || selectionVersion != WbbLang.Version || rebuildButtonContent == null)
                RecomputeSelection();
            WbbStyles.Separator();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(selectionText, midMini);
            GUILayout.FlexibleSpace();
            bool buildBusy = host != null && host.BuildBusy;
            using (new EditorGUI.DisabledScope(selectionTrackedCount == 0 || buildBusy))
            {
                if (WbbStyles.PlainButton(buildBusy ? host.RebuildButtonContent(rebuildButtonContent) : rebuildButtonContent, GUILayout.Height(WbbSize.Control), GUILayout.Width(WbbSize.Library.RebuildSelectedButton)))
                {
                    pendingRebuild = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            using (new EditorGUI.DisabledScope(selectionCount == 0))
            {
                if (WbbStyles.Button(deleteButtonContent, WbbStyles.IconTrash, WbbButtonKind.Danger, GUILayout.Height(WbbSize.Control)))
                {
                    pendingDelete = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            DrawStatusLine();
            GUILayout.Space(WbbSize.SpaceHair);
        }

        private void DrawStatusLine()
        {
            if (string.IsNullOrEmpty(statusText))
                return;
            GUILayout.Space(WbbSize.SpaceSmall);
            EditorGUILayout.BeginHorizontal();
            WbbStyles.Callout(statusText, statusTone);
            if (WbbStyles.IconButton(UI.SRGlyph.Close, L("Library.StatusClose.tip"), WbbButtonKind.Ghost, GUILayout.Width(WbbSize.IconButton), GUILayout.Height(WbbSize.ControlSmall)))
                SetStatus("", statusTone);
            EditorGUILayout.EndHorizontal();
        }
    }
}
