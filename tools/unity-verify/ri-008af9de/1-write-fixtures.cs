// WMM WARUDO root resolution (hub item ri-008af9de), step 1 of 5. lab_exec_file statement body, QA project only.
// Copies tools/unity-verify/ri-008af9de/fixture into <QaRoot>/Assets/WmmRootFixture and refreshes. The copy adds .asmdef and
// .cs files, so Unity reloads the domain: wait for the editor to be ready (lab_wait_ready) before 2-run.cs.
// Removed again by 4-cleanup.cs. Nothing outside Assets/WmmRootFixture is touched.
const string QaRoot = "";        // PARAMETER: the QA project root (Unity 2021.3.45f2, SDK 0.14.3.10)
const string FixtureSource = ""; // PARAMETER: absolute path of this repo's tools/unity-verify/ri-008af9de/fixture
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(FixtureSource)) return "ABORT set QaRoot and FixtureSource (PARAMETER constants at the top) first";
const string FixtureRoot = "Assets/WmmRootFixture";

if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEngine.Application.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready (playing or compiling)";
if (!System.IO.Directory.Exists(FixtureSource)) return "ABORT fixture source missing: " + FixtureSource;
string target = QaRoot + "/" + FixtureRoot;
if (System.IO.Directory.Exists(target)) return "ABORT fixture folder already exists, run 4-cleanup.cs first (nothing is overwritten): " + FixtureRoot;
if (UnityEditor.AssetDatabase.FindAssets("Wmm.RootFix.Runtime t:AssemblyDefinitionAsset").Length > 0) return "ABORT an assembly definition named Wmm.RootFix.Runtime already exists";

int copied = 0;
foreach (string source in System.IO.Directory.GetFiles(FixtureSource, "*", System.IO.SearchOption.AllDirectories))
{
    string rel = source.Replace('\\', '/').Substring(FixtureSource.Replace('\\', '/').TrimEnd('/').Length).TrimStart('/');
    string destination = target + "/" + rel;
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destination));
    System.IO.File.Copy(source, destination, false);
    copied++;
}

UnityEditor.AssetDatabase.Refresh();
return "WROTE " + copied + " files into " + FixtureRoot + ". Unity will compile and reload. Wait until ready (lab_wait_ready), then run 2-run.cs.";
