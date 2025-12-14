using DynamicForms.Models.Theming;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Service for validating theme accessibility compliance using WCAG 2.1 guidelines.
/// </summary>
public class ThemeValidatorService : IThemeValidatorService
{
    // WCAG 2.1 contrast ratio requirements
    private const double AANormalText = 4.5;     // AA for normal text
    private const double AALargeText = 3.0;      // AA for large text (14pt bold or 18pt)
    private const double AAANormalText = 7.0;    // AAA for normal text
    private const double AAALargeText = 4.5;     // AAA for large text

    /// <inheritdoc />
    public double CalculateContrastRatio(string foreground, string background)
    {
        var fgLuminance = GetRelativeLuminance(foreground);
        var bgLuminance = GetRelativeLuminance(background);

        var lighter = Math.Max(fgLuminance, bgLuminance);
        var darker = Math.Min(fgLuminance, bgLuminance);

        return (lighter + 0.05) / (darker + 0.05);
    }

    /// <inheritdoc />
    public ContrastCheckResult CheckContrast(string foreground, string background)
    {
        var ratio = CalculateContrastRatio(foreground, background);

        var passesAA = ratio >= AANormalText;
        var passesAALarge = ratio >= AALargeText;
        var passesAAA = ratio >= AAANormalText;
        var passesAAALarge = ratio >= AAALargeText;

        var level = WcagLevel.Fail;
        if (passesAAA) level = WcagLevel.AAA;
        else if (passesAAALarge && passesAA) level = WcagLevel.AAALarge;
        else if (passesAA) level = WcagLevel.AA;
        else if (passesAALarge) level = WcagLevel.AALarge;

        return new ContrastCheckResult
        {
            Ratio = ratio,
            PassesAA = passesAA,
            PassesAALarge = passesAALarge,
            PassesAAA = passesAAA,
            PassesAAALarge = passesAAALarge,
            Level = level
        };
    }

    /// <inheritdoc />
    public bool IsWcagAACompliant(string foreground, string background)
    {
        return CalculateContrastRatio(foreground, background) >= AANormalText;
    }

    /// <inheritdoc />
    public bool IsWcagAALargeTextCompliant(string foreground, string background)
    {
        return CalculateContrastRatio(foreground, background) >= AALargeText;
    }

    /// <inheritdoc />
    public bool IsWcagAAACompliant(string foreground, string background)
    {
        return CalculateContrastRatio(foreground, background) >= AAANormalText;
    }

    /// <inheritdoc />
    public bool IsWcagAAALargeTextCompliant(string foreground, string background)
    {
        return CalculateContrastRatio(foreground, background) >= AAALargeText;
    }

    /// <inheritdoc />
    public List<AccessibilityIssue> ValidateTheme(FormTheme theme)
    {
        var issues = new List<AccessibilityIssue>();

        if (theme.Colors is null) return issues;

        var colors = theme.Colors;

        // Check primary text on background
        CheckColorPair(issues, "Primary Text", colors.TextPrimary, colors.Background,
            "Primary text on main background");

        // Check secondary text on background
        CheckColorPair(issues, "Secondary Text", colors.TextSecondary, colors.Background,
            "Secondary text on main background", AccessibilityIssueSeverity.Warning);

        // Check primary button text on primary button
        CheckColorPair(issues, "Primary Button", colors.Primary, colors.Background,
            "Primary color on main background", AccessibilityIssueSeverity.Info);

        // Check text on surface
        CheckColorPair(issues, "Text on Surface", colors.TextPrimary, colors.Surface,
            "Primary text on surface background");

        // Check placeholder text
        CheckColorPair(issues, "Placeholder Text", colors.TextPlaceholder, colors.Background,
            "Placeholder text on background", AccessibilityIssueSeverity.Warning);

        // Check error text
        CheckColorPair(issues, "Error Text", colors.Error, colors.Background,
            "Error text on background");

        // Check success text
        CheckColorPair(issues, "Success Text", colors.Success, colors.Background,
            "Success text on background");

        // Check warning text
        CheckColorPair(issues, "Warning Text", colors.Warning, colors.Background,
            "Warning text on background");

        // Check border visibility
        var borderContrast = CalculateContrastRatio(colors.Border, colors.Background);
        if (borderContrast < 1.5)
        {
            issues.Add(new AccessibilityIssue
            {
                Severity = AccessibilityIssueSeverity.Warning,
                PropertyName = "Border",
                Description = "Border may not be visible against background",
                ForegroundColor = colors.Border,
                BackgroundColor = colors.Background,
                ContrastRatio = borderContrast,
                Suggestion = "Increase border contrast for better visibility"
            });
        }

        return issues;
    }

