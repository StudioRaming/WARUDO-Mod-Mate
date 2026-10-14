using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using StudioRaming.WarudoBatchBuilder;

namespace StudioRaming.WarudoBatchBuilder
{
    // Stubs for the Unity-side helpers WbbWarudoTypeCatalog calls, which are not compiled here. The message key is
    // enough to tell the two read errors apart; the log line goes to the console.
    internal static class WbbLang
    {
        public static string T(string key, params object[] args)
        {
            return args.Length == 0 ? key : key + ": " + string.Join(", ", args);
        }
    }

    internal static class WbbUtil
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }

    // Stub for the Unity-side locator: SteamBuildId only needs WARUDO's Steam app id.
    internal static class WbbWarudoLocator
    {
        public const string AppId = "2079120";
    }
}

static class Program
{
    // Expected results, one row per case. Rows 1-13 are the cases of the original WBB-03 harness (baseline output in the
    // Ops record). A row that differs from WARUDO's actual result fails the run. Rows that name WARUDO types must be
    // checked again after a WARUDO update; the failure output prints its version. The generic nested rows are not listed
    // here: they are built from Warudo.Core.dll at run time (GenericNestedName, in Main).
    static readonly (string Asm, string Name, bool Expected)[] FixedCases =
    {
        ("System", "System.Collections.Generic.Stack`1", true),
        ("System", "System.Collections.Generic.Stack`1+Enumerator", true),
        ("System", "System.Collections.Generic.Stack`1+DoesNotExist", false),
        ("System", "System.Collections.Generic.Queue`1", true),
        ("System", "System.Collections.Generic.Queue`1+Enumerator", true),
        ("mscorlib", "System.String", true),
        ("mscorlib", "System.Collections.Generic.List`1+Enumerator", true),
        ("Warudo.Core", "BleApi", true),
        ("Warudo.Core", "BleApi+ScanStatus", true),
        ("Warudo.Core", "BleApi+DoesNotExist", false),
        ("mscorlib", "System.Does.NotExist", false),
        ("System", "System.Does.NotExist", false),
        ("NoSuchAssembly", "System.String", false),
    };

    // WbbReferenceRule.Classify rows: reference name, whether another assembly of the mod has that name, whether WARUDO was
    // not found (host list null), expected verdict. The "missing" rows are DLLs absent from WARUDO's Managed folder and the
    // "ok" rows ones present in it; a WARUDO update can change them, and the failure output names the build.
    static readonly (string Name, bool InKnown, bool HostNull, string Expected)[] RefCases =
    {
        ("System.Core", false, false, "ok"),
        ("System", false, false, "ok"),
        ("mscorlib", false, false, "ok"),
        ("netstandard", false, false, "ok"),
        ("System.Memory", false, false, "ok"),
        ("System.Text.Json", false, false, "missing"),
        ("System.Numerics.Vectors", false, false, "missing"),
        ("Mono.Cecil", false, false, "missing"),
        ("UnityEditor.CoreModule", false, false, "editor"),
        ("Assembly-CSharp", false, false, "editor"),
        ("Wmm.FixtureOk.Runtime", true, false, "ok"),
        ("System.Text.Json", false, true, "unchecked"),
    };

