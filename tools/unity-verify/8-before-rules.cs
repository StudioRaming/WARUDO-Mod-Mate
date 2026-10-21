// WMM BeforeRules extension point, tests W-1..W-3 (task_id VMOD2-PLAN-20261017). lab_exec_file statement body, QA project only.
// Calls WbbSnapshot.SaveFromSceneObject and WbbBuildFixups.Apply on preview-scene copies. No build is started.
//   W-1 hook order      : the hook sees a stub component in namespace nadena.dev on the copy (so it runs before
//                         ModularAvatarRule), the copy is in a preview scene, and the saved prefab has no stub.
//   W-2 exception       : a throwing hook makes SaveFromSceneObject return false with the hook's message, and no
//                         prefab is written (fail closed; a try/catch inside the rule loop would hide it).
//   W-3 no-hook output  : with BeforeRules null, Apply gives the same report text as with a no-op hook.
// Stub: the nadena.dev stub is a MonoBehaviour that 1-write-fixtures-style setup writes into
// <QaRoot>/Assets/WmmBeforeRules/WmmBeforeRulesStub.cs. The first run only writes it and returns STUB WRITTEN: wait
// for the editor to finish reloading (lab_wait_ready), then run again with the same RunTag.
// Run it in the QA project only. Prefabs it saves (Assets/WmmBeforeRules/*_<RunTag>.prefab) are deleted at the end.
const string QaRoot = "";     // PARAMETER: the QA project root (empty Unity 2021.3.45f2 project)
const string RunTag = "A";    // PARAMETER, new for every run
if (string.IsNullOrEmpty(QaRoot)) return "ABORT set QaRoot (PARAMETER constant at the top) first";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEngine.Application.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready (playing or compiling)";
if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "ABORT a WMM build is running";
if (StudioRaming.WarudoBatchBuilder.WbbBuildFixups.BeforeRules != null) return "ABORT BeforeRules is already set by someone else";

const string StubTypeName = "nadena.dev.WmmBeforeRulesStub";
const string StubFolder = "Assets/WmmBeforeRules";
const string StubFile = StubFolder + "/WmmBeforeRulesStub.cs";
System.Type stubType = null;
foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
{
    try { stubType = asm.GetType(StubTypeName); } catch (System.Exception) { stubType = null; }
    if (stubType != null) break;
}
if (stubType == null)
{
    if (System.IO.File.Exists(QaRoot + "/" + StubFile)) return "ABORT stub file exists but its type is not loaded yet: wait for the editor to finish compiling";
    System.IO.Directory.CreateDirectory(QaRoot + "/" + StubFolder);
    System.IO.File.WriteAllText(QaRoot + "/" + StubFile,
        "namespace nadena.dev\n{\n    // Stands in for a Modular Avatar component (namespace nadena.dev) in the Mod Mate hook test.\n" +
        "    public class WmmBeforeRulesStub : UnityEngine.MonoBehaviour { }\n}\n");
    UnityEditor.AssetDatabase.Refresh();
    return "STUB WRITTEN " + StubFile + ": wait for the editor to be ready (lab_wait_ready), then run again with RunTag " + RunTag;
}

string folder = StubFolder + "/" + RunTag;
if (System.IO.Directory.Exists(QaRoot + "/" + folder)) return "ABORT " + folder + " exists, use a new RunTag";
System.IO.Directory.CreateDirectory(QaRoot + "/" + folder);

