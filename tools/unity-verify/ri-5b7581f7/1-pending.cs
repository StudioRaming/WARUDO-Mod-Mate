// ri-5b7581f7 auto review r1, step 1 of 3. lab_exec_file statement body, QA project only.
// Records the state it will touch, then writes a journal whose settings cannot be written back (profile index 999) and
// runs recovery once, leaving a pending journal for the UI capture. Step 2 (2-run.cs) runs the rest; 3-cleanup.cs undoes it.
const string QaRoot = ""; // PARAMETER: the QA project root (Unity 2021.3.45f2 with the WARUDO SDK and this branch installed)
const string StateFile = ""; // PARAMETER: a writable file outside the project, e.g. <temp>/wmm-5b-state.txt
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(StateFile)) return "ABORT set the PARAMETER constants at the top first";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready";
if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "ABORT a build is running";
if (StudioRaming.WarudoBatchBuilder.WbbJournal.Exists) return "ABORT a build journal already exists: " + StudioRaming.WarudoBatchBuilder.WbbJournal.JournalPath;
if (System.IO.File.Exists(StateFile)) return "ABORT state file from an earlier run exists, run 2-run.cs first: " + StateFile;

string lib = StudioRaming.WarudoBatchBuilder.WbbBuildLog.LibraryDirectory;
string ackKey = StudioRaming.WarudoBatchBuilder.WbbUtil.PrefPrefix + "Journal.SetAsideAcknowledged";
var state = new System.Text.StringBuilder();
state.AppendLine("ackHad=" + StudioRaming.WarudoBatchBuilder.WbbPrefs.HasKey(ackKey));
state.AppendLine("ackValue=" + StudioRaming.WarudoBatchBuilder.WbbPrefs.GetString(ackKey, "", false));
foreach (string f in System.IO.Directory.GetFiles(lib, "BuildJournal.json.unrecovered-*"))
    state.AppendLine("before=" + System.IO.Path.GetFileName(f));
System.IO.File.WriteAllText(StateFile, state.ToString());

var sb = new System.Text.StringBuilder();
sb.AppendLine("state recorded: " + state.ToString().Replace("\n", " | "));
sb.AppendLine("A0 unacknowledged set-aside before the test='" + StudioRaming.WarudoBatchBuilder.WbbJournal.UnacknowledgedSetAside + "'");
var data = new StudioRaming.WarudoBatchBuilder.WbbJournalData { batchId = "wmm5b-A", startedIso = "2026-10-12T00:00:00", profileCaptured = true, profileWritten = true, profileIndex = 999 };
System.IO.File.WriteAllText(StudioRaming.WarudoBatchBuilder.WbbJournal.JournalPath, UnityEngine.JsonUtility.ToJson(data, true), new System.Text.UTF8Encoding(false));
bool recovered = StudioRaming.WarudoBatchBuilder.WbbJournal.RecoverIfNeeded();
sb.AppendLine("A1 recover returned=" + recovered + " exists=" + StudioRaming.WarudoBatchBuilder.WbbJournal.Exists + " settingsPending=" + StudioRaming.WarudoBatchBuilder.WbbJournal.SettingsRestorePending);
var items = new System.Collections.Generic.List<StudioRaming.WarudoBatchBuilder.WbbBuildItem> { new StudioRaming.WarudoBatchBuilder.WbbBuildItem() };
var issues = StudioRaming.WarudoBatchBuilder.WbbBuildRunner.Preflight(items, new StudioRaming.WarudoBatchBuilder.WbbJob());
foreach (var issue in issues)
{
    var ex = issue as StudioRaming.WarudoBatchBuilder.WbbPreflightIssueEx;
    if (ex != null && ex.code.StartsWith("journal"))
        sb.AppendLine("A2 preflight code=" + ex.code + " blocking=" + ex.blocking + " message=" + ex.message);
}
string journalText = System.IO.File.ReadAllText(StudioRaming.WarudoBatchBuilder.WbbJournal.JournalPath);
sb.AppendLine("A3 journal recoveryAttempts line: " + journalText.Substring(journalText.IndexOf("\"recoveryAttempts\""), 24));
return sb.ToString();
