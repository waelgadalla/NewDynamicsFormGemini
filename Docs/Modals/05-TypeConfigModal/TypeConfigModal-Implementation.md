# TypeConfigModal Component - Implementation Plan

> **Component**: `TypeConfigModal.razor` + Individual Config Editors
> **Location**: `Src/VisualEditorOpus/Components/Editor/Modals/TypeConfig/`
> **Priority**: High (Required for advanced field types)
> **Estimated Effort**: 6-8 hours
> **Depends On**: ModalBase.razor

---

## Overview

TypeConfigModal is a dynamic modal that displays the appropriate configuration editor based on the field type. It handles four different TypeConfig types from Core.V4: DateConfig, FileUploadConfig, AutoCompleteConfig, and DataGridConfig.

---

## Core.V4 Schema Reference

### Base Type (Polymorphic)
```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(AutoCompleteConfig), typeDiscriminator: "autocomplete")]
[JsonDerivedType(typeof(DataGridConfig), typeDiscriminator: "datagrid")]
[JsonDerivedType(typeof(FileUploadConfig), typeDiscriminator: "fileupload")]
[JsonDerivedType(typeof(DateConfig), typeDiscriminator: "date")]
public abstract record FieldTypeConfig { }
```

### DateConfig
```csharp
public record DateConfig : FieldTypeConfig
{
    public bool AllowFuture { get; init; } = true;
    public bool AllowPast { get; init; } = true;
    public string? MinDate { get; init; }  // ISO 8601 or "Now", "Now+30d"
    public string? MaxDate { get; init; }
}
```

### FileUploadConfig
```csharp
public record FileUploadConfig : FieldTypeConfig
{
    public string[] AllowedExtensions { get; init; } = Array.Empty<string>();
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024; // 10MB
    public bool AllowMultiple { get; init; }
    public bool ScanRequired { get; init; } = true;
}
```

### AutoCompleteConfig
```csharp
public record AutoCompleteConfig : FieldTypeConfig
{
    public required string DataSourceUrl { get; init; }
    public string QueryParameter { get; init; } = "q";
    public int MinCharacters { get; init; } = 3;
    public required string ValueField { get; init; }
    public required string DisplayField { get; init; }
    public string? ItemTemplate { get; init; }
}
```

### DataGridConfig
```csharp
public record DataGridConfig : FieldTypeConfig
{
    public bool AllowAdd { get; init; } = true;
    public bool AllowEdit { get; init; } = true;
    public bool AllowDelete { get; init; } = true;
    public int? MaxRows { get; init; }
    public string EditorMode { get; init; } = "Modal"; // "Modal" | "Inline"
    public FormFieldSchema[] Columns { get; init; } = Array.Empty<FormFieldSchema>();
}
```

---

## File Structure

```
Components/Editor/Modals/TypeConfig/
├── TypeConfigModal.razor           # Main modal wrapper
├── TypeConfigModal.razor.cs
├── DateConfigEditor.razor          # Date field config
├── DateConfigEditor.razor.cs
├── FileUploadConfigEditor.razor    # File upload config
├── FileUploadConfigEditor.razor.cs
├── AutoCompleteConfigEditor.razor  # AutoComplete config
├── AutoCompleteConfigEditor.razor.cs
├── DataGridConfigEditor.razor      # DataGrid config
├── DataGridConfigEditor.razor.cs
├── DataGridColumnEditor.razor      # Sub-component for columns
└── TypeConfig.razor.css            # Shared styles
```

---

## Component API

### TypeConfigModal Parameters

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

[Parameter] public string FieldType { get; set; } = "";  // "DatePicker", "FileUpload", etc.
[Parameter] public FieldTypeConfig? ExistingConfig { get; set; }
[Parameter] public EventCallback<FieldTypeConfig> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

### Individual Editor Parameters

Each editor receives:
```csharp
[Parameter] public TConfig Config { get; set; }  // The specific config type
[Parameter] public EventCallback<TConfig> ConfigChanged { get; set; }
```

---

## Field Type to Config Mapping

| FieldType | Config Type | Editor Component |
|-----------|-------------|------------------|
| `DatePicker` | `DateConfig` | `DateConfigEditor` |
| `TimePicker` | `DateConfig` | `DateConfigEditor` |
| `DateTimePicker` | `DateConfig` | `DateConfigEditor` |
| `FileUpload` | `FileUploadConfig` | `FileUploadConfigEditor` |
| `AutoComplete` | `AutoCompleteConfig` | `AutoCompleteConfigEditor` |
| `DataGrid` | `DataGridConfig` | `DataGridConfigEditor` |

---

## Individual Editor Specifications

### 1. DateConfigEditor

