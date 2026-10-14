// ri-5b7581f7 cleanup after 2-run.cs and the UI checks. lab_exec_file statement body, QA project only.
// Moves the set-aside files the test made to ArtifactDir (not deleted), moves any leftover journal there too, and puts
// the acknowledgement pref back to what 1-pending.cs recorded.
const string QaRoot = ""; // PARAMETER: the QA project root (Unity 2021.3.45f2 with the WARUDO SDK and this branch installed)
const string StateFile = ""; // PARAMETER: a writable file outside the project, e.g. <temp>/wmm-5b-state.txt
const string ArtifactDir = ""; // PARAMETER: a folder outside the project that receives the test files (moved, not deleted)
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(StateFile) || string.IsNullOrEmpty(ArtifactDir)) return "ABORT set the PARAMETER constants at the top first";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (!System.IO.File.Exists(StateFile)) return "ABORT no state file";
string lib = StudioRaming.WarudoBatchBuilder.WbbBuildLog.LibraryDirectory;
string ackKey = StudioRaming.WarudoBatchBuilder.WbbUtil.PrefPrefix + "Journal.SetAsideAcknowledged";
string journalPath = StudioRaming.WarudoBatchBuilder.WbbJournal.JournalPath;
var sb = new System.Text.StringBuilder();
{
    var before = new System.Collections.Generic.HashSet<string>();
    bool ackHad = false;
    string ackValue = "";
    foreach (string line in System.IO.File.ReadAllLines(StateFile))
    {
        if (line.StartsWith("before=")) before.Add(line.Substring(7));
        else if (line.StartsWith("ackHad=")) ackHad = line.Substring(7) == "True";
        else if (line.StartsWith("ackValue=")) ackValue = line.Substring(9);
    }
    System.IO.Directory.CreateDirectory(ArtifactDir);
    foreach (string f in System.IO.Directory.GetFiles(lib, "BuildJournal.json.unrecovered-*"))
    {
        string n = System.IO.Path.GetFileName(f);
        if (before.Contains(n)) continue;
        System.IO.File.Move(f, ArtifactDir + "/" + n);
        sb.AppendLine("cleanup moved " + n + " to ArtifactDir");
    }
    if (StudioRaming.WarudoBatchBuilder.WbbJournal.Exists)
    {
        System.IO.File.Move(journalPath, ArtifactDir + "/BuildJournal.json.left-" + System.DateTime.Now.ToString("HHmmss"));
        sb.AppendLine("cleanup moved a leftover journal to ArtifactDir");
    }
    if (ackHad) StudioRaming.WarudoBatchBuilder.WbbPrefs.SetString(ackKey, ackValue);
    else StudioRaming.WarudoBatchBuilder.WbbPrefs.DeleteKey(ackKey);
    sb.AppendLine("cleanup ack pref restored (had=" + ackHad + ") now has=" + StudioRaming.WarudoBatchBuilder.WbbPrefs.HasKey(ackKey) + ", unacknowledged='" + StudioRaming.WarudoBatchBuilder.WbbJournal.UnacknowledgedSetAside + "'");
    System.IO.File.Move(StateFile, ArtifactDir + "/state-" + System.DateTime.Now.ToString("HHmmss") + ".txt");
}
return sb.ToString();
