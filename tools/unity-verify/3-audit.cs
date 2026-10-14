// WMM asmdef runtime support, step 3 of 3 (task_id PHYS-RESUME-20261008). lab_exec_file statement body, read-only.
// Poll after 2-build.cs with the same Case and RunTag. Returns RUNNING while WMM builds, then one line per check and a
// verdict. Writes <OutDir>/<modName>-entries.txt (the archive entry list: diff it between a v0.1.0 run and a branch run
// of plain-manual to see that a plain folder builds the same). Changes nothing.
const string Case = "ok-manual";                                   // PARAMETER, as in 2-build.cs
const string RunTag = "A";                                         // PARAMETER, as in 2-build.cs
const string QaRoot = "";                    // PARAMETER: the QA project root
const string OutDir = "";                    // PARAMETER: folder for archives and receipts (must exist)
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(OutDir)) return "ABORT set QaRoot and OutDir (PARAMETER constants at the top) first";

string kind = Case.Substring(0, Case.IndexOf('-'));
string modName = "WmmFix_" + Case.Replace('-', '_') + "_" + RunTag;
string archive = OutDir + "/" + modName + ".warudo";
string receiptPath = OutDir + "/" + modName + "-wmm-verify.txt";

if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "RUNNING";
if (UnityEditor.EditorApplication.isCompiling) return "RUNNING (editor is compiling)";
if (!System.IO.File.Exists(receiptPath)) return "ABORT no receipt for " + modName + ": run 2-build.cs first";

var report = new System.Text.StringBuilder();
int failures = 0;
int inconclusive = 0;
void Check(string name, bool ok, string detail)
{
    report.AppendLine((ok ? "PASS " : "FAIL ") + name + (detail.Length > 0 ? " : " + detail : ""));
    if (!ok) failures++;
}

string[] receipt = System.IO.File.ReadAllLines(receiptPath);
string Receipt(string key)
{
    foreach (string line in receipt)
    {
        if (line.StartsWith(key + "=", System.StringComparison.Ordinal)) return line.Substring(key.Length + 1);
    }

    return null;
}

// The newest record of this mod.
StudioRaming.WarudoBatchBuilder.WbbRecord record = null;
foreach (var candidate in StudioRaming.WarudoBatchBuilder.WbbDatabase.Records)
{
    if (candidate.modName == modName && (record == null || string.CompareOrdinal(candidate.startedIso, record.startedIso) > 0)) record = candidate;
}
Check("history record exists", record != null, record == null ? "none" : "status=" + (StudioRaming.WarudoBatchBuilder.WbbBuildStatus)record.status);
if (record == null) return string.Join("\n", new[] { report.ToString().TrimEnd(), "VERDICT FAIL (no record)" });
var status = (StudioRaming.WarudoBatchBuilder.WbbBuildStatus)record.status;
string message = record.message ?? "";
report.AppendLine("message=" + message);
report.AppendLine("buildLog=" + record.buildLogPath);

bool archiveExists = System.IO.File.Exists(archive);
System.Collections.Generic.List<string> entries = archiveExists ? StudioRaming.WarudoBatchBuilder.WbbBuildLog.ReadArchiveEntryNames(archive) : null;
if (entries != null) System.IO.File.WriteAllLines(OutDir + "/" + modName + "-entries.txt", entries.ToArray());
bool hasModules = entries != null && entries.Exists(n => string.Equals(n, "assemblymodules.dat", System.StringComparison.OrdinalIgnoreCase));

if (kind == "ok" || kind == "plain")
{
    Check("item succeeded", status == StudioRaming.WarudoBatchBuilder.WbbBuildStatus.Success, status.ToString());
    Check("archive exists and lists entries", entries != null && entries.Count > 0, entries == null ? "none" : entries.Count + " entries");
    Check("assemblymodules.dat packed", hasModules, "");
}

if (kind == "ok")
{
    Check("message names the compiled assembly", message.Contains("Wmm.FixtureOk.Runtime"), "");
    Check("editor-only asmdefs left out", !message.Contains("Wmm.FixtureOk.Editor") && !message.Contains("Wmm.FixtureOk.Tooling"), "");
    string auditPath = QaRoot + "/Library/WmmFixtureAudit.txt";
    string audit = System.IO.File.Exists(auditPath) ? System.IO.File.ReadAllText(auditPath) : "";
    int at = audit.LastIndexOf("build=" + modName + " stage=", System.StringComparison.Ordinal);
    // The SDK links components in the second processor pass; use the last block that has links for this build.
    string block = "";
    int from = 0;
    while ((from = audit.IndexOf("build=" + modName + " stage=", from, System.StringComparison.Ordinal)) >= 0)
    {
        int next = audit.IndexOf("\nbuild=", from + 1, System.StringComparison.Ordinal);
        string piece = next < 0 ? audit.Substring(from) : audit.Substring(from, next - from);
        if (piece.Contains(" links=") && !piece.Contains(" links=0")) block = piece;
        from = from + 1;
    }
    Check("audit saw a linked prefab for this build", block.Length > 0, at < 0 ? "no audit lines (is Wmm.FixtureOk.Editor compiled? " + auditPath + ")" : "");
    if (block.Length > 0)
    {
        Check("constraint linked against Wmm.FixtureOk.Runtime", block.Contains("link=WmmFixture.WmmFixtureConstraint|Wmm.FixtureOk.Runtime"), "");
        Check("data component linked against Wmm.FixtureOk.Runtime", block.Contains("link=WmmFixture.WmmFixtureData|Wmm.FixtureOk.Runtime"), "");
        Check("no raw fixture components remain", block.Contains(" raw=0"), "");
        Check("no missing scripts", block.Contains(" missing=0"), "");
    }
    report.AppendLine("audit block:\n" + block.Trim());
}

