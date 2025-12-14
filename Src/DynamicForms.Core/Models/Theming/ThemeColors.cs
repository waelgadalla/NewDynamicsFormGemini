using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Complete color palette for form theming.
/// All colors should be specified in hex format (#RRGGBB) or rgba() format.
/// </summary>
public class ThemeColors
{
    #region Primary Brand Colors

    /// <summary>
    /// Main brand/accent color used for buttons, links, and focus states.
    /// </summary>
    [JsonPropertyName("primary")]
    public string Primary { get; set; } = "#6366F1";

    /// <summary>
    /// Hover state for primary color elements.
    /// </summary>
    [JsonPropertyName("primaryHover")]
    public string PrimaryHover { get; set; } = "#4F46E5";

    /// <summary>
    /// Text color on primary-colored backgrounds.
    /// </summary>
    [JsonPropertyName("primaryForeground")]
    public string PrimaryForeground { get; set; } = "#FFFFFF";

    #endregion

    #region Secondary Colors

    /// <summary>
    /// Secondary brand color for less prominent actions.
    /// </summary>
    [JsonPropertyName("secondary")]
    public string Secondary { get; set; } = "#64748B";

    /// <summary>
    /// Hover state for secondary color elements.
    /// </summary>
    [JsonPropertyName("secondaryHover")]
    public string SecondaryHover { get; set; } = "#475569";

    /// <summary>
    /// Text color on secondary-colored backgrounds.
    /// </summary>
    [JsonPropertyName("secondaryForeground")]
    public string SecondaryForeground { get; set; } = "#FFFFFF";

    #endregion

    #region Surface Colors

    /// <summary>
    /// Main form/page background color.
    /// </summary>
    [JsonPropertyName("background")]
    public string Background { get; set; } = "#FFFFFF";

    /// <summary>
    /// Secondary/dimmed background for contrast areas.
    /// </summary>
    [JsonPropertyName("backgroundDim")]
    public string BackgroundDim { get; set; } = "#F8FAFC";

    /// <summary>
    /// Surface color for cards and elevated elements.
    /// </summary>
    [JsonPropertyName("surface")]
    public string Surface { get; set; } = "#FFFFFF";

    /// <summary>
    /// Hover state for surface elements.
    /// </summary>
    [JsonPropertyName("surfaceHover")]
    public string SurfaceHover { get; set; } = "#F1F5F9";

    #endregion

    #region Text Colors

    /// <summary>
    /// Primary text color for headings and important content.
    /// </summary>
    [JsonPropertyName("textPrimary")]
    public string TextPrimary { get; set; } = "#0F172A";

    /// <summary>
    /// Secondary text color for descriptions and less prominent content.
    /// </summary>
    [JsonPropertyName("textSecondary")]
    public string TextSecondary { get; set; } = "#64748B";

    /// <summary>
    /// Disabled text color for inactive elements.
    /// </summary>
    [JsonPropertyName("textDisabled")]
    public string TextDisabled { get; set; } = "#94A3B8";

    /// <summary>
    /// Placeholder text color for input fields.
    /// </summary>
    [JsonPropertyName("textPlaceholder")]
    public string TextPlaceholder { get; set; } = "#94A3B8";

    #endregion

    #region Border Colors

    /// <summary>
    /// Default border color for inputs and containers.
    /// </summary>
    [JsonPropertyName("border")]
    public string Border { get; set; } = "#E2E8F0";

    /// <summary>
    /// Border color on hover state.
    /// </summary>
    [JsonPropertyName("borderHover")]
    public string BorderHover { get; set; } = "#CBD5E1";

    /// <summary>
    /// Border color when element is focused.
    /// </summary>
    [JsonPropertyName("borderFocus")]
    public string BorderFocus { get; set; } = "#6366F1";

    #endregion

    #region State Colors - Error

    /// <summary>
    /// Error/danger state color for validation errors and destructive actions.
    /// </summary>
    [JsonPropertyName("error")]
    public string Error { get; set; } = "#EF4444";

    /// <summary>
    /// Background color for error message containers.
    /// </summary>
    [JsonPropertyName("errorBackground")]
    public string ErrorBackground { get; set; } = "#FEF2F2";

    #endregion

    #region State Colors - Success

    /// <summary>
    /// Success state color for confirmations and positive actions.
    /// </summary>
    [JsonPropertyName("success")]
    public string Success { get; set; } = "#10B981";

    /// <summary>
    /// Background color for success message containers.
    /// </summary>
    [JsonPropertyName("successBackground")]
    public string SuccessBackground { get; set; } = "#F0FDF4";

    #endregion

    #region State Colors - Warning

    /// <summary>
    /// Warning state color for cautions and alerts.
    /// </summary>
    [JsonPropertyName("warning")]
    public string Warning { get; set; } = "#F59E0B";

    /// <summary>
    /// Background color for warning message containers.
    /// </summary>
    [JsonPropertyName("warningBackground")]
    public string WarningBackground { get; set; } = "#FFFBEB";

    #endregion

    #region State Colors - Info

    /// <summary>
    /// Info state color for informational messages and hints.
    /// </summary>
    [JsonPropertyName("info")]
    public string Info { get; set; } = "#3B82F6";

    /// <summary>
    /// Background color for info message containers.
    /// </summary>
    [JsonPropertyName("infoBackground")]
    public string InfoBackground { get; set; } = "#EFF6FF";

    #endregion

    #region Interactive States

    /// <summary>
    /// Color for focus ring/outline around focused elements.
    /// </summary>
    [JsonPropertyName("focusRing")]
    public string FocusRing { get; set; } = "#6366F1";

    /// <summary>
    /// Background color for selected items in lists or dropdowns.
    /// </summary>
    [JsonPropertyName("selection")]
    public string Selection { get; set; } = "#DBEAFE";

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep clone of this color configuration.
    /// </summary>
    public ThemeColors Clone()
    {
        return new ThemeColors
        {
            Primary = Primary,
            PrimaryHover = PrimaryHover,
            PrimaryForeground = PrimaryForeground,
            Secondary = Secondary,
            SecondaryHover = SecondaryHover,
            SecondaryForeground = SecondaryForeground,
            Background = Background,
            BackgroundDim = BackgroundDim,
            Surface = Surface,
            SurfaceHover = SurfaceHover,
            TextPrimary = TextPrimary,
            TextSecondary = TextSecondary,
            TextDisabled = TextDisabled,
            TextPlaceholder = TextPlaceholder,
            Border = Border,
            BorderHover = BorderHover,
            BorderFocus = BorderFocus,
            Error = Error,
            ErrorBackground = ErrorBackground,
            Success = Success,
            SuccessBackground = SuccessBackground,
            Warning = Warning,
            WarningBackground = WarningBackground,
            Info = Info,
            InfoBackground = InfoBackground,
            FocusRing = FocusRing,
            Selection = Selection
        };
    }

    #endregion
}
