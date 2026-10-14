// WMM Quick Build over the same name (ri-b6afbfa3). lab_exec_file statement body, QA project only.
// WbbQuickBuild.Start always overwrites (job.overwriteExisting = true) and reads its output folder from the saved Prop
// output folder pref, so this script points that pref at OutDir for the length of one Start call and puts it back in
// the same call. Without the pref the Quick Build would write into the WARUDO install, so it refuses to start then.
// Run order, one call each, same RunTag (poll an audit until it stops saying RUNNING):
//   Phase "build" Pass 1 -> QUEUED       Phase "audit" Pass 1 -> PASS lines
//   Phase "build" Pass 2 -> QUEUED       Phase "audit" Pass 2 -> PASS lines and VERDICT
//   Phase "restore"      -> closes the check scene, prints the output folder pref
// Both passes expect no build journal file left. Pass 2 also expects: Success, the archive written by this build, no
// backup of this name left in OutputBackups, no outputs in the build journal, no dirty scene, no _WBB_Temp folder, and no quick-iteration reload notice (OutDir is not the
// Props folder of the WARUDO install). A failed Quick Build opens a result dialog: if an audit keeps saying RUNNING,
// look for that dialog. The fixture (a cube prefab and a check scene) stays in Assets/WmmQuickBuildCheck_<RunTag>.
const string Phase = "build";                // PARAMETER: build | audit | restore
const int Pass = 1;                          // PARAMETER: 1 or 2 (build and audit)
const string RunTag = "A";                   // PARAMETER, new for every pair of builds
const string QaRoot = "";                    // PARAMETER: the QA project root
const string OutDir = "";                    // PARAMETER: an existing folder for test outputs (never the WARUDO install)
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(OutDir)) return "ABORT set QaRoot and OutDir (PARAMETER constants at the top) first";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (!System.IO.Directory.Exists(OutDir)) return "ABORT OutDir does not exist: " + OutDir;
if (OutDir.IndexOf("Warudo_Data", System.StringComparison.OrdinalIgnoreCase) >= 0) return "ABORT OutDir is inside a WARUDO install: " + OutDir;

const string PrefKey = "StudioRaming.WarudoBatchBuilder.OutputDir.Prop"; // WbbBuildSection.OutputDirKey(Prop)
string modName = "WmmFix_quick_" + RunTag;
string archive = OutDir + "/" + modName + ".warudo";
string statePath = OutDir + "/" + modName + "-quick-state.txt";
string fixtureFolder = "Assets/WmmQuickBuildCheck_" + RunTag;
string scenePath = fixtureFolder + "/QuickCheck.unity";
string prefabPath = fixtureFolder + "/WmmQuickCube.prefab";
string Norm(string path)
{
    return (path ?? "").Replace('\\', '/').TrimEnd('/');
}
string Sha256(string path)
{
    using (var stream = System.IO.File.OpenRead(path))
    using (var sha = System.Security.Cryptography.SHA256.Create())
        return System.BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
}
var state = new System.Collections.Generic.Dictionary<string, string>();
if (System.IO.File.Exists(statePath))
{
    foreach (string line in System.IO.File.ReadAllLines(statePath))
    {
        int eq = line.IndexOf('=');
        if (eq > 0) state[line.Substring(0, eq)] = line.Substring(eq + 1);
    }
}
void SaveState()
{
    var lines = new System.Collections.Generic.List<string>();
    foreach (var pair in state) lines.Add(pair.Key + "=" + pair.Value);
    System.IO.File.WriteAllLines(statePath, lines.ToArray());
}
string State(string key)
{
    string value;
    return state.TryGetValue(key, out value) ? value : "";
}
UnityEngine.SceneManagement.Scene CheckScene()
{
    return UnityEngine.SceneManagement.SceneManager.GetSceneByPath(scenePath);
}

