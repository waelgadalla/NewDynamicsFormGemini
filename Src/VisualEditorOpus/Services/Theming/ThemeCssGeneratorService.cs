using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DynamicForms.Models.Theming;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Implementation of IThemeCssGeneratorService that generates CSS from FormTheme configurations.
/// Includes caching for performance optimization.
/// </summary>
public partial class ThemeCssGeneratorService : IThemeCssGeneratorService
{
    #region Cache

    // Cache for generated CSS variables (keyed by theme hash)
    private readonly ConcurrentDictionary<string, string> _variablesCache = new();

    // Cache for generated stylesheets (keyed by theme hash)
    private readonly ConcurrentDictionary<string, string> _stylesheetCache = new();

    // Cache for minified CSS (keyed by theme hash)
    private readonly ConcurrentDictionary<string, string> _minifiedCache = new();

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public string GenerateCssVariables(FormTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var cacheKey = ComputeThemeHash(theme);

        return _variablesCache.GetOrAdd(cacheKey, _ => GenerateCssVariablesInternal(theme));
    }

    /// <inheritdoc />
    public string GenerateStylesheet(FormTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var cacheKey = ComputeThemeHash(theme);

        return _stylesheetCache.GetOrAdd(cacheKey, _ => GenerateStylesheetInternal(theme));
    }

    /// <inheritdoc />
    public string GenerateMinifiedCss(FormTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var cacheKey = ComputeThemeHash(theme);

        return _minifiedCache.GetOrAdd(cacheKey, _ =>
        {
            var stylesheet = GenerateStylesheet(theme);
            return MinifyCss(stylesheet);
        });
    }

