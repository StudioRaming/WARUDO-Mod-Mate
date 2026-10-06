using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private void RunExport()
        {
            if (targets.Count == 0)
            {
                EditorUtility.DisplayDialog(L("Package.ExportTitle"), L("Package.ExportNoTargets"), L("Common.Ok"));
                return;
            }

            string dir = WbbUtil.NormalizePath(outputDir);
            if (string.IsNullOrEmpty(dir))
            {
                EditorUtility.DisplayDialog(L("Package.ExportTitle"), L("Package.OutputEmpty"), L("Common.Ok"));
                return;
            }

            if (!DirectoryExists(dir))
            {
                if (!EditorUtility.DisplayDialog(L("Package.ExportTitle"), L("Package.OutputMissing", dir), L("Package.OutputCreateYes"), L("Common.Cancel")))
                    return;
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog(L("Package.ExportTitle"), L("Package.OutputCreateFailed", e.Message), L("Common.Ok"));
                    return;
                }
            }

            var plans = new List<Plan>();
            var usedNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            var existingNames = new List<string>();
            int existing = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                Target target = targets[i];
                if (target.missing)
                    continue;
                string baseName = target.baseName;
                if (!usedNames.Add(baseName))
                {
                    for (int n = 2; n < 1000; n++)
                    {
                        string candidate = baseName + "_" + n;
                        if (usedNames.Add(candidate))
                        {
                            baseName = candidate;
                            break;
                        }
                    }
                }

                var plan = new Plan();
                plan.prefabPath = target.prefabPath;
                plan.baseName = baseName;
                plan.fileName = baseName + PackageExtension;
                plan.outputPath = dir + "/" + plan.fileName;
                plans.Add(plan);
                if (File.Exists(plan.outputPath))
                {
                    existing++;
                    if (existingNames.Count < OverwritePreview)
                        existingNames.Add(plan.fileName);
                }
            }

            if (plans.Count == 0)
            {
                EditorUtility.DisplayDialog(L("Package.ExportTitle"), L("Package.ExportNoTargets"), L("Common.Ok"));
                return;
            }

            bool renameInsteadOfOverwrite = false;
            if (existing > 0)
            {
                var preview = new System.Text.StringBuilder();
                for (int i = 0; i < existingNames.Count; i++)
                {
                    if (i > 0)
                        preview.Append('\n');
                    preview.Append(existingNames[i]);
                }

                if (existing > existingNames.Count)
                {
                    preview.Append('\n');
                    preview.Append(L("Package.OverwriteMore", existing - existingNames.Count));
                }

                int choice = EditorUtility.DisplayDialogComplex(L("Package.OverwriteTitle"), L("Package.OverwriteMessage", existing, preview.ToString()), L("Package.OverwriteYes"), L("Common.Cancel"), L("Package.OverwriteRename"));
                if (choice == 1)
                    return;
                renameInsteadOfOverwrite = choice == 2;
            }

            var listedByGuid = new Dictionary<string, WbbPackageAsset>(System.StringComparer.Ordinal);
            for (int i = 0; i < assets.Count; i++)
            {
                WbbPackageAsset listed = assets[i];
                if (listed != null && !string.IsNullOrEmpty(listed.guid))
                    listedByGuid[listed.guid] = listed;
            }

            int done = 0;
            int failed = 0;
            long totalBytes = 0L;
            bool canceled = false;
            var errors = new List<string>();
            busy = true;
            try
            {
                for (int i = 0; i < plans.Count; i++)
                {
                    Plan plan = plans[i];
                    float progress = (float)i / plans.Count;
                    if (EditorUtility.DisplayCancelableProgressBar(L("Package.ExportTitle"), L("Package.ExportProgress", i + 1, plans.Count, plan.fileName), progress))
                    {
                        canceled = true;
                        break;
                    }

                    List<WbbPackageAsset> collected = WbbPackageExporter.CollectDependencies(new[] { plan.prefabPath });
                    var keep = new List<WbbPackageAsset>();
                    for (int k = 0; k < collected.Count; k++)
                    {
                        WbbPackageAsset asset = collected[k];
                        if (asset == null)
                            continue;
                        WbbPackageAsset listed;
                        if (!string.IsNullOrEmpty(asset.guid) && listedByGuid.TryGetValue(asset.guid, out listed))
                        {
                            asset.redistributionRisk = listed.redistributionRisk && !asset.isRoot;
                            asset.customScript = listed.customScript;
                        }

                        if (WbbPackageExporter.ResolveIncluded(asset, excluded))
                            keep.Add(asset);
                    }

                    if (keep.Count == 0)
                    {
                        failed++;
                        errors.Add(L("Package.ResultFailedItem", plan.fileName, L("Package.ExportNoAssets")));
                        continue;
                    }

                    string outputPath = renameInsteadOfOverwrite ? MakeFreePath(dir, plan.baseName) : plan.outputPath;
                    string error;
                    if (WbbPackageExporter.WritePackage(keep, plan.baseName, outputPath, gather, out error))
                    {
                        done++;
                        totalBytes += FileSizeOf(outputPath);
                    }
                    else
                    {
                        failed++;
                        errors.Add(L("Package.ResultFailedItem", plan.fileName, error));
                    }
                }
            }
            catch (System.Exception e)
            {
                failed++;
                errors.Add(e.Message);
                WbbUtil.LogError("Package export aborted: " + e.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                busy = false;
            }

            for (int i = 0; i < errors.Count; i++)
                WbbUtil.LogWarning("Package export: " + errors[i]);
            string summary = canceled ? L("Package.ResultCanceled", done, plans.Count, WbbUtil.FormatSize(totalBytes)) : L("Package.ResultOk", done, failed, WbbUtil.FormatSize(totalBytes));
            SetResultStatus(summary, failed > 0 ? MessageType.Warning : MessageType.Info, dir);
            WbbUtil.Log(summary + " -> " + dir);
            if (host != null)
                host.RequestRepaint();
        }

        private static string MakeFreePath(string dir, string baseName)
        {
            string candidate = dir + "/" + baseName + PackageExtension;
            if (!File.Exists(candidate))
                return candidate;
            for (int n = 2; n < 10000; n++)
            {
                candidate = dir + "/" + baseName + "_" + n + PackageExtension;
                if (!File.Exists(candidate))
                    return candidate;
            }

            return dir + "/" + baseName + "_" + WbbUtil.NewId() + PackageExtension;
        }

        private static long FileSizeOf(string path)
        {
            try
            {
                return File.Exists(path) ? new FileInfo(path).Length : 0L;
            }
            catch (System.Exception)
            {
                return 0L;
            }
        }
    }
}
