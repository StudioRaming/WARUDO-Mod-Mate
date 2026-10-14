#if WBB_WARUDO_SDK
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using PlayerAssembly = UnityEditor.Compilation.Assembly;

namespace StudioRaming.WarudoBatchBuilder
{
    public sealed class WbbRuntimeScriptPlan
    {
        public string[] sources = new string[0];
        public string[] compilerTexts = new string[0];
        public string[] references = new string[0];
        public string[] defines = new string[0];
        // Assemblies defined by an .asmdef that the prefab needs. Their sources are not flattened into `sources`;
        // Mod Mate compiles each of them into its own DLL (see WbbRuntimeUnitCompiler).
        public string[] asmdefAssemblies = new string[0];
        public readonly HashSet<string> providedTypes = new HashSet<string>(StringComparer.Ordinal);
        private static WbbWarudoTypeCatalog catalog;
        private static string catalogRoot = "";
        // The catalog is read once per batch: the runner calls this at the start of a batch, and Collect reads it again only
        // when the WARUDO folder it is asked about is another one.
        public static void ResetCatalog()
        {
            catalog = null;
            catalogRoot = "";
        }

        private static WbbWarudoTypeCatalog CatalogFor(string warudoStreamingAssets)
        {
            if (catalog == null || !string.Equals(catalogRoot, warudoStreamingAssets ?? "", StringComparison.OrdinalIgnoreCase))
            {
                catalog = WbbWarudoTypeCatalog.Read(warudoStreamingAssets);
                catalogRoot = warudoStreamingAssets ?? "";
            }

            return catalog;
        }

        public static WbbRuntimeScriptPlan Collect(string prefabPath)
        {
            return Collect(prefabPath, WbbWarudoLocator.AmbientStreamingAssets);
        }

        // `warudoStreamingAssets`: the folder WbbWarudoLocator.ResolveForOutput chose for this build's output path.
        public static WbbRuntimeScriptPlan Collect(string prefabPath, string warudoStreamingAssets)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Build prefab is missing: " + prefabPath);
            var roots = new List<Type>();
            var visited = new HashSet<UnityEngine.Object>();
            foreach (var behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null || WbbBuildFixups.IsEditorOnlyType(behaviour.GetType()))
                    continue;
                CollectSerializedTypes(behaviour, roots, visited);
            }

            foreach (string dependency in AssetDatabase.GetDependencies(prefabPath, true))
            {
                if (!dependency.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    continue;
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(dependency);
                var type = script == null ? null : script.GetClass();
                if (type != null && !WbbBuildFixups.IsEditorOnlyType(type))
                    roots.Add(type);
            }

            return CollectTypes(roots, warudoStreamingAssets);
        }

