namespace StudioRaming.WarudoBatchBuilder
{
    public class WbbPreflightIssueEx : WbbPreflightIssue
    {
        public string code = "";
        public string actionPath = "";
        public WbbPreflightIssueEx(bool blocking, string message, string detail, string code, string actionPath = "") : base(blocking, message, detail)
        {
            this.code = code ?? "";
            this.actionPath = actionPath ?? "";
        }
    }

    public static class WbbPreflightCodes
    {
        public const string WarudoNotFound = "warudo-not-found";
        public const string UntitledScene = "untitled-scene";
        public const string PrefabStageDirty = "prefab-stage-dirty";
        public const string SettingsBroken = "settings-broken";
        public const string EnvironmentUnsupported = "environment-unsupported";
        public const string VersionInvalid = "version-invalid";
        public const string VersionAutoFixed = "version-autofixed";
        public const string AsmdefScripts = "asmdef-scripts";
        public const string DllInScriptFolder = "dll-in-script-folder";
        public const string UnsupportedFiles = "unsupported-files";
        public const string UModBusy = "umod-busy";
        public const string JournalPending = "journal-pending";
        public const string DatabaseLoadFailed = "database-load-failed";
    }
}
