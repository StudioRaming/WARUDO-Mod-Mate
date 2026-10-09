using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbLibrarySection
    {
        private void RefreshDisplay()
        {
            displayDirty = false;
            displayLangVersion = WbbLang.Version;
            string noVersion = L("Library.NoVersion");
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var file = row.file;
                row.nameText = file.fileName + file.extension;
                row.sizeText = WbbUtil.FormatSize(file.sizeBytes);
                row.dateText = WbbUtil.ToIso(file.lastWrite);
                if (row.record != null && !string.IsNullOrEmpty(row.record.modVersion))
                    row.versionText = row.record.modVersion;
                else
                    row.versionText = noVersion;
                string tipKey;
                if (row.record == null)
                {
                    row.badgeText = L("Library.BadgeUntracked");
                    row.badgeColor = WbbStyles.Muted;
                    tipKey = "Library.BadgeUntracked.tip";
                }
                else if (row.record.Status == WbbBuildStatus.Success)
                {
                    row.badgeText = L("Library.BadgeTracked");
                    row.badgeColor = WbbStyles.Success;
                    tipKey = "Library.BadgeTracked.tip";
                }
                else if (row.record.Status == WbbBuildStatus.Failed)
                {
                    row.badgeText = L("Library.BadgeFailed");
                    row.badgeColor = WbbStyles.Danger;
                    tipKey = "Library.BadgeFailed.tip";
                }
                else
                {
                    row.badgeText = L("Library.BadgeHistory");
                    row.badgeColor = WbbStyles.Warning;
                    tipKey = "Library.BadgeHistory.tip";
                }

                row.badgeContent = new GUIContent(string.Empty, L(tipKey));
            }

            RebuildSummary();
            filterDirty = true;
            selectionDirty = true;
        }

        private void RebuildSummary()
        {
            int total = 0;
            long totalSize = 0L;
            for (int t = 0; t < TypeCount; t++)
            {
                total += typeCounts[t];
                totalSize += typeSizes[t];
            }

            summaryLine = L("Library.Summary", total, WbbUtil.FormatSize(totalSize), trackedCount, untrackedCount);
            BuildTypeChips();
        }

        private void BuildTypeChips()
        {
            if (typeToggleContents == null)
                typeToggleContents = new GUIContent[TypeCount];
            typeToggleContents[0] = new GUIContent(L("Library.TypeChipProp", typeCounts[0], WbbUtil.FormatSize(typeSizes[0])), L("Library.FilterProp.tip"));
            typeToggleContents[1] = new GUIContent(L("Library.TypeChipCharacter", typeCounts[1], WbbUtil.FormatSize(typeSizes[1])), L("Library.FilterCharacter.tip"));
            typeToggleContents[2] = new GUIContent(L("Library.TypeChipEnvironment", typeCounts[2], WbbUtil.FormatSize(typeSizes[2])), L("Library.FilterEnvironment.tip"));
            float width = WbbStyles.ToggleGroupWidth(typeToggleContents);
            for (int t = 0; t < TypeCount; t++)
            {
                chipWidths[t] = width;
                chipGaps[t] = 0f;
            }
        }

        private void PlanTypeChips()
        {
            if (host == null)
                return;
            float available = host.position.width - WbbSize.SpaceEdge * 2f;
            WbbStyles.PlanWrap(chipWidths, chipGaps, available, chipBreak);
        }

        private struct Token
        {
            public string domain;
            public string value;
        }

        private static List<Token> ParseTokens(string query)
        {
            var result = new List<Token>();
            if (string.IsNullOrEmpty(query))
                return result;
            string[] parts = query.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                string domain = "any";
                string value = parts[i];
                int colon = value.IndexOf(':');
                if (colon > 0)
                {
                    string prefix = value.Substring(0, colon).ToLowerInvariant();
                    if (prefix == "type" || prefix == "ext" || prefix == "folder" || prefix == "author" || prefix == "ver" || prefix == "state" || prefix == "name")
                    {
                        domain = prefix;
                        value = value.Substring(colon + 1);
                    }
                }

                value = value.Trim().ToLowerInvariant().TrimStart('.');
                if (value.Length > 0)
                    result.Add(new Token { domain = domain, value = value });
            }

            return result;
        }

        private static bool Matches(Row row, List<Token> tokens)
        {
            if (tokens == null || tokens.Count == 0)
                return true;
            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                bool match;
                switch (token.domain)
                {
                    case "type":
                        match = row.keyType.Contains(token.value);
                        break;
                    case "ext":
                        match = row.keyExt.Contains(token.value);
                        break;
                    case "folder":
                        match = row.keyFolder.Contains(token.value);
                        break;
                    case "author":
                        match = row.keyAuthor.Contains(token.value);
                        break;
                    case "ver":
                        match = row.keyVersion.Contains(token.value);
                        break;
                    case "state":
                        if (token.value[0] == 'u')
                            match = !row.tracked;
                        else if (token.value[0] == 't')
                            match = row.tracked;
                        else
                            match = false;
                        break;
                    case "name":
                        match = row.keyName.Contains(token.value);
                        break;
                    default:
                        match = row.keyName.Contains(token.value) || row.keyFolder.Contains(token.value) || row.keyAuthor.Contains(token.value);
                        break;
                }

                if (!match)
                    return false;
            }

            return true;
        }

        private void RebuildFiltered()
        {
            filterDirty = false;
            filtered.Clear();
            var tokens = ParseTokens(search);
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                int t = Mathf.Clamp(row.file.exportType, 0, TypeCount - 1);
                if (!typeEnabled[t])
                    continue;
                if (!Matches(row, tokens))
                    continue;
                filtered.Add(i);
            }

            SortFiltered();
            filteredText = L("Library.FilteredCount", filtered.Count, rows.Count);
        }

        private void SortFiltered()
        {
            if (filtered.Count < 2)
                return;
            filtered.Sort(delegate (int a, int b)
            {
                Row ra = rows[a];
                Row rb = rows[b];
                int result;
                switch (sortMode)
                {
                    case 1:
                        result = string.Compare(ra.file.fileName, rb.file.fileName, System.StringComparison.OrdinalIgnoreCase);
                        break;
                    case 2:
                        result = rb.file.sizeBytes.CompareTo(ra.file.sizeBytes);
                        break;
                    default:
                        result = rb.file.lastWrite.CompareTo(ra.file.lastWrite);
                        break;
                }

                if (result != 0)
                    return result;
                return string.Compare(ra.file.filePath, rb.file.filePath, System.StringComparison.OrdinalIgnoreCase);
            });
        }

        private void SyncStructure()
        {
            if (pendingPathsOpen >= 0)
            {
                pathsOpen = pendingPathsOpen == 1;
                pendingPathsOpen = -1;
                WbbPrefs.SetBool(Pfx + "PathsOpen", pathsOpen);
            }

            if (pendingForceRescan)
            {
                pendingForceRescan = false;
                RescanNow();
            }

            if (displayDirty || displayLangVersion != WbbLang.Version)
                RefreshDisplay();
            if (filterDirty)
                RebuildFiltered();
            PlanTypeChips();
            if (statusPending)
            {
                statusPending = false;
                statusText = pendingStatusText;
                statusTone = pendingStatusTone;
            }

            float viewHeight = host != null ? Mathf.Max(120f, host.position.height) : 600f;
            int count = filtered.Count;
            if (count <= 0)
            {
                firstVisible = 0;
                lastVisible = -1;
                return;
            }

            firstVisible = Mathf.Clamp((int)((scroll.y - CullPad) / RowHeight), 0, count - 1);
            lastVisible = Mathf.Clamp((int)((scroll.y + viewHeight + CullPad) / RowHeight), 0, count - 1);
        }
    }
}
