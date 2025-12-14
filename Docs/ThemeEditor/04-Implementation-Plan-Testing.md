# Theme Editor Specification - Part 4
# Implementation Plan & Testing Strategy

**Document Version:** 2.0
**Date:** December 2025

---

## 1. Implementation Plan

### 1.1 Phase 1 Implementation - Foundation (3-4 Weeks)

#### Week 1: Core Infrastructure

**Day 1-2: Data Models**
```
Tasks:
├── Create Models/Theming/ folder
├── Implement FormTheme.cs with all sub-models
│   ├── ThemeColors.cs
│   ├── ThemeTypography.cs
│   ├── ThemeSpacing.cs
│   ├── ThemeBorders.cs
│   ├── ThemeShadows.cs
│   ├── ThemeHeader.cs (basic)
│   ├── ThemeBackground.cs (basic)
│   └── ThemeAccessibility.cs
├── Implement ThemePresets.cs with 10 presets
│   ├── Default
│   ├── Corporate
│   ├── Modern
│   ├── Government Federal
│   ├── Government Canada
│   ├── High Contrast
│   ├── Dark
│   ├── Dark Modern
│   ├── Healthcare
│   └── Financial
└── Add JSON serialization attributes
```

**Day 3-4: Services Layer**
```
Tasks:
├── Create Services/Theming/ folder
├── Implement IThemeCssGeneratorService
│   ├── GenerateCssVariables()
│   ├── GenerateStylesheet()
│   └── GenerateMinifiedCss()
├── Implement IThemePresetService
│   ├── GetAllPresets()
│   ├── GetPreset()
│   └── GetPresetsByCategory()
├── Implement IThemeEditorStateService (basic)
│   ├── CurrentTheme property
│   ├── LoadTheme()
│   ├── UpdateProperty()
│   └── OnThemeChanged event
└── Register services in Program.cs
```

**Day 5: Database & Persistence**
```
Tasks:
├── Create Themes table migration
├── Create ThemeEntity class
├── Implement IThemePersistenceService
│   ├── SaveThemeAsync()
│   ├── GetThemeAsync()
│   ├── ListThemesAsync()
│   └── DeleteThemeAsync()
└── Test persistence operations
```

#### Week 2: Core Components

**Day 1-2: ThemeScope Component**
```
Tasks:
├── Create Components/Theming/ folder
├── Implement ThemeScope.razor
│   ├── CSS variable injection
│   ├── Theme mode support (light/dark)
│   └── Logo/header support (basic)
├── Create theme-scope.css base styles
├── Create form-themed.css with themed form styles
└── Test with sample form
```

**Day 3-4: Basic Editor Controls**
```
Tasks:
├── Create Controls/ subfolder
├── Implement ColorPicker.razor
│   ├── Native color input
│   ├── Hex text input
│   ├── Color presets
│   └── Validation
├── Implement FontSelector.razor
│   ├── Font family dropdown
│   ├── System fonts
│   └── Google Fonts (common)
├── Implement SliderControl.razor
│   ├── Min/max/step
│   ├── Value display
│   └── Unit suffix
└── Implement SectionPanel.razor (collapsible)
```

**Day 5: Editor Sections**
```
Tasks:
├── Create Sections/ subfolder
├── Implement ThemePresetSection.razor
│   ├── Preset grid
│   ├── Preset cards
│   └── Selection handler
├── Implement ColorsSection.razor
│   ├── Primary color
│   ├── Secondary color
│   ├── Background color
│   └── Text color
├── Implement TypographySection.razor
│   ├── Font family
│   └── Base font size
└── Implement BordersSection.razor
    └── Border radius slider
```

#### Week 3: Editor Page & Integration

**Day 1-2: Editor Layout**
```
Tasks:
├── Implement ThemeEditorPage.razor
│   ├── Route: /admin/theme-editor
│   ├── Split pane layout
│   ├── Sidebar with sections
│   └── Preview panel
├── Implement ThemeEditorHeader.razor
│   ├── Theme name input
│   ├── Preset selector dropdown
│   ├── Mode toggle (Light/Dark)
│   └── Save button
├── Implement ThemeEditorSidebar.razor
│   ├── Section list
│   └── Scroll behavior
└── Add editor CSS files
```

