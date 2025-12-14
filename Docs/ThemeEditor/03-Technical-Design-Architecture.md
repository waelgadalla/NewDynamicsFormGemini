# Theme Editor Specification - Part 3
# Technical Design & Architecture

**Document Version:** 2.0
**Date:** December 2025

---

## 1. Architecture Overview

### 1.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           THEME EDITOR SYSTEM                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         PRESENTATION LAYER                           │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌────────────┐ │   │
│  │  │ ThemeEditor │  │ ThemeScope  │  │ ThemePreview│  │ ThemeLibrary│ │   │
│  │  │    Page     │  │  Wrapper    │  │  Component  │  │   Modal    │ │   │
│  │  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  └─────┬──────┘ │   │
│  │         │                │                │               │        │   │
│  │  ┌──────┴────────────────┴────────────────┴───────────────┴──────┐ │   │
│  │  │                    EDITOR COMPONENTS                          │ │   │
│  │  │  ColorPicker │ FontSelector │ SliderControl │ ImageUpload   │ │   │
│  │  │  PresetCard  │ SectionPanel │ ToggleSwitch  │ ResetButton   │ │   │
│  │  └───────────────────────────────────────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                      │                                     │
│                                      ▼                                     │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                          SERVICE LAYER                               │   │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌───────────────────┐ │   │
│  │  │ ThemeEditorState │  │ ThemePresetSvc   │  │ ThemePersistence  │ │   │
│  │  │    Service       │  │                  │  │    Service        │ │   │
│  │  │  - CurrentTheme  │  │  - GetPresets()  │  │  - SaveTheme()    │ │   │
│  │  │  - UndoStack     │  │  - GetPreset()   │  │  - LoadTheme()    │ │   │
│  │  │  - IsDirty       │  │  - ValidateTheme │  │  - DeleteTheme()  │ │   │
│  │  │  - Events        │  │                  │  │  - ListThemes()   │ │   │
│  │  └──────────────────┘  └──────────────────┘  └───────────────────┘ │   │
│  │                                                                     │   │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌───────────────────┐ │   │
│  │  │ ThemeCssGenerator│  │ ThemeImportExport│  │ ThemeValidator    │ │   │
│  │  │                  │  │    Service       │  │    Service        │ │   │
│  │  │  - ToCssVars()   │  │  - ExportJson()  │  │  - ValidateTheme()│ │   │
│  │  │  - ToStylesheet()│  │  - ImportJson()  │  │  - CheckContrast()│ │   │
│  │  │  - ToMinifiedCss │  │  - CopyCss()     │  │  - ValidateColors │ │   │
│  │  └──────────────────┘  └──────────────────┘  └───────────────────┘ │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                      │                                     │
│                                      ▼                                     │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                           DATA LAYER                                 │   │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌───────────────────┐ │   │
│  │  │   FormTheme      │  │   ThemeEntity    │  │   ThemePresets    │ │   │
│  │  │   (Model)        │  │   (DB Entity)    │  │   (Static)        │ │   │
│  │  └──────────────────┘  └──────────────────┘  └───────────────────┘ │   │
│  │                              │                                      │   │
│  │                              ▼                                      │   │
│  │                    ┌──────────────────┐                            │   │
│  │                    │   SQL Server     │                            │   │
│  │                    │   (Themes Table) │                            │   │
│  │                    └──────────────────┘                            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Component Hierarchy