if (Phase == "build")
{
    if (UnityEngine.Application.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready (playing or compiling)";
    if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning || StudioRaming.WarudoBatchBuilder.WbbQuickBuild.IsRunning) return "ABORT a WMM build is running";
    if (Pass == 1 && System.IO.File.Exists(statePath)) return "ABORT " + statePath + " exists, use a new RunTag";
    if (Pass == 1 && System.IO.File.Exists(archive)) return "ABORT output exists, use a new RunTag: " + archive;
    if (Pass == 2 && (State("pass1.sha256") == "" || !System.IO.File.Exists(archive))) return "ABORT run Phase audit Pass 1 to a PASS first";
    for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
    {
        var open = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
        if (open.isDirty) return "ABORT dirty scene (WMM would save it): " + open.path;
    }

    // An open Mod Mate window writes its Build tab folder back on flush (before ri-c8a9c861 it always did): close it.
    foreach (var window in UnityEngine.Resources.FindObjectsOfTypeAll<StudioRaming.WarudoBatchBuilder.WbbWindow>()) window.Close();

    if (Pass == 1)
    {
        System.IO.Directory.CreateDirectory(QaRoot + "/" + fixtureFolder);
        UnityEditor.AssetDatabase.Refresh();
        var cube = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
        cube.name = "WmmQuickCube";
        try { UnityEditor.PrefabUtility.SaveAsPrefabAsset(cube, prefabPath); }
        finally { UnityEngine.Object.DestroyImmediate(cube); }
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Additive);
        UnityEditor.PrefabUtility.InstantiatePrefab(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(prefabPath), scene);
        if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath)) return "ABORT could not save the check scene " + scenePath;
    }

    var checkScene = CheckScene();
    if (!checkScene.IsValid() || !checkScene.isLoaded) return "ABORT the check scene is not open: " + scenePath;
    UnityEngine.GameObject source = null;
    foreach (var root in checkScene.GetRootGameObjects())
    {
        if (root.name == "WmmQuickCube") source = root;
    }
    if (source == null) return "ABORT no WmmQuickCube in " + scenePath;

    bool hadPref = StudioRaming.WarudoBatchBuilder.WbbPrefs.HasKey(PrefKey);
    string oldPref = StudioRaming.WarudoBatchBuilder.WbbPrefs.GetString(PrefKey);
    if (Pass == 1)
    {
        state["pref.had"] = hadPref ? "true" : "false";
        state["pref.value"] = oldPref;
    }
    string started = "";
    string error = "";
    bool ok = false;
    try
    {
        StudioRaming.WarudoBatchBuilder.WbbPrefs.SetString(PrefKey, OutDir);
        if (Norm(StudioRaming.WarudoBatchBuilder.WbbPrefs.GetString(PrefKey)) != Norm(OutDir)) return "ABORT the output folder pref did not take " + OutDir;
        state["pass" + Pass + ".startTicks"] = System.DateTime.UtcNow.Ticks.ToString();
        var targets = new System.Collections.Generic.List<StudioRaming.WarudoBatchBuilder.WbbQuickBuild.Target>();
        targets.Add(new StudioRaming.WarudoBatchBuilder.WbbQuickBuild.Target { source = source, modName = modName });
        ok = StudioRaming.WarudoBatchBuilder.WbbQuickBuild.Start(targets, StudioRaming.WarudoBatchBuilder.WbbExportType.Prop, out error);
        if (ok)
        {
            // The folder the Quick Build took; anything but OutDir is cancelled before the first update tick exports.
            var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
            started = Norm((string)typeof(StudioRaming.WarudoBatchBuilder.WbbQuickBuild).GetField("outputDirectory", flags).GetValue(null));
            if (!string.Equals(started, Norm(OutDir), System.StringComparison.OrdinalIgnoreCase))
            {
                var runner = (StudioRaming.WarudoBatchBuilder.WbbBuildRunner)typeof(StudioRaming.WarudoBatchBuilder.WbbQuickBuild).GetField("runner", flags).GetValue(null);
                if (runner != null) runner.Cancel();
                return "ABORT the Quick Build took another output folder and was cancelled: " + started;
            }
        }
    }
    finally
    {
        // The job already holds the folder; the pref goes back now so nothing else reads OutDir from it.
        if (hadPref) StudioRaming.WarudoBatchBuilder.WbbPrefs.SetString(PrefKey, oldPref);
        else StudioRaming.WarudoBatchBuilder.WbbPrefs.DeleteKey(PrefKey);
        SaveState();
    }
    if (!ok) return "ABORT WbbQuickBuild.Start refused: " + error;
    return "QUEUED pass " + Pass + " " + modName + " output=" + started + "/" + modName + ".warudo; output folder pref back to " + (hadPref ? "'" + oldPref + "'" : "(none)") + ". Poll Phase audit Pass " + Pass + ".";
}

