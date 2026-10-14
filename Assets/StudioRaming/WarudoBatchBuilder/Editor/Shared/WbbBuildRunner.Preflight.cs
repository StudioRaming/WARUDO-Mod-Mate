using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildRunner
    {
        public static List<WbbPreflightIssue> Preflight(IList<WbbBuildItem> items, WbbJob job)
        {
            var issues = new List<WbbPreflightIssue>();
            if (items == null || items.Count == 0)
                issues.Add(new WbbPreflightIssue(true, L("Runner.NoSelection"), L("Runner.NoSelection.hint")));
            if (job == null)
            {
                issues.Add(new WbbPreflightIssue(true, L("Runner.NoBuildFolder"), L("Runner.NoBuildFolder.hint")));
                return issues;
            }

            WbbExportType type = ResolveExportType(job);
            if (type == WbbExportType.Environment)
                issues.Add(Coded(true, L("Safety.EnvironmentUnsupported"), L("Safety.EnvironmentUnsupported.hint"), WbbPreflightCodes.EnvironmentUnsupported));
            string buildFolder = WbbUtil.ResolveBuildFolder(job, type);
            if (string.IsNullOrEmpty(buildFolder))
                issues.Add(new WbbPreflightIssue(false, L("Runner.TempFolderNotice"), L("Runner.TempFolderNotice.hint")));
            string prefabFolder = WbbUtil.ResolveFolder(job.prefabFolderGuid, job.prefabFolderPath);
            if (string.IsNullOrEmpty(prefabFolder))
                issues.Add(new WbbPreflightIssue(true, L("Runner.NoPrefabFolder"), L("Runner.NoPrefabFolder.hint")));
            if (!string.IsNullOrEmpty(buildFolder) && !string.IsNullOrEmpty(prefabFolder) && string.Equals(buildFolder, prefabFolder, System.StringComparison.OrdinalIgnoreCase))
                issues.Add(new WbbPreflightIssue(true, L("Runner.SameFolder"), L("Safety.SameFolder.hint")));
            string outputDirectory = ResolveOutputDirectory(job);
            bool outputUsable = IsUsableOutputPath(outputDirectory);
            string warudoRoot;
            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputUsable = false;
                issues.Add(Coded(true, L("Safety.WarudoNotFound"), L("Safety.WarudoNotFound.hint"), WbbPreflightCodes.WarudoNotFound));
            }
            else if (!outputUsable)
            {
                issues.Add(new WbbPreflightIssue(true, L("Runner.OutputPathInvalid", outputDirectory), L("Runner.OutputPathInvalid.hint")));
            }
            else if (WbbWarudoLocator.IsInsideWarudoData(outputDirectory, out warudoRoot) && !Directory.Exists(warudoRoot))
            {
                outputUsable = false;
                issues.Add(Coded(true, L("Safety.WarudoFolderMissing", outputDirectory), L("Safety.WarudoNotFound.hint"), WbbPreflightCodes.WarudoNotFound));
            }
            else if (!OutputDirectoryExists(outputDirectory))
            {
                issues.Add(new WbbPreflightIssue(false, L("Runner.OutputFolderMissing", outputDirectory), L("Runner.OutputFolderMissing.hint")));
            }

            string warudoSource;
            string warudoStreaming = WbbWarudoLocator.ResolveForOutput(outputDirectory, out warudoSource);
#if WBB_WARUDO_SDK
            // Automatic script mode checks every type against WARUDO's own assemblies; without them each item would fail.
            if (!job.manualScriptFolder && outputUsable && warudoStreaming.Length == 0)
                issues.Add(Coded(true, L("Safety.WarudoManagedMissing"), L("Safety.WarudoManagedMissing.hint"), WbbPreflightCodes.WarudoManagedMissing));
#endif
            string error;
            if (!WbbExportSettings.ValidateAvailable(out error))
                issues.Add(new WbbPreflightIssue(true, L("Runner.NoUMod"), L("Runner.NoUMod.hint")));
            string healthDetail;
            WbbSettingsHealth health = WbbExportSettingsGuard.CheckHealth(out healthDetail);
            if (health == WbbSettingsHealth.Missing)
            {
                issues.Add(Coded(true, L("Runner.NoExportSettings"), L("Runner.NoExportSettings.hint"), WbbPreflightCodes.SettingsMissing));
            }
            else if (health == WbbSettingsHealth.Broken)
            {
                string backup = WbbExportSettingsGuard.NewestValidBackup();
                string countError;
                // A readable asset with no profile is not damaged, it is new: the SDK's own create call adds the profile.
                if (WbbExportSettings.GetProfileCount(out countError) == 0 && string.IsNullOrEmpty(backup))
                    issues.Add(Coded(true, L("Runner.NoProfiles"), L("Runner.NoProfiles.hint"), WbbPreflightCodes.SettingsNoProfile));
                else
                    issues.Add(Coded(true, L("Safety.SettingsBroken"), string.IsNullOrEmpty(backup) ? L("Safety.SettingsBroken.hintNoBackup") : L("Safety.SettingsBroken.hint", BackupLabel(backup)), WbbPreflightCodes.SettingsBroken, backup));
            }

            if (WbbExportSettings.IsUModBuilding() && !AnyRunning)
                issues.Add(Coded(true, L("Safety.UModBusy"), L("Safety.UModBusy.hint"), WbbPreflightCodes.UModBusy));
            string sceneName;
            if (WbbSceneAutoSave.HasDirtyUntitledScene(out sceneName))
                issues.Add(Coded(true, L("Safety.UntitledScene", sceneName), L("Safety.UntitledScene.hint"), WbbPreflightCodes.UntitledScene));
            string stagePath;
            if (WbbSceneAutoSave.HasDirtyPrefabStage(out stagePath))
                issues.Add(Coded(true, L("Safety.PrefabStageDirty", stagePath), L("Safety.PrefabStageDirty.hint"), WbbPreflightCodes.PrefabStageDirty));
            if (!AnyRunning && WbbJournal.Exists && WbbJournal.Current == null)
            {
                if (WbbJournal.SettingsRestorePending)
                    issues.Add(Coded(true, L("Safety.JournalPending"), L("Safety.JournalBlocksStart"), WbbPreflightCodes.JournalRestoreFailed));
                else
                    issues.Add(Coded(false, L("Safety.JournalPending"), L("Safety.JournalPending.hint"), WbbPreflightCodes.JournalPending));
            }

            string setAside = AnyRunning ? "" : WbbJournal.UnacknowledgedSetAside;
            if (setAside.Length > 0)
                issues.Add(Coded(true, L("Safety.JournalSetAsidePending", setAside), L("Safety.JournalSetAsidePending.hint"), WbbPreflightCodes.JournalSetAside));
            if (WbbDatabase.LoadFailed)
                issues.Add(Coded(false, L("Safety.DatabaseLoadFailed"), L("Safety.DatabaseLoadFailed.hint"), WbbPreflightCodes.DatabaseLoadFailed));
            bool versionFixed;
            string version = NormalizeModVersion(job.modVersion, out versionFixed);
            if (version.Length == 0)
                issues.Add(Coded(true, L("Safety.VersionInvalid", job.modVersion ?? ""), L("Safety.VersionInvalid.hint"), WbbPreflightCodes.VersionInvalid));
            else if (versionFixed)
                issues.Add(Coded(false, L("Safety.VersionAutoFixed", version), L("Safety.VersionInvalid.hint"), WbbPreflightCodes.VersionAutoFixed, version));
            string typeName = type.ToString();
            if (!string.IsNullOrEmpty(buildFolder))
            {
                WbbScriptFolderScan scan = WbbScriptFolderScan.Run(buildFolder);
                if (scan.asmdefAssemblies.Count > 0 || scan.plainScriptAssets.Count > 0)
                {
                    // Runtime code of the folder (assembly definitions and plain scripts) is compiled by Mod Mate itself;
                    // what stands in the way is reported here.
                    WbbAsmdefAnalysis asmdef = WbbAsmdefAnalysis.ForFolder(scan, warudoStreaming);
                    for (int i = 0; i < asmdef.issues.Count; i++)
                        issues.Add(Coded(asmdef.issues[i].blocking, asmdef.issues[i].message, asmdef.issues[i].detail, asmdef.issues[i].code));
                    // Without WARUDO's assemblies the names and references of this code are not compared with them.
                    if (warudoStreaming.Length == 0)
                        issues.Add(Coded(false, L("Safety.WarudoManagedSkipped"), L("Safety.WarudoManagedSkipped.hint"), WbbPreflightCodes.WarudoManagedSkipped));
                }

                if (scan.dlls.Count > 0)
                    issues.Add(Coded(true, L("Safety.DllInScriptFolder", scan.dlls.Count), L("Safety.DllInScriptFolder.hint", scan.dlls[0]), WbbPreflightCodes.DllInScriptFolder));
                if (scan.unsupported.Count > 0)
                    issues.Add(Coded(false, L("Safety.UnsupportedFiles", scan.unsupported.Count), L("Safety.UnsupportedFiles.hint", scan.unsupported[0]), WbbPreflightCodes.UnsupportedFiles));
                string leftover = WbbUtil.Combine(buildFolder, typeName + ".prefab");
                if (File.Exists(WbbUtil.ToAbsolutePath(leftover)))
                    issues.Add(new WbbPreflightIssue(true, L("Runner.LeftoverPrefab", leftover), L("Safety.LeftoverPrefab.hint")));
                try
                {
                    string absoluteBuildFolder = WbbUtil.ToAbsolutePath(buildFolder);
                    if (Directory.Exists(absoluteBuildFolder))
                    {
                        string[] existing = Directory.GetFiles(absoluteBuildFolder, "*.prefab", SearchOption.TopDirectoryOnly);
                        if (existing != null && existing.Length > 0)
                            issues.Add(new WbbPreflightIssue(false, L("Runner.PrefabsInBuildFolder", existing.Length), L("Safety.PrefabsInBuildFolder.hint")));
                    }
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("Preflight could not list the script folder: " + e.Message);
                }
            }

            if (items != null && items.Count > 0)
            {
                var seen = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
                var reportedDuplicates = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
                int missingIcons = 0;
                int overwrites = 0;
                string overwriteSample = "";
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item == null)
                        continue;
                    string modName = string.IsNullOrEmpty(item.modName) ? item.prefabName : item.modName;
                    if (string.IsNullOrEmpty(modName) || !WbbUtil.IsSafeModName(modName))
                    {
                        issues.Add(new WbbPreflightIssue(true, L("Runner.UnsafeModName", string.IsNullOrEmpty(modName) ? item.prefabName : modName), L("Runner.UnsafeModName.hint")));
                    }
                    else if (seen.ContainsKey(modName))
                    {
                        if (!reportedDuplicates.ContainsKey(modName))
                        {
                            reportedDuplicates[modName] = 1;
                            issues.Add(new WbbPreflightIssue(true, L("Runner.DuplicateModName", modName), L("Runner.DuplicateModName.hint")));
                        }
                    }
                    else
                    {
                        seen[modName] = 1;
                    }

                    string prefabPath = WbbUtil.NormalizePath(item.prefabPath);
                    if (string.IsNullOrEmpty(prefabPath) && !string.IsNullOrEmpty(item.prefabGuid))
                        prefabPath = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(item.prefabGuid));
                    if (string.IsNullOrEmpty(prefabPath) || !File.Exists(WbbUtil.ToAbsolutePath(prefabPath)))
                    {
                        issues.Add(new WbbPreflightIssue(true, L("Runner.PrefabMissing", string.IsNullOrEmpty(prefabPath) ? item.prefabName : prefabPath), L("Runner.PrefabMissing.hint")));
                    }

                    if (item.icon == null)
                        missingIcons++;
                    if (outputUsable && !string.IsNullOrEmpty(modName))
                    {
                        string outputFile = WbbUtil.Combine(outputDirectory, WbbUtil.SanitizeModName(modName) + WbbUtil.ModExtension);
                        try
                        {
                            if (File.Exists(WbbUtil.ToAbsolutePath(outputFile)))
                            {
                                overwrites++;
                                if (string.IsNullOrEmpty(overwriteSample))
                                    overwriteSample = outputFile;
                            }
                        }
                        catch (System.Exception e)
                        {
                            WbbUtil.LogWarning("Preflight could not check '" + outputFile + "': " + e.Message);
                        }
                    }
                }

                if (overwrites > 0)
                    issues.Add(new WbbPreflightIssue(false, L("Runner.OutputExists", overwrites), L("Runner.OutputExists.hint", overwriteSample)));
                if (missingIcons > 0)
                    issues.Add(new WbbPreflightIssue(false, L("Runner.NoIcon", missingIcons), L("Runner.NoIcon.hint")));
            }

            if (string.IsNullOrEmpty(job.modAuthor) || job.modAuthor.Trim().Length == 0)
                issues.Add(new WbbPreflightIssue(false, L("Runner.NoAuthor"), L("Runner.NoAuthor.hint")));
            return issues;
        }

        private static WbbPreflightIssueEx Coded(bool blocking, string message, string detail, string code, string actionPath = "")
        {
            return new WbbPreflightIssueEx(blocking, message, detail, code, actionPath);
        }

        private static string BackupLabel(string backupPath)
        {
            string name = Path.GetFileNameWithoutExtension(backupPath) ?? "";
            const string prefix = "ExportSettings_";
            string stamp = name.StartsWith(prefix, System.StringComparison.Ordinal) ? name.Substring(prefix.Length) : name;
            System.DateTime time;
            if (stamp.Length >= 15 && System.DateTime.TryParseExact(stamp.Substring(0, 15), "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out time))
                return WbbUtil.ToIso(time);
            return name;
        }
    }
}