    /// <inheritdoc />
    public string GetVariable(FormTheme theme, string variableName)
    {
        ArgumentNullException.ThrowIfNull(theme);

        if (string.IsNullOrWhiteSpace(variableName))
            return string.Empty;

        var variables = GetAllVariables(theme);
        return variables.TryGetValue(variableName, out var value) ? value : string.Empty;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> GetAllVariables(FormTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var variables = new Dictionary<string, string>();

        // Colors
        AddColorVariables(variables, theme.Colors);

        // Typography
        AddTypographyVariables(variables, theme.Typography);

        // Spacing
        AddSpacingVariables(variables, theme.Spacing);

        // Borders
        AddBorderVariables(variables, theme.Borders);

        // Shadows
        AddShadowVariables(variables, theme.Shadows);

        // Accessibility
        AddAccessibilityVariables(variables, theme.Accessibility);

        return variables;
    }

    /// <inheritdoc />
    public void ClearCache()
    {
        _variablesCache.Clear();
        _stylesheetCache.Clear();
        _minifiedCache.Clear();
    }

    #endregion

    #region Variable Generation

    private string GenerateCssVariablesInternal(FormTheme theme)
    {
        var variables = GetAllVariables(theme);
        var sb = new StringBuilder();

        foreach (var (name, value) in variables)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.Append($"{name}: {value}; ");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static void AddColorVariables(Dictionary<string, string> vars, ThemeColors colors)
    {
        // Primary colors
        AddVar(vars, "--df-primary", colors.Primary);
        AddVar(vars, "--df-primary-hover", colors.PrimaryHover);
        AddVar(vars, "--df-primary-fg", colors.PrimaryForeground);

        // Secondary colors
        AddVar(vars, "--df-secondary", colors.Secondary);
        AddVar(vars, "--df-secondary-hover", colors.SecondaryHover);
        AddVar(vars, "--df-secondary-fg", colors.SecondaryForeground);

        // Background colors
        AddVar(vars, "--df-bg", colors.Background);
        AddVar(vars, "--df-bg-dim", colors.BackgroundDim);
        AddVar(vars, "--df-surface", colors.Surface);
        AddVar(vars, "--df-surface-hover", colors.SurfaceHover);

        // Text colors
        AddVar(vars, "--df-text", colors.TextPrimary);
        AddVar(vars, "--df-text-secondary", colors.TextSecondary);
        AddVar(vars, "--df-text-disabled", colors.TextDisabled);
        AddVar(vars, "--df-text-placeholder", colors.TextPlaceholder);

        // Border colors
        AddVar(vars, "--df-border", colors.Border);
        AddVar(vars, "--df-border-hover", colors.BorderHover);
        AddVar(vars, "--df-border-focus", colors.BorderFocus);

        // State colors - Error
        AddVar(vars, "--df-error", colors.Error);
        AddVar(vars, "--df-error-bg", colors.ErrorBackground);

        // State colors - Success
        AddVar(vars, "--df-success", colors.Success);
        AddVar(vars, "--df-success-bg", colors.SuccessBackground);

        // State colors - Warning
        AddVar(vars, "--df-warning", colors.Warning);
        AddVar(vars, "--df-warning-bg", colors.WarningBackground);

        // State colors - Info
        AddVar(vars, "--df-info", colors.Info);
        AddVar(vars, "--df-info-bg", colors.InfoBackground);

        // Interactive
        AddVar(vars, "--df-focus-ring", colors.FocusRing);
        AddVar(vars, "--df-selection", colors.Selection);
    }

    private static void AddTypographyVariables(Dictionary<string, string> vars, ThemeTypography typography)
    {
        // Font families
        AddVar(vars, "--df-font", typography.FontFamily);
        AddVar(vars, "--df-font-heading", typography.EffectiveHeadingFontFamily);
        AddVar(vars, "--df-font-mono", typography.MonoFontFamily);

        // Base sizes
        AddVar(vars, "--df-text-base", typography.BaseFontSize);
        AddVar(vars, "--df-line-height", typography.LineHeight);

        // Heading sizes
        AddVar(vars, "--df-text-title", typography.FormTitleSize);
        AddVar(vars, "--df-text-section", typography.SectionTitleSize);
        AddVar(vars, "--df-text-question", typography.QuestionTitleSize);
        AddVar(vars, "--df-text-desc", typography.DescriptionSize);

        // Font weights
        AddVar(vars, "--df-font-normal", typography.FontWeightNormal);
        AddVar(vars, "--df-font-medium", typography.FontWeightMedium);
        AddVar(vars, "--df-font-semibold", typography.FontWeightSemibold);
        AddVar(vars, "--df-font-bold", typography.FontWeightBold);
    }

    private static void AddSpacingVariables(Dictionary<string, string> vars, ThemeSpacing spacing)
    {
        AddVar(vars, "--df-unit", spacing.BaseUnit);
        AddVar(vars, "--df-form-padding", spacing.FormPadding);
        AddVar(vars, "--df-section-gap", spacing.SectionSpacing);
        AddVar(vars, "--df-question-gap", spacing.QuestionSpacing);
        AddVar(vars, "--df-label-gap", spacing.LabelSpacing);
        AddVar(vars, "--df-option-gap", spacing.OptionSpacing);
        AddVar(vars, "--df-input-padding", spacing.InputPadding);
        AddVar(vars, "--df-input-padding-x", spacing.InputPaddingHorizontal);
        AddVar(vars, "--df-btn-padding-y", spacing.ButtonPaddingVertical);
        AddVar(vars, "--df-btn-padding-x", spacing.ButtonPaddingHorizontal);
        AddVar(vars, "--df-btn-gap", spacing.ButtonGap);
    }

    private static void AddBorderVariables(Dictionary<string, string> vars, ThemeBorders borders)
    {
        // Border properties
        AddVar(vars, "--df-border-width", borders.BorderWidth);
        AddVar(vars, "--df-border-style", borders.BorderStyle);

        // Border radius
        AddVar(vars, "--df-radius-sm", borders.RadiusSmall);
        AddVar(vars, "--df-radius-md", borders.RadiusMedium);
        AddVar(vars, "--df-radius-lg", borders.RadiusLarge);
        AddVar(vars, "--df-radius-xl", borders.RadiusXLarge);
        AddVar(vars, "--df-radius-full", borders.RadiusFull);

        // Focus ring
        AddVar(vars, "--df-focus-width", borders.FocusRingWidth);
        AddVar(vars, "--df-focus-offset", borders.FocusRingOffset);
        AddVar(vars, "--df-focus-style", borders.FocusRingStyle);
    }

    private static void AddShadowVariables(Dictionary<string, string> vars, ThemeShadows shadows)
    {
        // Shadow scale
        AddVar(vars, "--df-shadow-none", shadows.ShadowNone);
        AddVar(vars, "--df-shadow-xs", shadows.ShadowXSmall);
        AddVar(vars, "--df-shadow-sm", shadows.ShadowSmall);
        AddVar(vars, "--df-shadow-md", shadows.ShadowMedium);
        AddVar(vars, "--df-shadow-lg", shadows.ShadowLarge);
        AddVar(vars, "--df-shadow-xl", shadows.ShadowXLarge);
        AddVar(vars, "--df-shadow-2xl", shadows.Shadow2XLarge);

        // Component shadows
        AddVar(vars, "--df-shadow-card", shadows.CardShadow);
        AddVar(vars, "--df-shadow-dropdown", shadows.DropdownShadow);
        AddVar(vars, "--df-shadow-modal", shadows.ModalShadow);
        AddVar(vars, "--df-shadow-focus", shadows.InputFocusShadow);
        AddVar(vars, "--df-shadow-btn-hover", shadows.ButtonHoverShadow);
        AddVar(vars, "--df-shadow-inner", shadows.InnerShadow);
    }

    private static void AddAccessibilityVariables(Dictionary<string, string> vars, ThemeAccessibility accessibility)
    {
        AddVar(vars, "--df-scale", accessibility.ScaleFactor);
        AddVar(vars, "--df-min-font", accessibility.MinFontSize);
        AddVar(vars, "--df-transition", accessibility.ReduceMotion ? "none" : accessibility.TransitionDuration);
        AddVar(vars, "--df-touch-target", accessibility.MinTouchTargetSize);
    }

    private static void AddVar(Dictionary<string, string> vars, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            vars[name] = value;
        }
    }

    #endregion

    #region Stylesheet Generation

    private string GenerateStylesheetInternal(FormTheme theme)
    {
        var sb = new StringBuilder();

        // Header comment
        sb.AppendLine("/*");
        sb.AppendLine($" * Theme: {theme.Name}");
        sb.AppendLine($" * Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($" * Mode: {theme.Mode}");
        sb.AppendLine(" */");
        sb.AppendLine();

        // CSS Variables in :root (for standalone CSS file)
        sb.AppendLine(":root,");
        sb.AppendLine(".df-theme-scope {");
        var variables = GetAllVariables(theme);
        foreach (var (name, value) in variables)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.AppendLine($"  {name}: {value};");
            }
        }
        sb.AppendLine("}");
        sb.AppendLine();

        // Component styles
        sb.Append(GenerateFormStyles());
        sb.Append(GenerateInputStyles());
        sb.Append(GenerateButtonStyles());
        sb.Append(GenerateStateStyles());
        sb.Append(GenerateUtilityStyles());

        return sb.ToString();
    }

