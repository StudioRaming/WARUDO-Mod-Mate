using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private void OpenItemMenu(Item item)
        {
            if (item.isScene)
            {
                OpenSceneItemMenu(item);
                return;
            }

            var menu = new GenericMenu();
            Item target = item;
            if (busyLayout)
                menu.AddDisabledItem(LC("Build.MenuBuildThis"));
            else
                menu.AddItem(LC("Build.MenuBuildThis"), false, delegate
                {
                    selected.Clear();
                    selected.Add(target.guid);
                    lastClickedGuid = target.guid;
                    MarkSelectionChanged();
                    pendingBuild = true;
                    RequestRepaint();
                });
            menu.AddSeparator("");
            menu.AddItem(LC("Build.MenuRename"), false, delegate
            {
                pendingRenameGuid = target.guid;
                pendingRenameAsset = false;
                RequestRepaint();
            });
            menu.AddItem(LC("Build.MenuRenamePrefab"), false, delegate
            {
                pendingRenameGuid = target.guid;
                pendingRenameAsset = true;
                RequestRepaint();
            });
            if (!string.IsNullOrEmpty(target.manualName))
                menu.AddItem(LC("Build.MenuResetName"), false, delegate
                {
                    pendingResetNameGuid = target.guid;
                    RequestRepaint();
                });
            else
                menu.AddDisabledItem(LC("Build.MenuResetName"));
            menu.AddSeparator("");
            menu.AddItem(LC("Build.MenuRegenThumb"), false, delegate
            {
                pendingThumbGuid = target.guid;
                RequestRepaint();
            });
            if (target.thumb != null)
                menu.AddItem(LC("Build.MenuOpenThumb"), false, delegate
                {
                    pendingOpenThumbGuid = target.guid;
                    RequestRepaint();
                });
            else
                menu.AddDisabledItem(LC("Build.MenuOpenThumb"));
            menu.AddItem(LC("Build.MenuPing"), false, delegate
            {
                PingItem(target);
            });
            if (target.record != null && !string.IsNullOrEmpty(target.record.outputDirectory))
                menu.AddItem(LC("Build.MenuOpenOutput"), false, delegate
                {
                    pendingOpenOutputGuid = target.guid;
                    RequestRepaint();
                });
            else
                menu.AddDisabledItem(LC("Build.MenuOpenOutput"));
            menu.AddSeparator("");
            switch (target.reason)
            {
                case ShowReason.Hidden:
                    menu.AddItem(LC("Build.MenuUnhide"), false, delegate
                    {
                        pendingUnhideGuid = target.guid;
                        RequestRepaint();
                    });
                    break;
                case ShowReason.Raw:
                    menu.AddItem(LC("Build.MenuPin"), false, delegate
                    {
                        pendingPinGuid = target.guid;
                        RequestRepaint();
                    });
                    menu.AddItem(LC("Build.MenuHide"), false, delegate
                    {
                        pendingHideGuid = target.guid;
                        RequestRepaint();
                    });
                    break;
                case ShowReason.Pinned:
                    menu.AddItem(LC("Build.MenuUnpin"), false, delegate
                    {
                        pendingPinGuid = target.guid;
                        RequestRepaint();
                    });
                    menu.AddItem(LC("Build.MenuHide"), false, delegate
                    {
                        pendingHideGuid = target.guid;
                        RequestRepaint();
                    });
                    break;
                default:
                    menu.AddItem(LC("Build.MenuHide"), false, delegate
                    {
                        pendingHideGuid = target.guid;
                        RequestRepaint();
                    });
                    break;
            }

            string folder = target.folder;
            if (WbbListSettings.IsWorkFolder(folder))
                menu.AddItem(LC("Build.WorkFolderRemove"), false, delegate
                {
                    pendingWorkFolderRemove = folder;
                    RequestRepaint();
                });
            else
                menu.AddItem(LC("Build.WorkFolderAddItem"), false, delegate
                {
                    pendingWorkFolderAdd = folder;
                    RequestRepaint();
                });
            menu.AddItem(LC("Build.ScopeThis"), false, delegate
            {
                pendingScope = folder;
                RequestRepaint();
            });
            menu.ShowAsContext();
        }

        private void OpenSceneItemMenu(Item item)
        {
            var menu = new GenericMenu();
            Item target = item;
            WbbRecord record = item.record;
            if (busyLayout || record == null)
                menu.AddDisabledItem(new GUIContent(L("Build.SceneRebuild")));
            else
                menu.AddItem(new GUIContent(L("Build.SceneRebuild")), false, delegate
                {
                    pendingSceneRebuildGuid = target.guid;
                    RequestRepaint();
                });
            if (record != null)
                menu.AddItem(LC("Build.MenuPingScene"), false, delegate
                {
                    WbbQuickBuild.PingSource(record);
                });
            if (record != null && !string.IsNullOrEmpty(record.outputDirectory))
                menu.AddItem(LC("Build.MenuOpenOutput"), false, delegate
                {
                    pendingOpenOutputGuid = target.guid;
                    RequestRepaint();
                });
            else
                menu.AddDisabledItem(LC("Build.MenuOpenOutput"));
            menu.AddItem(LC("Build.OpenHistory"), false, delegate
            {
                if (host != null)
                    host.SwitchTab(WbbWindow.TabHistory);
            });
            menu.AddSeparator("");
            if (target.reason == ShowReason.Hidden)
                menu.AddItem(LC("Build.MenuUnhide"), false, delegate
                {
                    pendingUnhideGuid = target.guid;
                    RequestRepaint();
                });
            else
                menu.AddItem(LC("Build.MenuHide"), false, delegate
                {
                    pendingHideGuid = target.guid;
                    RequestRepaint();
                });
            menu.ShowAsContext();
        }

        private void PingItem(Item item)
        {
            if (item == null)
                return;
            if (item.isScene)
            {
                if (item.record != null)
                    WbbQuickBuild.PingSource(item.record);
                return;
            }

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(item.path);
            if (asset == null)
            {
                SetStatus(L("Build.PrefabLoadFail", item.name), MessageType.Warning);
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private void SetItemHidden(string guid, bool value)
        {
            int index = IndexOfGuid(guid);
            if (index < 0)
                return;
            Item item = items[index];
            if (item.isScene)
                WbbListSettings.SetSceneHidden(item.sceneKey, value);
            else
            {
                WbbListSettings.SetHidden(guid, value);
                if (value)
                    selected.Remove(guid);
                SyncHiddenToJob();
            }

            MarkSelectionChanged();
            RequestRepaint();
        }

        private const string RenameControlName = "WbbRenameField";
        private void DrawRenameField(Rect rect, Item item)
        {
            Event evt = Event.current;
            if (evt.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == RenameControlName)
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    CommitRename();
                    evt.Use();
                    return;
                }

                if (evt.keyCode == KeyCode.Escape)
                {
                    CancelRename();
                    evt.Use();
                    return;
                }
            }

            renameBuffer = WbbStyles.TextField(rect, renameBuffer, RenameControlName);
            if (renameFocusPending && evt.type == EventType.Repaint)
            {
                renameFocusPending = false;
                EditorGUI.FocusTextInControl(RenameControlName);
            }
        }

        private void BeginRename(string guid, bool assetName)
        {
            int index = IndexOfGuid(guid);
            if (index < 0 || items[index].isScene)
                return;
            Item item = items[index];
            renamingGuid = guid;
            renamingAssetName = assetName;
            renameBuffer = assetName ? item.name : (!string.IsNullOrEmpty(item.manualName) ? item.manualName : AutoBaseName(item));
            renameFocusPending = true;
            RequestRepaint();
        }

        private string AutoBaseName(Item item)
        {
            return WbbNaming.PrefabBaseName(item.name, item.folderName, item.folderPrefabs, job.useFolderNamePrefix, job.namePrefix);
        }

        private void CommitRename()
        {
            if (string.IsNullOrEmpty(renamingGuid))
                return;
            string guid = renamingGuid;
            string typed = renameBuffer == null ? "" : renameBuffer.Trim();
            bool assetName = renamingAssetName;
            renamingGuid = "";
            renamingAssetName = false;
            renameBuffer = "";
            GUI.FocusControl(null);
            if (assetName)
            {
                RenamePrefabAsset(guid, typed);
                RequestRepaint();
                return;
            }

            int index = IndexOfGuid(guid);
            if (index >= 0)
            {
                string auto = AutoBaseName(items[index]);
                string clean = WbbUtil.SanitizeModName(typed);
                WbbUtil.SetManualName(job, guid, clean.Length == 0 || string.Equals(clean, auto, System.StringComparison.Ordinal) ? "" : clean);
                SaveManualNames();
                namesDirty = true;
            }

            RequestRepaint();
        }

        private void RenamePrefabAsset(string guid, string newName)
        {
            int index = IndexOfGuid(guid);
            if (index < 0)
                return;
            Item item = items[index];
            string clean = WbbUtil.SanitizeModName(newName);
            if (clean.Length == 0 || string.Equals(clean, item.name, System.StringComparison.Ordinal))
                return;
            string oldName = item.name;
            Texture2D existing = WbbThumbnail.FindExisting(item.path, oldName);
            string thumbPath = existing != null ? AssetDatabase.GetAssetPath(existing) : "";
            if (!string.IsNullOrEmpty(thumbPath) && thumbPath.EndsWith(oldName + "_thumb.png", System.StringComparison.OrdinalIgnoreCase))
            {
                string thumbError = AssetDatabase.RenameAsset(thumbPath, clean + "_thumb");
                if (!string.IsNullOrEmpty(thumbError))
                    WbbUtil.LogWarning("Thumbnail rename failed (continuing): " + thumbError);
            }

            string error = AssetDatabase.RenameAsset(item.path, clean);
            if (!string.IsNullOrEmpty(error))
            {
                SetStatus(L("Build.RenamePrefabFailed", error), MessageType.Error);
                WbbUtil.LogError("Prefab rename failed: " + error);
                return;
            }

            AssetDatabase.SaveAssets();
            SetStatus(L("Build.RenamePrefabDone", oldName, clean), MessageType.Info);
            pendingRescan = true;
        }

        private void CancelRename()
        {
            renamingGuid = "";
            renameBuffer = "";
            GUI.FocusControl(null);
            RequestRepaint();
        }
    }
}
