using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbDatabase
    {
        public const int MaxRecords = 2000;
        private const string StorageFolderName = "StudioRaming";
        private const string JobsFileName = "WarudoBatchBuilder.Jobs.json";
        private const string HistoryFileName = "WarudoBatchBuilder.History.json";
        private static readonly List<WbbJob> jobs = new List<WbbJob>();
        private static readonly List<WbbRecord> records = new List<WbbRecord>();
        private static bool loaded;
        private static bool loading;
        private static string cachedStorageDir;
        private static bool jobsLoadFailed;
        private static bool historyLoadFailed;
        private static readonly List<string> corruptCopies = new List<string>();
        private static bool saveBlockWarned;
        [Serializable]
        private class JobWrapper
        {
            public List<WbbJob> items = new List<WbbJob>();
        }

        [Serializable]
        private class RecordWrapper
        {
            public List<WbbRecord> items = new List<WbbRecord>();
        }

        public static string StorageDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(cachedStorageDir))
                    cachedStorageDir = ComputeStorageDirectory();
                EnsureDirectory(cachedStorageDir);
                return cachedStorageDir;
            }
        }

        public static string JobsFilePath
        {
            get
            {
                return StorageDirectory + "/" + JobsFileName;
            }
        }

        public static string HistoryFilePath
        {
            get
            {
                return StorageDirectory + "/" + HistoryFileName;
            }
        }

        private static string ComputeStorageDirectory()
        {
            string dataPath = WbbUtil.NormalizePath(Application.dataPath);
            string projectRoot = dataPath;
            int idx = dataPath.LastIndexOf('/');
            if (idx > 0)
                projectRoot = dataPath.Substring(0, idx);
            return projectRoot + "/ProjectSettings/" + StorageFolderName;
        }

        private static void EnsureDirectory(string directory)
        {
            try
            {
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Failed to create storage directory: " + directory + " (" + e.Message + ")");
            }
        }

        public static IList<WbbJob> Jobs
        {
            get
            {
                EnsureLoaded();
                return jobs;
            }
        }

        public static IList<WbbRecord> Records
        {
            get
            {
                EnsureLoaded();
                return records;
            }
        }

        public static void EnsureLoaded()
        {
            if (loaded || loading)
                return;
            LoadAll();
        }

        public static void Reload()
        {
            loaded = false;
            LoadAll();
        }

        public static bool LoadFailed
        {
            get
            {
                EnsureLoaded();
                return jobsLoadFailed || historyLoadFailed;
            }
        }

        public static bool JobsLoadFailed
        {
            get
            {
                EnsureLoaded();
                return jobsLoadFailed;
            }
        }

        public static bool HistoryLoadFailed
        {
            get
            {
                EnsureLoaded();
                return historyLoadFailed;
            }
        }

        public static IList<string> CorruptCopyPaths
        {
            get
            {
                EnsureLoaded();
                return corruptCopies.AsReadOnly();
            }
        }

        public static string BackupPathOf(string path)
        {
            return path + ".bak";
        }

        public static bool CanRestoreBackup
        {
            get
            {
                EnsureLoaded();
                return (jobsLoadFailed && File.Exists(BackupPathOf(JobsFilePath))) || (historyLoadFailed && File.Exists(BackupPathOf(HistoryFilePath)));
            }
        }

        public static void AcknowledgeLoadFailure()
        {
            EnsureLoaded();
            bool jobsWereBlocked = jobsLoadFailed;
            bool historyWasBlocked = historyLoadFailed;
            jobsLoadFailed = false;
            historyLoadFailed = false;
            saveBlockWarned = false;
            if (jobsWereBlocked)
                SaveJobs();
            if (historyWasBlocked)
                SaveRecords();
        }

        public static bool TryRestoreBackup(out string error)
        {
            EnsureLoaded();
            error = "";
            var pendingRecords = new List<WbbRecord>(records);
            var pendingJobs = new List<WbbJob>(jobs);
            try
            {
                if (jobsLoadFailed && File.Exists(BackupPathOf(JobsFilePath)))
                    File.Copy(BackupPathOf(JobsFilePath), JobsFilePath, true);
                if (historyLoadFailed && File.Exists(BackupPathOf(HistoryFilePath)))
                    File.Copy(BackupPathOf(HistoryFilePath), HistoryFilePath, true);
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            Reload();
            if (LoadFailed)
            {
                error = "The backup could not be read either.";
                return false;
            }

            bool changedRecords = false;
            for (int i = 0; i < pendingRecords.Count; i++)
            {
                if (FindRecordIndex(pendingRecords[i].id) >= 0)
                    continue;
                InsertSorted(pendingRecords[i]);
                changedRecords = true;
            }

            bool changedJobs = false;
            for (int i = 0; i < pendingJobs.Count; i++)
            {
                if (GetJob(pendingJobs[i].id) != null)
                    continue;
                jobs.Add(pendingJobs[i]);
                changedJobs = true;
            }

            if (changedRecords)
            {
                TrimRecords();
                SaveRecords();
            }

            if (changedJobs)
                SaveJobs();
            return true;
        }

        private static int FindRecordIndex(string id)
        {
            if (string.IsNullOrEmpty(id))
                return -1;
            for (int i = 0; i < records.Count; i++)
            {
                if (string.Equals(records[i].id, id, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        private static void LoadAll()
        {
            loading = true;
            try
            {
                corruptCopies.Clear();
                jobsLoadFailed = !LoadJobs();
                historyLoadFailed = !LoadRecords();
                saveBlockWarned = false;
            }
            finally
            {
                loading = false;
                loaded = true;
            }
        }

        private static bool LoadJobs()
        {
            jobs.Clear();
            string path = JobsFilePath;
            try
            {
                if (!File.Exists(path))
                    return true;
                string json = File.ReadAllText(path, Encoding.UTF8);
                var wrapper = string.IsNullOrEmpty(json) || json.Trim().Length == 0 ? null : JsonUtility.FromJson<JobWrapper>(json);
                if (wrapper == null)
                    throw new FormatException("The file is empty or not a job list.");
                if (wrapper.items == null)
                    return true;
                for (int i = 0; i < wrapper.items.Count; i++)
                {
                    var job = wrapper.items[i];
                    if (job == null)
                        continue;
                    SanitizeJob(job);
                    jobs.Add(job);
                }

                return true;
            }
            catch (Exception e)
            {
                jobs.Clear();
                PreserveCorrupt(path);
                WbbUtil.LogError("Could not read the presets file; saving over it is blocked until you choose what to do: " + path + " (" + e.Message + ")");
                return false;
            }
        }

        private static bool LoadRecords()
        {
            records.Clear();
            string path = HistoryFilePath;
            bool ok = true;
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path, Encoding.UTF8);
                    var wrapper = string.IsNullOrEmpty(json) || json.Trim().Length == 0 ? null : JsonUtility.FromJson<RecordWrapper>(json);
                    if (wrapper == null)
                        throw new FormatException("The file is empty or not a build history.");
                    if (wrapper.items != null)
                    {
                        for (int i = 0; i < wrapper.items.Count; i++)
                        {
                            var record = wrapper.items[i];
                            if (record == null)
                                continue;
                            SanitizeRecord(record);
                            records.Add(record);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                records.Clear();
                PreserveCorrupt(path);
                WbbUtil.LogError("Could not read the build history; saving over it is blocked until you choose what to do: " + path + " (" + e.Message + ")");
                ok = false;
            }

            SortRecords();
            TrimRecords();
            return ok;
        }

        private static void PreserveCorrupt(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return;
                string directory = Path.GetDirectoryName(path);
                string name = Path.GetFileNameWithoutExtension(path);
                string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                string target = WbbUtil.NormalizePath(Path.Combine(directory ?? "", name + ".corrupt-" + stamp + ".json"));
                if (!File.Exists(target))
                    File.Copy(path, target, false);
                corruptCopies.Add(target);
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not keep a copy of the unreadable file " + path + ": " + e.Message);
            }
        }

        private static bool SaveBlocked(bool failedFlag, string what)
        {
            if (!failedFlag)
                return false;
            if (!saveBlockWarned)
            {
                saveBlockWarned = true;
                WbbUtil.LogWarning("Not saving " + what + ": the file on disk could not be read and is kept as-is " + "until you choose to restore the backup or start fresh (changes stay in memory).");
            }

            return true;
        }

        private static void SanitizeJob(WbbJob job)
        {
            if (job.id == null)
                job.id = "";
            if (string.IsNullOrEmpty(job.id))
                job.id = WbbUtil.NewId();
            if (job.name == null)
                job.name = "";
            if (job.buildFolderGuid == null)
                job.buildFolderGuid = "";
            if (job.buildFolderPath == null)
                job.buildFolderPath = "";
            if (job.prefabFolderGuid == null)
                job.prefabFolderGuid = "";
            if (job.prefabFolderPath == null)
                job.prefabFolderPath = "";
            if (job.modVersion == null)
                job.modVersion = "";
            if (job.modAuthor == null)
                job.modAuthor = "";
            if (job.modDescription == null)
                job.modDescription = "";
            if (job.outputDirectory == null)
                job.outputDirectory = "";
            if (job.namePrefix == null)
                job.namePrefix = "";
            if (job.lastUsedIso == null)
                job.lastUsedIso = "";
            if (job.excludedPrefabGuids == null)
                job.excludedPrefabGuids = new List<string>();
        }

        private static void SanitizeRecord(WbbRecord record)
        {
            if (record.id == null)
                record.id = "";
            if (string.IsNullOrEmpty(record.id))
                record.id = WbbUtil.NewId();
            if (record.startedIso == null)
                record.startedIso = "";
            if (record.finishedIso == null)
                record.finishedIso = "";
            if (record.modName == null)
                record.modName = "";
            if (record.modVersion == null)
                record.modVersion = "";
            if (record.modAuthor == null)
                record.modAuthor = "";
            if (record.prefabGuid == null)
                record.prefabGuid = "";
            if (record.prefabPath == null)
                record.prefabPath = "";
            if (record.prefabName == null)
                record.prefabName = "";
            if (record.outputDirectory == null)
                record.outputDirectory = "";
            if (record.outputFilePath == null)
                record.outputFilePath = "";
            if (record.message == null)
                record.message = "";
            if (record.jobId == null)
                record.jobId = "";
            if (record.jobName == null)
                record.jobName = "";
            if (record.sourceSignature == null)
                record.sourceSignature = "";
            if (record.sourceObjectId == null)
                record.sourceObjectId = "";
            if (record.sourceScenePath == null)
                record.sourceScenePath = "";
            if (record.sourceHierarchyPath == null)
                record.sourceHierarchyPath = "";
            if (record.fixupReport == null)
                record.fixupReport = "";
            if (record.buildLogPath == null)
                record.buildLogPath = "";
        }

        private static void SaveJobs()
        {
            if (SaveBlocked(jobsLoadFailed, "presets"))
                return;
            try
            {
                var wrapper = new JobWrapper();
                wrapper.items = jobs;
                WriteAtomic(JobsFilePath, JsonUtility.ToJson(wrapper, true));
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Failed to serialize jobs: " + e.Message);
            }
        }

        private static void SaveRecords()
        {
            if (SaveBlocked(historyLoadFailed, "build history"))
                return;
            try
            {
                var wrapper = new RecordWrapper();
                wrapper.items = records;
                WriteAtomic(HistoryFilePath, JsonUtility.ToJson(wrapper, true));
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Failed to serialize history: " + e.Message);
            }
        }

        internal static void WriteAtomic(string path, string content)
        {
            string tmp = path + ".tmp";
            string backup = BackupPathOf(path);
            try
            {
                string dir = Path.GetDirectoryName(path);
                EnsureDirectory(WbbUtil.NormalizePath(dir));
                File.WriteAllText(tmp, content, new UTF8Encoding(false));
                if (File.Exists(path))
                {
                    try
                    {
                        File.Replace(tmp, path, backup);
                    }
                    catch
                    {
                        try
                        {
                            File.Copy(path, backup, true);
                        }
                        catch (Exception e)
                        {
                            WbbUtil.LogWarning("Could not keep a backup of " + path + ": " + e.Message);
                        }

                        File.Copy(tmp, path, true);
                    }
                }
                else
                {
                    try
                    {
                        File.Move(tmp, path);
                    }
                    catch
                    {
                        File.Copy(tmp, path, true);
                    }
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Failed to save file (memory state kept): " + path + " (" + e.Message + ")");
            }
            finally
            {
                try
                {
                    if (File.Exists(tmp))
                        File.Delete(tmp);
                }
                catch
                {
                }
            }
        }

        public static WbbJob GetJob(string id)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(id))
                return null;
            for (int i = 0; i < jobs.Count; i++)
            {
                if (string.Equals(jobs[i].id, id, StringComparison.Ordinal))
                    return jobs[i];
            }

            return null;
        }

        public static void UpsertJob(WbbJob job)
        {
            EnsureLoaded();
            if (job == null)
                return;
            SanitizeJob(job);
            int found = -1;
            for (int i = 0; i < jobs.Count; i++)
            {
                if (string.Equals(jobs[i].id, job.id, StringComparison.Ordinal))
                {
                    found = i;
                    break;
                }
            }

            if (found >= 0)
                jobs[found] = job;
            else
                jobs.Add(job);
            SaveJobs();
        }

        public static void DeleteJob(string id)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(id))
                return;
            bool removed = false;
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                if (string.Equals(jobs[i].id, id, StringComparison.Ordinal))
                {
                    jobs.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
                SaveJobs();
        }

        public static WbbJob FindJobByName(string name)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(name))
                return null;
            string target = name.Trim();
            for (int i = 0; i < jobs.Count; i++)
            {
                string current = jobs[i].name == null ? "" : jobs[i].name.Trim();
                if (string.Equals(current, target, StringComparison.OrdinalIgnoreCase))
                    return jobs[i];
            }

            return null;
        }

        private static readonly string[] CsvHeader =
        {
            "id",
            "startedIso",
            "finishedIso",
            "durationSec",
            "modName",
            "exportType",
            "modVersion",
            "modAuthor",
            "prefabGuid",
            "prefabPath",
            "prefabName",
            "outputDirectory",
            "outputFilePath",
            "outputSizeBytes",
            "status",
            "message",
            "jobId",
            "jobName",
            "sourceSignature",
            "sourceObjectId",
            "sourceScenePath",
            "sourceHierarchyPath",
            "fixupReport",
            "buildLogPath",
        };
    }
}
