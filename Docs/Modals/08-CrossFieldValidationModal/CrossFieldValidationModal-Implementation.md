# CrossFieldValidationModal Component - Implementation Plan

> **Component**: `CrossFieldValidationModal.razor`
> **Location**: `Src/VisualEditorOpus/Components/Editor/Modals/CrossFieldValidationModal.razor`
> **Priority**: Medium
> **Estimated Effort**: 3-4 hours
> **Depends On**: ModalBase.razor

---

## Overview

CrossFieldValidationModal allows users to create validation rules that span multiple fields, such as "at least one phone number required" or "if address is filled, city must be filled".

---

## Core.V4 Schema Reference

### FieldSetValidation
```csharp
public record FieldSetValidation
{
    /// <summary>
    /// The type of validation: "AtLeastOne", "AllOrNone", "MutuallyExclusive"
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The IDs of the fields involved in this validation
    /// </summary>
    public required string[] FieldIds { get; init; }

    /// <summary>
    /// Error message (English)
    /// </summary>
    public string? ErrorMessageEn { get; init; }

    /// <summary>
    /// Error message (French)
    /// </summary>
    public string? ErrorMessageFr { get; init; }
}
```

---

## Validation Types

| Type | Description | Logic |
|------|-------------|-------|
| `AtLeastOne` | At least one field must have a value | `fields.Any(f => !IsEmpty(f))` |
| `AllOrNone` | Either all fields filled or all empty | `fields.All(filled) OR fields.All(empty)` |
| `MutuallyExclusive` | Only one field can have a value | `fields.Count(f => !IsEmpty(f)) <= 1` |

---

## Component API

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

[Parameter] public FieldSetValidation? ExistingValidation { get; set; }
[Parameter] public FormModuleSchema CurrentModule { get; set; } = default!;
[Parameter] public EventCallback<FieldSetValidation> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

---

## UI Structure

1. **Validation Type Selection** - Card selector with icon, name, description
2. **Field Selection** - Checkboxes for all fields in module
3. **Selected Fields Display** - Tags showing selected fields
4. **Error Messages** - EN/FR textareas
5. **Rule Preview** - Human-readable description of the rule

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the CrossFieldValidationModal component for my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Editor/Modals/
- Depends on: ModalBase.razor

## Schema Reference (DO NOT recreate)
```csharp
public record FieldSetValidation
{
    public required string Type { get; init; }        // "AtLeastOne", "AllOrNone", "MutuallyExclusive"
    public required string[] FieldIds { get; init; }
    public string? ErrorMessageEn { get; init; }
    public string? ErrorMessageFr { get; init; }
}
```

## Files to Create

### CrossFieldValidationModal.razor
Modal with:
1. Type selection cards (AtLeastOne, AllOrNone, MutuallyExclusive)
2. Field multi-select with checkboxes
3. Selected fields display as tags
4. Error message textareas (EN/FR)
5. Rule preview section
6. Save/Cancel buttons

### CrossFieldValidationModal.razor.cs
Parameters:
```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public FieldSetValidation? ExistingValidation { get; set; }
[Parameter] public FormModuleSchema CurrentModule { get; set; } = default!;
[Parameter] public EventCallback<FieldSetValidation> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

State:
```csharp
private string selectedType = "AtLeastOne";
private HashSet<string> selectedFieldIds = new();
private string? errorMessageEn;
private string? errorMessageFr;
```

## Validation Type Cards
```csharp
private static readonly List<ValidationTypeInfo> ValidationTypes = new()
{
    new("AtLeastOne", "At Least One", "One or more fields must have a value", "bi-1-circle"),
    new("AllOrNone", "All or None", "Either all fields filled or all empty", "bi-check-all"),
    new("MutuallyExclusive", "Mutually Exclusive", "Only one field can have a value", "bi-x-circle")
};
```

## Validation
- At least 2 fields must be selected
- Type must be selected

## Preview Text Generator
```csharp
private string GetPreviewText()
{
    var fields = string.Join(", ", selectedFieldIds);
    return selectedType switch
    {
        "AtLeastOne" => $"AT LEAST ONE of: {fields} must have a value",
        "AllOrNone" => $"Either ALL or NONE of: {fields} must have values",
        "MutuallyExclusive" => $"Only ONE of: {fields} can have a value",
        _ => ""
    };
}
```

## CSS Classes
- .type-cards (grid of type selection cards)
- .type-card, .type-card.selected
- .field-selector (scrollable checkbox list)
- .field-option, .field-option.selected
- .selected-fields (tag container)
- .field-tag
- .validation-preview

Please implement with complete, production-ready code.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `CrossFieldValidationModal-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for validation type selection
- "At Least One" type card selection testing
- "All or None" type card selection testing
- "Mutually Exclusive" type card selection testing
- Field multi-select checkbox testing
- Selected fields tag display testing
- Tag removal testing
- Error message EN/FR input testing
- Rule preview text verification
- Save validation (minimum 2 fields required)
- Edit existing validation testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Integration with module validation section
- FieldSetValidation array management in module schema
- ValidationTypeInfo static data setup
- Field list building from CurrentModule.Fields
- Preview text generator implementation
- CSS imports for type cards and field selector

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens with ModalBase
- [ ] Type cards display with icons and descriptions
- [ ] Type card selection highlights correctly
- [ ] Only one type can be selected at a time
- [ ] Field checkbox list shows all module fields
- [ ] Multiple fields can be selected
- [ ] Selected fields display as tags
- [ ] Tags can be removed by clicking X
- [ ] Error message EN textarea binds correctly
- [ ] Error message FR textarea binds correctly
- [ ] Preview updates dynamically when type changes
- [ ] Preview updates dynamically when fields change
- [ ] Save button disabled with fewer than 2 fields
- [ ] Save produces valid FieldSetValidation object
- [ ] Edit mode loads existing validation values
- [ ] Cancel closes modal without saving
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Type cards select correctly
- [ ] Field checkboxes toggle
- [ ] Selected fields show as tags
- [ ] Tags can be removed
- [ ] Error messages bind correctly
- [ ] Preview updates dynamically
- [ ] Save disabled with < 2 fields
- [ ] Edit mode loads existing values
