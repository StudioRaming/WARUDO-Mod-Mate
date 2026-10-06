using System;

namespace StudioRaming.WarudoBatchBuilder
{
    public struct WbbGatherOptions
    {
        public bool textures;
        public bool materials;
        public bool models;
        public bool prefabs;
        public static WbbGatherOptions None
        {
            get
            {
                return new WbbGatherOptions();
            }
        }

        public bool Any
        {
            get
            {
                return textures || materials || models || prefabs;
            }
        }

        public bool IsOn(WbbAssetCategory category)
        {
            switch (category)
            {
                case WbbAssetCategory.Tex:
                    return textures;
                case WbbAssetCategory.Mat:
                    return materials;
                case WbbAssetCategory.Fbx:
                    return models;
                case WbbAssetCategory.Prefab:
                    return prefabs;
                default:
                    return false;
            }
        }

        public static bool IsGatherableCategory(WbbAssetCategory category)
        {
            return category == WbbAssetCategory.Tex || category == WbbAssetCategory.Mat || category == WbbAssetCategory.Fbx || category == WbbAssetCategory.Prefab;
        }

        public static string FolderFor(WbbAssetCategory category)
        {
            switch (category)
            {
                case WbbAssetCategory.Tex:
                    return "Textures";
                case WbbAssetCategory.Mat:
                    return "Materials";
                case WbbAssetCategory.Fbx:
                    return "FBX";
                case WbbAssetCategory.Prefab:
                    return "Prefabs";
                default:
                    return string.Empty;
            }
        }

        public static bool IsPathBound(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return true;
            string path = WbbUtil.NormalizePath(assetPath);
            return path.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool TryGetFolder(string assetPath, WbbAssetCategory category, out string folder)
        {
            folder = string.Empty;
            if (!IsOn(category))
                return false;
            if (IsPathBound(assetPath))
                return false;
            folder = FolderFor(category);
            return !string.IsNullOrEmpty(folder);
        }
    }
}