```
ThemeEditorPage.razor
├── ThemeEditorHeader.razor
│   ├── ThemeNameInput
│   ├── PresetSelector
│   ├── ModeToggle (Light/Dark)
│   ├── UndoRedoButtons
│   ├── SaveButton
│   └── ActionsDropdown (Import/Export/Reset)
│
├── ThemeEditorLayout.razor (Split Pane)
│   │
│   ├── LEFT: ThemeEditorSidebar.razor
│   │   ├── BasicAdvancedToggle
│   │   ├── SearchBox
│   │   │
│   │   └── Sections (Collapsible)
│   │       ├── ThemePresetSection
│   │       │   └── PresetGrid
│   │       │
│   │       ├── ColorsSection
│   │       │   ├── ColorPicker (Primary)
│   │       │   ├── ColorPicker (Secondary)
│   │       │   ├── ColorPicker (Background)
│   │       │   ├── ColorPicker (Text)
│   │       │   └── [Advanced] Full color palette
│   │       │
│   │       ├── TypographySection
│   │       │   ├── FontFamilySelector
│   │       │   ├── FontSizeSlider
│   │       │   └── [Advanced] Heading fonts, weights
│   │       │
│   │       ├── SpacingSection
│   │       │   ├── FormPaddingSlider
│   │       │   ├── QuestionSpacingSlider
│   │       │   └── [Advanced] All spacing controls
│   │       │
│   │       ├── BordersSection
│   │       │   ├── BorderRadiusSlider
│   │       │   └── [Advanced] Border width, style
│   │       │
│   │       ├── ShadowsSection
│   │       │   └── ShadowPresetSelector
│   │       │
│   │       ├── HeaderSection
│   │       │   ├── LogoUpload
│   │       │   ├── LogoPositionSelector
│   │       │   └── [Advanced] Background, overlay
│   │       │
│   │       ├── BackgroundSection
│   │       │   ├── BackgroundColorPicker
│   │       │   └── [Advanced] Image, gradient
│   │       │
│   │       └── [Phase 3] ComponentsSection
│   │           ├── ButtonStyles
│   │           ├── InputStyles
│   │           └── ...
│   │
│   └── RIGHT: ThemePreviewPanel.razor
│       ├── PreviewToolbar
│       │   ├── DeviceSelector (Desktop/Tablet/Mobile)
│       │   ├── ZoomControl
│       │   └── LanguageToggle
│       │
│       └── ThemeScope (applies current theme)
│           └── PreviewForm (sample form)
│
└── Modals
    ├── ThemeLibraryModal.razor
    ├── ImportThemeModal.razor
    └── ExportThemeModal.razor
```

---

## 2. Service Layer Design

### 2.1 IThemeEditorStateService

Central state management for the theme editor:

```csharp
namespace VisualEditorOpus.Services.Theming
{
    public interface IThemeEditorStateService
    {
        // === Current State ===
        FormTheme CurrentTheme { get; }
        FormTheme? OriginalTheme { get; }  // For reset functionality
        bool IsDirty { get; }
        bool IsAdvancedMode { get; }

        // === Theme Operations ===
        void LoadTheme(FormTheme theme);
        void ApplyPreset(string presetId);
        void UpdateProperty<T>(Expression<Func<FormTheme, T>> property, T value);
        void UpdateColors(Action<ThemeColors> update);
        void UpdateTypography(Action<ThemeTypography> update);
        void UpdateSpacing(Action<ThemeSpacing> update);
        void UpdateBorders(Action<ThemeBorders> update);
        void UpdateShadows(Action<ThemeShadows> update);
        void UpdateHeader(Action<ThemeHeader> update);
        void UpdateBackground(Action<ThemeBackground> update);

        // === Undo/Redo ===
        bool CanUndo { get; }
        bool CanRedo { get; }
        void Undo();
        void Redo();
        void ClearHistory();

        // === Mode ===
        void SetMode(ThemeMode mode);
        void ToggleAdvancedMode();

        // === Reset ===
        void Reset();
        void ResetToPreset(string presetId);

        // === Events ===
        event Action? OnThemeChanged;
        event Action? OnDirtyStateChanged;
        event Action<string>? OnPropertyChanged;
    }
}
```

### 2.2 IThemePersistenceService

Database operations for themes:

```csharp
namespace VisualEditorOpus.Services.Theming
{
    public interface IThemePersistenceService
    {
        // === CRUD Operations ===
        Task<FormTheme?> GetThemeAsync(string themeId);
        Task<FormTheme?> GetDefaultThemeAsync();
        Task<IReadOnlyList<ThemeSummary>> ListThemesAsync();
        Task<string> SaveThemeAsync(FormTheme theme);
        Task UpdateThemeAsync(FormTheme theme);
        Task DeleteThemeAsync(string themeId);

        // === Organization Themes ===
        Task<IReadOnlyList<ThemeSummary>> GetOrganizationThemesAsync(string orgId);
        Task SetDefaultThemeAsync(string themeId, string? orgId = null);

        // === Theme Assignment ===
        Task AssignThemeToFormAsync(string formId, string themeId);
        Task<FormTheme?> GetFormThemeAsync(string formId);
    }

    public record ThemeSummary(
        string Id,
        string Name,
        string Description,
        ThemeMode Mode,
        string? PreviewImageUrl,
        DateTime ModifiedAt,
        bool IsDefault
    );
}
```

### 2.3 IThemeCssGeneratorService

