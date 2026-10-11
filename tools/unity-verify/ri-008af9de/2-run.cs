// WMM WARUDO root resolution (hub item ri-008af9de), step 2 of 5. lab_exec_file statement body, QA project only.
// Runs the set-up of one case and starts the build (the same WbbBuildRunner.Start call the Build tab makes). Cases:
//   A  Steam lookup off, output <WARUDO>/Warudo_Data/StreamingAssets/Props/<new folder>, auto mode, plain MonoBehaviour. Expect success, source a.
//   B  Steam lookup off, output in a temp folder, no WARUDO folder setting. Expect: preflight blocks (code warudo-managed-missing), Start refuses,
//      0 history records. Checked here at once (no build starts); returns PASS/FAIL.
//   C  as B, plus the WARUDO folder setting = WarudoRoot. Expect success, source b.
//   D  Steam lookup ON (as shipped). Output "warudo" (the Steam WARUDO Props folder, source a) or "temp" (source c). Items = 3 gives a
//      3-item batch: the type catalog must be read once. Run it on the v0.1.2 checkout too and diff the entries and the provided-type count.
//   E  Steam lookup off, output in a temp folder, MANUAL script folder with an asmdef runtime assembly. Expect a non-blocking preflight
//      warning (warudo-managed-skipped), the build starts and goes on.
// The editor must stay untouched while the build runs: poll 3-audit.cs (same Case and RunTag) until it stops saying RUNNING.
// 3-audit.cs restores the switch, the setting and removes the output it created. 4-cleanup.cs undoes everything unconditionally.
const string Case = "A";                     // PARAMETER: A, B, C, D or E
const string RunTag = "R1";                  // PARAMETER: new for every run (an existing output aborts)
const string QaRoot = "";                    // PARAMETER: the QA project root
const string OutDir = "";                    // PARAMETER: existing folder for receipts, copies of archives and temp outputs
const string WarudoRoot = "";                // PARAMETER: WARUDO install root (the folder with Warudo.exe); needed by A, C and D (output "warudo")
const string DOutput = "warudo";             // PARAMETER: case D only, "warudo" or "temp"
const int DItems = 3;                        // PARAMETER: case D only, how many items the batch holds
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(OutDir)) return "ABORT set QaRoot and OutDir (PARAMETER constants at the top) first";
const string FixtureRoot = "Assets/WmmRootFixture";
const string PrefKey = StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.RootPrefKey;

if (Case != "A" && Case != "B" && Case != "C" && Case != "D" && Case != "E") return "ABORT unknown Case " + Case;
bool needsWarudo = Case == "A" || Case == "C" || (Case == "D" && DOutput == "warudo");
string streaming = "";
if (needsWarudo || Case == "D")
{
    if (string.IsNullOrEmpty(WarudoRoot)) return "ABORT set WarudoRoot (the WARUDO install folder) for case " + Case;
    streaming = WarudoRoot.Replace('\\', '/').TrimEnd('/') + "/Warudo_Data/StreamingAssets";
    if (!System.IO.Directory.Exists(streaming + "/../Managed")) return "ABORT no Warudo_Data/Managed under " + WarudoRoot;
}
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEngine.Application.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready (playing or compiling)";
if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "ABORT a WMM build is already running";
if (!System.IO.Directory.Exists(OutDir)) return "ABORT OutDir does not exist: " + OutDir;

bool manual = Case == "E";
bool asm = Case == "E";
int count = Case == "D" ? DItems : 1;
string baseName = "WmmRoot_" + Case + "_" + RunTag;
string statePath = OutDir + "/" + baseName + "-root-state.txt";
if (System.IO.File.Exists(statePath)) return "ABORT state exists, use a new RunTag: " + statePath;
string prefabFolder = FixtureRoot + "/Prefabs";
string prefabPath = prefabFolder + (asm ? "/WmmRootAsm.prefab" : "/WmmRootPlain.prefab");
string buildFolder = FixtureRoot + "/Asmdef";
string prefPrevious = StudioRaming.WarudoBatchBuilder.WbbPrefs.HasKey(PrefKey) ? StudioRaming.WarudoBatchBuilder.WbbPrefs.GetString(PrefKey, "") : null;
if (prefPrevious != null && (Case != "D" || DOutput == "temp")) return "ABORT a WARUDO folder setting exists for this project (" + prefPrevious + "): clear it first (4-cleanup.cs with ClearSetting = true)";

string outputDirectory;
bool createdOutput;
if (Case == "A" || (Case == "D" && DOutput == "warudo")) outputDirectory = streaming + "/Props/WmmRootFix_" + RunTag;
else outputDirectory = OutDir + "/out_" + Case + "_" + RunTag;
createdOutput = !System.IO.Directory.Exists(outputDirectory);
if (!createdOutput) return "ABORT output folder exists, use a new RunTag: " + outputDirectory;
string expectSource = Case == "A" ? "a" : Case == "C" ? "b" : Case == "E" ? "none" : Case == "D" ? (DOutput == "warudo" ? "a" : "c") : "none";

