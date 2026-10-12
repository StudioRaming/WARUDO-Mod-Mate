// WMM asmdef runtime support, step 2 of 3 (task_id PHYS-RESUME-20261008). lab_exec_file statement body, QA project only.
// Starts one WMM build of a fixture through WbbBuildRunner.Start (the call the Build tab makes) and returns QUEUED.
// The build is a state machine on EditorApplication.update: leave the editor alone (no script edits, no Play Mode),
// poll with 3-audit.cs until it stops saying RUNNING. Change Case and RunTag for every run; nothing is overwritten.
//   ok-manual    asmdef runtime in a manual script folder          expect: Success, assemblymodules.dat, links to Wmm.FixtureOk.Runtime
//   ok-auto      same prefab, automatic script collection          expect: the same as ok-manual (needs WARUDO installed)
//   bad-manual   asmdef runtime that uses Process/System.IO        expect: Failed with per-API security entries
//   plain-manual no asmdef; plain scripts compiled by Mod Mate     expect: Success, assemblymodules.dat, history message names WbbRuntimeScripts
const string Case = "ok-manual";                                   // PARAMETER
const string RunTag = "A";                                         // PARAMETER, new for every run
const string QaRoot = "";                    // PARAMETER: the QA project root
const string OutDir = "";                    // PARAMETER: folder for archives and receipts (must exist)
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(OutDir)) return "ABORT set QaRoot and OutDir (PARAMETER constants at the top) first";
const string FixtureRoot = "Assets/WmmAsmdefFixture";

string kind = Case.Substring(0, Case.IndexOf('-'));
bool manual = Case.EndsWith("-manual");
string modName = "WmmFix_" + Case.Replace('-', '_') + "_" + RunTag;
string archive = OutDir + "/" + modName + ".warudo";
string receiptPath = OutDir + "/" + modName + "-wmm-verify.txt";
string buildFolder = kind == "ok" ? FixtureRoot + "/AsmdefOk" : kind == "bad" ? FixtureRoot + "/AsmdefBad" : FixtureRoot + "/Plain";
string prefabFolder = FixtureRoot + "/Prefabs";
string prefabPath = prefabFolder + "/WmmFix" + kind + ".prefab";

if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEngine.Application.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready (playing or compiling)";
if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "ABORT a WMM build is already running";
if (System.IO.File.Exists(archive) || System.IO.File.Exists(receiptPath)) return "ABORT output exists, use a new RunTag: " + modName;
if (kind != "ok" && kind != "bad" && kind != "plain") return "ABORT unknown Case " + Case;
if (!System.IO.Directory.Exists(QaRoot + "/" + buildFolder)) return "ABORT fixture folder missing, run 1-write-fixtures.cs first: " + buildFolder;
if (kind == "plain")
{
    string csproj = QaRoot + "/Assembly-CSharp.csproj";
    if (!System.IO.File.Exists(csproj) || !System.IO.File.ReadAllText(csproj).Contains("WmmFixturePlain.cs"))
        return "ABORT Assembly-CSharp.csproj does not list WmmFixturePlain.cs: regenerate the project files (Assets > Open C# Project) first";
}

