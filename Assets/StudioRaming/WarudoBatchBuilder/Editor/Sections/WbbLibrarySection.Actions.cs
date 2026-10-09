using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbLibrarySection
    {
        private void ProcessPending()
        {
            if (pathEditedAt > 0d)
            {
                if (EditorApplication.timeSinceStartup - pathEditedAt >= PathEditDelaySec)
                {
                    pathEditedAt = -1d;
                    pendingForceRescan = true;
                }

                if (host != null)
                    host.RequestRepaint();
            }

            if (Event.current.type == EventType.Layout)
                return;
            if (pendingPickedType >= 0)
            {
                int t = pendingPickedType;
                pendingPickedType = -1;
                ApplyPickedPath(t, pendingPickedPath);
                pendingPickedPath = "";
            }

            if (pendingBrowseType >= 0)
            {
                int t = pendingBrowseType;
                pendingBrowseType = -1;
                BrowseFolder(t);
            }

            if (!string.IsNullOrEmpty(pendingDeletePath))
            {
                string one = pendingDeletePath;
                pendingDeletePath = "";
                DeleteSingleFile(one);
            }

            if (pendingDelete)
            {
                pendingDelete = false;
                DeleteSelectedFiles();
            }

            if (pendingRebuild)
            {
                pendingRebuild = false;
                RebuildSelectedFiles();
            }

            if (pendingGoToBuild)
            {
                pendingGoToBuild = false;
                if (host != null)
                    host.SwitchTab(WbbWindow.TabBuild);
            }
        }

        private void BrowseFolder(int t)
        {
            if (t < 0 || t >= TypeCount)
                return;
            var type = (WbbExportType)t;
            string start = paths[t];
            if (string.IsNullOrEmpty(start))
                start = WbbUtil.DefaultOutputPath(type);
            string picked = EditorUtility.OpenFolderPanel(L("Library.BrowseTitle", WbbLang.TypeName(type)), start, "");
            if (string.IsNullOrEmpty(picked))
                return;
            GUI.FocusControl(null);
            pendingPickedType = t;
            pendingPickedPath = WbbUtil.NormalizePath(picked);
            if (host != null)
                host.RequestRepaint();
        }

        private void ApplyPickedPath(int t, string path)
        {
            if (t < 0 || t >= TypeCount || string.IsNullOrEmpty(path))
                return;
            paths[t] = path;
            WbbPrefs.SetString(OutputDirKey((WbbExportType)t), path);
            pathEditedAt = -1d;
            pendingForceRescan = true;
            if (host != null)
                host.RequestRepaint();
        }

        private static bool RemoveOutput(string filePath, out WbbRecycleOutcome outcome, out string error)
        {
            return WbbModScanner.TryDelete(filePath, "", out outcome, out error);
        }

        private static string TrashFolderFor(string filePath)
        {
            string normalized = WbbUtil.NormalizePath(filePath);
            int idx = string.IsNullOrEmpty(normalized) ? -1 : normalized.LastIndexOf('/');
            return idx > 0 ? normalized.Substring(0, idx) + "/" + WbbRecycleBin.TrashFolderName : "";
        }

        private static string TrashFolderNotice(IList<string> folders)
        {
            if (folders == null || folders.Count == 0)
                return "";
            var text = new System.Text.StringBuilder();
            for (int i = 0; i < folders.Count && i < DeletePreviewCount; i++)
                text.Append('\n').Append(L("Library.TrashFolderAt", folders[i]));
            if (folders.Count > DeletePreviewCount)
                text.Append('\n').Append(L("Library.DeleteMore", folders.Count - DeletePreviewCount));
            text.Append('\n').Append(L("Library.TrashRestoreHint", WbbRecycleBin.TrashSuffix));
            return text.ToString();
        }

        private void DeleteSingleFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;
            Row target = null;
            for (int i = 0; i < rows.Count; i++)
            {
                if (string.Equals(rows[i].file.filePath, filePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    target = rows[i];
                    break;
                }
            }

            if (target == null)
            {
                EditorUtility.DisplayDialog(L("Common.ProductName"), L("Library.DeleteNone"), L("Common.Ok"));
                return;
            }

            string sizeText = WbbUtil.FormatSize(target.file.sizeBytes);
            if (!EditorUtility.DisplayDialog(L("Library.DeleteTitle"), L("Library.DeleteOneConfirm", target.nameText, sizeText, target.file.filePath) + "\n\n" + L("Library.DeleteNukeNote"), L("Common.Delete"), L("Common.Cancel")))
                return;
            WbbUtil.Log("Removing 1 output file (Recycle Bin, fallback " + WbbRecycleBin.TrashFolderName + "), " + sizeText + " total:\n" + target.file.filePath);
            WbbRecycleOutcome outcome;
            string error;
            if (RemoveOutput(target.file.filePath, out outcome, out error))
            {
                selected.Remove(target.file.filePath);
                if (string.Equals(lastClickedPath, target.file.filePath, System.StringComparison.OrdinalIgnoreCase))
                    lastClickedPath = "";
                if (outcome == WbbRecycleOutcome.TrashFolder)
                {
                    var folders = new List<string>();
                    string folder = TrashFolderFor(target.file.filePath);
                    if (!string.IsNullOrEmpty(folder))
                        folders.Add(folder);
                    SetStatus(target.nameText + ": " + L("Safety.DeletedToTrashFolder") + TrashFolderNotice(folders), WbbTone.Info);
                }
                else
                {
                    SetStatus(target.nameText + ": " + L("Safety.DeletedToRecycleBin"), WbbTone.Success);
                }
            }
            else
            {
                WbbUtil.LogError("Failed to remove " + target.file.filePath + ": " + error);
                string failed = L("Library.DeleteFailedItem", target.nameText, error);
                SetStatus(failed, WbbTone.Error);
                EditorUtility.DisplayDialog(L("Library.DeleteTitle"), failed, L("Common.Ok"));
            }

            pendingForceRescan = true;
            if (host != null)
                host.RequestRepaint();
        }

        private void DeleteSelectedFiles()
        {
            var targets = CollectSelected();
            if (targets.Count == 0)
                return;
            if (!ConfirmDeleteSelected(targets))
                return;
            RemoveRows(targets);
        }

        private static string TotalSizeText(List<Row> targets)
        {
            long total = 0L;
            for (int i = 0; i < targets.Count; i++)
                total += targets[i].file.sizeBytes;
            return WbbUtil.FormatSize(total);
        }

        private bool ConfirmDeleteSelected(List<Row> targets)
        {
            string totalText = TotalSizeText(targets);
            var preview = new System.Text.StringBuilder();
            for (int i = 0; i < targets.Count && i < DeletePreviewCount; i++)
            {
                if (i > 0)
                    preview.Append('\n');
                preview.Append(targets[i].nameText);
            }

            if (targets.Count > DeletePreviewCount)
            {
                preview.Append('\n');
                preview.Append(L("Library.DeleteMore", targets.Count - DeletePreviewCount));
            }

            string hiddenNotice = hiddenSelectedCount > 0 ? "\n\n" + L("Library.DeleteHiddenWarn", hiddenSelectedCount) : "";
            return EditorUtility.DisplayDialog(L("Library.DeleteTitle"), L("Library.DeleteConfirm", targets.Count, totalText, preview.ToString()) + hiddenNotice + "\n\n" + L("Library.DeleteNukeNote"), L("Common.Delete"), L("Common.Cancel"));
        }

        private void RemoveRows(List<Row> targets)
        {
            string totalText = TotalSizeText(targets);
            var log = new System.Text.StringBuilder();
            log.Append("Removing ").Append(targets.Count).Append(" output file(s) (Recycle Bin, fallback ").Append(WbbRecycleBin.TrashFolderName).Append("), ").Append(totalText).Append(" total:");
            for (int i = 0; i < targets.Count; i++)
            {
                log.Append('\n').Append(targets[i].file.filePath).Append("  [").Append(WbbUtil.FormatSize(targets[i].file.sizeBytes)).Append(']');
            }

            WbbUtil.Log(log.ToString());
            int toRecycleBin = 0;
            int toTrashFolder = 0;
            int failed = 0;
            int untouched = 0;
            var trashFolders = new List<string>();
            var trashSeen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            var failures = new List<string>();
            try
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    Row row = targets[i];
                    if (EditorUtility.DisplayCancelableProgressBar(L("Library.DeleteTitle"), L("Library.DeleteProgress", i + 1, targets.Count), (float)i / targets.Count))
                    {
                        untouched = targets.Count - i;
                        WbbUtil.Log("Delete canceled by user. " + untouched + " file(s) left untouched.");
                        break;
                    }

                    WbbRecycleOutcome outcome;
                    string error;
                    if (RemoveOutput(row.file.filePath, out outcome, out error))
                    {
                        if (outcome == WbbRecycleOutcome.TrashFolder)
                        {
                            toTrashFolder++;
                            string folder = TrashFolderFor(row.file.filePath);
                            if (!string.IsNullOrEmpty(folder) && trashSeen.Add(folder))
                                trashFolders.Add(folder);
                        }
                        else
                        {
                            toRecycleBin++;
                        }

                        selected.Remove(row.file.filePath);
                    }
                    else
                    {
                        failed++;
                        string line = L("Library.DeleteFailedItem", row.nameText, error);
                        failures.Add(line);
                        WbbUtil.LogWarning(line);
                    }
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogError("Delete aborted: " + e.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            selectionDirty = true;
            pendingForceRescan = true;
            WbbUtil.Log("Removed " + (toRecycleBin + toTrashFolder) + " output file(s): " + toRecycleBin + " to the Recycle Bin, " + toTrashFolder + " to " + WbbRecycleBin.TrashFolderName + ", " + failed + " failed.");
            var result = new System.Text.StringBuilder();
            result.Append(L("Library.DeleteResult", toRecycleBin + toTrashFolder, failed));
            if (untouched > 0)
                result.Append('\n').Append(L("Library.DeleteCanceled", untouched));
            if (toRecycleBin > 0)
                result.Append('\n').Append(L("Library.DeleteOutcomeCount", L("Safety.DeletedToRecycleBin"), toRecycleBin));
            if (toTrashFolder > 0)
            {
                result.Append('\n').Append(L("Library.DeleteOutcomeCount", L("Safety.DeletedToTrashFolder"), toTrashFolder));
                result.Append(TrashFolderNotice(trashFolders));
            }

            WbbTone tone = failed > 0 ? WbbTone.Error : untouched > 0 ? WbbTone.Warning : toTrashFolder > 0 ? WbbTone.Info : WbbTone.Success;
            SetStatus(result.ToString(), tone);
            if (failures.Count > 0)
            {
                var failText = new System.Text.StringBuilder();
                failText.Append(L("Library.DeleteResult", toRecycleBin + toTrashFolder, failed)).Append('\n');
                for (int i = 0; i < failures.Count && i < DeletePreviewCount; i++)
                    failText.Append('\n').Append(failures[i]);
                if (failures.Count > DeletePreviewCount)
                    failText.Append('\n').Append(L("Library.DeleteMore", failures.Count - DeletePreviewCount));
                EditorUtility.DisplayDialog(L("Library.DeleteTitle"), failText.ToString(), L("Common.Ok"));
            }

            if (host != null)
                host.RequestRepaint();
        }

        private void RebuildSelectedFiles()
        {
            var targets = CollectSelected();
            if (targets.Count == 0)
                return;
            var records = new List<WbbRecord>();
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].record != null)
                    records.Add(targets[i].record);
            }

            if (records.Count == 0)
                return;
            if (host == null)
                return;
            var byGroup = new Dictionary<string, List<WbbRecord>>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < records.Count; i++)
            {
                string key = host.RebuildGroupKey(records[i]);
                List<WbbRecord> bucket;
                if (!byGroup.TryGetValue(key, out bucket))
                {
                    bucket = new List<WbbRecord>();
                    byGroup[key] = bucket;
                }

                bucket.Add(records[i]);
            }

            List<WbbRecord> chosen = null;
            foreach (var kv in byGroup)
            {
                if (chosen == null || kv.Value.Count > chosen.Count)
                    chosen = kv.Value;
            }

            if (chosen == null)
                chosen = records;
            int dropped = records.Count - chosen.Count;
            string extra = dropped > 0 ? "\n\n" + L("Library.RebuildMixedFolders", dropped, byGroup.Count) : "";
            if (!EditorUtility.DisplayDialog(L("Library.RebuildTitle"), L("Library.RebuildConfirm", targets.Count, chosen.Count) + extra, L("Library.RebuildSend"), L("Common.Cancel")))
                return;
            for (int i = 0; i < chosen.Count; i++)
                host.RequestRebuild(chosen[i]);
        }
    }
}
