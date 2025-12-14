# Theme Editor Specification - Part 5
# AI Prompts for Implementation

**Document Version:** 2.0
**Date:** December 2025

---

## 1. Overview

This document provides ready-to-use prompts for Claude to assist with implementing the Theme Editor. Each prompt is designed to be self-contained and produce working code.

**Usage Instructions:**
1. Copy the prompt for the feature you want to implement
2. Paste it into Claude Code
3. Let Claude implement the feature
4. Review and test the generated code
5. Move to the next prompt

---

## 2. Phase 1 Prompts

### Prompt 1.1: Create Theme Data Models

```
I'm implementing a Theme Editor for my Blazor form builder application (Visual Editor Opus).
I need you to create the complete theme data models.

Context:
- Project location: C:\Users\waelm\source\repos\waelgadalla\NewDynamicsFormGemini
- Target folder: Src/VisualEditorOpus/Models/Theming/
- Framework: .NET 9, Blazor Server
- The theme will be serialized to JSON and stored in SQL Server

Please create these model files:

1. FormTheme.cs - Main theme class containing:
   - Metadata (Id, Name, Description, BasePreset, CreatedAt, ModifiedAt, Version)
   - ThemeMode enum (Light, Dark, Auto)
   - IsPanelless bool
   - References to sub-models below

2. ThemeColors.cs - Complete color palette:
   - Primary, PrimaryHover, PrimaryForeground
   - Secondary colors
   - Background, BackgroundDim, Surface colors
   - Text colors (Primary, Secondary, Disabled, Placeholder)
   - Border colors (Default, Hover, Focus)
   - State colors (Error, Success, Warning, Info with backgrounds)
   - FocusRing, Selection

3. ThemeTypography.cs:
   - Font families (main, heading, mono)
   - Base font size, line height
   - Title/section/question/description sizes
   - Font weights

4. ThemeSpacing.cs:
   - Base unit, form padding
   - Section/question/label/option spacing
   - Input padding

5. ThemeBorders.cs:
   - Border width, style
   - Radius (small, medium, large, xlarge)
   - Focus ring width/offset

6. ThemeShadows.cs:
   - Shadow levels (none, small, medium, large, xlarge)
   - Component shadows (card, dropdown, modal, focus)

7. ThemeHeader.cs:
   - Enabled, Logo settings
   - Background (color, image, gradient)
   - Overlay settings
   - Layout (height, padding, alignment)

8. ThemeBackground.cs:
   - Type (color, image, gradient)
   - Image settings (URL, size, position, repeat, attachment, opacity)

9. ThemeAccessibility.cs:
   - Scale factor, min font size
   - High contrast mode, reduce motion
   - Focus indicator style

All models should:
- Have sensible default values (professional blue/indigo theme)
- Use System.Text.Json attributes for serialization
- Include XML documentation comments
- Be in the DynamicForms.Models.Theming namespace
```

### Prompt 1.2: Create Theme Presets

```
I need you to create a ThemePresets.cs file with at least 12 pre-built themes for my Blazor Theme Editor.

Context:
- Project: Visual Editor Opus (Blazor form builder)
- Location: Src/VisualEditorOpus/Models/Theming/ThemePresets.cs
- Uses the FormTheme model I already have

Create these presets:

PROFESSIONAL (3):
1. "default" - Clean indigo/blue professional theme
2. "corporate" - Traditional business (navy blue, Segoe UI, sharp corners)
3. "modern" - Contemporary (purple accent, rounded corners, subtle shadows)

GOVERNMENT (3):
4. "government-federal" - U.S. Federal style (USWDS inspired: #005EA2, Public Sans)
5. "government-canada" - Canada.ca style (#26374A, Noto Sans)
6. "government-uk" - UK GOV.UK style (#1D70B8, GDS Transport font)

DARK (2):
7. "dark" - Standard dark theme (slate backgrounds, light text)
8. "dark-modern" - Sleek dark with purple accents

ACCESSIBILITY (1):
9. "high-contrast" - Maximum contrast for accessibility (black/white/blue)

INDUSTRY (3):
10. "healthcare" - Medical (teal primary)
11. "education" - Schools (purple/blue)
12. "financial" - Banking (green primary)

Each preset should include:
- Unique Id and Name
- Description
- Category for grouping
- Appropriate colors, typography, spacing
- Mode (Light/Dark)

Also create:
- ThemePresetInfo record (Id, Name, Description, Category, Mode, PreviewColor, Tags)
- Static method to get all presets
- Static method to get preset by category
```

