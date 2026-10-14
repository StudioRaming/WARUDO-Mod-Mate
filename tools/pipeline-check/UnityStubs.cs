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
        // Reset to make the folder check hang, as it does on a share that does not answer; it goes on once this is set.
        public static readonly System.Threading.ManualResetEventSlim FolderCheck = new System.Threading.ManualResetEventSlim(true);
        // How many folder checks started, and on which thread the last one ran.
        public static int FolderChecks;
        public static int FolderCheckThread;

        public sealed class AmbientInputs
        {
            public readonly string root;

            public AmbientInputs(string root)
            {
                this.root = root;
            }

            public bool SameAs(AmbientInputs other)
            {
                return other != null && root == other.root;
            }
        }

        // The product reads the WARUDO the builds use; here that is the folder set above, or none when Found is false.
        public static AmbientInputs CaptureAmbient()
        {
            return new AmbientInputs(Found ? StreamingAssetsPath : "");
        }

        public static string ResolveAmbient(AmbientInputs inputs)
        {
            System.Threading.Interlocked.Increment(ref FolderChecks);
            FolderCheck.Wait();
            FolderCheckThread = System.Environment.CurrentManagedThreadId;
            return inputs.root;
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