**UI Elements:**
- Toggle: Allow Future Dates (default: true)
- Toggle: Allow Past Dates (default: true)
- Dropdown: Minimum Date (None, Today, Today-30d, Today-1y, Custom)
- Dropdown: Maximum Date (None, Today, Today+30d, Today+1y, Custom)
- Date input: Custom Min/Max (shown when "Custom" selected)

**Special Date Values:**
- `"Now"` - Current date
- `"Now+Nd"` - N days from now
- `"Now-Nd"` - N days ago
- `"Now+Nm"` - N months from now
- `"Now+Ny"` - N years from now

---

### 2. FileUploadConfigEditor

**UI Elements:**
- Tag input: Allowed Extensions (e.g., .pdf, .doc, .jpg)
- Number + Select: Max File Size (value + unit: KB/MB/GB)
- Toggle: Allow Multiple Files (default: false)
- Toggle: Virus Scan Required (default: true)

**Common Extension Presets:**
- Documents: .pdf, .doc, .docx, .xls, .xlsx, .ppt, .pptx
- Images: .jpg, .jpeg, .png, .gif, .webp
- All Common: Both above

---

### 3. AutoCompleteConfigEditor

**UI Elements:**
- Text input: Data Source URL (required)
- Text input: Query Parameter (default: "q")
- Number input: Minimum Characters (default: 3, range: 1-10)
- Text input: Value Field (required)
- Text input: Display Field (required)
- Text input: Item Template (optional, Handlebars syntax)

**Validation:**
- URL must start with "/" or "http"
- Value and Display fields are required

---

### 4. DataGridConfigEditor (Most Complex)

**UI Elements:**
- Toggle: Allow Add (default: true)
- Toggle: Allow Edit (default: true)
- Toggle: Allow Delete (default: true)
- Number input: Max Rows (optional)
- Select: Editor Mode (Modal/Inline)
- Column List: Sortable list of column definitions
- Add Column Button

**Column Editor (DataGridColumnEditor):**
Each column is essentially a mini FormFieldSchema:
- Text input: Column ID
- Text input: Label EN/FR
- Select: Field Type (TextBox, Number, DropDown, etc.)
- Toggle: Required
- Width setting

This is essentially a mini form builder within the modal!

---

## Implementation Notes

### Dynamic Component Loading
```razor
@* In TypeConfigModal.razor *@
<ModalBase @bind-IsOpen="IsOpen" Title="@GetTitle()" Icon="@GetIcon()" Size="ModalSize.Medium">
    @switch (FieldType)
    {
        case "DatePicker":
        case "TimePicker":
        case "DateTimePicker":
            <DateConfigEditor @bind-Config="dateConfig" />
            break;

        case "FileUpload":
            <FileUploadConfigEditor @bind-Config="fileUploadConfig" />
            break;

        case "AutoComplete":
            <AutoCompleteConfigEditor @bind-Config="autoCompleteConfig" />
            break;

        case "DataGrid":
            <DataGridConfigEditor @bind-Config="dataGridConfig" />
            break;

        default:
            <p>No configuration available for this field type.</p>
            break;
    }

    <FooterContent>
        <button class="btn btn-ghost" @onclick="HandleCancel">Cancel</button>
        <button class="btn btn-primary" @onclick="HandleSave">
            <i class="bi bi-check-lg"></i> Save Configuration
        </button>
    </FooterContent>
</ModalBase>
```

### Type Conversion on Load/Save
```csharp
protected override void OnParametersSet()
{
    // Convert existing config to mutable form
    if (ExistingConfig is DateConfig dc)
        dateConfig = new DateConfigModel(dc);
    else if (ExistingConfig is FileUploadConfig fc)
        fileUploadConfig = new FileUploadConfigModel(fc);
    // ... etc.
}

private async Task HandleSave()
{
    FieldTypeConfig result = FieldType switch
    {
        "DatePicker" or "TimePicker" or "DateTimePicker" => dateConfig.ToConfig(),
        "FileUpload" => fileUploadConfig.ToConfig(),
        "AutoComplete" => autoCompleteConfig.ToConfig(),
        "DataGrid" => dataGridConfig.ToConfig(),
        _ => throw new InvalidOperationException()
    };

    await OnSave.InvokeAsync(result);
    await IsOpenChanged.InvokeAsync(false);
}
```

---

## Testing Checklist

### DateConfigEditor
- [ ] Toggle Allow Future works
- [ ] Toggle Allow Past works
- [ ] Preset date options populate correctly
- [ ] Custom date input appears when selected
- [ ] Config produces valid DateConfig

### FileUploadConfigEditor
- [ ] Add extension by typing + Enter
- [ ] Remove extension by clicking X
- [ ] Preset buttons add correct extensions
- [ ] File size with unit converts to bytes
- [ ] Toggle multiple files works
- [ ] Toggle virus scan works

