// WMM same-name overwrite guard (ri-b6afbfa3). lab_exec_file statement body, QA project only. No build, a few seconds.
// Calls the guard the way WbbBuildRunner.Steps.cs does around an export (WbbJournal.GuardOutput before it, then
// CommitOutput on success or RevertOutput on failure) on small files under OutDir, and checks what each path leaves:
//   a) A exists, guard, build writes B, revert     -> path holds A, the newest Failed/ entry holds B, own backup gone,
//                                                     message is Runner.FailedOutputRestored
//   b) A exists, guard, path deleted, revert       -> path holds A again
//   c) A exists, guard, B, verified, commit        -> path holds B, own backup gone
//   d) no file, guard, B, revert                   -> path gone, Failed/ holds B
//   e) A exists, guard, nothing written, revert    -> path holds A, Failed/ unchanged
// RevertOutput keeps only the newest WbbJournal.KeepFailedOutputs entries in Failed/, so older failed outputs of this
// project can go: run it in the QA project only. Nothing is deleted by the script itself: at the end every file it made
// in the Library folders (names start with wmmguard_<RunTag>_) is moved into OutDir/wmmguard_<RunTag>/leftovers.
const string QaRoot = "";                    // PARAMETER: the QA project root
const string OutDir = "";                    // PARAMETER: an existing folder for test outputs (never the WARUDO install)
const string RunTag = "A";                   // PARAMETER, new for every run
if (string.IsNullOrEmpty(QaRoot) || string.IsNullOrEmpty(OutDir)) return "ABORT set QaRoot and OutDir (PARAMETER constants at the top) first";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEngine.Application.isPlaying || UnityEditor.EditorApplication.isCompiling) return "ABORT editor not ready (playing or compiling)";
if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "ABORT a WMM build is running";
// Without an open journal GuardOutput keeps its entry in memory only, so a real build's journal is never touched.
if (StudioRaming.WarudoBatchBuilder.WbbJournal.Current != null) return "ABORT a build journal is open (WbbJournal.Current is set)";
if (!System.IO.Directory.Exists(OutDir)) return "ABORT OutDir does not exist: " + OutDir;
if (OutDir.IndexOf("Warudo_Data", System.StringComparison.OrdinalIgnoreCase) >= 0) return "ABORT OutDir is inside a WARUDO install: " + OutDir;

string prefix = "wmmguard_" + RunTag + "_";
string work = OutDir + "/wmmguard_" + RunTag;
if (System.IO.Directory.Exists(work)) return "ABORT " + work + " exists, use a new RunTag";
System.IO.Directory.CreateDirectory(work);
string library = StudioRaming.WarudoBatchBuilder.WbbBuildLog.LibraryDirectory;
string failedDir = StudioRaming.WarudoBatchBuilder.WbbJournal.FailedOutputDirectory;
string backupDir = library + "/OutputBackups"; // WbbJournal.OutputBackupFolder (private)
string previousDir = library + "/Previous";    // kept previous versions, once ri-dc8e41c4 is in

var report = new System.Text.StringBuilder();
int failures = 0;
void Check(string name, bool ok, string detail)
{
    report.AppendLine((ok ? "PASS " : "FAIL ") + name + (detail.Length > 0 ? " : " + detail : ""));
    if (!ok) failures++;
}

