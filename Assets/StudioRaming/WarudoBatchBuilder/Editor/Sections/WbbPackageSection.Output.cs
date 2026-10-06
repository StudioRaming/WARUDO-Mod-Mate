using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private static readonly string[] GatherKeys =
        {
            "Package.GatherTex",
            "Package.GatherMat",
            "Package.GatherFbx",
            "Package.GatherPrefab"
        };
        private float outputRowWidth;
        private bool gatherTitleOwnLineLayout;
        private readonly bool[] gatherBreakLayout = new bool[4];
        private readonly float[] gatherWidths = new float[4];
        private readonly float[] gatherGaps = new float[4];
        private int gatherPlanVersion = -1;
        private float gatherPlanWidth = -1f;
        private float PaneRowLimit()
        {
            return PaneWidth > 0f ? PaneWidth : float.MaxValue;
        }

        private float CardRowLimit()
        {
            return PaneWidth > 0f ? Mathf.Max(0f, PaneWidth - WbbSize.CardInsetX) : float.MaxValue;
        }

        private float GatherRowWidth()
        {
            float measured = outputRowWidth > 0f ? outputRowWidth : float.MaxValue;
            float width = Mathf.Min(measured, CardRowLimit());
            return width < float.MaxValue ? width : 0f;
        }

        private void DrawOutputCard()
        {
            WbbStyles.BeginCard();
            Rect outputRow = EditorGUILayout.BeginHorizontal();
            if (Event.current.type == EventType.Repaint && outputRow.width > WbbSize.MeasureMinWidth && Mathf.Abs(outputRow.width - outputRowWidth) > 1f)
            {
                outputRowWidth = outputRow.width;
                if (host != null)
                    host.RequestRepaint();
            }

            GUILayout.Label(LC("Package.OutputTitle"), midMini, GUILayout.ExpandWidth(false));
            string current = outputDir == null ? "" : outputDir;
            string edited = WbbStyles.DelayedTextField(current);
            if (edited != current)
            {
                outputDir = WbbUtil.NormalizePath(edited);
                EditorPrefs.SetString(OutputDirKey, outputDir);
            }

            if (WbbStyles.MiniButton(LC("Package.Browse"), GUILayout.Width(WbbSize.Package.BrowseButton)))
            {
                GUI.FocusControl(null);
                pendingBrowseOutput = 1;
                if (host != null)
                    host.RequestRepaint();
            }

            bool noOutput = string.IsNullOrEmpty(outputDir);
            using (new EditorGUI.DisabledScope(noOutput))
            {
                if (WbbStyles.MiniButton(noOutput ? outputOpenOffContent : outputOpenContent, GUILayout.Width(WbbSize.Package.OutputOpenFolderButton)))
                    WbbUtil.RevealFolder(outputDir);
            }

            EditorGUILayout.EndHorizontal();
            DrawGatherRow();
            WbbStyles.EndCard();
        }

        private void DrawGatherRow()
        {
            GUILayout.Space(WbbSize.SpaceTight);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(LC("Package.GatherTitle"), midMini, GUILayout.ExpandWidth(false));
            WbbStyles.NextCell(gatherTitleOwnLineLayout, WbbSize.SpaceSmall);
            GatherToggle(GatherKeys[0], ref gather.textures, GatherTexKey);
            WbbStyles.NextCell(gatherBreakLayout[1], 0f);
            GatherToggle(GatherKeys[1], ref gather.materials, GatherMatKey);
            WbbStyles.NextCell(gatherBreakLayout[2], 0f);
            GatherToggle(GatherKeys[2], ref gather.models, GatherFbxKey);
            WbbStyles.NextCell(gatherBreakLayout[3], 0f);
            GatherToggle(GatherKeys[3], ref gather.prefabs, GatherPrefabKey);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void SyncGatherLayout()
        {
            float rowWidth = GatherRowWidth();
            if (rowWidth <= 0f)
                return;
            int version = WbbLang.Version;
            if (version == gatherPlanVersion && Mathf.Abs(rowWidth - gatherPlanWidth) < 0.5f)
                return;
            gatherPlanVersion = version;
            gatherPlanWidth = rowWidth;
            RectOffset label = WbbStyles.Caption.margin;
            RectOffset control = WbbStyles.ControlMargin;
            float toggles = 0f;
            for (int i = 0; i < GatherKeys.Length; i++)
            {
                gatherWidths[i] = WbbStyles.ToggleLeftFitWidth(LC(GatherKeys[i]));
                gatherGaps[i] = WbbStyles.LayoutGap(control, control);
                toggles += (i > 0 ? gatherGaps[i] : 0f) + gatherWidths[i];
            }

            float title = WbbStyles.TextWidth(WbbStyles.Caption, LC("Package.GatherTitle"));
            float oneLine = label.left + title + WbbStyles.LayoutGap(label, control, WbbSize.SpaceSmall) + toggles + control.right;
            gatherTitleOwnLineLayout = oneLine > rowWidth;
            float available = gatherTitleOwnLineLayout ? rowWidth - control.left - control.right : float.MaxValue;
            WbbStyles.PlanWrap(gatherWidths, gatherGaps, available, gatherBreakLayout);
        }

        public string SettingsSummary()
        {
            string summary = string.IsNullOrEmpty(outputDir) ? L("Package.OutputNotSet") : outputDir;
            bool[] on =
            {
                gather.textures,
                gather.materials,
                gather.models,
                gather.prefabs
            };
            string names = "";
            for (int i = 0; i < GatherKeys.Length; i++)
            {
                if (!on[i])
                    continue;
                names += (names.Length > 0 ? ", " : "") + L(GatherKeys[i]);
            }

            return names.Length > 0 ? summary + "  ·  " + L("Package.GatherTitle") + ": " + names : summary;
        }

        private void GatherToggle(string key, ref bool value, string prefKey)
        {
            bool next = WbbStyles.ToggleLeftFit(LC(key), value);
            if (next == value)
                return;
            value = next;
            EditorPrefs.SetBool(prefKey, value);
            if (host != null)
                host.RequestRepaint();
        }

        private void SaveGather()
        {
            EditorPrefs.SetBool(GatherTexKey, gather.textures);
            EditorPrefs.SetBool(GatherMatKey, gather.materials);
            EditorPrefs.SetBool(GatherFbxKey, gather.models);
            EditorPrefs.SetBool(GatherPrefabKey, gather.prefabs);
        }
    }
}
