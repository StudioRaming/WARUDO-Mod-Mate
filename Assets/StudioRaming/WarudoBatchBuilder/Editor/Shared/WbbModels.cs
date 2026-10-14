using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public enum WbbExportType
    {
        Prop = 0,
        Character = 1,
        Environment = 2,
    }

    public enum WbbOutputMode
    {
        WarudoMod = 0,
        UnityPackage = 1,
    }

    public enum WbbBuildStatus
    {
        Pending = 0,
        Running = 1,
        Success = 2,
        Failed = 3,
        Canceled = 4,
        Skipped = 5,
    }

    [System.Serializable]
    public class WbbJob
    {
        public string id = "";
        public string name = "";
        public string buildFolderGuid = "";
        public string buildFolderPath = "";
        public bool manualScriptFolder = false;
        public string prefabFolderGuid = "";
        public string prefabFolderPath = "";
        public int exportType = (int)WbbExportType.Prop;
        public int outputMode = (int)WbbOutputMode.WarudoMod;
        public string modVersion = "1.0.0";
        public string modAuthor = "";
        public string modDescription = "";
        public string outputDirectory = "";
        public bool appendDateSuffix = false;
        public bool useFolderNamePrefix = true;
        public string namePrefix = "";
        public bool recurseSubFolders = true;
        public List<string> excludedPrefabGuids = new List<string>();
        public string lastUsedIso = "";
        public bool overwriteExisting = true;
        public List<string> buildFolderGuidsByType = new List<string>();
        public List<string> buildFolderPathsByType = new List<string>();
        public List<string> manualNameGuids = new List<string>();
        public List<string> manualNames = new List<string>();
        public bool stripVrcComponents = true;
    }

    [System.Serializable]
    public class WbbRecord
    {
        public string id = "";
        public string startedIso = "";
        public string finishedIso = "";
        public double durationSec = 0d;
        public string modName = "";
        public int exportType = (int)WbbExportType.Prop;
        public string modVersion = "";
        public string modAuthor = "";
        public string prefabGuid = "";
        public string prefabPath = "";
        public string prefabName = "";
        public string outputDirectory = "";
        public string outputFilePath = "";
        public long outputSizeBytes = 0L;
        public int status = (int)WbbBuildStatus.Pending;
        public string message = "";
        public string jobId = "";
        public string jobName = "";
        public string sourceSignature = "";
        public string sourceObjectId = "";
        public string sourceScenePath = "";
        public string sourceHierarchyPath = "";
        public WbbBuildStatus Status
        {
            get
            {
                return (WbbBuildStatus)status;
            }
        }

        public WbbExportType ExportType
        {
            get
            {
                return (WbbExportType)exportType;
            }
        }

        public bool IsSceneSource
        {
            get
            {
                return !string.IsNullOrEmpty(sourceHierarchyPath);
            }
        }

        public string fixupReport = "";
        public string buildLogPath = "";
    }

    public class WbbBuildItem
    {
        public string prefabGuid = "";
        public string prefabPath = "";
        public string prefabName = "";
        public string modName = "";
        public UnityEngine.Texture2D icon;
        public WbbBuildStatus status = WbbBuildStatus.Pending;
        public string message = "";
        public double durationSec = 0d;
        public string sourceObjectId = "";
        public string sourceScenePath = "";
        public string sourceHierarchyPath = "";
        public bool disposableCopy = false;
        public string fixupReport = "";
        // Set by the runner when the item succeeded: the same-name output existed before and was replaced, and the build packed no runtime sources.
        public bool replacedExistingOutput = false;
        public bool packedRuntimeScripts = false;
    }

    public class WbbModFile
    {
        public string filePath = "";
        public string fileName = "";
        public string extension = "";
        public string relativeFolder = "";
        public long sizeBytes = 0L;
        public System.DateTime lastWrite = System.DateTime.MinValue;
        public int exportType = (int)WbbExportType.Prop;
        public WbbRecord matchedRecord;
    }

    public class WbbProfileSnapshot
    {
        public string modName = "";
        public string modAuthor = "";
        public string modVersion = "";
        public string modDescription = "";
        public string modAssetPath = "";
        public string modExportPath = "";
        public UnityEngine.Object modIcon;
        public WbbProfileSnapshot Clone()
        {
            return new WbbProfileSnapshot
            {
                modName = modName,
                modAuthor = modAuthor,
                modVersion = modVersion,
                modDescription = modDescription,
                modAssetPath = modAssetPath,
                modExportPath = modExportPath,
                modIcon = modIcon,
            };
        }
    }

    public class WbbPreflightIssue
    {
        public bool blocking;
        public string message = "";
        public string detail = "";
        public WbbPreflightIssue(bool blocking, string message, string detail)
        {
            this.blocking = blocking;
            this.message = message;
            this.detail = detail == null ? "" : detail;
        }
    }
}
