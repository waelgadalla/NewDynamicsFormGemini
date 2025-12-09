# ConditionBuilderModal Component - Implementation Plan

> **Component**: `ConditionBuilderModal.razor`
> **Location**: `Src/VisualEditorOpus/Components/Editor/Modals/ConditionBuilderModal.razor`
> **Priority**: Critical (Core feature for conditional logic)
> **Estimated Effort**: 8-10 hours
> **Depends On**: ModalBase.razor

---

## Overview

The ConditionBuilderModal is a visual interface for creating complex conditional logic rules. It supports nested AND/OR/NOT groups, cross-module field references, and generates `ConditionalRule` objects that conform to the Core.V4 schema.

---

## Core.V4 Schema Reference

### ConditionalRule (Target Output)
```csharp
public record ConditionalRule
{
    public required string Id { get; init; }
    public string? Description { get; init; }
    public string? TargetFieldId { get; init; }
    public int? TargetStepNumber { get; init; }
    public string? TargetModuleKey { get; init; }
    public required string Action { get; init; }  // show, hide, enable, disable, setRequired, setOptional
    public required Condition Condition { get; init; }
    public int Priority { get; init; } = 100;
    public bool IsActive { get; init; } = true;
    public string? Category { get; init; }
    public string[]? Tags { get; init; }
}
```

### Condition (Recursive Structure)
```csharp
public record Condition
{
    // Simple condition (leaf node)
    public string? Field { get; init; }           // e.g., "age" or "Step1.total"
    public ConditionOperator? Operator { get; init; }
    public object? Value { get; init; }

    // Complex condition (branch node)
    public LogicalOperator? LogicalOp { get; init; }  // And, Or, Not
    public Condition[]? Conditions { get; init; }
}
```

### ConditionOperator Enum
```csharp
public enum ConditionOperator
{
    Equals, NotEquals,
    GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual,
    Contains, NotContains, StartsWith, EndsWith,
    In, NotIn,
    IsNull, IsNotNull, IsEmpty, IsNotEmpty
}
```

### LogicalOperator Enum
```csharp
public enum LogicalOperator { And, Or, Not }
```

---

## Features

| Feature | Description |
|---------|-------------|
| Action Selector | Choose what happens when condition is true |
| Target Field Picker | Select which field the action applies to |
| Nested Groups | Create AND/OR/NOT groups with unlimited nesting |
| Field Picker | Dropdown with all fields from current and other modules |
| Operator Selection | All 16 operators from ConditionOperator enum |
| Value Input | Dynamic input based on field type (text, number, date, dropdown) |
| Cross-Module References | Support for `ModuleKey.FieldId` syntax |
| Live Preview | Human-readable and JSON preview of the condition |
| Validation | Ensure all required fields are filled |

---

## Component API

### Parameters

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

[Parameter] public ConditionalRule? ExistingRule { get; set; }  // For editing
[Parameter] public EventCallback<ConditionalRule> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }

// Context for field picker
[Parameter] public FormModuleSchema CurrentModule { get; set; } = default!;
[Parameter] public FormWorkflowSchema? Workflow { get; set; }  // For cross-module references
[Parameter] public Dictionary<int, FormModuleSchema>? WorkflowModules { get; set; }

// Defaults
[Parameter] public string? DefaultTargetFieldId { get; set; }
[Parameter] public string DefaultAction { get; set; } = "show";
```

---

## File Structure

```
Components/Editor/Modals/
├── ConditionBuilderModal.razor
├── ConditionBuilderModal.razor.cs
├── ConditionBuilderModal.razor.css
├── ConditionGroup.razor          # Reusable group component
├── ConditionGroup.razor.cs
├── ConditionRow.razor            # Single condition row
├── ConditionRow.razor.cs
└── ConditionRow.razor.css

Models/
├── ConditionGroupModel.cs        # UI model for condition groups
├── ConditionRowModel.cs          # UI model for condition rows
└── FieldReference.cs             # Model for field picker options
```

---

## UI Models (Mutable for Editing)

```csharp
// Models/ConditionGroupModel.cs
public class ConditionGroupModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public LogicalOperator LogicalOp { get; set; } = LogicalOperator.And;
    public List<ConditionRowModel> Conditions { get; set; } = new();
    public List<ConditionGroupModel> NestedGroups { get; set; } = new();
}

