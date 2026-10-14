// ri-5b7581f7 auto review r2, self-contained. lab_exec_file statement body, QA project only.
// The settings restore fails AND saving the kept journal fails (an open handle blocks File.Replace), so the file on disk
// still says recoveryAttempts 0. Start and Preflight must block anyway, and the in-memory flag must clear once the
// journal is completed or set aside. Every Start uses an Environment job, so a gate that wrongly lets the build through
// still stops at CheckStartBlockers (status Safety.EnvironmentUnsupported) and never builds. The finally block moves the
// files the test made to ArtifactDir (not deleted) and puts the acknowledgement pref back.
const string QaRoot = ""; // PARAMETER: the QA project root (Unity 2021.3.45f2 with the WARUDO SDK and this branch installed)
const string ArtifactDir = ""; // PARAMETER: a folder outside the project that receives the test files (moved, not deleted)
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(ArtifactDir)) return "ABORT set the PARAMETER constants at the top first";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready";
if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "ABORT a build is running";
if (StudioRaming.WarudoBatchBuilder.WbbJournal.Exists) return "ABORT a build journal already exists: " + StudioRaming.WarudoBatchBuilder.WbbJournal.JournalPath;

string lib = StudioRaming.WarudoBatchBuilder.WbbBuildLog.LibraryDirectory;
string ackKey = StudioRaming.WarudoBatchBuilder.WbbUtil.PrefPrefix + "Journal.SetAsideAcknowledged";
string journalPath = StudioRaming.WarudoBatchBuilder.WbbJournal.JournalPath;
bool ackHad = StudioRaming.WarudoBatchBuilder.WbbPrefs.HasKey(ackKey);
string ackValue = StudioRaming.WarudoBatchBuilder.WbbPrefs.GetString(ackKey, "", false);
var before = new System.Collections.Generic.HashSet<string>();
foreach (string f in System.IO.Directory.GetFiles(lib, "BuildJournal.json.unrecovered-*"))
    before.Add(System.IO.Path.GetFileName(f));

var sb = new System.Text.StringBuilder();
int fails = 0;
System.Action<string, bool> check = (label, ok) => { sb.AppendLine((ok ? "PASS " : "FAIL ") + label); if (!ok) fails++; };
var items = new System.Collections.Generic.List<StudioRaming.WarudoBatchBuilder.WbbBuildItem> { new StudioRaming.WarudoBatchBuilder.WbbBuildItem() };
var envJob = new StudioRaming.WarudoBatchBuilder.WbbJob { exportType = (int)StudioRaming.WarudoBatchBuilder.WbbExportType.Environment };
string envStop = StudioRaming.WarudoBatchBuilder.WbbLang.T("Safety.EnvironmentUnsupported");
string blocks = StudioRaming.WarudoBatchBuilder.WbbLang.T("Safety.JournalBlocksStart");
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
System.Action<string, bool> writeJournal = (batch, restorable) =>
{
    var d = new StudioRaming.WarudoBatchBuilder.WbbJournalData { batchId = batch, startedIso = "2026-10-12T00:00:00" };
    if (!restorable) { d.profileCaptured = true; d.profileWritten = true; d.profileIndex = 999; }
    System.IO.File.WriteAllText(journalPath, UnityEngine.JsonUtility.ToJson(d, true), new System.Text.UTF8Encoding(false));
};
System.Func<string> attemptsOnDisk = () =>
{
    string t = System.IO.File.ReadAllText(journalPath);
    int i = t.IndexOf("\"recoveryAttempts\"");
    return i < 0 ? "(none)" : t.Substring(i, 21).Trim();
};

