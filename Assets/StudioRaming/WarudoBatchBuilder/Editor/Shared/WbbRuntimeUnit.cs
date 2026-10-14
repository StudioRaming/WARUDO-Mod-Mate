using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using PlayerAssembly = UnityEditor.Compilation.Assembly;

namespace StudioRaming.WarudoBatchBuilder
{
    // One assembly that Mod Mate compiles into its own DLL and hands to UMod, instead of letting UMod compile loose scripts.
    public sealed class WbbRuntimeUnit
    {
        // The assembly that holds every plain (assembly-definition-free) runtime script of a mod.
        public const string PlainName = "WbbRuntimeScripts";
        public string name = "";
        public string asmdefPath = "";
        public bool plain;
        public bool allowUnsafe;
        public string[] sources = new string[0];
        public string[] defines = new string[0];
        public string[] compiledReferences = new string[0];
        public string[] extraReferences = new string[0];
        public string[] projectReferences = new string[0];
        public string[] projectReferencePaths = new string[0];
        // Every assembly definition below the direct references. A compile needs them for the types its references expose.
        public string[] indirectReferenceNames = new string[0];
        public string[] indirectReferencePaths = new string[0];
    }

    public sealed class WbbAsmdefIssue
    {
        public bool blocking;
        public string code = "";
        public string message = "";
        public string detail = "";
        public WbbAsmdefIssue(bool blocking, string code, string message, string detail)
        {
            this.blocking = blocking;
            this.code = code;
            this.message = message;
            this.detail = detail ?? "";
        }
    }

    // Maps the assemblies that WARUDO ships (its Managed folder) by simple name.
    internal static class WbbWarudoManaged
    {
        private const double CacheSeconds = 20d;
        private static Dictionary<string, string> cached;
        private static string cachedReason = "";
        private static string cachedRoot = "";
        private static double cachedAt = double.MinValue;
        // For code without an output path: the running batch's WARUDO folder, otherwise the setting or Steam.
        public static Dictionary<string, string> Read(out string reason)
        {
            return Read(WbbWarudoLocator.AmbientStreamingAssets, out reason);
        }

        // `streaming` is the Warudo_Data/StreamingAssets folder WbbWarudoLocator.ResolveForOutput decided on.
        public static Dictionary<string, string> Read(string streaming, out string reason)
        {
            streaming = streaming ?? "";
            double now = EditorApplication.timeSinceStartup;
            if (cached != null && string.Equals(cachedRoot, streaming, StringComparison.OrdinalIgnoreCase) && now - cachedAt < CacheSeconds && now >= cachedAt)
            {
                reason = cachedReason;
                return cached;
            }

            reason = "";
            Dictionary<string, string> found = null;
            try
            {
                if (string.IsNullOrEmpty(streaming))
                {
                    reason = WbbLang.T("Safety.WarudoManagedMissing");
                }
                else
                {
                    string directory = Path.Combine(Path.GetDirectoryName(streaming), "Managed");
                    if (!Directory.Exists(directory))
                    {
                        reason = WbbLang.T("Safety.WarudoManagedFolder", directory);
                    }
                    else
                    {
                        found = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (string file in Directory.GetFiles(directory, "*.dll"))
                            found[Path.GetFileNameWithoutExtension(file)] = WbbUtil.NormalizePath(file);
                    }
                }
            }
            catch (Exception e)
            {
                reason = e.Message;
                found = null;
            }

            cached = found;
            cachedReason = reason;
            cachedRoot = streaming;
            cachedAt = now;
            return found;
        }

        public static void Invalidate()
        {
            cached = null;
            cachedAt = double.MinValue;
        }

        public static bool IsFramework(string name)
        {
            return name == "mscorlib" || name == "netstandard" || name == "System" || name.StartsWith("System.", StringComparison.Ordinal) || name == "Microsoft.CSharp" || name.StartsWith("Mono.", StringComparison.Ordinal);
        }
    }

    // Decides which assembly definitions of a mod are runtime code that Mod Mate compiles itself,
    // and what stands in their way. Only assemblies that Unity itself compiles for the player count,
    // so editor-only assemblies, test assemblies and unmet define constraints drop out on their own.
    public sealed class WbbAsmdefAnalysis
    {
        [Serializable]
        private sealed class AsmdefJson
        {
            public string name = "";
            public bool overrideReferences;
            public string[] precompiledReferences = new string[0];
        }

        public readonly List<WbbRuntimeUnit> units = new List<WbbRuntimeUnit>();
        public readonly List<string> skipped = new List<string>();
        public readonly List<WbbAsmdefIssue> issues = new List<WbbAsmdefIssue>();
        public bool HasBlocking
        {
            get
            {
                for (int i = 0; i < issues.Count; i++)
                {
                    if (issues[i].blocking)
                        return true;
                }

                return false;
            }
        }