var report = new System.Text.StringBuilder();
int failures = 0;
void Check(string name, bool ok, string detail)
{
    report.AppendLine((ok ? "PASS " : "FAIL ") + name + (detail.Length > 0 ? " : " + detail : ""));
    if (!ok) failures++;
}
UnityEngine.SceneManagement.Scene NewPreview()
{
    return UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
}
UnityEngine.GameObject Source(UnityEngine.SceneManagement.Scene scene, string name)
{
    var go = new UnityEngine.GameObject(name);
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
    go.AddComponent(stubType);
    return go;
}
string AssetPath(string name)
{
    return folder + "/" + name + "_" + RunTag + ".prefab";
}
int StubCount(UnityEngine.GameObject go)
{
    return go.GetComponentsInChildren(stubType, true).Length;
}
// W-1: hook order
{
    var scene = NewPreview();
    var src = Source(scene, "W1");
    int stubAtHook = -1;
    bool inPreview = false;
    bool notAsset = false;
    StudioRaming.WarudoBatchBuilder.WbbBuildFixups.BeforeRules = copy =>
    {
        stubAtHook = StubCount(copy);
        inPreview = UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(copy.scene);
        notAsset = !UnityEditor.EditorUtility.IsPersistent(copy);
    };
    StudioRaming.WarudoBatchBuilder.WbbFixupReport rep;
    string error;
    bool ok;
    try
    {
        ok = StudioRaming.WarudoBatchBuilder.WbbSnapshot.SaveFromSceneObject(src, AssetPath("W1"), new StudioRaming.WarudoBatchBuilder.WbbFixupOptions(), out rep, out error);
    }
    finally
    {
        StudioRaming.WarudoBatchBuilder.WbbBuildFixups.BeforeRules = null;
        UnityEngine.Object.DestroyImmediate(src);
        UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
    }
    var saved = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(AssetPath("W1"));
    int stubAfter = saved == null ? -1 : StubCount(saved);
    Check("W-1 save succeeded", ok, ok ? "" : error);
    Check("W-1 hook got a preview-scene copy that is not an asset", inPreview && notAsset, "inPreview=" + inPreview + " notAsset=" + notAsset);
    Check("W-1 stub still on the copy when the hook runs", stubAtHook == 1, "count=" + stubAtHook);
    Check("W-1 ModularAvatarRule removed the stub after the hook", saved != null && stubAfter == 0, "count=" + stubAfter);
}

// W-2: a throwing hook fails the item
{
    var scene = NewPreview();
    var src = Source(scene, "W2");
    const string marker = "W2 hook failure";
    StudioRaming.WarudoBatchBuilder.WbbBuildFixups.BeforeRules = copy =>
    {
        throw new System.InvalidOperationException(marker);
    };
    StudioRaming.WarudoBatchBuilder.WbbFixupReport rep;
    string error;
    bool ok;
    try
    {
        ok = StudioRaming.WarudoBatchBuilder.WbbSnapshot.SaveFromSceneObject(src, AssetPath("W2"), new StudioRaming.WarudoBatchBuilder.WbbFixupOptions(), out rep, out error);
    }
    finally
    {
        StudioRaming.WarudoBatchBuilder.WbbBuildFixups.BeforeRules = null;
        UnityEngine.Object.DestroyImmediate(src);
        UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
    }
    bool written = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(AssetPath("W2")) != null;
    Check("W-2 SaveFromSceneObject returns false", !ok, "ok=" + ok);
    Check("W-2 error carries the hook's message", error != null && error.Contains(marker), "error=" + error);
    Check("W-2 no prefab written", !written, "written=" + written);
}

// W-3: no hook gives the same report as a no-op hook
{
    var scene = NewPreview();
    var a = Source(scene, "W3a");
    var b = Source(scene, "W3b");
    string textNull;
    string textNoop;
    try
    {
        StudioRaming.WarudoBatchBuilder.WbbBuildFixups.BeforeRules = null;
        textNull = StudioRaming.WarudoBatchBuilder.WbbBuildFixups.Apply(a, new StudioRaming.WarudoBatchBuilder.WbbFixupOptions()).ToText();
        StudioRaming.WarudoBatchBuilder.WbbBuildFixups.BeforeRules = copy => { };
        textNoop = StudioRaming.WarudoBatchBuilder.WbbBuildFixups.Apply(b, new StudioRaming.WarudoBatchBuilder.WbbFixupOptions()).ToText();
    }
    finally
    {
        StudioRaming.WarudoBatchBuilder.WbbBuildFixups.BeforeRules = null;
        UnityEngine.Object.DestroyImmediate(a);
        UnityEngine.Object.DestroyImmediate(b);
        UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
    }
    Check("W-3 report is not empty (the stub was removed by a rule)", !string.IsNullOrEmpty(textNull), "text=" + textNull);
    Check("W-3 no-hook report equals no-op-hook report", textNull == textNoop, "null=[" + textNull + "] noop=[" + textNoop + "]");
}

// Cleanup: only the prefabs this run saved in its own folder.
foreach (string file in System.IO.Directory.GetFiles(QaRoot + "/" + folder))
{
    if (file.EndsWith(".prefab")) UnityEditor.AssetDatabase.DeleteAsset(folder + "/" + System.IO.Path.GetFileName(file));
}
UnityEditor.AssetDatabase.Refresh();
Check("BeforeRules restored to null", StudioRaming.WarudoBatchBuilder.WbbBuildFixups.BeforeRules == null, "");
return report.ToString().TrimEnd() + "\nVERDICT " + (failures > 0 ? "FAIL (" + failures + " check(s))" : "PASS") + " RunTag=" + RunTag;
