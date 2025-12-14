namespace DynamicForms.Models.Theming;

/// <summary>
/// Metadata about a theme preset for display in the theme selector.
/// </summary>
/// <param name="Id">Unique identifier for the preset (e.g., "default", "corporate").</param>
/// <param name="Name">Display name of the preset.</param>
/// <param name="Description">Brief description of the theme style.</param>
/// <param name="Category">Grouping category (e.g., "Professional", "Government", "Dark").</param>
/// <param name="Mode">Whether this is a Light or Dark theme.</param>
/// <param name="PreviewColor">Primary color for visual preview in selectors.</param>
/// <param name="Tags">Optional tags for filtering (e.g., "accessible", "modern", "minimal").</param>
public record ThemePresetInfo(
    string Id,
    string Name,
    string Description,
    string Category,
    ThemeMode Mode,
    string PreviewColor,
    string[]? Tags = null
);

/// <summary>
/// Pre-built theme presets for the Theme Editor.
/// Provides ready-to-use themes for various use cases including professional,
/// government, dark mode, accessibility, and industry-specific themes.
/// </summary>
public static class ThemePresets
{
    #region Preset Categories

    public const string CategoryProfessional = "Professional";
    public const string CategoryGovernment = "Government";
    public const string CategoryDark = "Dark";
    public const string CategoryAccessibility = "Accessibility";
    public const string CategoryIndustry = "Industry";
    public const string CategoryMinimal = "Minimal";

    #endregion

    #region Preset Dictionary

    /// <summary>
    /// Dictionary of all available theme presets keyed by their ID.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, FormTheme> Presets = new Dictionary<string, FormTheme>
    {
        // ============================================================
        // PROFESSIONAL THEMES
        // ============================================================

        ["default"] = new FormTheme
        {
            Id = "default",
            Name = "Default",
            Description = "Clean, professional default theme with indigo accents",
            BasePreset = "default",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#6366F1",
                PrimaryHover = "#4F46E5",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#64748B",
                SecondaryHover = "#475569",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#F8FAFC",
                Surface = "#FFFFFF",
                SurfaceHover = "#F1F5F9",
                TextPrimary = "#0F172A",
                TextSecondary = "#64748B",
                TextDisabled = "#94A3B8",
                TextPlaceholder = "#94A3B8",
                Border = "#E2E8F0",
                BorderHover = "#CBD5E1",
                BorderFocus = "#6366F1",
                Error = "#EF4444",
                ErrorBackground = "#FEF2F2",
                Success = "#10B981",
                SuccessBackground = "#F0FDF4",
                Warning = "#F59E0B",
                WarningBackground = "#FFFBEB",
                Info = "#3B82F6",
                InfoBackground = "#EFF6FF",
                FocusRing = "#6366F1",
                Selection = "#E0E7FF"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'DM Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
                BaseFontSize = "14px",
                LineHeight = "1.5",
                FormTitleSize = "24px",
                SectionTitleSize = "18px",
                QuestionTitleSize = "14px",
                DescriptionSize = "13px"
            },
            Borders = new ThemeBorders
            {
                RadiusMedium = "6px",
                RadiusLarge = "8px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)"
            }
        },

