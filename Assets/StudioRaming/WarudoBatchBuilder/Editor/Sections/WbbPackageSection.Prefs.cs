using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private void LoadExcluded()
        {
            excluded.Clear();
            string raw = EditorPrefs.GetString(ExcludedKey, "");
            if (string.IsNullOrEmpty(raw))
                return;
            string[] parts = raw.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                string guid = parts[i] == null ? "" : parts[i].Trim();
                if (guid.Length > 0)
                    excluded.Add(guid);
            }
        }

        private void SaveExcluded()
        {
            if (excluded.Count == 0)
            {
                EditorPrefs.SetString(ExcludedKey, "");
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (string guid in excluded)
            {
                if (sb.Length > 0)
                    sb.Append(';');
                sb.Append(guid);
            }

            EditorPrefs.SetString(ExcludedKey, sb.ToString());
        }

        private void LoadCollapsed()
        {
            collapsed.Clear();
            string raw = EditorPrefs.GetString(CollapsedKey, "");
            if (string.IsNullOrEmpty(raw))
                return;
            string[] parts = raw.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                string path = parts[i] == null ? "" : parts[i].Trim();
                if (path.Length > 0)
                    collapsed.Add(path);
            }
        }

        private void SaveCollapsed()
        {
            if (collapsed.Count == 0)
            {
                EditorPrefs.SetString(CollapsedKey, "");
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (string path in collapsed)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append(path);
            }

            EditorPrefs.SetString(CollapsedKey, sb.ToString());
        }

        private void LoadTargets()
        {
            if (Embedded)
                return;
            targets.Clear();
            string raw = EditorPrefs.GetString(TargetsKey, "");
            if (string.IsNullOrEmpty(raw))
                return;
            string[] parts = raw.Split('\n');
            for (int i = 0; i < parts.Length; i++)
                AddTargetPath(parts[i]);
        }

        private void SaveTargets()
        {
            if (Embedded)
                return;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < targets.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append(targets[i].prefabPath);
            }

            EditorPrefs.SetString(TargetsKey, sb.ToString());
        }

        private static string DefaultOutputDir()
        {
            try
            {
                DirectoryInfo project = Directory.GetParent(Application.dataPath);
                if (project == null)
                    return "";
                DirectoryInfo parent = project.Parent;
                return WbbUtil.NormalizePath(parent != null ? parent.FullName : project.FullName);
            }
            catch (System.Exception)
            {
                return "";
            }
        }
    }
}
