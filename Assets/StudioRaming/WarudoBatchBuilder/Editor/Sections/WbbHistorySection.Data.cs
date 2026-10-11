using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
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

        private static int StatusBucket(WbbBuildStatus status)
        {
            if (status == WbbBuildStatus.Success)
                return 0;
            if (status == WbbBuildStatus.Failed)
                return 1;
            return 2;
        }

        private int ResolveFileState(WbbRecord record)
        {
            string path = record.outputFilePath;
            if (string.IsNullOrEmpty(path))
                return 0;
            bool exists;
            if (fileExists.TryGetValue(path, out exists))
                return exists ? 1 : 2;
            try
            {
                exists = File.Exists(path);
            }
            catch (System.Exception)
            {
                exists = false;
            }

            fileExists[path] = exists;
            return exists ? 1 : 2;
        }

        private bool DbChanged()
        {
            var list = WbbDatabase.Records;
            string top = list.Count > 0 ? list[0].id : "";
            return list.Count != dbCount || !string.Equals(top, dbTopId, System.StringComparison.Ordinal);
        }

        private void RebuildRows(bool clearFileCache)
        {
            WbbDatabase.EnsureLoaded();
            if (clearFileCache)
            {
                fileExists.Clear();
                lastRefreshTime = EditorApplication.timeSinceStartup;
            }
            var list = WbbDatabase.Records;
            dbCount = list.Count;
            dbTopId = list.Count > 0 ? list[0].id : "";
            rows.Clear();
            totalCount = 0;
            successCount = 0;
            failedCount = 0;
            otherCount = 0;
            oldCount = 0;
            totalSize = 0L;
            lastBuildRaw = "";
            System.DateTime cutoff = System.DateTime.Now.AddDays(-CleanDays);
            for (int i = 0; i < list.Count; i++)
            {
                var record = list[i];
                if (record == null)
                    continue;
                var row = new Row();
                row.record = record;
                string started = record.startedIso == null ? "" : record.startedIso;
                row.dateKey = started.Length >= 10 ? started.Substring(0, 10) : "";
                row.jobKey = record.jobName == null ? "" : record.jobName;
                row.keyName = Lower(record.modName);
                row.keyType = TypeKey(record.ExportType);
                row.keyVersion = Lower(record.modVersion);
                row.keyAuthor = Lower(record.modAuthor);
                row.keyJob = Lower(record.jobName);
                row.keyPrefab = Lower(record.prefabName);
                row.statusBucket = StatusBucket(record.Status);
                row.fileState = ResolveFileState(record);
                row.hasFixups = !string.IsNullOrWhiteSpace(record.fixupReport);
                rows.Add(row);
                totalCount++;
                if (row.statusBucket == 0)
                {
                    successCount++;
                    if (record.outputSizeBytes > 0L)
                        totalSize += record.outputSizeBytes;
                }
                else if (row.statusBucket == 1)
                {
                    failedCount++;
                }
                else
                {
                    otherCount++;
                }

                System.DateTime startedAt;
                if (WbbUtil.TryParseIso(record.startedIso, out startedAt) && startedAt < cutoff)
                    oldCount++;
                if (string.IsNullOrEmpty(lastBuildRaw))
                {
                    lastBuildRaw = string.IsNullOrEmpty(record.finishedIso) ? started : record.finishedIso;
                }
            }

            if (selected.Count > 0)
            {
                var alive = new HashSet<string>(System.StringComparer.Ordinal);
                for (int i = 0; i < rows.Count; i++)
                    alive.Add(rows[i].record.id);
                var drop = new List<string>();
                foreach (string id in selected)
                {
                    if (!alive.Contains(id))
                        drop.Add(id);
                }

                for (int i = 0; i < drop.Count; i++)
                    selected.Remove(drop[i]);
            }

            displayDirty = true;
            filterDirty = true;
            selectionDirty = true;
            hoverIndex = -1;
            strandedDirty = true;
        }

        private static string ShortTypeName(WbbExportType type)
        {
            switch (type)
            {
                case WbbExportType.Character:
                    return L("History.TypeShortCharacter");
                case WbbExportType.Environment:
                    return L("History.TypeShortEnvironment");
                default:
                    return L("History.TypeShortProp");
            }
        }

        private void RefreshDisplay()
        {
            displayDirty = false;
            displayLangVersion = WbbLang.Version;
            string none = L("Common.None");
            string fileFound = L("History.FileFound");
            string fileMissing = L("History.FileMissing");
            string fileUnknown = L("History.FileUnknown");
            fileMissingText = fileMissing;
            fixupBadgeText = L("History.FixupBadge");
            fixupBadgeWidth = System.Math.Max(WbbSize.Badge.MinWidth, WbbStyles.BadgeWidth(fixupBadgeText));
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var record = row.record;
                row.nameText = string.IsNullOrEmpty(record.modName) ? none : record.modName;
                row.typeText = ShortTypeName(record.ExportType);
                row.versionText = string.IsNullOrEmpty(record.modVersion) ? "-" : "v" + record.modVersion;
                row.authorText = string.IsNullOrEmpty(record.modAuthor) ? "-" : record.modAuthor;
                row.durationText = record.durationSec > 0d ? WbbUtil.FormatDuration(record.durationSec) : "-";
                row.sizeText = record.outputSizeBytes > 0L ? WbbUtil.FormatSize(record.outputSizeBytes) : "-";
                string started = record.startedIso == null ? "" : record.startedIso;
                row.timeText = started.Length >= 16 ? started.Substring(5, 11) : started;
                row.statusText = WbbLang.StatusName(record.Status);
                switch (record.Status)
                {
                    case WbbBuildStatus.Success:
                        row.statusTone = WbbTone.Success;
                        break;
                    case WbbBuildStatus.Failed:
                        row.statusTone = WbbTone.Error;
                        break;
                    case WbbBuildStatus.Running:
                        row.statusTone = WbbTone.Running;
                        break;
                    case WbbBuildStatus.Canceled:
                        row.statusTone = WbbTone.Warning;
                        break;
                    default:
                        row.statusTone = WbbTone.Neutral;
                        break;
                }

                if (row.fileState == 1)
                    row.fileText = fileFound;
                else if (row.fileState == 2)
                    row.fileText = fileMissing;
                else
                    row.fileText = fileUnknown;
            }

            string statsTip = L("History.Stats.tip", WbbUtil.FormatSize(totalSize));
            statsContent = new GUIContent(L("History.Stats", totalCount, successCount, failedCount, otherCount), statsTip);
            statsExtraContent = new GUIContent(L("History.StatsExtra", string.IsNullOrEmpty(lastBuildRaw) ? none : lastBuildRaw), statsTip);
            filterDirty = true;
            selectionDirty = true;
        }
    }
}