    /// <summary>
    /// Calculates the relative luminance of a color using the WCAG formula.
    /// L = 0.2126 * R + 0.7152 * G + 0.0722 * B
    /// </summary>
    private static double GetRelativeLuminance(string hexColor)
    {
        var (r, g, b) = ParseHexColor(hexColor);

        var rSrgb = r / 255.0;
        var gSrgb = g / 255.0;
        var bSrgb = b / 255.0;

        var rLinear = rSrgb <= 0.03928 ? rSrgb / 12.92 : Math.Pow((rSrgb + 0.055) / 1.055, 2.4);
        var gLinear = gSrgb <= 0.03928 ? gSrgb / 12.92 : Math.Pow((gSrgb + 0.055) / 1.055, 2.4);
        var bLinear = bSrgb <= 0.03928 ? bSrgb / 12.92 : Math.Pow((bSrgb + 0.055) / 1.055, 2.4);

        return 0.2126 * rLinear + 0.7152 * gLinear + 0.0722 * bLinear;
    }

    /// <summary>
    /// Parses a hex color string into RGB components.
    /// </summary>
    private static (int R, int G, int B) ParseHexColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor))
            return (0, 0, 0);

        var hex = hexColor.TrimStart('#');

        // Handle shorthand (#RGB) format
        if (hex.Length == 3)
        {
            hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
        }

        // Handle #RRGGBBAA format by taking first 6 chars
        if (hex.Length >= 6)
        {
            hex = hex[..6];
        }

        try
        {
            var r = Convert.ToInt32(hex[..2], 16);
            var g = Convert.ToInt32(hex[2..4], 16);
            var b = Convert.ToInt32(hex[4..6], 16);
            return (r, g, b);
        }
        catch
        {
            return (0, 0, 0);
        }
    }

    private void CheckColorPair(
        List<AccessibilityIssue> issues,
        string propertyName,
        string foreground,
        string background,
        string description,
        AccessibilityIssueSeverity minSeverity = AccessibilityIssueSeverity.Error)
    {
        if (string.IsNullOrEmpty(foreground) || string.IsNullOrEmpty(background))
            return;

        var result = CheckContrast(foreground, background);

        if (result.Level == WcagLevel.Fail)
        {
            issues.Add(new AccessibilityIssue
            {
                Severity = minSeverity,
                PropertyName = propertyName,
                Description = $"{description} fails WCAG contrast requirements",
                ForegroundColor = foreground,
                BackgroundColor = background,
                ContrastRatio = result.Ratio,
                Suggestion = $"Increase contrast ratio to at least {AALargeText}:1 for large text or {AANormalText}:1 for normal text"
            });
        }
        else if (result.Level == WcagLevel.AALarge && minSeverity <= AccessibilityIssueSeverity.Warning)
        {
            issues.Add(new AccessibilityIssue
            {
                Severity = AccessibilityIssueSeverity.Warning,
                PropertyName = propertyName,
                Description = $"{description} only passes AA for large text",
                ForegroundColor = foreground,
                BackgroundColor = background,
                ContrastRatio = result.Ratio,
                Suggestion = $"Consider increasing contrast to {AANormalText}:1 for full AA compliance"
            });
        }
    }
}
