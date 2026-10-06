using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private const float RowHeight = WbbSize.Package.RowHeight;
        private const float TargetRowHeight = WbbSize.Package.TargetRowHeight;
        private const float CullPad = 90f;
        private const float IndentWidth = WbbSize.Package.IndentWidth;
        private const int MaxIndentDepth = 12;
        private const int MaxTargetRows = 6;
        private const int OverwritePreview = 5;
        private const float RowChipGap = WbbSize.Package.RootBadgeReserve - WbbSize.Package.RootBadge;
        private const string PackageExtension = ".unitypackage";
        private const string Pfx = WbbUtil.PrefPrefix + "Package.";
        private const string ExcludedKey = Pfx + "Excluded";
        private const string OutputDirKey = Pfx + "OutputDir";
        private const string TargetsKey = Pfx + "Targets";
        private const string CollapsedKey = Pfx + "Collapsed";
        private const string GatherTexKey = Pfx + "GatherTex";
        private const string GatherMatKey = Pfx + "GatherMat";
        private const string GatherFbxKey = Pfx + "GatherFbx";
        private const string GatherPrefabKey = Pfx + "GatherPrefab";
        private const string AssetsPrefix = "Assets/";
        private static string L(string key)
        {
            return WbbLang.T(key);
        }

        private static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }

        private static GUIContent LC(string key)
        {
            return WbbLang.C(key);
        }

        private class Target
        {
            public string prefabPath = "";
            public string prefabName = "";
            public string folderName = "";
            public string baseName = "";
            public string fileName = "";
            public bool missing;
        }

        private class Node
        {
            public string path = "";
            public string label = "";
            public bool isFolder;
            public int depth;
            public int assetIndex = -1;
            public readonly List<int> children = new List<int>();
            public bool expanded = true;
            public bool visible;
            public int shownFiles;
            public int shownRoots;
            public int shownIncluded;
            public int shownRiskOff;
            public long shownSize;
            public string infoText = "";
            public GUIContent labelContent;
        }

        private class Plan
        {
            public string prefabPath = "";
            public string baseName = "";
            public string fileName = "";
            public string outputPath = "";
        }

        private readonly WbbWindow host;
        private readonly List<Target> targets = new List<Target>();
        private readonly List<WbbPackageAsset> assets = new List<WbbPackageAsset>();
        private readonly List<string> assetSizeTexts = new List<string>();
        private readonly List<string> assetFolderTexts = new List<string>();
        private readonly List<Node> nodes = new List<Node>();
        private readonly List<int> visual = new List<int>();
        private readonly List<int> shownAssets = new List<int>();
        private readonly HashSet<string> excluded = new HashSet<string>(System.StringComparer.Ordinal);
        private readonly HashSet<string> collapsed = new HashSet<string>(System.StringComparer.Ordinal);
        private string search = "";
        private bool searching;
        private string outputDir = "";
        private WbbGatherOptions gather;
        private Vector2 scroll;
        private Vector2 targetScroll;
        private int hoverIndex = -1;
        private int firstVisible;
        private int lastVisible = -1;
        private bool busy;
        private int includedCount;
        private long includedSize;
        private string includedText = "";
        private string targetSummaryText = "";
        private string excludedCountText = "";
        private string statusLine = "";
        private MessageType statusType = MessageType.Info;
        private string statusFolder = "";
        private int riskOffCount;
        private int riskOnCount;
        private string riskNoticePending = "";
        private WbbTone riskNoticePendingTone = WbbTone.Info;
        private string riskNoticeText = "";
        private WbbTone riskNoticeTone = WbbTone.Info;
        private const int EmptyNone = 0;
        private const int EmptyNoMatch = 1;
        private const int EmptyNoTargets = 2;
        private const int EmptyNoAssets = 3;
        private const int EmptyActionNone = 0;
        private const int EmptyActionAddSelection = 1;
        private const int EmptyActionCollect = 2;
        private int emptyKind;
        private int emptyAction;
        private string emptyTitle = "";
        private string emptyMessage = "";
        private string emptyActionLabel = "";
        private bool structureDirty = true;
        private int displayLangVersion = -1;
        private int contentVersion = -1;
        private bool pendingCollect;
        private bool pendingValidate;
        private bool pendingTargetsChanged;
        private bool pendingClearTargets;
        private int pendingRemoveIndex = -1;
        private readonly List<string> pendingAddPaths = new List<string>();
        private readonly List<string> pendingDropPaths = new List<string>();
        private bool pendingAddSelection;
        private bool pendingExport;
        private bool pendingClearExcluded;
        private int pendingBrowseOutput;
        private string pendingPingPath = "";
        private int pendingToggleNode = -1;
        private bool pendingToggleValue;
        private int pendingExpandAll = -1;
        private bool pendingStatusSet;
        private string pendingStatus = "";
        private MessageType pendingStatusType = MessageType.Info;
        private string pendingStatusFolder = "";
        private GUIStyle rightMini;
        private GUIStyle midMini;
        private GUIStyle mutedLabel;
        private bool localStylesDark;
        private readonly GUIContent rowScratch = new GUIContent();
        private GUIContent exportContent;
        private GUIContent exportNoTargetsContent;
        private GUIContent exportNoAssetsContent;
        private GUIContent outputOpenContent;
        private GUIContent outputOpenOffContent;
        private GUIContent pingContent;
        private GUIContent removeContent;
        private GUIContent rootLockContent;
        private GUIContent dropContent;
        private GUIContent searchTipContent;
        private GUIContent excludedCountContent;
        private GUIContent expandAllContent;
        private GUIContent collapseAllContent;
        private GUIContent selectAllContent;
        private GUIContent folderLockContent;
        private string rootBadgeText = "";
        private string riskChipText = "";
        private string customChipText = "";
        private GUIContent riskChipContent;
        private GUIContent customChipContent;
        private float riskChipWidth;
        private float customChipWidth;
        public WbbPackageSection(WbbWindow host)
        {
            this.host = host;
        }

        public bool IsBusy
        {
            get
            {
                return busy;
            }
        }

        public float PaneWidth { get; set; }

        public void OnEnable()
        {
            LoadExcluded();
            LoadCollapsed();
            outputDir = EditorPrefs.GetString(OutputDirKey, "");
            if (string.IsNullOrEmpty(outputDir))
                outputDir = DefaultOutputDir();
            gather.textures = EditorPrefs.GetBool(GatherTexKey, false);
            gather.materials = EditorPrefs.GetBool(GatherMatKey, false);
            gather.models = EditorPrefs.GetBool(GatherFbxKey, false);
            gather.prefabs = EditorPrefs.GetBool(GatherPrefabKey, false);
            LoadTargets();
            pendingValidate = true;
            pendingCollect = true;
            structureDirty = true;
        }

        public void OnDisable()
        {
            SaveExcluded();
            SaveCollapsed();
            SaveTargets();
            EditorPrefs.SetString(OutputDirKey, outputDir == null ? "" : outputDir);
            SaveGather();
        }

        public void Refresh()
        {
            pendingValidate = true;
            pendingCollect = true;
            if (host != null)
                host.RequestRepaint();
        }

        public void SetTargets(IList<string> prefabPaths)
        {
            pendingClearTargets = true;
            pendingAddPaths.Clear();
            if (prefabPaths != null)
            {
                for (int i = 0; i < prefabPaths.Count; i++)
                {
                    string path = WbbUtil.NormalizePath(prefabPaths[i]);
                    if (!string.IsNullOrEmpty(path))
                        pendingAddPaths.Add(path);
                }
            }

            if (host != null)
                host.RequestRepaint();
        }

        public bool Embedded { get; set; }
        public float BodyTop { get; set; }
        public bool OutputCardVisible { get; set; } = true;

        public void OnGUI()
        {
            EnsureLocalStyles();
            EnsureContents();
            if (Event.current.type == EventType.Layout)
                SyncStructure();
            if (!Embedded)
                DrawTargetsCard();
            if (OutputCardVisible)
                DrawOutputCard();
            DrawToolbar();
            DrawList();
            EditorGUILayout.BeginVertical();
            DrawBottomBar();
            EditorGUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint)
            {
                float height = GUILayoutUtility.GetLastRect().height;
                if (height > 1f && Mathf.Abs(height - measuredFooterHeight) > 1f)
                {
                    measuredFooterHeight = height;
                    if (host != null)
                        host.RequestRepaint();
                }
            }

            ProcessPending();
        }

        private void EnsureLocalStyles()
        {
            bool dark = EditorGUIUtility.isProSkin;
            if (rightMini != null && midMini != null && mutedLabel != null && localStylesDark == dark)
                return;
            localStylesDark = dark;
            rightMini = new GUIStyle(WbbStyles.Caption)
            {
                alignment = TextAnchor.MiddleRight
            };
            midMini = new GUIStyle(WbbStyles.Caption)
            {
                alignment = TextAnchor.MiddleLeft
            };
            mutedLabel = new GUIStyle(WbbStyles.RowLabel);
            mutedLabel.normal.textColor = WbbStyles.Muted;
        }

        private void EnsureContents()
        {
            if (pingContent != null && contentVersion == WbbLang.Version)
                return;
            contentVersion = WbbLang.Version;
            pingContent = new GUIContent("◎", L("Common.Ping.tip"));
            removeContent = new GUIContent(string.Empty, L("Package.RemoveTarget.tip"));
            rootLockContent = new GUIContent(string.Empty, L("Package.RootLocked.tip"));
            dropContent = new GUIContent(L("Package.DropHint"));
            searchTipContent = new GUIContent(string.Empty, L("Package.SearchHint") + "\n" + L("Package.SearchHint.tip"));
            excludedCountContent = new GUIContent(excludedCountText, L("Package.ExcludedCount.tip"));
            outputOpenContent = LC("Common.OpenFolder");
            outputOpenOffContent = new GUIContent(outputOpenContent.text, L("Package.OutputOpenOff.tip"));
            expandAllContent = new GUIContent(string.Empty, L("Package.ExpandAll.tip"));
            collapseAllContent = new GUIContent(string.Empty, L("Package.CollapseAll.tip"));
            selectAllContent = new GUIContent(L("Common.SelectAll"), L("Package.SelectAll.tip"));
            folderLockContent = new GUIContent(string.Empty, L("Package.FolderLocked.tip"));
            rootBadgeText = L("Package.RootBadge");
            riskChipText = L("Safety.PackageRisk");
            customChipText = L("Safety.PackageCustomScript");
            riskChipContent = new GUIContent(string.Empty, L("Safety.PackageRisk.tip"));
            customChipContent = new GUIContent(string.Empty, L("Safety.PackageCustomScript.tip"));
            riskChipWidth = Mathf.Max(WbbSize.Package.RootBadge, WbbStyles.BadgeWidth(riskChipText));
            customChipWidth = Mathf.Max(WbbSize.Package.RootBadge, WbbStyles.BadgeWidth(customChipText));
            BuildExportContents();
            targetSummaryText = L("Package.TargetSummary", targets.Count);
        }

        private void BuildExportContents()
        {
            string text = L("Package.Export", targets.Count);
            exportContent = new GUIContent(text, L("Package.Export.tip"));
            exportNoTargetsContent = new GUIContent(text, L(Embedded ? "Package.ExportOffNoTargetsEmbedded.tip" : "Package.ExportOffNoTargets.tip"));
            exportNoAssetsContent = new GUIContent(text, L("Package.ExportOffNoAssets.tip"));
        }
    }
}