**Day 3-4: Preview Panel**
```
Tasks:
├── Implement ThemePreviewPanel.razor
│   ├── ThemeScope wrapper
│   ├── Sample form with various fields
│   ├── Device frame (optional)
│   └── Zoom controls (optional)
├── Create sample preview form
│   ├── Text inputs
│   ├── Dropdowns
│   ├── Checkboxes/Radios
│   ├── Buttons
│   └── Error states
└── Wire up live preview updates
```

**Day 5: Dark Mode & Mode Toggle**
```
Tasks:
├── Add dark mode CSS variables
├── Implement mode toggle in header
├── Update ThemeScope for mode support
├── Create dark versions of presets
└── Test mode switching
```

#### Week 4: Import/Export & Polish

**Day 1-2: Import/Export**
```
Tasks:
├── Implement IThemeImportExportService
│   ├── ExportToJson()
│   ├── ImportFromJson()
│   └── GetCssVariablesForClipboard()
├── Implement ExportThemeModal.razor
│   ├── JSON download
│   ├── CSS copy to clipboard
│   └── Preview JSON
├── Implement ImportThemeModal.razor
│   ├── File upload
│   ├── JSON paste
│   └── Validation display
└── Add export/import buttons to header
```

**Day 3-4: Integration & Polish**
```
Tasks:
├── Integrate with FormPreview.razor
│   ├── Load form's theme
│   └── Apply ThemeScope wrapper
├── Integrate with RenderedForm.razor
│   ├── Consume theme variables
│   └── Update CSS classes
├── Add navigation to theme editor
├── Add loading states
├── Add error handling
└── Add toast notifications
```

**Day 5: Testing & Documentation**
```
Tasks:
├── Manual testing of all features
├── Fix bugs discovered
├── Write basic usage documentation
└── Code review and cleanup
```

---

### 1.2 Phase 2 Implementation - Enhanced (3-4 Weeks)

#### Week 1: Undo/Redo System

**Day 1-2: Undo/Redo Implementation**
```
Tasks:
├── Extend IThemeEditorStateService
│   ├── Add UndoStack
│   ├── Add RedoStack
│   ├── CanUndo/CanRedo properties
│   ├── Undo() method
│   ├── Redo() method
│   └── ClearHistory() method
├── Implement state snapshot system
├── Limit history to 50 states
└── Add keyboard shortcuts (Ctrl+Z, Ctrl+Y)
```

**Day 3-4: Basic/Advanced Mode**
```
Tasks:
├── Add IsAdvancedMode to state service
├── Add mode toggle button to header
├── Update all sections for mode support
│   ├── ColorsSection: Show 4 vs 20+ colors
│   ├── TypographySection: Show 2 vs 8+ options
│   ├── SpacingSection: Basic vs full
│   └── etc.
├── Remember mode preference
└── Add transition animations
```

**Day 5: UX Improvements**
```
Tasks:
├── Add Reset to Default button
├── Add section collapse/expand
├── Add tooltips to all controls
├── Add dirty state indicator
└── Add auto-save functionality
```

#### Week 2: Header Customization

**Day 1-2: Header Section**
```
Tasks:
├── Implement HeaderSection.razor
│   ├── Logo upload/URL input
│   ├── Logo position selector
│   ├── Background color picker
│   ├── Padding control
│   └── Enable/disable toggle
├── Implement ImageUpload.razor component
│   ├── File upload
│   ├── URL input
│   ├── Preview thumbnail
│   └── Clear button
└── Update ThemeScope for header
```

**Day 3-4: Advanced Header Options**
```
Tasks:
├── Add background image support
│   ├── Image upload
│   ├── Size (cover/contain)
│   ├── Position selector
│   └── Opacity control
├── Add overlay support
│   ├── Color picker
│   ├── Opacity slider
│   └── Enable toggle
├── Add layout options
│   ├── Content alignment
│   ├── Vertical alignment
│   ├── Full width toggle
│   └── Height control
└── Update preview
```

**Day 5: Background Customization**
```
Tasks:
├── Implement BackgroundSection.razor
│   ├── Background type (color/image)
│   ├── Color picker
│   ├── Image upload/URL
│   ├── Image controls (size, position, repeat)
│   └── Attachment (scroll/fixed)
├── Update ThemeScope for backgrounds
└── Test on preview
```

