using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public sealed class WbbBuildInvokeResult
    {
        public bool invoked;
        public bool resultKnown;
        public bool successful;
        public string errorMessage = "";
        public string exceptionMessage = "";
        public string builtFile = "";
        public string method = "";
    }

    public struct WbbUModQuietState
    {
        public bool captured;
        public bool clearConsoleOnBuild;
        public bool showOutputDirectory;
        public int logLevel;
    }

    public static class WbbExportSettings
    {
        public const string BuildMenuPath = "Warudo/Build Mod";
        public const int LogLevelWarnings = 3;
        private const string ModToolsUtilTypeName = "UMod.BuildEngine.ModToolsUtil";
        private const string ModToolsUtilQualified = "UMod.BuildEngine.ModToolsUtil, UMod-BuildEngine";
        private const string BuildEngineServiceTypeName = "UMod.BuildEngine.BuildEngineService";
        private const string SettingsAssetFileName = "ExportSettings.asset";
        private const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly string[] EssentialFields =
        {
            "modName",
            "modAssetPath",
            "modExportPath"
        };
        private static System.Type cachedUtilType;
        private static System.Type cachedEngineServiceType;
        private static string L(string key)
        {
            return WbbLang.T(key);
        }

        private static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }

        public static ScriptableObject Locate(out string error)
        {
            error = "";
            try
            {
                string chosen = LocatePath();
                if (string.IsNullOrEmpty(chosen))
                {
                    error = L("Runner.NoExportSettings");
                    return null;
                }

                var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(chosen);
                if (settings == null)
                {
                    error = L("Runner.SettingsLoadFailed", chosen);
                    return null;
                }

                return settings;
            }
            catch (System.Exception e)
            {
                error = L("Runner.SettingsLoadFailed", e.Message);
                return null;
            }
        }

        public static string LocatePath()
        {
            string[] guids = AssetDatabase.FindAssets("ExportSettings t:ScriptableObject");
            string exact = null;
            string nested = null;
            string loose = null;
            if (guids != null)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (string.IsNullOrEmpty(path))
                        continue;
                    path = WbbUtil.NormalizePath(path);
                    if (!path.StartsWith("Assets/"))
                        continue;
                    if (path == "Assets/" + SettingsAssetFileName)
                    {
                        exact = path;
                        break;
                    }

                    if (path.EndsWith("/" + SettingsAssetFileName))
                    {
                        if (nested == null)
                            nested = path;
                    }
                    else if (path.EndsWith(SettingsAssetFileName))
                    {
                        if (loose == null)
                            loose = path;
                    }
                }
            }

            string chosen = exact != null ? exact : (nested != null ? nested : loose);
            if (!string.IsNullOrEmpty(chosen))
                return chosen;
            const string fallback = "Assets/" + SettingsAssetFileName;
            try
            {
                if (System.IO.File.Exists(WbbUtil.ToAbsolutePath(fallback)))
                    return fallback;
            }
            catch (System.Exception)
            {
            }

            return "";
        }

        public static bool ValidateAvailable(out string error)
        {
            error = "";
            if (FindModToolsUtilType() != null)
                return true;
            if (HasBuildMenu())
                return true;
            error = L("Runner.NoUMod");
            return false;
        }

        private static System.Type FindModToolsUtilType()
        {
            if (cachedUtilType != null)
                return cachedUtilType;
            cachedUtilType = FindType(ModToolsUtilQualified, ModToolsUtilTypeName);
            return cachedUtilType;
        }

        internal static System.Type EngineServiceType()
        {
            if (cachedEngineServiceType == null)
                cachedEngineServiceType = FindType(null, BuildEngineServiceTypeName);
            return cachedEngineServiceType;
        }

        internal static System.Type FindType(string qualified, string fullName)
        {
            try
            {
                System.Type found = string.IsNullOrEmpty(qualified) ? null : System.Type.GetType(qualified);
                if (found != null)
                    return found;
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    try
                    {
                        var candidate = assemblies[i].GetType(fullName, false);
                        if (candidate != null)
                            return candidate;
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Failed to resolve type " + fullName + ": " + e.Message);
            }

            return null;
        }

        private static bool HasBuildMenu()
        {
            try
            {
                var menuType = typeof(EditorApplication).Assembly.GetType("UnityEditor.Menu");
                if (menuType == null)
                    return false;
                var method = menuType.GetMethod("GetEnabled", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                if (method == null)
                    return false;
                object result = method.Invoke(null, new object[] { BuildMenuPath });
                return result is bool && (bool)result;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static bool IsUModBuilding()
        {
            try
            {
                if (cachedEngineServiceType == null)
                    cachedEngineServiceType = FindType(null, BuildEngineServiceTypeName);
                for (System.Type t = cachedEngineServiceType; t != null && t != typeof(object); t = t.BaseType)
                {
                    if (!t.IsGenericType)
                        continue;
                    if (!t.GetGenericTypeDefinition().Name.StartsWith("BuildPipelineService", System.StringComparison.Ordinal))
                        continue;
                    PropertyInfo property = t.GetProperty("IsBuilding", BindingFlags.Public | BindingFlags.Static);
                    if (property == null)
                        return false;
                    object value = property.GetValue(null, null);
                    return value is bool && (bool)value;
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Could not read the UMod build state: " + e.Message);
            }

            return false;
        }

        public static string UModLogLocation()
        {
            try
            {
                if (cachedEngineServiceType == null)
                    cachedEngineServiceType = FindType(null, BuildEngineServiceTypeName);
                if (cachedEngineServiceType != null)
                {
                    PropertyInfo property = cachedEngineServiceType.GetProperty("LogLocation", BindingFlags.Public | BindingFlags.Static);
                    string value = property != null ? property.GetValue(null, null) as string : null;
                    if (!string.IsNullOrEmpty(value))
                        return WbbUtil.NormalizePath(value);
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Could not read the UMod log location: " + e.Message);
            }

            return WbbUtil.NormalizePath(System.IO.Path.Combine(Application.persistentDataPath, "uMod Exporter 2.0/Build.log"));
        }

        private class ProfileAccess
        {
            public ScriptableObject settings;
            public FieldInfo profilesField;
            public System.Array profiles;
            public int index;
            public object profile;
        }

        public static int GetActiveProfileIndex(out string error)
        {
            var settings = Locate(out error);
            if (settings == null)
                return -1;
            FieldInfo activeField = settings.GetType().GetField("activeProfile", FieldFlags);
            if (activeField == null)
            {
                error = L("Runner.FieldMissing", "activeProfile");
                return -1;
            }

            object rawIndex = activeField.GetValue(settings);
            return rawIndex is int ? (int)rawIndex : 0;
        }

        public static int GetProfileCount(out string error)
        {
            var settings = Locate(out error);
            if (settings == null)
                return -1;
            FieldInfo profilesField = settings.GetType().GetField("exportProfiles", FieldFlags);
            if (profilesField == null)
            {
                error = L("Runner.FieldMissing", "exportProfiles");
                return -1;
            }

            var profiles = profilesField.GetValue(settings) as System.Array;
            return profiles == null ? 0 : profiles.Length;
        }

        private static ProfileAccess OpenActiveProfile(out string error, int forceIndex = -1)
        {
            var settings = Locate(out error);
            if (settings == null)
                return null;
            System.Type settingsType = settings.GetType();
            FieldInfo activeField = settingsType.GetField("activeProfile", FieldFlags);
            FieldInfo profilesField = settingsType.GetField("exportProfiles", FieldFlags);
            if (activeField == null)
            {
                error = L("Runner.FieldMissing", "activeProfile");
                return null;
            }

            if (profilesField == null)
            {
                error = L("Runner.FieldMissing", "exportProfiles");
                return null;
            }

            var profiles = profilesField.GetValue(settings) as System.Array;
            if (profiles == null || profiles.Length == 0)
            {
                error = L("Runner.NoProfiles");
                return null;
            }

            int index;
            if (forceIndex >= 0)
            {
                if (forceIndex >= profiles.Length)
                {
                    error = L("Runner.ProfileIndexChanged");
                    return null;
                }

                index = forceIndex;
            }
            else
            {
                index = 0;
                object rawIndex = activeField.GetValue(settings);
                if (rawIndex is int)
                    index = (int)rawIndex;
                if (index < 0 || index >= profiles.Length)
                {
                    WbbUtil.LogWarning("activeProfile index out of range (" + index + "), falling back to 0.");
                    index = 0;
                }
            }

            object profile = profiles.GetValue(index);
            if (profile == null)
            {
                error = L("Runner.NoProfiles");
                return null;
            }

            var access = new ProfileAccess();
            access.settings = settings;
            access.profilesField = profilesField;
            access.profiles = profiles;
            access.index = index;
            access.profile = profile;
            error = "";
            return access;
        }

        private static void Commit(ProfileAccess access)
        {
            access.profiles.SetValue(access.profile, access.index);
            access.profilesField.SetValue(access.settings, access.profiles);
            EditorUtility.SetDirty(access.settings);
            AssetDatabase.SaveAssetIfDirty(access.settings);
        }

        public static bool TryReadActiveProfile(out WbbProfileSnapshot snapshot, out string error, int profileIndex = -1)
        {
            snapshot = null;
            var access = OpenActiveProfile(out error, profileIndex);
            if (access == null)
                return false;
            try
            {
                System.Type profileType = access.profile.GetType();
                var result = new WbbProfileSnapshot();
                result.modName = ReadString(profileType, access.profile, "modName");
                result.modAuthor = ReadString(profileType, access.profile, "modAuthor");
                result.modVersion = ReadString(profileType, access.profile, "modVersion");
                result.modDescription = ReadString(profileType, access.profile, "modDescription");
                result.modAssetPath = ReadString(profileType, access.profile, "modAssetPath");
                result.modExportPath = ReadString(profileType, access.profile, "modExportPath");
                result.modIcon = ReadObject(profileType, access.profile, "modIcon");
                snapshot = result;
                return true;
            }
            catch (System.Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public static bool TryWriteActiveProfile(WbbProfileSnapshot snapshot, out string error, int profileIndex = -1)
        {
            error = "";
            if (snapshot == null)
            {
                error = L("Runner.FieldMissing", "snapshot");
                return false;
            }

            var access = OpenActiveProfile(out error, profileIndex);
            if (access == null)
                return false;
            try
            {
                System.Type profileType = access.profile.GetType();
                var failed = new List<string>();
                if (!WriteString(profileType, access.profile, "modName", snapshot.modName))
                    failed.Add("modName");
                if (!WriteString(profileType, access.profile, "modAuthor", snapshot.modAuthor))
                    failed.Add("modAuthor");
                if (!WriteString(profileType, access.profile, "modVersion", snapshot.modVersion))
                    failed.Add("modVersion");
                if (!WriteString(profileType, access.profile, "modDescription", snapshot.modDescription))
                    failed.Add("modDescription");
                if (!WriteString(profileType, access.profile, "modAssetPath", snapshot.modAssetPath))
                    failed.Add("modAssetPath");
                if (!WriteString(profileType, access.profile, "modExportPath", snapshot.modExportPath))
                    failed.Add("modExportPath");
                if (!WriteObject(profileType, access.profile, "modIcon", snapshot.modIcon))
                    failed.Add("modIcon");
                Commit(access);
                for (int i = 0; i < failed.Count; i++)
                {
                    for (int k = 0; k < EssentialFields.Length; k++)
                    {
                        if (failed[i] == EssentialFields[k])
                        {
                            error = L("Runner.FieldMissing", failed[i]);
                            return false;
                        }
                    }

                    WbbUtil.LogWarning("ExportSettings field not written: " + failed[i]);
                }

                return true;
            }
            catch (System.Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public static bool TryReadReferencePaths(int profileIndex, out List<string> paths, out string error)
        {
            paths = new List<string>();
            var access = OpenActiveProfile(out error, profileIndex);
            if (access == null)
                return false;
            try
            {
                FieldInfo field = access.profile.GetType().GetField("referencePaths", FieldFlags);
                if (field == null)
                    return true;
                var list = field.GetValue(access.profile) as IEnumerable;
                if (list == null)
                    return true;
                foreach (object entry in list)
                {
                    if (entry != null)
                        paths.Add(entry.ToString());
                }

                return true;
            }
            catch (System.Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public static bool TryWriteReferencePaths(int profileIndex, IList<string> paths, out string error)
        {
            var access = OpenActiveProfile(out error, profileIndex);
            if (access == null)
                return false;
            try
            {
                FieldInfo field = access.profile.GetType().GetField("referencePaths", FieldFlags);
                if (field == null)
                    return true;
                if (!field.FieldType.IsAssignableFrom(typeof(List<string>)))
                {
                    error = L("Runner.FieldMissing", "referencePaths");
                    return false;
                }

                field.SetValue(access.profile, paths == null ? new List<string>() : new List<string>(paths));
                Commit(access);
                return true;
            }
            catch (System.Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public static bool TryReadQuietState(out WbbUModQuietState state, out string error)
        {
            state = new WbbUModQuietState();
            var settings = Locate(out error);
            if (settings == null)
                return false;
            try
            {
                System.Type type = settings.GetType();
                FieldInfo clear = type.GetField("clearConsoleOnBuild", FieldFlags);
                FieldInfo show = type.GetField("showOutputDirectory", FieldFlags);
                FieldInfo level = type.GetField("logLevel", FieldFlags);
                if (clear == null || show == null)
                {
                    error = L("Runner.FieldMissing", clear == null ? "clearConsoleOnBuild" : "showOutputDirectory");
                    return false;
                }

                state.clearConsoleOnBuild = ToBool(clear.GetValue(settings));
                state.showOutputDirectory = ToBool(show.GetValue(settings));
                state.logLevel = level != null ? ToInt(level.GetValue(settings), LogLevelWarnings) : LogLevelWarnings;
                state.captured = true;
                return true;
            }
            catch (System.Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public static bool TryWriteQuietState(WbbUModQuietState state, out string error)
        {
            var settings = Locate(out error);
            if (settings == null)
                return false;
            try
            {
                System.Type type = settings.GetType();
                FieldInfo clear = type.GetField("clearConsoleOnBuild", FieldFlags);
                FieldInfo show = type.GetField("showOutputDirectory", FieldFlags);
                FieldInfo level = type.GetField("logLevel", FieldFlags);
                if (clear != null && clear.FieldType == typeof(bool))
                    clear.SetValue(settings, state.clearConsoleOnBuild);
                if (show != null && show.FieldType == typeof(bool))
                    show.SetValue(settings, state.showOutputDirectory);
                if (level != null)
                {
                    if (level.FieldType.IsEnum)
                        level.SetValue(settings, System.Enum.ToObject(level.FieldType, state.logLevel));
                    else if (level.FieldType == typeof(int))
                        level.SetValue(settings, state.logLevel);
                }

                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssetIfDirty(settings);
                return true;
            }
            catch (System.Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public static WbbUModQuietState QuietFor(WbbUModQuietState original)
        {
            var quiet = original;
            quiet.clearConsoleOnBuild = false;
            quiet.showOutputDirectory = false;
            if (quiet.logLevel < LogLevelWarnings)
                quiet.logLevel = LogLevelWarnings;
            return quiet;
        }

        private static bool ToBool(object value)
        {
            return value is bool && (bool)value;
        }

        private static int ToInt(object value, int fallback)
        {
            try
            {
                return value == null ? fallback : System.Convert.ToInt32(value);
            }
            catch (System.Exception)
            {
                return fallback;
            }
        }

        private static string ReadString(System.Type type, object instance, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, FieldFlags);
            if (field == null)
                return "";
            object value = field.GetValue(instance);
            return value == null ? "" : value.ToString();
        }

        private static UnityEngine.Object ReadObject(System.Type type, object instance, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, FieldFlags);
            if (field == null)
                return null;
            return field.GetValue(instance) as UnityEngine.Object;
        }

        private static bool WriteString(System.Type type, object instance, string fieldName, string value)
        {
            FieldInfo field = type.GetField(fieldName, FieldFlags);
            if (field == null)
                return false;
            if (field.FieldType != typeof(string))
                return false;
            field.SetValue(instance, value == null ? "" : value);
            return true;
        }

        private static bool WriteObject(System.Type type, object instance, string fieldName, UnityEngine.Object value)
        {
            FieldInfo field = type.GetField(fieldName, FieldFlags);
            if (field == null)
                return false;
            if (field.FieldType.IsValueType)
                return false;
            if (value != null && !field.FieldType.IsInstanceOfType(value))
                return false;
            field.SetValue(instance, value);
            return true;
        }

        public static bool InvokeBuild(out string error)
        {
            WbbBuildInvokeResult result = InvokeBuildWithResult();
            error = result.errorMessage;
            if (!result.invoked)
                return false;
            return !result.resultKnown || result.successful;
        }

        public static WbbBuildInvokeResult InvokeBuildWithResult()
        {
            var result = new WbbBuildInvokeResult();
            string locateError;
            ScriptableObject settings = Locate(out locateError);
            System.Type utilType = FindModToolsUtilType();
            MethodInfo startBuild = settings != null && utilType != null ? FindStartBuild(utilType, settings) : null;
            if (startBuild != null)
            {
                result.method = "StartBuild";
                ParameterInfo[] parameters = startBuild.GetParameters();
                var args = new object[parameters.Length];
                args[0] = settings;
                for (int i = 1; i < parameters.Length; i++)
                    args[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
                object returned;
                try
                {
                    returned = startBuild.Invoke(null, args);
                }
                catch (TargetInvocationException tie)
                {
                    System.Exception inner = tie.InnerException != null ? tie.InnerException : tie;
                    result.invoked = true;
                    result.resultKnown = true;
                    result.successful = false;
                    result.errorMessage = inner.Message;
                    result.exceptionMessage = inner.GetType().Name + ": " + inner.Message;
                    return result;
                }
                catch (System.Exception e)
                {
                    result.errorMessage = e.Message;
                    return result;
                }

                result.invoked = true;
                result.resultKnown = true;
                if (returned == null)
                {
                    result.successful = false;
                    result.errorMessage = L("Safety.BuildSettingsRejected");
                    return result;
                }

                ReadBuildResult(returned, result);
                return result;
            }

            try
            {
                result.method = "menu";
                if (EditorApplication.ExecuteMenuItem(BuildMenuPath))
                {
                    result.invoked = true;
                    result.resultKnown = false;
                    return result;
                }

                result.errorMessage = settings == null && !string.IsNullOrEmpty(locateError) ? locateError : (utilType == null ? L("Runner.NoUMod") : L("Runner.StartBuildMissing"));
            }
            catch (System.Exception e)
            {
                result.errorMessage = e.Message;
            }

            return result;
        }

        private static MethodInfo FindStartBuild(System.Type utilType, ScriptableObject settings)
        {
            try
            {
                MethodInfo[] methods = utilType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                MethodInfo best = null;
                for (int i = 0; i < methods.Length; i++)
                {
                    if (methods[i].Name != "StartBuild")
                        continue;
                    ParameterInfo[] parameters = methods[i].GetParameters();
                    if (parameters.Length == 0)
                        continue;
                    if (!parameters[0].ParameterType.IsInstanceOfType(settings))
                        continue;
                    bool restOptional = true;
                    for (int k = 1; k < parameters.Length; k++)
                    {
                        if (!parameters[k].IsOptional && parameters[k].ParameterType.IsValueType)
                        {
                            restOptional = false;
                            break;
                        }
                    }

                    if (!restOptional)
                        continue;
                    if (best == null || parameters.Length < best.GetParameters().Length)
                        best = methods[i];
                }

                return best;
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Could not inspect ModToolsUtil.StartBuild: " + e.Message);
                return null;
            }
        }

        private static void ReadBuildResult(object returned, WbbBuildInvokeResult result)
        {
            try
            {
                System.Type type = returned.GetType();
                object successful = ReadProperty(type, returned, "Successful");
                result.successful = successful is bool && (bool)successful;
                string message = ReadProperty(type, returned, "ErrorMessage") as string;
                result.errorMessage = message == null ? "" : message;
                var main = ReadProperty(type, returned, "ErrorMainException") as System.Exception;
                if (main != null)
                {
                    result.exceptionMessage = main.GetType().Name + ": " + main.Message;
                    if (string.IsNullOrEmpty(result.errorMessage))
                        result.errorMessage = main.Message;
                }

                var file = ReadProperty(type, returned, "BuiltModFile") as System.IO.FileInfo;
                if (file != null)
                    result.builtFile = WbbUtil.NormalizePath(file.FullName);
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Could not read ModBuildResult: " + e.Message);
                result.resultKnown = false;
            }
        }

        private static object ReadProperty(System.Type type, object instance, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            return property == null ? null : property.GetValue(instance, null);
        }
    }
}
