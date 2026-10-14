using System;
using System.IO;
using System.Threading;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    // Reads which render pipeline WARUDO is set to, from the URP/NiloToon plugin option.
    // Source strings: "not-found" (no WARUDO install), "default" (no option file, WARUDO's default is URP),
    // "file" (read from the option file), "unreadable" (file present but no Enabled value could be read).
    //
    // The Build tab asks on every layout pass, and the WARUDO folder setting may name a share on another PC. One folder
    // check on a share that does not answer blocks for about 20 seconds, and Windows asks again about 30 seconds later, so
    // Read never touches the disk: it returns the last finished read and starts the next one on a worker thread. Poll,
    // called from the window's Update, takes a finished read and says when the tab has to lay out again.
    public static partial class WbbWarudoPipeline
    {
        private const string OptionFileName = "Warudo.NiloToon.json";
        private const double RecheckSeconds = 2.0;

        // One finished read. Never changed after it is made, so the worker can hand it to the main thread whole.
        private sealed class Snapshot
        {
            public readonly string path;
            public readonly bool exists;
            public readonly DateTime writeTimeUtc;
            // False after a failed read. WARUDO may hold the file open while writing it, or a read may catch it half
            // written; the time stamp need not change afterwards, so a failed result is read again instead of kept.
            public readonly bool readable;
            public readonly WbbUtil.PipelineKind? kind;
            public readonly string source;
            public readonly string warning;

            public Snapshot(string path, bool exists, DateTime writeTimeUtc, bool readable, WbbUtil.PipelineKind? kind, string source, string warning)
            {
                this.path = path;
                this.exists = exists;
                this.writeTimeUtc = writeTimeUtc;
                this.readable = readable;
                this.kind = kind;
                this.source = source;
                this.warning = warning;
            }
        }

        private static readonly Snapshot NotFound = new Snapshot("", false, DateTime.MinValue, true, null, "not-found", "");

        // Main thread only.
        private static Snapshot current = NotFound;
        private static WbbWarudoLocator.AmbientInputs startedInputs;
        private static double lastStartTime = double.NegativeInfinity;
        private static string lastWarning = "";
        // A read was started and its result not taken yet. Only the main thread sets and clears it, so a read that finished
        // is never still "running" when the next one is due.
        private static bool started;
        // Read wanted a new read (the inputs changed) while one was still out.
        private static bool deferred;

        // The one thing the worker writes: its result, taken by the main thread.
        private static Snapshot finished;

        // For tools/pipeline-check: a read is out, and whether its result is ready to be taken.
        internal static bool Busy
        {
            get { return started; }
        }

        internal static bool Ready
        {
            get { return Volatile.Read(ref finished) != null; }
        }

        public static WbbUtil.PipelineKind? Read(out string source)
        {
            Take();
            WbbWarudoLocator.AmbientInputs inputs = WbbWarudoLocator.CaptureAmbient();
            bool changed = startedInputs == null || !inputs.SameAs(startedInputs);
            double now = EditorApplication.timeSinceStartup;
            if (changed || now - lastStartTime >= RecheckSeconds)
            {
                if (!started)
                    Start(inputs, now);
                else if (changed)
                    deferred = true;
            }

            source = current.source;
            return current.kind;
        }

        // Main thread, from the window's Update. Takes a finished read; true when the tab has to lay out again, because the
        // result changed or because Read wanted a new read while the last one was still out.
        public static bool Poll()
        {
            return Take();
        }

        // Reads the option now, on the calling thread, for the diagnostics report. It can block on a share that does not
        // answer, so the Build tab never calls it.
        public static WbbUtil.PipelineKind? ReadNow(out string source)
        {
            Snapshot now = Resolve(null, WbbWarudoLocator.CaptureAmbient());
            source = now.source;
            return now.kind;
        }

        private static void Start(WbbWarudoLocator.AmbientInputs inputs, double now)
        {
            startedInputs = inputs;
            lastStartTime = now;
            deferred = false;
            started = true;
            Snapshot last = current;
            ThreadPool.QueueUserWorkItem(_ => Run(last, inputs));
        }

        private static void Run(Snapshot last, WbbWarudoLocator.AmbientInputs inputs)
        {
            Snapshot next;
            try
            {
                next = Resolve(last, inputs);
            }
            catch (Exception e)
            {
                next = new Snapshot("", false, DateTime.MinValue, false, null, "unreadable", "Could not read the WARUDO pipeline option: " + e.Message);
            }

            Interlocked.Exchange(ref finished, next);
        }

        private static bool Take()
        {
            Snapshot done = Interlocked.Exchange(ref finished, null);
            if (done == null)
                return false;
            started = false;
            bool changed = done.kind != current.kind || !string.Equals(done.source, current.source, StringComparison.Ordinal);
            current = done;
            // Retried every few seconds while it fails, so the same message is logged once.
            if (done.warning.Length > 0 && !string.Equals(done.warning, lastWarning, StringComparison.Ordinal))
                WbbUtil.LogWarning(done.warning);
            lastWarning = done.warning;
            bool again = deferred;
            deferred = false;
            return changed || again;
        }

        // Runs on the worker thread (or on the caller's for ReadNow): it reads the disk and its arguments; the only shared state
        // it touches is the Steam search, which ResolveAmbient swaps in whole.
        private static Snapshot Resolve(Snapshot last, WbbWarudoLocator.AmbientInputs inputs)
        {
            // The WARUDO the builds use (the WARUDO folder setting before Steam), not only a Steam install.
            string root = WbbWarudoLocator.ResolveAmbient(inputs);
            if (string.IsNullOrEmpty(root))
                return NotFound;

            string path = Path.Combine(root, "Plugins", "Data", OptionFileName);
            bool exists = File.Exists(path);
            DateTime writeTimeUtc = DateTime.MinValue;
            string warning = "";
            try
            {
                if (exists)
                    writeTimeUtc = File.GetLastWriteTimeUtc(path);
            }
            catch (Exception e)
            {
                warning = "Could not read the WARUDO pipeline option time: " + e.Message;
            }

            if (last != null && last.readable && path == last.path && exists == last.exists && writeTimeUtc == last.writeTimeUtc)
                return last;
            if (!exists)
                return new Snapshot(path, false, writeTimeUtc, true, WbbUtil.PipelineKind.Urp, "default", warning);

            try
            {
                bool? enabled = ParseNiloToonEnabled(File.ReadAllText(path));
                if (enabled.HasValue)
                    return new Snapshot(path, true, writeTimeUtc, true, enabled.Value ? WbbUtil.PipelineKind.Urp : WbbUtil.PipelineKind.BuiltIn, "file", warning);
            }
            catch (Exception e)
            {
                warning = "Could not read the WARUDO pipeline option: " + e.Message;
            }

            return new Snapshot(path, true, writeTimeUtc, false, null, "unreadable", warning);
        }
    }
}