Converts theme to CSS:

```csharp
namespace VisualEditorOpus.Services.Theming
{
    public interface IThemeCssGeneratorService
    {
        /// <summary>
        /// Generates inline CSS variables string for style attribute
        /// </summary>
        string GenerateCssVariables(FormTheme theme);

        /// <summary>
        /// Generates complete CSS stylesheet
        /// </summary>
        string GenerateStylesheet(FormTheme theme);

        /// <summary>
        /// Generates minified CSS for production
        /// </summary>
        string GenerateMinifiedCss(FormTheme theme);

        /// <summary>
        /// Gets individual variable value
        /// </summary>
        string GetVariable(FormTheme theme, string variableName);
    }
}
```

### 2.4 IThemeImportExportService

Import/export functionality:

```csharp
namespace VisualEditorOpus.Services.Theming
{
    public interface IThemeImportExportService
    {
        // === Export ===
        string ExportToJson(FormTheme theme, bool prettyPrint = true);
        byte[] ExportToJsonBytes(FormTheme theme);
        string ExportToCss(FormTheme theme);

        // === Import ===
        ThemeImportResult ImportFromJson(string json);
        ThemeImportResult ImportFromJson(Stream stream);

        // === Clipboard ===
        string GetCssVariablesForClipboard(FormTheme theme);
    }

    public record ThemeImportResult(
        bool Success,
        FormTheme? Theme,
        IReadOnlyList<string> Errors,
        IReadOnlyList<string> Warnings
    );
}
```

### 2.5 IThemePresetService

Preset management:

```csharp
namespace VisualEditorOpus.Services.Theming
{
    public interface IThemePresetService
    {
        IReadOnlyList<ThemePresetInfo> GetAllPresets();
        IReadOnlyList<ThemePresetInfo> GetPresetsByCategory(string category);
        FormTheme GetPreset(string presetId);
        bool PresetExists(string presetId);

        // === Categories ===
        IReadOnlyList<string> GetCategories();
    }

    public record ThemePresetInfo(
        string Id,
        string Name,
        string Description,
        string Category,        // "Professional", "Government", "Dark", etc.
        ThemeMode Mode,
        string PreviewColor,    // Primary color for visual preview
        string[]? Tags          // "accessible", "modern", "minimal"
    );
}
```

### 2.6 IThemeValidatorService

Validation and accessibility:

```csharp
namespace VisualEditorOpus.Services.Theming
{
    public interface IThemeValidatorService
    {
        ThemeValidationResult Validate(FormTheme theme);
        ContrastCheckResult CheckContrast(string foreground, string background);
        bool IsWcagAACompliant(string foreground, string background);
        bool IsWcagAAACompliant(string foreground, string background);
    }

    public record ThemeValidationResult(
        bool IsValid,
        IReadOnlyList<ThemeValidationIssue> Issues
    );

    public record ThemeValidationIssue(
        ThemeValidationSeverity Severity,
        string Property,
        string Message,
        string? Suggestion
    );

    public enum ThemeValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public record ContrastCheckResult(
        double Ratio,
        bool PassesAA,
        bool PassesAAA,
        bool PassesAALargeText,
        string Recommendation
    );
}
```

---

## 3. Component Design

### 3.1 ThemeScope Component

The core wrapper that injects CSS variables:

