using StudioRaming.WarudoBatchBuilder.UI;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public enum WbbTone
    {
        Neutral,
        Accent,
        Success,
        Warning,
        Error,
        Info,
        Running,
        Cancel,
        Skip,
    }

    public struct WbbToneColors
    {
        public Color Base;
        public Color Fg;
        public Color BadgeBg;
        public Color BadgeBorder;
        public WbbToneColors(Color b, Color f, Color bg, Color border)
        {
            Base = b;
            Fg = f;
            BadgeBg = bg;
            BadgeBorder = border;
        }
    }

    public sealed class WbbPalette
    {
        public Color BgWindow;
        public Color BgPanel;
        public Color BgCard;
        public Color BgCardHover;
        public Color BgSelected;
        public Color BgHeader;
        public Color BgSunken;
        public Color HeaderText;
        public Color HeaderSubText;
        public Color BorderSubtle;
        public Color BorderStrong;
        public Color BorderMark;
        public Color TextStrong;
        public Color TextPrimary;
        public Color TextSecondary;
        public Color TextDisabled;
        public Color Control;
        public Color ControlHover;
        public Color ControlPressed;
        public Color Accent;
        public Color AccentHover;
        public Color AccentPressed;
        public Color AccentBorder;
        public Color AccentLine;
        public Color AccentText;
        public Color AccentFg;
        public Color Focus;
        public Color Mint;
        public Color Link;
        public Color Product;
        public Color Scrim;
        public Color Transparent;
        public Color OverlayGroupHeader;
        public Color OverlayFolderRow;
        public Color OverlayFolderHover;
        public Color OverlayDropArea;
        public Color DropAreaLine;
        public Color ThumbWell;
        public WbbToneColors Neutral;
        public WbbToneColors Success;
        public WbbToneColors Error;
        public WbbToneColors Warning;
        public WbbToneColors Running;
        public WbbToneColors Cancel;
        public WbbToneColors Skip;
        public WbbToneColors Info;
        public WbbToneColors AccentTone;
    }

    public static class WbbTheme
    {
        private static WbbPalette dark;
        private static WbbPalette light;
        public static bool Dark
        {
            get
            {
                return EditorGUIUtility.isProSkin;
            }
        }

        public static WbbPalette P
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

        public const float DimmedAlpha = 0.55f;
        public const float DisabledIconAlpha = 0.5f;
        public const float IndexSaturation = 0.70f;
        public const float IndexValue = 0.95f;
        public static WbbToneColors Tone(WbbTone tone)
        {
            WbbPalette p = P;
            switch (tone)
            {
                case WbbTone.Accent:
                    return p.AccentTone;
                case WbbTone.Success:
                    return p.Success;
                case WbbTone.Warning:
                    return p.Warning;
                case WbbTone.Error:
                    return p.Error;
                case WbbTone.Info:
                    return p.Info;
                case WbbTone.Running:
                    return p.Running;
                case WbbTone.Cancel:
                    return p.Cancel;
                case WbbTone.Skip:
                    return p.Skip;
                default:
                    return p.Neutral;
            }
        }

        public static Color Fg(WbbTone tone)
        {
            return Tone(tone).Fg;
        }

        public static Color ScaleAlpha(Color color, float factor)
        {
            return new Color(color.r, color.g, color.b, color.a * factor);
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

        public static void Box(Rect rect, Color fill, Color border, float radius = 4f)
        {
            Fill(rect, fill, radius);
            Stroke(rect, border, radius, 1f);
        }

        private static WbbPalette BuildDark()
        {
            return FromShared(SRTheme.P);
        }

        private static WbbPalette BuildLight()
        {
            return FromShared(SRTheme.P);
        }

        private static WbbPalette FromShared(SRPalette s)
        {
            var p = new WbbPalette
            {
                BgWindow = s.SurfaceWindow,
                BgPanel = s.SurfaceSection,
                BgCard = s.SurfaceCard,
                BgCardHover = s.SurfaceHover,
                BgSelected = s.BrandTint,
                BgHeader = s.SurfaceApp,
                BgSunken = s.SurfaceSunken,
                HeaderText = s.TextStrong,
                HeaderSubText = s.TextMuted,
                BorderSubtle = s.LineSubtle,
                BorderStrong = s.LineControl,
                BorderMark = s.LineStrong,
                TextStrong = s.TextStrong,
                TextPrimary = s.Text,
                TextSecondary = s.TextMuted,
                TextDisabled = s.TextDisabled,
                Control = s.Control,
                ControlHover = s.ControlHover,
                ControlPressed = s.ControlPressed,
                Accent = s.Brand,
                AccentHover = s.BrandHover,
                AccentPressed = s.BrandPressed,
                AccentBorder = s.BrandBorder,
                AccentLine = s.BrandLine,
                AccentText = s.TextOnBrand,
                AccentFg = s.BrandText,
                Focus = s.Focus,
                Mint = s.Mint,
                Link = s.Link,
                Product = s.ProductWbb,
                Scrim = s.Scrim,
                Transparent = new Color(0f, 0f, 0f, 0f),
                OverlayGroupHeader = s.SurfaceSection,
                OverlayFolderRow = s.SurfaceSection,
                OverlayFolderHover = s.SurfaceHover,
                OverlayDropArea = s.SurfaceSunken,
                DropAreaLine = s.LineStrong,
                ThumbWell = s.SurfaceSunken,
                Success = new WbbToneColors(s.Success, s.SuccessText, s.SuccessBg, s.SuccessBorder),
                Error = new WbbToneColors(s.Danger, s.DangerText, s.DangerBg, s.DangerBorder),
                Warning = new WbbToneColors(s.Warning, s.WarningText, s.WarningBg, s.WarningBorder),
                Info = new WbbToneColors(s.Info, s.InfoText, s.InfoBg, s.InfoBorder),
                Running = new WbbToneColors(s.Info, s.InfoText, s.InfoBg, s.InfoBorder),
                AccentTone = new WbbToneColors(s.Brand, s.TextStrong, s.BrandTint, s.BrandBorder),
            };
            p.Neutral = new WbbToneColors(s.LineStrong, s.TextMuted, s.SurfaceSection, s.LineControl);
            p.Cancel = p.Neutral;
            p.Skip = p.Neutral;
            return p;
        }
    }

    public struct WbbInsets
    {
        public readonly int Left;
        public readonly int Right;
        public readonly int Top;
        public readonly int Bottom;
        public WbbInsets(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public RectOffset ToRectOffset()
        {
            return new RectOffset(Left, Right, Top, Bottom);
        }
    }

    public static class WbbSize
    {
        public const float RadiusCard = SRMetrics.RadiusCard;
        public const float RadiusControl = SRMetrics.RadiusButton;
        public const float RadiusInput = SRMetrics.RadiusControl;
        public const float RadiusPill = SRMetrics.RadiusPill;
        public const float RadiusThumb = SRMetrics.RadiusControl;
        public const float RadiusThumbCard = SRMetrics.RadiusButton;
        public const float Line = 1f;
        public const float TabUnderline = SRMetrics.AccentLine;
        public const float SpaceHair = SRMetrics.Spacing;
        public const float SpaceTight = SRMetrics.Spacing;
        public const float SpaceSmall = SRMetrics.Space4;
        public const float SpaceSnug = SRMetrics.Space4;
        public const float SpaceMedium = SRMetrics.Space6;
        public const float SpaceLarge = SRMetrics.Space8;
        public const float SpaceEdge = SRMetrics.Space12;
        public const float SpaceGroup = SRMetrics.Space16;
        public const float LineHeight = SRMetrics.Line;
        public const float ControlMini = SRMetrics.ButtonMini;
        public const float ControlSmall = SRMetrics.Button;
        public const float Control = SRMetrics.Button;
        public const float ControlLarge = SRMetrics.ButtonLarge;
        public const float ActionButton = SRMetrics.ButtonLarge;
        public const float RunButton = SRMetrics.ButtonHero;
        public const float CheckSize = 16f;
        public const float CheckBox = 14f;
        public const float FoldoutSize = 13f;
        public const float Glyph = 12f;
        public const float GlyphGap = SRMetrics.Space4;
        public const float GlyphAdvance = Glyph + GlyphGap;
        public const float GlyphStroke = 1.6f;
        public const float GlyphStrokeBold = 1.8f;
        public const float Icon = 16f;
        public const float IconGap = SRMetrics.Space4;
        public const float IconAdvance = Icon + IconGap;
        public const float IconButton = 22f;
        public const float IconButtonWide = 26f;
        public const float RowButtonInset = 2f;
        public const float TextMinWidth = 40f;
        public const float MeasureMinWidth = 8f;
        public const int FontHero = 19;
        public const int FontTitle = 14;
        public const int FontBody = 12;
        public const int FontSmall = 11;
        public const int FontMini = 10;
        public const int FontHeaderTitle = FontTitle;
        public const int FontHeaderButton = FontBody;
        public const int FontTab = FontBody;
        public const int FontCardTitle = FontBody;
        public const int FontSectionLabel = FontBody;
        public const int FontGuide = FontBody;
        public const int FontCaption = FontSmall;
        public const int FontButton = FontBody;
        public const int FontButtonMini = FontSmall;
        public static readonly WbbInsets CardPadding = new WbbInsets(8, 8, 8, 8);
        public static readonly WbbInsets CardMargin = new WbbInsets(4, 4, 3, 4);
        public static float CardInsetX
        {
            get
            {
                return CardMargin.Left + CardMargin.Right + CardPadding.Left + CardPadding.Right;
            }
        }

        public static readonly WbbInsets ButtonPadding = new WbbInsets(8, 8, 0, 0);
        public static readonly WbbInsets ButtonMargin = new WbbInsets(3, 3, 2, 2);
        public static readonly WbbInsets CalloutPadding = new WbbInsets(32, 8, 6, 6);
        public static readonly WbbInsets CalloutMargin = new WbbInsets(4, 4, 2, 4);
        public static readonly WbbInsets InputTextPadding = new WbbInsets(4, 4, 1, 1);
        public static readonly WbbInsets DropdownTextPadding = new WbbInsets(6, 18, 1, 1);
        public static readonly WbbInsets RowLabelPadding = new WbbInsets(2, 2, 0, 0);
        public static readonly WbbInsets CaptionPadding = new WbbInsets(1, 1, 0, 0);
        public static readonly WbbInsets Zero = new WbbInsets(0, 0, 0, 0);
        public static class Header
        {
            public const float Height = SRMetrics.Header;
            public const float AccentLine = SRMetrics.AccentLine;
            public const float HelpWidth = 22f;
            public const float ToolGap = SRMetrics.Space4;
        }

        public static class Tabs
        {
            public const float Height = SRMetrics.Tab;
        }

        public static class Section
        {
            public const float TitleHeight = 20f;
            public const float LabelLeft = 2f;
            public const float LabelHeight = SRMetrics.Line;
            public const float LabelMinWidth = 10f;
            public const float LabelRightPad = 4f;
            public const float SeparatorGap = SRMetrics.Space4;
            public const float SeparatorInset = 2f;
            public const float HeaderHeight = 22f;
            public const float HeaderInset = 6f;
        }

        public const float EmptyStateHeight = 160f;
        public static class Badge
        {
            public const float Height = 18f;
            public const float TextInset = 7f;
            public const float ExtraWidth = 14f;
            public const float MinWidth = 54f;
            public const float ChipTextPad = 6f;
        }

        public static class Group
        {
            public const float ArrowLeft = 6f;
            public const float ArrowWidth = 12f;
            public const float CheckGap = 4f;
            public const float TitleGap = 6f;
            public const float TitleGapNoCheck = 2f;
            public const float TitleMinWidth = 10f;
            public const float RightReserve = 150f;
            public const float DetailGap = 6f;
            public const float DetailRightPad = 12f;
        }

        public static class Field
        {
            public const float SegmentInset = 1f;
            public const float SearchMinWidth = 120f;
            public const float SearchFieldMinWidth = 40f;
            public const float SearchGlyphLeft = 6f;
            public const float SearchGlyphWidth = 14f;
            public const float SearchTextLeft = 20f;
            public const float SearchTextRight = 6f;
            public const float SearchClearWidth = 20f;
            public const float ProgressHeight = 24f;
            public const float ProgressLabelHeight = 14f;
            public const float ProgressBarTop = 17f;
            public const float ProgressBarHeight = 6f;
            public const float ProgressPercentWidth = 48f;
            public const float DropdownArrowWidth = 16f;
            public const float CheckDashInset = 3f;
            public const float CalloutIconLeft = 8f;
            public const float CalloutIconTop = 7f;
            public const float CalloutIcon = 16f;
            public const float CalloutMaxWidth = 900f;
            public const float FitMinWidth = 2f;
        }

        public static class Window
        {
            public const float MinWidth = 520f;
            public const float MinHeight = 560f;
        }

        public static class Build
        {
            public const float NarrowWidth = 720f;
            public const float ListMinWidth = 480f;
            public const float TreeWidth = 220f;
            public const float TreeMinWidth = 150f;
            public const float TreeMaxWidth = 420f;
            public const float TreeNarrowRatio = 0.45f;
            public const float SplitterWidth = 4f;
            public const float SplitterLineLeft = 1.5f;
            public const float TreeToggleWidth = 30f;
            public const float SearchMinWidth = 90f;
            public const float ViewSegmentWidth = 112f;
            public const float ScopeChipMinWidth = 80f;
            public const float ScopeChipMaxRatio = 0.4f;
            public const float IndexProgressHeight = Field.ProgressHeight;
            public const float SuggestAcceptMinWidth = 120f;
            public const float SuggestLaterMinWidth = 70f;
            public const float RowHeight = 36f;
            public const float GroupHeaderHeight = 26f;
            public const float GroupGap = 4f;
            public const float GroupTipLeft = 40f;
            public const float MeasureMinWidth = WbbSize.MeasureMinWidth;
            public const float RowInsetX = 2f;
            public const float RowPadLeft = 8f;
            public const float RowCheckAdvance = 22f;
            public const float RowThumbInset = 3f;
            public const float RowThumbGap = 8f;
            public const float RowPadRight = 6f;
            public const float RowMenuAdvance = 26f;
            public const float RowSceneActionMinWidth = 56f;
            public const float RowSceneActionMaxWidth = 86f;
            public const float RowSceneActionRatio = 0.16f;
            public const float RowActionGap = 6f;
            public const float RowIconActionAdvance = 28f;
            public const float RowWideWidth = 520f;
            public const float ColDate = 78f;
            public const float ColDateAdvance = 82f;
            public const float BadgeMinWidth = 56f;
            public const float BadgeMaxWidth = 104f;
            public const float BadgeGap = 8f;
            public const float NameTop = 3f;
            public const float NameHeight = 18f;
            public const float SubTop = 19f;
            public const float SubHeight = 14f;
            public const float CardHeight = 138f;
            public const float MinCellWidth = 118f;
            public const float GridMinWidth = 120f;
            public const float GridWidthReserve = 8f;
            public const float CardMinWidth = 64f;
            public const float CardCellInset = 2f;
            public const float CardPad = 5f;
            public const float CardThumbMinHeight = 34f;
            public const float CardTextReserve = 42f;
            public const float CardThumbMinWidth = 16f;
            public const float CardNameGap = 3f;
            public const float CardNameHeight = 16f;
            public const float CardSubHeight = 14f;
            public const float CardBadgeInset = 3f;
            public const float CardBadgeSize = 18f;
            public const float CardButtonRight = 22f;
            public const float CardButtonTop = 3f;
            public const float CardButtonBottom = 21f;
            public const float CardButtonWidth = 20f;
            public const float CardButtonHeight = 18f;
            public const float ModeSegmentCellPad = 24f;
            public const float TypePopupWidth = 110f;
            public const float VersionFieldWidth = 64f;
            public const float AuthorFieldMinWidth = 60f;
            public const float PresetButtonMaxWidth = 180f;
            public const float SetFromSelectionButton = 40f;
            public const float BuildProgressHeight = Field.ProgressHeight;
            public const float ResultCloseTop = (BuildProgressHeight - ControlSmall) * 0.5f;
            public const float StopButtonWidth = 132f;
            public const float PreflightChipMinWidth = 120f;
            public const float PreflightChipReserve = 250f;
            public const float PreflightChipPad = 14f;
            public const float RunButtonMinWidth = 170f;
            public const float RestoreSettingsButton = 110f;
            public const float OutputPickButton = 80f;
        }

        public static class History
        {
            public const float RowHeight = 22f;
            public const float StatusToggleButton = 64f;
            public const float GroupPopup = 96f;
            public const float SearchMinWidth = 140f;
            public const float SearchHelpWidth = 20f;
            public const float SearchHelpHeight = 18f;
            public const float ExportCsvButton = 118f;
            public const float RefreshButton = 76f;
            public const float SelectAllButton = 88f;
            public const float DeselectAllButton = 88f;
            public const float SelectionLabel = 90f;
            public const float RebuildSelectedButton = 118f;
            public const float RebuildFailedButton = 140f;
            public const float CleanOldButton = 186f;
            public const float GroupArrowLeft = 6f;
            public const float GroupArrowWidth = 16f;
            public const float GroupTitleLeft = 24f;
            public const float GroupTitleMinWidth = 20f;
            public const float GroupTitleReserve = 28f;
            public const float RowInset = 4f;
            public const float CheckTop = 3f;
            public const float CheckAdvance = 21f;
            public const float MenuHeightTrim = 4f;
            public const float MenuAdvance = 25f;
            public const float HugeMinWidth = 700f;
            public const float WideMinWidth = 560f;
            public const float MediumMinWidth = 430f;
            public const float ColStatus = 58f;
            public const float ColStatusStep = 62f;
            public const float ColTime = 76f;
            public const float ColTimeStep = 78f;
            public const float ColSize = 60f;
            public const float ColSizeStep = 62f;
            public const float ColDuration = 50f;
            public const float ColDurationStep = 52f;
            public const float ColAuthor = 86f;
            public const float ColAuthorStep = 88f;
            public const float ColVersion = 52f;
            public const float ColVersionStep = 54f;
            public const float ColType = 52f;
            public const float ColTypeStep = 54f;
            public const float ColFile = 62f;
            public const float ColFileStep = 66f;
        }

        public static class Library
        {
            public const float RowHeight = 22f;
            public const float TypeNameLabel = 128f;
            public const float OpenFolderButton = 84f;
            public const float SetPathButton = 110f;
            public const float SearchMinWidth = 140f;
            public const float SearchHelpWidth = 20f;
            public const float SearchHelpHeight = 18f;
            public const float SortPopup = 104f;
            public const float RefreshButton = 76f;
            public const float SelectAllButton = 88f;
            public const float DeselectAllButton = 88f;
            public const float RebuildSelectedButton = 120f;
            public const float RowInset = 4f;
            public const float CheckTop = 3f;
            public const float CheckAdvance = 21f;
            public const float MenuHeightTrim = 4f;
            public const float MenuAdvance = 25f;
            public const float WideMinWidth = 560f;
            public const float MediumMinWidth = 390f;
            public const float ColDate = 124f;
            public const float ColDateStep = 130f;
            public const float ColSize = 66f;
            public const float ColSizeStep = 68f;
            public const float ColVersion = 56f;
            public const float ColVersionStep = 58f;
            public const float ColBadge = 76f;
            public const float ColBadgeStep = 80f;
            public const float NameMinWidth = 36f;
        }

        public static class Package
        {
            public const float RowHeight = 20f;
            public const float TargetRowHeight = 20f;
            public const float EmbeddedListHeight = 160f;
            public const float EmbeddedEmptyHeight = EmbeddedListHeight - SpaceLarge;
            public const float IndentWidth = 13f;
            public const float TreeLeft = 4f;
            public const float ColFold = 14f;
            public const float FoldInset = 2f;
            public const float ColCheck = 19f;
            public const float CheckTop = 2f;
            public const float FolderRowRight = 6f;
            public const float ColFolderInfo = 140f;
            public const float ColFolderInfoReserve = 144f;
            public const float RowRight = 4f;
            public const float PingButton = 24f;
            public const float PingReserve = 27f;
            public const float RemoveButton = 20f;
            public const float RemoveReserve = 23f;
            public const float ColSize = 62f;
            public const float ColSizeReserve = 64f;
            public const float RootBadge = 44f;
            public const float RootBadgeReserve = 47f;
            public const float StrikeInset = 4f;
            public const float StrikeMinWidth = 4f;
            public const float StrikeLeft = 2f;
            public const float TargetTextMinWidth = 60f;
            public const float TargetTextReserve = 10f;
            public const float TargetNameMinWidth = 50f;
            public const float TargetNameRatio = 0.42f;
            public const float TargetNameLeft = 6f;
            public const float TargetFileReserve = 6f;
            public const float TargetScrollPad = 4f;
            public const float DropAreaHeight = 26f;
            public const float DropAreaLeft = 2f;
            public const float DropAreaTrim = 4f;
            public const float DropAreaMinWidth = 20f;
            public const float AddSelectionButton = 150f;
            public const float ClearTargetsButton = 96f;
            public const float BrowseButton = 30f;
            public const float OutputOpenFolderButton = 84f;
            public const float SearchMinWidth = 130f;
            public const float RefreshButton = 72f;
            public const float SelectAllButton = 80f;
            public const float ClearExcludedButton = 130f;
            public const float BarOpenFolderButton = 96f;
            public const float ExportButton = 170f;
        }

        public static class Folder
        {
            public const float RowHeight = 18f;
            public const float RowLabelMinWidth = 24f;
            public const float PickerWrapMinWidth = 140f;
            public const float PickerWrapReserve = 52f;
            public const float PickerGridWrapReserve = 60f;
            public const float PickerGridMaxHeight = 150f;
            public const float PickerBrowseButton = 74f;
            public const float PickerPingButton = 52f;
            public const float PickerUpButton = 78f;
            public const float PickerButtonTextPad = 12f;
            public const float PickerButtonMinWidth = 48f;
            public const float PickerButtonMaxWidth = 260f;
            public const float PickerButtonGap = 2f;
            public const float BrowserMinWidth = 340f;
            public const float BrowserMinHeight = 360f;
            public const float BrowserWidth = 440f;
            public const float BrowserHeight = 480f;
            public const float BrowserMouseOffsetX = 200f;
            public const float BrowserMouseOffsetY = 20f;
            public const float BrowserFallbackScreenPos = 200f;
            public const float BrowserSearchMinWidth = 140f;
            public const float BrowserRefreshButton = 72f;
            public const float BrowserSelectButton = 120f;
            public const float BrowserCancelButton = 72f;
            public const float BrowserIndentBase = 4f;
            public const float BrowserIndentStep = 14f;
            public const float BrowserFoldWidth = 14f;
            public const float BrowserLabelLeft = 16f;
            public const float BrowserLabelReserve = 20f;
            public const float TreeFilterButton = 28f;
            public const float TreeFilterButtonHeight = 18f;
            public const float TreeCollapseButton = 24f;
            public const float TreeSearchMinWidth = 60f;
            public const float TreeIndentBase = 2f;
            public const float TreeIndentStep = 12f;
            public const float TreeFoldWidth = 12f;
            public const float TreeLabelLeft = 14f;
            public const float TreeLabelReserve = 16f;
        }

        public static class Quick
        {
            public const float WindowWidth = 560f;
            public const float MinWidth = 440f;
            public const float MinHeight = 240f;
            public const float ChromeHeightEstimate = 215f;
            public const float RowHeightEstimate = 108f;
            public const float OpenMinHeight = 320f;
            public const float OpenMaxHeight = 780f;
            public const float OpenTopRatio = 0.4f;
            public const float ThumbSize = 96f;
            public const float OpenSettingsButton = 96f;
            public const float CancelButton = 90f;
            public const float BuildButton = 130f;
        }
    }
}
