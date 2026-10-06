using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbFolderPicker
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

        public const int MaxRecent = 12;
        internal const string AssetsRoot = "Assets";
        private static readonly string[] EmptyPaths = new string[0];
        private static readonly Dictionary<string, string> pendingPicks = new Dictionary<string, string>();
        private static readonly Dictionary<string, string[]> subFolderCache = new Dictionary<string, string[]>();
        private static double subFolderCacheStamp;
        private static readonly GUIContent dropHintContent = new GUIContent();
        public static string DrawFolderPicker(GUIContent label, string currentFolderPath, string recentKey, bool showBrowser = true)
        {
            string current = WbbUtil.NormalizePath(currentFolderPath);
            string key = string.IsNullOrEmpty(recentKey) ? "Recent.Default" : recentKey;
            string result = null;
            if (Event.current != null && Event.current.type == EventType.Layout)
            {
                string pending;
                if (pendingPicks.TryGetValue(key, out pending))
                {
                    pendingPicks.Remove(key);
                    if (!string.IsNullOrEmpty(pending) && !PathEquals(pending, current))
                        result = pending;
                }
            }

            float available = Mathf.Max(WbbSize.Folder.PickerWrapMinWidth, EditorGUIUtility.currentViewWidth - WbbSize.Folder.PickerWrapReserve);
            EditorGUILayout.BeginVertical(WbbStyles.CardFlat);
            GUILayout.Label(label, WbbStyles.SectionLabel);
            EditorGUILayout.BeginHorizontal();
            {
                string typed = WbbStyles.DelayedTextField(current);
                if (!PathEquals(typed, current))
                {
                    string normalized = WbbUtil.NormalizePath(typed);
                    if (WbbUtil.IsValidProjectFolder(normalized))
                        result = normalized;
                    else if (!string.IsNullOrEmpty(normalized))
                        WbbUtil.LogWarning("Folder not found: " + normalized);
                }

                if (showBrowser && WbbStyles.MiniButton(LC("Folder.Browse"), GUILayout.Width(WbbSize.Folder.PickerBrowseButton)))
                    OpenBrowserFor(key, current);
                using (new EditorGUI.DisabledScope(!WbbUtil.IsValidProjectFolder(current)))
                {
                    if (WbbStyles.MiniButton(LC("Folder.Ping"), GUILayout.Width(WbbSize.Folder.PickerPingButton)))
                        PingFolder(current);
                    if (WbbStyles.MiniButton(LC("Common.OpenFolder"), GUILayout.ExpandWidth(false)))
                        WbbUtil.RevealFolder(WbbUtil.ToAbsolutePath(current));
                }
            }

            EditorGUILayout.EndHorizontal();
            if (string.IsNullOrEmpty(current))
                GUILayout.Label(L("Folder.NotSet"), WbbStyles.MiniInfo);
            else if (!WbbUtil.IsValidProjectFolder(current))
                WbbStyles.StatusLine(L("Folder.InvalidPath", current), MessageType.Warning);
            Rect dropRect = GUILayoutUtility.GetRect(0f, WbbSize.ControlLarge, GUILayout.ExpandWidth(true));
            string dropped = DrawDropArea(dropRect, L("Folder.DropHint"));
            if (!string.IsNullOrEmpty(dropped) && !PathEquals(dropped, current))
                result = dropped;
            List<string> recent = GetRecent(key);
            EditorGUILayout.BeginHorizontal();
            {
                GUIContent recentLabel = LC("Folder.Recent");
                GUILayout.Label(recentLabel, WbbStyles.MiniInfo, GUILayout.Width(WbbStyles.TextWidth(WbbStyles.MiniInfo, recentLabel)));
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(recent.Count == 0))
                {
                    if (WbbStyles.MiniButton(LC("Folder.RecentClear"), GUILayout.ExpandWidth(false)))
                        ClearRecent(key);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (recent.Count == 0)
            {
                GUILayout.Label(L("Folder.RecentEmpty"), WbbStyles.MiniInfo);
            }
            else
            {
                string fromRecent = DrawFolderButtons(recent, current, available);
                if (!string.IsNullOrEmpty(fromRecent) && !PathEquals(fromRecent, current))
                    result = fromRecent;
            }

            EditorGUILayout.EndVertical();
            if (!string.IsNullOrEmpty(result))
            {
                result = WbbUtil.NormalizePath(result);
                AddRecent(key, result);
            }

            return result;
        }

        public static void OpenBrowser(string startFolderPath, System.Action<string> onPicked)
        {
            var window = ScriptableObject.CreateInstance<WbbFolderBrowserWindow>();
            window.titleContent = new GUIContent(L("Folder.BrowserTitle"));
            window.Setup(startFolderPath, onPicked);
            window.minSize = new Vector2(WbbSize.Folder.BrowserMinWidth, WbbSize.Folder.BrowserMinHeight);
            Vector2 screen = new Vector2(WbbSize.Folder.BrowserFallbackScreenPos, WbbSize.Folder.BrowserFallbackScreenPos);
            if (Event.current != null)
            {
                try
                {
                    screen = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                }
                catch (System.Exception)
                {
                    screen = new Vector2(WbbSize.Folder.BrowserFallbackScreenPos, WbbSize.Folder.BrowserFallbackScreenPos);
                }
            }

            window.position = new Rect(Mathf.Max(0f, screen.x - WbbSize.Folder.BrowserMouseOffsetX), Mathf.Max(0f, screen.y - WbbSize.Folder.BrowserMouseOffsetY), WbbSize.Folder.BrowserWidth, WbbSize.Folder.BrowserHeight);
            window.ShowAuxWindow();
        }

        public static string DrawSubFolderGrid(string rootFolderPath, string currentFolderPath, ref Vector2 scroll, float maxHeight = WbbSize.Folder.PickerGridMaxHeight)
        {
            string root = WbbUtil.NormalizePath(rootFolderPath);
            string current = WbbUtil.NormalizePath(currentFolderPath);
            if (!WbbUtil.IsValidProjectFolder(current) && !WbbUtil.IsValidProjectFolder(root))
            {
                GUILayout.Label(L("Folder.RootMissing"), WbbStyles.MiniInfo);
                return null;
            }

            if (!WbbUtil.IsValidProjectFolder(root))
                root = AssetsRoot;
            if (!WbbUtil.IsValidProjectFolder(current))
                current = root;
            if (!IsUnder(current, root))
                root = AssetsRoot;
            string parent = PathEquals(current, root) ? "" : ParentOf(current);
            if (!string.IsNullOrEmpty(parent) && !IsUnder(parent, root))
                parent = "";
            float available = Mathf.Max(WbbSize.Folder.PickerWrapMinWidth, EditorGUIUtility.currentViewWidth - WbbSize.Folder.PickerGridWrapReserve);
            string picked = null;
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(maxHeight), GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginHorizontal();
            {
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(parent)))
                {
                    if (WbbStyles.MiniButton(LC("Folder.Up"), GUILayout.Width(WbbSize.Folder.PickerUpButton)))
                        picked = parent;
                }

                GUILayout.Label(L("Folder.CurrentIs", WbbUtil.FolderName(current)), WbbStyles.MiniInfo);
                GUILayout.FlexibleSpace();
                if (WbbStyles.MiniButton(LC("Folder.Ping"), GUILayout.Width(WbbSize.Folder.PickerPingButton)))
                    PingFolder(current);
            }

            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(parent))
            {
                string[] siblings = SubFolders(parent);
                GUILayout.Label(L("Folder.Siblings", siblings.Length), WbbStyles.MiniInfo);
                string fromSibling = DrawFolderButtons(siblings, current, available);
                if (!string.IsNullOrEmpty(fromSibling))
                    picked = fromSibling;
            }

            string[] children = SubFolders(current);
            GUILayout.Label(L("Folder.Children", children.Length), WbbStyles.MiniInfo);
            if (children.Length == 0)
            {
                GUILayout.Label(L("Folder.NoChildren"), WbbStyles.MiniInfo);
            }
            else
            {
                string fromChild = DrawFolderButtons(children, "", available);
                if (!string.IsNullOrEmpty(fromChild))
                    picked = fromChild;
            }

            EditorGUILayout.EndScrollView();
            if (!string.IsNullOrEmpty(picked))
            {
                picked = WbbUtil.NormalizePath(picked);
                if (PathEquals(picked, current) || !WbbUtil.IsValidProjectFolder(picked))
                    picked = null;
            }

            return picked;
        }

        public static string DrawDropArea(Rect rect, string hintText)
        {
            Event evt = Event.current;
            if (evt == null)
                return null;
            bool over = rect.Contains(evt.mousePosition);
            dropHintContent.text = hintText == null ? "" : hintText;
            WbbStyles.DropArea(rect, dropHintContent, over && DragAndDrop.visualMode == DragAndDropVisualMode.Copy);
            if (!over || evt.type == EventType.Repaint)
                return null;
            string result = null;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    string folder = FirstFolderFromDrag();
                    if (!string.IsNullOrEmpty(folder))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            result = folder;
                        }

                        evt.Use();
                    }

                    break;
            }

            return result;
        }
    }
}