// Models/ConditionRowModel.cs
public class ConditionRowModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? FieldId { get; set; }           // e.g., "age" or "Step1.country"
    public ConditionOperator Operator { get; set; } = ConditionOperator.Equals;
    public string? Value { get; set; }
    public string? ValueType { get; set; }         // For rendering appropriate input
}

// Models/FieldReference.cs
public record FieldReference(
    string FieldId,              // Full reference e.g., "Step1.age"
    string DisplayName,          // e.g., "age (Number)"
    string FieldType,            // e.g., "Number", "DropDown"
    string? ModuleKey,           // e.g., "Step1" or null for current
    string? ModuleName,          // e.g., "Personal Info"
    List<SelectOption>? Options  // For dropdown fields
);
```

---

## Conversion Methods

```csharp
// Convert UI model to schema
public static Condition ToCondition(ConditionGroupModel group)
{
    if (group.Conditions.Count == 1 && group.NestedGroups.Count == 0)
    {
        // Single condition - return as simple condition
        var row = group.Conditions[0];
        return new Condition
        {
            Field = row.FieldId,
            Operator = row.Operator,
            Value = ParseValue(row.Value, row.ValueType)
        };
    }

    // Complex condition with groups
    var conditions = new List<Condition>();

    foreach (var row in group.Conditions)
    {
        conditions.Add(new Condition
        {
            Field = row.FieldId,
            Operator = row.Operator,
            Value = ParseValue(row.Value, row.ValueType)
        });
    }

    foreach (var nested in group.NestedGroups)
    {
        conditions.Add(ToCondition(nested));
    }

    return new Condition
    {
        LogicalOp = group.LogicalOp,
        Conditions = conditions.ToArray()
    };
}

// Convert schema to UI model
public static ConditionGroupModel FromCondition(Condition condition)
{
    var group = new ConditionGroupModel();

    if (condition.IsSimpleCondition)
    {
        group.Conditions.Add(new ConditionRowModel
        {
            FieldId = condition.Field,
            Operator = condition.Operator!.Value,
            Value = condition.Value?.ToString()
        });
    }
    else if (condition.IsComplexCondition)
    {
        group.LogicalOp = condition.LogicalOp!.Value;

        foreach (var sub in condition.Conditions!)
        {
            if (sub.IsSimpleCondition)
            {
                group.Conditions.Add(new ConditionRowModel
                {
                    FieldId = sub.Field,
                    Operator = sub.Operator!.Value,
                    Value = sub.Value?.ToString()
                });
            }
            else
            {
                group.NestedGroups.Add(FromCondition(sub));
            }
        }
    }

    return group;
}
```

---

## Actions Reference

| Action | Description | Icon |
|--------|-------------|------|
| `show` | Make field visible | `bi-eye` |
| `hide` | Make field hidden | `bi-eye-slash` |
| `enable` | Make field editable | `bi-unlock` |
| `disable` | Make field read-only | `bi-lock` |
| `setRequired` | Make field required | `bi-asterisk` |
| `setOptional` | Make field optional | `bi-dash` |

---

## Operator Display Names

| Operator | Display | Applicable To |
|----------|---------|---------------|
| `Equals` | "equals" | All |
| `NotEquals` | "does not equal" | All |
| `GreaterThan` | "is greater than" | Number, Date |
| `GreaterThanOrEqual` | "is greater than or equal to" | Number, Date |
| `LessThan` | "is less than" | Number, Date |
| `LessThanOrEqual` | "is less than or equal to" | Number, Date |
| `Contains` | "contains" | Text, Array |
| `NotContains` | "does not contain" | Text, Array |
| `StartsWith` | "starts with" | Text |
| `EndsWith` | "ends with" | Text |
| `In` | "is one of" | All |
| `NotIn` | "is not one of" | All |
| `IsNull` | "is null" | All (no value needed) |
| `IsNotNull` | "is not null" | All (no value needed) |
| `IsEmpty` | "is empty" | Text, Array |
| `IsNotEmpty` | "is not empty" | Text, Array |

---

## Testing Checklist

- [ ] Create simple single condition
- [ ] Create AND group with multiple conditions
- [ ] Create OR group with multiple conditions
- [ ] Create nested groups (AND containing OR)
- [ ] Toggle logical operator (AND/OR/NOT)
- [ ] Add/remove conditions
- [ ] Add/remove groups
- [ ] Select field from current module
- [ ] Select field from another module (cross-module)
- [ ] All 16 operators work correctly
- [ ] Value input adapts to field type
- [ ] Human-readable preview is accurate
- [ ] JSON preview is valid
- [ ] Edit existing rule loads correctly
- [ ] Save produces valid ConditionalRule
- [ ] Cancel discards changes
- [ ] Validation prevents saving incomplete conditions
- [ ] Dark mode styling works

---

## Claude Implementation Prompt

Copy and paste the following prompt to Claude to implement this component:

---

### PROMPT START

```
I need you to implement the ConditionBuilderModal component for my Blazor application. This is a complex visual condition builder for creating conditional logic rules.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Editor/Modals/
- Core Library: DynamicForms.Core.V4 (contains ConditionalRule, Condition, ConditionOperator, LogicalOperator)
- Depends on: ModalBase.razor (already implemented)

