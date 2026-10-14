// ri-5b7581f7 auto review r1, step 2 of 3. lab_exec_file statement body, QA project only. Before it, open the Mod Mate
// window (Build tab, one prefab selected, check results list open), press [Set record aside] next to the journal problem and
// confirm the dialog: U1 checks what that left. After it, press [Checked] in the same list, then run 3-cleanup.cs.
// Every Start uses an Environment job, so a gate that wrongly lets the build through still stops at CheckStartBlockers
// (status Safety.EnvironmentUnsupported) and never builds.
const string QaRoot = ""; // PARAMETER: the QA project root (Unity 2021.3.45f2 with the WARUDO SDK and this branch installed)
const string StateFile = ""; // PARAMETER: a writable file outside the project, e.g. <temp>/wmm-5b-state.txt
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(StateFile)) return "ABORT set the PARAMETER constants at the top first";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready";
if (!System.IO.File.Exists(StateFile)) return "ABORT run 1-pending.cs first";

string lib = StudioRaming.WarudoBatchBuilder.WbbBuildLog.LibraryDirectory;
string ackKey = StudioRaming.WarudoBatchBuilder.WbbUtil.PrefPrefix + "Journal.SetAsideAcknowledged";
string journalPath = StudioRaming.WarudoBatchBuilder.WbbJournal.JournalPath;
var sb = new System.Text.StringBuilder();
int fails = 0;
System.Action<string, bool> check = (label, ok) => { sb.AppendLine((ok ? "PASS " : "FAIL ") + label); if (!ok) fails++; };
var items = new System.Collections.Generic.List<StudioRaming.WarudoBatchBuilder.WbbBuildItem> { new StudioRaming.WarudoBatchBuilder.WbbBuildItem() };
var envJob = new StudioRaming.WarudoBatchBuilder.WbbJob { exportType = (int)StudioRaming.WarudoBatchBuilder.WbbExportType.Environment };
string envStop = StudioRaming.WarudoBatchBuilder.WbbLang.T("Safety.EnvironmentUnsupported");
System.Func<string> start = () =>
{
    var runner = new StudioRaming.WarudoBatchBuilder.WbbBuildRunner();
    bool started = runner.Start(items, envJob);
    if (started) { runner.Cancel(); return "STARTED(" + runner.StatusMessage + ")"; }
    return runner.StatusMessage;
};
System.Func<string, bool> preflightHas = code =>
{
    foreach (var issue in StudioRaming.WarudoBatchBuilder.WbbBuildRunner.Preflight(items, new StudioRaming.WarudoBatchBuilder.WbbJob()))
    {
        var ex = issue as StudioRaming.WarudoBatchBuilder.WbbPreflightIssueEx;
        if (ex != null && ex.code == code && ex.blocking) return true;
    }
    return false;
};
System.Action<int, string> writeJournal = (attempts, session) =>
{
    var d = new StudioRaming.WarudoBatchBuilder.WbbJournalData { batchId = "wmm5b", startedIso = "2026-10-12T00:00:00", profileCaptured = true, profileWritten = true, profileIndex = 999, recoveryAttempts = attempts, recoverySession = session };
    System.IO.File.WriteAllText(journalPath, UnityEngine.JsonUtility.ToJson(d, true), new System.Text.UTF8Encoding(false));
};

