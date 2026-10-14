using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    // Ownership of the Build tab settings that a rebuild borrows (type, version, author, output folder, output mode) and
    // of the output folder preference that the Library tab shares.
    public partial class WbbBuildSection
    {
        // The settings the user had before a rebuild borrowed the record's values. While this is not null, job holds the
        // record's values for the run, SavePrefs and the edit sites do not persist them, and the settings card is locked.
        private sealed class RebuildBorrow
        {
            public int exportType;
            public string modVersion;
            public string modAuthor;
            public int outputMode;
        }

        private RebuildBorrow rebuildBorrow;
        private int rebuildUnhiddenCount;
        // The output folder value (and its type) the Build tab last read from or wrote to the shared preference.
        private int seenOutputType = -1;
        private string seenOutputDir = "";

        public bool RebuildBorrowActive
        {
            get
            {
                return rebuildBorrow != null;
            }
        }

        private void NoteOutputDirSeen()
        {
            seenOutputType = (int)CurrentType;
            seenOutputDir = job.outputDirectory ?? "";
        }

        // Only a change made in the Build tab is written back; a value that is merely held in memory never overwrites what the
        // Library tab stored in the meantime.
        private void PersistOutputDirIfChanged()
        {
            if (rebuildBorrow != null || string.IsNullOrEmpty(job.outputDirectory))
                return;
            if (seenOutputType == (int)CurrentType && string.Equals(seenOutputDir, job.outputDirectory, System.StringComparison.Ordinal))
                return;
            WbbPrefs.SetString(OutputDirKey(CurrentType), job.outputDirectory);
            NoteOutputDirSeen();
        }

        // Tab entry and window focus: pick up a folder the Library tab changed. Never while a build runs (the run keeps its
        // own output path) and never while a rebuild holds the record's folder.
        public void ReloadOutputDirFromPref()
        {
            if (IsBusy || rebuildBorrow != null)
                return;
            string stored = WbbUtil.StoredOutputPath(OutputDirKey(CurrentType), CurrentType);
            if (string.Equals(stored, job.outputDirectory, System.StringComparison.Ordinal))
            {
                NoteOutputDirSeen();
                return;
            }

            job.outputDirectory = stored;
            NoteOutputDirSeen();
            namesDirty = true;
            preflightDirty = true;
            RequestRepaint();
        }

        private void BorrowForRebuild(int type, string version, string author, string outputDir)
        {
            if (rebuildBorrow == null)
            {
                rebuildBorrow = new RebuildBorrow
                {
                    exportType = job.exportType,
                    modVersion = job.modVersion,
                    modAuthor = job.modAuthor,
                    outputMode = job.outputMode
                };
            }

            job.exportType = type;
            job.modVersion = version;
            job.modAuthor = author;
            job.outputDirectory = outputDir;
            job.outputMode = (int)WbbOutputMode.WarudoMod;
        }

        // Back to the saved settings: the run is over, the selection changed, the user asked, or another settings change
        // (preset, type, automation) takes over.
        private void DiscardRebuildBorrow()
        {
            if (rebuildBorrow == null)
                return;
            RebuildBorrow saved = rebuildBorrow;
            rebuildBorrow = null;
            rebuildUnhiddenCount = 0;
            job.exportType = Mathf.Clamp(saved.exportType, 0, TypeCount - 1);
            job.modVersion = saved.modVersion;
            job.modAuthor = saved.modAuthor;
            job.outputMode = saved.outputMode;
            job.outputDirectory = WbbUtil.StoredOutputPath(OutputDirKey(CurrentType), CurrentType);
            NoteOutputDirSeen();
            ClearRebuildNameOverrides();
            namesDirty = true;
            preflightDirty = true;
            runContent = null;
            runContentCount = -1;
            RequestRepaint();
        }

        private void DrawRebuildBorrowNote()
        {
            if (rebuildBorrow == null)
                return;
            EditorGUILayout.BeginHorizontal();
            WbbStyles.Callout(L("Build.RebuildBorrowNote", WbbLang.TypeName(CurrentType), string.IsNullOrEmpty(job.modVersion) ? "1.0.0" : job.modVersion, string.IsNullOrEmpty(job.modAuthor) ? L("Build.NoAuthorUnset") : job.modAuthor, job.outputDirectory ?? ""), WbbTone.Info);
            if (WbbStyles.MiniButton(LC("Build.RebuildBorrowRestore"), GUILayout.ExpandWidth(false)))
                pendingRebuildRestore = true;
            EditorGUILayout.EndHorizontal();
        }
    }
}
