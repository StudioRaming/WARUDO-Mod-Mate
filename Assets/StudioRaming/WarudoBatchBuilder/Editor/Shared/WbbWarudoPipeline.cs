using System;
using System.IO;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    // Reads which render pipeline WARUDO is set to, from the URP/NiloToon plugin option.
    // Source strings: "not-found" (no WARUDO install), "default" (no option file, WARUDO's default is URP),
    // "file" (read from the option file), "unreadable" (file present but no Enabled value could be read).
    public static partial class WbbWarudoPipeline
    {
        private const string OptionFileName = "Warudo.NiloToon.json";
        private const double RecheckSeconds = 2.0;
        private static string cachedPath = "";
        private static bool cachedExists;
        private static DateTime cachedWriteTimeUtc;
        private static WbbUtil.PipelineKind? cachedKind;
        private static string cachedSource = "not-found";
        // False after a failed read. WARUDO may hold the file open while writing it, or a read may catch it half
        // written; the time stamp need not change afterwards, so a failed result is read again instead of kept.
        private static bool cachedReadable;
        private static string lastWarning = "";
        private static double lastCheckTime = double.NegativeInfinity;

        public static WbbUtil.PipelineKind? Read(out string source)
        {
            if (!WbbWarudoLocator.Found)
            {
                source = "not-found";
                return null;
            }

            string path = Path.Combine(WbbWarudoLocator.StreamingAssetsPath, "Plugins", "Data", OptionFileName);
            double now = EditorApplication.timeSinceStartup;
            if (path == cachedPath && now - lastCheckTime < RecheckSeconds)
            {
                source = cachedSource;
                return cachedKind;
            }

            lastCheckTime = now;
            bool exists = File.Exists(path);
            DateTime writeTimeUtc = DateTime.MinValue;
            try
            {
                if (exists)
                    writeTimeUtc = File.GetLastWriteTimeUtc(path);
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not read the WARUDO pipeline option time: " + e.Message);
            }

            if (path != cachedPath || exists != cachedExists || writeTimeUtc != cachedWriteTimeUtc || !cachedReadable)
            {
                cachedPath = path;
                cachedExists = exists;
                cachedWriteTimeUtc = writeTimeUtc;
                Resolve(path, exists);
            }

            source = cachedSource;
            return cachedKind;
        }

        private static void Resolve(string path, bool exists)
        {
            cachedReadable = true;
            if (!exists)
            {
                cachedKind = WbbUtil.PipelineKind.Urp;
                cachedSource = "default";
                lastWarning = "";
                return;
            }

            try
            {
                bool? enabled = ParseNiloToonEnabled(File.ReadAllText(path));
                if (enabled.HasValue)
                {
                    cachedKind = enabled.Value ? WbbUtil.PipelineKind.Urp : WbbUtil.PipelineKind.BuiltIn;
                    cachedSource = "file";
                    lastWarning = "";
                    return;
                }
            }
            catch (Exception e)
            {
                // Retried every few seconds while it fails, so the same message is logged once.
                string warning = "Could not read the WARUDO pipeline option: " + e.Message;
                if (warning != lastWarning)
                    WbbUtil.LogWarning(warning);
                lastWarning = warning;
            }

            cachedReadable = false;
            cachedKind = null;
            cachedSource = "unreadable";
        }
    }
}