    private static string GenerateFormStyles()
    {
        return """
            /* ============================================
               Form Container & Layout
               ============================================ */

            .df-theme-scope {
              font-family: var(--df-font);
              font-size: var(--df-text-base);
              line-height: var(--df-line-height);
              color: var(--df-text);
              background-color: var(--df-bg);
            }

            .df-form {
              background-color: var(--df-bg);
              color: var(--df-text);
              font-family: var(--df-font);
              font-size: var(--df-text-base);
              line-height: var(--df-line-height);
              padding: var(--df-form-padding);
              max-width: 100%;
            }

            .df-form-card {
              background-color: var(--df-surface);
              border-radius: var(--df-radius-lg);
              box-shadow: var(--df-shadow-card);
              padding: var(--df-form-padding);
            }

            /* Form Header */
            .df-form-header {
              margin-bottom: var(--df-section-gap);
            }

            .df-form-title {
              font-family: var(--df-font-heading);
              font-size: var(--df-text-title);
              font-weight: var(--df-font-bold);
              color: var(--df-text);
              margin: 0 0 var(--df-label-gap) 0;
              line-height: 1.2;
            }

            .df-form-description {
              font-size: var(--df-text-desc);
              color: var(--df-text-secondary);
              margin: 0;
            }

            /* Sections */
            .df-section {
              margin-bottom: var(--df-section-gap);
            }

            .df-section:last-child {
              margin-bottom: 0;
            }

            .df-section-title {
              font-family: var(--df-font-heading);
              font-size: var(--df-text-section);
              font-weight: var(--df-font-semibold);
              color: var(--df-text);
              margin: 0 0 var(--df-question-gap) 0;
              padding-bottom: var(--df-label-gap);
              border-bottom: var(--df-border-width) var(--df-border-style) var(--df-border);
            }

            .df-section-description {
              font-size: var(--df-text-desc);
              color: var(--df-text-secondary);
              margin: calc(var(--df-label-gap) * -1) 0 var(--df-question-gap) 0;
            }

            /* Questions/Fields */
            .df-question {
              margin-bottom: var(--df-question-gap);
            }

            .df-question:last-child {
              margin-bottom: 0;
            }

            .df-label {
              display: block;
              font-size: var(--df-text-question);
              font-weight: var(--df-font-medium);
              color: var(--df-text);
              margin-bottom: var(--df-label-gap);
            }

            .df-label-required::after {
              content: " *";
              color: var(--df-error);
            }

            .df-description {
              font-size: var(--df-text-desc);
              color: var(--df-text-secondary);
              margin-bottom: var(--df-label-gap);
            }

            .df-hint {
              font-size: var(--df-text-desc);
              color: var(--df-text-secondary);
              margin-top: var(--df-label-gap);
            }


            """;
    }

