using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbPackageSection
    {
        private void CollectNow()
        {
            assets.Clear();
            assetSizeTexts.Clear();
            assetFolderTexts.Clear();
            var roots = new List<string>();
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].missing)
                    continue;
                roots.Add(targets[i].prefabPath);
            }

            if (roots.Count > 0)
            {
                List<WbbPackageAsset> collected = WbbPackageExporter.CollectDependencies(roots);
                if (collected != null)
                    assets.AddRange(collected);
            }

            for (int i = 0; i < assets.Count; i++)
            {
                assetSizeTexts.Add(WbbUtil.FormatSize(assets[i].sizeBytes));
                string relative = RelativePath(assets[i].assetPath);
                int slash = relative.LastIndexOf('/');
                assetFolderTexts.Add(slash > 0 ? relative.Substring(0, slash) : "");
            }

            BuildTree();
            hoverIndex = -1;
            structureDirty = true;
        }

        private static string RelativePath(string assetPath)
        {
            string path = WbbUtil.NormalizePath(assetPath);
            if (string.IsNullOrEmpty(path))
                return "";
            return path.StartsWith(AssetsPrefix, System.StringComparison.Ordinal) ? path.Substring(AssetsPrefix.Length) : path;
        }

        private void BuildTree()
        {
            nodes.Clear();
            visual.Clear();
            var root = new Node();
            root.isFolder = true;
            root.depth = -1;
            nodes.Add(root);
            var folderIndex = new Dictionary<string, int>(System.StringComparer.Ordinal);
            folderIndex[""] = 0;
            for (int i = 0; i < assets.Count; i++)
            {
                WbbPackageAsset asset = assets[i];
                if (asset == null)
                    continue;
                string relative = RelativePath(asset.assetPath);
                if (string.IsNullOrEmpty(relative))
                    continue;
                int slash = relative.LastIndexOf('/');
                string folder = slash > 0 ? relative.Substring(0, slash) : "";
                int parent = EnsureFolderNode(folderIndex, folder);
                var file = new Node();
                file.path = relative;
                file.label = string.IsNullOrEmpty(asset.fileName) ? relative.Substring(slash + 1) : asset.fileName;
                file.assetIndex = i;
                nodes.Add(file);
                nodes[parent].children.Add(nodes.Count - 1);
            }

            CompressChains(0);
            SortChildren(0);
            AssignDepth(0, -1);
            RestoreExpanded();
        }

        private int EnsureFolderNode(Dictionary<string, int> folderIndex, string folder)
        {
            if (string.IsNullOrEmpty(folder))
                return 0;
            int index;
            if (folderIndex.TryGetValue(folder, out index))
                return index;
            int cut = folder.LastIndexOf('/');
            string parentPath = cut > 0 ? folder.Substring(0, cut) : "";
            int parent = EnsureFolderNode(folderIndex, parentPath);
            var node = new Node();
            node.isFolder = true;
            node.path = folder;
            node.label = cut >= 0 ? folder.Substring(cut + 1) : folder;
            nodes.Add(node);
            index = nodes.Count - 1;
            folderIndex[folder] = index;
            nodes[parent].children.Add(index);
            return index;
        }

        private void CompressChains(int index)
        {
            Node node = nodes[index];
            if (index != 0)
            {
                while (node.children.Count == 1)
                {
                    Node child = nodes[node.children[0]];
                    if (!child.isFolder)
                        break;
                    node.path = child.path;
                    node.label = node.label + "/" + child.label;
                    node.children.Clear();
                    node.children.AddRange(child.children);
                    child.children.Clear();
                }
            }

            for (int i = 0; i < node.children.Count; i++)
                CompressChains(node.children[i]);
        }

        private void SortChildren(int index)
        {
            Node node = nodes[index];
            if (node.children.Count > 1)
            {
                node.children.Sort(delegate (int a, int b)
                {
                    Node na = nodes[a];
                    Node nb = nodes[b];
                    if (na.isFolder != nb.isFolder)
                        return na.isFolder ? -1 : 1;
                    int order = string.Compare(na.label, nb.label, System.StringComparison.OrdinalIgnoreCase);
                    if (order != 0)
                        return order;
                    return string.Compare(na.path, nb.path, System.StringComparison.Ordinal);
                });
            }

            for (int i = 0; i < node.children.Count; i++)
                SortChildren(node.children[i]);
        }

        private void AssignDepth(int index, int depth)
        {
            Node node = nodes[index];
            node.depth = depth;
            for (int i = 0; i < node.children.Count; i++)
                AssignDepth(node.children[i], depth + 1);
        }

        private void RestoreExpanded()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (!node.isFolder)
                    continue;
                node.expanded = !collapsed.Contains(node.path);
            }
        }

        private void SetAllExpanded(bool open)
        {
            SetExpandedRecursive(0, open);
            SaveCollapsed();
        }

        private void SetExpandedRecursive(int index, bool open)
        {
            Node node = nodes[index];
            if (index != 0 && node.isFolder)
            {
                node.expanded = open;
                if (open)
                    collapsed.Remove(node.path);
                else
                    collapsed.Add(node.path);
            }

            for (int i = 0; i < node.children.Count; i++)
                SetExpandedRecursive(node.children[i], open);
        }
    }
}
