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
        public static WbbWarudoTypeCatalog Read()
        {
            string streaming = WbbWarudoLocator.StreamingAssetsPath;
            if (string.IsNullOrEmpty(streaming))
                throw new InvalidOperationException("WARUDO installation is required to identify provided runtime types.");
            string directory = Path.Combine(Path.GetDirectoryName(streaming), "Managed");
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException("WARUDO managed assemblies are missing: " + directory);
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

            return result;
        }

        public bool Contains(string assembly, string metadataName, out string path)
        {
            path = "";
            IAssemblySymbol symbol;
            if (!symbols.TryGetValue(assembly, out symbol))
                return false;
            // WARUDO's System.dll forwards Stack`1/Queue`1 to mscorlib; a forwarded type is still provided, but only
            // when the target resolves: a forward into an assembly WARUDO does not ship comes back as an error type.
            if (symbol.GetTypeByMetadataName(metadataName) == null)
            {
                var forwarded = symbol.ResolveForwardedType(metadataName);
                if (forwarded == null || forwarded.TypeKind == TypeKind.Error)
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