if (!System.IO.Directory.Exists(QaRoot + "/" + FixtureRoot)) return "ABORT fixture missing, run 1-write-fixtures.cs first";
for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
{
    var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
    if (scene.isDirty) return "ABORT dirty scene (WMM would save it): " + scene.path;
    if (string.IsNullOrEmpty(scene.path)) return "ABORT untitled scene loaded: " + scene.name;
}
if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) return "ABORT prefab stage open";

// The fixture prefab is made once, from the compiled fixture scripts.
if (!System.IO.File.Exists(QaRoot + "/" + prefabPath))
{
    string scriptPath = asm ? FixtureRoot + "/Asmdef/Runtime/WmmRootAsm.cs" : FixtureRoot + "/Plain/WmmRootPlain.cs";
    System.Type type = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(scriptPath)?.GetClass();
    if (type == null) return "ABORT the fixture script did not compile (wait for the reload, read the Console): " + scriptPath;
    System.IO.Directory.CreateDirectory(QaRoot + "/" + prefabFolder);
    var go = new UnityEngine.GameObject(asm ? "WmmRootAsm" : "WmmRootPlain");
    try
    {
        go.AddComponent(type);
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
    }
    finally
    {
        UnityEngine.Object.DestroyImmediate(go);
    }

    UnityEditor.AssetDatabase.ImportAsset(prefabPath);
}

string prefabGuid = UnityEditor.AssetDatabase.AssetPathToGUID(prefabPath);
if (string.IsNullOrEmpty(prefabGuid)) return "ABORT fixture prefab has no GUID: " + prefabPath;

var items = new System.Collections.Generic.List<StudioRaming.WarudoBatchBuilder.WbbBuildItem>();
var modNames = new System.Collections.Generic.List<string>();
for (int i = 0; i < count; i++)
{
    string modName = baseName + (count > 1 ? "_" + (i + 1) : "");
    modNames.Add(modName);
    items.Add(new StudioRaming.WarudoBatchBuilder.WbbBuildItem { prefabGuid = prefabGuid, prefabPath = prefabPath, prefabName = asm ? "WmmRootAsm" : "WmmRootPlain", modName = modName });
}

var job = new StudioRaming.WarudoBatchBuilder.WbbJob
{
    exportType = (int)StudioRaming.WarudoBatchBuilder.WbbExportType.Prop,
    outputMode = (int)StudioRaming.WarudoBatchBuilder.WbbOutputMode.WarudoMod,
    manualScriptFolder = manual,
    buildFolderGuid = manual ? UnityEditor.AssetDatabase.AssetPathToGUID(buildFolder) : "",
    buildFolderPath = manual ? buildFolder : "",
    prefabFolderGuid = UnityEditor.AssetDatabase.AssetPathToGUID(prefabFolder),
    prefabFolderPath = prefabFolder,
    modVersion = "0.1.0",
    modAuthor = "StudioRaming",
    modDescription = "WMM WARUDO root fixture (not for release).",
    outputDirectory = outputDirectory,
    useFolderNamePrefix = false,
    namePrefix = "",
    overwriteExisting = false
};

// Switch and setting for the case. A reload resets the switch (it is a static), which is the safe direction.
bool switchOn = Case != "D";
StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.DisableSteamLookupForTests = switchOn;
StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.Invalidate();
if (Case == "C") StudioRaming.WarudoBatchBuilder.WbbPrefs.SetString(PrefKey, WarudoRoot.Replace('\\', '/').TrimEnd('/'));

string logPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Unity/Editor/Editor.log");
long logOffset = 0;
try { logOffset = new System.IO.FileInfo(logPath).Length; } catch (System.Exception) { }

var issues = StudioRaming.WarudoBatchBuilder.WbbBuildRunner.Preflight(items, job);
var receipt = new System.Text.StringBuilder();
receipt.AppendLine("task_id=ri-008af9de case=" + Case + " runTag=" + RunTag + " utc=" + System.DateTime.UtcNow.ToString("o"));
receipt.AppendLine("unity=" + UnityEngine.Application.unityVersion + " dataPath=" + UnityEngine.Application.dataPath);
int blocking = 0;
bool sawMissing = false, sawSkipped = false;
string missingMessage = "";
foreach (var issue in issues)
{
    var coded = issue as StudioRaming.WarudoBatchBuilder.WbbPreflightIssueEx;
    string code = coded == null ? "" : coded.code;
    receipt.AppendLine((issue.blocking ? "BLOCK " : "WARN ") + (code.Length > 0 ? "[" + code + "] " : "") + issue.message + " | " + issue.detail);
    if (issue.blocking) blocking++;
    if (code == StudioRaming.WarudoBatchBuilder.WbbPreflightCodes.WarudoManagedMissing && issue.blocking) { sawMissing = true; missingMessage = issue.message; }
    if (code == StudioRaming.WarudoBatchBuilder.WbbPreflightCodes.WarudoManagedSkipped && !issue.blocking) sawSkipped = true;
}

