using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;

namespace StudioRaming.WarudoBatchBuilder
{
    public sealed class WbbCompiledUnit
    {
        public string name = "";
        public string asmdefPath = "";
        public string dllPath = "";
        public byte[] dll;
        public byte[] pdb;
        public string metaGuid = "";
        public string signature = "";
        public string[] sources = new string[0];
        // Why the references were not compared with WARUDO's assemblies (WARUDO not found); "" when they were.
        public string hostCheckSkipped = "";
    }

    public enum WbbCompileState
    {
        Running,
        Done,
        Failed,
    }

    // Compiles runtime units one after another with Unity's AssemblyBuilder, outside Assets so nothing
    // is imported and no domain reload follows. Finished units are kept for the whole batch, so a batch
    // of many mods that share a runtime compiles it once.
    public sealed class WbbRuntimeCompileSession : IDisposable
    {
        public const double TimeoutSeconds = 600d;
        // The same language level UMod's own script route works with; the compiled DLLs must load in WARUDO's Mono.
        private const string LanguageVersion = "7.3";
        // Roslyn reports every "feature is not available in C# <current version>" error under one code per current version
        // (CS8370 for 7.3), whatever language the compiler messages are in. Checked with the compiler of Unity 2021.3 for
        // C# 8 and C# 9 features (switch expressions, using declarations, ??=, nullable annotations, target-typed new ...).
        private const string LanguageVersionErrorCode = "CS8370";
        private static readonly Regex SnapshotPrefix = new Regex("^(\\d{4})_");
        // Unity's own assemblies of the project. A mod never references them: code outside the compiled sources has to be
        // part of the mod, so it is left out of the default references and shows up as a plain compile error.
        private static readonly string[] ProjectAssemblies = { "Assembly-CSharp", "Assembly-CSharp-firstpass", "Assembly-CSharp-Editor", "Assembly-CSharp-Editor-firstpass" };
        // Signatures of the units that need the newer language level. A signature covers the sources, so another mod of the
        // same batch with a unit of the same name is not affected.
        private readonly HashSet<string> relaxedLanguage = new HashSet<string>(StringComparer.Ordinal);
        private readonly string root;
        private readonly Dictionary<string, WbbCompiledUnit> cache = new Dictionary<string, WbbCompiledUnit>(StringComparer.Ordinal);
        private List<WbbRuntimeUnit> queue = new List<WbbRuntimeUnit>();
        private int next;
        private WbbRuntimeUnit current;
        private AssemblyBuilder builder;
        private int generation;
        private bool finished;
        private bool disposed;
        private CompilerMessage[] messages;
        private double startedAt;
        private bool warnedNoReferenceCheck;
        private bool warnedNoHostCheck;
        public WbbRuntimeCompileSession()
        {
            root = WbbBuildLog.LibraryDirectory + "/Runtime/" + Guid.NewGuid().ToString("N");
        }

        public string Root
        {
            get
            {
                return root;
            }
        }

        public static void DeleteStaleRoots()
        {
            try
            {
                string directory = WbbBuildLog.LibraryDirectory + "/Runtime";
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not clear old runtime compile output: " + e.Message);
            }
        }

        public void Begin(IList<WbbRuntimeUnit> ordered)
        {
            queue = new List<WbbRuntimeUnit>(ordered);
            next = 0;
            current = null;
            builder = null;
            finished = false;
        }

        public WbbCompiledUnit Find(string name)
        {
            WbbCompiledUnit unit;
            return cache.TryGetValue(name, out unit) ? unit : null;
        }

        public List<WbbCompiledUnit> Resolve(IList<WbbRuntimeUnit> requested)
        {
            var list = new List<WbbCompiledUnit>();
            for (int i = 0; i < requested.Count; i++)
            {
                WbbCompiledUnit unit = Find(requested[i].name);
                if (unit != null)
                    list.Add(unit);
            }

            return list;
        }

