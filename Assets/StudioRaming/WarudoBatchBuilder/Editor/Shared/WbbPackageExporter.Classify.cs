using System;
using System.Collections.Generic;
using System.IO;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbPackageExporter
    {
        private static readonly Dictionary<string, WbbAssetCategory> ExtensionMap = new Dictionary<string, WbbAssetCategory>(StringComparer.OrdinalIgnoreCase)
        {
            {
                ".prefab",
                WbbAssetCategory.Prefab
            },
            {
                ".mat",
                WbbAssetCategory.Mat
            },
            {
                ".physicmaterial",
                WbbAssetCategory.Mat
            },
            {
                ".physicsmaterial2d",
                WbbAssetCategory.Mat
            },
            {
                ".png",
                WbbAssetCategory.Tex
            },
            {
                ".jpg",
                WbbAssetCategory.Tex
            },
            {
                ".jpeg",
                WbbAssetCategory.Tex
            },
            {
                ".tga",
                WbbAssetCategory.Tex
            },
            {
                ".psd",
                WbbAssetCategory.Tex
            },
            {
                ".tif",
                WbbAssetCategory.Tex
            },
            {
                ".tiff",
                WbbAssetCategory.Tex
            },
            {
                ".exr",
                WbbAssetCategory.Tex
            },
            {
                ".bmp",
                WbbAssetCategory.Tex
            },
            {
                ".gif",
                WbbAssetCategory.Tex
            },
            {
                ".hdr",
                WbbAssetCategory.Tex
            },
            {
                ".anim",
                WbbAssetCategory.Anim
            },
            {
                ".controller",
                WbbAssetCategory.Anim
            },
            {
                ".overridecontroller",
                WbbAssetCategory.Anim
            },
            {
                ".mask",
                WbbAssetCategory.Anim
            },
            {
                ".playable",
                WbbAssetCategory.Anim
            },
            {
                ".fbx",
                WbbAssetCategory.Fbx
            },
            {
                ".obj",
                WbbAssetCategory.Fbx
            },
            {
                ".blend",
                WbbAssetCategory.Fbx
            },
            {
                ".dae",
                WbbAssetCategory.Fbx
            },
            {
                ".3ds",
                WbbAssetCategory.Fbx
            },
            {
                ".max",
                WbbAssetCategory.Fbx
            },
            {
                ".ma",
                WbbAssetCategory.Fbx
            },
            {
                ".mb",
                WbbAssetCategory.Fbx
            },
            {
                ".cs",
                WbbAssetCategory.Script
            },
            {
                ".dll",
                WbbAssetCategory.Script
            },
            {
                ".asmdef",
                WbbAssetCategory.Script
            },
            {
                ".asmref",
                WbbAssetCategory.Script
            },
            {
                ".shader",
                WbbAssetCategory.Shader
            },
            {
                ".shadergraph",
                WbbAssetCategory.Shader
            },
            {
                ".cginc",
                WbbAssetCategory.Shader
            },
            {
                ".hlsl",
                WbbAssetCategory.Shader
            },
            {
                ".glslinc",
                WbbAssetCategory.Shader
            },
            {
                ".compute",
                WbbAssetCategory.Shader
            },
            {
                ".shadervariants",
                WbbAssetCategory.Shader
            },
            {
                ".wav",
                WbbAssetCategory.Audio
            },
            {
                ".mp3",
                WbbAssetCategory.Audio
            },
            {
                ".ogg",
                WbbAssetCategory.Audio
            },
            {
                ".aiff",
                WbbAssetCategory.Audio
            },
            {
                ".aif",
                WbbAssetCategory.Audio
            },
            {
                ".flac",
                WbbAssetCategory.Audio
            },
            {
                ".asset",
                WbbAssetCategory.Asset
            },
            {
                ".preset",
                WbbAssetCategory.Asset
            },
            {
                ".mixer",
                WbbAssetCategory.Asset
            },
            {
                ".terrainlayer",
                WbbAssetCategory.Asset
            },
            {
                ".fontsettings",
                WbbAssetCategory.Asset
            },
            {
                ".ttf",
                WbbAssetCategory.Asset
            },
            {
                ".otf",
                WbbAssetCategory.Asset
            },
        };
        public static string FolderName(WbbAssetCategory category)
        {
            switch (category)
            {
                case WbbAssetCategory.Prefab:
                    return "Prefab";
                case WbbAssetCategory.Mat:
                    return "Mat";
                case WbbAssetCategory.Tex:
                    return "Tex";
                case WbbAssetCategory.Anim:
                    return "Anim";
                case WbbAssetCategory.Fbx:
                    return "Fbx";
                case WbbAssetCategory.Script:
                    return "Script";
                case WbbAssetCategory.Shader:
                    return "Shader";
                case WbbAssetCategory.Audio:
                    return "Audio";
                case WbbAssetCategory.Asset:
                    return "Asset";
                default:
                    return "Misc";
            }
        }

        public static WbbAssetCategory Classify(string assetPath)
        {
            string ext = GetExtension(assetPath);
            if (string.IsNullOrEmpty(ext))
                return WbbAssetCategory.Misc;
            WbbAssetCategory category;
            if (ExtensionMap.TryGetValue(ext, out category))
                return category;
            return WbbAssetCategory.Misc;
        }

        public static string GetExtension(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return string.Empty;
            string ext;
            try
            {
                ext = Path.GetExtension(assetPath);
            }
            catch
            {
                return string.Empty;
            }

            return string.IsNullOrEmpty(ext) ? string.Empty : ext.ToLowerInvariant();
        }
    }
}
