using System;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    // EditorPrefs scoped to the Unity project. EditorPrefs are shared by every project on the PC, so a key written in one
    // project (prefab folder, output folder, mod author, package targets) used to show up in all the others.
    // A key that starts with WbbUtil.PrefPrefix gets the project id after the prefix.
    //
    // One-time migration: a project that reads a key it has no value for copies the value that older versions wrote under
    // the shared key. The shared keys are never changed or deleted, so no project can take the old values away from
    // another one, and a value that does not fit a project (a folder that is not in it) is dropped by the code that reads
    // it. A key that a project deletes is marked, so the shared value is not copied back (see DeleteKey).
    // Values that belong to one project only are never migrated (GetString with migrate=false): the quick-build crash
    // recovery marker and the list of redistribution-risk files the user chose to include again in the Package tab.
    public static class WbbPrefs
    {
        private const string DeletedSuffix = ".deleted";
        private static string projectId;
        public static string ProjectId
        {
            get
            {
                if (projectId == null)
                {
                    string path = WbbUtil.NormalizePath(Application.dataPath).ToLowerInvariant();
                    using (SHA1 sha = SHA1.Create())
                        projectId = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(path)), 0, 4).Replace("-", "").ToLowerInvariant();
                }

                return projectId;
            }
        }

        public static string Scoped(string key)
        {
            if (string.IsNullOrEmpty(key) || !key.StartsWith(WbbUtil.PrefPrefix, StringComparison.Ordinal))
                return key;
            return WbbUtil.PrefPrefix + "P" + ProjectId + "." + key.Substring(WbbUtil.PrefPrefix.Length);
        }

        public static bool HasKey(string key)
        {
            string scoped = Scoped(key);
            return EditorPrefs.HasKey(scoped) || HasLegacy(key);
        }

        public static void DeleteKey(string key)
        {
            string scoped = Scoped(key);
            EditorPrefs.DeleteKey(scoped);
            // The shared value stays where it is; remember that this project cleared the key, or the next read copies it back.
            if (HasLegacy(key))
                EditorPrefs.SetBool(scoped + DeletedSuffix, true);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            return GetString(key, defaultValue, true);
        }

        // `migrate` is false for state that belongs to one run in one project (a crash-recovery marker): such a value is
        // never taken over from the shared key.
        public static string GetString(string key, string defaultValue, bool migrate)
        {
            string scoped = Scoped(key);
            if (EditorPrefs.HasKey(scoped))
                return EditorPrefs.GetString(scoped, defaultValue);
            if (!migrate || !HasLegacy(key))
                return defaultValue;
            string value = EditorPrefs.GetString(key, defaultValue);
            EditorPrefs.SetString(scoped, value);
            return value;
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            string scoped = Scoped(key);
            if (EditorPrefs.HasKey(scoped))
                return EditorPrefs.GetBool(scoped, defaultValue);
            if (!HasLegacy(key))
                return defaultValue;
            bool value = EditorPrefs.GetBool(key, defaultValue);
            EditorPrefs.SetBool(scoped, value);
            return value;
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            string scoped = Scoped(key);
            if (EditorPrefs.HasKey(scoped))
                return EditorPrefs.GetInt(scoped, defaultValue);
            if (!HasLegacy(key))
                return defaultValue;
            int value = EditorPrefs.GetInt(key, defaultValue);
            EditorPrefs.SetInt(scoped, value);
            return value;
        }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            string scoped = Scoped(key);
            if (EditorPrefs.HasKey(scoped))
                return EditorPrefs.GetFloat(scoped, defaultValue);
            if (!HasLegacy(key))
                return defaultValue;
            float value = EditorPrefs.GetFloat(key, defaultValue);
            EditorPrefs.SetFloat(scoped, value);
            return value;
        }

        public static void SetString(string key, string value)
        {
            EditorPrefs.SetString(Scoped(key), value);
        }

        public static void SetBool(string key, bool value)
        {
            EditorPrefs.SetBool(Scoped(key), value);
        }

        public static void SetInt(string key, int value)
        {
            EditorPrefs.SetInt(Scoped(key), value);
        }

        public static void SetFloat(string key, float value)
        {
            EditorPrefs.SetFloat(Scoped(key), value);
        }

        // True when the shared key holds a value that this project has not cleared (see the class comment).
        private static bool HasLegacy(string key)
        {
            if (string.IsNullOrEmpty(key) || !key.StartsWith(WbbUtil.PrefPrefix, StringComparison.Ordinal))
                return false;
            return EditorPrefs.HasKey(key) && !EditorPrefs.HasKey(Scoped(key) + DeletedSuffix);
        }
    }
}
