namespace StudioRaming.WarudoBatchBuilder
{
    public class WbbPackageAsset
    {
        public string guid;
        public string assetPath;
        public string fileName;
        public string extension;
        public WbbAssetCategory category;
        public long sizeBytes;
        public bool isRoot;
        public string keepStructureRoot = "";
        public bool redistributionRisk;
        public bool customScript;
    }

    public enum WbbAssetCategory
    {
        Prefab = 0,
        Mat = 1,
        Tex = 2,
        Anim = 3,
        Fbx = 4,
        Script = 5,
        Shader = 6,
        Audio = 7,
        Asset = 8,
        Misc = 9,
    }
}
