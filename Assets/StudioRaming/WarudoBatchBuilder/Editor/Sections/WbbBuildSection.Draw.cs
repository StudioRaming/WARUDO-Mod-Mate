using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private const float MinTreeWidth = WbbSize.Build.TreeMinWidth;
        private Vector2 packageBodyScroll;
        private float measuredPackageBodyTop;
        private bool packageBodyLayout;
        private Rect heightSplitterRect;
        private bool draggingHeightSplitter;
        private int capturedHeightSplitterId;
        private float heightDragStartY;
        private float heightDragStartValue;
        private const float MaxTreeWidth = WbbSize.Build.TreeMaxWidth;
        private bool dbFailedLayout;
        private bool suggestLayout;
        private bool resultLayout;
        private bool issuesLayout;
        private int pendingIssuesOpen = -1;
        private bool pendingCloseResult;
        private bool pendingClearStatus;
        private string statusCloseTip = "";
        private int pendingDbAction;
        private string pendingRestoreSettings;
        private bool pendingCreateSettings;
        private bool pendingTreeToggle;
        private float listViewportHeightLayout = WbbSize.Build.CardHeight * 2f;
        public void OnGUI()
        {
            guiEventSerial++;
            EnsureLocalStyles();
            EnsureContents();
            if (Event.current.type == EventType.Layout)
            {
                packageBodyLayout = IsPackageMode;
                if (pendingTreeToggle)
                {
                    pendingTreeToggle = false;
                    if (narrowLayout)
                        treeOpenWhileNarrow = !treeOpenWhileNarrow;
                    else
                    {
                        treeVisible = !treeVisible;
                        WbbPrefs.SetBool(Pfx + "TreeVisible", treeVisible);
                    }
                }

                if (pendingIssuesOpen >= 0)
                {
                    issuesOpen = pendingIssuesOpen == 1;
                    pendingIssuesOpen = -1;
                }

                if (pendingCloseResult)
                {
                    pendingCloseResult = false;
                    resultVisible = false;
                }

                if (pendingClearStatus)
                {
                    pendingClearStatus = false;
                    SetStatus("", MessageType.Info);
                }

                SyncStructure();
                dbFailedLayout = WbbDatabase.LoadFailed;
                suggestLayout = suggestions.Count > 0 && !busyLayout;
                resultLayout = resultVisible && !busyLayout;
                if (resultLayout && resultLabelVersion != WbbLang.Version)
                    RefreshResultLabel();
                issuesLayout = issuesOpen && preflightIssues.Count > 0 && !busyLayout && !IsPackageMode;
            }

            if (dbFailedLayout)
                DrawDatabaseBanner();
            if (packageBodyLayout)
            {
                Rect top = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
                if (Event.current.type == EventType.Repaint && top.y > 0f && Mathf.Abs(top.y - measuredPackageBodyTop) > 1f)
                {
                    measuredPackageBodyTop = top.y;
                    RequestRepaint();
                }

                float height = host != null ? host.position.height - measuredPackageBodyTop - WbbSize.SpaceEdge * 2f : WbbSize.Window.MinHeight;
                packageBodyScroll = EditorGUILayout.BeginScrollView(packageBodyScroll, GUILayout.Height(Mathf.Max(80f, height)));
                if (packageSection != null)
                    packageSection.BodyTop = measuredPackageBodyTop;
            }

            EditorGUILayout.BeginHorizontal();
            if (treeShownLayout)
            {
                using (new EditorGUI.DisabledScope(busyLayout))
                {
                    DrawLeftPanel();
                }

                DrawSplitter();
            }

            EditorGUILayout.BeginVertical();
            {
                DrawSourceHeading();
                DrawToolbar();
                DrawBanners();
                DrawList();
                DrawHeightSplitter();
                DrawRebuildBorrowNote();
                using (new EditorGUI.DisabledScope(busyLayout || rebuildBorrow != null))
                {
                    DrawSettingsCard();
                }

                if (IsPackageMode)
                {
                    SyncPackageTargets();
                    if (packageSection != null)
                        packageSection.OnGUI();
                    DrawStatusArea();
                }
                else
                {
                    DrawStatusArea();
                    DrawBottomBar();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            if (packageBodyLayout)
                EditorGUILayout.EndScrollView();
            ProcessPending();
            ProcessBannerActions();
        }

        private void DrawSourceHeading()
        {
            Rect header = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(WbbSize.Section.HeaderHeight));
            WbbStyles.SectionSurface(header);
            GUILayout.Space(WbbSize.Section.HeaderInset);
            GUILayout.Label(LC("Build.SourceSection"), WbbStyles.SectionLabel, GUILayout.Height(WbbSize.LineHeight));
            GUILayout.FlexibleSpace();
            GUILayout.Space(WbbSize.Section.HeaderInset);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            float width = treeWidthLayout;
            EditorGUILayout.BeginVertical(GUILayout.Width(width));
            if (folderTree == null)
            {
                folderTree = new WbbFolderTreePanel();
                folderTree.IsMarked = WbbListSettings.IsWorkFolder;
                folderTree.BuildContextMenu = AddFolderMenuItems;
                folderTree.RevealPath(scopeFolder);
            }

            string picked = folderTree.Draw(scopeFolder, width);
            if (!string.IsNullOrEmpty(picked) && !WbbFolderPicker.PathEquals(picked, scopeFolder))
            {
                pendingScope = picked;
                RequestRepaint();
            }

            EditorGUILayout.EndVertical();
        }

        private void AddFolderMenuItems(string folder, GenericMenu menu)
        {
            string target = folder;
            if (WbbListSettings.IsWorkFolder(target))
                menu.AddItem(LC("Build.WorkFolderRemove"), false, delegate
                {
                    pendingWorkFolderRemove = target;
                    RequestRepaint();
                });
            else
                menu.AddItem(LC("Build.WorkFolderAdd"), false, delegate
                {
                    pendingWorkFolderAdd = target;
                    RequestRepaint();
                });
            menu.AddSeparator("");
            menu.AddItem(LC("Build.ScopeThis"), false, delegate
            {
                pendingScope = target;
                RequestRepaint();
            });
            menu.AddItem(LC("Build.ScopeAll"), false, delegate
            {
                pendingScope = "Assets";
                RequestRepaint();
            });
        }

        private void DrawSplitter()
        {
            const float splitter = WbbSize.Build.SplitterWidth;
            Rect rect = GUILayoutUtility.GetRect(splitter, splitter, GUILayout.Width(splitter), GUILayout.ExpandHeight(true));
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(rect.x + WbbSize.Build.SplitterLineLeft, rect.y, WbbSize.Line, rect.height), WbbTheme.P.BorderSubtle);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition))
            {
                draggingSplitter = true;
                evt.Use();
            }
            else if (draggingSplitter && evt.type == EventType.MouseDrag)
            {
                float maxByWindow = host != null ? host.position.width - WbbSize.Build.ListMinWidth - WbbSize.Build.SplitterWidth - WbbSize.SpaceEdge * 2f : MaxTreeWidth;
                treeWidth = Mathf.Clamp(treeWidth + evt.delta.x, MinTreeWidth, Mathf.Max(MinTreeWidth, Mathf.Min(MaxTreeWidth, maxByWindow)));
                RequestRepaint();
                evt.Use();
            }
            else if (evt.type == EventType.MouseUp && draggingSplitter)
            {
                draggingSplitter = false;
                WbbPrefs.SetFloat(Pfx + "TreeWidth", treeWidth);
                evt.Use();
            }
        }

        private void DrawHeightSplitter()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, WbbSize.Build.HeightSplitterSize, GUILayout.ExpandWidth(true));
            int id = GUIUtility.GetControlID("WARUDO Mod Mate height splitter".GetHashCode(), FocusType.Passive, rect);
            Event evt = Event.current;
            if (evt.type == EventType.Repaint)
            {
                heightSplitterRect = rect;
                Color color = rect.Contains(evt.mousePosition) || draggingHeightSplitter ? WbbTheme.P.BorderMark : WbbTheme.P.BorderSubtle;
                EditorGUI.DrawRect(new Rect(rect.x, rect.center.y, rect.width, WbbSize.Line), color);
                float gripWidth = WbbSize.IconButtonWide * 2f;
                EditorGUI.DrawRect(new Rect(rect.center.x - gripWidth * .5f, rect.center.y - WbbSize.Line * 2f, gripWidth, WbbSize.Line), color);
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);
            GUI.Label(rect, new GUIContent("", L("Ui.HeightSplitter.tip")), GUIStyle.none);
            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button != 0 || !rect.Contains(evt.mousePosition) || !GUI.enabled || GUIUtility.hotControl != 0)
                        break;
                    if (evt.clickCount == 2)
                    {
                        WbbPrefs.SetFloat(HeightSplitterPrefKey(), -1f);
                        RequestRepaint();
                        evt.Use();
                        break;
                    }

                    draggingHeightSplitter = true;
                    heightDragStartY = evt.mousePosition.y;
                    heightDragStartValue = listViewportHeightLayout;
                    GUIUtility.hotControl = id;
                    capturedHeightSplitterId = id;
                    evt.Use();
                    break;
                case EventType.MouseDrag:
                    if (!draggingHeightSplitter || GUIUtility.hotControl != capturedHeightSplitterId)
                        break;
                    float next = Mathf.Clamp(heightDragStartValue + evt.mousePosition.y - heightDragStartY, WbbSize.Build.RowHeight * 2f, MaximumManualListHeight());
                    WbbPrefs.SetFloat(HeightSplitterPrefKey(), next);
                    RequestRepaint();
                    evt.Use();
                    break;
                case EventType.MouseUp:
                    if (!draggingHeightSplitter || GUIUtility.hotControl != capturedHeightSplitterId)
                        break;
                    draggingHeightSplitter = false;
                    GUIUtility.hotControl = 0;
                    capturedHeightSplitterId = 0;
                    evt.Use();
                    break;
            }
        }

        private void DrawToolbar()
        {
            GUILayout.Space(WbbSize.SpaceSmall);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(WbbSize.SpaceSmall);
            bool treeNow = WbbStyles.ToggleIconButton(treeShownLayout, WbbStyles.IconTree, treeToggleContent.tooltip, GUILayout.Width(WbbSize.Build.TreeToggleWidth), GUILayout.Height(WbbSize.ControlSmall));
            if (treeNow != treeShownLayout)
            {
                pendingTreeToggle = true;
                RequestRepaint();
            }

            string newSearch = WbbStyles.SearchField(search, WbbSize.Build.SearchMinWidth);
            if (newSearch != search)
            {
                search = newSearch == null ? "" : newSearch;
                filterDirty = true;
                RequestRepaint();
            }

            int nextView = WbbStyles.Segmented(view, viewContents, GUILayout.Width(WbbSize.Build.ViewSegmentWidth));
            if (nextView != view)
            {
                pendingView = nextView;
                RequestRepaint();
            }

            if (WbbStyles.IconButton(WbbStyles.IconMenu, listMenuContent.tooltip, WbbButtonKind.Ghost, GUILayout.Width(WbbSize.IconButtonWide), GUILayout.Height(WbbSize.ControlSmall)))
                OpenListMenu();
            GUILayout.Space(WbbSize.SpaceHair);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(WbbSize.SpaceTight);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(WbbSize.SpaceSmall);
            if (!string.Equals(scopeFolder, "Assets", System.StringComparison.OrdinalIgnoreCase))
            {
                rowScratch.text = L("Build.ScopeChip", WbbUtil.FolderName(scopeFolder));
                rowScratch.tooltip = L("Build.ScopeChip.tip", scopeFolder);
                rowScratch.image = null;
                if (WbbStyles.ToggleButton(true, rowScratch, UI.SRGlyph.Close, Mathf.Max(WbbSize.Build.ScopeChipMinWidth, viewWidth * WbbSize.Build.ScopeChipMaxRatio), GUILayout.Height(WbbSize.ControlSmall)) == false)
                {
                    pendingScope = "Assets";
                    RequestRepaint();
                }
            }

            if (rawCount + userHiddenCount > 0 || showHidden)
            {
                rowScratch.text = hiddenChipText;
                rowScratch.tooltip = hiddenChipTip;
                rowScratch.image = null;
                bool nowShow = WbbStyles.ToggleButton(showHidden, rowScratch, GUILayout.Height(WbbSize.ControlSmall));
                if (nowShow != showHidden)
                {
                    pendingShowHidden = nowShow ? 1 : 0;
                    RequestRepaint();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label(countLine, rightMini);
            GUILayout.Space(WbbSize.SpaceSmall);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(WbbSize.SpaceTight);
        }

        private void OpenListMenu()
        {
            var menu = new GenericMenu();
            if (busyLayout)
            {
                menu.AddDisabledItem(LC("Build.BusyLock"));
                menu.ShowAsContext();
                return;
            }

            menu.AddItem(LC("Common.SelectAll"), false, delegate
            {
                SetSelectionForFiltered(true);
            });
            menu.AddItem(LC("Common.DeselectAll"), false, delegate
            {
                SetSelectionForFiltered(false);
            });
            menu.AddSeparator("");
            for (int s = SortName; s <= SortState; s++)
            {
                int captured = s;
                menu.AddItem(new GUIContent(L("Common.Sort") + "/" + sortNames[s]), sortMode == s, delegate
                {
                    sortMode = Mathf.Clamp(captured, SortName, SortState);
                    WbbPrefs.SetInt(Pfx + "Sort", sortMode);
                    filterDirty = true;
                    RequestRepaint();
                });
            }

            menu.AddItem(LC("Build.CollapseAll"), false, delegate
            {
                for (int i = 0; i < groups.Count; i++)
                    collapsed.Add(groups[i].key);
                SaveCollapsed();
                filterDirty = true;
                RequestRepaint();
            });
            menu.AddItem(LC("Build.ExpandAll"), false, delegate
            {
                collapsed.Clear();
                SaveCollapsed();
                filterDirty = true;
                RequestRepaint();
            });
            menu.AddSeparator("");
            if (selectedCount > 0)
                menu.AddItem(LC("Build.ExportPackage"), false, delegate
                {
                    pendingPackageExport = true;
                    RequestRepaint();
                });
            else
                menu.AddDisabledItem(LC("Build.ExportPackage"));
            if (items.Count > 0)
                menu.AddItem(LC("Build.GenerateThumbs"), false, delegate
                {
                    pendingThumbBatch = true;
                    RequestRepaint();
                });
            else
                menu.AddDisabledItem(LC("Build.GenerateThumbs"));
            menu.AddSeparator("");
            menu.AddItem(LC("Build.WorkFolderAddScope"), false, delegate
            {
                if (!string.Equals(scopeFolder, "Assets", System.StringComparison.OrdinalIgnoreCase))
                    pendingWorkFolderAdd = scopeFolder;
                else
                    SetStatus(L("Build.WorkFolderPickHint"), MessageType.Info);
                RequestRepaint();
            });
            menu.AddItem(LC("Build.Rescan"), false, delegate
            {
                pendingRescan = true;
                pendingForceIndex = true;
                RequestRepaint();
            });
            menu.ShowAsContext();
        }

        private void DrawBanners()
        {
            if (indexBusyLayout)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(WbbSize.SpaceSmall);
                EditorGUILayout.BeginVertical();
                WbbStyles.ProgressBar(indexProgressLayout, L("Build.IndexProgress"), WbbSize.Build.IndexProgressHeight);
                EditorGUILayout.EndVertical();
                GUILayout.Space(WbbSize.SpaceSmall);
                EditorGUILayout.EndHorizontal();
            }

            if (suggestLayout)
            {
                WbbStyles.BeginCard();
                GUILayout.Label(suggestionLine, WbbStyles.BodyText);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (WbbStyles.Button(LC("Build.SuggestAccept"), WbbButtonKind.Secondary, GUILayout.Height(WbbSize.Control), GUILayout.MinWidth(WbbSize.Build.SuggestAcceptMinWidth)))
                {
                    pendingAcceptSuggestions = true;
                    RequestRepaint();
                }

                if (WbbStyles.Button(LC("Build.SuggestLater"), WbbButtonKind.Ghost, GUILayout.Height(WbbSize.Control), GUILayout.MinWidth(WbbSize.Build.SuggestLaterMinWidth)))
                {
                    pendingDismissSuggestions = true;
                    RequestRepaint();
                }

                if (WbbStyles.Button(LC("Build.SuggestNever"), WbbButtonKind.Ghost, GUILayout.Height(WbbSize.Control), GUILayout.MinWidth(WbbSize.Build.SuggestLaterMinWidth)))
                {
                    pendingNeverSuggest = true;
                    RequestRepaint();
                }

                EditorGUILayout.EndHorizontal();
                WbbStyles.EndCard();
            }
        }

        private void DrawStatusArea()
        {
            if (busyLayout)
                WbbStyles.StatusLine(L("Build.BusyLock"), MessageType.Info);
            if (string.IsNullOrEmpty(statusShown))
                return;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            WbbStyles.StatusLine(statusShown, statusShownType);
            EditorGUILayout.EndVertical();
            if (WbbStyles.IconButton(UI.SRGlyph.Close, statusCloseTip, WbbButtonKind.Ghost, GUILayout.Width(WbbSize.IconButton), GUILayout.Height(WbbSize.ControlSmall)))
            {
                pendingClearStatus = true;
                RequestRepaint();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawDatabaseBanner()
        {
            WbbStyles.Callout(L("Safety.DbLoadFailedBanner"), WbbTone.Error);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(!WbbDatabase.CanRestoreBackup))
            {
                if (WbbStyles.Button(LC("Safety.DbRestoreBackup"), WbbButtonKind.Primary, GUILayout.Height(WbbSize.Control)))
                {
                    pendingDbAction = 1;
                    RequestRepaint();
                }
            }

            if (WbbStyles.Button(LC("Safety.DbStartFresh"), WbbButtonKind.Secondary, GUILayout.Height(WbbSize.Control)))
            {
                pendingDbAction = 2;
                RequestRepaint();
            }

            GUILayout.Space(WbbSize.SpaceSmall);
            EditorGUILayout.EndHorizontal();
        }

        private void ProcessBannerActions()
        {
            if (Event.current.type == EventType.Layout)
                return;
            if (!string.IsNullOrEmpty(pendingRestoreSettings))
            {
                string backup = pendingRestoreSettings;
                pendingRestoreSettings = null;
                string err;
                bool ok = WbbExportSettingsGuard.Restore(backup, out err);
                SetStatus(ok ? L("Safety.SettingsRestored") : L("Safety.SettingsRestoreFailed", err), ok ? MessageType.Info : MessageType.Error);
                preflightDirty = true;
                RequestRepaint();
            }

            if (pendingCreateSettings)
            {
                pendingCreateSettings = false;
                string createdPath;
                string createError;
                bool created = WbbExportSettingsCreator.TryCreate(out createdPath, out createError);
                SetStatus(created ? L("Safety.SettingsCreated", createdPath) : L("Safety.SettingsCreateFailed", createError), created ? MessageType.Info : MessageType.Error);
                preflightDirty = true;
                RequestRepaint();
            }

            if (pendingDbAction == 0)
                return;
            int action = pendingDbAction;
            pendingDbAction = 0;
            if (action == 1)
            {
                string error;
                if (!WbbDatabase.TryRestoreBackup(out error))
                    EditorUtility.DisplayDialog(L("Common.ProductName"), error, L("Common.Ok"));
            }
            else if (action == 2)
            {
                if (EditorUtility.DisplayDialog(L("Common.ProductName"), L("Safety.DbStartFresh.tip"), L("Safety.DbStartFresh"), L("Common.Cancel")))
                    WbbDatabase.AcknowledgeLoadFailure();
            }

            itemsDirty = true;
            preflightDirty = true;
            RequestRepaint();
        }
    }
}