#### Week 3: Shadows & Additional Controls

**Day 1-2: Shadow Controls**
```
Tasks:
├── Implement ShadowsSection.razor
│   ├── Shadow preset selector (None/Small/Medium/Large/Heavy)
│   ├── Card shadow preview
│   └── [Advanced] Custom shadow editor
├── Implement ShadowPicker.razor
│   ├── X/Y offset sliders
│   ├── Blur slider
│   ├── Spread slider
│   ├── Color picker
│   └── Preview box
└── Update form styles for shadows
```

**Day 3-4: Spacing & State Colors**
```
Tasks:
├── Implement SpacingSection.razor
│   ├── Form padding
│   ├── Section spacing
│   ├── Question spacing
│   ├── Input padding
│   └── Label spacing
├── Add state colors to ColorsSection
│   ├── Error color
│   ├── Success color
│   ├── Warning color
│   └── Info color
└── Update themed form styles
```

**Day 5: Border Enhancements**
```
Tasks:
├── Expand BordersSection.razor
│   ├── Border width control
│   ├── Border style selector
│   ├── Focus ring width
│   ├── Focus ring offset
│   └── Multiple radius controls
├── Update form input styles
└── Test focus states
```

#### Week 4: Testing & Refinement

**Day 1-3: Comprehensive Testing**
```
Tasks:
├── Test all new features
├── Test undo/redo with complex workflows
├── Test header customization
├── Test background images
├── Test shadow controls
├── Fix discovered bugs
└── Performance optimization
```

**Day 4-5: Documentation & Cleanup**
```
Tasks:
├── Update documentation
├── Code review
├── Refactor if needed
├── Add inline comments
└── Prepare for Phase 3
```

---

### 1.3 Phase 3 Implementation - Advanced (4-5 Weeks)

#### Week 1-2: Component-Level Styling

**Component Styling Implementation**
```
Tasks:
├── Implement ComponentsSection.razor (container)
├── Button Styles
│   ├── ButtonStyleEditor.razor
│   ├── Primary button colors
│   ├── Secondary button colors
│   ├── Button radius
│   ├── Button padding
│   └── Hover states
├── Input Styles
│   ├── InputStyleEditor.razor
│   ├── Input background
│   ├── Input border color
│   ├── Input focus styles
│   └── Placeholder color
├── Dropdown Styles
│   ├── DropdownStyleEditor.razor
│   ├── Dropdown background
│   ├── Arrow icon color
│   └── Option hover
├── Checkbox/Radio Styles
│   ├── CheckboxStyleEditor.razor
│   ├── Check mark color
│   ├── Box size
│   └── Border radius
├── Panel Styles
│   ├── PanelStyleEditor.razor
│   ├── Panel background
│   ├── Panel border
│   └── Panel shadow
└── Update CSS generator for component styles
```

#### Week 3: Accessibility Features

**Accessibility Implementation**
```
Tasks:
├── Add accessibility section to editor
├── Scale Factor Control
│   ├── Slider 80%-150%
│   ├── Preview at different scales
│   └── Persist setting
├── Focus Indicator Options
│   ├── Ring style
│   ├── Outline style
│   ├── Underline style
│   └── Custom color
├── Contrast Checker
│   ├── Implement IThemeValidatorService
│   ├── WCAG AA calculation
│   ├── WCAG AAA calculation
│   ├── Inline warnings
│   └── Suggestions
├── Additional Options
│   ├── Reduce motion toggle
│   ├── Min font size
│   └── High contrast mode
└── Add accessibility report
```

#### Week 4: Theme Management

**Theme Library Implementation**
```
Tasks:
├── Implement ThemeLibraryModal.razor
│   ├── Theme grid/list view
│   ├── Search/filter
│   ├── Sort options
│   ├── Delete confirmation
│   └── Duplicate action
├── Theme Management Features
│   ├── Rename theme
│   ├── Update description
│   ├── Set as default
│   └── Theme categories
├── Enterprise Features (optional)
│   ├── Theme locking
│   ├── Organization assignment
│   └── Approval workflow (design only)
└── Database updates for management
```

#### Week 5: Polish & Enterprise