## Schema Types (from DynamicForms.Core.V4)

These types already exist - DO NOT recreate them:

```csharp
// Enums/ConditionOperator.cs
public enum ConditionOperator
{
    Equals, NotEquals,
    GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual,
    Contains, NotContains, StartsWith, EndsWith,
    In, NotIn,
    IsNull, IsNotNull, IsEmpty, IsNotEmpty
}

// Enums/LogicalOperator.cs
public enum LogicalOperator { And, Or, Not }

// Schemas/Condition.cs
public record Condition
{
    public string? Field { get; init; }
    public ConditionOperator? Operator { get; init; }
    public object? Value { get; init; }
    public LogicalOperator? LogicalOp { get; init; }
    public Condition[]? Conditions { get; init; }
    public bool IsSimpleCondition => !string.IsNullOrWhiteSpace(Field) && Operator.HasValue;
    public bool IsComplexCondition => LogicalOp.HasValue && Conditions != null && Conditions.Length > 0;
}

// Schemas/ConditionalRule.cs
public record ConditionalRule
{
    public required string Id { get; init; }
    public string? Description { get; init; }
    public string? TargetFieldId { get; init; }
    public int? TargetStepNumber { get; init; }
    public string? TargetModuleKey { get; init; }
    public required string Action { get; init; }
    public required Condition Condition { get; init; }
    public int Priority { get; init; } = 100;
    public bool IsActive { get; init; } = true;
    public string? Category { get; init; }
    public string[]? Tags { get; init; }
}
```

## Files to Create

### 1. Models/ConditionGroupModel.cs
```csharp
namespace VisualEditorOpus.Models;

using DynamicForms.Core.V4.Enums;

public class ConditionGroupModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public LogicalOperator LogicalOp { get; set; } = LogicalOperator.And;
    public List<ConditionRowModel> Conditions { get; set; } = new();
    public List<ConditionGroupModel> NestedGroups { get; set; } = new();

    public bool IsEmpty => Conditions.Count == 0 && NestedGroups.Count == 0;
}
```

### 2. Models/ConditionRowModel.cs
```csharp
namespace VisualEditorOpus.Models;

using DynamicForms.Core.V4.Enums;

public class ConditionRowModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? FieldId { get; set; }
    public ConditionOperator Operator { get; set; } = ConditionOperator.Equals;
    public string? Value { get; set; }
    public string? FieldType { get; set; }  // For rendering appropriate input
}
```

### 3. Models/FieldReference.cs
```csharp
namespace VisualEditorOpus.Models;

public record FieldReference(
    string FieldId,
    string DisplayName,
    string FieldType,
    string? ModuleKey,
    string? ModuleName,
    List<SelectOption>? Options
);
```

### 4. ConditionRow.razor
A single condition row component with:
- Field dropdown (grouped by module)
- Operator dropdown (filtered based on field type)
- Value input (text, number, date, or dropdown based on field type)
- Delete button

Parameters:
```csharp
[Parameter] public ConditionRowModel Model { get; set; } = default!;
[Parameter] public EventCallback OnDelete { get; set; }
[Parameter] public EventCallback OnChanged { get; set; }
[Parameter] public List<FieldReference> AvailableFields { get; set; } = new();
```

### 5. ConditionGroup.razor
A group component that contains condition rows and nested groups:
- Header with logical operator toggle (AND/OR/NOT)
- List of ConditionRow components
- List of nested ConditionGroup components (recursive)
- Add condition button
- Add nested group button
- Delete group button

