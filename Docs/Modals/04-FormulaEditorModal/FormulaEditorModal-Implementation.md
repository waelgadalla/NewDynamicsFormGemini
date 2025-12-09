# FormulaEditorModal Component - Implementation Plan

> **Component**: `FormulaEditorModal.razor`
> **Location**: `Src/VisualEditorOpus/Components/Editor/Modals/FormulaEditorModal.razor`
> **Priority**: High (Required for computed fields)
> **Estimated Effort**: 4-5 hours
> **Depends On**: ModalBase.razor

---

## Overview

The FormulaEditorModal provides a user-friendly interface for creating computed field formulas. It features a code editor with syntax highlighting, field picker, function library, and live preview of the computed result.

---

## Core.V4 Schema Reference

### ComputedFormula (Target Output)
```csharp
public record ComputedFormula
{
    /// <summary>
    /// The formula expression (e.g. "Quantity * Price").
    /// </summary>
    public required string Expression { get; init; }

    /// <summary>
    /// The IDs of fields referenced in the formula.
    /// </summary>
    public string[]? DependentFieldIds { get; init; }
}
```

---

## Features

| Feature | Description |
|---------|-------------|
| Code Editor | Monospace text area for formula input |
| Operator Bar | Quick buttons for +, -, *, /, %, () |
| Field Picker | Sidebar with clickable fields to insert |
| Function Library | Built-in functions with descriptions |
| Syntax Validation | Real-time validation with error messages |
| Live Preview | Shows computed result with sample data |
| Dependency Tracking | Automatically extracts referenced fields |

---

## Component API

### Parameters

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

[Parameter] public ComputedFormula? ExistingFormula { get; set; }
[Parameter] public EventCallback<ComputedFormula> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }

// Context for field picker
[Parameter] public FormModuleSchema CurrentModule { get; set; } = default!;
[Parameter] public Dictionary<string, object?>? SampleData { get; set; }  // For preview
```

---

## Supported Functions

| Function | Syntax | Description |
|----------|--------|-------------|
| `SUM` | `SUM(a, b, ...)` | Add multiple values |
| `AVG` | `AVG(a, b, ...)` | Calculate average |
| `MIN` | `MIN(a, b, ...)` | Get minimum value |
| `MAX` | `MAX(a, b, ...)` | Get maximum value |
| `ROUND` | `ROUND(value, decimals)` | Round to decimal places |
| `FLOOR` | `FLOOR(value)` | Round down |
| `CEIL` | `CEIL(value)` | Round up |
| `ABS` | `ABS(value)` | Absolute value |
| `IF` | `IF(condition, trueVal, falseVal)` | Conditional |
| `CONCAT` | `CONCAT(str1, str2, ...)` | Join strings |
| `DATEDIFF` | `DATEDIFF(date1, date2)` | Days between dates |
| `NOW` | `NOW()` | Current date/time |
| `COALESCE` | `COALESCE(val1, val2, ...)` | First non-null |
| `ISNULL` | `ISNULL(value)` | Check if null |

---

## File Structure

```
Components/Editor/Modals/
├── FormulaEditorModal.razor
├── FormulaEditorModal.razor.cs
└── FormulaEditorModal.razor.css
```

---

## Visual Structure

```
┌─────────────────────────────────────────────────────────────────────┐
│ fx Formula Editor                                               [×] │
├─────────────────────────────────────────────────────────────────────┤
│ ┌────────────────────────────────────────────┐ ┌──────────────────┐ │
│ │ FORMULA EXPRESSION                     🗑️ │ │ FIELDS           │ │
│ │ ┌────────────────────────────────────────┐ │ │ [Search...]      │ │
│ │ │ Quantity * UnitPrice * (1 - Discount/ │ │ │ ─────────────────│ │
│ │ │ 100)                                   │ │ │ 📊 Quantity  Num │ │
│ │ └────────────────────────────────────────┘ │ │ 💰 UnitPrice Cur │ │
│ │                                            │ │ % Discount   Num │ │
│ │ [ + ] [ - ] [ * ] [ / ] [ % ] [ ( ] [ ) ] │ │ % TaxRate    Num │ │
│ │ [ IF ] [ SUM ] [ AVG ] [ ROUND ]          │ │                  │ │
│ │                                            │ ├──────────────────┤ │
│ │ ┌──────────────────────────────────────┐   │ │ FUNCTIONS        │ │
│ │ │ ✓ Formula is valid                   │   │ │ SUM() - Add vals │ │
│ │ └──────────────────────────────────────┘   │ │ AVG() - Average  │ │
│ │                                            │ │ IF() - Condition │ │
│ │ PREVIEW RESULT                             │ │ ROUND() - Round  │ │
│ │ ┌──────────────────────────────────────┐   │ │ ...              │ │
│ │ │ $425.00                              │   │ │                  │ │
│ │ │ Type: Currency                       │   │ │                  │ │
│ │ └──────────────────────────────────────┘   │ │                  │ │
│ │                                            │ │                  │ │
│ │ DEPENDENCIES                               │ │                  │ │
│ │ [Quantity] [UnitPrice] [Discount]          │ │                  │ │
│ └────────────────────────────────────────────┘ └──────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ [Cancel]                               [Test] [✓ Save Formula]      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Formula Validation