```razor
@* File: Components/Theming/ThemeScope.razor *@

<div class="df-theme-scope @CssClass"
     style="@GetCssVariables()"
     data-theme="@Theme.Mode.ToString().ToLower()">

    @if (Theme.Header.Enabled && !string.IsNullOrEmpty(Theme.Header.LogoUrl))
    {
        <div class="df-form-header" style="@GetHeaderStyles()">
            @if (Theme.Header.OverlayEnabled)
            {
                <div class="df-header-overlay"
                     style="background: @Theme.Header.OverlayColor;"></div>
            }
            <div class="df-header-content" style="@GetHeaderContentStyles()">
                <img src="@Theme.Header.LogoUrl"
                     alt="Logo"
                     class="df-logo"
                     style="max-height: @Theme.Header.LogoMaxHeight;" />
            </div>
        </div>
    }

    @ChildContent
</div>

@code {
    [Parameter] public FormTheme Theme { get; set; } = new();
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string CssClass { get; set; } = "";

    [Inject] private IThemeCssGeneratorService CssGenerator { get; set; } = default!;

    private string GetCssVariables() => CssGenerator.GenerateCssVariables(Theme);

    private string GetHeaderStyles()
    {
        var styles = new List<string>();

        if (Theme.Header.BackgroundType == "color")
            styles.Add($"background-color: {Theme.Header.BackgroundColor}");
        else if (Theme.Header.BackgroundType == "image")
        {
            styles.Add($"background-image: url('{Theme.Header.BackgroundImage}')");
            styles.Add($"background-size: {Theme.Header.BackgroundSize}");
            styles.Add($"background-position: {Theme.Header.BackgroundPosition}");
        }

        if (Theme.Header.Height != "auto")
            styles.Add($"height: {Theme.Header.Height}");

        styles.Add($"padding: {Theme.Header.Padding}");

        return string.Join("; ", styles);
    }

    private string GetHeaderContentStyles()
    {
        var align = Theme.Header.ContentAlignment switch
        {
            "left" => "flex-start",
            "right" => "flex-end",
            _ => "center"
        };

        var vAlign = Theme.Header.VerticalAlignment switch
        {
            "top" => "flex-start",
            "bottom" => "flex-end",
            _ => "center"
        };

        return $"justify-content: {align}; align-items: {vAlign};";
    }
}
```

### 3.2 ThemeCssGeneratorService Implementation

