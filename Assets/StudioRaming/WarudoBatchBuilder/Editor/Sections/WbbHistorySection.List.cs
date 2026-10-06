using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
        private const float ColumnHeaderHeight = 22f;
        private readonly GUIContent columnHeaderScratch = new GUIContent();
        private float rowWidthObserved;
        private float headerWidthLayout;
        private void DrawList()
        {
            if (entries.Count > 0)
                DrawColumnHeader();
            Rect top = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
            ObserveListTop(top);
            if (Event.current.type == EventType.Layout)
                viewportHeightLayout = listTopObserved > 0f && host != null ? Mathf.Max(EntryHeight * 2f, host.position.height - listTopObserved - footerHeightObserved) : Mathf.Max(EntryHeight * 2f, ListViewportHeight() - 120f);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(viewportHeightLayout));
            int count = entries.Count;
            if (count == 0)
            {
                GUILayout.Space(WbbSize.SpaceLarge);
                if (rows.Count == 0)
                {
                    if (WbbStyles.EmptyState(L("History.EmptyTitle"), L("History.EmptyBody"), L("History.EmptyGoBuild")))
                    {
                        pendingGoToBuild = true;
                        if (host != null)
                            host.RequestRepaint();
                    }
                }
                else
                {
                    GUILayout.Label(L("Common.NoMatch", rows.Count), WbbStyles.MiniInfo);
                }
            }
            else
            {
                bool tracking = Event.current.type == EventType.MouseMove;
                int hoverCandidate = -1;
                float observedWidth = 0f;
                for (int i = 0; i < count; i++)
                {
                    Rect rect = GUILayoutUtility.GetRect(0f, EntryHeight, GUILayout.ExpandWidth(true));
                    if (i == 0)
                        observedWidth = rect.width;
                    if (i < firstVisible || i > lastVisible)
                        continue;
                    Entry entry = entries[i];
                    if (entry.header)
                        DrawGroupHeader(entry, rect);
                    else
                        DrawRow(i, entry.row, rect, tracking, ref hoverCandidate);
                }

                if (tracking && hoverCandidate != hoverIndex)
                {
                    hoverIndex = hoverCandidate;
                    if (host != null)
                        host.RequestRepaint();
                }

                if (Event.current.type == EventType.Repaint && observedWidth > 0f && Mathf.Abs(observedWidth - rowWidthObserved) > 1f)
                {
                    rowWidthObserved = observedWidth;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private float ListViewportHeight()
        {
            float windowHeight = host != null ? host.position.height : Screen.height;
            float reserve = 300f;
            reserve += Mathf.Max(0, typeFilterLinesLayout - 1) * EntryHeight;
            reserve += Mathf.Max(0, statusFilterLinesLayout - 1) * EntryHeight;
            if (groupOwnLineLayout)
                reserve += EntryHeight;
            if (toolbarActionsOwnLineLayout)
                reserve += EntryHeight;
            if (showRecovery)
                reserve += WbbSize.ControlSmall + WbbSize.SpaceSmall;
            if (noticesViewportHeightLayout > 0f)
                reserve += noticesViewportHeightLayout;
            return Mathf.Max(EntryHeight * 2f, windowHeight - reserve);
        }

        private void DrawColumnHeader()
        {
            float width = headerWidthLayout > 0f ? headerWidthLayout : paneWidthLayout;
            width = Mathf.Max(1f, Mathf.Min(width, paneWidthLayout > 0f ? paneWidthLayout : width));
            Rect rect = GUILayoutUtility.GetRect(0f, ColumnHeaderHeight, GUILayout.Width(width), GUILayout.Height(ColumnHeaderHeight));
            WbbStyles.SectionSurface(rect);
            float left = rect.x + WbbSize.History.RowInset + WbbSize.History.CheckAdvance;
            Rect statusRect = new Rect(left, rect.y, WbbSize.History.ColStatus, rect.height);
            left += WbbSize.History.ColStatusStep;
            float right = rect.xMax - WbbSize.History.RowInset - WbbSize.History.MenuAdvance;
            bool huge = rect.width > WbbSize.History.HugeMinWidth;
            bool wide = rect.width > WbbSize.History.WideMinWidth;
            bool medium = rect.width > WbbSize.History.MediumMinWidth;
            bool authorCol = huge && showAuthorColumn;
            bool fileCol = medium && showFileColumn;
            Rect timeRect = new Rect(0f, 0f, 0f, 0f);
            if (medium)
            {
                timeRect = new Rect(right - WbbSize.History.ColTime, rect.y, WbbSize.History.ColTime, rect.height);
                right -= WbbSize.History.ColTimeStep;
            }

            Rect sizeRect = new Rect(right - WbbSize.History.ColSize, rect.y, WbbSize.History.ColSize, rect.height);
            right -= WbbSize.History.ColSizeStep;
            Rect durationRect = new Rect(0f, 0f, 0f, 0f);
            if (wide)
            {
                durationRect = new Rect(right - WbbSize.History.ColDuration, rect.y, WbbSize.History.ColDuration, rect.height);
                right -= WbbSize.History.ColDurationStep;
            }

            Rect authorRect = new Rect(0f, 0f, 0f, 0f);
            if (authorCol)
            {
                authorRect = new Rect(right - WbbSize.History.ColAuthor, rect.y, WbbSize.History.ColAuthor, rect.height);
                right -= WbbSize.History.ColAuthorStep;
            }

            Rect versionRect = new Rect(0f, 0f, 0f, 0f);
            if (medium)
            {
                versionRect = new Rect(right - WbbSize.History.ColVersion, rect.y, WbbSize.History.ColVersion, rect.height);
                right -= WbbSize.History.ColVersionStep;
            }

            Rect typeRect = new Rect(0f, 0f, 0f, 0f);
            if (wide)
            {
                typeRect = new Rect(right - WbbSize.History.ColType, rect.y, WbbSize.History.ColType, rect.height);
                right -= WbbSize.History.ColTypeStep;
            }

            Rect fileRect = new Rect(0f, 0f, 0f, 0f);
            if (fileCol)
            {
                fileRect = new Rect(right - WbbSize.History.ColFile, rect.y, WbbSize.History.ColFile, rect.height);
                right -= WbbSize.History.ColFileStep;
            }

            Rect nameRect = new Rect(left, rect.y, Mathf.Max(WbbSize.TextMinWidth, right - left), rect.height);
            DrawHeaderLabel(statusRect, L("Ui.History.ColumnStatus"), true);
            DrawHeaderLabel(nameRect, L("Ui.History.ColumnName"), false);
            if (fileCol)
                DrawHeaderLabel(fileRect, L("Ui.History.ColumnFile"), true);
            if (wide)
                DrawHeaderLabel(typeRect, L("Ui.History.ColumnType"), true);
            if (medium)
                DrawHeaderLabel(versionRect, L("Ui.History.ColumnVersion"), true);
            if (authorCol)
                DrawHeaderLabel(authorRect, L("Ui.History.ColumnAuthor"), true);
            if (wide)
                DrawHeaderLabel(durationRect, L("Ui.History.ColumnDuration"), true);
            DrawHeaderLabel(sizeRect, L("Ui.History.ColumnSize"), true);
            if (medium)
                DrawHeaderLabel(timeRect, L("Ui.History.ColumnTime"), true);
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

        private void DrawGroupHeader(Entry entry, Rect rect)
        {
            bool isCollapsed = collapsed.Contains(entry.groupKey);
            Event evt = Event.current;
            if (evt.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, WbbTheme.P.OverlayGroupHeader);
            WbbStyles.Chevron(new Rect(rect.x + WbbSize.History.GroupArrowLeft, rect.y, WbbSize.History.GroupArrowWidth, rect.height), !isCollapsed);
            GUI.Label(new Rect(rect.x + WbbSize.History.GroupTitleLeft, rect.y, Mathf.Max(WbbSize.History.GroupTitleMinWidth, rect.width - WbbSize.History.GroupTitleReserve), rect.height), entry.text, WbbStyles.RowLabelBold);
            if (evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition))
            {
                if (isCollapsed)
                    collapsed.Remove(entry.groupKey);
                else
                    collapsed.Add(entry.groupKey);
                filterDirty = true;
                GUIUtility.keyboardControl = 0;
                evt.Use();
                if (host != null)
                    host.RequestRepaint();
            }
        }

        private void DrawRow(int entryIndex, int rowIndex, Rect rect, bool tracking, ref int hoverCandidate)
        {
            if (rowIndex < 0 || rowIndex >= rows.Count)
                return;
            Row row = rows[rowIndex];
            WbbRecord record = row.record;
            Event evt = Event.current;
            string id = record.id;
            bool isSelected = selected.Contains(id);
            bool hovered = hoverIndex == entryIndex;
            if (tracking && rect.Contains(evt.mousePosition))
                hoverCandidate = entryIndex;
            WbbStyles.RowBackground(rect, isSelected, hovered, (entryIndex & 1) == 1);
            float left = rect.x + WbbSize.History.RowInset;
            var checkRect = new Rect(left, rect.y + WbbSize.History.CheckTop, WbbSize.CheckSize, WbbSize.CheckSize);
            left += WbbSize.History.CheckAdvance;
            var statusRect = new Rect(left, rect.y, WbbSize.History.ColStatus, rect.height);
            left += WbbSize.History.ColStatusStep;
            float right = rect.xMax - WbbSize.History.RowInset;
            var menuRect = new Rect(right - WbbSize.IconButton, rect.y + WbbSize.RowButtonInset, WbbSize.IconButton, rect.height - WbbSize.History.MenuHeightTrim);
            right -= WbbSize.History.MenuAdvance;
            bool huge = rect.width > WbbSize.History.HugeMinWidth;
            bool wide = rect.width > WbbSize.History.WideMinWidth;
            bool medium = rect.width > WbbSize.History.MediumMinWidth;
            bool authorCol = huge && showAuthorColumn;
            bool fileCol = medium && showFileColumn;
            var timeRect = new Rect(0f, 0f, 0f, 0f);
            if (medium)
            {
                timeRect = new Rect(right - WbbSize.History.ColTime, rect.y, WbbSize.History.ColTime, rect.height);
                right -= WbbSize.History.ColTimeStep;
            }

            var sizeRect = new Rect(right - WbbSize.History.ColSize, rect.y, WbbSize.History.ColSize, rect.height);
            right -= WbbSize.History.ColSizeStep;
            var durationRect = new Rect(0f, 0f, 0f, 0f);
            if (wide)
            {
                durationRect = new Rect(right - WbbSize.History.ColDuration, rect.y, WbbSize.History.ColDuration, rect.height);
                right -= WbbSize.History.ColDurationStep;
            }

            var authorRect = new Rect(0f, 0f, 0f, 0f);
            if (authorCol)
            {
                authorRect = new Rect(right - WbbSize.History.ColAuthor, rect.y, WbbSize.History.ColAuthor, rect.height);
                right -= WbbSize.History.ColAuthorStep;
            }

            var versionRect = new Rect(0f, 0f, 0f, 0f);
            if (medium)
            {
                versionRect = new Rect(right - WbbSize.History.ColVersion, rect.y, WbbSize.History.ColVersion, rect.height);
                right -= WbbSize.History.ColVersionStep;
            }

            var typeRect = new Rect(0f, 0f, 0f, 0f);
            if (wide)
            {
                typeRect = new Rect(right - WbbSize.History.ColType, rect.y, WbbSize.History.ColType, rect.height);
                right -= WbbSize.History.ColTypeStep;
            }

            var fileRect = new Rect(0f, 0f, 0f, 0f);
            if (fileCol)
            {
                fileRect = new Rect(right - WbbSize.History.ColFile, rect.y, WbbSize.History.ColFile, rect.height);
                right -= WbbSize.History.ColFileStep;
            }

            var nameRect = new Rect(left, rect.y, Mathf.Max(WbbSize.TextMinWidth, right - left), rect.height);
            var fixupRect = new Rect(0f, 0f, 0f, 0f);
            bool showFixup = row.hasFixups && nameRect.width - fixupBadgeWidth - WbbSize.SpaceSmall >= WbbSize.TextMinWidth;
            if (showFixup)
            {
                fixupRect = new Rect(nameRect.xMax - fixupBadgeWidth, rect.y, fixupBadgeWidth, rect.height);
                nameRect.width -= fixupBadgeWidth + WbbSize.SpaceSmall;
            }

            bool nowChecked = WbbStyles.Checkbox(checkRect, isSelected);
            if (nowChecked != isSelected)
            {
                if (evt.shift)
                    ApplyRangeSelection(entryIndex, nowChecked);
                else
                    SetSelected(id, nowChecked);
                lastClickedId = id;
                isSelected = nowChecked;
                if (host != null)
                    host.RequestRepaint();
            }

            WbbStyles.Badge(statusRect, row.statusText, row.statusTone);
            GUIStyle nameStyle = row.statusBucket == 1 ? WbbStyles.RowLabelBold : WbbStyles.RowLabel;
            bool repaint = evt.type == EventType.Repaint;
            rowScratch.text = repaint ? WbbStyles.Fit(row.nameText, nameStyle, nameRect.width) : row.nameText;
            rowScratch.tooltip = hovered && repaint ? BuildRowTip(row) : string.Empty;
            GUI.Label(nameRect, rowScratch, nameStyle);
            if (showFixup)
            {
                WbbStyles.Badge(fixupRect, fixupBadgeText, WbbTone.Info);
                fixupTipScratch.text = string.Empty;
                fixupTipScratch.tooltip = hovered && evt.type == EventType.Repaint ? FixupTip(record) : string.Empty;
                GUI.Label(fixupRect, fixupTipScratch, GUIStyle.none);
            }

            if (fileCol && row.fileState == 2)
                WbbStyles.Badge(fileRect, fileMissingText, WbbTone.Warning);
            if (wide)
                GUI.Label(typeRect, row.typeText, rightMini);
            if (medium)
                GUI.Label(versionRect, row.versionText, rightMini);
            if (authorCol)
                GUI.Label(authorRect, row.authorText, rightMini);
            if (wide)
                GUI.Label(durationRect, row.durationText, rightMini);
            GUI.Label(sizeRect, row.sizeText, rightMini);
            if (medium)
                GUI.Label(timeRect, row.timeText, rightMini);
            if (WbbStyles.IconButton(menuRect, WbbStyles.IconMenu, menuButtonContent.tooltip, WbbButtonKind.Secondary))
            {
                if (!isSelected)
                    SelectOnly(entryIndex);
                OpenRowMenu(row);
            }

            if (evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition) && !checkRect.Contains(evt.mousePosition) && !menuRect.Contains(evt.mousePosition))
            {
                if (evt.clickCount >= 2)
                {
                    SelectOnly(entryIndex);
                    PingPrefab(record);
                }
                else if (evt.control || evt.command)
                {
                    SetSelected(id, !isSelected);
                    lastClickedId = id;
                }
                else if (evt.shift)
                {
                    ApplyRangeSelection(entryIndex, true);
                    lastClickedId = id;
                }
                else
                {
                    SelectOnly(entryIndex);
                }

                GUIUtility.keyboardControl = 0;
                evt.Use();
                if (host != null)
                    host.RequestRepaint();
            }
            else if (evt.type == EventType.ContextClick && rect.Contains(evt.mousePosition))
            {
                if (!isSelected)
                    SelectOnly(entryIndex);
                OpenRowMenu(row);
                evt.Use();
            }
        }

        private string BuildRowTip(Row row)
        {
            WbbRecord record = row.record;
            string none = L("Common.None");
            var sb = new System.Text.StringBuilder();
            sb.Append(row.nameText).Append('\n');
            sb.Append(L("History.TipLine1", WbbLang.TypeName(record.ExportType), string.IsNullOrEmpty(record.modVersion) ? none : record.modVersion, string.IsNullOrEmpty(record.modAuthor) ? none : record.modAuthor));
            sb.Append('\n').Append(L("History.TipLine2", row.statusText, string.IsNullOrEmpty(record.startedIso) ? none : record.startedIso, WbbUtil.FormatDuration(record.durationSec)));
            sb.Append('\n').Append(L("History.TipLine3", string.IsNullOrEmpty(record.outputFilePath) ? none : record.outputFilePath, row.fileText));
            if (record.IsSceneSource)
                sb.Append('\n').Append(WbbQuickBuild.DescribeSource(record));
            else
                sb.Append('\n').Append(L("History.TipLine4", string.IsNullOrEmpty(record.prefabPath) ? none : record.prefabPath));
            if (!string.IsNullOrEmpty(record.jobName))
                sb.Append('\n').Append(L("History.TipLine6", record.jobName));
            if (!string.IsNullOrEmpty(record.message))
                sb.Append('\n').Append(L("History.TipLine5", record.message));
            if (row.hasFixups)
                sb.Append("\n\n").Append(FixupTip(record));
            return sb.ToString();
        }

        private static string FixupTip(WbbRecord record)
        {
            return L("Safety.FixupHeader") + "\n" + (record.fixupReport ?? "").Trim();
        }
    }
}
