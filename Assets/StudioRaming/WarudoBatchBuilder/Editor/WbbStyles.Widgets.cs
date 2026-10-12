using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public enum WbbButtonKind
    {
        Secondary,
        Primary,
        Danger,
        Ghost,
    }

    public static partial class WbbStyles
    {
        public static void Badge(Rect rect, string text, WbbTone tone)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;
            if (rect.width <= 2f || rect.height <= 2f)
                return;
            EnsureStyles();
            Rect box = rect;
            if (box.height > BadgeHeight)
                box = new Rect(box.x, box.y + (box.height - BadgeHeight) * 0.5f, box.width, BadgeHeight);
            WbbToneColors c = WbbTheme.Tone(tone);
            WbbTheme.Box(box, c.BadgeBg, c.BadgeBorder, box.height * 0.5f);
            if (string.IsNullOrEmpty(text))
                return;
            var textRect = new Rect(box.x + WbbSize.Badge.TextInset, box.y, Mathf.Max(0f, box.width - WbbSize.Badge.TextInset * 2f), box.height);
            Color old = badgeText.normal.textColor;
            badgeText.normal.textColor = c.Fg;
            GUI.Label(textRect, Fit(text, badgeText, textRect.width), badgeText);
            badgeText.normal.textColor = old;
        }

        public static void Badge(Rect rect, string text, Color color)
        {
            Badge(rect, text, ToneOfColor(color));
        }

        public static void Chip(Rect rect, string text, WbbTone tone)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;
            EnsureStyles();
            WbbToneColors c = WbbTheme.Tone(tone);
            WbbTheme.Box(rect, c.BadgeBg, c.BadgeBorder, rect.height * 0.5f);
            Color old = chipText.normal.textColor;
            chipText.normal.textColor = c.Fg;
            GUI.Label(rect, Fit(text, chipText, rect.width - WbbSize.Badge.ChipTextPad), chipText);
            chipText.normal.textColor = old;
        }

        public static void Chip(Rect rect, string text, WbbTone tone, UI.SRGlyph leading)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;
            EnsureStyles();
            WbbToneColors c = WbbTheme.Tone(tone);
            WbbTheme.Box(rect, c.BadgeBg, c.BadgeBorder, rect.height * 0.5f);
            var glyphRect = new Rect(rect.x + WbbSize.Badge.TextInset, rect.y, WbbSize.Glyph, rect.height);
            Glyph(glyphRect, leading, c.Fg);
            float textLeft = glyphRect.xMax + WbbSize.GlyphGap;
            var textRect = new Rect(textLeft, rect.y, Mathf.Max(0f, rect.xMax - WbbSize.Badge.TextInset - textLeft), rect.height);
            Color old = chipText.normal.textColor;
            chipText.normal.textColor = c.Fg;
            GUI.Label(textRect, Fit(text, chipText, textRect.width), chipText);
            chipText.normal.textColor = old;
        }

        public static void Badge(Rect rect, UI.SRGlyph glyph, WbbTone tone)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;
            if (rect.width <= 2f || rect.height <= 2f)
                return;
            Rect box = rect;
            if (box.height > BadgeHeight)
                box = new Rect(box.x, box.y + (box.height - BadgeHeight) * 0.5f, box.width, BadgeHeight);
            WbbToneColors c = WbbTheme.Tone(tone);
            WbbTheme.Box(box, c.BadgeBg, c.BadgeBorder, box.height * 0.5f);
            float size = Mathf.Min(WbbSize.Glyph, Mathf.Min(box.width, box.height));
            var mark = new Rect(box.center.x - size * 0.5f, box.center.y - size * 0.5f, size, size);
            UI.SRGui.Glyph(mark, glyph, WbbTheme.ScaleAlpha(c.Fg, GUI.color.a), WbbSize.GlyphStrokeBold);
        }

        public static Rect BadgeLayout(string text, WbbTone tone, float minWidth = WbbSize.Badge.MinWidth)
        {
            EnsureStyles();
            measure.text = text == null ? "" : text;
            measure.tooltip = string.Empty;
            measure.image = null;
            float width = Mathf.Max(minWidth, badgeText.CalcSize(measure).x + WbbSize.Badge.ExtraWidth);
            Rect rect = GUILayoutUtility.GetRect(width, BadgeHeight, GUILayout.Width(width), GUILayout.Height(BadgeHeight));
            Badge(rect, text, tone);
            return rect;
        }

        public static float BadgeWidth(string text)
        {
            EnsureStyles();
            measure.text = text == null ? "" : text;
            measure.tooltip = string.Empty;
            measure.image = null;
            return badgeText.CalcSize(measure).x + WbbSize.Badge.ExtraWidth;
        }

        private static WbbTone ToneOfColor(Color color)
        {
            WbbPalette p = WbbTheme.P;
            if (Same(color, p.Success.Fg) || Same(color, p.Success.Base))
                return WbbTone.Success;
            if (Same(color, p.Error.Fg) || Same(color, p.Error.Base))
                return WbbTone.Error;
            if (Same(color, p.Warning.Fg) || Same(color, p.Warning.Base))
                return WbbTone.Warning;
            if (Same(color, p.AccentFg) || Same(color, p.Accent))
                return WbbTone.Accent;
            if (Same(color, p.Info.Fg))
                return WbbTone.Info;
            return WbbTone.Neutral;
        }

        private static bool Same(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f;
        }

        public static void RowBackground(Rect rect, bool selected, bool hover, bool alternate)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;
            if (rect.width <= 0f || rect.height <= 0f)
                return;
            WbbPalette p = WbbTheme.P;
            Color face = selected ? p.BgSelected : hover ? p.BgCardHover : alternate ? p.BgWindow : p.BgCard;
            EditorGUI.DrawRect(rect, face);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - WbbSize.Line, rect.width, WbbSize.Line), p.BorderSubtle);
        }

        public static void GroupHeader(Rect rect, string title, string detail, bool open, int selected, int total, bool marked, out bool toggledAll, out bool toggledOpen)
        {
            EnsureStyles();
            toggledAll = false;
            toggledOpen = false;
            WbbPalette p = WbbTheme.P;
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, p.BgPanel);
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - WbbSize.Line, rect.width, WbbSize.Line), p.BorderSubtle);
            }

            bool selectable = total > 0;
            const float check = WbbSize.CheckSize;
            var arrowRect = new Rect(rect.x + WbbSize.Group.ArrowLeft, rect.y, WbbSize.Group.ArrowWidth, rect.height);
            var checkRect = new Rect(arrowRect.xMax + WbbSize.Group.CheckGap, rect.y + (rect.height - check) * 0.5f, selectable ? check : 0f, check);
            var titleRect = new Rect(checkRect.xMax + (selectable ? WbbSize.Group.TitleGap : WbbSize.Group.TitleGapNoCheck), rect.y, Mathf.Max(WbbSize.Group.TitleMinWidth, rect.width - WbbSize.Group.RightReserve), rect.height);
            var titleHit = new Rect(titleRect.x, rect.y, rect.xMax - titleRect.x, rect.height);
            Chevron(arrowRect, open);
            if (marked)
            {
                MarkIcon(new Rect(titleRect.x, rect.y, WbbSize.Icon, rect.height));
                titleRect.x += WbbSize.IconAdvance;
                titleRect.width = Mathf.Max(WbbSize.Group.TitleMinWidth, titleRect.width - WbbSize.IconAdvance);
            }

            if (selectable)
            {
                bool all = selected >= total;
                bool some = selected > 0 && !all;
                bool next = Checkbox(checkRect, all, some);
                if (next != all)
                    toggledAll = true;
            }

            GUI.Label(titleRect, title, groupLabel);
            if (!string.IsNullOrEmpty(detail))
            {
                measure.text = title;
                measure.image = null;
                measure.tooltip = "";
                float used = groupLabel.CalcSize(measure).x;
                var detailRect = new Rect(titleRect.x + used + WbbSize.Group.DetailGap, rect.y, Mathf.Max(0f, rect.xMax - titleRect.x - used - WbbSize.Group.DetailRightPad), rect.height);
                GUI.Label(detailRect, detail, caption);
            }

            var openRect = new Rect(arrowRect.x, rect.y, arrowRect.width, rect.height);
            if (GUI.Button(openRect, GUIContent.none, GUIStyle.none) || GUI.Button(titleHit, GUIContent.none, GUIStyle.none))
                toggledOpen = true;
        }

        public static bool Button(GUIContent content, WbbButtonKind kind, params GUILayoutOption[] options)
        {
            EnsureStyles();
            Rect rect = LayoutButtonRect(content, kind == WbbButtonKind.Primary ? primaryText : buttonText, WbbSize.Control, options);
            return Button(rect, content, kind);
        }

        public static bool Button(Rect rect, GUIContent content, WbbButtonKind kind)
        {
            EnsureStyles();
            bool clicked = UI.SRGui.Button(rect, content, ToShared(kind), SizeFor(rect.height));
            if (!string.IsNullOrEmpty(content.tooltip))
                TooltipArea(rect, content.tooltip);
            return clicked;
        }

        public static bool Button(GUIContent content, UI.SRGlyph trailing, WbbButtonKind kind, params GUILayoutOption[] options)
        {
            EnsureStyles();
            GUIStyle measureStyle = kind == WbbButtonKind.Primary ? primaryText : buttonText;
            Rect rect = GUILayoutUtility.GetRect(Mathf.Ceil(measureStyle.CalcSize(content).x) + WbbSize.GlyphAdvance, WbbSize.Control, NoStretch(options));
            return Button(rect, content, trailing, kind);
        }

        public static bool Button(Rect rect, GUIContent content, UI.SRGlyph trailing, WbbButtonKind kind)
        {
            EnsureStyles();
            faceScratch.text = string.Empty;
            faceScratch.image = null;
            faceScratch.tooltip = content != null ? content.tooltip : string.Empty;
            bool clicked = Button(rect, faceScratch, kind);
            GUIStyle style = kind == WbbButtonKind.Primary ? primaryText : buttonText;
            int oldSize = style.fontSize;
            style.fontSize = SizeFor(rect.height) == UI.SRButtonSize.Mini ? WbbSize.FontButtonMini : WbbSize.FontButton;
            DrawTextWithGlyph(rect, content, style, ButtonInk(kind), trailing);
            style.fontSize = oldSize;
            return clicked;
        }

        public static float ButtonWidth(GUIContent content, UI.SRGlyph trailing)
        {
            return ButtonWidth(content) + WbbSize.GlyphAdvance;
        }

        public static bool Button(GUIContent content, string leadingIcon, WbbButtonKind kind, params GUILayoutOption[] options)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(ButtonWidth(content, leadingIcon), WbbSize.Control, NoStretch(options));
            return Button(rect, content, leadingIcon, kind);
        }

        public static bool Button(Rect rect, GUIContent content, string leadingIcon, WbbButtonKind kind)
        {
            EnsureStyles();
            faceScratch.text = string.Empty;
            faceScratch.image = null;
            faceScratch.tooltip = content != null ? content.tooltip : string.Empty;
            bool clicked = Button(rect, faceScratch, kind);
            if (Event.current.type != EventType.Repaint)
                return clicked;
            GUIStyle style = kind == WbbButtonKind.Primary ? primaryText : buttonText;
            int oldSize = style.fontSize;
            style.fontSize = SizeFor(rect.height) == UI.SRButtonSize.Mini ? WbbSize.FontButtonMini : WbbSize.FontButton;
            try
            {
                DrawIconWithText(rect, content, style, ButtonInk(kind), EditorIcon(leadingIcon));
            }
            finally
            {
                style.fontSize = oldSize;
            }

            return clicked;
        }

        public static float ButtonWidth(GUIContent content, string leadingIcon)
        {
            return ButtonWidth(content) + (EditorIcon(leadingIcon) != null ? WbbSize.IconAdvance : 0f);
        }

        private static void DrawIconWithText(Rect rect, GUIContent content, GUIStyle style, Color ink, Texture icon)
        {
            RectOffset pad = style.padding;
            float iconRoom = icon != null ? WbbSize.IconAdvance : 0f;
            float room = Mathf.Max(0f, rect.width - pad.horizontal - iconRoom);
            inkScratch.image = null;
            inkScratch.tooltip = string.Empty;
            inkScratch.text = content != null && content.text != null ? content.text : string.Empty;
            float textWidth = style.CalcSize(inkScratch).x - pad.horizontal;
            if (textWidth > room + FitSlack)
            {
                inkScratch.text = Fit(inkScratch.text, style, room + pad.horizontal);
                textWidth = Mathf.Min(room, style.CalcSize(inkScratch).x - pad.horizontal);
            }

            float left = rect.x + (rect.width - iconRoom - textWidth) * 0.5f;
            if (icon != null)
                Icon(new Rect(left, rect.y, WbbSize.Icon, rect.height), icon);
            var textRect = new Rect(left + iconRoom - pad.left, rect.y, textWidth + pad.horizontal, rect.height);
            Color old = style.normal.textColor;
            style.normal.textColor = ink;
            try
            {
                UI.SRGui.Ink(textRect, inkScratch, style);
            }
            finally
            {
                style.normal.textColor = old;
            }
        }

        private static readonly GUIContent faceScratch = new GUIContent();
        private static Color ButtonInk(WbbButtonKind kind)
        {
            WbbPalette p = WbbTheme.P;
            switch (kind)
            {
                case WbbButtonKind.Primary:
                    return p.AccentText;
                case WbbButtonKind.Danger:
                    return p.Error.Fg;
                case WbbButtonKind.Ghost:
                    return p.TextPrimary;
                default:
                    return p.TextStrong;
            }
        }

        private static Rect LayoutButtonRect(GUIContent content, GUIStyle textStyle, float height, GUILayoutOption[] options)
        {
            float width = Mathf.Ceil(textStyle.CalcSize(content).x);
            return GUILayoutUtility.GetRect(width, height, NoStretch(options));
        }

        private const float FitSlack = 0.5f;
        private static readonly GUILayoutOption noStretch = GUILayout.ExpandWidth(false);
        private static readonly GUILayoutOption[] noStretchOnly =
        {
            noStretch
        };
        private static GUILayoutOption[] NoStretch(GUILayoutOption[] options)
        {
            if (options == null || options.Length == 0)
                return noStretchOnly;
            var merged = new GUILayoutOption[options.Length + 1];
            merged[0] = noStretch;
            System.Array.Copy(options, 0, merged, 1, options.Length);
            return merged;
        }

        public static float ButtonWidth(GUIContent content)
        {
            EnsureStyles();
            return content == null ? 0f : Mathf.Ceil(buttonText.CalcSize(content).x);
        }

        public static float ToggleGroupWidth(GUIContent[] contents)
        {
            EnsureStyles();
            if (contents == null)
                return 0f;
            FontStyle oldFont = buttonText.fontStyle;
            buttonText.fontStyle = FontStyle.Bold;
            float width = 0f;
            try
            {
                for (int i = 0; i < contents.Length; i++)
                    if (contents[i] != null)
                        width = Mathf.Max(width, buttonText.CalcSize(contents[i]).x);
            }
            finally
            {
                buttonText.fontStyle = oldFont;
            }

            return Mathf.Ceil(width);
        }

        private static UI.SRButtonKind ToShared(WbbButtonKind kind)
        {
            switch (kind)
            {
                case WbbButtonKind.Primary:
                    return UI.SRButtonKind.Primary;
                case WbbButtonKind.Danger:
                    return UI.SRButtonKind.Danger;
                case WbbButtonKind.Ghost:
                    return UI.SRButtonKind.Ghost;
                default:
                    return UI.SRButtonKind.Secondary;
            }
        }

        private static UI.SRButtonSize SizeFor(float height)
        {
            if (height >= WbbSize.RunButton)
                return UI.SRButtonSize.Hero;
            if (height >= WbbSize.ControlLarge)
                return UI.SRButtonSize.Large;
            if (height <= WbbSize.ControlMini)
                return UI.SRButtonSize.Mini;
            return UI.SRButtonSize.Default;
        }

        private static void TooltipArea(Rect rect, string tooltip)
        {
            hitContent.text = "";
            hitContent.image = null;
            hitContent.tooltip = tooltip;
            GUI.Label(rect, hitContent, GUIStyle.none);
        }

        private static readonly GUIContent hitContent = new GUIContent();
        public static bool HitButton(Rect rect, string tooltip)
        {
            hitContent.text = "";
            hitContent.image = null;
            hitContent.tooltip = tooltip ?? "";
            return GUI.Button(rect, hitContent, GUIStyle.none);
        }

        public static bool ToggleButton(bool value, GUIContent content, params GUILayoutOption[] options)
        {
            EnsureStyles();
            FontStyle oldFont = buttonText.fontStyle;
            buttonText.fontStyle = value ? FontStyle.Bold : FontStyle.Normal;
            Rect rect = LayoutButtonRect(content, buttonText, WbbSize.ControlSmall, options);
            WbbPalette p = WbbTheme.P;
            DrawToggleFace(rect, value);
            Color old = buttonText.normal.textColor;
            buttonText.normal.textColor = value ? p.TextStrong : p.TextPrimary;
            UI.SRGui.Ink(rect, content, buttonText);
            buttonText.normal.textColor = old;
            buttonText.fontStyle = oldFont;
            if (HitButton(rect, content.tooltip))
                return !value;
            return value;
        }

        public static bool ToggleButton(bool value, GUIContent content, UI.SRGlyph trailing, params GUILayoutOption[] options)
        {
            return ToggleButton(value, content, trailing, float.MaxValue, options);
        }

        public static bool ToggleButton(bool value, GUIContent content, UI.SRGlyph trailing, float maxWidth, params GUILayoutOption[] options)
        {
            EnsureStyles();
            WbbPalette p = WbbTheme.P;
            FontStyle oldFont = buttonText.fontStyle;
            buttonText.fontStyle = value ? FontStyle.Bold : FontStyle.Normal;
            float width = Mathf.Min(Mathf.Ceil(buttonText.CalcSize(content).x) + WbbSize.GlyphAdvance, maxWidth);
            Rect rect = GUILayoutUtility.GetRect(width, WbbSize.ControlSmall, NoStretch(options));
            DrawToggleFace(rect, value);
            DrawTextWithGlyph(rect, content, buttonText, value ? p.TextStrong : p.TextPrimary, trailing);
            buttonText.fontStyle = oldFont;
            if (HitButton(rect, content.tooltip))
                return !value;
            return value;
        }

        public static bool ToggleIconButton(bool value, string iconName, string tooltip, params GUILayoutOption[] options)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(WbbSize.ControlSmall, WbbSize.ControlSmall, NoStretch(options));
            DrawToggleFace(rect, value);
            Texture icon = EditorIcon(iconName);
            if (icon != null)
            {
                Icon(rect, icon);
            }
            else
            {
                WbbPalette p = WbbTheme.P;
                inkScratch.text = IconFallback(iconName);
                inkScratch.image = null;
                inkScratch.tooltip = string.Empty;
                Color old = buttonText.normal.textColor;
                buttonText.normal.textColor = value ? p.TextStrong : p.TextPrimary;
                UI.SRGui.Ink(rect, inkScratch, buttonText);
                buttonText.normal.textColor = old;
            }

            if (HitButton(rect, tooltip))
                return !value;
            return value;
        }

        private static void DrawToggleFace(Rect rect, bool value)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            WbbPalette p = WbbTheme.P;
            bool enabled = GUI.enabled;
            bool hover = enabled && rect.Contains(Event.current.mousePosition);
            Color fill = !enabled ? p.BgCard : value ? p.ControlPressed : (hover ? p.ControlHover : p.Control);
            WbbTheme.Box(rect, fill, value && enabled ? p.AccentLine : p.BorderStrong, ControlRadius);
        }

        public static int Segmented(int current, GUIContent[] options, params GUILayoutOption[] layout)
        {
            EnsureStyles();
            if (options == null || options.Length == 0)
                return current;
            Rect rect = EditorGUILayout.GetControlRect(false, WbbSize.Control, layout);
            WbbPalette p = WbbTheme.P;
            WbbTheme.Box(rect, GUI.enabled ? p.Control : p.BgCard, p.BorderStrong, ControlRadius);
            const float inset = WbbSize.Field.SegmentInset;
            float w = rect.width / options.Length;
            int result = current;
            for (int i = 0; i < options.Length; i++)
            {
                var cell = new Rect(rect.x + w * i, rect.y, w, rect.height);
                bool on = i == current;
                bool hover = GUI.enabled && cell.Contains(Event.current.mousePosition);
                if (Event.current.type == EventType.Repaint)
                {
                    var face = new Rect(cell.x + inset, cell.y + inset, cell.width - inset * 2f, cell.height - inset * 2f);
                    if (on)
                        WbbTheme.Fill(face, p.BgSelected, ControlRadius - 1f);
                    else if (hover)
                        WbbTheme.Fill(face, p.ControlHover, ControlRadius - 1f);
                    if (i > 0)
                        EditorGUI.DrawRect(new Rect(cell.x, cell.y, WbbSize.Line, cell.height), p.BorderStrong);
                }

                GUIStyle style = on ? rowLabelBold : rowLabel;
                Color old = style.normal.textColor;
                TextAnchor oldAnchor = style.alignment;
                style.normal.textColor = on ? p.TextStrong : p.TextPrimary;
                style.alignment = TextAnchor.MiddleCenter;
                UI.SRGui.Ink(cell, options[i], style);
                style.normal.textColor = old;
                style.alignment = oldAnchor;
                if (HitButton(cell, options[i].tooltip))
                    result = i;
            }

            return result;
        }

        public static bool DangerButton(GUIContent content, params GUILayoutOption[] options)
        {
            return Button(content, WbbButtonKind.Danger, options);
        }

        public static bool PrimaryButton(GUIContent content, params GUILayoutOption[] options)
        {
            return Button(content, WbbButtonKind.Primary, options);
        }

        public static bool MiniButton(GUIContent content, params GUILayoutOption[] options)
        {
            EnsureStyles();
            Rect rect = LayoutButtonRect(content, buttonText, WbbSize.ControlMini, options);
            return Button(rect, content, WbbButtonKind.Secondary);
        }

        public static bool MiniButton(Rect rect, GUIContent content)
        {
            return Button(rect, content, WbbButtonKind.Secondary);
        }

        public static bool PlainButton(GUIContent content, params GUILayoutOption[] options)
        {
            EnsureStyles();
            Rect rect = LayoutButtonRect(content, buttonText, WbbSize.Control, options);
            return Button(rect, content, WbbButtonKind.Secondary);
        }

        private static readonly GUIContent plainScratch = new GUIContent();
        public static bool PlainButton(string text, params GUILayoutOption[] options)
        {
            plainScratch.text = text ?? "";
            plainScratch.image = null;
            plainScratch.tooltip = "";
            return PlainButton(plainScratch, options);
        }

        private static int fieldSerial;
        private static string NextFieldName()
        {
            fieldSerial = (fieldSerial + 1) & 0xFFFF;
            return "Wbb.Field." + fieldSerial;
        }

        public static string TextField(string value, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, WbbSize.LineHeight, options);
            return DrawTextField(rect, value, false);
        }

        public static string DelayedTextField(string value, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, WbbSize.LineHeight, options);
            return DrawTextField(rect, value, true);
        }

        public static string TextField(Rect rect, string value)
        {
            return DrawTextField(rect, value, false);
        }

        public static string TextField(Rect rect, string value, string controlName)
        {
            return DrawTextField(rect, value, false, controlName);
        }

        public static string LabeledTextField(GUIContent label, string value, string controlName = null)
        {
            EnsureStyles();
            Rect row = EditorGUILayout.GetControlRect(false, WbbSize.LineHeight);
            float labelWidth = Mathf.Min(EditorGUIUtility.labelWidth, row.width);
            WbbPalette p = WbbTheme.P;
            Color old = rowLabel.normal.textColor;
            rowLabel.normal.textColor = p.TextPrimary;
            UI.SRGui.Ink(new Rect(row.x, row.y, labelWidth, row.height), label, rowLabel);
            rowLabel.normal.textColor = old;
            var field = new Rect(row.x + labelWidth, row.y, Mathf.Max(0f, row.width - labelWidth), row.height);
            return DrawTextField(field, value, false, controlName);
        }

        private static string DrawTextField(Rect rect, string value, bool delayed, string controlName = null)
        {
            EnsureStyles();
            WbbPalette p = WbbTheme.P;
            WbbTheme.Box(rect, p.BgSunken, p.BorderStrong, WbbSize.RadiusInput);
            SetTextColor(fieldText, GUI.enabled ? p.TextPrimary : UI.SRTheme.DisabledInk(p.BgSunken));
            string name = string.IsNullOrEmpty(controlName) ? NextFieldName() : controlName;
            GUI.SetNextControlName(name);
            string current = value ?? "";
            string next = delayed ? EditorGUI.DelayedTextField(rect, current, fieldText) : EditorGUI.TextField(rect, current, fieldText);
            if (GUI.GetNameOfFocusedControl() == name)
                WbbTheme.Stroke(rect, p.Focus, WbbSize.RadiusInput, WbbSize.Line);
            return next;
        }

        public static float PopupWidth(string[] options)
        {
            EnsureStyles();
            float width = 0f;
            if (options == null)
                return width;
            measure.image = null;
            measure.tooltip = string.Empty;
            for (int i = 0; i < options.Length; i++)
            {
                measure.text = options[i] ?? string.Empty;
                width = Mathf.Max(width, popupText.CalcSize(measure).x);
            }

            return Mathf.Ceil(width);
        }

        public static int Popup(int index, string[] options, params GUILayoutOption[] layout)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, WbbSize.LineHeight, layout);
            return Dropdown(rect, index, options);
        }

        public static int Popup(int index, GUIContent[] options, params GUILayoutOption[] layout)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, WbbSize.LineHeight, layout);
            return Dropdown(rect, index, options);
        }

        public static int Dropdown(Rect rect, int index, string[] options)
        {
            DropdownFace(rect);
            int result = EditorGUI.Popup(rect, index, options, popupText);
            DropdownArrow(rect);
            return result;
        }

        public static int Dropdown(Rect rect, int index, GUIContent[] options)
        {
            DropdownFace(rect);
            int result = EditorGUI.Popup(rect, index, options, popupText);
            DropdownArrow(rect);
            return result;
        }

        private static void DropdownFace(Rect rect)
        {
            EnsureStyles();
            WbbPalette p = WbbTheme.P;
            bool hover = GUI.enabled && rect.Contains(Event.current.mousePosition);
            Color face = !GUI.enabled ? p.BgCard : (hover ? p.ControlHover : p.Control);
            WbbTheme.Box(rect, face, p.BorderStrong, WbbSize.RadiusInput);
            SetTextColor(popupText, GUI.enabled ? p.TextStrong : UI.SRTheme.DisabledInk(face));
        }

        private static void DropdownArrow(Rect rect)
        {
            Chevron(new Rect(rect.xMax - WbbSize.Field.DropdownArrowWidth, rect.y, WbbSize.Glyph, rect.height), true);
        }

        public static bool Checkbox(Rect rect, bool value, bool mixed = false)
        {
            EnsureStyles();
            bool oldMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixed;
            bool result = EditorGUI.Toggle(rect, value, GUIStyle.none);
            EditorGUI.showMixedValue = oldMixed;
            DrawCheckFace(new Rect(rect.x + (rect.width - WbbSize.CheckBox) * 0.5f, rect.y + (rect.height - WbbSize.CheckBox) * 0.5f, WbbSize.CheckBox, WbbSize.CheckBox), result, mixed);
            return result;
        }

        public static bool ToggleLeft(GUIContent label, bool value, params GUILayoutOption[] options)
        {
            EnsureStyles();
            Rect row = EditorGUILayout.GetControlRect(false, WbbSize.LineHeight, options);
            bool mixed = EditorGUI.showMixedValue;
            bool result = EditorGUI.Toggle(row, value, GUIStyle.none);
            DrawCheckFace(new Rect(row.x + (WbbSize.CheckSize - WbbSize.CheckBox) * 0.5f, row.y + (row.height - WbbSize.CheckBox) * 0.5f, WbbSize.CheckBox, WbbSize.CheckBox), result, mixed);
            var labelRect = new Rect(row.x + WbbSize.CheckSize + WbbSize.SpaceSmall, row.y, Mathf.Max(0f, row.width - WbbSize.CheckSize - WbbSize.SpaceSmall), row.height);
            WbbPalette p = WbbTheme.P;
            Color old = rowLabel.normal.textColor;
            rowLabel.normal.textColor = p.TextPrimary;
            UI.SRGui.Ink(labelRect, label, rowLabel);
            rowLabel.normal.textColor = old;
            return result;
        }

        public static bool ToggleLeftFit(GUIContent label, bool value)
        {
            return ToggleLeft(label, value, GUILayout.Width(ToggleLeftFitWidth(label)));
        }

        public static float ToggleLeftFitWidth(GUIContent label)
        {
            EnsureStyles();
            return WbbSize.CheckSize + WbbSize.SpaceSmall + (label == null ? 0f : rowLabel.CalcSize(label).x);
        }

        private static void DrawCheckFace(Rect box, bool on, bool mixed)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            WbbPalette p = WbbTheme.P;
            bool enabled = GUI.enabled;
            if (on && !mixed)
            {
                WbbTheme.Box(box, enabled ? p.Accent : p.TextDisabled, p.AccentBorder, WbbSize.RadiusInput);
                var tick = new Rect(box.center.x - WbbSize.Glyph * 0.5f, box.center.y - WbbSize.Glyph * 0.5f, WbbSize.Glyph, WbbSize.Glyph);
                UI.SRGui.Glyph(tick, UI.SRGlyph.Check, p.AccentText, WbbSize.GlyphStrokeBold);
                return;
            }

            WbbTheme.Box(box, p.BgSunken, enabled ? p.BorderMark : p.BorderStrong, WbbSize.RadiusInput);
            if (mixed)
            {
                const float inset = WbbSize.Field.CheckDashInset;
                WbbTheme.Fill(new Rect(box.x + inset, box.center.y - WbbSize.Line, box.width - inset * 2f, WbbSize.Line * 2f), p.TextPrimary);
            }
        }

        public static bool Foldout(bool open, GUIContent label)
        {
            EnsureStyles();
            Rect row = EditorGUILayout.GetControlRect(false, WbbSize.Section.HeaderHeight);
            SectionSurface(row);
            float x = row.x + WbbSize.Section.HeaderInset;
            float y = row.y + (row.height - WbbSize.LineHeight) * 0.5f;
            Chevron(new Rect(x, y, WbbSize.Group.ArrowWidth, WbbSize.LineHeight), open);
            x += WbbSize.Group.ArrowWidth + WbbSize.SpaceSmall;
            GUI.Label(new Rect(x, y, Mathf.Max(0f, row.xMax - x - WbbSize.SpaceSmall), WbbSize.LineHeight), label, cardTitle);
            if (GUI.Button(row, GUIContent.none, GUIStyle.none))
            {
                GUIUtility.keyboardControl = 0;
                return !open;
            }

            return open;
        }

        public static bool FoldoutFit(bool open, GUIContent label)
        {
            EnsureStyles();
            float width = WbbSize.Group.ArrowWidth + WbbSize.SpaceSmall + cardTitle.CalcSize(label).x;
            Rect row = EditorGUILayout.GetControlRect(false, WbbSize.LineHeight, GUILayout.Width(width));
            Chevron(new Rect(row.x, row.y, WbbSize.Group.ArrowWidth, row.height), open);
            GUI.Label(new Rect(row.x + WbbSize.Group.ArrowWidth + WbbSize.SpaceSmall, row.y, Mathf.Max(0f, row.width - WbbSize.Group.ArrowWidth - WbbSize.SpaceSmall), row.height), label, cardTitle);
            if (GUI.Button(row, GUIContent.none, GUIStyle.none))
            {
                GUIUtility.keyboardControl = 0;
                return !open;
            }

            return open;
        }

        public static bool FoldoutArrow(Rect rect, bool open)
        {
            EnsureStyles();
            Chevron(rect, open);
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                return !open;
            return open;
        }

        public static void DropArea(Rect rect, GUIContent hint, bool active)
        {
            EnsureStyles();
            WbbPalette p = WbbTheme.P;
            WbbTheme.Box(rect, active ? p.BgSelected : p.OverlayDropArea, active ? p.AccentLine : p.DropAreaLine, WbbSize.RadiusControl);
            GUI.Label(rect, hint, dropHint);
        }

        private static Texture searchIcon;
        private static int searchIconState = -1;
        private static void DrawSearchIcon(Rect cell)
        {
            // IconContent returns the variant for the current skin and screen scale, so the cache follows both like
            // EditorIcon's; otherwise a Dark to Light theme switch keeps the pale icon on the light field.
            int state = (Dark ? 1 : 0) | (EditorGUIUtility.pixelsPerPoint > 1f ? 2 : 0);
            if (searchIconState != state)
            {
                searchIconState = state;
                try
                {
                    GUIContent found = EditorGUIUtility.IconContent("Search Icon");
                    searchIcon = found != null ? found.image : null;
                }
                catch (System.Exception)
                {
                    searchIcon = null;
                }
            }

            if (searchIcon == null)
            {
                GUI.Label(cell, "○", glyph);
                return;
            }

            if (Event.current.type != EventType.Repaint)
                return;
            float size = Mathf.Min(cell.width, WbbSize.Field.SearchGlyphWidth);
            var icon = new Rect(cell.x + (cell.width - size) * 0.5f, cell.y + (cell.height - size) * 0.5f, size, size);
            GUI.DrawTexture(icon, searchIcon, ScaleMode.ScaleToFit, true);
        }

        public static string SearchField(string value, float minWidth = WbbSize.Field.SearchMinWidth)
        {
            EnsureStyles();
            if (clearContent == null || clearContentVersion != WbbLang.Version)
            {
                clearContentVersion = WbbLang.Version;
                clearContent = new GUIContent(string.Empty, L("Common.SearchClear"));
            }

            string current = value == null ? "" : value;
            EditorGUILayout.BeginHorizontal();
            Rect r = EditorGUILayout.GetControlRect(false, WbbSize.Control, GUILayout.MinWidth(Mathf.Max(WbbSize.Field.SearchFieldMinWidth, minWidth)));
            WbbPalette p = WbbTheme.P;
            WbbTheme.Box(r, p.BgSunken, p.BorderStrong, r.height * 0.5f);
            DrawSearchIcon(new Rect(r.x + WbbSize.Field.SearchGlyphLeft, r.y, WbbSize.Field.SearchGlyphWidth, r.height));
            var textRect = new Rect(r.x + WbbSize.Field.SearchTextLeft, r.y + 1f, Mathf.Max(0f, r.width - WbbSize.Field.SearchTextLeft - WbbSize.Field.SearchTextRight), r.height - 2f);
            string name = NextFieldName();
            Event e = Event.current;
            SetTextColor(searchText, GUI.enabled ? p.TextPrimary : UI.SRTheme.DisabledInk(p.BgSunken));
            GUI.SetNextControlName(name);
            string next = EditorGUI.TextField(textRect, current, searchText);
            bool focused = GUI.GetNameOfFocusedControl() == name;
            if (focused)
            {
                WbbTheme.Stroke(r, p.Focus, r.height * 0.5f, WbbSize.Line);
                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
                {
                    next = "";
                    GUIUtility.keyboardControl = 0;
                    EditorGUIUtility.editingTextField = false;
                    e.Use();
                }
            }

            if (string.IsNullOrEmpty(next) && !focused && e.type == EventType.Repaint)
                GUI.Label(textRect, L("Common.Search"), searchPlaceholder);
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(next)))
            {
                const float clearWidth = WbbSize.Field.SearchClearWidth;
                Rect clearRect = GUILayoutUtility.GetRect(clearWidth, WbbSize.Control, GUILayout.Width(clearWidth), GUILayout.Height(WbbSize.Control));
                if (IconButton(clearRect, UI.SRGlyph.Close, clearContent.tooltip, WbbButtonKind.Ghost))
                {
                    next = "";
                    GUIUtility.keyboardControl = 0;
                    EditorGUIUtility.editingTextField = false;
                }
            }

            EditorGUILayout.EndHorizontal();
            return next;
        }

        public static void ProgressBar(float progress01, string label, float height = WbbSize.Field.ProgressHeight, WbbTone tone = WbbTone.Running)
        {
            EnsureStyles();
            Rect r = GUILayoutUtility.GetRect(0f, Mathf.Max(height, WbbSize.Field.ProgressHeight), GUILayout.ExpandWidth(true));
            if (Event.current.type != EventType.Repaint)
                return;
            WbbPalette p = WbbTheme.P;
            float t = Mathf.Clamp01(progress01);
            const float pctWidth = WbbSize.Field.ProgressPercentWidth;
            const float labelHeight = WbbSize.Field.ProgressLabelHeight;
            if (!string.IsNullOrEmpty(label))
                GUI.Label(new Rect(r.x, r.y, Mathf.Max(0f, r.width - pctWidth), labelHeight), Fit(label, caption, r.width - pctWidth), caption);
            GUI.Label(new Rect(r.xMax - pctWidth, r.y, pctWidth, labelHeight), Mathf.RoundToInt(t * 100f) + "%", progressPercent);
            const float barHeight = WbbSize.Field.ProgressBarHeight;
            var bar = new Rect(r.x, r.y + WbbSize.Field.ProgressBarTop, r.width, barHeight);
            WbbTheme.Box(bar, p.BgSunken, p.BorderStrong, barHeight * 0.5f);
            Color fill = tone == WbbTone.Success || tone == WbbTone.Error || tone == WbbTone.Warning ? WbbTheme.Tone(tone).Base : p.Mint;
            if (t > 0f)
                WbbTheme.Fill(new Rect(bar.x, bar.y, bar.width * t, bar.height), fill, barHeight * 0.5f);
        }

        public static bool EmptyState(string title, string message, string actionLabel, float minHeight = WbbSize.EmptyStateHeight)
        {
            EnsureStyles();
            return UI.SRGui.EmptyState(title ?? "", message ?? "", actionLabel, minHeight);
        }

        public static void SectionSurface(Rect row)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            WbbPalette p = WbbTheme.P;
            WbbTheme.Fill(row, p.BgPanel);
            WbbTheme.Fill(new Rect(row.x, row.y, row.width, WbbSize.Line), p.BorderSubtle);
            WbbTheme.Fill(new Rect(row.x, row.yMax - WbbSize.Line, row.width, WbbSize.Line), p.BorderSubtle);
        }

        public const string IconMenu = "_Menu";
        public const string IconTree = "UnityEditor.SceneHierarchyWindow";
        public const string IconStar = "Favorite";
        public const string IconCamera = "SceneViewCamera";
        public const string IconPlay = "PlayButton";
        public const string IconHide = "scenevis_hidden";
        public const string IconUnhide = "scenevis_visible";
        public const string IconPin = "Toolbar Plus";
        public const string IconRefresh = "Refresh";
        public const string IconPrefab = "Prefab Icon";
        public const string IconScene = "UnityEditor.SceneView";
        public const string IconTrash = "TreeEditor.Trash";
        private static readonly GUIContent rowTextScratch = new GUIContent();
        public static void RowText(Rect rect, GUIContent content, GUIStyle style, bool dimmed)
        {
            if (!dimmed)
            {
                GUI.Label(rect, content, style);
                return;
            }

            Color normal = style.normal.textColor;
            Color hover = style.hover.textColor;
            Color ink = WbbTheme.P.TextDisabled;
            style.normal.textColor = ink;
            style.hover.textColor = ink;
            try
            {
                GUI.Label(rect, content, style);
            }
            finally
            {
                style.normal.textColor = normal;
                style.hover.textColor = hover;
            }
        }

        public static void RowText(Rect rect, string text, GUIStyle style, bool dimmed)
        {
            rowTextScratch.text = text ?? string.Empty;
            rowTextScratch.image = null;
            rowTextScratch.tooltip = string.Empty;
            RowText(rect, rowTextScratch, style, dimmed);
        }

        private static readonly System.Collections.Generic.Dictionary<string, Texture> iconCache = new System.Collections.Generic.Dictionary<string, Texture>(System.StringComparer.Ordinal);
        private static int iconCacheState = -1;
        private static readonly GUIContent iconScratch = new GUIContent();
        private static readonly GUIContent inkScratch = new GUIContent();
        public static Texture EditorIcon(string lightName)
        {
            if (string.IsNullOrEmpty(lightName))
                return null;
            bool dark = Dark;
            bool hiDpi = EditorGUIUtility.pixelsPerPoint > 1f;
            int state = (dark ? 1 : 0) | (hiDpi ? 2 : 0);
            if (state != iconCacheState)
            {
                iconCache.Clear();
                iconCacheState = state;
            }

            Texture found;
            if (iconCache.TryGetValue(lightName, out found) && (found != null || ReferenceEquals(found, null)))
                return found;
            found = null;
            if (dark && hiDpi)
                found = FindIcon("d_" + lightName + "@2x");
            if (found == null && dark)
                found = FindIcon("d_" + lightName);
            if (found == null && hiDpi)
                found = FindIcon(lightName + "@2x");
            if (found == null)
                found = FindIcon(lightName);
            iconCache[lightName] = found;
            return found;
        }

        private static Texture FindIcon(string name)
        {
            try
            {
                Texture2D tex = EditorGUIUtility.FindTexture(name);
                return tex != null ? tex : null;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static string IconFallback(string iconName)
        {
            switch (iconName)
            {
                case IconMenu:
                    return "...";
                case IconTree:
                    return "=";
                case IconStar:
                    return "*";
                case IconCamera:
                    return "+";
                case IconPlay:
                    return ">";
                case IconHide:
                    return "-";
                case IconUnhide:
                    return "o";
                case IconPin:
                    return "+";
                case IconRefresh:
                    return "r";
                case IconPrefab:
                    return string.Empty;
                case IconScene:
                    return string.Empty;
                case IconTrash:
                    return string.Empty;
                default:
                    return "?";
            }
        }

        public static void Glyph(Rect cell, UI.SRGlyph glyph)
        {
            Glyph(cell, glyph, WbbTheme.P.TextSecondary);
        }

        public static void Glyph(Rect cell, UI.SRGlyph glyph, Color color)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;
            float size = Mathf.Min(WbbSize.Glyph, Mathf.Min(cell.width, cell.height));
            if (size <= 0f)
                return;
            var box = new Rect(cell.x + (cell.width - size) * 0.5f, cell.y + (cell.height - size) * 0.5f, size, size);
            UI.SRGui.Glyph(box, glyph, GUI.enabled ? color : WbbTheme.P.TextDisabled, WbbSize.GlyphStroke);
        }

        public static void Chevron(Rect cell, bool open)
        {
            Glyph(cell, open ? UI.SRGlyph.ChevronDown : UI.SRGlyph.ChevronRight);
        }

        public static void Icon(Rect cell, Texture icon)
        {
            Icon(cell, icon, GUI.enabled ? 1f : WbbTheme.DisabledIconAlpha);
        }

        public static void Icon(Rect cell, Texture icon, float alpha)
        {
            if (icon == null || Event.current == null || Event.current.type != EventType.Repaint)
                return;
            float size = Mathf.Min(WbbSize.Icon, Mathf.Min(cell.width, cell.height));
            if (size <= 0f)
                return;
            var box = new Rect(cell.x + (cell.width - size) * 0.5f, cell.y + (cell.height - size) * 0.5f, size, size);
            DrawTextureAlpha(box, icon, alpha);
        }

        public static void DrawTextureAlpha(Rect rect, Texture texture, float alpha)
        {
            if (texture == null || Event.current == null || Event.current.type != EventType.Repaint)
                return;
            Color old = GUI.color;
            if (alpha < 1f)
                GUI.color = WbbTheme.ScaleAlpha(old, alpha);
            try
            {
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
            }
            finally
            {
                GUI.color = old;
            }
        }

        public static void MarkIcon(Rect cell)
        {
            Texture star = EditorIcon(IconStar);
            if (star != null)
            {
                Icon(cell, star);
                return;
            }

            if (Event.current.type != EventType.Repaint)
                return;
            EnsureStyles();
            GUI.Label(cell, IconFallback(IconStar), glyph);
        }

        public static bool IconButton(Rect rect, UI.SRGlyph glyph, string tooltip, WbbButtonKind kind = WbbButtonKind.Ghost)
        {
            EnsureStyles();
            return UI.SRGui.IconButton(rect, glyph, tooltip ?? string.Empty, ToShared(kind));
        }

        public static bool IconButton(UI.SRGlyph glyph, string tooltip, WbbButtonKind kind, params GUILayoutOption[] options)
        {
            Rect rect = GUILayoutUtility.GetRect(WbbSize.Control, WbbSize.Control, NoStretch(options));
            return IconButton(rect, glyph, tooltip, kind);
        }

        public static bool IconButton(Rect rect, string iconName, string tooltip, WbbButtonKind kind = WbbButtonKind.Ghost)
        {
            Texture icon = EditorIcon(iconName);
            iconScratch.image = icon;
            iconScratch.text = icon != null ? string.Empty : IconFallback(iconName);
            iconScratch.tooltip = tooltip ?? string.Empty;
            return Button(rect, iconScratch, kind);
        }

        public static bool IconButton(string iconName, string tooltip, WbbButtonKind kind, params GUILayoutOption[] options)
        {
            Rect rect = GUILayoutUtility.GetRect(WbbSize.Control, WbbSize.Control, NoStretch(options));
            return IconButton(rect, iconName, tooltip, kind);
        }

        private static void DrawTextWithGlyph(Rect rect, GUIContent content, GUIStyle style, Color ink, UI.SRGlyph glyphKind)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            RectOffset pad = style.padding;
            float room = Mathf.Max(0f, rect.width - pad.horizontal - WbbSize.GlyphAdvance);
            inkScratch.image = null;
            inkScratch.tooltip = string.Empty;
            inkScratch.text = content != null && content.text != null ? content.text : string.Empty;
            float textWidth = style.CalcSize(inkScratch).x - pad.horizontal;
            if (textWidth > room + FitSlack)
            {
                inkScratch.text = Fit(inkScratch.text, style, room + pad.horizontal);
                textWidth = Mathf.Min(room, style.CalcSize(inkScratch).x - pad.horizontal);
            }

            float left = rect.x + (rect.width - textWidth - WbbSize.GlyphAdvance) * 0.5f;
            var textRect = new Rect(left - pad.left, rect.y, textWidth + pad.horizontal, rect.height);
            var glyphRect = new Rect(left + textWidth + WbbSize.GlyphGap, rect.y, WbbSize.Glyph, rect.height);
            Color old = style.normal.textColor;
            style.normal.textColor = ink;
            UI.SRGui.Ink(textRect, inkScratch, style);
            style.normal.textColor = old;
            Glyph(glyphRect, glyphKind, ink);
        }

        public static string Fit(string text, GUIStyle style, float maxWidth)
        {
            if (string.IsNullOrEmpty(text) || style == null)
                return text;
            if (maxWidth <= WbbSize.Field.FitMinWidth)
                return "";
            measure.tooltip = string.Empty;
            measure.image = null;
            measure.text = text;
            if (style.CalcSize(measure).x <= maxWidth)
                return text;
            int low = 0;
            int high = text.Length;
            string best = "…";
            while (low <= high)
            {
                int mid = (low + high) / 2;
                measure.text = text.Substring(0, mid) + "…";
                if (style.CalcSize(measure).x <= maxWidth)
                {
                    best = measure.text;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return best;
        }
    }
}
