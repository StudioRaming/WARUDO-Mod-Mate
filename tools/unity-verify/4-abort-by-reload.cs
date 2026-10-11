// WMM asmdef runtime support, item 5 check (task_id PHYS-RESUME-20261008). lab_exec_file statement body, QA project only.
// Shows what a build does when a script recompile cuts it off, three calls:
//   Phase = "reload"  : call this ONLY while a build from 2-build.cs is RUNNING (3-audit.cs says RUNNING). It waits for
//                       the item to reach Stage, WriteSettings or CompileRuntime, then asks Unity to recompile scripts,
//                       which reloads the domain and aborts the WMM runner. The bridge may report the disconnect; wait
//                       for the editor to be ready again.
//   Phase = "check"   : after the reload. Expects the item Canceled with the reason in its message, a note kept for the
//                       Build tab, the build profile restored, no temp folder and no journal left. Read-only.
// Start the build with 2-build.cs (Case ok-manual, a new RunTag) and call "reload" right after it returns QUEUED. An
// immediate recompile request lands while the batch is still in Prepare, before the item starts: the item is Skipped
// and leaves no history record, so "check" can only say INCONCLUSIVE (measured 2026-10-11). Hence the wait.
const string Phase = "reload";                                     // PARAMETER: reload | check
const string Case = "ok-manual";                                   // PARAMETER, as in 2-build.cs
const string RunTag = "R1";                                        // PARAMETER, the tag of the interrupted build
const string QaRoot = "";                    // PARAMETER: the QA project root
const string OutDir = "";                    // PARAMETER: folder for archives and receipts (must exist)
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(OutDir)) return "ABORT set QaRoot and OutDir (PARAMETER constants at the top) first";

string modName = "WmmFix_" + Case.Replace('-', '_') + "_" + RunTag;
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;

if (Phase == "reload")
{
    if (!StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "ABORT no WMM build is running; start one with 2-build.cs first";
    // The runner's private step is read by reflection; without it, fall back to an immediate request.
    var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance;
    var runnerType = typeof(StudioRaming.WarudoBatchBuilder.WbbBuildRunner);
    var activeField = runnerType.GetField("activeRunner", flags);
    var stepField = runnerType.GetField("step", flags);
    if (activeField == null || stepField == null)
    {
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        return "REQUESTED a script recompile at once (runner step not readable, the result may be INCONCLUSIVE). Wait for the editor, then run Phase=check.";
    }

    int ticks = 0;
    UnityEditor.EditorApplication.CallbackFunction hook = null;
    hook = () =>
    {
        ticks++;
        object runner = activeField.GetValue(null);
        if (runner == null || !StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning || ticks > 20000)
        {
            UnityEditor.EditorApplication.update -= hook;
            UnityEngine.Debug.Log("[WmmAbortCheck] no recompile requested: the build ended or the wait ran out at tick " + ticks);
            return;
        }

        string step = stepField.GetValue(runner).ToString();
        if (step == "Stage" || step == "WriteSettings" || step == "CompileRuntime")
        {
            UnityEditor.EditorApplication.update -= hook;
            UnityEngine.Debug.Log("[WmmAbortCheck] requesting a script recompile at step " + step + ", tick " + ticks);
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    };
    UnityEditor.EditorApplication.update += hook;
    return "WAITING to request a script recompile once " + modName + " reaches Stage, WriteSettings or CompileRuntime (the Console logs [WmmAbortCheck]). Wait for the editor, then run Phase=check.";
}

if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "RUNNING";
if (UnityEditor.EditorApplication.isCompiling) return "RUNNING (editor is compiling)";
string receiptPath = OutDir + "/" + modName + "-wmm-verify.txt";
if (!System.IO.File.Exists(receiptPath)) return "ABORT no receipt for " + modName;
string[] receipt = System.IO.File.ReadAllLines(receiptPath);
string Receipt(string key)
{
    foreach (string line in receipt)
    {
        if (line.StartsWith(key + "=", System.StringComparison.Ordinal)) return line.Substring(key.Length + 1);
    }

    return null;
}

var report = new System.Text.StringBuilder();
int failures = 0;
void Check(string name, bool ok, string detail)
{
    report.AppendLine((ok ? "PASS " : "FAIL ") + name + (detail.Length > 0 ? " : " + detail : ""));
    if (!ok) failures++;
}

StudioRaming.WarudoBatchBuilder.WbbRecord record = null;
foreach (var candidate in StudioRaming.WarudoBatchBuilder.WbbDatabase.Records)
{
    if (candidate.modName == modName && (record == null || string.CompareOrdinal(candidate.startedIso, record.startedIso) > 0)) record = candidate;
}
if (record == null)
{
    // The build finished before the reload reached it, or the reload stopped the batch before the item started: report which.
    bool built = System.IO.File.Exists(OutDir + "/" + modName + ".warudo");
    return "INCONCLUSIVE no history record for " + modName + (built ? " but the archive exists: the build finished before the recompile" : " and no archive: the reload stopped the batch before the item started (item Skipped, no record), or the build never ran. Rerun with a new RunTag");
}

var status = (StudioRaming.WarudoBatchBuilder.WbbBuildStatus)record.status;
if (status == StudioRaming.WarudoBatchBuilder.WbbBuildStatus.Success) return "INCONCLUSIVE the build finished before the recompile reached it (status Success)";
Check("item is Canceled", status == StudioRaming.WarudoBatchBuilder.WbbBuildStatus.Canceled, status.ToString());
string message = record.message ?? "";
report.AppendLine("record.message=" + message);
Check("record message says what happened", message.IndexOf("recompil", System.StringComparison.OrdinalIgnoreCase) >= 0 || message.Contains("\uB2E4\uC2DC \uCEF4\uD30C\uC77C") || message.Contains("\u518D\u30B3\u30F3\u30D1\u30A4\u30EB"), "");
string note = StudioRaming.WarudoBatchBuilder.WbbRunnerNotice.Peek();
report.AppendLine("notice=" + note);
Check("a note survived the reload for the Build tab", note.Length > 0, "");

string profileError;
int profileIndex = StudioRaming.WarudoBatchBuilder.WbbExportSettings.GetActiveProfileIndex(out profileError);
StudioRaming.WarudoBatchBuilder.WbbProfileSnapshot after = null;
if (profileIndex >= 0) StudioRaming.WarudoBatchBuilder.WbbExportSettings.TryReadActiveProfile(out after, out profileError, profileIndex);
if (after != null && Receipt("before.modName") != null)
{
    Check("build profile restored", Receipt("before.modName") == after.modName && Receipt("before.modAuthor") == after.modAuthor && Receipt("before.modVersion") == after.modVersion
        && Receipt("before.modDescription") == after.modDescription && Receipt("before.modAssetPath") == after.modAssetPath && Receipt("before.modExportPath") == after.modExportPath,
        "after: " + after.modName + " | " + after.modAssetPath + " | " + after.modExportPath);
}
bool tempLeft = false;
foreach (string folder in UnityEditor.AssetDatabase.GetSubFolders("Assets"))
{
    if (folder.Contains("_WBB_Temp_do_not_edit_")) tempLeft = true;
}
Check("no WMM temp folder left in Assets", !tempLeft, "");
Check("build journal cleared", !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists, "");
Check("no half-written archive", !System.IO.File.Exists(OutDir + "/" + modName + ".warudo"), "");
return report.ToString().TrimEnd() + "\nVERDICT " + (failures > 0 ? "FAIL (" + failures + " check(s))" : "PASS") + " modName=" + modName;