byte[] bytesA = System.Text.Encoding.ASCII.GetBytes("wmmguard " + RunTag + " A: the file a previous build left");
byte[] bytesB = System.Text.Encoding.ASCII.GetBytes("wmmguard " + RunTag + " B: the file this build wrote, longer than A");
bool Holds(string path, byte[] want)
{
    return !string.IsNullOrEmpty(path) && System.IO.File.Exists(path) && System.Linq.Enumerable.SequenceEqual(System.IO.File.ReadAllBytes(path), want);
}
string PathOf(string name)
{
    return work + "/" + prefix + name + ".warudo";
}
// The file a previous build left: older than the build start, so WbbJournal does not take it for this build's output.
void WriteA(string path)
{
    System.IO.File.WriteAllBytes(path, bytesA);
    System.IO.File.SetLastWriteTimeUtc(path, System.DateTime.UtcNow.AddHours(-1));
}
System.Collections.Generic.List<string> Own(string dir)
{
    var found = new System.Collections.Generic.List<string>();
    if (System.IO.Directory.Exists(dir))
    {
        foreach (string file in System.IO.Directory.GetFiles(dir, prefix + "*")) found.Add(file.Replace('\\', '/'));
    }
    return found;
}
string Newest(string dir)
{
    if (!System.IO.Directory.Exists(dir)) return "";
    System.IO.FileSystemInfo newest = null;
    foreach (var info in new System.IO.DirectoryInfo(dir).GetFileSystemInfos())
    {
        if (newest == null || info.CreationTimeUtc > newest.CreationTimeUtc) newest = info;
    }
    return newest == null ? "" : newest.FullName.Replace('\\', '/');
}
string Names(string dir)
{
    if (!System.IO.Directory.Exists(dir)) return "";
    var names = new System.Collections.Generic.List<string>();
    foreach (var info in new System.IO.DirectoryInfo(dir).GetFileSystemInfos()) names.Add(info.Name);
    names.Sort(System.StringComparer.Ordinal);
    return string.Join("|", names.ToArray());
}

report.AppendLine("start Failed/=" + (System.IO.Directory.Exists(failedDir) ? System.IO.Directory.GetFileSystemEntries(failedDir).Length : 0)
    + " OutputBackups/=" + (System.IO.Directory.Exists(backupDir) ? System.IO.Directory.GetFileSystemEntries(backupDir).Length : 0));