        public WbbCompileState Pump(out string failure)
        {
            failure = "";
            while (true)
            {
                if (current == null)
                {
                    if (next >= queue.Count)
                        return WbbCompileState.Done;
                    WbbRuntimeUnit candidate = queue[next];
                    string signature = Signature(candidate);
                    WbbCompiledUnit cached;
                    if (cache.TryGetValue(candidate.name, out cached) && cached.signature == signature)
                    {
                        next++;
                        continue;
                    }

                    if (!StartCompile(candidate, signature, out failure))
                        return WbbCompileState.Failed;
                    current = candidate;
                    return WbbCompileState.Running;
                }

                if (!finished)
                {
                    if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
                    {
                        failure = WbbLang.T("Asmdef.CompileTimeout", current.name, (int)TimeoutSeconds);
                        return WbbCompileState.Failed;
                    }

                    return WbbCompileState.Running;
                }

                WbbRuntimeUnit done = current;
                current = null;
                builder = null;
                if (NeedsNewerLanguage(done))
                {
                    // The sources use a newer C# than the 7.3 the compile starts with. UMod's own route accepted them, so
                    // the same code must keep building: compile once more at Unity's default language level.
                    relaxedLanguage.Add(pendingSignature);
                    WbbUtil.Log("The sources of " + done.name + " need a newer C# than " + LanguageVersion + "; compiling again at Unity's default language level.");
                    if (!StartCompile(done, pendingSignature, out failure))
                        return WbbCompileState.Failed;
                    current = done;
                    return WbbCompileState.Running;
                }

                WbbCompiledUnit compiled = Validate(done, out failure);
                if (compiled == null)
                    return WbbCompileState.Failed;
                cache[done.name] = compiled;
                next++;
            }
        }

        public void Dispose()
        {
            disposed = true;
            builder = null;
            current = null;
            try
            {
                if (Directory.Exists(root) && IsInside(WbbBuildLog.LibraryDirectory + "/Runtime", root))
                    Directory.Delete(root, true);
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not remove the runtime compile folder " + root + ": " + e.Message);
            }
        }

        // The folder of one unit below the session root, or "" when the name is not a plain folder name. The unit name comes
        // from an assembly definition the user owns and the folder is deleted recursively, so it must stay inside the root.
        private string UnitDirectory(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "." || name == ".." || name.IndexOfAny(new[] { '/', '\\', ':' }) >= 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return "";
            string directory = root + "/" + name;
            return IsInside(root, directory) ? directory : "";
        }

        private static bool IsInside(string parent, string child)
        {
            string outer = Path.GetFullPath(parent).TrimEnd('/', '\\') + Path.DirectorySeparatorChar;
            string inner = Path.GetFullPath(child).TrimEnd('/', '\\') + Path.DirectorySeparatorChar;
            return inner.Length > outer.Length && inner.StartsWith(outer, StringComparison.OrdinalIgnoreCase);
        }