// Nothing dirty may exist: WMM saves dirty scenes before and during the build.
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
    System.IO.Directory.CreateDirectory(QaRoot + "/" + prefabFolder);
    var go = new UnityEngine.GameObject("WmmFix" + kind);
    try
    {
        if (kind == "ok")
        {
            System.Type constraintType = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(FixtureRoot + "/AsmdefOk/Runtime/WmmFixtureConstraint.cs")?.GetClass();
            System.Type dataType = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(FixtureRoot + "/AsmdefOk/Runtime/WmmFixtureData.cs")?.GetClass();
            if (constraintType == null || dataType == null) return "ABORT the AsmdefOk fixture scripts did not compile (wait for the reload, read the Console)";
            var constraint = go.AddComponent(constraintType);
            var data = go.AddComponent(dataType);
            var serialized = new UnityEditor.SerializedObject(data);
            serialized.FindProperty("peer").objectReferenceValue = constraint;
            serialized.FindProperty("target").objectReferenceValue = go.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        else if (kind == "bad")
        {
            System.Type badType = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(FixtureRoot + "/AsmdefBad/Runtime/WmmFixtureBad.cs")?.GetClass();
            if (badType == null) return "ABORT the AsmdefBad fixture script did not compile";
            go.AddComponent(badType);
        }
        else
        {
            System.Type plainType = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(FixtureRoot + "/Plain/Scripts/WmmFixturePlain.cs")?.GetClass();
            if (plainType == null) return "ABORT the Plain fixture script did not compile";
            go.AddComponent(plainType);
        }

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
items.Add(new StudioRaming.WarudoBatchBuilder.WbbBuildItem { prefabGuid = prefabGuid, prefabPath = prefabPath, prefabName = "WmmFix" + kind, modName = modName });
var job = new StudioRaming.WarudoBatchBuilder.WbbJob
{
    exportType = (int)StudioRaming.WarudoBatchBuilder.WbbExportType.Prop,
    outputMode = (int)StudioRaming.WarudoBatchBuilder.WbbOutputMode.WarudoMod,
    manualScriptFolder = manual,
    buildFolderGuid = UnityEditor.AssetDatabase.AssetPathToGUID(buildFolder),
    buildFolderPath = buildFolder,
    prefabFolderGuid = UnityEditor.AssetDatabase.AssetPathToGUID(prefabFolder),
    prefabFolderPath = prefabFolder,
    modVersion = "0.1.0",
    modAuthor = "StudioRaming",
    modDescription = "WMM asmdef fixture (not for release).",
    outputDirectory = OutDir,
    useFolderNamePrefix = false,
    namePrefix = "",
    overwriteExisting = false
};

// Snapshots the audit compares afterwards: the active build profile, and how many handlers UMod's hook already has.
string profileError;
int profileIndex = StudioRaming.WarudoBatchBuilder.WbbExportSettings.GetActiveProfileIndex(out profileError);
StudioRaming.WarudoBatchBuilder.WbbProfileSnapshot before = null;
if (profileIndex >= 0) StudioRaming.WarudoBatchBuilder.WbbExportSettings.TryReadActiveProfile(out before, out profileError, profileIndex);
int hookCount = -1;
System.Type engineType = null;
foreach (var loaded in System.AppDomain.CurrentDomain.GetAssemblies())
{
    engineType = loaded.GetType("UMod.BuildEngine.BuildEngineService", false);
    if (engineType != null) break;
}
for (var t = engineType; t != null && t != typeof(object); t = t.BaseType)
{
    if (!t.IsGenericType) continue;
    var hookField = t.GetField("beforeRunProcessors", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
    var current = hookField == null ? null : hookField.GetValue(null) as System.Delegate;
    hookCount = current == null ? 0 : current.GetInvocationList().Length;
    break;
}

var issues = StudioRaming.WarudoBatchBuilder.WbbBuildRunner.Preflight(items, job);
var receipt = new System.Text.StringBuilder();
receipt.AppendLine("task_id=PHYS-RESUME-20261008 step=wmm-asmdef-verify case=" + Case + " modName=" + modName + " utc=" + System.DateTime.UtcNow.ToString("o"));
receipt.AppendLine("unity=" + UnityEngine.Application.unityVersion + " dataPath=" + UnityEngine.Application.dataPath);
receipt.AppendLine("hookCount.before=" + hookCount);
if (before != null)
{
    receipt.AppendLine("before.modName=" + before.modName);
    receipt.AppendLine("before.modAuthor=" + before.modAuthor);
    receipt.AppendLine("before.modVersion=" + before.modVersion);
    receipt.AppendLine("before.modDescription=" + before.modDescription);
    receipt.AppendLine("before.modAssetPath=" + before.modAssetPath);
    receipt.AppendLine("before.modExportPath=" + before.modExportPath);
}
else
{
    // Without this line 3-audit.cs can only say INCONCLUSIVE, and it says why.
    receipt.AppendLine("before.error=" + profileIndex + "|" + profileError);
}
int blocking = 0;
foreach (var issue in issues)
{
    var coded = issue as StudioRaming.WarudoBatchBuilder.WbbPreflightIssueEx;
    receipt.AppendLine((issue.blocking ? "BLOCK " : "WARN ") + (coded == null ? "" : "[" + coded.code + "] ") + issue.message + " | " + issue.detail);
    if (issue.blocking) blocking++;
}
System.IO.Directory.CreateDirectory(OutDir);
System.IO.File.WriteAllText(receiptPath, receipt.ToString());
if (blocking > 0) return "ABORT WMM preflight blocks (" + blocking + "), see " + receiptPath;

var runner = new StudioRaming.WarudoBatchBuilder.WbbBuildRunner();
if (!runner.Start(items, job)) return "ABORT WMM refused start: " + runner.StatusMessage;
return "QUEUED " + modName + " (manual=" + manual + ") output=" + archive + " receipt=" + receiptPath + ". Poll with 3-audit.cs (same Case and RunTag).";
