using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private const int EnglishIndex = (int)WbbLanguage.English;
        private static readonly HashSet<string> LabelKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "Build.SettingsSection",
            "Build.Type",
            "Build.Version",
            "Build.Author",
            "Build.Output",
            "Build.NamePrefix",
            "Build.Description",
            "Build.StagingFolder",
            "Common.Guide",
            "Folder.Recent",
            "Folder.SearchHint",
            "History.FilterType",
            "History.FilterStatus",
            "History.Group",
            "Library.Paths",
            "Library.SortMode",
            "Package.OutputTitle",
            "Package.TargetsTitle",
            "Package.GatherTitle",
            "Package.GatherTex",
            "Package.GatherMat",
            "Package.GatherPrefab",
            "Package.SearchHint",
            "Quick.ModName",
            "Safety.StripVrc",
            "Tree.Title",
        };
        private static readonly Dictionary<string, string[]> composedLabels = new Dictionary<string, string[]>(StringComparer.Ordinal);
        public static bool IsLabelKey(string key)
        {
            return key != null && LabelKeys.Contains(key);
        }

        public static bool SplitLabel(string key, out string translated, out string original)
        {
            return SplitLabelFor(key, (int)Current, out translated, out original);
        }

        internal static bool SplitLabelFor(string key, int language, out string translated, out string original)
        {
            string[] localized;
            if (key == null || !Texts.TryGetValue(key, out localized) || localized == null || localized.Length == 0)
            {
                translated = key ?? string.Empty;
                original = string.Empty;
                return false;
            }

            int lang = Mathf.Clamp(language, 0, localized.Length - 1);
            bool label = LabelKeys.Contains(key);
            translated = label ? WithoutColon(localized[lang]) : localized[lang];
            if (lang == EnglishIndex)
            {
                original = translated;
                return false;
            }

            string english = OriginalFor(translated, WithoutColon(localized[Mathf.Min(EnglishIndex, localized.Length - 1)]));
            bool show = label && NeedsOriginal(translated, english);
            original = show ? english : string.Empty;
            return show;
        }

        internal static string TextFor(string key, int language)
        {
            string[] localized;
            return key != null && Texts.TryGetValue(key, out localized) && localized != null && localized.Length > 0 ? Localize(key, localized, language) : key;
        }

        private static string Localize(string key, string[] localized, int language)
        {
            int lang = Mathf.Clamp(language, 0, localized.Length - 1);
            if (!LabelKeys.Contains(key))
                return localized[lang];
            string[] composed;
            if (!composedLabels.TryGetValue(key, out composed))
            {
                string english = WithoutColon(localized[Mathf.Min(EnglishIndex, localized.Length - 1)]);
                composed = new string[localized.Length];
                for (int i = 0; i < localized.Length; i++)
                {
                    string body = WithoutColon(localized[i]);
                    string original = OriginalFor(body, english);
                    composed[i] = i == EnglishIndex || !NeedsOriginal(body, original) ? localized[i] : body + " (" + original + ")" + ColonSuffix(localized[i]);
                }

                composedLabels[key] = composed;
            }

            return composed[lang];
        }

        private static bool NeedsOriginal(string translated, string english)
        {
            if (string.IsNullOrEmpty(english) || string.IsNullOrEmpty(translated))
                return false;
            if (translated.IndexOf('(') >= 0 || translated.IndexOf('（') >= 0)
                return false;
            return translated.IndexOf(english, StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static string OriginalFor(string translated, string english)
        {
            int cut = SharedMarkerLength(translated, english);
            return cut > 0 && cut < english.Length ? english.Substring(cut) : english;
        }

        private static int SharedMarkerLength(string translated, string english)
        {
            if (string.IsNullOrEmpty(translated) || string.IsNullOrEmpty(english))
                return 0;
            int space = english.IndexOf(' ');
            if (space <= 0 || space >= translated.Length || translated[space] != ' ')
                return 0;
            if (string.CompareOrdinal(translated, 0, english, 0, space) != 0)
                return 0;
            for (int i = 0; i < space; i++)
            {
                if (char.IsLetter(english[i]))
                    return 0;
            }

            int end = space;
            while (end < english.Length && english[end] == ' ')
                end++;
            return end;
        }

        private static string ColonSuffix(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2)
                return string.Empty;
            char last = value[value.Length - 1];
            return last == ':' || last == '：' ? last.ToString() : string.Empty;
        }

        private static string WithoutColon(string value)
        {
            string colon = ColonSuffix(value);
            return colon.Length == 0 ? value : value.Substring(0, value.Length - 1).TrimEnd();
        }
    }
}
