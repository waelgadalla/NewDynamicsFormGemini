# E.4 WorkflowRulesModal - Implementation Guide

## Overview

The WorkflowRulesModal component provides a comprehensive interface for managing workflow rules including skip rules, go-to rules, completion rules, and validation rules. Rules control the dynamic behavior of the workflow at runtime.

## Component Architecture

```
WorkflowRulesModal/
├── WorkflowRulesModal.razor          # Main modal container with tabs
├── WorkflowRulesModal.razor.css      # Scoped styles
├── RulesList.razor                   # Generic rules list component
├── RuleCard.razor                    # Individual rule display card
├── RuleEditor.razor                  # Rule editing form
├── ConditionBuilder.razor            # Visual condition builder
├── ActionBuilder.razor               # Action configuration builder
└── Models/
    ├── WorkflowRule.cs               # Base rule model
    ├── SkipRule.cs                   # Skip rule model
    ├── GoToRule.cs                   # Go-to rule model
    ├── CompletionRule.cs             # Completion rule model
    ├── ValidationRule.cs             # Validation rule model
    ├── RuleCondition.cs              # Condition model
    └── RuleAction.cs                 # Action model
```

## Data Models

### WorkflowRule.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

/// <summary>
/// Base class for all workflow rules
/// </summary>
public abstract record WorkflowRule
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public bool IsEnabled { get; init; } = true;
    public int Priority { get; init; } = 0;
    public RuleCondition Condition { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; init; }
    public abstract RuleType Type { get; }
}

public enum RuleType
{
    Skip,
    GoTo,
    Completion,
    Validation
}
```

### SkipRule.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

/// <summary>
/// Rule that determines when a workflow step should be skipped
/// </summary>
public record SkipRule : WorkflowRule
{
    public override RuleType Type => RuleType.Skip;

    /// <summary>
    /// The step ID(s) to skip when condition is met
    /// </summary>
    public List<string> TargetStepIds { get; init; } = new();

    /// <summary>
    /// Whether to skip all subsequent steps until a specific step
    /// </summary>
    public bool SkipUntilStep { get; init; } = false;

    /// <summary>
    /// The step to resume at when SkipUntilStep is true
    /// </summary>
    public string? ResumeAtStepId { get; init; }
}
```

### GoToRule.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

/// <summary>
/// Rule that redirects workflow to a different step
/// </summary>
public record GoToRule : WorkflowRule
{
    public override RuleType Type => RuleType.GoTo;

    /// <summary>
    /// The source step where this rule applies
    /// </summary>
    public string SourceStepId { get; init; } = "";

    /// <summary>
    /// The target step to navigate to when condition is met
    /// </summary>
    public string TargetStepId { get; init; } = "";

    /// <summary>
    /// Whether to preserve form data when navigating
    /// </summary>
    public bool PreserveFormData { get; init; } = true;

    /// <summary>
    /// Optional message to display during navigation
    /// </summary>
    public string? NavigationMessage { get; init; }
}
```

### CompletionRule.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

/// <summary>
/// Rule that determines workflow completion behavior
/// </summary>
public record CompletionRule : WorkflowRule
{
    public override RuleType Type => RuleType.Completion;

    /// <summary>
    /// Action to perform when workflow completes
    /// </summary>
    public CompletionActionType ActionType { get; init; } = CompletionActionType.ShowMessage;

    /// <summary>
    /// Message to display on completion
    /// </summary>
    public string? CompletionMessage { get; init; }

    /// <summary>
    /// URL to redirect to on completion
    /// </summary>
    public string? RedirectUrl { get; init; }

    /// <summary>
    /// Whether to submit form data on completion
    /// </summary>
    public bool SubmitData { get; init; } = true;

    /// <summary>
    /// API endpoint to submit data to
    /// </summary>
    public string? SubmitEndpoint { get; init; }

    /// <summary>
    /// Custom completion handler name
    /// </summary>
    public string? CustomHandler { get; init; }
}

public enum CompletionActionType
{
    ShowMessage,
    Redirect,
    SubmitAndRedirect,
    CustomAction,
    RestartWorkflow
}
```

### ValidationRule.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

/// <summary>
/// Rule that performs custom validation on workflow steps
/// </summary>
public record ValidationRule : WorkflowRule
{
    public override RuleType Type => RuleType.Validation;

    /// <summary>
    /// The step(s) where this validation applies
    /// </summary>
    public List<string> ApplicableStepIds { get; init; } = new();

    /// <summary>
    /// Whether to apply to all steps
    /// </summary>
    public bool ApplyToAllSteps { get; init; } = false;

    /// <summary>
    /// The field to validate
    /// </summary>
    public string TargetFieldId { get; init; } = "";

    /// <summary>
    /// Validation type
    /// </summary>
    public ValidationType ValidationType { get; init; } = ValidationType.Required;

    /// <summary>
    /// Error message to display when validation fails
    /// </summary>
    public string ErrorMessage { get; init; } = "";

    /// <summary>
    /// Validation parameters (e.g., min/max values, regex pattern)
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();
}

public enum ValidationType
{
    Required,
    MinLength,
    MaxLength,
    Range,
    Regex,
    Email,
    Phone,
    Date,
    Custom,
    CrossField,
    Async
}
```

### RuleCondition.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

/// <summary>
/// Represents a condition that must be met for a rule to execute
/// </summary>
public record RuleCondition
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public ConditionType Type { get; init; } = ConditionType.Simple;
    public LogicalOperator Operator { get; init; } = LogicalOperator.And;
    public List<RuleCondition> SubConditions { get; init; } = new();

    // Simple condition properties
    public string? FieldId { get; init; }
    public ComparisonOperator Comparison { get; init; } = ComparisonOperator.Equals;
    public object? Value { get; init; }
    public string? OtherFieldId { get; init; } // For field-to-field comparison
}

public enum ConditionType
{
    Simple,      // Single field comparison
    Compound,    // Multiple conditions combined
    Expression,  // Custom expression
    Always,      // Always true
    Never        // Always false
}

public enum LogicalOperator
{
    And,
    Or,
    Not
}

public enum ComparisonOperator
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
    IsEmpty,
    IsNotEmpty,
    IsNull,
    IsNotNull,
    In,
    NotIn,
    Between,
    Regex
}
```

