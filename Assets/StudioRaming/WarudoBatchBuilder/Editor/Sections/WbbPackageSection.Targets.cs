using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private bool AddTargetPath(string rawPath)
        {
            string path = WbbUtil.NormalizePath(rawPath);
            if (string.IsNullOrEmpty(path))
                return false;
            if (!path.StartsWith(AssetsPrefix, System.StringComparison.Ordinal))
                return false;
            if (!path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                return false;
            for (int i = 0; i < targets.Count; i++)
            {
                if (string.Equals(targets[i].prefabPath, path, System.StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            targets.Add(MakeTarget(path));
            return true;
        }

        private static Target MakeTarget(string path)
        {
            var target = new Target();
            target.prefabPath = path;
            int slash = path.LastIndexOf('/');
            string file = slash >= 0 ? path.Substring(slash + 1) : path;
            int dot = file.LastIndexOf('.');
            target.prefabName = dot > 0 ? file.Substring(0, dot) : file;
            string folder = slash > 0 ? path.Substring(0, slash) : "";
            target.folderName = WbbUtil.FolderName(folder);
            string core = string.IsNullOrEmpty(target.folderName) ? target.prefabName : target.folderName + "_" + target.prefabName;
            target.baseName = WbbUtil.SanitizeModName(core);
            if (string.IsNullOrEmpty(target.baseName))
                target.baseName = "Package";
            target.fileName = target.baseName + PackageExtension;
            return target;
        }

        private void ValidateTargets()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                Target target = targets[i];
                try
                {
                    string absolute = WbbUtil.ToAbsolutePath(target.prefabPath);
                    target.missing = string.IsNullOrEmpty(absolute) || !File.Exists(absolute);
                }
                catch (System.Exception)
                {
                    target.missing = true;
                }
            }
        }

        private void DrawTargetsCard()
        {
            WbbStyles.BeginCard();
            WbbStyles.SectionTitle(L("Package.TargetsTitle"));
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(busy))
            {
                if (WbbStyles.MiniButton(LC("Package.AddSelection"), GUILayout.Width(WbbSize.Package.AddSelectionButton)))
                {
                    pendingAddSelection = true;
                    if (host != null)
                        host.RequestRepaint();
                }

                if (WbbStyles.MiniButton(LC("Package.ClearTargets"), GUILayout.Width(WbbSize.Package.ClearTargetsButton)))
                {
                    pendingClearTargets = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label(targetSummaryText, midMini);
            EditorGUILayout.EndHorizontal();
            DrawDropArea();
            if (targets.Count == 0)
            {
                GUILayout.Label(L("Package.TargetsEmpty"), WbbStyles.MiniInfo);
            }
            else
            {
                bool scrolled = targets.Count > MaxTargetRows;
                if (scrolled)
                {
                    targetScroll = EditorGUILayout.BeginScrollView(targetScroll, GUILayout.Height(MaxTargetRows * TargetRowHeight + WbbSize.Package.TargetScrollPad));
                }

                for (int i = 0; i < targets.Count; i++)
                {
                    Rect rect = GUILayoutUtility.GetRect(0f, TargetRowHeight, GUILayout.ExpandWidth(true));
                    DrawTargetRow(i, rect);
                }

                if (scrolled)
                    EditorGUILayout.EndScrollView();
            }

            WbbStyles.EndCard();
        }

        private void DrawTargetRow(int index, Rect rect)
        {
            Target target = targets[index];
            WbbStyles.RowBackground(rect, false, false, (index & 1) == 1);
            float right = rect.xMax - WbbSize.Package.RowRight;
            var removeRect = new Rect(right - WbbSize.Package.RemoveButton, rect.y + 1f, WbbSize.Package.RemoveButton, rect.height - WbbSize.RowButtonInset);
            right -= WbbSize.Package.RemoveReserve;
            var pingRect = new Rect(right - WbbSize.Package.PingButton, rect.y + 1f, WbbSize.Package.PingButton, rect.height - WbbSize.RowButtonInset);
            right -= WbbSize.Package.PingReserve;
            float available = Mathf.Max(WbbSize.Package.TargetTextMinWidth, right - rect.x - WbbSize.Package.TargetTextReserve);
            float nameWidth = Mathf.Max(WbbSize.Package.TargetNameMinWidth, available * WbbSize.Package.TargetNameRatio);
            var nameRect = new Rect(rect.x + WbbSize.Package.TargetNameLeft, rect.y, nameWidth, rect.height);
            var fileRect = new Rect(nameRect.xMax + WbbSize.SpaceSmall, rect.y, Mathf.Max(WbbSize.TextMinWidth, right - nameRect.xMax - WbbSize.Package.TargetFileReserve), rect.height);
            rowScratch.text = target.prefabName;
            rowScratch.tooltip = Event.current.type == EventType.Repaint ? L("Package.TargetTip", target.prefabName, target.prefabPath, target.fileName) : string.Empty;
            GUI.Label(nameRect, rowScratch, target.missing ? mutedLabel : WbbStyles.RowLabel);
            GUI.Label(fileRect, target.missing ? L("Package.TargetMissing") : target.fileName, rightMini);
            if (WbbStyles.MiniButton(pingRect, pingContent))
                pendingPingPath = target.prefabPath;
            if (WbbStyles.IconButton(removeRect, UI.SRGlyph.Close, removeContent.tooltip, WbbButtonKind.Secondary))
            {
                pendingRemoveIndex = index;
                if (host != null)
                    host.RequestRepaint();
            }
        }

        private void DrawDropArea()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, WbbSize.Package.DropAreaHeight, GUILayout.ExpandWidth(true));
            rect.x += WbbSize.Package.DropAreaLeft;
            rect.width = Mathf.Max(WbbSize.Package.DropAreaMinWidth, rect.width - WbbSize.Package.DropAreaTrim);
            bool active = !busy && rect.Contains(Event.current.mousePosition) && DragAndDrop.visualMode == DragAndDropVisualMode.Copy;
            WbbStyles.DropArea(rect, dropContent, active);
            HandleDrag(rect);
        }

        private void HandleDrag(Rect rect)
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return;
            if (busy)
                return;
            if (!rect.Contains(evt.mousePosition))
                return;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type != EventType.DragPerform)
                return;
            DragAndDrop.AcceptDrag();
            string[] paths = DragAndDrop.paths;
            if (paths != null)
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    string path = WbbUtil.NormalizePath(paths[i]);
                    if (!string.IsNullOrEmpty(path))
                        pendingDropPaths.Add(path);
                }
            }

            evt.Use();
            if (host != null)
                host.RequestRepaint();
        }
    }
}
