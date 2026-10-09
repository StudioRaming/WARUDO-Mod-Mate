using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace StudioRaming.WarudoBatchBuilder
{
    // Hands the DLLs that Mod Mate compiled to UMod's build while one build runs.
    // UMod calls BuildPipelineService<BuildContext, ...>.beforeRunProcessors before each processor pass;
    // the hook drops every .cs source from the build and registers the DLLs once. All runtime code of the mod is in those
    // DLLs, so a script that is left over (the source of an editor-only, test or define-filtered assembly) must not reach
    // UMod's own script compile either: that compile needs the project's .csproj and would fail without it.
    // Everything goes through reflection like the rest of the UMod access in Mod Mate, so a SDK version
    // that changes these members fails with a clear message instead of breaking the compile of this tool.
    public sealed class WbbUModBuildHook : IDisposable
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly string modName;
        private readonly string modFolderFull;
        private readonly List<WbbCompiledUnit> units;
        private readonly bool scriptsUnderFolderOnly;
        private readonly HashSet<string> excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private FieldInfo field;
        private Delegate handler;
        private object registeredContext;
        public int ExcludedCount { get; private set; }
        public int RegisteredCount { get; private set; }

        private WbbUModBuildHook(string modName, string modFolder, IList<WbbCompiledUnit> units, bool scriptsUnderFolderOnly)
        {
            this.modName = modName;
            this.scriptsUnderFolderOnly = scriptsUnderFolderOnly;
            this.modFolderFull = TrimEnd(Path.GetFullPath(modFolder));
            this.units = new List<WbbCompiledUnit>(units);
            for (int i = 0; i < this.units.Count; i++)
            {
                for (int k = 0; k < this.units[i].sources.Length; k++)
                    excluded.Add(Path.GetFullPath(this.units[i].sources[k]));
                if (!string.IsNullOrEmpty(this.units[i].asmdefPath))
                    excluded.Add(Path.GetFullPath(WbbUtil.ToAbsolutePath(this.units[i].asmdefPath)));
            }
        }

        // `scriptsUnderFolderOnly`: a manual script folder is the whole mod, so every .cs below it is dropped. Automatic mode
        // builds from a temporary folder and the scripts come in as dependencies of the prefab, so every .cs is dropped.
        public static WbbUModBuildHook Install(string modName, string modFolder, IList<WbbCompiledUnit> units, bool scriptsUnderFolderOnly, out string error)
        {
            error = "";
            try
            {
                System.Type engine = WbbExportSettings.EngineServiceType();
                if (engine == null)
                {
                    error = WbbLang.T("Asmdef.HookUnavailable", "UMod.BuildEngine.BuildEngineService");
                    return null;
                }

                System.Type pipeline = null;
                for (System.Type t = engine; t != null && t != typeof(object); t = t.BaseType)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition().Name.StartsWith("BuildPipelineService", StringComparison.Ordinal))
                    {
                        pipeline = t;
                        break;
                    }
                }

                FieldInfo hookField = pipeline == null ? null : pipeline.GetField("beforeRunProcessors", BindingFlags.Public | BindingFlags.Static);
                if (hookField == null || !typeof(Delegate).IsAssignableFrom(hookField.FieldType))
                {
                    error = WbbLang.T("Asmdef.HookUnavailable", "BuildPipelineService.beforeRunProcessors");
                    return null;
                }

                System.Type contextType = pipeline.GetGenericArguments()[0];
                var hook = new WbbUModBuildHook(modName, modFolder, units, scriptsUnderFolderOnly);
                MethodInfo generic = typeof(WbbUModBuildHook).GetMethod("OnBeforeRun", BindingFlags.Instance | BindingFlags.NonPublic);
                hook.handler = Delegate.CreateDelegate(hookField.FieldType, hook, generic.MakeGenericMethod(contextType));
                hook.field = hookField;
                hookField.SetValue(null, Delegate.Combine((Delegate)hookField.GetValue(null), hook.handler));
                return hook;
            }
            catch (Exception e)
            {
                error = WbbLang.T("Asmdef.HookUnavailable", e.Message);
                return null;
            }
        }

        public void Dispose()
        {
            if (field == null || handler == null)
                return;
            try
            {
                field.SetValue(null, Delegate.Remove((Delegate)field.GetValue(null), handler));
            }
            catch (Exception e)
            {
                WbbUtil.LogError("Could not remove the UMod build hook: " + e.Message);
            }

            handler = null;
            field = null;
        }

        // Runs for every processor pass of every UMod build; only the build of this mod is touched.
        private void OnBeforeRun<T>(T context)
        {
            object build = context;
            if (build == null || !Owned(build))
                return;
            if (!ScriptsAllowed(build))
                throw new InvalidOperationException(WbbLang.T("Asmdef.ScriptsNotAllowed"));
            Exclude(build);
            if (ReferenceEquals(registeredContext, build))
                return;
            Register(build);
            registeredContext = build;
        }

        private bool Owned(object build)
        {
            object profile = Get(build, "ExportProfile");
            string name = Get(profile, "ModName") as string;
            string assets = Get(build, "ModAssetsPath") as string;
            if (!string.Equals(name, modName, StringComparison.Ordinal) || string.IsNullOrEmpty(assets))
                return false;
            return string.Equals(TrimEnd(Path.GetFullPath(assets)), modFolderFull, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ScriptsAllowed(object build)
        {
            try
            {
                object security = Get(Get(build, "ToolSettings"), "Security");
                object value = Get(security, "AllowScriptsInMods");
                return !(value is bool) || (bool)value;
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not read whether UMod allows scripts in mods: " + e.Message);
                return true;
            }
        }

        private void Exclude(object build)
        {
            object assets = Get(build, "BuildAssets");
            MethodInfo list = Single(assets.GetType(), "GetAssetsOfType", typeof(string[]));
            if (list == null)
                throw new InvalidOperationException(WbbLang.T("Asmdef.HookUnavailable", "BuildAssets.GetAssetsOfType"));
            var found = list.Invoke(assets, new object[] { new[] { ".cs", ".asmdef", ".asmref" } }) as Array;
            if (found == null || found.Length == 0)
                return;
            MethodInfo exclude = Single(assets.GetType(), "ExcludeAsset", found.GetValue(0).GetType());
            if (exclude == null)
                throw new InvalidOperationException(WbbLang.T("Asmdef.HookUnavailable", "BuildAssets.ExcludeAsset"));
            foreach (object asset in found)
            {
                string full = Get(asset, "FullPath") as string;
                if (string.IsNullOrEmpty(full) || !ShouldExclude(Path.GetFullPath(full)))
                    continue;
                exclude.Invoke(assets, new[] { asset });
                ExcludedCount++;
            }
        }

        private bool ShouldExclude(string full)
        {
            if (excluded.Contains(full))
                return true;
            string extension = Path.GetExtension(full);
            if (string.Equals(extension, ".cs", StringComparison.OrdinalIgnoreCase))
                return !scriptsUnderFolderOnly || full.StartsWith(modFolderFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
            bool definition = string.Equals(extension, ".asmdef", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".asmref", StringComparison.OrdinalIgnoreCase);
            return definition && full.StartsWith(modFolderFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private void Register(object build)
        {
            object assemblies = Get(build, "BuildAssemblies");
            System.Type type = assemblies.GetType();
            MethodInfo register = type.GetMethod("RegisterAssemblyForBuild", new[] { typeof(byte[]), typeof(bool) });
            MethodInfo metadata = type.GetMethod("RegisterMetaDataForBuild", new[] { typeof(string), typeof(byte[]) });
            MethodInfo symbols = type.GetMethod("RegisterSymbolsForBuild", new[] { typeof(string), typeof(byte[]) });
            if (register == null || metadata == null)
                throw new InvalidOperationException(WbbLang.T("Asmdef.HookUnavailable", "BuildAssemblies.RegisterAssemblyForBuild"));
            for (int i = 0; i < units.Count; i++)
            {
                WbbCompiledUnit unit = units[i];
                if (!(bool)register.Invoke(assemblies, new object[] { unit.dll, true }) || !(bool)metadata.Invoke(assemblies, new object[] { unit.name, ModuleMeta(unit.metaGuid) }))
                    throw new InvalidOperationException(WbbLang.T("Asmdef.RegisterFailed", unit.name));
                if (unit.pdb != null && symbols != null)
                    symbols.Invoke(assemblies, new object[] { unit.name, unit.pdb });
                RegisteredCount++;
            }
        }

        // The import settings UMod stores next to a script assembly: loaded in every platform, none of the editor.
        private static byte[] ModuleMeta(string guid)
        {
            return Encoding.UTF8.GetBytes("fileFormatVersion: 2\nguid: " + guid + "\nPluginImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  iconMap: {}\n  executionOrder: {}\n  defineConstraints: []\n"
                + "  isPreloaded: 0\n  isOverridable: 0\n  isExplicitlyReferenced: 0\n  validateReferences: 1\n  platformData:\n"
                + "  - first:\n      Any: \n    second:\n      enabled: 1\n      settings: {}\n  - first:\n      Editor: Editor\n    second:\n      enabled: 0\n      settings:\n        DefaultValueInitialized: true\n");
        }

        // The one-parameter public method of that name whose parameter takes `argument` (any when null).
        // GetMethod(name) alone throws when the SDK offers overloads.
        private static MethodInfo Single(System.Type type, string name, System.Type argument)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name != name)
                    continue;
                ParameterInfo[] parameters = methods[i].GetParameters();
                if (parameters.Length == 1 && (argument == null || parameters[0].ParameterType.IsAssignableFrom(argument)))
                    return methods[i];
            }

            return null;
        }

        private static object Get(object target, string name)
        {
            if (target == null)
                throw new InvalidOperationException(WbbLang.T("Asmdef.HookUnavailable", name));
            PropertyInfo property = target.GetType().GetProperty(name, Members);
            if (property == null)
                throw new InvalidOperationException(WbbLang.T("Asmdef.HookUnavailable", target.GetType().Name + "." + name));
            return property.GetValue(target, null);
        }

        private static string TrimEnd(string path)
        {
            return path.TrimEnd('/', '\\');
        }
    }
}