### Validation Rules
1. **Non-empty**: Formula cannot be blank
2. **Balanced parentheses**: All `(` must have matching `)`
3. **Valid field references**: All referenced fields must exist in module
4. **Valid functions**: Function names must be in allowed list
5. **No syntax errors**: Basic expression parsing

### Validation Implementation
```csharp
public class FormulaValidator
{
    private static readonly HashSet<string> AllowedFunctions = new()
    {
        "SUM", "AVG", "MIN", "MAX", "ROUND", "FLOOR", "CEIL", "ABS",
        "IF", "CONCAT", "DATEDIFF", "NOW", "COALESCE", "ISNULL"
    };

    public (bool IsValid, string? Error) Validate(string formula, IEnumerable<string> fieldIds)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return (false, "Formula cannot be empty");

        // Check balanced parentheses
        int parenCount = 0;
        foreach (char c in formula)
        {
            if (c == '(') parenCount++;
            if (c == ')') parenCount--;
            if (parenCount < 0)
                return (false, "Unmatched closing parenthesis");
        }
        if (parenCount != 0)
            return (false, "Unmatched opening parenthesis");

        // Extract and validate field references
        var referencedFields = ExtractFieldReferences(formula);
        var unknownFields = referencedFields.Except(fieldIds).ToList();
        if (unknownFields.Any())
            return (false, $"Unknown field(s): {string.Join(", ", unknownFields)}");

        // Validate function names
        var usedFunctions = ExtractFunctionNames(formula);
        var unknownFunctions = usedFunctions.Except(AllowedFunctions).ToList();
        if (unknownFunctions.Any())
            return (false, $"Unknown function(s): {string.Join(", ", unknownFunctions)}");

        return (true, null);
    }

    public string[] ExtractFieldReferences(string formula)
    {
        // Extract identifiers that match field names
        var regex = new Regex(@"\b([A-Za-z_][A-Za-z0-9_]*)\b");
        var matches = regex.Matches(formula);

        return matches
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(v => !AllowedFunctions.Contains(v.ToUpper()))
            .Where(v => !IsNumericLiteral(v))
            .Distinct()
            .ToArray();
    }

    private string[] ExtractFunctionNames(string formula)
    {
        var regex = new Regex(@"\b([A-Z]+)\s*\(");
        return regex.Matches(formula)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToArray();
    }
}
```

---

## Preview Evaluation

For the preview, use sample data to evaluate the formula:

```csharp
public object? EvaluatePreview(string formula, Dictionary<string, object?> sampleData)
{
    try
    {
        // Replace field references with sample values
        var expression = formula;
        foreach (var (field, value) in sampleData)
        {
            expression = Regex.Replace(
                expression,
                $@"\b{Regex.Escape(field)}\b",
                value?.ToString() ?? "0"
            );
        }

        // Use DataTable.Compute for simple math or a proper expression evaluator
        var table = new DataTable();
        var result = table.Compute(expression, null);
        return result;
    }
    catch
    {
        return null;  // Evaluation failed
    }
}
```

---

## Testing Checklist

- [ ] Create simple formula (a + b)
- [ ] Create formula with parentheses
- [ ] Insert field by clicking
- [ ] Insert operator by clicking
- [ ] Insert function by clicking
- [ ] Validation shows error for unbalanced parentheses
- [ ] Validation shows error for unknown field
- [ ] Preview shows computed result
- [ ] Dependencies are extracted correctly
- [ ] Edit existing formula loads correctly
- [ ] Save produces valid ComputedFormula
- [ ] Search filters field list
- [ ] Dark mode styling works