### AutoCompleteConfigEditor
- [ ] All text inputs bind correctly
- [ ] Validation shows error for missing required fields
- [ ] Min characters has proper range

### DataGridConfigEditor
- [ ] All toggles work
- [ ] Columns can be added
- [ ] Columns can be edited (opens sub-modal)
- [ ] Columns can be deleted
- [ ] Columns can be reordered (drag or up/down)
- [ ] Editor mode dropdown works

---

## Claude Implementation Prompt

Copy and paste the following prompt to Claude to implement this component:

---

### PROMPT START

```
I need you to implement the TypeConfigModal system for my Blazor application. This is a dynamic modal that shows different configuration editors based on field type.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Editor/Modals/TypeConfig/
- Core Library: DynamicForms.Core.V4 (contains FieldTypeConfig and derived types)
- Depends on: ModalBase.razor (already implemented)

## Schema Types (from DynamicForms.Core.V4 - DO NOT recreate)

```csharp
// Base polymorphic type
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(AutoCompleteConfig), typeDiscriminator: "autocomplete")]
[JsonDerivedType(typeof(DataGridConfig), typeDiscriminator: "datagrid")]
[JsonDerivedType(typeof(FileUploadConfig), typeDiscriminator: "fileupload")]
[JsonDerivedType(typeof(DateConfig), typeDiscriminator: "date")]
public abstract record FieldTypeConfig { }

public record DateConfig : FieldTypeConfig
{
    public bool AllowFuture { get; init; } = true;
    public bool AllowPast { get; init; } = true;
    public string? MinDate { get; init; }
    public string? MaxDate { get; init; }
}

public record FileUploadConfig : FieldTypeConfig
{
    public string[] AllowedExtensions { get; init; } = Array.Empty<string>();
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;
    public bool AllowMultiple { get; init; }
    public bool ScanRequired { get; init; } = true;
}

public record AutoCompleteConfig : FieldTypeConfig
{
    public required string DataSourceUrl { get; init; }
    public string QueryParameter { get; init; } = "q";
    public int MinCharacters { get; init; } = 3;
    public required string ValueField { get; init; }
    public required string DisplayField { get; init; }
    public string? ItemTemplate { get; init; }
}

public record DataGridConfig : FieldTypeConfig
{
    public bool AllowAdd { get; init; } = true;
    public bool AllowEdit { get; init; } = true;
    public bool AllowDelete { get; init; } = true;
    public int? MaxRows { get; init; }
    public string EditorMode { get; init; } = "Modal";
    public FormFieldSchema[] Columns { get; init; } = Array.Empty<FormFieldSchema>();
}
```

## Files to Create

### 1. TypeConfigModal.razor + TypeConfigModal.razor.cs
Main wrapper modal that:
- Accepts FieldType parameter to determine which editor to show
- Uses switch statement to render appropriate editor
- Handles Save/Cancel with type conversion

Parameters:
```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public string FieldType { get; set; } = "";
[Parameter] public FieldTypeConfig? ExistingConfig { get; set; }
[Parameter] public EventCallback<FieldTypeConfig> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

### 2. DateConfigEditor.razor
Editor for DateConfig with:
- Toggle: Allow Future Dates
- Toggle: Allow Past Dates
- Select: Minimum Date (None, Now, Now-30d, Now-1y, Custom)
- Select: Maximum Date (None, Now, Now+30d, Now+1y, Custom)
- Date input for custom values

### 3. FileUploadConfigEditor.razor
Editor for FileUploadConfig with:
- Tag input for allowed extensions
- Preset buttons: "Documents", "Images", "All Common"
- Number + unit dropdown for max file size
- Toggle: Allow Multiple
- Toggle: Virus Scan Required

### 4. AutoCompleteConfigEditor.razor
Editor for AutoCompleteConfig with:
- Text: Data Source URL (required)
- Text: Query Parameter
- Number: Min Characters (1-10)
- Text: Value Field (required)
- Text: Display Field (required)
- Text: Item Template (optional)

### 5. DataGridConfigEditor.razor
Editor for DataGridConfig with:
- Toggle: Allow Add/Edit/Delete
- Number: Max Rows
- Select: Editor Mode (Modal/Inline)
- List of columns with Add/Edit/Delete/Reorder
- For MVP: Simple column list without full sub-editor

### 6. TypeConfig.razor.css
Shared styles for all editors

## UI Component Patterns

### Toggle Switch
```razor
<div class="toggle-group">
    <div>
        <div class="toggle-label">@Label</div>
        <div class="toggle-desc">@Description</div>
    </div>
    <div class="toggle @(Value ? "active" : "")" @onclick="() => Value = !Value"></div>
</div>
```

