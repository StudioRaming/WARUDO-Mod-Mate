using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbLibrarySection
    {
        private readonly GUIContent selectionCueScratch = new GUIContent();
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
            if (typeToggleContents == null)
                BuildTypeChips();
            if (sortNames != null && contentVersion == WbbLang.Version)
                return;
            contentVersion = WbbLang.Version;
            sortNames = new[]
            {
                L("Common.SortDate"),
                L("Common.SortName"),
                L("Common.SortSize")
            };
            sortLabelContent = new GUIContent(L("Library.SortMode"), L("Library.SortMode.tip"));
            sortLabelWidth = WbbStyles.TextWidth(midMini, sortLabelContent);
            menuButtonContent = new GUIContent(string.Empty, L("Library.RowMenu.tip"));
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

            searchHelpContent = helpIcon != null ? new GUIContent(helpIcon, L("Library.SearchHint")) : new GUIContent(L("Common.Search"), L("Library.SearchHint"));
        }

        private void DrawPathsCard()
        {
            WbbStyles.BeginCard();
            bool next = WbbStyles.Foldout(pathsOpen, LC("Library.Paths"));
            if (next != pathsOpen)
            {
                pendingPathsOpen = next ? 1 : 0;
                if (host != null)
                    host.RequestRepaint();
            }

            if (pathsOpen)
            {
                for (int t = 0; t < TypeCount; t++)
                    DrawPathRow(t);
            }
            else
            {
                for (int t = 0; t < TypeCount; t++)
                    DrawPathProblem(t);
            }

            WbbStyles.EndCard();
        }

        private void DrawPathProblem(int t)
        {
            if (pathStates[t] == PathState.Ok)
                return;
            string typeName = WbbLang.TypeName((WbbExportType)t);
            switch (pathStates[t])
            {
                case PathState.Empty:
                    WbbStyles.StatusLine(L("Library.PathEmpty", typeName), MessageType.Warning);
                    break;
                case PathState.Missing:
                    WbbStyles.StatusLine(L("Library.FolderMissing", typeName, paths[t]), MessageType.Warning);
                    break;
                default:
                    WbbStyles.StatusLine(L("Library.ScanError", typeName, pathErrors[t]), MessageType.Error);
                    break;
            }

            DrawSetPathButton(t);
        }

        private readonly GUIContent pathRowScratch = new GUIContent();
        private void DrawPathRow(int t)
        {
            var type = (WbbExportType)t;
            string typeName = WbbLang.TypeName(type);
            EditorGUILayout.BeginHorizontal();
            pathRowScratch.text = typeName;
            pathRowScratch.tooltip = L("Library.Paths.tip");
            GUILayout.Label(pathRowScratch, midMini, GUILayout.Width(WbbSize.Library.TypeNameLabel));
            string current = paths[t] == null ? "" : paths[t];
            string edited = WbbStyles.DelayedTextField(current);
            if (edited != current)
            {
                paths[t] = edited;
                WbbPrefs.SetString(OutputDirKey(type), edited);
                pathEditedAt = EditorApplication.timeSinceStartup;
            }

            if (WbbStyles.MiniButton(LC("Library.Browse"), GUILayout.Width(WbbSize.IconButtonWide)))
                pendingBrowseType = t;
            using (new EditorGUI.DisabledScope(pathStates[t] != PathState.Ok))
            {
                if (WbbStyles.MiniButton(LC("Common.OpenFolder"), GUILayout.Width(WbbSize.Library.OpenFolderButton)))
                    WbbUtil.RevealFolder(WbbUtil.NormalizePath(paths[t]));
            }

            EditorGUILayout.EndHorizontal();
            switch (pathStates[t])
            {
                case PathState.Empty:
                    WbbStyles.StatusLine(L("Library.PathEmpty", typeName), MessageType.Warning);
                    DrawSetPathButton(t);
                    break;
                case PathState.Missing:
                    WbbStyles.StatusLine(L("Library.FolderMissing", typeName, paths[t]), MessageType.Warning);
                    DrawSetPathButton(t);
                    break;
                case PathState.Error:
                    WbbStyles.StatusLine(L("Library.ScanError", typeName, pathErrors[t]), MessageType.Error);
                    DrawSetPathButton(t);
                    break;
            }
        }

        private void DrawSetPathButton(int t)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (WbbStyles.MiniButton(LC("Library.SetPath"), GUILayout.Width(WbbSize.Library.SetPathButton)))
                pendingBrowseType = t;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTypeFilter()
        {
            EditorGUILayout.BeginHorizontal();
            for (int t = 0; t < TypeCount; t++)
            {
                if (t > 0)
                    WbbStyles.NextCell(chipBreak[t], 0f);
                bool next = WbbStyles.ToggleButton(typeEnabled[t], typeToggleContents[t], GUILayout.Height(WbbSize.ControlSmall), GUILayout.Width(chipWidths[t]));
                if (next != typeEnabled[t])
                {
                    typeEnabled[t] = next;
                    WbbPrefs.SetBool(Pfx + "Type" + t, next);
                    filterDirty = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSummary()
        {
            GUILayout.Space(WbbSize.SpaceHair);
            GUILayout.Label(summaryLine, WbbStyles.MiniInfo);
            if (rows.Count > 0)
                GUILayout.Label(L("Ui.Library.StatusLegend", L("Library.BadgeTracked"), L("Library.BadgeUntracked")), WbbStyles.MiniInfo);
        }

        private void DrawToolbar()
        {
            if (selectionDirty || selectionVersion != WbbLang.Version || rebuildButtonContent == null)
                RecomputeSelection();
            EditorGUILayout.BeginHorizontal();
            string newSearch = WbbStyles.SearchField(search, WbbSize.Library.SearchMinWidth);
            if (newSearch != search)
            {
                search = newSearch;
                filterDirty = true;
                if (host != null)
                    host.RequestRepaint();
            }

            GUILayout.Label(searchHelpContent, midMini, GUILayout.Width(WbbSize.Library.SearchHelpWidth), GUILayout.Height(WbbSize.Library.SearchHelpHeight));
            GUILayout.Label(sortLabelContent, midMini, GUILayout.Width(sortLabelWidth));
            int newSort = WbbStyles.Popup(sortMode, sortNames, GUILayout.Width(WbbSize.Library.SortPopup));
            if (newSort != sortMode)
            {
                sortMode = Mathf.Clamp(newSort, 0, 2);
                WbbPrefs.SetInt(Pfx + "Sort", sortMode);
                filterDirty = true;
                if (host != null)
                    host.RequestRepaint();
            }

            if (WbbStyles.MiniButton(LC("Common.Refresh"), GUILayout.Width(WbbSize.Library.RefreshButton)))
            {
                pendingForceRescan = true;
                if (host != null)
                    host.RequestRepaint();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (WbbStyles.MiniButton(LC("Common.SelectAll"), GUILayout.Width(WbbSize.Library.SelectAllButton)))
                SetSelectionForFiltered(true);
            if (WbbStyles.MiniButton(LC("Common.DeselectAll"), GUILayout.Width(WbbSize.Library.DeselectAllButton)))
                SetSelectionForFiltered(false);
            GUILayout.Space(WbbSize.SpaceSmall);
            selectionCueScratch.text = L("Ui.Library.SelectionCue", selectionCount, selectionTrackedCount, Mathf.Max(0, selectionCount - selectionTrackedCount));
            selectionCueScratch.tooltip = selectionCueScratch.text;
            GUILayout.Label(selectionCueScratch, midMini, GUILayout.MinWidth(0f), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.Label(filteredText, midMini, GUILayout.MinWidth(0f));
            EditorGUILayout.EndHorizontal();
        }
    }
}
