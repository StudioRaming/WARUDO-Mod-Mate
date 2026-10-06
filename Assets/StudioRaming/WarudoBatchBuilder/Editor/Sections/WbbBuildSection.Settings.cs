using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private bool modeSegmentOwnLineLayout;
        private readonly bool[] row1BreakLayout = new bool[3];
        private readonly float[] row1Widths = new float[3];
        private readonly float[] row1Gaps = new float[3];
        private readonly bool[] row3BreakLayout = new bool[4];
        private readonly float[] row3Widths = new float[4];
        private readonly float[] row3Gaps = new float[4];
        private float settingsRowWidth;
        private readonly GUIContent settingsMeasure = new GUIContent();
        private bool pipelineBadgeLayout;
        private WbbUtil.PipelineKind pipelineKind;
        private string pipelineTypeName;
        private int pipelineLangVersion = -1;
        private string pipelineBadgeText = "";
        private float pipelineBadgeWidth;
        private WbbTone pipelineTone = WbbTone.Info;
        private readonly GUIContent pipelineTip = new GUIContent();
        private float typeLabelWidth, versionLabelWidth, authorLabelWidth, outputLabelWidth;
        private float settingsTitleWidth;
        private float settingsColumnWidth;
        private void DrawSettingsCard()
        {
            EnsureSettingsContents();
            WbbStyles.BeginCard();
            Rect header = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(WbbSize.Section.HeaderHeight));
            WbbStyles.SectionSurface(header);
            if (Event.current.type == EventType.Repaint && header.width > WbbSize.MeasureMinWidth && Mathf.Abs(header.width - settingsRowWidth) > 1f)
            {
                settingsRowWidth = header.width;
                RequestRepaint();
            }

            GUILayout.Space(WbbSize.Section.HeaderInset);
            BeginHeaderCell(false);
            bool next = WbbStyles.FoldoutFit(settingsOpen, LC("Build.SettingsSection"));
            EndHeaderCell();
            if (next != settingsOpen)
            {
                pendingSettingsOpen = next ? 1 : 0;
                RequestRepaint();
            }

            GUILayout.Space(WbbSize.SpaceMedium);
            if (settingsOpen)
            {
                if (!modeSegmentOwnLineLayout)
                {
                    BeginHeaderCell(false);
                    DrawModeSegment(GUILayout.Width(modeSegmentWidth), GUILayout.Height(WbbSize.LineHeight));
                    EndHeaderCell();
                }

                GUILayout.FlexibleSpace();
            }
            else
            {
                string summary;
                if (!IsPackageMode)
                {
                    string version = string.IsNullOrEmpty(job.modVersion) ? "1.0.0" : job.modVersion;
                    summary = WbbLang.TypeName(CurrentType) + "  ·  v" + version + "  ·  " + (string.IsNullOrEmpty(job.outputDirectory) ? L("Build.OutputNotSet") : job.outputDirectory);
                }
                else
                {
                    summary = L("Build.ModePackage");
                    if (packageSection != null)
                        summary += "  ·  " + packageSection.SettingsSummary();
                }

                BeginHeaderCell(true);
                Rect summaryRect = GUILayoutUtility.GetRect(GUIContent.none, midMini, GUILayout.ExpandWidth(true), GUILayout.MinWidth(WbbSize.TextMinWidth), GUILayout.Height(WbbSize.LineHeight));
                GUI.Label(summaryRect, new GUIContent(WbbStyles.Fit(summary, midMini, summaryRect.width), summary), midMini);
                EndHeaderCell();
                GUILayout.Space(WbbSize.SpaceMedium);
            }

            rowScratch.text = PresetButtonText();
            rowScratch.tooltip = L("Build.PresetMenu.tip");
            rowScratch.image = null;
            BeginHeaderCell(false);
            float presetWidth = Mathf.Min(WbbStyles.ButtonWidth(rowScratch, UI.SRGlyph.ChevronDown), WbbSize.Build.PresetButtonMaxWidth);
            bool presetClicked = WbbStyles.Button(rowScratch, UI.SRGlyph.ChevronDown, WbbButtonKind.Ghost, GUILayout.Height(WbbSize.ControlMini), GUILayout.Width(presetWidth));
            EndHeaderCell();
            GUILayout.Space(WbbSize.SpaceSmall);
            EditorGUILayout.EndHorizontal();
            if (presetClicked)
                OpenPresetMenu();
            if (settingsOpen && modeSegmentOwnLineLayout)
            {
                GUILayout.Space(WbbSize.SpaceTight);
                DrawModeSegment();
            }

            if (!settingsOpen || IsPackageMode)
            {
                WbbStyles.EndCard();
                return;
            }

            GUILayout.Space(WbbSize.SpaceTight);
            EditorGUILayout.BeginHorizontal();
            FieldLabel(LC("Build.Type"), typeLabelWidth);
            int typeIndex = Mathf.Clamp(job.exportType, 0, TypeCount - 1);
            int newType = WbbStyles.Popup(typeIndex, typeNames, GUILayout.Width(typePopupWidth));
            WbbStyles.NextCell(row1BreakLayout[1], WbbSize.SpaceLarge);
            FieldLabel(LC("Build.Version"), versionLabelWidth);
            string currentVersion = job.modVersion ?? "";
            string editedVersion = WbbStyles.TextField(currentVersion, GUILayout.Width(WbbSize.Build.VersionFieldWidth));
            WbbStyles.NextCell(row1BreakLayout[2], WbbSize.SpaceLarge);
            DrawAuthorField();
            EditorGUILayout.EndHorizontal();
            if (newType != typeIndex)
            {
                int picked = Mathf.Clamp(newType, 0, TypeCount - 1);
                AfterFocusRelease(delegate
                {
                    ChangeType(picked);
                });
            }

            if (editedVersion != currentVersion)
            {
                job.modVersion = editedVersion;
                EditorPrefs.SetString(Pfx + "Version", editedVersion);
                preflightDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            FieldLabel(LC("Build.Output"), outputLabelWidth);
            string currentOutput = job.outputDirectory == null ? "" : job.outputDirectory;
            string editedOutput = WbbStyles.DelayedTextField(currentOutput);
            if (editedOutput != currentOutput)
            {
                job.outputDirectory = editedOutput;
                EditorPrefs.SetString(OutputDirKey(CurrentType), editedOutput);
                namesDirty = true;
                preflightDirty = true;
            }

            if (WbbStyles.MiniButton(LC("Build.OutputPick"), GUILayout.Width(WbbSize.IconButtonWide)))
            {
                pendingPickOutput = true;
                RequestRepaint();
            }

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(job.outputDirectory)))
            {
                if (WbbStyles.MiniButton(LC("Build.OutputOpen")))
                    WbbUtil.RevealFolder(WbbUtil.NormalizePath(job.outputDirectory));
            }

            EditorGUILayout.EndHorizontal();
            if (string.IsNullOrEmpty(job.outputDirectory))
                GUILayout.Label(L("Build.OutputNotFoundHint"), WbbStyles.MiniInfo);
            EditorGUILayout.BeginHorizontal();
            bool useFolder = WbbStyles.ToggleLeftFit(useFolderShortContent, job.useFolderNamePrefix);
            if (useFolder != job.useFolderNamePrefix)
            {
                job.useFolderNamePrefix = useFolder;
                EditorPrefs.SetBool(Pfx + "UseFolderPrefix", useFolder);
                ClearRebuildNameOverrides();
                namesDirty = true;
                RequestRepaint();
            }

            WbbStyles.NextCell(row3BreakLayout[1], 0f);
            bool appendDate = WbbStyles.ToggleLeftFit(appendDateShortContent, job.appendDateSuffix);
            if (appendDate != job.appendDateSuffix)
            {
                job.appendDateSuffix = appendDate;
                EditorPrefs.SetBool(Pfx + "AppendDate", appendDate);
                ClearRebuildNameOverrides();
                namesDirty = true;
                RequestRepaint();
            }

            WbbStyles.NextCell(row3BreakLayout[2], 0f);
            bool overwrite = WbbStyles.ToggleLeftFit(overwriteShortContent, job.overwriteExisting);
            if (overwrite != job.overwriteExisting)
            {
                job.overwriteExisting = overwrite;
                EditorPrefs.SetBool(Pfx + "Overwrite", overwrite);
                ClearRebuildNameOverrides();
                namesDirty = true;
                RequestRepaint();
            }

            WbbStyles.NextCell(row3BreakLayout[3], 0f);
            GUILayout.FlexibleSpace();
            bool advancedClicked;
            EditorGUILayout.BeginVertical(WbbStyles.FitCell, GUILayout.Height(Row3CellHeight()), GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal(WbbStyles.FitCell);
            if (pipelineBadgeLayout)
            {
                Rect badge = GUILayoutUtility.GetRect(pipelineBadgeWidth, WbbSize.Badge.Height, GUILayout.Width(pipelineBadgeWidth), GUILayout.Height(WbbSize.Badge.Height));
                WbbStyles.Badge(badge, pipelineBadgeText, pipelineTone);
                GUI.Label(badge, pipelineTip, GUIStyle.none);
                GUILayout.Space(WbbSize.SpaceSmall);
            }

            advancedClicked = WbbStyles.Button(advancedContent, advancedOpen ? UI.SRGlyph.ChevronDown : UI.SRGlyph.ChevronRight, WbbButtonKind.Ghost, GUILayout.Height(WbbSize.ControlMini));
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            if (advancedClicked)
            {
                pendingAdvancedOpen = advancedOpen ? 0 : 1;
                RequestRepaint();
            }

            EditorGUILayout.EndHorizontal();
            if (!job.useFolderNamePrefix)
            {
                LabeledText("Build.NamePrefix", job.namePrefix, delegate (string v)
                {
                    job.namePrefix = v;
                    EditorPrefs.SetString(Pfx + "NamePrefix", v);
                    ClearRebuildNameOverrides();
                    namesDirty = true;
                });
            }

            if (advancedOpen)
            {
                GUILayout.Space(WbbSize.SpaceTight);
                LabeledText("Build.Description", job.modDescription, delegate (string v)
                {
                    job.modDescription = v;
                    EditorPrefs.SetString(Pfx + "Description", v);
                });
                bool manualScripts = WbbStyles.ToggleLeft(LC("Build.ManualScripts"), job.manualScriptFolder);
                if (manualScripts != job.manualScriptFolder)
                {
                    job.manualScriptFolder = manualScripts;
                    WbbListSettings.ManualScriptFolder = manualScripts;
                    preflightDirty = true;
                }

                if (manualScripts)
                    DrawBuildFolderRow();
                else
                    GUILayout.Label(LC("Build.AutoScripts"), WbbStyles.MiniInfo);
                bool strip = WbbStyles.ToggleLeft(LC("Safety.StripVrc"), job.stripVrcComponents);
                if (strip != job.stripVrcComponents)
                {
                    job.stripVrcComponents = strip;
                    EditorPrefs.SetBool(Pfx + "StripVrc", strip);
                    preflightDirty = true;
                }
            }

            WbbStyles.EndCard();
        }

        private void DrawModeSegment(params GUILayoutOption[] options)
        {
            int newMode = WbbStyles.Segmented(Mathf.Clamp(job.outputMode, 0, OutputModeCount - 1), outputModeContents, options);
            if (newMode != job.outputMode)
            {
                pendingOutputMode = newMode;
                RequestRepaint();
            }
        }

        private static void BeginHeaderCell(bool expandWidth)
        {
            EditorGUILayout.BeginVertical(WbbStyles.FitCell, GUILayout.Height(WbbSize.Section.HeaderHeight), GUILayout.ExpandWidth(expandWidth));
            GUILayout.FlexibleSpace();
        }

        private static void EndHeaderCell()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        private static float Row3CellHeight()
        {
            RectOffset margin = WbbStyles.ControlMargin;
            return WbbSize.LineHeight + margin.top + margin.bottom;
        }

        private void FieldLabel(GUIContent label, float width)
        {
            GUILayout.Label(label, fieldLabel, GUILayout.Width(width), GUILayout.Height(WbbSize.LineHeight));
        }

        private void DrawAuthorField()
        {
            FieldLabel(LC("Build.Author"), authorLabelWidth);
            string current = job.modAuthor ?? "";
            string edited = WbbStyles.TextField(current, GUILayout.MinWidth(WbbSize.Build.AuthorFieldMinWidth));
            if (edited != current)
            {
                job.modAuthor = edited;
                EditorPrefs.SetString(Pfx + "Author", edited);
                preflightDirty = true;
            }
        }

        private GUIContent useFolderShortContent, appendDateShortContent, overwriteShortContent;
        private GUIContent advancedContent;
        private int settingsContentsVersion = -1;
        private void EnsureSettingsContents()
        {
            if (useFolderShortContent != null && settingsContentsVersion == WbbLang.Version)
                return;
            settingsContentsVersion = WbbLang.Version;
            useFolderShortContent = new GUIContent(L("Build.UseFolderPrefix"), L("Build.UseFolderPrefix.tip"));
            appendDateShortContent = new GUIContent(L("Build.AppendDate.short"), L("Build.AppendDate.tip"));
            overwriteShortContent = new GUIContent(L("Build.Overwrite.short"), L("Build.Overwrite.tip") + "\n" + L("Build.OverwriteOffHint"));
            advancedContent = new GUIContent(L("Build.AdvancedSection"), L("Build.AdvancedToggle.tip"));
            float widest = 0f;
            if (outputModeContents != null)
            {
                for (int i = 0; i < outputModeContents.Length; i++)
                    widest = Mathf.Max(widest, WbbStyles.RowLabelBold.CalcSize(outputModeContents[i]).x);
            }

            int cells = outputModeContents != null ? Mathf.Max(1, outputModeContents.Length) : 2;
            modeSegmentWidth = cells * (widest + WbbSize.Build.ModeSegmentCellPad);
            settingsTitleWidth = WbbSize.Group.ArrowWidth + WbbSize.SpaceSmall + WbbStyles.CardTitle.CalcSize(LC("Build.SettingsSection")).x;
            typeLabelWidth = WbbStyles.TextWidth(fieldLabel, LC("Build.Type"));
            versionLabelWidth = WbbStyles.TextWidth(fieldLabel, LC("Build.Version"));
            authorLabelWidth = WbbStyles.TextWidth(fieldLabel, LC("Build.Author"));
            outputLabelWidth = WbbStyles.TextWidth(fieldLabel, LC("Build.Output"));
            settingsColumnWidth = Mathf.Max(WbbStyles.TextWidth(fieldLabel, LC("Build.NamePrefix")), Mathf.Max(WbbStyles.TextWidth(fieldLabel, LC("Build.Description")), WbbStyles.TextWidth(fieldLabel, LC("Build.StagingFolder"))));
            typePopupWidth = Mathf.Max(WbbSize.Build.TypePopupWidth, WbbStyles.PopupWidth(typeNames));
        }

        private float modeSegmentWidth = 2f * WbbSize.Build.ModeSegmentCellPad;
        private float typePopupWidth = WbbSize.Build.TypePopupWidth;
        private string PresetButtonText()
        {
            return string.IsNullOrEmpty(job.name) ? L("Common.Preset") : L("Build.PresetActive", job.name);
        }

        private void SyncSettingsLayout()
        {
            EnsureSettingsContents();
            float cardRow = paneWidthLayout - WbbSize.CardInsetX;
            float row = settingsRowWidth > 0f ? Mathf.Min(settingsRowWidth, cardRow) : cardRow;
            RectOffset label = fieldLabel.margin;
            RectOffset control = WbbStyles.ControlMargin;
            RectOffset none = GUIStyle.none.margin;
            RefreshPipeline();
            pipelineBadgeLayout = !IsPackageMode;
            settingsMeasure.text = PresetButtonText();
            float preset = Mathf.Min(WbbStyles.ButtonWidth(settingsMeasure, UI.SRGlyph.ChevronDown), WbbSize.Build.PresetButtonMaxWidth);
            float header = WbbSize.Section.HeaderInset + control.left + settingsTitleWidth + WbbStyles.LayoutGap(control, control, WbbSize.SpaceMedium) + modeSegmentWidth + WbbStyles.LayoutGap(control, none) + preset + WbbSize.SpaceSmall;
            modeSegmentOwnLineLayout = header > row;
            float inside = WbbStyles.LayoutGap(label, control);
            row1Widths[0] = typeLabelWidth + inside + typePopupWidth;
            row1Widths[1] = versionLabelWidth + inside + WbbSize.Build.VersionFieldWidth;
            row1Widths[2] = authorLabelWidth + inside + WbbSize.Build.AuthorFieldMinWidth;
            row1Gaps[1] = row1Gaps[2] = WbbStyles.LayoutGap(control, label, WbbSize.SpaceLarge);
            WbbStyles.PlanWrap(row1Widths, row1Gaps, row - label.left - control.right, row1BreakLayout);
            row3Widths[0] = WbbStyles.ToggleLeftFitWidth(useFolderShortContent);
            row3Widths[1] = WbbStyles.ToggleLeftFitWidth(appendDateShortContent);
            row3Widths[2] = WbbStyles.ToggleLeftFitWidth(overwriteShortContent);
            row3Widths[3] = WbbStyles.ButtonWidth(advancedContent, UI.SRGlyph.ChevronRight) + (pipelineBadgeLayout ? pipelineBadgeWidth + WbbSize.SpaceSmall : 0f);
            row3Gaps[1] = row3Gaps[2] = WbbStyles.LayoutGap(control, control);
            row3Gaps[3] = WbbStyles.LayoutGap(control, none);
            WbbStyles.PlanWrap(row3Widths, row3Gaps, row - control.left - control.right, row3BreakLayout);
        }

        private void RefreshPipeline()
        {
            string typeName;
            WbbUtil.PipelineKind kind = WbbUtil.CurrentPipeline(out typeName);
            int langVersion = WbbLang.Version;
            if (kind == pipelineKind && langVersion == pipelineLangVersion && string.Equals(typeName, pipelineTypeName, System.StringComparison.Ordinal))
                return;
            pipelineKind = kind;
            pipelineTypeName = typeName;
            pipelineLangVersion = langVersion;
            pipelineBadgeText = WbbUtil.PipelineName(kind, typeName);
            pipelineBadgeWidth = Mathf.Clamp(WbbStyles.BadgeWidth(pipelineBadgeText), WbbSize.Build.BadgeMinWidth, WbbSize.Build.BadgeMaxWidth);
            string hintKey;
            switch (kind)
            {
                case WbbUtil.PipelineKind.Urp:
                    hintKey = "Build.PipelineUrp";
                    pipelineTone = WbbTone.Info;
                    break;
                case WbbUtil.PipelineKind.Hdrp:
                    hintKey = "Build.PipelineHdrp";
                    pipelineTone = WbbTone.Warning;
                    break;
                case WbbUtil.PipelineKind.Custom:
                    hintKey = "Build.PipelineCustom";
                    pipelineTone = WbbTone.Neutral;
                    break;
                default:
                    hintKey = "Build.PipelineBuiltIn";
                    pipelineTone = WbbTone.Info;
                    break;
            }

            pipelineTip.text = string.Empty;
            pipelineTip.image = null;
            pipelineTip.tooltip = L("Build.Pipeline", pipelineBadgeText) + "\n" + L(hintKey) + "\n\n" + L("Build.Pipeline.tip");
        }

        private void LabeledText(string langKey, string value, System.Action<string> onChange)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(LC(langKey), fieldLabel, GUILayout.Width(settingsColumnWidth));
            string current = value ?? "";
            string edited = WbbStyles.TextField(current);
            EditorGUILayout.EndHorizontal();
            if (edited != current)
                onChange(edited);
        }

        private void DrawBuildFolderRow()
        {
            var type = CurrentType;
            string stagingFolder = WbbUtil.ResolveBuildFolder(job, type);
            bool stagingEmpty = string.IsNullOrEmpty(stagingFolder);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(LC("Build.StagingFolder"), fieldLabel, GUILayout.Width(settingsColumnWidth));
            string shown = stagingEmpty ? L("Build.StagingNotSet") : stagingFolder;
            GUILayout.Label(new GUIContent(shown, stagingEmpty ? L("Build.StagingFolder.tip") : stagingFolder), stagingEmpty ? WbbStyles.MiniInfo : midMini, GUILayout.MinWidth(WbbSize.TextMinWidth));
            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(string.Equals(scopeFolder, "Assets", System.StringComparison.OrdinalIgnoreCase)))
            {
                if (WbbStyles.MiniButton(LC("Tree.SetFromSelection"), GUILayout.Width(WbbSize.Build.SetFromSelectionButton)))
                {
                    pendingBuildFolder = scopeFolder;
                    RequestRepaint();
                }
            }

            if (WbbStyles.MiniButton(LC("Build.OutputPick"), GUILayout.Width(WbbSize.IconButtonWide)))
            {
                pendingPickStaging = true;
                RequestRepaint();
            }

            EditorGUILayout.EndHorizontal();
            if (!stagingEmpty && !HasPerTypeStaging(type))
                GUILayout.Label(L("Build.StagingShared"), WbbStyles.MiniInfo);
        }

        private bool HasPerTypeStaging(WbbExportType type)
        {
            int index = (int)type;
            if (job.buildFolderPathsByType == null || index >= job.buildFolderPathsByType.Count)
                return false;
            return !string.IsNullOrEmpty(job.buildFolderPathsByType[index]);
        }

        private void ChangeType(int typeIndex)
        {
            job.exportType = typeIndex;
            EditorPrefs.SetInt(Pfx + "Type", typeIndex);
            var type = (WbbExportType)typeIndex;
            job.outputDirectory = EditorPrefs.GetString(OutputDirKey(type), WbbUtil.DefaultOutputPath(type));
            ClearRebuildNameOverrides();
            namesDirty = true;
            preflightDirty = true;
            RequestRepaint();
        }

        private void OpenPresetMenu()
        {
            var menu = new GenericMenu();
            IList<WbbJob> presets = WbbDatabase.Jobs;
            menu.AddItem(LC("Build.PresetSaveNew"), false, delegate
            {
                presetNameInput = "";
                pendingSavePreset = true;
                RequestRepaint();
            });
            if (presets.Count > 0)
                menu.AddSeparator("");
            for (int i = 0; i < presets.Count; i++)
            {
                WbbJob preset = presets[i];
                if (preset == null)
                    continue;
                string name = string.IsNullOrEmpty(preset.name) ? L("Common.None") : preset.name;
                string id = preset.id;
                bool active = string.Equals(activeJobId, id, System.StringComparison.Ordinal);
                menu.AddItem(new GUIContent(L("Build.PresetApplyMenu") + "/" + name), active, delegate
                {
                    pendingApplyPresetId = id;
                    RequestRepaint();
                });
                menu.AddItem(new GUIContent(L("Build.PresetOverwriteMenu") + "/" + name), false, delegate
                {
                    presetNameInput = name;
                    pendingSavePreset = true;
                    RequestRepaint();
                });
                menu.AddItem(new GUIContent(L("Build.PresetDeleteMenu") + "/" + name), false, delegate
                {
                    pendingDeletePresetId = id;
                    RequestRepaint();
                });
            }

            menu.ShowAsContext();
        }

        private string NextPresetName()
        {
            int n = WbbDatabase.Jobs.Count + 1;
            while (WbbDatabase.FindJobByName(L("Common.PresetAutoName", n)) != null)
                n++;
            return L("Common.PresetAutoName", n);
        }

        private void SavePreset()
        {
            string name = presetNameInput == null ? "" : presetNameInput.Trim();
            if (name.Length == 0)
                name = NextPresetName();
            WbbJob existing = WbbDatabase.FindJobByName(name);
            if (existing != null && !EditorUtility.DisplayDialog(L("Common.Preset"), L("Build.PresetOverwrite", name), L("Build.PresetOverwriteOk"), L("Common.Cancel")))
                return;
            SyncHiddenToJob();
            WbbJob stored = CloneJob(job);
            stored.id = existing != null ? existing.id : WbbUtil.NewId();
            stored.name = name;
            stored.lastUsedIso = WbbUtil.NowIso();
            WbbDatabase.UpsertJob(stored);
            job.id = stored.id;
            job.name = name;
            activeJobId = stored.id;
            EditorPrefs.SetString(Pfx + "ActiveJob", activeJobId);
            presetNameInput = "";
            SetStatus(L("Build.PresetApplied", name), MessageType.Info);
            RequestRepaint();
        }

        private void ApplyPreset(string id)
        {
            WbbJob stored = WbbDatabase.GetJob(id);
            if (stored == null)
                return;
            job.id = stored.id;
            job.name = stored.name;
            job.buildFolderGuid = stored.buildFolderGuid;
            job.buildFolderPath = stored.buildFolderPath;
            job.manualScriptFolder = stored.manualScriptFolder;
            job.exportType = Mathf.Clamp(stored.exportType, 0, TypeCount - 1);
            job.modVersion = stored.modVersion;
            job.modAuthor = stored.modAuthor;
            job.modDescription = stored.modDescription;
            job.appendDateSuffix = stored.appendDateSuffix;
            job.useFolderNamePrefix = stored.useFolderNamePrefix;
            job.namePrefix = stored.namePrefix;
            job.overwriteExisting = stored.overwriteExisting;
            job.outputMode = Mathf.Clamp(stored.outputMode, 0, OutputModeCount - 1);
            job.stripVrcComponents = stored.stripVrcComponents;
            job.buildFolderGuidsByType = stored.buildFolderGuidsByType != null ? new List<string>(stored.buildFolderGuidsByType) : new List<string>();
            job.buildFolderPathsByType = stored.buildFolderPathsByType != null ? new List<string>(stored.buildFolderPathsByType) : new List<string>();
            job.manualNameGuids = stored.manualNameGuids != null ? new List<string>(stored.manualNameGuids) : new List<string>();
            job.manualNames = stored.manualNames != null ? new List<string>(stored.manualNames) : new List<string>();
            SaveManualNames();
            job.outputDirectory = string.IsNullOrEmpty(stored.outputDirectory) ? EditorPrefs.GetString(OutputDirKey(CurrentType), WbbUtil.DefaultOutputPath(CurrentType)) : stored.outputDirectory;
            WbbListSettings.AddHidden(stored.excludedPrefabGuids);
            string scope = WbbUtil.ResolveFolder(stored.prefabFolderGuid, stored.prefabFolderPath);
            if (WbbUtil.IsValidProjectFolder(scope))
                pendingScope = scope;
            activeJobId = stored.id;
            SavePrefs();
            ClearRebuildNameOverrides();
            itemsDirty = true;
            namesDirty = true;
            preflightDirty = true;
            SetStatus(L("Build.PresetApplied", string.IsNullOrEmpty(stored.name) ? L("Common.None") : stored.name), MessageType.Info);
            RequestRepaint();
        }

        private void DeletePreset(string id)
        {
            WbbJob stored = WbbDatabase.GetJob(id);
            if (stored == null)
                return;
            string name = string.IsNullOrEmpty(stored.name) ? L("Common.None") : stored.name;
            if (!EditorUtility.DisplayDialog(L("Common.Preset"), L("Build.PresetDeleteConfirm", name), L("Common.Delete"), L("Common.Cancel")))
                return;
            WbbDatabase.DeleteJob(id);
            if (string.Equals(activeJobId, id, System.StringComparison.Ordinal))
            {
                activeJobId = "";
                job.id = "";
                job.name = "";
                EditorPrefs.SetString(Pfx + "ActiveJob", "");
            }

            RequestRepaint();
        }

        private static WbbJob CloneJob(WbbJob source)
        {
            var copy = new WbbJob();
            if (source == null)
                return copy;
            copy.id = source.id;
            copy.name = source.name;
            copy.buildFolderGuid = source.buildFolderGuid;
            copy.buildFolderPath = source.buildFolderPath;
            copy.manualScriptFolder = source.manualScriptFolder;
            copy.prefabFolderGuid = source.prefabFolderGuid;
            copy.prefabFolderPath = source.prefabFolderPath;
            copy.exportType = source.exportType;
            copy.modVersion = source.modVersion;
            copy.modAuthor = source.modAuthor;
            copy.modDescription = source.modDescription;
            copy.outputDirectory = source.outputDirectory;
            copy.appendDateSuffix = source.appendDateSuffix;
            copy.useFolderNamePrefix = source.useFolderNamePrefix;
            copy.namePrefix = source.namePrefix;
            copy.recurseSubFolders = source.recurseSubFolders;
            copy.overwriteExisting = source.overwriteExisting;
            copy.outputMode = source.outputMode;
            copy.stripVrcComponents = source.stripVrcComponents;
            copy.lastUsedIso = source.lastUsedIso;
            copy.excludedPrefabGuids = source.excludedPrefabGuids == null ? new List<string>() : new List<string>(source.excludedPrefabGuids);
            copy.buildFolderGuidsByType = source.buildFolderGuidsByType == null ? new List<string>() : new List<string>(source.buildFolderGuidsByType);
            copy.buildFolderPathsByType = source.buildFolderPathsByType == null ? new List<string>() : new List<string>(source.buildFolderPathsByType);
            copy.manualNameGuids = source.manualNameGuids == null ? new List<string>() : new List<string>(source.manualNameGuids);
            copy.manualNames = source.manualNames == null ? new List<string>() : new List<string>(source.manualNames);
            return copy;
        }
    }
}
