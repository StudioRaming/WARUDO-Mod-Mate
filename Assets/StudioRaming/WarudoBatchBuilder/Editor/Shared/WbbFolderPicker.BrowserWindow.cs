using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public class WbbFolderBrowserWindow : EditorWindow
    {
        private static string L(string key)
        {
            return WbbLang.T(key);
        }

        private static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }

        private static GUIContent LC(string key)
        {
            return WbbLang.C(key);
        }

        private const float RowHeight = WbbSize.Folder.RowHeight;
        private const int MaxSearchResults = 400;
        private System.Action<string> onPicked;
        private string selectedPath = "";
        private string search = "";
        private string lastSearch;
        private Vector2 scroll;
        private readonly HashSet<string> expanded = new HashSet<string>();
        private readonly Dictionary<string, string[]> childCache = new Dictionary<string, string[]>();
        private readonly List<string> searchResults = new List<string>();
        private List<string> allFolders;
        private bool searchTruncated;
        private string pendingToggle;
        private bool confirmRequested;
        private bool closeRequested;
        private bool searchLabelOwnLine;
        private float searchLabelWidth;
        private int searchLabelVersion = -1;
        public void Setup(string startFolderPath, System.Action<string> picked)
        {
            onPicked = picked;
            expanded.Clear();
            expanded.Add(WbbFolderPicker.AssetsRoot);
            string start = WbbUtil.NormalizePath(startFolderPath);
            if (WbbUtil.IsValidProjectFolder(start))
            {
                selectedPath = start;
                ExpandTo(start);
            }
            else
            {
                selectedPath = WbbFolderPicker.AssetsRoot;
            }
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.MouseMove)
                Repaint();
            WbbStyles.WindowBackground(this);
            DrawToolbar();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (!string.IsNullOrEmpty(search))
                DrawSearchResults();
            else
                DrawNode(WbbFolderPicker.AssetsRoot, 0);
            EditorGUILayout.EndScrollView();
            DrawBottomBar();
            HandleKeys();
            if (!string.IsNullOrEmpty(pendingToggle))
            {
                if (expanded.Contains(pendingToggle))
                    expanded.Remove(pendingToggle);
                else
                    expanded.Add(pendingToggle);
                pendingToggle = null;
                Repaint();
            }

            if (confirmRequested)
            {
                confirmRequested = false;
                DoConfirm();
                return;
            }

            if (closeRequested)
            {
                closeRequested = false;
                Close();
            }
        }

        private void DrawToolbar()
        {
            GUIContent searchLabel = LC("Folder.SearchHint");
            if (Event.current.type == EventType.Layout)
                SyncSearchRow(searchLabel);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(searchLabel, WbbStyles.RowLabel, GUILayout.Width(searchLabelWidth));
                WbbStyles.NextCell(searchLabelOwnLine, 0f);
                string newSearch = WbbStyles.SearchField(search, WbbSize.Folder.BrowserSearchMinWidth);
                if (newSearch != search)
                    search = newSearch == null ? "" : newSearch;
                if (WbbStyles.MiniButton(LC("Common.Refresh"), GUILayout.Width(WbbSize.Folder.BrowserRefreshButton)))
                {
                    childCache.Clear();
                    allFolders = null;
                    lastSearch = null;
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                if (WbbStyles.MiniButton(LC("Folder.UseProjectSelection"), GUILayout.ExpandWidth(false)))
                    UseProjectSelection();
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void SyncSearchRow(GUIContent label)
        {
            if (searchLabelVersion != WbbLang.Version)
            {
                searchLabelVersion = WbbLang.Version;
                searchLabelWidth = WbbStyles.TextWidth(WbbStyles.RowLabel, label);
            }

            RectOffset text = WbbStyles.RowLabel.margin;
            RectOffset control = WbbStyles.ControlMargin;
            RectOffset none = GUIStyle.none.margin;
            float need = text.left + searchLabelWidth + WbbStyles.LayoutGap(text, control) + WbbSize.Folder.BrowserSearchMinWidth + WbbStyles.LayoutGap(control, none) + WbbSize.Field.SearchClearWidth + WbbStyles.LayoutGap(none, none) + WbbSize.Folder.BrowserRefreshButton;
            searchLabelOwnLine = need > position.width;
        }

        private void DrawBottomBar()
        {
            EditorGUILayout.BeginHorizontal(WbbStyles.CardFlat);
            {
                GUILayout.Label(string.IsNullOrEmpty(selectedPath) ? L("Folder.NoSelection") : selectedPath, WbbStyles.MiniInfo);
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(!WbbUtil.IsValidProjectFolder(selectedPath)))
                {
                    if (WbbStyles.PrimaryButton(LC("Folder.Select"), GUILayout.Width(WbbSize.Folder.BrowserSelectButton)))
                        confirmRequested = true;
                }

                if (WbbStyles.PlainButton(LC("Common.Cancel"), GUILayout.Width(WbbSize.Folder.BrowserCancelButton)))
                    closeRequested = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSearchResults()
        {
            EnsureSearch();
            if (searchResults.Count == 0)
            {
                GUILayout.Label(L("Folder.SearchNoMatch"), WbbStyles.MiniInfo);
                return;
            }

            GUILayout.Label(L("Folder.SearchResult", searchResults.Count), WbbStyles.MiniInfo);
            if (searchTruncated)
                GUILayout.Label(L("Folder.SearchTruncated", MaxSearchResults), WbbStyles.MiniInfo);
            for (int i = 0; i < searchResults.Count; i++)
                DrawRow(searchResults[i], 0, false, true);
        }

        private void DrawNode(string path, int depth)
        {
            DrawRow(path, depth, true, false);
            if (!expanded.Contains(path))
                return;
            string[] children = GetChildren(path);
            for (int i = 0; i < children.Length; i++)
                DrawNode(children[i], depth + 1);
        }

        private void DrawRow(string path, int depth, bool showFoldout, bool fullPath)
        {
            Event evt = Event.current;
            Rect rect = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
            bool selected = WbbFolderPicker.PathEquals(path, selectedPath);
            bool hover = rect.Contains(evt.mousePosition);
            WbbStyles.RowBackground(rect, selected, hover, false);
            float indent = WbbSize.Folder.BrowserIndentBase + depth * WbbSize.Folder.BrowserIndentStep;
            Rect foldRect = new Rect(rect.x + indent, rect.y + 1f, WbbSize.Folder.BrowserFoldWidth, rect.height - 2f);
            Rect labelRect = new Rect(rect.x + indent + WbbSize.Folder.BrowserLabelLeft, rect.y, Mathf.Max(WbbSize.Folder.RowLabelMinWidth, rect.width - indent - WbbSize.Folder.BrowserLabelReserve), rect.height);
            if (showFoldout)
            {
                bool isExpanded = expanded.Contains(path);
                bool nowExpanded = WbbStyles.FoldoutArrow(foldRect, isExpanded);
                if (nowExpanded != isExpanded)
                    pendingToggle = path;
            }

            var content = new GUIContent(fullPath ? path : WbbUtil.FolderName(path), path);
            GUI.Label(labelRect, content, selected ? WbbStyles.RowLabelBold : WbbStyles.RowLabel);
            if (evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition) && !foldRect.Contains(evt.mousePosition))
            {
                selectedPath = path;
                if (evt.clickCount >= 2)
                    confirmRequested = true;
                evt.Use();
                Repaint();
            }
        }

        private void HandleKeys()
        {
            Event evt = Event.current;
            if (evt == null || evt.type != EventType.KeyDown)
                return;
            if (evt.keyCode == KeyCode.Escape)
            {
                closeRequested = true;
                evt.Use();
            }
            else if ((evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) && WbbUtil.IsValidProjectFolder(selectedPath))
            {
                confirmRequested = true;
                evt.Use();
            }
        }

        private void DoConfirm()
        {
            string path = WbbUtil.NormalizePath(selectedPath);
            System.Action<string> callback = onPicked;
            onPicked = null;
            Close();
            if (callback != null && WbbUtil.IsValidProjectFolder(path))
                callback(path);
        }

        private void UseProjectSelection()
        {
            Object obj = Selection.activeObject;
            string path = obj == null ? "" : WbbUtil.NormalizePath(AssetDatabase.GetAssetPath(obj));
            if (WbbUtil.IsValidProjectFolder(path))
            {
                selectedPath = path;
                search = "";
                lastSearch = null;
                ExpandTo(path);
                Repaint();
            }
            else
            {
                WbbUtil.LogWarning(L("Folder.SelectionNotFolder"));
            }
        }

        private void ExpandTo(string path)
        {
            string normalized = WbbUtil.NormalizePath(path);
            if (string.IsNullOrEmpty(normalized))
                return;
            string[] parts = normalized.Split('/');
            string accumulated = "";
            for (int i = 0; i < parts.Length; i++)
            {
                accumulated = i == 0 ? parts[0] : accumulated + "/" + parts[i];
                expanded.Add(accumulated);
            }
        }

        private string[] GetChildren(string path)
        {
            string[] cached;
            if (childCache.TryGetValue(path, out cached))
                return cached;
            string[] subs;
            try
            {
                subs = AssetDatabase.GetSubFolders(path);
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("GetSubFolders failed: " + path + " (" + e.Message + ")");
                subs = null;
            }

            if (subs == null)
                subs = new string[0];
            for (int i = 0; i < subs.Length; i++)
                subs[i] = WbbUtil.NormalizePath(subs[i]);
            System.Array.Sort(subs, System.StringComparer.OrdinalIgnoreCase);
            childCache[path] = subs;
            return subs;
        }

        private void EnsureSearch()
        {
            if (search == lastSearch)
                return;
            lastSearch = search;
            searchResults.Clear();
            searchTruncated = false;
            string needle = search == null ? "" : search.Trim();
            if (needle.Length == 0)
                return;
            if (allFolders == null)
                allFolders = CollectAllFolders();
            for (int i = 0; i < allFolders.Count; i++)
            {
                string path = allFolders[i];
                string name = WbbUtil.FolderName(path);
                bool hit = name.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0;
                if (!hit)
                    continue;
                if (searchResults.Count >= MaxSearchResults)
                {
                    searchTruncated = true;
                    break;
                }

                searchResults.Add(path);
            }
        }

        private static List<string> CollectAllFolders()
        {
            var result = new List<string>();
            string dataPath = WbbUtil.NormalizePath(Application.dataPath);
            if (string.IsNullOrEmpty(dataPath))
                return result;
            var stack = new Stack<string>();
            stack.Push(dataPath);
            while (stack.Count > 0)
            {
                string dir = stack.Pop();
                string[] subs;
                try
                {
                    subs = Directory.GetDirectories(dir);
                }
                catch (System.Exception)
                {
                    continue;
                }

                for (int i = 0; i < subs.Length; i++)
                {
                    string abs = WbbUtil.NormalizePath(subs[i]);
                    string name = WbbUtil.FolderName(abs);
                    if (string.IsNullOrEmpty(name))
                        continue;
                    if (name[0] == '.')
                        continue;
                    if (name[name.Length - 1] == '~')
                        continue;
                    if (string.Equals(name, "CVS", System.StringComparison.OrdinalIgnoreCase))
                        continue;
                    result.Add(WbbFolderPicker.AssetsRoot + abs.Substring(dataPath.Length));
                    stack.Push(abs);
                }
            }

            result.Sort(System.StringComparer.OrdinalIgnoreCase);
            return result;
        }
    }
}