### Prompt 1.3: Create CSS Generator Service

```
I need you to implement the ThemeCssGeneratorService for my Blazor Theme Editor.

Context:
- Project: Visual Editor Opus
- Location: Src/VisualEditorOpus/Services/Theming/
- Uses FormTheme model from Models/Theming/

Create these files:

1. IThemeCssGeneratorService.cs - Interface with:
   - string GenerateCssVariables(FormTheme theme) - For inline style attribute
   - string GenerateStylesheet(FormTheme theme) - Complete CSS file
   - string GenerateMinifiedCss(FormTheme theme) - Minified version
   - string GetVariable(FormTheme theme, string variableName)

2. ThemeCssGeneratorService.cs - Implementation that:
   - Maps all FormTheme properties to CSS variables with --df- prefix
   - Variable naming convention:
     - Colors: --df-primary, --df-bg, --df-text, --df-error, etc.
     - Typography: --df-font, --df-text-base, --df-font-bold, etc.
     - Spacing: --df-form-padding, --df-question-gap, etc.
     - Borders: --df-radius-md, --df-border-width, etc.
     - Shadows: --df-shadow-sm, --df-shadow-card, etc.

   - GenerateStylesheet should also include:
     - .df-form base styles
     - .df-form-card styles
     - .df-section, .df-question, .df-label styles
     - .df-input, .df-input:hover, .df-input:focus styles
     - .df-btn, .df-btn-primary, .df-btn-secondary styles
     - Error/success state styles

   - Include caching for performance (use ConcurrentDictionary)
   - Handle null/empty values gracefully

Add the service registration to be added to Program.cs.
```

### Prompt 1.4: Create ThemeScope Component

```
I need you to create the ThemeScope Blazor component that wraps content and injects CSS variables.

Context:
- Project: Visual Editor Opus (Blazor Server)
- Location: Src/VisualEditorOpus/Components/Theming/ThemeScope.razor
- Uses IThemeCssGeneratorService and FormTheme model

Create:

1. ThemeScope.razor - Component that:
   - Accepts FormTheme as parameter
   - Accepts RenderFragment ChildContent
   - Accepts optional CssClass parameter
   - Renders a div with class "df-theme-scope"
   - Sets data-theme attribute based on theme.Mode
   - Injects CSS variables via style attribute using IThemeCssGeneratorService
   - Optionally renders header with logo if Theme.Header.Enabled and LogoUrl set
   - Header should support:
     - Background color/image
     - Overlay
     - Content alignment
     - Logo positioning

2. ThemeScope.razor.css - Scoped styles:
   - .df-theme-scope base styles
   - .df-form-header styles
   - .df-header-overlay styles
   - .df-header-content styles
   - .df-logo styles

The component should be reusable and wrap any form content to apply theming.
```

### Prompt 1.5: Create Theme Editor Page

```
I need you to create the main Theme Editor page for my Blazor application.

Context:
- Project: Visual Editor Opus
- Location: Src/VisualEditorOpus/Components/Theming/Editor/
- Follow the existing editor patterns (see Components/Editor/ for reference)
- Use the existing shared components (Button, SelectInput, etc. from Components/Shared/)

Create these files:

1. ThemeEditorPage.razor - Main page:
   - Route: /admin/theme-editor and /admin/theme-editor/{ThemeId}
   - Layout similar to ModuleEditor (header + split pane)
   - Left sidebar (320px) with editing sections
   - Right panel with live preview
   - Load theme from URL parameter or create new

2. ThemeEditorHeader.razor:
   - Theme name input (editable)
   - Preset selector dropdown
   - Light/Dark mode toggle
   - Save button
   - Actions dropdown (Import, Export, Reset)

3. ThemeEditorSidebar.razor:
   - Scrollable container
   - Contains all editing sections
   - Each section is collapsible

4. Create these section components in Sections/ folder:
   - ThemePresetSection.razor - Grid of preset cards to select
   - ColorsSection.razor - Color pickers for primary, secondary, bg, text
   - TypographySection.razor - Font family dropdown, size control
   - BordersSection.razor - Border radius slider

5. ThemePreviewPanel.razor:
   - Uses ThemeScope to wrap preview
   - Contains sample form with various field types
   - Shows form title, text inputs, dropdown, checkboxes, buttons
   - Responsive container

Add necessary CSS files for each component.
Wire up the services (IThemeEditorStateService, IThemePresetService, etc.)
```