```csharp
namespace VisualEditorOpus.Services.Theming
{
    public class ThemeCssGeneratorService : IThemeCssGeneratorService
    {
        public string GenerateCssVariables(FormTheme theme)
        {
            var vars = new StringBuilder();

            // === Colors ===
            AppendVar(vars, "--df-primary", theme.Colors.Primary);
            AppendVar(vars, "--df-primary-hover", theme.Colors.PrimaryHover);
            AppendVar(vars, "--df-primary-fg", theme.Colors.PrimaryForeground);
            AppendVar(vars, "--df-secondary", theme.Colors.Secondary);
            AppendVar(vars, "--df-secondary-hover", theme.Colors.SecondaryHover);
            AppendVar(vars, "--df-secondary-fg", theme.Colors.SecondaryForeground);

            AppendVar(vars, "--df-bg", theme.Colors.Background);
            AppendVar(vars, "--df-bg-dim", theme.Colors.BackgroundDim);
            AppendVar(vars, "--df-surface", theme.Colors.Surface);
            AppendVar(vars, "--df-surface-hover", theme.Colors.SurfaceHover);

            AppendVar(vars, "--df-text", theme.Colors.TextPrimary);
            AppendVar(vars, "--df-text-secondary", theme.Colors.TextSecondary);
            AppendVar(vars, "--df-text-disabled", theme.Colors.TextDisabled);
            AppendVar(vars, "--df-text-placeholder", theme.Colors.TextPlaceholder);

            AppendVar(vars, "--df-border", theme.Colors.Border);
            AppendVar(vars, "--df-border-hover", theme.Colors.BorderHover);
            AppendVar(vars, "--df-border-focus", theme.Colors.BorderFocus);

            AppendVar(vars, "--df-error", theme.Colors.Error);
            AppendVar(vars, "--df-error-bg", theme.Colors.ErrorBackground);
            AppendVar(vars, "--df-success", theme.Colors.Success);
            AppendVar(vars, "--df-success-bg", theme.Colors.SuccessBackground);
            AppendVar(vars, "--df-warning", theme.Colors.Warning);
            AppendVar(vars, "--df-warning-bg", theme.Colors.WarningBackground);
            AppendVar(vars, "--df-info", theme.Colors.Info);
            AppendVar(vars, "--df-info-bg", theme.Colors.InfoBackground);

            AppendVar(vars, "--df-focus-ring", theme.Colors.FocusRing);
            AppendVar(vars, "--df-selection", theme.Colors.Selection);

            // === Typography ===
            AppendVar(vars, "--df-font", theme.Typography.FontFamily);
            AppendVar(vars, "--df-font-heading",
                string.IsNullOrEmpty(theme.Typography.HeadingFontFamily)
                    ? theme.Typography.FontFamily
                    : theme.Typography.HeadingFontFamily);
            AppendVar(vars, "--df-font-mono", theme.Typography.MonoFontFamily);

            AppendVar(vars, "--df-text-base", theme.Typography.BaseFontSize);
            AppendVar(vars, "--df-line-height", theme.Typography.LineHeight);
            AppendVar(vars, "--df-text-title", theme.Typography.FormTitleSize);
            AppendVar(vars, "--df-text-section", theme.Typography.SectionTitleSize);
            AppendVar(vars, "--df-text-question", theme.Typography.QuestionTitleSize);
            AppendVar(vars, "--df-text-desc", theme.Typography.DescriptionSize);

            AppendVar(vars, "--df-font-normal", theme.Typography.FontWeightNormal);
            AppendVar(vars, "--df-font-medium", theme.Typography.FontWeightMedium);
            AppendVar(vars, "--df-font-semibold", theme.Typography.FontWeightSemibold);
            AppendVar(vars, "--df-font-bold", theme.Typography.FontWeightBold);

            // === Spacing ===
            AppendVar(vars, "--df-unit", theme.Spacing.BaseUnit);
            AppendVar(vars, "--df-form-padding", theme.Spacing.FormPadding);
            AppendVar(vars, "--df-section-gap", theme.Spacing.SectionSpacing);
            AppendVar(vars, "--df-question-gap", theme.Spacing.QuestionSpacing);
            AppendVar(vars, "--df-input-padding", theme.Spacing.InputPadding);
            AppendVar(vars, "--df-label-gap", theme.Spacing.LabelSpacing);
            AppendVar(vars, "--df-option-gap", theme.Spacing.OptionSpacing);

            // === Borders ===
            AppendVar(vars, "--df-border-width", theme.Borders.BorderWidth);
            AppendVar(vars, "--df-border-style", theme.Borders.BorderStyle);
            AppendVar(vars, "--df-radius-sm", theme.Borders.RadiusSmall);
            AppendVar(vars, "--df-radius-md", theme.Borders.RadiusMedium);
            AppendVar(vars, "--df-radius-lg", theme.Borders.RadiusLarge);
            AppendVar(vars, "--df-radius-xl", theme.Borders.RadiusXLarge);
            AppendVar(vars, "--df-focus-width", theme.Borders.FocusRingWidth);
            AppendVar(vars, "--df-focus-offset", theme.Borders.FocusRingOffset);

            // === Shadows ===
            AppendVar(vars, "--df-shadow-none", theme.Shadows.ShadowNone);
            AppendVar(vars, "--df-shadow-sm", theme.Shadows.ShadowSmall);
            AppendVar(vars, "--df-shadow-md", theme.Shadows.ShadowMedium);
            AppendVar(vars, "--df-shadow-lg", theme.Shadows.ShadowLarge);
            AppendVar(vars, "--df-shadow-xl", theme.Shadows.ShadowXLarge);
            AppendVar(vars, "--df-shadow-card", theme.Shadows.CardShadow);
            AppendVar(vars, "--df-shadow-dropdown", theme.Shadows.DropdownShadow);
            AppendVar(vars, "--df-shadow-modal", theme.Shadows.ModalShadow);
            AppendVar(vars, "--df-shadow-focus", theme.Shadows.InputFocusShadow);

            // === Accessibility ===
            AppendVar(vars, "--df-scale", theme.Accessibility.ScaleFactor);
            AppendVar(vars, "--df-min-font", theme.Accessibility.MinFontSize);

            return vars.ToString().TrimEnd(';');
        }

        private void AppendVar(StringBuilder sb, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.Append($"{name}: {value}; ");
            }
        }

        public string GenerateStylesheet(FormTheme theme)
        {
            var css = new StringBuilder();

            css.AppendLine(".df-theme-scope {");
            css.AppendLine($"  {GenerateCssVariables(theme)}");
            css.AppendLine("}");
            css.AppendLine();

            // Add component styles that consume variables
            css.AppendLine(GenerateComponentStyles());

            return css.ToString();
        }

        private string GenerateComponentStyles()
        {
            return @"
/* Form Container */
.df-form {
    background-color: var(--df-bg);
    color: var(--df-text);
    font-family: var(--df-font);
    font-size: var(--df-text-base);
    line-height: var(--df-line-height);
    padding: var(--df-form-padding);
}

/* Form Card */
.df-form-card {
    background-color: var(--df-surface);
    border-radius: var(--df-radius-lg);
    box-shadow: var(--df-shadow-card);
    padding: var(--df-form-padding);
}

/* Section */
.df-section {
    margin-bottom: var(--df-section-gap);
}

.df-section-title {
    font-family: var(--df-font-heading);
    font-size: var(--df-text-section);
    font-weight: var(--df-font-semibold);
    color: var(--df-text);
    margin-bottom: var(--df-label-gap);
}

/* Question */
.df-question {
    margin-bottom: var(--df-question-gap);
}

.df-label {
    display: block;
    font-size: var(--df-text-question);
    font-weight: var(--df-font-medium);
    color: var(--df-text);
    margin-bottom: var(--df-label-gap);
}

.df-description {
    font-size: var(--df-text-desc);
    color: var(--df-text-secondary);
    margin-bottom: var(--df-label-gap);
}

/* Inputs */
.df-input {
    width: 100%;
    padding: var(--df-input-padding);
    font-family: var(--df-font);
    font-size: var(--df-text-base);
    color: var(--df-text);
    background-color: var(--df-surface);
    border: var(--df-border-width) var(--df-border-style) var(--df-border);
    border-radius: var(--df-radius-md);
    transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.df-input::placeholder {
    color: var(--df-text-placeholder);
}

.df-input:hover {
    border-color: var(--df-border-hover);
}

.df-input:focus {
    outline: none;
    border-color: var(--df-border-focus);
    box-shadow: var(--df-shadow-focus);
}

.df-input:disabled {
    background-color: var(--df-bg-dim);
    color: var(--df-text-disabled);
    cursor: not-allowed;
}

/* Buttons */
.df-btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: var(--df-input-padding) calc(var(--df-input-padding) * 2);
    font-family: var(--df-font);
    font-size: var(--df-text-base);
    font-weight: var(--df-font-medium);
    border-radius: var(--df-radius-md);
    border: none;
    cursor: pointer;
    transition: background-color 0.15s ease, transform 0.1s ease;
}

.df-btn:active {
    transform: scale(0.98);
}

.df-btn-primary {
    background-color: var(--df-primary);
    color: var(--df-primary-fg);
}

.df-btn-primary:hover {
    background-color: var(--df-primary-hover);
}

.df-btn-secondary {
    background-color: var(--df-secondary);
    color: var(--df-secondary-fg);
}

.df-btn-secondary:hover {
    background-color: var(--df-secondary-hover);
}

/* Error States */
.df-input-error {
    border-color: var(--df-error);
}

.df-input-error:focus {
    border-color: var(--df-error);
    box-shadow: 0 0 0 3px var(--df-error-bg);
}

.df-error-message {
    color: var(--df-error);
    font-size: var(--df-text-desc);
    margin-top: var(--df-label-gap);
}

/* Success States */
.df-input-success {
    border-color: var(--df-success);
}

.df-success-message {
    color: var(--df-success);
    font-size: var(--df-text-desc);
    margin-top: var(--df-label-gap);
}
";
        }

        public string GenerateMinifiedCss(FormTheme theme)
        {
            var css = GenerateStylesheet(theme);
            // Simple minification - remove newlines and extra spaces
            return System.Text.RegularExpressions.Regex.Replace(css, @"\s+", " ")
                .Replace(" { ", "{")
                .Replace(" } ", "}")
                .Replace("; ", ";")
                .Replace(": ", ":")
                .Trim();
        }

        public string GetVariable(FormTheme theme, string variableName)
        {
            // Parse and return specific variable
            var vars = GenerateCssVariables(theme);
            var pattern = $@"{variableName}:\s*([^;]+)";
            var match = System.Text.RegularExpressions.Regex.Match(vars, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }
    }
}
```