    private static string GenerateInputStyles()
    {
        return """
            /* ============================================
               Form Inputs
               ============================================ */

            .df-input,
            .df-select,
            .df-textarea {
              width: 100%;
              padding: var(--df-input-padding) var(--df-input-padding-x);
              font-family: var(--df-font);
              font-size: var(--df-text-base);
              color: var(--df-text);
              background-color: var(--df-surface);
              border: var(--df-border-width) var(--df-border-style) var(--df-border);
              border-radius: var(--df-radius-md);
              transition: border-color var(--df-transition) ease,
                          box-shadow var(--df-transition) ease,
                          background-color var(--df-transition) ease;
              outline: none;
            }

            .df-input::placeholder,
            .df-textarea::placeholder {
              color: var(--df-text-placeholder);
            }

            .df-input:hover,
            .df-select:hover,
            .df-textarea:hover {
              border-color: var(--df-border-hover);
            }

            .df-input:focus,
            .df-select:focus,
            .df-textarea:focus {
              border-color: var(--df-border-focus);
              box-shadow: var(--df-shadow-focus);
            }

            .df-input:disabled,
            .df-select:disabled,
            .df-textarea:disabled {
              background-color: var(--df-bg-dim);
              color: var(--df-text-disabled);
              cursor: not-allowed;
              opacity: 0.7;
            }

            .df-input:read-only {
              background-color: var(--df-bg-dim);
            }

            /* Textarea specific */
            .df-textarea {
              min-height: 100px;
              resize: vertical;
            }

            /* Select specific */
            .df-select {
              appearance: none;
              background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%2364748B' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E");
              background-repeat: no-repeat;
              background-position: right var(--df-input-padding-x) center;
              padding-right: calc(var(--df-input-padding-x) * 2 + 16px);
            }

            /* Checkbox & Radio */
            .df-checkbox-group,
            .df-radio-group {
              display: flex;
              flex-direction: column;
              gap: var(--df-option-gap);
            }

            .df-checkbox-group.df-inline,
            .df-radio-group.df-inline {
              flex-direction: row;
              flex-wrap: wrap;
            }

            .df-checkbox-item,
            .df-radio-item {
              display: flex;
              align-items: flex-start;
              gap: var(--df-label-gap);
              cursor: pointer;
            }

            .df-checkbox,
            .df-radio {
              width: 18px;
              height: 18px;
              margin: 2px 0 0 0;
              accent-color: var(--df-primary);
              cursor: pointer;
              flex-shrink: 0;
            }

            .df-checkbox-label,
            .df-radio-label {
              font-size: var(--df-text-base);
              color: var(--df-text);
              cursor: pointer;
              user-select: none;
            }

            /* Date/Time inputs */
            .df-input[type="date"],
            .df-input[type="time"],
            .df-input[type="datetime-local"] {
              padding-right: var(--df-input-padding-x);
            }

            /* Number input */
            .df-input[type="number"] {
              -moz-appearance: textfield;
            }

            .df-input[type="number"]::-webkit-outer-spin-button,
            .df-input[type="number"]::-webkit-inner-spin-button {
              -webkit-appearance: none;
              margin: 0;
            }

            /* File input */
            .df-file-input {
              padding: var(--df-input-padding);
              background-color: var(--df-surface);
              border: var(--df-border-width) dashed var(--df-border);
              border-radius: var(--df-radius-md);
              text-align: center;
              cursor: pointer;
              transition: border-color var(--df-transition) ease,
                          background-color var(--df-transition) ease;
            }

            .df-file-input:hover {
              border-color: var(--df-border-hover);
              background-color: var(--df-surface-hover);
            }

            .df-file-input.df-dragover {
              border-color: var(--df-primary);
              background-color: var(--df-selection);
            }


            """;
    }

