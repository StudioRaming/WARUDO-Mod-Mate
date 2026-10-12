// Offline check of WbbWarudoPipeline.ParseNiloToonEnabled (the Parse half of the Mod Mate product code) and of the
// read cache in WbbWarudoPipeline.Read, which runs against the stubs in UnityStubs.cs.
//
// Run from the repo root:  dotnet run --project tools/pipeline-check [path-to-Warudo.NiloToon.json]
// Without an argument the default WARUDO install path is read in place (the file is never copied into the repo).
// A missing WARUDO file is SKIP for case (a), not a failure. Exit code 1 on any mismatch.
using System;
using System.IO;
using StudioRaming.WarudoBatchBuilder;

static class Program
{
    const string DefaultPath = "C:/Program Files (x86)/Steam/steamapps/common/Warudo/Warudo_Data/StreamingAssets/Plugins/Data/Warudo.NiloToon.json";

    static int run;
    static int passed;
    static bool anyMismatch;

    static int Main(string[] args)
    {
        Console.WriteLine("case | expected | actual");

        string path = args.Length > 0 ? args[0] : DefaultPath;
        // The real file holds whichever pipeline WARUDO is set to right now, so (a) only requires a readable value.
        if (File.Exists(path))
            CheckReadable("(a) WARUDO file " + path, WbbWarudoPipeline.ParseNiloToonEnabled(File.ReadAllText(path)));
        else
            Console.WriteLine("(a) WARUDO file " + path + " | SKIP | file not found");

        Check("(b) value false", false, WbbWarudoPipeline.ParseNiloToonEnabled("{\"dataInputs\":{\"Enabled\":{\"type\":\"bool\",\"typeKind\":\"Value\",\"defaultValue\":\"true\",\"value\":\"false\"}}}"));
        Check("(c1) Parse(null)", null, WbbWarudoPipeline.ParseNiloToonEnabled(null));
        Check("(c2) Parse(empty)", null, WbbWarudoPipeline.ParseNiloToonEnabled(""));
        Check("(d) truncated inside Enabled", null, WbbWarudoPipeline.ParseNiloToonEnabled("{\"dataInputs\":{\"Enabled\":{\"type\":\"bool\",\"typeKind\":\"Value\",\"defaultValue\":\"true\",\"val"));
        Check("(e) value after nested object", null, WbbWarudoPipeline.ParseNiloToonEnabled("{\"dataInputs\":{\"Enabled\":{\"type\":\"bool\",\"properties\":{\"x\":1},\"value\":\"false\"}}}"));
        Check("(f) other block first", true, WbbWarudoPipeline.ParseNiloToonEnabled("{\"dataInputs\":{\"Other\":{\"value\":\"false\"},\"Enabled\":{\"type\":\"bool\",\"value\":\"true\"}}}"));
        CheckLockedRead();

        Console.WriteLine((anyMismatch ? "FAIL " : "PASS ") + passed + "/" + run);
        return anyMismatch ? 1 : 0;
    }

    // (g) Read() while WARUDO holds the option file open: the failed read must not stick once the file is free again,
    // although its time stamp did not change. The editor clock is a stub, so the 2 s recheck is stepped by hand.
    // The fixture lives in a new folder under the system temp folder and is removed afterwards.
    static void CheckLockedRead()
    {
        string root = Path.Combine(Path.GetTempPath(), "wmm-pipeline-check-" + Guid.NewGuid().ToString("N"));
        string data = Path.Combine(root, "Plugins", "Data");
        Directory.CreateDirectory(data);
        string file = Path.Combine(data, "Warudo.NiloToon.json");
        File.WriteAllText(file, "{\"dataInputs\":{\"Enabled\":{\"type\":\"bool\",\"value\":\"true\"}}}");
        WbbWarudoLocator.Found = true;
        WbbWarudoLocator.StreamingAssetsPath = root;
        try
        {
            string source;
            WbbUtil.PipelineKind? kind;
            using (new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                UnityEditor.EditorApplication.timeSinceStartup = 100;
                kind = WbbWarudoPipeline.Read(out source);
                CheckText("(g1) Read while the file is locked", "null/unreadable", Show(kind) + "/" + source);
                UnityEditor.EditorApplication.timeSinceStartup = 103;
                WbbWarudoPipeline.Read(out source);
                CheckText("(g2) a repeated failure logs once", "1", WbbUtil.Warnings.ToString());
            }

            UnityEditor.EditorApplication.timeSinceStartup = 106;
            kind = WbbWarudoPipeline.Read(out source);
            CheckText("(g3) Read after the lock is gone, same time stamp", "URP/file", Show(kind) + "/" + source);
        }
        finally
        {
            WbbWarudoLocator.Found = false;
            Directory.Delete(root, true);
        }
    }

    static string Show(WbbUtil.PipelineKind? kind)
    {
        return kind.HasValue ? (kind.Value == WbbUtil.PipelineKind.Urp ? "URP" : kind.Value.ToString()) : "null";
    }

    static void CheckText(string name, string expected, string actual)
    {
        run++;
        bool ok = expected == actual;
        if (ok)
            passed++;
        else
            anyMismatch = true;
        Console.WriteLine(name + " | " + expected + " | " + actual + (ok ? "" : "  <-- MISMATCH"));
    }

    static void Check(string name, bool? expected, bool? actual)
    {
        run++;
        bool ok = expected == actual;
        if (ok)
            passed++;
        else
            anyMismatch = true;
        Console.WriteLine(name + " | " + Show(expected) + " | " + Show(actual) + (ok ? "" : "  <-- MISMATCH"));
    }

    static void CheckReadable(string name, bool? actual)
    {
        run++;
        if (actual.HasValue)
            passed++;
        else
            anyMismatch = true;
        Console.WriteLine(name + " | true or false | " + Show(actual) + (actual.HasValue ? " (" + (actual.Value ? "URP" : "Built-in") + ")" : "  <-- MISMATCH"));
    }

    static string Show(bool? value)
    {
        return value.HasValue ? (value.Value ? "true" : "false") : "null";
    }
}