## Blazor Components

### WorkflowRulesModal.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="modal-backdrop @(IsOpen ? "show" : "")" @onclick="HandleBackdropClick">
    <div class="workflow-rules-modal @(IsOpen ? "show" : "")" @onclick:stopPropagation>
        <div class="modal-header">
            <h2 class="modal-title">
                <i class="bi bi-diagram-3"></i>
                Workflow Rules
            </h2>
            <button class="btn-close" @onclick="Close" title="Close">
                <i class="bi bi-x-lg"></i>
            </button>
        </div>

        <div class="modal-tabs">
            @foreach (var tab in _tabs)
            {
                <button class="tab-button @(ActiveTab == tab.Type ? "active" : "")"
                        @onclick="() => SetActiveTab(tab.Type)">
                    <i class="bi @tab.Icon"></i>
                    <span>@tab.Label</span>
                    <span class="badge">@GetRuleCount(tab.Type)</span>
                </button>
            }
        </div>

        <div class="modal-body">
            @switch (ActiveTab)
            {
                case RuleType.Skip:
                    <RulesList TRule="SkipRule"
                               Rules="SkipRules"
                               OnAdd="AddSkipRule"
                               OnEdit="EditSkipRule"
                               OnDelete="DeleteSkipRule"
                               OnToggle="ToggleSkipRule"
                               EmptyMessage="No skip rules defined. Skip rules allow steps to be skipped based on conditions." />
                    break;

                case RuleType.GoTo:
                    <RulesList TRule="GoToRule"
                               Rules="GoToRules"
                               OnAdd="AddGoToRule"
                               OnEdit="EditGoToRule"
                               OnDelete="DeleteGoToRule"
                               OnToggle="ToggleGoToRule"
                               EmptyMessage="No go-to rules defined. Go-to rules redirect the workflow to different steps." />
                    break;

                case RuleType.Completion:
                    <RulesList TRule="CompletionRule"
                               Rules="CompletionRules"
                               OnAdd="AddCompletionRule"
                               OnEdit="EditCompletionRule"
                               OnDelete="DeleteCompletionRule"
                               OnToggle="ToggleCompletionRule"
                               EmptyMessage="No completion rules defined. Completion rules control what happens when the workflow ends." />
                    break;

                case RuleType.Validation:
                    <RulesList TRule="ValidationRule"
                               Rules="ValidationRules"
                               OnAdd="AddValidationRule"
                               OnEdit="EditValidationRule"
                               OnDelete="DeleteValidationRule"
                               OnToggle="ToggleValidationRule"
                               EmptyMessage="No validation rules defined. Validation rules perform custom field validation." />
                    break;
            }
        </div>

        <div class="modal-footer">
            <div class="footer-info">
                <i class="bi bi-info-circle"></i>
                <span>Rules are evaluated in priority order (lower number = higher priority)</span>
            </div>
            <div class="footer-actions">
                <button class="btn btn-secondary" @onclick="Close">
                    Cancel
                </button>
                <button class="btn btn-primary" @onclick="SaveRules">
                    <i class="bi bi-check-lg"></i>
                    Save Rules
                </button>
            </div>
        </div>
    </div>
</div>

