using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbBuildSection
    {
        private void RefreshNames()
        {
            namesDirty = false;
            string outDir = WbbUtil.NormalizePath(job.outputDirectory);
            var existsCache = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
            System.Func<string, bool> exists = delegate (string name)
            {
                if (string.IsNullOrEmpty(outDir) || string.IsNullOrEmpty(name))
                    return false;
                bool value;
                if (existsCache.TryGetValue(name, out value))
                    return value;
                value = FileExistsSafe(outDir + "/" + name + WbbUtil.ModExtension);
                existsCache[name] = value;
                return value;
            };
            bool autoNumber = !job.overwriteExisting;
            string dateSuffix = WbbUtil.DateSuffix();
            var single = new List<WbbNaming.Request>(1);
            for (int i = 0; i < items.Count; i++)
            {
                Item it = items[i];
                it.nameDuplicate = false;
                it.nameOverwrites = false;
                if (it.isScene)
                    continue;
                single.Clear();
                single.Add(RequestFor(it));
                WbbNaming.Result[] one = WbbNaming.Resolve(single, job.appendDateSuffix, autoNumber, dateSuffix, exists);
                it.modName = one[0].finalName;
            }

            List<int> order = SelectedInBuildOrder();
            var batch = new List<WbbNaming.Request>(order.Count);
            for (int i = 0; i < order.Count; i++)
                batch.Add(RequestFor(items[order[i]]));
            WbbNaming.Result[] results = WbbNaming.Resolve(batch, job.appendDateSuffix, autoNumber, dateSuffix, exists);
            for (int i = 0; i < order.Count; i++)
            {
                Item it = items[order[i]];
                it.modName = results[i].finalName;
                it.nameDuplicate = results[i].duplicate;
                it.nameOverwrites = results[i].overwrites;
            }

            displayDirty = true;
            summaryDirty = true;
            preflightDirty = true;
        }

        private WbbNaming.Request RequestFor(Item it)
        {
            it.manualName = WbbUtil.GetManualName(job, it.guid);
            if (!string.IsNullOrEmpty(it.modNameOverride))
                return new WbbNaming.Request
                {
                    baseName = it.modNameOverride,
                    exact = true
                };
            if (!string.IsNullOrEmpty(it.manualName))
                return new WbbNaming.Request
                {
                    baseName = it.manualName
                };
            return new WbbNaming.Request
            {
                baseName = WbbNaming.PrefabBaseName(it.name, it.folderName, it.folderPrefabs, job.useFolderNamePrefix, job.namePrefix),
            };
        }

        private List<int> SelectedInBuildOrder()
        {
            var order = new List<int>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].isScene)
                    continue;
                if (selected.Contains(items[i].guid))
                    order.Add(i);
            }

            order.Sort(delegate (int a, int b)
            {
                return string.Compare(items[a].path, items[b].path, System.StringComparison.OrdinalIgnoreCase);
            });
            return order;
        }

        private void RefreshDisplay()
        {
            displayDirty = false;
            displayLangVersion = WbbLang.Version;
            string none = L("Common.None");
            string notBuilt = L("Build.StatusNotBuilt");
            string sourceChanged = L("Build.StatusSourceChanged");
            string missingOutput = L("Build.StatusMissingOutput");
            string failedText = L("Build.StatusFailed");
            string tipNone = L("Build.BadgeTipNone");
            string tipChanged = L("Build.BadgeTipChanged");
            string tipMissing = L("Build.BadgeTipMissing");
            string duplicateText = L("Build.NameDuplicate");
            string overwriteText = L("Build.NameOverwrites");
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                WbbRecord record = item.record;
                string stamp = record == null ? "" : (string.IsNullOrEmpty(record.finishedIso) ? record.startedIso : record.finishedIso);
                item.badgeMark = null;
                switch (item.state)
                {
                    case BadgeState.Built:
                        item.badgeText = L("Build.StatusBuiltShort");
                        item.badgeTone = WbbTone.Success;
                        item.badgeGlyph = "";
                        item.badgeMark = UI.SRGlyph.Check;
                        break;
                    case BadgeState.SourceChanged:
                        item.badgeText = sourceChanged;
                        item.badgeTone = WbbTone.Warning;
                        item.badgeGlyph = "↻";
                        break;
                    case BadgeState.MissingOutput:
                        item.badgeText = missingOutput;
                        item.badgeTone = WbbTone.Warning;
                        item.badgeGlyph = "!";
                        break;
                    case BadgeState.Failed:
                        item.badgeText = failedText;
                        item.badgeTone = WbbTone.Error;
                        item.badgeGlyph = "";
                        item.badgeMark = UI.SRGlyph.Close;
                        break;
                    case BadgeState.Other:
                        item.badgeText = WbbLang.StatusName(record.Status);
                        item.badgeTone = WbbTone.Neutral;
                        item.badgeGlyph = "·";
                        break;
                    default:
                        item.badgeText = notBuilt;
                        item.badgeTone = WbbTone.Neutral;
                        item.badgeGlyph = "·";
                        break;
                }

                string tip;
                if (record == null)
                {
                    tip = tipNone;
                    item.dateText = "";
                }
                else
                {
                    tip = L("Build.BadgeTip", string.IsNullOrEmpty(record.modName) ? none : record.modName, string.IsNullOrEmpty(record.modVersion) ? none : record.modVersion, string.IsNullOrEmpty(stamp) ? none : stamp, record.outputSizeBytes > 0L ? WbbUtil.FormatSize(record.outputSizeBytes) : none, string.IsNullOrEmpty(record.outputFilePath) ? none : record.outputFilePath);
                    if (item.state == BadgeState.SourceChanged)
                        tip += "\n" + tipChanged;
                    else if (item.state == BadgeState.MissingOutput)
                        tip += "\n" + tipMissing;
                    if (!string.IsNullOrEmpty(record.message))
                        tip += "\n" + record.message;
                    item.dateText = stamp.Length >= 16 ? stamp.Substring(5, 11) : stamp;
                }

                item.badgeContent = new GUIContent(string.Empty, tip);
                var sb = new System.Text.StringBuilder();
                if (item.isScene)
                {
                    sb.Append(item.sceneLabel);
                    if (item.buildCount > 1)
                        sb.Append(" · ").Append(L("Build.BuildCount", item.buildCount));
                }
                else
                {
                    sb.Append(item.name).Append(".prefab");
                    sb.Append(" · ").Append(reasonTexts[(int)item.reason]);
                    if (!string.IsNullOrEmpty(item.manualName))
                        sb.Append(" · ").Append(L("Build.NameManual"));
                    if (item.nameDuplicate)
                        sb.Append(" · ").Append(duplicateText);
                    else if (item.nameOverwrites)
                        sb.Append(" · ").Append(overwriteText);
                }

                item.subText = sb.ToString();
                item.cardTip = item.isScene ? L("Build.SceneCardTip", item.modName + WbbUtil.ModExtension, item.sceneLabel) : L("Build.CardTip", item.badgeText, item.modName + WbbUtil.ModExtension, item.path);
            }

            filterDirty = true;
            summaryDirty = true;
        }

        private static bool MatchesSearch(Item item, string[] words)
        {
            if (words == null || words.Length == 0)
                return true;
            for (int i = 0; i < words.Length; i++)
            {
                if (!item.keyName.Contains(words[i]))
                    return false;
            }

            return true;
        }

        private void RebuildFiltered()
        {
            filterDirty = false;
            filtered.Clear();
            groups.Clear();
            string[] words = null;
            if (!string.IsNullOrEmpty(search))
            {
                words = search.ToLowerInvariant().Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 0)
                    words = null;
            }

            var perFolder = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].isScene)
                    continue;
                int n;
                perFolder.TryGetValue(items[i].folder, out n);
                perFolder[items[i].folder] = n + 1;
            }

            var byKey = new Dictionary<string, Group>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                item.keyName = (item.modName + "\n" + item.name + "\n" + item.folder + "\n" + item.sceneLabel).ToLowerInvariant();
                if (!MatchesSearch(item, words))
                {
                    item.group = -1;
                    continue;
                }

                string key = item.isScene ? SceneGroupKey : GroupKeyOf(item.folder, perFolder);
                Group g;
                if (!byKey.TryGetValue(key, out g))
                {
                    g = NewGroup(key);
                    byKey[key] = g;
                    groups.Add(g);
                }

                g.members.Add(i);
                if (item.sortTicks > g.newestTicks)
                    g.newestTicks = item.sortTicks;
                if ((int)item.state < g.worstState)
                    g.worstState = (int)item.state;
            }

            bool searching = words != null;
            for (int gi = 0; gi < groups.Count; gi++)
            {
                Group g = groups[gi];
                g.open = searching || !collapsed.Contains(g.key);
                SortMembers(g.members);
                FillGroupDetail(g);
            }

            SortGroups();
            for (int gi = 0; gi < groups.Count; gi++)
            {
                List<int> m = groups[gi].members;
                for (int k = 0; k < m.Count; k++)
                {
                    items[m[k]].group = gi;
                    filtered.Add(m[k]);
                }
            }

            summaryDirty = true;
        }

        private static string GroupKeyOf(string folder, Dictionary<string, int> perFolder)
        {
            int n;
            perFolder.TryGetValue(folder, out n);
            if (n >= 2 || string.Equals(folder, "Assets", System.StringComparison.OrdinalIgnoreCase))
                return folder;
            int slash = folder.LastIndexOf('/');
            return slash > 0 ? folder.Substring(0, slash) : folder;
        }

        private Group NewGroup(string key)
        {
            var g = new Group();
            g.key = key;
            if (key == SceneGroupKey)
            {
                g.scene = true;
                g.title = L("Build.SceneGroup");
                g.tooltip = L("Build.SceneGroup.tip");
                return g;
            }

            string rel = key.Length > 7 && key.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase) ? key.Substring(7) : (string.Equals(key, "Assets", System.StringComparison.OrdinalIgnoreCase) ? L("Build.GroupRoot") : key);
            string[] parts = rel.Split('/');
            g.title = parts.Length > 2 ? "… / " + parts[parts.Length - 2] + " / " + parts[parts.Length - 1] : rel.Replace("/", " / ");
            g.tooltip = key;
            g.marked = WbbListSettings.IsWorkFolder(key);
            return g;
        }

        private void FillGroupDetail(Group g)
        {
            int built = 0, changed = 0, failed = 0, missing = 0;
            g.selectable = 0;
            for (int k = 0; k < g.members.Count; k++)
            {
                Item it = items[g.members[k]];
                if (!it.isScene)
                    g.selectable++;
                switch (it.state)
                {
                    case BadgeState.Built:
                        built++;
                        break;
                    case BadgeState.SourceChanged:
                        changed++;
                        break;
                    case BadgeState.Failed:
                        failed++;
                        break;
                    case BadgeState.MissingOutput:
                        missing++;
                        break;
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.Append(L("Build.GroupCount", g.members.Count));
            if (built > 0)
                sb.Append(" · ").Append(L("Build.GroupBuilt", built));
            if (changed > 0)
                sb.Append(" · ").Append(L("Build.GroupChanged", changed));
            if (missing > 0)
                sb.Append(" · ").Append(L("Build.GroupMissing", missing));
            if (failed > 0)
                sb.Append(" · ").Append(L("Build.GroupFailed", failed));
            g.detail = sb.ToString();
        }

        private void SortMembers(List<int> members)
        {
            if (members.Count < 2)
                return;
            members.Sort(delegate (int a, int b)
            {
                Item ia = items[a];
                Item ib = items[b];
                int result = 0;
                if (sortMode == SortDate)
                    result = ib.sortTicks.CompareTo(ia.sortTicks);
                else if (sortMode == SortState)
                    result = ((int)ia.state).CompareTo((int)ib.state);
                if (result != 0)
                    return result;
                if (ia.dimmed != ib.dimmed)
                    return ia.dimmed ? 1 : -1;
                return CompareByName(ia, ib);
            });
        }

        private void SortGroups()
        {
            groups.Sort(delegate (Group a, Group b)
            {
                if (a.scene != b.scene)
                    return a.scene ? -1 : 1;
                int result = 0;
                if (sortMode == SortDate)
                    result = b.newestTicks.CompareTo(a.newestTicks);
                else if (sortMode == SortState)
                    result = a.worstState.CompareTo(b.worstState);
                if (result != 0)
                    return result;
                return EditorUtility.NaturalCompare(a.key, b.key);
            });
        }

        private void RefreshSummary()
        {
            summaryDirty = false;
            summaryLangVersion = WbbLang.Version;
            int count = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].isScene && selected.Contains(items[i].guid))
                    count++;
            }

            selectedCount = count;
            for (int gi = 0; gi < groups.Count; gi++)
            {
                Group g = groups[gi];
                g.selected = 0;
                for (int k = 0; k < g.members.Count; k++)
                {
                    Item it = items[g.members[k]];
                    if (!it.isScene && selected.Contains(it.guid))
                        g.selected++;
                }
            }

            int visiblePrefabs = 0;
            for (int i = 0; i < filtered.Count; i++)
                if (!items[filtered[i]].isScene)
                    visiblePrefabs++;
            countLine = L("Build.CountLine", visiblePrefabs, count);
            int hiddenTotal = rawCount + userHiddenCount;
            hiddenChipText = showHidden ? L("Build.HiddenChipOn", hiddenTotal) : L("Build.HiddenChip", hiddenTotal);
            hiddenChipTip = L("Build.HiddenChip.tip", rawCount, userHiddenCount);
            Item preview = null;
            List<int> order = SelectedInBuildOrder();
            if (order.Count > 0)
                preview = items[order[0]];
            previewLine = preview == null ? string.Empty : L("Build.NamePreview", preview.modName + WbbUtil.ModExtension);
        }

        private void RecomputePreflight()
        {
            preflightDirty = false;
            preflightIssues.Clear();
            blockingCount = 0;
            warningCount = 0;
            if (IsPackageMode)
            {
                preflightLine = selectedCount > 0 ? L("Build.PreflightPackage") : "";
                return;
            }

            if (selectedCount == 0)
            {
                preflightLine = "";
                return;
            }

            List<WbbBuildItem> targets = CollectBuildItems();
            for (int i = 0; i < items.Count; i++)
            {
                Item it = items[i];
                if (it.isScene || !it.nameDuplicate || !selected.Contains(it.guid))
                    continue;
                preflightIssues.Add(new WbbPreflightIssue(true, L("Build.PreflightDuplicateName", it.modName), it.path));
            }

            try
            {
                List<WbbPreflightIssue> issues = WbbBuildRunner.Preflight(targets, job);
                for (int i = 0; issues != null && i < issues.Count; i++)
                {
                    WbbPreflightIssue issue = issues[i];
                    if (issue == null)
                        continue;
                    var ex = issue as WbbPreflightIssueEx;
                    if (ex != null && ex.code == WbbPreflightCodes.DatabaseLoadFailed && !ex.blocking)
                        issue = new WbbPreflightIssueEx(true, ex.message, ex.detail, ex.code, ex.actionPath);
                    preflightIssues.Add(issue);
                }
            }
            catch (System.Exception e)
            {
                WbbUtil.LogWarning("Preflight failed: " + e.Message);
            }

            for (int i = 0; i < preflightIssues.Count; i++)
            {
                if (preflightIssues[i] == null)
                    continue;
                if (preflightIssues[i].blocking)
                    blockingCount++;
                else
                    warningCount++;
            }

            if (blockingCount > 0)
                preflightLine = L("Build.PreflightBlockLine", blockingCount);
            else if (warningCount > 0)
                preflightLine = L("Build.PreflightWarnLine", warningCount);
            else
                preflightLine = L("Build.PreflightOk", targets.Count);
        }

        private List<WbbBuildItem> CollectBuildItems()
        {
            var result = new List<WbbBuildItem>();
            List<int> order = SelectedInBuildOrder();
            for (int i = 0; i < order.Count; i++)
            {
                Item item = items[order[i]];
                var built = new WbbBuildItem();
                built.prefabGuid = item.guid;
                built.prefabPath = item.path;
                built.prefabName = item.name;
                built.modName = item.modName;
                built.icon = EnsureThumb(item);
                result.Add(built);
            }

            return result;
        }

        private static bool FileExistsSafe(string path)
        {
            try
            {
                string absolute = WbbUtil.ToAbsolutePath(path);
                return !string.IsNullOrEmpty(absolute) && File.Exists(absolute);
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
