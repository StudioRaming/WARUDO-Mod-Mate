using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder.UI
{
    public enum SRTone
    {
        Neutral,
        Brand,
        Success,
        Warning,
        Danger,
        Info
    }

    public enum SRProduct
    {
        MingToon,
        Physics,
        CharacterToolbox,
        MaskMaker,
        EasyMeshEditor,
        WarudoBatchBuilder
    }

    public enum SRButtonKind
    {
        Primary,
        Secondary,
        Ghost,
        Danger
    }

    public enum SRButtonSize
    {
        Mini,
        Default,
        Large,
        Hero
    }

    public enum SRGlyph
    {
        Check,
        ChevronDown,
        ChevronRight,
        Close,
        Search
    }

    public enum SRLang
    {
        Korean = 0,
        English = 1,
        Japanese = 2
    }

    public static class SRLanguage
    {
        public const string SharedKey = "StudioRaming.Language";
        private static int cached = -1;
        private static int version;
        public static int Version
        {
            get
            {
                return version;
            }
        }

        public static SRLang Get(string productKey)
        {
            if (cached < 0)
            {
                int fallback = Application.systemLanguage == SystemLanguage.Korean ? 0 : Application.systemLanguage == SystemLanguage.Japanese ? 2 : 1;
                if (EditorPrefs.HasKey(SharedKey))
                {
                    cached = EditorPrefs.GetInt(SharedKey, fallback);
                }
                else
                {
                    cached = string.IsNullOrEmpty(productKey) ? fallback : EditorPrefs.GetInt(productKey, fallback);
                    EditorPrefs.SetInt(SharedKey, cached);
                }
            }

            return (SRLang)Mathf.Clamp(cached, 0, 2);
        }

        public static event Action Changed;
        private static readonly System.Collections.Generic.List<Type> windowTypes = new System.Collections.Generic.List<Type>();
        public static void RegisterWindowType(Type windowType)
        {
            if (windowType != null && !windowTypes.Contains(windowType))
                windowTypes.Add(windowType);
        }

        public static void Set(SRLang value, string productKey)
        {
            cached = (int)value;
            version++;
            EditorPrefs.SetInt(SharedKey, (int)value);
            if (!string.IsNullOrEmpty(productKey))
                EditorPrefs.SetInt(productKey, (int)value);
            RepaintStudioRamingWindows();
            if (Changed != null)
                Changed();
        }

        public static void Invalidate()
        {
            if (cached < 0)
                return;
            int before = cached;
            cached = -1;
            if (EditorPrefs.HasKey(SharedKey) && EditorPrefs.GetInt(SharedKey, before) != before)
                version++;
        }

        public static string Pick(string productKey, string ko, string en, string ja)
        {
            switch (Get(productKey))
            {
                case SRLang.Korean:
                    return ko;
                case SRLang.Japanese:
                    return ja;
                default:
                    return en;
            }
        }

        private static void RepaintStudioRamingWindows()
        {
            foreach (EditorWindow w in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                Type type = w.GetType();
                string ns = type.Namespace;
                bool ours = ns != null && ns.StartsWith("StudioRaming", StringComparison.Ordinal);
                for (int i = 0; !ours && i < windowTypes.Count; i++)
                    ours = windowTypes[i].IsAssignableFrom(type);
                if (ours)
                    w.Repaint();
            }
        }
    }

    public struct SRToneColors
    {
        public Color Mark;
        public Color Text;
        public Color Bg;
        public Color Border;
        public SRToneColors(Color mark, Color text, Color bg, Color border)
        {
            Mark = mark;
            Text = text;
            Bg = bg;
            Border = border;
        }
    }

    public sealed class SRPalette
    {
        public Color SurfaceApp;
        public Color SurfaceWindow;
        public Color SurfaceCard;
        public Color SurfaceSection;
        public Color SurfaceHover;
        public Color SurfaceSunken;
        public Color Control;
        public Color ControlHover;
        public Color ControlPressed;
        public Color LineSubtle;
        public Color LineControl;
        public Color LineStrong;
        public Color TextStrong;
        public Color Text;
        public Color TextMuted;
        public Color TextDisabled;
        public Color Brand;
        public Color BrandHover;
        public Color BrandPressed;
        public Color BrandBorder;
        public Color BrandLine;
        public Color TextOnBrand;
        public Color BrandText;
        public Color Mint;
        public Color Focus;
        public Color Link;
        public Color BrandTint;
        public Color Success;
        public Color SuccessText;
        public Color SuccessBg;
        public Color SuccessBorder;
        public Color Warning;
        public Color WarningText;
        public Color WarningBg;
        public Color WarningBorder;
        public Color Danger;
        public Color DangerText;
        public Color DangerBg;
        public Color DangerBorder;
        public Color Info;
        public Color InfoText;
        public Color InfoBg;
        public Color InfoBorder;
        public Color ProductMingtoon;
        public Color ProductPhysics;
        public Color ProductCtb;
        public Color ProductMm;
        public Color ProductEme;
        public Color ProductWbb;
        public Color SceneAdd;
        public Color SceneRemove;
        public Color SceneHintBg;
        public Color SceneHintText;
        public Color Scrim;
    }

    public static class SRMetrics
    {
        public const float Line = 18f;
        public const float Spacing = 2f;
        public const float Row = 20f;
        public const float ListRow = 22f;
        public const float ButtonMini = 18f;
        public const float Button = 20f;
        public const float ButtonLarge = 24f;
        public const float ButtonHero = 32f;
        public const float Tab = 24f;
        public const float Header = 44f;
        public const float AccentLine = 2f;
        public const float Indent = 15f;
        public const float LabelMin = 120f;
        public const float InspectorMin = 275f;
        public const float RadiusControl = 3f;
        public const float RadiusButton = 4f;
        public const float RadiusCard = 6f;
        public const float RadiusPill = 9f;
        public const float Space4 = 4f;
        public const float Space6 = 6f;
        public const float Space8 = 8f;
        public const float Space12 = 12f;
        public const float Space16 = 16f;
        public const int FontHero = 19;
        public const int FontTitle = 14;
        public const int FontBody = 12;
        public const int FontSmall = 11;
        public const int FontMini = 10;
        public static float LabelWidth(float contextWidth)
        {
            return Mathf.Max(contextWidth * 0.45f - 40f, LabelMin);
        }
    }

    public static class SRTheme
    {
        private static SRPalette dark;
        private static SRPalette light;
        public static bool Dark
        {
            get
            {
                return EditorGUIUtility.isProSkin;
            }
        }

        public static SRPalette P
        {
            get
            {
                if (Dark)
                {
                    if (dark == null)
                        dark = BuildDark();
                    return dark;
                }

                if (light == null)
                    light = BuildLight();
                return light;
            }
        }

        public static SRToneColors Tone(SRTone tone)
        {
            SRPalette p = P;
            switch (tone)
            {
                case SRTone.Brand:
                    return new SRToneColors(p.Brand, p.TextStrong, p.BrandTint, p.BrandBorder);
                case SRTone.Success:
                    return new SRToneColors(p.Success, p.SuccessText, p.SuccessBg, p.SuccessBorder);
                case SRTone.Warning:
                    return new SRToneColors(p.Warning, p.WarningText, p.WarningBg, p.WarningBorder);
                case SRTone.Danger:
                    return new SRToneColors(p.Danger, p.DangerText, p.DangerBg, p.DangerBorder);
                case SRTone.Info:
                    return new SRToneColors(p.Info, p.InfoText, p.InfoBg, p.InfoBorder);
                default:
                    return new SRToneColors(p.LineStrong, p.TextMuted, p.SurfaceSection, p.LineControl);
            }
        }

        public static Color Product(SRProduct product)
        {
            SRPalette p = P;
            switch (product)
            {
                case SRProduct.Physics:
                    return p.ProductPhysics;
                case SRProduct.CharacterToolbox:
                    return p.ProductCtb;
                case SRProduct.MaskMaker:
                    return p.ProductMm;
                case SRProduct.EasyMeshEditor:
                    return p.ProductEme;
                case SRProduct.WarudoBatchBuilder:
                    return p.ProductWbb;
                default:
                    return p.ProductMingtoon;
            }
        }

        public static void Fill(Rect rect, Color color, float radius = 0f)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;
            if (rect.width <= 0f || rect.height <= 0f || color.a <= 0f)
                return;
            if (radius <= 0f)
            {
                EditorGUI.DrawRect(rect, color);
                return;
            }

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, radius);
        }

        public static void Stroke(Rect rect, Color color, float radius = 0f, float width = 1f)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;
            if (rect.width <= 0f || rect.height <= 0f || color.a <= 0f)
                return;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, width, radius);
        }

        public static void Box(Rect rect, Color fill, Color border, float radius)
        {
            Fill(rect, fill, radius);
            Stroke(rect, border, radius, 1f);
        }

        public static string Hex(Color c)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(c);
        }

        public static Color DisabledInk(Color face)
        {
            Color t = P.TextDisabled;
            return new Color(Mathf.Clamp01(2f * t.r - face.r), Mathf.Clamp01(2f * t.g - face.g), Mathf.Clamp01(2f * t.b - face.b), 1f);
        }

        private static SRPalette BuildDark()
        {
            var p = new SRPalette();
            p.SurfaceApp = new Color32(34, 25, 30, 255);
            p.SurfaceWindow = new Color32(62, 53, 59, 255);
            p.SurfaceCard = new Color32(70, 61, 67, 255);
            p.SurfaceSection = new Color32(77, 68, 74, 255);
            p.SurfaceHover = new Color32(85, 75, 81, 255);
            p.SurfaceSunken = new Color32(46, 40, 44, 255);
            p.Control = new Color32(94, 85, 90, 255);
            p.ControlHover = new Color32(102, 93, 99, 255);
            p.ControlPressed = new Color32(96, 72, 81, 255);
            p.LineSubtle = new Color32(45, 39, 43, 255);
            p.LineControl = new Color32(36, 31, 34, 255);
            p.LineStrong = new Color32(155, 146, 151, 255);
            p.TextStrong = new Color32(244, 238, 236, 255);
            p.Text = new Color32(213, 207, 210, 255);
            p.TextMuted = new Color32(185, 178, 182, 255);
            p.TextDisabled = new Color32(150, 144, 148, 255);
            p.Brand = new Color32(215, 154, 170, 255);
            p.BrandHover = new Color32(227, 168, 183, 255);
            p.BrandPressed = new Color32(202, 139, 156, 255);
            p.BrandBorder = new Color32(184, 118, 136, 255);
            p.BrandLine = new Color32(215, 154, 170, 255);
            p.TextOnBrand = new Color32(36, 23, 29, 255);
            p.BrandText = new Color32(229, 171, 186, 255);
            p.Mint = new Color32(57, 197, 187, 255);
            p.Focus = new Color32(57, 197, 187, 255);
            p.Link = new Color32(57, 197, 187, 255);
            p.BrandTint = new Color32(96, 76, 84, 255);
            p.Success = new Color32(111, 208, 135, 255);
            p.SuccessText = new Color32(147, 228, 164, 255);
            p.SuccessBg = new Color32(71, 73, 70, 255);
            p.SuccessBorder = new Color32(90, 124, 95, 255);
            p.Warning = new Color32(255, 193, 7, 255);
            p.WarningText = new Color32(251, 205, 104, 255);
            p.WarningBg = new Color32(86, 71, 63, 255);
            p.WarningBorder = new Color32(149, 117, 65, 255);
            p.Danger = new Color32(255, 83, 74, 255);
            p.DangerText = new Color32(253, 161, 154, 255);
            p.DangerBg = new Color32(88, 60, 63, 255);
            p.DangerBorder = new Color32(151, 73, 70, 255);
            p.Info = new Color32(115, 186, 234, 255);
            p.InfoText = new Color32(151, 207, 247, 255);
            p.InfoBg = new Color32(71, 70, 81, 255);
            p.InfoBorder = new Color32(90, 114, 138, 255);
            p.ProductMingtoon = new Color32(215, 154, 170, 255);
            p.ProductPhysics = new Color32(57, 197, 187, 255);
            p.ProductCtb = new Color32(77, 204, 255, 255);
            p.ProductMm = new Color32(115, 217, 140, 255);
            p.ProductEme = new Color32(234, 118, 0, 255);
            p.ProductWbb = new Color32(192, 191, 227, 255);
            p.SceneAdd = new Color32(51, 229, 255, 89);
            p.SceneRemove = new Color32(255, 64, 51, 89);
            p.SceneHintBg = new Color32(0, 0, 0, 140);
            p.SceneHintText = new Color32(255, 255, 255, 255);
            p.Scrim = new Color32(16, 19, 20, 115);
            return p;
        }

        private static SRPalette BuildLight()
        {
            var p = new SRPalette();
            p.SurfaceApp = new Color32(187, 179, 184, 255);
            p.SurfaceWindow = new Color32(205, 198, 202, 255);
            p.SurfaceCard = new Color32(216, 209, 213, 255);
            p.SurfaceSection = new Color32(196, 189, 193, 255);
            p.SurfaceHover = new Color32(224, 218, 222, 255);
            p.SurfaceSunken = new Color32(247, 243, 245, 255);
            p.Control = new Color32(231, 226, 229, 255);
            p.ControlHover = new Color32(239, 235, 238, 255);
            p.ControlPressed = new Color32(225, 192, 204, 255);
            p.LineSubtle = new Color32(175, 168, 173, 255);
            p.LineControl = new Color32(162, 156, 160, 255);
            p.LineStrong = new Color32(103, 97, 101, 255);
            p.TextStrong = new Color32(34, 23, 30, 255);
            p.Text = new Color32(26, 20, 24, 255);
            p.TextMuted = new Color32(76, 69, 74, 255);
            p.TextDisabled = new Color32(111, 106, 109, 255);
            p.Brand = new Color32(215, 154, 170, 255);
            p.BrandHover = new Color32(207, 142, 159, 255);
            p.BrandPressed = new Color32(195, 128, 147, 255);
            p.BrandBorder = new Color32(159, 89, 109, 255);
            p.BrandLine = new Color32(149, 69, 94, 255);
            p.TextOnBrand = new Color32(36, 23, 29, 255);
            p.BrandText = new Color32(120, 50, 73, 255);
            p.Mint = new Color32(0, 112, 106, 255);
            p.Focus = new Color32(0, 112, 106, 255);
            p.Link = new Color32(0, 85, 80, 255);
            p.BrandTint = new Color32(217, 190, 198, 255);
            p.Success = new Color32(33, 118, 60, 255);
            p.SuccessText = new Color32(17, 86, 41, 255);
            p.SuccessBg = new Color32(218, 225, 218, 255);
            p.SuccessBorder = new Color32(134, 174, 140, 255);
            p.Warning = new Color32(132, 91, 0, 255);
            p.WarningText = new Color32(102, 66, 0, 255);
            p.WarningBg = new Color32(231, 221, 213, 255);
            p.WarningBorder = new Color32(183, 158, 122, 255);
            p.Danger = new Color32(177, 12, 12, 255);
            p.DangerText = new Color32(141, 27, 25, 255);
            p.DangerBg = new Color32(242, 215, 213, 255);
            p.DangerBorder = new Color32(217, 132, 121, 255);
            p.Info = new Color32(26, 96, 158, 255);
            p.InfoText = new Color32(5, 73, 128, 255);
            p.InfoBg = new Color32(216, 222, 234, 255);
            p.InfoBorder = new Color32(129, 161, 199, 255);
            p.ProductMingtoon = new Color32(129, 59, 81, 255);
            p.ProductPhysics = new Color32(0, 100, 94, 255);
            p.ProductCtb = new Color32(0, 95, 134, 255);
            p.ProductMm = new Color32(9, 103, 46, 255);
            p.ProductEme = new Color32(146, 63, 0, 255);
            p.ProductWbb = new Color32(56, 48, 120, 255);
            p.SceneAdd = new Color32(51, 229, 255, 89);
            p.SceneRemove = new Color32(255, 64, 51, 89);
            p.SceneHintBg = new Color32(0, 0, 0, 140);
            p.SceneHintText = new Color32(255, 255, 255, 255);
            p.Scrim = new Color32(16, 19, 20, 64);
            return p;
        }
    }

    public static class SRStyles
    {
        private static bool builtForDark;
        private static bool built;
        private static GUIStyle body, muted, strong, title, section, small, mini, hero, center, buttonText, fieldText, popupText, wrap;
        private static GUIStyle helpLayout, helpActionLayout;
        public static GUIStyle Body
        {
            get
            {
                Ensure();
                return body;
            }
        }

        public static GUIStyle Muted
        {
            get
            {
                Ensure();
                return muted;
            }
        }

        public static GUIStyle Strong
        {
            get
            {
                Ensure();
                return strong;
            }
        }

        public static GUIStyle Title
        {
            get
            {
                Ensure();
                return title;
            }
        }

        public static GUIStyle Section
        {
            get
            {
                Ensure();
                return section;
            }
        }

        public static GUIStyle Small
        {
            get
            {
                Ensure();
                return small;
            }
        }

        public static GUIStyle Mini
        {
            get
            {
                Ensure();
                return mini;
            }
        }

        public static GUIStyle Hero
        {
            get
            {
                Ensure();
                return hero;
            }
        }

        public static GUIStyle Center
        {
            get
            {
                Ensure();
                return center;
            }
        }

        public static GUIStyle ButtonText
        {
            get
            {
                Ensure();
                return buttonText;
            }
        }

        public static GUIStyle FieldText
        {
            get
            {
                Ensure();
                return fieldText;
            }
        }

        public static GUIStyle PopupText
        {
            get
            {
                Ensure();
                return popupText;
            }
        }

        public static GUIStyle Wrap
        {
            get
            {
                Ensure();
                return wrap;
            }
        }

        public static GUIStyle HelpLayout
        {
            get
            {
                Ensure();
                return helpLayout;
            }
        }

        public static GUIStyle HelpActionLayout
        {
            get
            {
                Ensure();
                return helpActionLayout;
            }
        }

        private static void Ensure()
        {
            if (built && builtForDark == SRTheme.Dark)
                return;
            built = true;
            builtForDark = SRTheme.Dark;
            SRPalette p = SRTheme.P;
            body = Text(EditorStyles.label, p.Text, SRMetrics.FontBody, FontStyle.Normal);
            muted = Text(EditorStyles.label, p.TextMuted, SRMetrics.FontSmall, FontStyle.Normal);
            strong = Text(EditorStyles.label, p.TextStrong, SRMetrics.FontBody, FontStyle.Bold);
            title = Text(EditorStyles.label, p.TextStrong, SRMetrics.FontTitle, FontStyle.Bold);
            section = Text(EditorStyles.label, p.TextStrong, SRMetrics.FontBody, FontStyle.Bold);
            small = Text(EditorStyles.label, p.TextMuted, SRMetrics.FontSmall, FontStyle.Normal);
            mini = Text(EditorStyles.label, p.TextMuted, SRMetrics.FontMini, FontStyle.Bold);
            mini.alignment = TextAnchor.MiddleCenter;
            hero = Text(EditorStyles.label, p.TextStrong, SRMetrics.FontHero, FontStyle.Bold);
            hero.alignment = TextAnchor.MiddleCenter;
            hero.wordWrap = true;
            center = Text(EditorStyles.label, p.TextMuted, SRMetrics.FontBody, FontStyle.Normal);
            center.alignment = TextAnchor.MiddleCenter;
            center.wordWrap = true;
            wrap = Text(EditorStyles.label, p.Text, SRMetrics.FontBody, FontStyle.Normal);
            wrap.wordWrap = true;
            helpLayout = new GUIStyle(wrap)
            {
                padding = new RectOffset(32, 8, 6, 6)
            };
            helpActionLayout = new GUIStyle(wrap)
            {
                padding = new RectOffset(32, 8, 6, 6 + (int)SRMetrics.ButtonMini + 4)
            };
            buttonText = Text(EditorStyles.label, p.TextStrong, SRMetrics.FontBody, FontStyle.Normal);
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.padding = new RectOffset(8, 8, 0, 0);
            buttonText.imagePosition = ImagePosition.ImageLeft;
            fieldText = Bare(EditorStyles.textField, p.Text);
            fieldText.padding = new RectOffset(4, 4, 1, 1);
            popupText = Bare(EditorStyles.popup, p.TextStrong);
            popupText.padding = new RectOffset(6, 18, 1, 1);
        }

        private static GUIStyle Text(GUIStyle source, Color color, int size, FontStyle fontStyle)
        {
            var s = new GUIStyle(source)
            {
                fontSize = size,
                fontStyle = fontStyle,
                richText = true
            };
            SetTextColor(s, color);
            s.margin = new RectOffset(0, 0, 0, 0);
            s.padding = new RectOffset(0, 0, 0, 0);
            return s;
        }

        private static GUIStyle Bare(GUIStyle source, Color color)
        {
            var s = new GUIStyle(source);
            foreach (GUIStyleState st in States(s))
            {
                st.background = null;
                st.scaledBackgrounds = new Texture2D[0];
                st.textColor = color;
            }

            s.border = new RectOffset(0, 0, 0, 0);
            s.margin = new RectOffset(0, 0, 0, 0);
            return s;
        }

        internal static GUIStyleState[] States(GUIStyle s)
        {
            return new[]
            {
                s.normal,
                s.hover,
                s.active,
                s.focused,
                s.onNormal,
                s.onHover,
                s.onActive,
                s.onFocused
            };
        }

        internal static void SetTextColor(GUIStyle s, Color c)
        {
            foreach (GUIStyleState st in States(s))
                st.textColor = c;
        }
    }

    public struct SRLabelScope : IDisposable
    {
        private readonly Color[] saved;
        private readonly bool savedRich;
        private SRLabelScope(Color[] saved, bool savedRich)
        {
            this.saved = saved;
            this.savedRich = savedRich;
        }

        public static SRLabelScope Begin()
        {
            GUIStyle s = EditorStyles.label;
            GUIStyleState[] states = SRStyles.States(s);
            var saved = new Color[states.Length];
            for (int i = 0; i < states.Length; i++)
                saved[i] = states[i].textColor;
            var scope = new SRLabelScope(saved, s.richText);
            SRPalette p = SRTheme.P;
            Color ink = GUI.enabled ? p.Text : SRTheme.DisabledInk(p.SurfaceWindow);
            s.normal.textColor = ink;
            s.hover.textColor = ink;
            s.active.textColor = p.Focus;
            s.focused.textColor = p.Focus;
            s.onNormal.textColor = p.Focus;
            s.onHover.textColor = p.Focus;
            s.onActive.textColor = p.Focus;
            s.onFocused.textColor = p.Focus;
            s.richText = true;
            return scope;
        }

        public void Dispose()
        {
            if (saved == null)
                return;
            GUIStyle s = EditorStyles.label;
            GUIStyleState[] states = SRStyles.States(s);
            for (int i = 0; i < states.Length && i < saved.Length; i++)
                states[i].textColor = saved[i];
            s.richText = savedRich;
        }
    }

    public static class SRGui
    {
        private static readonly int ButtonHash = "SRGui.Button".GetHashCode();
        private static readonly int DragHash = "SRGui.LabelDrag".GetHashCode();
        private static readonly int SectionHash = "SRGui.Section".GetHashCode();
        private static int fieldSerial;
        public static GUIContent Label(string translated, string original, string tooltip)
        {
            if (string.IsNullOrEmpty(original) || original == translated)
                return new GUIContent(translated, tooltip);
            return new GUIContent(translated + " <color=" + SRTheme.Hex(SRTheme.P.TextMuted) + ">(" + original + ")</color>", tooltip);
        }

        public static GUIContent Label(SRLang lang, string translated, string original, string tooltip)
        {
            return lang == SRLang.English ? new GUIContent(original, tooltip) : Label(translated, original, tooltip);
        }

        public static void LabelRow(GUIContent label)
        {
            Rect row = EditorGUILayout.GetControlRect(false, SRMetrics.Line);
            Ink(row, label, SRStyles.Body);
        }

        private static readonly GUIContent InkContent = new GUIContent();
        private static readonly Regex ColorTag = new Regex("</?color[^>]*>", RegexOptions.IgnoreCase);
        public static void Ink(Rect r, GUIContent content, GUIStyle style)
        {
            if (GUI.enabled)
            {
                GUI.Label(r, content, style);
                return;
            }

            GUIContent c = content;
            if (content != null && content.text != null && content.text.IndexOf("<color", StringComparison.OrdinalIgnoreCase) >= 0)
                c = new GUIContent(ColorTag.Replace(content.text, string.Empty), content.image, content.tooltip);
            Color oldNormal = style.normal.textColor;
            Color oldHover = style.hover.textColor;
            style.normal.textColor = SRTheme.P.TextDisabled;
            style.hover.textColor = SRTheme.P.TextDisabled;
            GUI.enabled = true;
            try
            {
                GUI.Label(r, c, style);
            }
            finally
            {
                GUI.enabled = false;
                style.normal.textColor = oldNormal;
                style.hover.textColor = oldHover;
            }
        }

        public static void Ink(Rect r, string text, GUIStyle style)
        {
            if (GUI.enabled)
            {
                GUI.Label(r, text, style);
                return;
            }

            InkContent.text = text;
            Ink(r, InkContent, style);
        }

        public static void WindowBackground(EditorWindow window)
        {
            if (window == null)
                return;
            SRTheme.Fill(new Rect(0f, 0f, window.position.width, window.position.height), SRTheme.P.SurfaceWindow);
        }

        public static Rect Header(SRProduct product, string markLetter, string title, string subtitle)
        {
            Rect r = GUILayoutUtility.GetRect(0f, SRMetrics.Header + SRMetrics.AccentLine, GUILayout.ExpandWidth(true));
            SRPalette p = SRTheme.P;
            Color accent = SRTheme.Product(product);
            SRTheme.Fill(new Rect(r.x, r.y, r.width, SRMetrics.Header), p.SurfaceApp);
            SRTheme.Fill(new Rect(r.x, r.y + SRMetrics.Header, r.width, SRMetrics.AccentLine), accent);
            var mark = new Rect(r.x + SRMetrics.Space12, r.y + (SRMetrics.Header - 22f) * 0.5f, 22f, 22f);
            SRTheme.Fill(mark, accent, SRMetrics.RadiusButton);
            GUIStyle markStyle = SRStyles.Strong;
            Color old = markStyle.normal.textColor;
            markStyle.normal.textColor = p.SurfaceSunken;
            TextAnchor oldAnchor = markStyle.alignment;
            markStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(mark, markLetter, markStyle);
            markStyle.alignment = oldAnchor;
            markStyle.normal.textColor = old;
            float textX = mark.xMax + SRMetrics.Space8;
            float toolsWidth = Mathf.Min(140f, r.width * 0.35f);
            float textW = Mathf.Max(0f, r.xMax - SRMetrics.Space12 - toolsWidth - textX);
            bool showSub = !string.IsNullOrEmpty(subtitle) && r.width >= SRMetrics.InspectorMin + 40f;
            float titleY = showSub ? r.y + 6f : r.y + (SRMetrics.Header - 18f) * 0.5f;
            GUI.Label(new Rect(textX, titleY, textW, 18f), Fit(title, SRStyles.Title, textW), SRStyles.Title);
            if (showSub)
                GUI.Label(new Rect(textX, titleY + 19f, textW, 14f), Fit(subtitle, SRStyles.Small, textW), SRStyles.Small);
            return new Rect(r.xMax - SRMetrics.Space12 - toolsWidth, r.y + (SRMetrics.Header - SRMetrics.Line) * 0.5f, toolsWidth, SRMetrics.Line);
        }

        public static int Tabs(int current, GUIContent[] tabs, int[] badges = null)
        {
            Rect r = GUILayoutUtility.GetRect(0f, SRMetrics.Tab, GUILayout.ExpandWidth(true));
            SRPalette p = SRTheme.P;
            SRTheme.Fill(r, p.SurfaceSection);
            SRTheme.Fill(new Rect(r.x, r.yMax - 1f, r.width, 1f), p.LineSubtle);
            float x = r.x;
            for (int i = 0; i < tabs.Length; i++)
            {
                bool on = i == current;
                GUIStyle style = on ? SRStyles.Strong : SRStyles.Body;
                float w = style.CalcSize(tabs[i]).x + 24f;
                string badge = badges != null && i < badges.Length && badges[i] > 0 ? badges[i].ToString() : null;
                if (badge != null)
                    w += BadgeWidth(badge) + 4f;
                var tr = new Rect(x, r.y, Mathf.Min(w, r.xMax - x), r.height - 1f);
                if (tr.width <= 0f)
                    break;
                bool hover = GUI.enabled && tr.Contains(Event.current.mousePosition);
                if (on && GUI.enabled)
                    SRTheme.Fill(tr, p.BrandTint);
                else if (hover)
                    SRTheme.Fill(tr, p.SurfaceHover);
                if (on)
                    SRTheme.Fill(new Rect(tr.x, tr.yMax - 1f, tr.width, SRMetrics.AccentLine), GUI.enabled ? p.BrandLine : p.TextDisabled);
                Color oldColor = style.normal.textColor;
                style.normal.textColor = on ? p.TextStrong : (hover ? p.Text : p.TextMuted);
                Ink(new Rect(tr.x + 12f, tr.y, tr.width - 12f, tr.height), tabs[i], style);
                style.normal.textColor = oldColor;
                if (badge != null)
                    Badge(new Rect(tr.xMax - 12f - BadgeWidth(badge), tr.y + 3f, BadgeWidth(badge), 18f), badge, SRTone.Warning);
                if (GUI.Button(tr, GUIContent.none, GUIStyle.none))
                    current = i;
                x += tr.width;
            }

            return current;
        }

        public static int StepGuide(string[] steps, int current)
        {
            Rect r = GUILayoutUtility.GetRect(0f, 36f, GUILayout.ExpandWidth(true));
            SRPalette p = SRTheme.P;
            SRTheme.Box(r, p.SurfaceCard, p.LineSubtle, SRMetrics.RadiusCard);
            float[] widths = new float[steps.Length];
            float total = 0f;
            for (int i = 0; i < steps.Length; i++)
            {
                GUIStyle s = i == current ? SRStyles.Strong : SRStyles.Small;
                widths[i] = 18f + 6f + s.CalcSize(new GUIContent(steps[i])).x;
                total += widths[i];
            }

            float gap = steps.Length > 1 ? Mathf.Max(8f, (r.width - 16f - total) / (steps.Length - 1)) : 0f;
            float x = r.x + 8f;
            float cy = r.y + r.height * 0.5f;
            int result = current;
            for (int i = 0; i < steps.Length; i++)
            {
                var dot = new Rect(x, cy - 9f, 18f, 18f);
                bool done = i < current;
                bool now = i == current;
                if (done)
                    SRTheme.Box(dot, p.Brand, p.BrandBorder, 9f);
                else if (now)
                {
                    SRTheme.Fill(dot, p.SurfaceWindow, 9f);
                    SRTheme.Stroke(dot, p.BrandLine, 9f, 2f);
                }
                else
                    SRTheme.Box(dot, p.SurfaceSunken, p.LineStrong, 9f);
                if (done)
                {
                    Glyph(new Rect(dot.x + 3f, dot.y + 3f, 12f, 12f), SRGlyph.Check, p.TextOnBrand, 1.8f);
                }
                else
                {
                    GUIStyle num = SRStyles.Mini;
                    Color old = num.normal.textColor;
                    num.normal.textColor = now ? p.BrandText : p.TextMuted;
                    Ink(dot, (i + 1).ToString(), num);
                    num.normal.textColor = old;
                }

                GUIStyle ls = now ? SRStyles.Strong : SRStyles.Small;
                var lr = new Rect(dot.xMax + 6f, r.y, widths[i] - 24f, r.height);
                Ink(lr, steps[i], ls);
                if (done)
                {
                    EditorGUIUtility.AddCursorRect(new Rect(dot.x, r.y, widths[i], r.height), MouseCursor.Link);
                    if (GUI.Button(new Rect(dot.x, r.y, widths[i], r.height), GUIContent.none, GUIStyle.none))
                        result = i;
                }

                x += widths[i];
                if (i < steps.Length - 1)
                {
                    SRTheme.Fill(new Rect(x + 4f, cy, gap - 8f, 1f), p.LineStrong);
                    x += gap;
                }
            }

            return result;
        }

        public static bool Section(string prefKey, GUIContent label, bool defaultOpen, string resetLabel, out bool resetClicked)
        {
            bool open = EditorPrefs.GetBool(prefKey, defaultOpen);
            bool now = SectionHeader(open, label, resetLabel, out resetClicked);
            if (now != open)
                EditorPrefs.SetBool(prefKey, now);
            return now;
        }

        public static bool Section(ref bool open, GUIContent label, string resetLabel, out bool resetClicked)
        {
            open = SectionHeader(open, label, resetLabel, out resetClicked);
            return open;
        }

        private static bool SectionHeader(bool open, GUIContent label, string resetLabel, out bool resetClicked)
        {
            resetClicked = false;
            Rect r = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));
            r.y += 2f;
            SRPalette p = SRTheme.P;
            SRTheme.Fill(r, p.SurfaceSection);
            SRTheme.Fill(new Rect(r.x, r.y, r.width, 1f), p.LineSubtle);
            SRTheme.Fill(new Rect(r.x, r.yMax - 1f, r.width, 1f), p.LineSubtle);
            Rect resetRect = Rect.zero;
            if (!string.IsNullOrEmpty(resetLabel))
            {
                float rw = SRStyles.Small.CalcSize(new GUIContent(resetLabel)).x + 8f;
                resetRect = new Rect(r.xMax - rw - 4f, r.y, rw, r.height);
                Ink(resetRect, resetLabel, SRStyles.Small);
                if (GUI.Button(resetRect, GUIContent.none, GUIStyle.none))
                    resetClicked = true;
                if (GUI.enabled)
                    EditorGUIUtility.AddCursorRect(resetRect, MouseCursor.Link);
            }

            GUIStyle ls = open ? SRStyles.Section : SRStyles.Body;
            Glyph(new Rect(r.x + 6f, r.y + 5f, 12f, 12f), open ? SRGlyph.ChevronDown : SRGlyph.ChevronRight, GUI.enabled ? p.TextMuted : p.TextDisabled);
            var titleRect = new Rect(r.x + 20f, r.y + 2f, r.width - 24f - resetRect.width, 18f);
            Ink(titleRect, FitContent(label, ls, titleRect.width), ls);
            GUIUtility.GetControlID(SectionHash, FocusType.Passive, r);
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && r.Contains(e.mousePosition) && !resetRect.Contains(e.mousePosition))
            {
                open = !open;
                GUIUtility.keyboardControl = 0;
                e.Use();
            }

            GUILayout.Space(4f);
            return open;
        }

        public static void BeginCard()
        {
            Rect r = EditorGUILayout.BeginVertical();
            SRTheme.Box(r, SRTheme.P.SurfaceCard, SRTheme.P.LineSubtle, SRMetrics.RadiusCard);
            GUILayout.Space(SRMetrics.Space8);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(SRMetrics.Space8);
            EditorGUILayout.BeginVertical();
        }

        public static void EndCard()
        {
            EditorGUILayout.EndVertical();
            GUILayout.Space(SRMetrics.Space8);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(SRMetrics.Space8);
            EditorGUILayout.EndVertical();
        }

        private static Rect FieldPart(Rect row)
        {
            float x = row.x + EditorGUIUtility.labelWidth + 2f;
            return new Rect(x, row.y, Mathf.Max(0f, row.xMax - x), row.height);
        }

        private static string NextFieldName()
        {
            fieldSerial = (fieldSerial + 1) & 0xFFFF;
            return "SRGui.Field." + fieldSerial;
        }

        private static void FieldFace(Rect field)
        {
            SRPalette p = SRTheme.P;
            SRTheme.Box(field, p.SurfaceSunken, p.LineControl, SRMetrics.RadiusControl);
            SetFieldInk();
        }

        private static void SetFieldInk()
        {
            SRPalette p = SRTheme.P;
            SRStyles.FieldText.normal.textColor = GUI.enabled ? p.Text : SRTheme.DisabledInk(p.SurfaceSunken);
        }

        private static void FieldFocus(Rect field, string controlName)
        {
            if (!GUI.enabled || GUI.GetNameOfFocusedControl() != controlName)
                return;
            SRPalette p = SRTheme.P;
            SRTheme.Stroke(field, p.Focus, SRMetrics.RadiusControl, 1f);
            SRTheme.Stroke(new Rect(field.x + 1f, field.y + 1f, field.width - 2f, field.height - 2f), p.Focus, SRMetrics.RadiusControl, 1f);
        }

        public static float FloatField(GUIContent label, float value, params GUILayoutOption[] options)
        {
            Rect row = EditorGUILayout.GetControlRect(true, SRMetrics.Line, options);
            Rect field = FieldPart(row);
            string name = NextFieldName();
            FieldFace(field);
            float result;
            using (SRLabelScope.Begin())
            {
                GUI.SetNextControlName(name);
                result = EditorGUI.FloatField(row, label, value, SRStyles.FieldText);
            }

            FieldFocus(field, name);
            return result;
        }

        public static int IntField(GUIContent label, int value, params GUILayoutOption[] options)
        {
            Rect row = EditorGUILayout.GetControlRect(true, SRMetrics.Line, options);
            Rect field = FieldPart(row);
            string name = NextFieldName();
            FieldFace(field);
            int result;
            using (SRLabelScope.Begin())
            {
                GUI.SetNextControlName(name);
                result = EditorGUI.IntField(row, label, value, SRStyles.FieldText);
            }

            FieldFocus(field, name);
            return result;
        }

        public static string TextField(GUIContent label, string value, params GUILayoutOption[] options)
        {
            return TextField(null, label, value, options);
        }

        public static string TextField(string controlName, GUIContent label, string value, params GUILayoutOption[] options)
        {
            Rect row = EditorGUILayout.GetControlRect(true, SRMetrics.Line, options);
            Rect field = label == null || label == GUIContent.none ? row : FieldPart(row);
            string name = string.IsNullOrEmpty(controlName) ? NextFieldName() : controlName;
            FieldFace(field);
            string result;
            using (SRLabelScope.Begin())
            {
                GUI.SetNextControlName(name);
                result = label == null || label == GUIContent.none ? EditorGUI.TextField(row, value, SRStyles.FieldText) : EditorGUI.TextField(row, label, value, SRStyles.FieldText);
            }

            FieldFocus(field, name);
            return result;
        }

        public static void PropertyFloat(SerializedProperty prop, GUIContent label)
        {
            Rect row = EditorGUILayout.GetControlRect(true, SRMetrics.Line);
            Rect field = FieldPart(row);
            string name = NextFieldName();
            FieldFace(field);
            using (SRLabelScope.Begin())
            {
                label = EditorGUI.BeginProperty(row, label, prop);
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(name);
                float v = EditorGUI.FloatField(row, label, prop.floatValue, SRStyles.FieldText);
                if (EditorGUI.EndChangeCheck())
                    prop.floatValue = v;
                EditorGUI.EndProperty();
            }

            FieldFocus(field, name);
        }

        public static int Popup(GUIContent label, int index, GUIContent[] options)
        {
            return Popup(EditorGUILayout.GetControlRect(true, SRMetrics.Line), label, index, options);
        }

        public static int Popup(Rect row, GUIContent label, int index, GUIContent[] options)
        {
            bool labelled = label != null && label != GUIContent.none;
            Rect field = labelled ? FieldPart(row) : row;
            SRPalette p = SRTheme.P;
            bool hover = GUI.enabled && field.Contains(Event.current.mousePosition);
            if (GUI.enabled)
                SRTheme.Box(field, hover ? p.ControlHover : p.Control, p.LineControl, SRMetrics.RadiusControl);
            else
                SRTheme.Box(field, p.SurfaceCard, p.LineSubtle, SRMetrics.RadiusControl);
            SRStyles.PopupText.normal.textColor = GUI.enabled ? p.TextStrong : SRTheme.DisabledInk(p.SurfaceCard);
            int result;
            using (SRLabelScope.Begin())
                result = EditorGUI.Popup(row, label, index, options, SRStyles.PopupText);
            Glyph(new Rect(field.xMax - 16f, field.y + (field.height - 12f) * 0.5f, 12f, 12f), SRGlyph.ChevronDown, GUI.enabled ? p.TextMuted : p.TextDisabled);
            return result;
        }

        public static bool Toggle(GUIContent label, bool value)
        {
            return Toggle(EditorGUILayout.GetControlRect(true, SRMetrics.Line), label, value);
        }

        public static bool Toggle(Rect row, GUIContent label, bool value)
        {
            Rect field = FieldPart(row);
            var box = new Rect(field.x, field.y + 2f, 14f, 14f);
            SRPalette p = SRTheme.P;
            bool mixed = EditorGUI.showMixedValue;
            bool result;
            using (SRLabelScope.Begin())
                result = EditorGUI.Toggle(row, label, value, GUIStyle.none);
            CheckFace(box, result, mixed);
            return result;
        }

        private static void CheckFace(Rect box, bool on, bool mixed)
        {
            SRPalette p = SRTheme.P;
            if (on && !mixed)
            {
                SRTheme.Box(box, GUI.enabled ? p.Brand : p.TextDisabled, p.BrandBorder, SRMetrics.RadiusControl);
                Glyph(new Rect(box.x + 1f, box.y + 1f, 12f, 12f), SRGlyph.Check, p.TextOnBrand, 1.8f);
            }
            else
            {
                SRTheme.Box(box, p.SurfaceSunken, GUI.enabled ? p.LineStrong : p.LineControl, SRMetrics.RadiusControl);
                if (mixed)
                    SRTheme.Fill(new Rect(box.x + 3f, box.y + 6f, 8f, 2f), GUI.enabled ? p.Text : p.TextDisabled);
            }
        }

        public static bool ToggleLeft(GUIContent label, bool value)
        {
            Rect row = EditorGUILayout.GetControlRect(false, SRMetrics.Line);
            var box = new Rect(row.x, row.y + 2f, 14f, 14f);
            bool mixed = EditorGUI.showMixedValue;
            bool result = GUI.Toggle(row, value, GUIContent.none, GUIStyle.none);
            CheckFace(box, result, mixed);
            Ink(new Rect(box.xMax + 6f, row.y, row.width - 20f, row.height), label, SRStyles.Body);
            return result;
        }

        public static void PropertyToggle(SerializedProperty prop, GUIContent label)
        {
            Rect row = EditorGUILayout.GetControlRect(true, SRMetrics.Line);
            label = EditorGUI.BeginProperty(row, label, prop);
            EditorGUI.BeginChangeCheck();
            bool v = Toggle(row, label, prop.boolValue);
            if (EditorGUI.EndChangeCheck())
                prop.boolValue = v;
            EditorGUI.EndProperty();
        }

        public static float Slider(GUIContent label, float value, float min, float max)
        {
            return Slider(EditorGUILayout.GetControlRect(true, SRMetrics.Line), label, value, min, max);
        }

        public static float Slider(Rect row, GUIContent label, float value, float min, float max)
        {
            SRPalette p = SRTheme.P;
            var labelRect = new Rect(row.x, row.y, EditorGUIUtility.labelWidth, row.height);
            Rect field = FieldPart(row);
            float numW = Mathf.Min(52f, field.width * 0.4f);
            var num = new Rect(field.xMax - numW, field.y, numW, field.height);
            var track = new Rect(field.x + 7f, field.y, Mathf.Max(0f, field.width - numW - 6f - 14f), field.height);
            using (SRLabelScope.Begin())
                EditorGUI.LabelField(labelRect, label);
            value = LabelDrag(labelRect, value, min, max, track.width);
            float t = Mathf.InverseLerp(min, max, value);
            var groove = new Rect(track.x, track.y + track.height * 0.5f - 2f, track.width, 4f);
            SRTheme.Fill(groove, p.LineControl, 2f);
            SRTheme.Fill(new Rect(groove.x, groove.y, groove.width * t, groove.height), GUI.enabled ? p.BrandLine : p.TextDisabled, 2f);
            value = GUI.HorizontalSlider(new Rect(track.x - 7f, track.y, track.width + 14f, track.height), value, min, max, GUIStyle.none, ThumbHit());
            t = Mathf.InverseLerp(min, max, value);
            var thumb = new Rect(track.x + track.width * t - 7f, track.y + track.height * 0.5f - 7f, 14f, 14f);
            SRTheme.Box(thumb, GUI.enabled ? p.Brand : p.TextDisabled, p.BrandBorder, 7f);
            string name = NextFieldName();
            FieldFace(num);
            GUI.SetNextControlName(name);
            value = Mathf.Clamp(EditorGUI.FloatField(num, value, SRStyles.FieldText), min, max);
            FieldFocus(num, name);
            return value;
        }

        public static void PropertySlider(SerializedProperty prop, GUIContent label, float min, float max)
        {
            Rect row = EditorGUILayout.GetControlRect(true, SRMetrics.Line);
            label = EditorGUI.BeginProperty(row, label, prop);
            EditorGUI.BeginChangeCheck();
            float v = Slider(row, label, prop.floatValue, min, max);
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = v;
            EditorGUI.EndProperty();
        }

        private static GUIStyle thumbHit;
        private static GUIStyle ThumbHit()
        {
            if (thumbHit == null)
                thumbHit = new GUIStyle
                {
                    fixedWidth = 14f,
                    fixedHeight = 14f
                };
            return thumbHit;
        }

        private static float LabelDrag(Rect labelRect, float value, float min, float max, float pixelsForRange)
        {
            if (!GUI.enabled)
                return value;
            int id = GUIUtility.GetControlID(DragHash, FocusType.Passive, labelRect);
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.SlideArrow);
            Event e = Event.current;
            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && labelRect.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        GUIUtility.keyboardControl = 0;
                        e.Use();
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        float perPixel = (max - min) / Mathf.Max(pixelsForRange, 1f);
                        value = Mathf.Clamp(value + e.delta.x * perPixel, min, max);
                        GUI.changed = true;
                        e.Use();
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }

                    break;
            }

            return value;
        }

        public static float QuickSlider(string name, string valueText, float value01, string leftWord, string rightWord, float default01)
        {
            Rect r = GUILayoutUtility.GetRect(0f, 62f, GUILayout.ExpandWidth(true));
            SRPalette p = SRTheme.P;
            SRTheme.Box(r, p.SurfaceCard, p.LineSubtle, SRMetrics.RadiusCard);
            Ink(new Rect(r.x + 10f, r.y + 6f, r.width * 0.6f, 18f), name, SRStyles.Section);
            GUIStyle val = SRStyles.Strong;
            Color old = val.normal.textColor;
            TextAnchor oldA = val.alignment;
            val.normal.textColor = p.BrandText;
            val.alignment = TextAnchor.MiddleRight;
            Ink(new Rect(r.x + r.width * 0.4f, r.y + 6f, r.width * 0.6f - 10f, 18f), valueText, val);
            val.normal.textColor = old;
            val.alignment = oldA;
            var track = new Rect(r.x + 19f, r.y + 29f, r.width - 38f, 6f);
            SRTheme.Fill(track, p.LineControl, 3f);
            SRTheme.Fill(new Rect(track.x, track.y, track.width * Mathf.Clamp01(value01), track.height), GUI.enabled ? p.BrandLine : p.TextDisabled, 3f);
            SRTheme.Fill(new Rect(track.x + track.width * Mathf.Clamp01(default01) - 1f, track.y - 3f, 2f, 12f), p.TextMuted, 1f);
            value01 = GUI.HorizontalSlider(new Rect(track.x - 9f, track.y - 6f, track.width + 18f, 18f), value01, 0f, 1f, GUIStyle.none, QuickThumbHit());
            var thumb = new Rect(track.x + track.width * Mathf.Clamp01(value01) - 9f, track.y - 6f, 18f, 18f);
            SRTheme.Box(thumb, GUI.enabled ? p.Brand : p.TextDisabled, p.BrandBorder, 9f);
            Ink(new Rect(r.x + 10f, r.y + 42f, r.width * 0.5f, 14f), leftWord, SRStyles.Small);
            GUIStyle right = SRStyles.Small;
            TextAnchor oldR = right.alignment;
            right.alignment = TextAnchor.MiddleRight;
            Ink(new Rect(r.x + r.width * 0.5f, r.y + 42f, r.width * 0.5f - 10f, 14f), rightWord, right);
            right.alignment = oldR;
            GUILayout.Space(SRMetrics.Space6);
            return value01;
        }

        private static GUIStyle quickThumbHit;
        private static GUIStyle QuickThumbHit()
        {
            if (quickThumbHit == null)
                quickThumbHit = new GUIStyle
                {
                    fixedWidth = 18f,
                    fixedHeight = 18f
                };
            return quickThumbHit;
        }

        public static AnimationCurve Curve(GUIContent label, AnimationCurve curve, Rect ranges)
        {
            using (SRLabelScope.Begin())
                return EditorGUILayout.CurveField(label, curve, SRTheme.P.Brand, ranges);
        }

        public static bool Button(GUIContent content, SRButtonKind kind, SRButtonSize size, params GUILayoutOption[] options)
        {
            float h = size == SRButtonSize.Mini ? SRMetrics.ButtonMini : size == SRButtonSize.Large ? SRMetrics.ButtonLarge : size == SRButtonSize.Hero ? SRMetrics.ButtonHero : SRMetrics.Button;
            GUIStyle ts = SRStyles.ButtonText;
            float w = ts.CalcSize(content).x + (size == SRButtonSize.Hero ? 16f : 4f);
            Rect r = GUILayoutUtility.GetRect(w, h, options.Length > 0 ? options : new[] { GUILayout.Width(w), GUILayout.Height(h) });
            r.height = h;
            return Button(r, content, kind, size);
        }

        public static bool Button(Rect r, GUIContent content, SRButtonKind kind, SRButtonSize size)
        {
            int id = GUIUtility.GetControlID(ButtonHash, FocusType.Keyboard, r);
            Event e = Event.current;
            bool enabled = GUI.enabled;
            bool clicked = false;
            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (enabled && e.button == 0 && r.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        e.Use();
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                        if (r.Contains(e.mousePosition))
                            clicked = true;
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                        e.Use();
                    break;
                case EventType.KeyDown:
                    if (enabled && GUIUtility.keyboardControl == id && (e.keyCode == KeyCode.Space || e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
                    {
                        clicked = true;
                        e.Use();
                    }

                    break;
                case EventType.Repaint:
                    DrawButtonFace(r, content, kind, size, id, enabled);
                    if (content != null && !string.IsNullOrEmpty(content.tooltip))
                    {
                        TipContent.tooltip = content.tooltip;
                        GUI.Label(r, TipContent, GUIStyle.none);
                    }

                    break;
            }

            if (clicked)
                GUI.changed = true;
            return clicked;
        }

        private static readonly GUIContent TipContent = new GUIContent(string.Empty);
        private static bool faceToggleOn;
        private static int faceGlyph = -1;
        public static bool ToggleButton(GUIContent content, bool on, SRButtonSize size = SRButtonSize.Default, params GUILayoutOption[] options)
        {
            float h = size == SRButtonSize.Mini ? SRMetrics.ButtonMini : size == SRButtonSize.Large ? SRMetrics.ButtonLarge : SRMetrics.Button;
            GUIStyle ts = SRStyles.ButtonText;
            FontStyle old = ts.fontStyle;
            ts.fontStyle = FontStyle.Bold;
            float w = ts.CalcSize(content).x + 4f;
            ts.fontStyle = old;
            Rect r = GUILayoutUtility.GetRect(w, h, options.Length > 0 ? options : new[] { GUILayout.Width(w), GUILayout.Height(h) });
            r.height = h;
            return ToggleButton(r, content, on, size);
        }

        public static bool ToggleButton(Rect r, GUIContent content, bool on, SRButtonSize size = SRButtonSize.Default)
        {
            faceToggleOn = on;
            bool clicked;
            try
            {
                clicked = Button(r, content, SRButtonKind.Secondary, size);
            }
            finally
            {
                faceToggleOn = false;
            }

            return clicked ? !on : on;
        }

        public static bool IconButton(GUIContent content, SRButtonKind kind = SRButtonKind.Ghost, SRButtonSize size = SRButtonSize.Default)
        {
            float s = size == SRButtonSize.Mini ? SRMetrics.ButtonMini : size == SRButtonSize.Large ? SRMetrics.ButtonLarge : SRMetrics.Button;
            Rect r = GUILayoutUtility.GetRect(s, s, GUILayout.Width(s), GUILayout.Height(s));
            return IconButton(r, content, kind);
        }

        public static bool IconButton(Rect r, GUIContent content, SRButtonKind kind = SRButtonKind.Ghost)
        {
            return Button(r, content, kind, SRButtonSize.Mini);
        }

        public static bool IconButton(Rect r, SRGlyph glyph, string tooltip, SRButtonKind kind = SRButtonKind.Ghost)
        {
            faceGlyph = (int)glyph;
            try
            {
                return Button(r, new GUIContent(string.Empty, tooltip), kind, SRButtonSize.Mini);
            }
            finally
            {
                faceGlyph = -1;
            }
        }

        private static void DrawButtonFace(Rect r, GUIContent content, SRButtonKind kind, SRButtonSize size, int id, bool enabled)
        {
            SRPalette p = SRTheme.P;
            bool hover = enabled && r.Contains(Event.current.mousePosition);
            bool down = GUIUtility.hotControl == id;
            bool focus = GUIUtility.keyboardControl == id;
            Color face, edge, text;
            switch (kind)
            {
                case SRButtonKind.Primary:
                    face = down ? p.BrandPressed : hover ? p.BrandHover : p.Brand;
                    edge = p.BrandBorder;
                    text = p.TextOnBrand;
                    break;
                case SRButtonKind.Ghost:
                    face = down ? p.ControlPressed : hover ? p.SurfaceHover : Color.clear;
                    edge = Color.clear;
                    text = p.Text;
                    break;
                case SRButtonKind.Danger:
                    face = p.DangerBg;
                    edge = hover ? p.Danger : p.DangerBorder;
                    text = p.DangerText;
                    break;
                default:
                    face = down ? p.ControlPressed : hover ? p.ControlHover : p.Control;
                    edge = p.LineControl;
                    text = p.TextStrong;
                    if (faceToggleOn)
                    {
                        face = down ? p.ControlPressed : p.BrandTint;
                        edge = p.BrandLine;
                    }

                    break;
            }

            if (!enabled)
            {
                face = kind == SRButtonKind.Ghost ? Color.clear : p.SurfaceCard;
                edge = kind == SRButtonKind.Ghost ? Color.clear : p.LineSubtle;
                text = p.TextDisabled;
                if (faceToggleOn)
                    edge = p.TextDisabled;
            }

            SRTheme.Box(r, face, focus ? p.Focus : edge, SRMetrics.RadiusButton);
            bool iconOnly = content != null && content.image != null && string.IsNullOrEmpty(content.text);
            if (faceGlyph >= 0)
            {
                float g = Mathf.Min(12f, r.height - 6f);
                Glyph(new Rect(r.center.x - g * 0.5f, r.center.y - g * 0.5f, g, g), (SRGlyph)faceGlyph, text);
                return;
            }

            if (iconOnly)
            {
                float s = Mathf.Min(16f, r.height - 4f);
                Color oldGui = GUI.color;
                if (!enabled)
                    GUI.color = new Color(oldGui.r, oldGui.g, oldGui.b, oldGui.a * 0.5f);
                GUI.DrawTexture(new Rect(r.center.x - s * 0.5f, r.center.y - s * 0.5f, s, s), content.image, ScaleMode.ScaleToFit);
                GUI.color = oldGui;
                return;
            }

            GUIStyle ts = SRStyles.ButtonText;
            Color oldColor = ts.normal.textColor;
            FontStyle oldFont = ts.fontStyle;
            int oldSize = ts.fontSize;
            SRStyles.SetTextColor(ts, text);
            ts.fontStyle = kind == SRButtonKind.Primary || faceToggleOn ? FontStyle.Bold : FontStyle.Normal;
            ts.fontSize = size == SRButtonSize.Mini ? SRMetrics.FontSmall : SRMetrics.FontBody;
            GUI.enabled = true;
            try
            {
                ts.Draw(r, content, id, false, hover);
            }
            finally
            {
                GUI.enabled = enabled;
                SRStyles.SetTextColor(ts, oldColor);
                ts.fontStyle = oldFont;
                ts.fontSize = oldSize;
            }
        }

        public static int Segmented(GUIContent label, int index, GUIContent[] options)
        {
            Rect row = EditorGUILayout.GetControlRect(true, SRMetrics.Button);
            SRPalette p = SRTheme.P;
            Rect field = row;
            if (label != null && label != GUIContent.none)
            {
                using (SRLabelScope.Begin())
                    EditorGUI.LabelField(new Rect(row.x, row.y, EditorGUIUtility.labelWidth, row.height), label);
                field = FieldPart(row);
            }

            bool enabled = GUI.enabled;
            SRTheme.Box(field, enabled ? p.Control : p.SurfaceCard, enabled ? p.LineControl : p.LineSubtle, SRMetrics.RadiusButton);
            float w = field.width / Mathf.Max(1, options.Length);
            for (int i = 0; i < options.Length; i++)
            {
                var c = new Rect(field.x + w * i, field.y, w, field.height);
                bool on = i == index;
                if (on)
                {
                    if (enabled)
                        SRTheme.Fill(new Rect(c.x + 1f, c.y + 1f, c.width - 2f, c.height - 2f), p.BrandTint, SRMetrics.RadiusButton - 1f);
                    SRTheme.Fill(new Rect(c.x + 3f, c.yMax - 1f - SRMetrics.AccentLine, c.width - 6f, SRMetrics.AccentLine), enabled ? p.BrandLine : p.TextDisabled, 1f);
                }

                if (i > 0)
                    SRTheme.Fill(new Rect(c.x, c.y, 1f, c.height), enabled ? p.LineControl : p.LineSubtle);
                GUIStyle ts = on ? SRStyles.Strong : SRStyles.Body;
                TextAnchor old = ts.alignment;
                ts.alignment = TextAnchor.MiddleCenter;
                Ink(c, options[i], ts);
                ts.alignment = old;
                if (GUI.Button(c, GUIContent.none, GUIStyle.none))
                    index = i;
            }

            return index;
        }

        private static int searchFocusId;
        public static string SearchField(string value, string placeholder, params GUILayoutOption[] options)
        {
            return SearchField(null, value, placeholder, options);
        }

        public static string SearchField(string controlName, string value, string placeholder, params GUILayoutOption[] options)
        {
            Rect r = EditorGUILayout.GetControlRect(false, SRMetrics.Button, options);
            SRPalette p = SRTheme.P;
            Event e = Event.current;
            value = value ?? string.Empty;
            string name = string.IsNullOrEmpty(controlName) ? NextFieldName() : controlName;
            float radius = r.height * 0.5f;
            SRTheme.Box(r, p.SurfaceSunken, p.LineControl, radius);
            Glyph(new Rect(r.x + 6f, r.y + (r.height - 12f) * 0.5f, 12f, 12f), SRGlyph.Search, GUI.enabled ? p.TextMuted : p.TextDisabled);
            bool hasText = value.Length > 0;
            var clear = new Rect(r.xMax - 18f, r.y + (r.height - 14f) * 0.5f, 14f, 14f);
            var text = new Rect(r.x + 20f, r.y + 1f, r.width - 26f - (hasText ? 14f : 0f), r.height - 2f);
            if (hasText && GUI.enabled)
            {
                if (e.type == EventType.Repaint && clear.Contains(e.mousePosition))
                    SRTheme.Fill(clear, p.SurfaceHover, 7f);
                Glyph(new Rect(clear.x + 3f, clear.y + 3f, 8f, 8f), SRGlyph.Close, p.TextMuted);
                EditorGUIUtility.AddCursorRect(clear, MouseCursor.Arrow);
                if (GUI.Button(clear, GUIContent.none, GUIStyle.none))
                {
                    value = string.Empty;
                    GUI.changed = true;
                    GUIUtility.keyboardControl = 0;
                }
            }

            bool hadFocus = searchFocusId != 0 && GUIUtility.keyboardControl == searchFocusId;
            if (hadFocus && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                value = string.Empty;
                GUI.changed = true;
                GUIUtility.keyboardControl = 0;
                e.Use();
            }

            SetFieldInk();
            GUI.SetNextControlName(name);
            value = EditorGUI.TextField(text, value, SRStyles.FieldText);
            bool focus = GUI.enabled && GUI.GetNameOfFocusedControl() == name;
            if (focus)
                searchFocusId = GUIUtility.keyboardControl;
            if (focus)
            {
                SRTheme.Stroke(r, p.Focus, radius, 1f);
                SRTheme.Stroke(new Rect(r.x + 1f, r.y + 1f, r.width - 2f, r.height - 2f), p.Focus, radius - 1f, 1f);
            }
            else if (value.Length == 0)
            {
                Ink(text, placeholder, SRStyles.Muted);
            }

            return value;
        }

        private static readonly int DropHash = "SRGui.DropArea".GetHashCode();
        public static UnityEngine.Object[] DropArea(GUIContent label, float height, Func<UnityEngine.Object, bool> accept = null)
        {
            Rect r = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            return DropArea(r, label, accept);
        }

        public static UnityEngine.Object[] DropArea(Rect r, GUIContent label, Func<UnityEngine.Object, bool> accept = null)
        {
            int id = GUIUtility.GetControlID(DropHash, FocusType.Passive, r);
            Event e = Event.current;
            SRPalette p = SRTheme.P;
            UnityEngine.Object[] dragged = DragAndDrop.objectReferences;
            bool over = GUI.enabled && r.Contains(e.mousePosition) && dragged != null && dragged.Length > 0;
            UnityEngine.Object[] dropped = null;
            switch (e.type)
            {
                case EventType.DragUpdated:
                    if (over)
                    {
                        DragAndDrop.visualMode = CountAccepted(dragged, accept) > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                        DragAndDrop.activeControlID = id;
                        e.Use();
                    }
                    else if (DragAndDrop.activeControlID == id)
                    {
                        DragAndDrop.activeControlID = 0;
                    }

                    break;
                case EventType.DragPerform:
                    if (over && CountAccepted(dragged, accept) > 0)
                    {
                        DragAndDrop.AcceptDrag();
                        dropped = new UnityEngine.Object[CountAccepted(dragged, accept)];
                        int n = 0;
                        foreach (UnityEngine.Object o in dragged)
                            if (o != null && (accept == null || accept(o)))
                                dropped[n++] = o;
                        DragAndDrop.activeControlID = 0;
                        GUI.changed = true;
                        e.Use();
                    }

                    break;
                case EventType.DragExited:
                    if (DragAndDrop.activeControlID == id)
                        DragAndDrop.activeControlID = 0;
                    break;
                case EventType.Repaint:
                    bool active = DragAndDrop.activeControlID == id && over;
                    bool ok = active && DragAndDrop.visualMode != DragAndDropVisualMode.Rejected;
                    if (ok)
                    {
                        SRTheme.Box(r, p.BrandTint, p.BrandLine, SRMetrics.RadiusCard);
                        SRTheme.Stroke(new Rect(r.x + 1f, r.y + 1f, r.width - 2f, r.height - 2f), p.BrandLine, SRMetrics.RadiusCard - 1f, 1f);
                    }
                    else
                    {
                        SRTheme.Fill(r, active ? p.DangerBg : p.SurfaceSunken, SRMetrics.RadiusCard);
                        DashedBorder(r, active ? p.Danger : p.LineStrong);
                    }

                    GUIStyle s = SRStyles.Center;
                    Color old = s.normal.textColor;
                    s.normal.textColor = ok ? p.TextStrong : p.TextMuted;
                    Ink(new Rect(r.x + 8f, r.y + 4f, r.width - 16f, r.height - 8f), label, s);
                    s.normal.textColor = old;
                    break;
            }

            return dropped;
        }

        private static int CountAccepted(UnityEngine.Object[] objects, Func<UnityEngine.Object, bool> accept)
        {
            int n = 0;
            if (objects == null)
                return 0;
            foreach (UnityEngine.Object o in objects)
                if (o != null && (accept == null || accept(o)))
                    n++;
            return n;
        }

        private static void DashedBorder(Rect r, Color c)
        {
            const float dash = 4f, gap = 3f, inset = 4f;
            for (float x = r.x + inset; x < r.xMax - inset; x += dash + gap)
            {
                float w = Mathf.Min(dash, r.xMax - inset - x);
                SRTheme.Fill(new Rect(x, r.y, w, 1f), c);
                SRTheme.Fill(new Rect(x, r.yMax - 1f, w, 1f), c);
            }

            for (float y = r.y + inset; y < r.yMax - inset; y += dash + gap)
            {
                float h = Mathf.Min(dash, r.yMax - inset - y);
                SRTheme.Fill(new Rect(r.x, y, 1f, h), c);
                SRTheme.Fill(new Rect(r.xMax - 1f, y, 1f, h), c);
            }
        }

        public static void Glyph(Rect r, SRGlyph glyph, Color color, float thickness = 1.6f)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            float s = Mathf.Min(r.width, r.height) / 12f;
            var o = new Vector2(r.center.x - 6f * s, r.center.y - 6f * s);
            float t = thickness * Mathf.Max(0.75f, s);
            switch (glyph)
            {
                case SRGlyph.Check:
                    Segment(o + new Vector2(2.6f, 6.4f) * s, o + new Vector2(5.0f, 8.8f) * s, color, t);
                    Segment(o + new Vector2(5.0f, 8.8f) * s, o + new Vector2(9.6f, 3.4f) * s, color, t);
                    break;
                case SRGlyph.ChevronDown:
                    Segment(o + new Vector2(3.0f, 4.6f) * s, o + new Vector2(6.0f, 7.6f) * s, color, t);
                    Segment(o + new Vector2(6.0f, 7.6f) * s, o + new Vector2(9.0f, 4.6f) * s, color, t);
                    break;
                case SRGlyph.ChevronRight:
                    Segment(o + new Vector2(4.6f, 3.0f) * s, o + new Vector2(7.6f, 6.0f) * s, color, t);
                    Segment(o + new Vector2(7.6f, 6.0f) * s, o + new Vector2(4.6f, 9.0f) * s, color, t);
                    break;
                case SRGlyph.Close:
                    Segment(o + new Vector2(3.0f, 3.0f) * s, o + new Vector2(9.0f, 9.0f) * s, color, t);
                    Segment(o + new Vector2(9.0f, 3.0f) * s, o + new Vector2(3.0f, 9.0f) * s, color, t);
                    break;
                case SRGlyph.Search:
                    float d = 7.2f * s;
                    SRTheme.Stroke(new Rect(o.x + 1.2f * s, o.y + 1.2f * s, d, d), color, d * 0.5f, t);
                    Segment(o + new Vector2(7.6f, 7.6f) * s, o + new Vector2(10.6f, 10.6f) * s, color, t);
                    break;
            }
        }

        public static void Segment(Vector2 a, Vector2 b, Color color, float thickness)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            Vector2 d = b - a;
            float len = d.magnitude;
            if (len < 0.01f)
                return;
            Vector2 mid = (a + b) * 0.5f;
            Matrix4x4 old = GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, mid);
            SRTheme.Fill(new Rect(mid.x - len * 0.5f - thickness * 0.5f, mid.y - thickness * 0.5f, len + thickness, thickness), color, thickness * 0.5f);
            GUI.matrix = old;
        }

        public static float BadgeWidth(string text)
        {
            return SRStyles.Mini.CalcSize(new GUIContent(text)).x + 14f;
        }

        public static void Badge(Rect r, string text, SRTone tone)
        {
            SRToneColors c = SRTheme.Tone(tone);
            SRTheme.Box(r, c.Bg, c.Border, SRMetrics.RadiusPill);
            GUIStyle s = SRStyles.Mini;
            Color old = s.normal.textColor;
            s.normal.textColor = tone == SRTone.Brand ? SRTheme.P.TextStrong : c.Text;
            GUI.Label(r, text, s);
            s.normal.textColor = old;
        }

        public static void BadgeLayout(string text, SRTone tone)
        {
            float w = BadgeWidth(text);
            Rect r = GUILayoutUtility.GetRect(w, 18f, GUILayout.Width(w), GUILayout.Height(18f));
            Badge(r, text, tone);
        }

        private static readonly int ChipHash = "SRGui.Chip".GetHashCode();
        public static float ChipWidth(string text, bool removable)
        {
            return SRStyles.Small.CalcSize(new GUIContent(text)).x + 16f + (removable ? 16f : 0f);
        }

        public static bool Chip(Rect r, string text, bool removable)
        {
            SRPalette p = SRTheme.P;
            if (GUI.enabled)
                SRTheme.Box(r, p.Control, p.LineControl, SRMetrics.RadiusPill);
            else
                SRTheme.Box(r, p.SurfaceCard, p.LineSubtle, SRMetrics.RadiusPill);
            GUIStyle s = SRStyles.Small;
            Color old = s.normal.textColor;
            TextAnchor oldA = s.alignment;
            s.normal.textColor = p.Text;
            s.alignment = TextAnchor.MiddleLeft;
            Ink(new Rect(r.x + 8f, r.y, r.width - 16f - (removable ? 16f : 0f), r.height), text, s);
            s.normal.textColor = old;
            s.alignment = oldA;
            if (!removable)
                return false;
            var x = new Rect(r.xMax - 20f, r.y + (r.height - 14f) * 0.5f, 14f, 14f);
            if (GUI.enabled && Event.current.type == EventType.Repaint && x.Contains(Event.current.mousePosition))
                SRTheme.Fill(x, p.SurfaceHover, 7f);
            Glyph(new Rect(x.x + 3f, x.y + 3f, 8f, 8f), SRGlyph.Close, GUI.enabled ? p.TextMuted : p.TextDisabled);
            GUIUtility.GetControlID(ChipHash, FocusType.Passive, r);
            return GUI.Button(x, GUIContent.none, GUIStyle.none);
        }

        public static bool ChipLayout(string text, bool removable)
        {
            float w = ChipWidth(text, removable);
            Rect r = GUILayoutUtility.GetRect(w, SRMetrics.Button, GUILayout.Width(w), GUILayout.Height(SRMetrics.Button));
            return Chip(r, text, removable);
        }

        public static float ColorChipWidth(string text)
        {
            return SRStyles.Small.CalcSize(new GUIContent(text)).x + 34f;
        }

        public static bool ColorChip(Rect r, Color color, string text, bool selected)
        {
            SRPalette p = SRTheme.P;
            bool hover = GUI.enabled && r.Contains(Event.current.mousePosition);
            Color face = selected ? p.BrandTint : hover ? p.ControlHover : p.Control;
            Color edge = selected ? p.BrandLine : p.LineControl;
            if (!GUI.enabled)
            {
                face = p.SurfaceCard;
                edge = selected ? p.TextDisabled : p.LineSubtle;
            }

            SRTheme.Box(r, face, edge, SRMetrics.RadiusPill);
            var sw = new Rect(r.x + 5f, r.y + (r.height - 12f) * 0.5f, 12f, 12f);
            SRTheme.Box(sw, new Color(color.r, color.g, color.b, 1f), p.LineStrong, 6f);
            GUIStyle s = selected ? SRStyles.Strong : SRStyles.Small;
            Color old = s.normal.textColor;
            TextAnchor oldA = s.alignment;
            if (!selected)
                s.normal.textColor = p.Text;
            s.alignment = TextAnchor.MiddleLeft;
            Ink(new Rect(sw.xMax + 6f, r.y, r.width - 29f, r.height), text, s);
            s.normal.textColor = old;
            s.alignment = oldA;
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        public static bool ColorChipLayout(Color color, string text, bool selected)
        {
            float w = ColorChipWidth(text);
            Rect r = GUILayoutUtility.GetRect(w, SRMetrics.Button, GUILayout.Width(w), GUILayout.Height(SRMetrics.Button));
            return ColorChip(r, color, text, selected);
        }

        public static bool HelpBox(string message, SRTone tone, string actionLabel = null)
        {
            bool hasAction = !string.IsNullOrEmpty(actionLabel);
            var content = new GUIContent(message);
            GUIStyle layout = hasAction ? SRStyles.HelpActionLayout : SRStyles.HelpLayout;
            Rect r = GUILayoutUtility.GetRect(content, layout, GUILayout.ExpandWidth(true), GUILayout.MinHeight(30f));
            bool clicked = HelpBox(r, content, tone, hasAction ? actionLabel : null);
            GUILayout.Space(SRMetrics.Space4);
            return clicked;
        }

        public static bool HelpBox(Rect r, GUIContent content, SRTone tone, string actionLabel = null)
        {
            SRToneColors c = SRTheme.Tone(tone);
            SRPalette p = SRTheme.P;
            bool hasAction = !string.IsNullOrEmpty(actionLabel);
            RectOffset pad = (hasAction ? SRStyles.HelpActionLayout : SRStyles.HelpLayout).padding;
            SRTheme.Box(r, c.Bg, c.Border, SRMetrics.RadiusButton);
            var icon = new Rect(r.x + 8f, r.y + 7f, 16f, 16f);
            SRTheme.Fill(icon, c.Mark, 8f);
            if (tone == SRTone.Success || tone == SRTone.Danger)
            {
                Glyph(new Rect(icon.x + 2f, icon.y + 2f, 12f, 12f), tone == SRTone.Success ? SRGlyph.Check : SRGlyph.Close, p.SurfaceWindow, 1.8f);
            }
            else
            {
                GUIStyle glyph = SRStyles.Mini;
                Color old = glyph.normal.textColor;
                glyph.normal.textColor = p.SurfaceWindow;
                GUI.Label(icon, tone == SRTone.Warning ? "!" : "i", glyph);
                glyph.normal.textColor = old;
            }

            var text = new Rect(r.x + pad.left, r.y + pad.top, r.width - pad.horizontal, Mathf.Max(16f, r.height - pad.vertical));
            Ink(text, content, SRStyles.Wrap);
            if (!hasAction)
                return false;
            var ac = new GUIContent(actionLabel);
            float bw = SRStyles.ButtonText.CalcSize(ac).x;
            return Button(new Rect(text.x, text.yMax + 3f, bw, SRMetrics.ButtonMini), ac, SRButtonKind.Secondary, SRButtonSize.Mini);
        }

        public static float HelpBoxHeight(string message, float width, bool hasAction = false)
        {
            GUIStyle layout = hasAction ? SRStyles.HelpActionLayout : SRStyles.HelpLayout;
            return Mathf.Max(30f, layout.CalcHeight(new GUIContent(message), width));
        }

        public static void ProgressBar(float progress01, string label, SRTone tone = SRTone.Info)
        {
            Rect r = GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true));
            SRPalette p = SRTheme.P;
            Ink(new Rect(r.x, r.y, r.width - 48f, 14f), label, SRStyles.Small);
            GUIStyle pct = SRStyles.Strong;
            TextAnchor old = pct.alignment;
            pct.alignment = TextAnchor.MiddleRight;
            Ink(new Rect(r.xMax - 48f, r.y, 48f, 14f), Mathf.RoundToInt(Mathf.Clamp01(progress01) * 100f) + "%", pct);
            pct.alignment = old;
            var bar = new Rect(r.x, r.y + 17f, r.width, 6f);
            SRTheme.Box(bar, p.SurfaceSunken, p.LineControl, 3f);
            Color fill = tone == SRTone.Success ? p.Success : tone == SRTone.Danger ? p.Danger : p.Mint;
            SRTheme.Fill(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(progress01), bar.height), fill, 3f);
        }

        public static void RowBackground(Rect r, bool selected, bool hover, bool alternate)
        {
            SRPalette p = SRTheme.P;
            Color c = selected ? p.BrandTint : hover ? p.SurfaceHover : alternate ? p.SurfaceWindow : p.SurfaceCard;
            SRTheme.Fill(r, c);
            SRTheme.Fill(new Rect(r.x, r.yMax - 1f, r.width, 1f), p.LineSubtle);
            if (selected)
                SRTheme.Stroke(r, p.BrandLine, 0f, 1f);
        }

        public static bool EmptyState(string title, string message, string actionLabel, float minHeight = 180f)
        {
            Rect r = GUILayoutUtility.GetRect(0f, minHeight, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            SRPalette p = SRTheme.P;
            SRTheme.Box(r, p.SurfaceCard, p.LineControl, SRMetrics.RadiusCard);
            float w = Mathf.Min(300f, r.width - 32f);
            float th = SRStyles.Hero.CalcHeight(new GUIContent(title), w);
            float mh = SRStyles.Center.CalcHeight(new GUIContent(message), w);
            float total = th + 8f + mh + (string.IsNullOrEmpty(actionLabel) ? 0f : 8f + SRMetrics.ButtonMini);
            float y = r.y + Mathf.Max(16f, (r.height - total) * 0.5f);
            float x = r.x + (r.width - w) * 0.5f;
            Ink(new Rect(x, y, w, th), title, SRStyles.Hero);
            y += th + 8f;
            Ink(new Rect(x, y, w, mh), message, SRStyles.Center);
            y += mh + 8f;
            if (string.IsNullOrEmpty(actionLabel))
                return false;
            var ac = new GUIContent(actionLabel);
            float bw = SRStyles.ButtonText.CalcSize(ac).x;
            return Button(new Rect(r.x + (r.width - bw) * 0.5f, y, bw, SRMetrics.ButtonMini), ac, SRButtonKind.Secondary, SRButtonSize.Mini);
        }

        public static void SceneHint(SceneView view, string text)
        {
            if (view == null || string.IsNullOrEmpty(text))
                return;
            Handles.BeginGUI();
            SRPalette p = SRTheme.P;
            GUIStyle s = SRStyles.Small;
            Color old = s.normal.textColor;
            s.normal.textColor = p.SceneHintText;
            var content = new GUIContent(text);
            Vector2 size = s.CalcSize(content);
            var r = new Rect(10f, view.position.height - size.y - 8f - 10f - 22f, size.x + 20f, size.y + 8f);
            SRTheme.Fill(r, p.SceneHintBg, SRMetrics.RadiusButton);
            GUI.Label(new Rect(r.x + 10f, r.y + 4f, size.x, size.y), content, s);
            s.normal.textColor = old;
            Handles.EndGUI();
        }

        public static GUIContent FitContent(GUIContent content, GUIStyle style, float maxWidth)
        {
            if (content == null || string.IsNullOrEmpty(content.text) || style.CalcSize(content).x <= maxWidth)
                return content;
            string tip = !string.IsNullOrEmpty(content.tooltip) ? content.tooltip : style.richText ? RichTag.Replace(content.text, string.Empty) : content.text;
            return new GUIContent(Fit(content.text, style, maxWidth), content.image, tip);
        }

        private static readonly Regex RichTag = new Regex("</?(color|b|i|size)(=[^>]*)?>", RegexOptions.IgnoreCase);
        public static string Fit(string text, GUIStyle style, float maxWidth)
        {
            if (string.IsNullOrEmpty(text) || style.CalcSize(new GUIContent(text)).x <= maxWidth)
                return text;
            const string ell = "…";
            if (!style.richText || text.IndexOf('<') < 0)
            {
                int lo = 0, hi = text.Length;
                while (lo < hi)
                {
                    int mid = (lo + hi + 1) / 2;
                    if (style.CalcSize(new GUIContent(text.Substring(0, mid) + ell)).x <= maxWidth)
                        lo = mid;
                    else
                        hi = mid - 1;
                }

                return text.Substring(0, lo) + ell;
            }

            var cut = new System.Collections.Generic.List<int>
            {
                0
            };
            for (int i = 0; i < text.Length;)
            {
                Match m = RichTag.Match(text, i);
                if (m.Success && m.Index == i)
                {
                    i += m.Length;
                    continue;
                }

                i++;
                cut.Add(i);
            }

            int a = 0, b = cut.Count - 1;
            while (a < b)
            {
                int mid = (a + b + 1) / 2;
                if (style.CalcSize(new GUIContent(ClosedPrefix(text, cut[mid]) + ell)).x <= maxWidth)
                    a = mid;
                else
                    b = mid - 1;
            }

            return ClosedPrefix(text, cut[a]) + ell;
        }

        private static string ClosedPrefix(string text, int length)
        {
            string prefix = text.Substring(0, length);
            var open = new System.Collections.Generic.List<string>();
            foreach (Match m in RichTag.Matches(prefix))
            {
                string name = m.Groups[1].Value.ToLowerInvariant();
                if (m.Value[1] != '/')
                    open.Add(name);
                else
                {
                    int at = open.LastIndexOf(name);
                    if (at >= 0)
                        open.RemoveAt(at);
                }
            }

            for (int i = open.Count - 1; i >= 0; i--)
                prefix += "</" + open[i] + ">";
            return prefix;
        }
    }
}
