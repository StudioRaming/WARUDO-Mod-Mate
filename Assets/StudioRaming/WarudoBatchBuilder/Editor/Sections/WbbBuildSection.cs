using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private const float RowHeight = WbbSize.Build.RowHeight;
        private const float HeaderRowHeight = WbbSize.Build.GroupHeaderHeight;
        private const float CardHeight = WbbSize.Build.CardHeight;
        private const float MinCellWidth = WbbSize.Build.MinCellWidth;
        private const float CullPad = 160f;
        private const int MaxColumns = 12;
        private const int TypeCount = 3;
        private const int OutputModeCount = 2;
        private const int ConfirmPreviewCount = 3;
        private const int IssuePreviewCount = 8;
        private const float NarrowWidth = WbbSize.Build.NarrowWidth;
        private const double IndexBudgetSeconds = 0.012;
        private const int ViewGrid = 0;
        private const int ViewList = 1;
        private const int SortName = 0;
        private const int SortDate = 1;
        private const int SortState = 2;
        private const string Pfx = WbbUtil.PrefPrefix + "Build.";
        private const string SceneGroupKey = "\u0001scene";
        private const string ScenePrefix = "scene:";
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

        private enum BadgeState
        {
            Failed = 0,
            SourceChanged = 1,
            MissingOutput = 2,
            Other = 3,
            NotBuilt = 4,
            Built = 5,
        }

        private enum ShowReason
        {
            Pinned,
            WorkFolder,
            Built,
            Physics,
            Raw,
            Hidden,
        }

        private class Item
        {
            public bool isScene;
            public string guid = "";
            public string path = "";
            public string folder = "";
            public string folderName = "";
            public int folderPrefabs;
            public string name = "";
            public string keyName = "";
            public string signature = "";
            public ShowReason reason;
            public bool dimmed;
            public string modName = "";
            public string modNameOverride = "";
            public string manualName = "";
            public bool nameDuplicate;
            public bool nameOverwrites;
            public Texture2D thumb;
            public bool thumbLooked;
            public WbbRecord record;
            public int buildCount;
            public BadgeState state = BadgeState.NotBuilt;
            public bool outputExists;
            public long sortTicks;
            public string sceneKey = "";
            public string sceneLabel = "";
            public int group = -1;
            public string badgeText = "";
            public string badgeGlyph = "";
            public UI.SRGlyph? badgeMark;
            public WbbTone badgeTone = WbbTone.Neutral;
            public GUIContent badgeContent;
            public string subText = "";
            public string dateText = "";
            public string cardTip = "";
        }

        private class Group
        {
            public string key = "";
            public string title = "";
            public string tooltip = "";
            public string detail = "";
            public bool scene;
            public bool marked;
            public bool open = true;
            public long newestTicks;
            public int worstState = (int)BadgeState.Built;
            public readonly List<int> members = new List<int>();
            public int selectable;
            public int selected;
        }

        private const int UnitHeader = 0;
        private const int UnitRow = 1;
        private const int UnitCards = 2;
        private struct Unit
        {
            public int kind;
            public int group;
            public int start;
            public int count;
            public float y;
            public float height;
        }

        private static WbbBuildRunner sharedRunner;
        private readonly WbbWindow host;
        private readonly WbbJob job = new WbbJob();
        private WbbBuildRunner runner;
        private bool subscribed;
        private readonly List<Item> items = new List<Item>();
        private readonly List<int> filtered = new List<int>();
        private readonly List<Group> groups = new List<Group>();
        private readonly List<Unit> units = new List<Unit>();
        private readonly Dictionary<string, int> guidIndex = new Dictionary<string, int>(System.StringComparer.Ordinal);
        private readonly HashSet<string> selected = new HashSet<string>(System.StringComparer.Ordinal);
        private readonly HashSet<string> collapsed = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        private string search = "";
        private int view = ViewList;
        private int sortMode = SortName;
        private bool showHidden;
        private bool settingsOpen;
        private bool advancedOpen;
        private bool narrowLayoutPrev;
        private bool treeVisible = true;
        private bool treeOpenWhileNarrow;
        private string activeJobId = "";
        private string presetNameInput = "";
        private Vector2 scroll;
        private int hoverIndex = -1;
        private string lastClickedGuid = "";
        private string scopeFolder = "Assets";
        private int columns = 1;
        private int firstUnit;
        private int lastUnit = -1;
        private float totalHeight;
        private float viewWidth = 460f;
        private float measuredWidth = 460f;
        private float viewportHeight = 400f;
        private float measuredViewport = 400f;
        private bool busyLayout;
        private bool indexBusyLayout;
        private float indexProgressLayout;
        private bool narrowLayout;
        private bool treeShownLayout;
        private float treeWidthLayout = WbbSize.Build.TreeMinWidth;
        private float paneWidthLayout = 460f;
        private string statusShown = "";
        private MessageType statusShownType = MessageType.Info;
        private bool itemsDirty = true;
        private bool namesDirty = true;
        private bool displayDirty = true;
        private bool filterDirty = true;
        private bool summaryDirty = true;
        private bool preflightDirty = true;
        private int displayLangVersion = -1;
        private int summaryLangVersion = -1;
        private int indexVersionSeen = -1;
        private int listVersionSeen = -1;
        private string countLine = "";
        private string previewLine = "";
        private string preflightLine = "";
        private string hiddenChipText = "";
        private string hiddenChipTip = "";
        private string statusMessage = "";
        private MessageType statusType = MessageType.Info;
        private int selectedCount;
        private int blockingCount;
        private int warningCount;
        private int rawCount;
        private int userHiddenCount;
        private readonly List<WbbPreflightIssue> preflightIssues = new List<WbbPreflightIssue>();
        private bool issuesOpen;
        private readonly List<string> suggestions = new List<string>();
        private int suggestionsIndexVersion = -1;
        private string suggestionLine = "";
        private int emptyKind;
        private int emptyAction;
        private string emptyTitle = "";
        private string emptyMessage = "";
        private string emptyActionLabel = "";
        private bool pendingRescan;
        private bool pendingForceIndex;
        private bool pendingBuild;
        private bool pendingThumbBatch;
        private bool pendingPickOutput;
        private bool pendingPickStaging;
        private bool pendingPackageExport;
        private int pendingOutputMode = -1;
        private string pendingScope;
        private int pendingSettingsOpen = -1;
        private int pendingAdvancedOpen = -1;
        private int pendingView = -1;
        private int pendingShowHidden = -1;
        private string pendingToggleGroup;
        private string pendingGroupSelectKey;
        private bool pendingGroupSelectValue;
        private bool pendingAcceptSuggestions;
        private bool pendingDismissSuggestions;
        private bool pendingNeverSuggest;
        private const string SuggestSkippedKey = Pfx + "SuggestSkipped";
        private string pendingRenameGuid = "";
        private bool pendingRenameAsset;
        private string pendingResetNameGuid = "";
        private string renamingGuid = "";
        private bool renamingAssetName;
        private string renameBuffer = "";
        private bool renameFocusPending;
        private bool pendingSavePreset;
        private bool pendingResult;
        private string pendingThumbGuid = "";
        private string pendingOpenThumbGuid = "";
        private string pendingOpenOutputGuid = "";
        private string pendingHideGuid = "";
        private string pendingUnhideGuid = "";
        private string pendingPinGuid = "";
        private string pendingSceneRebuildGuid = "";
        private string pendingApplyPresetId = "";
        private string pendingDeletePresetId = "";
        private string pendingBuildFolder;
        private string pendingWorkFolderAdd;
        private string pendingWorkFolderRemove;
        private readonly List<string> pendingSelectGuids = new List<string>();
        private readonly List<string> pendingSelectNames = new List<string>();
        private readonly Dictionary<string, string> pendingRebuildModNames = new Dictionary<string, string>();
        private readonly Dictionary<string, string> rebuildModNames = new Dictionary<string, string>();
        private bool pendingSelectReplace;
        private string rebuildToken = "";
        private int rebuildQueuedCount;
        private string dbFingerprint = "";
        private WbbFolderTreePanel folderTree;
        private float treeWidth = WbbSize.Build.TreeWidth;
        private bool draggingSplitter;
        private WbbPackageSection packageSection;
        private string packageTargetsKey;
        private bool resultCanceled;
        private int resultSuccess;
        private int resultFailed;
        private int resultCanceledItems;
        private int resultSkipped;
        private string resultOutputDir = "";
        private bool resultVisible;
        private string resultLabel = "";
        private WbbTone resultTone = WbbTone.Success;
        private int resultLabelVersion = -1;
        private GUIStyle rightMini;
        private GUIStyle midMini;
        private GUIStyle fieldLabel;
        private GUIStyle subLine;
        private bool localStylesDark;
        private readonly GUIContent rowScratch = new GUIContent();
        private GUIContent[] viewContents;
        private GUIContent[] outputModeContents;
        private GUIContent menuButtonContent;
        private GUIContent listMenuContent;
        private GUIContent hideContent;
        private GUIContent unhideContent;
        private GUIContent pinContent;
        private GUIContent thumbCreateContent;
        private GUIContent thumbRegenContent;
        private GUIContent treeToggleContent;
        private GUIContent sceneRebuildContent;
        private GUIContent sceneRebuildIcon;
        private GUIContent runNoneContent;
        private GUIContent runContent;
        private GUIContent resultCloseContent;
        private string[] sortNames;
        private string[] typeNames;
        private string thumbNoneText = "";
        private string[] reasonTexts;
        private int contentVersion = -1;
        private int runContentCount = -1;
        private bool runContentPackage;
        public WbbBuildSection(WbbWindow host)
        {
            this.host = host;
        }

        public bool IsBusy
        {
            get
            {
                return runner != null && runner.IsRunning;
            }
        }

        public void OnEnable()
        {
            WbbDatabase.EnsureLoaded();
            WbbListSettings.EnsureLoaded();
            LoadPrefs();
            if (sharedRunner == null)
                sharedRunner = new WbbBuildRunner();
            runner = sharedRunner;
            EnsureSubscribed();
            if (packageSection == null)
            {
                packageSection = new WbbPackageSection(host);
                packageSection.Embedded = true;
                packageSection.OnEnable();
            }

            itemsDirty = true;
            pendingRescan = true;
            string stopped = WbbRunnerNotice.Peek();
            if (stopped.Length > 0 && string.IsNullOrEmpty(statusMessage) && !IsBusy)
                SetStatus(stopped, MessageType.Warning);
        }

        public void OnDisable()
        {
            SavePrefs();
            Unsubscribe();
            if (packageSection != null)
                packageSection.OnDisable();
            ReleaseKeptThumbs();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].isScene)
                {
                    items[i].thumb = null;
                    items[i].thumbLooked = false;
                }
            }

            pendingResult = false;
            pendingBuild = false;
            pendingThumbBatch = false;
            pendingThumbGuid = "";
        }

        public void Update()
        {
            EnsureSubscribed();
            if (WbbPrefabIndex.Tick(IndexBudgetSeconds) || WbbPrefabIndex.Busy)
                RequestRepaint();
            ProcessDeferredWork();
        }

        private void EnsureSubscribed()
        {
            if (runner == null || subscribed)
                return;
            runner.RecordFinished += OnRecordFinished;
            runner.Finished += OnBuildFinished;
            runner.RepaintRequested += RequestRepaint;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (runner == null || !subscribed)
                return;
            runner.RecordFinished -= OnRecordFinished;
            runner.Finished -= OnBuildFinished;
            runner.RepaintRequested -= RequestRepaint;
            subscribed = false;
        }

        private void RequestRepaint()
        {
            if (host != null)
                host.RequestRepaint();
        }

        private void SetStatus(string message, MessageType type)
        {
            statusMessage = message == null ? "" : message;
            statusType = type;
        }
    }
}