        ["corporate"] = new FormTheme
        {
            Id = "corporate",
            Name = "Corporate",
            Description = "Traditional business styling with sharp corners and navy blue",
            BasePreset = "corporate",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#1E40AF",
                PrimaryHover = "#1E3A8A",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#4B5563",
                SecondaryHover = "#374151",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#F9FAFB",
                Surface = "#FFFFFF",
                SurfaceHover = "#F3F4F6",
                TextPrimary = "#1F2937",
                TextSecondary = "#6B7280",
                TextDisabled = "#9CA3AF",
                TextPlaceholder = "#9CA3AF",
                Border = "#D1D5DB",
                BorderHover = "#9CA3AF",
                BorderFocus = "#1E40AF",
                Error = "#DC2626",
                ErrorBackground = "#FEF2F2",
                Success = "#059669",
                SuccessBackground = "#ECFDF5",
                Warning = "#D97706",
                WarningBackground = "#FFFBEB",
                Info = "#2563EB",
                InfoBackground = "#EFF6FF",
                FocusRing = "#1E40AF",
                Selection = "#DBEAFE"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Segoe UI', 'Roboto', -apple-system, BlinkMacSystemFont, sans-serif",
                BaseFontSize = "14px",
                LineHeight = "1.5",
                FormTitleSize = "22px",
                SectionTitleSize = "16px",
                QuestionTitleSize = "14px",
                DescriptionSize = "13px",
                FontWeightSemibold = "600"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "28px",
                QuestionSpacing = "18px"
            },
            Borders = new ThemeBorders
            {
                BorderWidth = "1px",
                RadiusSmall = "2px",
                RadiusMedium = "4px",
                RadiusLarge = "4px",
                RadiusXLarge = "6px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 1px 2px rgba(0, 0, 0, 0.05)"
            }
        },

        ["modern"] = new FormTheme
        {
            Id = "modern",
            Name = "Modern",
            Description = "Contemporary minimalist design with purple accents and rounded corners",
            BasePreset = "modern",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#8B5CF6",
                PrimaryHover = "#7C3AED",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#71717A",
                SecondaryHover = "#52525B",
                SecondaryForeground = "#FFFFFF",
                Background = "#FAFAFA",
                BackgroundDim = "#F4F4F5",
                Surface = "#FFFFFF",
                SurfaceHover = "#F4F4F5",
                TextPrimary = "#18181B",
                TextSecondary = "#71717A",
                TextDisabled = "#A1A1AA",
                TextPlaceholder = "#A1A1AA",
                Border = "#E4E4E7",
                BorderHover = "#D4D4D8",
                BorderFocus = "#8B5CF6",
                Error = "#EF4444",
                ErrorBackground = "#FEF2F2",
                Success = "#22C55E",
                SuccessBackground = "#F0FDF4",
                Warning = "#F59E0B",
                WarningBackground = "#FFFBEB",
                Info = "#6366F1",
                InfoBackground = "#EEF2FF",
                FocusRing = "#8B5CF6",
                Selection = "#EDE9FE"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
                BaseFontSize = "15px",
                LineHeight = "1.6",
                FormTitleSize = "28px",
                SectionTitleSize = "20px",
                QuestionTitleSize = "15px",
                DescriptionSize = "14px",
                FontWeightMedium = "500",
                FontWeightSemibold = "600"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "36px",
                SectionSpacing = "28px",
                QuestionSpacing = "24px",
                InputPadding = "14px"
            },
            Borders = new ThemeBorders
            {
                RadiusSmall = "6px",
                RadiusMedium = "10px",
                RadiusLarge = "14px",
                RadiusXLarge = "18px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 4px 20px rgba(0, 0, 0, 0.06)",
                InputFocusShadow = "0 0 0 4px rgba(139, 92, 246, 0.12)"
            }
        },

        // ============================================================
        // GOVERNMENT THEMES
        // ============================================================

        ["government-federal"] = new FormTheme
        {
            Id = "government-federal",
            Name = "U.S. Federal",
            Description = "U.S. Federal agency styling inspired by USWDS design system",
            BasePreset = "government-federal",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#005EA2",
                PrimaryHover = "#1A4480",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#71767A",
                SecondaryHover = "#565C65",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#F0F0F0",
                Surface = "#FFFFFF",
                SurfaceHover = "#F0F0F0",
                TextPrimary = "#1B1B1B",
                TextSecondary = "#565C65",
                TextDisabled = "#A9AEB1",
                TextPlaceholder = "#71767A",
                Border = "#C6CACE",
                BorderHover = "#A9AEB1",
                BorderFocus = "#005EA2",
                Error = "#B50909",
                ErrorBackground = "#F4E3DB",
                Success = "#00A91C",
                SuccessBackground = "#DDF9C7",
                Warning = "#FFBE2E",
                WarningBackground = "#FAF3D1",
                Info = "#00BDE3",
                InfoBackground = "#E1F3F8",
                FocusRing = "#2491FF",
                Selection = "#D9E8F6"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Public Sans', -apple-system, BlinkMacSystemFont, 'Helvetica Neue', sans-serif",
                BaseFontSize = "16px",
                LineHeight = "1.5",
                FormTitleSize = "28px",
                SectionTitleSize = "22px",
                QuestionTitleSize = "16px",
                DescriptionSize = "14px",
                FontWeightNormal = "400",
                FontWeightBold = "700"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "32px",
                SectionSpacing = "32px",
                QuestionSpacing = "24px",
                LabelSpacing = "8px"
            },
            Borders = new ThemeBorders
            {
                BorderWidth = "1px",
                RadiusSmall = "0px",
                RadiusMedium = "0px",
                RadiusLarge = "0px",
                RadiusXLarge = "0px",
                FocusRingWidth = "2px",
                FocusRingOffset = "2px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "none",
                InputFocusShadow = "0 0 0 4px #2491FF"
            },
            Accessibility = new ThemeAccessibility
            {
                MinFontSize = "16px",
                ContrastTarget = "aa"
            }
        },

        ["government-canada"] = new FormTheme
        {
            Id = "government-canada",
            Name = "Government of Canada",
            Description = "Canada.ca Web Experience Toolkit (WET) styling",
            BasePreset = "government-canada",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#26374A",
                PrimaryHover = "#1C578A",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#335075",
                SecondaryHover = "#26374A",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#F8F8F8",
                Surface = "#FFFFFF",
                SurfaceHover = "#F5F5F5",
                TextPrimary = "#333333",
                TextSecondary = "#555555",
                TextDisabled = "#999999",
                TextPlaceholder = "#767676",
                Border = "#CCCCCC",
                BorderHover = "#999999",
                BorderFocus = "#26374A",
                Error = "#D3080C",
                ErrorBackground = "#F3E9E8",
                Success = "#278400",
                SuccessBackground = "#D8EECA",
                Warning = "#EE7100",
                WarningBackground = "#F9F4D4",
                Info = "#269ABC",
                InfoBackground = "#D7FAFF",
                FocusRing = "#FFBF47",
                Selection = "#CFD1D5"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Noto Sans', 'Lato', sans-serif",
                BaseFontSize = "16px",
                LineHeight = "1.5",
                FormTitleSize = "28px",
                SectionTitleSize = "22px",
                QuestionTitleSize = "16px",
                DescriptionSize = "14px"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "30px",
                SectionSpacing = "30px",
                QuestionSpacing = "20px"
            },
            Borders = new ThemeBorders
            {
                RadiusSmall = "4px",
                RadiusMedium = "4px",
                RadiusLarge = "4px",
                FocusRingWidth = "3px",
                FocusRingOffset = "1px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 1px 3px rgba(0, 0, 0, 0.12)"
            }
        },

        ["government-uk"] = new FormTheme
        {
            Id = "government-uk",
            Name = "UK GOV.UK",
            Description = "UK Government Digital Service design system styling",
            BasePreset = "government-uk",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#1D70B8",
                PrimaryHover = "#003078",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#505A5F",
                SecondaryHover = "#383F43",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#F3F2F1",
                Surface = "#FFFFFF",
                SurfaceHover = "#F3F2F1",
                TextPrimary = "#0B0C0C",
                TextSecondary = "#505A5F",
                TextDisabled = "#B1B4B6",
                TextPlaceholder = "#505A5F",
                Border = "#B1B4B6",
                BorderHover = "#0B0C0C",
                BorderFocus = "#0B0C0C",
                Error = "#D4351C",
                ErrorBackground = "#F6D7D2",
                Success = "#00703C",
                SuccessBackground = "#CCE2D8",
                Warning = "#FFDD00",
                WarningBackground = "#FFF7BF",
                Info = "#1D70B8",
                InfoBackground = "#D2E2F1",
                FocusRing = "#FFDD00",
                Selection = "#B4D5FE"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'GDS Transport', Arial, sans-serif",
                BaseFontSize = "19px",
                LineHeight = "1.3",
                FormTitleSize = "32px",
                SectionTitleSize = "24px",
                QuestionTitleSize = "19px",
                DescriptionSize = "16px",
                FontWeightNormal = "400",
                FontWeightBold = "700"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "30px",
                SectionSpacing = "30px",
                QuestionSpacing = "30px",
                LabelSpacing = "5px",
                InputPadding = "10px"
            },
            Borders = new ThemeBorders
            {
                BorderWidth = "2px",
                RadiusSmall = "0px",
                RadiusMedium = "0px",
                RadiusLarge = "0px",
                RadiusXLarge = "0px",
                FocusRingWidth = "3px",
                FocusRingOffset = "0px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "none",
                InputFocusShadow = "inset 0 0 0 2px"
            },
            Accessibility = new ThemeAccessibility
            {
                MinFontSize = "16px",
                ContrastTarget = "aaa"
            }
        },

        // ============================================================
        // DARK THEMES
        // ============================================================

        ["dark"] = new FormTheme
        {
            Id = "dark",
            Name = "Dark",
            Description = "Standard dark theme with slate backgrounds and light text",
            BasePreset = "dark",
            Mode = ThemeMode.Dark,
            Colors = new ThemeColors
            {
                Primary = "#818CF8",
                PrimaryHover = "#6366F1",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#94A3B8",
                SecondaryHover = "#64748B",
                SecondaryForeground = "#0F172A",
                Background = "#0F172A",
                BackgroundDim = "#1E293B",
                Surface = "#1E293B",
                SurfaceHover = "#334155",
                TextPrimary = "#F1F5F9",
                TextSecondary = "#94A3B8",
                TextDisabled = "#475569",
                TextPlaceholder = "#64748B",
                Border = "#334155",
                BorderHover = "#475569",
                BorderFocus = "#818CF8",
                Error = "#F87171",
                ErrorBackground = "#450A0A",
                Success = "#4ADE80",
                SuccessBackground = "#052E16",
                Warning = "#FBBF24",
                WarningBackground = "#451A03",
                Info = "#60A5FA",
                InfoBackground = "#172554",
                FocusRing = "#818CF8",
                Selection = "#312E81"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'DM Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
                BaseFontSize = "14px"
            },
            Borders = new ThemeBorders
            {
                RadiusMedium = "6px",
                RadiusLarge = "8px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 4px 6px rgba(0, 0, 0, 0.3)",
                DropdownShadow = "0 10px 20px rgba(0, 0, 0, 0.4)",
                InputFocusShadow = "0 0 0 3px rgba(129, 140, 248, 0.25)"
            },
            Background = new ThemeBackground
            {
                Type = "color",
                Color = "#0F172A"
            }
        },

        ["dark-modern"] = new FormTheme
        {
            Id = "dark-modern",
            Name = "Dark Modern",
            Description = "Sleek dark theme with purple accents and modern styling",
            BasePreset = "dark-modern",
            Mode = ThemeMode.Dark,
            Colors = new ThemeColors
            {
                Primary = "#A78BFA",
                PrimaryHover = "#8B5CF6",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#A1A1AA",
                SecondaryHover = "#71717A",
                SecondaryForeground = "#18181B",
                Background = "#09090B",
                BackgroundDim = "#18181B",
                Surface = "#18181B",
                SurfaceHover = "#27272A",
                TextPrimary = "#FAFAFA",
                TextSecondary = "#A1A1AA",
                TextDisabled = "#52525B",
                TextPlaceholder = "#71717A",
                Border = "#27272A",
                BorderHover = "#3F3F46",
                BorderFocus = "#A78BFA",
                Error = "#F87171",
                ErrorBackground = "#371717",
                Success = "#4ADE80",
                SuccessBackground = "#14291F",
                Warning = "#FBBF24",
                WarningBackground = "#2D2006",
                Info = "#818CF8",
                InfoBackground = "#1E1B4B",
                FocusRing = "#A78BFA",
                Selection = "#3B0764"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
                BaseFontSize = "15px",
                LineHeight = "1.6",
                FormTitleSize = "28px",
                SectionTitleSize = "20px"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "36px",
                QuestionSpacing = "24px"
            },
            Borders = new ThemeBorders
            {
                RadiusSmall = "6px",
                RadiusMedium = "10px",
                RadiusLarge = "14px",
                RadiusXLarge = "18px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 8px 32px rgba(0, 0, 0, 0.4)",
                InputFocusShadow = "0 0 0 4px rgba(167, 139, 250, 0.2)"
            },
            Background = new ThemeBackground
            {
                Type = "color",
                Color = "#09090B"
            }
        },

        // ============================================================
        // ACCESSIBILITY THEMES
        // ============================================================

        ["high-contrast"] = new FormTheme
        {
            Id = "high-contrast",
            Name = "High Contrast",
            Description = "Maximum contrast theme for accessibility and visual impairments",
            BasePreset = "high-contrast",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#0000EE",
                PrimaryHover = "#0000CC",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#000000",
                SecondaryHover = "#333333",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#FFFFFF",
                Surface = "#FFFFFF",
                SurfaceHover = "#F0F0F0",
                TextPrimary = "#000000",
                TextSecondary = "#000000",
                TextDisabled = "#595959",
                TextPlaceholder = "#595959",
                Border = "#000000",
                BorderHover = "#000000",
                BorderFocus = "#0000EE",
                Error = "#CC0000",
                ErrorBackground = "#FFE6E6",
                Success = "#006600",
                SuccessBackground = "#E6FFE6",
                Warning = "#CC6600",
                WarningBackground = "#FFF5E6",
                Info = "#0000EE",
                InfoBackground = "#E6E6FF",
                FocusRing = "#FF0000",
                Selection = "#FFFF00"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "Arial, Helvetica, sans-serif",
                BaseFontSize = "18px",
                LineHeight = "1.6",
                FormTitleSize = "28px",
                SectionTitleSize = "22px",
                QuestionTitleSize = "18px",
                DescriptionSize = "16px",
                FontWeightNormal = "400",
                FontWeightMedium = "700",
                FontWeightSemibold = "700",
                FontWeightBold = "900"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "32px",
                SectionSpacing = "32px",
                QuestionSpacing = "28px",
                LabelSpacing = "10px",
                OptionSpacing = "12px"
            },
            Borders = new ThemeBorders
            {
                BorderWidth = "2px",
                RadiusSmall = "0px",
                RadiusMedium = "0px",
                RadiusLarge = "0px",
                RadiusXLarge = "0px",
                FocusRingWidth = "3px",
                FocusRingOffset = "2px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 0 0 2px #000000",
                InputFocusShadow = "0 0 0 4px #FF0000"
            },
            Accessibility = new ThemeAccessibility
            {
                ScaleFactor = "110%",
                MinFontSize = "16px",
                HighContrastMode = true,
                ContrastTarget = "aaa",
                FocusIndicatorStyle = "outline",
                AlwaysShowFocusIndicator = true,
                MinTouchTargetSize = "48px"
            }
        },

        // ============================================================
        // INDUSTRY THEMES
        // ============================================================

        ["healthcare"] = new FormTheme
        {
            Id = "healthcare",
            Name = "Healthcare",
            Description = "Professional medical and healthcare theme with teal accents",
            BasePreset = "healthcare",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#0D9488",
                PrimaryHover = "#0F766E",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#0891B2",
                SecondaryHover = "#0E7490",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#F0FDFA",
                Surface = "#FFFFFF",
                SurfaceHover = "#F0FDFA",
                TextPrimary = "#134E4A",
                TextSecondary = "#5EEAD4",
                TextDisabled = "#99F6E4",
                TextPlaceholder = "#5EEAD4",
                Border = "#99F6E4",
                BorderHover = "#5EEAD4",
                BorderFocus = "#0D9488",
                Error = "#DC2626",
                ErrorBackground = "#FEF2F2",
                Success = "#059669",
                SuccessBackground = "#ECFDF5",
                Warning = "#D97706",
                WarningBackground = "#FFFBEB",
                Info = "#0284C7",
                InfoBackground = "#F0F9FF",
                FocusRing = "#0D9488",
                Selection = "#CCFBF1"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Open Sans', -apple-system, BlinkMacSystemFont, sans-serif",
                BaseFontSize = "15px",
                LineHeight = "1.6",
                FormTitleSize = "26px",
                SectionTitleSize = "20px",
                QuestionTitleSize = "15px",
                DescriptionSize = "14px"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "32px",
                QuestionSpacing = "22px"
            },
            Borders = new ThemeBorders
            {
                RadiusMedium = "8px",
                RadiusLarge = "10px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 2px 8px rgba(13, 148, 136, 0.08)",
                InputFocusShadow = "0 0 0 3px rgba(13, 148, 136, 0.15)"
            }
        },

        ["education"] = new FormTheme
        {
            Id = "education",
            Name = "Education",
            Description = "Friendly and approachable theme for schools and universities",
            BasePreset = "education",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#7C3AED",
                PrimaryHover = "#6D28D9",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#2563EB",
                SecondaryHover = "#1D4ED8",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#FAF5FF",
                Surface = "#FFFFFF",
                SurfaceHover = "#F5F3FF",
                TextPrimary = "#1E1B4B",
                TextSecondary = "#6B7280",
                TextDisabled = "#9CA3AF",
                TextPlaceholder = "#9CA3AF",
                Border = "#E5E7EB",
                BorderHover = "#D1D5DB",
                BorderFocus = "#7C3AED",
                Error = "#DC2626",
                ErrorBackground = "#FEF2F2",
                Success = "#059669",
                SuccessBackground = "#ECFDF5",
                Warning = "#D97706",
                WarningBackground = "#FFFBEB",
                Info = "#2563EB",
                InfoBackground = "#EFF6FF",
                FocusRing = "#7C3AED",
                Selection = "#EDE9FE"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Nunito', 'Lato', -apple-system, BlinkMacSystemFont, sans-serif",
                BaseFontSize = "15px",
                LineHeight = "1.6",
                FormTitleSize = "26px",
                SectionTitleSize = "20px",
                QuestionTitleSize = "15px",
                DescriptionSize = "14px",
                FontWeightMedium = "600",
                FontWeightSemibold = "700"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "32px",
                QuestionSpacing = "22px",
                OptionSpacing = "10px"
            },
            Borders = new ThemeBorders
            {
                RadiusMedium = "8px",
                RadiusLarge = "12px",
                RadiusXLarge = "16px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 4px 12px rgba(124, 58, 237, 0.08)",
                InputFocusShadow = "0 0 0 3px rgba(124, 58, 237, 0.15)"
            }
        },

        ["financial"] = new FormTheme
        {
            Id = "financial",
            Name = "Financial",
            Description = "Professional banking and finance theme with green accents",
            BasePreset = "financial",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#047857",
                PrimaryHover = "#065F46",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#0F766E",
                SecondaryHover = "#115E59",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#F8FAFC",
                Surface = "#FFFFFF",
                SurfaceHover = "#F1F5F9",
                TextPrimary = "#064E3B",
                TextSecondary = "#6B7280",
                TextDisabled = "#9CA3AF",
                TextPlaceholder = "#9CA3AF",
                Border = "#D1D5DB",
                BorderHover = "#9CA3AF",
                BorderFocus = "#047857",
                Error = "#DC2626",
                ErrorBackground = "#FEF2F2",
                Success = "#059669",
                SuccessBackground = "#ECFDF5",
                Warning = "#D97706",
                WarningBackground = "#FFFBEB",
                Info = "#0284C7",
                InfoBackground = "#F0F9FF",
                FocusRing = "#047857",
                Selection = "#D1FAE5"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Inter', 'Helvetica Neue', Arial, sans-serif",
                BaseFontSize = "14px",
                LineHeight = "1.5",
                FormTitleSize = "24px",
                SectionTitleSize = "18px",
                QuestionTitleSize = "14px",
                DescriptionSize = "13px"
            },
            Spacing = new ThemeSpacing
            {
                FormPadding = "28px",
                QuestionSpacing = "20px"
            },
            Borders = new ThemeBorders
            {
                RadiusSmall = "4px",
                RadiusMedium = "6px",
                RadiusLarge = "8px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 1px 3px rgba(0, 0, 0, 0.08)",
                InputFocusShadow = "0 0 0 3px rgba(4, 120, 87, 0.12)"
            }
        },

        // ============================================================
        // MINIMAL THEMES
        // ============================================================

        ["borderless"] = new FormTheme
        {
            Id = "borderless",
            Name = "Borderless",
            Description = "Clean, border-free minimalist design",
            BasePreset = "borderless",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#3B82F6",
                PrimaryHover = "#2563EB",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#6B7280",
                SecondaryHover = "#4B5563",
                SecondaryForeground = "#FFFFFF",
                Background = "#FFFFFF",
                BackgroundDim = "#F9FAFB",
                Surface = "#F9FAFB",
                SurfaceHover = "#F3F4F6",
                TextPrimary = "#111827",
                TextSecondary = "#6B7280",
                TextDisabled = "#9CA3AF",
                TextPlaceholder = "#9CA3AF",
                Border = "transparent",
                BorderHover = "#E5E7EB",
                BorderFocus = "#3B82F6",
                Error = "#EF4444",
                ErrorBackground = "#FEF2F2",
                Success = "#10B981",
                SuccessBackground = "#F0FDF4",
                Warning = "#F59E0B",
                WarningBackground = "#FFFBEB",
                Info = "#3B82F6",
                InfoBackground = "#EFF6FF",
                FocusRing = "#3B82F6",
                Selection = "#DBEAFE"
            },
            Borders = new ThemeBorders
            {
                BorderWidth = "0px",
                RadiusMedium = "8px",
                RadiusLarge = "12px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "none",
                InputFocusShadow = "0 0 0 2px rgba(59, 130, 246, 0.5)"
            }
        },

        ["flat"] = new FormTheme
        {
            Id = "flat",
            Name = "Flat",
            Description = "Shadow-free flat design with bold colors",
            BasePreset = "flat",
            Mode = ThemeMode.Light,
            Colors = new ThemeColors
            {
                Primary = "#2563EB",
                PrimaryHover = "#1D4ED8",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#64748B",
                SecondaryHover = "#475569",
                SecondaryForeground = "#FFFFFF",
                Background = "#F8FAFC",
                BackgroundDim = "#E2E8F0",
                Surface = "#FFFFFF",
                SurfaceHover = "#F1F5F9",
                TextPrimary = "#0F172A",
                TextSecondary = "#475569",
                TextDisabled = "#94A3B8",
                TextPlaceholder = "#94A3B8",
                Border = "#CBD5E1",
                BorderHover = "#94A3B8",
                BorderFocus = "#2563EB",
                Error = "#DC2626",
                ErrorBackground = "#FEE2E2",
                Success = "#16A34A",
                SuccessBackground = "#DCFCE7",
                Warning = "#CA8A04",
                WarningBackground = "#FEF9C3",
                Info = "#2563EB",
                InfoBackground = "#DBEAFE",
                FocusRing = "#2563EB",
                Selection = "#BFDBFE"
            },
            Borders = new ThemeBorders
            {
                BorderWidth = "2px",
                RadiusMedium = "4px",
                RadiusLarge = "6px"
            },
            Shadows = new ThemeShadows
            {
                ShadowNone = "none",
                ShadowSmall = "none",
                ShadowMedium = "none",
                ShadowLarge = "none",
                ShadowXLarge = "none",
                CardShadow = "none",
                DropdownShadow = "none",
                ModalShadow = "none",
                InputFocusShadow = "none"
            }
        }
    };

    #endregion

    #region Preset Info Collection

    /// <summary>
    /// Metadata for all available presets, used for display in theme selectors.
    /// </summary>
    public static readonly IReadOnlyList<ThemePresetInfo> PresetInfos = new List<ThemePresetInfo>
    {
        // Professional
        new("default", "Default", "Clean, professional default theme with indigo accents", CategoryProfessional, ThemeMode.Light, "#6366F1", new[] { "professional", "modern", "clean" }),
        new("corporate", "Corporate", "Traditional business styling with sharp corners", CategoryProfessional, ThemeMode.Light, "#1E40AF", new[] { "business", "traditional", "formal" }),
        new("modern", "Modern", "Contemporary minimalist design with purple accents", CategoryProfessional, ThemeMode.Light, "#8B5CF6", new[] { "modern", "minimal", "contemporary" }),

        // Government
        new("government-federal", "U.S. Federal", "U.S. Federal agency styling (USWDS inspired)", CategoryGovernment, ThemeMode.Light, "#005EA2", new[] { "government", "federal", "accessible", "uswds" }),
        new("government-canada", "Government of Canada", "Canada.ca Web Experience Toolkit styling", CategoryGovernment, ThemeMode.Light, "#26374A", new[] { "government", "canada", "wet" }),
        new("government-uk", "UK GOV.UK", "UK Government Digital Service design system", CategoryGovernment, ThemeMode.Light, "#1D70B8", new[] { "government", "uk", "gds", "accessible" }),

        // Dark
        new("dark", "Dark", "Standard dark theme with slate backgrounds", CategoryDark, ThemeMode.Dark, "#818CF8", new[] { "dark", "night", "slate" }),
        new("dark-modern", "Dark Modern", "Sleek dark theme with purple accents", CategoryDark, ThemeMode.Dark, "#A78BFA", new[] { "dark", "modern", "purple" }),

        // Accessibility
        new("high-contrast", "High Contrast", "Maximum contrast for accessibility", CategoryAccessibility, ThemeMode.Light, "#0000EE", new[] { "accessible", "high-contrast", "wcag", "aaa" }),

        // Industry
        new("healthcare", "Healthcare", "Professional medical and healthcare theme", CategoryIndustry, ThemeMode.Light, "#0D9488", new[] { "medical", "health", "teal" }),
        new("education", "Education", "Friendly theme for schools and universities", CategoryIndustry, ThemeMode.Light, "#7C3AED", new[] { "school", "university", "purple" }),
        new("financial", "Financial", "Professional banking and finance theme", CategoryIndustry, ThemeMode.Light, "#047857", new[] { "bank", "finance", "green" }),

        // Minimal
        new("borderless", "Borderless", "Clean, border-free minimalist design", CategoryMinimal, ThemeMode.Light, "#3B82F6", new[] { "minimal", "clean", "borderless" }),
        new("flat", "Flat", "Shadow-free flat design with bold colors", CategoryMinimal, ThemeMode.Light, "#2563EB", new[] { "flat", "minimal", "no-shadow" })
    };

    #endregion

    #region Static Methods

    /// <summary>
    /// Gets all available theme presets.
    /// </summary>
    public static IReadOnlyList<ThemePresetInfo> GetAllPresets() => PresetInfos;

    /// <summary>
    /// Gets all available preset categories.
    /// </summary>
    public static IReadOnlyList<string> GetCategories() => PresetInfos
        .Select(p => p.Category)
        .Distinct()
        .OrderBy(c => c)
        .ToList();

    /// <summary>
    /// Gets presets filtered by category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    public static IReadOnlyList<ThemePresetInfo> GetPresetsByCategory(string category) => PresetInfos
        .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
        .ToList();

    /// <summary>
    /// Gets presets filtered by theme mode.
    /// </summary>
    /// <param name="mode">The theme mode to filter by.</param>
    public static IReadOnlyList<ThemePresetInfo> GetPresetsByMode(ThemeMode mode) => PresetInfos
        .Where(p => p.Mode == mode)
        .ToList();

    /// <summary>
    /// Gets presets that match any of the specified tags.
    /// </summary>
    /// <param name="tags">Tags to search for.</param>
    public static IReadOnlyList<ThemePresetInfo> GetPresetsByTags(params string[] tags) => PresetInfos
        .Where(p => p.Tags != null && p.Tags.Any(t => tags.Contains(t, StringComparer.OrdinalIgnoreCase)))
        .ToList();

    /// <summary>
    /// Gets a specific theme preset by ID.
    /// </summary>
    /// <param name="presetId">The preset ID.</param>
    /// <returns>The FormTheme preset.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if preset doesn't exist.</exception>
    public static FormTheme GetPreset(string presetId)
    {
        if (Presets.TryGetValue(presetId, out var preset))
        {
            return preset.Clone(); // Return a clone to prevent modification of the original
        }

        throw new KeyNotFoundException($"Theme preset '{presetId}' not found.");
    }

    /// <summary>
    /// Tries to get a specific theme preset by ID.
    /// </summary>
    /// <param name="presetId">The preset ID.</param>
    /// <param name="preset">The found preset, or null if not found.</param>
    /// <returns>True if found, false otherwise.</returns>
    public static bool TryGetPreset(string presetId, out FormTheme? preset)
    {
        if (Presets.TryGetValue(presetId, out var found))
        {
            preset = found.Clone();
            return true;
        }

        preset = null;
        return false;
    }

    /// <summary>
    /// Gets preset info by ID.
    /// </summary>
    /// <param name="presetId">The preset ID.</param>
    /// <returns>The preset info, or null if not found.</returns>
    public static ThemePresetInfo? GetPresetInfo(string presetId) =>
        PresetInfos.FirstOrDefault(p => p.Id.Equals(presetId, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if a preset exists.
    /// </summary>
    /// <param name="presetId">The preset ID to check.</param>
    public static bool PresetExists(string presetId) =>
        Presets.ContainsKey(presetId);

    /// <summary>
    /// Searches presets by name or description.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    public static IReadOnlyList<ThemePresetInfo> SearchPresets(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return PresetInfos;

        var term = searchTerm.ToLowerInvariant();
        return PresetInfos
            .Where(p =>
                p.Name.ToLowerInvariant().Contains(term) ||
                p.Description.ToLowerInvariant().Contains(term) ||
                (p.Tags?.Any(t => t.ToLowerInvariant().Contains(term)) ?? false))
            .ToList();
    }

    #endregion
}
