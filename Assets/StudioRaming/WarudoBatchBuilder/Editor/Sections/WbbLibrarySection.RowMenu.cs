using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbLibrarySection
    {
        private void OpenRowMenu(Row row)
        {
            var menu = new GenericMenu();
            Row target = row;
            menu.AddItem(LC("Library.MenuReveal"), false, delegate
            {
                WbbUtil.RevealFolder(target.file.filePath);
            });
            menu.AddItem(LC("Library.MenuCopyPath"), false, delegate
            {
                EditorGUIUtility.systemCopyBuffer = target.file.filePath;
                WbbUtil.Log(L("Library.CopiedPath", target.file.filePath));
            });
            menu.AddSeparator("");
            if (target.record != null)
            {
                menu.AddItem(LC("Library.MenuPingPrefab"), false, delegate
                {
                    PingPrefab(target);
                });
                menu.AddItem(LC("Common.Rebuild"), false, delegate
                {
                    if (host != null)
                        host.RequestRebuild(target.record);
                });
            }
            else
            {
                menu.AddDisabledItem(LC("Library.MenuPingPrefabMissing"));
                menu.AddDisabledItem(LC("Library.MenuRebuildMissing"));
            }

            menu.AddSeparator("");
            var deleteLabel = new GUIContent(L("Library.MenuDeleteOne", target.file.fileName + target.file.extension));
            menu.AddItem(deleteLabel, false, delegate
            {
                pendingDeletePath = target.file.filePath;
                if (host != null)
                    host.RequestRepaint();
            });
            menu.ShowAsContext();
        }

        private void PingPrefab(Row row)
        {
            WbbRecord record = row == null ? null : row.record;
            if (record == null)
                return;
            if (record.IsSceneSource)
            {
                WbbQuickBuild.PingSource(record);
                return;
            }

            string path = "";
            if (!string.IsNullOrEmpty(record.prefabGuid))
                path = AssetDatabase.GUIDToAssetPath(record.prefabGuid);
            if (string.IsNullOrEmpty(path))
                path = record.prefabPath;
            UnityEngine.Object asset = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null)
            {
                string shown = string.IsNullOrEmpty(record.prefabName) ? path : record.prefabName;
                if (string.IsNullOrEmpty(shown))
                    shown = L("Common.None");
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("Library.PrefabMissing", shown), L("Common.Ok"));
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