**Final Features & Polish**
```
Tasks:
├── Preview Enhancements
│   ├── Multiple preview forms
│   ├── Device preview modes
│   └── Print preview (optional)
├── Advanced Backgrounds
│   ├── Gradient editor (optional)
│   └── Opacity layers
├── Final Testing
│   ├── Full regression testing
│   ├── Performance testing
│   ├── Accessibility testing
│   └── Cross-browser testing
├── Documentation
│   ├── User guide
│   ├── API documentation
│   └── Migration guide
└── Release Preparation
```

---

## 2. Testing Strategy

### 2.1 Unit Testing

#### Service Tests

```csharp
namespace VisualEditorOpus.Tests.Services.Theming
{
    public class ThemeCssGeneratorServiceTests
    {
        private readonly ThemeCssGeneratorService _service;

        public ThemeCssGeneratorServiceTests()
        {
            _service = new ThemeCssGeneratorService();
        }

        [Fact]
        public void GenerateCssVariables_DefaultTheme_ContainsAllVariables()
        {
            // Arrange
            var theme = new FormTheme();

            // Act
            var css = _service.GenerateCssVariables(theme);

            // Assert
            Assert.Contains("--df-primary:", css);
            Assert.Contains("--df-bg:", css);
            Assert.Contains("--df-text:", css);
            Assert.Contains("--df-font:", css);
            Assert.Contains("--df-radius-md:", css);
        }

        [Fact]
        public void GenerateCssVariables_CustomPrimaryColor_IncludesColor()
        {
            // Arrange
            var theme = new FormTheme
            {
                Colors = new ThemeColors { Primary = "#FF5733" }
            };

            // Act
            var css = _service.GenerateCssVariables(theme);

            // Assert
            Assert.Contains("--df-primary: #FF5733", css);
        }

        [Fact]
        public void GenerateStylesheet_ValidTheme_ContainsSelectorAndVariables()
        {
            // Arrange
            var theme = new FormTheme();

            // Act
            var css = _service.GenerateStylesheet(theme);

            // Assert
            Assert.Contains(".df-theme-scope", css);
            Assert.Contains(".df-form", css);
            Assert.Contains(".df-btn", css);
        }

        [Theory]
        [InlineData("--df-primary", "#6366F1")]
        [InlineData("--df-bg", "#FFFFFF")]
        public void GetVariable_ExistingVariable_ReturnsCorrectValue(
            string varName, string expected)
        {
            // Arrange
            var theme = new FormTheme();

            // Act
            var value = _service.GetVariable(theme, varName);

            // Assert
            Assert.Equal(expected, value);
        }
    }

    public class ThemePresetServiceTests
    {
        private readonly ThemePresetService _service;

        public ThemePresetServiceTests()
        {
            _service = new ThemePresetService();
        }

        [Fact]
        public void GetAllPresets_ReturnsAtLeast10Presets()
        {
            // Act
            var presets = _service.GetAllPresets();

            // Assert
            Assert.True(presets.Count >= 10);
        }

        [Fact]
        public void GetPreset_ValidId_ReturnsTheme()
        {
            // Act
            var theme = _service.GetPreset("default");

            // Assert
            Assert.NotNull(theme);
            Assert.Equal("Default", theme.Name);
        }

        [Fact]
        public void GetPreset_InvalidId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<KeyNotFoundException>(
                () => _service.GetPreset("nonexistent"));
        }

        [Theory]
        [InlineData("Government")]
        [InlineData("Dark")]
        public void GetPresetsByCategory_ValidCategory_ReturnsPresets(string category)
        {
            // Act
            var presets = _service.GetPresetsByCategory(category);

            // Assert
            Assert.NotEmpty(presets);
            Assert.All(presets, p => Assert.Equal(category, p.Category));
        }
    }

    public class ThemeValidatorServiceTests
    {
        private readonly ThemeValidatorService _service;

        public ThemeValidatorServiceTests()
        {
            _service = new ThemeValidatorService();
        }

        [Theory]
        [InlineData("#000000", "#FFFFFF", true)]  // High contrast - passes
        [InlineData("#777777", "#888888", false)] // Low contrast - fails
        public void IsWcagAACompliant_VariousContrasts_ReturnsExpected(
            string foreground, string background, bool expected)
        {
            // Act
            var result = _service.IsWcagAACompliant(foreground, background);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CheckContrast_BlackOnWhite_ReturnsHighRatio()
        {
            // Act
            var result = _service.CheckContrast("#000000", "#FFFFFF");

            // Assert
            Assert.True(result.Ratio >= 21.0); // Max contrast
            Assert.True(result.PassesAA);
            Assert.True(result.PassesAAA);
        }

        [Fact]
        public void Validate_ValidTheme_ReturnsSuccess()
        {
            // Arrange
            var theme = new FormTheme(); // Defaults should be valid

            // Act
            var result = _service.Validate(theme);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Issues.Where(i =>
                i.Severity == ThemeValidationSeverity.Error));
        }
    }

    public class ThemeImportExportServiceTests
    {
        private readonly ThemeImportExportService _service;

        public ThemeImportExportServiceTests()
        {
            _service = new ThemeImportExportService();
        }

        [Fact]
        public void ExportToJson_ValidTheme_ProducesValidJson()
        {
            // Arrange
            var theme = new FormTheme { Name = "Test Theme" };

            // Act
            var json = _service.ExportToJson(theme);

            // Assert
            Assert.NotEmpty(json);
            var deserialized = JsonSerializer.Deserialize<FormTheme>(json);
            Assert.NotNull(deserialized);
            Assert.Equal("Test Theme", deserialized.Name);
        }

        [Fact]
        public void ImportFromJson_ValidJson_ReturnsTheme()
        {
            // Arrange
            var theme = new FormTheme { Name = "Import Test" };
            var json = JsonSerializer.Serialize(theme);

            // Act
            var result = _service.ImportFromJson(json);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Theme);
            Assert.Equal("Import Test", result.Theme.Name);
        }

        [Fact]
        public void ImportFromJson_InvalidJson_ReturnsErrors()
        {
            // Act
            var result = _service.ImportFromJson("{ invalid json }");

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Null(result.Theme);
        }

        [Fact]
        public void RoundTrip_ExportThenImport_PreservesAllProperties()
        {
            // Arrange
            var original = new FormTheme
            {
                Name = "Round Trip Test",
                Colors = new ThemeColors { Primary = "#123456" },
                Typography = new ThemeTypography { BaseFontSize = "18px" },
                Mode = ThemeMode.Dark
            };

            // Act
            var json = _service.ExportToJson(original);
            var result = _service.ImportFromJson(json);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(original.Name, result.Theme!.Name);
            Assert.Equal(original.Colors.Primary, result.Theme.Colors.Primary);
            Assert.Equal(original.Typography.BaseFontSize, result.Theme.Typography.BaseFontSize);
            Assert.Equal(original.Mode, result.Theme.Mode);
        }
    }
}
```

