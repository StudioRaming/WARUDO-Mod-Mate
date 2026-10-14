using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WmmFixture.EditorTools
{
    // Test aid only. For every UMod build of a mod named WmmFix_*, appends to Library/WmmFixtureAudit.txt which
    // components the SDK linked (LinkBehaviourV2), against which assembly, and whether raw fixture components or
    // missing scripts remain. Reflection only, so it compiles whatever the SDK version.
    [InitializeOnLoad]
    public static class WmmFixtureAudit
    {
        private const string Prefix = "WmmFix_";
        private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static FieldInfo field;
        private static Delegate handler;

        static WmmFixtureAudit()
        {
            try
            {
                Install();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[WmmFixtureAudit] not installed: " + e.Message);
            }
        }

        public static string LogPath
        {
            get
            {
                return Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/WmmFixtureAudit.txt"));
            }
        }

        private static void Install()
        {
            Type engine = FindType("UMod.BuildEngine.BuildEngineService");
            Type pipeline = null;
            for (Type t = engine; t != null && t != typeof(object); t = t.BaseType)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition().Name.StartsWith("BuildPipelineService", StringComparison.Ordinal))
                {
                    pipeline = t;
                    break;
                }
            }

            if (pipeline == null)
                return;
            field = pipeline.GetField("afterRunProcessors", BindingFlags.Public | BindingFlags.Static);
            MethodInfo generic = typeof(WmmFixtureAudit).GetMethod("OnAfterRun", BindingFlags.Static | BindingFlags.NonPublic);
            handler = Delegate.CreateDelegate(field.FieldType, generic.MakeGenericMethod(pipeline.GetGenericArguments()[0]));
            field.SetValue(null, Delegate.Combine((Delegate)field.GetValue(null), handler));
        }

        private static void OnAfterRun<T>(T context)
        {
            try
            {
                Audit(context);
            }
            catch (Exception e)
            {
                Append("audit error: " + e.GetType().Name + " " + e.Message);
            }
        }

        private static void Audit(object context)
        {
            string modName = Get(Get(context, "ExportProfile"), "ModName") as string;
            if (modName == null || !modName.StartsWith(Prefix, StringComparison.Ordinal))
                return;
            string stage = Convert.ToString(Get(context, "ProcessorStage"));
            Type linkType = FindType("UMod.Shared.Linker.LinkBehaviourV2");
            var assets = (Array)Get(Get(context, "BuildAssets"), "AllAssets");
            foreach (object asset in assets)
            {
                if (!string.Equals(Get(asset, "Extension") as string, ".prefab", StringComparison.Ordinal))
                    continue;
                var go = Get(asset, "LoadedObject") as GameObject;
                if (go == null)
                    continue;
                int links = 0;
                int raw = 0;
                int missing = 0;
                var lines = new StringBuilder();
                foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                    missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                foreach (Component c in go.GetComponentsInChildren(linkType, true))
                {
                    links++;
                    object reference = Get(c, "TypeReference");
                    string script = Get(reference, "ScriptName") as string;
                    FieldInfo assemblyField = reference.GetType().GetField("assemblyName", Any);
                    string assembly = assemblyField == null ? "?" : new System.Reflection.AssemblyName((string)assemblyField.GetValue(reference)).Name;
                    lines.AppendLine("link=" + script + "|" + assembly);
                }

                foreach (MonoBehaviour b in go.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (b != null && !linkType.IsInstanceOfType(b) && b.GetType().Namespace != null && b.GetType().Namespace.StartsWith("WmmFixture", StringComparison.Ordinal))
                        raw++;
                }

                Append("build=" + modName + " stage=" + stage + " prefab=" + Get(asset, "RelativePath") + " links=" + links + " raw=" + raw + " missing=" + missing + "\n" + lines.ToString().TrimEnd());
            }
        }

        private static void Append(string text)
        {
            File.AppendAllText(LogPath, text + "\n", new UTF8Encoding(false));
        }

        private static object Get(object target, string name)
        {
            if (target == null)
                throw new InvalidOperationException("null before " + name);
            PropertyInfo property = target.GetType().GetProperty(name, Any);
            if (property == null)
                throw new InvalidOperationException("missing " + target.GetType().Name + "." + name);
            return property.GetValue(target, null);
        }

        private static Type FindType(string fullName)
        {
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = assembly.GetType(fullName, false);
                if (t != null)
                    return t;
            }

            return null;
        }
    }
}
