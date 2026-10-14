# Changelog

## Unreleased

Fixes

- A build journal whose settings could not be restored is kept. After an interrupted build, the recovery record used to be deleted even when writing the original ExportSettings profile back failed, so the next build took Mod Mate's temporary values for the originals. The record now stays with only the settings fields and a `recoveryAttempts` count; after 3 failed attempts it is set aside as `BuildJournal.json.unrecovered-<time>`. While such a record is pending, a new build does not start and preflight blocks it. A record that can never be written back (its profile no longer exists) can be moved aside with [Set record aside] in the check results list; nothing is deleted. A set-aside record keeps blocking builds until you press [Checked] after checking ExportSettings, instead of only showing a toast. Restoring ExportSettings from a backup now also retries writing the pending record back. A leftover journal that recovery has already handled no longer blocks a build.
- Overwriting a package file is atomic. The export is written to a temporary file next to the target and swapped in only when it is complete, so a failed or interrupted export leaves the previous `.unitypackage` unchanged. The Package tab also warns when some assets were skipped.
- Automatic collection no longer fails on C# 8 and 9 syntax (such as `??=` or switch expressions) that Unity 2021.3 compiles. The analysis parses at C# 9, and its "feature not available" (CS8370, CS8773) and preview (CS8652) errors are left to the compile, which falls back to Unity's default language level. Other analysis errors name the project file and line.
- The output folder is saved once and read by both the Build tab and the Mod list tab (Library). The Build tab used to write its in-memory folder back whenever it saved a setting or the window closed, so a folder changed in the Mod list tab went back to the old one. It now writes the folder only when the Build tab changed it. Both tabs read the saved folder again on tab entry and window focus, the Build tab only while no build is running. The Package tab folder is separate and unchanged.
- Rebuild no longer replaces the saved Build tab settings. The record's type, version, author and output folder, and the mod output mode, are used for that rebuild only and are not saved; a note with a "Use my settings" button shows them on the Build tab, and the saved settings come back when the build ends, when the selection changes or when the button is pressed. A hidden prefab that a rebuild shows again is still shown again, and the status line says how many.
- Rebuilding several records while a build is running shows one "build is running" notice instead of one per record, and the rebuild buttons and menu items of the History and Mod list tabs are disabled during a build (the buttons say why in their tooltip).
- Clear Selection clears the whole selection in the Mod list, History and Build list menu, including items a search or filter hides. It used to unselect only the visible ones and left Delete selected enabled. Select All still takes only what is visible, and its tooltip says so. The History delete confirm now says how many of the selected records are hidden, like the Mod list one does.
- The Build tab keeps a selection made in another folder when the folder tree changes scope. The kept prefabs are not listed but are named in the build confirm and built; the list count line says how many come from other folders.
- The History tab re-checks which output files still exist when it is entered or the window is focused (at most every 3 seconds), so a file moved to the Recycle Bin shows as missing. Entering the Mod list tab refreshes it too.
- The Quick Build button says why it is disabled while another build runs (tooltip and a status line) and turns on by itself when that build ends.
- The History rebuild confirm says how many selected records are left out because their type, version, author or output folder differ from the largest group.
- Automatic script mode no longer fails every item when Steam does not list WARUDO. Mod Mate now takes the WARUDO folder from the output path (when it is inside Warudo_Data/StreamingAssets), then from the new per-project "Set WARUDO folder" setting, then from Steam. Without any of them, preflight blocks the batch with one message and a [Set WARUDO folder] button instead of failing each item. Manual script folders and assembly definitions show a warning that the check against WARUDO's DLLs is skipped.
- The Package tab's list of redistribution-risk files you chose to include again is no longer taken over from 0.1.0. 0.1.0 stored it PC-wide, and since 0.1.1 every project that had no list of its own copied it, so a paid shader or DLL re-included in one project started out included in every other project that imported the same package. This is an exception to the per-project migration described under 0.1.1: the list is now chosen in each project. A project opened with 0.1.1 or 0.1.2 may still hold the copied list; check the rows with the warning chip, or reset it with Clear exclusions in the Package tab (it also clears the re-included risk files).
- The runtime reference check no longer lets `System.*` and `Mono.*` DLLs through unchecked: a runtime assembly that references one WARUDO does not ship (for example `System.Text.Json` or `Mono.Cecil`) now fails the build instead of losing its scripts when WARUDO loads the mod. When WARUDO is not found the references cannot be compared; the build goes on and its history record says so.
- A package export fails with a message when an included file changes while it is being packaged, and no package is left behind. A file that grew used to be cut at its old size and a file that shrank was padded with zero bytes, while the export still reported success.
- Removing a work folder no longer also removes a renamed work folder whose old path now holds the folder being removed.
- The magnifier icon in search fields follows an Editor Theme switch and a move to a monitor with another display scale. It kept the first theme's icon, which is hard to see after switching from Dark to Light.
- Script reloads no longer leave the window's card textures behind in memory when only Quick Build or the folder browser is open.
- When the build journal cannot be written (for example while another program holds the file), the item fails before it overwrites an existing mod file or writes the build profile, and the referenced mods list and console options are left as they are. Before, a Unity crash right after such a failed write made the next start delete the finished backup of the old mod file, and could leave the build profile, the referenced mods list or the console options changed.

