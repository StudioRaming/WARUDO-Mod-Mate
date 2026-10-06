using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private void DrawGroupHeader(Rect rect, Group g)
        {
            bool toggledAll, toggledOpen;
            WbbStyles.GroupHeader(rect, g.title, g.detail, g.open, g.selected, g.scene ? 0 : g.selectable, g.marked, out toggledAll, out toggledOpen);
            if (groupTip == null)
                groupTip = new GUIContent();
            groupTip.text = "";
            groupTip.tooltip = g.tooltip;
            const float tipLeft = WbbSize.Build.GroupTipLeft;
            GUI.Label(new Rect(rect.x + tipLeft, rect.y, Mathf.Max(0f, rect.width - tipLeft), rect.height), groupTip, GUIStyle.none);
            if (toggledAll)
            {
                SetGroupSelection(g, g.selected < g.selectable);
                GUIUtility.keyboardControl = 0;
            }
            else if (toggledOpen)
            {
                pendingToggleGroup = g.key;
                RequestRepaint();
            }
        }

        private void DrawCardRow(Group g, int start, int count, Rect rowRect, bool tracking, ref int hoverCandidate)
        {
            int cols = Mathf.Max(1, columns);
            const float inset = WbbSize.Build.CardCellInset;
            float cellWidth = Mathf.Max(WbbSize.Build.CardMinWidth, (rowRect.width - inset * 2f) / cols);
            for (int c = 0; c < count; c++)
            {
                int memberIndex = start + c;
                if (memberIndex >= g.members.Count)
                    break;
                var cell = new Rect(rowRect.x + inset + c * cellWidth, rowRect.y + inset, cellWidth - inset * 2f, CardHeight - inset * 2f);
                DrawCard(g.members[memberIndex], cell, tracking, ref hoverCandidate);
            }
        }

        private void DrawCard(int itemIndex, Rect rect, bool tracking, ref int hoverCandidate)
        {
            if (itemIndex < 0 || itemIndex >= items.Count)
                return;
            Item item = items[itemIndex];
            Event evt = Event.current;
            bool isSelected = !item.isScene && selected.Contains(item.guid);
            bool hovered = hoverIndex == itemIndex;
            if (tracking && rect.Contains(evt.mousePosition))
                hoverCandidate = itemIndex;
            if (evt.type == EventType.Repaint)
            {
                WbbPalette p = WbbTheme.P;
                Color fill = isSelected ? p.BgSelected : (hovered ? p.BgCardHover : p.BgCard);
                Color border = isSelected ? p.AccentLine : p.BorderSubtle;
                WbbTheme.Box(rect, fill, border, WbbStyles.CardRadius);
            }

            bool dim = item.dimmed;
            const float pad = WbbSize.Build.CardPad;
            const float badgeInset = WbbSize.Build.CardBadgeInset;
            float thumbHeight = Mathf.Max(WbbSize.Build.CardThumbMinHeight, rect.height - WbbSize.Build.CardTextReserve);
            var thumbRect = new Rect(rect.x + pad, rect.y + pad, Mathf.Max(WbbSize.Build.CardThumbMinWidth, rect.width - pad * 2f), thumbHeight);
            var nameRect = new Rect(thumbRect.x, thumbRect.yMax + WbbSize.Build.CardNameGap, thumbRect.width, WbbSize.Build.CardNameHeight);
            var subRect = new Rect(thumbRect.x, nameRect.yMax, thumbRect.width, WbbSize.Build.CardSubHeight);
            var badgeRect = new Rect(thumbRect.x + badgeInset, thumbRect.y + badgeInset, WbbSize.Build.CardBadgeSize, WbbSize.Build.CardBadgeSize);
            if (evt.type == EventType.Repaint)
            {
                WbbTheme.Fill(thumbRect, WbbTheme.P.BgWindow, WbbSize.RadiusThumbCard);
                Texture2D thumb = EnsureThumb(item);
                if (thumb != null)
                    WbbStyles.DrawTextureAlpha(thumbRect, thumb, dim ? WbbTheme.DimmedAlpha : 1f);
                else
                    WbbStyles.RowText(thumbRect, thumbNoneText, WbbStyles.CenteredMini, dim);
            }

            if (item.badgeMark.HasValue)
                WbbStyles.Badge(badgeRect, item.badgeMark.Value, item.badgeTone);
            else
                WbbStyles.Badge(badgeRect, item.badgeGlyph, item.badgeTone);
            if (item.badgeContent != null)
                GUI.Label(badgeRect, item.badgeContent, GUIStyle.none);
            if (string.Equals(renamingGuid, item.guid, System.StringComparison.Ordinal))
            {
                DrawRenameField(nameRect, item);
            }
            else
            {
                rowScratch.text = WbbStyles.Fit(item.modName, WbbStyles.CenteredMini, nameRect.width);
                rowScratch.tooltip = hovered && evt.type == EventType.Repaint ? item.cardTip : string.Empty;
                rowScratch.image = null;
                WbbStyles.RowText(nameRect, rowScratch, isSelected ? WbbStyles.RowLabelBold : WbbStyles.CenteredMini, dim);
            }

            if (evt.type == EventType.Repaint)
                WbbStyles.RowText(subRect, WbbStyles.Fit(item.isScene ? item.sceneLabel : reasonTexts[(int)item.reason], WbbStyles.CenteredMini, subRect.width), WbbStyles.CenteredMini, dim);
            var actionRect = new Rect(thumbRect.xMax - WbbSize.Build.CardButtonRight, thumbRect.y + WbbSize.Build.CardButtonTop, WbbSize.Build.CardButtonWidth, WbbSize.Build.CardButtonHeight);
            Rect thumbButton = new Rect(0f, 0f, 0f, 0f);
            if (item.isScene)
            {
                if (WbbStyles.IconButton(actionRect, WbbStyles.IconPlay, sceneRebuildIcon.tooltip, WbbButtonKind.Secondary))
                {
                    pendingSceneRebuildGuid = item.guid;
                    RequestRepaint();
                }
            }
            else
            {
                if (WbbStyles.IconButton(actionRect, RowActionIcon(item), RowActionContent(item).tooltip, WbbButtonKind.Secondary))
                {
                    if (item.reason == ShowReason.Hidden)
                        pendingUnhideGuid = item.guid;
                    else if (item.reason == ShowReason.Raw)
                        pendingPinGuid = item.guid;
                    else
                        pendingHideGuid = item.guid;
                    RequestRepaint();
                }

                thumbButton = new Rect(thumbRect.xMax - WbbSize.Build.CardButtonRight, thumbRect.yMax - WbbSize.Build.CardButtonBottom, WbbSize.Build.CardButtonWidth, WbbSize.Build.CardButtonHeight);
                bool thumbClicked = item.thumb != null ? WbbStyles.IconButton(thumbButton, WbbStyles.IconRefresh, thumbRegenContent.tooltip, WbbButtonKind.Secondary) : WbbStyles.IconButton(thumbButton, WbbStyles.IconCamera, thumbCreateContent.tooltip, WbbButtonKind.Secondary);
                if (thumbClicked)
                {
                    pendingThumbGuid = item.guid;
                    RequestRepaint();
                }
            }

            if (sourceListAcceptsPointer && evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition) && !actionRect.Contains(evt.mousePosition) && !(thumbButton.width > 0f && thumbButton.Contains(evt.mousePosition)))
            {
                HandleClick(itemIndex, item, evt);
            }
            else if (sourceListAcceptsPointer && evt.type == EventType.ContextClick && rect.Contains(evt.mousePosition))
            {
                if (!item.isScene && !isSelected)
                    SelectOnly(itemIndex);
                OpenItemMenu(item);
                evt.Use();
            }
        }

        private static string RowActionIcon(Item item)
        {
            if (item.reason == ShowReason.Hidden)
                return WbbStyles.IconUnhide;
            if (item.reason == ShowReason.Raw)
                return WbbStyles.IconPin;
            return WbbStyles.IconHide;
        }

        private GUIContent RowActionContent(Item item)
        {
            if (item.reason == ShowReason.Hidden)
                return unhideContent;
            if (item.reason == ShowReason.Raw)
                return pinContent;
            return hideContent;
        }
    }
}
