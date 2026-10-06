using System.Collections.Generic;
using System.IO;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbModScanner
    {
        public static readonly string[] KnownExtensions =
        {
            ".warudo",
            ".vrm"
        };
        private const int MaxDirectoryVisits = 20000;
        public static List<WbbModFile> Scan(string rootDirectory, WbbExportType type, out string error)
        {
            error = "";
            var result = new List<WbbModFile>();
            if (string.IsNullOrEmpty(rootDirectory))
            {
                error = "Output folder is not set.";
                return result;
            }

            string root = WbbUtil.NormalizePath(rootDirectory);
            bool exists;
            try
            {
                exists = Directory.Exists(root);
            }
            catch (System.Exception e)
            {
                error = "Cannot access folder: " + root + " (" + e.Message + ")";
                return result;
            }

            if (!exists)
            {
                error = "Folder not found: " + root;
                return result;
            }

            var paths = new List<string>();
            string collectError;
            CollectFiles(root, paths, out collectError);
            if (!string.IsNullOrEmpty(collectError) && paths.Count == 0)
            {
                error = "Cannot list folder: " + root + " (" + collectError + ")";
                return result;
            }

            for (int i = 0; i < paths.Count; i++)
            {
                if (IsInTrash(paths[i]))
                    continue;
                var file = Describe(paths[i], root, type);
                if (file != null)
                    result.Add(file);
            }

            SortByLastWriteDesc(result);
            return result;
        }

        public static List<WbbModFile> ScanAll(string propsDir, string charactersDir, string environmentsDir)
        {
            var result = new List<WbbModFile>();
            AppendScan(result, propsDir, WbbExportType.Prop);
            AppendScan(result, charactersDir, WbbExportType.Character);
            AppendScan(result, environmentsDir, WbbExportType.Environment);
            SortByLastWriteDesc(result);
            return result;
        }

        private static void AppendScan(List<WbbModFile> target, string directory, WbbExportType type)
        {
            if (string.IsNullOrEmpty(directory))
                return;
            string error;
            var files = Scan(directory, type, out error);
            if (!string.IsNullOrEmpty(error))
                WbbUtil.LogWarning("Scan skipped (" + type + "): " + error);
            target.AddRange(files);
        }

        public static void AttachRecords(IList<WbbModFile> files)
        {
            if (files == null)
                return;
            try
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (file == null)
                        continue;
                    file.matchedRecord = string.IsNullOrEmpty(file.fileName) ? null : WbbDatabase.FindLatestByModName(file.fileName);
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Failed to attach build records: " + e.Message);
            }
        }

        public static bool TryDelete(string absoluteFilePath, out string error)
        {
            WbbRecycleOutcome outcome;
            return TryDelete(absoluteFilePath, "", out outcome, out error);
        }

        public static bool TryDelete(string absoluteFilePath, string trashRoot, out WbbRecycleOutcome outcome, out string error)
        {
            error = "";
            outcome = WbbRecycleOutcome.Failed;
            if (string.IsNullOrEmpty(absoluteFilePath))
            {
                error = WbbRecycleBin.Describe(WbbRecycleFailure.NotFound, "");
                return false;
            }

            WbbRecycleFailure failure;
            string detail;
            outcome = WbbRecycleBin.Recycle(absoluteFilePath, trashRoot, out failure, out detail);
            if (outcome == WbbRecycleOutcome.Failed)
            {
                error = WbbRecycleBin.Describe(failure, detail);
                WbbUtil.LogWarning("Could not remove " + absoluteFilePath + ": " + failure + (string.IsNullOrEmpty(detail) ? "" : " (" + detail + ")"));
                return false;
            }

            return true;
        }

        public static string FindExistingOutput(string outputDirectory, string modName)
        {
            if (string.IsNullOrEmpty(outputDirectory) || string.IsNullOrEmpty(modName))
                return null;
            string dir = WbbUtil.NormalizePath(outputDirectory);
            try
            {
                for (int i = 0; i < KnownExtensions.Length; i++)
                {
                    string candidate = dir + "/" + modName + KnownExtensions[i];
                    if (File.Exists(candidate))
                        return candidate;
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Failed to probe existing output in " + dir + ": " + e.Message);
            }

            return null;
        }

        private static bool IsInTrash(string path)
        {
            string normalized = WbbUtil.NormalizePath(path);
            return normalized.IndexOf("/" + WbbRecycleBin.TrashFolderName + "/", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void CollectFiles(string root, List<string> output, out string error)
        {
            error = "";
            try
            {
                string[] all = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
                output.AddRange(all);
                return;
            }
            catch (System.Exception e)
            {
                error = e.Message;
            }

            output.Clear();
            var stack = new Stack<string>();
            stack.Push(root);
            int visits = 0;
            while (stack.Count > 0 && visits < MaxDirectoryVisits)
            {
                visits++;
                string dir = stack.Pop();
                try
                {
                    string[] files = Directory.GetFiles(dir);
                    output.AddRange(files);
                }
                catch (System.Exception)
                {
                }

                try
                {
                    string[] subs = Directory.GetDirectories(dir);
                    for (int i = 0; i < subs.Length; i++)
                        stack.Push(subs[i]);
                }
                catch (System.Exception)
                {
                }
            }

            if (output.Count > 0)
            {
                WbbUtil.LogWarning("Partial scan of " + root + " (" + error + ")");
                error = "";
            }
        }

        private static WbbModFile Describe(string absolutePath, string root, WbbExportType type)
        {
            if (string.IsNullOrEmpty(absolutePath))
                return null;
            string extension;
            try
            {
                extension = Path.GetExtension(absolutePath);
            }
            catch (System.Exception)
            {
                return null;
            }

            if (string.IsNullOrEmpty(extension))
                return null;
            bool known = false;
            for (int i = 0; i < KnownExtensions.Length; i++)
            {
                if (string.Equals(extension, KnownExtensions[i], System.StringComparison.OrdinalIgnoreCase))
                {
                    extension = KnownExtensions[i];
                    known = true;
                    break;
                }
            }

            if (!known)
                return null;
            try
            {
                var info = new FileInfo(absolutePath);
                var file = new WbbModFile();
                file.filePath = WbbUtil.NormalizePath(absolutePath);
                file.fileName = Path.GetFileNameWithoutExtension(absolutePath);
                file.extension = extension;
                file.relativeFolder = RelativeFolderOf(file.filePath, root);
                file.sizeBytes = info.Length;
                file.lastWrite = info.LastWriteTime;
                file.exportType = (int)type;
                return file;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Skipped unreadable file entry: " + absolutePath + " (" + e.Message + ")");
                return null;
            }
        }

        private static string RelativeFolderOf(string normalizedFilePath, string root)
        {
            string dir;
            try
            {
                dir = Path.GetDirectoryName(normalizedFilePath);
            }
            catch (System.Exception)
            {
                return "";
            }

            dir = WbbUtil.NormalizePath(dir);
            string normalizedRoot = WbbUtil.NormalizePath(root);
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(normalizedRoot))
                return "";
            if (string.Equals(dir, normalizedRoot, System.StringComparison.OrdinalIgnoreCase))
                return "";
            string prefix = normalizedRoot + "/";
            if (dir.Length > prefix.Length && dir.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return dir.Substring(prefix.Length);
            }

            return "";
        }

        private static void SortByLastWriteDesc(List<WbbModFile> files)
        {
            if (files == null || files.Count < 2)
                return;
            files.Sort(CompareLastWriteDesc);
        }

        private static int CompareLastWriteDesc(WbbModFile a, WbbModFile b)
        {
            if (a == null)
                return b == null ? 0 : 1;
            if (b == null)
                return -1;
            int byTime = b.lastWrite.CompareTo(a.lastWrite);
            if (byTime != 0)
                return byTime;
            return string.Compare(a.fileName, b.fileName, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
