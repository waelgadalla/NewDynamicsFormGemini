# D.2 Form Preview - Implementation Plan

> **Task**: Form Preview Component
> **Location**: `Src/VisualEditorOpus/Components/Preview/`
> **Priority**: High
> **Estimated Effort**: 4-5 hours
> **Delegation**: 80% AI

---

## Overview

The Form Preview renders the form as end-users would see it. It supports responsive device simulation, language switching (EN/FR), zoom controls, and a browser-like frame for realism.

---

## Components to Create

### FormPreview.razor (Main Container)

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="preview-container">
    <PreviewToolbar
        Device="@CurrentDevice"
        Language="@CurrentLanguage"
        Zoom="@ZoomLevel"
        OnDeviceChange="SetDevice"
        OnLanguageChange="SetLanguage"
        OnZoomChange="SetZoom" />

    <div class="device-frame @DeviceClass">
        <div class="device-chrome">
            <div class="device-dot red"></div>
            <div class="device-dot yellow"></div>
            <div class="device-dot green"></div>
            <div class="device-url">preview.forms.example.com/@Module?.Id</div>
        </div>
        <div class="device-content" style="transform: scale(@(ZoomLevel / 100.0));">
            <div class="preview-badge">
                <i class="bi bi-eye"></i>
                Preview Mode
            </div>
            <RenderedForm
                Module="@Module"
                Language="@CurrentLanguage"
                OnSubmit="HandleSubmit" />
        </div>
    </div>
</div>

@code {
    [Parameter] public FormModuleSchema? Module { get; set; }

    private PreviewDevice CurrentDevice { get; set; } = PreviewDevice.Desktop;
    private string CurrentLanguage { get; set; } = "en";
    private int ZoomLevel { get; set; } = 100;

    private string DeviceClass => CurrentDevice switch
    {
        PreviewDevice.Tablet => "device-tablet",
        PreviewDevice.Mobile => "device-mobile",
        _ => "device-desktop"
    };

    private void SetDevice(PreviewDevice device) => CurrentDevice = device;
    private void SetLanguage(string lang) => CurrentLanguage = lang;
    private void SetZoom(int zoom) => ZoomLevel = Math.Clamp(zoom, 50, 150);

    private void HandleSubmit(Dictionary<string, object> formData)
    {
        // Handle preview submission (show success message)
    }
}
```

### PreviewToolbar.razor

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="preview-toolbar">
    <div class="preview-toolbar-left">
        <div class="device-switcher">
            <button class="device-btn @(Device == PreviewDevice.Desktop ? "active" : "")"
                    @onclick="() => OnDeviceChange.InvokeAsync(PreviewDevice.Desktop)"
                    title="Desktop">
                <i class="bi bi-display"></i>
            </button>
            <button class="device-btn @(Device == PreviewDevice.Tablet ? "active" : "")"
                    @onclick="() => OnDeviceChange.InvokeAsync(PreviewDevice.Tablet)"
                    title="Tablet">
                <i class="bi bi-tablet"></i>
            </button>
            <button class="device-btn @(Device == PreviewDevice.Mobile ? "active" : "")"
                    @onclick="() => OnDeviceChange.InvokeAsync(PreviewDevice.Mobile)"
                    title="Mobile">
                <i class="bi bi-phone"></i>
            </button>
        </div>
        <div class="language-toggle">
            <button class="lang-btn @(Language == "en" ? "active" : "")"
                    @onclick="() => OnLanguageChange.InvokeAsync(\"en\")">EN</button>
            <button class="lang-btn @(Language == "fr" ? "active" : "")"
                    @onclick="() => OnLanguageChange.InvokeAsync(\"fr\")">FR</button>
        </div>
    </div>
    <div class="zoom-controls">
        <button class="zoom-btn" @onclick="() => OnZoomChange.InvokeAsync(Zoom - 10)">
            <i class="bi bi-dash"></i>
        </button>
        <span class="zoom-value">@Zoom%</span>
        <button class="zoom-btn" @onclick="() => OnZoomChange.InvokeAsync(Zoom + 10)">
            <i class="bi bi-plus"></i>
        </button>
    </div>
</div>

@code {
    [Parameter] public PreviewDevice Device { get; set; }
    [Parameter] public string Language { get; set; } = "en";
    [Parameter] public int Zoom { get; set; } = 100;
    [Parameter] public EventCallback<PreviewDevice> OnDeviceChange { get; set; }
    [Parameter] public EventCallback<string> OnLanguageChange { get; set; }
    [Parameter] public EventCallback<int> OnZoomChange { get; set; }
}
```