Parameters:
```csharp
[Parameter] public ConditionGroupModel Model { get; set; } = default!;
[Parameter] public EventCallback OnDelete { get; set; }
[Parameter] public EventCallback OnChanged { get; set; }
[Parameter] public List<FieldReference> AvailableFields { get; set; } = new();
[Parameter] public int Depth { get; set; } = 0;  // For indentation
```

### 6. ConditionBuilderModal.razor
Main modal component containing:
- Action selector (show, hide, enable, disable, setRequired, setOptional)
- Target field picker
- Root ConditionGroup
- Preview section (human-readable and JSON tabs)
- Save/Cancel buttons

Parameters:
```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public ConditionalRule? ExistingRule { get; set; }
[Parameter] public EventCallback<ConditionalRule> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
[Parameter] public FormModuleSchema CurrentModule { get; set; } = default!;
[Parameter] public FormWorkflowSchema? Workflow { get; set; }
[Parameter] public Dictionary<int, FormModuleSchema>? WorkflowModules { get; set; }
[Parameter] public string? DefaultTargetFieldId { get; set; }
[Parameter] public string DefaultAction { get; set; } = "show";
```

## Key Implementation Details

### 1. Field Picker Building
Build the list of FieldReference objects from:
- CurrentModule.Fields (prefix: none, shown as "Current Module" group)
- WorkflowModules (prefix: module key, shown as "Step1 - Module Name" groups)

```csharp
private List<FieldReference> BuildFieldReferences()
{
    var refs = new List<FieldReference>();

    // Current module fields
    foreach (var field in CurrentModule.Fields)
    {
        refs.Add(new FieldReference(
            FieldId: field.Id,
            DisplayName: $"{field.Id} ({field.FieldType})",
            FieldType: field.FieldType,
            ModuleKey: null,
            ModuleName: "Current Module",
            Options: field.Options?.Select(o => new SelectOption(o.Value, o.LabelEn)).ToList()
        ));
    }

    // Cross-module fields
    if (WorkflowModules != null)
    {
        foreach (var (moduleId, module) in WorkflowModules)
        {
            if (module.Id == CurrentModule.Id) continue;

            var moduleKey = $"Step{moduleId}";
            foreach (var field in module.Fields)
            {
                refs.Add(new FieldReference(
                    FieldId: $"{moduleKey}.{field.Id}",
                    DisplayName: $"{field.Id} ({field.FieldType})",
                    FieldType: field.FieldType,
                    ModuleKey: moduleKey,
                    ModuleName: module.TitleEn ?? $"Module {moduleId}",
                    Options: field.Options?.Select(o => new SelectOption(o.Value, o.LabelEn)).ToList()
                ));
            }
        }
    }

    return refs;
}
```

### 2. Operator Filtering by Field Type
Show appropriate operators based on field type:
- Text: All except numeric comparisons
- Number/Currency: All except string operations
- Date: All except string operations
- DropDown/Radio/Checkbox: Equals, NotEquals, In, NotIn, IsNull, IsNotNull
- Boolean: Equals, NotEquals

### 3. Value Input Rendering
Based on field type:
- Text: `<input type="text">`
- Number: `<input type="number">`
- Date: `<input type="date">`
- DropDown with options: `<select>` with options from FieldReference
- For In/NotIn operators: Multi-select or comma-separated input

### 4. Conversion to/from Schema
Implement these methods in ConditionBuilderModal.razor.cs:
- `ConditionGroupModel FromCondition(Condition condition)` - Load existing
- `Condition ToCondition(ConditionGroupModel group)` - Build for save
- `ConditionalRule BuildRule()` - Create final ConditionalRule

### 5. Human-Readable Preview
Generate preview like:
"SHOW field "email" WHEN (age > 18 AND country = "CA") AND (employmentStatus = "employed" OR employmentStatus = "self-employed")"

### 6. Validation
Before save, validate:
- Target field is selected
- Action is selected
- At least one condition exists
- All conditions have field, operator, and value (except IsNull/IsNotNull/IsEmpty/IsNotEmpty)

## CSS Styling

Use these CSS variables from the project:
- --primary, --primary-light, --primary-dark
- --bg-primary, --bg-secondary, --bg-tertiary
- --text-primary, --text-secondary, --text-muted
- --border-color, --border-strong
- --danger, --warning, --success, --info and their -light variants
- --radius-sm, --radius-md, --radius-lg
- --font-mono for code preview

