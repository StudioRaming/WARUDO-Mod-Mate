using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Compilation;

namespace StudioRaming.WarudoBatchBuilder
{
    public sealed class WbbScriptFolderScan
    {
        private const int MaxListed = 20;
        private static readonly string[] UnsupportedExtensions =
        {
            ".js",
            ".unitypackage",
            ".mb",
            ".rtf",
            ".giparams"
        };
        private static readonly string[] ReservedFolderNames =
        {
            "UMod",
            "UModExporter",
            "UModLite"
        };
        public int runtimeScripts;
        public readonly List<string> asmdefScripts = new List<string>();
        public readonly List<string> dlls = new List<string>();
        public readonly List<string> unsupported = new List<string>();
        public bool failed;
        public static WbbScriptFolderScan Run(string assetFolder)
        {
            var scan = new WbbScriptFolderScan();
            string root = WbbUtil.NormalizePath(assetFolder);
            if (string.IsNullOrEmpty(root))
                return scan;
            string absoluteRoot = WbbUtil.ToAbsolutePath(root);
            string[] files;
            try
            {
                if (!Directory.Exists(absoluteRoot))
                    return scan;
                files = Directory.GetFiles(absoluteRoot, "*", SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                scan.failed = true;
                WbbUtil.LogWarning("Could not inspect the script folder " + root + ": " + e.Message);
                return scan;
            }

            for (int i = 0; i < files.Length; i++)
            {
                string absolute = WbbUtil.NormalizePath(files[i]);
                if (absolute.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;
                string relative = absolute.Length > absoluteRoot.Length ? absolute.Substring(absoluteRoot.Length).TrimStart('/') : Path.GetFileName(absolute);
                string assetPath = root + "/" + relative;
                string extension = Path.GetExtension(absolute);
                if (InReservedFolder(relative))
                {
                    AddLimited(scan.unsupported, assetPath);
                    continue;
                }

                if (string.Equals(extension, ".cs", StringComparison.OrdinalIgnoreCase))
                {
                    if (InEditorFolder(relative))
                        continue;
                    scan.runtimeScripts++;
                    string asmdef = null;
                    try
                    {
                        asmdef = CompilationPipeline.GetAssemblyDefinitionFilePathFromScriptPath(assetPath);
                    }
                    catch (Exception)
                    {
                    }

                    if (!string.IsNullOrEmpty(asmdef))
                        AddLimited(scan.asmdefScripts, assetPath);
                    continue;
                }

                if (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
                {
                    AddLimited(scan.dlls, assetPath);
                    continue;
                }

                if (string.IsNullOrEmpty(extension) || IsUnsupportedExtension(extension))
                    AddLimited(scan.unsupported, assetPath);
            }

            return scan;
        }

        private static bool IsUnsupportedExtension(string extension)
        {
            for (int i = 0; i < UnsupportedExtensions.Length; i++)
            {
                if (string.Equals(extension, UnsupportedExtensions[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool InEditorFolder(string relative)
        {
            string[] parts = relative.Split('/');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (string.Equals(parts[i], "Editor", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool InReservedFolder(string relative)
        {
            string[] parts = relative.Split('/');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                for (int k = 0; k < ReservedFolderNames.Length; k++)
                {
                    if (string.Equals(parts[i], ReservedFolderNames[k], StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        private static void AddLimited(List<string> list, string value)
        {
            if (list.Count < MaxListed)
                list.Add(value);
        }
    }
}