### 2.2 Integration Tests

```csharp
namespace VisualEditorOpus.Tests.Integration.Theming
{
    public class ThemePersistenceIntegrationTests : IClassFixture<DatabaseFixture>
    {
        private readonly IThemePersistenceService _service;
        private readonly ApplicationDbContext _context;

        public ThemePersistenceIntegrationTests(DatabaseFixture fixture)
        {
            _context = fixture.Context;
            _service = new ThemePersistenceService(_context);
        }

        [Fact]
        public async Task SaveAndLoad_NewTheme_PersistsCorrectly()
        {
            // Arrange
            var theme = new FormTheme
            {
                Name = "Integration Test Theme",
                Colors = new ThemeColors { Primary = "#ABCDEF" }
            };

            // Act
            var id = await _service.SaveThemeAsync(theme);
            var loaded = await _service.GetThemeAsync(id);

            // Assert
            Assert.NotNull(loaded);
            Assert.Equal(theme.Name, loaded.Name);
            Assert.Equal(theme.Colors.Primary, loaded.Colors.Primary);
        }

        [Fact]
        public async Task ListThemes_MultipleThemes_ReturnsAll()
        {
            // Arrange
            await _service.SaveThemeAsync(new FormTheme { Name = "Theme 1" });
            await _service.SaveThemeAsync(new FormTheme { Name = "Theme 2" });

            // Act
            var themes = await _service.ListThemesAsync();

            // Assert
            Assert.True(themes.Count >= 2);
        }

        [Fact]
        public async Task DeleteTheme_ExistingTheme_RemovesFromDatabase()
        {
            // Arrange
            var theme = new FormTheme { Name = "To Delete" };
            var id = await _service.SaveThemeAsync(theme);

            // Act
            await _service.DeleteThemeAsync(id);
            var loaded = await _service.GetThemeAsync(id);

            // Assert
            Assert.Null(loaded);
        }
    }
}
```