    private static string GenerateButtonStyles()
    {
        return """
            /* ============================================
               Buttons
               ============================================ */

            .df-btn {
              display: inline-flex;
              align-items: center;
              justify-content: center;
              gap: var(--df-label-gap);
              padding: var(--df-btn-padding-y) var(--df-btn-padding-x);
              font-family: var(--df-font);
              font-size: var(--df-text-base);
              font-weight: var(--df-font-medium);
              line-height: 1;
              text-decoration: none;
              border-radius: var(--df-radius-md);
              border: var(--df-border-width) var(--df-border-style) transparent;
              cursor: pointer;
              transition: background-color var(--df-transition) ease,
                          border-color var(--df-transition) ease,
                          color var(--df-transition) ease,
                          box-shadow var(--df-transition) ease,
                          transform var(--df-transition) ease;
              outline: none;
              user-select: none;
              white-space: nowrap;
            }

            .df-btn:focus-visible {
              box-shadow: 0 0 0 var(--df-focus-width) var(--df-focus-ring);
            }

            .df-btn:active {
              transform: scale(0.98);
            }

            .df-btn:disabled {
              opacity: 0.6;
              cursor: not-allowed;
              transform: none;
            }

            /* Primary Button */
            .df-btn-primary {
              background-color: var(--df-primary);
              color: var(--df-primary-fg);
              border-color: var(--df-primary);
            }

            .df-btn-primary:hover:not(:disabled) {
              background-color: var(--df-primary-hover);
              border-color: var(--df-primary-hover);
              box-shadow: var(--df-shadow-btn-hover);
            }

            /* Secondary Button */
            .df-btn-secondary {
              background-color: var(--df-secondary);
              color: var(--df-secondary-fg);
              border-color: var(--df-secondary);
            }

            .df-btn-secondary:hover:not(:disabled) {
              background-color: var(--df-secondary-hover);
              border-color: var(--df-secondary-hover);
            }

            /* Outline Button */
            .df-btn-outline {
              background-color: transparent;
              color: var(--df-primary);
              border-color: var(--df-primary);
            }

            .df-btn-outline:hover:not(:disabled) {
              background-color: var(--df-primary);
              color: var(--df-primary-fg);
            }

            /* Ghost/Text Button */
            .df-btn-ghost {
              background-color: transparent;
              color: var(--df-text);
              border-color: transparent;
            }

            .df-btn-ghost:hover:not(:disabled) {
              background-color: var(--df-surface-hover);
            }

            /* Danger Button */
            .df-btn-danger {
              background-color: var(--df-error);
              color: white;
              border-color: var(--df-error);
            }

            .df-btn-danger:hover:not(:disabled) {
              background-color: color-mix(in srgb, var(--df-error) 85%, black);
              border-color: color-mix(in srgb, var(--df-error) 85%, black);
            }

            /* Button Sizes */
            .df-btn-sm {
              padding: calc(var(--df-btn-padding-y) * 0.75) calc(var(--df-btn-padding-x) * 0.75);
              font-size: calc(var(--df-text-base) * 0.875);
            }

            .df-btn-lg {
              padding: calc(var(--df-btn-padding-y) * 1.25) calc(var(--df-btn-padding-x) * 1.25);
              font-size: calc(var(--df-text-base) * 1.125);
            }

            /* Button Group */
            .df-btn-group {
              display: flex;
              gap: var(--df-btn-gap);
              flex-wrap: wrap;
            }

            .df-btn-group.df-justify-end {
              justify-content: flex-end;
            }

            .df-btn-group.df-justify-center {
              justify-content: center;
            }

            .df-btn-group.df-justify-between {
              justify-content: space-between;
            }

            /* Form Actions (Submit/Reset area) */
            .df-form-actions {
              display: flex;
              gap: var(--df-btn-gap);
              margin-top: var(--df-section-gap);
              padding-top: var(--df-question-gap);
              border-top: var(--df-border-width) var(--df-border-style) var(--df-border);
            }


            """;
    }

