# WorkflowRulesModal Component - Implementation Plan

> **Component**: `WorkflowRulesModal.razor`
> **Location**: `Src/VisualEditorOpus/Components/Editor/Modals/WorkflowRulesModal.razor`
> **Priority**: High
> **Estimated Effort**: 4-5 hours
> **Depends On**: ModalBase.razor, ConditionBuilderModal.razor

---

## Overview

WorkflowRulesModal manages workflow-level conditional rules that control step navigation, skipping, and workflow completion. These rules evaluate conditions based on field values from any module in the workflow and trigger actions like skipStep, goToStep, or completeWorkflow.

---

## Core.V4 Schema Reference

### ConditionalRule
```csharp
public record ConditionalRule
{
    /// <summary>
    /// Unique identifier for the rule
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Target field ID for field-level rules (null for workflow rules)
    /// </summary>
    public string? TargetFieldId { get; init; }

    /// <summary>
    /// Target step number for workflow rules (skipStep, goToStep)
    /// </summary>
    public int? TargetStepNumber { get; init; }

    /// <summary>
    /// The action to perform: show, hide, enable, disable, setRequired,
    /// setOptional, skipStep, goToStep, completeWorkflow
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// The condition that triggers this rule
    /// </summary>
    public required Condition Condition { get; init; }

    /// <summary>
    /// Priority for rule evaluation (lower = higher priority)
    /// </summary>
    public int Priority { get; init; } = 100;

    /// <summary>
    /// Whether the rule is currently active
    /// </summary>
    public bool IsActive { get; init; } = true;
}
```

### Condition
```csharp
public record Condition
{
    /// <summary>
    /// Field reference using dot notation: "Step1.fieldId" or "ModuleName.fieldId"
    /// </summary>
    public string? Field { get; init; }

    /// <summary>
    /// The comparison operator
    /// </summary>
    public ConditionOperator? Operator { get; init; }

    /// <summary>
    /// The value to compare against
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// Logical operator for compound conditions (And, Or, Not)
    /// </summary>
    public LogicalOperator? LogicalOp { get; init; }

    /// <summary>
    /// Nested conditions for compound expressions
    /// </summary>
    public Condition[]? Conditions { get; init; }
}
```

### ConditionOperator Enum
```csharp
public enum ConditionOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEquals,
    LessThan,
    LessThanOrEquals,
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    IsNull,
    IsNotNull,
    IsEmpty,
    IsNotEmpty,
    In,
    NotIn
}
```

### LogicalOperator Enum
```csharp
public enum LogicalOperator
{
    And,
    Or,
    Not
}
```

---

## Workflow Rule Actions

| Action | Description | Target |
|--------|-------------|--------|
| `skipStep` | Skip a specific step when condition is true | `TargetStepNumber` |
| `goToStep` | Navigate to a specific step | `TargetStepNumber` |
| `completeWorkflow` | Complete the workflow immediately | None |

---

## Component API

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

[Parameter] public FormWorkflowSchema Workflow { get; set; } = default!;
[Parameter] public List<ConditionalRule> WorkflowRules { get; set; } = new();
[Parameter] public EventCallback<List<ConditionalRule>> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

---

## UI Structure

1. **Rules Header** - Title with active rule count badge + Add Rule button
2. **Rules List** - Scrollable list of existing rules with:
   - Action icon (color-coded by type)
   - Rule description and target
   - Condition preview (monospace)
   - Active toggle switch
   - Edit/Delete action buttons (show on hover)
3. **Add/Edit Rule Section** - Expandable form with:
   - Action type cards (Skip Step, Go To Step, Complete)
   - Rule description input
   - Target step dropdown (conditional on action type)
   - Condition input with Builder button to open ConditionBuilderModal
4. **Footer** - Close and Save Changes buttons

---

## State Management

```csharp
private List<WorkflowRuleViewModel> rules = new();
private bool isAddingRule = false;
private bool isEditingRule = false;
private string? editingRuleId = null;

// Form state for add/edit
private string selectedAction = "skipStep";
private string ruleDescription = "";
private int? targetStepNumber = null;
private Condition? currentCondition = null;

// Condition builder modal
private bool showConditionBuilder = false;
```

---

## View Model

```csharp
public class WorkflowRuleViewModel
{
    public string Id { get; set; } = "";
    public string Action { get; set; } = "skipStep";
    public string Description { get; set; } = "";
    public int? TargetStepNumber { get; set; }
    public Condition? Condition { get; set; }
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
}
```

---

## Helper Methods

### Get Action Display Info
```csharp
private static (string Icon, string CssClass, string Label) GetActionInfo(string action) => action switch
{
    "skipStep" => ("bi-skip-forward", "skip", "Skip Step"),
    "goToStep" => ("bi-arrow-right-circle", "goto", "Go To Step"),
    "completeWorkflow" => ("bi-check-circle", "complete", "Complete Workflow"),
    _ => ("bi-question", "", action)
};
```