        public string FirstBlockingMessage
        {
            get
            {
                for (int i = 0; i < issues.Count; i++)
                {
                    if (issues[i].blocking)
                        return issues[i].message;
                }

                return "";
            }
        }

        // Manual script folder: the folder is the whole mod, so every dependency has to be in it or in WARUDO.
        // Assemblies defined in the folder become units of their own; the plain scripts of the folder become one more unit
        // that is compiled last, so it can use the types of the others.
        public static WbbAsmdefAnalysis ForFolder(WbbScriptFolderScan scan, string warudoStreamingAssets = null)
        {
            var names = new List<string>();
            if (scan != null)
                names.AddRange(scan.asmdefAssemblies.Keys);
            WbbAsmdefAnalysis result = names.Count > 0 ? Analyze(names, false, warudoStreamingAssets) : new WbbAsmdefAnalysis();
            if (scan == null || scan.plainScriptAssets.Count == 0)
                return result;
            string reason;
            WbbRuntimeUnit plain = BuildPlainUnit(scan.plainScriptAssets, out reason);
            if (plain == null)
                result.issues.Add(new WbbAsmdefIssue(true, WbbPreflightCodes.PlainScriptsUnresolved, WbbLang.T("Asmdef.PlainUnresolved"), reason));
            else
                result.units.Add(plain);
            return result;
        }