        private string Signature(WbbRuntimeUnit unit)
        {
            var text = new StringBuilder();
            text.Append(unit.name).Append('|').Append(unit.plain).Append('|').Append(unit.allowUnsafe).Append('|').Append(string.Join(";", unit.defines)).Append('|').Append(string.Join(";", unit.extraReferences));
            for (int i = 0; i < unit.sources.Length; i++)
            {
                try
                {
                    var info = new FileInfo(unit.sources[i]);
                    text.Append('\n').Append(unit.sources[i]).Append('|').Append(info.Length).Append('|').Append(info.LastWriteTimeUtc.Ticks);
                }
                catch (Exception)
                {
                    text.Append('\n').Append(unit.sources[i]).Append("|missing");
                }
            }

            for (int i = 0; i < unit.projectReferences.Length; i++)
            {
                WbbCompiledUnit dependency;
                if (cache.TryGetValue(unit.projectReferences[i], out dependency))
                    text.Append("\ndep|").Append(dependency.name).Append('|').Append(dependency.signature);
            }

            using (SHA1 sha = SHA1.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", "");
        }

        private bool StartCompile(WbbRuntimeUnit unit, string signature, out string failure)
        {
            failure = "";
            if (unit.sources.Length == 0)
            {
                failure = WbbLang.T("Asmdef.CompileFailed", unit.name, "no source files");
                return false;
            }

            string directory = UnitDirectory(unit.name);
            if (directory.Length == 0)
            {
                failure = WbbLang.T("Asmdef.CompileFailed", unit.name, "the assembly name cannot be used as a folder name");
                return false;
            }

            string[] snapshots = new string[unit.sources.Length];
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
                Directory.CreateDirectory(directory + "/src");
                for (int i = 0; i < unit.sources.Length; i++)
                {
                    snapshots[i] = directory + "/src/" + i.ToString("0000") + "_" + Path.GetFileName(unit.sources[i]);
                    File.Copy(unit.sources[i], snapshots[i], true);
                }
            }
            catch (Exception e)
            {
                failure = WbbLang.T("Asmdef.CompileFailed", unit.name, e.Message);
                return false;
            }

            string[] references = ResolveReferences(unit, out failure);
            if (references == null)
                return false;
            // Whatever is added by path replaces the default reference of the same name; two copies of one identity cannot compile.
            var replaced = ReplacedNames(unit);
            for (int i = 0; i < references.Length; i++)
                replaced.Add(Path.GetFileNameWithoutExtension(references[i]));
            for (int i = 0; i < ProjectAssemblies.Length; i++)
                replaced.Add(ProjectAssemblies[i]);
            string dll = directory + "/" + unit.name + ".dll";
            var created = new AssemblyBuilder(dll, snapshots);
            created.flags = AssemblyBuilderFlags.None;
            created.referencesOptions = ReferencesOptions.UseEngineModules;
            created.buildTarget = UnityEditor.BuildTarget.StandaloneWindows64;
            created.buildTargetGroup = UnityEditor.BuildTargetGroup.Standalone;
            created.additionalReferences = references;
            created.additionalDefines = unit.defines;
            created.excludeReferences = created.defaultReferences.Where(p => replaced.Contains(Path.GetFileNameWithoutExtension(p))).ToArray();
            if (created.defaultDefines.Any(d => d == "UNITY_EDITOR" || d.StartsWith("UNITY_EDITOR_", StringComparison.Ordinal)))
            {
                failure = WbbLang.T("Asmdef.CompileFailed", unit.name, "the player compile carries editor defines");
                return false;
            }

            created.compilerOptions.AllowUnsafeCode = unit.allowUnsafe;
            created.compilerOptions.CodeOptimization = CodeOptimization.Release;
            created.compilerOptions.AdditionalCompilerArguments = relaxedLanguage.Contains(signature) ? new[] { "-debug:portable" } : new[] { "-langversion:" + LanguageVersion, "-debug:portable" };
            int mine = ++generation;
            finished = false;
            messages = null;
            created.buildFinished += (path, result) =>
            {
                if (disposed || mine != generation)
                    return;
                messages = result;
                finished = true;
            };
            if (!created.Build())
            {
                failure = WbbLang.T("Asmdef.CompileDeclined", unit.name);
                return false;
            }

            builder = created;
            startedAt = EditorApplication.timeSinceStartup;
            pendingSignature = signature;
            WbbUtil.Log("Compiling runtime assembly " + unit.name + " (" + unit.sources.Length + " sources, " + references.Length + " references).");
            return true;
        }

        private string pendingSignature = "";
        private bool NeedsNewerLanguage(WbbRuntimeUnit unit)
        {
            if (messages == null || relaxedLanguage.Contains(pendingSignature))
                return false;
            for (int i = 0; i < messages.Length; i++)
            {
                if (messages[i].type == CompilerMessageType.Error && (messages[i].message ?? "").IndexOf(LanguageVersionErrorCode, StringComparison.Ordinal) >= 0)
                    return true;
            }

            return false;
        }

        // Units that are compiled by Mod Mate in this batch: the only assemblies a unit may reference that WARUDO does not provide.
        private HashSet<string> CompiledNames(WbbRuntimeUnit unit)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            names.Add(unit.name);
            for (int i = 0; i < queue.Count; i++)
                names.Add(queue[i].name);
            foreach (string key in cache.Keys)
                names.Add(key);
            return names;
        }