Create scoped CSS for:
- .action-selector (chip-style action buttons)
- .target-selector (highlighted info box)
- .condition-group (bordered group container)
- .condition-group-header (with logical operator toggle)
- .condition-row (flex row with field/operator/value)
- .logical-operator (styled badge that cycles AND/OR/NOT on click)
- .preview-section (tab container for human/JSON preview)

## Actions Reference
```csharp
private static readonly Dictionary<string, (string Label, string Icon)> Actions = new()
{
    ["show"] = ("Show", "bi-eye"),
    ["hide"] = ("Hide", "bi-eye-slash"),
    ["enable"] = ("Enable", "bi-unlock"),
    ["disable"] = ("Disable", "bi-lock"),
    ["setRequired"] = ("Set Required", "bi-asterisk"),
    ["setOptional"] = ("Set Optional", "bi-dash")
};
```

## Example Usage
```razor
<ConditionBuilderModal @bind-IsOpen="showConditionBuilder"
                       ExistingRule="@selectedRule"
                       CurrentModule="@currentModule"
                       Workflow="@workflow"
                       WorkflowModules="@workflowModules"
                       DefaultTargetFieldId="@selectedFieldId"
                       OnSave="HandleConditionSave"
                       OnCancel="() => showConditionBuilder = false" />
```

Please implement all files with complete, production-ready code. The component should:
1. Use ModalBase as the container (Size="ModalSize.Large")
2. Support creating new rules and editing existing rules
3. Handle all 16 operators correctly
4. Support cross-module field references
5. Generate valid ConditionalRule objects
6. Show real-time preview
7. Validate before saving

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `ConditionBuilderModal-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for creating simple single conditions
- Testing AND/OR/NOT group creation and toggling
- Nested group creation testing (AND containing OR, etc.)
- Field picker testing (current module and cross-module fields)
- Operator dropdown testing for each field type
- Value input testing (text, number, date, dropdown)
- Cross-module field reference testing (Step1.fieldId syntax)
- Human-readable preview verification
- JSON preview verification
- Edit existing rule testing
- Validation testing (incomplete conditions)
- Save/Cancel behavior testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Model files location setup (ConditionGroupModel, ConditionRowModel, FieldReference)
- Integration with ConditionalSection.razor
- EditorStateService.UpdateFieldRules implementation
- Field reference building from workflow modules
- Conversion methods testing with real data
- CSS imports for scoped styles

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens with ModalBase (Large size)
- [ ] Action selector shows all 6 actions with icons
- [ ] Target field picker works correctly
- [ ] Can create simple single condition
- [ ] Can create AND group with multiple conditions
- [ ] Can create OR group with multiple conditions
- [ ] Can create NOT group
- [ ] Can toggle logical operator (AND/OR/NOT)
- [ ] Can add/remove conditions within group
- [ ] Can add/remove nested groups
- [ ] Nested groups indent correctly
- [ ] Field dropdown shows current module fields
- [ ] Field dropdown shows cross-module fields (when workflow present)
- [ ] Operators filter based on field type
- [ ] Value input adapts to field type (text/number/date/dropdown)
- [ ] Human-readable preview updates in real-time
- [ ] JSON preview shows valid JSON
- [ ] Loading existing rule populates correctly
- [ ] Save produces valid ConditionalRule object
- [ ] Validation prevents saving incomplete conditions
- [ ] Cancel discards changes
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Recursive component infinite loop | Use `@key` directive and limit depth |
| Value serialization | Parse values to appropriate types (int, bool, string) |
| Cross-module references | Use dot notation: `Step1.fieldId` |
| Operator compatibility | Filter operators based on field type |
| Empty groups | Don't save empty groups, show validation error |

---

## Integration with ConditionalSection

The existing `ConditionalSection.razor` in the properties panel has a TODO to open this modal:

```csharp
// In ConditionalSection.razor.cs
private void OpenConditionBuilder()
{
    // TODO: Open condition builder modal
}
```

After implementing ConditionBuilderModal, update ConditionalSection to:
```csharp
private bool showConditionBuilder = false;
private ConditionalRule? editingRule;

private void OpenConditionBuilder(ConditionalRule? rule = null)
{
    editingRule = rule;
    showConditionBuilder = true;
}

private void HandleConditionSave(ConditionalRule rule)
{
    // Add or update rule in field's ConditionalRules array
    EditorStateService.UpdateFieldRules(SelectedField.Id, rule);
    showConditionBuilder = false;
}
```
