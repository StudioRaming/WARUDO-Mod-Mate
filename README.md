# WARUDO Mod Mate 0.1.2

**English** | [한국어](README.ko.md) | [日本語](README.ja.md)

**A free Unity Editor helper for building WARUDO mods from selected prefabs.** Batch-build prepared prefabs, quick-build scene objects, inspect earlier results and export prefab dependencies from one window.

**[Download the Unity package directly](https://github.com/StudioRaming/WARUDO-Mod-Mate/releases/latest/download/WARUDO_Mod_Mate_0.1.2.unitypackage)** · [All releases](https://github.com/StudioRaming/WARUDO-Mod-Mate/releases) · [MIT License](LICENSE)

Import the downloaded package into Unity; no manual source compilation is required. The screenshots show the actual 0.1.0 Editor UI in Korean. The example cubes, folders and records are not included in the release package.

![Build screen](Assets/StudioRaming/WarudoBatchBuilder/Documentation/images/01-build.png)

## Contents

1. [What it does](#what-it-does)
2. [Requirements and installation](#requirements-and-installation)
3. [First build: one cube](#first-build-one-cube)
4. [Understanding Build settings](#understanding-build-settings)
5. [Resize the two areas](#resize-the-two-areas)
6. [Quick-build a scene object](#quick-build-a-scene-object)
7. [Export a Unity package](#export-a-unity-package)
8. [Library and History](#library-and-history)
9. [Troubleshooting](#troubleshooting)
10. [FAQ, updates and support](#faq-updates-and-support)

## What it does

| Your task | Feature | Result |
| --- | --- | --- |
| Build several prepared props | Select prefabs in Build and start the batch | A `.warudo` file and result record per prefab |
| Build a prepared character | Choose Character as the mod type | A character mod file |
| Test an object assembled in a scene | Hierarchy → WARUDO Mod Mate → Prop / Character | A temporary-copy build of the current object |
| Find or rebuild an earlier result | Library / History | File lookup, source lookup and rebuild preparation |
| Transfer a prefab to another project | Unity package export | A package containing the prefab and selected dependencies |

0.1.x supports **Prop and Character prefab builds** and Unity package export. Environment prefab builds are blocked; follow the [official Environment Mod workflow](https://docs.warudo.app/docs/modding/environment-mod). Library can still list existing environment files.

Live Test, avatar rigging, automatic shader conversion and physics setup are not included. Choosing Character does not configure a rig or physics system.

## Requirements and installation

Use a Windows PC and Unity **2021.3.45f2**. Building `.warudo` requires the separately installed WARUDO Mod Tool SDK and compatible content. WARUDO is needed to load and test outputs. Auto script mode builds also need WARUDO on the same PC (Steam auto-detect, the output path, or a set WARUDO folder). Unity package export works without the SDK.

The tested SDK baseline is **0.14.3.10**. Other versions should be tried in a separate project. See the [official SDK installation guide](https://docs.warudo.app/docs/modding/sdk-installation). In **Player → Other Settings**, select **.NET Framework** API Compatibility Level and disable **Assembly Version Validation**. Wait until SDK import finishes without red Console errors.

### Import Mod Mate

1. Back up an existing project before importing.
2. Use the direct download link above.
3. Choose **Assets → Import Package → Custom Package…** in Unity.
4. Select `WARUDO_Mod_Mate_0.1.2.unitypackage` and import its code, manual and images together.
5. Wait for compilation and open **StudioRaming → WARUDO Mod Mate**.
6. Confirm **v0.1.2** in the header.

Missing menu? Resolve the first red Console compilation error first.

If Auto Refresh is disabled, use **Assets → Refresh** and wait for script compilation. Replaced source files can still show the older window until compilation finishes.

### Prepare export settings

An SDK installation may not yet have mod export settings. Prepare them through **Warudo → New Mod**; see the [official first-mod tutorial](https://docs.warudo.app/docs/modding/creating-your-first-mod). Mod Mate temporarily uses these settings and restores them after a build.

## First build: one cube

Try a small prefab before a large character to separate installation issues from asset-specific issues.

1. Create a cube through **GameObject → 3D Object → Cube**.
2. Make an example Project folder and drag the cube into it to save a prefab.
3. Save your intended scene and prefab state.
4. Open **Build** and choose the prefab folder under **① Folders**.
5. Tick the cube.
6. Under **② Build settings**, choose **Warudo mod build** and **Prop**.
7. Check author, mod version and output folder. A separate test folder makes first results easy to identify.
8. Inspect the final mod name in the list; folder-name and date options can change it.
9. Resolve blockers and click the button to build the selected prefab.
10. Inspect the output folder and **History**.

A successful result is a `.warudo` file and success record. Load it in WARUDO afterward to check rendering and behavior. The official first-mod tutorial explains Props/Characters folders and Source selection.

**Tool version and mod version differ.** The header's `v0.1.2` is Mod Mate's version. `Mod version` describes your output and may be `1.0.0`.

## Understanding Build settings

### Folders and selection

Choose a folder, narrow the list with search and tick targets. List/grid views, sorting and group expansion help with larger lists. If something is missing, check folder scope, search, hidden items and index progress, then refresh. Dimmed entries are distinguished from normal working targets. Check the final selection count and output names.

| Control | What to check |
| --- | --- |
| Mod type | Prop or prepared Character; Environment prefab builds are unsupported |
| Mod version / Author | Metadata for your output mod |
| Output path | Where files are written; paths from another project may remain |
| Folder name / date options | Affect final names shown in the list |
| Overwrite same name | Update an existing file, with backup and failed-build restoration |
| Built-in / URP badge | Current Unity pipeline; match WARUDO's setting |
| Presets | Reuse settings, then recheck paths, author and selection |

### Advanced settings and scripts

![Advanced settings](Assets/StudioRaming/WarudoBatchBuilder/Documentation/images/02-advanced.png)

Advanced settings include description, manual script folders and leaving out VRChat-only components.

- Automatic runtime-script collection follows used sources and references. Editor-only code and WARUDO-provided types are excluded.
- Manual folders are an advanced source-management option. Start with a simple working prefab.
- Leaving out VRChat-only components cleans the build copy and preserves the original; it does not set up WARUDO physics.
- Arbitrary DLLs, dynamic loading and third-party plugins are not automatically made compatible.

### Preflight, progress and cancellation

Resolve red blockers before starting. Some yellow warnings allow a build after you inspect them. Expand notices or read tooltips for their reasons.

Items run sequentially. Follow the progress and logs. Cancellation may finish the current item before stopping the remaining queue; wait for the terminal state.

Save scenes and prefab edits beforehand. The SDK build can save modified scenes that already have names. Untitled scenes and unsaved Prefab editing can block preflight.

## Resize the two areas

![Height adjustment](Assets/StudioRaming/WarudoBatchBuilder/Documentation/images/08-height-splitter.png)

Hover the horizontal boundary between the upper prefab list and lower settings.

- Drag **up** for more space in the lower settings/export area.
- Drag **down** to enlarge the upper list.
- **Double-click** to restore automatic height.
- Mod-build and package-export modes remember their heights separately.

Use the wheel within a long list. In small windows, the export body also scrolls so bottom controls remain reachable. Enlarging the window gives the lower list more room.

## Quick-build a scene object

![Quick-build window](Assets/StudioRaming/WarudoBatchBuilder/Documentation/images/07-quick-build.png)

1. Select a **scene object** in Hierarchy.
2. Right-click → **WARUDO Mod Mate → Prop / Character**; these entries also appear under **GameObject**.
3. Check targets and names in the quick-build window.
4. Rename duplicates or enable numbering.
5. Version, author and output folder come from the main Build settings; open them when necessary.
6. Build and inspect outputs and History.

The current object is built through a temporary copy. Source prefabs are not permanently moved or renamed. Shaders, character settings and physics must already be prepared.

Disabled menu? Check Hierarchy selection, Play Mode and other active builds. A Project asset selection is different from a scene-object selection.

## Export a Unity package

![Package export](Assets/StudioRaming/WarudoBatchBuilder/Documentation/images/03-package.png)

`.warudo` is for WARUDO; `.unitypackage` transfers assets between Unity projects.

1. Select prefabs in the upper list.
2. Choose **.unitypackage export** in Build settings.
3. Set the output folder.
4. Inspect materials, textures, models and other dependencies below.
5. Choose optional gathering into texture/material/FBX/prefab folders, or retain original paths.
6. Check included counts and redistribution cautions.
7. Export and inspect the output folder. Multiple targets produce packages per prefab.

Selected prefabs marked **root** are mandatory. Search only changes what is visible; also check total included and target counts.

### Redistribution cautions

![Caution details](Assets/StudioRaming/WarudoBatchBuilder/Documentation/images/04-package-caution.png)

Code outside script folders, shaders and DLLs can be excluded by default. Open **Details** or use the tooltip to understand the caution.

**Select all and folder ticks do not automatically include caution files.** Include them individually only when you have redistribution rights. Mod Mate's MIT License does not license your avatars, clothing, shaders or SDKs.

## Library and History

### Library: current files

![Library](Assets/StudioRaming/WarudoBatchBuilder/Documentation/images/06-library.png)

Expand folder-path settings, configure Prop/Character/Environment folders and refresh. Filter by type or search.

Finding a file and linking it to history are separate. External, copied or moved files can be untracked; rebuilding requires a linked source. Open-folder and copy-path actions show the real location. Library deletion acts on actual files, so verify targets first.

### History: previous operations

![History](Assets/StudioRaming/WarudoBatchBuilder/Documentation/images/05-history.png)

Filter records by type, state or search, and group by date, preset or a flat list. Row actions include rebuild preparation, open output folder, copy path and view log. Prepared rebuilds start from the main Build tab; settings differences may create separate groups.

**File missing** means refresh could not locate an old output. Success history does not prove that a file still exists or that it runs correctly in WARUDO.

Failed-rebuild preparation reduces duplicates. Use CSV export to retain records. Deleting a history record and deleting a mod file are different actions.

## Troubleshooting

| Symptom | Check and action |
| --- | --- |
| No menu | Resolve the first red Console compilation error |
| SDK required | Install WARUDO Mod Tool and wait for compilation |
| Export settings missing | Warudo → New Mod and the first-mod tutorial |
| SDK .NET/Reflection errors | .NET Framework compatibility; Assembly Version Validation disabled |
| Empty list | Folder scope, search, hidden items, indexing; refresh |
| Build disabled | Selection count, blockers and active jobs |
| Environment blocked | Use the official Environment workflow; only Prop/Character prefabs are batch-built |
| Quick-build disabled | Select a Hierarchy object, leave Play Mode and wait for builds |
| Unexpected name | Inspect edited names and folder/date options |
| Lower list too small | Drag the boundary up, scroll or enlarge the window |
| Apparent stall | Check shader/script compilation and the current log; avoid starting duplicates |
| Invisible or magenta result | Match pipelines and verify shader compatibility |
| Source collection fails | Inspect source errors and external dependencies; test a smaller target |
| Recovery notice | Wait for completion, retain the message/log and inspect the reported backup location |

## FAQ, updates and support

**Rename the original prefab to Prop or Character?** No; choose the type and Mod Mate prepares a build copy.

**Combine a batch into one mod?** Normal batches create outputs per prefab. Prepare a single combined prefab for one mod.

**Does success include WARUDO testing?** No. Check loading, rendering, physics and scripts in WARUDO separately.

**Use without SDK?** Unity package export works; `.warudo` creation needs the SDK.

**Collect all scripts?** Collection follows used sources/references and excludes host-provided and Editor code. Arbitrary external plugins are not guaranteed compatible.

**Another project's path appears?** Some UI/output settings use PC-wide EditorPrefs; project-specific script folders are separate. Recheck paths before building.

**Commercial use?** MIT allows it for Mod Mate. Assets and SDKs retain their own terms.

Download updates and import into the same product folder. The internal **WarudoBatchBuilder** folder preserves settings/history compatibility; do not casually rename it. Back up the project before updates.

The header **?** menu includes the user guide, latest-package downloads and GitHub source link.

Contact **studioraming@gmail.com** or [GitHub Issues](https://github.com/StudioRaming/WARUDO-Mod-Mate/issues). Include versions, operation type, reproduction steps and the first relevant error. **Copy environment info for support** helps collect details; inspect private paths and asset information before sending.

This independent StudioRaming tool and its documentation use the [MIT License](LICENSE). It does not bundle or replace the licenses of WARUDO SDKs, shaders or avatars.
