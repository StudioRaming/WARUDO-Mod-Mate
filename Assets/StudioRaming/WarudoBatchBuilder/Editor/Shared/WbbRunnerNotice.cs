using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    // A note about a build that Unity stopped from outside (script reload, Play Mode). The runner restores
    // the settings before the reload, but its own memory is gone afterwards, so the note is kept in SessionState.
    // The Build tab shows it until the next build starts; the Console already got it when the build was stopped.
    public static class WbbRunnerNotice
    {
        private const string TextKey = WbbUtil.PrefPrefix + "Runner.Notice";
        public static void Save(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            SessionState.SetString(TextKey, text);
        }

        public static string Peek()
        {
            return SessionState.GetString(TextKey, "");
        }

        public static void Clear()
        {
            SessionState.EraseString(TextKey);
        }
    }
}
