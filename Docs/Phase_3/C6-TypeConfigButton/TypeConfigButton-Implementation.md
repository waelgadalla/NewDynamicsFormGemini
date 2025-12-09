# C.6 TypeConfig Button - Implementation Plan

> **Task**: TypeConfig Button Component
> **Location**: `Src/VisualEditorOpus/Components/PropertyPanel/`
> **Priority**: Medium
> **Estimated Effort**: 2-3 hours
> **Delegation**: 85% AI

---

## Overview

The TypeConfig Button provides access to type-specific configuration options for each field type. It displays as a prominent button in the property panel that indicates whether the field has been configured and shows a summary of current settings.

---

## Schema Reference

From `DynamicForms.Core.V4`:

```csharp
// FormFieldSchema.cs
public object? TypeConfig { get; init; }  // JSON object with type-specific settings

// Example TypeConfig for different field types:
// Text: { inputMask: string, maxLength: int, pattern: string, autocomplete: string }
// Number: { min: number, max: number, step: number, format: string }
// Date: { displayFormat: string, minDate: string, maxDate: string, disabledDays: string[] }
// File: { allowedTypes: string[], maxSize: int, maxFiles: int, storage: string }
```

---

## Component to Create

### TypeConfigButton.razor

```razor
@namespace VisualEditorOpus.Components.PropertyPanel

<button class="type-config-btn @(IsConfigured ? "configured" : "")"
        @onclick="OpenConfigModal">
    <div class="type-config-icon">
        <i class="bi bi-@(IsConfigured ? "check-lg" : IconName)"></i>
    </div>
    <div class="type-config-content">
        <div class="type-config-title">@Title</div>
        <div class="type-config-desc">@Description</div>
        @if (IsConfigured && ConfigSummary.Any())
        {
            <div class="config-summary">
                @foreach (var item in ConfigSummary.Take(3))
                {
                    <span class="config-tag">@item</span>
                }
            </div>
        }
    </div>
    @if (IsConfigured)
    {
        <span class="type-config-badge">Configured</span>
    }
    <i class="bi bi-chevron-right type-config-arrow"></i>
</button>

@* Type-Specific Config Modal *@
@switch (Field.Type)
{
    case FieldType.Text:
    case FieldType.Email:
    case FieldType.Phone:
        <TextConfigModal @ref="textConfigModal" Field="@Field" OnSave="SaveConfig" />
        break;
    case FieldType.Number:
        <NumberConfigModal @ref="numberConfigModal" Field="@Field" OnSave="SaveConfig" />
        break;
    case FieldType.Date:
    case FieldType.DateTime:
        <DateConfigModal @ref="dateConfigModal" Field="@Field" OnSave="SaveConfig" />
        break;
    case FieldType.Time:
        <TimeConfigModal @ref="timeConfigModal" Field="@Field" OnSave="SaveConfig" />
        break;
    case FieldType.File:
        <FileConfigModal @ref="fileConfigModal" Field="@Field" OnSave="SaveConfig" />
        break;
    case FieldType.Image:
        <ImageConfigModal @ref="imageConfigModal" Field="@Field" OnSave="SaveConfig" />
        break;
    case FieldType.Signature:
        <SignatureConfigModal @ref="signatureConfigModal" Field="@Field" OnSave="SaveConfig" />
        break;
    case FieldType.Repeater:
        <RepeaterConfigModal @ref="repeaterConfigModal" Field="@Field" OnSave="SaveConfig" />
        break;
}

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = default!;
    [Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }

    // Modal references
    private TextConfigModal? textConfigModal;
    private NumberConfigModal? numberConfigModal;
    private DateConfigModal? dateConfigModal;
    private TimeConfigModal? timeConfigModal;
    private FileConfigModal? fileConfigModal;
    private ImageConfigModal? imageConfigModal;
    private SignatureConfigModal? signatureConfigModal;
    private RepeaterConfigModal? repeaterConfigModal;

    private bool IsConfigured => Field.TypeConfig != null;

    private string IconName => Field.Type switch
    {
        FieldType.Text or FieldType.Email or FieldType.Phone => "sliders",
        FieldType.Number => "123",
        FieldType.Date or FieldType.DateTime => "calendar-event",
        FieldType.Time => "clock",
        FieldType.File => "upload",
        FieldType.Image => "image",
        FieldType.Signature => "pen",
        FieldType.Repeater => "collection",
        _ => "gear"
    };

    private string Title => Field.Type switch
    {
        FieldType.Text => "Configure Text Options",
        FieldType.Email => "Configure Email Options",
        FieldType.Phone => "Configure Phone Options",
        FieldType.Number => "Number Options",
        FieldType.Date => "Date Options",
        FieldType.DateTime => "DateTime Options",
        FieldType.Time => "Time Options",
        FieldType.File => "Configure File Options",
        FieldType.Image => "Configure Image Options",
        FieldType.Signature => "Signature Options",
        FieldType.Repeater => "Repeater Options",
        _ => "Type Configuration"
    };

    private string Description
    {
        get
        {
            if (IsConfigured)
            {
                var count = GetConfiguredSettingsCount();
                return $"Configured with {count} setting{(count != 1 ? "s" : "")}";
            }

            return Field.Type switch
            {
                FieldType.Text => "Set input mask, format, limits",
                FieldType.Email => "Set validation, autocomplete",
                FieldType.Phone => "Set format, country code",
                FieldType.Number => "Set min, max, step, format",
                FieldType.Date or FieldType.DateTime => "Set range, format, disabled days",
                FieldType.Time => "Set 12h/24h, step, range",
                FieldType.File => "Set allowed types, size limits",
                FieldType.Image => "Set dimensions, format, crop",
                FieldType.Signature => "Set canvas size, stroke",
                FieldType.Repeater => "Set min/max rows, layout",
                _ => "Configure type-specific settings"
            };
        }
    }

    private IEnumerable<string> ConfigSummary
    {
        get
        {
            if (Field.TypeConfig == null) return Enumerable.Empty<string>();

            var config = Field.TypeConfig as IDictionary<string, object>;
            if (config == null) return Enumerable.Empty<string>();

            return Field.Type switch
            {
                FieldType.Number => GetNumberConfigSummary(config),
                FieldType.Date or FieldType.DateTime => GetDateConfigSummary(config),
                FieldType.File => GetFileConfigSummary(config),
                _ => GetGenericConfigSummary(config)
            };
        }
    }

    private int GetConfiguredSettingsCount()
    {
        if (Field.TypeConfig == null) return 0;

        var config = Field.TypeConfig as IDictionary<string, object>;
        return config?.Count(kv => kv.Value != null) ?? 0;
    }

    private IEnumerable<string> GetNumberConfigSummary(IDictionary<string, object> config)
    {
        if (config.TryGetValue("min", out var min)) yield return $"Min: {min}";
        if (config.TryGetValue("max", out var max)) yield return $"Max: {max}";
        if (config.TryGetValue("step", out var step)) yield return $"Step: {step}";
        if (config.TryGetValue("format", out var format)) yield return $"{format}";
    }

    private IEnumerable<string> GetDateConfigSummary(IDictionary<string, object> config)
    {
        if (config.TryGetValue("displayFormat", out var format)) yield return $"Format: {format}";
        if (config.TryGetValue("minDate", out var min)) yield return $"Min: {min}";
        if (config.TryGetValue("maxDate", out var max)) yield return $"Max: {max}";
    }

    private IEnumerable<string> GetFileConfigSummary(IDictionary<string, object> config)
    {
        if (config.TryGetValue("allowedTypes", out var types))
            yield return string.Join(", ", (types as IEnumerable<string>) ?? []);
        if (config.TryGetValue("maxSize", out var size)) yield return $"Max: {size}MB";
        if (config.TryGetValue("maxFiles", out var count)) yield return $"Files: {count}";
    }

    private IEnumerable<string> GetGenericConfigSummary(IDictionary<string, object> config)
    {
        return config.Take(3).Select(kv => $"{kv.Key}: {kv.Value}");
    }

    private async Task OpenConfigModal()
    {
        switch (Field.Type)
        {
            case FieldType.Text:
            case FieldType.Email:
            case FieldType.Phone:
                await textConfigModal?.Open()!;
                break;
            case FieldType.Number:
                await numberConfigModal?.Open()!;
                break;
            case FieldType.Date:
            case FieldType.DateTime:
                await dateConfigModal?.Open()!;
                break;
            case FieldType.Time:
                await timeConfigModal?.Open()!;
                break;
            case FieldType.File:
                await fileConfigModal?.Open()!;
                break;
            case FieldType.Image:
                await imageConfigModal?.Open()!;
                break;
            case FieldType.Signature:
                await signatureConfigModal?.Open()!;
                break;
            case FieldType.Repeater:
                await repeaterConfigModal?.Open()!;
                break;
        }
    }

    private async Task SaveConfig(object typeConfig)
    {
        var updated = Field with { TypeConfig = typeConfig };
        await OnFieldChanged.InvokeAsync(updated);
    }
}
```