### 2.3 Component Tests (bUnit)

```csharp
namespace VisualEditorOpus.Tests.Components.Theming
{
    public class ColorPickerTests : TestContext
    {
        [Fact]
        public void ColorPicker_InitialValue_DisplaysCorrectly()
        {
            // Act
            var cut = RenderComponent<ColorPicker>(parameters => parameters
                .Add(p => p.Value, "#FF0000")
                .Add(p => p.Label, "Test Color"));

            // Assert
            var swatch = cut.Find(".df-color-swatch");
            Assert.Contains("background-color: #FF0000", swatch.GetAttribute("style"));

            var textInput = cut.Find(".df-color-text-input");
            Assert.Equal("#FF0000", textInput.GetAttribute("value"));
        }

        [Fact]
        public void ColorPicker_TextInput_TriggersValueChanged()
        {
            // Arrange
            string? changedValue = null;
            var cut = RenderComponent<ColorPicker>(parameters => parameters
                .Add(p => p.Value, "#000000")
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(
                    this, v => changedValue = v)));

            // Act
            var textInput = cut.Find(".df-color-text-input");
            textInput.Input("#123ABC");

            // Assert
            Assert.Equal("#123ABC", changedValue);
        }

        [Fact]
        public void ColorPicker_PresetClick_SelectsPreset()
        {
            // Arrange
            string? changedValue = null;
            var cut = RenderComponent<ColorPicker>(parameters => parameters
                .Add(p => p.Value, "#000000")
                .Add(p => p.ShowPresets, true)
                .Add(p => p.Presets, new[] { "#FF0000", "#00FF00" })
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(
                    this, v => changedValue = v)));

            // Act
            var firstPreset = cut.FindAll(".df-color-preset")[0];
            firstPreset.Click();

            // Assert
            Assert.Equal("#FF0000", changedValue);
        }
    }

    public class ThemeScopeTests : TestContext
    {
        [Fact]
        public void ThemeScope_AppliesVariables_ToStyleAttribute()
        {
            // Arrange
            var theme = new FormTheme
            {
                Colors = new ThemeColors { Primary = "#123456" }
            };

            Services.AddSingleton<IThemeCssGeneratorService, ThemeCssGeneratorService>();

            // Act
            var cut = RenderComponent<ThemeScope>(parameters => parameters
                .Add(p => p.Theme, theme)
                .AddChildContent("<p>Test content</p>"));

            // Assert
            var scope = cut.Find(".df-theme-scope");
            var style = scope.GetAttribute("style");
            Assert.Contains("--df-primary: #123456", style);
        }

        [Fact]
        public void ThemeScope_DarkMode_SetsDataAttribute()
        {
            // Arrange
            var theme = new FormTheme { Mode = ThemeMode.Dark };
            Services.AddSingleton<IThemeCssGeneratorService, ThemeCssGeneratorService>();

            // Act
            var cut = RenderComponent<ThemeScope>(parameters => parameters
                .Add(p => p.Theme, theme));

            // Assert
            var scope = cut.Find(".df-theme-scope");
            Assert.Equal("dark", scope.GetAttribute("data-theme"));
        }

        [Fact]
        public void ThemeScope_WithLogo_RendersHeader()
        {
            // Arrange
            var theme = new FormTheme
            {
                Header = new ThemeHeader
                {
                    Enabled = true,
                    LogoUrl = "https://example.com/logo.png"
                }
            };
            Services.AddSingleton<IThemeCssGeneratorService, ThemeCssGeneratorService>();

            // Act
            var cut = RenderComponent<ThemeScope>(parameters => parameters
                .Add(p => p.Theme, theme));

            // Assert
            var logo = cut.Find(".df-logo");
            Assert.Equal("https://example.com/logo.png", logo.GetAttribute("src"));
        }
    }

    public class ThemeEditorStateTests : TestContext
    {
        [Fact]
        public void LoadTheme_SetsCurrentTheme()
        {
            // Arrange
            var service = new ThemeEditorStateService();
            var theme = new FormTheme { Name = "Test" };

            // Act
            service.LoadTheme(theme);

            // Assert
            Assert.Equal("Test", service.CurrentTheme.Name);
        }

        [Fact]
        public void UpdateProperty_SetsIsDirty()
        {
            // Arrange
            var service = new ThemeEditorStateService();
            service.LoadTheme(new FormTheme());

            // Act
            service.UpdateColors(c => c.Primary = "#999999");

            // Assert
            Assert.True(service.IsDirty);
        }

        [Fact]
        public void Undo_RevertsLastChange()
        {
            // Arrange
            var service = new ThemeEditorStateService();
            service.LoadTheme(new FormTheme { Colors = new ThemeColors { Primary = "#111111" } });
            service.UpdateColors(c => c.Primary = "#222222");

            // Act
            service.Undo();

            // Assert
            Assert.Equal("#111111", service.CurrentTheme.Colors.Primary);
        }

        [Fact]
        public void Redo_ReappliesUndoneChange()
        {
            // Arrange
            var service = new ThemeEditorStateService();
            service.LoadTheme(new FormTheme { Colors = new ThemeColors { Primary = "#111111" } });
            service.UpdateColors(c => c.Primary = "#222222");
            service.Undo();

            // Act
            service.Redo();

            // Assert
            Assert.Equal("#222222", service.CurrentTheme.Colors.Primary);
        }
    }
}
```

