using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    [InitializeOnLoad]
    public partial class WbbBuildRunner
    {
        public const double OutputGraceSeconds = 10d;
        public const double OutputTimeoutSeconds = OutputGraceSeconds;
        private const double PollIntervalSeconds = 0.5d;
        private const double StepDelaySeconds = 0.3d;
        private const int RequiredStableSamples = 2;
        private enum Step
        {
            Idle,
            Prepare,
            Copy,
            Stage,
            WriteSettings,
            CompileRuntime,
            InvokeBuild,
            WaitOutput,
            Verify,
            Cleanup,
            Record,
            Done,
        }

        private static string L(string key)
        {
            return WbbLang.T(key);
        }

        private static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }

        private static readonly WbbBuildItem[] EmptyItems = new WbbBuildItem[0];
        private static WbbBuildRunner activeRunner;
        public static bool AnyRunning
        {
            get
            {
                return activeRunner != null && activeRunner.running;
            }
        }

        public static bool AbortingSilently { get; private set; }

        static WbbBuildRunner()
        {
            AssemblyReloadEvents.beforeAssemblyReload += AbortActiveForReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void AbortActiveForReload()
        {
            if (activeRunner == null || !activeRunner.running)
                return;
            WbbUtil.LogWarning("Script reload during a build. Stopping the batch and cleaning up first.");
            AbortSilently(L("Runner.AbortReload"));
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;
            if (activeRunner == null || !activeRunner.running)
                return;
            WbbUtil.LogWarning("Entering Play Mode during a build. Stopping the batch and cleaning up first.");
            AbortSilently(L("Runner.AbortPlayMode"));
        }

        private static void AbortSilently(string reason)
        {
            AbortingSilently = true;
            try
            {
                activeRunner.AbortAndRestore(reason);
            }
            catch (System.Exception e)
            {
                WbbUtil.LogError("Abort before reload/play mode failed: " + e.Message);
            }
            finally
            {
                AbortingSilently = false;
            }
        }

        public event System.Action<WbbRecord> RecordFinished;
        public event System.Action<bool> Finished;
        public event System.Action RepaintRequested;
        private IList<WbbBuildItem> items;
        private WbbJob job;
        private Step step = Step.Idle;
        private bool running;
        private bool cancelRequested;
        private bool advancing;
        private bool updateHooked;
        private double waitUntil;
        private int index = -1;
        private string modFolder = "";
        private bool usingUserScriptFolder;
        private string tempRoot = "";
        private string outputDirectory = "";
        private string typeName = "Prop";
        private string modVersion = "1.0.0";
        private WbbFixupOptions fixupOptions = new WbbFixupOptions();
        private bool expectScripts;
        private SceneSetup[] sceneSetup;
        private WbbProfileSnapshot originalProfile;
        private int profileIndex = -1;
        private bool profileWritten;
        private List<string> originalReferencePaths;
        private bool referencePathsCleared;
        private WbbUModQuietState originalQuiet;
        private bool quietApplied;
        private int successCount;
        private int failedCount;
        private int skippedCount;
        private string statusMessage = "";
        private string currentModName = "";
        private bool itemActive;
        private string originalPrefabPath = "";
        private string copyPath = "";
        private string copyGuid = "";
        private bool restoreFailed;
        private string sourceSignature = "";
        private string outputFilePath = "";
        private long outputSizeBytes;
        private double itemStartTime;
        private string itemStartIso = "";
        private string itemRecordId = "";
        private string itemFixupReport = "";
        private string itemBuildLogPath = "";
        private string securityHeader = "";
        private List<string> securityEntries;
        private System.DateTime buildStartUtc = System.DateTime.MinValue;
        private double waitStartTime;
        private long lastSize = -1L;
        private int stableSamples;
        private WbbLogCapture capture;
        private WbbBuildInvokeResult invokeResult;
        private WbbRuntimeCompileSession compileSession;
        private WbbAsmdefAnalysis folderAnalysis;
        private List<WbbRuntimeUnit> itemUnits = new List<WbbRuntimeUnit>();
        private List<WbbCompiledUnit> itemCompiled = new List<WbbCompiledUnit>();
        private WbbRuntimeScriptPlan autoPlan;
        private string autoPlanError = "";
        private bool compileStarted;
        private string abortReason = "";
        private bool lastFinishClean = true;
        private WbbJournalOutput outputGuard;
        public bool IsRunning
        {
            get
            {
                return running;
            }
        }

        public bool CancelRequested
        {
            get
            {
                return cancelRequested;
            }
        }

        public IList<WbbBuildItem> Items
        {
            get
            {
                return items != null ? items : (IList<WbbBuildItem>)EmptyItems;
            }
        }

        public int TotalCount
        {
            get
            {
                return items == null ? 0 : items.Count;
            }
        }

        public int FinishedCount
        {
            get
            {
                return successCount + failedCount + skippedCount;
            }
        }

        public int SuccessCount
        {
            get
            {
                return successCount;
            }
        }

        public int FailedCount
        {
            get
            {
                return failedCount;
            }
        }

        public float Progress
        {
            get
            {
                int total = TotalCount;
                if (total <= 0)
                    return 0f;
                return Mathf.Clamp01((float)FinishedCount / total);
            }
        }

        public string StatusMessage
        {
            get
            {
                return statusMessage;
            }
        }

        public string CurrentModName
        {
            get
            {
                return currentModName;
            }
        }

        public bool Start(IList<WbbBuildItem> newItems, WbbJob newJob)
        {
            if (running)
                return false;
            if (activeRunner != null && activeRunner != this && activeRunner.running)
            {
                SetStatus(L("Runner.OtherBuildRunning"));
                return false;
            }

            if (newItems == null || newItems.Count == 0)
            {
                SetStatus(L("Runner.NoSelection"));
                return false;
            }

            if (newJob == null)
            {
                SetStatus(L("Runner.NoBuildFolder"));
                return false;
            }

            string recoveryError = "";
            try
            {
                WbbJournal.RecoverIfNeeded();
            }
            catch (System.Exception e)
            {
                recoveryError = e.Message;
                WbbUtil.LogWarning("Leftover build cleanup failed: " + e.Message);
            }

            // Begin overwrites the journal file. That is only safe once recovery has read it: a journal whose settings could
            // not be written back, or one recovery failed on, still holds the only copy of the original settings. A journal
            // left only because deleting it failed after a full recovery holds nothing that is still needed.
            if (WbbJournal.SettingsRestorePending)
            {
                SetStatus(L("Safety.JournalBlocksStart"));
                return false;
            }

            if (recoveryError.Length > 0 && WbbJournal.Exists && WbbJournal.Current == null)
            {
                SetStatus(L("Safety.JournalRecoveryError", recoveryError));
                return false;
            }

            string setAside = WbbJournal.UnacknowledgedSetAside;
            if (setAside.Length > 0)
            {
                SetStatus(L("Safety.JournalSetAsidePending", setAside) + " " + L("Safety.JournalSetAsidePending.hint"));
                return false;
            }

            WbbRunnerNotice.Clear();

            string blocker = CheckStartBlockers(newJob);
            if (blocker.Length > 0)
            {
                SetStatus(blocker);
                return false;
            }

            if (!PrepareBatch(newItems, newJob))
                return false;
            successCount = 0;
            failedCount = 0;
            skippedCount = 0;
            index = 0;
            itemActive = false;
            cancelRequested = false;
            running = true;
            activeRunner = this;
            currentModName = "";
            ResetItemState();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null)
                    continue;
                item.status = WbbBuildStatus.Pending;
                item.message = "";
                item.durationSec = 0d;
            }

            step = Step.Prepare;
            waitUntil = 0d;
            HookUpdate(true);
            SetStatus(L("Runner.PreparingBatch"));
            WbbUtil.Log("Batch build started: " + items.Count + " item(s), type=" + typeName + ", output=" + outputDirectory + ", scripts=" + (usingUserScriptFolder ? modFolder : "(temporary)") + ", pipeline=" + WbbUtil.PipelineLogText());
            RequestRepaint();
            return true;
        }

        public void Cancel()
        {
            if (!running || cancelRequested)
                return;
            cancelRequested = true;
            SetStatus(L("Runner.CancelRequested"));
            RequestRepaint();
        }

        public void Tick()
        {
            if (updateHooked)
                return;
            Step1();
        }

        public void Update()
        {
            Tick();
        }

        private void HookUpdate(bool on)
        {
            if (on == updateHooked)
                return;
            if (on)
                EditorApplication.update += OnEditorUpdate;
            else
                EditorApplication.update -= OnEditorUpdate;
            updateHooked = on;
        }

        private void OnEditorUpdate()
        {
            if (!running)
            {
                HookUpdate(false);
                return;
            }

            Step1();
        }

        private void Step1()
        {
            if (!running || advancing)
                return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (EditorApplication.timeSinceStartup < waitUntil)
                return;
            advancing = true;
            try
            {
                Advance();
            }
            catch (System.Exception e)
            {
                HandleTickException(e);
            }
            finally
            {
                advancing = false;
            }
        }

        private void Advance()
        {
            if (cancelRequested && IsCancelSafeStep(step))
            {
                if (itemActive)
                {
                    MarkCurrent(WbbBuildStatus.Canceled, L("Runner.CanceledItem"));
                    step = Step.Cleanup;
                }
                else
                {
                    step = Step.Done;
                }

                RequestRepaint();
                return;
            }

            switch (step)
            {
                case Step.Prepare:
                    DoPrepare();
                    break;
                case Step.Copy:
                    DoCopy();
                    break;
                case Step.Stage:
                    DoStage();
                    break;
                case Step.WriteSettings:
                    DoWriteSettings();
                    break;
                case Step.CompileRuntime:
                    DoCompileRuntime();
                    break;
                case Step.InvokeBuild:
                    DoInvokeBuild();
                    break;
                case Step.WaitOutput:
                    DoWaitOutput();
                    break;
                case Step.Verify:
                    DoVerify();
                    break;
                case Step.Cleanup:
                    DoCleanup();
                    break;
                case Step.Record:
                    DoRecord();
                    break;
                case Step.Done:
                    FinishBatch(cancelRequested);
                    break;
                default:
                    running = false;
                    step = Step.Idle;
                    HookUpdate(false);
                    break;
            }

            RequestRepaint();
        }

        private static bool IsCancelSafeStep(Step value)
        {
            return value == Step.Prepare || value == Step.Copy || value == Step.Stage || value == Step.WriteSettings || value == Step.CompileRuntime || value == Step.InvokeBuild;
        }

        private void HandleTickException(System.Exception e)
        {
            WbbUtil.LogError("Build runner failed at step " + step + ": " + e.Message + "\n" + e.StackTrace);
            try
            {
                if (step == Step.Cleanup)
                {
                    FlagCleanupFailure(copyPath);
                    step = Step.Record;
                }
                else if (step == Step.Record || step == Step.Done)
                {
                    FinishBatch(cancelRequested);
                }
                else
                {
                    Fail(L("Runner.ErrorException", e.Message));
                    step = Step.Cleanup;
                }
            }
            catch (System.Exception inner)
            {
                WbbUtil.LogError("Recovery failed, stopping runner: " + inner.Message);
                EmergencyStop(L("Runner.ErrorException", inner.Message));
            }

            Delay(StepDelaySeconds);
            RequestRepaint();
        }
    }
}