---

## CSS Styles

Add to `type-config-button.css`:

```css
/* ===== TYPE CONFIG BUTTON ===== */

.type-config-btn {
    display: flex;
    align-items: center;
    gap: 12px;
    width: 100%;
    padding: 14px 16px;
    border: 1px solid var(--border-color);
    border-radius: var(--radius-lg);
    background: var(--bg-primary);
    cursor: pointer;
    transition: all 0.2s;
    text-align: left;
}

.type-config-btn:hover {
    border-color: var(--primary);
    background: var(--primary-light);
}

.type-config-btn.configured {
    border-color: var(--success);
    background: var(--success-light);
}

.type-config-icon {
    width: 44px;
    height: 44px;
    border-radius: var(--radius-md);
    background: var(--bg-tertiary);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 20px;
    color: var(--text-muted);
    flex-shrink: 0;
    transition: all 0.2s;
}

.type-config-btn:hover .type-config-icon {
    background: var(--primary);
    color: white;
}

.type-config-btn.configured .type-config-icon {
    background: var(--success);
    color: white;
}

.type-config-content {
    flex: 1;
    min-width: 0;
}

.type-config-title {
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary);
}

.type-config-desc {
    font-size: 12px;
    color: var(--text-muted);
}

.type-config-btn.configured .type-config-desc {
    color: var(--success);
}

.config-summary {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    margin-top: 8px;
}

.config-tag {
    font-size: 11px;
    padding: 3px 8px;
    border-radius: 6px;
    background: var(--bg-tertiary);
    color: var(--text-secondary);
}

.type-config-btn.configured .config-tag {
    background: rgba(255, 255, 255, 0.5);
    color: var(--success);
}

.type-config-badge {
    font-size: 10px;
    padding: 2px 8px;
    border-radius: 10px;
    background: var(--success);
    color: white;
    font-weight: 600;
    flex-shrink: 0;
}

.type-config-arrow {
    color: var(--text-muted);
    font-size: 16px;
    flex-shrink: 0;
}

.type-config-btn:hover .type-config-arrow {
    color: var(--primary);
}
```