### Prompt 1.6: Create Theme Editor State Service

```
I need you to implement the ThemeEditorStateService for managing theme editor state.

Context:
- Project: Visual Editor Opus
- Location: Src/VisualEditorOpus/Services/Theming/
- Follow the pattern of existing EditorStateService

Create:

1. IThemeEditorStateService.cs - Interface:
   - FormTheme CurrentTheme { get; }
   - FormTheme? OriginalTheme { get; }
   - bool IsDirty { get; }
   - bool IsAdvancedMode { get; }

   - void LoadTheme(FormTheme theme)
   - void ApplyPreset(string presetId)
   - void UpdateColors(Action<ThemeColors> update)
   - void UpdateTypography(Action<ThemeTypography> update)
   - void UpdateSpacing(Action<ThemeSpacing> update)
   - void UpdateBorders(Action<ThemeBorders> update)
   - void UpdateShadows(Action<ThemeShadows> update)
   - void UpdateHeader(Action<ThemeHeader> update)
   - void UpdateBackground(Action<ThemeBackground> update)

   - bool CanUndo { get; }
   - bool CanRedo { get; }
   - void Undo()
   - void Redo()
   - void ClearHistory()

   - void SetMode(ThemeMode mode)
   - void ToggleAdvancedMode()
   - void Reset()

   - event Action? OnThemeChanged
   - event Action? OnDirtyStateChanged

2. ThemeEditorStateService.cs - Implementation:
   - Use immutable state pattern (deep clone on changes)
   - Implement undo/redo with stack (limit 50 states)
   - Fire events on changes
   - Support debounced preview updates (50ms)
   - Track dirty state by comparing to original
   - Inject IThemePresetService for preset operations

Register as Scoped service.
```

### Prompt 1.7: Create Import/Export Service

```
I need you to implement theme import/export functionality.

Context:
- Project: Visual Editor Opus
- Location: Src/VisualEditorOpus/Services/Theming/

Create:

1. IThemeImportExportService.cs:
   - string ExportToJson(FormTheme theme, bool prettyPrint = true)
   - byte[] ExportToJsonBytes(FormTheme theme)
   - string ExportToCss(FormTheme theme)
   - ThemeImportResult ImportFromJson(string json)
   - ThemeImportResult ImportFromJson(Stream stream)
   - string GetCssVariablesForClipboard(FormTheme theme)

2. ThemeImportResult.cs record:
   - bool Success
   - FormTheme? Theme
   - IReadOnlyList<string> Errors
   - IReadOnlyList<string> Warnings

3. ThemeImportExportService.cs:
   - Use System.Text.Json for serialization
   - Pretty print with indentation for readability
   - Validate imported JSON structure
   - Handle missing properties gracefully (use defaults)
   - Detect and warn about deprecated properties
   - CSS export should use IThemeCssGeneratorService

4. Create modal components:
   - ImportThemeModal.razor - File upload, paste JSON, validation display
   - ExportThemeModal.razor - Preview, download button, copy to clipboard

The modals should use the existing ModalBase component pattern.
```

### Prompt 1.8: Create Theme Persistence Service

