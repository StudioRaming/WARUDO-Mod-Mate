using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbStyles
    {
        public const float HeaderHeight = WbbSize.Header.Height;
        public const float TabBarHeight = WbbSize.Tabs.Height;
        private static GUIContent helpMenuContent;
        private static int helpMenuVersion = -1;
        public static void WindowBackground(EditorWindow window)
        {
            if (window == null || Event.current.type != EventType.Repaint)
                return;
            EditorGUI.DrawRect(new Rect(0f, 0f, window.position.width, window.position.height), WbbTheme.P.BgWindow);
        }

        public static void BeginWindowBody()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(WbbSize.SpaceEdge);
            EditorGUILayout.BeginVertical();
        }

        public static void EndWindowBody()
        {
            EditorGUILayout.EndVertical();
            GUILayout.Space(WbbSize.SpaceEdge);
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawHeader()
        {
            EnsureStyles();
            if (helpMenuContent == null || helpMenuVersion != WbbLang.Version)
            {
                helpMenuVersion = WbbLang.Version;
                helpMenuContent = new GUIContent("?", L("Ui.HelpMenu.tip"));
            }

            Rect tools = UI.SRGui.Header(UI.SRProduct.WarudoBatchBuilder, HeaderMark, WbbProduct.Name, L("Ui.HeaderSubtitle", WbbProduct.Version, WbbProduct.Publisher));
            const float helpWidth = WbbSize.Header.HelpWidth;
            var helpRect = new Rect(tools.xMax - helpWidth, tools.y, helpWidth, tools.height);
            var langRect = new Rect(tools.x, tools.y, Mathf.Max(0f, tools.width - helpWidth - WbbSize.Header.ToolGap), tools.height);
            int current = (int)WbbLang.Current;
            int next = Dropdown(langRect, current, WbbLang.LanguageLabels);
            if (next != current)
                WbbLang.Current = (WbbLanguage)next;
            if (Button(helpRect, helpMenuContent, WbbButtonKind.Ghost))
                ShowHelpMenu();
        }

        private const string HeaderMark = "W";
        private static void ShowHelpMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(L("Ui.Help.Manual")), false, WbbDiagnostics.OpenManual);
            menu.AddItem(new GUIContent(L("Ui.Help.About")), false, WbbDiagnostics.ShowAbout);
            menu.AddItem(new GUIContent(L("Ui.Help.CopyDiagnostics")), false, WbbDiagnostics.CopyToClipboard);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent(L("Ui.Help.Repository")), false, WbbDiagnostics.OpenRepository);
            menu.AddItem(new GUIContent(L("Ui.Help.Downloads")), false, WbbDiagnostics.OpenDownloads);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent(L("Ui.Help.CopyEmail")), false, WbbDiagnostics.CopySupportEmail);
            menu.ShowAsContext();
        }

        public static int TabBar(int current, GUIContent[] tabs, int[] badges)
        {
            EnsureStyles();
            if (tabs == null || tabs.Length == 0)
            {
                GUILayoutUtility.GetRect(0f, TabBarHeight, GUILayout.ExpandWidth(true));
                return current;
            }

            return UI.SRGui.Tabs(current, tabs, badges);
        }

        public static void BeginCard()
        {
            EnsureStyles();
            EditorGUILayout.BeginVertical(card);
        }

        public static void BeginCard(params GUILayoutOption[] options)
        {
            EnsureStyles();
            EditorGUILayout.BeginVertical(card, options);
        }

        public static void EndCard()
        {
            EditorGUILayout.EndVertical();
        }

        public static void SectionTitle(string text)
        {
            EnsureStyles();
            Rect r = GUILayoutUtility.GetRect(0f, WbbSize.Section.TitleHeight, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(r.x + WbbSize.Section.LabelLeft, r.y + 1f, Mathf.Max(WbbSize.Section.LabelMinWidth, r.width - WbbSize.Section.LabelRightPad), WbbSize.Section.LabelHeight), text == null ? "" : text, cardTitle);
        }

        public static void Separator()
        {
            GUILayout.Space(WbbSize.Section.SeparatorGap);
            Rect rect = EditorGUILayout.GetControlRect(false, WbbSize.Line);
            rect.x += WbbSize.Section.SeparatorInset;
            rect.width = Mathf.Max(1f, rect.width - WbbSize.Section.SeparatorInset * 2f);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, WbbTheme.P.BorderSubtle);
            GUILayout.Space(WbbSize.Section.SeparatorGap);
        }

        public static RectOffset ControlMargin
        {
            get
            {
                return EditorStyles.layerMaskField.margin;
            }
        }

        public static float LayoutGap(RectOffset before, RectOffset after, float space = 0f)
        {
            return space + Mathf.Max(before.right, after.left);
        }

        public static GUIStyle FitCell
        {
            get
            {
                if (fitCell == null)
                    fitCell = new GUIStyle(GUIStyle.none)
                    {
                        stretchWidth = false
                    };
                return fitCell;
            }
        }

        private static GUIStyle fitCell;
        public static int PlanWrap(float[] widths, float[] gaps, float available, bool[] breakBefore)
        {
            int lines = widths.Length > 0 ? 1 : 0;
            float x = 0f;
            for (int i = 0; i < widths.Length; i++)
            {
                bool wrap = i > 0 && x + gaps[i] + widths[i] > available;
                breakBefore[i] = wrap;
                if (i == 0 || wrap)
                {
                    if (wrap)
                        lines++;
                    x = widths[i];
                }
                else
                {
                    x += gaps[i] + widths[i];
                }
            }

            return lines;
        }

        public static void NextCell(bool newLine, float space)
        {
            if (!newLine)
            {
                if (space > 0f)
                    GUILayout.Space(space);
                return;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
        }

        public static float TextWidth(GUIStyle style, GUIContent content)
        {
            return style == null || content == null ? 0f : Mathf.Ceil(style.CalcSize(content).x);
        }

        public static Color IndexColor(int index, float s = WbbTheme.IndexSaturation, float v = WbbTheme.IndexValue)
        {
            if (index < 0)
                index = -index;
            return Color.HSVToRGB((index * 0.61803399f) % 1f, s, v);
        }

        public static void DrawGuide(string prefKey, string guideLangKey)
        {
            EnsureStyles();
            string key = WbbUtil.PrefPrefix + "Guide." + (prefKey == null ? "" : prefKey);
            bool reset;
            bool open = UI.SRGui.Section(key, LC("Common.Guide"), false, null, out reset);
            if (!open)
                return;
            BeginCard();
            GUILayout.Label(WbbLang.T(guideLangKey), guideBody);
            EndCard();
        }

        public static void StatusLine(string message, MessageType type = MessageType.Info)
        {
            if (string.IsNullOrEmpty(message))
                return;
            Callout(message, ToneOf(type));
        }

        public static void Callout(string message, WbbTone tone)
        {
            if (string.IsNullOrEmpty(message))
                return;
            EnsureStyles();
            measure.text = message;
            measure.image = null;
            measure.tooltip = string.Empty;
            Rect r = GUILayoutUtility.GetRect(measure, calloutText, GUILayout.MaxWidth(WbbSize.Field.CalloutMaxWidth));
            if (Event.current.type != EventType.Repaint)
                return;
            WbbToneColors c = WbbTheme.Tone(tone);
            WbbTheme.Box(r, c.BadgeBg, c.BadgeBorder, WbbSize.RadiusControl);
            var icon = new Rect(r.x + WbbSize.Field.CalloutIconLeft, r.y + WbbSize.Field.CalloutIconTop, WbbSize.Field.CalloutIcon, WbbSize.Field.CalloutIcon);
            WbbTheme.Fill(icon, c.Base, WbbSize.Field.CalloutIcon * 0.5f);
            if (tone == WbbTone.Success || tone == WbbTone.Error)
            {
                var mark = new Rect(icon.center.x - WbbSize.Glyph * 0.5f, icon.center.y - WbbSize.Glyph * 0.5f, WbbSize.Glyph, WbbSize.Glyph);
                UI.SRGui.Glyph(mark, tone == WbbTone.Success ? UI.SRGlyph.Check : UI.SRGlyph.Close, WbbTheme.P.BgWindow, WbbSize.GlyphStrokeBold);
            }
            else
            {
                Color oldGlyph = badgeText.normal.textColor;
                badgeText.normal.textColor = WbbTheme.P.BgWindow;
                GUI.Label(icon, tone == WbbTone.Warning ? "!" : "i", badgeText);
                badgeText.normal.textColor = oldGlyph;
            }

            calloutText.Draw(r, measure, false, false, false, false);
        }

        public static WbbTone ToneOf(MessageType type)
        {
            switch (type)
            {
                case MessageType.Error:
                    return WbbTone.Error;
                case MessageType.Warning:
                    return WbbTone.Warning;
                case MessageType.Info:
                    return WbbTone.Info;
                default:
                    return WbbTone.Neutral;
            }
        }
    }
}
