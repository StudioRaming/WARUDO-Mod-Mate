using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildRunner
    {
        private void DoPrepare()
        {
            if (items == null || index < 0 || index >= items.Count)
            {
                step = Step.Done;
                return;
            }

            var item = items[index];
            if (item == null)
            {
                index++;
                return;
            }

            ResetItemState();
            itemActive = true;
            itemStartTime = EditorApplication.timeSinceStartup;
            itemStartIso = WbbUtil.NowIso();
            itemRecordId = WbbUtil.NewId();
            originalPrefabPath = WbbUtil.NormalizePath(item.prefabPath);
            if (string.IsNullOrEmpty(originalPrefabPath) && !string.IsNullOrEmpty(item.prefabGuid))
                originalPrefabPath = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(item.prefabGuid));
            string rawName = string.IsNullOrEmpty(item.modName) ? item.prefabName : item.modName;
            currentModName = WbbUtil.SanitizeModName(rawName);
            outputFilePath = WbbUtil.Combine(outputDirectory, currentModName + WbbUtil.ModExtension);
            sourceSignature = item.disposableCopy ? "" : WbbUtil.SourceSignature(originalPrefabPath);
            item.status = WbbBuildStatus.Running;
            item.message = "";
            if (string.IsNullOrEmpty(originalPrefabPath) || !File.Exists(WbbUtil.ToAbsolutePath(originalPrefabPath)))
            {
                Fail(L("Runner.PrefabMissing", string.IsNullOrEmpty(originalPrefabPath) ? item.prefabName : originalPrefabPath));
                step = Step.Cleanup;
                return;
            }

            if (string.IsNullOrEmpty(currentModName))
            {
                Fail(L("Runner.UnsafeModName", rawName));
                step = Step.Cleanup;
                return;
            }

            SetStatus(L("Runner.Preparing", currentModName));
            step = Step.Copy;
            Delay(StepDelaySeconds);
        }

        private void DoCopy()
        {
            SetStatus(L("Safety.Copying", currentModName));
            var item = CurrentItem();
            if (item != null && item.disposableCopy)
            {
                copyPath = originalPrefabPath;
                copyGuid = AssetDatabase.AssetPathToGUID(copyPath);
                itemFixupReport = item.fixupReport ?? "";
                WbbJournal.TrackCopy(copyPath, copyGuid);
                step = Step.Stage;
                Delay(StepDelaySeconds);
                return;
            }

            string target = UniqueCopyPath();
            WbbJournal.TrackCopy(target, "");
            WbbFixupReport report;
            string error;
            if (!WbbSnapshot.SaveFromPrefabAsset(originalPrefabPath, target, fixupOptions, out report, out error))
            {
                copyPath = File.Exists(WbbUtil.ToAbsolutePath(target)) ? target : "";
                copyGuid = copyPath.Length > 0 ? AssetDatabase.AssetPathToGUID(copyPath) : "";
                if (copyPath.Length == 0)
                    WbbJournal.ForgetCopy(target, "");
                Fail(L("Safety.CopyFailed", error));
                step = Step.Cleanup;
                return;
            }

            copyPath = target;
            copyGuid = AssetDatabase.AssetPathToGUID(target);
            WbbJournal.TrackCopy(copyPath, copyGuid);
            itemFixupReport = report.ToText();
            if (!report.IsEmpty)
                WbbUtil.Log("Build copy of " + originalPrefabPath + " was tidied: " + itemFixupReport.Replace('\n', ' '));
            step = Step.Stage;
            Delay(StepDelaySeconds);
        }

        private void DoStage()
        {
            SetStatus(L("Safety.Staging", currentModName));
            string target = WbbUtil.Combine(modFolder, typeName + ".prefab");
            if (!string.Equals(copyPath, target, System.StringComparison.OrdinalIgnoreCase) && File.Exists(WbbUtil.ToAbsolutePath(target)))
            {
                Fail(L("Runner.ErrorMove", L("Runner.ErrorNameCollision", target)));
                step = Step.Cleanup;
                return;
            }

            string error = AssetDatabase.MoveAsset(copyPath, target);
            if (!string.IsNullOrEmpty(error))
            {
                Fail(L("Runner.ErrorMove", error));
                step = Step.Cleanup;
                return;
            }

            copyPath = target;
            WbbJournal.TrackCopy(copyPath, copyGuid);
            step = Step.WriteSettings;
            Delay(StepDelaySeconds);
        }

        private void DoWriteSettings()
        {
            SetStatus(L("Safety.WritingSettings", currentModName));
            var item = CurrentItem();
            var snapshot = new WbbProfileSnapshot();
            snapshot.modName = currentModName;
            snapshot.modAuthor = job.modAuthor == null ? "" : job.modAuthor;
            snapshot.modVersion = modVersion;
            snapshot.modDescription = job.modDescription == null ? "" : job.modDescription;
            snapshot.modAssetPath = modFolder;
            snapshot.modExportPath = outputDirectory;
            snapshot.modIcon = (item != null && item.icon != null) ? (UnityEngine.Object)item.icon : (originalProfile != null ? originalProfile.modIcon : null);
            if (!profileWritten && WbbJournal.Current != null)
            {
                WbbJournal.Current.profileWritten = true;
                WbbJournal.Save();
            }

            profileWritten = true;
            string error;
            if (!WbbExportSettings.TryWriteActiveProfile(snapshot, out error, profileIndex))
            {
                Fail(L("Runner.ErrorSettings", error));
                step = Step.Cleanup;
                return;
            }

            step = Step.CompileRuntime;
            Delay(StepDelaySeconds);
        }

        // Runtime code in assembly definitions is compiled here into DLLs that the build step hands to UMod.
        // A mod without such code passes through this step without doing anything.
        private void DoCompileRuntime()
        {
            if (!compileStarted)
            {
                compileStarted = true;
                if (!PlanRuntimeUnits())
                    return;
                if (itemUnits.Count == 0)
                {
                    step = Step.InvokeBuild;
                    return;
                }

                if (compileSession == null)
                {
                    WbbRuntimeCompileSession.DeleteStaleRoots();
                    compileSession = new WbbRuntimeCompileSession();
                }

                SetStatus(L("Asmdef.Compiling", currentModName));
                compileSession.Begin(itemUnits);
            }

            string failure;
            switch (compileSession.Pump(out failure))
            {
                case WbbCompileState.Running:
                    Delay(PollIntervalSeconds);
                    return;
                case WbbCompileState.Failed:
                    Fail(failure);
                    step = Step.Cleanup;
                    return;
            }

            itemCompiled = compileSession.Resolve(itemUnits);
            var names = new List<string>();
            for (int i = 0; i < itemCompiled.Count; i++)
                names.Add(itemCompiled[i].name);
            AppendMessage(L("Asmdef.Compiled", itemCompiled.Count, string.Join(", ", names.ToArray())));
            WbbUtil.Log("Runtime assemblies for " + currentModName + ": " + string.Join(", ", names.ToArray()));
            step = Step.InvokeBuild;
            Delay(StepDelaySeconds);
        }

        // Which units this item needs. Manual script folder: the assemblies found in the folder at the start of the batch.
        // Automatic mode: the assemblies of the types the prefab uses; the plain project scripts it also needs
        // (if any) become one more unit so everything reaches UMod the same way.
        private bool PlanRuntimeUnits()
        {
            itemUnits = new List<WbbRuntimeUnit>();
            if (job.manualScriptFolder)
            {
                if (folderAnalysis != null)
                    itemUnits.AddRange(folderAnalysis.units);
                return true;
            }

            try
            {
                autoPlan = WbbRuntimeScriptPlan.Collect(copyPath);
            }
            catch (System.Exception e)
            {
                // Reported by the build step exactly like before.
                autoPlanError = e.Message;
                autoPlan = null;
                return true;
            }

            // A prefab that needs only plain project scripts keeps the automatic-source adapter's route when the SDK has
            // one. Everything else (assembly definitions, or an SDK without the adapter) is compiled here.
            if (autoPlan.asmdefAssemblies.Length == 0 && WbbRuntimeScriptPlan.AdapterAvailable)
                return true;
            WbbAsmdefAnalysis analysis = autoPlan.asmdefAssemblies.Length > 0 ? WbbAsmdefAnalysis.ForAssemblies(autoPlan.asmdefAssemblies) : new WbbAsmdefAnalysis();
            if (analysis.HasBlocking)
            {
                Fail(analysis.FirstBlockingMessage);
                step = Step.Cleanup;
                return false;
            }

            itemUnits.AddRange(analysis.units);
            if (autoPlan.sources.Length > 0)
            {
                var plain = new WbbRuntimeUnit();
                plain.name = WbbRuntimeUnit.PlainName;
                plain.plain = true;
                plain.allowUnsafe = true;
                plain.sources = autoPlan.sources;
                plain.defines = autoPlan.defines;
                plain.extraReferences = autoPlan.references;
                plain.projectReferences = analysis.units.ConvertAll(u => u.name).ToArray();
                plain.projectReferencePaths = new string[plain.projectReferences.Length];
                itemUnits.Add(plain);
            }

            return true;
        }

        private void DoInvokeBuild()
        {
            SetStatus(L("Runner.Building", currentModName));
            string sceneError;
            WbbSceneAutoSave.SaveDirtyScenes(out sceneError);
            string untitled;
            if (WbbSceneAutoSave.HasDirtyUntitledScene(out untitled))
            {
                Fail(L("Safety.UntitledScene", untitled));
                cancelRequested = true;
                step = Step.Cleanup;
                return;
            }

            buildStartUtc = System.DateTime.UtcNow.AddSeconds(-2d);
            lastSize = -1L;
            stableSamples = 0;
            outputSizeBytes = 0L;
            string backupError;
            outputGuard = WbbJournal.GuardOutput(WbbUtil.ToAbsolutePath(outputFilePath), buildStartUtc, out backupError);
            if (outputGuard == null)
            {
                Fail(L("Runner.OutputBackupFailed", backupError));
                step = Step.Cleanup;
                return;
            }

            WbbUtil.Log("Invoking UMod build (editor will block until this item finishes): " + currentModName);
            StopCapture();
            capture = new WbbLogCapture();
            capture.Start();
            WbbUModBuildHook hook = null;
            int hookRegistered = 0;
            try
            {
                // Only while this one synchronous build runs, so builds started from the SDK's own menu are never touched.
                if (itemCompiled.Count > 0)
                {
                    string hookError;
                    hook = WbbUModBuildHook.Install(currentModName, modFolder, itemCompiled, job.manualScriptFolder, out hookError);
                    if (hook == null)
                        throw new System.InvalidOperationException(hookError);
                }

                if (job.manualScriptFolder)
                    invokeResult = WbbExportSettings.InvokeBuildWithResult();
                else
                {
                    if (autoPlanError.Length > 0)
                        throw new System.InvalidOperationException(autoPlanError);
                    var scripts = autoPlan ?? WbbRuntimeScriptPlan.Collect(copyPath);
                    expectScripts = scripts.sources.Length > 0 || scripts.asmdefAssemblies.Length > 0;
                    WbbUtil.Log("Automatic runtime sources: " + scripts.sources.Length + "; WARUDO-provided types: " + scripts.providedTypes.Count);
                    AppendMessage(L("Runner.AutoScripts", scripts.sources.Length, scripts.providedTypes.Count));
                    // Compiled by Mod Mate, or nothing to add and no adapter in this SDK: the build runs as it is.
                    if (itemCompiled.Count > 0 || !WbbRuntimeScriptPlan.AdapterAvailable)
                    {
                        invokeResult = WbbExportSettings.InvokeBuildWithResult();
                    }
                    else
                    {
                        using (scripts.BeginBuild(modFolder))
                            invokeResult = WbbExportSettings.InvokeBuildWithResult();
                    }
                }
            }
            catch (System.Exception e)
            {
                invokeResult = new WbbBuildInvokeResult
                {
                    invoked = false,
                    errorMessage = e.Message
                };
            }
            finally
            {
                if (hook != null)
                {
                    hookRegistered = hook.RegisteredCount;
                    hook.Dispose();
                }
            }

            waitStartTime = EditorApplication.timeSinceStartup;
            itemBuildLogPath = WbbBuildLog.CopyUModLog(itemRecordId, buildStartUtc);
            if (!invokeResult.invoked)
            {
                Fail(L("Runner.ErrorBuild", invokeResult.errorMessage));
                AppendCapturedErrors();
                step = Step.Cleanup;
                return;
            }

            if (invokeResult.resultKnown && !invokeResult.successful)
            {
                Fail(L("Safety.BuildFailed", FirstNonEmpty(invokeResult.errorMessage, invokeResult.exceptionMessage, L("Safety.BuildFailedNoReason"))));
                if (!string.IsNullOrEmpty(invokeResult.exceptionMessage) && invokeResult.exceptionMessage.IndexOf(invokeResult.errorMessage ?? "", System.StringComparison.Ordinal) < 0)
                    AppendMessage(WbbBuildLog.Shorten(invokeResult.exceptionMessage));
                if (!AppendSecurityReport())
                    AppendCapturedErrors();
                step = Step.Cleanup;
                return;
            }

            if (itemCompiled.Count > 0 && hookRegistered != itemCompiled.Count)
            {
                Fail(L("Asmdef.ModuleMissing"));
                step = Step.Cleanup;
                return;
            }

            if (!string.IsNullOrEmpty(invokeResult.builtFile) && !string.Equals(WbbUtil.NormalizePath(invokeResult.builtFile), WbbUtil.NormalizePath(outputFilePath), System.StringComparison.OrdinalIgnoreCase))
            {
                WbbUtil.LogWarning("UMod reported a different output file than expected: " + invokeResult.builtFile + " (expected " + outputFilePath + ")");
            }

            step = Step.WaitOutput;
            Delay(PollIntervalSeconds);
        }

        private void DoWaitOutput()
        {
            double elapsed = EditorApplication.timeSinceStartup - waitStartTime;
            SetStatus(L("Runner.WaitingOutput", currentModName, WbbUtil.FormatDuration(elapsed)));
            bool completed = false;
            try
            {
                string absolute = WbbUtil.ToAbsolutePath(outputFilePath);
                if (File.Exists(absolute))
                {
                    var info = new FileInfo(absolute);
                    if (info.LastWriteTimeUtc >= buildStartUtc)
                    {
                        long length = info.Length;
                        if (length > 0L && length == lastSize)
                            stableSamples++;
                        else
                        {
                            stableSamples = 0;
                            lastSize = length;
                        }

                        if (stableSamples >= RequiredStableSamples)
                        {
                            outputSizeBytes = length;
                            completed = true;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Output polling failed: " + e.Message);
            }

            if (completed)
            {
                step = Step.Verify;
                return;
            }

            if (elapsed >= OutputGraceSeconds)
            {
                if (TryConfirmOutput())
                {
                    step = Step.Verify;
                    return;
                }

                Fail(L("Runner.ErrorNoOutput", outputFilePath));
                AppendCapturedErrors();
                step = Step.Cleanup;
                return;
            }

            Delay(PollIntervalSeconds);
        }

        private bool TryConfirmOutput()
        {
            try
            {
                string absolute = WbbUtil.ToAbsolutePath(outputFilePath);
                if (!File.Exists(absolute))
                    return false;
                var info = new FileInfo(absolute);
                if (info.Length <= 0L)
                    return false;
                if (info.LastWriteTimeUtc < buildStartUtc)
                    return false;
                outputSizeBytes = info.Length;
                WbbUtil.LogWarning("Output size never stabilized but the file was updated by this build: " + outputFilePath);
                return true;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Final output check failed: " + e.Message);
                return false;
            }
        }

        private void DoVerify()
        {
            SetStatus(L("Safety.Verifying", currentModName));
            StopCapture();
            var reasons = new List<string>();
            if (capture != null)
            {
                for (int i = 0; i < capture.SilentFailures.Count; i++)
                    AddUnique(reasons, capture.SilentFailures[i]);
            }

            RefreshBuildLog();
            List<string> logFailures = WbbBuildLog.ScanLogForFailures(itemBuildLogPath, WbbLogCapture.MaxLines);
            for (int i = 0; i < logFailures.Count && reasons.Count < WbbLogCapture.MaxLines; i++)
                AddUnique(reasons, logFailures[i]);
            if (reasons.Count > 0)
            {
                Fail(L("Safety.SilentFailure"));
                for (int i = 0; i < reasons.Count; i++)
                    AppendMessage(reasons[i]);
                step = Step.Cleanup;
                return;
            }

            if (expectScripts)
            {
                bool? hasScripts = WbbBuildLog.ArchiveHasScripts(WbbUtil.ToAbsolutePath(outputFilePath));
                if (hasScripts.HasValue && !hasScripts.Value)
                {
                    Fail(itemCompiled.Count > 0 ? L("Asmdef.ModuleMissing") : L("Safety.ScriptsMissing"));
                    step = Step.Cleanup;
                    return;
                }

                if (!hasScripts.HasValue)
                    WbbUtil.LogWarning("Could not confirm that custom scripts were packed into " + outputFilePath);
            }

            MarkCurrent(WbbBuildStatus.Success, "");
            WbbJournal.MarkOutputVerified(outputGuard);
            step = Step.Cleanup;
        }

        private void DoCleanup()
        {
            StopCapture();
            SettleOutput();
            if (!string.IsNullOrEmpty(copyPath) && !restoreFailed)
            {
                SetStatus(L("Safety.CleaningUp", currentModName));
                DeleteCopy();
            }

            step = Step.Record;
            Delay(StepDelaySeconds);
        }

        private void DoRecord()
        {
            var item = CurrentItem();
            if (item != null && (item.status == WbbBuildStatus.Running || item.status == WbbBuildStatus.Pending))
            {
                item.status = WbbBuildStatus.Failed;
                AppendMessage(L("Runner.ErrorNoOutput", outputFilePath));
            }

            FinishItem();
            index++;
            step = Step.Prepare;
            Delay(StepDelaySeconds);
        }

        private void SettleOutput()
        {
            WbbJournalOutput entry = outputGuard;
            outputGuard = null;
            if (entry == null)
                return;
            var item = CurrentItem();
            if (item != null && item.status == WbbBuildStatus.Success)
            {
                WbbJournal.CommitOutput(entry);
                return;
            }

            bool problem;
            string note = WbbJournal.RevertOutput(entry, out problem);
            if (note.Length > 0)
                AppendMessage(note);
        }

        private string UniqueCopyPath()
        {
            string baseName = "Copy_" + index.ToString("000");
            string candidate = tempRoot + "/" + baseName + ".prefab";
            int n = 2;
            while (File.Exists(WbbUtil.ToAbsolutePath(candidate)))
                candidate = tempRoot + "/" + baseName + "_" + n++ + ".prefab";
            return candidate;
        }

        private void DeleteCopy()
        {
            string path = copyPath;
            if (!string.IsNullOrEmpty(copyGuid))
            {
                string byGuid = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(copyGuid));
                if (!string.IsNullOrEmpty(byGuid))
                    path = byGuid;
            }

            if (string.IsNullOrEmpty(path) || !File.Exists(WbbUtil.ToAbsolutePath(path)))
            {
                WbbJournal.ForgetCopy(copyPath, copyGuid);
                copyPath = "";
                return;
            }

            bool ours = string.IsNullOrEmpty(copyGuid) ? WbbJournal.IsUnder(path, tempRoot) : string.Equals(AssetDatabase.AssetPathToGUID(path), copyGuid, System.StringComparison.Ordinal);
            if (!ours)
            {
                FlagCleanupFailure(path);
                return;
            }

            if (!AssetDatabase.DeleteAsset(path))
            {
                FlagCleanupFailure(path);
                return;
            }

            WbbJournal.ForgetCopy(copyPath, copyGuid);
            copyPath = "";
        }

        private const int SecurityMessageLimit = 12;
        // UMod writes the per-API entries of a code security rejection to the Unity console only, never to its
        // build log, so they come from the captured console errors. The full list is also kept in the build log.
        private bool AppendSecurityReport()
        {
            StopCapture();
            if (capture == null || invokeResult == null)
                return false;
            bool security = ContainsSecurity(invokeResult.errorMessage) || ContainsSecurity(invokeResult.exceptionMessage) || capture.SawSecurityFailure;
            if (!security)
                return false;
            List<string> entries = capture.SecurityEntries();
            if (entries.Count == 0)
                return false;
            string header = L("Safety.SecurityHeader", entries.Count);
            AppendMessage(header);
            int shown = System.Math.Min(entries.Count, SecurityMessageLimit);
            for (int i = 0; i < shown; i++)
                AppendMessage(entries[i]);
            if (entries.Count > shown)
                AppendMessage(L("Safety.SecurityMore", entries.Count - shown));
            securityHeader = header;
            securityEntries = entries;
            WbbBuildLog.AppendSection(itemBuildLogPath, header, entries);
            return true;
        }

        // Called again when the item ends: the UMod log can have grown after the first copy, and the security section
        // that was appended to that copy has to survive the refresh.
        private void RefreshBuildLog()
        {
            if (string.IsNullOrEmpty(itemBuildLogPath))
                return;
            if (WbbBuildLog.RefreshUModLog(itemRecordId, buildStartUtc, ref itemBuildLogPath) && securityEntries != null)
                WbbBuildLog.AppendSection(itemBuildLogPath, securityHeader, securityEntries);
        }

        private static bool ContainsSecurity(string text)
        {
            return !string.IsNullOrEmpty(text) && text.IndexOf("security", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void StopCapture()
        {
            if (capture != null)
                capture.Stop();
        }

        private void AppendCapturedErrors()
        {
            StopCapture();
            if (capture == null)
                return;
            for (int i = 0; i < capture.SilentFailures.Count; i++)
                AppendMessage(capture.SilentFailures[i]);
            for (int i = 0; i < capture.Errors.Count; i++)
            {
                string line = capture.Errors[i];
                if (line.IndexOf("BUILD FAILED", System.StringComparison.Ordinal) >= 0)
                    continue;
                AppendMessage(line);
            }
        }

        private static void AddUnique(List<string> list, string value)
        {
            if (string.IsNullOrEmpty(value) || list.Contains(value))
                return;
            list.Add(value);
        }

        private static string FirstNonEmpty(string a, string b, string c)
        {
            if (!string.IsNullOrEmpty(a))
                return WbbBuildLog.Shorten(a);
            if (!string.IsNullOrEmpty(b))
                return WbbBuildLog.Shorten(b);
            return c;
        }
    }
}
