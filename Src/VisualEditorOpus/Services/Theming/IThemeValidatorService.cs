namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Service for validating theme accessibility compliance.
/// Implements WCAG 2.1 contrast ratio calculations and compliance checking.
/// </summary>
public interface IThemeValidatorService
{
    /// <summary>
    /// Calculates the contrast ratio between two colors.
    /// </summary>
    /// <param name="foreground">Foreground color in hex format (#RRGGBB or #RGB)</param>
    /// <param name="background">Background color in hex format (#RRGGBB or #RGB)</param>
    /// <returns>Contrast ratio from 1:1 to 21:1</returns>
    double CalculateContrastRatio(string foreground, string background);

    /// <summary>
    /// Checks the contrast between two colors and returns detailed compliance info.
    /// </summary>
    /// <param name="foreground">Foreground color in hex format</param>
    /// <param name="background">Background color in hex format</param>
    /// <returns>Contrast check result with compliance levels</returns>
    ContrastCheckResult CheckContrast(string foreground, string background);

    /// <summary>
    /// Checks if the contrast ratio meets WCAG AA requirements for normal text (4.5:1).
    /// </summary>
    bool IsWcagAACompliant(string foreground, string background);

    /// <summary>
    /// Checks if the contrast ratio meets WCAG AA requirements for large text (3:1).
    /// </summary>
    bool IsWcagAALargeTextCompliant(string foreground, string background);

    /// <summary>
    /// Checks if the contrast ratio meets WCAG AAA requirements for normal text (7:1).
    /// </summary>
    bool IsWcagAAACompliant(string foreground, string background);

    /// <summary>
    /// Checks if the contrast ratio meets WCAG AAA requirements for large text (4.5:1).
    /// </summary>
    bool IsWcagAAALargeTextCompliant(string foreground, string background);

    /// <summary>
    /// Validates all color combinations in a theme and returns issues.
    /// </summary>
    /// <param name="theme">The theme to validate</param>
    /// <returns>List of accessibility issues found</returns>
    List<AccessibilityIssue> ValidateTheme(DynamicForms.Models.Theming.FormTheme theme);
}

/// <summary>
/// Result of a contrast check between two colors.
/// </summary>
public record ContrastCheckResult
{
    /// <summary>
    /// The calculated contrast ratio (1:1 to 21:1).
    /// </summary>
    public double Ratio { get; init; }

    /// <summary>
    /// Formatted ratio string (e.g., "4.5:1").
    /// </summary>
    public string RatioDisplay => $"{Ratio:F2}:1";

    /// <summary>
    /// Whether the ratio passes WCAG AA for normal text (4.5:1).
    /// </summary>
    public bool PassesAA { get; init; }

    /// <summary>
    /// Whether the ratio passes WCAG AA for large text (3:1).
    /// </summary>
    public bool PassesAALarge { get; init; }

    /// <summary>
    /// Whether the ratio passes WCAG AAA for normal text (7:1).
    /// </summary>
    public bool PassesAAA { get; init; }

    /// <summary>
    /// Whether the ratio passes WCAG AAA for large text (4.5:1).
    /// </summary>
    public bool PassesAAALarge { get; init; }

    /// <summary>
    /// The overall compliance level achieved.
    /// </summary>
    public WcagLevel Level { get; init; }
}

/// <summary>
/// WCAG compliance levels.
/// </summary>
public enum WcagLevel
{
    /// <summary>Does not meet minimum requirements.</summary>
    Fail,
    /// <summary>Passes AA for large text only (3:1).</summary>
    AALarge,
    /// <summary>Passes AA for all text (4.5:1).</summary>
    AA,
    /// <summary>Passes AAA for large text (4.5:1).</summary>
    AAALarge,
    /// <summary>Passes AAA for all text (7:1).</summary>
    AAA
}

/// <summary>
/// Represents an accessibility issue found during theme validation.
/// </summary>
public record AccessibilityIssue
{
    /// <summary>
    /// The severity of the issue.
    /// </summary>
    public AccessibilityIssueSeverity Severity { get; init; }

    /// <summary>
    /// The name of the property with the issue.
    /// </summary>
    public string PropertyName { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable description of the issue.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The foreground color being checked.
    /// </summary>
    public string? ForegroundColor { get; init; }

    /// <summary>
    /// The background color being checked.
    /// </summary>
    public string? BackgroundColor { get; init; }

    /// <summary>
    /// The current contrast ratio (if applicable).
    /// </summary>
    public double? ContrastRatio { get; init; }

    /// <summary>
    /// Suggested fix for the issue.
    /// </summary>
    public string? Suggestion { get; init; }
}

/// <summary>
/// Severity levels for accessibility issues.
/// </summary>
public enum AccessibilityIssueSeverity
{
    /// <summary>Informational, not a compliance issue.</summary>
    Info,
    /// <summary>Warning - may cause accessibility problems.</summary>
    Warning,
    /// <summary>Error - fails WCAG compliance.</summary>
    Error
}
