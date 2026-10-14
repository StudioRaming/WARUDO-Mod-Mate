using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private void StartBuild()
        {
            string error;
            TryStartBuild(true, out error);
        }

        private bool TryStartBuild(bool confirm, out string error)
        {
            error = "";
            if (runner == null || runner.IsRunning)
            {
                error = L("Win.BusyRebuild");
                return false;
            }

            if (namesDirty)
                RefreshNames();
            List<WbbBuildItem> targets = CollectBuildItems();
            if (targets.Count == 0)
            {
                preflightDirty = true;
                RequestRepaint();
                error = "No prefab is selected in the Build tab.";
                return false;
            }

            RecomputePreflight();
            if (blockingCount > 0)
            {
                issuesOpen = true;
                SetStatus(L("Build.PreflightBlockLine", blockingCount), MessageType.Error);
                RequestRepaint();
                var blocking = new List<WbbPreflightIssue>();
                for (int i = 0; i < preflightIssues.Count; i++)
                {
                    if (preflightIssues[i] != null && preflightIssues[i].blocking)
                        blocking.Add(preflightIssues[i]);
                }

                error = L("Build.PreflightBlockLine", blockingCount) + "\n" + FormatIssues(blocking);
                return false;
            }

            var warnings = new List<WbbPreflightIssue>();
            for (int i = 0; i < preflightIssues.Count; i++)
            {
                if (preflightIssues[i] != null && !preflightIssues[i].blocking)
                    warnings.Add(preflightIssues[i]);
            }

            string outputDir = WbbUtil.NormalizePath(job.outputDirectory);
            if (string.IsNullOrEmpty(outputDir))
                outputDir = WbbUtil.DefaultOutputPath(CurrentType);
            if (string.IsNullOrEmpty(outputDir))
            {
                if (confirm)
                    pendingPickOutput = true;
                RequestRepaint();
                error = "No output folder: WARUDO was not found and the Build tab has no output folder for this type.";
                return false;
            }

            var preview = new System.Text.StringBuilder();
            for (int i = 0; i < targets.Count && i < ConfirmPreviewCount; i++)
            {
                if (i > 0)
                    preview.Append('\n');
                preview.Append(targets[i].modName).Append(WbbUtil.ModExtension);
            }

            if (targets.Count > ConfirmPreviewCount)
            {
                preview.Append('\n');
                preview.Append(L("Build.ConfirmMore", targets.Count - ConfirmPreviewCount));
            }

            string body = L("Build.ConfirmBody", WbbLang.TypeName(CurrentType), targets.Count, string.IsNullOrEmpty(job.modVersion) ? "1.0.0" : job.modVersion, string.IsNullOrEmpty(job.modAuthor) ? L("Build.NoAuthorUnset") : job.modAuthor, outputDir, preview.ToString());
            if (warnings.Count > 0)
                body += L("Build.WarnHeader") + "\n" + FormatIssues(warnings);
            if (confirm && !EditorUtility.DisplayDialog(L("Build.ConfirmTitle"), body, L("Build.ConfirmGo"), L("Common.Cancel")))
            {
                error = "Canceled.";
                return false;
            }

            SyncHiddenToJob();
            WbbJob snapshot = CloneJob(job);
            snapshot.outputDirectory = outputDir;
            snapshot.lastUsedIso = WbbUtil.NowIso();
            if (!runner.Start(targets, snapshot))
            {
                error = string.IsNullOrEmpty(runner.StatusMessage) ? L("Build.PreflightBlockLine", 1) : runner.StatusMessage;
                SetStatus(error, MessageType.Error);
                preflightDirty = true;
                RequestRepaint();
                return false;
            }

            resultOutputDir = outputDir;
            resultVisible = false;
            if (!string.IsNullOrEmpty(activeJobId))
            {
                WbbJob stored = WbbDatabase.GetJob(activeJobId);
                if (stored != null)
                {
                    stored.lastUsedIso = snapshot.lastUsedIso;
                    WbbDatabase.UpsertJob(stored);
                }
            }

            rebuildQueuedCount = 0;
            rebuildToken = "";
            SetStatus(L("Build.WalkAwayHint"), MessageType.Info);
            RequestRepaint();
            return true;
        }

        private static string FormatIssues(List<WbbPreflightIssue> issues)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < issues.Count && i < IssuePreviewCount; i++)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append("· ").Append(issues[i].message);
                if (!string.IsNullOrEmpty(issues[i].detail))
                    sb.Append("\n   ").Append(issues[i].detail);
            }

            if (issues.Count > IssuePreviewCount)
            {
                sb.Append('\n');
                sb.Append(L("Build.ConfirmMore", issues.Count - IssuePreviewCount));
            }

            return sb.ToString();
        }

        private void OnRecordFinished(WbbRecord record)
        {
            if (record == null)
                return;
            int index = IndexOfGuid(record.prefabGuid);
            if (index >= 0)
            {
                Item item = items[index];
                item.record = record;
                item.buildCount++;
                item.signature = WbbUtil.SourceSignature(item.path);
                ComputeState(item);
                displayDirty = true;
                filterDirty = true;
            }

            RequestRepaint();
        }

        private void OnBuildFinished(bool canceled)
        {
            DiscardRebuildBorrow();
            resultCanceled = canceled;
            resultSuccess = runner != null ? runner.SuccessCount : 0;
            resultFailed = runner != null ? runner.FailedCount : 0;
            resultCanceledItems = 0;
            resultSkipped = 0;
            if (runner != null)
            {
                IList<WbbBuildItem> ran = runner.Items;
                for (int i = 0; i < ran.Count; i++)
                {
                    if (ran[i] == null)
                        continue;
                    switch (ran[i].status)
                    {
                        case WbbBuildStatus.Success:
                        case WbbBuildStatus.Failed:
                            break;
                        case WbbBuildStatus.Skipped:
                            resultSkipped++;
                            break;
                        default:
                            resultCanceledItems++;
                            break;
                    }
                }
            }

            RefreshResultLabel();
            SetStatus("", MessageType.Info);
            if (!WbbBuildRunner.AbortingSilently)
            {
                resultVisible = true;
                pendingResult = true;
            }

            itemsDirty = true;
            preflightDirty = true;
            RequestRepaint();
        }

        private void RefreshResultLabel()
        {
            resultLabelVersion = WbbLang.Version;
            var sb = new System.Text.StringBuilder();
            sb.Append(L(resultCanceled ? "Build.ResultHeadCanceled" : "Build.ResultHeadDone"));
            if (resultSuccess > 0)
                sb.Append(" · ").Append(L("Build.ResultSuccess", resultSuccess));
            if (resultFailed > 0)
                sb.Append(" · ").Append(L("Build.ResultFailed", resultFailed));
            if (resultCanceledItems > 0)
                sb.Append(" · ").Append(L("Build.ResultCanceledItems", resultCanceledItems));
            if (resultSkipped > 0)
                sb.Append(" · ").Append(L("Build.ResultSkipped", resultSkipped));
            resultLabel = sb.ToString();
            if (resultFailed > 0)
                resultTone = WbbTone.Error;
            else if (resultCanceled || resultCanceledItems > 0 || resultSkipped > 0)
                resultTone = WbbTone.Warning;
            else
                resultTone = WbbTone.Success;
        }

        private void ShowResult()
        {
            resultVisible = true;
            if (host != null)
            {
                if (resultLabelVersion != WbbLang.Version)
                    RefreshResultLabel();
                host.ShowNotification(new GUIContent(resultLabel), 4.0);
            }

            RequestRepaint();
        }
    }
}
