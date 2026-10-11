// WMM WARUDO root resolution (hub item ri-008af9de), step 3 of 5. lab_exec_file statement body.
// Poll after 2-run.cs (cases A, C, D, E) with the same Case and RunTag. Returns RUNNING while WMM builds. Then it prints one line
// per check and a VERDICT, writes <OutDir>/<modName>-entries.txt (the archive entry list, for the v0.1.2 diff of case D), copies
// an archive that sits inside WARUDO to OutDir, and cleans up: the switch is restored, a setting that case C wrote is put back,
// and an output folder that 2-run.cs created inside WARUDO is deleted (only that folder, only when its name starts with WmmRootFix_).
const string Case = "A";                     // PARAMETER, as in 2-run.cs
const string RunTag = "R1";                  // PARAMETER, as in 2-run.cs
const string QaRoot = "";                    // PARAMETER: the QA project root
const string OutDir = "";                    // PARAMETER: as in 2-run.cs
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(OutDir)) return "ABORT set QaRoot and OutDir (PARAMETER constants at the top) first";
const string PrefKey = StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.RootPrefKey;

string baseName = "WmmRoot_" + Case + "_" + RunTag;
string statePath = OutDir + "/" + baseName + "-root-state.txt";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "RUNNING";
if (UnityEditor.EditorApplication.isCompiling) return "RUNNING (editor is compiling)";
if (Case == "B") return "ABORT case B is decided inside 2-run.cs, there is nothing to audit";
if (!System.IO.File.Exists(statePath)) return "ABORT no state for " + baseName + ": run 2-run.cs first";

string[] state = System.IO.File.ReadAllLines(statePath);
string State(string key)
{
    foreach (string line in state) if (line.StartsWith(key + "=", System.StringComparison.Ordinal)) return line.Substring(key.Length + 1);
    return null;
}

var report = new System.Text.StringBuilder();
int failures = 0;
void Check(string name, bool ok, string detail)
{
    report.AppendLine((ok ? "PASS " : "FAIL ") + name + (detail.Length > 0 ? " : " + detail : ""));
    if (!ok) failures++;
}

string[] modNames = (State("state.modNames") ?? "").Split(',');
string outputDirectory = State("state.outputDirectory") ?? "";
bool createdOutput = State("state.createdOutput") == "1";
bool insideWarudo = State("state.outputInsideWarudo") == "1";
string expectSource = State("state.expectSource") ?? "";
long logOffset = long.Parse(State("state.logOffset") ?? "0");
string prefPrevious = State("state.prefPrevious") ?? "<unset>";
int expectedItems = int.Parse(State("state.items") ?? "1");

// Editor.log since the build started.
string logText = "";
try
{
    string logPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Unity/Editor/Editor.log");
    using (var stream = new System.IO.FileStream(logPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite | System.IO.FileShare.Delete))
    {
        if (logOffset > stream.Length) logOffset = 0;
        stream.Seek(logOffset, System.IO.SeekOrigin.Begin);
        using (var reader = new System.IO.StreamReader(stream)) logText = reader.ReadToEnd();
    }
}
catch (System.Exception e) { report.AppendLine("INFO could not read Editor.log: " + e.Message); }

int sourceAt = logText.LastIndexOf("WARUDO folder for this batch: source ", System.StringComparison.Ordinal);
string loggedSource = "";
string loggedLine = "";
if (sourceAt >= 0)
{
    int end = logText.IndexOf('\n', sourceAt);
    loggedLine = (end < 0 ? logText.Substring(sourceAt) : logText.Substring(sourceAt, end - sourceAt)).Trim();
    int valueAt = "WARUDO folder for this batch: source ".Length;
    int space = loggedLine.IndexOf(' ', valueAt);
    loggedSource = space < 0 ? loggedLine.Substring(valueAt) : loggedLine.Substring(valueAt, space - valueAt);
}
Check("batch start log names the source and path", loggedSource == expectSource, "expected " + expectSource + ", log: " + loggedLine);

