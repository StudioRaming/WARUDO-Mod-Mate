using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbPackageExporter
    {
        private const int FileBufferSize = 64 * 1024;
        private static uint GzipTimeStamp()
        {
            try
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                double seconds = (DateTime.UtcNow - epoch).TotalSeconds;
                if (seconds < 0d)
                    return 0u;
                if (seconds > uint.MaxValue)
                    return 0u;
                return (uint)seconds;
            }
            catch
            {
                return 0u;
            }
        }

        private class Entry
        {
            public string guid;
            public string assetPath;
            public string absolutePath;
            public string metaAbsolutePath;
            public string pathname;
        }

        public static bool WritePackage(IList<WbbPackageAsset> assets, string packageRootName, string outputFilePath, WbbGatherOptions gather, out string error)
        {
            int skippedCount;
            return WritePackage(assets, packageRootName, outputFilePath, gather, out error, out skippedCount);
        }

        public static bool WritePackage(IList<WbbPackageAsset> assets, string packageRootName, string outputFilePath, WbbGatherOptions gather, out string error, out int skippedCount)
        {
            error = string.Empty;
            skippedCount = 0;
            if (assets == null || assets.Count == 0)
            {
                error = "No assets to package.";
                return false;
            }

            if (string.IsNullOrEmpty(outputFilePath))
            {
                error = "Output file path is empty.";
                return false;
            }

            string rootName = WbbUtil.SanitizeModName(packageRootName);
            if (string.IsNullOrEmpty(rootName))
                rootName = "Package";
            string outputPath = WbbUtil.NormalizePath(outputFilePath);
            try
            {
                string directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch (Exception e)
            {
                error = "Cannot create output folder: " + e.Message;
                return false;
            }

            int skipped;
            List<Entry> entries = BuildEntries(assets, rootName, gather, out skipped);
            skippedCount = skipped;
            if (entries.Count == 0)
            {
                error = "No readable asset to package. (" + skipped + " skipped)";
                return false;
            }

            // Written beside the target and swapped in at the end, so a failed or interrupted export leaves the old package alone.
            string tempPath = outputPath + ".tmp-" + Guid.NewGuid().ToString("N");
            int written = 0;
            long totalBytes = 0L;
            bool ok;
            try
            {
                using (var file = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileBufferSize))
                using (var gzip = new WbbGzipStream(file, "archtemp.tar", GzipTimeStamp(), true))
                using (var tar = new WbbTarWriter(gzip))
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        Entry entry = entries[i];
                        try
                        {
                            byte[] meta = File.ReadAllBytes(entry.metaAbsolutePath);
                            long length = new FileInfo(entry.absolutePath).Length;
                            tar.AddDirectory(entry.guid);
                            using (var source = new FileStream(entry.absolutePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileBufferSize))
                            {
                                if (!tar.AddFile(entry.guid + "/asset", source, length))
                                {
                                    WbbUtil.LogWarning("Package: truncated read on '" + entry.assetPath + "' (file changed while exporting?)");
                                }
                            }

                            tar.AddFile(entry.guid + "/asset.meta", meta);
                            tar.AddFile(entry.guid + "/pathname", entry.pathname);
                            written++;
                            totalBytes += length;
                        }
                        catch (Exception e)
                        {
                            skipped++;
                            WbbUtil.LogWarning("Package: skipped '" + entry.assetPath + "' (" + e.Message + ")");
                        }
                    }
                }

                ok = written > 0;
                if (!ok)
                    error = "No asset could be written. (" + skipped + " skipped)";
            }
            catch (Exception e)
            {
                error = e.Message;
                ok = false;
            }

            if (ok)
            {
                string commitError;
                if (!CommitFile(tempPath, outputPath, out commitError))
                {
                    error = commitError;
                    ok = false;
                }
            }

            if (!ok)
            {
                skippedCount = skipped;
                TryDelete(tempPath);
                return false;
            }

            skippedCount = skipped;

            WbbUtil.Log("Package written: " + outputPath + " (" + written + " assets, " + WbbUtil.FormatSize(totalBytes) + " raw" + (skipped > 0 ? ", " + skipped + " skipped" : "") + ")");
            return true;
        }

        // Swaps the finished temp file in for the target: File.Replace when the target exists, File.Move otherwise.
        internal static bool CommitFile(string tempPath, string outputPath, out string error)
        {
            error = string.Empty;
            try
            {
                if (File.Exists(outputPath))
                    File.Replace(tempPath, outputPath, null);
                else
                    File.Move(tempPath, outputPath);
                return true;
            }
            catch (Exception e)
            {
                error = "Cannot replace the package file: " + e.Message;
                return false;
            }
        }

        private static List<Entry> BuildEntries(IList<WbbPackageAsset> assets, string rootName, WbbGatherOptions gather, out int skipped)
        {
            skipped = 0;
            var entries = new List<Entry>(assets.Count);
            var seenGuids = new HashSet<string>(StringComparer.Ordinal);
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < assets.Count; i++)
            {
                WbbPackageAsset asset = assets[i];
                if (asset == null || string.IsNullOrEmpty(asset.assetPath))
                {
                    skipped++;
                    continue;
                }

                string assetPath = WbbUtil.NormalizePath(asset.assetPath);
                if (!IsPackable(assetPath))
                {
                    skipped++;
                    WbbUtil.LogWarning("Package: not packable, skipped '" + assetPath + "'");
                    continue;
                }

                string guid = string.IsNullOrEmpty(asset.guid) ? AssetDatabase.AssetPathToGUID(assetPath) : asset.guid;
                if (string.IsNullOrEmpty(guid))
                {
                    skipped++;
                    continue;
                }

                if (!seenGuids.Add(guid))
                    continue;
                string absolute = WbbUtil.ToAbsolutePath(assetPath);
                string meta = absolute + ".meta";
                bool readable;
                try
                {
                    readable = !string.IsNullOrEmpty(absolute) && File.Exists(absolute) && File.Exists(meta);
                }
                catch
                {
                    readable = false;
                }

                if (!readable)
                {
                    skipped++;
                    WbbUtil.LogWarning("Package: file or .meta missing, skipped '" + assetPath + "'");
                    continue;
                }

                entries.Add(new Entry { guid = guid, assetPath = assetPath, absolutePath = absolute, metaAbsolutePath = meta, pathname = "Assets/" + rootName + "/" + PlaceInPackage(assetPath, gather, usedNames), });
            }

            return entries;
        }

        private static string PlaceInPackage(string assetPath, WbbGatherOptions gather, HashSet<string> usedNames)
        {
            string gatherFolder;
            if (gather.TryGetFolder(assetPath, Classify(assetPath), out gatherFolder))
                return gatherFolder + "/" + MakeUnique(usedNames, gatherFolder, FileNameOf(assetPath));
            return assetPath.StartsWith("Assets/", StringComparison.Ordinal) ? assetPath.Substring("Assets/".Length) : FileNameOf(assetPath);
        }

        private static string MakeUnique(HashSet<string> usedNames, string folder, string fileName)
        {
            string key = folder + "/" + fileName;
            if (usedNames.Add(key))
                return fileName;
            string stem = fileName;
            string ext = string.Empty;
            int dot = fileName.LastIndexOf('.');
            if (dot > 0)
            {
                stem = fileName.Substring(0, dot);
                ext = fileName.Substring(dot);
            }

            for (int n = 2; n < 10000; n++)
            {
                string candidate = stem + "_" + n + ext;
                if (usedNames.Add(folder + "/" + candidate))
                    return candidate;
            }

            string fallback = stem + "_" + WbbUtil.NewId() + ext;
            usedNames.Add(folder + "/" + fallback);
            return fallback;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Package: cannot remove temporary file '" + path + "' (" + e.Message + ")");
            }
        }
    }
}
