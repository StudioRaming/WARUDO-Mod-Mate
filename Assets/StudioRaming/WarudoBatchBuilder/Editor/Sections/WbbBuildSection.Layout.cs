using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private void SyncStructure()
        {
            viewportHeight = Mathf.Max(80f, measuredViewport);
            busyLayout = IsBusy;
            statusShown = statusMessage;
            statusShownType = statusType;
            indexBusyLayout = WbbPrefabIndex.Busy;
            indexProgressLayout = WbbPrefabIndex.Progress;
            narrowLayout = host != null && (host.position.width < NarrowWidth || ListWidthWithTree(host.position.width) < WbbSize.Build.ListMinWidth);
            if (narrowLayout && !narrowLayoutPrev)
                treeOpenWhileNarrow = false;
            narrowLayoutPrev = narrowLayout;
            treeShownLayout = narrowLayout ? treeOpenWhileNarrow : treeVisible;
            treeWidthLayout = TreePanelWidth();
            paneWidthLayout = PaneWidth();
            viewWidth = Mathf.Max(200f, Mathf.Min(measuredWidth, paneWidthLayout));
            if (packageSection != null)
                packageSection.PaneWidth = paneWidthLayout;
            listViewportHeightLayout = ComputeListViewportHeight();
            SyncSettingsLayout();
            string fingerprint = DatabaseFingerprint();
            if (!string.Equals(fingerprint, dbFingerprint, System.StringComparison.Ordinal))
            {
                dbFingerprint = fingerprint;
                if (!busyLayout)
                    itemsDirty = true;
            }

            if (indexVersionSeen != WbbPrefabIndex.Version || listVersionSeen != WbbListSettings.Version)
                itemsDirty = true;
            ApplyPendingStructure();
            if (packageSection != null)
                packageSection.OutputCardVisible = settingsOpen;
            if (pendingRescan)
            {
                pendingRescan = false;
                WbbPrefabIndex.RequestRefresh(pendingForceIndex);
                pendingForceIndex = false;
                if (folderTree != null)
                    folderTree.InvalidateCounts();
                itemsDirty = true;
            }

            if (itemsDirty)
                RebuildItems();
            if (pendingSelectGuids.Count > 0)
                ApplyPendingSelection();
            if (namesDirty)
                RefreshNames();
            if (displayDirty || displayLangVersion != WbbLang.Version)
                RefreshDisplay();
            if (filterDirty)
                RebuildFiltered();
            if (summaryDirty || summaryLangVersion != WbbLang.Version)
                RefreshSummary();
            if (preflightDirty)
                RecomputePreflight();
            RefreshSuggestions();
            RecomputeLayout();
            RefreshEmptyState();
        }

        private float ComputeListViewportHeight()
        {
            float windowHeight = host != null ? host.position.height : WbbSize.Window.MinHeight;
            if (windowHeight <= 0f)
                windowHeight = WbbSize.Window.MinHeight;
            float manualHeight = WbbPrefs.GetFloat(HeightSplitterPrefKey(), -1f);
            if (manualHeight >= 0f)
                return Mathf.Clamp(manualHeight, WbbSize.Build.RowHeight * 2f, MaximumManualListHeight());
            float reserved = WbbSize.Header.Height + WbbSize.Tabs.Height + WbbSize.SpaceEdge * 2f + WbbSize.EmptyStateHeight + WbbSize.ControlSmall * 4f + WbbSize.Control * 4f + WbbSize.RunButton + WbbSize.Section.HeaderHeight;
            if (settingsOpen)
                reserved += WbbSize.Control * 2f;
            if (advancedOpen)
                reserved += WbbSize.ControlLarge * 4f;
            if (issuesOpen)
                reserved += WbbSize.Control * 3f;
            float available = windowHeight - reserved;
            float minimum = WbbSize.Build.RowHeight * 2f;
            if (IsPackageMode)
                return Mathf.Clamp(available * 0.2f, minimum, WbbSize.Build.RowHeight * 3f);
            float maximum = WbbSize.Build.CardHeight * 3f;
            return Mathf.Clamp(available, minimum, maximum);
        }

        private string HeightSplitterPrefKey()
        {
            return Pfx + "SourceListHeight." + (IsPackageMode ? "Package" : "Build");
        }

        private float MaximumManualListHeight()
        {
            float height = host != null ? host.position.height : WbbSize.Window.MinHeight;
            float lowerReserve = WbbSize.Header.Height + WbbSize.Tabs.Height + WbbSize.SpaceEdge * 2f + WbbSize.Section.HeaderHeight * 2f + WbbSize.Control * 8f + WbbSize.RunButton + WbbSize.Build.RowHeight * 2f;
            if (advancedOpen)
                lowerReserve += WbbSize.ControlLarge * 3f;
            return Mathf.Max(WbbSize.Build.RowHeight * 2f, height - lowerReserve);
        }

        private float ListWidthWithTree(float windowWidth)
        {
            return windowWidth - treeWidth - WbbSize.Build.SplitterWidth - WbbSize.SpaceEdge * 2f;
        }

        private float BodyWidth()
        {
            return host != null ? host.position.width - WbbSize.SpaceEdge * 2f : measuredWidth;
        }

        private float TreePanelWidth()
        {
            if (!narrowLayout)
                return treeWidth;
            return Mathf.Min(treeWidth, Mathf.Max(MinTreeWidth, BodyWidth() * WbbSize.Build.TreeNarrowRatio));
        }

        private float PaneWidth()
        {
            float width = BodyWidth();
            if (treeShownLayout)
                width -= treeWidthLayout + WbbSize.Build.SplitterWidth;
            return Mathf.Max(WbbSize.Build.MeasureMinWidth, width);
        }

        private void ApplyPendingStructure()
        {
            if (pendingSettingsOpen >= 0)
            {
                settingsOpen = pendingSettingsOpen == 1;
                pendingSettingsOpen = -1;
                WbbPrefs.SetBool(Pfx + "SettingsOpen", settingsOpen);
            }

            if (pendingAdvancedOpen >= 0)
            {
                advancedOpen = pendingAdvancedOpen == 1;
                pendingAdvancedOpen = -1;
                WbbPrefs.SetBool(Pfx + "AdvancedOpen", advancedOpen);
            }

            if (pendingView >= 0)
            {
                view = Mathf.Clamp(pendingView, ViewGrid, ViewList);
                pendingView = -1;
                WbbPrefs.SetInt(Pfx + "View", view);
                scroll = Vector2.zero;
                hoverIndex = -1;
            }

            if (pendingShowHidden >= 0)
            {
                showHidden = pendingShowHidden == 1;
                pendingShowHidden = -1;
                WbbPrefs.SetBool(Pfx + "ShowHidden", showHidden);
                itemsDirty = true;
            }

            if (!string.IsNullOrEmpty(pendingToggleGroup))
            {
                string key = pendingToggleGroup;
                pendingToggleGroup = null;
                if (!collapsed.Remove(key))
                    collapsed.Add(key);
                SaveCollapsed();
                filterDirty = true;
            }

            if (!string.IsNullOrEmpty(pendingWorkFolderAdd))
            {
                WbbListSettings.AddWorkFolder(pendingWorkFolderAdd);
                pendingWorkFolderAdd = null;
                itemsDirty = true;
            }

            if (!string.IsNullOrEmpty(pendingWorkFolderRemove))
            {
                WbbListSettings.RemoveWorkFolder(pendingWorkFolderRemove);
                pendingWorkFolderRemove = null;
                itemsDirty = true;
            }

            if (pendingAcceptSuggestions)
            {
                pendingAcceptSuggestions = false;
                for (int i = 0; i < suggestions.Count; i++)
                    WbbListSettings.AddWorkFolder(suggestions[i]);
                WbbListSettings.SuggestionAnswered = true;
                suggestions.Clear();
                itemsDirty = true;
            }

            if (pendingDismissSuggestions)
            {
                pendingDismissSuggestions = false;
                SessionState.SetBool(SuggestSkippedKey, true);
                suggestions.Clear();
            }

            if (pendingNeverSuggest)
            {
                pendingNeverSuggest = false;
                WbbListSettings.SuggestionAnswered = true;
                suggestions.Clear();
            }

            if (busyLayout)
            {
                pendingScope = null;
                pendingBuildFolder = null;
                return;
            }

            if (!string.IsNullOrEmpty(pendingScope))
            {
                string folder = WbbUtil.NormalizePath(pendingScope);
                pendingScope = null;
                if (WbbUtil.IsValidProjectFolder(folder) && !string.Equals(folder, scopeFolder, System.StringComparison.OrdinalIgnoreCase))
                {
                    scopeFolder = folder;
                    job.prefabFolderPath = folder;
                    job.prefabFolderGuid = WbbUtil.FolderGuid(folder);
                    WbbPrefs.SetString(Pfx + "PrefabPath", job.prefabFolderPath);
                    WbbPrefs.SetString(Pfx + "PrefabGuid", job.prefabFolderGuid);
                    if (folderTree != null)
                        folderTree.RevealPath(folder);
                    scroll = Vector2.zero;
                    itemsDirty = true;
                }
            }

            if (!string.IsNullOrEmpty(pendingBuildFolder))
            {
                string folder = WbbUtil.NormalizePath(pendingBuildFolder);
                pendingBuildFolder = null;
                if (WbbUtil.IsValidProjectFolder(folder))
                {
                    job.buildFolderPath = folder;
                    job.buildFolderGuid = WbbUtil.FolderGuid(folder);
                    WbbUtil.SetBuildFolderForType(job, CurrentType, folder);
                    SaveScriptFolders(job);
                    preflightDirty = true;
                }
            }
        }

        private void RefreshSuggestions()
        {
            bool hidden = WbbListSettings.SuggestionAnswered || SessionState.GetBool(SuggestSkippedKey, false);
            if (hidden || WbbPrefabIndex.Busy)
            {
                if (hidden)
                    suggestions.Clear();
                return;
            }

            if (suggestionsIndexVersion == WbbPrefabIndex.Version && suggestionsLangVersion == WbbLang.Version)
                return;
            suggestionsIndexVersion = WbbPrefabIndex.Version;
            suggestionsLangVersion = WbbLang.Version;
            suggestions.Clear();
            suggestions.AddRange(WbbPrefabIndex.SuggestWorkFolders());
            if (suggestions.Count == 0)
            {
                suggestionLine = "";
                return;
            }

            var names = new System.Text.StringBuilder();
            for (int i = 0; i < suggestions.Count && i < 4; i++)
            {
                if (i > 0)
                    names.Append(", ");
                names.Append(WbbUtil.FolderName(suggestions[i]));
            }

            if (suggestions.Count > 4)
                names.Append(" …");
            suggestionLine = L("Build.SuggestWork", names.ToString());
        }

        private int suggestionsLangVersion = -1;
        private void ApplyPendingSelection()
        {
            bool replace = pendingSelectReplace;
            pendingSelectReplace = false;
            if (replace)
            {
                selected.Clear();
                lastClickedGuid = "";
            }

            int applied = 0;
            string firstName = "";
            string missingName = "";
            int missing = 0;
            for (int i = 0; i < pendingSelectGuids.Count; i++)
            {
                string guid = pendingSelectGuids[i];
                string shown = i < pendingSelectNames.Count ? pendingSelectNames[i] : guid;
                int index = IndexOfGuid(guid);
                if (index < 0)
                {
                    missing++;
                    if (string.IsNullOrEmpty(missingName))
                        missingName = string.IsNullOrEmpty(shown) ? guid : shown;
                    continue;
                }

                Item item = items[index];
                if (selected.Add(item.guid))
                    applied++;
                lastClickedGuid = item.guid;
                string overrideName;
                if (pendingRebuildModNames.TryGetValue(guid, out overrideName) && !string.IsNullOrEmpty(overrideName))
                {
                    item.modNameOverride = overrideName;
                    rebuildModNames[guid] = overrideName;
                }

                if (string.IsNullOrEmpty(firstName))
                    firstName = string.IsNullOrEmpty(overrideName) ? item.name : overrideName;
                if (collapsed.Count > 0 && !string.IsNullOrEmpty(item.folder))
                {
                    bool opened = collapsed.Remove(item.folder);
                    int slash = item.folder.LastIndexOf('/');
                    if (slash > 0)
                        opened |= collapsed.Remove(item.folder.Substring(0, slash));
                    if (opened)
                        SaveCollapsed();
                }
            }

            pendingSelectGuids.Clear();
            pendingSelectNames.Clear();
            pendingRebuildModNames.Clear();
            rebuildQueuedCount += applied;
            namesDirty = true;
            filterDirty = true;
            summaryDirty = true;
            preflightDirty = true;
            if (rebuildQueuedCount <= 0)
            {
                SetStatus(L("Build.RebuildNotFound", string.IsNullOrEmpty(missingName) ? L("Common.None") : missingName), MessageType.Warning);
                return;
            }

            if (rebuildQueuedCount == 1 && missing == 0)
            {
                string one = L("Build.RebuildQueued", string.IsNullOrEmpty(firstName) ? L("Common.None") : firstName);
                if (rebuildUnhiddenCount > 0)
                    one += " " + L("Build.RebuildUnhid", rebuildUnhiddenCount);
                SetStatus(one, MessageType.Info);
                return;
            }

            string message = L("Build.RebuildQueuedMany", rebuildQueuedCount);
            if (missing > 0)
                message += " " + L("Build.RebuildNotFound", missingName);
            if (rebuildUnhiddenCount > 0)
                message += " " + L("Build.RebuildUnhid", rebuildUnhiddenCount);
            SetStatus(message, missing > 0 ? MessageType.Warning : MessageType.Info);
        }

        private void RecomputeLayout()
        {
            columns = view == ViewGrid ? Mathf.Clamp((int)(Mathf.Max(WbbSize.Build.GridMinWidth, viewWidth - WbbSize.Build.GridWidthReserve) / MinCellWidth), 1, MaxColumns) : 1;
            units.Clear();
            float y = 0f;
            for (int gi = 0; gi < groups.Count; gi++)
            {
                Group g = groups[gi];
                units.Add(new Unit { kind = UnitHeader, group = gi, start = 0, count = 0, y = y, height = HeaderRowHeight });
                y += HeaderRowHeight;
                if (!g.open)
                    continue;
                if (view == ViewGrid)
                {
                    for (int s = 0; s < g.members.Count; s += columns)
                    {
                        int count = Mathf.Min(columns, g.members.Count - s);
                        units.Add(new Unit { kind = UnitCards, group = gi, start = s, count = count, y = y, height = CardHeight });
                        y += CardHeight;
                    }
                }
                else
                {
                    for (int s = 0; s < g.members.Count; s++)
                    {
                        units.Add(new Unit { kind = UnitRow, group = gi, start = s, count = 1, y = y, height = RowHeight });
                        y += RowHeight;
                    }
                }

                y += WbbSize.Build.GroupGap;
            }

            totalHeight = Mathf.Max(0f, y);
            firstUnit = 0;
            lastUnit = -1;
            if (units.Count == 0)
                return;
            float top = scroll.y - CullPad;
            float bottom = scroll.y + viewportHeight + CullPad;
            firstUnit = units.Count;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].y + units[i].height >= top)
                {
                    firstUnit = i;
                    break;
                }
            }

            lastUnit = firstUnit - 1;
            for (int i = firstUnit; i < units.Count; i++)
            {
                if (units[i].y > bottom)
                    break;
                lastUnit = i;
            }
        }
    }
}