    static string DefaultStreamingAssets => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        "Steam", "steamapps", "common", "Warudo", "Warudo_Data", "StreamingAssets");

    static int Main(string[] args)
    {
        // A trailing separator (shell tab completion adds one) would make GetDirectoryName return StreamingAssets itself.
        string streaming = Path.GetFullPath(args.Length > 0 ? args[0] : DefaultStreamingAssets)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string managed = Path.Combine(Path.GetDirectoryName(streaming), "Managed");
        string version = VersionOf(Path.Combine(managed, "Warudo.Core.dll"));
        Console.WriteLine("WARUDO managed: " + managed + " | Warudo.Core.dll version: " + version);

        WbbWarudoTypeCatalog catalog;
        try
        {
            catalog = WbbWarudoTypeCatalog.Read(streaming);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR reading WARUDO catalog (WARUDO " + version + "): " + e.Message);
            return 2;
        }

        var cases = new List<(string Asm, string Name, bool Expected)>(FixedCases);
        string generic = GenericNestedName(Path.Combine(managed, "Warudo.Core.dll"));
        if (generic.Length > 0)
        {
            // Generic outer and generic inner, so the check covers backtick arity on both sides of the '+'.
            cases.Add(("Warudo.Core", generic, true));
            cases.Add(("Warudo.Core", generic.Substring(0, generic.IndexOf('+')) + "+DoesNotExist`1", false));
            Console.WriteLine("generic nested case found in Warudo.Core.dll: " + generic);
        }
        else
        {
            Console.WriteLine("SKIP generic nested cases: Warudo.Core.dll has no generic type nested in a generic type"
                + " (WARUDO " + version + ")");
        }

        int failures = 0;
        Console.WriteLine("case | expected | actual");
        foreach (var c in cases)
        {
            string path;
            bool actual = catalog.Contains(c.Asm, c.Name, out path);
            Console.WriteLine(c.Asm + ":" + c.Name + " | " + c.Expected + " | " + actual);
            if (actual != c.Expected)
            {
                failures++;
                Console.WriteLine("MISMATCH " + c.Asm + ":" + c.Name + " expected " + c.Expected + " got " + actual
                    + " (WARUDO " + version + ")");
            }
        }

        // hostNames: what WARUDO ships, by file name without extension, like WbbWarudoManaged.Read builds it.
        var hostNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string file in Directory.GetFiles(managed, "*.dll"))
            hostNames.Add(Path.GetFileNameWithoutExtension(file));
        var knownNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Wmm.FixtureOk.Runtime" };
        int refFailures = 0;
        Console.WriteLine("reference | inKnown | hostNull | expected | actual");
        foreach (var r in RefCases)
        {
            string verdict = WbbReferenceRule.Classify(r.Name, r.InKnown ? knownNames : new HashSet<string>(),
                r.HostNull ? null : hostNames);
            Console.WriteLine(r.Name + " | " + r.InKnown + " | " + r.HostNull + " | " + r.Expected + " | " + verdict);
            if (verdict != r.Expected)
            {
                refFailures++;
                Console.WriteLine("MISMATCH reference " + r.Name + " expected " + r.Expected + " got " + verdict);
            }
        }

        if (refFailures > 0)
        {
            Console.WriteLine("FAIL " + refFailures + " of " + RefCases.Length + " reference cases differ (WARUDO managed: "
                + managed + ", Steam buildid " + SteamBuildId(streaming) + ")");
            return 1;
        }

        Console.WriteLine("PASS " + RefCases.Length + "/" + RefCases.Length + " reference cases match (Steam buildid "
            + SteamBuildId(streaming) + ")");
        string skipped = generic.Length > 0 ? "" : ", generic nested cases skipped";
        if (failures > 0)
        {
            Console.WriteLine("FAIL " + failures + " of " + cases.Count + " cases differ" + skipped + " (WARUDO " + version + ")");
            return 1;
        }

        // Without the two generic nested rows the arity on both sides of '+' is not checked, so the run is not a pass.
        if (generic.Length == 0)
        {
            Console.WriteLine("INCONCLUSIVE " + cases.Count + "/" + cases.Count + " cases match" + skipped + " (WARUDO " + version + ")");
            return 3;
        }

        Console.WriteLine("PASS " + cases.Count + "/" + cases.Count + " cases match (WARUDO " + version + ")");
        return 0;
    }

    // "buildid" of steamapps/appmanifest_2079120.acf, read the way tools/warudo-fingerprint does. The steamapps folder is
    // the nearest parent of the StreamingAssets argument with that name; a WARUDO that is not under Steam has none.
    static string SteamBuildId(string streaming)
    {
        for (string dir = streaming; !string.IsNullOrEmpty(dir); dir = Path.GetDirectoryName(dir))
        {
            if (!string.Equals(Path.GetFileName(dir), "steamapps", StringComparison.OrdinalIgnoreCase)) continue;
            string manifest = Path.Combine(dir, "appmanifest_" + WbbWarudoLocator.AppId + ".acf");
            if (!File.Exists(manifest)) return "unknown (no " + Path.GetFileName(manifest) + ")";
            var match = System.Text.RegularExpressions.Regex.Match(File.ReadAllText(manifest), @"""buildid""\s+""(\d+)""");
            return match.Success ? match.Groups[1].Value : "unknown (no buildid in the manifest)";
        }

        return "unknown (not under a steamapps folder)";
    }

    // The first (by ordinal full name) type in the DLL whose name and whose top-level declaring type both carry a generic
    // arity, as a metadata name "Namespace.Outer`n+Inner`m". Read from metadata, not hard-coded: such types are
    // compiler-generated closures whose names change when WARUDO edits its lambdas. "" when there is none.
    static string GenericNestedName(string dll)
    {
        if (!File.Exists(dll)) return "";
        using (var stream = File.OpenRead(dll))
        using (var pe = new PEReader(stream))
        {
            MetadataReader md = pe.GetMetadataReader();
            var names = new List<string>();
            foreach (TypeDefinitionHandle handle in md.TypeDefinitions)
            {
                TypeDefinition inner = md.GetTypeDefinition(handle);
                TypeDefinitionHandle outerHandle = inner.GetDeclaringType();
                if (outerHandle.IsNil) continue;
                TypeDefinition outer = md.GetTypeDefinition(outerHandle);
                if (!outer.GetDeclaringType().IsNil) continue;
                string innerName = md.GetString(inner.Name);
                string outerName = md.GetString(outer.Name);
                if (innerName.IndexOf('`') < 0 || outerName.IndexOf('`') < 0) continue;
                string ns = md.GetString(outer.Namespace);
                names.Add((ns.Length > 0 ? ns + "." : "") + outerName + "+" + innerName);
            }

            return names.OrderBy(n => n, StringComparer.Ordinal).FirstOrDefault() ?? "";
        }
    }

    // WARUDO ships no version string in Warudo.Core.dll (FileVersion is 0.0.0.0), so the identity is the assembly
    // version plus the file size and time stamp.
    static string VersionOf(string dll)
    {
        if (!File.Exists(dll)) return "missing";
        var info = new FileInfo(dll);
        string assembly = AssemblyName.GetAssemblyName(dll).Version.ToString();
        return assembly + " (" + info.Length + " bytes, modified " + info.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm") + " UTC)";
    }
}