### Format Condition for Display
```csharp
private string FormatCondition(Condition? condition)
{
    if (condition == null) return "No condition";

    if (condition.LogicalOp.HasValue && condition.Conditions?.Length > 0)
    {
        var parts = condition.Conditions.Select(c => FormatCondition(c));
        var op = condition.LogicalOp == LogicalOperator.And ? " AND " : " OR ";
        return string.Join(op, parts);
    }

    if (condition.Field != null && condition.Operator.HasValue)
    {
        var op = GetOperatorSymbol(condition.Operator.Value);
        var val = condition.Value?.ToString() ?? "null";
        return $"{condition.Field} {op} {val}";
    }

    return "Invalid condition";
}

private static string GetOperatorSymbol(ConditionOperator op) => op switch
{
    ConditionOperator.Equals => "=",
    ConditionOperator.NotEquals => "!=",
    ConditionOperator.GreaterThan => ">",
    ConditionOperator.LessThan => "<",
    ConditionOperator.Contains => "contains",
    ConditionOperator.IsNull => "is null",
    ConditionOperator.IsNotNull => "is not null",
    _ => op.ToString()
};
```

### Get Available Steps
```csharp
private List<(int Number, string Title)> GetAvailableSteps()
{
    return Workflow.Modules
        .Select((m, i) => (Number: i + 1, Title: m.TitleEn ?? $"Step {i + 1}"))
        .ToList();
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the WorkflowRulesModal component for my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Editor/Modals/
- Core schemas: ConditionalRule, Condition from DynamicForms.Core.V4
- Depends on: ModalBase.razor, ConditionBuilderModal.razor

## Schema Reference (DO NOT recreate - these exist in Core.V4)
```csharp
public record ConditionalRule
{
    public required string Id { get; init; }
    public string? TargetFieldId { get; init; }
    public int? TargetStepNumber { get; init; }
    public required string Action { get; init; }  // skipStep, goToStep, completeWorkflow
    public required Condition Condition { get; init; }
    public int Priority { get; init; } = 100;
    public bool IsActive { get; init; } = true;
}

public record Condition
{
    public string? Field { get; init; }              // "Step1.fieldId" or "ModuleName.fieldId"
    public ConditionOperator? Operator { get; init; }
    public object? Value { get; init; }
    public LogicalOperator? LogicalOp { get; init; } // And, Or, Not
    public Condition[]? Conditions { get; init; }    // For compound conditions
}
```

## Files to Create

### WorkflowRulesModal.razor
Modal with:
1. Header showing "Workflow Rules" with active count badge
2. Rules list with:
   - Color-coded action icons (skip=warning, goto=info, complete=success)
   - Rule description and target step info
   - Condition preview in monospace
   - Active toggle switch
   - Edit/Delete buttons (visible on hover)
3. Add/Edit rule section (expandable):
   - Action type cards (Skip Step, Go To Step, Complete Workflow)
   - Description input
   - Target step dropdown (hidden for completeWorkflow)
   - Condition input with Builder button
4. Footer with Close and Save Changes buttons

### WorkflowRulesModal.razor.cs
Parameters:
```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public FormWorkflowSchema Workflow { get; set; } = default!;
[Parameter] public List<ConditionalRule> WorkflowRules { get; set; } = new();
[Parameter] public EventCallback<List<ConditionalRule>> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

State:
```csharp
private List<WorkflowRuleViewModel> rules = new();
private bool isAddingRule = false;
private bool isEditingRule = false;
private string? editingRuleId = null;
private string selectedAction = "skipStep";
private string ruleDescription = "";
private int? targetStepNumber = null;
private Condition? currentCondition = null;
private bool showConditionBuilder = false;
```

Methods:
- OnParametersSet() - Initialize rules from WorkflowRules
- AddRule() - Show add rule form
- EditRule(string ruleId) - Load rule into form for editing
- DeleteRule(string ruleId) - Remove rule from list
- ToggleRuleActive(string ruleId) - Toggle IsActive flag
- SaveRule() - Create/update rule in list
- CancelEdit() - Hide add/edit form
- HandleConditionSaved(Condition condition) - Callback from ConditionBuilderModal
- HandleSave() - Convert ViewModels to ConditionalRules and invoke OnSave

## View Model
```csharp
public class WorkflowRuleViewModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Action { get; set; } = "skipStep";
    public string Description { get; set; } = "";
    public int? TargetStepNumber { get; set; }
    public Condition? Condition { get; set; }
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
}
```

## Action Type Cards Configuration
```csharp
private static readonly List<ActionTypeInfo> ActionTypes = new()
{
    new("skipStep", "Skip Step", "Skip a specific step/module", "bi-skip-forward", "skip"),
    new("goToStep", "Go To Step", "Navigate to a specific step", "bi-arrow-right-circle", "goto"),
    new("completeWorkflow", "Complete", "Complete the workflow", "bi-check-circle", "complete")
};
```

## CSS Classes (match existing mockup styles)
- .rules-header (flex between title and add button)
- .rules-list (bordered container)
- .rule-item (row with icon, content, toggle, actions)
- .rule-icon.skip, .rule-icon.goto, .rule-icon.complete (color variants)
- .rule-content, .rule-title, .rule-description, .rule-condition
- .rule-toggle, .toggle, .toggle.active
- .rule-actions (hidden until hover)
- .action-cards (3-column grid)
- .action-card, .action-card.selected
- .badge.badge-warning (for active count)

