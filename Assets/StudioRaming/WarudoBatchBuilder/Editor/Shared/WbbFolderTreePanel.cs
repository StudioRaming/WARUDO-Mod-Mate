using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public class WbbFolderTreePanel
    {
        private const float RowHeight = WbbSize.Folder.RowHeight;
        private const int MaxSearchResults = 200;
        private readonly HashSet<string> expanded = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        private readonly List<string> searchResults = new List<string>();
        private Vector2 scroll;
        private string search = "";
        private string searchApplied = "\0";
        private bool searchTruncated;
        private string pendingToggle;
        private string pendingSelect;
        private Dictionary<string, int> directCounts;
        private HashSet<string> foldersWithPrefabs;
        private double countsStamp = -1d;
        public bool OnlyPrefabFolders = true;
        public System.Func<string, bool> IsMarked;
        public System.Action<string, GenericMenu> BuildContextMenu;
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

        public WbbFolderTreePanel()
        {
            expanded.Add(WbbFolderPicker.AssetsRoot);
            OnlyPrefabFolders = WbbPrefs.GetBool(WbbUtil.PrefPrefix + "Tree.OnlyPrefabs", true);
        }

        public void InvalidateCounts()
        {
            countsStamp = -1d;
        }

        private void EnsureCounts()
        {
            bool layout = Event.current == null || Event.current.type == EventType.Layout;
            bool expired = countsStamp < 0d || (EditorApplication.timeSinceStartup - countsStamp > 8.0);
            if (directCounts != null && !(layout && expired))
                return;
            if (directCounts != null && !layout)
                return;
            countsStamp = EditorApplication.timeSinceStartup;
            directCounts = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            foldersWithPrefabs = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            string[] guids;
            try
            {
                guids = AssetDatabase.FindAssets("t:Prefab");
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Prefab scan for folder tree failed: " + e.Message);
                return;
            }

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path))
                    continue;
                int slash = path.LastIndexOf('/');
                if (slash <= 0)
                    continue;
                string folder = path.Substring(0, slash);
                int current;
                directCounts.TryGetValue(folder, out current);
                directCounts[folder] = current + 1;
                string walk = folder;
                while (!string.IsNullOrEmpty(walk))
                {
                    if (!foldersWithPrefabs.Add(walk))
                        break;
                    int cut = walk.LastIndexOf('/');
                    if (cut <= 0)
                        break;
                    walk = walk.Substring(0, cut);
                }
            }
        }

        private int DirectCount(string folder)
        {
            if (directCounts == null)
                return 0;
            int n;
            return directCounts.TryGetValue(folder, out n) ? n : 0;
        }

        private bool ShouldShow(string folder)
        {
            if (!OnlyPrefabFolders)
                return true;
            if (foldersWithPrefabs == null)
                return true;
            if (WbbFolderPicker.PathEquals(folder, WbbFolderPicker.AssetsRoot))
                return true;
            return foldersWithPrefabs.Contains(folder);
        }

        public void RevealPath(string folderPath)
        {
            if (!WbbUtil.IsValidProjectFolder(folderPath))
                return;
            string normalized = WbbUtil.NormalizePath(folderPath);
            string[] parts = normalized.Split('/');
            string accumulated = "";
            for (int i = 0; i < parts.Length; i++)
            {
                accumulated = i == 0 ? parts[i] : accumulated + "/" + parts[i];
                expanded.Add(accumulated);
            }
        }

        public string Draw(string currentPath, float width)
        {
            string picked = null;
            if (Event.current.type == EventType.Layout)
            {
                if (!string.IsNullOrEmpty(pendingToggle))
                {
                    if (expanded.Contains(pendingToggle))
                        expanded.Remove(pendingToggle);
                    else
                        expanded.Add(pendingToggle);
                    pendingToggle = null;
                }

                if (!string.IsNullOrEmpty(pendingSelect))
                {
                    picked = pendingSelect;
                    pendingSelect = null;
                }
            }

            EnsureCounts();
            EditorGUILayout.BeginVertical(GUILayout.Width(width));
            DrawHeader();
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            if (string.IsNullOrEmpty(search))
            {
                DrawNode(WbbFolderPicker.AssetsRoot, 0, currentPath);
            }
            else
            {
                EnsureSearch();
                if (searchResults.Count == 0)
                {
                    GUILayout.Label(L("Folder.SearchNoMatch"), WbbStyles.MiniInfo);
                }
                else
                {
                    for (int i = 0; i < searchResults.Count; i++)
                        DrawRow(searchResults[i], 0, false, true, currentPath);
                    if (searchTruncated)
                        GUILayout.Label(L("Folder.SearchTruncated", MaxSearchResults), WbbStyles.MiniInfo);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            return picked;
        }

        private readonly GUIContent titleContent = new GUIContent();
        private float titleFitWidth = -1f;
        private int titleFitVersion = -1;
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            Rect titleRect = GUILayoutUtility.GetRect(GUIContent.none, WbbStyles.SectionLabel, GUILayout.ExpandWidth(true), GUILayout.MinWidth(WbbSize.TextMinWidth));
            if (Event.current.type == EventType.Repaint && (titleFitVersion != WbbLang.Version || Mathf.Abs(titleRect.width - titleFitWidth) > 0.5f))
            {
                titleFitVersion = WbbLang.Version;
                titleFitWidth = titleRect.width;
                GUIContent source = LC("Tree.Title");
                titleContent.text = WbbStyles.Fit(source.text, WbbStyles.SectionLabel, titleRect.width);
                bool cut = titleContent.text != source.text;
                titleContent.tooltip = !cut ? source.tooltip : string.IsNullOrEmpty(source.tooltip) ? source.text : source.text + "\n" + source.tooltip;
            }

            GUI.Label(titleRect, titleContent, WbbStyles.SectionLabel);
            bool nowOnly = WbbStyles.ToggleButton(OnlyPrefabFolders, OnlyPrefabsContent(), GUILayout.Width(WbbSize.Folder.TreeFilterButton), GUILayout.Height(WbbSize.Folder.TreeFilterButtonHeight));
            if (nowOnly != OnlyPrefabFolders)
            {
                OnlyPrefabFolders = nowOnly;
                WbbPrefs.SetBool(WbbUtil.PrefPrefix + "Tree.OnlyPrefabs", OnlyPrefabFolders);
            }

            if (WbbStyles.MiniButton(LC("Tree.Collapse"), GUILayout.Width(WbbSize.Folder.TreeCollapseButton)))
            {
                expanded.Clear();
                expanded.Add(WbbFolderPicker.AssetsRoot);
            }

            EditorGUILayout.EndHorizontal();
            string next = WbbStyles.SearchField(search, WbbSize.Folder.TreeSearchMinWidth);
            if (next != search)
            {
                search = next;
                searchApplied = "\0";
            }
        }

        private GUIContent onlyPrefabsContent;
        private int onlyPrefabsVersion = -1;
        private GUIContent OnlyPrefabsContent()
        {
            if (onlyPrefabsContent != null && onlyPrefabsVersion == WbbLang.Version)
                return onlyPrefabsContent;
            onlyPrefabsVersion = WbbLang.Version;
            Texture icon = null;
            try
            {
                GUIContent found = EditorGUIUtility.IconContent("Prefab Icon");
                if (found != null)
                    icon = found.image;
            }
            catch (System.Exception)
            {
                icon = null;
            }

            onlyPrefabsContent = icon != null ? new GUIContent(icon, L("Tree.OnlyPrefabs.tip")) : new GUIContent(L("Tree.OnlyPrefabs"), L("Tree.OnlyPrefabs.tip"));
            return onlyPrefabsContent;
        }

        private void DrawNode(string path, int depth, string currentPath)
        {
            DrawRow(path, depth, true, false, currentPath);
            if (!expanded.Contains(path))
                return;
            string[] children = WbbFolderPicker.GetChildFolders(path);
            for (int i = 0; i < children.Length; i++)
            {
                if (!ShouldShow(children[i]))
                    continue;
                DrawNode(children[i], depth + 1, currentPath);
            }
        }

        private bool HasVisibleChildren(string path)
        {
            string[] children = WbbFolderPicker.GetChildFolders(path);
            for (int i = 0; i < children.Length; i++)
            {
                if (ShouldShow(children[i]))
                    return true;
            }

            return false;
        }

        private void DrawRow(string path, int depth, bool showFoldout, bool fullPath, string currentPath)
        {
            Event evt = Event.current;
            Rect rect = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
            bool selected = WbbFolderPicker.PathEquals(path, currentPath);
            bool hover = rect.Contains(evt.mousePosition);
            WbbStyles.RowBackground(rect, selected, hover, false);
            float indent = WbbSize.Folder.TreeIndentBase + depth * WbbSize.Folder.TreeIndentStep;
            Rect foldRect = new Rect(rect.x + indent, rect.y + 1f, WbbSize.Folder.TreeFoldWidth, rect.height - 2f);
            Rect labelRect = new Rect(rect.x + indent + WbbSize.Folder.TreeLabelLeft, rect.y, Mathf.Max(WbbSize.Folder.RowLabelMinWidth, rect.width - indent - WbbSize.Folder.TreeLabelReserve), rect.height);
            if (showFoldout && HasVisibleChildren(path))
            {
                bool isExpanded = expanded.Contains(path);
                bool nowExpanded = WbbStyles.FoldoutArrow(foldRect, isExpanded);
                if (nowExpanded != isExpanded)
                    pendingToggle = path;
            }

            if (IsMarked != null && IsMarked(path))
            {
                WbbStyles.MarkIcon(new Rect(labelRect.x, rect.y, WbbSize.Icon, rect.height));
                labelRect.x += WbbSize.IconAdvance;
                labelRect.width = Mathf.Max(WbbSize.Folder.RowLabelMinWidth, labelRect.width - WbbSize.IconAdvance);
            }

            string label = fullPath ? path : WbbUtil.FolderName(path);
            int count = DirectCount(path);
            if (count > 0)
                label += "  (" + count + ")";
            var content = new GUIContent(label, path);
            GUI.Label(labelRect, content, selected ? WbbStyles.RowLabelBold : WbbStyles.RowLabel);
            if (evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition) && !foldRect.Contains(evt.mousePosition))
            {
                pendingSelect = path;
                if (!expanded.Contains(path))
                    pendingToggle = path;
                evt.Use();
            }
            else if (evt.type == EventType.ContextClick && BuildContextMenu != null && rect.Contains(evt.mousePosition))
            {
                var menu = new GenericMenu();
                BuildContextMenu(path, menu);
                menu.ShowAsContext();
                evt.Use();
            }
        }

        private void EnsureSearch()
        {
            if (string.Equals(searchApplied, search, System.StringComparison.Ordinal))
                return;
            searchApplied = search;
            searchResults.Clear();
            searchTruncated = false;
            string needle = search.Trim().ToLowerInvariant();
            if (needle.Length == 0)
                return;
            CollectMatches(WbbFolderPicker.AssetsRoot, needle);
        }

        private void CollectMatches(string path, string needle)
        {
            if (searchResults.Count >= MaxSearchResults)
            {
                searchTruncated = true;
                return;
            }

            if (ShouldShow(path) && WbbUtil.FolderName(path).ToLowerInvariant().Contains(needle))
                searchResults.Add(path);
            string[] children = WbbFolderPicker.GetChildFolders(path);
            for (int i = 0; i < children.Length; i++)
            {
                if (searchResults.Count >= MaxSearchResults)
                {
                    searchTruncated = true;
                    return;
                }

                CollectMatches(children[i], needle);
            }
        }
    }
}
