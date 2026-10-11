using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    [Serializable]
    public class WbbJournalCopy
    {
        public string guid = "";
        public string path = "";
    }

    [Serializable]
    public class WbbJournalOutput
    {
        public string outputPath = "";
        public string backupPath = "";
        public bool backupReady;
        public long originalLength;
        public long originalWriteTicks;
        public long buildStartTicks;
        public bool verified;
        public string failedPath = "";
    }

    [Serializable]
    public class WbbJournalData
    {
        public int version = 1;
        public string batchId = "";
        public string startedIso = "";
        public string tempRoot = "";
        public List<string> tempFolders = new List<string>();
        public List<WbbJournalCopy> copies = new List<WbbJournalCopy>();
        public List<WbbJournalOutput> outputs = new List<WbbJournalOutput>();
        public bool profileCaptured;
        public bool profileWritten;
        public int profileIndex = -1;
        public string modName = "";
        public string modAuthor = "";
        public string modVersion = "";
        public string modDescription = "";
        public string modAssetPath = "";
        public string modExportPath = "";
        public string modIconGuid = "";
        public long modIconLocalId;
        public bool referencePathsCaptured;
        public bool referencePathsCleared;
        public List<string> referencePaths = new List<string>();
        public bool umodCaptured;
        public bool umodWritten;
        public bool clearConsoleOnBuild;
        public bool showOutputDirectory;
        public int logLevel;
        public int recoveryAttempts;
        public string recoverySession = "";
    }

    public class WbbStrandedOriginal
    {
        public string currentPath = "";
        public string originalPath = "";
        public string modName = "";
        public string recordId = "";
    }

    [InitializeOnLoad]
    public static class WbbJournal
    {
        public const string TempFolderPrefix = "_WBB_Temp_do_not_edit_";
        private const string FileName = "BuildJournal.json";
        private const string StrandReportedKey = WbbUtil.PrefPrefix + "Journal.StrandReported";
        public const int MaxRecoveryAttempts = 3;
        private static WbbJournalData current;
        private static string lastRecoverySummary = "";
        private static string lastRecoveryIso = "";
        private static string sessionId;
        private static long pendingStamp = -1L;
        private static bool pendingCached;
        static WbbJournal()
        {
            EditorApplication.delayCall += OnEditorLoaded;
        }

        public static string JournalPath
        {
            get
            {
                return WbbBuildLog.LibraryDirectory + "/" + FileName;
            }
        }

        public static bool Exists
        {
            get
            {
                try
                {
                    return File.Exists(JournalPath);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static WbbJournalData Current
        {
            get
            {
                return current;
            }
        }

        public static string LastRecoverySummary
        {
            get
            {
                return lastRecoverySummary;
            }
        }

        // True while a journal is kept because the build settings could not be written back; a new batch must wait for it.
        // Derived from the file (recoveryAttempts > 0), so it survives domain reloads; cached by the file's write time.
        public static bool SettingsRestorePending
        {
            get
            {
                try
                {
                    if (!File.Exists(JournalPath))
                    {
                        pendingStamp = -1L;
                        return false;
                    }

                    var info = new FileInfo(JournalPath);
                    long stamp = info.LastWriteTimeUtc.Ticks ^ info.Length;
                    if (stamp != pendingStamp)
                    {
                        WbbJournalData data = Read();
                        pendingCached = data != null && data.recoveryAttempts > 0;
                        pendingStamp = stamp;
                    }

                    return pendingCached;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        // Identifies one editor run: stable across domain reloads, different after the editor is restarted.
        private static string EditorSessionId
        {
            get
            {
                if (sessionId == null)
                {
                    try
                    {
                        var process = System.Diagnostics.Process.GetCurrentProcess();
                        sessionId = process.Id + "-" + process.StartTime.ToUniversalTime().Ticks;
                    }
                    catch (Exception)
                    {
                        sessionId = "unknown";
                    }
                }

                return sessionId;
            }
        }

        public static string LastRecoveryIso
        {
            get
            {
                return lastRecoveryIso;
            }
        }

        public static bool IsTempFolderPath(string assetPath)
        {
            string normalized = WbbUtil.NormalizePath(assetPath);
            if (!normalized.StartsWith("Assets/" + TempFolderPrefix, StringComparison.Ordinal))
                return false;
            return normalized.IndexOf('/', "Assets/".Length) < 0;
        }

        public static bool IsUnder(string path, string root)
        {
            string p = WbbUtil.NormalizePath(path);
            string r = WbbUtil.NormalizePath(root);
            if (string.IsNullOrEmpty(p) || string.IsNullOrEmpty(r))
                return false;
            return string.Equals(p, r, StringComparison.OrdinalIgnoreCase) || p.StartsWith(r + "/", StringComparison.OrdinalIgnoreCase);
        }

        public static bool Begin(WbbJournalData data)
        {
            if (data == null)
                return false;
            current = data;
            if (string.IsNullOrEmpty(current.batchId))
                current.batchId = WbbUtil.NewId();
            if (string.IsNullOrEmpty(current.startedIso))
                current.startedIso = WbbUtil.NowIso();
            if (Write(current))
                return true;
            current = null;
            return false;
        }

        public static void Save()
        {
            if (current != null)
                Write(current);
        }

        public static void AddFolder(string assetPath)
        {
            if (current == null || string.IsNullOrEmpty(assetPath))
                return;
            if (!current.tempFolders.Contains(assetPath))
                current.tempFolders.Add(assetPath);
            Save();
        }

        public static void TrackCopy(string path, string guid)
        {
            if (current == null)
                return;
            WbbJournalCopy entry = FindCopy(path, guid);
            if (entry == null)
            {
                entry = new WbbJournalCopy();
                current.copies.Add(entry);
            }

            entry.path = path ?? "";
            if (!string.IsNullOrEmpty(guid))
                entry.guid = guid;
            Save();
        }

        public static void ForgetCopy(string path, string guid)
        {
            if (current == null)
                return;
            WbbJournalCopy entry = FindCopy(path, guid);
            if (entry == null)
                return;
            current.copies.Remove(entry);
            Save();
        }

        private static WbbJournalCopy FindCopy(string path, string guid)
        {
            if (current == null)
                return null;
            for (int i = 0; i < current.copies.Count; i++)
            {
                WbbJournalCopy entry = current.copies[i];
                if (!string.IsNullOrEmpty(guid) && string.Equals(entry.guid, guid, StringComparison.Ordinal))
                    return entry;
                if (!string.IsNullOrEmpty(path) && string.Equals(entry.path, path, StringComparison.Ordinal))
                    return entry;
            }

            return null;
        }

        public static bool HasPendingCopies
        {
            get
            {
                return current != null && current.copies.Count > 0;
            }
        }

        public static bool HasPendingOutputs
        {
            get
            {
                return current != null && current.outputs.Count > 0;
            }
        }

        private const string OutputBackupFolder = "OutputBackups";
        private const string FailedOutputFolder = "Failed";
        public static string FailedOutputDirectory
        {
            get
            {
                return WbbBuildLog.LibraryDirectory + "/" + FailedOutputFolder;
            }
        }

        public static WbbJournalOutput GuardOutput(string outputPath, DateTime buildStartUtc, out string error)
        {
            error = "";
            var entry = new WbbJournalOutput();
            entry.outputPath = WbbUtil.NormalizePath(outputPath);
            entry.buildStartTicks = buildStartUtc.Ticks;
            try
            {
                if (File.Exists(entry.outputPath))
                {
                    var info = new FileInfo(entry.outputPath);
                    entry.originalLength = info.Length;
                    entry.originalWriteTicks = info.LastWriteTimeUtc.Ticks;
                    string dir = WbbBuildLog.LibraryDirectory + "/" + OutputBackupFolder;
                    Directory.CreateDirectory(dir);
                    entry.backupPath = UniqueFile(dir, Path.GetFileNameWithoutExtension(entry.outputPath), Path.GetExtension(entry.outputPath));
                }

                if (current != null)
                {
                    current.outputs.Add(entry);
                    Save();
                }

                if (entry.backupPath.Length > 0)
                {
                    File.Copy(entry.outputPath, entry.backupPath, false);
                    if (new FileInfo(entry.backupPath).Length != entry.originalLength)
                        throw new IOException("The backup copy has a different size.");
                    entry.backupReady = true;
                    Save();
                }

                return entry;
            }
            catch (Exception e)
            {
                error = e.Message;
                DeleteOwnBackup(entry.backupPath);
                ForgetOutput(entry);
                WbbUtil.LogError("Could not back up the existing output before building: " + entry.outputPath + " (" + e.Message + ")");
                return null;
            }
        }

        public static void MarkOutputVerified(WbbJournalOutput entry)
        {
            if (entry == null)
                return;
            entry.verified = true;
            Save();
        }

        public static void CommitOutput(WbbJournalOutput entry)
        {
            if (entry == null)
                return;
            DeleteOwnBackup(entry.backupPath);
            ForgetOutput(entry);
        }

        public static string RevertOutput(WbbJournalOutput entry, out bool problem)
        {
            problem = false;
            if (entry == null || string.IsNullOrEmpty(entry.outputPath))
                return "";
            string target = entry.outputPath;
            bool hadPrevious = !string.IsNullOrEmpty(entry.backupPath);
            if (hadPrevious && !entry.backupReady)
            {
                DeleteOwnBackup(entry.backupPath);
                ForgetOutput(entry);
                return "";
            }

            bool backupPresent = hadPrevious && File.Exists(entry.backupPath);
            string failedPath = "";
            bool restored = false;
            try
            {
                if (WrittenByBuild(entry))
                {
                    if (!string.IsNullOrEmpty(entry.failedPath) && File.Exists(entry.failedPath))
                    {
                        failedPath = entry.failedPath;
                    }
                    else
                    {
                        string dir = FailedOutputDirectory;
                        Directory.CreateDirectory(dir);
                        failedPath = UniqueFile(dir, Path.GetFileNameWithoutExtension(target), Path.GetExtension(target));
                        File.Copy(target, failedPath, false);
                        if (new FileInfo(failedPath).Length != new FileInfo(target).Length)
                            throw new IOException("The copy in the Failed folder has a different size.");
                        entry.failedPath = failedPath;
                        Save();
                        PruneFailedOutputs();
                    }

                    if (backupPresent)
                    {
                        File.Copy(entry.backupPath, target, true);
                        restored = true;
                    }
                    else
                    {
                        File.Delete(target);
                    }
                }
                else if (backupPresent && !File.Exists(target))
                {
                    File.Copy(entry.backupPath, target, false);
                    restored = true;
                }
            }
            catch (Exception e)
            {
                problem = true;
                WbbUtil.LogError("Could not put the output folder back after a failed build: " + target + " (" + e.Message + ")");
                Save();
                return backupPresent ? WbbLang.T("Runner.OutputRestoreFailed", entry.backupPath) : WbbLang.T("Runner.FailedOutputStuck", target);
            }

            DeleteOwnBackup(entry.backupPath);
            ForgetOutput(entry);
            if (hadPrevious && !backupPresent && failedPath.Length > 0)
            {
                problem = true;
                WbbUtil.LogError("The backup of the previous output is missing: " + entry.backupPath);
                return WbbLang.T("Runner.OutputRestoreFailed", entry.backupPath);
            }

            if (failedPath.Length > 0)
            {
                WbbUtil.LogWarning("Moved the output of a failed build out of the output folder: " + target + " -> " + failedPath + (restored ? " (the previous file was put back)" : ""));
                return restored ? WbbLang.T("Runner.FailedOutputRestored", failedPath) : WbbLang.T("Runner.FailedOutputMoved", failedPath);
            }

            if (restored)
            {
                WbbUtil.LogWarning("Put the previous output back after a failed build: " + target);
                return WbbLang.T("Runner.PreviousOutputRestored");
            }

            return "";
        }

        private static bool WrittenByBuild(WbbJournalOutput entry)
        {
            if (!File.Exists(entry.outputPath))
                return false;
            var info = new FileInfo(entry.outputPath);
            if (info.LastWriteTimeUtc.Ticks >= entry.buildStartTicks)
                return true;
            if (!string.IsNullOrEmpty(entry.backupPath))
                return info.Length != entry.originalLength || info.LastWriteTimeUtc.Ticks != entry.originalWriteTicks;
            return false;
        }

        private static void ForgetOutput(WbbJournalOutput entry)
        {
            if (current == null || entry == null)
                return;
            if (current.outputs.Remove(entry))
                Save();
        }

        private static bool DeleteOwnBackup(string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;
            try
            {
                if (!File.Exists(path))
                    return true;
                if (!IsUnder(path, WbbBuildLog.LibraryDirectory + "/" + OutputBackupFolder))
                    return false;
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not remove the output backup " + path + ": " + e.Message);
                return false;
            }
        }

        // Failed builds keep their output here so nothing is lost, but the folder is only Mod Mate's own and would
        // grow forever. The newest few stay; older entries go, and only entries directly inside this folder.
        public const int KeepFailedOutputs = 3;
        private static void PruneFailedOutputs()
        {
            try
            {
                string root = FailedOutputDirectory;
                if (!Directory.Exists(root))
                    return;
                var protectedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (current != null)
                {
                    for (int i = 0; i < current.outputs.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(current.outputs[i].failedPath))
                            protectedPaths.Add(WbbUtil.NormalizePath(current.outputs[i].failedPath));
                    }
                }

                var entries = new List<FileSystemInfo>(new DirectoryInfo(root).GetFileSystemInfos());
                entries.Sort((a, b) => b.CreationTimeUtc.CompareTo(a.CreationTimeUtc));
                int kept = 0;
                for (int i = 0; i < entries.Count; i++)
                {
                    FileSystemInfo info = entries[i];
                    if (kept < KeepFailedOutputs || protectedPaths.Contains(WbbUtil.NormalizePath(info.FullName)))
                    {
                        kept++;
                        continue;
                    }

                    // A link could lead out of the folder; it is left alone.
                    if ((info.Attributes & FileAttributes.ReparsePoint) != 0 || !IsUnder(info.FullName, root))
                        continue;
                    try
                    {
                        if (info is DirectoryInfo)
                            ((DirectoryInfo)info).Delete(true);
                        else
                            info.Delete();
                    }
                    catch (Exception e)
                    {
                        WbbUtil.LogWarning("Could not remove an old failed build output " + info.FullName + ": " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not tidy the Failed outputs folder: " + e.Message);
            }
        }

        private static string UniqueFile(string dir, string stem, string ext)
        {
            string baseName = stem + "_" + DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            string candidate = dir + "/" + baseName + ext;
            int n = 2;
            while (File.Exists(candidate))
                candidate = dir + "/" + baseName + "_" + n++ + ext;
            return candidate;
        }

        public static void Release()
        {
            if (current != null)
                Write(current);
            current = null;
        }

        public static void Complete()
        {
            current = null;
            try
            {
                if (File.Exists(JournalPath))
                    File.Delete(JournalPath);
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not remove the build journal: " + e.Message);
            }
        }

        private static bool Write(WbbJournalData data)
        {
            string path = JournalPath;
            string tmp = path + ".tmp";
            try
            {
                File.WriteAllText(tmp, JsonUtility.ToJson(data, true), new UTF8Encoding(false));
                if (File.Exists(path))
                    File.Replace(tmp, path, null);
                else
                    File.Move(tmp, path);
                return true;
            }
            catch (Exception e)
            {
                WbbUtil.LogError("Could not write the build journal: " + e.Message);
                try
                {
                    if (File.Exists(tmp))
                        File.Delete(tmp);
                }
                catch (Exception)
                {
                }

                return false;
            }
        }

        private static WbbJournalData Read()
        {
            try
            {
                if (!File.Exists(JournalPath))
                    return null;
                string json = File.ReadAllText(JournalPath, Encoding.UTF8);
                if (string.IsNullOrEmpty(json))
                    return null;
                var data = JsonUtility.FromJson<WbbJournalData>(json);
                if (data == null)
                    return null;
                if (data.tempFolders == null)
                    data.tempFolders = new List<string>();
                if (data.copies == null)
                    data.copies = new List<WbbJournalCopy>();
                if (data.outputs == null)
                    data.outputs = new List<WbbJournalOutput>();
                if (data.referencePaths == null)
                    data.referencePaths = new List<string>();
                return data;
            }
            catch (Exception e)
            {
                WbbUtil.LogError("The build journal is unreadable: " + e.Message);
                return null;
            }
        }

        public static void CaptureIcon(WbbJournalData data, UnityEngine.Object icon)
        {
            data.modIconGuid = "";
            data.modIconLocalId = 0;
            if (icon == null)
                return;
            string guid;
            long localId;
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(icon, out guid, out localId))
            {
                data.modIconGuid = guid ?? "";
                data.modIconLocalId = localId;
            }
        }

        private static UnityEngine.Object ResolveIcon(WbbJournalData data)
        {
            if (string.IsNullOrEmpty(data.modIconGuid))
                return null;
            string path = AssetDatabase.GUIDToAssetPath(data.modIconGuid);
            if (string.IsNullOrEmpty(path))
                return null;
            UnityEngine.Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < all.Length; i++)
            {
                string guid;
                long localId;
                if (all[i] != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(all[i], out guid, out localId) && localId == data.modIconLocalId)
                    return all[i];
            }

            return AssetDatabase.LoadMainAssetAtPath(path);
        }

        public static WbbProfileSnapshot ProfileOf(WbbJournalData data)
        {
            var snapshot = new WbbProfileSnapshot();
            snapshot.modName = data.modName ?? "";
            snapshot.modAuthor = data.modAuthor ?? "";
            snapshot.modVersion = data.modVersion ?? "";
            snapshot.modDescription = data.modDescription ?? "";
            snapshot.modAssetPath = data.modAssetPath ?? "";
            snapshot.modExportPath = data.modExportPath ?? "";
            snapshot.modIcon = ResolveIcon(data);
            return snapshot;
        }

        private static void OnEditorLoaded()
        {
            try
            {
                if (!WbbBuildRunner.AnyRunning && Exists)
                    RecoverIfNeeded();
            }
            catch (Exception e)
            {
                WbbUtil.LogError("Build journal recovery failed: " + e.Message);
            }

            try
            {
                ReportStrandedOriginalsOnce();
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not check history for stranded prefabs: " + e.Message);
            }
        }

        public static bool RecoverIfNeeded()
        {
            if (current != null || WbbBuildRunner.AnyRunning)
                return false;
            if (!Exists)
                return false;
            WbbJournalData data = Read();
            if (data == null)
            {
                PreserveUnreadable();
                return false;
            }

            int removedCopies = 0;
            var problems = new List<string>();
            for (int i = 0; i < data.copies.Count; i++)
            {
                WbbJournalCopy copy = data.copies[i];
                string path = ResolveCopyPath(copy, data);
                if (string.IsNullOrEmpty(path))
                    continue;
                if (AssetDatabase.DeleteAsset(path))
                    removedCopies++;
                else
                    problems.Add(path);
            }

            var folders = new List<string>(data.tempFolders);
            if (!string.IsNullOrEmpty(data.tempRoot) && !folders.Contains(data.tempRoot))
                folders.Insert(0, data.tempRoot);
            folders.Sort((a, b) => b.Length.CompareTo(a.Length));
            for (int i = 0; i < folders.Count; i++)
            {
                string folder = folders[i];
                if (!IsUnder(folder, data.tempRoot) || !IsTempFolderPath(data.tempRoot))
                    continue;
                if (!AssetDatabase.IsValidFolder(folder))
                    continue;
                if (!IsEffectivelyEmpty(folder))
                {
                    problems.Add(folder);
                    continue;
                }

                if (!AssetDatabase.DeleteAsset(folder))
                    problems.Add(folder);
            }

            int revertedOutputs = 0;
            var outputs = new List<WbbJournalOutput>(data.outputs);
            current = data;
            try
            {
                for (int i = 0; i < outputs.Count; i++)
                {
                    WbbJournalOutput output = outputs[i];
                    if (output == null)
                        continue;
                    if (output.verified)
                    {
                        CommitOutput(output);
                        continue;
                    }

                    bool outputProblem;
                    string note = RevertOutput(output, out outputProblem);
                    if (outputProblem)
                        problems.Add(note);
                    else if (note.Length > 0)
                        revertedOutputs++;
                }
            }
            finally
            {
                current = null;
            }

            bool settingsRestored = RestoreSettings(data, problems);
            string summary = problems.Count == 0 ? WbbLang.T("Safety.Recovered", removedCopies) : WbbLang.T("Safety.RecoveredWithProblems", removedCopies, problems.Count, problems[0]);
            if (revertedOutputs > 0)
                summary += " " + WbbLang.T("Safety.RecoveredOutputs", revertedOutputs, FailedOutputDirectory);
            lastRecoverySummary = summary;
            lastRecoveryIso = WbbUtil.NowIso();
            WbbUtil.LogWarning("Recovered from an interrupted build: removed " + removedCopies + " temporary copy(ies), " + "reverted " + revertedOutputs + " unconfirmed output file(s), " + "build settings restored=" + settingsRestored + (problems.Count > 0 ? ", left for manual check: " + string.Join(", ", problems.ToArray()) : "") + ".");
            if (settingsRestored)
            {
                Notify(summary);
                Complete();
                return true;
            }

            KeepForSettingsRestore(data, summary);
            return true;
        }

        // Settings could not be written back: the journal is the only record of the original values, so keep it
        // (settings fields only) and retry later, up to MaxRecoveryAttempts, then set it aside like an unreadable one.
        private static void KeepForSettingsRestore(WbbJournalData data, string summary)
        {
            data.copies.Clear();
            data.tempFolders.Clear();
            data.tempRoot = "";
            data.outputs.Clear();
            // The cap counts editor sessions, not calls: recovery also runs on every script reload and every Start click,
            // so counting those would burn the attempts on one locked file in a minute and then discard the only record
            // of the original settings. A retry inside the same session still restores and still blocks Start, but does not count.
            string session = EditorSessionId;
            if (data.recoverySession != session)
            {
                data.recoveryAttempts++;
                data.recoverySession = session;
            }

            if (data.recoveryAttempts < 1)
                data.recoveryAttempts = 1;
            bool saved = Write(data);
            if (data.recoveryAttempts >= MaxRecoveryAttempts)
            {
                string target = PreserveUnrecovered();
                if (target.Length > 0)
                {
                    Notify(WbbLang.T("Safety.JournalUnrecovered", Path.GetFileName(target)));
                    return;
                }
            }

            if (!saved)
                WbbUtil.LogError("The recovery attempt count could not be saved; the previous journal file is kept as it was.");
            Notify(summary);
        }

        private static string PreserveUnrecovered()
        {
            try
            {
                string target = JournalPath + ".unrecovered-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                File.Move(JournalPath, target);
                WbbUtil.LogError("The build settings could not be restored after " + MaxRecoveryAttempts + " attempts; the recovery record was set aside (nothing was deleted): " + target + ". Restore ExportSettings from a backup.");
                return target;
            }
            catch (Exception e)
            {
                WbbUtil.LogError("Could not set aside the unrecovered build journal: " + e.Message);
                return "";
            }
        }

        private static string ResolveCopyPath(WbbJournalCopy copy, WbbJournalData data)
        {
            if (copy == null)
                return "";
            if (!string.IsNullOrEmpty(copy.guid))
            {
                string byGuid = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(copy.guid));
                if (!string.IsNullOrEmpty(byGuid) && File.Exists(WbbUtil.ToAbsolutePath(byGuid)) && IsSafeCopy(byGuid, copy.guid))
                    return byGuid;
                string recorded = WbbUtil.NormalizePath(copy.path);
                if (!string.IsNullOrEmpty(recorded) && File.Exists(WbbUtil.ToAbsolutePath(recorded)) && IsSafeCopy(recorded, copy.guid))
                    return recorded;
                return "";
            }

            string path = WbbUtil.NormalizePath(copy.path);
            if (string.IsNullOrEmpty(path) || !File.Exists(WbbUtil.ToAbsolutePath(path)))
                return "";
            if (!IsTempFolderPath(data.tempRoot) || !IsUnder(path, data.tempRoot))
                return "";
            return path;
        }

        private static bool IsSafeCopy(string assetPath, string expectedGuid)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
                return false;
            if (!assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                return false;
            string actual = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(actual))
                actual = MetaGuid(assetPath);
            return string.Equals(actual, expectedGuid, StringComparison.Ordinal);
        }

        private static string MetaGuid(string assetPath)
        {
            try
            {
                string meta = WbbUtil.ToAbsolutePath(assetPath) + ".meta";
                if (!File.Exists(meta))
                    return "";
                string[] lines = File.ReadAllLines(meta);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.StartsWith("guid:", StringComparison.Ordinal))
                        return line.Substring(5).Trim();
                }
            }
            catch (Exception)
            {
            }

            return "";
        }

        public static bool IsEffectivelyEmpty(string assetFolder)
        {
            try
            {
                string absolute = WbbUtil.ToAbsolutePath(assetFolder);
                if (!Directory.Exists(absolute))
                    return true;
                string[] files = Directory.GetFiles(absolute, "*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (!files[i].EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool RestoreSettings(WbbJournalData data, List<string> problems)
        {
            bool ok = true;
            string error;
            if (data.profileCaptured && data.profileWritten)
            {
                if (!WbbExportSettings.TryWriteActiveProfile(ProfileOf(data), out error, data.profileIndex))
                {
                    ok = false;
                    problems.Add(WbbLang.T("Safety.RecoverSettingsFailed", error));
                }
            }

            if (data.referencePathsCaptured && data.referencePathsCleared)
            {
                if (!WbbExportSettings.TryWriteReferencePaths(data.profileIndex, data.referencePaths, out error))
                {
                    ok = false;
                    problems.Add(WbbLang.T("Safety.RecoverSettingsFailed", error));
                }
            }

            if (data.umodCaptured && data.umodWritten)
            {
                var state = new WbbUModQuietState
                {
                    captured = true,
                    clearConsoleOnBuild = data.clearConsoleOnBuild,
                    showOutputDirectory = data.showOutputDirectory,
                    logLevel = data.logLevel,
                };
                if (!WbbExportSettings.TryWriteQuietState(state, out error))
                {
                    ok = false;
                    problems.Add(WbbLang.T("Safety.RecoverSettingsFailed", error));
                }
            }

            return ok;
        }

        private static void PreserveUnreadable()
        {
            try
            {
                string target = JournalPath + ".unreadable-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                File.Move(JournalPath, target);
                WbbUtil.LogError("An unreadable build journal was set aside (nothing was deleted): " + target);
                Notify(WbbLang.T("Safety.JournalUnreadable"));
            }
            catch (Exception e)
            {
                WbbUtil.LogError("Could not set aside the unreadable build journal: " + e.Message);
            }
        }

        public static void Notify(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            try
            {
                var content = new GUIContent(text);
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.ShowNotification(content, 6d);
                    return;
                }

                if (EditorWindow.focusedWindow != null)
                    EditorWindow.focusedWindow.ShowNotification(content, 6d);
            }
            catch (Exception)
            {
            }
        }

        private static readonly string[] ManualRestorePrefixes =
        {
            "수동 복구 필요: ",
            "Manual restore required: ",
            "手動での復旧が必要です: ",
        };
        public static List<WbbStrandedOriginal> FindStrandedOriginals()
        {
            var result = new List<WbbStrandedOriginal>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IList<WbbRecord> records = WbbDatabase.Records;
            for (int i = 0; i < records.Count; i++)
            {
                WbbRecord record = records[i];
                if (record == null || string.IsNullOrEmpty(record.message))
                    continue;
                string[] parts = record.message.Split(new[] { " / " }, StringSplitOptions.None);
                for (int p = 0; p < parts.Length; p++)
                {
                    for (int k = 0; k < ManualRestorePrefixes.Length; k++)
                    {
                        string part = parts[p].Trim();
                        if (!part.StartsWith(ManualRestorePrefixes[k], StringComparison.Ordinal))
                            continue;
                        string path = WbbUtil.NormalizePath(part.Substring(ManualRestorePrefixes[k].Length).Trim());
                        if (!path.StartsWith("Assets/", StringComparison.Ordinal))
                            continue;
                        if (!File.Exists(WbbUtil.ToAbsolutePath(path)) || !seen.Add(path))
                            continue;
                        result.Add(new WbbStrandedOriginal { currentPath = path, originalPath = record.prefabPath ?? "", modName = record.modName ?? "", recordId = record.id ?? "", });
                    }
                }
            }

            return result;
        }

        private static void ReportStrandedOriginalsOnce()
        {
            if (SessionState.GetBool(StrandReportedKey, false))
                return;
            SessionState.SetBool(StrandReportedKey, true);
            List<WbbStrandedOriginal> stranded = FindStrandedOriginals();
            if (stranded.Count == 0)
                return;
            var sb = new StringBuilder();
            sb.Append("Found ").Append(stranded.Count).Append(" prefab(s) left in a script folder by an older version of ").Append(WbbProduct.Name).Append(" (nothing was moved):");
            for (int i = 0; i < stranded.Count; i++)
            {
                sb.Append("\n  ").Append(stranded[i].currentPath);
                if (!string.IsNullOrEmpty(stranded[i].originalPath))
                    sb.Append("  (was ").Append(stranded[i].originalPath).Append(')');
            }

            WbbUtil.LogWarning(sb.ToString());
        }
    }
}