        private HashSet<string> ReplacedNames(WbbRuntimeUnit unit)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            names.Add(unit.name);
            for (int i = 0; i < queue.Count; i++)
                names.Add(queue[i].name);
            foreach (string key in cache.Keys)
                names.Add(key);
            for (int i = 0; i < unit.projectReferences.Length; i++)
                names.Add(unit.projectReferences[i]);
            return names;
        }

        private string[] ResolveReferences(WbbRuntimeUnit unit, out string failure)
        {
            failure = "";
            HashSet<string> replaced = ReplacedNames(unit);
            string reason;
            Dictionary<string, string> host = WbbWarudoManaged.Read(out reason);
            var references = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < unit.compiledReferences.Length; i++)
            {
                string path = unit.compiledReferences[i];
                string name = Path.GetFileNameWithoutExtension(path ?? "");
                if (string.IsNullOrEmpty(path) || !File.Exists(path) || replaced.Contains(name) || name == "UnityEngine" || IsEditorReference(path))
                    continue;
                if (seen.Add(name))
                    references.Add(Path.GetFullPath(path));
            }

            for (int i = 0; i < unit.extraReferences.Length; i++)
            {
                string path = unit.extraReferences[i];
                string name = Path.GetFileNameWithoutExtension(path ?? "");
                if (string.IsNullOrEmpty(path) || !File.Exists(path) || replaced.Contains(name) || name == "UnityEngine")
                    continue;
                if (seen.Add(name))
                    references.Add(Path.GetFullPath(path));
            }

            for (int i = 0; i < unit.projectReferences.Length; i++)
            {
                string dependency = unit.projectReferences[i];
                string path;
                WbbCompiledUnit compiled;
                string hostPath;
                // The project's own build of an assembly first: that is what Unity compiles the same code against.
                // WARUDO's copy is the fallback; the compiled DLL is checked against WARUDO's list afterwards either way.
                path = i < unit.projectReferencePaths.Length ? unit.projectReferencePaths[i] : "";
                if (cache.TryGetValue(dependency, out compiled))
                    path = compiled.dllPath;
                else if ((string.IsNullOrEmpty(path) || !File.Exists(path)) && host != null && host.TryGetValue(dependency, out hostPath))
                    path = hostPath;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    // The plain scripts of a folder get every assembly the project hands to Assembly-CSharp; one that has no
                    // output is only a problem when the code uses it, and then the compile error says so.
                    if (unit.plain)
                        continue;
                    failure = WbbLang.T("Asmdef.ReferenceMissing", unit.name, dependency);
                    return null;
                }

                if (seen.Add(dependency))
                    references.Add(Path.GetFullPath(path));
            }

            for (int i = 0; i < unit.indirectReferenceNames.Length; i++)
            {
                string name = unit.indirectReferenceNames[i];
                string path = unit.indirectReferencePaths[i];
                WbbCompiledUnit compiled;
                if (cache.TryGetValue(name, out compiled))
                    path = compiled.dllPath;
                if (string.IsNullOrEmpty(path) || !File.Exists(path) || IsEditorReference(path))
                    continue;
                if (seen.Add(name))
                    references.Add(Path.GetFullPath(path));
            }

            return references.ToArray();
        }

        private static bool IsEditorReference(string path)
        {
            string file = Path.GetFileName(path);
            if (file.StartsWith("UnityEditor", StringComparison.Ordinal) || Path.GetFileNameWithoutExtension(file).EndsWith(".Editor", StringComparison.OrdinalIgnoreCase))
                return true;
            // Inside the project only the part below the project folder says whether a DLL is an editor plugin, so a project
            // that sits in a folder called "Editor" keeps its references. Outside it (the Unity install, "Hub/Editor/...") the
            // whole path counts: those are the editor's own copies, and the player compile already has its own set.
            string normalized = WbbUtil.NormalizePath(path);
            string data = WbbUtil.NormalizePath(UnityEngine.Application.dataPath);
            int slash = data.LastIndexOf('/');
            string project = slash > 0 ? data.Substring(0, slash + 1) : "";
            if (project.Length > 0 && normalized.StartsWith(project, StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(project.Length);
            string[] parts = normalized.Split('/');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (string.Equals(parts[i], "Editor", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private WbbCompiledUnit Validate(WbbRuntimeUnit unit, out string failure)
        {
            failure = "";
            string dll = UnitDirectory(unit.name) + "/" + unit.name + ".dll";
            CompilerMessage[] result = messages ?? new CompilerMessage[0];
            var errors = new List<string>();
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i].type == CompilerMessageType.Error)
                    errors.Add(DescribeMessage(unit, result[i]));
            }

            if (errors.Count > 0 || !File.Exists(dll))
            {
                string detail = errors.Count == 0 ? "no output was produced" : string.Join(" / ", errors.Take(3).ToArray()) + (errors.Count > 3 ? " (+" + (errors.Count - 3) + ")" : "");
                failure = WbbLang.T("Asmdef.CompileFailed", unit.name, detail);
                if (unit.plain && HasMissingNameError(result))
                    failure += " " + WbbLang.T("Asmdef.PlainCompileHint");
                return null;
            }

            string identity = System.Reflection.AssemblyName.GetAssemblyName(dll).Name;
            if (!string.Equals(identity, unit.name, StringComparison.Ordinal))
            {
                failure = WbbLang.T("Asmdef.CompileFailed", unit.name, "the DLL is named " + identity);
                return null;
            }

            byte[] bytes = File.ReadAllBytes(dll);
            string hostSkipped;
            if (!CheckReferences(unit, bytes, out hostSkipped, out failure))
                return null;
            string pdbPath = Path.ChangeExtension(dll, ".pdb");
            var compiled = new WbbCompiledUnit();
            compiled.name = unit.name;
            compiled.asmdefPath = unit.asmdefPath;
            compiled.dllPath = dll;
            compiled.dll = bytes;
            compiled.pdb = File.Exists(pdbPath) ? File.ReadAllBytes(pdbPath) : null;
            compiled.metaGuid = Guid.NewGuid().ToString("N");
            compiled.signature = pendingSignature;
            compiled.sources = unit.sources;
            compiled.hostCheckSkipped = hostSkipped;
            return compiled;
        }

        // "Type or namespace not found" and "name does not exist": what code that lives in a project script outside the
        // compiled sources looks like, because Assembly-CSharp is never a reference of a mod.
        private static bool HasMissingNameError(CompilerMessage[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i].type != CompilerMessageType.Error)
                    continue;
                string text = result[i].message ?? "";
                if (text.IndexOf("CS0246", StringComparison.Ordinal) >= 0 || text.IndexOf("CS0103", StringComparison.Ordinal) >= 0 || text.IndexOf("CS0234", StringComparison.Ordinal) >= 0)
                    return true;
            }

            return false;
        }

        // The references the compiler recorded are what the unit really uses. Unity also hands every asmdef the
        // auto-referenced plugins of the project, so the declared list would block mods for DLLs they never touch.
        private bool CheckReferences(WbbRuntimeUnit unit, byte[] image, out string skipped, out string failure)
        {
            failure = "";
            skipped = "";
            string[] referenced = WbbRuntimeUnitReferences.Read(image);
            if (referenced == null)
            {
                if (!warnedNoReferenceCheck)
                {
                    warnedNoReferenceCheck = true;
                    WbbUtil.LogWarning("The references of the compiled runtime assemblies could not be read, so they are not checked against WARUDO.");
                }

                return true;
            }

            string reason;
            Dictionary<string, string> host = WbbWarudoManaged.Read(out reason);
            HashSet<string> known = CompiledNames(unit);
            for (int i = 0; i < referenced.Length; i++)
            {
                string name = referenced[i];
                string verdict = WbbReferenceRule.Classify(name, known, host == null ? null : host.Keys);
                if (verdict == WbbReferenceRule.Editor)
                {
                    failure = WbbLang.T("Asmdef.EditorReference", unit.name, name);
                    return false;
                }

                if (verdict == WbbReferenceRule.Missing)
                {
                    failure = WbbLang.T("Asmdef.HostMissingRef", unit.name, name);
                    return false;
                }

                if (verdict == WbbReferenceRule.Unchecked)
                    skipped = string.IsNullOrEmpty(reason) ? "?" : reason;
            }

            if (skipped.Length > 0 && !warnedNoHostCheck)
            {
                warnedNoHostCheck = true;
                WbbUtil.LogWarning("WARUDO was not found, so the references of the compiled runtime assemblies are not checked against WARUDO: " + skipped);
            }

            return true;
        }

        private static string DescribeMessage(WbbRuntimeUnit unit, CompilerMessage message)
        {
            string file = Path.GetFileName(message.file ?? "");
            Match match = SnapshotPrefix.Match(file);
            if (match.Success)
            {
                int index;
                if (int.TryParse(match.Groups[1].Value, out index) && index >= 0 && index < unit.sources.Length)
                    file = ProjectRelative(unit.sources[index]);
            }

            return (file.Length == 0 ? unit.name : file) + "(" + message.line + "): " + WbbBuildLog.Shorten(message.message);
        }

        internal static string ProjectRelative(string absolute)
        {
            string normalized = WbbUtil.NormalizePath(absolute);
            string data = WbbUtil.NormalizePath(UnityEngine.Application.dataPath);
            int slash = data.LastIndexOf('/');
            string project = slash > 0 ? data.Substring(0, slash + 1) : "";
            if (project.Length > 0 && normalized.StartsWith(project, StringComparison.OrdinalIgnoreCase))
                return normalized.Substring(project.Length);
            return Path.GetFileName(normalized);
        }
    }
}