```
I need you to implement database persistence for themes.

Context:
- Project: Visual Editor Opus
- Location: Src/VisualEditorOpus/Services/Theming/ and Src/VisualEditorOpus/Data/
- Use SQL Server (existing connection in appsettings.json)
- Follow existing persistence patterns (see EditorPersistenceService)

Create:

1. SQL Migration script for Themes table:
   - Id (NVARCHAR(50) PK)
   - Name (NVARCHAR(100))
   - Description (NVARCHAR(500))
   - BasePreset (NVARCHAR(50))
   - JsonData (NVARCHAR(MAX)) - Serialized FormTheme
   - PreviewImageUrl (NVARCHAR(500))
   - IsDefault (BIT)
   - IsLocked (BIT)
   - OrganizationId (NVARCHAR(50))
   - CreatedBy (NVARCHAR(100))
   - CreatedAt (DATETIME2)
   - ModifiedAt (DATETIME2)
   - Version (INT)

2. ThemeEntity.cs in Data/Entities/

3. IThemePersistenceService.cs:
   - Task<FormTheme?> GetThemeAsync(string themeId)
   - Task<FormTheme?> GetDefaultThemeAsync()
   - Task<IReadOnlyList<ThemeSummary>> ListThemesAsync()
   - Task<string> SaveThemeAsync(FormTheme theme)
   - Task UpdateThemeAsync(FormTheme theme)
   - Task DeleteThemeAsync(string themeId)
   - Task SetDefaultThemeAsync(string themeId)

4. ThemeSummary.cs record for list operations

5. ThemePersistenceService.cs implementation:
   - Use Dapper or raw ADO.NET (match existing pattern)
   - Serialize/deserialize FormTheme to/from JsonData
   - Handle optimistic concurrency with Version

Register as Scoped service.
```

### Prompt 1.9: Create Color Picker Component

```
I need a professional ColorPicker component for the theme editor.

Context:
- Project: Visual Editor Opus
- Location: Src/VisualEditorOpus/Components/Theming/Controls/

Create ColorPicker.razor with these features:

Parameters:
- string Value (hex color)
- EventCallback<string> ValueChanged
- string Label
- string Tooltip
- bool ShowPresets (default true)
- string[]? Presets (custom preset colors)
- bool Compact (smaller version)
- bool ShowOpacity (optional alpha slider)

UI Elements:
- Color swatch (clickable, shows current color)
- Native HTML5 color input (hidden, triggered by swatch click)
- Text input for hex value with validation
- Optional opacity slider (0-100%)
- Optional preset color buttons

Features:
- Two-way binding with ValueChanged
- Validate hex color format (#RGB or #RRGGBB)
- Default presets if none provided:
  ["#6366F1", "#8B5CF6", "#EC4899", "#EF4444", "#F59E0B",
   "#10B981", "#14B8A6", "#3B82F6", "#1E40AF", "#0F172A"]
- Active state on selected preset
- Tooltip on hover (if provided)

Create ColorPicker.razor.css with styles:
- .df-color-picker container
- .df-color-swatch (24x24px rounded square)
- .df-color-text-input (styled text input)
- .df-color-presets (flex wrap grid)
- .df-color-preset (small buttons)
- Compact variant styles
```

### Prompt 1.10: Integrate Theme with Form Preview

```
I need you to integrate the theme system with the existing form preview.

Context:
- Project: Visual Editor Opus
- Existing preview: Components/Preview/FormPreview.razor and RenderedForm.razor
- Theme system: Components/Theming/ThemeScope.razor

Tasks:

1. Update RenderedForm.razor:
   - Add FormTheme? Theme parameter
   - Wrap content in ThemeScope if theme is provided
   - Update element classes to use df- prefixed classes
   - Ensure all form elements consume CSS variables

2. Update FormPreview.razor:
   - Add theme loading from form's assigned theme
   - Pass theme to RenderedForm
   - Add theme indicator in preview toolbar (optional)

3. Create/update CSS file wwwroot/css/theming/form-themed.css:
   - Base styles for .df-form
   - Styles for all form elements using var(--df-xxx)
   - Support both themed and non-themed modes
   - Ensure backwards compatibility

4. Update the preview sample form to demonstrate:
   - Form title and description
   - Text input with label
   - Required field indicator
   - Dropdown/select
   - Checkbox group
   - Radio button group
   - Date picker
   - Text area
   - Primary and secondary buttons
   - Validation error state
   - Success message

Make sure the integration doesn't break existing preview functionality.
```

---

## 3. Phase 2 Prompts

### Prompt 2.1: Implement Undo/Redo System