        // The plain scripts of a folder belong to Unity's predefined assemblies (Assembly-CSharp and its siblings).
        // Their references are the references of those assemblies, so the folder compiles against the same
        // assembly definitions and plugins as it does in the project. Editor-only references never come along.
        private static WbbRuntimeUnit BuildPlainUnit(IList<string> scriptAssets, out string reason)
        {
            reason = "";
            var players = PlayerAssemblies();
            var owners = new SortedSet<string>(StringComparer.Ordinal);
            var sources = new List<string>();
            for (int i = 0; i < scriptAssets.Count; i++)
            {
                string owner = "";
                try
                {
                    owner = CompilationPipeline.GetAssemblyNameFromScriptPath(scriptAssets[i]) ?? "";
                }
                catch (Exception)
                {
                }

                if (owner.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    owner = owner.Substring(0, owner.Length - 4);
                if (owner.Length == 0 || !players.ContainsKey(owner))
                {
                    reason = WbbLang.T("Asmdef.PlainUnresolved.hint", scriptAssets[i]);
                    return null;
                }

                owners.Add(owner);
                sources.Add(Path.GetFullPath(WbbUtil.ToAbsolutePath(scriptAssets[i])));
            }

            var unit = new WbbRuntimeUnit();
            unit.name = WbbRuntimeUnit.PlainName;
            unit.plain = true;
            unit.sources = sources.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
            var defines = new HashSet<string>(StringComparer.Ordinal);
            var compiled = new List<string>();
            var projectNames = new List<string>();
            var projectPaths = new List<string>();
            var indirectNames = new List<string>();
            var indirectPaths = new List<string>();
            foreach (string owner in owners)
            {
                WbbRuntimeUnit part = BuildUnit(players[owner]);
                defines.UnionWith(part.defines);
                compiled.AddRange(part.compiledReferences);
                unit.allowUnsafe |= part.allowUnsafe;
                for (int i = 0; i < part.projectReferences.Length; i++)
                {
                    if (owners.Contains(part.projectReferences[i]) || projectNames.Contains(part.projectReferences[i]))
                        continue;
                    projectNames.Add(part.projectReferences[i]);
                    projectPaths.Add(part.projectReferencePaths[i]);
                }

                for (int i = 0; i < part.indirectReferenceNames.Length; i++)
                {
                    if (owners.Contains(part.indirectReferenceNames[i]) || indirectNames.Contains(part.indirectReferenceNames[i]))
                        continue;
                    indirectNames.Add(part.indirectReferenceNames[i]);
                    indirectPaths.Add(part.indirectReferencePaths[i]);
                }
            }

            unit.defines = defines.OrderBy(d => d, StringComparer.Ordinal).ToArray();
            unit.compiledReferences = compiled.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            unit.projectReferences = projectNames.ToArray();
            unit.projectReferencePaths = projectPaths.ToArray();
            unit.indirectReferenceNames = indirectNames.ToArray();
            unit.indirectReferencePaths = indirectPaths.ToArray();
            return unit;
        }

        // Automatic mode: the prefab names the assemblies, their asmdef dependencies come along.
        public static WbbAsmdefAnalysis ForAssemblies(IEnumerable<string> names, string warudoStreamingAssets = null)
        {
            return Analyze(names, true, warudoStreamingAssets);
        }

        public static Dictionary<string, PlayerAssembly> PlayerAssemblies()
        {
            var map = new Dictionary<string, PlayerAssembly>(StringComparer.Ordinal);
            PlayerAssembly[] all = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
            for (int i = 0; i < all.Length; i++)
                map[all[i].name] = all[i];
            return map;
        }

        public static bool IsAsmdefAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return false;
            try
            {
                return !string.IsNullOrEmpty(CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static WbbRuntimeUnit BuildUnit(PlayerAssembly assembly)
        {
            var unit = new WbbRuntimeUnit();
            unit.name = assembly.name;
            unit.asmdefPath = WbbUtil.NormalizePath(CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name));
            unit.sources = assembly.sourceFiles.Select(p => Path.GetFullPath(p)).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
            unit.defines = RuntimeDefines(assembly.defines);
            unit.projectReferences = assembly.assemblyReferences.Select(r => r.name).ToArray();
            unit.projectReferencePaths = assembly.assemblyReferences.Select(r => r.outputPath ?? "").ToArray();
            var compiled = new List<string>(assembly.compiledAssemblyReferences ?? new string[0]);
            var indirectNames = new List<string>();
            var indirectPaths = new List<string>();
            var visited = new HashSet<string>(StringComparer.Ordinal) { assembly.name };
            var pending = new Queue<PlayerAssembly>(assembly.assemblyReferences);
            while (pending.Count > 0)
            {
                PlayerAssembly dependency = pending.Dequeue();
                if (!visited.Add(dependency.name))
                    continue;
                compiled.AddRange(dependency.compiledAssemblyReferences ?? new string[0]);
                if (!unit.projectReferences.Contains(dependency.name))
                {
                    indirectNames.Add(dependency.name);
                    indirectPaths.Add(dependency.outputPath ?? "");
                }

                foreach (PlayerAssembly below in dependency.assemblyReferences)
                    pending.Enqueue(below);
            }

            unit.compiledReferences = compiled.ToArray();
            unit.indirectReferenceNames = indirectNames.ToArray();
            unit.indirectReferencePaths = indirectPaths.ToArray();
            unit.allowUnsafe = assembly.compilerOptions != null && assembly.compilerOptions.AllowUnsafeCode;
            return unit;
        }

        public static string[] RuntimeDefines(IEnumerable<string> defines)
        {
            if (defines == null)
                return new string[0];
            return defines.Where(d => !string.IsNullOrEmpty(d) && d != "UNITY_EDITOR" && !d.StartsWith("UNITY_EDITOR_", StringComparison.Ordinal)).Distinct(StringComparer.Ordinal).ToArray();
        }

        // `warudoStreamingAssets` null: the root of the running batch (see WbbWarudoManaged.Read).
        private static WbbAsmdefAnalysis Analyze(IEnumerable<string> requested, bool expand, string warudoStreamingAssets)
        {
            var result = new WbbAsmdefAnalysis();
            var players = PlayerAssemblies();
            string hostReason;
            Dictionary<string, string> host = warudoStreamingAssets == null ? WbbWarudoManaged.Read(out hostReason) : WbbWarudoManaged.Read(warudoStreamingAssets, out hostReason);
            var chosen = new Dictionary<string, PlayerAssembly>(StringComparer.Ordinal);
            var queue = new Queue<string>(requested.Distinct(StringComparer.Ordinal).OrderBy(n => n, StringComparer.Ordinal));
            while (queue.Count > 0)
            {
                string name = queue.Dequeue();
                if (chosen.ContainsKey(name) || result.skipped.Contains(name))
                    continue;
                if (host != null && host.ContainsKey(name))
                {
                    result.issues.Add(new WbbAsmdefIssue(true, WbbPreflightCodes.AsmdefNameCollidesHost, WbbLang.T("Asmdef.NameCollidesHost", name), WbbLang.T("Asmdef.NameCollidesHost.hint")));
                    continue;
                }

                PlayerAssembly assembly;
                if (!players.TryGetValue(name, out assembly))
                {
                    // An assembly in an Editor folder is the usual editor tooling, not something to mention.
                    if (!InEditorFolder(CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(name)))
                        result.skipped.Add(name);
                    continue;
                }

                chosen[name] = assembly;
                if (!expand)
                    continue;
                foreach (PlayerAssembly dependency in assembly.assemblyReferences)
                {
                    if (host != null && host.ContainsKey(dependency.name))
                        continue;
                    // A package assembly that WARUDO lacks is not mod code: it stays a missing reference below instead of being
                    // compiled from the package source into the mod.
                    if (!chosen.ContainsKey(dependency.name) && !InForeignPackage(dependency.name))
                        queue.Enqueue(dependency.name);
                }
            }

            if (result.skipped.Count > 0)
            {
                result.skipped.Sort(StringComparer.Ordinal);
                result.issues.Add(new WbbAsmdefIssue(false, WbbPreflightCodes.AsmdefNotCompiled, WbbLang.T("Asmdef.NotCompiled", result.skipped.Count), WbbLang.T("Asmdef.NotCompiled.hint", result.skipped[0])));
            }

            int unverified = 0;
            foreach (KeyValuePair<string, PlayerAssembly> pair in chosen)
            {
                WbbRuntimeUnit unit = BuildUnit(pair.Value);
                List<string> precompiled = DeclaredPrecompiled(unit);
                if (host == null)
                {
                    if (unit.projectReferences.Length > 0 || precompiled.Count > 0)
                        unverified++;
                }
                else
                {
                    for (int i = 0; i < unit.projectReferences.Length; i++)
                    {
                        string dependency = unit.projectReferences[i];
                        if (!chosen.ContainsKey(dependency) && !host.ContainsKey(dependency))
                            result.issues.Add(new WbbAsmdefIssue(true, WbbPreflightCodes.AsmdefReference, WbbLang.T("Asmdef.ReferenceMissing", unit.name, dependency), WbbLang.T("Asmdef.ReferenceMissing.hint", unit.name, dependency)));
                    }

                    for (int i = 0; i < precompiled.Count; i++)
                    {
                        if (!host.ContainsKey(precompiled[i]))
                            result.issues.Add(new WbbAsmdefIssue(true, WbbPreflightCodes.AsmdefPrecompiled, WbbLang.T("Asmdef.PrecompiledMissing", unit.name, precompiled[i]), WbbLang.T("Asmdef.PrecompiledMissing.hint")));
                    }
                }

                result.units.Add(unit);
            }

            if (unverified > 0)
                result.issues.Add(new WbbAsmdefIssue(false, WbbPreflightCodes.AsmdefHostUnknown, WbbLang.T("Asmdef.HostUnknown", unverified), WbbLang.T("Asmdef.HostUnknown.hint")));
            result.OrderByDependency();
            return result;
        }

        // An assembly of a Unity package that is not part of the project's own code (registry, git, built-in or tarball).
        // Embedded packages and local ones are the project's own sources.
        private static bool InForeignPackage(string assemblyName)
        {
            try
            {
                string path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName);
                if (string.IsNullOrEmpty(path))
                    return false;
                UnityEditor.PackageManager.PackageInfo package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(WbbUtil.NormalizePath(path));
                return package != null && package.source != UnityEditor.PackageManager.PackageSource.Embedded && package.source != UnityEditor.PackageManager.PackageSource.Local;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool InEditorFolder(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
            string[] parts = WbbUtil.NormalizePath(assetPath).Split('/');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (string.Equals(parts[i], "Editor", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static List<string> DeclaredPrecompiled(WbbRuntimeUnit unit)
        {
            var names = new List<string>();
            if (string.IsNullOrEmpty(unit.asmdefPath))
                return names;
            try
            {
                string absolute = WbbUtil.ToAbsolutePath(unit.asmdefPath);
                if (!File.Exists(absolute))
                    return names;
                AsmdefJson json = JsonUtility.FromJson<AsmdefJson>(File.ReadAllText(absolute));
                if (json == null || !json.overrideReferences || json.precompiledReferences == null)
                    return names;
                for (int i = 0; i < json.precompiledReferences.Length; i++)
                {
                    string file = json.precompiledReferences[i];
                    if (string.IsNullOrEmpty(file))
                        continue;
                    names.Add(file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? file.Substring(0, file.Length - 4) : file);
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not read " + unit.asmdefPath + ": " + e.Message);
            }

            return names;
        }

        // Kahn's algorithm over the references that stay inside the chosen set; ties break by name.
        private void OrderByDependency()
        {
            var byName = new Dictionary<string, WbbRuntimeUnit>(StringComparer.Ordinal);
            for (int i = 0; i < units.Count; i++)
                byName[units[i].name] = units[i];
            var remaining = new SortedSet<string>(byName.Keys, StringComparer.Ordinal);
            var ordered = new List<WbbRuntimeUnit>();
            while (remaining.Count > 0)
            {
                string next = null;
                foreach (string candidate in remaining)
                {
                    bool ready = true;
                    string[] dependencies = byName[candidate].projectReferences;
                    for (int k = 0; k < dependencies.Length; k++)
                    {
                        if (remaining.Contains(dependencies[k]) && dependencies[k] != candidate)
                        {
                            ready = false;
                            break;
                        }
                    }

                    if (ready)
                    {
                        next = candidate;
                        break;
                    }
                }

                if (next == null)
                    next = remaining.Min;
                remaining.Remove(next);
                ordered.Add(byName[next]);
            }

            units.Clear();
            units.AddRange(ordered);
        }
    }
}