---

## Field Types with TypeConfig

| Field Type | Config Properties |
|------------|-------------------|
| **Text** | inputMask, maxLength, pattern, autocomplete, transform |
| **Email** | allowMultiple, domains, autocomplete |
| **Phone** | format, countryCode, validateFormat |
| **Number** | min, max, step, format (currency, percent, decimal) |
| **Date** | displayFormat, minDate, maxDate, disabledDays, disabledDates |
| **Time** | format (12h/24h), step, minTime, maxTime |
| **DateTime** | dateFormat, timeFormat, timezone |
| **File** | allowedTypes[], maxSize, maxFiles, storage |
| **Image** | maxWidth, maxHeight, formats[], crop, compress |
| **Signature** | width, height, strokeColor, strokeWidth, backgroundColor |
| **Repeater** | minRows, maxRows, layout (table/list), addLabel, removeLabel |

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the TypeConfig Button component for the property panel in my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/PropertyPanel/
- Schema: DynamicForms.Core.V4 (FormFieldSchema has TypeConfig property)

## Component: TypeConfigButton.razor

A button that opens type-specific configuration modals.

### Features Required:

1. **Button Display**
   - Large clickable area (full width)
   - Icon based on field type
   - Title and description
   - Different states: default, hover, configured

2. **Configured State**
   - Green styling when TypeConfig is not null
   - Shows "Configured" badge
   - Shows summary tags of current settings
   - Checkmark icon instead of type icon

