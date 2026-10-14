#if WBB_WARUDO_SDK
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace StudioRaming.WarudoBatchBuilder
{
    internal sealed class WbbWarudoTypeCatalog
    {
        private readonly Dictionary<string, IAssemblySymbol> symbols = new Dictionary<string, IAssemblySymbol>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> paths = new Dictionary<string, string>(StringComparer.Ordinal);
        // `streaming` is the Warudo_Data/StreamingAssets folder WbbWarudoLocator.ResolveForOutput decided on.
        public static WbbWarudoTypeCatalog Read(string streaming)
        {
            if (string.IsNullOrEmpty(streaming))
                throw new InvalidOperationException(WbbLang.T("Safety.WarudoManagedMissing"));
            string directory = Path.Combine(Path.GetDirectoryName(streaming), "Managed");
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(WbbLang.T("Safety.WarudoManagedFolder", directory));
            var result = new WbbWarudoTypeCatalog();
            var refs = new List<MetadataReference>();
            foreach (var file in Directory.GetFiles(directory, "*.dll").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    refs.Add(MetadataReference.CreateFromFile(file));
                }
                catch (BadImageFormatException)
                {
                }
            }

            var compilation = CSharpCompilation.Create("WbbWarudoMetadata", references: refs);
            foreach (var reference in refs)
            {
                var assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                if (assembly == null)
                    continue;
                result.symbols[assembly.Identity.Name] = assembly;
                result.paths[assembly.Identity.Name] = reference.Display;
            }

            WbbUtil.Log("WARUDO type catalog read: " + result.symbols.Count + " assemblies from " + directory);
            return result;
        }

        public bool Contains(string assembly, string metadataName, out string path)
        {
            path = "";
            IAssemblySymbol symbol;
            if (!symbols.TryGetValue(assembly, out symbol))
                return false;
            // A nested name is Outer+Inner: resolve the top-level part, then descend, so an Inner that this WARUDO
            // build lacks is rejected even when Outer exists.
            string[] parts = metadataName.Split('+');
            // WARUDO's System.dll forwards Stack`1/Queue`1 to mscorlib; a forwarded type is still provided, but only
            // when the target resolves: a forward into an assembly WARUDO does not ship comes back as an error type.
            INamedTypeSymbol current = symbol.GetTypeByMetadataName(parts[0]);
            if (current == null)
            {
                current = symbol.ResolveForwardedType(parts[0]);
                if (current == null || current.TypeKind == TypeKind.Error)
                    return false;
            }

            for (int i = 1; i < parts.Length; i++)
            {
                string name = parts[i];
                int arity = 0;
                int tick = name.LastIndexOf('`');
                if (tick >= 0 && int.TryParse(name.Substring(tick + 1), out arity))
                    name = name.Substring(0, tick);
                else
                    arity = 0;
                current = current.GetTypeMembers(name, arity).FirstOrDefault();
                if (current == null)
                    return false;
            }

            path = paths[assembly];
            return true;
        }

        public string AssemblyPath(string assembly)
        {
            string path;
            return paths.TryGetValue(assembly, out path) ? path : "";
        }

        public IEnumerable<string> AssemblyDependencies(string assembly)
        {
            IAssemblySymbol symbol;
            if (!symbols.TryGetValue(assembly, out symbol))
                return new string[0];
            return symbol.Modules.SelectMany(m => m.ReferencedAssemblySymbols).Select(s => s.Identity.Name).Distinct(StringComparer.Ordinal);
        }
    }
}
#endif