@if (_showEditor)
{
    <RuleEditor Rule="_editingRule"
                RuleType="ActiveTab"
                WorkflowNodes="WorkflowNodes"
                FormFields="FormFields"
                OnSave="SaveRule"
                OnCancel="CancelEdit" />
}

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public List<SkipRule> SkipRules { get; set; } = new();
    [Parameter] public EventCallback<List<SkipRule>> SkipRulesChanged { get; set; }
    [Parameter] public List<GoToRule> GoToRules { get; set; } = new();
    [Parameter] public EventCallback<List<GoToRule>> GoToRulesChanged { get; set; }
    [Parameter] public List<CompletionRule> CompletionRules { get; set; } = new();
    [Parameter] public EventCallback<List<CompletionRule>> CompletionRulesChanged { get; set; }
    [Parameter] public List<ValidationRule> ValidationRules { get; set; } = new();
    [Parameter] public EventCallback<List<ValidationRule>> ValidationRulesChanged { get; set; }
    [Parameter] public List<WorkflowNode> WorkflowNodes { get; set; } = new();
    [Parameter] public List<FormFieldInfo> FormFields { get; set; } = new();
    [Parameter] public EventCallback OnSave { get; set; }

    private RuleType ActiveTab { get; set; } = RuleType.Skip;
    private bool _showEditor = false;
    private WorkflowRule? _editingRule;

    private readonly List<(RuleType Type, string Label, string Icon)> _tabs = new()
    {
        (RuleType.Skip, "Skip Rules", "bi-skip-forward"),
        (RuleType.GoTo, "Go To Rules", "bi-signpost-split"),
        (RuleType.Completion, "Completion Rules", "bi-flag"),
        (RuleType.Validation, "Validation Rules", "bi-shield-check")
    };

    private void SetActiveTab(RuleType type) => ActiveTab = type;

    private int GetRuleCount(RuleType type) => type switch
    {
        RuleType.Skip => SkipRules.Count,
        RuleType.GoTo => GoToRules.Count,
        RuleType.Completion => CompletionRules.Count,
        RuleType.Validation => ValidationRules.Count,
        _ => 0
    };

    private void HandleBackdropClick() => Close();

    private async Task Close()
    {
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
    }

    // Skip Rules
    private void AddSkipRule()
    {
        _editingRule = new SkipRule { Name = "New Skip Rule" };
        _showEditor = true;
    }

    private void EditSkipRule(SkipRule rule)
    {
        _editingRule = rule;
        _showEditor = true;
    }

    private async Task DeleteSkipRule(SkipRule rule)
    {
        SkipRules = SkipRules.Where(r => r.Id != rule.Id).ToList();
        await SkipRulesChanged.InvokeAsync(SkipRules);
    }

    private async Task ToggleSkipRule(SkipRule rule)
    {
        var index = SkipRules.FindIndex(r => r.Id == rule.Id);
        if (index >= 0)
        {
            SkipRules[index] = rule with { IsEnabled = !rule.IsEnabled };
            await SkipRulesChanged.InvokeAsync(SkipRules);
        }
    }

    // Go To Rules
    private void AddGoToRule()
    {
        _editingRule = new GoToRule { Name = "New Go To Rule" };
        _showEditor = true;
    }

    private void EditGoToRule(GoToRule rule)
    {
        _editingRule = rule;
        _showEditor = true;
    }

    private async Task DeleteGoToRule(GoToRule rule)
    {
        GoToRules = GoToRules.Where(r => r.Id != rule.Id).ToList();
        await GoToRulesChanged.InvokeAsync(GoToRules);
    }

    private async Task ToggleGoToRule(GoToRule rule)
    {
        var index = GoToRules.FindIndex(r => r.Id == rule.Id);
        if (index >= 0)
        {
            GoToRules[index] = rule with { IsEnabled = !rule.IsEnabled };
            await GoToRulesChanged.InvokeAsync(GoToRules);
        }
    }

    // Completion Rules
    private void AddCompletionRule()
    {
        _editingRule = new CompletionRule { Name = "New Completion Rule" };
        _showEditor = true;
    }

    private void EditCompletionRule(CompletionRule rule)
    {
        _editingRule = rule;
        _showEditor = true;
    }

    private async Task DeleteCompletionRule(CompletionRule rule)
    {
        CompletionRules = CompletionRules.Where(r => r.Id != rule.Id).ToList();
        await CompletionRulesChanged.InvokeAsync(CompletionRules);
    }

    private async Task ToggleCompletionRule(CompletionRule rule)
    {
        var index = CompletionRules.FindIndex(r => r.Id == rule.Id);
        if (index >= 0)
        {
            CompletionRules[index] = rule with { IsEnabled = !rule.IsEnabled };
            await CompletionRulesChanged.InvokeAsync(CompletionRules);
        }
    }

    // Validation Rules
    private void AddValidationRule()
    {
        _editingRule = new ValidationRule { Name = "New Validation Rule" };
        _showEditor = true;
    }

    private void EditValidationRule(ValidationRule rule)
    {
        _editingRule = rule;
        _showEditor = true;
    }

    private async Task DeleteValidationRule(ValidationRule rule)
    {
        ValidationRules = ValidationRules.Where(r => r.Id != rule.Id).ToList();
        await ValidationRulesChanged.InvokeAsync(ValidationRules);
    }

    private async Task ToggleValidationRule(ValidationRule rule)
    {
        var index = ValidationRules.FindIndex(r => r.Id == rule.Id);
        if (index >= 0)
        {
            ValidationRules[index] = rule with { IsEnabled = !rule.IsEnabled };
            await ValidationRulesChanged.InvokeAsync(ValidationRules);
        }
    }

    // Editor
    private async Task SaveRule(WorkflowRule rule)
    {
        switch (rule)
        {
            case SkipRule skipRule:
                var skipIndex = SkipRules.FindIndex(r => r.Id == skipRule.Id);
                if (skipIndex >= 0)
                    SkipRules[skipIndex] = skipRule;
                else
                    SkipRules.Add(skipRule);
                await SkipRulesChanged.InvokeAsync(SkipRules);
                break;

            case GoToRule goToRule:
                var goToIndex = GoToRules.FindIndex(r => r.Id == goToRule.Id);
                if (goToIndex >= 0)
                    GoToRules[goToIndex] = goToRule;
                else
                    GoToRules.Add(goToRule);
                await GoToRulesChanged.InvokeAsync(GoToRules);
                break;

            case CompletionRule completionRule:
                var completionIndex = CompletionRules.FindIndex(r => r.Id == completionRule.Id);
                if (completionIndex >= 0)
                    CompletionRules[completionIndex] = completionRule;
                else
                    CompletionRules.Add(completionRule);
                await CompletionRulesChanged.InvokeAsync(CompletionRules);
                break;

            case ValidationRule validationRule:
                var validationIndex = ValidationRules.FindIndex(r => r.Id == validationRule.Id);
                if (validationIndex >= 0)
                    ValidationRules[validationIndex] = validationRule;
                else
                    ValidationRules.Add(validationRule);
                await ValidationRulesChanged.InvokeAsync(ValidationRules);
                break;
        }

        _showEditor = false;
        _editingRule = null;
    }

    private void CancelEdit()
    {
        _showEditor = false;
        _editingRule = null;
    }

    private async Task SaveRules()
    {
        await OnSave.InvokeAsync();
        await Close();
    }
}
```

### WorkflowRulesModal.razor.css

```css
/* Modal Backdrop */
.modal-backdrop {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    backdrop-filter: blur(4px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
    opacity: 0;
    visibility: hidden;
    transition: all 0.2s ease;
}

.modal-backdrop.show {
    opacity: 1;
    visibility: visible;
}

/* Modal Container */
.workflow-rules-modal {
    background: var(--bg-primary, #ffffff);
    border-radius: 12px;
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
    width: 90vw;
    max-width: 900px;
    max-height: 85vh;
    display: flex;
    flex-direction: column;
    transform: scale(0.95) translateY(20px);
    opacity: 0;
    transition: all 0.2s ease;
}

.workflow-rules-modal.show {
    transform: scale(1) translateY(0);
    opacity: 1;
}

/* Modal Header */
.modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid var(--border-color, #e5e7eb);
}

.modal-title {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    margin: 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--text-primary, #1f2937);
}

.modal-title i {
    color: var(--primary, #6366f1);
}

.btn-close {
    background: transparent;
    border: none;
    padding: 0.5rem;
    border-radius: 8px;
    cursor: pointer;
    color: var(--text-secondary, #6b7280);
    transition: all 0.15s ease;
}

.btn-close:hover {
    background: var(--bg-secondary, #f3f4f6);
    color: var(--text-primary, #1f2937);
}

/* Modal Tabs */
.modal-tabs {
    display: flex;
    gap: 0.25rem;
    padding: 0.75rem 1.5rem;
    border-bottom: 1px solid var(--border-color, #e5e7eb);
    background: var(--bg-secondary, #f9fafb);
}

.tab-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.625rem 1rem;
    background: transparent;
    border: none;
    border-radius: 8px;
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--text-secondary, #6b7280);
    cursor: pointer;
    transition: all 0.15s ease;
}

.tab-button:hover {
    background: var(--bg-primary, #ffffff);
    color: var(--text-primary, #1f2937);
}

.tab-button.active {
    background: var(--primary, #6366f1);
    color: white;
}

.tab-button .badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 1.25rem;
    height: 1.25rem;
    padding: 0 0.375rem;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 600;
}

.tab-button:not(.active) .badge {
    background: var(--bg-tertiary, #e5e7eb);
}

/* Modal Body */
.modal-body {
    flex: 1;
    overflow-y: auto;
    padding: 1.5rem;
}

/* Modal Footer */
.modal-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 1rem 1.5rem;
    border-top: 1px solid var(--border-color, #e5e7eb);
    background: var(--bg-secondary, #f9fafb);
}

.footer-info {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.8125rem;
    color: var(--text-muted, #9ca3af);
}

.footer-actions {
    display: flex;
    gap: 0.75rem;
}

/* Buttons */
.btn {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.625rem 1.25rem;
    border: none;
    border-radius: 8px;
    font-size: 0.875rem;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.15s ease;
}

.btn-primary {
    background: var(--primary, #6366f1);
    color: white;
}

.btn-primary:hover {
    background: var(--primary-hover, #4f46e5);
}

.btn-secondary {
    background: var(--bg-primary, #ffffff);
    color: var(--text-primary, #1f2937);
    border: 1px solid var(--border-color, #e5e7eb);
}

.btn-secondary:hover {
    background: var(--bg-secondary, #f3f4f6);
}

/* Dark Mode */
:global([data-theme="dark"]) .modal-backdrop {
    background: rgba(0, 0, 0, 0.7);
}

:global([data-theme="dark"]) .workflow-rules-modal {
    background: var(--bg-primary, #1f2937);
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
}

:global([data-theme="dark"]) .modal-header {
    border-color: var(--border-color, #374151);
}

:global([data-theme="dark"]) .modal-tabs {
    background: var(--bg-tertiary, #111827);
    border-color: var(--border-color, #374151);
}

:global([data-theme="dark"]) .modal-footer {
    background: var(--bg-tertiary, #111827);
    border-color: var(--border-color, #374151);
}
```

### RulesList.razor

```razor
@namespace VisualEditorOpus.Components.Workflow
@typeparam TRule where TRule : WorkflowRule

<div class="rules-list">
    <div class="rules-header">
        <button class="btn btn-add" @onclick="OnAdd">
            <i class="bi bi-plus-lg"></i>
            Add Rule
        </button>
    </div>

    @if (Rules.Count == 0)
    {
        <div class="empty-state">
            <div class="empty-icon">
                <i class="bi bi-inbox"></i>
            </div>
            <p class="empty-message">@EmptyMessage</p>
            <button class="btn btn-primary" @onclick="OnAdd">
                <i class="bi bi-plus-lg"></i>
                Create First Rule
            </button>
        </div>
    }
    else
    {
        <div class="rules-grid">
            @foreach (var rule in Rules.OrderBy(r => r.Priority))
            {
                <RuleCard Rule="rule"
                          OnEdit="() => OnEdit.InvokeAsync(rule)"
                          OnDelete="() => OnDelete.InvokeAsync(rule)"
                          OnToggle="() => OnToggle.InvokeAsync(rule)" />
            }
        </div>
    }
</div>

@code {
    [Parameter] public List<TRule> Rules { get; set; } = new();
    [Parameter] public EventCallback OnAdd { get; set; }
    [Parameter] public EventCallback<TRule> OnEdit { get; set; }
    [Parameter] public EventCallback<TRule> OnDelete { get; set; }
    [Parameter] public EventCallback<TRule> OnToggle { get; set; }
    [Parameter] public string EmptyMessage { get; set; } = "No rules defined.";
}
```

### RulesList.razor.css

```css
.rules-list {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.rules-header {
    display: flex;
    justify-content: flex-end;
}

.btn-add {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background: var(--bg-primary, #ffffff);
    border: 1px dashed var(--border-color, #d1d5db);
    border-radius: 8px;
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--text-secondary, #6b7280);
    cursor: pointer;
    transition: all 0.15s ease;
}

.btn-add:hover {
    border-color: var(--primary, #6366f1);
    color: var(--primary, #6366f1);
    background: var(--primary-light, #eef2ff);
}

/* Empty State */
.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem 2rem;
    text-align: center;
}

.empty-icon {
    width: 4rem;
    height: 4rem;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--bg-secondary, #f3f4f6);
    border-radius: 50%;
    margin-bottom: 1rem;
}

.empty-icon i {
    font-size: 1.5rem;
    color: var(--text-muted, #9ca3af);
}

.empty-message {
    margin: 0 0 1.5rem;
    color: var(--text-secondary, #6b7280);
    max-width: 300px;
}

/* Rules Grid */
.rules-grid {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
}

/* Dark Mode */
:global([data-theme="dark"]) .btn-add {
    background: var(--bg-secondary, #374151);
    border-color: var(--border-color, #4b5563);
}

:global([data-theme="dark"]) .btn-add:hover {
    background: var(--primary-dark, #312e81);
}

:global([data-theme="dark"]) .empty-icon {
    background: var(--bg-tertiary, #1f2937);
}
```

### RuleCard.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="rule-card @(Rule.IsEnabled ? "" : "disabled")">
    <div class="rule-header">
        <div class="rule-toggle">
            <label class="toggle-switch">
                <input type="checkbox" checked="@Rule.IsEnabled" @onchange="OnToggle" />
                <span class="toggle-slider"></span>
            </label>
        </div>

        <div class="rule-info">
            <h4 class="rule-name">@Rule.Name</h4>
            @if (!string.IsNullOrEmpty(Rule.Description))
            {
                <p class="rule-description">@Rule.Description</p>
            }
        </div>

        <div class="rule-actions">
            <button class="btn-icon" @onclick="OnEdit" title="Edit">
                <i class="bi bi-pencil"></i>
            </button>
            <button class="btn-icon btn-danger" @onclick="OnDelete" title="Delete">
                <i class="bi bi-trash"></i>
            </button>
        </div>
    </div>

    <div class="rule-details">
        <div class="detail-section">
            <span class="detail-label">
                <i class="bi bi-filter"></i>
                Condition
            </span>
            <span class="detail-value">@FormatCondition(Rule.Condition)</span>
        </div>

        <div class="detail-section">
            <span class="detail-label">
                <i class="bi bi-lightning"></i>
                Action
            </span>
            <span class="detail-value">@FormatAction(Rule)</span>
        </div>

        <div class="rule-meta">
            <span class="priority-badge">
                Priority: @Rule.Priority
            </span>
        </div>
    </div>
</div>

@code {
    [Parameter] public WorkflowRule Rule { get; set; } = default!;
    [Parameter] public EventCallback OnEdit { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public EventCallback OnToggle { get; set; }

    private string FormatCondition(RuleCondition condition)
    {
        if (condition.Type == ConditionType.Always)
            return "Always";
        if (condition.Type == ConditionType.Never)
            return "Never";
        if (condition.Type == ConditionType.Compound && condition.SubConditions.Count > 0)
            return $"{condition.SubConditions.Count} conditions ({condition.Operator})";
        if (!string.IsNullOrEmpty(condition.FieldId))
            return $"{condition.FieldId} {FormatOperator(condition.Comparison)} {condition.Value}";
        return "No condition";
    }

    private string FormatOperator(ComparisonOperator op) => op switch
    {
        ComparisonOperator.Equals => "=",
        ComparisonOperator.NotEquals => "!=",
        ComparisonOperator.GreaterThan => ">",
        ComparisonOperator.GreaterThanOrEquals => ">=",
        ComparisonOperator.LessThan => "<",
        ComparisonOperator.LessThanOrEquals => "<=",
        ComparisonOperator.Contains => "contains",
        ComparisonOperator.IsEmpty => "is empty",
        ComparisonOperator.IsNotEmpty => "is not empty",
        _ => op.ToString()
    };

    private string FormatAction(WorkflowRule rule) => rule switch
    {
        SkipRule skip => skip.TargetStepIds.Count > 0
            ? $"Skip {skip.TargetStepIds.Count} step(s)"
            : "Skip step",
        GoToRule goTo => $"Go to step: {goTo.TargetStepId}",
        CompletionRule completion => completion.ActionType.ToString(),
        ValidationRule validation => $"{validation.ValidationType} on {validation.TargetFieldId}",
        _ => "Unknown action"
    };
}
```

### RuleCard.razor.css

```css
.rule-card {
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-color, #e5e7eb);
    border-radius: 10px;
    overflow: hidden;
    transition: all 0.15s ease;
}

.rule-card:hover {
    border-color: var(--primary, #6366f1);
    box-shadow: 0 4px 12px rgba(99, 102, 241, 0.1);
}

.rule-card.disabled {
    opacity: 0.6;
}

.rule-card.disabled .rule-name {
    color: var(--text-muted, #9ca3af);
}

/* Rule Header */
.rule-header {
    display: flex;
    align-items: flex-start;
    gap: 1rem;
    padding: 1rem;
}

.rule-toggle {
    flex-shrink: 0;
    padding-top: 0.125rem;
}

/* Toggle Switch */
.toggle-switch {
    position: relative;
    display: inline-block;
    width: 2.5rem;
    height: 1.5rem;
}

.toggle-switch input {
    opacity: 0;
    width: 0;
    height: 0;
}

.toggle-slider {
    position: absolute;
    cursor: pointer;
    inset: 0;
    background: var(--bg-tertiary, #d1d5db);
    border-radius: 9999px;
    transition: 0.2s;
}

.toggle-slider::before {
    position: absolute;
    content: "";
    height: 1.125rem;
    width: 1.125rem;
    left: 0.1875rem;
    bottom: 0.1875rem;
    background: white;
    border-radius: 50%;
    transition: 0.2s;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
}

input:checked + .toggle-slider {
    background: var(--success, #10b981);
}

input:checked + .toggle-slider::before {
    transform: translateX(1rem);
}

/* Rule Info */
.rule-info {
    flex: 1;
    min-width: 0;
}

.rule-name {
    margin: 0;
    font-size: 0.9375rem;
    font-weight: 600;
    color: var(--text-primary, #1f2937);
}

.rule-description {
    margin: 0.25rem 0 0;
    font-size: 0.8125rem;
    color: var(--text-secondary, #6b7280);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

/* Rule Actions */
.rule-actions {
    display: flex;
    gap: 0.25rem;
    flex-shrink: 0;
}

.btn-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 2rem;
    height: 2rem;
    background: transparent;
    border: none;
    border-radius: 6px;
    color: var(--text-secondary, #6b7280);
    cursor: pointer;
    transition: all 0.15s ease;
}

.btn-icon:hover {
    background: var(--bg-secondary, #f3f4f6);
    color: var(--text-primary, #1f2937);
}

.btn-icon.btn-danger:hover {
    background: var(--danger-light, #fee2e2);
    color: var(--danger, #ef4444);
}

/* Rule Details */
.rule-details {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    padding: 0.75rem 1rem;
    background: var(--bg-secondary, #f9fafb);
    border-top: 1px solid var(--border-color, #e5e7eb);
}

.detail-section {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.detail-label {
    display: flex;
    align-items: center;
    gap: 0.25rem;
    font-size: 0.75rem;
    font-weight: 500;
    color: var(--text-muted, #9ca3af);
    text-transform: uppercase;
}

.detail-value {
    font-size: 0.8125rem;
    color: var(--text-primary, #1f2937);
    font-family: 'SF Mono', Monaco, monospace;
    background: var(--bg-primary, #ffffff);
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    border: 1px solid var(--border-color, #e5e7eb);
}

.rule-meta {
    margin-left: auto;
}

.priority-badge {
    font-size: 0.75rem;
    font-weight: 500;
    color: var(--text-secondary, #6b7280);
    background: var(--bg-primary, #ffffff);
    padding: 0.25rem 0.625rem;
    border-radius: 9999px;
    border: 1px solid var(--border-color, #e5e7eb);
}

/* Dark Mode */
:global([data-theme="dark"]) .rule-card {
    background: var(--bg-secondary, #374151);
    border-color: var(--border-color, #4b5563);
}

:global([data-theme="dark"]) .rule-card:hover {
    border-color: var(--primary, #6366f1);
}

:global([data-theme="dark"]) .toggle-slider {
    background: var(--bg-tertiary, #4b5563);
}

:global([data-theme="dark"]) .rule-details {
    background: var(--bg-tertiary, #1f2937);
    border-color: var(--border-color, #4b5563);
}

:global([data-theme="dark"]) .detail-value,
:global([data-theme="dark"]) .priority-badge {
    background: var(--bg-secondary, #374151);
    border-color: var(--border-color, #4b5563);
}
```

### ConditionBuilder.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="condition-builder">
    <div class="condition-header">
        <h4>Condition</h4>
        <select class="condition-type-select" @bind="Condition.Type">
            <option value="@ConditionType.Simple">Simple</option>
            <option value="@ConditionType.Compound">Compound</option>
            <option value="@ConditionType.Always">Always True</option>
            <option value="@ConditionType.Never">Never</option>
        </select>
    </div>

    @if (Condition.Type == ConditionType.Simple)
    {
        <div class="simple-condition">
            <div class="condition-row">
                <select class="field-select" @bind="Condition.FieldId">
                    <option value="">Select field...</option>
                    @foreach (var field in FormFields)
                    {
                        <option value="@field.Id">@field.Label</option>
                    }
                </select>

                <select class="operator-select" @bind="Condition.Comparison">
                    @foreach (var op in GetOperators())
                    {
                        <option value="@op">@FormatOperator(op)</option>
                    }
                </select>

                @if (!IsUnaryOperator(Condition.Comparison))
                {
                    <input type="text"
                           class="value-input"
                           placeholder="Value..."
                           @bind="ValueString" />
                }
            </div>
        </div>
    }
    else if (Condition.Type == ConditionType.Compound)
    {
        <div class="compound-condition">
            <div class="operator-row">
                <label>Combine with:</label>
                <div class="operator-buttons">
                    <button class="@(Condition.Operator == LogicalOperator.And ? "active" : "")"
                            @onclick="() => SetOperator(LogicalOperator.And)">
                        AND
                    </button>
                    <button class="@(Condition.Operator == LogicalOperator.Or ? "active" : "")"
                            @onclick="() => SetOperator(LogicalOperator.Or)">
                        OR
                    </button>
                </div>
            </div>

            <div class="sub-conditions">
                @foreach (var (sub, index) in Condition.SubConditions.Select((s, i) => (s, i)))
                {
                    <div class="sub-condition-row">
                        <span class="condition-index">@(index + 1)</span>
                        <ConditionBuilder Condition="sub"
                                          FormFields="FormFields"
                                          OnChange="c => UpdateSubCondition(index, c)" />
                        <button class="btn-remove" @onclick="() => RemoveSubCondition(index)">
                            <i class="bi bi-x"></i>
                        </button>
                    </div>
                }
            </div>

            <button class="btn-add-condition" @onclick="AddSubCondition">
                <i class="bi bi-plus"></i>
                Add Condition
            </button>
        </div>
    }
</div>

@code {
    [Parameter] public RuleCondition Condition { get; set; } = new();
    [Parameter] public EventCallback<RuleCondition> ConditionChanged { get; set; }
    [Parameter] public EventCallback<RuleCondition> OnChange { get; set; }
    [Parameter] public List<FormFieldInfo> FormFields { get; set; } = new();

    private string ValueString
    {
        get => Condition.Value?.ToString() ?? "";
        set => UpdateCondition(Condition with { Value = value });
    }

    private IEnumerable<ComparisonOperator> GetOperators()
    {
        return Enum.GetValues<ComparisonOperator>();
    }

    private string FormatOperator(ComparisonOperator op) => op switch
    {
        ComparisonOperator.Equals => "equals",
        ComparisonOperator.NotEquals => "not equals",
        ComparisonOperator.GreaterThan => "greater than",
        ComparisonOperator.GreaterThanOrEquals => "greater than or equals",
        ComparisonOperator.LessThan => "less than",
        ComparisonOperator.LessThanOrEquals => "less than or equals",
        ComparisonOperator.Contains => "contains",
        ComparisonOperator.NotContains => "not contains",
        ComparisonOperator.StartsWith => "starts with",
        ComparisonOperator.EndsWith => "ends with",
        ComparisonOperator.IsEmpty => "is empty",
        ComparisonOperator.IsNotEmpty => "is not empty",
        ComparisonOperator.IsNull => "is null",
        ComparisonOperator.IsNotNull => "is not null",
        ComparisonOperator.In => "in list",
        ComparisonOperator.NotIn => "not in list",
        ComparisonOperator.Between => "between",
        ComparisonOperator.Regex => "matches regex",
        _ => op.ToString()
    };

    private bool IsUnaryOperator(ComparisonOperator op)
    {
        return op is ComparisonOperator.IsEmpty or ComparisonOperator.IsNotEmpty
            or ComparisonOperator.IsNull or ComparisonOperator.IsNotNull;
    }

    private async Task UpdateCondition(RuleCondition condition)
    {
        Condition = condition;
        await ConditionChanged.InvokeAsync(condition);
        await OnChange.InvokeAsync(condition);
    }

    private void SetOperator(LogicalOperator op)
    {
        UpdateCondition(Condition with { Operator = op });
    }

    private void AddSubCondition()
    {
        var newSubs = Condition.SubConditions.ToList();
        newSubs.Add(new RuleCondition());
        UpdateCondition(Condition with { SubConditions = newSubs });
    }

    private void RemoveSubCondition(int index)
    {
        var newSubs = Condition.SubConditions.ToList();
        newSubs.RemoveAt(index);
        UpdateCondition(Condition with { SubConditions = newSubs });
    }

    private void UpdateSubCondition(int index, RuleCondition sub)
    {
        var newSubs = Condition.SubConditions.ToList();
        newSubs[index] = sub;
        UpdateCondition(Condition with { SubConditions = newSubs });
    }
}
```

### ConditionBuilder.razor.css

```css
.condition-builder {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.condition-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.condition-header h4 {
    margin: 0;
    font-size: 0.875rem;
    font-weight: 600;
    color: var(--text-primary, #1f2937);
}

.condition-type-select {
    padding: 0.375rem 0.75rem;
    border: 1px solid var(--border-color, #e5e7eb);
    border-radius: 6px;
    font-size: 0.8125rem;
    background: var(--bg-primary, #ffffff);
    color: var(--text-primary, #1f2937);
}

/* Simple Condition */
.simple-condition .condition-row {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
}

.field-select,
.operator-select,
.value-input {
    padding: 0.5rem 0.75rem;
    border: 1px solid var(--border-color, #e5e7eb);
    border-radius: 6px;
    font-size: 0.875rem;
    background: var(--bg-primary, #ffffff);
    color: var(--text-primary, #1f2937);
}

.field-select {
    flex: 1;
    min-width: 150px;
}

.operator-select {
    min-width: 140px;
}

.value-input {
    flex: 1;
    min-width: 120px;
}

/* Compound Condition */
.compound-condition {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.operator-row {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.operator-row label {
    font-size: 0.8125rem;
    color: var(--text-secondary, #6b7280);
}

.operator-buttons {
    display: flex;
    gap: 0.25rem;
}

.operator-buttons button {
    padding: 0.375rem 1rem;
    border: 1px solid var(--border-color, #e5e7eb);
    background: var(--bg-primary, #ffffff);
    font-size: 0.75rem;
    font-weight: 600;
    color: var(--text-secondary, #6b7280);
    cursor: pointer;
    transition: all 0.15s ease;
}

.operator-buttons button:first-child {
    border-radius: 6px 0 0 6px;
}

.operator-buttons button:last-child {
    border-radius: 0 6px 6px 0;
}

.operator-buttons button.active {
    background: var(--primary, #6366f1);
    border-color: var(--primary, #6366f1);
    color: white;
}

/* Sub-conditions */
.sub-conditions {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    padding-left: 1rem;
    border-left: 2px solid var(--border-color, #e5e7eb);
}

.sub-condition-row {
    display: flex;
    align-items: flex-start;
    gap: 0.75rem;
}

.condition-index {
    flex-shrink: 0;
    width: 1.5rem;
    height: 1.5rem;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--bg-secondary, #f3f4f6);
    border-radius: 50%;
    font-size: 0.75rem;
    font-weight: 600;
    color: var(--text-secondary, #6b7280);
}

.btn-remove {
    flex-shrink: 0;
    width: 1.75rem;
    height: 1.75rem;
    display: flex;
    align-items: center;
    justify-content: center;
    background: transparent;
    border: none;
    border-radius: 4px;
    color: var(--text-muted, #9ca3af);
    cursor: pointer;
    transition: all 0.15s ease;
}

.btn-remove:hover {
    background: var(--danger-light, #fee2e2);
    color: var(--danger, #ef4444);
}

.btn-add-condition {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background: transparent;
    border: 1px dashed var(--border-color, #d1d5db);
    border-radius: 6px;
    font-size: 0.8125rem;
    color: var(--text-secondary, #6b7280);
    cursor: pointer;
    transition: all 0.15s ease;
}

.btn-add-condition:hover {
    border-color: var(--primary, #6366f1);
    color: var(--primary, #6366f1);
}

/* Dark Mode */
:global([data-theme="dark"]) .condition-type-select,
:global([data-theme="dark"]) .field-select,
:global([data-theme="dark"]) .operator-select,
:global([data-theme="dark"]) .value-input {
    background: var(--bg-secondary, #374151);
    border-color: var(--border-color, #4b5563);
}

:global([data-theme="dark"]) .operator-buttons button {
    background: var(--bg-secondary, #374151);
    border-color: var(--border-color, #4b5563);
}

:global([data-theme="dark"]) .sub-conditions {
    border-color: var(--border-color, #4b5563);
}

:global([data-theme="dark"]) .condition-index {
    background: var(--bg-tertiary, #1f2937);
}
```

## Supporting Types

### FormFieldInfo.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

/// <summary>
/// Information about a form field for rule building
/// </summary>
public record FormFieldInfo
{
    public string Id { get; init; } = "";
    public string Label { get; init; } = "";
    public string Type { get; init; } = "text";
    public string? GroupId { get; init; }
    public string? StepId { get; init; }
    public List<string>? Options { get; init; }
}
```

## Usage Example

```razor
@page "/workflow-designer"

<WorkflowCanvas @ref="_canvas" Nodes="Nodes" Connections="Connections">
    <!-- Canvas content -->
</WorkflowCanvas>

<button class="btn btn-rules" @onclick="OpenRulesModal">
    <i class="bi bi-diagram-3"></i>
    Rules
</button>

<WorkflowRulesModal @bind-IsOpen="_rulesModalOpen"
                    @bind-SkipRules="SkipRules"
                    @bind-GoToRules="GoToRules"
                    @bind-CompletionRules="CompletionRules"
                    @bind-ValidationRules="ValidationRules"
                    WorkflowNodes="Nodes"
                    FormFields="FormFields"
                    OnSave="HandleRulesSaved" />

@code {
    private bool _rulesModalOpen = false;
    private List<SkipRule> SkipRules { get; set; } = new();
    private List<GoToRule> GoToRules { get; set; } = new();
    private List<CompletionRule> CompletionRules { get; set; } = new();
    private List<ValidationRule> ValidationRules { get; set; } = new();
    private List<WorkflowNode> Nodes { get; set; } = new();
    private List<FormFieldInfo> FormFields { get; set; } = new();

    private void OpenRulesModal() => _rulesModalOpen = true;

    private async Task HandleRulesSaved()
    {
        // Persist rules to storage
        await SaveWorkflowRules();
    }
}
```

## Claude Prompt for Implementation

```
Implement the WorkflowRulesModal component for managing workflow rules.

Requirements:
1. Modal with tabbed interface for 4 rule types: Skip, GoTo, Completion, Validation
2. Each tab shows a list of rules with add/edit/delete/toggle functionality
3. Rule cards display name, description, condition summary, and action summary
4. ConditionBuilder component for visual condition creation
5. Support for simple conditions (field comparisons) and compound conditions (AND/OR)
6. Priority ordering for rule evaluation
7. Dark mode support

Use the existing design system:
- CSS variables for colors (--primary: #6366f1, etc.)
- Consistent button styles and form inputs
- Modal animations and backdrop blur
- Bootstrap Icons for iconography

Data models should use C# records with immutability for state management.
Ensure all callbacks properly bubble changes up through two-way binding.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `WorkflowRulesModal-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing each rule type tab (Skip, GoTo, Completion, Validation)
- Rule creation workflow testing
- Rule editing and updating testing
- Rule deletion with confirmation testing
- Rule enable/disable toggle testing
- Condition builder simple condition testing
- Condition builder compound condition (AND/OR) testing
- Priority ordering verification
- Modal open/close animations

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Rule model files creation (WorkflowRule, SkipRule, GoToRule, CompletionRule, ValidationRule)
- RuleCondition model file creation
- Enum files (RuleType, ConditionType, LogicalOperator, ComparisonOperator, ValidationType)
- FormFieldInfo model creation
- CSS file imports
- Component registration in _Imports.razor
- Rule persistence/storage implementation
- RuleEngine service for condition evaluation

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens with animation
- [ ] Modal closes on backdrop click
- [ ] Modal closes on X button click
- [ ] All 4 tabs display correctly
- [ ] Tab switching works
- [ ] Tab badges show rule counts
- [ ] Empty state displays for no rules
- [ ] Add rule button creates new rule
- [ ] Rule cards display correctly
- [ ] Rule toggle enables/disables
- [ ] Rule edit opens editor
- [ ] Rule delete removes rule
- [ ] Condition builder works for simple conditions
- [ ] Compound conditions with AND work
- [ ] Compound conditions with OR work
- [ ] Priority badge displays
- [ ] Save button saves all rules
- [ ] Cancel button closes without saving
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

## Integration Notes

1. **State Management**: Rules are stored as separate lists by type for easier management
2. **Condition Evaluation**: Conditions are evaluated at runtime by a RuleEngine service
3. **Priority**: Lower priority numbers are evaluated first
4. **Form Fields**: FormFieldInfo provides metadata for condition building
5. **Workflow Nodes**: Node information enables step selection for skip/goto rules