if (kind == "bad")
{
    if (status == StudioRaming.WarudoBatchBuilder.WbbBuildStatus.Success)
    {
        inconclusive++;
        report.AppendLine("INCONCLUSIVE the code security check did not refuse Process.Start/System.IO in this SDK setting (read ModToolsSettings security values)");
    }
    else
    {
        Check("item failed", status == StudioRaming.WarudoBatchBuilder.WbbBuildStatus.Failed, status.ToString());
        Check("message is not only the bare failure", message.Split(new[] { " / " }, System.StringSplitOptions.None).Length >= 3, message.Split(new[] { " / " }, System.StringSplitOptions.None).Length + " parts");
        Check("message mentions Process or System.IO", message.IndexOf("Process", System.StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("System.IO", System.StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("File", System.StringComparison.OrdinalIgnoreCase) >= 0, "");
        bool logHasSection = !string.IsNullOrEmpty(record.buildLogPath) && System.IO.File.Exists(record.buildLogPath) && System.IO.File.ReadAllText(record.buildLogPath).Contains("[WarudoBatchBuilder]");
        Check("kept build log holds the full security section", logHasSection, record.buildLogPath);
        Check("no archive left behind", !archiveExists, "");
    }
}

// Whatever the case: settings, temp folders, compile output and the UMod hook must be back to normal.
string profileError;
int profileIndex = StudioRaming.WarudoBatchBuilder.WbbExportSettings.GetActiveProfileIndex(out profileError);
StudioRaming.WarudoBatchBuilder.WbbProfileSnapshot after = null;
if (profileIndex >= 0) StudioRaming.WarudoBatchBuilder.WbbExportSettings.TryReadActiveProfile(out after, out profileError, profileIndex);
if (Receipt("before.modName") == null)
{
    // No before snapshot in the receipt: the profile cannot be compared, so this is not a pass.
    inconclusive++;
    report.AppendLine("INCONCLUSIVE build profile restored: no before snapshot (" + Receipt("before.error") + ")");
}
else if (after == null)
{
    Check("build profile restored", false, "after profile unreadable: " + profileError);
}
else
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
string runtimeRoot = QaRoot + "/Library/StudioRaming/WarudoBatchBuilder/Runtime";
Check("runtime compile output removed", !System.IO.Directory.Exists(runtimeRoot) || System.IO.Directory.GetDirectories(runtimeRoot).Length == 0, runtimeRoot);
Check("build journal cleared", !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists, "");
System.Type engineType = null;
foreach (var loaded in System.AppDomain.CurrentDomain.GetAssemblies())
{
    engineType = loaded.GetType("UMod.BuildEngine.BuildEngineService", false);
    if (engineType != null) break;
}
int hookAfter = -1;
for (var t = engineType; t != null && t != typeof(object); t = t.BaseType)
{
    if (!t.IsGenericType) continue;
    var hookField = t.GetField("beforeRunProcessors", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
    if (hookField == null) break;
    var current = hookField.GetValue(null) as System.Delegate;
    hookAfter = current == null ? 0 : current.GetInvocationList().Length;
    break;
}
// -1 means the hook field was not found (UMod changed): "-1" == "-1" would be a pass that compared nothing.
if (Receipt("hookCount.before") == "-1" || hookAfter == -1)
{
    inconclusive++;
    report.AppendLine("INCONCLUSIVE UMod beforeRunProcessors back to its handler count: hook field not found (before=" + Receipt("hookCount.before") + " after=" + hookAfter + ")");
}
else
    Check("UMod beforeRunProcessors back to its handler count", Receipt("hookCount.before") == hookAfter.ToString(), "before=" + Receipt("hookCount.before") + " after=" + hookAfter);

string verdict = failures > 0 ? "FAIL (" + failures + " check(s))" : inconclusive > 0 ? "INCONCLUSIVE" : "PASS";
return report.ToString().TrimEnd() + "\nVERDICT " + verdict + " case=" + Case + " modName=" + modName + (entries != null ? " entries=" + OutDir + "/" + modName + "-entries.txt" : "");
