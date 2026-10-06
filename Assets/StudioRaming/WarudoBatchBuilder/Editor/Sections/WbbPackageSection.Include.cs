namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private bool IsIncluded(WbbPackageAsset asset)
        {
            return WbbPackageExporter.ResolveIncluded(asset, excluded);
        }

        private bool ApplyInclude(WbbPackageAsset asset, bool include)
        {
            if (asset == null || asset.isRoot || string.IsNullOrEmpty(asset.guid))
                return false;
            if (IsIncluded(asset) == include)
                return false;
            return WbbPackageExporter.SetIncluded(asset, include, excluded);
        }

        private bool ApplyBulkInclude(WbbPackageAsset asset, bool include)
        {
            if (include && asset != null && asset.redistributionRisk)
                return false;
            return ApplyInclude(asset, include);
        }

        private void SetIncluded(WbbPackageAsset asset, bool include)
        {
            if (!ApplyInclude(asset, include))
                return;
            SaveExcluded();
            RecomputeCounts();
            if (host != null)
                host.RequestRepaint();
        }

        private void SetFolderIncluded(int nodeIndex, bool include)
        {
            if (nodeIndex < 0 || nodeIndex >= nodes.Count)
                return;
            if (!ApplyIncludeRecursive(nodeIndex, include))
                return;
            SaveExcluded();
            RecomputeCounts();
            if (host != null)
                host.RequestRepaint();
        }

        private bool ApplyIncludeRecursive(int index, bool include)
        {
            Node node = nodes[index];
            if (!node.visible)
                return false;
            if (!node.isFolder)
            {
                if (node.assetIndex < 0 || node.assetIndex >= assets.Count)
                    return false;
                return ApplyBulkInclude(assets[node.assetIndex], include);
            }

            bool changed = false;
            for (int i = 0; i < node.children.Count; i++)
                changed |= ApplyIncludeRecursive(node.children[i], include);
            return changed;
        }

        private void SetAllIncluded(bool include)
        {
            bool changed = false;
            for (int i = 0; i < shownAssets.Count; i++)
                changed |= ApplyBulkInclude(assets[shownAssets[i]], include);
            if (!changed)
                return;
            SaveExcluded();
            RecomputeCounts();
            if (host != null)
                host.RequestRepaint();
        }

        private void RecomputeCounts()
        {
            if (nodes.Count > 0)
                AccumulateIncluded(0);
            includedCount = 0;
            includedSize = 0L;
            riskOffCount = 0;
            riskOnCount = 0;
            for (int i = 0; i < assets.Count; i++)
            {
                WbbPackageAsset asset = assets[i];
                bool on = IsIncluded(asset);
                if (asset != null && asset.redistributionRisk)
                {
                    if (on)
                        riskOnCount++;
                    else
                        riskOffCount++;
                }

                if (!on)
                    continue;
                includedCount++;
                includedSize += asset.sizeBytes;
            }

            includedText = L("Package.IncludedSummary", includedCount, assets.Count, WbbUtil.FormatSize(includedSize));
            excludedCountText = L("Package.ExcludedCount", excluded.Count);
            BuildRiskNotice();
        }

        private void BuildRiskNotice()
        {
            if (riskOnCount > 0)
            {
                riskNoticePending = L("Package.RiskIncludedNotice", riskOnCount);
                riskNoticePendingTone = WbbTone.Warning;
            }
            else if (riskOffCount > 0)
            {
                riskNoticePending = L("Package.RiskExcludedNotice", riskOffCount);
                riskNoticePendingTone = WbbTone.Info;
            }
            else
            {
                riskNoticePending = "";
                riskNoticePendingTone = WbbTone.Info;
            }
        }

        private void AccumulateIncluded(int index)
        {
            Node node = nodes[index];
            node.shownIncluded = 0;
            node.shownRiskOff = 0;
            if (!node.visible && index != 0)
                return;
            if (!node.isFolder)
            {
                WbbPackageAsset asset = node.assetIndex >= 0 && node.assetIndex < assets.Count ? assets[node.assetIndex] : null;
                if (asset == null)
                    return;
                if (IsIncluded(asset))
                    node.shownIncluded = 1;
                else if (asset.redistributionRisk)
                    node.shownRiskOff = 1;
                return;
            }

            for (int i = 0; i < node.children.Count; i++)
            {
                int child = node.children[i];
                AccumulateIncluded(child);
                Node c = nodes[child];
                node.shownIncluded += c.shownIncluded;
                node.shownRiskOff += c.shownRiskOff;
            }
        }
    }
}
