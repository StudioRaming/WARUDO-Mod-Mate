// WMM WARUDO root resolution (hub item ri-008af9de), step 4 of 5. lab_exec_file statement body, QA project only.
// Unconditional clean-up after the cases (or after an aborted run): restores the test switch, optionally clears the WARUDO folder
// setting this project holds, and removes the fixture folder (Assets/WmmRootFixture) from the QA project. Receipts, entry lists
// and archives in OutDir are left for the report. Does not touch WARUDO or the Steam registry: an output folder left inside WARUDO
// by an aborted case is listed, not deleted.
const string QaRoot = "";                    // PARAMETER: the QA project root
const bool ClearSetting = false;             // PARAMETER: true clears the per-project WARUDO folder setting (use after a run that died in case C)
const bool RemoveFixture = true;             // PARAMETER: false keeps Assets/WmmRootFixture for another round
const string WarudoRoot = "";                // PARAMETER: optional, only to list leftovers under <WarudoRoot>/Warudo_Data/StreamingAssets/Props
if (string.IsNullOrEmpty(QaRoot)) return "ABORT set QaRoot (PARAMETER constant at the top) first";
if (UnityEngine.Application.dataPath != QaRoot + "/Assets") return "ABORT not the QA project: " + UnityEngine.Application.dataPath;
if (StudioRaming.WarudoBatchBuilder.WbbBuildRunner.AnyRunning) return "ABORT a WMM build is running";

var lines = new System.Text.StringBuilder();
StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.DisableSteamLookupForTests = false;
StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.Invalidate();
lines.AppendLine("switch restored (DisableSteamLookupForTests = false)");
if (ClearSetting)
{
    StudioRaming.WarudoBatchBuilder.WbbPrefs.DeleteKey(StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.RootPrefKey);
    lines.AppendLine("WARUDO folder setting cleared");
}
else
{
    lines.AppendLine("WARUDO folder setting now: '" + StudioRaming.WarudoBatchBuilder.WbbPrefs.GetString(StudioRaming.WarudoBatchBuilder.WbbWarudoLocator.RootPrefKey, "") + "'");
}

if (RemoveFixture)
{
    const string FixtureRoot = "Assets/WmmRootFixture";
    if (System.IO.Directory.Exists(QaRoot + "/" + FixtureRoot))
        lines.AppendLine(UnityEditor.AssetDatabase.DeleteAsset(FixtureRoot) ? "removed " + FixtureRoot : "FAILED to remove " + FixtureRoot);
    else
        lines.AppendLine("no fixture folder");
    UnityEditor.AssetDatabase.Refresh();
}

if (!string.IsNullOrEmpty(WarudoRoot))
{
    string props = WarudoRoot.Replace('\\', '/').TrimEnd('/') + "/Warudo_Data/StreamingAssets/Props";
    if (System.IO.Directory.Exists(props))
    {
        foreach (string dir in System.IO.Directory.GetDirectories(props, "WmmRootFix_*"))
            lines.AppendLine("LEFTOVER inside WARUDO (delete by hand if no case is running): " + dir.Replace('\\', '/'));
    }
}

return lines.ToString().TrimEnd();
