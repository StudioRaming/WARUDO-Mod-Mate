using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbLibrarySection
    {
        private const float RowHeight = WbbSize.Library.RowHeight;
        private const float CullPad = 90f;
        private const double FocusRescanMinSec = 3.0;
        private const double PathEditDelaySec = 0.55;
        private const int DeletePreviewCount = 6;
        private const int TypeCount = 3;
        private const string Pfx = WbbUtil.PrefPrefix + "Library.";
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

        private enum PathState
        {
            Ok = 0,
            Empty = 1,
            Missing = 2,
            Error = 3,
        }

        private class Row
        {
            public WbbModFile file;
            public WbbRecord record;
            public string keyName = "";
            public string keyFolder = "";
            public string keyExt = "";
            public string keyAuthor = "";
            public string keyVersion = "";
            public string keyType = "";
            public bool tracked;
            public string nameText = "";
            public string versionText = "";
            public string sizeText = "";
            public string dateText = "";
            public string badgeText = "";
            public Color badgeColor;
            public GUIContent badgeContent;
        }

        private readonly WbbWindow host;
        private readonly string[] paths = new string[TypeCount];
        private readonly bool[] typeEnabled = new bool[TypeCount];
        private readonly PathState[] pathStates = new PathState[TypeCount];
        private readonly string[] pathErrors = new string[TypeCount];
        private readonly int[] typeCounts = new int[TypeCount];
        private readonly long[] typeSizes = new long[TypeCount];
        private readonly List<Row> rows = new List<Row>();
        private readonly List<int> filtered = new List<int>();
        private readonly HashSet<string> selected = new HashSet<string>();
        private string search = "";
        private int sortMode;
        private bool pathsOpen;
        private Vector2 scroll;
        private int hoverIndex = -1;
        private string lastClickedPath = "";
        private int firstVisible;
        private int lastVisible = -1;
        private bool filterDirty = true;
        private bool displayDirty = true;
        private int displayLangVersion = -1;
        private double lastScanTime = -999d;
        private int trackedCount;
        private int untrackedCount;
        private string summaryLine = "";
        private string filteredText = "";
        private readonly float[] chipWidths = new float[TypeCount];
        private readonly float[] chipGaps = new float[TypeCount];
        private readonly bool[] chipBreak = new bool[TypeCount];
        private bool selectionDirty = true;
        private int selectionVersion = -1;
        private string selectionText = "";
        private int selectionCount;
        private int selectionTrackedCount;
        private GUIContent rebuildButtonContent;
        private GUIContent deleteButtonContent;
        private string statusText = "";
        private WbbTone statusTone = WbbTone.Info;
        private bool statusPending;
        private string pendingStatusText = "";
        private WbbTone pendingStatusTone = WbbTone.Info;
        private int hiddenSelectedCount;
        private bool pendingForceRescan;
        private bool pendingDelete;
        private string pendingDeletePath = "";
        private bool pendingRebuild;
        private bool pendingGoToBuild;
        private int pendingBrowseType = -1;
        private int pendingPickedType = -1;
        private string pendingPickedPath = "";
        private int pendingPathsOpen = -1;
        private double pathEditedAt = -1d;
        private GUIStyle rightMini;
        private GUIStyle midMini;
        private bool localStylesDark;
        private readonly GUIContent rowScratch = new GUIContent();
        private GUIContent[] typeToggleContents;
        private GUIContent sortLabelContent;
        private float sortLabelWidth;
        private GUIContent searchHelpContent;
        private GUIContent menuButtonContent;
        private string[] sortNames;
        private int contentVersion = -1;
        public WbbLibrarySection(WbbWindow host)
        {
            this.host = host;
        }

        public bool IsBusy
        {
            get
            {
                return false;
            }
        }

        public void OnEnable()
        {
            WbbDatabase.EnsureLoaded();
            for (int t = 0; t < TypeCount; t++)
            {
                var type = (WbbExportType)t;
                paths[t] = EditorPrefs.GetString(OutputDirKey(type), WbbUtil.DefaultOutputPath(type));
                typeEnabled[t] = EditorPrefs.GetBool(Pfx + "Type" + t, true);
                pathErrors[t] = "";
            }

            sortMode = Mathf.Clamp(EditorPrefs.GetInt(Pfx + "Sort", 0), 0, 2);
            pathsOpen = EditorPrefs.GetBool(Pfx + "PathsOpen", false);
            RescanNow();
        }

        public void OnDisable()
        {
            for (int t = 0; t < TypeCount; t++)
                EditorPrefs.SetBool(Pfx + "Type" + t, typeEnabled[t]);
            EditorPrefs.SetInt(Pfx + "Sort", sortMode);
            EditorPrefs.SetBool(Pfx + "PathsOpen", pathsOpen);
        }

        public void Refresh()
        {
            if (EditorApplication.timeSinceStartup - lastScanTime < FocusRescanMinSec)
                return;
            pendingForceRescan = true;
            if (host != null)
                host.RequestRepaint();
        }

        private void SetStatus(string text, WbbTone tone)
        {
            pendingStatusText = text ?? "";
            pendingStatusTone = tone;
            statusPending = true;
            if (host != null)
                host.RequestRepaint();
        }

        public void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
                SyncStructure();
            EnsureLocalStyles();
            EnsureContents();
            DrawPathsCard();
            DrawTypeFilter();
            DrawSummary();
            DrawToolbar();
            DrawList();
            DrawBottomBar();
            ProcessPending();
        }
    }
}
