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