### 2.4 End-to-End Test Scenarios

```gherkin
Feature: Theme Editor

  Scenario: Create a new custom theme
    Given I am on the theme editor page
    When I select the "Modern" preset
    And I change the primary color to "#FF5733"
    And I change the font family to "Helvetica"
    And I set the border radius to "12px"
    Then the preview should update in real-time
    And the preview primary color should be "#FF5733"
    When I click "Save Theme"
    And I enter the name "My Custom Theme"
    And I click "Save"
    Then I should see a success notification
    And "My Custom Theme" should appear in the theme library

  Scenario: Import theme from JSON
    Given I am on the theme editor page
    When I click "Import"
    And I upload a valid theme JSON file
    Then the theme settings should be applied
    And the preview should reflect the imported theme
    And I should see "Theme imported successfully"

  Scenario: Export theme to JSON
    Given I have customized a theme
    When I click "Export"
    And I select "JSON" format
    Then a JSON file should be downloaded
    And the file should contain all theme settings

  Scenario: Undo/Redo changes
    Given I am editing a theme
    When I change the primary color to "#FF0000"
    And I change the primary color to "#00FF00"
    And I press Ctrl+Z
    Then the primary color should be "#FF0000"
    When I press Ctrl+Y
    Then the primary color should be "#00FF00"

  Scenario: Toggle dark mode
    Given I am on the theme editor page
    When I toggle the "Dark Mode" switch
    Then the preview should display in dark mode
    And the background should be dark
    And the text should be light

  Scenario: Accessibility contrast warning
    Given I am editing a theme in advanced mode
    When I set the text color to "#AAAAAA"
    And I set the background color to "#BBBBBB"
    Then I should see a contrast warning
    And the warning should suggest "Increase contrast for WCAG compliance"
```

### 2.5 Test Coverage Targets

| Category | Target Coverage |
|----------|-----------------|
| Services | 90% |
| Models | 85% |
| Components (unit) | 80% |
| Integration | 70% |
| E2E critical paths | 100% |

---

## 3. Performance Considerations

### 3.1 CSS Variable Injection

**Optimization Strategy:**
```csharp
// Cache generated CSS variables
public class ThemeCssGeneratorService : IThemeCssGeneratorService
{
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public string GenerateCssVariables(FormTheme theme)
    {
        var cacheKey = ComputeThemeHash(theme);

        return _cache.GetOrAdd(cacheKey, _ => GenerateCssVariablesInternal(theme));
    }

    private string ComputeThemeHash(FormTheme theme)
    {
        var json = JsonSerializer.Serialize(theme);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(hash);
    }
}
```

### 3.2 Live Preview Performance

