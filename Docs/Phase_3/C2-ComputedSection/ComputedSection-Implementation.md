# C.2 Computed Section - Implementation Plan

> **Task**: Computed Section for Property Panel
> **Location**: `Src/VisualEditorOpus/Components/PropertyPanel/Sections/`
> **Priority**: High
> **Estimated Effort**: 2-3 hours
> **Delegation**: 90% AI

---

## Overview

The Computed Section manages formula-based calculated fields. It displays the current formula, dependent fields, live preview, and provides access to the FormulaEditorModal for editing.

---

## Schema Reference

From `DynamicForms.Core.V4`:

```csharp
// ComputedFormula.cs
public record ComputedFormula
{
    public required string Expression { get; init; }
    public string[]? DependentFieldIds { get; init; }
}

// FormFieldSchema.cs
public ComputedFormula? ComputedValue { get; init; }
```

---

## Component to Create

### ComputedSection.razor

```razor
@namespace VisualEditorOpus.Components.PropertyPanel.Sections

<div class="property-section">
    <div class="section-header @(IsExpanded ? "expanded" : "")" @onclick="ToggleExpanded">
        <div class="section-header-left">
            <i class="bi bi-calculator"></i>
            <span>Computed Value</span>
        </div>
        <i class="bi bi-chevron-down section-chevron"></i>
    </div>

    @if (IsExpanded)
    {
        <div class="section-content">
            @* Status Badge *@
            <div class="computed-status @StatusClass">
                <i class="bi bi-@StatusIcon"></i>
                <div class="computed-status-content">
                    <div class="computed-status-title">@StatusTitle</div>
                    <div class="computed-status-desc">@StatusDescription</div>
                </div>
            </div>

            @if (HasFormula)
            {
                @* Formula Display *@
                <div class="formula-display @(HasError ? "has-error" : "")">
                    <div class="formula-code">
                        @((MarkupString)HighlightedFormula)
                    </div>

                    @if (DependentFields.Any())
                    {
                        <div class="dependent-fields">
                            <div class="dependent-fields-label">Dependent Fields</div>
                            <div class="dependent-field-tags">
                                @foreach (var field in DependentFields)
                                {
                                    <span class="dependent-field-tag @(field.Exists ? "linked" : "missing")">
                                        <i class="bi bi-@(field.Exists ? "link-45deg" : "exclamation-circle")"></i>
                                        @field.Label
                                    </span>
                                }
                            </div>
                        </div>
                    }
                </div>

                @* Error Display *@
                @if (HasError)
                {
                    <div class="formula-error">
                        <i class="bi bi-@ErrorIcon"></i>
                        <div class="formula-error-content">
                            <div class="formula-error-title">@ErrorTitle</div>
                            <div class="formula-error-message">@ErrorMessage</div>
                        </div>
                    </div>
                }

                @* Live Preview *@
                @if (!HasError && PreviewValue != null)
                {
                    <div class="formula-preview">
                        <div class="preview-label">
                            <i class="bi bi-eye"></i>
                            Live Preview
                        </div>
                        <div class="preview-value">@PreviewValue</div>
                        @if (PreviewCalculation != null)
                        {
                            <div class="preview-calculation">@PreviewCalculation</div>
                        }
                    </div>
                }

                @* Action Buttons *@
                <div class="formula-actions">
                    <button class="action-btn primary" @onclick="OpenFormulaEditor">
                        <i class="bi bi-pencil"></i>
                        @(HasError ? "Fix Formula" : "Edit Formula")
                    </button>
                    <button class="action-btn danger" @onclick="RemoveFormula">
                        <i class="bi bi-trash"></i>
                        Remove
                    </button>
                </div>
            }
            else
            {
                @* Add Formula Button *@
                <button class="edit-formula-btn" @onclick="OpenFormulaEditor">
                    <i class="bi bi-plus-lg"></i>
                    Add Formula
                </button>

                @* Quick Templates *@
                <div class="formula-examples">
                    <div class="examples-title">
                        <i class="bi bi-lightbulb"></i>
                        Quick Templates
                    </div>
                    @foreach (var template in FormulaTemplates)
                    {
                        <div class="example-item" @onclick="() => ApplyTemplate(template)">
                            <div class="example-icon">
                                <i class="bi bi-@template.Icon"></i>
                            </div>
                            <div class="example-content">
                                <div class="example-name">@template.Name</div>
                                <div class="example-formula">@template.Formula</div>
                            </div>
                        </div>
                    }
                </div>
            }
        </div>
    }
</div>

@* Formula Editor Modal *@
<FormulaEditorModal @ref="formulaEditorModal"
                    Formula="@Field.ComputedValue"
                    AvailableFields="@AvailableFields"
                    OnSave="SaveFormula" />

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = default!;
    [Parameter] public IEnumerable<FormFieldSchema> AllFields { get; set; } = [];
    [Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }

    private FormulaEditorModal? formulaEditorModal;
    private bool IsExpanded { get; set; } = true;

    private bool HasFormula => Field.ComputedValue != null && !string.IsNullOrEmpty(Field.ComputedValue.Expression);
    private bool HasError => ValidationError != null;
    private FormulaValidationResult? ValidationError { get; set; }

    // Available fields for formula references
    private IEnumerable<FieldReference> AvailableFields => AllFields
        .Where(f => f.Id != Field.Id)
        .Select(f => new FieldReference(f.Id, f.LabelEn ?? f.Id, f.Type));

    // Dependent fields analysis
    private IEnumerable<DependentFieldInfo> DependentFields { get; set; } = [];

    // Preview
    private string? PreviewValue { get; set; }
    private string? PreviewCalculation { get; set; }

    // Status properties
    private string StatusClass => HasError ? "has-error" : (HasFormula ? "has-formula" : "no-formula");
    private string StatusIcon => HasError ? "exclamation-circle-fill" : (HasFormula ? "check-circle-fill" : "calculator");
    private string StatusTitle => HasError ? "Formula Error" : (HasFormula ? "Formula Active" : "No Formula Defined");
    private string StatusDescription => HasError
        ? "There's a problem with your formula"
        : (HasFormula
            ? $"Value computed from {DependentFields.Count()} dependent field{(DependentFields.Count() != 1 ? "s" : "")}"
            : "Add a formula to compute this field's value automatically");

    // Error display
    private string ErrorIcon => ValidationError?.ErrorType switch
    {
        FormulaErrorType.CircularReference => "arrow-repeat",
        FormulaErrorType.MissingField => "exclamation-triangle-fill",
        FormulaErrorType.SyntaxError => "code-slash",
        _ => "exclamation-triangle-fill"
    };
    private string ErrorTitle => ValidationError?.ErrorType switch
    {
        FormulaErrorType.CircularReference => "Circular Reference Detected",
        FormulaErrorType.MissingField => "Reference Error",
        FormulaErrorType.SyntaxError => "Syntax Error",
        _ => "Validation Error"
    };
    private string? ErrorMessage => ValidationError?.Message;

    // Syntax highlighted formula
    private string HighlightedFormula => HighlightFormula(Field.ComputedValue?.Expression ?? "");

    // Formula templates
    private static readonly FormulaTemplate[] FormulaTemplates = new[]
    {
        new FormulaTemplate("plus-slash-minus", "Sum Fields", "[field1] + [field2]"),
        new FormulaTemplate("percent", "Percentage", "[field1] * 0.1"),
        new FormulaTemplate("type", "Concatenate", "CONCAT([first], \" \", [last])"),
        new FormulaTemplate("question-circle", "Conditional", "IF([condition], [value1], [value2])")
    };

    protected override void OnParametersSet()
    {
        if (HasFormula)
        {
            ValidateFormula();
            BuildDependentFields();
            CalculatePreview();
        }
    }

    private void ToggleExpanded() => IsExpanded = !IsExpanded;

    private void ValidateFormula()
    {
        // Validate the formula expression
        var expression = Field.ComputedValue?.Expression ?? "";

        // Check for circular references
        if (HasCircularReference(Field.Id, Field.ComputedValue?.DependentFieldIds))
        {
            ValidationError = new FormulaValidationResult(
                FormulaErrorType.CircularReference,
                $"This formula references itself through a dependency chain");
            return;
        }

        // Check for missing field references
        var missingFields = FindMissingFields(expression);
        if (missingFields.Any())
        {
            ValidationError = new FormulaValidationResult(
                FormulaErrorType.MissingField,
                $"Field \"{missingFields.First()}\" does not exist in this module");
            return;
        }

        // Basic syntax validation
        if (!IsValidSyntax(expression))
        {
            ValidationError = new FormulaValidationResult(
                FormulaErrorType.SyntaxError,
                "Invalid formula syntax");
            return;
        }

        ValidationError = null;
    }

    private void BuildDependentFields()
    {
        var dependentIds = Field.ComputedValue?.DependentFieldIds ?? [];
        DependentFields = dependentIds.Select(id =>
        {
            var field = AllFields.FirstOrDefault(f => f.Id == id);
            return new DependentFieldInfo
            {
                Id = id,
                Label = field?.LabelEn ?? id,
                Exists = field != null
            };
        }).ToList();
    }

    private void CalculatePreview()
    {
        // In a real implementation, this would evaluate the formula
        // with sample/current values from dependent fields
        if (!HasError && HasFormula)
        {
            PreviewValue = "$425.00"; // Placeholder
            PreviewCalculation = "5 × $100.00 × (1 - 0.15) = $425.00"; // Placeholder
        }
    }

    private bool HasCircularReference(string fieldId, string[]? dependentIds)
    {
        if (dependentIds == null) return false;

        var visited = new HashSet<string> { fieldId };
        var queue = new Queue<string>(dependentIds);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visited.Contains(current)) return true;
            visited.Add(current);

            var field = AllFields.FirstOrDefault(f => f.Id == current);
            var deps = field?.ComputedValue?.DependentFieldIds ?? [];
            foreach (var dep in deps)
            {
                queue.Enqueue(dep);
            }
        }

        return false;
    }

    private IEnumerable<string> FindMissingFields(string expression)
    {
        // Extract field references like [FieldName]
        var fieldPattern = new Regex(@"\[([^\]]+)\]");
        var matches = fieldPattern.Matches(expression);

        return matches
            .Select(m => m.Groups[1].Value)
            .Where(name => !AllFields.Any(f =>
                f.LabelEn?.Equals(name, StringComparison.OrdinalIgnoreCase) == true ||
                f.Id.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    private bool IsValidSyntax(string expression)
    {
        // Basic syntax validation
        // Check for balanced brackets, valid operators, etc.
        var openBrackets = expression.Count(c => c == '[');
        var closeBrackets = expression.Count(c => c == ']');
        return openBrackets == closeBrackets;
    }

    private string HighlightFormula(string expression)
    {
        if (string.IsNullOrEmpty(expression)) return "";

        // Highlight field references
        expression = Regex.Replace(expression, @"\[([^\]]+)\]",
            m => $"[<span class=\"field-ref\">{m.Groups[1].Value}</span>]");

        // Highlight operators
        expression = Regex.Replace(expression, @"(\+|\-|\*|\/|\%|\^)",
            m => $"<span class=\"operator\">{m.Value}</span>");

        // Highlight numbers
        expression = Regex.Replace(expression, @"\b(\d+\.?\d*)\b",
            m => $"<span class=\"number\">{m.Value}</span>");

        // Highlight keywords/functions
        var functions = new[] { "SUM", "AVG", "IF", "CONCAT", "MIN", "MAX", "ROUND", "ABS" };
        foreach (var func in functions)
        {
            expression = Regex.Replace(expression, $@"\b({func})\b",
                m => $"<span class=\"keyword\">{m.Value}</span>",
                RegexOptions.IgnoreCase);
        }

        return expression;
    }

    private async Task OpenFormulaEditor()
    {
        await formulaEditorModal?.Open()!;
    }

    private async Task SaveFormula(ComputedFormula formula)
    {
        var updated = Field with { ComputedValue = formula };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task RemoveFormula()
    {
        var updated = Field with { ComputedValue = null };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task ApplyTemplate(FormulaTemplate template)
    {
        // Open formula editor with template pre-filled
        await formulaEditorModal?.OpenWithTemplate(template.Formula)!;
    }

    // Helper types
    private record DependentFieldInfo
    {
        public string Id { get; init; } = "";
        public string Label { get; init; } = "";
        public bool Exists { get; init; }
    }

    private record FieldReference(string Id, string Label, FieldType Type);

    private record FormulaTemplate(string Icon, string Name, string Formula);

    private record FormulaValidationResult(FormulaErrorType ErrorType, string Message);

    private enum FormulaErrorType { SyntaxError, MissingField, CircularReference }
}
```

