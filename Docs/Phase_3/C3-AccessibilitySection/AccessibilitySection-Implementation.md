# C.3 Accessibility Section - Implementation Plan

> **Task**: Accessibility Section for Property Panel
> **Location**: `Src/VisualEditorOpus/Components/PropertyPanel/Sections/`
> **Priority**: High
> **Estimated Effort**: 2-3 hours
> **Delegation**: 90% AI

---

## Overview

The Accessibility Section manages WCAG-compliant ARIA attributes for form fields. It provides bilingual label support (English/French), role selection, live region configuration, and compliance scoring.

---

## Schema Reference

From `DynamicForms.Core.V4`:

```csharp
// AccessibilityConfig.cs
public record AccessibilityConfig
{
    public string? AriaLabelEn { get; init; }
    public string? AriaLabelFr { get; init; }
    public string? AriaDescribedBy { get; init; }
    public string? AriaRole { get; init; }
    public bool AriaLive { get; init; }
}

// FormFieldSchema.cs
public AccessibilityConfig? Accessibility { get; init; }
```

---

## Component to Create

### AccessibilitySection.razor

```razor
@namespace VisualEditorOpus.Components.PropertyPanel.Sections

<div class="property-section">
    <div class="section-header @(IsExpanded ? "expanded" : "")" @onclick="ToggleExpanded">
        <div class="section-header-left">
            <i class="bi bi-universal-access"></i>
            <span>Accessibility</span>
        </div>
        <i class="bi bi-chevron-down section-chevron"></i>
    </div>

    @if (IsExpanded)
    {
        <div class="section-content">
            @* Accessibility Score *@
            <div class="a11y-score @ScoreClass">
                <div class="a11y-score-circle">
                    <span>@ScoreGrade</span>
                </div>
                <div class="a11y-score-content">
                    <div class="a11y-score-title">@ScoreTitle</div>
                    <div class="a11y-score-desc">@ScoreDescription</div>
                </div>
            </div>

            @* ARIA Label with Language Tabs *@
            <div class="form-group">
                <label class="form-label">
                    <i class="bi bi-tag"></i>
                    ARIA Label
                </label>
                <div class="language-tabs">
                    <button class="language-tab @(ActiveLanguage == "en" ? "active" : "")"
                            @onclick="() => ActiveLanguage = \"en\"">
                        <span>🇺🇸</span> English
                    </button>
                    <button class="language-tab @(ActiveLanguage == "fr" ? "active" : "")"
                            @onclick="() => ActiveLanguage = \"fr\"">
                        <span>🇫🇷</span> French
                    </button>
                </div>
                @if (ActiveLanguage == "en")
                {
                    <input type="text"
                           class="form-input"
                           value="@Config.AriaLabelEn"
                           @oninput="e => UpdateAriaLabel(e.Value?.ToString(), \"en\")"
                           placeholder="Accessible label for screen readers" />
                }
                else
                {
                    <input type="text"
                           class="form-input"
                           value="@Config.AriaLabelFr"
                           @oninput="e => UpdateAriaLabel(e.Value?.ToString(), \"fr\")"
                           placeholder="Étiquette accessible pour les lecteurs d'écran" />
                }
                <div class="help-text">
                    <i class="bi bi-info-circle"></i>
                    <span>Screen readers announce this instead of the visible label</span>
                </div>
            </div>

            @* ARIA Described By *@
            <div class="form-group">
                <label class="form-label">
                    <i class="bi bi-link-45deg"></i>
                    Described By (Field Reference)
                </label>
                <div class="field-reference-select">
                    <input type="text"
                           class="field-reference-input"
                           value="@Config.AriaDescribedBy"
                           @oninput="e => UpdateAriaDescribedBy(e.Value?.ToString())"
                           placeholder="Reference another field's ID" />
                    <button class="field-reference-btn" @onclick="OpenFieldSelector">
                        <i class="bi bi-search"></i>
                    </button>
                </div>
                <div class="help-text">
                    <i class="bi bi-info-circle"></i>
                    <span>Link to a field that provides additional description</span>
                </div>
            </div>

            @* ARIA Role *@
            <div class="form-group">
                <label class="form-label">
                    <i class="bi bi-diagram-2"></i>
                    ARIA Role
                </label>
                <select class="form-select"
                        value="@Config.AriaRole"
                        @onchange="e => UpdateAriaRole(e.Value?.ToString())">
                    <option value="">Default (based on field type)</option>
                    @foreach (var role in AvailableRoles)
                    {
                        <option value="@role.Value" selected="@(Config.AriaRole == role.Value)">
                            @role.Label
                        </option>
                    }
                </select>
            </div>

            @* Live Region Toggle *@
            <div class="form-group">
                <label class="form-label">
                    <i class="bi bi-broadcast"></i>
                    Live Region
                </label>
                <div class="toggle-row">
                    <div class="toggle-label">
                        <div class="toggle-icon">
                            <i class="bi bi-megaphone"></i>
                        </div>
                        <div>
                            <div class="toggle-text">Announce Changes</div>
                            <div class="toggle-desc">Screen readers announce when value changes</div>
                        </div>
                    </div>
                    <div class="toggle-switch @(Config.AriaLive ? "active" : "")"
                         @onclick="ToggleAriaLive"></div>
                </div>
            </div>

            @* ARIA Preview *@
            <div class="aria-preview">
                <div class="aria-preview-title">
                    <i class="bi bi-code-slash"></i>
                    Generated Attributes
                </div>
                <div class="aria-preview-code">@((MarkupString)GeneratedAttributesHtml)</div>
            </div>

            @* Quick Actions *@
            <div class="quick-actions">
                <button class="quick-action-btn" @onclick="AutoGenerate">
                    <i class="bi bi-magic"></i>
                    Auto-Generate
                </button>
                <button class="quick-action-btn" @onclick="CopyToFrench">
                    <i class="bi bi-translate"></i>
                    Copy to French
                </button>
            </div>
        </div>
    }
</div>

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = default!;
    [Parameter] public IEnumerable<FormFieldSchema> AllFields { get; set; } = [];
    [Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }

    private bool IsExpanded { get; set; } = true;
    private string ActiveLanguage { get; set; } = "en";

    private AccessibilityConfig Config => Field.Accessibility ?? new AccessibilityConfig();

    // Accessibility Score
    private int Score => CalculateScore();
    private string ScoreGrade => Score >= 80 ? "A+" : (Score >= 60 ? "B" : "C");
    private string ScoreClass => Score >= 80 ? "excellent" : (Score >= 60 ? "good" : "needs-work");
    private string ScoreTitle => Score >= 80 ? "Excellent Accessibility" : (Score >= 60 ? "Good Accessibility" : "Needs Improvement");
    private string ScoreDescription => Score >= 80
        ? "All required ARIA attributes configured"
        : (Score >= 60 ? "Most accessibility requirements met" : "Missing required accessibility attributes");

    // Available ARIA Roles
    private static readonly AriaRoleOption[] AvailableRoles = new[]
    {
        new AriaRoleOption("textbox", "textbox - Text input"),
        new AriaRoleOption("searchbox", "searchbox - Search input"),
        new AriaRoleOption("spinbutton", "spinbutton - Number input"),
        new AriaRoleOption("combobox", "combobox - Autocomplete"),
        new AriaRoleOption("listbox", "listbox - Select list"),
        new AriaRoleOption("menu", "menu - Menu"),
        new AriaRoleOption("radiogroup", "radiogroup - Radio options"),
        new AriaRoleOption("checkbox", "checkbox - Checkbox"),
        new AriaRoleOption("switch", "switch - Toggle"),
        new AriaRoleOption("slider", "slider - Range slider")
    };

    private void ToggleExpanded() => IsExpanded = !IsExpanded;

    private int CalculateScore()
    {
        int score = 0;

        // English label: 25 points
        if (!string.IsNullOrWhiteSpace(Config.AriaLabelEn)) score += 25;

        // French label: 25 points
        if (!string.IsNullOrWhiteSpace(Config.AriaLabelFr)) score += 25;

        // Described by: 20 points
        if (!string.IsNullOrWhiteSpace(Config.AriaDescribedBy)) score += 20;

        // Role defined: 15 points
        if (!string.IsNullOrWhiteSpace(Config.AriaRole)) score += 15;

        // Live region considered: 15 points
        if (Config.AriaLive || FieldDoesNotNeedLive()) score += 15;

        return score;
    }

    private bool FieldDoesNotNeedLive()
    {
        // Static fields don't need live regions
        return Field.Type is FieldType.Hidden or FieldType.Section or FieldType.File or FieldType.Image;
    }

    private string GeneratedAttributesHtml
    {
        get
        {
            var lines = new List<string> { "&lt;input" };

            if (!string.IsNullOrWhiteSpace(Config.AriaLabelEn))
                lines.Add($"  <span class=\"attr-name\">aria-label</span>=<span class=\"attr-value\">\"{Config.AriaLabelEn}\"</span>");

            if (!string.IsNullOrWhiteSpace(Config.AriaDescribedBy))
                lines.Add($"  <span class=\"attr-name\">aria-describedby</span>=<span class=\"attr-value\">\"{Config.AriaDescribedBy}\"</span>");

            if (!string.IsNullOrWhiteSpace(Config.AriaRole))
                lines.Add($"  <span class=\"attr-name\">role</span>=<span class=\"attr-value\">\"{Config.AriaRole}\"</span>");

            if (Config.AriaLive)
                lines.Add($"  <span class=\"attr-name\">aria-live</span>=<span class=\"attr-value\">\"polite\"</span>");

            lines.Add("/&gt;");
            return string.Join("\n", lines);
        }
    }

    private async Task UpdateAriaLabel(string? value, string language)
    {
        var newConfig = language == "en"
            ? Config with { AriaLabelEn = value }
            : Config with { AriaLabelFr = value };

        await UpdateConfig(newConfig);
    }

    private async Task UpdateAriaDescribedBy(string? value)
    {
        var newConfig = Config with { AriaDescribedBy = value };
        await UpdateConfig(newConfig);
    }

    private async Task UpdateAriaRole(string? value)
    {
        var newConfig = Config with { AriaRole = value };
        await UpdateConfig(newConfig);
    }

    private async Task ToggleAriaLive()
    {
        var newConfig = Config with { AriaLive = !Config.AriaLive };
        await UpdateConfig(newConfig);
    }

    private async Task UpdateConfig(AccessibilityConfig newConfig)
    {
        var updated = Field with { Accessibility = newConfig };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task AutoGenerate()
    {
        // Generate sensible defaults based on field properties
        var labelEn = GenerateDefaultLabel(Field, "en");
        var labelFr = GenerateDefaultLabel(Field, "fr");
        var role = GetDefaultRole(Field.Type);

        var newConfig = new AccessibilityConfig
        {
            AriaLabelEn = string.IsNullOrWhiteSpace(Config.AriaLabelEn) ? labelEn : Config.AriaLabelEn,
            AriaLabelFr = string.IsNullOrWhiteSpace(Config.AriaLabelFr) ? labelFr : Config.AriaLabelFr,
            AriaDescribedBy = Config.AriaDescribedBy,
            AriaRole = string.IsNullOrWhiteSpace(Config.AriaRole) ? role : Config.AriaRole,
            AriaLive = Config.AriaLive
        };

        await UpdateConfig(newConfig);
    }

    private async Task CopyToFrench()
    {
        // Simple copy - in production, use translation service
        if (!string.IsNullOrWhiteSpace(Config.AriaLabelEn))
        {
            var newConfig = Config with { AriaLabelFr = Config.AriaLabelEn };
            await UpdateConfig(newConfig);
            ActiveLanguage = "fr";
        }
    }

    private void OpenFieldSelector()
    {
        // TODO: Open field selector modal
    }

    private static string GenerateDefaultLabel(FormFieldSchema field, string language)
    {
        var label = language == "en" ? field.LabelEn : field.LabelFr;
        if (string.IsNullOrWhiteSpace(label)) label = field.LabelEn ?? field.Id;

        // Add context based on field type
        return field.Type switch
        {
            FieldType.Email => language == "en" ? $"Enter your {label}" : $"Entrez votre {label}",
            FieldType.Phone => language == "en" ? $"Enter your {label}" : $"Entrez votre {label}",
            FieldType.Date => language == "en" ? $"Select {label}" : $"Sélectionnez {label}",
            FieldType.Select => language == "en" ? $"Choose {label}" : $"Choisissez {label}",
            FieldType.Checkbox => language == "en" ? $"Check if {label}" : $"Cochez si {label}",
            FieldType.File => language == "en" ? $"Upload {label}" : $"Télécharger {label}",
            _ => language == "en" ? $"Enter {label}" : $"Entrez {label}"
        };
    }

    private static string GetDefaultRole(FieldType type) => type switch
    {
        FieldType.Text => "textbox",
        FieldType.Email => "textbox",
        FieldType.Phone => "textbox",
        FieldType.Number => "spinbutton",
        FieldType.Select => "listbox",
        FieldType.MultiSelect => "listbox",
        FieldType.Radio => "radiogroup",
        FieldType.Checkbox => "checkbox",
        FieldType.Toggle => "switch",
        FieldType.TextArea => "textbox",
        FieldType.Date => "textbox",
        FieldType.Time => "textbox",
        _ => ""
    };

    private record AriaRoleOption(string Value, string Label);
}
```

