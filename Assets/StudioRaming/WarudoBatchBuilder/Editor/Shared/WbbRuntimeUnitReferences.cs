#if WBB_WARUDO_SDK
using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace StudioRaming.WarudoBatchBuilder
{
    // Names of the assemblies a compiled DLL really references. Read from the file bytes each time,
    // so a rebuilt DLL of the same name is never answered from an older copy.
    internal static class WbbRuntimeUnitReferences
    {
        public static string[] Read(byte[] image)
        {
            try
            {
                MetadataReference reference = MetadataReference.CreateFromImage(image);
                CSharpCompilation compilation = CSharpCompilation.Create("WbbReferenceProbe", references: new[] { reference });
                var assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                if (assembly == null)
                    return null;
                return assembly.Modules.SelectMany(m => m.ReferencedAssemblySymbols).Select(s => s.Identity.Name).Distinct(StringComparer.Ordinal).OrderBy(n => n, StringComparer.Ordinal).ToArray();
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not read the references of a compiled assembly: " + e.Message);
                return null;
            }
        }
    }
}
#endif
