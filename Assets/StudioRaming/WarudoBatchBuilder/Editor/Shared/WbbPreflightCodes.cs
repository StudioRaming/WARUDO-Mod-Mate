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
        public const string WarudoManagedMissing = "warudo-managed-missing";
        public const string WarudoManagedSkipped = "warudo-managed-skipped";
        public const string UntitledScene = "untitled-scene";
        public const string PrefabStageDirty = "prefab-stage-dirty";
        public const string SettingsBroken = "settings-broken";
        public const string EnvironmentUnsupported = "environment-unsupported";
        public const string VersionInvalid = "version-invalid";
        public const string VersionAutoFixed = "version-autofixed";
        public const string AsmdefNotCompiled = "asmdef-not-compiled";
        public const string AsmdefReference = "asmdef-reference";
        public const string AsmdefPrecompiled = "asmdef-precompiled";
        public const string AsmdefHostUnknown = "asmdef-host-unknown";
        public const string AsmdefNameCollidesHost = "asmdef-name-collides-host";
        public const string PlainScriptsUnresolved = "plain-scripts-unresolved";
        public const string SettingsMissing = "settings-missing";
        public const string SettingsNoProfile = "settings-no-profile";
        public const string DllInScriptFolder = "dll-in-script-folder";
        public const string UnsupportedFiles = "unsupported-files";
        public const string UModBusy = "umod-busy";
        public const string JournalPending = "journal-pending";
        public const string DatabaseLoadFailed = "database-load-failed";
    }
}
