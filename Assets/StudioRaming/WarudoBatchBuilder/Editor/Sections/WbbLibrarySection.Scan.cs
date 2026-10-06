using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbLibrarySection
    {
        private static string OutputDirKey(WbbExportType type)
        {
            return WbbUtil.PrefPrefix + "OutputDir." + type;
        }

        private void RescanNow()
        {
            lastScanTime = EditorApplication.timeSinceStartup;
            WbbDatabase.EnsureLoaded();
            rows.Clear();
            for (int t = 0; t < TypeCount; t++)
            {
                typeCounts[t] = 0;
                typeSizes[t] = 0L;
                pathErrors[t] = "";
                pathStates[t] = PathState.Ok;
            }

            var files = new List<WbbModFile>();
            for (int t = 0; t < TypeCount; t++)
            {
                var type = (WbbExportType)t;
                string dir = paths[t] == null ? "" : paths[t].Trim();
                if (string.IsNullOrEmpty(dir))
                {
                    dir = WbbUtil.DefaultOutputPath(type);
                    paths[t] = dir;
                }

                if (string.IsNullOrEmpty(dir))
                {
                    pathStates[t] = PathState.Empty;
                    continue;
                }

                bool exists;
                try
                {
                    exists = Directory.Exists(WbbUtil.NormalizePath(dir));
                }
                catch (System.Exception e)
                {
                    pathStates[t] = PathState.Error;
                    pathErrors[t] = e.Message;
                    continue;
                }

                if (!exists)
                {
                    pathStates[t] = PathState.Missing;
                    continue;
                }

                string error;
                var scanned = WbbModScanner.Scan(dir, type, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    pathStates[t] = PathState.Error;
                    pathErrors[t] = error;
                }

                files.AddRange(scanned);
            }

            WbbModScanner.AttachRecords(files);
            trackedCount = 0;
            untrackedCount = 0;
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file == null)
                    continue;
                var row = new Row();
                row.file = file;
                row.record = file.matchedRecord;
                row.tracked = row.record != null;
                row.keyName = Lower(file.fileName);
                row.keyFolder = Lower(file.relativeFolder);
                row.keyExt = Lower(file.extension).TrimStart('.');
                row.keyType = TypeKey((WbbExportType)file.exportType);
                row.keyAuthor = row.record == null ? "" : Lower(row.record.modAuthor);
                row.keyVersion = row.record == null ? "" : Lower(row.record.modVersion);
                rows.Add(row);
                int t = Mathf.Clamp(file.exportType, 0, TypeCount - 1);
                typeCounts[t]++;
                typeSizes[t] += file.sizeBytes;
                if (row.tracked)
                    trackedCount++;
                else
                    untrackedCount++;
            }

            if (selected.Count > 0)
            {
                var alive = new HashSet<string>();
                for (int i = 0; i < rows.Count; i++)
                    alive.Add(rows[i].file.filePath);
                var drop = new List<string>();
                foreach (var path in selected)
                {
                    if (!alive.Contains(path))
                        drop.Add(path);
                }

                for (int i = 0; i < drop.Count; i++)
                    selected.Remove(drop[i]);
            }

            displayDirty = true;
            filterDirty = true;
            hoverIndex = -1;
        }

        private static string Lower(string value)
        {
            return string.IsNullOrEmpty(value) ? "" : value.ToLowerInvariant();
        }

        private static string TypeKey(WbbExportType type)
        {
            switch (type)
            {
                case WbbExportType.Character:
                    return "character";
                case WbbExportType.Environment:
                    return "environment";
                default:
                    return "prop";
            }
        }
    }
}
