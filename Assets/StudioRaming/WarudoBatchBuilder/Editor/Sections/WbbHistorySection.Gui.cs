using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
        private readonly GUIContent selectionCueScratch = new GUIContent();
        private readonly float[] typeFilterWidths = new float[TypeCount];
        private readonly float[] typeFilterGaps = new float[TypeCount];
        private readonly bool[] typeFilterBreakLayout = new bool[TypeCount];
        private readonly float[] statusFilterWidths = new float[StatusCount];
        private readonly float[] statusFilterGaps = new float[StatusCount];
        private readonly bool[] statusFilterBreakLayout = new bool[StatusCount];
        private bool groupOwnLineLayout;
        private bool toolbarActionsOwnLineLayout;
        private int typeFilterLinesLayout = 1;
        private int statusFilterLinesLayout = 1;
        private float paneWidthLayout;
        private float noticesViewportHeightLayout;
        private Vector2 noticesScroll;
        public void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
                SyncStructure();
            EnsureLocalStyles();
            EnsureContents();
            if (Event.current.type == EventType.Layout)
                SyncResponsiveLayout();
            DrawNotices();
            DrawStats();
            DrawFilters();
            DrawToolbar();
            DrawList();
            DrawFooterAndStatus();
            ProcessPending();
        }

        private void EnsureLocalStyles()
        {
            bool dark = EditorGUIUtility.isProSkin;
            if (rightMini != null && midMini != null && localStylesDark == dark)
                return;
            localStylesDark = dark;
            rightMini = new GUIStyle(WbbStyles.Caption)
            {
                alignment = TextAnchor.MiddleRight
            };
            midMini = new GUIStyle(WbbStyles.Caption)
            {
                alignment = TextAnchor.MiddleLeft
            };
        }

        private void EnsureContents()
        {
            if (typeToggleContents != null && contentVersion == WbbLang.Version)
                return;
            contentVersion = WbbLang.Version;
            string typeTip = L("History.FilterType.tip");
            typeLabelContent = new GUIContent(L("History.FilterType"), typeTip);
            typeToggleContents = new GUIContent[TypeCount];
            typeToggleContents[0] = new GUIContent(L("Common.TypeProp"), typeTip);
            typeToggleContents[1] = new GUIContent(L("Common.TypeCharacter"), typeTip);
            typeToggleContents[2] = new GUIContent(L("Common.TypeEnvironment"), typeTip);
            string stateTip = L("History.FilterStatus.tip");
            statusToggleContents = new GUIContent[StatusCount];
            statusToggleContents[0] = new GUIContent(L("Common.StatusSuccess"), stateTip);
            statusToggleContents[1] = new GUIContent(L("Common.StatusFailed"), stateTip);
            statusToggleContents[2] = new GUIContent(L("History.StatusOther"), L("History.StatusOther.tip"));
            statusLabelContent = new GUIContent(L("History.FilterStatus"), stateTip);
            groupLabelContent = new GUIContent(L("History.Group"), L("History.Group.tip"));
            filterLabelWidth = Mathf.Max(WbbStyles.TextWidth(midMini, typeLabelContent), WbbStyles.TextWidth(midMini, statusLabelContent));
            filterChipWidth = Mathf.Max(WbbSize.History.StatusToggleButton, Mathf.Max(WbbStyles.ToggleGroupWidth(typeToggleContents), WbbStyles.ToggleGroupWidth(statusToggleContents)));
            groupLabelWidth = WbbStyles.TextWidth(midMini, groupLabelContent);
            groupNames = new[]
            {
                L("History.GroupDate"),
                L("History.GroupJob"),
                L("History.GroupFlat")
            };
            menuButtonContent = new GUIContent(string.Empty, L("History.RowMenu.tip"));
            Texture helpIcon = null;
            try
            {
                GUIContent icon = EditorGUIUtility.IconContent("_Help");
                if (icon != null)
                    helpIcon = icon.image;
            }
            catch (System.Exception)
            {
                helpIcon = null;
            }

            searchHelpContent = helpIcon != null ? new GUIContent(helpIcon, L("History.SearchHint")) : new GUIContent(L("Common.Search"), L("History.SearchHint"));
        }

        private void SyncResponsiveLayout()
        {
            float available = host != null ? host.position.width - WbbSize.SpaceEdge * 2f : Screen.width - WbbSize.SpaceEdge * 2f;
            paneWidthLayout = Mathf.Max(120f, available);
            headerWidthLayout = rowWidthObserved > 0f ? Mathf.Min(rowWidthObserved, paneWidthLayout) : paneWidthLayout;
            float chipsAvailable = Mathf.Max(80f, paneWidthLayout - filterLabelWidth - WbbSize.SpaceSmall);
            for (int i = 0; i < TypeCount; i++)
            {
                typeFilterWidths[i] = filterChipWidth;
                typeFilterGaps[i] = 0f;
            }

            for (int i = 0; i < StatusCount; i++)
            {
                statusFilterWidths[i] = filterChipWidth;
                statusFilterGaps[i] = 0f;
            }

            typeFilterLinesLayout = WbbStyles.PlanWrap(typeFilterWidths, typeFilterGaps, chipsAvailable, typeFilterBreakLayout);
            statusFilterLinesLayout = WbbStyles.PlanWrap(statusFilterWidths, statusFilterGaps, chipsAvailable, statusFilterBreakLayout);
            float groupWidth = filterLabelWidth + filterChipWidth * StatusCount + WbbSize.SpaceMedium + groupLabelWidth + WbbSize.History.GroupPopup + WbbSize.SpaceLarge;
            groupOwnLineLayout = typeFilterLinesLayout > 1 || statusFilterLinesLayout > 1 || groupWidth > paneWidthLayout;
            float toolbarWidth = WbbSize.History.SearchMinWidth + WbbSize.Field.SearchClearWidth + WbbSize.History.SearchHelpWidth + WbbSize.History.ExportCsvButton + WbbSize.History.RefreshButton + WbbSize.SpaceLarge * 2f;
            toolbarActionsOwnLineLayout = toolbarWidth > paneWidthLayout;
            bool hasStrandedNotice = stranded.Count > 0 && strandedListContent != null && !strandedDismissed;
            if (showRecovery || hasStrandedNotice)
            {
                float estimate = 0f;
                if (showRecovery)
                    estimate += WbbSize.ControlSmall * 2f + WbbSize.SpaceSmall;
                if (hasStrandedNotice)
                {
                    estimate += WbbSize.ControlSmall * 3f + WbbSize.SpaceSmall;
                    if (strandedOpen)
                        estimate += Mathf.Min(stranded.Count, 4) * WbbSize.ControlSmall;
                }

                float maxHeight = paneWidthLayout < 600f ? 120f : 160f;
                noticesViewportHeightLayout = Mathf.Clamp(estimate, 80f, maxHeight);
            }
            else
            {
                noticesViewportHeightLayout = 0f;
            }
        }

        private static string RecoveryKey()
        {
            return WbbJournal.LastRecoveryIso + "|" + WbbJournal.LastRecoverySummary;
        }

        private void SyncNotices()
        {
            string summary = WbbJournal.LastRecoverySummary;
            showRecovery = !string.IsNullOrEmpty(summary) && !string.Equals(RecoveryKey(), dismissedRecoveryKey, System.StringComparison.Ordinal);
            recoveryText = showRecovery ? summary : "";
            strandedOpen = strandedOpenNext;
            if (strandedDirty)
            {
                strandedDirty = false;
                stranded.Clear();
                try
                {
                    var found = WbbJournal.FindStrandedOriginals();
                    if (found != null)
                        stranded.AddRange(found);
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("Could not check for prefabs left behind by an older version: " + e.Message);
                }

                strandedDismissed = stranded.Count > 0 && AllStrandedDismissed();
                strandedLangVersion = -1;
            }

            if (strandedDismissNext)
            {
                strandedDismissNext = false;
                SaveStrandedDismissed();
                strandedDismissed = stranded.Count > 0;
            }

            if (strandedLangVersion == WbbLang.Version)
                return;
            strandedLangVersion = WbbLang.Version;
            int count = stranded.Count;
            strandedItemContents = new GUIContent[count];
            if (count == 0)
            {
                strandedText = "";
                strandedListContent = null;
                return;
            }

            string none = L("Common.None");
            WbbStrandedOriginal first = stranded[0];
            strandedText = L("Safety.StrandedOriginals", count) + "\n" + L("Safety.StrandedOriginals.hint", first.currentPath, string.IsNullOrEmpty(first.originalPath) ? none : first.originalPath);
            strandedListContent = new GUIContent(L("History.StrandedList", count), L("History.StrandedList.tip"));
            for (int i = 0; i < count; i++)
            {
                WbbStrandedOriginal item = stranded[i];
                strandedItemContents[i] = new GUIContent(item.currentPath, L("History.StrandedItemTip", string.IsNullOrEmpty(item.modName) ? none : item.modName, string.IsNullOrEmpty(item.originalPath) ? none : item.originalPath));
            }
        }

        private static string StrandedDismissedPath
        {
            get
            {
                return WbbBuildLog.LibraryDirectory + "/StrandedDismissed.txt";
            }
        }

        private static string StrandedKey(WbbStrandedOriginal item)
        {
            return (item.recordId ?? "") + "|" + (item.currentPath ?? "");
        }

        private bool AllStrandedDismissed()
        {
            try
            {
                string path = StrandedDismissedPath;
                if (!System.IO.File.Exists(path))
                    return false;
                var dismissed = new System.Collections.Generic.HashSet<string>(System.IO.File.ReadAllLines(path), System.StringComparer.Ordinal);
                for (int i = 0; i < stranded.Count; i++)
                {
                    if (!dismissed.Contains(StrandedKey(stranded[i])))
                        return false;
                }

                return true;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Could not read the dismissed notice list: " + e.Message);
                return false;
            }
        }

        private void SaveStrandedDismissed()
        {
            var lines = new string[stranded.Count];
            for (int i = 0; i < stranded.Count; i++)
                lines[i] = StrandedKey(stranded[i]);
            try
            {
                System.IO.File.WriteAllLines(StrandedDismissedPath, lines);
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Could not save the dismissed notice list: " + e.Message);
            }
        }

        private void DrawNotices()
        {
            bool hasStrandedNotice = stranded.Count > 0 && strandedListContent != null && !strandedDismissed;
            if (!showRecovery && !hasStrandedNotice)
                return;
            float viewportHeight = noticesViewportHeightLayout > 0f ? noticesViewportHeightLayout : 120f;
            noticesScroll = EditorGUILayout.BeginScrollView(noticesScroll, GUILayout.Height(viewportHeight));
            if (showRecovery)
            {
                WbbStyles.Callout(recoveryText, WbbTone.Warning);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (WbbStyles.Button(LC("History.RecoveryDismiss"), WbbButtonKind.Ghost, GUILayout.Height(WbbSize.ControlSmall)))
                {
                    dismissedRecoveryKey = RecoveryKey();
                    if (host != null)
                        host.RequestRepaint();
                }

                GUILayout.Space(WbbSize.SpaceSmall);
                EditorGUILayout.EndHorizontal();
            }

            if (hasStrandedNotice)
            {
                WbbStyles.Callout(strandedText, WbbTone.Warning);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (WbbStyles.Button(LC("History.StrandedDismiss"), WbbButtonKind.Ghost, GUILayout.Height(WbbSize.ControlSmall)))
                {
                    strandedDismissNext = true;
                    if (host != null)
                        host.RequestRepaint();
                }

                GUILayout.Space(WbbSize.SpaceSmall);
                EditorGUILayout.EndHorizontal();
                bool open = WbbStyles.Foldout(strandedOpen, strandedListContent);
                if (open != strandedOpen)
                {
                    strandedOpenNext = open;
                    if (host != null)
                        host.RequestRepaint();
                }

                if (strandedOpen)
                {
                    int count = Mathf.Min(stranded.Count, strandedItemContents.Length);
                    for (int i = 0; i < count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(WbbSize.SpaceGroup);
                        Rect label = GUILayoutUtility.GetRect(0f, WbbSize.ControlSmall, GUILayout.ExpandWidth(true));
                        GUI.Label(label, strandedItemContents[i], midMini);
                        if (WbbStyles.MiniButton(LC("Common.Ping")))
                        {
                            pendingStrandedPing = stranded[i].currentPath;
                            if (host != null)
                                host.RequestRepaint();
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    GUILayout.Space(WbbSize.SpaceSmall);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStats()
        {
            WbbStyles.BeginCard();
            GUILayout.Label(statsContent, WbbStyles.CardTitle);
            GUILayout.Label(statsExtraContent, WbbStyles.MiniInfo);
            WbbStyles.EndCard();
        }

        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(typeLabelContent, midMini, GUILayout.Width(filterLabelWidth));
            for (int t = 0; t < TypeCount; t++)
            {
                if (t > 0 && typeFilterBreakLayout[t])
                {
                    WbbStyles.NextCell(true, 0f);
                    GUILayout.Space(filterLabelWidth);
                }

                bool next = WbbStyles.ToggleButton(typeEnabled[t], typeToggleContents[t], GUILayout.Height(WbbSize.ControlSmall), GUILayout.Width(filterChipWidth));
                if (next != typeEnabled[t])
                {
                    typeEnabled[t] = next;
                    EditorPrefs.SetBool(Pfx + "Type" + t, next);
                    filterDirty = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(statusLabelContent, midMini, GUILayout.Width(filterLabelWidth));
            for (int s = 0; s < StatusCount; s++)
            {
                if (s > 0 && statusFilterBreakLayout[s])
                {
                    WbbStyles.NextCell(true, 0f);
                    GUILayout.Space(filterLabelWidth);
                }

                bool next = WbbStyles.ToggleButton(statusEnabled[s], statusToggleContents[s], GUILayout.Height(WbbSize.ControlSmall), GUILayout.Width(filterChipWidth));
                if (next != statusEnabled[s])
                {
                    statusEnabled[s] = next;
                    EditorPrefs.SetBool(Pfx + "State" + s, next);
                    filterDirty = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            if (!groupOwnLineLayout)
            {
                GUILayout.Space(WbbSize.SpaceMedium);
                GUILayout.Label(groupLabelContent, midMini, GUILayout.Width(groupLabelWidth));
                DrawGroupPopup();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (groupOwnLineLayout)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(groupLabelContent, midMini, GUILayout.Width(filterLabelWidth));
                DrawGroupPopup();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawGroupPopup()
        {
            int nextGroup = WbbStyles.Popup(groupMode, groupNames, GUILayout.Width(WbbSize.History.GroupPopup));
            if (nextGroup != groupMode)
            {
                groupMode = Mathf.Clamp(nextGroup, 0, 2);
                EditorPrefs.SetInt(Pfx + "Group", groupMode);
                filterDirty = true;
                if (host != null)
                    host.RequestRepaint();
            }
        }

        private void DrawToolbar()
        {
            if (selectionDirty || selectionVersion != WbbLang.Version)
                RecomputeSelection();
            EnsureButtonContents();
            EditorGUILayout.BeginHorizontal();
            string newSearch = WbbStyles.SearchField(search, WbbSize.History.SearchMinWidth);
            if (newSearch != search)
            {
                search = newSearch;
                filterDirty = true;
                if (host != null)
                    host.RequestRepaint();
            }

            GUILayout.Label(searchHelpContent, midMini, GUILayout.Width(WbbSize.History.SearchHelpWidth), GUILayout.Height(WbbSize.History.SearchHelpHeight));
            if (toolbarActionsOwnLineLayout)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }

            if (WbbStyles.MiniButton(LC("History.ExportCsv"), GUILayout.Width(WbbSize.History.ExportCsvButton)))
            {
                pendingExportCsv = true;
                if (host != null)
                    host.RequestRepaint();
            }

            if (WbbStyles.MiniButton(LC("Common.Refresh"), GUILayout.Width(WbbSize.History.RefreshButton)))
            {
                pendingForceRefresh = true;
                if (host != null)
                    host.RequestRepaint();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (WbbStyles.MiniButton(LC("Common.SelectAll"), GUILayout.Width(WbbSize.History.SelectAllButton)))
                SetSelectionForVisible(true);
            if (WbbStyles.MiniButton(LC("Common.DeselectAll"), GUILayout.Width(WbbSize.History.DeselectAllButton)))
                SetSelectionForVisible(false);
            GUILayout.Space(WbbSize.SpaceSmall);
            selectionCueScratch.text = L("Ui.History.SelectionCue", selectionCount);
            selectionCueScratch.tooltip = selectionCueScratch.text;
            GUILayout.Label(selectionCueScratch, midMini, GUILayout.MinWidth(0f), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.Label(filteredText, midMini, GUILayout.MinWidth(0f));
            EditorGUILayout.EndHorizontal();
        }
    }
}
