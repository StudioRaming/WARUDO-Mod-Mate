using System;
using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbDatabase
    {
        private static int CompareRecordsNewestFirst(WbbRecord a, WbbRecord b)
        {
            string sa = a.startedIso == null ? "" : a.startedIso;
            string sb = b.startedIso == null ? "" : b.startedIso;
            int cmp = string.CompareOrdinal(sb, sa);
            if (cmp != 0)
                return cmp;
            string ia = a.id == null ? "" : a.id;
            string ib = b.id == null ? "" : b.id;
            return string.CompareOrdinal(ib, ia);
        }

        private static void SortRecords()
        {
            records.Sort(CompareRecordsNewestFirst);
        }

        private static void InsertSorted(WbbRecord record)
        {
            int index = 0;
            while (index < records.Count && CompareRecordsNewestFirst(record, records[index]) > 0)
                index++;
            records.Insert(index, record);
        }

        private static void TrimRecords()
        {
            if (records.Count <= MaxRecords)
                return;
            records.RemoveRange(MaxRecords, records.Count - MaxRecords);
        }

        public static void AddRecord(WbbRecord record)
        {
            EnsureLoaded();
            if (record == null)
                return;
            SanitizeRecord(record);
            if (string.IsNullOrEmpty(record.startedIso))
                record.startedIso = WbbUtil.NowIso();
            InsertSorted(record);
            TrimRecords();
            SaveRecords();
        }

        public static void UpdateRecord(WbbRecord record)
        {
            EnsureLoaded();
            if (record == null)
                return;
            SanitizeRecord(record);
            if (string.IsNullOrEmpty(record.startedIso))
                record.startedIso = WbbUtil.NowIso();
            int found = -1;
            for (int i = 0; i < records.Count; i++)
            {
                if (string.Equals(records[i].id, record.id, StringComparison.Ordinal))
                {
                    found = i;
                    break;
                }
            }

            if (found >= 0)
            {
                records.RemoveAt(found);
                InsertSorted(record);
            }
            else
            {
                InsertSorted(record);
            }

            TrimRecords();
            SaveRecords();
        }

        public static void DeleteRecord(string id)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(id))
                return;
            bool removed = false;
            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (string.Equals(records[i].id, id, StringComparison.Ordinal))
                {
                    records.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
                SaveRecords();
        }

        public static void DeleteRecords(IEnumerable<string> ids)
        {
            EnsureLoaded();
            if (ids == null)
                return;
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                    set.Add(id);
            }

            if (set.Count == 0)
                return;
            bool removed = false;
            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (set.Contains(records[i].id))
                {
                    records.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
                SaveRecords();
        }

        public static void ClearRecords()
        {
            EnsureLoaded();
            if (records.Count == 0)
                return;
            records.Clear();
            SaveRecords();
        }

        public static WbbRecord FindLatestSuccessByPrefab(string prefabGuid)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(prefabGuid))
                return null;
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                if (record.status != (int)WbbBuildStatus.Success)
                    continue;
                if (string.Equals(record.prefabGuid, prefabGuid, StringComparison.Ordinal))
                    return record;
            }

            return null;
        }

        public static WbbRecord FindLatestByPrefab(string prefabGuid)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(prefabGuid))
                return null;
            for (int i = 0; i < records.Count; i++)
            {
                if (string.Equals(records[i].prefabGuid, prefabGuid, StringComparison.Ordinal))
                    return records[i];
            }

            return null;
        }

        public static WbbRecord FindLatestByModName(string modName)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(modName))
                return null;
            for (int i = 0; i < records.Count; i++)
            {
                if (string.Equals(records[i].modName, modName, StringComparison.OrdinalIgnoreCase))
                    return records[i];
            }

            return null;
        }
    }
}
