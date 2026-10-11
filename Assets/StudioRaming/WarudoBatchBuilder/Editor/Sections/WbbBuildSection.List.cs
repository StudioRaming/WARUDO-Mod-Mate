using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private GUIContent groupTip;
        private Rect sourceListViewportRect;
        private bool sourceListAcceptsPointer;
        private void DrawList()
        {
            sourceListAcceptsPointer = sourceListViewportRect.width > 0f && sourceListViewportRect.Contains(Event.current.mousePosition);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(listViewportHeightLayout));
            using (new EditorGUI.DisabledScope((Event.current.isMouse || Event.current.type == EventType.ContextClick) && !sourceListAcceptsPointer))
            {
                if (units.Count == 0)
                {
                    if (emptyKind == EmptyNoMatch)
                    {
                        GUILayout.Space(WbbSize.SpaceLarge);
                        GUILayout.Label(emptyMessage, WbbStyles.GuideBody);
                    }
                    else if (WbbStyles.EmptyState(emptyTitle, emptyMessage, emptyActionLabel))
                    {
                        RequestEmptyAction();
                    }

                    Rect probe = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
                    MeasureWidth(probe.width);
                }
                else
                {
                    Rect area = GUILayoutUtility.GetRect(0f, Mathf.Max(1f, totalHeight), GUILayout.ExpandWidth(true));
                    MeasureWidth(area.width);
                    bool tracking = Event.current.type == EventType.MouseMove;
                    int hoverCandidate = -1;
                    for (int u = firstUnit; u <= lastUnit && u < units.Count; u++)
                    {
                        Unit unit = units[u];
                        var r = new Rect(area.x, area.y + unit.y, area.width, unit.height);
                        if (unit.group < 0 || unit.group >= groups.Count)
                            continue;
                        Group g = groups[unit.group];
                        switch (unit.kind)
                        {
                            case UnitHeader:
                                DrawGroupHeader(r, g);
                                break;
                            case UnitRow:
                                DrawRow(g.members[unit.start], r, tracking, ref hoverCandidate);
                                break;
                            default:
                                DrawCardRow(g, unit.start, unit.count, r, tracking, ref hoverCandidate);
                                break;
                        }
                    }

                    if (tracking && hoverCandidate != hoverIndex)
                    {
                        hoverIndex = hoverCandidate;
                        RequestRepaint();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            if (Event.current.type == EventType.Repaint)
            {
                Rect viewport = GUILayoutUtility.GetLastRect();
                sourceListViewportRect = viewport;
                if (viewport.height > 10f && Mathf.Abs(viewport.height - measuredViewport) > 1f)
                {
                    measuredViewport = viewport.height;
                    RequestRepaint();
                }
            }
        }

        private void MeasureWidth(float width)
        {
            if (Event.current.type == EventType.Repaint && width > WbbSize.Build.MeasureMinWidth && Mathf.Abs(width - measuredWidth) > 1f)
            {
                measuredWidth = width;
                RequestRepaint();
            }
        }

        private const int EmptyNone = 0;
        private const int EmptyNoMatch = 1;
        private const int EmptyIndexing = 2;
        private const int EmptyAllHidden = 3;
        private const int EmptyNoPrefabs = 4;
        private const int EmptyActionNone = 0;
        private const int EmptyActionShowHidden = 1;
        private const int EmptyActionScopeAll = 2;
        private const int EmptyActionRescan = 3;
        private void RefreshEmptyState()
        {
            emptyAction = EmptyActionNone;
            emptyTitle = "";
            emptyMessage = "";
            if (units.Count > 0)
            {
                emptyKind = EmptyNone;
            }
            else if (items.Count - otherScopeCount > 0)
            {
                emptyKind = EmptyNoMatch;
                emptyMessage = L("Common.NoMatch", items.Count - otherScopeCount);
            }
            else if (indexBusyLayout)
            {
                emptyKind = EmptyIndexing;
                emptyTitle = L("Build.EmptyIndexingTitle");
                emptyMessage = L("Build.EmptyIndexing");
            }
            else if (rawCount + userHiddenCount > 0)
            {
                emptyKind = EmptyAllHidden;
                emptyTitle = L("Build.EmptyTitle");
                emptyMessage = L("Build.EmptyAllHidden", rawCount + userHiddenCount);
                emptyAction = EmptyActionShowHidden;
            }
            else
            {
                bool wholeProject = string.Equals(scopeFolder, "Assets", System.StringComparison.OrdinalIgnoreCase);
                emptyKind = EmptyNoPrefabs;
                emptyTitle = L("Build.EmptyTitle");
                emptyMessage = wholeProject ? L("Build.EmptyNoPrefabsProject") : L("Build.EmptyNoPrefabs");
                if (!busyLayout)
                    emptyAction = wholeProject ? EmptyActionRescan : EmptyActionScopeAll;
            }

            switch (emptyAction)
            {
                case EmptyActionShowHidden:
                    emptyActionLabel = L("Build.EmptyShowHidden");
                    break;
                case EmptyActionScopeAll:
                    emptyActionLabel = L("Build.ScopeAll");
                    break;
                case EmptyActionRescan:
                    emptyActionLabel = L("Build.Rescan");
                    break;
                default:
                    emptyActionLabel = "";
                    break;
            }
        }

        private void RequestEmptyAction()
        {
            switch (emptyAction)
            {
                case EmptyActionShowHidden:
                    pendingShowHidden = 1;
                    break;
                case EmptyActionScopeAll:
                    pendingScope = "Assets";
                    break;
                case EmptyActionRescan:
                    pendingRescan = true;
                    pendingForceIndex = true;
                    break;
                default:
                    return;
            }

            GUIUtility.keyboardControl = 0;
            RequestRepaint();
        }

        private void DrawRow(int itemIndex, Rect rect, bool tracking, ref int hoverCandidate)
        {
            if (itemIndex < 0 || itemIndex >= items.Count)
                return;
            Item item = items[itemIndex];
            Event evt = Event.current;
            bool isSelected = !item.isScene && selected.Contains(item.guid);
            bool hovered = hoverIndex == itemIndex;
            if (tracking && rect.Contains(evt.mousePosition))
                hoverCandidate = itemIndex;
            const float insetX = WbbSize.Build.RowInsetX;
            var inner = new Rect(rect.x + insetX, rect.y + 1f, rect.width - insetX * 2f, rect.height - 2f);
            WbbStyles.RowBackground(inner, isSelected, hovered, false);
            bool dim = item.dimmed;
            const float check = WbbSize.CheckSize;
            const float button = WbbSize.ControlSmall;
            const float thumbInset = WbbSize.Build.RowThumbInset;
            float left = inner.x + WbbSize.Build.RowPadLeft;
            var checkRect = new Rect(left, inner.y + (inner.height - check) * 0.5f, check, check);
            left += WbbSize.Build.RowCheckAdvance;
            var thumbRect = new Rect(left, inner.y + thumbInset, inner.height - thumbInset * 2f, inner.height - thumbInset * 2f);
            left += thumbRect.width + WbbSize.Build.RowThumbGap;
            float right = inner.xMax - WbbSize.Build.RowPadRight;
            var menuRect = new Rect(right - WbbSize.IconButton, inner.y + (inner.height - button) * 0.5f, WbbSize.IconButton, button);
            right -= WbbSize.Build.RowMenuAdvance;
            Rect actionRect;
            if (item.isScene)
            {
                float w = Mathf.Min(WbbSize.Build.RowSceneActionMaxWidth, Mathf.Max(WbbSize.Build.RowSceneActionMinWidth, inner.width * WbbSize.Build.RowSceneActionRatio));
                actionRect = new Rect(right - w, inner.y + (inner.height - button) * 0.5f, w, button);
                right -= w + WbbSize.Build.RowActionGap;
            }
            else
            {
                actionRect = new Rect(right - WbbSize.IconButton, inner.y + (inner.height - button) * 0.5f, WbbSize.IconButton, button);
                right -= WbbSize.Build.RowIconActionAdvance;
            }

            bool wide = inner.width > WbbSize.Build.RowWideWidth;
            var dateRect = new Rect(0f, 0f, 0f, 0f);
            if (wide && !string.IsNullOrEmpty(item.dateText))
            {
                dateRect = new Rect(right - WbbSize.Build.ColDate, inner.y, WbbSize.Build.ColDate, inner.height);
                right -= WbbSize.Build.ColDateAdvance;
            }

            float badgeWidth = Mathf.Clamp(WbbStyles.BadgeWidth(item.badgeText), WbbSize.Build.BadgeMinWidth, WbbSize.Build.BadgeMaxWidth);
            var badgeRect = new Rect(right - badgeWidth, inner.y, badgeWidth, inner.height);
            right -= badgeWidth + WbbSize.Build.BadgeGap;
            float textWidth = Mathf.Max(WbbSize.TextMinWidth, right - left);
            var nameRect = new Rect(left, inner.y + WbbSize.Build.NameTop, textWidth, WbbSize.Build.NameHeight);
            var subRect = new Rect(left, inner.y + WbbSize.Build.SubTop, textWidth, WbbSize.Build.SubHeight);
            if (!item.isScene)
            {
                bool nowChecked = WbbStyles.Checkbox(checkRect, isSelected);
                if (nowChecked != isSelected)
                {
                    if (evt.shift)
                        ApplyRangeSelection(itemIndex, nowChecked);
                    else
                        SetSelected(item.guid, nowChecked);
                    lastClickedGuid = item.guid;
                    isSelected = nowChecked;
                    RequestRepaint();
                }
            }

            if (evt.type == EventType.Repaint)
            {
                WbbTheme.Fill(thumbRect, WbbTheme.P.BgWindow, WbbSize.RadiusThumb);
                Texture2D thumb = EnsureThumb(item);
                if (thumb != null)
                    WbbStyles.DrawTextureAlpha(thumbRect, thumb, dim ? WbbTheme.DimmedAlpha : 1f);
                else
                    WbbStyles.Icon(thumbRect, WbbStyles.EditorIcon(item.isScene ? WbbStyles.IconScene : WbbStyles.IconPrefab), WbbTheme.DisabledIconAlpha);
            }

            if (string.Equals(renamingGuid, item.guid, System.StringComparison.Ordinal))
            {
                DrawRenameField(nameRect, item);
            }
            else
            {
                rowScratch.text = WbbStyles.Fit(item.modName, isSelected ? WbbStyles.RowLabelBold : WbbStyles.RowLabel, nameRect.width);
                rowScratch.tooltip = hovered && evt.type == EventType.Repaint ? item.cardTip : string.Empty;
                rowScratch.image = null;
                WbbStyles.RowText(nameRect, rowScratch, isSelected ? WbbStyles.RowLabelBold : WbbStyles.RowLabel, dim);
            }

            if (evt.type == EventType.Repaint)
            {
                string subText = WbbStyles.Fit(item.subText, subLine, subRect.width);
                if (item.nameDuplicate)
                {
                    Color sub = subLine.normal.textColor;
                    subLine.normal.textColor = WbbTheme.Fg(WbbTone.Error);
                    try
                    {
                        GUI.Label(subRect, subText, subLine);
                    }
                    finally
                    {
                        subLine.normal.textColor = sub;
                    }
                }
                else
                {
                    WbbStyles.RowText(subRect, subText, subLine, dim);
                }
            }

            WbbStyles.Badge(badgeRect, item.badgeText, item.badgeTone);
            if (item.badgeContent != null)
                GUI.Label(badgeRect, item.badgeContent, GUIStyle.none);
            if (dateRect.width > 0f)
                WbbStyles.RowText(dateRect, item.dateText, rightMini, dim);
            if (item.isScene)
            {
                using (new EditorGUI.DisabledScope(busyLayout))
                {
                    if (WbbStyles.Button(actionRect, sceneRebuildContent, WbbButtonKind.Secondary))
                    {
                        pendingSceneRebuildGuid = item.guid;
                        RequestRepaint();
                    }
                }
            }
            else
            {
                if (WbbStyles.IconButton(actionRect, RowActionIcon(item), RowActionContent(item).tooltip, WbbButtonKind.Ghost))
                {
                    if (item.reason == ShowReason.Hidden)
                        pendingUnhideGuid = item.guid;
                    else if (item.reason == ShowReason.Raw)
                        pendingPinGuid = item.guid;
                    else
                        pendingHideGuid = item.guid;
                    RequestRepaint();
                }
            }

            if (WbbStyles.IconButton(menuRect, WbbStyles.IconMenu, menuButtonContent.tooltip, WbbButtonKind.Ghost))
            {
                if (!item.isScene && !isSelected)
                    SelectOnly(itemIndex);
                OpenItemMenu(item);
            }

            if (sourceListAcceptsPointer && evt.type == EventType.MouseDown && evt.button == 0 && inner.Contains(evt.mousePosition) && !checkRect.Contains(evt.mousePosition) && !actionRect.Contains(evt.mousePosition) && !menuRect.Contains(evt.mousePosition))
            {
                HandleClick(itemIndex, item, evt);
            }
            else if (sourceListAcceptsPointer && evt.type == EventType.ContextClick && inner.Contains(evt.mousePosition))
            {
                if (!item.isScene && !isSelected)
                    SelectOnly(itemIndex);
                OpenItemMenu(item);
                evt.Use();
            }
        }

        private void HandleClick(int itemIndex, Item item, Event evt)
        {
            if (!string.IsNullOrEmpty(renamingGuid) && !string.Equals(renamingGuid, item.guid, System.StringComparison.Ordinal))
            {
                CommitRename();
            }

            if (item.isScene)
            {
                if (evt.clickCount >= 2 && item.record != null)
                    WbbQuickBuild.PingSource(item.record);
            }
            else if (evt.clickCount >= 2)
            {
                SelectOnly(itemIndex);
                PingItem(item);
            }
            else if (evt.control || evt.command)
            {
                SetSelected(item.guid, !selected.Contains(item.guid));
                lastClickedGuid = item.guid;
            }
            else if (evt.shift)
            {
                ApplyRangeSelection(itemIndex, true);
                lastClickedGuid = item.guid;
            }
            else
            {
                SelectOnly(itemIndex);
            }

            GUIUtility.keyboardControl = 0;
            evt.Use();
            RequestRepaint();
        }

        private void MarkSelectionChanged()
        {
            namesDirty = true;
            summaryDirty = true;
            preflightDirty = true;
            rebuildQueuedCount = 0;
            rebuildToken = "";
        }

        private void ClearRebuildNameOverrides()
        {
            if (rebuildModNames.Count == 0)
                return;
            rebuildModNames.Clear();
            for (int i = 0; i < items.Count; i++)
                items[i].modNameOverride = "";
            namesDirty = true;
        }

        private void SetSelected(string guid, bool value)
        {
            if (string.IsNullOrEmpty(guid) || guid.StartsWith(ScenePrefix, System.StringComparison.Ordinal))
                return;
            if (value)
                selected.Add(guid);
            else
                selected.Remove(guid);
            MarkSelectionChanged();
        }

        private void SelectOnly(int itemIndex)
        {
            selected.Clear();
            string guid = itemIndex >= 0 && itemIndex < items.Count && !items[itemIndex].isScene ? items[itemIndex].guid : "";
            if (!string.IsNullOrEmpty(guid))
                selected.Add(guid);
            lastClickedGuid = guid;
            MarkSelectionChanged();
        }

        private void ApplyRangeSelection(int itemIndex, bool value)
        {
            int anchor = -1, target = -1;
            for (int i = 0; i < filtered.Count; i++)
            {
                if (filtered[i] == itemIndex)
                    target = i;
                if (string.Equals(items[filtered[i]].guid, lastClickedGuid, System.StringComparison.Ordinal))
                    anchor = i;
            }

            if (target < 0)
                return;
            if (anchor < 0)
            {
                if (value)
                    SelectOnly(itemIndex);
                else
                    SetSelected(items[itemIndex].guid, false);
                return;
            }

            int min = Mathf.Min(anchor, target);
            int max = Mathf.Max(anchor, target);
            for (int i = min; i <= max; i++)
            {
                Item it = items[filtered[i]];
                if (it.isScene || it.group < 0 || it.group >= groups.Count || !groups[it.group].open)
                    continue;
                SetSelected(it.guid, value);
            }
        }

        private void SetGroupSelection(Group g, bool value)
        {
            for (int k = 0; k < g.members.Count; k++)
            {
                Item it = items[g.members[k]];
                if (it.isScene)
                    continue;
                if (value)
                    selected.Add(it.guid);
                else
                    selected.Remove(it.guid);
            }

            MarkSelectionChanged();
            RequestRepaint();
        }

        // Clear Selection drops every selected prefab, including ones a search, filter or other folder scope hides.
        private void ClearAllSelection()
        {
            selected.Clear();
            MarkSelectionChanged();
            RequestRepaint();
        }

        private void SetSelectionForFiltered(bool value)
        {
            for (int i = 0; i < filtered.Count; i++)
            {
                Item it = items[filtered[i]];
                if (it.isScene)
                    continue;
                if (value)
                    selected.Add(it.guid);
                else
                    selected.Remove(it.guid);
            }

            MarkSelectionChanged();
            RequestRepaint();
        }
    }
}