void Restore()
{
    StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.DisableSteamLookupForTests = false;
    StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.Invalidate();
    if (Case == "C")
    {
        if (prefPrevious == null) StudioRaming.WarudoBatchBuilder.WbbPrefs.DeleteKey(PrefKey);
        else StudioRaming.WarudoBatchBuilder.WbbPrefs.SetString(PrefKey, prefPrevious);
    }
}

if (Case == "B")
{
    // Decided here: nothing may start, nothing may be recorded.
    string keyText = StudioRaming.WarudoBatchBuilder.WbbLang.T("Safety.WarudoManagedMissing");
    string buttonText = StudioRaming.WarudoBatchBuilder.WbbLang.T("Safety.PickWarudoFolder");
    int recordsBefore = 0;
    foreach (var r in StudioRaming.WarudoBatchBuilder.WbbDatabase.Records) if (r.modName.StartsWith(baseName, System.StringComparison.Ordinal)) recordsBefore++;
    var runner = new StudioRaming.WarudoBatchBuilder.WbbBuildRunner();
    bool started = false;
    string status = "";
    try
    {
        started = runner.Start(items, job);
        status = runner.StatusMessage;
        if (started) runner.AbortAndRestore("verify case B");
    }
    finally
    {
        Restore();
    }

    int recordsAfter = 0;
    foreach (var r in StudioRaming.WarudoBatchBuilder.WbbDatabase.Records) if (r.modName.StartsWith(baseName, System.StringComparison.Ordinal)) recordsAfter++;
    var report = new System.Text.StringBuilder();
    int failures = 0;
    void Check(string name, bool ok, string detail) { report.AppendLine((ok ? "PASS " : "FAIL ") + name + (detail.Length > 0 ? " : " + detail : "")); if (!ok) failures++; }
    Check("preflight has a blocking warudo-managed-missing issue", sawMissing, missingMessage);
    Check("issue text is translated (not a language key)", sawMissing && !missingMessage.StartsWith("Safety.", System.StringComparison.Ordinal) && missingMessage.Length > 10, missingMessage);
    Check("[Set WARUDO folder] button label exists", !buttonText.StartsWith("Safety.", System.StringComparison.Ordinal), buttonText);
    Check("Start refused before the batch", !started, "status=" + status);
    Check("refusal text is the translated message", status == keyText, status);
    Check("0 failed item records", recordsAfter == recordsBefore, "records " + recordsBefore + " -> " + recordsAfter);
    Check("switch restored", !StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.DisableSteamLookupForTests, "");
    System.IO.File.WriteAllText(statePath, receipt.ToString());
    return report.ToString().TrimEnd() + "\nVERDICT " + (failures > 0 ? "FAIL (" + failures + ")" : "PASS") + " case=B receipt=" + statePath;
}

if (blocking > 0)
{
    Restore();
    System.IO.File.WriteAllText(statePath, receipt.ToString());
    return "ABORT WMM preflight blocks (" + blocking + "), see " + statePath;
}
if (Case == "E" && !sawSkipped)
{
    Restore();
    System.IO.File.WriteAllText(statePath, receipt.ToString());
    return "FAIL case E: no non-blocking warudo-managed-skipped warning, see " + statePath;
}

receipt.AppendLine("state.modNames=" + string.Join(",", modNames.ToArray()));
receipt.AppendLine("state.outputDirectory=" + outputDirectory);
receipt.AppendLine("state.createdOutput=" + (createdOutput ? "1" : "0"));
receipt.AppendLine("state.outputInsideWarudo=" + (Case == "A" || (Case == "D" && DOutput == "warudo") ? "1" : "0"));
receipt.AppendLine("state.expectSource=" + expectSource);
receipt.AppendLine("state.logOffset=" + logOffset);
receipt.AppendLine("state.prefPrevious=" + (prefPrevious == null ? "<unset>" : prefPrevious));
receipt.AppendLine("state.items=" + count);
System.IO.File.WriteAllText(statePath, receipt.ToString());

var starter = new StudioRaming.WarudoBatchBuilder.WbbBuildRunner();
if (!starter.Start(items, job))
{
    string refused = starter.StatusMessage;
    Restore();
    return "ABORT WMM refused start: " + refused + " (switch and setting restored)";
}

return "QUEUED case=" + Case + " items=" + count + " output=" + outputDirectory + " expectSource=" + expectSource + " receipt=" + statePath + ". Poll 3-audit.cs (same Case and RunTag, DOutput as here).";
