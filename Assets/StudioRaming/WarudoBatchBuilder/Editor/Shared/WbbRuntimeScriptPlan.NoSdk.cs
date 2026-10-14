#if !WBB_WARUDO_SDK
using System;
using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public sealed class WbbRuntimeScriptPlan
    {
        public string[] sources = new string[0];
        public string[] references = new string[0];
        public string[] defines = new string[0];
        public string[] asmdefAssemblies = new string[0];
        public readonly HashSet<string> providedTypes = new HashSet<string>();
        public static void ResetCatalog()
        {
        }

        public static WbbRuntimeScriptPlan Collect(string path)
        {
            throw new InvalidOperationException("Install the WARUDO Mod Tool package to collect runtime sources.");
        }

        public static WbbRuntimeScriptPlan Collect(string path, string warudoStreamingAssets)
        {
            throw new InvalidOperationException("Install the WARUDO Mod Tool package to collect runtime sources.");
        }

        public static WbbRuntimeScriptPlan CollectTypes(IEnumerable<Type> types)
        {
            throw new InvalidOperationException("The WARUDO Mod Tool package is required.");
        }

        public static WbbRuntimeScriptPlan CollectTypes(IEnumerable<Type> types, string warudoStreamingAssets)
        {
            throw new InvalidOperationException("The WARUDO Mod Tool package is required.");
        }

        public static bool AdapterAvailable
        {
            get
            {
                return false;
            }
        }

        public IDisposable BeginBuild(string folder)
        {
            throw new InvalidOperationException("The WARUDO Mod Tool package is required.");
        }
    }
}
#endif