        private static void CollectSerializedTypes(UnityEngine.Object target, List<Type> roots, HashSet<UnityEngine.Object> visited)
        {
            if (target == null || !visited.Add(target) || WbbBuildFixups.IsEditorOnlyType(target.GetType()))
                return;
            roots.Add(target.GetType());
            using (var serialized = new SerializedObject(target))
            {
                var property = serialized.GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        var value = property.managedReferenceValue;
                        if (value != null)
                            roots.Add(value.GetType());
                        else if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
                            throw new InvalidOperationException("Serialized runtime type is unavailable: " + property.managedReferenceFullTypename);
                    }
                    else if (property.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var referenced = property.objectReferenceValue as ScriptableObject;
                        if (referenced != null)
                            CollectSerializedTypes(referenced, roots, visited);
                    }
                }
            }
        }

        public static WbbRuntimeScriptPlan CollectTypes(IEnumerable<Type> roots)
        {
            return CollectTypes(roots, WbbWarudoLocator.AmbientStreamingAssets);
        }

        public static WbbRuntimeScriptPlan CollectTypes(IEnumerable<Type> roots, string warudoStreamingAssets)
        {
            var collector = new Collector(CatalogFor(warudoStreamingAssets));
            var queue = new Queue<Type>(roots);
            var visited = new HashSet<Type>();
            while (queue.Count > 0)
            {
                var type = queue.Dequeue();
                if (type == null || !visited.Add(type) || type.IsGenericParameter)
                    continue;
                if (type.HasElementType)
                {
                    queue.Enqueue(type.GetElementType());
                    continue;
                }

                if (type.IsGenericType)
                {
                    foreach (var argument in type.GetGenericArguments())
                        queue.Enqueue(argument);
                    type = type.GetGenericTypeDefinition();
                }

                collector.Require(type.Assembly.GetName().Name, type.FullName);
            }

            return collector.Finish();
        }

        // The SDK's automatic-source adapter. The SDK package 0.14.3 does not ship it; without it Mod Mate compiles the
        // collected sources itself (see WbbRuntimeUnitCompiler), so a missing adapter is no longer a failed build.
        public static bool AdapterAvailable
        {
            get
            {
                var adapter = Type.GetType("UModEngineModuleScriptProcessor, Assembly-CSharp-Editor");
                return adapter != null && adapter.GetMethod("BeginAutoScripts") != null;
            }
        }

        public IDisposable BeginBuild(string modFolder)
        {
            var adapter = Type.GetType("UModEngineModuleScriptProcessor, Assembly-CSharp-Editor");
            var begin = adapter == null ? null : adapter.GetMethod("BeginAutoScripts");
            if (begin == null)
                throw new InvalidOperationException("The UMod automatic-source adapter is unavailable.");
            return (IDisposable)begin.Invoke(null, new object[] { modFolder, sources, compilerTexts, references, defines });
        }

        private sealed class Collector
        {
            private readonly WbbRuntimeScriptPlan plan = new WbbRuntimeScriptPlan();
            private readonly WbbWarudoTypeCatalog host;
            private readonly Dictionary<string, PlayerAssembly> assemblies;
            private readonly Dictionary<string, CSharpCompilation> compilations = new Dictionary<string, CSharpCompilation>();
            private readonly Dictionary<string, MetadataReference> metadata = new Dictionary<string, MetadataReference>(StringComparer.OrdinalIgnoreCase);
            private readonly HashSet<string> needed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly Queue<SyntaxTree> queue = new Queue<SyntaxTree>();
            private readonly Dictionary<SyntaxTree, CSharpCompilation> owners = new Dictionary<SyntaxTree, CSharpCompilation>();
            private readonly HashSet<string> usedAssemblies = new HashSet<string>(StringComparer.Ordinal);
            private readonly HashSet<string> requiredTypes = new HashSet<string>(StringComparer.Ordinal);
            private readonly HashSet<string> nativeAssemblies = new HashSet<string>(StringComparer.Ordinal);
            private readonly HashSet<string> asmdefAssemblies = new HashSet<string>(StringComparer.Ordinal);
            private readonly Dictionary<string, bool> asmdefKnown = new Dictionary<string, bool>(StringComparer.Ordinal);
            public Collector(WbbWarudoTypeCatalog host)
            {
                this.host = host;
                assemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies).ToDictionary(a => a.name, StringComparer.Ordinal);
            }

            public void Require(string assembly, string name)
            {
                if (string.IsNullOrEmpty(name) || !requiredTypes.Add(assembly + ":" + name))
                    return;
                string provided;
                if (host.Contains(assembly, name, out provided))
                {
                    plan.providedTypes.Add(assembly + ":" + name);
                    nativeAssemblies.Add(assembly);
                    return;
                }

                PlayerAssembly info;
                if (!assemblies.TryGetValue(assembly, out info))
                {
                    throw new InvalidOperationException("WARUDO does not provide this DLL-only type: " + assembly + ":" + name);
                }

                if (IsAsmdef(assembly))
                {
                    asmdefAssemblies.Add(assembly);
                    return;
                }

                var compilation = Compilation(info);
                var type = compilation.GetTypeByMetadataName(name);
                if (type == null || type.DeclaringSyntaxReferences.Length == 0)
                    throw new InvalidOperationException("Cannot locate runtime source for " + assembly + ":" + name);
                RequireSymbol(type);
            }

            private bool IsAsmdef(string assembly)
            {
                bool known;
                if (!asmdefKnown.TryGetValue(assembly, out known))
                    asmdefKnown[assembly] = known = WbbAsmdefAnalysis.IsAsmdefAssembly(assembly);
                return known;
            }

            private void RequireSymbol(ITypeSymbol type)
            {
                if (type == null || type.TypeKind == TypeKind.Error || type.TypeKind == TypeKind.TypeParameter)
                    return;
                var array = type as IArrayTypeSymbol;
                if (array != null)
                {
                    RequireSymbol(array.ElementType);
                    return;
                }

                var pointer = type as IPointerTypeSymbol;
                if (pointer != null)
                {
                    RequireSymbol(pointer.PointedAtType);
                    return;
                }

                var named = type as INamedTypeSymbol;
                if (named == null || named.IsAnonymousType)
                    return;
                foreach (var argument in named.TypeArguments)
                    RequireSymbol(argument);
                named = named.OriginalDefinition;
                // The nested name decides what WARUDO provides; the outermost type is only the unit of source enqueue.
                string nestedName = MetadataName(named);
                while (named.ContainingType != null)
                    named = named.ContainingType;
                string assembly = named.ContainingAssembly.Identity.Name;
                string name = MetadataName(named);
                string provided;
                if (host.Contains(assembly, nestedName, out provided))
                {
                    plan.providedTypes.Add(assembly + ":" + nestedName);
                    nativeAssemblies.Add(assembly);
                    return;
                }

                if (named.DeclaringSyntaxReferences.Length == 0)
                {
                    Require(assembly, nestedName);
                    return;
                }

                if (assemblies.ContainsKey(assembly) && IsAsmdef(assembly))
                {
                    asmdefAssemblies.Add(assembly);
                    return;
                }

                usedAssemblies.Add(assembly);
                foreach (var syntax in named.DeclaringSyntaxReferences)
                {
                    var tree = syntax.SyntaxTree;
                    if (needed.Add(tree.FilePath))
                        queue.Enqueue(tree);
                }
            }

            private CSharpCompilation Compilation(PlayerAssembly info)
            {
                CSharpCompilation found;
                if (compilations.TryGetValue(info.name, out found))
                    return found;
                // Unity 2021.3's default language level, the one the compile falls back to. The 7.3 limit stays with the compile.
                var options = new CSharpParseOptions(LanguageVersion.CSharp9, preprocessorSymbols: RuntimeDefines(info.defines));
                var trees = info.sourceFiles.Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p), options, Path.GetFullPath(p))).ToArray();
                var paths = info.compiledAssemblyReferences;
                var refs = new List<MetadataReference>();
                foreach (var path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!File.Exists(path))
                        throw new FileNotFoundException("A runtime assembly reference is missing.", path);
                    MetadataReference reference;
                    string full = Path.GetFullPath(path);
                    if (!metadata.TryGetValue(full, out reference))
                        metadata[full] = reference = MetadataReference.CreateFromFile(full);
                    refs.Add(reference);
                }

                foreach (var dependency in info.assemblyReferences)
                {
                    if (string.IsNullOrEmpty(host.AssemblyPath(dependency.name)))
                        refs.Add(Compilation(dependency).ToMetadataReference());
                    else
                    {
                        string full = Path.GetFullPath(dependency.outputPath);
                        MetadataReference reference;
                        if (!metadata.TryGetValue(full, out reference))
                            metadata[full] = reference = MetadataReference.CreateFromFile(full);
                        refs.Add(reference);
                    }
                }

                found = CSharpCompilation.Create(info.name, trees, refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
                compilations.Add(info.name, found);
                foreach (var tree in trees)
                    owners.Add(tree, found);
                return found;
            }

            public WbbRuntimeScriptPlan Finish()
            {
                while (queue.Count > 0)
                {
                    var tree = queue.Dequeue();
                    var model = owners[tree].GetSemanticModel(tree);
                    foreach (var error in model.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error && !IsLanguageLevelError(d)))
                        throw new InvalidOperationException("Cannot analyse runtime source: " + DescribeError(error));
                    foreach (var node in tree.GetRoot().DescendantNodes())
                    {
                        if (node is BaseTypeDeclarationSyntax || node is DelegateDeclarationSyntax)
                        {
                            var declared = model.GetDeclaredSymbol(node) as INamedTypeSymbol;
                            if (declared != null)
                            {
                                string path;
                                if (host.Contains(declared.ContainingAssembly.Identity.Name, MetadataName(declared), out path))
                                    throw new InvalidOperationException("A source file mixes custom and WARUDO-provided types: " + tree.FilePath);
                                RequireSymbol(declared);
                            }
                        }

                        if (!(node is ExpressionSyntax) && !(node is AttributeSyntax) && !(node is TypeSyntax))
                            continue;
                        RequireSymbol(model.GetTypeInfo(node).Type);
                        var symbol = model.GetSymbolInfo(node).Symbol;
                        if (symbol is ITypeSymbol)
                            RequireSymbol((ITypeSymbol)symbol);
                        else if (symbol != null)
                            RequireSymbol(symbol.ContainingType);
                    }
                }

                plan.sources = needed.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
                var trees = owners.Keys.ToDictionary(t => t.FilePath, StringComparer.OrdinalIgnoreCase);
                plan.compilerTexts = plan.sources.Select(p => RuntimeText(trees[p])).ToArray();
                var refs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var defines = new HashSet<string>(StringComparer.Ordinal);
                foreach (var name in usedAssemblies)
                {
                    var info = assemblies[name];
                    defines.UnionWith(RuntimeDefines(info.defines));
                }

                foreach (var name in nativeAssemblies)
                {
                    if (name == "mscorlib" || name == "netstandard" || name == "System" || name.StartsWith("System.", StringComparison.Ordinal) || name == "UnityEngine" || name.StartsWith("UnityEngine.", StringComparison.Ordinal))
                        continue;
                    refs.Add(host.AssemblyPath(name));
                }

                var nativeQueue = new Queue<string>(nativeAssemblies.Where(n => !IsFrameworkOrEngine(n)));
                var expanded = new HashSet<string>(StringComparer.Ordinal);
                while (nativeQueue.Count > 0)
                {
                    string name = nativeQueue.Dequeue();
                    if (!expanded.Add(name) || IsFrameworkOrEngine(name))
                        continue;
                    string path = host.AssemblyPath(name);
                    if (string.IsNullOrEmpty(path))
                        throw new InvalidOperationException("A WARUDO assembly dependency is missing: " + name);
                    refs.Add(path);
                    foreach (string dependency in host.AssemblyDependencies(name))
                        nativeQueue.Enqueue(dependency);
                }

                plan.asmdefAssemblies = asmdefAssemblies.OrderBy(n => n, StringComparer.Ordinal).ToArray();
                plan.references = refs.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
                plan.defines = defines.OrderBy(d => d, StringComparer.Ordinal).ToArray();
                return plan;
            }

            private static string MetadataName(INamedTypeSymbol type)
            {
                if (type.ContainingType != null)
                    return MetadataName(type.ContainingType) + "+" + type.MetadataName;
                string ns = type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString() + ".";
                return ns + type.MetadataName;
            }

            private static bool IsFrameworkOrEngine(string name)
            {
                return name == "mscorlib" || name == "netstandard" || name == "System" || name.StartsWith("System.", StringComparison.Ordinal) || name == "UnityEngine" || name.StartsWith("UnityEngine.", StringComparison.Ordinal);
            }

            private static string RuntimeText(SyntaxTree tree)
            {
                var root = tree.GetRoot();
                var conditional = root.DescendantTrivia().Where(t => t.IsKind(SyntaxKind.DisabledTextTrivia) || t.IsKind(SyntaxKind.IfDirectiveTrivia) || t.IsKind(SyntaxKind.ElifDirectiveTrivia) || t.IsKind(SyntaxKind.ElseDirectiveTrivia) || t.IsKind(SyntaxKind.EndIfDirectiveTrivia) || t.IsKind(SyntaxKind.DefineDirectiveTrivia) || t.IsKind(SyntaxKind.UndefDirectiveTrivia));
                var active = root.ReplaceTrivia(conditional, (t, rewritten) => SyntaxFactory.Whitespace(new string (t.ToFullString().Select(c => c == '\r' || c == '\n' ? c : ' ').ToArray())));
                return "#line 1 \"" + tree.FilePath.Replace('\\', '/') + "\"\n" + active.ToFullString();
            }

            private static IEnumerable<string> RuntimeDefines(IEnumerable<string> defines)
            {
                return defines.Where(d => d != "UNITY_EDITOR" && !d.StartsWith("UNITY_EDITOR_", StringComparison.Ordinal));
            }

            // The analysis parses at C# 9, so a newer feature is an error here. The compile decides those: it compiles again at
            // Unity's default level (WbbRuntimeCompileSession). CS8370 is "Feature is not available in C# 7.3 / 8", CS8773 is
            // "Feature is not available in C# 9.0" (Roslyn 4.13 uses a separate code for 9.0), and CS8652 is "Feature is currently
            // in Preview and unsupported". Nothing else is skipped.
            private static bool IsLanguageLevelError(Diagnostic error)
            {
                return error.Id == "CS8370" || error.Id == "CS8773" || error.Id == "CS8652";
            }

            // Same shape as the compile errors: project-relative file, line, then the message.
            private static string DescribeError(Diagnostic error)
            {
                string text = "error " + error.Id + ": " + WbbBuildLog.Shorten(error.GetMessage());
                if (!error.Location.IsInSource)
                    return text;
                var span = error.Location.GetLineSpan();
                return WbbRuntimeCompileSession.ProjectRelative(span.Path) + "(" + (span.StartLinePosition.Line + 1) + "): " + text;
            }
        }
    }
}
#endif