## Integration with ConditionBuilderModal
When the user clicks the "Builder" button next to the condition input:
1. Set showConditionBuilder = true
2. Pass currentCondition as ExistingCondition to ConditionBuilderModal
3. Pass all workflow fields (from all modules) as AvailableFields
4. Handle OnSave callback to update currentCondition

## Field Reference Format
Fields in conditions use cross-module reference format:
- "Step1.firstName" - References field from step 1
- "PersonalInfo.age" - References by module title
The modal should display available fields grouped by step/module.

## Validation
- Description is required
- Target step required for skipStep/goToStep actions
- At least one condition required
- Condition must be valid (have field, operator, value or be compound)

Please implement with complete, production-ready code.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `WorkflowRulesModal-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for adding a new workflow rule
- Action type card selection testing (Skip Step, Go To Step, Complete Workflow)
- Target step dropdown testing (shows all workflow steps)
- Target step visibility based on action type
- Description input testing
- Condition Builder button integration testing
- Condition preview display testing
- Active toggle switch testing
- Edit existing rule testing
- Delete rule testing
- Rule priority reordering (if implemented)
- Empty state display testing
- Save all changes testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Integration with workflow designer toolbar
- ConditionBuilderModal integration for conditions
- GetAllWorkflowFields helper implementation
- WorkflowRuleViewModel to ConditionalRule conversion
- Field reference format setup (Step1.fieldId)
- CSS imports for rule items and action cards

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens with ModalBase (Large size)
- [ ] Rules header shows active rule count badge
- [ ] Add Rule button expands the add form
- [ ] Action type cards display with icons
- [ ] Skip Step card selection works
- [ ] Go To Step card selection works
- [ ] Complete Workflow card selection works
- [ ] Target step dropdown shows all workflow steps
- [ ] Target step hidden when "Complete Workflow" selected
- [ ] Description input binds correctly
- [ ] Builder button opens ConditionBuilderModal
- [ ] Condition from Builder saved correctly
- [ ] Condition preview displays in monospace
- [ ] Save rule adds to rules list
- [ ] Rules list displays all rules
- [ ] Active toggle switches rule state
- [ ] Edit button loads rule into form
- [ ] Delete button removes rule from list
- [ ] Empty state shows when no rules
- [ ] Validation prevents saving invalid rules
- [ ] Save Changes creates valid ConditionalRule records
- [ ] Cancel closes modal without saving
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Integration Points

### With ConditionBuilderModal
```razor
<ConditionBuilderModal
    IsOpen="@showConditionBuilder"
    IsOpenChanged="@((bool v) => showConditionBuilder = v)"
    ExistingCondition="@currentCondition"
    AvailableFields="@GetAllWorkflowFields()"
    OnSave="@HandleConditionSaved"
    OnCancel="@(() => showConditionBuilder = false)" />
```

### Getting All Workflow Fields
```csharp
private List<FieldOption> GetAllWorkflowFields()
{
    var fields = new List<FieldOption>();
    for (int i = 0; i < Workflow.Modules.Count; i++)
    {
        var module = Workflow.Modules[i];
        var stepPrefix = $"Step{i + 1}";
        foreach (var field in module.Fields ?? Array.Empty<FieldSchema>())
        {
            fields.Add(new FieldOption
            {
                Id = $"{stepPrefix}.{field.Id}",
                Label = $"{stepPrefix}: {field.LabelEn ?? field.Id}",
                Type = field.FieldType,
                Group = module.TitleEn ?? stepPrefix
            });
        }
    }
    return fields;
}
```

---

## Testing Checklist

- [ ] Rules load from WorkflowRules parameter
- [ ] Action type cards select correctly
- [ ] Target step dropdown shows all workflow steps
- [ ] Target step hidden when "Complete" selected
- [ ] Condition Builder button opens ConditionBuilderModal
- [ ] Condition from Builder saved correctly
- [ ] Active toggle switches rule state
- [ ] Edit loads existing rule into form
- [ ] Delete removes rule from list
- [ ] Save creates valid ConditionalRule records
- [ ] Empty state shown when no rules
- [ ] Validation prevents saving invalid rules

---

## Empty State

When there are no workflow rules:
```html
<div class="empty-state">
    <i class="bi bi-signpost-split"></i>
    <p>No workflow rules configured</p>
    <button class="btn btn-primary btn-sm" @onclick="AddRule">
        <i class="bi bi-plus"></i> Add First Rule
    </button>
</div>
```

---

## Notes

1. **Cross-Module References**: Workflow rules can reference fields from any module using dot notation (Step1.fieldId or ModuleName.fieldId)
2. **Rule Priority**: Lower priority numbers are evaluated first
3. **Active Toggle**: Allows disabling rules without deleting them
4. **Condition Builder Integration**: Reuses the ConditionBuilderModal for complex conditions