if (Phase == "audit")
{
    if (StudioRaming.WarudoBatchBuilder.WbbQuickBuild.IsRunning || StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "RUNNING";
    if (UnityEditor.EditorApplication.isCompiling) return "RUNNING (editor is compiling)";
    if (State("pass" + Pass + ".startTicks") == "") return "ABORT run Phase build Pass " + Pass + " first";
    long startTicks = long.Parse(State("pass" + Pass + ".startTicks"));
    var report = new System.Text.StringBuilder();
    int failures = 0;
    void Check(string name, bool pass, string detail)
    {
        report.AppendLine((pass ? "PASS " : "FAIL ") + name + (detail.Length > 0 ? " : " + detail : ""));
        if (!pass) failures++;
    }

    StudioRaming.WarudoBatchBuilder.WbbRecord record = null;
    foreach (var candidate in StudioRaming.WarudoBatchBuilder.WbbDatabase.Records)
    {
        if (candidate.modName == modName && (record == null || string.CompareOrdinal(candidate.startedIso, record.startedIso) > 0)) record = candidate;
    }
    var status = record == null ? StudioRaming.WarudoBatchBuilder.WbbBuildStatus.Pending : (StudioRaming.WarudoBatchBuilder.WbbBuildStatus)record.status;
    Check("pass " + Pass + " record succeeded", status == StudioRaming.WarudoBatchBuilder.WbbBuildStatus.Success, record == null ? "no record" : status + " " + record.message);
    bool archiveExists = System.IO.File.Exists(archive);
    Check("pass " + Pass + " archive written by this build", archiveExists && System.IO.File.GetLastWriteTimeUtc(archive).Ticks >= startTicks,
        archiveExists ? System.IO.File.GetLastWriteTimeUtc(archive).ToString("o") : "no archive");
    if (record != null) Check("pass " + Pass + " record output is OutDir", string.Equals(Norm(record.outputFilePath), Norm(archive), System.StringComparison.OrdinalIgnoreCase), record.outputFilePath);
    // WbbJournal.Complete forgets the journal in memory before it deletes the file, so a file it could not delete
    // (a lock) shows only on disk: WbbJournal.Current is null either way.
    Check("pass " + Pass + " build journal cleared", !StudioRaming.WarudoBatchBuilder.WbbJournal.Exists, StudioRaming.WarudoBatchBuilder.WbbJournal.Exists ? "BuildJournal.json still on disk" : "");

    if (Pass == 1)
    {
        if (archiveExists)
        {
            state["pass1.sha256"] = Sha256(archive);
            SaveState();
        }
    }
    else
    {
        string backups = StudioRaming.WarudoBatchBuilder.WbbBuildLog.LibraryDirectory + "/OutputBackups";
        int backupsLeft = System.IO.Directory.Exists(backups) ? System.IO.Directory.GetFiles(backups, modName + "_*").Length : 0;
        Check("overwrite: no backup of this name left in OutputBackups", backupsLeft == 0, backupsLeft + " file(s) in " + backups);
        var journal = StudioRaming.WarudoBatchBuilder.WbbJournal.Current;
        Check("overwrite: build journal holds no outputs", journal == null || journal.outputs.Count == 0, journal == null ? "" : journal.outputs.Count + " output(s)");
        int dirty = 0;
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) dirty++;
        }
        Check("no dirty scene after the build", dirty == 0, dirty + " dirty");
        int temp = 0;
        foreach (string folder in UnityEditor.AssetDatabase.GetSubFolders("Assets"))
        {
            if (folder.Contains("_WBB_Temp")) temp++;
        }
        Check("no _WBB_Temp folder left in Assets", temp == 0, temp + " folder(s)");
        // The reload notice belongs only to the Props folder of the WARUDO install (ri-18ecfb70); OutDir is not that.
        var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
        bool notice = (bool)typeof(StudioRaming.WarudoBatchBuilder.WbbQuickBuild).GetField("quickIterateNotice", flags).GetValue(null);
        Check("no quick-iteration reload notice outside the WARUDO Props folder", !notice, "quickIterateNotice=" + notice);
        report.AppendLine("INFO pass 1 sha256=" + State("pass1.sha256") + " pass 2 sha256=" + (archiveExists ? Sha256(archive) : "none"));
    }

    return report.ToString().TrimEnd() + "\nVERDICT " + (failures > 0 ? "FAIL (" + failures + " check(s))" : "PASS") + " pass=" + Pass + " modName=" + modName;
}

if (Phase == "restore")
{
    if (StudioRaming.WarudoBatchBuilder.WbbQuickBuild.IsRunning) return "RUNNING";
    var checkScene = CheckScene();
    bool closed = checkScene.IsValid() && UnityEditor.SceneManagement.EditorSceneManager.CloseScene(checkScene, true);
    // Each build call already put the pref back; this only shows it, and repairs it if a call died in between.
    string want = State("pref.value");
    if (State("pref.had") == "true" && StudioRaming.WarudoBatchBuilder.WbbPrefs.GetString(PrefKey) != want) StudioRaming.WarudoBatchBuilder.WbbPrefs.SetString(PrefKey, want);
    if (State("pref.had") == "false" && StudioRaming.WarudoBatchBuilder.WbbPrefs.HasKey(PrefKey)) StudioRaming.WarudoBatchBuilder.WbbPrefs.DeleteKey(PrefKey);
    return "RESTORED check scene closed=" + closed + "; output folder pref = " + (StudioRaming.WarudoBatchBuilder.WbbPrefs.HasKey(PrefKey) ? "'" + StudioRaming.WarudoBatchBuilder.WbbPrefs.GetString(PrefKey) + "'" : "(none)")
        + " (at the start: " + (State("pref.had") == "true" ? "'" + want + "'" : "(none)") + "). Fixture left in " + fixtureFolder;
}

return "ABORT unknown Phase " + Phase;