    private static string GenerateStateStyles()
    {
        return """
            /* ============================================
               Validation & State Styles
               ============================================ */

            /* Error State */
            .df-input-error,
            .df-select-error,
            .df-textarea-error {
              border-color: var(--df-error);
            }

            .df-input-error:focus,
            .df-select-error:focus,
            .df-textarea-error:focus {
              border-color: var(--df-error);
              box-shadow: 0 0 0 3px var(--df-error-bg);
            }

            .df-error-message {
              display: flex;
              align-items: center;
              gap: var(--df-label-gap);
              color: var(--df-error);
              font-size: var(--df-text-desc);
              margin-top: var(--df-label-gap);
            }

            .df-error-message::before {
              content: "";
              width: 16px;
              height: 16px;
              background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23EF4444' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Ccircle cx='12' cy='12' r='10'/%3E%3Cline x1='12' x2='12' y1='8' y2='12'/%3E%3Cline x1='12' x2='12.01' y1='16' y2='16'/%3E%3C/svg%3E");
              background-size: contain;
              flex-shrink: 0;
            }

            .df-question-error {
              padding: var(--df-input-padding);
              background-color: var(--df-error-bg);
              border-radius: var(--df-radius-md);
              border-left: 3px solid var(--df-error);
            }

            /* Success State */
            .df-input-success,
            .df-select-success,
            .df-textarea-success {
              border-color: var(--df-success);
            }

            .df-success-message {
              display: flex;
              align-items: center;
              gap: var(--df-label-gap);
              color: var(--df-success);
              font-size: var(--df-text-desc);
              margin-top: var(--df-label-gap);
            }

            .df-success-message::before {
              content: "";
              width: 16px;
              height: 16px;
              background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%2310B981' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M22 11.08V12a10 10 0 1 1-5.93-9.14'/%3E%3Cpath d='m9 11 3 3L22 4'/%3E%3C/svg%3E");
              background-size: contain;
              flex-shrink: 0;
            }

            /* Warning State */
            .df-warning-message {
              display: flex;
              align-items: center;
              gap: var(--df-label-gap);
              color: var(--df-warning);
              font-size: var(--df-text-desc);
              margin-top: var(--df-label-gap);
              padding: var(--df-input-padding);
              background-color: var(--df-warning-bg);
              border-radius: var(--df-radius-md);
            }

            /* Info State */
            .df-info-message {
              display: flex;
              align-items: center;
              gap: var(--df-label-gap);
              color: var(--df-info);
              font-size: var(--df-text-desc);
              margin-top: var(--df-label-gap);
              padding: var(--df-input-padding);
              background-color: var(--df-info-bg);
              border-radius: var(--df-radius-md);
            }

            /* Alert Boxes */
            .df-alert {
              padding: var(--df-input-padding);
              border-radius: var(--df-radius-md);
              margin-bottom: var(--df-question-gap);
            }

            .df-alert-error {
              background-color: var(--df-error-bg);
              color: var(--df-error);
              border: var(--df-border-width) var(--df-border-style) var(--df-error);
            }

            .df-alert-success {
              background-color: var(--df-success-bg);
              color: var(--df-success);
              border: var(--df-border-width) var(--df-border-style) var(--df-success);
            }

            .df-alert-warning {
              background-color: var(--df-warning-bg);
              color: var(--df-warning);
              border: var(--df-border-width) var(--df-border-style) var(--df-warning);
            }

            .df-alert-info {
              background-color: var(--df-info-bg);
              color: var(--df-info);
              border: var(--df-border-width) var(--df-border-style) var(--df-info);
            }


            """;
    }

