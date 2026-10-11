using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
        private const float EntryHeight = WbbSize.History.RowHeight;
        private const float CullPad = 90f;
        private const int TypeCount = 3;
        private const int StatusCount = 3;
        private const int CleanDays = 90;
        private const int GroupByDate = 0;
        private const int GroupByJob = 1;
        private const int GroupFlat = 2;
        private const string Pfx = WbbUtil.PrefPrefix + "History.";
        // The confirm dialogs of delete and rebuild. Internal so a verification script can read the text without a click.
        internal static System.Func<string, string, string, string, bool> ConfirmDialog = EditorUtility.DisplayDialog;
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

        private class Row
        {
            public WbbRecord record;
            public string dateKey = "";
            public string jobKey = "";
            public string keyName = "";
            public string keyType = "";
            public string keyVersion = "";
            public string keyAuthor = "";
            public string keyJob = "";
            public string keyPrefab = "";
            public int statusBucket;
            public int fileState;
            public bool hasFixups;
            public string nameText = "";
            public string typeText = "";
            public string versionText = "";
            public string authorText = "";
            public string durationText = "";
            public string sizeText = "";
            public string timeText = "";
            public string statusText = "";
            public WbbTone statusTone;
            public string fileText = "";
        }

        private struct Entry
        {
            public bool header;
            public string groupKey;
            public string text;
            public int row;
        }

        private readonly WbbWindow host;
        private readonly List<Row> rows = new List<Row>();
        private readonly List<Entry> entries = new List<Entry>();
        private readonly HashSet<string> selected = new HashSet<string>(System.StringComparer.Ordinal);
        private readonly HashSet<string> collapsed = new HashSet<string>(System.StringComparer.Ordinal);
        private readonly Dictionary<string, bool> fileExists = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
        private readonly bool[] typeEnabled = new bool[TypeCount];
        private readonly bool[] statusEnabled = new bool[StatusCount];
        private string search = "";
        private int groupMode = GroupByDate;
        private Vector2 scroll;
        private int hoverIndex = -1;
        private string lastClickedId = "";
        private int firstVisible;
        private int lastVisible = -1;
        private bool filterDirty = true;
        private bool displayDirty = true;
        private int displayLangVersion = -1;
        private int dbCount = -1;
        private string dbTopId = "";
        private int totalCount;
        private int successCount;
        private int failedCount;
        private int otherCount;
        private int oldCount;
        private long totalSize;
        private string lastBuildRaw = "";
        private GUIContent statsContent = new GUIContent();
        private GUIContent statsExtraContent = new GUIContent();
        private string filteredText = "";
        private bool showAuthorColumn;
        private bool showFileColumn;
        private int failedInView;
        private string fileMissingText = "";
        private bool selectionDirty = true;
        private int selectionVersion = -1;
        private int selectionCount;
        private string selectionText = "";
        private GUIContent rebuildSelectedContent;
        private GUIContent rebuildFailedContent;
        private GUIContent deleteSelectedContent;
        private GUIContent cleanOldContent;
        private GUIContent clearAllContent;
        private int buttonsFailed = -1;
        private int buttonsOld = -1;
        private int buttonsRows = -1;
        private int buttonsSelected = -1;
        private int buttonsLangVersion = -1;
        private string statusLineText = "";
        private WbbTone statusLineTone = WbbTone.Info;
        private bool statusLinePending;
        private string pendingStatusLineText = "";
        private WbbTone pendingStatusLineTone = WbbTone.Info;
        private bool pendingForceRefresh;
        private int hiddenSelectedCount;
        private const double RefreshMinSec = 3.0;
        private double lastRefreshTime = -999d;
        private bool pendingExportCsv;
        private bool pendingRebuildSelected;
        private bool pendingRebuildFailed;
        private bool pendingDeleteSelected;
        private bool pendingCleanOld;
        private bool pendingClearAll;
        private bool pendingGoToBuild;
        private string pendingDeleteId = "";
        private string pendingStrandedPing = "";
        private static string dismissedRecoveryKey;
        private bool showRecovery;
        private string recoveryText = "";
        private readonly List<WbbStrandedOriginal> stranded = new List<WbbStrandedOriginal>();
        private bool strandedDirty = true;
        private bool strandedOpen;
        private bool strandedOpenNext;
        private bool strandedDismissed;
        private bool strandedDismissNext;
        private int strandedLangVersion = -1;
        private string strandedText = "";
        private GUIContent strandedListContent;
        private GUIContent[] strandedItemContents = new GUIContent[0];
        private string fixupBadgeText = "";
        private float fixupBadgeWidth;
        private readonly GUIContent fixupTipScratch = new GUIContent();
        private GUIStyle rightMini;
        private GUIStyle midMini;
        private bool localStylesDark;
        private readonly GUIContent rowScratch = new GUIContent();
        private GUIContent[] typeToggleContents;
        private GUIContent[] statusToggleContents;
        private GUIContent typeLabelContent;
        private GUIContent groupLabelContent;
        private GUIContent statusLabelContent;
        private float filterLabelWidth;
        private float groupLabelWidth;
        private float filterChipWidth;
        private GUIContent searchHelpContent;
        private GUIContent menuButtonContent;
        private string[] groupNames;
        private int contentVersion = -1;
        public WbbHistorySection(WbbWindow host)
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
                typeEnabled[t] = WbbPrefs.GetBool(Pfx + "Type" + t, true);
            for (int s = 0; s < StatusCount; s++)
                statusEnabled[s] = WbbPrefs.GetBool(Pfx + "State" + s, true);
            groupMode = Mathf.Clamp(WbbPrefs.GetInt(Pfx + "Group", GroupByDate), 0, 2);
            search = "";
            RebuildRows(true);
        }

        public void OnDisable()
        {
            for (int t = 0; t < TypeCount; t++)
                WbbPrefs.SetBool(Pfx + "Type" + t, typeEnabled[t]);
            for (int s = 0; s < StatusCount; s++)
                WbbPrefs.SetBool(Pfx + "State" + s, statusEnabled[s]);
            WbbPrefs.SetInt(Pfx + "Group", groupMode);
        }

        // Re-checks the output files on tab entry and focus; the minimum gap keeps a big history from rescanning on every click.
        public void Refresh()
        {
            if (EditorApplication.timeSinceStartup - lastRefreshTime < RefreshMinSec)
                return;
            pendingForceRefresh = true;
            if (host != null)
                host.RequestRepaint();
        }

        private void SetStatus(string text, WbbTone tone)
        {
            pendingStatusLineText = text ?? "";
            pendingStatusLineTone = tone;
            statusLinePending = true;
            if (host != null)
                host.RequestRepaint();
        }
    }
}
