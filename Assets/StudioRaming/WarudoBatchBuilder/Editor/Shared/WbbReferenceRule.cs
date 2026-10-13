using System;
using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    // Judges one assembly reference of a compiled runtime unit. Pure on purpose: no Unity types, so the dotnet
    // catalog-check compiles this very file against WARUDO's real Managed folder.
    internal static class WbbReferenceRule
    {
        public const string Ok = "ok";
        public const string Editor = "editor";
        public const string Missing = "missing";
        public const string Unchecked = "unchecked";

        // known: the other assemblies of the same mod. hostNames: the assemblies WARUDO ships, or null when WARUDO was not found.
        // Only mscorlib and netstandard pass without the host list. System.* and Mono.* are no exception: WARUDO's Managed
        // folder lacks System.Text.Json and Mono.Cecil, and a runtime unit that needs them loses its scripts at load time.
        public static string Classify(string name, ISet<string> known, ICollection<string> hostNames)
        {
            if (name == "UnityEditor" || name.StartsWith("UnityEditor.", StringComparison.Ordinal) || name == "Assembly-CSharp" || name.StartsWith("Assembly-CSharp-", StringComparison.Ordinal))
                return Editor;
            if (known != null && known.Contains(name))
                return Ok;
            if (hostNames == null)
                return Unchecked;
            if (name == "mscorlib" || name == "netstandard")
                return Ok;
            return hostNames.Contains(name) ? Ok : Missing;
        }
    }
}
