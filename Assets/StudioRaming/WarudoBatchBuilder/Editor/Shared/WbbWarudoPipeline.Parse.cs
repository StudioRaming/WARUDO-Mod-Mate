using System.Text.RegularExpressions;

namespace StudioRaming.WarudoBatchBuilder
{
    // Parse half of WbbWarudoPipeline. Kept free of Unity so tools/pipeline-check can compile it alone.
    public static partial class WbbWarudoPipeline
    {
        // WARUDO stores the URP/NiloToon "Enabled" option as a dictionary entry. The value sits in the same
        // {...} block as the key, so the match cannot run past that block into another plugin option.
        private static readonly Regex NiloToonEnabled = new Regex("\"Enabled\"\\s*:\\s*\\{[^{}]*?\"value\"\\s*:\\s*\"(true|false)\"");

        // Returns true for URP, false for Built-in, null when the file has no readable Enabled value.
        public static bool? ParseNiloToonEnabled(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            Match match = NiloToonEnabled.Match(json);
            if (!match.Success)
                return null;
            return match.Groups[1].Value == "true";
        }
    }
}
