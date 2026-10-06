using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public enum WbbSettingsHealth
    {
        Ok = 0,
        Missing = 1,
        Broken = 2,
    }

    public static class WbbExportSettingsGuard
    {
        public const int KeepCount = 20;
        private const string BackupPrefix = "ExportSettings_";
        private const string BackupExtension = ".asset";
        private const string DefaultAssetPath = "Assets/ExportSettings.asset";
        private static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }

        public static string BackupDirectory
        {
            get
            {
                string dir = WbbBuildLog.LibraryDirectory + "/ExportSettingsBackups";
                try
                {
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                }
                catch (Exception e)
                {
                    WbbUtil.LogWarning("Could not create the build settings backup folder: " + e.Message);
                }

                return dir;
            }
        }

        public static string SettingsAssetPath()
        {
            string path = WbbExportSettings.LocatePath();
            return string.IsNullOrEmpty(path) ? DefaultAssetPath : path;
        }

        public static WbbSettingsHealth CheckHealth(out string detail)
        {
            detail = "";
            string assetPath = SettingsAssetPath();
            string absolute = WbbUtil.ToAbsolutePath(assetPath);
            try
            {
                if (!File.Exists(absolute))
                {
                    detail = "Build settings asset not found: " + assetPath;
                    return WbbSettingsHealth.Missing;
                }
            }
            catch (Exception e)
            {
                detail = e.Message;
                return WbbSettingsHealth.Missing;
            }

            string error;
            int count = WbbExportSettings.GetProfileCount(out error);
            if (count > 0)
                return WbbSettingsHealth.Ok;
            detail = count < 0 ? "Build settings asset could not be read: " + error : "Build settings have no profile.";
            return WbbSettingsHealth.Broken;
        }

        public static bool IsValidSettingsText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            if (text.IndexOf("MonoBehaviour:", StringComparison.Ordinal) < 0)
                return false;
            int profiles = text.IndexOf("exportProfiles:", StringComparison.Ordinal);
            if (profiles < 0)
                return false;
            string after = text.Substring(profiles + "exportProfiles:".Length);
            if (after.TrimStart().StartsWith("[]", StringComparison.Ordinal))
                return false;
            return after.IndexOf("modName:", StringComparison.Ordinal) >= 0;
        }

        public static bool Backup(out string backupPath, out string error)
        {
            backupPath = "";
            error = "";
            try
            {
                string source = WbbUtil.ToAbsolutePath(SettingsAssetPath());
                if (!File.Exists(source))
                {
                    error = "Build settings asset not found.";
                    return false;
                }

                string text = File.ReadAllText(source, Encoding.UTF8);
                if (!IsValidSettingsText(text))
                {
                    error = "Build settings look broken; not backing them up.";
                    return false;
                }

                List<string> existing = Backups();
                if (existing.Count > 0)
                {
                    string newest = File.ReadAllText(existing[0], Encoding.UTF8);
                    if (string.Equals(newest, text, StringComparison.Ordinal))
                    {
                        backupPath = existing[0];
                        return true;
                    }
                }

                string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                string target = BackupDirectory + "/" + BackupPrefix + stamp + BackupExtension;
                int n = 2;
                while (File.Exists(target))
                    target = BackupDirectory + "/" + BackupPrefix + stamp + "_" + n++ + BackupExtension;
                File.WriteAllText(target, text, new UTF8Encoding(false));
                backupPath = WbbUtil.NormalizePath(target);
                Prune();
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                WbbUtil.LogWarning("Build settings backup failed: " + e.Message);
                return false;
            }
        }

        public static List<string> Backups()
        {
            var result = new List<string>();
            try
            {
                var info = new DirectoryInfo(BackupDirectory);
                FileInfo[] files = info.GetFiles(BackupPrefix + "*" + BackupExtension);
                Array.Sort(files, (a, b) => string.CompareOrdinal(b.Name, a.Name));
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.IndexOf("_before-restore", StringComparison.Ordinal) >= 0)
                        continue;
                    result.Add(WbbUtil.NormalizePath(files[i].FullName));
                }
            }
            catch (Exception e)
            {
                WbbUtil.LogWarning("Could not list build settings backups: " + e.Message);
            }

            return result;
        }

        public static string NewestValidBackup()
        {
            List<string> backups = Backups();
            for (int i = 0; i < backups.Count; i++)
            {
                try
                {
                    if (IsValidSettingsText(File.ReadAllText(backups[i], Encoding.UTF8)))
                        return backups[i];
                }
                catch (Exception)
                {
                }
            }

            return "";
        }

        private static void Prune()
        {
            List<string> backups = Backups();
            for (int i = KeepCount; i < backups.Count; i++)
            {
                try
                {
                    File.Delete(backups[i]);
                }
                catch (Exception)
                {
                }
            }
        }

        public static bool Restore(string backupPath, out string error)
        {
            error = "";
            if (WbbBuildRunner.AnyRunning)
            {
                error = L("Runner.OtherBuildRunning");
                return false;
            }

            string normalized = WbbUtil.NormalizePath(backupPath);
            string dir = WbbUtil.NormalizePath(BackupDirectory);
            if (string.IsNullOrEmpty(normalized) || !normalized.StartsWith(dir + "/", StringComparison.OrdinalIgnoreCase))
            {
                error = L("Safety.SettingsRestoreBadFile");
                return false;
            }

            try
            {
                string text = File.ReadAllText(normalized, Encoding.UTF8);
                if (!IsValidSettingsText(text))
                {
                    error = L("Safety.SettingsRestoreBadFile");
                    return false;
                }

                string assetPath = SettingsAssetPath();
                string absolute = WbbUtil.ToAbsolutePath(assetPath);
                if (File.Exists(absolute))
                {
                    string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                    File.Copy(absolute, dir + "/" + BackupPrefix + stamp + "_before-restore" + BackupExtension, true);
                }

                File.WriteAllText(absolute, text, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                WbbUtil.Log("Restored build settings from backup: " + normalized);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                WbbUtil.LogError("Build settings restore failed: " + e.Message);
                return false;
            }
        }
    }
}
