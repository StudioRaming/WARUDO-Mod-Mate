# Unity verification of asmdef runtime builds

These scripts check that Mod Mate builds a folder that holds an asmdef runtime assembly, that a plain folder still
builds, and that a build over a file of the same name (the Build tab with Overwrite on, and every Quick Build) keeps or
restores the right file. They run in the Unity Editor (2021.3.45f2, SDK 0.14.3.10) through the Unity agent bridge's
`lab_exec_file` tool. They cannot run without the editor, so they are not part of the catalog check.

## Files

| File | Phase | What it does |
|---|---|---|
| `fixture/` | input | `AsmdefOk` (runtime asmdef plus editor-only asmdefs), `AsmdefBad` (runtime asmdef that uses `System.IO`/`Process`), `Plain` (no asmdef) |
| `1-write-fixtures.cs` | setup | Copies `fixture/` into `<QaRoot>/Assets/WmmAsmdefFixture`. Refuses to overwrite. Triggers a reload. |
| `2-build.cs` | build | Makes the fixture prefab once, then starts one build through `WbbBuildRunner.Start`. Returns `QUEUED`. |
| `3-audit.cs` | check | Polls until the build stops running, then prints one `PASS`/`FAIL` line per check and a `VERDICT`. Changes nothing. |
| `4-abort-by-reload.cs` | check | Waits for a running build's item to start staging, forces a script reload then (`Phase = "reload"`), then checks the cancel path (`Phase = "check"`). |
| `5-output-guard.cs` | check | No build. Calls the same-name overwrite guard (`WbbJournal.GuardOutput`, then `CommitOutput` or `RevertOutput`) on small files under `OutDir` for five cases. Takes a few seconds. |
| `6-quick-build.cs` | build + check | Two Quick Builds of one scene object under the same name (`Phase = "build"` / `"audit"`, `Pass = 1` then `2`), then `Phase = "restore"`. |

## Parameters

Every script has `PARAMETER` constants at the top. They ship empty. A script with an empty `QaRoot`, `OutDir` or
`FixtureSource` returns `ABORT ...` before it touches anything. `lab_exec_file` runs a file and takes no arguments, so
set them in a copy of the script outside the repository for each run. Never commit them (they hold local paths):

- `QaRoot`: the empty Unity 2021.3.45f2 project (its `Assets` folder is checked against `Application.dataPath`).
- `OutDir`: an existing folder for archives and receipts.
- `FixtureSource`: absolute path of this repo's `tools/unity-verify/fixture`.
- `Case` (`ok-manual`, `ok-auto`, `bad-manual`, `plain-manual`) and `RunTag` (new for every run; an existing output
  aborts the build).
- `Overwrite` (`2-build.cs`, default `false`): build over the output an earlier run of the same name left, the path
  the Quick Build always takes. `NameOf` (`2-build.cs` and `3-audit.cs`, default empty): build under another case's
  name, so `bad-manual` can fail over the `ok-manual` output. Give `3-audit.cs` the same `NameOf`.
- `Phase` and `Pass` (`6-quick-build.cs`).

## Order

1. `1-write-fixtures.cs`, then wait until the editor is ready (`lab_wait_ready`).
2. `2-build.cs` with `Case = "ok-manual"`, then poll `3-audit.cs` until it no longer says `RUNNING`.
3. Repeat step 2 for `ok-auto`, `bad-manual` and `plain-manual`, each with a new `RunTag`.
4. `2-build.cs` with `RunTag = "R1"`, then at once `4-abort-by-reload.cs` with `Phase = "reload"` (it returns `WAITING`
   and requests the recompile itself once the item reaches staging), wait for the editor, then `Phase = "check"`.
5. `5-output-guard.cs` with a new `RunTag`.
6. Same-name overwrite, each followed by `3-audit.cs` with the same parameters until it no longer says `RUNNING`:
   1. `2-build.cs` with `Case = "ok-manual"`, `RunTag = "OW"`.
   2. The same with `Overwrite = true`.
   3. `Case = "bad-manual"`, `NameOf = "ok-manual"`, `RunTag = "OW"`, `Overwrite = true`.
7. `6-quick-build.cs`: `Phase = "build"`, `Pass = 1`; `Phase = "audit"`, `Pass = 1`; the same with `Pass = 2`; then
   `Phase = "restore"`. Keep one `RunTag` for the four calls.