string error;
bool problem;
try
{
    // a) A build that fails after writing its output: the previous file comes back and the failed one is kept.
    string pa = PathOf("a");
    WriteA(pa);
    var ga = StudioRaming.WarudoBatchBuilder.WbbJournal.GuardOutput(pa, System.DateTime.UtcNow.AddSeconds(-2), out error);
    Check("a guard made a backup", ga != null && error == "" && ga.backupReady && System.IO.File.Exists(ga.backupPath), ga == null ? error : ga.backupPath);
    if (ga != null)
    {
        System.IO.File.WriteAllBytes(pa, bytesB);
        string message = StudioRaming.WarudoBatchBuilder.WbbJournal.RevertOutput(ga, out problem);
        Check("a path holds A", Holds(pa, bytesA), "");
        Check("a newest Failed/ entry holds B", ga.failedPath.Length > 0 && Holds(ga.failedPath, bytesB)
            && string.Equals(Newest(failedDir), ga.failedPath.Replace('\\', '/'), System.StringComparison.OrdinalIgnoreCase), ga.failedPath);
        Check("a own backup gone", Own(backupDir).Count == 0, string.Join(", ", Own(backupDir).ToArray()));
        Check("a message is Runner.FailedOutputRestored", !problem && message == StudioRaming.WarudoBatchBuilder.WbbLang.T("Runner.FailedOutputRestored", ga.failedPath), message);
    }

    // b) A build that fails after deleting the output: the previous file comes back.
    string pb = PathOf("b");
    WriteA(pb);
    var gb = StudioRaming.WarudoBatchBuilder.WbbJournal.GuardOutput(pb, System.DateTime.UtcNow.AddSeconds(-2), out error);
    Check("b guard made a backup", gb != null && error == "" && gb.backupReady, gb == null ? error : "");
    if (gb != null)
    {
        System.IO.File.Delete(pb);
        string message = StudioRaming.WarudoBatchBuilder.WbbJournal.RevertOutput(gb, out problem);
        Check("b path holds A", Holds(pb, bytesA), message);
    }

    // c) A build that succeeds: the new file stays and the backup leaves OutputBackups.
    string pc = PathOf("c");
    WriteA(pc);
    var gc = StudioRaming.WarudoBatchBuilder.WbbJournal.GuardOutput(pc, System.DateTime.UtcNow.AddSeconds(-2), out error);
    Check("c guard made a backup", gc != null && error == "" && gc.backupReady, gc == null ? error : "");
    if (gc != null)
    {
        System.IO.File.WriteAllBytes(pc, bytesB);
        StudioRaming.WarudoBatchBuilder.WbbJournal.MarkOutputVerified(gc);
        StudioRaming.WarudoBatchBuilder.WbbJournal.CommitOutput(gc);
        Check("c path holds B", Holds(pc, bytesB), "");
        Check("c own backup gone", !System.IO.File.Exists(gc.backupPath) && Own(backupDir).Count == 0, gc.backupPath);
        // Today the backup is deleted; with ri-dc8e41c4 it moves to Previous/ and this count becomes 1.
        report.AppendLine("INFO c Previous/ entries for this run: " + Own(previousDir).Count);
    }

    // d) A first build of this name that fails: nothing to restore, the failed output is kept.
    string pd = PathOf("d");
    var gd = StudioRaming.WarudoBatchBuilder.WbbJournal.GuardOutput(pd, System.DateTime.UtcNow.AddSeconds(-2), out error);
    Check("d guard without a previous file", gd != null && error == "" && gd.backupPath == "", gd == null ? error : gd.backupPath);
    if (gd != null)
    {
        System.IO.File.WriteAllBytes(pd, bytesB);
        string message = StudioRaming.WarudoBatchBuilder.WbbJournal.RevertOutput(gd, out problem);
        Check("d path gone", !System.IO.File.Exists(pd), "");
        Check("d Failed/ holds B", gd.failedPath.Length > 0 && Holds(gd.failedPath, bytesB), gd.failedPath);
        Check("d message is Runner.FailedOutputMoved", !problem && message == StudioRaming.WarudoBatchBuilder.WbbLang.T("Runner.FailedOutputMoved", gd.failedPath), message);
    }

    // e) A build that fails before writing anything: the previous file is left as it is and nothing is kept as failed.
    string pe = PathOf("e");
    WriteA(pe);
    string failedBefore = Names(failedDir);
    var ge = StudioRaming.WarudoBatchBuilder.WbbJournal.GuardOutput(pe, System.DateTime.UtcNow.AddSeconds(-2), out error);
    Check("e guard made a backup", ge != null && error == "" && ge.backupReady, ge == null ? error : "");
    if (ge != null)
    {
        string message = StudioRaming.WarudoBatchBuilder.WbbJournal.RevertOutput(ge, out problem);
        Check("e path holds A", Holds(pe, bytesA), "");
        Check("e Failed/ unchanged", Names(failedDir) == failedBefore && ge.failedPath == "", ge.failedPath);
        Check("e no message", !problem && message == "", message);
    }
}
catch (System.Exception e)
{
    Check("no exception", false, e.GetType().Name + ": " + e.Message);
}
finally
{
    // Library folders go back to what they held at the start; this run's files end up next to its outputs.
    string leftovers = work + "/leftovers";
    foreach (string dir in new[] { failedDir, backupDir, previousDir })
    {
        foreach (string file in Own(dir))
        {
            System.IO.Directory.CreateDirectory(leftovers);
            string target = leftovers + "/" + System.IO.Path.GetFileName(dir) + "_" + System.IO.Path.GetFileName(file);
            try { System.IO.File.Move(file, target); }
            catch (System.Exception e) { report.AppendLine("CLEANUP could not move " + file + ": " + e.Message); }
        }
    }
}

report.AppendLine("end Failed/=" + (System.IO.Directory.Exists(failedDir) ? System.IO.Directory.GetFileSystemEntries(failedDir).Length : 0)
    + " OutputBackups/=" + (System.IO.Directory.Exists(backupDir) ? System.IO.Directory.GetFileSystemEntries(backupDir).Length : 0) + " leftovers in " + work);
return report.ToString().TrimEnd() + "\nVERDICT " + (failures > 0 ? "FAIL (" + failures + " check(s))" : "PASS") + " RunTag=" + RunTag;