---

## CSS Styles

Add to `computed-section.css`:

```css
/* ===== COMPUTED SECTION ===== */

/* Status Badge */
.computed-status {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 14px;
    border-radius: var(--radius-md);
    margin-bottom: 16px;
}

.computed-status.no-formula {
    background: var(--bg-tertiary);
}

.computed-status.has-formula {
    background: var(--primary-light);
    border: 1px solid rgba(99, 102, 241, 0.2);
}

.computed-status.has-error {
    background: var(--danger-light);
    border: 1px solid rgba(239, 68, 68, 0.2);
}

.computed-status i {
    font-size: 18px;
}

.computed-status.no-formula i { color: var(--text-muted); }
.computed-status.has-formula i { color: var(--primary); }
.computed-status.has-error i { color: var(--danger); }

.computed-status-content { flex: 1; }

.computed-status-title {
    font-size: 13px;
    font-weight: 600;
}

.computed-status.no-formula .computed-status-title { color: var(--text-secondary); }
.computed-status.has-formula .computed-status-title { color: var(--primary); }
.computed-status.has-error .computed-status-title { color: var(--danger); }

.computed-status-desc {
    font-size: 12px;
    color: var(--text-muted);
}

/* Formula Display */
.formula-display {
    background: var(--bg-secondary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    padding: 14px;
    margin-bottom: 16px;
}

.formula-display.has-error {
    border-color: rgba(239, 68, 68, 0.3);
}

.formula-code {
    font-family: 'Fira Code', 'Cascadia Code', 'Consolas', monospace;
    font-size: 13px;
    color: var(--text-primary);
    background: var(--bg-tertiary);
    padding: 12px 14px;
    border-radius: var(--radius-sm);
    overflow-x: auto;
    white-space: nowrap;
}

.formula-code .keyword { color: var(--primary); font-weight: 600; }
.formula-code .field-ref { color: var(--success); }
.formula-code .operator { color: var(--warning); }
.formula-code .number { color: #F472B6; }
.formula-code .string { color: #34D399; }

/* Dependent Fields */
.dependent-fields {
    margin-top: 12px;
    padding-top: 12px;
    border-top: 1px solid var(--border-color);
}

.dependent-fields-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    margin-bottom: 8px;
}

.dependent-field-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.dependent-field-tag {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 4px 10px;
    background: var(--bg-tertiary);
    border-radius: 12px;
    font-size: 12px;
    color: var(--text-secondary);
}

.dependent-field-tag i {
    font-size: 10px;
    color: var(--text-muted);
}

.dependent-field-tag.linked {
    background: var(--primary-light);
    color: var(--primary);
}

.dependent-field-tag.linked i { color: var(--primary); }

.dependent-field-tag.missing {
    background: var(--danger-light);
    color: var(--danger);
}

.dependent-field-tag.missing i { color: var(--danger); }

/* Formula Error */
.formula-error {
    display: flex;
    gap: 10px;
    padding: 12px;
    background: var(--danger-light);
    border: 1px solid rgba(239, 68, 68, 0.2);
    border-radius: var(--radius-md);
    margin-bottom: 12px;
}

.formula-error i {
    color: var(--danger);
    font-size: 16px;
    flex-shrink: 0;
}

.formula-error-content { flex: 1; }

.formula-error-title {
    font-size: 13px;
    font-weight: 600;
    color: var(--danger);
}

.formula-error-message {
    font-size: 12px;
    color: var(--text-secondary);
}

/* Formula Preview */
.formula-preview {
    background: var(--bg-secondary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    padding: 14px;
    margin-bottom: 16px;
}

.preview-label {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    margin-bottom: 8px;
    display: flex;
    align-items: center;
    gap: 6px;
}

.preview-value {
    font-size: 20px;
    font-weight: 700;
    color: var(--primary);
}

.preview-calculation {
    font-size: 12px;
    color: var(--text-muted);
    margin-top: 4px;
}

/* Add Formula Button */
.edit-formula-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    width: 100%;
    padding: 12px;
    border: 1px dashed var(--border-color);
    border-radius: var(--radius-md);
    background: var(--bg-primary);
    color: var(--text-secondary);
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s;
}

.edit-formula-btn:hover {
    border-color: var(--primary);
    color: var(--primary);
    background: var(--primary-light);
}

.edit-formula-btn i { font-size: 16px; }

/* Formula Examples */
.formula-examples {
    margin-top: 16px;
    padding: 14px;
    background: var(--bg-secondary);
    border-radius: var(--radius-md);
}

.examples-title {
    font-size: 12px;
    font-weight: 600;
    color: var(--text-secondary);
    margin-bottom: 10px;
    display: flex;
    align-items: center;
    gap: 6px;
}

.example-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 8px 10px;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-sm);
    margin-bottom: 6px;
    cursor: pointer;
    transition: all 0.15s;
}

.example-item:last-child { margin-bottom: 0; }

.example-item:hover {
    border-color: var(--primary);
    background: var(--primary-light);
}

.example-icon {
    width: 28px;
    height: 28px;
    border-radius: var(--radius-sm);
    background: var(--bg-tertiary);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    color: var(--text-muted);
}

.example-item:hover .example-icon {
    background: var(--primary);
    color: white;
}

.example-content { flex: 1; }

.example-name {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-primary);
}

.example-formula {
    font-size: 11px;
    color: var(--text-muted);
    font-family: monospace;
}

/* Action Buttons */
.formula-actions {
    display: flex;
    gap: 8px;
}

.action-btn {
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

.action-btn:hover {
    border-color: var(--primary);
    color: var(--primary);
    background: var(--primary-light);
}

.action-btn.primary {
    background: var(--primary);
    border-color: var(--primary);
    color: white;
}

.action-btn.primary:hover {
    background: var(--primary-hover);
}

.action-btn.danger:hover {
    border-color: var(--danger);
    color: var(--danger);
    background: var(--danger-light);
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the Computed Section component for the property panel in my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/PropertyPanel/Sections/
- Schema: DynamicForms.Core.V4 (FormFieldSchema has ComputedValue property)

## Component: ComputedSection.razor

This section manages formula-based calculated field values.

### Features Required:

1. **Status Badge**
   - No formula: "No Formula Defined" with gray styling
   - Has formula: "Formula Active" with primary styling
   - Has error: "Formula Error" with danger styling

2. **Formula Display** (when has formula)
   - Syntax highlighted code display
   - Colors: keywords (primary), field refs (green), operators (warning), numbers (pink)
   - Dependent fields list with tags
   - Missing fields marked with warning icon

3. **Error Handling**
   - Circular reference detection
   - Missing field reference detection
   - Syntax validation
   - Clear error messages with icons

4. **Live Preview**
   - Shows computed result
   - Shows calculation breakdown
   - Updates as dependent values change

5. **Quick Templates** (when no formula)
   - Sum Fields: [field1] + [field2]
   - Percentage: [field1] * 0.1
   - Concatenate: CONCAT([first], " ", [last])
   - Conditional: IF([condition], [value1], [value2])

6. **Actions**
   - Edit Formula: Opens FormulaEditorModal
   - Remove: Clears the computed value
   - Apply Template: Pre-fills editor with template

### Parameters:
```csharp
[Parameter] public FormFieldSchema Field { get; set; }
[Parameter] public IEnumerable<FormFieldSchema> AllFields { get; set; }
[Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }
```

### Schema Reference:
```csharp
public record ComputedFormula
{
    public required string Expression { get; init; }
    public string[]? DependentFieldIds { get; init; }
}
```

### Key Logic:
- Parse expression to find [FieldName] references
- Check for circular dependencies
- Highlight syntax with regex replacements
- Validate field references exist

Please implement complete, production-ready code with proper CSS styling.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `ComputedSection-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing "No Formula Defined" state
- Add Formula button testing
- Quick Templates click testing (Sum, Percentage, Concatenate, Conditional)
- Formula display with syntax highlighting testing
- Dependent fields tag display testing
- Missing field reference warning testing
- Circular reference error detection testing
- Syntax error display testing
- Live preview value display testing
- Edit Formula button testing
- Remove Formula button testing
- FormulaEditorModal integration testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- FormulaEditorModal component creation/integration
- Sample data provision for preview calculation
- Circular reference detection logic verification
- Field reference pattern validation ([FieldName] format)
- CSS imports for computed-section.css
- Syntax highlighting regex patterns verification

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Empty state shows "No Formula Defined" status
- [ ] Add Formula button opens FormulaEditorModal
- [ ] Quick Templates display with icons
- [ ] Clicking template opens modal with pre-filled formula
- [ ] Formula displays with syntax highlighting
- [ ] Keywords colored (primary)
- [ ] Field references colored (green)
- [ ] Operators colored (warning)
- [ ] Numbers colored (pink)
- [ ] Dependent fields listed as tags
- [ ] Missing fields marked with warning icon
- [ ] Circular reference error displays correctly
- [ ] Syntax error displays correctly
- [ ] Live preview shows computed value
- [ ] Preview calculation breakdown shows
- [ ] Edit Formula opens modal
- [ ] Remove clears ComputedValue
- [ ] Section expands/collapses correctly
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Empty state shows "No Formula Defined"
- [ ] Add Formula button opens modal
- [ ] Quick templates are clickable
- [ ] Formula displays with syntax highlighting
- [ ] Dependent fields are listed
- [ ] Missing field references show warning
- [ ] Circular reference error displays
- [ ] Syntax error displays
- [ ] Live preview shows value
- [ ] Edit Formula opens modal
- [ ] Remove clears formula
- [ ] Dark mode styling correct
- [ ] Section collapses/expands

---

## Notes

- FormulaEditorModal is a separate component (see Modal documentation)
- Consider adding formula auto-complete in future
- Preview should use sample values if no actual data
- Support common functions: SUM, AVG, IF, CONCAT, MIN, MAX, ROUND, ABS
- Consider adding formula versioning for undo/redo
