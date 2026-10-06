using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbFolderPicker
    {
        private static void OpenBrowserFor(string key, string startFolder)
        {
            EditorWindow host = EditorWindow.focusedWindow;
            if (host == null)
                host = EditorWindow.mouseOverWindow;
            OpenBrowser(startFolder, delegate (string picked)
            {
                if (string.IsNullOrEmpty(picked))
                    return;
                pendingPicks[key] = picked;
                if (host != null)
                    host.Repaint();
            });
        }

        internal static void PingFolder(string folderPath)
        {
            string path = WbbUtil.NormalizePath(folderPath);
            if (!WbbUtil.IsValidProjectFolder(path))
                return;
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (obj != null)
                EditorGUIUtility.PingObject(obj);
        }

        private static string FirstFolderFromDrag()
        {
            Object[] refs = DragAndDrop.objectReferences;
            if (refs == null || refs.Length == 0)
                return null;
            for (int i = 0; i < refs.Length; i++)
            {
                string path = WbbUtil.NormalizePath(AssetDatabase.GetAssetPath(refs[i]));
                if (WbbUtil.IsValidProjectFolder(path))
                    return path;
            }

            for (int i = 0; i < refs.Length; i++)
            {
                string path = WbbUtil.NormalizePath(AssetDatabase.GetAssetPath(refs[i]));
                if (string.IsNullOrEmpty(path))
                    continue;
                string parent = ParentOf(path);
                if (WbbUtil.IsValidProjectFolder(parent))
                    return parent;
            }

            return null;
        }

        private static string DrawFolderButtons(IList<string> folders, string highlightPath, float available)
        {
            if (folders == null || folders.Count == 0)
                return null;
            string picked = null;
            float x = 0f;
            bool rowOpen = false;
            for (int i = 0; i < folders.Count; i++)
            {
                string path = folders[i];
                if (string.IsNullOrEmpty(path))
                    continue;
                var content = new GUIContent(WbbUtil.FolderName(path), path);
                float width = Mathf.Clamp(EditorStyles.miniButton.CalcSize(content).x + WbbSize.Folder.PickerButtonTextPad, WbbSize.Folder.PickerButtonMinWidth, WbbSize.Folder.PickerButtonMaxWidth);
                if (!rowOpen || (x > 0f && x + width > available))
                {
                    if (rowOpen)
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    rowOpen = true;
                    x = 0f;
                }

                bool isCurrent = PathEquals(path, highlightPath);
                if (WbbStyles.Button(content, isCurrent ? WbbButtonKind.Primary : WbbButtonKind.Secondary, GUILayout.Width(width), GUILayout.Height(WbbSize.ControlMini)))
                    picked = path;
                x += width + WbbSize.Folder.PickerButtonGap;
            }

            if (rowOpen)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            return picked;
        }

        internal static string[] GetChildFolders(string folderPath)
        {
            return SubFolders(folderPath);
        }

        private static string[] SubFolders(string folderPath)
        {
            string path = WbbUtil.NormalizePath(folderPath);
            if (!WbbUtil.IsValidProjectFolder(path))
                return EmptyPaths;
            if (Event.current == null || Event.current.type == EventType.Layout)
            {
                if (EditorApplication.timeSinceStartup - subFolderCacheStamp > 4.0)
                {
                    subFolderCache.Clear();
                    subFolderCacheStamp = EditorApplication.timeSinceStartup;
                }
            }

            string[] cached;
            if (subFolderCache.TryGetValue(path, out cached))
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
                subs = EmptyPaths;
            for (int i = 0; i < subs.Length; i++)
                subs[i] = WbbUtil.NormalizePath(subs[i]);
            System.Array.Sort(subs, System.StringComparer.OrdinalIgnoreCase);
            subFolderCache[path] = subs;
            return subs;
        }

        internal static string ParentOf(string path)
        {
            string p = WbbUtil.NormalizePath(path);
            int idx = p.LastIndexOf('/');
            if (idx <= 0)
                return "";
            return p.Substring(0, idx);
        }

        internal static bool IsUnder(string path, string root)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(root))
                return false;
            if (PathEquals(path, root))
                return true;
            return path.StartsWith(root + "/", System.StringComparison.OrdinalIgnoreCase);
        }

        internal static bool PathEquals(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
                return true;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;
            return string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
