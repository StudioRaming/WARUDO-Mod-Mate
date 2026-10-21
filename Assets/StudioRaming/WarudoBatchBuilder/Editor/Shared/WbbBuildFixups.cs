using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StudioRaming.WarudoBatchBuilder
{
    public sealed class WbbFixupOptions
    {
        public bool stripVrcComponents = true;
        public static WbbFixupOptions FromJob(WbbJob job)
        {
            var options = new WbbFixupOptions();
            if (job != null)
                options.stripVrcComponents = job.stripVrcComponents;
            return options;
        }
    }

    public sealed class WbbFixupReport
    {
        public struct Entry
        {
            public string ruleId;
            public int count;
            public string text;
        }

        private readonly List<Entry> entries = new List<Entry>();
        public IList<Entry> Entries
        {
            get
            {
                return entries;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return entries.Count == 0;
            }
        }

        public void Add(string ruleId, int count, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            entries.Add(new Entry { ruleId = ruleId ?? "", count = count, text = text });
        }

        public int CountOf(string ruleId)
        {
            int total = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                if (string.Equals(entries[i].ruleId, ruleId, StringComparison.Ordinal))
                    total += entries[i].count;
            }

            return total;
        }

        public string ToText()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < entries.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append(entries[i].text);
            }

            return sb.ToString();
        }
    }

    public abstract class WbbFixupRule
    {
        public abstract string Id { get; }

        public virtual bool IsEnabled(WbbFixupOptions options)
        {
            return true;
        }

        public abstract void Apply(GameObject copyRoot, WbbFixupOptions options, WbbFixupReport report);
        protected static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }
    }

    public static class WbbBuildFixups
    {
        public const string RuleMissingScripts = "missing-scripts";
        public const string RuleVrcSdk = "vrc-sdk";
        public const string RuleModularAvatar = "modular-avatar-ndmf";
        public const string RuleEditorAssembly = "editor-only-assembly";
        public const string RuleEditorOnlyTag = "editor-only-tag";
        // Extension point for tools that process the guarded copy before the build rules.
        // Receives the copy (a temporary object in a preview scene, never an asset). Runs on every
        // copy before the rule loop and outside its try/catch, so an exception fails the item.
        public static Action<GameObject> BeforeRules;
        private static readonly List<WbbFixupRule> rules = new List<WbbFixupRule>
        {
            new MissingScriptsRule(),
            new VrcSdkRule(),
            new ModularAvatarRule(),
            new EditorAssemblyRule(),
            new EditorOnlyTagRule(),
        };
        public static IList<WbbFixupRule> Rules
        {
            get
            {
                return rules.AsReadOnly();
            }
        }

        public static void Register(WbbFixupRule rule)
        {
            if (rule == null || string.IsNullOrEmpty(rule.Id))
                return;
            for (int i = 0; i < rules.Count; i++)
            {
                if (string.Equals(rules[i].Id, rule.Id, StringComparison.Ordinal))
                {
                    rules[i] = rule;
                    return;
                }
            }

            rules.Add(rule);
        }

        public static WbbFixupReport Apply(GameObject copyRoot, WbbFixupOptions options)
        {
            var report = new WbbFixupReport();
            if (copyRoot == null)
                return report;
            GuardCopy(copyRoot);
            Action<GameObject> before = BeforeRules;
            if (before != null)
                before(copyRoot);
            if (options == null)
                options = new WbbFixupOptions();
            for (int i = 0; i < rules.Count; i++)
            {
                WbbFixupRule rule = rules[i];
                if (rule == null || !rule.IsEnabled(options))
                    continue;
                try
                {
                    rule.Apply(copyRoot, options, report);
                }
                catch (Exception e)
                {
                    WbbUtil.LogWarning("Build fixup '" + rule.Id + "' failed on the temporary copy: " + e.Message);
                }
            }

            return report;
        }

        private static void GuardCopy(GameObject root)
        {
            if (EditorUtility.IsPersistent(root))
                throw new InvalidOperationException("Build fixups must never run on an asset: " + AssetDatabase.GetAssetPath(root));
            if (!root.scene.IsValid() || !EditorSceneManager.IsPreviewScene(root.scene))
                throw new InvalidOperationException("Build fixups only run on a temporary copy inside a preview scene.");
        }

        public static int RemoveComponents(GameObject root, Func<Type, bool> match, out int kept)
        {
            int removed = 0;
            kept = 0;
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            var candidates = new List<Component>();
            for (int t = 0; t < all.Length; t++)
            {
                GameObject go = all[t].gameObject;
                candidates.Clear();
                Component[] components = go.GetComponents<Component>();
                for (int c = 0; c < components.Length; c++)
                {
                    Component component = components[c];
                    if (component == null || component is Transform)
                        continue;
                    if (match(component.GetType()))
                        candidates.Add(component);
                }

                if (candidates.Count == 0)
                    continue;
                bool progress = true;
                while (candidates.Count > 0 && progress)
                {
                    progress = false;
                    for (int i = candidates.Count - 1; i >= 0; i--)
                    {
                        Component component = candidates[i];
                        if (component == null)
                        {
                            candidates.RemoveAt(i);
                            continue;
                        }

                        if (IsRequiredByOther(go, component))
                            continue;
                        Object.DestroyImmediate(component);
                        candidates.RemoveAt(i);
                        removed++;
                        progress = true;
                    }
                }

                kept += candidates.Count;
            }

            return removed;
        }

        private static bool IsRequiredByOther(GameObject go, Component target)
        {
            Type targetType = target.GetType();
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component other = components[i];
                if (other == null || other == target)
                    continue;
                object[] attributes = other.GetType().GetCustomAttributes(typeof(RequireComponent), true);
                for (int a = 0; a < attributes.Length; a++)
                {
                    var require = attributes[a] as RequireComponent;
                    if (require == null)
                        continue;
                    if (Requires(require.m_Type0, targetType, components, target))
                        return true;
                    if (Requires(require.m_Type1, targetType, components, target))
                        return true;
                    if (Requires(require.m_Type2, targetType, components, target))
                        return true;
                }
            }

            return false;
        }

        private static bool Requires(Type required, Type targetType, Component[] components, Component target)
        {
            if (required == null || !required.IsAssignableFrom(targetType))
                return false;
            for (int i = 0; i < components.Length; i++)
            {
                Component c = components[i];
                if (c != null && c != target && required.IsInstanceOfType(c))
                    return false;
            }

            return true;
        }

        private static readonly Dictionary<Type, int> originCache = new Dictionary<Type, int>();
        private const int OriginOther = 0, OriginVrc = 1, OriginNadena = 2, OriginEditor = 3;
        private static HashSet<string> editorOnlyAssemblies;
        private static int OriginOf(Type type)
        {
            int origin;
            if (originCache.TryGetValue(type, out origin))
                return origin;
            string ns = type.Namespace ?? "";
            string assembly = type.Assembly.GetName().Name ?? "";
            if (ns.StartsWith("nadena.dev", StringComparison.Ordinal) || assembly.StartsWith("nadena.dev", StringComparison.OrdinalIgnoreCase))
                origin = OriginNadena;
            else if (ns == "VRC" || ns.StartsWith("VRC.", StringComparison.Ordinal) || assembly.StartsWith("VRC.", StringComparison.Ordinal) || assembly.StartsWith("VRCSDK", StringComparison.Ordinal) || assembly.StartsWith("VRCCore", StringComparison.Ordinal))
                origin = OriginVrc;
            else if (IsEditorOnlyAssembly(assembly))
                origin = OriginEditor;
            else
                origin = OriginOther;
            originCache[type] = origin;
            return origin;
        }

        public static bool IsVrcSdkType(Type type)
        {
            return type != null && OriginOf(type) == OriginVrc;
        }

        public static bool IsModularAvatarType(Type type)
        {
            return type != null && OriginOf(type) == OriginNadena;
        }

        public static bool IsEditorOnlyType(Type type)
        {
            return type != null && OriginOf(type) == OriginEditor;
        }

        private static bool IsEditorOnlyAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return false;
            if (assemblyName.StartsWith("UnityEditor", StringComparison.Ordinal))
                return true;
            if (editorOnlyAssemblies == null)
                editorOnlyAssemblies = BuildEditorOnlySet();
            return editorOnlyAssemblies.Contains(assemblyName);
        }

        private static HashSet<string> BuildEditorOnlySet()
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            try
            {
                var player = new HashSet<string>(StringComparer.Ordinal);
                UnityEditor.Compilation.Assembly[] playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
                for (int i = 0; i < playerAssemblies.Length; i++)
                    player.Add(playerAssemblies[i].name);
                UnityEditor.Compilation.Assembly[] editorAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
                for (int i = 0; i < editorAssemblies.Length; i++)
                {
                    if (!player.Contains(editorAssemblies[i].name))
                        set.Add(editorAssemblies[i].name);
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not list compiled assemblies for build fixups: " + e.Message);
            }

            try
            {
                PluginImporter[] importers = PluginImporter.GetAllImporters();
                for (int i = 0; i < importers.Length; i++)
                {
                    PluginImporter importer = importers[i];
                    if (importer == null || importer.isNativePlugin)
                        continue;
                    if (!importer.assetPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (!importer.GetCompatibleWithEditor())
                        continue;
                    if (importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64))
                        continue;
                    set.Add(Path.GetFileNameWithoutExtension(importer.assetPath));
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not inspect plugin import settings for build fixups: " + e.Message);
            }

            return set;
        }

        private sealed class MissingScriptsRule : WbbFixupRule
        {
            public override string Id
            {
                get
                {
                    return RuleMissingScripts;
                }
            }

            public override void Apply(GameObject copyRoot, WbbFixupOptions options, WbbFixupReport report)
            {
                int removed = 0;
                Transform[] all = copyRoot.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < all.Length; i++)
                {
                    GameObject go = all[i].gameObject;
                    int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                    if (count <= 0)
                        continue;
                    removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                }

                if (removed > 0)
                    report.Add(Id, removed, L("Safety.Fixup.MissingScripts", removed));
            }
        }

        private sealed class VrcSdkRule : WbbFixupRule
        {
            public override string Id
            {
                get
                {
                    return RuleVrcSdk;
                }
            }

            public override bool IsEnabled(WbbFixupOptions options)
            {
                return options != null && options.stripVrcComponents;
            }

            public override void Apply(GameObject copyRoot, WbbFixupOptions options, WbbFixupReport report)
            {
                int kept;
                int removed = RemoveComponents(copyRoot, IsVrcSdkType, out kept);
                if (removed > 0)
                    report.Add(Id, removed, L("Safety.Fixup.Vrc", removed));
                if (kept > 0)
                    report.Add(Id + ".kept", kept, L("Safety.Fixup.Kept", kept));
            }
        }

        private sealed class ModularAvatarRule : WbbFixupRule
        {
            public override string Id
            {
                get
                {
                    return RuleModularAvatar;
                }
            }

            public override void Apply(GameObject copyRoot, WbbFixupOptions options, WbbFixupReport report)
            {
                int kept;
                int removed = RemoveComponents(copyRoot, IsModularAvatarType, out kept);
                if (removed > 0)
                    report.Add(Id, removed, L("Safety.Fixup.ModularAvatar", removed));
                if (kept > 0)
                    report.Add(Id + ".kept", kept, L("Safety.Fixup.Kept", kept));
            }
        }

        private sealed class EditorAssemblyRule : WbbFixupRule
        {
            public override string Id
            {
                get
                {
                    return RuleEditorAssembly;
                }
            }

            public override void Apply(GameObject copyRoot, WbbFixupOptions options, WbbFixupReport report)
            {
                int kept;
                int removed = RemoveComponents(copyRoot, IsEditorOnlyType, out kept);
                if (removed > 0)
                    report.Add(Id, removed, L("Safety.Fixup.EditorOnlyComponent", removed));
                if (kept > 0)
                    report.Add(Id + ".kept", kept, L("Safety.Fixup.Kept", kept));
            }
        }

        private sealed class EditorOnlyTagRule : WbbFixupRule
        {
            public override string Id
            {
                get
                {
                    return RuleEditorOnlyTag;
                }
            }

            public override void Apply(GameObject copyRoot, WbbFixupOptions options, WbbFixupReport report)
            {
                int count = 0;
                Transform[] all = copyRoot.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i].gameObject.CompareTag("EditorOnly"))
                        count++;
                }

                if (count > 0)
                    report.Add(Id, count, L("Safety.Fixup.EditorOnlyTag", count));
            }
        }
    }
}
