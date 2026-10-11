# Unity verification of asmdef runtime builds

These four scripts check that Mod Mate builds a folder that holds an asmdef runtime assembly, and that a plain
folder still builds. They run in the Unity Editor (2021.3.45f2, SDK 0.14.3.10) through the Unity agent bridge's
`lab_exec_file` tool. They cannot run without the editor, so they are not part of the catalog check.

## Files

| File | Phase | What it does |
|---|---|---|
| `fixture/` | input | `AsmdefOk` (runtime asmdef plus editor-only asmdefs), `AsmdefBad` (runtime asmdef that uses `System.IO`/`Process`), `Plain` (no asmdef) |
| `1-write-fixtures.cs` | setup | Copies `fixture/` into `<QaRoot>/Assets/WmmAsmdefFixture`. Refuses to overwrite. Triggers a reload. |
| `2-build.cs` | build | Makes the fixture prefab once, then starts one build through `WbbBuildRunner.Start`. Returns `QUEUED`. |
| `3-audit.cs` | check | Polls until the build stops running, then prints one `PASS`/`FAIL` line per check and a `VERDICT`. Changes nothing. |
| `4-abort-by-reload.cs` | check | Waits for a running build's item to start staging, forces a script reload then (`Phase = "reload"`), then checks the cancel path (`Phase = "check"`). |

## Parameters

Every script has `PARAMETER` constants at the top. They ship empty. A script with an empty `QaRoot`, `OutDir` or
`FixtureSource` returns `ABORT ...` before it touches anything. Set them per run, in the bridge call, never in the file:

- `QaRoot`: the empty Unity 2021.3.45f2 project (its `Assets` folder is checked against `Application.dataPath`).
- `OutDir`: an existing folder for archives and receipts.
- `FixtureSource`: absolute path of this repo's `tools/unity-verify/fixture`.
- `Case` (`ok-manual`, `ok-auto`, `bad-manual`, `plain-manual`) and `RunTag` (new for every run; an existing output
  aborts the build).

## Order

1. `1-write-fixtures.cs`, then wait until the editor is ready (`lab_wait_ready`).
2. `2-build.cs` with `Case = "ok-manual"`, then poll `3-audit.cs` until it no longer says `RUNNING`.
3. Repeat step 2 for `ok-auto`, `bad-manual` and `plain-manual`, each with a new `RunTag`.
4. `2-build.cs` with `RunTag = "R1"`, then at once `4-abort-by-reload.cs` with `Phase = "reload"` (it returns `WAITING`
   and requests the recompile itself once the item reaches staging), wait for the editor, then `Phase = "check"`.

Do not edit scripts in `Assets/` during a run. A recompile during a build cancels it.

## Expected results

| Case | Build (`2-build.cs`) | Audit (`3-audit.cs`) VERDICT | Expected |
|---|---|---|---|
| `ok-manual` | asmdef runtime in a manual script folder | `PASS` | Status `Success`; archive lists entries; `assemblymodules.dat` packed; message names `Wmm.FixtureOk.Runtime` and not the editor-only asmdefs; the audit hook links the constraint and data components to `Wmm.FixtureOk.Runtime`, with `raw=0` and `missing=0` |
| `ok-auto` | same prefab, automatic script collection | `PASS` | Same as `ok-manual`. Needs WARUDO installed (the reference the automatic path uses). |
| `bad-manual` | asmdef runtime that uses `Process` / `System.IO` | `PASS` | Status `Failed`; message has at least 3 ` / `-separated parts and mentions `Process`, `System.IO` or `File`; kept build log holds the security section; no archive left behind. The per-API wording is not asserted by the script; read it by hand. |
| `plain-manual` | no asmdef; plain scripts compiled by Mod Mate | `PASS` | Status `Success`; archive lists entries; `assemblymodules.dat` packed. The history message naming `WbbRuntimeScripts` is not asserted by the script; read it by hand. |
| `4-abort-by-reload` (check) | build interrupted by a reload | `PASS` | Item status `Canceled`; record message says the build was cancelled by a recompile; a note survives the reload; no WMM temp folder; no build journal; no half-written archive |

`INCONCLUSIVE` means the reload missed the item: the build finished before it, or it stopped the batch before the item
started (the item is then `Skipped` and has no history record). Rerun with a new `RunTag`. It is not a pass.
`FAIL (n check(s))` lists each failed check on its own `FAIL` line.
