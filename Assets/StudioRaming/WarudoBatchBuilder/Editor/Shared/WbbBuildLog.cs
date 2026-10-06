using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public sealed class WbbLogCapture : IDisposable
    {
        public const int MaxLines = 3;
        public const int MaxLineLength = 240;
        private readonly List<string> errors = new List<string>();
        private readonly List<string> silentFailures = new List<string>();
        private bool attached;
        public IList<string> Errors
        {
            get
            {
                return errors;
            }
        }

        public IList<string> SilentFailures
        {
            get
            {
                return silentFailures;
            }
        }

        public void Start()
        {
            if (attached)
                return;
            Application.logMessageReceived += OnLog;
            attached = true;
        }

        public void Stop()
        {
            if (!attached)
                return;
            Application.logMessageReceived -= OnLog;
            attached = false;
        }

        public void Dispose()
        {
            Stop();
        }

        private void OnLog(string condition, string stackTrace, LogType type)
        {
            if (string.IsNullOrEmpty(condition))
                return;
            if (condition.StartsWith("[WarudoBatchBuilder]", StringComparison.Ordinal))
                return;
            if (WbbBuildLog.IsSilentFailureLine(condition))
                AddLimited(silentFailures, condition);
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                AddLimited(errors, condition);
        }

        private static void AddLimited(List<string> target, string text)
        {
            if (target.Count >= MaxLines)
                return;
            target.Add(WbbBuildLog.Shorten(text));
        }
    }

    public static class WbbBuildLog
    {
        public const int KeepLogs = 300;
        public const string AssemblyEntryName = "assemblymodules.dat";
        private static readonly string[] SilentFailureMarkers =
        {
            "Failed to run asset batch processor",
            "Failed to run asset processor",
            "UMod module compilation",
        };
        public static string LibraryDirectory
        {
            get
            {
                string dataPath = WbbUtil.NormalizePath(Application.dataPath);
                int slash = dataPath.LastIndexOf('/');
                string projectRoot = slash > 0 ? dataPath.Substring(0, slash) : dataPath;
                string dir = projectRoot + "/Library/StudioRaming/WarudoBatchBuilder";
                EnsureDirectory(dir);
                return dir;
            }
        }

        public static string LogsDirectory
        {
            get
            {
                string dir = LibraryDirectory + "/Logs";
                EnsureDirectory(dir);
                return dir;
            }
        }

        private static void EnsureDirectory(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not create folder " + dir + ": " + e.Message);
            }
        }

        public static string CopyUModLog(string recordId, DateTime notBeforeUtc)
        {
            if (string.IsNullOrEmpty(recordId))
                return "";
            try
            {
                string source = WbbExportSettings.UModLogLocation();
                if (string.IsNullOrEmpty(source) || !File.Exists(source))
                    return "";
                if (File.GetLastWriteTimeUtc(source) < notBeforeUtc)
                    return "";
                string destination = LogsDirectory + "/" + recordId + ".log";
                using (var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    var start = new byte[3];
                    int read = input.Read(start, 0, start.Length);
                    bool bom = read == 3 && start[0] == 0xEF && start[1] == 0xBB && start[2] == 0xBF;
                    if (bom)
                        output.Write(start, 0, read);
                    byte[] header = Encoding.UTF8.GetBytes("[WarudoBatchBuilder] Render pipeline: " + WbbUtil.PipelineLogText() + Environment.NewLine);
                    output.Write(header, 0, header.Length);
                    if (!bom && read > 0)
                        output.Write(start, 0, read);
                    input.CopyTo(output);
                }

                Prune();
                return WbbUtil.NormalizePath(destination);
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not keep a copy of the UMod build log: " + e.Message);
                return "";
            }
        }

        private static void Prune()
        {
            try
            {
                var files = new List<FileInfo>(new DirectoryInfo(LogsDirectory).GetFiles("*.log"));
                if (files.Count <= KeepLogs)
                    return;
                files.Sort((a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));
                for (int i = KeepLogs; i < files.Count; i++)
                {
                    try
                    {
                        files[i].Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Build log pruning failed: " + e.Message);
            }
        }

        public static bool IsSilentFailureLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return false;
            for (int i = 0; i < SilentFailureMarkers.Length; i++)
            {
                if (line.IndexOf(SilentFailureMarkers[i], StringComparison.Ordinal) >= 0)
                    return true;
            }

            return false;
        }

        public static List<string> ScanLogForFailures(string logPath, int max)
        {
            var found = new List<string>();
            if (string.IsNullOrEmpty(logPath))
                return found;
            try
            {
                if (!File.Exists(logPath))
                    return found;
                using (var stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null && found.Count < max)
                    {
                        string trimmed = line.TrimStart();
                        if (trimmed.StartsWith("ERROR:", StringComparison.Ordinal) || IsSilentFailureLine(trimmed))
                            found.Add(Shorten(trimmed));
                    }
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not read the build log " + logPath + ": " + e.Message);
            }

            return found;
        }

        public static string Shorten(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            string first = text;
            int newline = first.IndexOfAny(new[] { '\r', '\n' });
            if (newline >= 0)
                first = first.Substring(0, newline);
            first = first.Trim();
            return first.Length <= WbbLogCapture.MaxLineLength ? first : first.Substring(0, WbbLogCapture.MaxLineLength - 1) + "…";
        }

        public static List<string> ReadArchiveEntryNames(string modFilePath)
        {
            try
            {
                using (var stream = new FileStream(modFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    long length = stream.Length;
                    if (length < 22)
                        return null;
                    int tail = (int)Math.Min(length, 22 + 65535);
                    var buffer = new byte[tail];
                    stream.Seek(length - tail, SeekOrigin.Begin);
                    ReadFully(stream, buffer, 0, tail);
                    int eocd = -1;
                    for (int i = tail - 22; i >= 0; i--)
                    {
                        if (buffer[i] == 0x50 && buffer[i + 1] == 0x4B && buffer[i + 2] == 0x05 && buffer[i + 3] == 0x06)
                        {
                            eocd = i;
                            break;
                        }
                    }

                    if (eocd < 0)
                        return null;
                    int entryCount = BitConverter.ToUInt16(buffer, eocd + 10);
                    uint cdSize = BitConverter.ToUInt32(buffer, eocd + 12);
                    if (cdSize == 0xFFFFFFFF || entryCount == 0xFFFF)
                        return null;
                    long eocdAbsolute = length - tail + eocd;
                    long cdStart = eocdAbsolute - cdSize;
                    if (cdStart < 0 || cdSize > 64L * 1024 * 1024)
                        return null;
                    var directory = new byte[cdSize];
                    stream.Seek(cdStart, SeekOrigin.Begin);
                    ReadFully(stream, directory, 0, (int)cdSize);
                    var names = new List<string>(entryCount);
                    int offset = 0;
                    for (int n = 0; n < entryCount; n++)
                    {
                        if (offset + 46 > directory.Length)
                            return null;
                        if (directory[offset] != 0x50 || directory[offset + 1] != 0x4B || directory[offset + 2] != 0x01 || directory[offset + 3] != 0x02)
                            return null;
                        int nameLength = BitConverter.ToUInt16(directory, offset + 28);
                        int extraLength = BitConverter.ToUInt16(directory, offset + 30);
                        int commentLength = BitConverter.ToUInt16(directory, offset + 32);
                        if (offset + 46 + nameLength > directory.Length)
                            return null;
                        names.Add(Encoding.UTF8.GetString(directory, offset + 46, nameLength));
                        offset += 46 + nameLength + extraLength + commentLength;
                    }

                    return names;
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not inspect the mod archive " + modFilePath + ": " + e.Message);
                return null;
            }
        }

        public static bool? ArchiveHasScripts(string modFilePath)
        {
            List<string> names = ReadArchiveEntryNames(modFilePath);
            if (names == null)
                return null;
            for (int i = 0; i < names.Count; i++)
            {
                if (string.Equals(names[i], AssemblyEntryName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void ReadFully(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int read = stream.Read(buffer, offset, count);
                if (read <= 0)
                    throw new EndOfStreamException();
                offset += read;
                count -= read;
            }
        }
    }
}
