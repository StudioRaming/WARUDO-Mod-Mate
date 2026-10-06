using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private readonly float[] toolWidths = new float[3];
        private float measuredListHeight;
        private Rect listViewportRect;
        private float measuredListTop;
        private float measuredFooterHeight = WbbSize.ControlLarge;
        private float listHeightLayout = WbbSize.Package.EmbeddedListHeight;
        private bool riskNoticeExpanded;
        private int pendingRiskNoticeExpanded = -1;
        private readonly GUIContent riskSummaryContent = new GUIContent();
        private readonly float[] toolGaps = new float[3];
        private readonly bool[] toolBreakLayout = new bool[3];
        private readonly float[] barWidths = new float[2];
        private readonly float[] barGaps = new float[2];
        private readonly bool[] barBreakLayout = new bool[2];
        private float selectAllWidth = WbbSize.Package.SelectAllButton;
        private float clearExcludedWidth = WbbSize.Package.ClearExcludedButton;
        private float refreshWidth = WbbSize.Package.RefreshButton;
        private float resultOpenWidth = WbbSize.Package.BarOpenFolderButton;
        private void SyncBarLayout()
        {
            selectAllWidth = Mathf.Max(WbbSize.Package.SelectAllButton, Mathf.Max(WbbStyles.ButtonWidth(selectAllContent), WbbStyles.ButtonWidth(LC("Common.DeselectAll"))));
            clearExcludedWidth = Mathf.Max(WbbSize.Package.ClearExcludedButton, WbbStyles.ButtonWidth(LC("Package.ClearExcluded")));
            refreshWidth = Mathf.Max(WbbSize.Package.RefreshButton, WbbStyles.ButtonWidth(LC("Common.Refresh")));
            resultOpenWidth = Mathf.Max(WbbSize.Package.BarOpenFolderButton, WbbStyles.ButtonWidth(outputOpenContent));
            RectOffset none = GUIStyle.none.margin;
            RectOffset label = WbbStyles.Caption.margin;
            float available = PaneRowLimit();
            excludedCountContent.text = excludedCountText;
            toolWidths[0] = selectAllWidth * 2f + WbbSize.SpaceMedium + WbbSize.IconButtonWide * 2f;
            toolWidths[1] = WbbStyles.TextWidth(WbbStyles.Caption, excludedCountContent);
            toolWidths[2] = clearExcludedWidth;
            toolGaps[1] = WbbStyles.LayoutGap(none, label, WbbSize.SpaceMedium);
            toolGaps[2] = WbbStyles.LayoutGap(label, none);
            WbbStyles.PlanWrap(toolWidths, toolGaps, available, toolBreakLayout);
            barWidths[0] = label.left + WbbStyles.TextWidth(WbbStyles.Caption, GUIContentOf(includedText));
            barWidths[1] = WbbSize.Package.ExportButton;
            barGaps[1] = WbbStyles.LayoutGap(label, none);
            WbbStyles.PlanWrap(barWidths, barGaps, available, barBreakLayout);
        }

        private readonly GUIContent measureScratch = new GUIContent();
        private GUIContent GUIContentOf(string text)
        {
            measureScratch.text = text ?? "";
            measureScratch.tooltip = string.Empty;
            measureScratch.image = null;
            return measureScratch;
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            string next = WbbStyles.SearchField(search, WbbSize.Package.SearchMinWidth);
            if (next != search)
            {
                search = next;
                structureDirty = true;
                if (host != null)
                    host.RequestRepaint();
            }

            Rect searchRow = GUILayoutUtility.GetLastRect();
            GUI.Label(new Rect(searchRow.x, searchRow.y, Mathf.Max(0f, searchRow.width - WbbSize.Field.SearchClearWidth), searchRow.height), searchTipContent, GUIStyle.none);
            using (new EditorGUI.DisabledScope(busy))
            {
                Rect refreshRect = EditorGUILayout.GetControlRect(false, WbbSize.ControlSmall, GUILayout.Width(refreshWidth));
                if (WbbStyles.Button(refreshRect, LC("Common.Refresh"), WbbButtonKind.Secondary))
                {
                    pendingValidate = true;
                    pendingCollect = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(busy || assets.Count == 0))
            {
                if (WbbStyles.MiniButton(selectAllContent, GUILayout.Width(selectAllWidth)))
                    SetAllIncluded(true);
                if (WbbStyles.MiniButton(LC("Common.DeselectAll"), GUILayout.Width(selectAllWidth)))
                    SetAllIncluded(false);
                GUILayout.Space(WbbSize.SpaceMedium);
                if (WbbStyles.IconButton(UI.SRGlyph.ChevronDown, expandAllContent.tooltip, WbbButtonKind.Secondary, GUILayout.Width(WbbSize.IconButtonWide), GUILayout.Height(WbbSize.ControlMini)))
                {
                    pendingExpandAll = 1;
                    if (host != null)
                        host.RequestRepaint();
                }

                if (WbbStyles.IconButton(UI.SRGlyph.ChevronRight, collapseAllContent.tooltip, WbbButtonKind.Secondary, GUILayout.Width(WbbSize.IconButtonWide), GUILayout.Height(WbbSize.ControlMini)))
                {
                    pendingExpandAll = 0;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            WbbStyles.NextCell(toolBreakLayout[1], WbbSize.SpaceMedium);
            excludedCountContent.text = excludedCountText;
            GUILayout.Label(excludedCountContent, midMini);
            WbbStyles.NextCell(toolBreakLayout[2], 0f);
            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(busy))
            {
                if (WbbStyles.MiniButton(LC("Package.ClearExcluded"), GUILayout.Width(clearExcludedWidth)))
                {
                    pendingClearExcluded = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(riskNoticeText))
            {
                EditorGUILayout.BeginHorizontal();
                riskSummaryContent.text = riskOnCount > 0 ? L("Package.RiskIncludedSummary", riskOnCount, riskOffCount) : L("Package.RiskExcludedSummary", riskOffCount);
                riskSummaryContent.tooltip = riskNoticeText;
                riskSummaryContent.image = UnityEditor.EditorGUIUtility.IconContent(riskOnCount > 0 ? "console.warnicon.sml" : "console.infoicon.sml").image;
                GUILayout.Label(riskSummaryContent, WbbStyles.Caption, GUILayout.MinWidth(0f));
                GUILayout.FlexibleSpace();
                if (WbbStyles.MiniButton(LC(riskNoticeExpanded ? "Package.RiskLess" : "Package.RiskMore")))
                {
                    pendingRiskNoticeExpanded = riskNoticeExpanded ? 0 : 1;
                    if (host != null)
                        host.RequestRepaint();
                }

                EditorGUILayout.EndHorizontal();
                if (riskNoticeExpanded)
                    WbbStyles.Callout(riskNoticeText, riskNoticeTone);
            }
        }

        private void DrawList()
        {
            if (Event.current.type == EventType.ScrollWheel && visual.Count > 0 && listViewportRect.Contains(Event.current.mousePosition))
            {
                scroll.y = Mathf.Clamp(scroll.y + Event.current.delta.y * RowHeight * 3f, 0f, Mathf.Max(0f, visual.Count * RowHeight - measuredListHeight));
                Event.current.Use();
                if (host != null)
                    host.RequestRepaint();
            }

            Rect top = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint && top.y > 0f && Mathf.Abs(top.y - measuredListTop) > 1f)
            {
                measuredListTop = top.y;
                if (host != null)
                    host.RequestRepaint();
            }

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(listHeightLayout));
            int count = visual.Count;
            if (count == 0)
            {
                if (emptyKind == EmptyNoMatch)
                {
                    GUILayout.Space(WbbSize.SpaceLarge);
                    GUILayout.Label(emptyMessage, WbbStyles.MiniInfo);
                }
                else
                {
                    float minHeight = Embedded ? WbbSize.Package.EmbeddedEmptyHeight : WbbSize.EmptyStateHeight;
                    if (WbbStyles.EmptyState(emptyTitle, emptyMessage, emptyActionLabel, minHeight))
                        RequestEmptyAction();
                }
            }
            else
            {
                bool tracking = Event.current.type == EventType.MouseMove;
                int hoverCandidate = -1;
                for (int i = 0; i < count; i++)
                {
                    Rect rect = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
                    if (i < firstVisible || i > lastVisible)
                        continue;
                    int nodeIndex = visual[i];
                    if (nodeIndex < 0 || nodeIndex >= nodes.Count)
                        continue;
                    if (nodes[nodeIndex].isFolder)
                        DrawFolderRow(i, nodeIndex, rect, tracking, ref hoverCandidate);
                    else
                        DrawFileRow(i, nodeIndex, rect, tracking, ref hoverCandidate);
                }

                if (tracking && hoverCandidate != hoverIndex)
                {
                    hoverIndex = hoverCandidate;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndScrollView();
            if (Event.current.type == EventType.Repaint)
            {
                listViewportRect = GUILayoutUtility.GetLastRect();
                float height = listViewportRect.height;
                if (height > 1f && Mathf.Abs(height - measuredListHeight) > 1f)
                {
                    measuredListHeight = height;
                    if (host != null)
                        host.RequestRepaint();
                }
            }
        }

        private float IndentOf(Node node)
        {
            int depth = Mathf.Clamp(node.depth, 0, MaxIndentDepth);
            return WbbSize.Package.TreeLeft + depth * IndentWidth;
        }

        private void DrawFolderRow(int visualIndex, int nodeIndex, Rect rect, bool tracking, ref int hoverCandidate)
        {
            Node node = nodes[nodeIndex];
            Event evt = Event.current;
            bool hovered = hoverIndex == visualIndex;
            if (tracking && rect.Contains(evt.mousePosition))
                hoverCandidate = visualIndex;
            if (evt.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, WbbTheme.P.OverlayFolderRow);
                if (hovered)
                {
                    EditorGUI.DrawRect(rect, WbbTheme.P.OverlayFolderHover);
                }
            }

            float left = rect.x + IndentOf(node);
            var foldRect = new Rect(left, rect.y + 1f, WbbSize.FoldoutSize, rect.height - WbbSize.Package.FoldInset);
            left += WbbSize.Package.ColFold;
            var checkRect = new Rect(left, rect.y + WbbSize.Package.CheckTop, WbbSize.CheckSize, WbbSize.CheckSize);
            left += WbbSize.Package.ColCheck;
            float right = rect.xMax - WbbSize.Package.FolderRowRight;
            var infoRect = new Rect(right - WbbSize.Package.ColFolderInfo, rect.y, WbbSize.Package.ColFolderInfo, rect.height);
            right -= WbbSize.Package.ColFolderInfoReserve;
            var labelRect = new Rect(left, rect.y, Mathf.Max(WbbSize.TextMinWidth, right - left), rect.height);
            int canTurnOff = node.shownIncluded - node.shownRoots;
            int canTurnOn = node.shownFiles - node.shownIncluded - node.shownRiskOff;
            bool locked = canTurnOff <= 0 && canTurnOn <= 0;
            bool allOn = locked ? node.shownFiles > 0 && node.shownIncluded >= node.shownFiles : canTurnOn <= 0;
            bool mixed = !locked && canTurnOff > 0 && canTurnOn > 0;
            using (new EditorGUI.DisabledScope(locked))
            {
                bool nextChecked = WbbStyles.Checkbox(checkRect, allOn, mixed);
                if (nextChecked != allOn)
                    SetFolderIncluded(nodeIndex, nextChecked);
            }

            if (locked)
                GUI.Label(checkRect, folderLockContent, GUIStyle.none);
            bool open = searching || node.expanded;
            bool nextOpen = WbbStyles.FoldoutArrow(foldRect, open);
            if (!searching && nextOpen != open)
                RequestToggle(nodeIndex, nextOpen);
            GUI.Label(labelRect, node.labelContent, WbbStyles.RowLabelBold);
            GUI.Label(infoRect, node.infoText, rightMini);
            if (!searching && evt.type == EventType.MouseDown && evt.button == 0 && labelRect.Contains(evt.mousePosition))
            {
                RequestToggle(nodeIndex, !node.expanded);
                evt.Use();
            }
        }

        private void RequestEmptyAction()
        {
            if (busy)
                return;
            switch (emptyAction)
            {
                case EmptyActionAddSelection:
                    pendingAddSelection = true;
                    break;
                case EmptyActionCollect:
                    pendingValidate = true;
                    pendingCollect = true;
                    break;
                default:
                    return;
            }

            if (host != null)
                host.RequestRepaint();
        }

        private void RequestToggle(int nodeIndex, bool open)
        {
            pendingToggleNode = nodeIndex;
            pendingToggleValue = open;
            if (host != null)
                host.RequestRepaint();
        }

        private void DrawFileRow(int visualIndex, int nodeIndex, Rect rect, bool tracking, ref int hoverCandidate)
        {
            Node node = nodes[nodeIndex];
            int assetIndex = node.assetIndex;
            if (assetIndex < 0 || assetIndex >= assets.Count)
                return;
            WbbPackageAsset asset = assets[assetIndex];
            Event evt = Event.current;
            bool included = IsIncluded(asset);
            bool hovered = hoverIndex == visualIndex;
            if (tracking && rect.Contains(evt.mousePosition))
                hoverCandidate = visualIndex;
            WbbStyles.RowBackground(rect, false, hovered, (visualIndex & 1) == 1);
            float left = rect.x + IndentOf(node) + WbbSize.Package.ColFold;
            var checkRect = new Rect(left, rect.y + WbbSize.Package.CheckTop, WbbSize.CheckSize, WbbSize.CheckSize);
            left += WbbSize.Package.ColCheck;
            float right = rect.xMax - WbbSize.Package.RowRight;
            var pingRect = new Rect(right - WbbSize.Package.PingButton, rect.y + 1f, WbbSize.Package.PingButton, rect.height - WbbSize.RowButtonInset);
            right -= WbbSize.Package.PingReserve;
            var sizeRect = new Rect(right - WbbSize.Package.ColSize, rect.y, WbbSize.Package.ColSize, rect.height);
            right -= WbbSize.Package.ColSizeReserve;
            var badgeRect = new Rect(0f, 0f, 0f, 0f);
            if (asset.isRoot)
            {
                badgeRect = new Rect(right - WbbSize.Package.RootBadge, rect.y, WbbSize.Package.RootBadge, rect.height);
                right -= WbbSize.Package.RootBadgeReserve;
            }

            string chipText = null;
            GUIContent chipTip = null;
            WbbTone chipTone = WbbTone.Info;
            float chipWidth = 0f;
            if (asset.redistributionRisk)
            {
                chipText = riskChipText;
                chipTip = riskChipContent;
                chipTone = WbbTone.Warning;
                chipWidth = riskChipWidth;
            }
            else if (asset.customScript)
            {
                chipText = customChipText;
                chipTip = customChipContent;
                chipTone = WbbTone.Info;
                chipWidth = customChipWidth;
            }

            var chipRect = new Rect(0f, 0f, 0f, 0f);
            if (chipText != null)
            {
                chipRect = new Rect(right - chipWidth, rect.y, chipWidth, rect.height);
                right -= chipWidth + RowChipGap;
            }

            var nameRect = new Rect(left, rect.y, Mathf.Max(WbbSize.TextMinWidth, right - left), rect.height);
            using (new EditorGUI.DisabledScope(asset.isRoot))
            {
                bool nextChecked = WbbStyles.Checkbox(checkRect, included);
                if (nextChecked != included)
                    SetIncluded(asset, nextChecked);
            }

            if (asset.isRoot)
                GUI.Label(checkRect, rootLockContent, GUIStyle.none);
            GUIStyle nameStyle = included ? WbbStyles.RowLabel : mutedLabel;
            rowScratch.text = node.label;
            rowScratch.tooltip = hovered && evt.type == EventType.Repaint ? BuildAssetTip(assetIndex, asset, included) : string.Empty;
            GUI.Label(nameRect, rowScratch, nameStyle);
            if (!included && evt.type == EventType.Repaint)
            {
                rowScratch.tooltip = string.Empty;
                float textWidth = Mathf.Min(nameRect.width - WbbSize.Package.StrikeInset, nameStyle.CalcSize(rowScratch).x);
                if (textWidth > WbbSize.Package.StrikeMinWidth)
                {
                    EditorGUI.DrawRect(new Rect(nameRect.x + WbbSize.Package.StrikeLeft, rect.y + rect.height * 0.5f, textWidth, WbbSize.Line), WbbStyles.Muted);
                }
            }

            if (asset.isRoot)
            {
                WbbStyles.Badge(badgeRect, rootBadgeText, WbbTone.Neutral);
                GUI.Label(badgeRect, rootLockContent, GUIStyle.none);
            }

            if (chipText != null)
            {
                WbbStyles.Badge(chipRect, chipText, chipTone);
                GUI.Label(chipRect, chipTip, GUIStyle.none);
            }

            GUI.Label(sizeRect, assetSizeTexts[assetIndex], rightMini);
            if (WbbStyles.MiniButton(pingRect, pingContent))
                pendingPingPath = asset.assetPath;
        }

        private string BuildAssetTip(int assetIndex, WbbPackageAsset asset, bool included)
        {
            string folder;
            if (!gather.TryGetFolder(asset.assetPath, asset.category, out folder))
            {
                folder = assetIndex >= 0 && assetIndex < assetFolderTexts.Count ? assetFolderTexts[assetIndex] : "";
            }

            string state;
            if (included)
                state = L("Package.StateIncluded");
            else if (asset.redistributionRisk && !excluded.Contains(asset.guid))
                state = L("Package.StateRiskExcluded");
            else
                state = L("Package.StateExcluded");
            return L("Package.ItemTip", asset.fileName, asset.assetPath, folder, WbbUtil.FormatSize(asset.sizeBytes), state);
        }

        private void DrawBottomBar()
        {
            WbbStyles.Separator();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(includedText, midMini);
            WbbStyles.NextCell(barBreakLayout[1], 0f);
            GUILayout.FlexibleSpace();
            GUIContent export = targets.Count == 0 ? exportNoTargetsContent : includedCount == 0 ? exportNoAssetsContent : exportContent;
            using (new EditorGUI.DisabledScope(busy || targets.Count == 0 || includedCount == 0))
            {
                if (WbbStyles.PrimaryButton(export, GUILayout.Height(WbbSize.Control), GUILayout.Width(WbbSize.Package.ExportButton)))
                {
                    pendingExport = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(statusLine) && !string.IsNullOrEmpty(statusFolder))
            {
                EditorGUILayout.BeginHorizontal();
                WbbStyles.Callout(statusLine, WbbStyles.ToneOf(statusType));
                if (WbbStyles.PlainButton(outputOpenContent, GUILayout.Height(WbbSize.Control), GUILayout.Width(resultOpenWidth)))
                    WbbUtil.RevealFolder(statusFolder);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                WbbStyles.StatusLine(statusLine, statusType);
            }

            GUILayout.Space(WbbSize.SpaceHair);
        }
    }
}