### RenderedForm.razor (Form Renderer)

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="rendered-form">
    <h1 class="form-title">@GetLabel(Module?.TitleEn, Module?.TitleFr)</h1>
    @if (!string.IsNullOrEmpty(GetLabel(Module?.DescriptionEn, Module?.DescriptionFr)))
    {
        <p class="form-description">@GetLabel(Module?.DescriptionEn, Module?.DescriptionFr)</p>
    }

    @foreach (var section in GroupedFields)
    {
        <div class="form-section">
            @if (section.Parent != null)
            {
                <h3 class="section-title">@GetLabel(section.Parent.LabelEn, section.Parent.LabelFr)</h3>
            }

            @foreach (var field in section.Fields)
            {
                <RenderedField
                    Field="@field"
                    Language="@Language"
                    Value="@GetFieldValue(field.Id)"
                    Errors="@GetFieldErrors(field.Id)"
                    OnValueChange="value => SetFieldValue(field.Id, value)" />
            }
        </div>
    }

    <div class="form-actions">
        <button class="btn btn-primary" @onclick="Submit">Submit</button>
        <button class="btn btn-secondary" @onclick="Reset">Reset</button>
    </div>
</div>

@code {
    [Parameter] public FormModuleSchema? Module { get; set; }
    [Parameter] public string Language { get; set; } = "en";
    [Parameter] public EventCallback<Dictionary<string, object>> OnSubmit { get; set; }

    private Dictionary<string, object> FormData { get; set; } = new();
    private Dictionary<string, List<string>> ValidationErrors { get; set; } = new();

    private IEnumerable<FieldSection> GroupedFields => GroupFieldsByParent();

    private string GetLabel(string? en, string? fr) =>
        Language == "fr" && !string.IsNullOrEmpty(fr) ? fr : en ?? "";

    private object? GetFieldValue(string fieldId) =>
        FormData.TryGetValue(fieldId, out var value) ? value : null;

    private IEnumerable<string> GetFieldErrors(string fieldId) =>
        ValidationErrors.TryGetValue(fieldId, out var errors) ? errors : [];

    private void SetFieldValue(string fieldId, object value)
    {
        FormData[fieldId] = value;
        ValidateField(fieldId);
    }

    private void ValidateField(string fieldId)
    {
        var field = Module?.Fields?.FirstOrDefault(f => f.Id == fieldId);
        if (field == null) return;

        var errors = new List<string>();
        var value = GetFieldValue(fieldId);

        // Required validation
        if (field.IsRequired && (value == null || string.IsNullOrEmpty(value.ToString())))
        {
            errors.Add(Language == "fr" ? "Ce champ est requis" : "This field is required");
        }

        // Type-specific validation
        switch (field.Type)
        {
            case FieldType.Email:
                if (value != null && !IsValidEmail(value.ToString()!))
                    errors.Add("Please enter a valid email address");
                break;
            case FieldType.Number:
                // Min/max validation from TypeConfig
                break;
        }

        ValidationErrors[fieldId] = errors;
    }

    private bool IsValidEmail(string email) =>
        System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    private IEnumerable<FieldSection> GroupFieldsByParent()
    {
        if (Module?.Fields == null) return [];

        var rootFields = Module.Fields.Where(f => f.ParentId == null);
        var sections = new List<FieldSection>();

        foreach (var field in rootFields.OrderBy(f => f.SortOrder))
        {
            if (field.Type == FieldType.Section)
            {
                var children = Module.Fields
                    .Where(f => f.ParentId == field.Id)
                    .OrderBy(f => f.SortOrder);
                sections.Add(new FieldSection(field, children));
            }
            else
            {
                // Add to "default" section
                var defaultSection = sections.FirstOrDefault(s => s.Parent == null);
                if (defaultSection == null)
                {
                    defaultSection = new FieldSection(null, new List<FormFieldSchema>());
                    sections.Insert(0, defaultSection);
                }
                ((List<FormFieldSchema>)defaultSection.Fields).Add(field);
            }
        }

        return sections;
    }

    private async Task Submit()
    {
        // Validate all fields
        foreach (var field in Module?.Fields ?? [])
        {
            ValidateField(field.Id);
        }

        if (!ValidationErrors.Values.Any(e => e.Any()))
        {
            await OnSubmit.InvokeAsync(FormData);
        }
    }

    private void Reset()
    {
        FormData.Clear();
        ValidationErrors.Clear();
    }

    private record FieldSection(FormFieldSchema? Parent, IEnumerable<FormFieldSchema> Fields);
}
```

### RenderedField.razor (Field Renderer)

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="form-field @(HasErrors ? "has-error" : "")">
    <label class="field-label">
        @Label
        @if (Field.IsRequired)
        {
            <span class="required">*</span>
        }
    </label>

    @switch (Field.Type)
    {
        case FieldType.Text:
        case FieldType.Email:
        case FieldType.Phone:
            <input type="@InputType"
                   class="field-input @(HasErrors ? "error" : "")"
                   value="@Value"
                   placeholder="@Placeholder"
                   @oninput="e => OnValueChange.InvokeAsync(e.Value)" />
            break;

        case FieldType.Number:
            <input type="number"
                   class="field-input"
                   value="@Value"
                   @oninput="e => OnValueChange.InvokeAsync(e.Value)" />
            break;

        case FieldType.TextArea:
            <textarea class="field-input field-textarea"
                      @oninput="e => OnValueChange.InvokeAsync(e.Value)">@Value</textarea>
            break;

        case FieldType.Select:
            <select class="field-input form-select"
                    @onchange="e => OnValueChange.InvokeAsync(e.Value)">
                <option value="">@SelectPlaceholder</option>
                @foreach (var option in Field.Options ?? [])
                {
                    <option value="@option.Value" selected="@(Value?.ToString() == option.Value)">
                        @GetOptionLabel(option)
                    </option>
                }
            </select>
            break;

        case FieldType.Radio:
            <div class="radio-group">
                @foreach (var option in Field.Options ?? [])
                {
                    <label class="radio-item">
                        <input type="radio"
                               name="@Field.Id"
                               value="@option.Value"
                               checked="@(Value?.ToString() == option.Value)"
                               @onchange="e => OnValueChange.InvokeAsync(option.Value)" />
                        @GetOptionLabel(option)
                    </label>
                }
            </div>
            break;

        case FieldType.Checkbox:
            <div class="checkbox-group">
                @foreach (var option in Field.Options ?? [])
                {
                    <label class="checkbox-item">
                        <input type="checkbox"
                               value="@option.Value"
                               checked="@IsChecked(option.Value)"
                               @onchange="e => ToggleCheckbox(option.Value)" />
                        @GetOptionLabel(option)
                    </label>
                }
            </div>
            break;

        case FieldType.Toggle:
            <div class="toggle-field">
                <input type="checkbox"
                       class="toggle-input"
                       checked="@(Value?.ToString() == "true")"
                       @onchange="e => OnValueChange.InvokeAsync(((bool)e.Value!).ToString())" />
            </div>
            break;

        case FieldType.Date:
            <input type="date"
                   class="field-input"
                   value="@Value"
                   @oninput="e => OnValueChange.InvokeAsync(e.Value)" />
            break;

        case FieldType.Time:
            <input type="time"
                   class="field-input"
                   value="@Value"
                   @oninput="e => OnValueChange.InvokeAsync(e.Value)" />
            break;
    }

    @if (!string.IsNullOrEmpty(HelpText))
    {
        <div class="field-help">@HelpText</div>
    }

    @if (HasErrors)
    {
        @foreach (var error in Errors)
        {
            <div class="field-error">
                <i class="bi bi-exclamation-circle"></i>
                @error
            </div>
        }
    }
</div>

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = default!;
    [Parameter] public string Language { get; set; } = "en";
    [Parameter] public object? Value { get; set; }
    [Parameter] public IEnumerable<string> Errors { get; set; } = [];
    [Parameter] public EventCallback<object> OnValueChange { get; set; }

    private bool HasErrors => Errors.Any();

    private string Label => Language == "fr" && !string.IsNullOrEmpty(Field.LabelFr)
        ? Field.LabelFr
        : Field.LabelEn ?? Field.Id;

    private string? Placeholder => Language == "fr"
        ? Field.PlaceholderFr
        : Field.PlaceholderEn;

    private string? HelpText => Language == "fr"
        ? Field.HelpTextFr
        : Field.HelpTextEn;

    private string SelectPlaceholder => Language == "fr"
        ? "Sélectionner..."
        : "Select...";

    private string InputType => Field.Type switch
    {
        FieldType.Email => "email",
        FieldType.Phone => "tel",
        _ => "text"
    };

    private string GetOptionLabel(FieldOption option) =>
        Language == "fr" && !string.IsNullOrEmpty(option.LabelFr)
            ? option.LabelFr
            : option.LabelEn;

    private bool IsChecked(string value)
    {
        if (Value is IEnumerable<string> list)
            return list.Contains(value);
        return false;
    }

    private async Task ToggleCheckbox(string value)
    {
        var list = (Value as IEnumerable<string>)?.ToList() ?? new List<string>();
        if (list.Contains(value))
            list.Remove(value);
        else
            list.Add(value);
        await OnValueChange.InvokeAsync(list);
    }
}
```