**Debounce Updates:**
```csharp
public class ThemeEditorStateService : IThemeEditorStateService
{
    private Timer? _debounceTimer;
    private const int DebounceMs = 50; // 50ms debounce for preview updates

    public void UpdateProperty<T>(Expression<Func<FormTheme, T>> property, T value)
    {
        // Apply change immediately to state
        ApplyPropertyChange(property, value);

        // Debounce the preview update event
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            InvokeAsync(() => OnThemeChanged?.Invoke());
        }, null, DebounceMs, Timeout.Infinite);
    }
}
```

### 3.3 Image Loading

**Lazy Load Background Images:**
```razor
@if (Theme.Background.Type == "image" && !string.IsNullOrEmpty(Theme.Background.Image))
{
    <div class="df-background-image"
         style="background-image: url('@Theme.Background.Image');"
         loading="lazy">
    </div>
}
```

---

## 4. Security Considerations

### 4.1 Input Validation

```csharp
public class ThemeValidatorService : IThemeValidatorService
{
    public ThemeValidationResult Validate(FormTheme theme)
    {
        var issues = new List<ThemeValidationIssue>();

        // Validate color formats
        ValidateColor(theme.Colors.Primary, "Primary Color", issues);
        ValidateColor(theme.Colors.Background, "Background Color", issues);
        // ... more color validations

        // Validate URLs (prevent XSS)
        ValidateUrl(theme.Header.LogoUrl, "Logo URL", issues);
        ValidateUrl(theme.Background.Image, "Background Image", issues);

        // Validate CSS values (prevent injection)
        ValidateCssValue(theme.Typography.FontFamily, "Font Family", issues);
        ValidateCssValue(theme.Borders.BorderWidth, "Border Width", issues);

        return new ThemeValidationResult(
            !issues.Any(i => i.Severity == ThemeValidationSeverity.Error),
            issues);
    }

    private void ValidateColor(string color, string propertyName,
        List<ThemeValidationIssue> issues)
    {
        if (string.IsNullOrEmpty(color)) return;

        // Only allow valid hex colors or CSS color names
        var hexPattern = @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
        var rgbaPattern = @"^rgba?\(\s*\d+\s*,\s*\d+\s*,\s*\d+\s*(,\s*[\d.]+\s*)?\)$";

        if (!Regex.IsMatch(color, hexPattern) && !Regex.IsMatch(color, rgbaPattern))
        {
            issues.Add(new ThemeValidationIssue(
                ThemeValidationSeverity.Error,
                propertyName,
                $"Invalid color format: {color}",
                "Use hex format (#RRGGBB) or rgba()"));
        }
    }

    private void ValidateUrl(string url, string propertyName,
        List<ThemeValidationIssue> issues)
    {
        if (string.IsNullOrEmpty(url)) return;

        // Only allow http(s) and data URIs
        if (!url.StartsWith("https://") &&
            !url.StartsWith("http://") &&
            !url.StartsWith("data:image/"))
        {
            issues.Add(new ThemeValidationIssue(
                ThemeValidationSeverity.Error,
                propertyName,
                "Invalid URL scheme",
                "Use https://, http://, or data:image/ URLs"));
        }

        // Check for script injection attempts
        if (url.Contains("javascript:") || url.Contains("<script"))
        {
            issues.Add(new ThemeValidationIssue(
                ThemeValidationSeverity.Error,
                propertyName,
                "Potential XSS detected",
                "Remove script content from URL"));
        }
    }

    private void ValidateCssValue(string value, string propertyName,
        List<ThemeValidationIssue> issues)
    {
        if (string.IsNullOrEmpty(value)) return;

        // Check for CSS injection
        var dangerousPatterns = new[] { "expression(", "url(", "import", "@" };
        foreach (var pattern in dangerousPatterns)
        {
            if (value.ToLower().Contains(pattern))
            {
                issues.Add(new ThemeValidationIssue(
                    ThemeValidationSeverity.Error,
                    propertyName,
                    $"Potentially dangerous CSS: {pattern}",
                    "Remove dynamic CSS expressions"));
            }
        }
    }
}
```

### 4.2 Content Security Policy

When serving themes, ensure CSP headers allow inline styles:
```csharp
// In Program.cs or middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' https: data:;");
    await next();
});
```

---

## Next Document

Continue to **Part 5: AI Prompts for Implementation** for ready-to-use Claude prompts to assist with implementation.