System.IO.FileStream hold = null;
try
{
    // W: restore fails and the kept journal cannot be saved.
    writeJournal("wmm5b-W", false);
    hold = new System.IO.FileStream(journalPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
    StudioRaming.WarudoBatchBuilder.WbbJournal.RecoverIfNeeded();
    check("W1 journal kept", StudioRaming.WarudoBatchBuilder.WbbJournal.Exists);
    string disk = attemptsOnDisk();
    check("W2 save was blocked, disk still says 0: " + disk, disk == "\"recoveryAttempts\": 0");
    check("W3 SettingsRestorePending true from memory", StudioRaming.WarudoBatchBuilder.WbbJournal.SettingsRestorePending);
    string s = start();
    check("W4 Start blocked by JournalBlocksStart: " + s, s == blocks);
    check("W5 preflight blocks with journal-restore-failed", preflightHas("journal-restore-failed"));
    check("W6 disk still says 0 after Start's retry: " + attemptsOnDisk(), attemptsOnDisk() == "\"recoveryAttempts\": 0");
    hold.Dispose();
    hold = null;

    // The lock goes away: the next recovery saves the count and the gate keeps holding.
    StudioRaming.WarudoBatchBuilder.WbbJournal.RecoverIfNeeded();
    disk = attemptsOnDisk();
    check("W7 count saved once the handle closed: " + disk, disk == "\"recoveryAttempts\": 1");
    check("W8 still pending", StudioRaming.WarudoBatchBuilder.WbbJournal.SettingsRestorePending);
    string name = StudioRaming.WarudoBatchBuilder.WbbJournal.SetAsidePending("wmm5b r2 test");
    check("W9 set aside: '" + name + "'", name.StartsWith("BuildJournal.json.unrecovered-") && !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists);
    check("W10 flag cleared by the set-aside", !StudioRaming.WarudoBatchBuilder.WbbJournal.SettingsRestorePending);
    s = start();
    check("W11 Start passes the journal gates: " + s, s == envStop);

    // X: the flag also clears on Complete, so a later fully recoverable journal does not block.
    writeJournal("wmm5b-X", false);
    hold = new System.IO.FileStream(journalPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
    StudioRaming.WarudoBatchBuilder.WbbJournal.RecoverIfNeeded();
    check("X1 pending from memory again", StudioRaming.WarudoBatchBuilder.WbbJournal.SettingsRestorePending && attemptsOnDisk() == "\"recoveryAttempts\": 0");
    hold.Dispose();
    hold = null;
    StudioRaming.WarudoBatchBuilder.WbbJournal.Complete();
    check("X2 Complete removed it and cleared the flag", !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists && !StudioRaming.WarudoBatchBuilder.WbbJournal.SettingsRestorePending);
    System.Threading.Thread.Sleep(1100);
    writeJournal("wmm5b-X2", true);
    check("X3 a fresh journal with nothing to restore is not pending", !StudioRaming.WarudoBatchBuilder.WbbJournal.SettingsRestorePending);
    s = start();
    check("X4 Start recovers it and passes the journal gates: " + s, s == envStop && !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists);
}
catch (System.Exception e)
{
    fails++;
    sb.AppendLine("FAIL exception " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
}
finally
{
    if (hold != null) hold.Dispose();
    System.IO.Directory.CreateDirectory(ArtifactDir);
    foreach (string f in System.IO.Directory.GetFiles(lib, "BuildJournal.json.unrecovered-*"))
    {
        string n = System.IO.Path.GetFileName(f);
        if (before.Contains(n)) continue;
        System.IO.File.Move(f, ArtifactDir + "/" + n);
        sb.AppendLine("cleanup moved " + n + " to ArtifactDir");
    }
    if (System.IO.File.Exists(journalPath))
    {
        System.IO.File.Move(journalPath, ArtifactDir + "/BuildJournal.json.left-" + System.DateTime.Now.ToString("HHmmss"));
        sb.AppendLine("cleanup moved a leftover journal to ArtifactDir");
    }
    if (ackHad) StudioRaming.WarudoBatchBuilder.WbbPrefs.SetString(ackKey, ackValue);
    else StudioRaming.WarudoBatchBuilder.WbbPrefs.DeleteKey(ackKey);
    sb.AppendLine("cleanup ack pref restored (had=" + ackHad + ") now has=" + StudioRaming.WarudoBatchBuilder.WbbPrefs.HasKey(ackKey));
}
sb.AppendLine(fails == 0 ? "RESULT PASS" : "RESULT FAIL " + fails);
return sb.ToString();
