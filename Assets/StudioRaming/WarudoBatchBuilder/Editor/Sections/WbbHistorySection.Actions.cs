using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
        private bool maintenanceOpen;
        private int pendingMaintenance = -1;
        private bool footerNarrowLayout;
        private void EnsureButtonContents()
        {
            if (rebuildSelectedContent != null && buttonsSelected == selectionCount && buttonsFailed == failedInView && buttonsOld == oldCount && buttonsRows == rows.Count && buttonsLangVersion == WbbLang.Version)
                return;
            buttonsSelected = selectionCount;
            buttonsFailed = failedInView;
            buttonsOld = oldCount;
            buttonsRows = rows.Count;
            buttonsLangVersion = WbbLang.Version;
            rebuildSelectedContent = new GUIContent(L("History.RebuildSelected"), selectionCount == 0 ? L("History.RebuildSelectedNone.tip") : L("History.RebuildSelected.tip"));
            rebuildFailedContent = new GUIContent(L("History.RebuildFailedOnly"), failedInView == 0 ? L("History.RebuildFailedNone.tip") : L("History.RebuildFailedOnly.tip"));
            deleteSelectedContent = new GUIContent(L("Ui.History.DeleteRecords"), selectionCount == 0 ? L("History.DeleteSelectedNone.tip") : L("History.DeleteSelected.tip"));
            cleanOldContent = new GUIContent(L("History.CleanOld"), oldCount == 0 ? L("History.CleanOldNone.tip", CleanDays) : L("History.CleanOld.tip"));
            clearAllContent = new GUIContent(L("Ui.History.ClearRecords"), rows.Count == 0 ? L("History.ClearAllNone.tip") : L("History.ClearAll.tip"));
        }

        private void DrawBottomBar()
        {
            if (Event.current.type == EventType.Layout)
                footerNarrowLayout = host != null && host.position.width < WbbSize.History.WideMinWidth;
            if (Event.current.type == EventType.Layout && pendingMaintenance >= 0)
            {
                maintenanceOpen = pendingMaintenance == 1;
                pendingMaintenance = -1;
            }

            if (selectionDirty || selectionVersion != WbbLang.Version)
                RecomputeSelection();
            EnsureButtonContents();
            WbbStyles.Separator();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(selectionText, midMini, GUILayout.Width(WbbSize.History.SelectionLabel));
            GUILayout.FlexibleSpace();
            if (footerNarrowLayout)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }

            using (new EditorGUI.DisabledScope(selectionCount == 0))
            {
                if (WbbStyles.PlainButton(rebuildSelectedContent, GUILayout.Height(WbbSize.Control), GUILayout.Width(WbbSize.History.RebuildSelectedButton)))
                {
                    pendingRebuildSelected = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            using (new EditorGUI.DisabledScope(failedInView == 0))
            {
                if (WbbStyles.PlainButton(rebuildFailedContent, GUILayout.Height(WbbSize.Control), GUILayout.Width(WbbSize.History.RebuildFailedButton)))
                {
                    pendingRebuildFailed = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            using (new EditorGUI.DisabledScope(selectionCount == 0))
            {
                if (WbbStyles.Button(deleteSelectedContent, WbbStyles.IconTrash, WbbButtonKind.Danger, GUILayout.Height(WbbSize.Control)))
                {
                    pendingDeleteSelected = true;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            bool next = WbbStyles.Foldout(maintenanceOpen, LC("Ui.History.Manage"));
            if (next != maintenanceOpen)
            {
                pendingMaintenance = next ? 1 : 0;
                if (host != null)
                    host.RequestRepaint();
            }

            if (maintenanceOpen)
            {
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(oldCount == 0))
                {
                    if (WbbStyles.MiniButton(cleanOldContent, GUILayout.Height(WbbSize.ControlSmall), GUILayout.Width(WbbSize.History.CleanOldButton)))
                    {
                        pendingCleanOld = true;
                        if (host != null)
                            host.RequestRepaint();
                    }
                }

                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(rows.Count == 0))
                {
                    if (WbbStyles.Button(clearAllContent, WbbStyles.IconTrash, WbbButtonKind.Danger, GUILayout.Height(WbbSize.ControlSmall)))
                    {
                        pendingClearAll = true;
                        if (host != null)
                            host.RequestRepaint();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(WbbSize.SpaceHair);
        }

        private void DrawStatusLine()
        {
            if (string.IsNullOrEmpty(statusLineText))
                return;
            GUILayout.Space(WbbSize.SpaceSmall);
            EditorGUILayout.BeginHorizontal();
            WbbStyles.Callout(statusLineText, statusLineTone);
            if (WbbStyles.IconButton(UI.SRGlyph.Close, L("History.StatusClose.tip"), WbbButtonKind.Ghost, GUILayout.Width(WbbSize.IconButton), GUILayout.Height(WbbSize.ControlSmall)))
                SetStatus("", statusLineTone);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(WbbSize.SpaceHair);
        }

        private void OpenRowMenu(Row row)
        {
            var menu = new GenericMenu();
            Row target = row;
            WbbRecord record = row.record;
            menu.AddItem(LC("Common.Rebuild"), false, delegate
            {
                if (host != null)
                    host.RequestRebuild(record);
            });
            menu.AddSeparator("");
            if (target.fileState == 1)
            {
                menu.AddItem(LC("History.OpenOutput"), false, delegate
                {
                    WbbUtil.RevealFolder(record.outputFilePath);
                });
            }
            else
            {
                menu.AddItem(LC("History.OpenOutput"), false, delegate
                {
                    string shown = string.IsNullOrEmpty(record.outputFilePath) ? L("Common.None") : record.outputFilePath;
                    EditorUtility.DisplayDialog(L("Common.ProductName"), L("History.NoOutputFile", shown), L("Common.Ok"));
                });
            }

            menu.AddItem(LC("History.PingPrefab"), false, delegate
            {
                PingPrefab(record);
            });
            menu.AddItem(LC("History.CopyPath"), false, delegate
            {
                string path = string.IsNullOrEmpty(record.outputFilePath) ? record.prefabPath : record.outputFilePath;
                if (string.IsNullOrEmpty(path))
                    return;
                EditorGUIUtility.systemCopyBuffer = path;
                WbbUtil.Log(L("History.CopyDone", path));
            });
            menu.AddSeparator("");
            if (target.hasFixups)
            {
                menu.AddItem(LC("History.ShowFixups"), false, delegate
                {
                    EditorUtility.DisplayDialog(L("Safety.FixupHeader"), (record.fixupReport ?? "").Trim(), L("Common.Ok"));
                });
            }

            string logPath = record.buildLogPath;
            if (LogFileExists(logPath))
            {
                menu.AddItem(LC("Safety.ViewLog"), false, delegate
                {
                    OpenLogFile(logPath);
                });
                menu.AddItem(LC("Safety.ViewLogReveal"), false, delegate
                {
                    WbbUtil.RevealFolder(logPath);
                });
            }
            else
            {
                menu.AddDisabledItem(LC("Safety.ViewLog"));
                menu.AddDisabledItem(LC("Safety.ViewLogReveal"));
            }

            menu.AddSeparator("");
            menu.AddItem(LC("History.DeleteOne"), false, delegate
            {
                pendingDeleteId = record.id;
                if (host != null)
                    host.RequestRepaint();
            });
            menu.ShowAsContext();
        }

        private void PingPrefab(WbbRecord record)
        {
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
                string shown = string.IsNullOrEmpty(path) ? record.prefabName : path;
                if (string.IsNullOrEmpty(shown))
                    shown = L("Common.None");
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("History.NoPrefab", shown), L("Common.Ok"));
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static bool LogFileExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            try
            {
                return System.IO.File.Exists(path);
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static void OpenLogFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            string full = path;
            try
            {
                full = System.IO.Path.GetFullPath(path);
            }
            catch (System.Exception)
            {
                full = path;
            }

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                try
                {
                    using (System.Diagnostics.Process.Start("notepad.exe", "\"" + full + "\""))
                    {
                    }

                    return;
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("Could not open the build log in Notepad: " + e.Message);
                }
            }

            try
            {
                EditorUtility.OpenWithDefaultApp(full);
                return;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Could not open the build log with the default app: " + e.Message);
            }

            WbbUtil.RevealFolder(full);
        }

        private void PingStranded(string path)
        {
            UnityEngine.Object asset = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null)
            {
                strandedDirty = true;
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("History.NoPrefab", string.IsNullOrEmpty(path) ? L("Common.None") : path), L("Common.Ok"));
                if (host != null)
                    host.RequestRepaint();
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private void ProcessPending()
        {
            if (Event.current.type == EventType.Layout)
                return;
            if (pendingExportCsv)
            {
                pendingExportCsv = false;
                ExportCsv();
            }

            if (!string.IsNullOrEmpty(pendingStrandedPing))
            {
                string path = pendingStrandedPing;
                pendingStrandedPing = "";
                PingStranded(path);
            }

            if (!string.IsNullOrEmpty(pendingDeleteId))
            {
                string id = pendingDeleteId;
                pendingDeleteId = "";
                DeleteSingle(id);
            }

            if (pendingDeleteSelected)
            {
                pendingDeleteSelected = false;
                DeleteSelected();
            }

            if (pendingRebuildSelected)
            {
                pendingRebuildSelected = false;
                RebuildSelected();
            }

            if (pendingRebuildFailed)
            {
                pendingRebuildFailed = false;
                RebuildFailedOnly();
            }

            if (pendingCleanOld)
            {
                pendingCleanOld = false;
                CleanOldRecords();
            }

            if (pendingClearAll)
            {
                pendingClearAll = false;
                ClearAllRecords();
            }

            if (pendingGoToBuild)
            {
                pendingGoToBuild = false;
                if (host != null)
                    host.SwitchTab(WbbWindow.TabBuild);
            }
        }
    }
}