try
{
    // Finding 1: a journal whose settings cannot be written back blocks Start, and the user can set it aside.
    // The UI button already set the 5b-1 journal aside (Set record aside, then the dialog's confirm button).
    check("U1 the UI set-aside left no journal and acknowledged itself", !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists && StudioRaming.WarudoBatchBuilder.WbbJournal.UnacknowledgedSetAside == "" && System.IO.Directory.GetFiles(lib, "BuildJournal.json.unrecovered-*").Length > 0);
    writeJournal(0, "");
    StudioRaming.WarudoBatchBuilder.WbbJournal.RecoverIfNeeded();
    check("A4 a new pending journal", StudioRaming.WarudoBatchBuilder.WbbJournal.SettingsRestorePending);
    string s = start();
    check("A5 Start blocked by JournalBlocksStart: " + s, s == StudioRaming.WarudoBatchBuilder.WbbLang.T("Safety.JournalBlocksStart"));
    string name = StudioRaming.WarudoBatchBuilder.WbbJournal.SetAsidePending("wmm5b test");
    check("A6 SetAsidePending moved it: '" + name + "'", name.StartsWith("BuildJournal.json.unrecovered-") && System.IO.File.Exists(lib + "/" + name));
    check("A7 journal gone after set-aside", !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists && !StudioRaming.WarudoBatchBuilder.WbbJournal.SettingsRestorePending);
    check("A8 user set-aside counts as acknowledged", StudioRaming.WarudoBatchBuilder.WbbJournal.UnacknowledgedSetAside == "");
    check("A9 preflight has no journal block", !preflightHas("journal-restore-failed") && !preflightHas("journal-set-aside"));
    s = start();
    check("A10 Start passes the journal gates: " + s, s == envStop);
    check("A11 SetAsidePending with nothing pending returns empty", StudioRaming.WarudoBatchBuilder.WbbJournal.SetAsidePending("wmm5b test") == "");

    // Finding 3: the attempt cap sets the record aside and builds stay blocked until acknowledged.
    System.Threading.Thread.Sleep(1100);
    writeJournal(2, "an-earlier-session");
    StudioRaming.WarudoBatchBuilder.WbbJournal.RecoverIfNeeded();
    string capped = StudioRaming.WarudoBatchBuilder.WbbJournal.UnacknowledgedSetAside;
    check("D1 cap moved the journal aside", !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists && capped.Length > 0 && string.CompareOrdinal(capped, name) > 0);
    check("D2 preflight blocks with journal-set-aside", preflightHas("journal-set-aside"));
    s = start();
    check("D3 Start blocked by JournalSetAsidePending: " + s, s.StartsWith(StudioRaming.WarudoBatchBuilder.WbbLang.T("Safety.JournalSetAsidePending", capped)) && s.Contains(StudioRaming.WarudoBatchBuilder.WbbLang.T("Safety.JournalSetAsideAck")));
    StudioRaming.WarudoBatchBuilder.WbbJournal.AcknowledgeSetAside();
    check("D4 acknowledged", StudioRaming.WarudoBatchBuilder.WbbJournal.UnacknowledgedSetAside == "" && !preflightHas("journal-set-aside"));
    s = start();
    check("D5 Start passes after acknowledging: " + s, s == envStop);
    System.Threading.Thread.Sleep(1100);
    writeJournal(2, "an-earlier-session");
    StudioRaming.WarudoBatchBuilder.WbbJournal.RecoverIfNeeded();
    string later = StudioRaming.WarudoBatchBuilder.WbbJournal.UnacknowledgedSetAside;
    check("D6 a newer set-aside blocks again after an older one was acknowledged: '" + later + "'", later.Length > 0 && string.CompareOrdinal(later, capped) > 0 && preflightHas("journal-set-aside"));
    StudioRaming.WarudoBatchBuilder.WbbJournal.AcknowledgeSetAside();

    // Finding 2: a journal that recovery fully handled but could not delete no longer blocks Start.
    var clean = new StudioRaming.WarudoBatchBuilder.WbbJournalData { batchId = "wmm5b-C", startedIso = "2026-10-12T00:00:00" };
    System.IO.File.WriteAllText(journalPath, UnityEngine.JsonUtility.ToJson(clean, true), new System.Text.UTF8Encoding(false));
    using (var hold = new System.IO.FileStream(journalPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
    {
        s = start();
        check("C1 journal still there (delete blocked by the open handle)", StudioRaming.WarudoBatchBuilder.WbbJournal.Exists);
        check("C2 Start passes the journal gates with an undeletable recovered journal: " + s, s == envStop);
    }
    StudioRaming.WarudoBatchBuilder.WbbJournal.Complete();
    check("C3 journal removed once the handle closed", !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists);

    // Leave one unacknowledged set-aside for the [Checked] button capture; 3-cleanup.cs undoes everything.
    System.Threading.Thread.Sleep(1100);
    writeJournal(2, "an-earlier-session");
    StudioRaming.WarudoBatchBuilder.WbbJournal.RecoverIfNeeded();
    check("E0 an unacknowledged set-aside is left for the UI: '" + StudioRaming.WarudoBatchBuilder.WbbJournal.UnacknowledgedSetAside + "'", StudioRaming.WarudoBatchBuilder.WbbJournal.UnacknowledgedSetAside.Length > 0);
}
catch (System.Exception e)
{
    fails++;
    sb.AppendLine("FAIL exception " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
}
sb.AppendLine(fails == 0 ? "RESULT PASS" : "RESULT FAIL " + fails);
return sb.ToString();