```
I need you to enhance the ThemeEditorStateService with a proper undo/redo system.

Context:
- Project: Visual Editor Opus
- Existing service: Services/Theming/ThemeEditorStateService.cs

Implement:

1. Undo/Redo Stack:
   - Use Stack<FormTheme> for undo history
   - Use Stack<FormTheme> for redo history
   - Maximum 50 states in undo stack
   - Clear redo stack on new changes

2. State Capture:
   - Deep clone theme before each change
   - Batch rapid changes (debounce 100ms)
   - Skip duplicate states

3. Methods:
   - PushToUndoStack() - internal
   - Undo() - pop from undo, push current to redo
   - Redo() - pop from redo, push current to undo
   - ClearHistory() - clear both stacks
   - CanUndo/CanRedo properties

4. Keyboard Shortcuts:
   - Create JS interop for Ctrl+Z (undo) and Ctrl+Y/Ctrl+Shift+Z (redo)
   - Register shortcuts when editor mounts
   - Unregister on dispose

5. Update ThemeEditorHeader.razor:
   - Add undo/redo buttons with icons
   - Disable when CanUndo/CanRedo is false
   - Show tooltip with action description

Test scenarios:
- Change color -> undo -> color reverts
- Undo -> redo -> change restored
- Make 60 changes -> only last 50 are undoable
- Undo -> make new change -> redo stack cleared
```

### Prompt 2.2: Implement Basic/Advanced Mode Toggle

```
I need you to implement Basic/Advanced mode switching for the theme editor.

Context:
- Project: Visual Editor Opus
- Theme Editor: Components/Theming/Editor/

Implement:

1. Update IThemeEditorStateService:
   - bool IsAdvancedMode property
   - void ToggleAdvancedMode()
   - void SetAdvancedMode(bool advanced)
   - Persist preference to localStorage

2. Update ThemeEditorHeader.razor:
   - Add toggle switch for Basic/Advanced mode
   - Visual indicator of current mode

3. Update each section component to conditionally show controls:

   ColorsSection.razor:
   - Basic: Primary, Background, Text (3 controls)
   - Advanced: All 20+ color controls grouped by category

   TypographySection.razor:
   - Basic: Font family, Base size (2 controls)
   - Advanced: All fonts, all sizes, all weights (10+ controls)

   SpacingSection.razor:
   - Basic: Form padding only (1 control)
   - Advanced: All spacing controls (6+ controls)

   BordersSection.razor:
   - Basic: Border radius (1 control)
   - Advanced: All border controls (6+ controls)

   ShadowsSection.razor:
   - Basic: Shadow preset dropdown (1 control)
   - Advanced: Individual shadow controls

4. Add smooth transitions between modes
5. Remember scroll position when toggling
6. Group advanced controls with sub-headings
```

### Prompt 2.3: Implement Header Customization Section

```
I need you to create a complete header customization section for the theme editor.

Context:
- Project: Visual Editor Opus
- Location: Components/Theming/Sections/HeaderSection.razor

Implement:

1. HeaderSection.razor with these controls:

   Enable/Disable:
   - Toggle switch to enable header

   Logo Settings:
   - Image upload component (accepts PNG, JPG, SVG)
   - URL input alternative
   - Preview thumbnail
   - Position selector (Left, Center, Right)
   - Max height slider (24px - 80px)
   - Clear/remove button

   Background (Basic):
   - Type selector (Color, Image)
   - Color picker for background color

   Background (Advanced):
   - Gradient option
   - Image upload/URL
   - Image size (Cover, Contain, Auto)
   - Image position (9-point grid selector)

   Overlay (Advanced):
   - Enable toggle
   - Color picker with opacity

   Layout (Advanced):
   - Height (Auto or fixed px)
   - Padding control
   - Content alignment (Left, Center, Right)
   - Vertical alignment (Top, Center, Bottom)
   - Full width toggle

2. Create ImageUpload.razor reusable component:
   - File input (hidden)
   - Drop zone
   - URL input mode
   - Preview with aspect ratio
   - Clear button
   - Size validation
   - Format validation

3. Create PositionSelector.razor (9-point grid):
   - 3x3 grid of buttons
   - Visual indicator of selected position
   - Returns position string (top-left, center, etc.)

4. Update ThemeScope to render the configured header

5. Update preview to show header changes in real-time
```

### Prompt 2.4: Implement Background Customization Section