---

## Enums

```csharp
public enum PreviewDevice
{
    Desktop,
    Tablet,
    Mobile
}
```

---

## CSS Styles

See mockup HTML for complete CSS. Key styles include:
- Device frames with browser chrome
- Responsive width constraints (768px tablet, 375px mobile)
- Zoom transform
- Form field rendering styles
- Error state styling

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the Form Preview component for my Blazor form editor.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Preview/

## Components to Create:

### 1. FormPreview.razor
Main container with:
- Device simulator (Desktop/Tablet/Mobile)
- Language switcher (EN/FR)
- Zoom controls (50%-150%)
- Browser-like frame

### 2. PreviewToolbar.razor
Controls for device, language, zoom

### 3. RenderedForm.razor
Renders the form from FormModuleSchema:
- Groups fields by section
- Handles validation
- Collects form data

### 4. RenderedField.razor
Renders individual field based on FieldType:
- Text, Email, Phone, Number
- TextArea, RichText
- Select, Radio, Checkbox
- Date, Time, DateTime
- Toggle, File

### Features:
- Real-time validation
- Bilingual labels (EN/FR)
- Help text display
- Error message display
- Required field indicators

Please implement complete, production-ready code with CSS.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `FormPreview-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for Desktop view testing
- Tablet view (768px) testing
- Mobile view (375px) testing
- Language switch (EN/FR) testing
- Zoom controls (+/-) testing (50%-150%)
- Browser frame chrome display testing
- All field type rendering testing (Text, Email, Number, Select, Radio, Checkbox, Date, etc.)
- Required field indicator testing
- Required field validation testing
- Email format validation testing
- Help text display testing
- Error message display testing
- Submit button functionality testing
- Reset button functionality testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- PreviewDevice enum file creation
- FieldSection helper record setup
- Additional field type renderers (File, Image, Signature, RichText)
- Type-specific validation from TypeConfig
- CSS file imports for rendered form styling
- Integration with ViewSwitcher as Preview view

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Device switcher shows Desktop/Tablet/Mobile buttons
- [ ] Desktop view full width
- [ ] Tablet view constrained to 768px
- [ ] Mobile view constrained to 375px
- [ ] EN/FR language toggle works
- [ ] Zoom decrease button works
- [ ] Zoom increase button works
- [ ] Zoom percentage displays correctly
- [ ] Browser chrome shows red/yellow/green dots
- [ ] URL bar displays form ID
- [ ] Preview badge displays
- [ ] Form title renders (EN or FR)
- [ ] Form description renders
- [ ] Text input renders and accepts input
- [ ] Email input renders and validates
- [ ] Number input renders
- [ ] Select dropdown renders with options
- [ ] Radio buttons render with options
- [ ] Checkbox group renders with options
- [ ] Date input renders
- [ ] Toggle switch renders
- [ ] Required asterisk shows for required fields
- [ ] Required validation shows error message
- [ ] Help text displays below fields
- [ ] Error messages display with icon
- [ ] Submit validates all fields
- [ ] Reset clears form data
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Desktop view displays correctly
- [ ] Tablet view (768px) works
- [ ] Mobile view (375px) works
- [ ] EN/FR language switch works
- [ ] Zoom controls work
- [ ] All field types render
- [ ] Required validation works
- [ ] Email validation works
- [ ] Error messages display
- [ ] Help text displays
- [ ] Submit button works
- [ ] Reset button works
- [ ] Dark mode styling correct

---

## Notes

- Preview is read-only for schema, editable for form values
- Validation should match runtime validation
- Consider adding print preview
- Consider adding QR code for mobile testing
- Browser frame adds context but can be toggled off