---

## Claude Implementation Prompt

Copy and paste the following prompt to Claude to implement this component:

---

### PROMPT START

```
I need you to implement the FormulaEditorModal component for my Blazor application. This is a formula editor for creating computed field expressions.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Editor/Modals/
- Core Library: DynamicForms.Core.V4 (contains ComputedFormula)
- Depends on: ModalBase.razor (already implemented)

## Schema Type (from DynamicForms.Core.V4)

This type already exists - DO NOT recreate it:
```csharp
public record ComputedFormula
{
    public required string Expression { get; init; }
    public string[]? DependentFieldIds { get; init; }
}
```

## Files to Create

### 1. FormulaEditorModal.razor
Create the modal component with:
- Two-column layout: main editor (left), sidebar (right)
- Code editor (textarea with monospace font)
- Operator bar with quick-insert buttons
- Validation status indicator
- Result preview section
- Field dependencies display
- Sidebar with:
  - Searchable field list
  - Function reference list

### 2. FormulaEditorModal.razor.cs
Code-behind with:
- Parameters (see below)
- Formula state management
- Validation logic
- Field extraction logic
- Preview evaluation (simple)
- Insert helper methods

### 3. FormulaEditorModal.razor.css
Scoped styles for:
- .formula-editor-container (grid layout)
- .code-editor-wrapper, .code-editor
- .operator-bar, .operator-btn
- .validation-status.valid, .validation-status.invalid
- .result-preview
- .sidebar-section, .field-item, .function-item
- .dependency-tag

## Parameters

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public ComputedFormula? ExistingFormula { get; set; }
[Parameter] public EventCallback<ComputedFormula> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
[Parameter] public FormModuleSchema CurrentModule { get; set; } = default!;
[Parameter] public Dictionary<string, object?>? SampleData { get; set; }
```

## Required Functionality

### 1. Field Picker
Build list of fields from CurrentModule.Fields, showing:
- Field ID (monospace)
- Field type icon (bi-123 for number, bi-currency-dollar for currency, bi-calendar for date, etc.)
- Field type label

Click to insert field name at cursor position in editor.

### 2. Function Library
Static list of supported functions:
```csharp
private static readonly List<FunctionInfo> Functions = new()
{
    new("SUM", "SUM(a, b, ...)", "Add multiple values"),
    new("AVG", "AVG(a, b, ...)", "Calculate average"),
    new("MIN", "MIN(a, b, ...)", "Get minimum value"),
    new("MAX", "MAX(a, b, ...)", "Get maximum value"),
    new("ROUND", "ROUND(value, decimals)", "Round to decimal places"),
    new("FLOOR", "FLOOR(value)", "Round down"),
    new("CEIL", "CEIL(value)", "Round up"),
    new("ABS", "ABS(value)", "Absolute value"),
    new("IF", "IF(condition, trueVal, falseVal)", "Conditional value"),
    new("CONCAT", "CONCAT(str1, str2)", "Join strings"),
    new("DATEDIFF", "DATEDIFF(date1, date2)", "Days between dates"),
    new("NOW", "NOW()", "Current date/time"),
    new("COALESCE", "COALESCE(val1, val2)", "First non-null value")
};

private record FunctionInfo(string Name, string Syntax, string Description);
```

Click to insert function syntax at cursor.

### 3. Operator Bar
Quick-insert buttons for: + - * / % ( )
Plus function buttons for: IF, SUM, AVG, ROUND

### 4. Validation
On every change, validate:
- Non-empty
- Balanced parentheses
- All referenced fields exist in CurrentModule

Show validation status with icon:
- Valid: green checkmark, "Formula is valid"
- Invalid: red X, error message

### 5. Dependency Extraction
Extract field names from formula using regex:
```csharp
private string[] ExtractDependencies(string formula)
{
    var fieldIds = CurrentModule.Fields.Select(f => f.Id).ToHashSet();
    var regex = new Regex(@"\b([A-Za-z_][A-Za-z0-9_]*)\b");

    return regex.Matches(formula)
        .Cast<Match>()
        .Select(m => m.Value)
        .Where(fieldIds.Contains)
        .Distinct()
        .ToArray();
}
```

