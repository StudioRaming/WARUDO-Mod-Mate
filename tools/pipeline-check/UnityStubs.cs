// Stubs for the Unity-side members that WbbWarudoPipeline.cs uses, so its read cache runs here without Unity.
// Only what the cache needs: the editor clock, the located WARUDO folder, the pipeline kind and the warning log.
namespace UnityEditor
{
    public static class EditorApplication
    {
        public static double timeSinceStartup;
    }
}

namespace StudioRaming.WarudoBatchBuilder
{
    public static class WbbWarudoLocator
    {
        public static bool Found;
        public static string StreamingAssetsPath = "";

        // The product reads the WARUDO the builds use; here that is the folder set above, or none when Found is false.
        public static string AmbientStreamingAssets
        {
            get { return Found ? StreamingAssetsPath : ""; }
        }
    }

    public static class WbbUtil
    {
        public enum PipelineKind
        {
            BuiltIn,
            Urp,
            Hdrp,
            Custom,
        }

        public static int Warnings;

        public static void LogWarning(string message)
        {
            Warnings++;
            System.Console.WriteLine("  warning: " + message);
        }
    }
}
