using System;
using System.IO;
using System.Reflection;
using StudioRaming.WarudoBatchBuilder;

namespace StudioRaming.WarudoBatchBuilder
{
    // Stub for the Unity-side locator, which is not compiled here. Only the path is needed by WbbWarudoTypeCatalog.
    internal static class WbbWarudoLocator
    {
        public static string StreamingAssetsPath;
    }
}

static class Program
{
    // Expected results, one row per case. Rows 1-13 are the cases of the original WBB-03 harness (baseline output in the
    // Ops record), row 14 is the generic nested case. A row that differs from WARUDO's actual result fails the run.
    // Rows that name WARUDO types must be checked again after a WARUDO update; the failure output prints its version.
    static readonly (string Asm, string Name, bool Expected)[] Cases =
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
        // Generic outer and generic inner. Picked from Warudo.Core.dll (the only generic-outer nested types in
        // Warudo.Core.dll and Warudo.Plugins.Core.dll are compiler-generated closures; this one has a generic inner).
        ("Warudo.Core", "Warudo.Core.Data.FlowEntity`2+<>c__DisplayClass11_0`1", true),
        ("Warudo.Core", "Warudo.Core.Data.FlowEntity`2+DoesNotExist`1", false),
    };

    static string DefaultStreamingAssets => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        "Steam", "steamapps", "common", "Warudo", "Warudo_Data", "StreamingAssets");

    static int Main(string[] args)
    {
        string streaming = args.Length > 0 ? args[0] : DefaultStreamingAssets;
        WbbWarudoLocator.StreamingAssetsPath = streaming;
        string managed = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(streaming)), "Managed");
        string version = VersionOf(Path.Combine(managed, "Warudo.Core.dll"));
        Console.WriteLine("WARUDO managed: " + managed + " | Warudo.Core.dll version: " + version);

        WbbWarudoTypeCatalog catalog;
        try
        {
            catalog = WbbWarudoTypeCatalog.Read();
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR reading WARUDO catalog (WARUDO " + version + "): " + e.Message);
            return 2;
        }

        int failures = 0;
        Console.WriteLine("case | expected | actual");
        foreach (var c in Cases)
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

        Console.WriteLine(failures == 0
            ? "PASS " + Cases.Length + "/" + Cases.Length + " cases match (WARUDO " + version + ")"
            : "FAIL " + failures + " of " + Cases.Length + " cases differ (WARUDO " + version + ")");
        return failures == 0 ? 0 : 1;
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
