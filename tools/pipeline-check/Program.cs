// Offline check of WbbWarudoPipeline.ParseNiloToonEnabled (the Parse half of the Mod Mate product code) and of the
// read cache in WbbWarudoPipeline.Read, which runs against the stubs in UnityStubs.cs: (g) a locked option file, (h) a
// folder check that hangs, which Read must leave to its worker thread.
//
// Run from the repo root:  dotnet run --project tools/pipeline-check [path-to-Warudo.NiloToon.json]
// Without an argument the default WARUDO install path is read in place (the file is never copied into the repo).
// A missing or unreadable WARUDO file is SKIP for case (a), not a failure. Exit code 1 on any mismatch.
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
        // WARUDO holds the file open while it saves it; that is SKIP like a missing file, so cases (b) to (g) still run.
        string text = null;
        string skip = "file not found";
        if (File.Exists(path))
        {
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                skip = "could not read the file (" + e.Message + ")";
            }
        }

        if (text != null)
            CheckReadable("(a) WARUDO file " + path, WbbWarudoPipeline.ParseNiloToonEnabled(text));
        else
            Console.WriteLine("(a) WARUDO file " + path + " | SKIP | " + skip);

        Check("(b) value false", false, WbbWarudoPipeline.ParseNiloToonEnabled("{\"dataInputs\":{\"Enabled\":{\"type\":\"bool\",\"typeKind\":\"Value\",\"defaultValue\":\"true\",\"value\":\"false\"}}}"));
        Check("(c1) Parse(null)", null, WbbWarudoPipeline.ParseNiloToonEnabled(null));
        Check("(c2) Parse(empty)", null, WbbWarudoPipeline.ParseNiloToonEnabled(""));
        Check("(d) truncated inside Enabled", null, WbbWarudoPipeline.ParseNiloToonEnabled("{\"dataInputs\":{\"Enabled\":{\"type\":\"bool\",\"typeKind\":\"Value\",\"defaultValue\":\"true\",\"val"));
        Check("(e) value after nested object", null, WbbWarudoPipeline.ParseNiloToonEnabled("{\"dataInputs\":{\"Enabled\":{\"type\":\"bool\",\"properties\":{\"x\":1},\"value\":\"false\"}}}"));
        Check("(f) other block first", true, WbbWarudoPipeline.ParseNiloToonEnabled("{\"dataInputs\":{\"Other\":{\"value\":\"false\"},\"Enabled\":{\"type\":\"bool\",\"value\":\"true\"}}}"));
        CheckLockedRead();
        CheckHungFolder();

        Console.WriteLine((anyMismatch ? "FAIL " : "PASS ") + passed + "/" + run);
        return anyMismatch ? 1 : 0;
    }

    // (g) Read() while WARUDO holds the option file open: the failed read must not stick once the file is free again,
    // although its time stamp did not change. The editor clock is a stub, so the 2 s recheck is stepped by hand, and Read
    // only starts a read on a worker thread, so ReadSettled waits for it and takes the result.
    // The fixtures live in new folders under the system temp folder and are removed afterwards.
    static void CheckLockedRead()
    {
        string root = NewFixture("true");
        string file = OptionFile(root);
        WbbWarudoLocator.Found = true;
        WbbWarudoLocator.StreamingAssetsPath = root;
        try
        {
            string source;
            WbbUtil.PipelineKind? kind;
            using (new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                UnityEditor.EditorApplication.timeSinceStartup = 100;
                kind = ReadSettled(out source);
                CheckText("(g1) Read while the file is locked", "null/unreadable", Show(kind) + "/" + source);
                UnityEditor.EditorApplication.timeSinceStartup = 103;
                ReadSettled(out source);
                CheckText("(g2) a repeated failure logs once", "1", WbbUtil.Warnings.ToString());
            }

            UnityEditor.EditorApplication.timeSinceStartup = 106;
            kind = ReadSettled(out source);
            CheckText("(g3) Read after the lock is gone, same time stamp", "URP/file", Show(kind) + "/" + source);
        }
        finally
        {
            WbbWarudoLocator.Found = false;
            Directory.Delete(root, true);
        }
    }

    // (h) The folder check hangs, as it does for a WARUDO folder setting that names a share on a PC that is off (about 21 s
    // per check, measured). The Build tab calls Read on every layout pass, so Read must return at once with the last
    // result and start no second check while one hangs; once the check answers, Poll must ask for a repaint. (h4, h5) A
    // WARUDO change while a check hangs must still be read afterwards, although the hung check's own answer changes nothing.
    static void CheckHungFolder()
    {
        string builtIn = NewFixture("false");
        string urp = NewFixture("true");
        WbbWarudoLocator.Found = true;
        try
        {
            string source;
            WbbUtil.PipelineKind? kind;
            WbbWarudoLocator.StreamingAssetsPath = builtIn;
            int checks = WbbWarudoLocator.FolderChecks;
            WbbWarudoLocator.FolderCheck.Reset();
            var watch = Stopwatch.StartNew();
            kind = WbbWarudoPipeline.Read(out source);
            WbbWarudoPipeline.Read(out source);
            watch.Stop();
            CheckText("(h1) Read returns while the folder check hangs", "under 500 ms", watch.ElapsedMilliseconds < 500 ? "under 500 ms" : watch.ElapsedMilliseconds + " ms");
            // The worker may not have run yet; wait for its check to start before counting.
            var started = Stopwatch.StartNew();
            while (WbbWarudoLocator.FolderChecks == checks && started.ElapsedMilliseconds < 5000)
                Thread.Sleep(5);
            CheckText("(h2) it keeps the last result and starts one check", "URP/file 1", Show(kind) + "/" + source + " " + (WbbWarudoLocator.FolderChecks - checks));
            WbbWarudoLocator.FolderCheck.Set();
            Settle();
            bool repaint = WbbWarudoPipeline.Poll();
            kind = WbbWarudoPipeline.Read(out source);
            bool otherThread = WbbWarudoLocator.FolderCheckThread != Environment.CurrentManagedThreadId;
            CheckText("(h3) the answer asks for a repaint, checked off this thread", "True BuiltIn/file True", repaint + " " + Show(kind) + "/" + source + " " + otherThread);

            // Recheck the Built-in folder (it hangs) and switch to the URP folder meanwhile.
            WbbWarudoLocator.FolderCheck.Reset();
            UnityEditor.EditorApplication.timeSinceStartup += 3;
            WbbWarudoPipeline.Read(out source);
            WbbWarudoLocator.StreamingAssetsPath = urp;
            WbbWarudoPipeline.Read(out source);
            WbbWarudoLocator.FolderCheck.Set();
            Settle();
            repaint = WbbWarudoPipeline.Poll();
            CheckText("(h4) a WARUDO change during a hung check asks for a repaint", "True", repaint.ToString());
            kind = ReadSettled(out source);
            CheckText("(h5) the next layout reads the new WARUDO", "URP/file", Show(kind) + "/" + source);
        }
        finally
        {
            WbbWarudoLocator.FolderCheck.Set();
            Settle();
            WbbWarudoLocator.Found = false;
            Directory.Delete(builtIn, true);
            Directory.Delete(urp, true);
        }
    }

    // Read starts the read and returns the last result; this waits for the worker and takes the new one, as the next
    // layout pass would.
    static WbbUtil.PipelineKind? ReadSettled(out string source)
    {
        WbbWarudoPipeline.Read(out source);
        Settle();
        return WbbWarudoPipeline.Read(out source);
    }

    // Waits until the read that is out has its result ready (Busy stays true until Read or Poll takes it). A local read
    // takes milliseconds; 5 s only keeps a broken worker from hanging the check.
    static void Settle()
    {
        var watch = Stopwatch.StartNew();
        while (WbbWarudoPipeline.Busy && !WbbWarudoPipeline.Ready && watch.ElapsedMilliseconds < 5000)
            Thread.Sleep(5);
    }

    static string NewFixture(string enabled)
    {
        string root = Path.Combine(Path.GetTempPath(), "wmm-pipeline-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "Plugins", "Data"));
        File.WriteAllText(OptionFile(root), "{\"dataInputs\":{\"Enabled\":{\"type\":\"bool\",\"value\":\"" + enabled + "\"}}}");
        return root;
    }

    static string OptionFile(string root)
    {
        return Path.Combine(root, "Plugins", "Data", "Warudo.NiloToon.json");
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
