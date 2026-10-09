using System.Collections.Generic;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbFolderPicker
    {
        public static List<string> GetRecent(string recentKey)
        {
            var list = new List<string>();
            string key = RecentPrefKey(recentKey);
            string raw = WbbPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(raw))
                return list;
            string[] guids = raw.Split(';');
            var keptGuids = new List<string>(guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                if (string.IsNullOrEmpty(guid))
                    continue;
                string path = WbbUtil.NormalizePath(AssetDatabase.GUIDToAssetPath(guid));
                if (!WbbUtil.IsValidProjectFolder(path))
                    continue;
                if (list.Contains(path))
                    continue;
                list.Add(path);
                keptGuids.Add(guid);
                if (list.Count >= MaxRecent)
                    break;
            }

            if (keptGuids.Count != guids.Length)
                WbbPrefs.SetString(key, string.Join(";", keptGuids.ToArray()));
            return list;
        }

        public static void AddRecent(string recentKey, string folderPath)
        {
            string path = WbbUtil.NormalizePath(folderPath);
            if (!WbbUtil.IsValidProjectFolder(path))
                return;
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
                return;
            string key = RecentPrefKey(recentKey);
            string raw = WbbPrefs.GetString(key, "");
            var guids = new List<string>();
            guids.Add(guid);
            if (!string.IsNullOrEmpty(raw))
            {
                string[] old = raw.Split(';');
                for (int i = 0; i < old.Length; i++)
                {
                    if (string.IsNullOrEmpty(old[i]))
                        continue;
                    if (old[i] == guid)
                        continue;
                    guids.Add(old[i]);
                    if (guids.Count >= MaxRecent)
                        break;
                }
            }

            WbbPrefs.SetString(key, string.Join(";", guids.ToArray()));
        }

        public static void ClearRecent(string recentKey)
        {
            WbbPrefs.DeleteKey(RecentPrefKey(recentKey));
        }

        private static string RecentPrefKey(string recentKey)
        {
            return WbbUtil.PrefPrefix + (string.IsNullOrEmpty(recentKey) ? "Recent.Default" : recentKey);
        }
    }
}