### 3.3 ColorPicker Component

```razor
@* File: Components/Theming/Controls/ColorPicker.razor *@

<div class="df-color-picker @(Compact ? "compact" : "")">
    @if (!string.IsNullOrEmpty(Label))
    {
        <label class="df-color-picker-label">
            @Label
            @if (ShowTooltip && !string.IsNullOrEmpty(Tooltip))
            {
                <span class="df-tooltip" title="@Tooltip">
                    <i class="bi bi-question-circle"></i>
                </span>
            }
        </label>
    }

    <div class="df-color-picker-input-group">
        <div class="df-color-swatch"
             style="background-color: @Value;"
             @onclick="OpenPicker">
        </div>

        <input type="color"
               class="df-color-native-picker"
               value="@Value"
               @ref="_colorInput"
               @onchange="OnColorChange" />

        <input type="text"
               class="df-color-text-input"
               value="@Value"
               @oninput="OnTextInput"
               placeholder="#000000"
               maxlength="7" />

        @if (ShowOpacity)
        {
            <input type="range"
                   class="df-color-opacity"
                   min="0" max="100"
                   value="@Opacity"
                   @oninput="OnOpacityChange" />
        }
    </div>

    @if (ShowPresets && Presets?.Any() == true)
    {
        <div class="df-color-presets">
            @foreach (var preset in Presets)
            {
                <button class="df-color-preset @(preset == Value ? "active" : "")"
                        style="background-color: @preset;"
                        @onclick="() => SelectPreset(preset)"
                        title="@preset">
                </button>
            }
        </div>
    }
</div>

@code {
    [Parameter] public string Value { get; set; } = "#000000";
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string Tooltip { get; set; } = "";
    [Parameter] public bool ShowTooltip { get; set; } = true;
    [Parameter] public bool ShowOpacity { get; set; } = false;
    [Parameter] public int Opacity { get; set; } = 100;
    [Parameter] public EventCallback<int> OpacityChanged { get; set; }
    [Parameter] public bool ShowPresets { get; set; } = true;
    [Parameter] public string[]? Presets { get; set; }
    [Parameter] public bool Compact { get; set; } = false;

    private ElementReference _colorInput;

    // Default color presets
    private static readonly string[] DefaultPresets = new[]
    {
        "#6366F1", "#8B5CF6", "#EC4899", "#EF4444", "#F59E0B",
        "#10B981", "#14B8A6", "#3B82F6", "#1E40AF", "#0F172A"
    };

    protected override void OnInitialized()
    {
        Presets ??= DefaultPresets;
    }

    private async Task OpenPicker()
    {
        // Trigger native color picker
        await _colorInput.FocusAsync();
    }

    private async Task OnColorChange(ChangeEventArgs e)
    {
        var color = e.Value?.ToString() ?? "#000000";
        await ValueChanged.InvokeAsync(color);
    }

    private async Task OnTextInput(ChangeEventArgs e)
    {
        var input = e.Value?.ToString() ?? "";
        if (IsValidHexColor(input))
        {
            await ValueChanged.InvokeAsync(input);
        }
    }

    private async Task OnOpacityChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var opacity))
        {
            await OpacityChanged.InvokeAsync(opacity);
        }
    }

    private async Task SelectPreset(string color)
    {
        await ValueChanged.InvokeAsync(color);
    }

    private bool IsValidHexColor(string color)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(
            color, "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
    }
}
```