```
I need you to create the background customization section.

Context:
- Project: Visual Editor Opus
- Location: Components/Theming/Sections/BackgroundSection.razor

Implement:

1. BackgroundSection.razor:

   Type Selector:
   - Radio/segmented control: Color | Image | Gradient

   Color Mode:
   - Color picker for page background

   Image Mode:
   - ImageUpload component (reuse from header)
   - Image size: Cover, Contain, Tile
   - Image position: PositionSelector component
   - Image repeat: No-repeat, Repeat, Repeat-X, Repeat-Y
   - Image attachment: Scroll, Fixed
   - Opacity slider (0-100%)

   Gradient Mode (Advanced only):
   - Gradient type: Linear, Radial
   - Angle slider (for linear)
   - Color stops editor:
     - List of color stops
     - Add/remove stops
     - Position percentage
     - Color picker for each

2. Create GradientEditor.razor:
   - Visual gradient preview bar
   - Draggable color stops
   - Add stop on click
   - Remove stop on double-click or delete
   - Gradient string output (CSS format)

3. Update ThemeScope to apply background:
   - Apply to page wrapper or body
   - Handle all background types
   - Support fixed attachment properly

4. Ensure preview shows background correctly
   - May need to apply to preview container
   - Show background behind form card
```

### Prompt 2.5: Implement Shadow Controls Section

```
I need you to create the shadows customization section.

Context:
- Project: Visual Editor Opus
- Location: Components/Theming/Sections/ShadowsSection.razor

Implement:

1. ShadowsSection.razor:

   Quick Presets (Basic Mode):
   - Visual preset cards: None, Subtle, Medium, Pronounced, Heavy
   - Each shows preview of shadow
   - Click to apply preset values

   Individual Controls (Advanced Mode):

   Card Shadow:
   - X offset slider (-20 to 20px)
   - Y offset slider (-20 to 20px)
   - Blur slider (0 to 50px)
   - Spread slider (-20 to 20px)
   - Color picker with opacity
   - Preview box showing shadow

   Input Focus Shadow:
   - Same controls as above
   - Preview shows input with focus state

   Dropdown Shadow:
   - Same controls
   - Preview shows dropdown menu

   Modal Shadow:
   - Same controls
   - Preview shows modal overlay

2. Create ShadowEditor.razor reusable component:
   - All shadow controls in one component
   - Parameters: Label, Value (shadow string), ValueChanged
   - Real-time preview box
   - Reset to default button

3. Create shadow preview boxes:
   - Show actual shadow on representative element
   - Update in real-time as values change

4. Create shadow preset definitions:
   - None: "none"
   - Subtle: "0 1px 2px rgba(0,0,0,0.05)"
   - Medium: "0 4px 6px rgba(0,0,0,0.1)"
   - Pronounced: "0 10px 15px rgba(0,0,0,0.1)"
   - Heavy: "0 20px 25px rgba(0,0,0,0.15)"
```

---

## 4. Phase 3 Prompts

### Prompt 3.1: Implement Component-Level Styling

```
I need you to create component-level styling for the theme editor.

Context:
- Project: Visual Editor Opus
- Location: Components/Theming/Sections/ComponentsSection.razor

This is for Advanced Mode only. Implement:

1. ComponentsSection.razor - Container with tabs/accordion:
   - Buttons
   - Inputs
   - Dropdowns
   - Checkboxes & Radios
   - Panels
   - Progress Bar
   - Navigation

2. ButtonStyleEditor.razor:
   - Primary Button:
     - Background color
     - Text color
     - Border color
     - Border radius
     - Padding (vertical/horizontal)
     - Hover background
     - Hover text
   - Secondary Button: Same controls
   - Preview showing both buttons

3. InputStyleEditor.razor:
   - Background color
   - Text color
   - Border color
   - Border width
   - Border radius
   - Padding
   - Placeholder color
   - Focus border color
   - Focus shadow
   - Disabled styles
   - Preview showing input states

4. DropdownStyleEditor.razor:
   - Select background
   - Arrow icon color
   - Option hover background
   - Selected option background
   - Border styles
   - Preview with dropdown

5. CheckboxRadioStyleEditor.razor:
   - Box size
   - Border color
   - Checked background
   - Check mark color
   - Border radius (checkbox only)
   - Label spacing
   - Preview with both types

6. PanelStyleEditor.razor:
   - Background color
   - Border color
   - Border radius
   - Shadow
   - Padding
   - Header background (if applicable)
   - Preview with panel

7. Update ThemeComponentStyles model and CSS generator
8. Update form-themed.css to use component variables
```