var records = new System.Collections.Generic.List<StudioRaming.WarudoBatchBuilder.WbbRecord>();
foreach (string modName in modNames)
{
    StudioRaming.WarudoBatchBuilder.WbbRecord record = null;
    foreach (var candidate in StudioRaming.WarudoBatchBuilder.WbbDatabase.Records)
        if (candidate.modName == modName && (record == null || string.CompareOrdinal(candidate.startedIso, record.startedIso) > 0)) record = candidate;
    Check("history record exists: " + modName, record != null, record == null ? "none" : "status=" + (StudioRaming.WarudoBatchBuilder.WbbBuildStatus)record.status);
    if (record == null) continue;
    records.Add(record);
    var status = (StudioRaming.WarudoBatchBuilder.WbbBuildStatus)record.status;
    string message = record.message ?? "";
    report.AppendLine("message[" + modName + "]=" + message);
    Check("item succeeded: " + modName, status == StudioRaming.WarudoBatchBuilder.WbbBuildStatus.Success, status.ToString());
    Check("no 'WARUDO installation is required' failure: " + modName, !message.Contains("WARUDO installation is required"), "");

    string archive = outputDirectory + "/" + modName + ".warudo";
    var entries = System.IO.File.Exists(archive) ? StudioRaming.WarudoBatchBuilder.WbbBuildLog.ReadArchiveEntryNames(archive) : null;
    Check("archive exists and lists entries: " + modName, entries != null && entries.Count > 0, entries == null ? archive + " missing" : entries.Count + " entries");
    if (entries != null)
    {
        System.IO.File.WriteAllLines(OutDir + "/" + modName + "-entries.txt", entries.ToArray());
        if (insideWarudo) System.IO.File.Copy(archive, OutDir + "/" + modName + ".warudo", true);
        if (Case != "E")
            Check("assemblymodules.dat packed: " + modName, entries.Exists(n => string.Equals(n, "assemblymodules.dat", System.StringComparison.OrdinalIgnoreCase)), "");
    }
}
Check("all " + expectedItems + " items have a record", records.Count == expectedItems, records.Count.ToString());

if (Case == "D")
{
    int reads = 0, at = 0;
    while ((at = logText.IndexOf("WARUDO type catalog read:", at, System.StringComparison.Ordinal)) >= 0) { reads++; at++; }
    Check("type catalog read once for the batch", reads == 1, reads + " read(s) in Editor.log for " + expectedItems + " item(s)");
}
if (Case == "E")
{
    Check("preflight showed the skip warning", string.Join("\n", state).Contains("[warudo-managed-skipped]"), "");
}

// Clean up what this case created.
StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.DisableSteamLookupForTests = false;
StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.Invalidate();
if (Case == "C")
{
    if (prefPrevious == "<unset>") StudioRaming.WarudoBatchBuilder.WbbPrefs.DeleteKey(PrefKey);
    else StudioRaming.WarudoBatchBuilder.WbbPrefs.SetString(PrefKey, prefPrevious);
    report.AppendLine("restored the WARUDO folder setting: " + prefPrevious);
}
if (insideWarudo && createdOutput && System.IO.Directory.Exists(outputDirectory)
    && System.IO.Path.GetFileName(outputDirectory.TrimEnd('/', '\\')).StartsWith("WmmRootFix_", System.StringComparison.Ordinal))
{
    System.IO.Directory.Delete(outputDirectory, true);
    report.AppendLine("removed the output folder this case created inside WARUDO: " + outputDirectory);
}
Check("switch restored", !StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.DisableSteamLookupForTests, "");

string verdict = failures > 0 ? "FAIL (" + failures + " check(s))" : "PASS";
return report.ToString().TrimEnd() + "\nVERDICT " + verdict + " case=" + Case + " runTag=" + RunTag;
