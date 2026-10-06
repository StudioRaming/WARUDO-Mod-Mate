using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private void SyncStructure()
        {
            if (host != null && measuredListTop > 0f)
                listHeightLayout = Mathf.Max(RowHeight * 4f, host.position.height - BodyTop - measuredListTop - measuredFooterHeight - WbbSize.SpaceEdge * 2f - WbbSize.Control * 2f);
            if (pendingRiskNoticeExpanded >= 0)
            {
                riskNoticeExpanded = pendingRiskNoticeExpanded == 1;
                pendingRiskNoticeExpanded = -1;
            }

            if (pendingStatusSet)
            {
                pendingStatusSet = false;
                statusLine = pendingStatus;
                statusType = pendingStatusType;
                statusFolder = pendingStatusFolder;
            }

            if (pendingToggleNode >= 0)
            {
                int index = pendingToggleNode;
                pendingToggleNode = -1;
                if (index < nodes.Count && nodes[index].isFolder)
                {
                    Node node = nodes[index];
                    node.expanded = pendingToggleValue;
                    if (pendingToggleValue)
                        collapsed.Remove(node.path);
                    else
                        collapsed.Add(node.path);
                    SaveCollapsed();
                    structureDirty = true;
                }
            }

            if (pendingExpandAll >= 0)
            {
                bool open = pendingExpandAll == 1;
                pendingExpandAll = -1;
                if (nodes.Count > 0)
                    SetAllExpanded(open);
                structureDirty = true;
            }

            if (pendingClearTargets)
            {
                pendingClearTargets = false;
                if (targets.Count > 0)
                {
                    targets.Clear();
                    pendingTargetsChanged = true;
                }
            }

            if (pendingRemoveIndex >= 0)
            {
                int index = pendingRemoveIndex;
                pendingRemoveIndex = -1;
                if (index < targets.Count)
                {
                    targets.RemoveAt(index);
                    pendingTargetsChanged = true;
                }
            }

            if (pendingAddPaths.Count > 0)
            {
                for (int i = 0; i < pendingAddPaths.Count; i++)
                {
                    if (AddTargetPath(pendingAddPaths[i]))
                        pendingTargetsChanged = true;
                }

                pendingAddPaths.Clear();
            }

            if (pendingTargetsChanged)
            {
                pendingTargetsChanged = false;
                SaveTargets();
                pendingValidate = true;
                pendingCollect = true;
                structureDirty = true;
            }

            if (pendingValidate)
            {
                pendingValidate = false;
                ValidateTargets();
            }

            if (pendingCollect)
            {
                pendingCollect = false;
                CollectNow();
            }

            if (structureDirty || displayLangVersion != WbbLang.Version)
                RebuildDisplay();
            riskNoticeText = riskNoticePending;
            riskNoticeTone = riskNoticePendingTone;
            RefreshEmptyState();
            SyncGatherLayout();
            SyncBarLayout();
            float viewHeight = measuredListHeight > 1f ? measuredListHeight : (host != null ? host.position.height : Screen.height);
            viewHeight = Mathf.Max(160f, viewHeight);
            int count = visual.Count;
            if (count <= 0)
            {
                firstVisible = 0;
                lastVisible = -1;
                return;
            }

            firstVisible = Mathf.Clamp((int)((scroll.y - CullPad) / RowHeight), 0, count - 1);
            lastVisible = Mathf.Clamp((int)((scroll.y + viewHeight + CullPad) / RowHeight), 0, count - 1);
        }

        private void RebuildDisplay()
        {
            structureDirty = false;
            displayLangVersion = WbbLang.Version;
            string[] tokens = ParseTokens(search);
            searching = tokens != null;
            shownAssets.Clear();
            visual.Clear();
            if (nodes.Count > 0)
            {
                string folderTip = L("Package.FolderToggle.tip");
                ComputeVisibility(0, tokens, folderTip);
                AppendVisible(0);
            }

            RecomputeCounts();
            targetSummaryText = L("Package.TargetSummary", targets.Count);
            BuildExportContents();
            rootBadgeText = L("Package.RootBadge");
            hoverIndex = -1;
        }

        private void RefreshEmptyState()
        {
            emptyAction = EmptyActionNone;
            emptyTitle = "";
            emptyMessage = "";
            if (visual.Count > 0)
            {
                emptyKind = EmptyNone;
            }
            else if (targets.Count == 0)
            {
                emptyKind = EmptyNoTargets;
                emptyTitle = L("Package.EmptyTargetsTitle");
                emptyMessage = Embedded ? L("Package.EmptyTargetsEmbedded") : L("Package.DepsEmpty");
                if (!Embedded)
                    emptyAction = EmptyActionAddSelection;
            }
            else if (assets.Count == 0)
            {
                emptyKind = EmptyNoAssets;
                emptyTitle = L("Package.EmptyAssetsTitle");
                emptyMessage = L("Package.DepsNone");
                emptyAction = EmptyActionCollect;
            }
            else
            {
                emptyKind = EmptyNoMatch;
                emptyMessage = L("Common.NoMatch", assets.Count);
            }

            switch (emptyAction)
            {
                case EmptyActionAddSelection:
                    emptyActionLabel = L("Package.AddSelection");
                    break;
                case EmptyActionCollect:
                    emptyActionLabel = L("Common.Refresh");
                    break;
                default:
                    emptyActionLabel = "";
                    break;
            }
        }

        private void ComputeVisibility(int index, string[] tokens, string folderTip)
        {
            Node node = nodes[index];
            if (!node.isFolder)
            {
                WbbPackageAsset asset = node.assetIndex >= 0 && node.assetIndex < assets.Count ? assets[node.assetIndex] : null;
                node.visible = asset != null && MatchesSearch(asset, tokens);
                node.shownFiles = node.visible ? 1 : 0;
                node.shownSize = node.visible ? asset.sizeBytes : 0L;
                node.shownRoots = node.visible && asset.isRoot ? 1 : 0;
                if (node.visible)
                    shownAssets.Add(node.assetIndex);
                return;
            }

            node.shownFiles = 0;
            node.shownRoots = 0;
            node.shownSize = 0L;
            for (int i = 0; i < node.children.Count; i++)
            {
                int child = node.children[i];
                ComputeVisibility(child, tokens, folderTip);
                Node c = nodes[child];
                node.shownFiles += c.shownFiles;
                node.shownRoots += c.shownRoots;
                node.shownSize += c.shownSize;
            }

            node.visible = node.shownFiles > 0;
            if (index == 0 || !node.visible)
                return;
            node.infoText = L("Package.GroupInfo", node.shownFiles, WbbUtil.FormatSize(node.shownSize));
            node.labelContent = new GUIContent(node.label + "/", folderTip);
        }

        private void AppendVisible(int index)
        {
            Node node = nodes[index];
            for (int i = 0; i < node.children.Count; i++)
            {
                int child = node.children[i];
                Node c = nodes[child];
                if (!c.visible)
                    continue;
                visual.Add(child);
                if (c.isFolder && (searching || c.expanded))
                    AppendVisible(child);
            }
        }

        private static string[] ParseTokens(string query)
        {
            if (string.IsNullOrEmpty(query))
                return null;
            string[] parts = query.ToLowerInvariant().Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? null : parts;
        }

        private static bool MatchesSearch(WbbPackageAsset asset, string[] tokens)
        {
            if (tokens == null)
                return true;
            if (asset == null)
                return false;
            string name = asset.fileName == null ? "" : asset.fileName.ToLowerInvariant();
            string path = asset.assetPath == null ? "" : asset.assetPath.ToLowerInvariant();
            for (int i = 0; i < tokens.Length; i++)
            {
                if (name.Contains(tokens[i]))
                    continue;
                if (path.Contains(tokens[i]))
                    continue;
                return false;
            }

            return true;
        }
    }
}