---

## CSS Styles

Add to `accessibility-section.css`:

```css
/* ===== ACCESSIBILITY SECTION ===== */

/* Score Display */
.a11y-score {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 16px;
    background: var(--bg-secondary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    margin-bottom: 16px;
}

.a11y-score-circle {
    width: 56px;
    height: 56px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 18px;
    font-weight: 700;
    position: relative;
}

.a11y-score.excellent .a11y-score-circle {
    color: var(--success);
    background: var(--success-light);
}

.a11y-score.good .a11y-score-circle {
    color: var(--primary);
    background: var(--primary-light);
}

.a11y-score.needs-work .a11y-score-circle {
    color: var(--warning);
    background: var(--warning-light);
}

.a11y-score-content { flex: 1; }

.a11y-score-title {
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary);
}

.a11y-score-desc {
    font-size: 12px;
    color: var(--text-muted);
}

/* Language Tabs */
.language-tabs {
    display: flex;
    gap: 4px;
    margin-bottom: 12px;
    padding: 4px;
    background: var(--bg-tertiary);
    border-radius: var(--radius-md);
}

.language-tab {
    flex: 1;
    padding: 8px 16px;
    border: none;
    background: transparent;
    border-radius: var(--radius-sm);
    font-size: 13px;
    font-weight: 500;
    color: var(--text-secondary);
    cursor: pointer;
    transition: all 0.15s;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
}

.language-tab:hover {
    color: var(--text-primary);
    background: var(--bg-secondary);
}

.language-tab.active {
    background: var(--bg-primary);
    color: var(--primary);
    box-shadow: var(--shadow-sm);
}

/* Field Reference Selector */
.field-reference-select {
    position: relative;
}

.field-reference-input {
    width: 100%;
    padding: 10px 14px;
    padding-right: 40px;
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    font-size: 14px;
    color: var(--text-primary);
    background: var(--bg-primary);
}

.field-reference-input:focus {
    outline: none;
    border-color: var(--primary);
    box-shadow: 0 0 0 3px var(--primary-light);
}

.field-reference-btn {
    position: absolute;
    right: 8px;
    top: 50%;
    transform: translateY(-50%);
    width: 28px;
    height: 28px;
    border: none;
    background: var(--bg-tertiary);
    border-radius: var(--radius-sm);
    color: var(--text-muted);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
}

.field-reference-btn:hover {
    background: var(--primary-light);
    color: var(--primary);
}

/* Toggle Row */
.toggle-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 14px;
    background: var(--bg-secondary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
}

.toggle-label {
    display: flex;
    align-items: center;
    gap: 10px;
}

.toggle-icon {
    width: 32px;
    height: 32px;
    border-radius: var(--radius-sm);
    background: var(--bg-tertiary);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    color: var(--text-muted);
}

.toggle-text {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-primary);
}

.toggle-desc {
    font-size: 11px;
    color: var(--text-muted);
}

.toggle-switch {
    width: 44px;
    height: 24px;
    background: var(--border-color);
    border-radius: 12px;
    position: relative;
    cursor: pointer;
    transition: background 0.2s;
}

.toggle-switch::after {
    content: '';
    position: absolute;
    width: 20px;
    height: 20px;
    background: white;
    border-radius: 50%;
    top: 2px;
    left: 2px;
    transition: transform 0.2s;
    box-shadow: var(--shadow-sm);
}

.toggle-switch.active {
    background: var(--primary);
}

.toggle-switch.active::after {
    transform: translateX(20px);
}

/* ARIA Preview */
.aria-preview {
    background: var(--bg-tertiary);
    border-radius: var(--radius-md);
    padding: 14px;
    margin-top: 16px;
}

.aria-preview-title {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    margin-bottom: 10px;
    display: flex;
    align-items: center;
    gap: 6px;
}

.aria-preview-code {
    font-family: 'Fira Code', 'Cascadia Code', monospace;
    font-size: 12px;
    color: var(--text-primary);
    background: var(--bg-primary);
    padding: 12px 14px;
    border-radius: var(--radius-sm);
    overflow-x: auto;
    white-space: pre;
}

.aria-preview-code .attr-name { color: var(--primary); }
.aria-preview-code .attr-value { color: var(--success); }

/* Quick Actions */
.quick-actions {
    display: flex;
    gap: 8px;
    margin-top: 16px;
    padding-top: 16px;
    border-top: 1px solid var(--border-color);
}

.quick-action-btn {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
    padding: 10px;
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    background: var(--bg-primary);
    color: var(--text-secondary);
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.15s;
}

.quick-action-btn:hover {
    border-color: var(--primary);
    color: var(--primary);
    background: var(--primary-light);
}

/* Help Text */
.help-text {
    font-size: 12px;
    color: var(--text-muted);
    margin-top: 6px;
    display: flex;
    align-items: flex-start;
    gap: 6px;
}

.help-text i {
    font-size: 12px;
    margin-top: 2px;
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the Accessibility Section component for the property panel in my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/PropertyPanel/Sections/
- Schema: DynamicForms.Core.V4 (FormFieldSchema has Accessibility property)

## Component: AccessibilitySection.razor

This section manages WCAG-compliant ARIA attributes for form fields.

### Features Required:

1. **Accessibility Score**
   - Visual score indicator (A+, B, C grades)
   - Color-coded: green (excellent), blue (good), yellow (needs work)
   - Score based on: labels, descriptions, role, live region

2. **ARIA Label with Language Tabs**
   - Tab switcher: English / French
   - Input field for aria-label
   - Help text explaining purpose

3. **ARIA Described By**
   - Input with field search button
   - Links to another field's ID
   - Help text explaining purpose

4. **ARIA Role Selector**
   - Dropdown with common roles
   - Default option based on field type
   - Roles: textbox, searchbox, spinbutton, combobox, listbox, radiogroup, checkbox, switch, slider

5. **Live Region Toggle**
   - Toggle switch for aria-live
   - Explains that screen readers announce changes
   - Visual toggle with icon

6. **Generated Attributes Preview**
   - Shows HTML-like preview of generated attributes
   - Syntax highlighted (attribute names in blue, values in green)
   - Updates in real-time

7. **Quick Actions**
   - Auto-Generate: Create sensible defaults from field properties
   - Copy to French: Duplicate English label to French field

### Parameters:
```csharp
[Parameter] public FormFieldSchema Field { get; set; }
[Parameter] public IEnumerable<FormFieldSchema> AllFields { get; set; }
[Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }
```

### Schema Reference:
```csharp
public record AccessibilityConfig
{
    public string? AriaLabelEn { get; init; }
    public string? AriaLabelFr { get; init; }
    public string? AriaDescribedBy { get; init; }
    public string? AriaRole { get; init; }
    public bool AriaLive { get; init; }
}
```

### Key Logic:
- Score calculation (25 pts each label, 20 pts described by, 15 pts role, 15 pts live)
- Auto-generate labels based on field type and label
- Default roles based on FieldType enum
- French/English tab switching

Please implement complete, production-ready code with proper CSS styling.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `AccessibilitySection-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for accessibility score display testing
- Score update as fields are configured
- Language tab switching (English/French)
- ARIA Label input testing for both languages
- ARIA Described By field reference testing
- Field search button functionality testing
- ARIA Role dropdown selection testing
- Live Region toggle switch testing
- Generated Attributes preview testing
- Auto-Generate quick action testing
- Copy to French quick action testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Integration with RightSidebar property panel
- Field selector modal for AriaDescribedBy (TODO in code)
- Translation service integration for auto-translate (future)
- AccessibilityConfig schema import from Core.V4
- CSS imports for accessibility-section.css
- Score calculation thresholds verification

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Score displays with grade (A+, B, C)
- [ ] Excellent (green) for score >= 80
- [ ] Good (blue) for score >= 60
- [ ] Needs Work (yellow) for score < 60
- [ ] Score updates when fields change
- [ ] English tab shows English label input
- [ ] French tab shows French label input
- [ ] Tab switching updates active tab styling
- [ ] ARIA Label saves to correct language property
- [ ] Described By input accepts field reference
- [ ] Search button is clickable (future modal)
- [ ] Role dropdown shows all common roles
- [ ] Default role based on field type
- [ ] Live Region toggle switches on/off
- [ ] Toggle animation works smoothly
- [ ] Preview shows generated HTML attributes
- [ ] Attribute names in blue, values in green
- [ ] Preview updates in real-time
- [ ] Auto-Generate creates sensible defaults
- [ ] Copy to French copies English label
- [ ] Section expands/collapses correctly
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Score displays correctly based on configuration
- [ ] Score updates when fields change
- [ ] Language tabs switch correctly
- [ ] ARIA label saves for correct language
- [ ] Described by field saves
- [ ] Field search button works (future modal)
- [ ] Role dropdown works
- [ ] Live region toggle works
- [ ] Preview updates in real-time
- [ ] Auto-Generate creates sensible defaults
- [ ] Copy to French copies label
- [ ] Dark mode styling correct
- [ ] Section collapses/expands

---

## Notes

- Bilingual support is critical for Canadian government forms
- Consider adding validation for field reference in AriaDescribedBy
- Future: Add translation service integration
- Consider adding WCAG checklist display
- Screen reader testing recommended during implementation