Changes

- The Build tab shows WARUDO's render pipeline next to the project badge, read from the URP/NiloToon Enabled option in WARUDO's plugin data. A mismatch between the two shows a warning line and warning badges; the build is not blocked. "Copy environment info for support" in the ? menu includes the WARUDO pipeline as well.

Documentation

- The manual's runtime code section says which rules apply to the default automatic collection and which only to a manual script folder. It told every user to move scripts "into the script folder", which does not exist in the default mode.
- Six button and status names in the manual now match the window: Build Warudo mod, Export .unitypackage and Missing (English), Warudo MODをビルド and .unitypackage を書き出す (Japanese), 실패만 재빌드 (Korean).

## 0.1.2 (2026-10-11)

Fixes

- Types that WARUDO provides through an assembly type-forward are accepted. WARUDO's System.dll forwards ``Stack`1`` and ``Queue`1`` to mscorlib, so a mod whose runtime used them was rejected with "WARUDO does not provide this DLL-only type". A forward whose target does not resolve is still refused.
- Nested types are checked by their full metadata name (Outer+Inner) against WARUDO. An Inner type that the installed WARUDO lacks is now rejected even when Outer exists. `Stack<T>.Enumerator` and `Queue<T>.Enumerator` (foreach over Stack or Queue) are no longer rejected.

## 0.1.1 (2026-10-09)

Mod runtime code

- Mod Mate now compiles the mod's runtime code itself and hands the DLLs to UMod, instead of letting UMod compile loose scripts. Each assembly definition (.asmdef) becomes one DLL, and the plain scripts of a manual script folder (subfolders included) become one more DLL, `WbbRuntimeScripts`. The compile uses Unity's AssemblyBuilder with the engine module references (`UseEngineModules`), the project's own references for everything the folder uses, and C# 7.3 first; sources that need a newer language level (the compiler's "feature not available in C# 7.3" error, CS8370) are compiled again at Unity's default level, as UMod's route accepted them. That fallback is remembered per unit contents, so another mod of the same batch with an assembly of the same name is compiled on its own.
- Why: UMod compiles loose scripts against the monolithic UnityEngine.dll, while Warudo.Core, Warudo.Plugins.Core and Unity.InputSystem are built against the module assemblies, so any member typed with a Unity type that reaches the mod through them failed with CS0012 (Transform, Animator, Vector2 ...). Adding module references to UMod's compile is not possible (CS0433), so the compile has to happen here.
- The DLLs are written outside Assets (nothing is imported, Unity does not reload), registered with UMod for the duration of one build, and removed afterwards. Every .cs source is left out of UMod's own compile: in a manual script folder all scripts below the folder (including the sources of editor-only, test or define-filtered assemblies that are not compiled), in automatic mode all scripts that come in as dependencies of the prefab. The unit compile folders are named after the assembly; a name that is not a plain folder name is refused, and a compile folder is only deleted when it is inside Mod Mate's own Library folder. UMod's code security check runs on the DLLs like on any other script. Prefab MonoBehaviours link to the DLL by type name.
- Only assemblies Unity compiles for the player count. Editor-only and test assemblies, scripts in Editor folders and assemblies whose define constraints are not met stay out; preflight lists skipped assembly definitions.
- Code in a plain folder (no .asmdef) that uses a project script outside the mod folder (Assembly-CSharp) is a compile error, as the assembly it comes from never reaches WARUDO. UMod's own route used to accept it and the mod then failed in WARUDO. A "type or name not found" error of the plain scripts now says that the script may live outside the folder and has to be moved into it.
- Automatic collection does not compile the assembly definitions of Unity packages (registry, git, built-in) into the mod: an assembly the mod depends on that is neither in the project's own code nor provided by WARUDO is reported as a missing reference. Embedded and local packages count as the project's own code.
- Automatic collection: types from assembly definitions are no longer flattened into the sources handed to the automatic-source adapter. The SDK package 0.14.3 does not ship that adapter, so every automatic build used to fail with "The UMod automatic-source adapter is unavailable". Without the adapter the collected sources are compiled by Mod Mate as `WbbRuntimeScripts` like above, and a prefab without custom scripts builds without any script step. With an SDK that has the adapter, a prefab that needs only plain project scripts still uses it, as before.
- A mod no longer needs the project's generated .csproj for its scripts: UMod only reads it for the loose scripts it compiles itself, and none are left. If UMod does still skip scripts because the project file is missing ("Failed to locate script project file"), the build is now reported as failed instead of finishing without scripts.

