// WMM asmdef runtime support, step 1 of 3 (task_id PHYS-RESUME-20261008). lab_exec_file statement body, QA project only.
// Copies tools/unity-verify/fixture into <QaRoot>/Assets/WmmAsmdefFixture and refreshes. The copy adds .asmdef and .cs
// files, so Unity reloads the domain: wait for the editor to be ready (lab_wait_ready) before 2-build.cs.
// Fixtures: AsmdefOk (runtime asmdef that needs a WARUDO reference + an editor-only asmdef in an Editor folder
// + an editor-only asmdef outside one + the audit hook), AsmdefBad (runtime asmdef a code security check should refuse),
// Plain (no asmdef). Nothing outside Assets/WmmAsmdefFixture is touched.
const string QaRoot = "";        // PARAMETER: the QA project root (empty Unity 2021.3.45f2 project, SDK 0.14.3.10)
const string FixtureSource = ""; // PARAMETER: absolute path of this repo's tools/unity-verify/fixture
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(FixtureSource)) return "ABORT set QaRoot and FixtureSource (PARAMETER constants at the top) first";
const string FixtureRoot = "Assets/WmmAsmdefFixture";

if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEngine.Application.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready (playing or compiling)";
if (!System.IO.Directory.Exists(FixtureSource)) return "ABORT fixture source missing: " + FixtureSource;
string target = QaRoot + "/" + FixtureRoot;
if (System.IO.Directory.Exists(target)) return "ABORT fixture folder already exists, remove it first (nothing is overwritten): " + FixtureRoot;
foreach (string existing in new[] { "Wmm.FixtureOk.Runtime", "Wmm.FixtureBad.Runtime", "Wmm.FixtureOk.Editor", "Wmm.FixtureOk.Tooling" })
{
    string[] hits = UnityEditor.AssetDatabase.FindAssets(existing + " t:AssemblyDefinitionAsset");
    if (hits.Length > 0) return "ABORT an assembly definition named " + existing + " already exists: " + UnityEditor.AssetDatabase.GUIDToAssetPath(hits[0]);
}

int copied = 0;
foreach (string source in System.IO.Directory.GetFiles(FixtureSource, "*", System.IO.SearchOption.AllDirectories))
{
    string rel = source.Replace('\\', '/').Substring(FixtureSource.Length).TrimStart('/');
    string destination = target + "/" + rel;
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destination));
    System.IO.File.Copy(source, destination, false);
    copied++;
}

UnityEditor.AssetDatabase.Refresh();
return "WROTE " + copied + " files into " + FixtureRoot + ". Unity will compile and reload. Wait until ready, then run 2-build.cs (Case ok-manual first).";
