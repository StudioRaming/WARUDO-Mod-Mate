// Offline check of WbbWarudoPipeline.ParseNiloToonEnabled (the Parse half of the Mod Mate product code).
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

        Console.WriteLine((anyMismatch ? "FAIL " : "PASS ") + passed + "/" + run);
        return anyMismatch ? 1 : 0;
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