Preflight and messages

- The old block on every script folder that holds an .asmdef is gone. Preflight now blocks a runtime assembly that references an assembly WARUDO does not provide and that is not part of the folder, a declared precompiled DLL WARUDO lacks, and an assembly whose name collides with one of WARUDO's own. After compiling, the real references of each DLL are checked against WARUDO's assemblies and against editor-only or project-only assemblies, and a compile error names the source file (relative to the project) and line.
- A missing build settings file or a settings file without a mod now offers a "Create settings" button on the Build tab. It runs the SDK's own LoadOrCreate and CreateNewExportProfile calls, the same ones its Export Settings window uses. The message names that action instead of only pointing at Warudo > New Mod.
- A code security rejection shows the rejected APIs from the UMod report in the item message (first 12, with a count) and adds the full list to the kept build log. The kept UMod build log is copied right after the build and copied again when the item is verified and when it ends, if it grew in between, so the end of the log is no longer cut off and the window is not blocked while UMod's log writer catches up.
- A build that Unity stops from outside (script recompile, entering Play Mode) says so: the history record names the reason, a note stays on the Build tab across the reload, and the log says whether the build settings were restored.

Fixes

- Per-project state: Build, Library, History and Package tab settings (prefab folder, output folders, mod author and version, package targets, recent folders) are stored per Unity project instead of one PC-wide set. Values written by older versions are copied once into each project that reads a setting it has no value for yet; the old shared values are never changed or deleted, so opening another project first cannot take them away from the original one, and clearing a list (for example the recent folders) stays cleared. A folder that does not exist in a project is dropped there as before. The shared UI language is unchanged.
- The quick-build crash-recovery marker is project-scoped and is never taken over from another project.
- An exception inside the Build, Library or History tab no longer stops the whole window frame: the tab shows the error with a Retry button and the details go to the Console.
- Failed build outputs kept in Library/StudioRaming/WarudoBatchBuilder/Failed: the newest 3 are kept, older ones are deleted. Only entries inside that Mod Mate folder are touched.
- The history export suggests `WarudoModMate_History_<date>.csv`; the Library status legend says "Mod Mate build record" instead of "WBB".
- `WbbUtil.ComposeModName` no longer appends the date twice when the date option is on and overwrite is off (the function has no caller in this version; the shipped naming path already added it once).
- Not changed: the template package GUID concern (D13) does not apply to this source: the package exporter writes each asset's existing GUID and .meta from the project, it never creates GUIDs.

## 0.1.0

- First public source and Unity package release under the MIT License.
- Product name: WARUDO Mod Mate.
- Batch builds, scene quick builds, build history and prefab package export.
- GitHub source and latest-package links in the Help menu.
- Resizable package asset list with scrolling and small-window overflow handling.
- Compact redistribution caution summary with expandable details.
- Korean, English and Japanese interface and manual.
- The experimental Live Test workflow is excluded from this release.
- Corrected the initially uploaded product-version label from 1.0.0 to 0.1.0.
- Added a draggable divider between prefab selection and the lower settings/export area, with separate saved heights and double-click reset.
- Added an illustrated user handbook in Korean, English and Japanese, with eight actual Unity captures.
- Preserved import-safe file timestamps in Unity packages; SOURCE_DATE_EPOCH is available for reproducible builds.