Display as tags below the editor.

### 6. Preview (Optional but nice)
If SampleData is provided, try to evaluate and show result.
For MVP, can just show "Preview requires sample data" if not provided.

### 7. Save
On save, create ComputedFormula:
```csharp
private async Task HandleSave()
{
    if (!IsValid) return;

    var formula = new ComputedFormula
    {
        Expression = formulaText,
        DependentFieldIds = ExtractDependencies(formulaText)
    };

    await OnSave.InvokeAsync(formula);
    await IsOpenChanged.InvokeAsync(false);
}
```

## CSS Variables to Use
- --font-mono for code editor
- --bg-primary, --bg-secondary, --bg-tertiary
- --text-primary, --text-secondary, --text-muted
- --border-color
- --success, --success-light (valid status)
- --danger, --danger-light (invalid status)
- --info, --info-light (dependency tags)
- --primary, --primary-light (hover states)

## Important Notes

1. Use ModalBase with Size="ModalSize.Large"
2. Editor should be a simple textarea (not a full code editor) for MVP
3. Focus the editor when modal opens
4. Disable Save button when invalid
5. Support cursor position for insertions (use JS interop or just append)
6. Field search should filter in real-time

## Example Usage
```razor
<FormulaEditorModal @bind-IsOpen="showFormulaEditor"
                    ExistingFormula="@selectedField?.ComputedValue"
                    CurrentModule="@currentModule"
                    SampleData="@GetSampleData()"
                    OnSave="HandleFormulaSave"
                    OnCancel="() => showFormulaEditor = false" />
```

Please implement all files with complete, production-ready code.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `FormulaEditorModal-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for creating simple formulas (a + b)
- Testing formula with parentheses and order of operations
- Field picker insertion testing (click to insert at cursor)
- Operator bar button testing (all operators)
- Function insertion testing (from library sidebar)
- Validation testing (empty, unbalanced parentheses, unknown fields)
- Preview result verification with sample data
- Dependency extraction verification
- Edit existing formula testing
- Field search filtering testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Integration with ComputedSection.razor in properties panel
- Sample data provision for preview feature
- Expression evaluation library integration (if needed beyond DataTable.Compute)
- JS interop for cursor position management (if implemented)
- CSS imports for editor styling

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens with ModalBase (Large size)
- [ ] Two-column layout displays correctly
- [ ] Code editor textarea has monospace font
- [ ] Operator bar shows all buttons (+, -, *, /, %, (, ))
- [ ] Function buttons work (IF, SUM, AVG, ROUND)
- [ ] Field picker shows all module fields with icons
- [ ] Field picker search filters correctly
- [ ] Function library displays with syntax and description
- [ ] Click on field inserts at cursor position
- [ ] Click on function inserts syntax
- [ ] Validation shows green checkmark for valid formula
- [ ] Validation shows red X with error message for invalid
- [ ] Dependencies extracted and displayed as tags
- [ ] Preview shows computed result (when sample data provided)
- [ ] Save button disabled when invalid
- [ ] Save produces valid ComputedFormula object
- [ ] Edit existing formula loads correctly
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Integration with ComputedSection

After implementing FormulaEditorModal, create or update `ComputedSection.razor` in the properties panel:

```razor
@if (SelectedField?.ComputedValue != null)
{
    <div class="computed-display">
        <code>@SelectedField.ComputedValue.Expression</code>
        <button class="btn btn-sm btn-ghost" @onclick="EditFormula">
            <i class="bi bi-pencil"></i>
        </button>
    </div>
}
else
{
    <button class="btn btn-sm btn-outline" @onclick="CreateFormula">
        <i class="bi bi-plus"></i> Add Formula
    </button>
}

<FormulaEditorModal @bind-IsOpen="showFormulaEditor"
                    ExistingFormula="@SelectedField?.ComputedValue"
                    CurrentModule="@CurrentModule"
                    OnSave="HandleFormulaSave" />
```

---

## Future Enhancements

1. **Syntax highlighting**: Use a proper code editor like CodeMirror or Monaco
2. **Autocomplete**: Show field suggestions as you type
3. **Error positions**: Highlight specific position of syntax errors
4. **Formula templates**: Pre-built formulas for common calculations
5. **Cross-module references**: Support for `Step1.FieldId` syntax