`5-output-guard.cs` lets `RevertOutput` prune `Library/StudioRaming/WarudoBatchBuilder/Failed` to its newest entries,
and `6-quick-build.cs` points the saved Prop output folder at `OutDir` for the length of each Start call: run both in
the QA project only.

Do not edit scripts in `Assets/` during a run. A recompile during a build cancels it.

## Expected results

| Case | Build (`2-build.cs`) | Audit (`3-audit.cs`) VERDICT | Expected |
|---|---|---|---|
| `ok-manual` | asmdef runtime in a manual script folder | `PASS` | Status `Success`; archive lists entries; `assemblymodules.dat` packed; message names `Wmm.FixtureOk.Runtime` and not the editor-only asmdefs; the audit hook links the constraint and data components to `Wmm.FixtureOk.Runtime`, with `raw=0` and `missing=0` |
| `ok-auto` | same prefab, automatic script collection | `PASS` | Same as `ok-manual`. Needs WARUDO installed (the reference the automatic path uses). |
| `bad-manual` | asmdef runtime that uses `Process` / `System.IO` | `PASS` | Status `Failed`; message has at least 3 ` / `-separated parts and mentions `Process`, `System.IO` or `File`; kept build log holds the security section; no archive left behind. The per-API wording is not asserted by the script; read it by hand. |
| `plain-manual` | no asmdef; plain scripts compiled by Mod Mate | `PASS` | Status `Success`; archive lists entries; `assemblymodules.dat` packed. The history message naming `WbbRuntimeScripts` is not asserted by the script; read it by hand. |
| `4-abort-by-reload` (check) | build interrupted by a reload | `PASS` | Item status `Canceled`; record message says the build was cancelled by a recompile; a note survives the reload; no WMM temp folder; no build journal; no half-written archive |
| `5-output-guard` | none | `PASS` | a) failed build that wrote B over A: the path holds A, the newest `Failed/` entry holds B, no backup of the run left in `OutputBackups`, message `Runner.FailedOutputRestored`; b) failed build that deleted the path: A is back; c) successful build: the path holds B and the backup is gone; d) failed first build: the path is gone and `Failed/` holds B, message `Runner.FailedOutputMoved`; e) failed build that wrote nothing: A untouched, `Failed/` unchanged, no message. The run's files end up in `OutDir/wmmguard_<RunTag>/leftovers`. `RevertOutput` deletes all but the newest three `Failed/` entries, so what `Failed/` held before the run waits in `Library/StudioRaming/WarudoBatchBuilder/FailedHeld_<RunTag>` and is moved back at the end (last check: `Failed/` holds what it held at the start). |
| `ok-manual` `OW` | first build of the name | `PASS` | As `ok-manual`. |
| `ok-manual` `OW` `Overwrite` | over the first build | `PASS` | As `ok-manual`, and: the archive was written by this build, no backup of this name left in `OutputBackups`, no outputs in the build journal. |
| `bad-manual` `NameOf = ok-manual` `OW` `Overwrite` | failing build over the second one | `PASS` | As `bad-manual`, except that the archive of the second build is back (same SHA-256); no backup left, no outputs in the journal. The security check fails before UMod writes the archive, so `Failed/` usually gains nothing (an `INFO` line gives the count; `5-output-guard` checks the `Failed/` paths). |
| `6-quick-build` | two Quick Builds of one name | `PASS` (both audits) | Both passes: no build journal file left. Pass 1: success, archive in `OutDir`. Pass 2: success, the archive written by this build, no backup of this name left, no outputs in the journal, no dirty scene, no `_WBB_Temp` folder, no quick-iteration reload notice (`OutDir` is not the `Props` folder of the WARUDO install). `restore` prints the output folder pref as it was at the start. |

`3-audit.cs` judges only what this build left: the newest history record of the name that started after the build
did (the receipt's `build.startTicks`), and the fixture audit lines written after it (`audit.startLength`). A run that
never started its item fails `history record exists` even when an earlier run of the name left one.

`INCONCLUSIVE` means the reload missed the item: the build finished before it, or it stopped the batch before the item
started (the item is then `Skipped` and has no history record). Rerun with a new `RunTag`. It is not a pass.
For `bad-manual`, INCONCLUSIVE means the build succeeded: the SDK's code security check did not refuse the fixture.
A rerun gives the same result; read the security values in ModToolsSettings first.
`FAIL (n check(s))` lists each failed check on its own `FAIL` line.

No before snapshot gives INCONCLUSIVE; a profile that cannot be read after the build gives FAIL.
A UMod hook field that cannot be found (handler count `-1`) also gives INCONCLUSIVE.