### Prompt 3.2: Implement Accessibility Features

```
I need you to implement accessibility features in the theme editor.

Context:
- Project: Visual Editor Opus
- Location: Components/Theming/Sections/AccessibilitySection.razor

Implement:

1. AccessibilitySection.razor:

   Scale Factor:
   - Slider from 80% to 150%
   - Preview updates to show scaled form
   - Affects all font sizes proportionally
   - Persists in theme

   Focus Indicators:
   - Style selector: Ring, Outline, Underline
   - Color picker for focus color
   - Width slider (1-4px)
   - Offset slider (0-4px)
   - Preview showing focused input

   Contrast Checker:
   - Automatic checking of text/background combinations
   - Display WCAG level (Fail, AA, AAA)
   - Show contrast ratio
   - Suggestions for failing combinations
   - Check all text colors against their backgrounds

   Additional Options:
   - Reduce motion toggle (disables transitions)
   - Minimum font size (12-18px)
   - High contrast mode toggle (applies high-contrast preset)

2. Implement IThemeValidatorService:
   - ContrastCheckResult CheckContrast(string fg, string bg)
   - double CalculateContrastRatio(string color1, string color2)
   - bool IsWcagAACompliant(string fg, string bg)
   - bool IsWcagAAACompliant(string fg, string bg)

   Use relative luminance formula:
   L = 0.2126 * R + 0.7152 * G + 0.0722 * B
   Contrast = (L1 + 0.05) / (L2 + 0.05)
   AA requires 4.5:1 for normal text, 3:1 for large
   AAA requires 7:1 for normal text, 4.5:1 for large

3. Create ContrastBadge.razor:
   - Shows Pass/Fail with level (AA/AAA)
   - Color coded (green/yellow/red)
   - Tooltip with ratio and requirements

4. Add contrast warnings inline in ColorsSection
5. Create AccessibilityReport.razor showing all issues
```

### Prompt 3.3: Implement Theme Library Management

```
I need you to create a theme library management system.

Context:
- Project: Visual Editor Opus
- Location: Components/Theming/Modals/ThemeLibraryModal.razor

Implement:

1. ThemeLibraryModal.razor:

   Header:
   - Title "Theme Library"
   - Search input
   - Filter dropdown (All, Light, Dark, Custom, Presets)
   - View toggle (Grid, List)

   Grid View:
   - Theme cards with:
     - Color preview (primary color swatch)
     - Theme name
     - Mode indicator (light/dark icon)
     - Default badge if applicable
     - Last modified date
   - Click to select
   - Hover shows actions

   List View:
   - Table with columns:
     - Preview swatch
     - Name
     - Description
     - Mode
     - Modified
     - Actions
   - Sortable columns

   Actions (per theme):
   - Edit (opens in editor)
   - Duplicate
   - Set as default
   - Export
   - Delete (with confirmation)

   Footer:
   - Create New Theme button
   - Import Theme button
   - Cancel / Close

2. ThemeCard.razor:
   - Reusable theme preview card
   - Shows theme preview colors
   - Name and metadata
   - Action buttons

3. Update persistence service for:
   - List with filtering
   - Search by name
   - Duplicate theme
   - Set/unset default

4. Add theme library button to editor header
5. Show theme count in button badge
```

### Prompt 3.4: Final Integration and Polish

```
I need you to complete the final integration and polish the theme editor.

Context:
- Project: Visual Editor Opus
- All theme editor components should be complete

Tasks:

1. Navigation Integration:
   - Add "Themes" item to GlobalNav.razor
   - Icon: palette or paint brush
   - Route to /admin/theme-editor

2. Form Integration:
   - Add theme selector to form settings
   - Allow assigning theme to individual forms
   - Show theme preview in form list

3. Loading States:
   - Add loading skeletons for theme list
   - Show loading indicator during save
   - Handle slow image uploads gracefully

4. Error Handling:
   - Graceful handling of invalid themes
   - Recovery from save failures
   - Validation error display

5. Toast Notifications:
   - Theme saved successfully
   - Theme deleted
   - Import succeeded/failed
   - Export completed

6. Keyboard Shortcuts:
   - Ctrl+S to save
   - Ctrl+Z/Y for undo/redo
   - Escape to close modals
   - Document shortcuts in help

7. Responsive Design:
   - Mobile-friendly sidebar collapse
   - Touch-friendly color pickers
   - Responsive preview panel

8. Performance:
   - Lazy load theme images
   - Debounce preview updates
   - Optimize CSS generation

9. Documentation:
   - Add help tooltips throughout
   - Create user guide section
   - Document all CSS variables

10. Final Testing Checklist:
    - All presets load correctly
    - Undo/redo works reliably
    - Import/export round-trips perfectly
    - All controls update preview
    - Accessibility checker accurate
    - Themes persist correctly
    - Default theme applies to new forms
```