3. **Type-Specific Behavior**
   - Opens different modals based on FieldType
   - Dynamic title and description per type
   - Summary extraction per type

4. **Field Types Supported**
   - Text/Email/Phone: mask, pattern, autocomplete
   - Number: min, max, step, format
   - Date/DateTime: format, range, disabled days
   - Time: 12h/24h, step, range
   - File: types, size, count
   - Image: dimensions, formats, crop
   - Signature: canvas size, stroke
   - Repeater: min/max rows, layout

### Parameters:
```csharp
[Parameter] public FormFieldSchema Field { get; set; }
[Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }
```

### Key Logic:
- Check Field.TypeConfig != null for configured state
- Parse TypeConfig as dictionary for summary
- Route to appropriate modal based on FieldType

Please implement complete, production-ready code with proper CSS styling.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `TypeConfigButton-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing unconfigured button display
- Configured button display testing (green state)
- Icon change based on field type testing
- Title change based on field type testing
- Description change when configured testing
- Config summary tags display testing
- Hover state animation testing
- Click opens correct modal for each type:
  - Text/Email/Phone → TextConfigModal
  - Number → NumberConfigModal
  - Date/DateTime → DateConfigModal
  - Time → TimeConfigModal
  - File → FileConfigModal
  - Image → ImageConfigModal
  - Signature → SignatureConfigModal
  - Repeater → RepeaterConfigModal
- Modal save updates TypeConfig testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Type-specific config modals creation (TextConfigModal, NumberConfigModal, etc.)
- TypeConfig JSON parsing implementation
- Config summary extraction per type verification
- Integration with RightSidebar property panel
- CSS imports for type-config-button.css

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Button displays full width
- [ ] Icon changes based on FieldType
- [ ] Title changes based on FieldType
- [ ] Description shows default text when unconfigured
- [ ] Description shows setting count when configured
- [ ] Hover state changes background and icon color
- [ ] Configured state shows green background
- [ ] Configured state shows checkmark icon
- [ ] "Configured" badge appears when configured
- [ ] Config summary tags display (max 3)
- [ ] Chevron arrow shows on right
- [ ] Click opens TextConfigModal for Text/Email/Phone
- [ ] Click opens NumberConfigModal for Number
- [ ] Click opens DateConfigModal for Date/DateTime
- [ ] Click opens TimeConfigModal for Time
- [ ] Click opens FileConfigModal for File
- [ ] Click opens ImageConfigModal for Image
- [ ] Click opens SignatureConfigModal for Signature
- [ ] Click opens RepeaterConfigModal for Repeater
- [ ] Modal save updates field.TypeConfig
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Button displays correctly for unconfigured field
- [ ] Button displays correctly for configured field
- [ ] Icon changes based on field type
- [ ] Title changes based on field type
- [ ] Description shows setting count when configured
- [ ] Summary tags display correctly
- [ ] Hover state works
- [ ] Clicking opens correct modal for type
- [ ] Modal saves config correctly
- [ ] Dark mode styling correct

---

## Notes

- TypeConfig modals are separate components (see Modal documentation)
- Consider adding TypeConfig validation
- TypeConfig is stored as JSON object
- Consider using System.Text.Json for parsing
- Not all field types have TypeConfig options
