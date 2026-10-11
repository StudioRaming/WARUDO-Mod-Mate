using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private void EnsureLocalStyles()
        {
            bool dark = EditorGUIUtility.isProSkin;
            if (rightMini != null && midMini != null && fieldLabel != null && subLine != null && localStylesDark == dark)
                return;
            localStylesDark = dark;
            rightMini = new GUIStyle(WbbStyles.MiniInfo)
            {
                alignment = TextAnchor.MiddleRight,
                clipping = TextClipping.Clip
            };
            midMini = new GUIStyle(WbbStyles.MiniInfo)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };
            fieldLabel = new GUIStyle(midMini)
            {
                wordWrap = false
            };
            subLine = new GUIStyle(WbbStyles.MiniInfo)
            {
                alignment = TextAnchor.UpperLeft,
                clipping = TextClipping.Clip,
                wordWrap = false
            };
        }

        private void EnsureContents()
        {
            if (viewContents != null && contentVersion == WbbLang.Version)
                return;
            contentVersion = WbbLang.Version;
            viewContents = new[]
            {
                new GUIContent(L("Build.ViewGrid"), L("Build.ViewGrid.tip")),
                new GUIContent(L("Build.ViewList"), L("Build.ViewList.tip")),
            };
            outputModeContents = new[]
            {
                new GUIContent(L("Build.ModeWarudo")),
                new GUIContent(L("Build.ModePackage"))
            };
            sortNames = new[]
            {
                L("Common.SortName"),
                L("Common.SortDate"),
                L("Build.SortStatus")
            };
            typeNames = new[]
            {
                L("Common.TypeProp"),
                L("Common.TypeCharacter"),
                L("Common.TypeEnvironment")
            };
            reasonTexts = new[]
            {
                L("Build.ReasonPinned"),
                L("Build.ReasonWork"),
                L("Build.ReasonBuilt"),
                L("Build.ReasonPhysics"),
                L("Build.ReasonRaw"),
                L("Build.ReasonHidden"),
            };
            menuButtonContent = new GUIContent(string.Empty, L("Build.RowMenu.tip"));
            listMenuContent = new GUIContent(string.Empty, L("Build.ListMenu.tip"));
            hideContent = new GUIContent(string.Empty, L("Build.RowHide.tip"));
            unhideContent = new GUIContent(string.Empty, L("Build.RowUnhide.tip"));
            pinContent = new GUIContent(string.Empty, L("Build.RowPin.tip"));
            thumbCreateContent = new GUIContent(string.Empty, L("Build.ThumbCreate.tip"));
            thumbRegenContent = new GUIContent(string.Empty, L("Build.MenuRegenThumb.tip"));
            treeToggleContent = new GUIContent(string.Empty, L("Build.TreeToggle.tip"));
            sceneRebuildContent = new GUIContent(L("Build.SceneRebuild"), L("Build.SceneRebuild.tip"));
            sceneRebuildIcon = new GUIContent(string.Empty, L("Build.SceneRebuild.tip"));
            runNoneContent = new GUIContent(L("Build.RunNone"), L("Runner.NoSelection.hint"));
            resultCloseContent = new GUIContent(string.Empty, L("Build.ResultClose.tip"));
            statusCloseTip = L("Build.StatusClose.tip");
            thumbNoneText = L("Build.ThumbNone");
            runContent = null;
            runContentCount = -1;
            displayDirty = true;
        }

        private void DrawBottomBar()
        {
            WbbStyles.Separator();
            if (busyLayout && runner != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(WbbSize.SpaceSmall);
                EditorGUILayout.BeginVertical();
                WbbStyles.ProgressBar(runner.Progress, L("Build.Progress", runner.FinishedCount, runner.TotalCount, runner.StatusMessage), WbbSize.Build.BuildProgressHeight);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(runner.CurrentModName, midMini);
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(runner.CancelRequested))
                {
                    if (WbbStyles.Button(LC("Build.Stop"), WbbButtonKind.Danger, GUILayout.Height(WbbSize.ControlLarge), GUILayout.Width(WbbSize.Build.StopButtonWidth)))
                        runner.Cancel();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(WbbSize.SpaceSmall);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(WbbSize.SpaceSmall);
                return;
            }

            if (resultLayout)
                DrawResultLine();
            if (issuesLayout)
                DrawIssueList();
            if (runContent == null || runContentCount != selectedCount || runContentPackage != IsPackageMode)
            {
                runContentCount = selectedCount;
                runContentPackage = IsPackageMode;
                runContent = IsPackageMode ? new GUIContent(L("Build.RunPackage", selectedCount), L("Build.RunPackage.tip")) : new GUIContent(L("Build.Run", selectedCount), L("Build.Run.tip"));
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(WbbSize.SpaceMedium);
            if (selectedCount == 0)
            {
                rowScratch.text = L("Runner.NoSelection");
                rowScratch.tooltip = L("Runner.NoSelection.hint");
                rowScratch.image = null;
                float hintWidth = Mathf.Max(WbbSize.Build.PreflightChipMinWidth, Mathf.Ceil(WbbStyles.MiniInfo.CalcSize(rowScratch).x) + WbbSize.Badge.ChipTextPad);
                hintWidth = Mathf.Min(hintWidth, Mathf.Max(WbbSize.Build.PreflightChipMinWidth, paneWidthLayout - WbbSize.Build.RunButtonMinWidth - WbbSize.SpaceMedium * 4f));
                Rect hint = GUILayoutUtility.GetRect(hintWidth, WbbSize.Control, GUILayout.Width(hintWidth), GUILayout.Height(WbbSize.Control));
                WbbStyles.Chip(hint, rowScratch.text, WbbTone.Warning);
                rowScratch.text = string.Empty;
                GUI.Label(hint, rowScratch, GUIStyle.none);
            }
            else if (!string.IsNullOrEmpty(preflightLine))
            {
                WbbTone tone = blockingCount > 0 ? WbbTone.Error : (warningCount > 0 ? WbbTone.Warning : WbbTone.Success);
                bool hasIssues = preflightIssues.Count > 0;
                rowScratch.text = preflightLine;
                rowScratch.tooltip = hasIssues ? L("Build.IssuesToggle.tip") : "";
                rowScratch.image = null;
                float chipWidth = Mathf.Min(Mathf.Ceil(WbbStyles.MiniInfo.CalcSize(rowScratch).x), Mathf.Max(WbbSize.Build.PreflightChipMinWidth, viewWidth - WbbSize.Build.PreflightChipReserve));
                Rect chip = GUILayoutUtility.GetRect(chipWidth, WbbSize.Control, GUILayout.ExpandWidth(false));
                chip.width += WbbSize.Build.PreflightChipPad + (hasIssues ? WbbSize.GlyphAdvance : 0f);
                if (hasIssues)
                    WbbStyles.Chip(chip, preflightLine, tone, issuesOpen ? UI.SRGlyph.ChevronDown : UI.SRGlyph.ChevronRight);
                else
                    WbbStyles.Chip(chip, preflightLine, tone);
                if (hasIssues && WbbStyles.HitButton(chip, rowScratch.tooltip))
                {
                    pendingIssuesOpen = issuesOpen ? 0 : 1;
                    RequestRepaint();
                }
            }
            else
            {
                GUILayout.Label(previewLine, midMini);
            }

            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(selectedCount == 0))
            {
                if (WbbStyles.Button(selectedCount > 0 ? runContent : runNoneContent, WbbButtonKind.Primary, GUILayout.Height(WbbSize.RunButton), GUILayout.MinWidth(WbbSize.Build.RunButtonMinWidth)))
                {
                    if (IsPackageMode)
                        pendingPackageExport = true;
                    else
                        pendingBuild = true;
                    RequestRepaint();
                }
            }

            GUILayout.Space(WbbSize.SpaceMedium);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(WbbSize.SpaceSnug);
        }

        private void DrawResultLine()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(WbbSize.SpaceSmall);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            WbbStyles.ProgressBar(1f, resultLabel, WbbSize.Build.BuildProgressHeight, resultTone);
            rowScratch.text = "";
            rowScratch.tooltip = resultLabel;
            rowScratch.image = null;
            GUI.Label(GUILayoutUtility.GetLastRect(), rowScratch, GUIStyle.none);
            EditorGUILayout.BeginVertical(GUILayout.Width(WbbSize.IconButton));
            GUILayout.Space(WbbSize.Build.ResultCloseTop);
            if (WbbStyles.IconButton(UI.SRGlyph.Close, resultCloseContent.tooltip, WbbButtonKind.Ghost, GUILayout.Width(WbbSize.IconButton), GUILayout.Height(WbbSize.ControlSmall)))
            {
                pendingCloseResult = true;
                RequestRepaint();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            if (resultQuickIterate)
                WbbStyles.Callout(L("QuickIterate.Notice"), WbbTone.Info);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (WbbStyles.Button(LC("Build.OpenOutput"), WbbButtonKind.Secondary, GUILayout.Height(WbbSize.ControlSmall)))
                WbbUtil.RevealFolder(WbbUtil.NormalizePath(string.IsNullOrEmpty(resultOutputDir) ? job.outputDirectory : resultOutputDir));
            if (WbbStyles.Button(LC("Build.OpenHistory"), WbbButtonKind.Secondary, GUILayout.Height(WbbSize.ControlSmall)) && host != null)
                host.SwitchTab(WbbWindow.TabHistory);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(WbbSize.SpaceSmall);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(WbbSize.SpaceSmall);
        }

        private void DrawIssueList()
        {
            WbbStyles.BeginCard();
            int shown = Mathf.Min(preflightIssues.Count, IssuePreviewCount);
            for (int i = 0; i < shown; i++)
            {
                WbbPreflightIssue issue = preflightIssues[i];
                if (issue == null)
                    continue;
                var ex = issue as WbbPreflightIssueEx;
                EditorGUILayout.BeginHorizontal();
                string text = string.IsNullOrEmpty(issue.detail) ? issue.message : issue.message + "\n" + issue.detail;
                WbbStyles.Callout(text, issue.blocking ? WbbTone.Error : WbbTone.Warning);
                if (ex != null && ex.code == WbbPreflightCodes.SettingsBroken && !string.IsNullOrEmpty(ex.actionPath))
                {
                    if (WbbStyles.Button(LC("Safety.RestoreSettings"), WbbButtonKind.Secondary, GUILayout.Width(WbbSize.Build.RestoreSettingsButton), GUILayout.Height(WbbSize.Control)))
                    {
                        pendingRestoreSettings = ex.actionPath;
                        RequestRepaint();
                    }
                }
                else if (ex != null && (ex.code == WbbPreflightCodes.SettingsMissing || ex.code == WbbPreflightCodes.SettingsNoProfile))
                {
                    if (WbbStyles.Button(LC("Safety.CreateSettings"), WbbButtonKind.Secondary, GUILayout.Width(WbbSize.Build.RestoreSettingsButton), GUILayout.Height(WbbSize.Control)))
                    {
                        pendingCreateSettings = true;
                        RequestRepaint();
                    }
                }
                else if (ex != null && ex.code == WbbPreflightCodes.WarudoNotFound)
                {
                    if (WbbStyles.Button(LC("Build.OutputPick"), WbbButtonKind.Secondary, GUILayout.Width(WbbSize.Build.OutputPickButton), GUILayout.Height(WbbSize.Control)))
                    {
                        pendingPickOutput = true;
                        RequestRepaint();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (preflightIssues.Count > shown)
                GUILayout.Label(L("Build.ConfirmMore", preflightIssues.Count - shown), WbbStyles.MiniInfo);
            WbbStyles.EndCard();
        }
    }
}