---

## 4. Database Schema

### 4.1 Themes Table

```sql
CREATE TABLE [dbo].[Themes] (
    [Id]              NVARCHAR(50)   NOT NULL PRIMARY KEY,
    [Name]            NVARCHAR(100)  NOT NULL,
    [Description]     NVARCHAR(500)  NULL,
    [BasePreset]      NVARCHAR(50)   NULL,
    [JsonData]        NVARCHAR(MAX)  NOT NULL,  -- Serialized FormTheme
    [PreviewImageUrl] NVARCHAR(500)  NULL,
    [IsDefault]       BIT            NOT NULL DEFAULT 0,
    [IsLocked]        BIT            NOT NULL DEFAULT 0,
    [OrganizationId]  NVARCHAR(50)   NULL,      -- For multi-tenant
    [CreatedBy]       NVARCHAR(100)  NULL,
    [CreatedAt]       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    [Version]         INT            NOT NULL DEFAULT 1,

    INDEX IX_Themes_Organization (OrganizationId),
    INDEX IX_Themes_IsDefault (IsDefault),
    INDEX IX_Themes_ModifiedAt (ModifiedAt DESC)
);

-- Form-Theme assignment
CREATE TABLE [dbo].[FormThemes] (
    [FormId]    NVARCHAR(50)  NOT NULL,
    [ThemeId]   NVARCHAR(50)  NOT NULL,
    [AppliedAt] DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    PRIMARY KEY (FormId),
    FOREIGN KEY (ThemeId) REFERENCES Themes(Id)
);

-- Theme version history (for Phase 3)
CREATE TABLE [dbo].[ThemeVersions] (
    [Id]        INT           IDENTITY(1,1) PRIMARY KEY,
    [ThemeId]   NVARCHAR(50)  NOT NULL,
    [Version]   INT           NOT NULL,
    [JsonData]  NVARCHAR(MAX) NOT NULL,
    [ChangedBy] NVARCHAR(100) NULL,
    [ChangedAt] DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    [ChangeNote] NVARCHAR(500) NULL,

    FOREIGN KEY (ThemeId) REFERENCES Themes(Id) ON DELETE CASCADE,
    INDEX IX_ThemeVersions_ThemeId_Version (ThemeId, Version DESC)
);
```

