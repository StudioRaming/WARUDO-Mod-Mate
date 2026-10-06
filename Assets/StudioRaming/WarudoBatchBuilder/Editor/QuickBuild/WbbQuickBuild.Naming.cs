using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbQuickBuild
    {
        public const int MaxAutoNumber = WbbNaming.MaxAutoNumber;
        public const string AppendDatePrefKey = WbbUtil.PrefPrefix + "Quick.AppendDate";
        public const string AutoNumberPrefKey = WbbUtil.PrefPrefix + "Quick.AutoNumber";
        public const string PrependScenePrefKey = WbbUtil.PrefPrefix + "Quick.PrependScene";
        public struct NameRequest
        {
            public string baseName;
            public bool exact;
            public string sceneName;
        }

        public struct NameResult
        {
            public string finalName;
            public bool duplicate;
            public bool overwrites;
        }

        public static NameResult[] ResolveNames(IList<NameRequest> requests, bool prependScene, bool appendDate, bool autoNumber, string dateSuffix, System.Func<string, bool> exists)
        {
            int count = requests == null ? 0 : requests.Count;
            var shared = new List<WbbNaming.Request>(count);
            for (int i = 0; i < count; i++)
            {
                NameRequest request = requests[i];
                shared.Add(new WbbNaming.Request { baseName = request.baseName, prefix = prependScene ? request.sceneName : "", exact = request.exact, });
            }

            WbbNaming.Result[] resolved = WbbNaming.Resolve(shared, appendDate, autoNumber, dateSuffix, exists);
            var results = new NameResult[count];
            for (int i = 0; i < count; i++)
            {
                results[i] = new NameResult
                {
                    finalName = resolved[i].finalName,
                    duplicate = resolved[i].duplicate,
                    overwrites = resolved[i].overwrites,
                };
            }

            return results;
        }

        public static string WithScenePrefix(string baseName, string sceneName)
        {
            return WbbNaming.WithPrefix(baseName, sceneName);
        }
    }
}
