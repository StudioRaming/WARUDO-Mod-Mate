using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildRunner
    {
        private static string CheckStartBlockers(WbbJob newJob)
        {
            if (ResolveExportType(newJob) == WbbExportType.Environment)
                return L("Safety.EnvironmentUnsupported");
            string error;
            if (!WbbExportSettings.ValidateAvailable(out error))
                return error;
            if (WbbExportSettings.IsUModBuilding())
                return L("Safety.UModBusy");
            string sceneName;
            if (WbbSceneAutoSave.HasDirtyUntitledScene(out sceneName))
                return L("Safety.UntitledScene", sceneName);
            string stagePath;
            if (WbbSceneAutoSave.HasDirtyPrefabStage(out stagePath))
                return L("Safety.PrefabStageDirty", stagePath);
            string detail;
            WbbSettingsHealth health = WbbExportSettingsGuard.CheckHealth(out detail);
            if (health == WbbSettingsHealth.Missing)
                return L("Runner.NoExportSettings");
            if (health == WbbSettingsHealth.Broken)
                return L("Safety.SettingsBroken");
            bool autoFixed;
            if (NormalizeModVersion(newJob.modVersion, out autoFixed).Length == 0)
                return L("Safety.VersionInvalid", newJob.modVersion ?? "");
            return "";
        }

        private bool PrepareBatch(IList<WbbBuildItem> newItems, WbbJob newJob)
        {
            WbbExportType type = ResolveExportType(newJob);
            string userFolder = WbbUtil.ResolveBuildFolder(newJob, type);
            string outDir = ResolveOutputDirectory(newJob);
            if (string.IsNullOrEmpty(outDir))
            {
                SetStatus(L("Safety.WarudoNotFound"));
                return false;
            }

            string warudoRoot;
            if (WbbWarudoLocator.IsInsideWarudoData(outDir, out warudoRoot) && !Directory.Exists(warudoRoot))
            {
                SetStatus(L("Safety.WarudoFolderMissing", outDir));
                return false;
            }

            if (!IsUsableOutputPath(outDir))
            {
                SetStatus(L("Runner.OutputPathInvalid", outDir));
                return false;
            }

            if (!EnsureDirectory(outDir))
            {
                SetStatus(L("Runner.OutputFolderFail", outDir));
                return false;
            }

            string error;
            int lockedIndex = WbbExportSettings.GetActiveProfileIndex(out error);
            if (lockedIndex < 0)
            {
                SetStatus(L("Runner.ProfileSnapshotFailed", error));
                WbbUtil.LogError("Cannot resolve the active build settings profile index: " + error);
                return false;
            }

            WbbProfileSnapshot snapshot;
            if (!WbbExportSettings.TryReadActiveProfile(out snapshot, out error, lockedIndex))
            {
                SetStatus(L("Runner.ProfileSnapshotFailed", error));
                WbbUtil.LogError("Cannot snapshot the active build settings profile: " + error);
                return false;
            }

            List<string> references;
            if (!WbbExportSettings.TryReadReferencePaths(lockedIndex, out references, out error))
            {
                WbbUtil.LogWarning("Could not read referenced mods of the active profile: " + error);
                references = null;
            }

            WbbUModQuietState quiet;
            if (!WbbExportSettings.TryReadQuietState(out quiet, out error))
            {
                WbbUtil.LogWarning("Could not read the build log/console settings: " + error);
                quiet = new WbbUModQuietState();
            }

            string backupPath;
            if (!WbbExportSettingsGuard.Backup(out backupPath, out error))
                WbbUtil.LogWarning("Build settings backup skipped: " + error);
            WbbScriptFolderScan scan = string.IsNullOrEmpty(userFolder) ? null : WbbScriptFolderScan.Run(userFolder);
            items = newItems;
            job = newJob;
            typeName = type.ToString();
            outputDirectory = outDir;
            usingUserScriptFolder = !string.IsNullOrEmpty(userFolder);
            modFolder = usingUserScriptFolder ? userFolder : "";
            bool versionFixed;
            modVersion = NormalizeModVersion(newJob.modVersion, out versionFixed);
            if (versionFixed)
                WbbUtil.LogWarning("Mod version '" + (newJob.modVersion ?? "") + "' was completed to '" + modVersion + "'.");
            fixupOptions = WbbFixupOptions.FromJob(newJob);
            expectScripts = scan != null && scan.runtimeScripts > 0;
            originalProfile = snapshot.Clone();
            profileIndex = lockedIndex;
            profileWritten = false;
            originalReferencePaths = references;
            referencePathsCleared = false;
            originalQuiet = quiet;
            quietApplied = false;
            tempRoot = WbbUtil.Combine("Assets", WbbJournal.TempFolderPrefix + System.DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "_" + Random.Range(1000, 9999));
            var data = new WbbJournalData();
            data.tempRoot = tempRoot;
            data.tempFolders.Add(tempRoot);
            if (!usingUserScriptFolder)
                data.tempFolders.Add(tempRoot + "/Build");
            data.profileCaptured = true;
            data.profileIndex = profileIndex;
            data.modName = snapshot.modName;
            data.modAuthor = snapshot.modAuthor;
            data.modVersion = snapshot.modVersion;
            data.modDescription = snapshot.modDescription;
            data.modAssetPath = snapshot.modAssetPath;
            data.modExportPath = snapshot.modExportPath;
            WbbJournal.CaptureIcon(data, snapshot.modIcon);
            if (references != null)
            {
                data.referencePathsCaptured = true;
                data.referencePaths = new List<string>(references);
            }

            if (quiet.captured)
            {
                data.umodCaptured = true;
                data.clearConsoleOnBuild = quiet.clearConsoleOnBuild;
                data.showOutputDirectory = quiet.showOutputDirectory;
                data.logLevel = quiet.logLevel;
            }

            if (!WbbJournal.Begin(data))
            {
                SetStatus(L("Safety.JournalWriteFailed"));
                ClearBatchFields();
                return false;
            }

            string sceneError;
            WbbSceneAutoSave.SaveDirtyScenes(out sceneError);
            if (sceneError.Length > 0)
            {
                SetStatus(L("Safety.SceneSaveFailed", sceneError));
                WbbJournal.Complete();
                ClearBatchFields();
                return false;
            }

            sceneSetup = WbbSceneAutoSave.CaptureSetup();
            if (!CreateTempFolders())
            {
                SetStatus(L("Runner.TempFolderFailed"));
                DeleteTempFolders();
                WbbJournal.Complete();
                ClearBatchFields();
                return false;
            }

            if (references != null && references.Count > 0)
            {
                WbbJournal.Current.referencePathsCleared = true;
                WbbJournal.Save();
                if (WbbExportSettings.TryWriteReferencePaths(profileIndex, new List<string>(), out error))
                {
                    referencePathsCleared = true;
                    WbbUtil.Log("Referenced mods of the active profile are cleared for this batch (" + references.Count + ") and will be restored afterwards.");
                }
                else
                {
                    WbbUtil.LogWarning("Could not clear referenced mods for this batch: " + error);
                }
            }

            if (quiet.captured)
            {
                WbbJournal.Current.umodWritten = true;
                WbbJournal.Save();
                if (WbbExportSettings.TryWriteQuietState(WbbExportSettings.QuietFor(quiet), out error))
                    quietApplied = true;
                else
                    WbbUtil.LogWarning("Could not switch off console clearing / output folder popup: " + error);
            }

            return true;
        }

        private void ClearBatchFields()
        {
            items = null;
            job = null;
            originalProfile = null;
            profileIndex = -1;
            profileWritten = false;
            originalReferencePaths = null;
            referencePathsCleared = false;
            quietApplied = false;
            tempRoot = "";
            modFolder = "";
            sceneSetup = null;
        }

        private bool CreateTempFolders()
        {
            try
            {
                string name = tempRoot.Substring("Assets/".Length);
                string guid = AssetDatabase.CreateFolder("Assets", name);
                string created = string.IsNullOrEmpty(guid) ? "" : WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(guid));
                if (!WbbUtil.IsValidProjectFolder(created))
                    return false;
                if (!string.Equals(created, tempRoot, System.StringComparison.Ordinal))
                {
                    tempRoot = created;
                    WbbJournal.Current.tempRoot = created;
                    WbbJournal.Current.tempFolders[0] = created;
                    WbbJournal.Save();
                }

                if (!usingUserScriptFolder)
                {
                    string buildGuid = AssetDatabase.CreateFolder(tempRoot, "Build");
                    string buildPath = string.IsNullOrEmpty(buildGuid) ? "" : WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(buildGuid));
                    if (!WbbUtil.IsValidProjectFolder(buildPath))
                        return false;
                    modFolder = buildPath;
                    if (WbbJournal.Current.tempFolders.Count > 1)
                        WbbJournal.Current.tempFolders[1] = buildPath;
                    else
                        WbbJournal.Current.tempFolders.Add(buildPath);
                    WbbJournal.Save();
                    WbbUtil.Log("No custom script folder set. Building from a temporary folder: " + buildPath);
                }

                return true;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogError("Failed to create the temporary build folder: " + e.Message);
                return false;
            }
        }

        public void AbortAndRestore()
        {
            if (!running)
                return;
            cancelRequested = true;
            try
            {
                StopCapture();
                if (!restoreFailed && !string.IsNullOrEmpty(copyPath))
                    DeleteCopy();
                var item = CurrentItem();
                if (itemActive && item != null)
                {
                    if (item.status == WbbBuildStatus.Running || item.status == WbbBuildStatus.Pending)
                        MarkCurrent(WbbBuildStatus.Canceled, L("Runner.CanceledItem"));
                    SettleOutput();
                    FinishItem();
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogError("AbortAndRestore failed: " + e.Message);
            }

            FinishBatch(true);
        }

        private void FinishItem()
        {
            var item = CurrentItem();
            itemActive = false;
            StopCapture();
            if (item == null)
                return;
            double duration = EditorApplication.timeSinceStartup - itemStartTime;
            if (duration < 0d)
                duration = 0d;
            item.durationSec = duration;
            long size = 0L;
            if (item.status == WbbBuildStatus.Success)
            {
                size = outputSizeBytes;
                if (size <= 0L)
                {
                    try
                    {
                        string absolute = WbbUtil.ToAbsolutePath(outputFilePath);
                        if (File.Exists(absolute))
                            size = new FileInfo(absolute).Length;
                    }
                    catch (System.Exception e)
                    {
                        WbbUtil.LogWarning("Failed to stat output file: " + e.Message);
                    }
                }
            }

            switch (item.status)
            {
                case WbbBuildStatus.Success:
                    successCount++;
                    break;
                case WbbBuildStatus.Failed:
                    failedCount++;
                    break;
                default:
                    skippedCount++;
                    break;
            }

            bool fromScene = !string.IsNullOrEmpty(item.sourceHierarchyPath);
            var entry = new WbbRecord();
            entry.id = string.IsNullOrEmpty(itemRecordId) ? WbbUtil.NewId() : itemRecordId;
            entry.startedIso = itemStartIso;
            entry.finishedIso = WbbUtil.NowIso();
            entry.durationSec = duration;
            entry.modName = currentModName;
            entry.exportType = job != null ? job.exportType : (int)WbbExportType.Prop;
            entry.modVersion = job != null ? modVersion : "";
            entry.modAuthor = job != null ? job.modAuthor : "";
            entry.prefabGuid = fromScene ? "" : item.prefabGuid;
            entry.prefabPath = fromScene ? "" : originalPrefabPath;
            entry.prefabName = item.prefabName;
            entry.outputDirectory = outputDirectory;
            entry.outputFilePath = outputFilePath;
            entry.outputSizeBytes = size;
            entry.status = (int)item.status;
            entry.message = item.message == null ? "" : item.message;
            entry.jobId = job != null ? job.id : "";
            entry.jobName = job != null ? job.name : "";
            entry.sourceSignature = fromScene ? "" : sourceSignature;
            entry.sourceObjectId = item.sourceObjectId == null ? "" : item.sourceObjectId;
            entry.sourceScenePath = item.sourceScenePath == null ? "" : item.sourceScenePath;
            entry.sourceHierarchyPath = item.sourceHierarchyPath == null ? "" : item.sourceHierarchyPath;
            entry.fixupReport = itemFixupReport ?? "";
            entry.buildLogPath = itemBuildLogPath ?? "";
            try
            {
                WbbDatabase.AddRecord(entry);
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Failed to store build record: " + e.Message);
            }

            var handler = RecordFinished;
            if (handler != null)
            {
                try
                {
                    handler(entry);
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("RecordFinished handler threw: " + e.Message);
                }
            }
        }

        private void FinishBatch(bool canceled)
        {
            step = Step.Idle;
            running = false;
            itemActive = false;
            HookUpdate(false);
            StopCapture();
            MarkRemainingSkipped();
            bool clean = true;
            clean &= RestoreProfile();
            clean &= RestoreReferencePaths();
            clean &= RestoreQuietState();
            clean &= DeleteTempFolders();
            clean &= !WbbJournal.HasPendingCopies;
            clean &= !WbbJournal.HasPendingOutputs;
            if (!AbortingSilently)
                WbbSceneAutoSave.RestoreSetupIfChanged(sceneSetup);
            sceneSetup = null;
            if (clean)
            {
                WbbJournal.Complete();
            }
            else
            {
                WbbJournal.Release();
                WbbUtil.LogWarning("Some build cleanup steps did not finish; they will be retried before the next build " + "or the next time the editor loads.");
            }

            if (activeRunner == this)
                activeRunner = null;
            currentModName = "";
            SetStatus(canceled ? L("Runner.Canceled", successCount, failedCount, skippedCount) : L("Runner.Done", successCount, failedCount));
            WbbUtil.Log("Batch build finished. success=" + successCount + " failed=" + failedCount + " skipped=" + skippedCount + " canceled=" + canceled);
            var handler = Finished;
            if (handler != null)
            {
                try
                {
                    handler(canceled);
                }
                catch (System.Exception e)
                {
                    WbbUtil.LogWarning("Finished handler threw: " + e.Message);
                }
            }

            RequestRepaint();
        }

        private void EmergencyStop(string message)
        {
            running = false;
            step = Step.Idle;
            itemActive = false;
            HookUpdate(false);
            try
            {
                StopCapture();
            }
            catch (System.Exception ex)
            {
                WbbUtil.LogError("StopCapture failed: " + ex.Message);
            }

            try
            {
                MarkRemainingSkipped();
            }
            catch (System.Exception ex)
            {
                WbbUtil.LogError("MarkRemainingSkipped failed: " + ex.Message);
            }

            try
            {
                RestoreProfile();
            }
            catch (System.Exception ex)
            {
                WbbUtil.LogError("RestoreProfile failed: " + ex.Message);
            }

            try
            {
                RestoreReferencePaths();
            }
            catch (System.Exception ex)
            {
                WbbUtil.LogError("RestoreReferencePaths failed: " + ex.Message);
            }

            try
            {
                RestoreQuietState();
            }
            catch (System.Exception ex)
            {
                WbbUtil.LogError("RestoreQuietState failed: " + ex.Message);
            }

            try
            {
                WbbJournal.Release();
            }
            catch (System.Exception ex)
            {
                WbbUtil.LogError("Journal release failed: " + ex.Message);
            }

            if (activeRunner == this)
                activeRunner = null;
            currentModName = "";
            try
            {
                SetStatus(message);
            }
            catch (System.Exception)
            {
            }

            var handler = Finished;
            if (handler != null)
            {
                try
                {
                    handler(true);
                }
                catch (System.Exception ex)
                {
                    WbbUtil.LogWarning("Finished handler threw: " + ex.Message);
                }
            }
        }

        private void MarkRemainingSkipped()
        {
            if (items == null)
                return;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null)
                    continue;
                if (item.status != WbbBuildStatus.Pending)
                    continue;
                item.status = WbbBuildStatus.Skipped;
                if (string.IsNullOrEmpty(item.message))
                    item.message = L("Runner.SkippedItem");
                skippedCount++;
            }
        }

        private bool DeleteTempFolders()
        {
            if (string.IsNullOrEmpty(tempRoot))
                return true;
            string folder = tempRoot;
            try
            {
                if (!WbbJournal.IsTempFolderPath(folder) || !WbbUtil.IsValidProjectFolder(folder))
                {
                    tempRoot = "";
                    return true;
                }

                if (!WbbJournal.IsEffectivelyEmpty(folder))
                {
                    WbbUtil.LogWarning("Temporary build folder is not empty, leaving it in place: " + folder);
                    return false;
                }

                if (!AssetDatabase.DeleteAsset(folder))
                {
                    WbbUtil.LogWarning("Could not remove the temporary build folder: " + folder);
                    return false;
                }

                tempRoot = "";
                return true;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Temporary build folder cleanup failed: " + e.Message);
                return false;
            }
        }

        private bool RestoreProfile()
        {
            if (originalProfile == null)
                return true;
            if (!profileWritten)
            {
                originalProfile = null;
                return true;
            }

            string error;
            bool ok = WbbExportSettings.TryWriteActiveProfile(originalProfile, out error, profileIndex);
            if (ok)
                WbbUtil.Log(L("Runner.ProfileRestored"));
            else
                WbbUtil.LogError(L("Runner.ProfileRestoreFailed", error));
            if (ok)
            {
                originalProfile = null;
                profileWritten = false;
                if (WbbJournal.Current != null)
                {
                    WbbJournal.Current.profileWritten = false;
                    WbbJournal.Save();
                }
            }

            return ok;
        }

        private bool RestoreReferencePaths()
        {
            if (!referencePathsCleared || originalReferencePaths == null)
                return true;
            string error;
            bool ok = WbbExportSettings.TryWriteReferencePaths(profileIndex, originalReferencePaths, out error);
            if (!ok)
            {
                WbbUtil.LogError("Could not restore referenced mods of the build settings: " + error);
                return false;
            }

            referencePathsCleared = false;
            if (WbbJournal.Current != null)
            {
                WbbJournal.Current.referencePathsCleared = false;
                WbbJournal.Save();
            }

            return true;
        }

        private bool RestoreQuietState()
        {
            if (!quietApplied)
                return true;
            string error;
            bool ok = WbbExportSettings.TryWriteQuietState(originalQuiet, out error);
            if (!ok)
            {
                WbbUtil.LogError("Could not restore the console/output-folder build options: " + error);
                return false;
            }

            quietApplied = false;
            if (WbbJournal.Current != null)
            {
                WbbJournal.Current.umodWritten = false;
                WbbJournal.Save();
            }

            return true;
        }
    }
}
