// WMM WARUDO root resolution (hub item ri-008af9de), step 5 of 5 (check F). lab_exec_file statement body, read-only.
// Run once with the SDK (as installed) and once without it (remove WBB_WARUDO_SDK from the QA project's scripting defines, wait
// for the reload, run again; put the define back afterwards). Reports whether the last compile failed and counts `error CS` lines
// of Editor.log since the editor started this session. Expect: compileFailed=False, error CS count 0.
const string QaRoot = "";                    // PARAMETER: the QA project root
if (string.IsNullOrEmpty(QaRoot)) return "ABORT set QaRoot (PARAMETER constant at the top) first";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (UnityEditor.EditorApplication.isCompiling) return "RUNNING (editor is compiling)";

string defines = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Standalone);
bool sdkDefine = (";" + defines + ";").Contains(";WBB_WARUDO_SDK;");
int errors = 0;
string firstError = "";
try
{
    string logPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Unity/Editor/Editor.log");
    using (var stream = new System.IO.FileStream(logPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite | System.IO.FileShare.Delete))
    using (var reader = new System.IO.StreamReader(stream))
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.IndexOf("error CS", System.StringComparison.Ordinal) < 0) continue;
            errors++;
            if (firstError.Length == 0) firstError = line.Trim();
        }
    }
}
catch (System.Exception e) { return "ABORT could not read Editor.log: " + e.Message; }

bool failed = UnityEditor.EditorUtility.scriptCompilationFailed;
bool ok = !failed && errors == 0;
return (ok ? "PASS" : "FAIL") + " compileFailed=" + failed + " errorCS=" + errors + " WBB_WARUDO_SDK=" + sdkDefine + (firstError.Length > 0 ? " first=" + firstError : "");