    private static string GenerateUtilityStyles()
    {
        return """
            /* ============================================
               Utility Classes
               ============================================ */

            /* Progress Bar */
            .df-progress {
              height: 8px;
              background-color: var(--df-bg-dim);
              border-radius: var(--df-radius-full);
              overflow: hidden;
            }

            .df-progress-bar {
              height: 100%;
              background-color: var(--df-primary);
              border-radius: var(--df-radius-full);
              transition: width var(--df-transition) ease;
            }

            .df-progress-text {
              font-size: var(--df-text-desc);
              color: var(--df-text-secondary);
              margin-top: var(--df-label-gap);
              text-align: center;
            }

            /* Divider */
            .df-divider {
              height: var(--df-border-width);
              background-color: var(--df-border);
              margin: var(--df-section-gap) 0;
            }

            /* Loading States */
            .df-loading {
              opacity: 0.6;
              pointer-events: none;
            }

            .df-spinner {
              width: 20px;
              height: 20px;
              border: 2px solid var(--df-border);
              border-top-color: var(--df-primary);
              border-radius: 50%;
              animation: df-spin 0.8s linear infinite;
            }

            @keyframes df-spin {
              to {
                transform: rotate(360deg);
              }
            }

            /* Screen Reader Only */
            .df-sr-only {
              position: absolute;
              width: 1px;
              height: 1px;
              padding: 0;
              margin: -1px;
              overflow: hidden;
              clip: rect(0, 0, 0, 0);
              white-space: nowrap;
              border: 0;
            }

            /* Focus Visible (for keyboard navigation) */
            .df-focus-visible:focus-visible {
              outline: var(--df-focus-width) var(--df-focus-style) var(--df-focus-ring);
              outline-offset: var(--df-focus-offset);
            }

            /* Responsive Utilities */
            @media (max-width: 640px) {
              .df-form {
                padding: calc(var(--df-form-padding) * 0.75);
              }

              .df-form-card {
                padding: calc(var(--df-form-padding) * 0.75);
                border-radius: var(--df-radius-md);
              }

              .df-btn-group {
                flex-direction: column;
              }

              .df-btn-group .df-btn {
                width: 100%;
              }

              .df-checkbox-group.df-inline,
              .df-radio-group.df-inline {
                flex-direction: column;
              }
            }

            /* Print Styles */
            @media print {
              .df-theme-scope {
                background-color: white !important;
                color: black !important;
              }

              .df-btn,
              .df-form-actions {
                display: none !important;
              }

              .df-input,
              .df-select,
              .df-textarea {
                border: 1px solid #ccc !important;
                box-shadow: none !important;
              }
            }

            """;
    }

    #endregion

    #region Minification

    private static string MinifyCss(string css)
    {
        if (string.IsNullOrWhiteSpace(css))
            return string.Empty;

        // Remove comments
        css = RemoveCssComments().Replace(css, string.Empty);

        // Remove newlines and multiple spaces
        css = CollapseWhitespace().Replace(css, " ");

        // Remove spaces around specific characters
        css = SpaceBeforeBrace().Replace(css, "{");
        css = SpaceAfterBrace().Replace(css, "}");
        css = SpaceAroundColon().Replace(css, ":");
        css = SpaceAroundSemicolon().Replace(css, ";");
        css = SpaceAroundComma().Replace(css, ",");

        // Remove trailing semicolons before closing braces
        css = TrailingSemicolon().Replace(css, "}");

        // Remove leading/trailing whitespace
        css = css.Trim();

        return css;
    }

    [GeneratedRegex(@"/\*[\s\S]*?\*/", RegexOptions.Compiled)]
    private static partial Regex RemoveCssComments();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex CollapseWhitespace();

    [GeneratedRegex(@"\s*\{\s*", RegexOptions.Compiled)]
    private static partial Regex SpaceBeforeBrace();

    [GeneratedRegex(@"\s*\}\s*", RegexOptions.Compiled)]
    private static partial Regex SpaceAfterBrace();

    [GeneratedRegex(@"\s*:\s*", RegexOptions.Compiled)]
    private static partial Regex SpaceAroundColon();

    [GeneratedRegex(@"\s*;\s*", RegexOptions.Compiled)]
    private static partial Regex SpaceAroundSemicolon();

    [GeneratedRegex(@"\s*,\s*", RegexOptions.Compiled)]
    private static partial Regex SpaceAroundComma();

    [GeneratedRegex(@";\s*\}", RegexOptions.Compiled)]
    private static partial Regex TrailingSemicolon();

    #endregion

    #region Hashing

    private static string ComputeThemeHash(FormTheme theme)
    {
        // Create a hash based on the theme's JSON representation
        var json = JsonSerializer.Serialize(theme, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(hashBytes)[..16]; // Use first 16 chars for brevity
    }

    #endregion
}
