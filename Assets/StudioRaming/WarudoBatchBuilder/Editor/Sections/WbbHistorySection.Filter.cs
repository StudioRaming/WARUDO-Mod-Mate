using System.Collections.Generic;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
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
                    if (prefix == "name" || prefix == "type" || prefix == "ver" || prefix == "author" || prefix == "job" || prefix == "state" || prefix == "file")
                    {
                        domain = prefix;
                        value = value.Substring(colon + 1);
                    }
                }

                value = value.Trim().ToLowerInvariant();
                if (value.Length > 0)
                    result.Add(new Token { domain = domain, value = value });
            }

            return result;
        }

        private static bool MatchesState(Row row, string value)
        {
            if (value == "ok" || value[0] == 's')
                return row.statusBucket == 0;
            if (value[0] == 'f')
                return row.statusBucket == 1;
            if (value[0] == 'o')
                return row.statusBucket == 2;
            return false;
        }

        private static bool MatchesFile(Row row, string value)
        {
            if (value[0] == 'y' || value[0] == '1' || value == "ok")
                return row.fileState == 1;
            if (value[0] == 'n' || value[0] == '0' || value[0] == 'm')
                return row.fileState == 2;
            return false;
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
                    case "name":
                        match = row.keyName.Contains(token.value);
                        break;
                    case "type":
                        match = row.keyType.Contains(token.value);
                        break;
                    case "ver":
                        match = row.keyVersion.Contains(token.value);
                        break;
                    case "author":
                        match = row.keyAuthor.Contains(token.value);
                        break;
                    case "job":
                        match = row.keyJob.Contains(token.value);
                        break;
                    case "state":
                        match = MatchesState(row, token.value);
                        break;
                    case "file":
                        match = MatchesFile(row, token.value);
                        break;
                    default:
                        match = row.keyName.Contains(token.value) || row.keyPrefab.Contains(token.value) || row.keyAuthor.Contains(token.value) || row.keyJob.Contains(token.value);
                        break;
                }

                if (!match)
                    return false;
            }

            return true;
        }

        private bool PassesFilter(Row row, List<Token> tokens)
        {
            int t = Mathf.Clamp(row.record.exportType, 0, TypeCount - 1);
            if (!typeEnabled[t])
                return false;
            if (!statusEnabled[Mathf.Clamp(row.statusBucket, 0, StatusCount - 1)])
                return false;
            return Matches(row, tokens);
        }

        private string CollapseKey(string groupKey)
        {
            return groupMode + ":" + (groupKey == null ? "" : groupKey);
        }

        private void RebuildEntries()
        {
            filterDirty = false;
            entries.Clear();
            var tokens = ParseTokens(search);
            var pass = new List<int>();
            showAuthorColumn = false;
            showFileColumn = false;
            failedInView = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                Row row = rows[i];
                if (!PassesFilter(row, tokens))
                    continue;
                pass.Add(i);
                if (!showAuthorColumn && !string.IsNullOrEmpty(row.record.modAuthor))
                    showAuthorColumn = true;
                if (row.fileState == 2)
                    showFileColumn = true;
                if (row.statusBucket == 1)
                    failedInView++;
            }

            if (groupMode == GroupFlat)
            {
                for (int i = 0; i < pass.Count; i++)
                    entries.Add(new Entry { header = false, groupKey = "", text = "", row = pass[i] });
            }
            else
            {
                var order = new List<string>();
                var buckets = new Dictionary<string, List<int>>(System.StringComparer.Ordinal);
                for (int i = 0; i < pass.Count; i++)
                {
                    int rowIndex = pass[i];
                    string key = groupMode == GroupByJob ? rows[rowIndex].jobKey : rows[rowIndex].dateKey;
                    List<int> bucket;
                    if (!buckets.TryGetValue(key, out bucket))
                    {
                        bucket = new List<int>();
                        buckets[key] = bucket;
                        order.Add(key);
                    }

                    bucket.Add(rowIndex);
                }

                for (int g = 0; g < order.Count; g++)
                {
                    string key = order[g];
                    List<int> bucket = buckets[key];
                    int ok = 0;
                    int fail = 0;
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        int b = rows[bucket[i]].statusBucket;
                        if (b == 0)
                            ok++;
                        else if (b == 1)
                            fail++;
                    }

                    string title;
                    if (string.IsNullOrEmpty(key))
                        title = groupMode == GroupByJob ? L("History.GroupNoJob") : L("Common.None");
                    else
                        title = key;
                    string collapseKey = CollapseKey(key);
                    entries.Add(new Entry { header = true, groupKey = collapseKey, text = L("History.GroupHeader", title, bucket.Count, ok, fail), row = -1, });
                    if (collapsed.Contains(collapseKey))
                        continue;
                    for (int i = 0; i < bucket.Count; i++)
                        entries.Add(new Entry { header = false, groupKey = collapseKey, text = "", row = bucket[i] });
                }
            }

            filteredText = L("History.Showing", pass.Count, rows.Count);
        }

        private void SyncStructure()
        {
            if (pendingForceRefresh)
            {
                pendingForceRefresh = false;
                RebuildRows(true);
            }
            else if (DbChanged())
            {
                RebuildRows(false);
            }

            if (displayDirty || displayLangVersion != WbbLang.Version)
                RefreshDisplay();
            if (filterDirty)
                RebuildEntries();
            SyncNotices();
            if (statusLinePending)
            {
                statusLinePending = false;
                statusLineText = pendingStatusLineText;
                statusLineTone = pendingStatusLineTone;
            }

            float viewHeight = host != null ? Mathf.Max(120f, host.position.height) : 600f;
            int count = entries.Count;
            if (count <= 0)
            {
                firstVisible = 0;
                lastVisible = -1;
                return;
            }

            firstVisible = Mathf.Clamp((int)((scroll.y - CullPad) / EntryHeight), 0, count - 1);
            lastVisible = Mathf.Clamp((int)((scroll.y + viewHeight + CullPad) / EntryHeight), 0, count - 1);
        }
    }
}
