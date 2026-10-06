using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbLibrarySection
    {
        private const float ColumnHeaderHeight = 22f;
        private float rowWidthObserved, headerWidthLayout;
        private readonly GUIContent columnHeaderScratch = new GUIContent();
        private void DrawList()
        {
            if (Event.current.type == EventType.Layout)
                headerWidthLayout = rowWidthObserved > 0f ? Mathf.Min(rowWidthObserved, host != null ? host.position.width - WbbSize.SpaceEdge * 2f : rowWidthObserved) : 0f;
            float observedWidth = 0f;
            if (filtered.Count > 0)
                DrawColumnHeader();
            float viewportHeight = host != null ? Mathf.Max(80f, host.position.height - 346f) : 260f;
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(viewportHeight));
            int count = filtered.Count;
            if (count == 0)
            {
                GUILayout.Space(WbbSize.SpaceLarge);
                if (!typeEnabled[0] && !typeEnabled[1] && !typeEnabled[2])
                    GUILayout.Label(L("Library.NoTypeEnabled"), WbbStyles.MiniInfo);
                else if (rows.Count == 0)
                    DrawEmptyState();
                else
                    GUILayout.Label(L("Common.NoMatch", rows.Count), WbbStyles.MiniInfo);
            }
            else
            {
                bool tracking = Event.current.type == EventType.MouseMove;
                int hoverCandidate = -1;
                for (int i = 0; i < count; i++)
                {
                    Rect rect = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
                    if (Event.current.type == EventType.Repaint)
                        observedWidth = rect.width;
                    if (i < firstVisible || i > lastVisible)
                        continue;
                    DrawRow(i, rect, tracking, ref hoverCandidate);
                }

                if (tracking && hoverCandidate != hoverIndex)
                {
                    hoverIndex = hoverCandidate;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndScrollView();
            if (observedWidth > 0f && Mathf.Abs(observedWidth - rowWidthObserved) > 1f)
            {
                rowWidthObserved = observedWidth;
                if (host != null)
                    host.RequestRepaint();
            }
        }

        private void DrawColumnHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, ColumnHeaderHeight, GUILayout.ExpandWidth(true));
            if (headerWidthLayout > 0f)
                rect.width = Mathf.Min(rect.width, headerWidthLayout);
            WbbStyles.SectionSurface(rect);
            float left = rect.x + WbbSize.Library.RowInset + WbbSize.Library.CheckAdvance;
            float right = rect.xMax - WbbSize.Library.RowInset - WbbSize.Library.MenuAdvance;
            bool wide = rect.width > WbbSize.Library.WideMinWidth;
            bool medium = rect.width > WbbSize.Library.MediumMinWidth;
            Rect dateRect = new Rect(0f, 0f, 0f, 0f);
            if (wide)
            {
                dateRect = new Rect(right - WbbSize.Library.ColDate, rect.y, WbbSize.Library.ColDate, rect.height);
                right -= WbbSize.Library.ColDateStep;
            }

            Rect sizeRect = new Rect(right - WbbSize.Library.ColSize, rect.y, WbbSize.Library.ColSize, rect.height);
            right -= WbbSize.Library.ColSizeStep;
            Rect versionRect = new Rect(0f, 0f, 0f, 0f);
            if (medium)
            {
                versionRect = new Rect(right - WbbSize.Library.ColVersion, rect.y, WbbSize.Library.ColVersion, rect.height);
                right -= WbbSize.Library.ColVersionStep;
            }

            Rect badgeRect = new Rect(right - WbbSize.Library.ColBadge, rect.y, WbbSize.Library.ColBadge, rect.height);
            right -= WbbSize.Library.ColBadgeStep;
            Rect nameRect = new Rect(left, rect.y, Mathf.Max(WbbSize.Library.NameMinWidth, right - left), rect.height);
            DrawHeaderLabel(nameRect, L("Ui.Library.ColumnName"), false);
            DrawHeaderLabel(badgeRect, L("Ui.Library.ColumnStatus"), true);
            if (medium)
                DrawHeaderLabel(versionRect, L("Ui.Library.ColumnVersion"), true);
            DrawHeaderLabel(sizeRect, L("Ui.Library.ColumnSize"), true);
            if (wide)
                DrawHeaderLabel(dateRect, L("Ui.Library.ColumnModified"), true);
        }

        private void DrawHeaderLabel(Rect rect, string text, bool rightAligned)
        {
            if (rect.width <= 0f)
                return;
            GUIStyle style = rightAligned ? rightMini : midMini;
            columnHeaderScratch.text = WbbStyles.Fit(text, style, rect.width);
            columnHeaderScratch.tooltip = string.Empty;
            GUI.Label(rect, columnHeaderScratch, style);
        }

        private void DrawEmptyState()
        {
            int problem = FirstPathProblem();
            if (problem >= 0)
            {
                bool noPath = pathStates[problem] == PathState.Empty;
                if (WbbStyles.EmptyState(L(noPath ? "Library.EmptyNoPathTitle" : "Library.EmptyBadPathTitle"), L(noPath ? "Library.EmptyNoPathBody" : "Library.EmptyBadPathBody"), L("Library.SetPath")))
                {
                    pendingBrowseType = problem;
                    if (host != null)
                        host.RequestRepaint();
                }

                return;
            }

            if (WbbStyles.EmptyState(L("Library.EmptyNoFilesTitle"), L("Library.EmptyNoFilesBody"), L("Library.EmptyGoBuild")))
            {
                pendingGoToBuild = true;
                if (host != null)
                    host.RequestRepaint();
            }
        }

        private int FirstPathProblem()
        {
            for (int t = 0; t < TypeCount; t++)
            {
                if (typeEnabled[t] && pathStates[t] != PathState.Ok)
                    return t;
            }

            return -1;
        }

        private void DrawRow(int index, Rect rect, bool tracking, ref int hoverCandidate)
        {
            int rowIndex = filtered[index];
            if (rowIndex < 0 || rowIndex >= rows.Count)
                return;
            Row row = rows[rowIndex];
            Event evt = Event.current;
            string path = row.file.filePath;
            bool isSelected = selected.Contains(path);
            bool hovered = hoverIndex == index;
            if (tracking && rect.Contains(evt.mousePosition))
                hoverCandidate = index;
            WbbStyles.RowBackground(rect, isSelected, hovered, (index & 1) == 1);
            float left = rect.x + WbbSize.Library.RowInset;
            var checkRect = new Rect(left, rect.y + WbbSize.Library.CheckTop, WbbSize.CheckSize, WbbSize.CheckSize);
            left += WbbSize.Library.CheckAdvance;
            float right = rect.xMax - WbbSize.Library.RowInset;
            var menuRect = new Rect(right - WbbSize.IconButton, rect.y + WbbSize.RowButtonInset, WbbSize.IconButton, rect.height - WbbSize.Library.MenuHeightTrim);
            right -= WbbSize.Library.MenuAdvance;
            bool wide = rect.width > WbbSize.Library.WideMinWidth;
            bool medium = rect.width > WbbSize.Library.MediumMinWidth;
            var dateRect = new Rect(0f, 0f, 0f, 0f);
            if (wide)
            {
                dateRect = new Rect(right - WbbSize.Library.ColDate, rect.y, WbbSize.Library.ColDate, rect.height);
                right -= WbbSize.Library.ColDateStep;
            }

            var sizeRect = new Rect(right - WbbSize.Library.ColSize, rect.y, WbbSize.Library.ColSize, rect.height);
            right -= WbbSize.Library.ColSizeStep;
            var versionRect = new Rect(0f, 0f, 0f, 0f);
            if (medium)
            {
                versionRect = new Rect(right - WbbSize.Library.ColVersion, rect.y, WbbSize.Library.ColVersion, rect.height);
                right -= WbbSize.Library.ColVersionStep;
            }

            var badgeRect = new Rect(right - WbbSize.Library.ColBadge, rect.y, WbbSize.Library.ColBadge, rect.height);
            right -= WbbSize.Library.ColBadgeStep;
            var nameRect = new Rect(left, rect.y, Mathf.Max(WbbSize.Library.NameMinWidth, right - left), rect.height);
            bool nowChecked = WbbStyles.Checkbox(checkRect, isSelected);
            if (nowChecked != isSelected)
            {
                if (evt.shift)
                    ApplyRangeSelection(index, nowChecked);
                else
                    SetSelected(path, nowChecked);
                lastClickedPath = path;
                isSelected = nowChecked;
                if (host != null)
                    host.RequestRepaint();
            }

            GUIStyle nameStyle = row.tracked ? WbbStyles.RowLabelBold : WbbStyles.RowLabel;
            bool repaint = evt.type == EventType.Repaint;
            rowScratch.text = repaint ? WbbStyles.Fit(row.nameText, nameStyle, nameRect.width) : row.nameText;
            rowScratch.tooltip = hovered && repaint ? BuildRowTip(row) : string.Empty;
            GUI.Label(nameRect, rowScratch, nameStyle);
            WbbStyles.Badge(badgeRect, row.badgeText, row.badgeColor);
            if (row.badgeContent != null)
                GUI.Label(badgeRect, row.badgeContent, GUIStyle.none);
            if (medium)
                GUI.Label(versionRect, row.versionText, rightMini);
            GUI.Label(sizeRect, row.sizeText, rightMini);
            if (wide)
                GUI.Label(dateRect, row.dateText, rightMini);
            if (WbbStyles.IconButton(menuRect, WbbStyles.IconMenu, menuButtonContent.tooltip, WbbButtonKind.Secondary))
            {
                if (!isSelected)
                    SelectOnly(index);
                OpenRowMenu(row);
            }

            if (evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition) && !checkRect.Contains(evt.mousePosition) && !menuRect.Contains(evt.mousePosition))
            {
                if (evt.clickCount >= 2)
                {
                    SelectOnly(index);
                    WbbUtil.RevealFolder(row.file.filePath);
                }
                else if (evt.control || evt.command)
                {
                    SetSelected(path, !isSelected);
                    lastClickedPath = path;
                }
                else if (evt.shift)
                {
                    ApplyRangeSelection(index, true);
                    lastClickedPath = path;
                }
                else
                {
                    SelectOnly(index);
                }

                GUIUtility.keyboardControl = 0;
                evt.Use();
                if (host != null)
                    host.RequestRepaint();
            }
            else if (evt.type == EventType.ContextClick && rect.Contains(evt.mousePosition))
            {
                if (!isSelected)
                    SelectOnly(index);
                OpenRowMenu(row);
                evt.Use();
            }
        }

        private string BuildRowTip(Row row)
        {
            var record = row.record;
            string extra;
            if (record == null)
            {
                extra = L("Library.RowTipUntracked");
            }
            else
            {
                string version = string.IsNullOrEmpty(record.modVersion) ? L("Library.NoVersion") : record.modVersion;
                string author = string.IsNullOrEmpty(record.modAuthor) ? L("Common.None") : record.modAuthor;
                string when = string.IsNullOrEmpty(record.finishedIso) ? record.startedIso : record.finishedIso;
                if (string.IsNullOrEmpty(when))
                    when = L("Common.None");
                string prefab = string.IsNullOrEmpty(record.prefabName) ? L("Common.None") : record.prefabName;
                extra = L("Library.RowTipTracked", version, author, when, prefab);
            }

            return L("Library.RowTip", row.nameText, row.file.filePath, row.sizeText, row.dateText, extra);
        }
    }
}