### Tag Input (for extensions)
```razor
<div class="tag-input-wrapper">
    @foreach (var tag in Tags)
    {
        <span class="tag">
            @tag
            <button @onclick="() => RemoveTag(tag)">&times;</button>
        </span>
    }
    <input type="text" @bind="newTag" @onkeydown="HandleTagKeyDown" placeholder="Type and press Enter">
</div>
```

## CSS Classes to Create

```css
.toggle-group { /* Flex container for label + toggle */ }
.toggle { /* Switch base */ }
.toggle.active { /* Switch active state */ }
.tag-input-wrapper { /* Container for tags */ }
.tag { /* Individual tag pill */ }
.tag-input { /* Input for new tags */ }
.config-preview { /* JSON preview section */ }
.column-list { /* DataGrid column list */ }
.column-item { /* Individual column row */ }
```

## Example Usage

```razor
<TypeConfigModal @bind-IsOpen="showTypeConfig"
                 FieldType="@selectedField.FieldType"
                 ExistingConfig="@selectedField.TypeConfig"
                 OnSave="HandleTypeConfigSave"
                 OnCancel="() => showTypeConfig = false" />
```

## Important Notes

1. Use ModalBase with Size="ModalSize.Medium" (or Large for DataGrid)
2. Create mutable model classes for each config type (records are immutable)
3. Convert from schema to mutable model on load
4. Convert from mutable model to schema on save
5. DataGrid columns are complex - for MVP, show read-only list with "Edit in Field Builder" button
6. All editors should show a "Configuration Preview" section with JSON output

Please implement TypeConfigModal.razor, DateConfigEditor.razor, FileUploadConfigEditor.razor, AutoCompleteConfigEditor.razor, and DataGridConfigEditor.razor with complete, production-ready code.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `TypeConfigModal-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for opening TypeConfigModal for each field type
- DateConfigEditor testing (toggles, date presets, custom dates)
- FileUploadConfigEditor testing (extension tags, presets, file size with units)
- AutoCompleteConfigEditor testing (all text inputs, validation)
- DataGridConfigEditor testing (toggles, column list, editor mode)
- Dynamic editor switching based on field type
- Save/Cancel behavior testing for each editor
- Configuration preview JSON verification
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Integration with RightSidebar "Configure" button
- HasTypeConfig helper function implementation
- Mutable model classes creation for each config type
- Folder structure setup (Components/Editor/Modals/TypeConfig/)
- DataGrid column sub-editor integration (if full feature needed)
- CSS imports for shared TypeConfig.razor.css

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] TypeConfigModal opens with correct editor based on FieldType
- [ ] DateConfigEditor: Allow Future toggle works
- [ ] DateConfigEditor: Allow Past toggle works
- [ ] DateConfigEditor: Preset date options populate correctly
- [ ] DateConfigEditor: Custom date input appears when selected
- [ ] FileUploadConfigEditor: Add extension by typing + Enter
- [ ] FileUploadConfigEditor: Remove extension by clicking X
- [ ] FileUploadConfigEditor: Preset buttons add correct extensions
- [ ] FileUploadConfigEditor: File size converts to bytes correctly
- [ ] FileUploadConfigEditor: Allow Multiple toggle works
- [ ] FileUploadConfigEditor: Virus Scan toggle works
- [ ] AutoCompleteConfigEditor: All text inputs bind correctly
- [ ] AutoCompleteConfigEditor: Validation for required fields
- [ ] AutoCompleteConfigEditor: Min characters range 1-10
- [ ] DataGridConfigEditor: Allow Add/Edit/Delete toggles work
- [ ] DataGridConfigEditor: Max Rows input works
- [ ] DataGridConfigEditor: Editor Mode dropdown works
- [ ] DataGridConfigEditor: Column list displays
- [ ] All editors produce valid FieldTypeConfig on save
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Integration with RightSidebar

After implementing TypeConfigModal, add a "Configure" button to RightSidebar for fields that support TypeConfig:

```razor
@if (HasTypeConfig(SelectedField.FieldType))
{
    <button class="btn btn-sm btn-outline" @onclick="OpenTypeConfig">
        <i class="bi bi-sliders"></i>
        @(SelectedField.TypeConfig != null ? "Edit Configuration" : "Configure")
    </button>

    @if (SelectedField.TypeConfig != null)
    {
        <span class="badge badge-info">Configured</span>
    }
}

@code {
    private bool HasTypeConfig(string fieldType) => fieldType is
        "DatePicker" or "TimePicker" or "DateTimePicker" or
        "FileUpload" or "AutoComplete" or "DataGrid";
}
```
