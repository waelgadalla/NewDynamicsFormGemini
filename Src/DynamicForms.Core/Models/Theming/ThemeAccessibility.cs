using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Accessibility settings for form theming.
/// Supports WCAG 2.1 compliance and user preference accommodations.
/// </summary>
public class ThemeAccessibility
{
    #region Scale Settings

    /// <summary>
    /// Global scale factor for all UI elements (e.g., "100%", "125%", "150%").
    /// Affects font sizes, spacing, and touch targets proportionally.
    /// Range: 80% to 150%
    /// </summary>
    [JsonPropertyName("scaleFactor")]
    public string ScaleFactor { get; set; } = "100%";

    /// <summary>
    /// Minimum font size allowed regardless of scale (e.g., "12px", "14px").
    /// Ensures text never becomes too small to read.
    /// </summary>
    [JsonPropertyName("minFontSize")]
    public string MinFontSize { get; set; } = "12px";

    #endregion

    #region Contrast Settings

    /// <summary>
    /// Whether high contrast mode is enabled.
    /// Increases contrast ratios for better visibility.
    /// </summary>
    [JsonPropertyName("highContrastMode")]
    public bool HighContrastMode { get; set; } = false;

    /// <summary>
    /// Minimum contrast ratio target: "aa" (4.5:1) or "aaa" (7:1).
    /// Used for validation and warnings in the theme editor.
    /// </summary>
    [JsonPropertyName("contrastTarget")]
    public string ContrastTarget { get; set; } = "aa";

    #endregion

    #region Motion Settings

    /// <summary>
    /// Whether to reduce or disable animations and transitions.
    /// Respects prefers-reduced-motion user preference when true.
    /// </summary>
    [JsonPropertyName("reduceMotion")]
    public bool ReduceMotion { get; set; } = false;

    /// <summary>
    /// Default transition duration when animations are enabled (e.g., "150ms").
    /// </summary>
    [JsonPropertyName("transitionDuration")]
    public string TransitionDuration { get; set; } = "150ms";

    #endregion

    #region Focus Indicators

    /// <summary>
    /// Style of focus indicator: "ring", "outline", "underline", or "none".
    /// </summary>
    [JsonPropertyName("focusIndicatorStyle")]
    public string FocusIndicatorStyle { get; set; } = "ring";

    /// <summary>
    /// Whether to always show focus indicators, not just for keyboard users.
    /// </summary>
    [JsonPropertyName("alwaysShowFocusIndicator")]
    public bool AlwaysShowFocusIndicator { get; set; } = false;

    #endregion

    #region Touch & Interaction

    /// <summary>
    /// Minimum touch target size for interactive elements (e.g., "44px").
    /// WCAG recommends 44x44 pixels minimum.
    /// </summary>
    [JsonPropertyName("minTouchTargetSize")]
    public string MinTouchTargetSize { get; set; } = "44px";

    /// <summary>
    /// Extra spacing between touch targets on mobile.
    /// </summary>
    [JsonPropertyName("touchTargetSpacing")]
    public string TouchTargetSpacing { get; set; } = "8px";

    #endregion

    #region Text Settings

    /// <summary>
    /// Maximum line length for optimal readability (characters).
    /// Recommended range: 45-75 characters.
    /// </summary>
    [JsonPropertyName("maxLineLength")]
    public int MaxLineLength { get; set; } = 65;

    /// <summary>
    /// Whether to allow user font size preferences to override theme settings.
    /// </summary>
    [JsonPropertyName("respectUserFontSize")]
    public bool RespectUserFontSize { get; set; } = true;

    #endregion

    #region Screen Reader Support

    /// <summary>
    /// Whether to include additional ARIA labels for enhanced screen reader support.
    /// </summary>
    [JsonPropertyName("enhancedAriaLabels")]
    public bool EnhancedAriaLabels { get; set; } = true;

    /// <summary>
    /// Whether to announce form validation errors to screen readers.
    /// </summary>
    [JsonPropertyName("announceErrors")]
    public bool AnnounceErrors { get; set; } = true;

    #endregion

    #region Methods

    /// <summary>
    /// Gets the numeric scale factor value (e.g., 1.0 for "100%", 1.25 for "125%").
    /// </summary>
    [JsonIgnore]
    public double ScaleFactorNumeric
    {
        get
        {
            var value = ScaleFactor.TrimEnd('%');
            return double.TryParse(value, out var result) ? result / 100.0 : 1.0;
        }
    }

    /// <summary>
    /// Creates a deep clone of this accessibility configuration.
    /// </summary>
    public ThemeAccessibility Clone()
    {
        return new ThemeAccessibility
        {
            ScaleFactor = ScaleFactor,
            MinFontSize = MinFontSize,
            HighContrastMode = HighContrastMode,
            ContrastTarget = ContrastTarget,
            ReduceMotion = ReduceMotion,
            TransitionDuration = TransitionDuration,
            FocusIndicatorStyle = FocusIndicatorStyle,
            AlwaysShowFocusIndicator = AlwaysShowFocusIndicator,
            MinTouchTargetSize = MinTouchTargetSize,
            TouchTargetSpacing = TouchTargetSpacing,
            MaxLineLength = MaxLineLength,
            RespectUserFontSize = RespectUserFontSize,
            EnhancedAriaLabels = EnhancedAriaLabels,
            AnnounceErrors = AnnounceErrors
        };
    }

    #endregion
}
