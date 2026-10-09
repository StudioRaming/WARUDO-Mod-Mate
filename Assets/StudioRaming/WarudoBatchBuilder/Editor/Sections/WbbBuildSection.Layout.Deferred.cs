using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private System.Action focusDeferred;
        private int focusDeferredSerial;
        private int guiEventSerial;
        private void AfterFocusRelease(System.Action apply)
        {
            GUI.FocusControl(null);
            focusDeferred += apply;
            focusDeferredSerial = guiEventSerial;
            RequestRepaint();
        }

        private void RunFocusDeferred()
        {
            if (focusDeferred == null || guiEventSerial == focusDeferredSerial)
                return;
            EventType type = Event.current.type;
            if (type == EventType.ExecuteCommand || type == EventType.ValidateCommand || type == EventType.Used)
                return;
            System.Action run = focusDeferred;
            focusDeferred = null;
            run();
        }

        private void ProcessPending()
        {
            if (Event.current.type == EventType.Layout)
                return;
            RunFocusDeferred();
            if (!string.IsNullOrEmpty(pendingHideGuid))
            {
                string guid = pendingHideGuid;
                pendingHideGuid = "";
                SetItemHidden(guid, true);
            }

            if (!string.IsNullOrEmpty(pendingUnhideGuid))
            {
                string guid = pendingUnhideGuid;
                pendingUnhideGuid = "";
                SetItemHidden(guid, false);
            }

            if (!string.IsNullOrEmpty(pendingPinGuid))
            {
                string guid = pendingPinGuid;
                pendingPinGuid = "";
                int index = IndexOfGuid(guid);
                if (index >= 0)
                    WbbListSettings.SetPinned(guid, items[index].reason != ShowReason.Pinned);
                RequestRepaint();
            }

            if (!string.IsNullOrEmpty(pendingSceneRebuildGuid))
            {
                string guid = pendingSceneRebuildGuid;
                pendingSceneRebuildGuid = "";
                int index = IndexOfGuid(guid);
                if (index >= 0 && items[index].record != null)
                    WbbQuickBuild.QueueRebuild(items[index].record);
            }

            if (!string.IsNullOrEmpty(pendingOpenThumbGuid))
            {
                string guid = pendingOpenThumbGuid;
                pendingOpenThumbGuid = "";
                OpenThumbnail(guid);
            }

            if (!string.IsNullOrEmpty(pendingOpenOutputGuid))
            {
                string guid = pendingOpenOutputGuid;
                pendingOpenOutputGuid = "";
                OpenItemOutputFolder(guid);
            }

            if (pendingPickOutput)
            {
                pendingPickOutput = false;
                AfterFocusRelease(PickOutputFolder);
            }

            if (pendingPickStaging)
            {
                pendingPickStaging = false;
                PickStagingFolder();
            }

            if (pendingPackageExport)
            {
                pendingPackageExport = false;
                SendSelectionToPackageTab();
            }

            if (pendingOutputMode >= 0)
            {
                int mode = pendingOutputMode;
                pendingOutputMode = -1;
                if (mode != job.outputMode)
                {
                    job.outputMode = Mathf.Clamp(mode, 0, OutputModeCount - 1);
                    WbbPrefs.SetInt(Pfx + "OutputMode", job.outputMode);
                    preflightDirty = true;
                    runContent = null;
                    runContentCount = -1;
                    RequestRepaint();
                }
            }

            if (!string.IsNullOrEmpty(pendingRenameGuid))
            {
                string guid = pendingRenameGuid;
                bool asset = pendingRenameAsset;
                pendingRenameGuid = "";
                pendingRenameAsset = false;
                BeginRename(guid, asset);
            }

            if (!string.IsNullOrEmpty(pendingResetNameGuid))
            {
                string guid = pendingResetNameGuid;
                pendingResetNameGuid = "";
                WbbUtil.SetManualName(job, guid, "");
                SaveManualNames();
                namesDirty = true;
                RequestRepaint();
            }

            if (pendingSavePreset)
            {
                pendingSavePreset = false;
                SavePreset();
            }

            if (!string.IsNullOrEmpty(pendingApplyPresetId))
            {
                string id = pendingApplyPresetId;
                pendingApplyPresetId = "";
                AfterFocusRelease(delegate
                {
                    ApplyPreset(id);
                });
            }

            if (!string.IsNullOrEmpty(pendingDeletePresetId))
            {
                string id = pendingDeletePresetId;
                pendingDeletePresetId = "";
                DeletePreset(id);
            }

            if (pendingResult)
            {
                pendingResult = false;
                ShowResult();
            }
        }

        private void ProcessDeferredWork()
        {
            if (IsBusy)
            {
                pendingBuild = false;
                pendingThumbBatch = false;
                pendingThumbGuid = "";
                return;
            }

            if (pendingBuild)
            {
                pendingBuild = false;
                StartBuild();
                return;
            }

            if (!string.IsNullOrEmpty(pendingThumbGuid))
            {
                string guid = pendingThumbGuid;
                pendingThumbGuid = "";
                GenerateThumbnail(guid);
                return;
            }

            if (pendingThumbBatch)
            {
                pendingThumbBatch = false;
                GenerateThumbnailBatch();
            }
        }
    }
}
