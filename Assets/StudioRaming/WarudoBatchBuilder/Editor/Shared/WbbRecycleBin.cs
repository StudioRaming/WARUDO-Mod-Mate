using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace StudioRaming.WarudoBatchBuilder
{
    public enum WbbRecycleOutcome
    {
        RecycleBin = 0,
        TrashFolder = 1,
        Failed = 2,
    }

    public enum WbbRecycleFailure
    {
        None = 0,
        NotFound = 1,
        ReadOnly = 2,
        InUse = 3,
        AccessDenied = 4,
        Other = 5,
    }

    public static class WbbRecycleBin
    {
        public const string TrashFolderName = "_WBB_Trash";
        public const string TrashSuffix = ".wbbdeleted";
        private const uint FO_DELETE = 0x0003;
        private const ushort FOF_SILENT = 0x0004;
        private const ushort FOF_NOCONFIRMATION = 0x0010;
        private const ushort FOF_ALLOWUNDO = 0x0040;
        private const ushort FOF_NOERRORUI = 0x0400;
        private const ushort FOF_WANTNUKEWARNING = 0x4000;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public ushort fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT fileOp);
        public static WbbRecycleOutcome Recycle(string absoluteFilePath, string trashRoot, out WbbRecycleFailure failure, out string detail)
        {
            failure = WbbRecycleFailure.None;
            detail = "";
            string path = WbbUtil.NormalizePath(absoluteFilePath);
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    failure = WbbRecycleFailure.NotFound;
                    return WbbRecycleOutcome.Failed;
                }

                if ((File.GetAttributes(path) & FileAttributes.ReadOnly) != 0)
                {
                    failure = WbbRecycleFailure.ReadOnly;
                    return WbbRecycleOutcome.Failed;
                }
            }
            catch (Exception e)
            {
                failure = Classify(e);
                detail = e.Message;
                return WbbRecycleOutcome.Failed;
            }

            if (IsWindows() && TryShellRecycle(path, out detail))
                return WbbRecycleOutcome.RecycleBin;
            string moveDetail;
            if (TryMoveToTrash(path, trashRoot, out failure, out moveDetail))
                return WbbRecycleOutcome.TrashFolder;
            detail = string.IsNullOrEmpty(detail) ? moveDetail : detail + " / " + moveDetail;
            return WbbRecycleOutcome.Failed;
        }

        private static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        private static bool TryShellRecycle(string path, out string detail)
        {
            detail = "";
            try
            {
                var op = new SHFILEOPSTRUCT
                {
                    hwnd = IntPtr.Zero,
                    wFunc = FO_DELETE,
                    pFrom = path.Replace('/', '\\') + "\0",
                    pTo = null,
                    fFlags = (ushort)(FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT | FOF_NOERRORUI | FOF_WANTNUKEWARNING),
                };
                int result = SHFileOperation(ref op);
                if (result == 0 && !op.fAnyOperationsAborted && !File.Exists(path))
                    return true;
                detail = "Recycle Bin refused (code 0x" + result.ToString("X", CultureInfo.InvariantCulture) + (op.fAnyOperationsAborted ? ", aborted" : "") + ")";
                return false;
            }
            catch (Exception e)
            {
                detail = "Recycle Bin unavailable: " + e.Message;
                return false;
            }
        }

        private static bool TryMoveToTrash(string path, string trashRoot, out WbbRecycleFailure failure, out string detail)
        {
            failure = WbbRecycleFailure.None;
            detail = "";
            try
            {
                string root = WbbUtil.NormalizePath(string.IsNullOrEmpty(trashRoot) ? Path.GetDirectoryName(path) : trashRoot);
                string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                string folder = root + "/" + TrashFolderName + "/" + stamp;
                Directory.CreateDirectory(folder);
                string target = folder + "/" + Path.GetFileName(path) + TrashSuffix;
                int n = 2;
                while (File.Exists(target))
                    target = folder + "/" + Path.GetFileName(path) + "_" + n++ + TrashSuffix;
                File.Move(path, target);
                WbbUtil.Log("Moved to the WBB trash folder instead of deleting: " + path + " -> " + target);
                return true;
            }
            catch (Exception e)
            {
                failure = Classify(e);
                detail = e.Message;
                return false;
            }
        }

        private static WbbRecycleFailure Classify(Exception e)
        {
            if (e is FileNotFoundException || e is DirectoryNotFoundException)
                return WbbRecycleFailure.NotFound;
            if (e is UnauthorizedAccessException)
                return WbbRecycleFailure.AccessDenied;
            if (e is IOException)
                return WbbRecycleFailure.InUse;
            return WbbRecycleFailure.Other;
        }

        public static string Describe(WbbRecycleFailure failure, string detail)
        {
            switch (failure)
            {
                case WbbRecycleFailure.NotFound:
                    return WbbLang.T("Safety.DeleteNotFound");
                case WbbRecycleFailure.ReadOnly:
                    return WbbLang.T("Safety.DeleteReadOnly");
                case WbbRecycleFailure.InUse:
                    return WbbLang.T("Safety.DeleteInUse");
                case WbbRecycleFailure.AccessDenied:
                    return WbbLang.T("Safety.DeleteAccessDenied");
                default:
                    return string.IsNullOrEmpty(detail) ? WbbLang.T("Safety.DeleteFailed") : WbbLang.T("Safety.DeleteFailed") + " (" + detail + ")";
            }
        }
    }
}
