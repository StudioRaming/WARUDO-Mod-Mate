using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbStyles
    {
        public static Color Accent
        {
            get
            {
                return WbbTheme.P.AccentFg;
            }
        }

        public static Color Danger
        {
            get
            {
                return WbbTheme.P.Error.Fg;
            }
        }

        public static Color Success
        {
            get
            {
                return WbbTheme.P.Success.Fg;
            }
        }

        public static Color Warning
        {
            get
            {
                return WbbTheme.P.Warning.Fg;
            }
        }

        public static Color Muted
        {
            get
            {
                return WbbTheme.P.TextSecondary;
            }
        }

        public static Color Text
        {
            get
            {
                return WbbTheme.P.TextPrimary;
            }
        }

        public const float CardRadius = WbbSize.RadiusCard;
        public const float ControlRadius = WbbSize.RadiusControl;
        private const float BadgeHeight = WbbSize.Badge.Height;
        private static GUIStyle cardTitle, card, cardFlat, guideBody, miniInfo, bodyText, caption;
        private static GUIStyle rowLabel, rowLabelBold, centeredMini, sectionLabel;
        private static GUIStyle badgeText, chipText, buttonText, primaryText, groupLabel, glyph;
        private static GUIStyle searchPlaceholder, fieldText, searchText, popupText, calloutText, progressPercent, dropHint;
        private static Texture2D cardTex, cardTex2x, cardFlatTex, cardFlatTex2x;
        private static readonly List<Texture2D> ownedTextures = new List<Texture2D>();
        private static bool builtDark;
        private static bool built;
        private static bool reloadHooked;
        private static readonly GUIContent measure = new GUIContent();
        private static GUIContent clearContent;
        private static int clearContentVersion = -1;
        private static string L(string key)
        {
            return WbbLang.T(key);
        }

        private static string L(string key, params object[] args)
        {
            return WbbLang.T(key, args);
        }

        private static GUIContent LC(string key)
        {
            return WbbLang.C(key);
        }

        private static bool Dark
        {
            get
            {
                return EditorGUIUtility.isProSkin;
            }
        }

        private static void EnsureStyles()
        {
            bool dark = Dark;
            if (built && builtDark == dark && cardTex != null && cardFlatTex != null && miniInfo != null)
                return;
            if (!reloadHooked)
            {
                // The card textures are HideAndDontSave and outlive a domain reload, but the list that owns them does
                // not. WbbWindow.OnDisable frees them; with only Quick Build or the folder browser open nothing did.
                reloadHooked = true;
                AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
            }

            DestroyTextures();
            builtDark = dark;
            built = true;
            WbbPalette p = WbbTheme.P;
            cardTex = RoundedTexture(p.BgCard, p.BorderSubtle, (int)CardRadius, 1);
            cardTex2x = RoundedTexture(p.BgCard, p.BorderSubtle, (int)CardRadius, 2);
            cardFlatTex = RoundedTexture(p.BgPanel, p.BorderSubtle, (int)CardRadius, 1);
            cardFlatTex2x = RoundedTexture(p.BgPanel, p.BorderSubtle, (int)CardRadius, 2);
            int slice = (int)CardRadius + 2;
            card = new GUIStyle
            {
                padding = WbbSize.CardPadding.ToRectOffset(),
                margin = WbbSize.CardMargin.ToRectOffset(),
                border = new RectOffset(slice, slice, slice, slice),
            };
            SetBackground(card, cardTex, cardTex2x);
            cardFlat = new GUIStyle(card);
            SetBackground(cardFlat, cardFlatTex, cardFlatTex2x);
            cardTitle = MakeText(EditorStyles.label, p.TextStrong, WbbSize.FontCardTitle);
            cardTitle.fontStyle = FontStyle.Bold;
            sectionLabel = MakeText(EditorStyles.label, p.TextStrong, WbbSize.FontSectionLabel);
            sectionLabel.fontStyle = FontStyle.Bold;
            guideBody = MakeText(EditorStyles.label, p.TextPrimary, WbbSize.FontGuide);
            guideBody.wordWrap = true;
            guideBody.richText = true;
            bodyText = MakeText(EditorStyles.label, p.TextPrimary, WbbSize.FontBody);
            bodyText.wordWrap = true;
            miniInfo = MakeText(EditorStyles.label, p.TextSecondary, WbbSize.FontSmall);
            miniInfo.wordWrap = true;
            caption = MakeText(EditorStyles.label, p.TextSecondary, WbbSize.FontSmall);
            caption.alignment = TextAnchor.MiddleLeft;
            caption.clipping = TextClipping.Clip;
            rowLabel = MakeText(EditorStyles.label, p.TextPrimary, WbbSize.FontBody);
            rowLabel.alignment = TextAnchor.MiddleLeft;
            rowLabel.clipping = TextClipping.Clip;
            rowLabel.padding = WbbSize.RowLabelPadding.ToRectOffset();
            rowLabelBold = new GUIStyle(rowLabel)
            {
                fontStyle = FontStyle.Bold
            };
            SetTextColor(rowLabelBold, p.TextStrong);
            centeredMini = MakeText(EditorStyles.label, p.TextSecondary, WbbSize.FontCaption);
            centeredMini.alignment = TextAnchor.UpperCenter;
            centeredMini.wordWrap = true;
            centeredMini.padding = WbbSize.CaptionPadding.ToRectOffset();
            badgeText = MakeText(EditorStyles.label, p.TextPrimary, WbbSize.FontMini);
            badgeText.alignment = TextAnchor.MiddleCenter;
            badgeText.fontStyle = FontStyle.Bold;
            badgeText.clipping = TextClipping.Clip;
            badgeText.padding = WbbSize.Zero.ToRectOffset();
            badgeText.margin = WbbSize.Zero.ToRectOffset();
            chipText = new GUIStyle(badgeText);
            buttonText = MakeText(EditorStyles.label, p.TextStrong, WbbSize.FontButton);
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.clipping = TextClipping.Clip;
            buttonText.padding = WbbSize.ButtonPadding.ToRectOffset();
            buttonText.margin = WbbSize.ButtonMargin.ToRectOffset();
            buttonText.imagePosition = ImagePosition.ImageLeft;
            primaryText = new GUIStyle(buttonText)
            {
                fontStyle = FontStyle.Bold
            };
            SetTextColor(primaryText, p.AccentText);
            groupLabel = MakeText(EditorStyles.label, p.TextStrong, WbbSize.FontSectionLabel);
            groupLabel.fontStyle = FontStyle.Bold;
            groupLabel.alignment = TextAnchor.MiddleLeft;
            groupLabel.clipping = TextClipping.Clip;
            glyph = MakeText(EditorStyles.label, p.TextSecondary, WbbSize.FontBody);
            glyph.alignment = TextAnchor.MiddleCenter;
            searchPlaceholder = MakeText(EditorStyles.label, p.TextSecondary, WbbSize.FontSmall);
            searchPlaceholder.alignment = TextAnchor.MiddleLeft;
            searchPlaceholder.clipping = TextClipping.Clip;
            fieldText = Bare(EditorStyles.textField, p.TextPrimary);
            fieldText.padding = WbbSize.InputTextPadding.ToRectOffset();
            fieldText.alignment = TextAnchor.MiddleLeft;
            searchText = new GUIStyle(GUIStyle.none)
            {
                font = EditorStyles.textField.font,
                fontSize = fieldText.fontSize,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                padding = WbbSize.InputTextPadding.ToRectOffset(),
            };
            SetTextColor(searchText, p.TextPrimary);
            popupText = Bare(EditorStyles.popup, p.TextStrong);
            popupText.padding = WbbSize.DropdownTextPadding.ToRectOffset();
            progressPercent = MakeText(EditorStyles.label, p.TextStrong, WbbSize.FontSmall);
            progressPercent.fontStyle = FontStyle.Bold;
            progressPercent.alignment = TextAnchor.MiddleRight;
            calloutText = MakeText(EditorStyles.label, p.TextPrimary, WbbSize.FontBody);
            calloutText.wordWrap = true;
            calloutText.richText = false;
            calloutText.padding = WbbSize.CalloutPadding.ToRectOffset();
            calloutText.margin = WbbSize.CalloutMargin.ToRectOffset();
            dropHint = MakeText(EditorStyles.label, p.TextSecondary, WbbSize.FontSmall);
            dropHint.alignment = TextAnchor.MiddleCenter;
            dropHint.wordWrap = true;
            dropHint.clipping = TextClipping.Clip;
        }

        private static GUIStyle Bare(GUIStyle source, Color color)
        {
            var s = new GUIStyle(source);
            GUIStyleState[] states =
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
            for (int i = 0; i < states.Length; i++)
            {
                states[i].background = null;
                states[i].scaledBackgrounds = new Texture2D[0];
                states[i].textColor = color;
            }

            s.border = WbbSize.Zero.ToRectOffset();
            s.margin = WbbSize.Zero.ToRectOffset();
            return s;
        }

        private static GUIStyle MakeText(GUIStyle source, Color color, int fontSize)
        {
            var style = new GUIStyle(source);
            if (fontSize > 0)
                style.fontSize = fontSize;
            SetTextColor(style, color);
            return style;
        }

        private static void SetTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            style.onNormal.textColor = color;
            style.onHover.textColor = color;
            style.onActive.textColor = color;
            style.onFocused.textColor = color;
        }

        private static void SetBackground(GUIStyle style, Texture2D tex, Texture2D tex2x)
        {
            style.normal.background = tex;
            if (tex2x != null)
                style.normal.scaledBackgrounds = new[]
                {
                    tex2x
                };
        }

        private static Texture2D RoundedTexture(Color fill, Color border, int radius, int scale)
        {
            int r = Mathf.Max(1, radius) * scale;
            int slice = r + 2 * scale;
            int size = slice * 2 + 2;
            float stroke = 1f * scale;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            var pixels = new Color[size * size];
            float half = size * 0.5f;
            float inner = half - r;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = Mathf.Abs(x + 0.5f - half);
                    float py = Mathf.Abs(y + 0.5f - half);
                    float dx = Mathf.Max(px - inner, 0f);
                    float dy = Mathf.Max(py - inner, 0f);
                    float outside = Mathf.Sqrt(dx * dx + dy * dy) - r;
                    float edge = Mathf.Max(outside, Mathf.Max(px, py) - half);
                    float coverage = Mathf.Clamp01(0.5f - edge);
                    float borderMix = Mathf.Clamp01(edge + stroke + 0.5f);
                    Color c = Color.Lerp(fill, border, borderMix);
                    c.a *= coverage;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, true);
            ownedTextures.Add(tex);
            return tex;
        }

        private static void DestroyTextures()
        {
            for (int i = 0; i < ownedTextures.Count; i++)
            {
                if (ownedTextures[i] != null)
                    Object.DestroyImmediate(ownedTextures[i]);
            }

            ownedTextures.Clear();
            cardTex = null;
            cardTex2x = null;
            cardFlatTex = null;
            cardFlatTex2x = null;
        }

        public static void Cleanup()
        {
            DestroyTextures();
            built = false;
            clearContent = null;
            clearContentVersion = -1;
            searchIconState = -1;
            iconCache.Clear();
            iconCacheState = -1;
        }

        public static GUIStyle MiniInfo
        {
            get
            {
                EnsureStyles();
                return miniInfo;
            }
        }

        public static GUIStyle Caption
        {
            get
            {
                EnsureStyles();
                return caption;
            }
        }

        public static GUIStyle BodyText
        {
            get
            {
                EnsureStyles();
                return bodyText;
            }
        }

        public static GUIStyle GuideBody
        {
            get
            {
                EnsureStyles();
                return guideBody;
            }
        }

        public static GUIStyle CardTitle
        {
            get
            {
                EnsureStyles();
                return cardTitle;
            }
        }

        public static GUIStyle SectionLabel
        {
            get
            {
                EnsureStyles();
                return sectionLabel;
            }
        }

        public static GUIStyle RowLabel
        {
            get
            {
                EnsureStyles();
                return rowLabel;
            }
        }

        public static GUIStyle RowLabelBold
        {
            get
            {
                EnsureStyles();
                return rowLabelBold;
            }
        }

        public static GUIStyle CenteredMini
        {
            get
            {
                EnsureStyles();
                return centeredMini;
            }
        }

        public static GUIStyle GroupLabel
        {
            get
            {
                EnsureStyles();
                return groupLabel;
            }
        }

        public static GUIStyle Card
        {
            get
            {
                EnsureStyles();
                return card;
            }
        }

        public static GUIStyle CardFlat
        {
            get
            {
                EnsureStyles();
                return cardFlat;
            }
        }
    }
}
