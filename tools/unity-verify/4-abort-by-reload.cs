// WMM asmdef runtime support, item 5 check (task_id PHYS-RESUME-20261008). lab_exec_file statement body, QA project only.
// Shows what a build does when a script recompile cuts it off, three calls:
//   Phase = "reload"  : call this ONLY while a build from 2-build.cs is RUNNING (3-audit.cs says RUNNING). It asks Unity to
//                       recompile scripts, which reloads the domain and aborts the WMM runner. The bridge may report the
//                       disconnect; wait for the editor to be ready again.
//   Phase = "check"   : after the reload. Expects the item Canceled with the reason in its message, a note kept for the
//                       Build tab, the build profile restored, no temp folder and no journal left. Read-only.
// Start the build with 2-build.cs (Case ok-manual, a new RunTag) right before "reload"; the compile step of a larger
// runtime is the best moment, so use a runtime with many files if the fixture finishes too fast.
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
    UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    return "REQUESTED a script recompile while " + modName + " runs. Wait for the editor, then run Phase=check.";
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
    // The build finished before the reload reached it, or never ran: report which.
    bool built = System.IO.File.Exists(OutDir + "/" + modName + ".warudo");
    return "INCONCLUSIVE no history record for " + modName + (built ? " but the archive exists: the build finished before the recompile" : " and no archive: the build never started");
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