---

## 5. Utility Prompts

### Prompt: Debug Theme CSS Issues

```
I'm having issues with theme CSS in my Blazor form builder. Help me debug.

Context:
- Project: Visual Editor Opus
- Theme system uses CSS custom properties (--df-xxx)
- ThemeScope component injects variables via style attribute

Problem: [DESCRIBE YOUR SPECIFIC ISSUE]

Please:
1. Review my ThemeScope component
2. Check the CSS variable injection
3. Verify the form components consume variables correctly
4. Identify any specificity conflicts
5. Suggest fixes

Files to review:
- Components/Theming/ThemeScope.razor
- Services/Theming/ThemeCssGeneratorService.cs
- wwwroot/css/theming/form-themed.css
```

### Prompt: Add New Theme Preset

```
I need to add a new theme preset to the theme editor.

Preset Details:
- Name: [PRESET NAME]
- Category: [CATEGORY]
- Description: [DESCRIPTION]
- Colors:
  - Primary: [COLOR]
  - Background: [COLOR]
  - Text: [COLOR]
- Special requirements: [ANY SPECIAL STYLING]

Context:
- Project: Visual Editor Opus
- Presets location: Models/Theming/ThemePresets.cs

Please:
1. Create the FormTheme object with all appropriate values
2. Add to the Presets dictionary
3. Add preset info for the selector
4. Ensure dark mode variant if applicable
```

### Prompt: Create Custom Theme Control

```
I need to create a new custom control for the theme editor.

Control: [DESCRIBE THE CONTROL]
Purpose: [WHAT IT CONFIGURES]
Location: Components/Theming/Controls/

Requirements:
- Two-way binding with EventCallback
- Label and tooltip support
- Validation
- Matches existing control styling

Context:
- Project: Visual Editor Opus
- Follow patterns from ColorPicker.razor and SliderControl.razor

Please create:
1. The Razor component
2. Scoped CSS
3. Usage example in a section
```

---

## 6. Troubleshooting Prompts

### Prompt: Fix Live Preview Not Updating

```
The theme editor preview isn't updating when I change values.

Context:
- Project: Visual Editor Opus
- Using ThemeEditorStateService for state
- ThemeScope wraps the preview

Symptoms:
- Color picker changes value
- State service shows new value
- Preview doesn't reflect change

Please debug:
1. Check event firing in state service
2. Verify component re-rendering
3. Check ThemeScope CSS variable output
4. Look for caching issues
5. Check for missing StateHasChanged calls
```

### Prompt: Fix Import/Export Issues

```
Theme import/export isn't working correctly.

Context:
- Project: Visual Editor Opus
- Using System.Text.Json

Problem: [DESCRIBE SPECIFIC ISSUE]
- Export produces invalid JSON
- Import fails to parse
- Properties missing after round-trip
- etc.

Please:
1. Review serialization settings
2. Check for nullable property handling
3. Verify JSON structure matches model
4. Add appropriate error handling
5. Test round-trip serialization
```

---

## 7. Summary

This document provides comprehensive prompts for implementing all phases of the Theme Editor. Use these prompts sequentially for the best results, as later prompts may depend on earlier implementations.

**Recommended Order:**
1. Data models (Prompts 1.1-1.2)
2. Core services (Prompts 1.3-1.4, 1.6-1.8)
3. Basic UI (Prompts 1.5, 1.9)
4. Integration (Prompt 1.10)
5. Phase 2 enhancements (Prompts 2.1-2.5)
6. Phase 3 advanced features (Prompts 3.1-3.4)

Each prompt is designed to produce a complete, working implementation. Review and test each step before proceeding to the next.