### 4.2 Entity Classes

```csharp
namespace VisualEditorOpus.Data.Entities
{
    public class ThemeEntity
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? BasePreset { get; set; }
        public string JsonData { get; set; } = "{}";
        public string? PreviewImageUrl { get; set; }
        public bool IsDefault { get; set; }
        public bool IsLocked { get; set; }
        public string? OrganizationId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int Version { get; set; }
    }

    public class FormThemeEntity
    {
        public string FormId { get; set; } = "";
        public string ThemeId { get; set; } = "";
        public DateTime AppliedAt { get; set; }

        public ThemeEntity? Theme { get; set; }
    }
}
```

---

## 5. File Structure

### 5.1 Proposed Directory Structure

```
Src/VisualEditorOpus/
├── Components/
│   └── Theming/                          # NEW FOLDER
│       ├── ThemeScope.razor              # CSS variable injector wrapper
│       ├── ThemeScope.razor.css
│       │
│       ├── Editor/                       # Theme editor components
│       │   ├── ThemeEditorPage.razor     # Main editor page
│       │   ├── ThemeEditorPage.razor.css
│       │   ├── ThemeEditorHeader.razor
│       │   ├── ThemeEditorSidebar.razor
│       │   ├── ThemeEditorSidebar.razor.css
│       │   ├── ThemePreviewPanel.razor
│       │   └── ThemePreviewPanel.razor.css
│       │
│       ├── Sections/                     # Editor sections
│       │   ├── ThemePresetSection.razor
│       │   ├── ColorsSection.razor
│       │   ├── TypographySection.razor
│       │   ├── SpacingSection.razor
│       │   ├── BordersSection.razor
│       │   ├── ShadowsSection.razor
│       │   ├── HeaderSection.razor
│       │   ├── BackgroundSection.razor
│       │   └── ComponentsSection.razor   # Phase 3
│       │
│       ├── Controls/                     # Reusable controls
│       │   ├── ColorPicker.razor
│       │   ├── ColorPicker.razor.css
│       │   ├── FontSelector.razor
│       │   ├── SliderControl.razor
│       │   ├── ImageUpload.razor
│       │   ├── PresetCard.razor
│       │   ├── PresetCard.razor.css
│       │   ├── SectionPanel.razor
│       │   └── ShadowPicker.razor
│       │
│       └── Modals/
│           ├── ThemeLibraryModal.razor
│           ├── ImportThemeModal.razor
│           └── ExportThemeModal.razor
│
├── Models/
│   └── Theming/                          # NEW FOLDER
│       ├── FormTheme.cs                  # Main theme model
│       ├── ThemeColors.cs
│       ├── ThemeTypography.cs
│       ├── ThemeSpacing.cs
│       ├── ThemeBorders.cs
│       ├── ThemeShadows.cs
│       ├── ThemeHeader.cs
│       ├── ThemeBackground.cs
│       ├── ThemeComponentStyles.cs
│       ├── ThemeAccessibility.cs
│       └── ThemePresets.cs               # Static preset definitions
│
├── Services/
│   └── Theming/                          # NEW FOLDER
│       ├── IThemeEditorStateService.cs
│       ├── ThemeEditorStateService.cs
│       ├── IThemePersistenceService.cs
│       ├── ThemePersistenceService.cs
│       ├── IThemeCssGeneratorService.cs
│       ├── ThemeCssGeneratorService.cs
│       ├── IThemeImportExportService.cs
│       ├── ThemeImportExportService.cs
│       ├── IThemePresetService.cs
│       ├── ThemePresetService.cs
│       ├── IThemeValidatorService.cs
│       └── ThemeValidatorService.cs
│
├── Data/
│   └── Entities/
│       └── ThemeEntity.cs                # DB entity
│
└── wwwroot/
    └── css/
        └── theming/                      # NEW FOLDER
            ├── theme-editor.css          # Editor styles
            ├── theme-preview.css         # Preview styles
            ├── form-themed.css           # Themed form base styles
            └── presets/                  # Optional: preset CSS files
                ├── default.css
                ├── corporate.css
                └── ...
```

---

## Next Document

Continue to **Part 4: Implementation Plan & Testing Strategy** for step-by-step implementation guidance and testing approaches.
