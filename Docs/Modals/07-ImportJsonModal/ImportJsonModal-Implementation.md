# ImportJsonModal Component - Implementation Plan

> **Component**: `ImportJsonModal.razor`
> **Location**: `Src/VisualEditorOpus/Components/Editor/Modals/ImportJsonModal.razor`
> **Priority**: High
> **Estimated Effort**: 2-3 hours
> **Depends On**: ModalBase.razor

---

## Overview

ImportJsonModal allows users to import form module or workflow schemas from JSON files or pasted content. It validates the JSON, shows a preview, and provides options for merge vs. replace.

---

## Features

| Feature | Description |
|---------|-------------|
| File Upload | Drag & drop or click to select JSON file |
| Paste JSON | Tab to paste JSON directly |
| Validation | Real-time JSON parsing and schema validation |
| Preview | Shows detected type, title, field count |
| Import Options | Replace or merge with existing |

---

## Component API

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

[Parameter] public ImportTargetType TargetType { get; set; } = ImportTargetType.Module;
[Parameter] public EventCallback<FormModuleSchema> OnImportModule { get; set; }
[Parameter] public EventCallback<FormWorkflowSchema> OnImportWorkflow { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }

public enum ImportTargetType { Module, Workflow }
public enum ImportMode { Replace, Merge }
```

---

## Validation Logic

```csharp
private async Task ValidateJson(string json)
{
    try
    {
        // Try to deserialize as FormModuleSchema first
        var module = JsonSerializer.Deserialize<FormModuleSchema>(json, jsonOptions);
        if (module != null && !string.IsNullOrEmpty(module.TitleEn))
        {
            detectedType = ImportTargetType.Module;
            parsedModule = module;
            isValid = true;
            validationMessage = $"Valid FormModuleSchema with {module.Fields?.Length ?? 0} fields";
            return;
        }
    }
    catch { }

    try
    {
        // Try workflow
        var workflow = JsonSerializer.Deserialize<FormWorkflowSchema>(json, jsonOptions);
        if (workflow != null && !string.IsNullOrEmpty(workflow.TitleEn))
        {
            detectedType = ImportTargetType.Workflow;
            parsedWorkflow = workflow;
            isValid = true;
            validationMessage = $"Valid FormWorkflowSchema with {workflow.ModuleIds?.Length ?? 0} modules";
            return;
        }
    }
    catch { }

    isValid = false;
    validationMessage = "Invalid JSON or unrecognized schema";
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the ImportJsonModal component for my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Editor/Modals/
- Core schemas: FormModuleSchema, FormWorkflowSchema from DynamicForms.Core.V4
- Depends on: ModalBase.razor

## Files to Create

### ImportJsonModal.razor
Create modal with:
1. Tab bar: "Upload File" | "Paste JSON"
2. File upload tab: Drop zone with drag/drop and file input
3. Paste tab: Large textarea for JSON
4. Validation result (success/error with message)
5. Import preview (type, title, field count)
6. Import options (Replace/Merge radio buttons)
7. Import/Cancel buttons

### ImportJsonModal.razor.cs
Parameters:
```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public EventCallback<FormModuleSchema> OnImportModule { get; set; }
[Parameter] public EventCallback<FormWorkflowSchema> OnImportWorkflow { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

State:
```csharp
private string jsonContent = "";
private bool isValid = false;
private string? validationMessage;
private string? detectedType; // "module" or "workflow"
private FormModuleSchema? parsedModule;
private FormWorkflowSchema? parsedWorkflow;
private ImportMode importMode = ImportMode.Replace;
private string activeTab = "file";
```

Methods:
- HandleFileSelected(InputFileChangeEventArgs e)
- ValidateJson()
- HandleImport()

## File Upload with JS Interop
For drag/drop, you may need minimal JS interop or use InputFile component.

## CSS Classes
- .drop-zone (dashed border area)
- .drop-zone.drag-over (highlight on drag)
- .import-tabs, .import-tab
- .json-input (monospace textarea)
- .validation-result.success, .validation-result.error
- .import-preview
- .import-options

Please implement with complete code. Use System.Text.Json for deserialization.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `ImportJsonModal-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for file upload testing (click to browse)
- Drag and drop file upload testing
- Tab switching between "Upload File" and "Paste JSON"
- Paste JSON validation testing
- Valid module JSON import testing
- Valid workflow JSON import testing
- Invalid JSON error display testing
- Import preview information verification
- Import mode (Replace/Merge) selection testing
- Import callback invocation testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Integration with empty canvas "Import JSON" button
- Integration with toolbar/menu import option
- Merge logic implementation (if Replace/Merge both supported)
- JS interop for drag/drop (if needed beyond InputFile)
- FormModuleSchema and FormWorkflowSchema deserialization testing
- Error handling for malformed JSON

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens with ModalBase
- [ ] "Upload File" tab is default active
- [ ] "Paste JSON" tab can be selected
- [ ] Tab switching works correctly
- [ ] File input allows JSON file selection
- [ ] Drop zone has dashed border styling
- [ ] Drop zone highlights on drag over
- [ ] Dropped file content is read correctly
- [ ] Paste textarea accepts JSON input
- [ ] Validation runs on content change
- [ ] Valid module JSON shows success with field count
- [ ] Valid workflow JSON shows success with module count
- [ ] Invalid JSON shows error message
- [ ] Import preview displays detected type and title
- [ ] Replace/Merge radio buttons work
- [ ] Import button disabled when invalid
- [ ] Import button invokes correct callback (Module or Workflow)
- [ ] Cancel closes modal
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] File upload works (click to browse)
- [ ] Drag and drop highlights zone
- [ ] Tab switching works
- [ ] Paste JSON validates on button click
- [ ] Valid module JSON shows success
- [ ] Valid workflow JSON shows success
- [ ] Invalid JSON shows error
- [ ] Preview shows correct info
- [ ] Import mode radio buttons work
- [ ] Import button invokes correct callback
