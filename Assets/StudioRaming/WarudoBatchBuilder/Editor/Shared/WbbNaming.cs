using System;
using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbNaming
    {
        public const int MaxAutoNumber = 999;
        public struct Request
        {
            public string baseName;
            public string prefix;
            public bool exact;
        }

        public struct Result
        {
            public string finalName;
            public bool duplicate;
            public bool overwrites;
        }

        public static Result[] Resolve(IList<Request> requests, bool appendDate, bool autoNumber, string dateSuffix, Func<string, bool> exists)
        {
            int count = requests == null ? 0 : requests.Count;
            var results = new Result[count];
            var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < count; i++)
            {
                Request request = requests[i];
                string baseName = WbbUtil.SanitizeModName(request.baseName ?? "");
                if (baseName.Length == 0)
                {
                    results[i] = new Result
                    {
                        finalName = ""
                    };
                    continue;
                }

                string core = baseName;
                if (!request.exact)
                {
                    core = WithPrefix(core, request.prefix);
                    if (appendDate)
                        core += dateSuffix ?? "";
                    core = WbbUtil.SanitizeModName(core);
                }

                string finalName = core;
                if (!request.exact && autoNumber)
                {
                    for (int n = 2; n <= MaxAutoNumber && (taken.Contains(finalName) || Exists(exists, finalName)); n++)
                        finalName = core + "_" + n;
                }

                bool duplicate = taken.Contains(finalName);
                results[i] = new Result
                {
                    finalName = finalName,
                    duplicate = duplicate,
                    overwrites = !duplicate && Exists(exists, finalName),
                };
                taken.Add(finalName);
            }

            return results;
        }

        public static string WithPrefix(string baseName, string prefix)
        {
            baseName = baseName ?? "";
            string clean = WbbUtil.SanitizeModName(prefix == null ? "" : prefix.Trim());
            if (clean.Length == 0 || baseName.Length == 0)
                return baseName;
            if (string.Equals(baseName, clean, StringComparison.OrdinalIgnoreCase))
                return baseName;
            if (baseName.StartsWith(clean + "_", StringComparison.OrdinalIgnoreCase))
                return baseName;
            return clean + "_" + baseName;
        }

        public static string PrefabBaseName(string prefabName, string folderName, int folderPrefabCount, bool useFolderName, string customPrefix)
        {
            prefabName = prefabName ?? "";
            if (useFolderName)
            {
                string folder = (folderName ?? "").Trim();
                if (folder.Length == 0)
                    return prefabName;
                if (folderPrefabCount == 1)
                    return folder;
                return WithPrefix(prefabName, folder);
            }

            return WithPrefix(prefabName, customPrefix);
        }

        private static bool Exists(Func<string, bool> exists, string name)
        {
            return exists != null && exists(name);
        }
    }
}
