# Releasing WARUDO Mod Mate

Run the steps in this order. Each step needs the output of the one before it.

## 1. Type catalog check

Unity is not needed. The check compiles `WbbWarudoTypeCatalog.cs` against the WARUDO Managed DLLs and compares 15
expected results. It exits 1 on any mismatch and prints the `Warudo.Core.dll` identity (assembly version, size and time
stamp) so a failure can be tied to a WARUDO update.

Pass the two paths as arguments. Nothing is stored in the repository:

```
dotnet run --project tools/catalog-check -p:WarudoSdkRoslynDir="<modtool SDK>/Packages/UMod-AddOns/UMod-RoslynCSharp/Plugin" -- "<WARUDO>/Warudo_Data/StreamingAssets"
```

- `<modtool SDK>` is the `app.warudo.modtool@<hash>` package in a Unity project's `Library/PackageCache`.
- `<WARUDO>` is the WARUDO install folder. Without the argument the check uses the Steam default path.
- Exit 0 is required. Exit 2 means WARUDO could not be read; exit 1 means at least one expected result differs.

## 2. Unity verification

Run the four cases in `tools/unity-verify/README.md` in an empty Unity 2021.3.45f2 project with SDK 0.14.3.10.
Record the `VERDICT` line of each case. Every case must match its expected row in that README. INCONCLUSIVE is not a pass.

## 3. Package

```
python tools/build_package.py --output dist/WARUDO_Mod_Mate_<version>.unitypackage
```

The script packs only `Assets/StudioRaming/WarudoBatchBuilder` (and its parent `.meta`). It does not include `tools/`,
`RELEASING.md` or any file outside `Assets/StudioRaming/`. Check that `dist/` stays out of git (it is ignored).

## 4. Record

Record the SHA-256 of the `.unitypackage` (the `sha256` field of the `.manifest.json` written next to it) and the commit
hash the package was built from. Set `SOURCE_DATE_EPOCH` for a reproducible build.

## Privacy check before publishing

No personal paths, account names or local drive folders may appear in tracked files. The marker patterns are kept
outside the repository (in the release checklist), so this file does not repeat them. Run the case-insensitive search
with those patterns over the whole tree:

```
git grep -n -i -E "<markers>"
```

It must print nothing. Check both forward-slash and backslash spellings of each path marker.
