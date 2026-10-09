using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    // Creates the build settings asset (and one profile) the way the SDK's own Export Settings window does:
    // ModScriptableAssetExtensions.LoadOrCreate for the asset, ExportSettings.CreateNewExportProfile for the profile.
    // Nothing else is written; Mod Mate fills the profile in at build time and puts it back afterwards.
    public static class WbbExportSettingsCreator
    {
        private const string SettingsType = "UMod.ModTools.Export.ExportSettings";
        private const string ExtensionsType = "UMod.Shared.ModScriptableAssetExtensions";
        // Creates the asset when none exists and adds a profile when it has none. Returns the asset path.
        public static bool TryCreate(out string assetPath, out string error)
        {
            assetPath = "";
            error = "";
            try
            {
                System.Type settingsType = WbbExportSettings.FindType(null, SettingsType);
                System.Type extensions = WbbExportSettings.FindType(null, ExtensionsType);
                if (settingsType == null || extensions == null)
                {
                    error = WbbLang.T("Runner.NoUMod");
                    return false;
                }

                MethodInfo loadOrCreate = null;
                MethodInfo[] methods = extensions.GetMethods(BindingFlags.Public | BindingFlags.Static);
                for (int i = 0; i < methods.Length; i++)
                {
                    if (methods[i].Name == "LoadOrCreate" && methods[i].IsGenericMethodDefinition && methods[i].GetParameters().Length == 2)
                    {
                        loadOrCreate = methods[i];
                        break;
                    }
                }

                MethodInfo addProfile = settingsType.GetMethod("CreateNewExportProfile", BindingFlags.Public | BindingFlags.Instance);
                if (loadOrCreate == null || addProfile == null)
                {
                    error = WbbLang.T("Runner.FieldMissing", "LoadOrCreate / CreateNewExportProfile");
                    return false;
                }

                // The first argument is only the extension receiver; the SDK ignores it.
                var settings = loadOrCreate.MakeGenericMethod(settingsType).Invoke(null, new object[] { null, null }) as ScriptableObject;
                if (settings == null)
                {
                    error = WbbLang.T("Runner.SettingsLoadFailed", SettingsType);
                    return false;
                }

                string error2;
                int count = WbbExportSettings.GetProfileCount(out error2);
                if (count == 0)
                {
                    addProfile.Invoke(settings, new object[] { true });
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssetIfDirty(settings);
                }

                assetPath = AssetDatabase.GetAssetPath(settings);
                return true;
            }
            catch (System.Exception e)
            {
                error = e.InnerException != null ? e.InnerException.Message : e.Message;
                return false;
            }
        }
    }
}
