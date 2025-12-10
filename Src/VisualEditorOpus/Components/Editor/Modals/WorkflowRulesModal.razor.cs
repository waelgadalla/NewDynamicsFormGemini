using Microsoft.AspNetCore.Components;
using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Enums;

namespace VisualEditorOpus.Components.Editor.Modals;

/// <summary>
/// Code-behind for the WorkflowRulesModal component.
/// Manages workflow-level conditional rules for step navigation and workflow completion.
/// Provides a tabbed interface for different rule types.
/// </summary>
public partial class WorkflowRulesModal : ComponentBase
{
    /// <summary>
    /// Gets or sets whether the modal is open.
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>
    /// Event callback for two-way binding of IsOpen.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsOpenChanged { get; set; }

    /// <summary>
    /// The workflow schema containing module information.
    /// </summary>
    [Parameter]
    public FormWorkflowSchema Workflow { get; set; } = default!;

    /// <summary>
    /// List of modules in the workflow (for getting step titles and fields).
    /// </summary>
    [Parameter]
    public List<FormModuleSchema> WorkflowModules { get; set; } = new();

    /// <summary>
    /// Existing workflow rules to edit.
    /// </summary>
    [Parameter]
    public List<ConditionalRule> WorkflowRules { get; set; } = new();

    /// <summary>
    /// Callback when rules are saved.
    /// </summary>
    [Parameter]
    public EventCallback<List<ConditionalRule>> OnSave { get; set; }

    /// <summary>
    /// Callback when the modal is cancelled.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    // Tab state
    private RuleTab activeTab = RuleTab.Skip;

    // State
    private List<WorkflowRuleViewModel> rules = new();
    private bool isAddingRule;
    private bool isEditingRule;
    private string? editingRuleId;

    // Form state for add/edit
    private string selectedAction = "skipStep";
    private string ruleDescription = "";
    private int? targetStepNumber;
    private string? targetFieldId;
    private Condition? currentCondition;
    private int rulePriority = 100;

    // Condition builder modal
    private bool showConditionBuilder;

    // Action type definitions
    private static readonly List<ActionTypeInfo> ActionTypes = new()
    {
        new("skipStep", "Skip Step", "Skip a specific step/module", "bi-skip-forward-fill", "skip"),
        new("goToStep", "Go To Step", "Navigate to a specific step", "bi-arrow-return-left", "goto"),
        new("completeWorkflow", "Complete", "Complete the workflow", "bi-check-circle-fill", "complete"),
        new("validate", "Validate", "Custom field validation", "bi-shield-check", "validate")
    };

    private bool IsRuleValid =>
        !string.IsNullOrWhiteSpace(ruleDescription) &&
        currentCondition != null &&
        (selectedAction == "completeWorkflow" ||
         selectedAction == "validate" ? !string.IsNullOrEmpty(targetFieldId) : targetStepNumber.HasValue);

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (IsOpen)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        // Convert ConditionalRules to ViewModels
        rules = WorkflowRules
            .Where(r => IsWorkflowRule(r.Action))
            .Select(r => new WorkflowRuleViewModel
            {
                Id = r.Id,
                Action = r.Action,
                Description = r.Description ?? "",
                TargetStepNumber = r.TargetStepNumber,
                TargetFieldId = r.TargetFieldId,
                Condition = r.Condition,
                Priority = r.Priority,
                IsActive = r.IsActive
            })
            .ToList();

        ResetForm();
    }

    private static bool IsWorkflowRule(string action) =>
        action is "skipStep" or "goToStep" or "completeWorkflow" or "validate";

    private void SetActiveTab(RuleTab tab)
    {
        activeTab = tab;
        ResetForm();

        // Set default action based on tab
        selectedAction = tab switch
        {
            RuleTab.Skip => "skipStep",
            RuleTab.GoTo => "goToStep",
            RuleTab.Completion => "completeWorkflow",
            RuleTab.Validation => "validate",
            _ => "skipStep"
        };
    }

    private int GetRuleCountByType(string action)
    {
        return rules.Count(r => r.Action == action);
    }

    private List<WorkflowRuleViewModel> GetFilteredRules()
    {
        var actionType = activeTab switch
        {
            RuleTab.Skip => "skipStep",
            RuleTab.GoTo => "goToStep",
            RuleTab.Completion => "completeWorkflow",
            RuleTab.Validation => "validate",
            _ => ""
        };

        return rules
            .Where(r => r.Action == actionType)
            .OrderBy(r => r.Priority)
            .ToList();
    }

    private List<ActionTypeInfo> GetAvailableActions()
    {
        return activeTab switch
        {
            RuleTab.Skip => ActionTypes.Where(a => a.Id == "skipStep").ToList(),
            RuleTab.GoTo => ActionTypes.Where(a => a.Id == "goToStep").ToList(),
            RuleTab.Completion => ActionTypes.Where(a => a.Id == "completeWorkflow").ToList(),
            RuleTab.Validation => ActionTypes.Where(a => a.Id == "validate").ToList(),
            _ => ActionTypes.ToList()
        };
    }

    private string GetTabTitle() => activeTab switch
    {
        RuleTab.Skip => "Skip Rule",
        RuleTab.GoTo => "Go To Rule",
        RuleTab.Completion => "Completion Rule",
        RuleTab.Validation => "Validation Rule",
        _ => "Rule"
    };

    private string GetEmptyStateIcon() => activeTab switch
    {
        RuleTab.Skip => "bi-skip-forward",
        RuleTab.GoTo => "bi-arrow-return-right",
        RuleTab.Completion => "bi-check-circle",
        RuleTab.Validation => "bi-shield-check",
        _ => "bi-inbox"
    };

    private string GetEmptyStateDescription() => activeTab switch
    {
        RuleTab.Skip => "Skip rules allow steps to be skipped based on conditions.",
        RuleTab.GoTo => "Go To rules redirect the workflow to different steps.",
        RuleTab.Completion => "Completion rules control what happens when the workflow ends.",
        RuleTab.Validation => "Validation rules perform custom field validation.",
        _ => "No rules defined."
    };

    private string GetActionLabel(string action) => action switch
    {
        "skipStep" => "Then Skip",
        "goToStep" => "Then Go To",
        "completeWorkflow" => "Then Complete",
        "validate" => "Validate Field",
        _ => "Then"
    };

    private string GetActionValue(WorkflowRuleViewModel rule) => rule.Action switch
    {
        "skipStep" when rule.TargetStepNumber.HasValue => GetStepName(rule.TargetStepNumber.Value),
        "goToStep" when rule.TargetStepNumber.HasValue => GetStepName(rule.TargetStepNumber.Value),
        "completeWorkflow" => "Workflow completion",
        "validate" when !string.IsNullOrEmpty(rule.TargetFieldId) => rule.TargetFieldId,
        _ => "Unknown"
    };

    private string GetStepName(int stepNumber)
    {
        if (WorkflowModules.Count >= stepNumber && stepNumber > 0)
        {
            var module = WorkflowModules[stepNumber - 1];
            return $"Step {stepNumber}: {module.TitleEn ?? "Untitled"}";
        }
        return $"Step {stepNumber}";
    }

    private void ResetForm()
    {
        isAddingRule = false;
        isEditingRule = false;
        editingRuleId = null;
        selectedAction = activeTab switch
        {
            RuleTab.Skip => "skipStep",
            RuleTab.GoTo => "goToStep",
            RuleTab.Completion => "completeWorkflow",
            RuleTab.Validation => "validate",
            _ => "skipStep"
        };
        ruleDescription = "";
        targetStepNumber = null;
        targetFieldId = null;
        currentCondition = null;
        rulePriority = 100;
    }

    private void AddRule()
    {
        ResetForm();
        isAddingRule = true;
    }

    private void EditRule(string ruleId)
    {
        var rule = rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule == null) return;

        isEditingRule = true;
        editingRuleId = ruleId;
        selectedAction = rule.Action;
        ruleDescription = rule.Description;
        targetStepNumber = rule.TargetStepNumber;
        targetFieldId = rule.TargetFieldId;
        currentCondition = rule.Condition;
        rulePriority = rule.Priority;
    }

    private void DeleteRule(string ruleId)
    {
        rules.RemoveAll(r => r.Id == ruleId);
    }

    private void ToggleRuleActive(string ruleId)
    {
        var rule = rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null)
        {
            rule.IsActive = !rule.IsActive;
        }
    }

    private void SelectAction(string actionId)
    {
        selectedAction = actionId;
        if (actionId == "completeWorkflow")
        {
            targetStepNumber = null;
        }
        if (actionId != "validate")
        {
            targetFieldId = null;
        }
    }

    private void SaveRule()
    {
        if (!IsRuleValid) return;

        if (isEditingRule && editingRuleId != null)
        {
            // Update existing rule
            var rule = rules.FirstOrDefault(r => r.Id == editingRuleId);
            if (rule != null)
            {
                rule.Action = selectedAction;
                rule.Description = ruleDescription;
                rule.TargetStepNumber = selectedAction is "completeWorkflow" or "validate" ? null : targetStepNumber;
                rule.TargetFieldId = selectedAction == "validate" ? targetFieldId : null;
                rule.Condition = currentCondition;
                rule.Priority = rulePriority;
            }
        }
        else
        {
            // Add new rule
            rules.Add(new WorkflowRuleViewModel
            {
                Id = Guid.NewGuid().ToString(),
                Action = selectedAction,
                Description = ruleDescription,
                TargetStepNumber = selectedAction is "completeWorkflow" or "validate" ? null : targetStepNumber,
                TargetFieldId = selectedAction == "validate" ? targetFieldId : null,
                Condition = currentCondition,
                Priority = rulePriority,
                IsActive = true
            });
        }

        ResetForm();
    }

    private void CancelEdit()
    {
        ResetForm();
    }

    private void OpenConditionBuilder()
    {
        showConditionBuilder = true;
    }

    private void HandleConditionSaved(ConditionalRule savedRule)
    {
        // Extract condition from the saved rule
        currentCondition = savedRule.Condition;
        showConditionBuilder = false;
    }

    private ConditionalRule? GetExistingRuleForConditionBuilder()
    {
        if (currentCondition == null) return null;

        return new ConditionalRule
        {
            Id = Guid.NewGuid().ToString(),
            Action = selectedAction,
            Condition = currentCondition
        };
    }

    private int GetActiveRuleCount() => rules.Count(r => r.IsActive);

    private List<(int Number, string Title)> GetAvailableSteps()
    {
        var steps = new List<(int Number, string Title)>();

        if (WorkflowModules.Count > 0)
        {
            for (int i = 0; i < WorkflowModules.Count; i++)
            {
                var module = WorkflowModules[i];
                steps.Add((i + 1, module.TitleEn ?? $"Step {i + 1}"));
            }
        }
        else if (Workflow?.ModuleIds != null)
        {
            for (int i = 0; i < Workflow.ModuleIds.Length; i++)
            {
                steps.Add((i + 1, $"Step {i + 1}"));
            }
        }

        return steps;
    }

    private List<WorkflowFieldOption> GetAllWorkflowFields()
    {
        var fields = new List<WorkflowFieldOption>();

        for (int i = 0; i < WorkflowModules.Count; i++)
        {
            var module = WorkflowModules[i];
            var stepPrefix = $"Step{i + 1}";

            foreach (var field in module.Fields ?? Array.Empty<FormFieldSchema>())
            {
                fields.Add(new WorkflowFieldOption
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

    private static (string Icon, string CssClass, string Label) GetActionInfo(string action) => action switch
    {
        "skipStep" => ("bi-skip-forward-fill", "skip", "Skip"),
        "goToStep" => ("bi-arrow-return-left", "goto", "Go To"),
        "completeWorkflow" => ("bi-check-circle-fill", "complete", "Complete"),
        "validate" => ("bi-shield-check", "validate", "Validate"),
        _ => ("bi-question", "", action)
    };

    private string GetRuleTargetDescription(WorkflowRuleViewModel rule)
    {
        return rule.Action switch
        {
            "skipStep" when rule.TargetStepNumber.HasValue =>
                $"Skip Step {rule.TargetStepNumber} when condition is met",
            "goToStep" when rule.TargetStepNumber.HasValue =>
                $"Navigate to Step {rule.TargetStepNumber} when condition is met",
            "completeWorkflow" =>
                "Complete workflow immediately when condition is met",
            "validate" when !string.IsNullOrEmpty(rule.TargetFieldId) =>
                $"Validate {rule.TargetFieldId} when condition is met",
            _ => "Unknown action"
        };
    }

    private string FormatCondition(Condition? condition)
    {
        if (condition == null) return "No condition";

        if (condition.IsComplexCondition && condition.Conditions?.Length > 0)
        {
            var parts = condition.Conditions.Select(c => FormatCondition(c));
            var op = condition.LogicalOp == LogicalOperator.And ? " AND " : " OR ";
            return $"({string.Join(op, parts)})";
        }

        if (condition.IsSimpleCondition && condition.Field != null && condition.Operator.HasValue)
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
        ConditionOperator.GreaterThanOrEqual => ">=",
        ConditionOperator.LessThan => "<",
        ConditionOperator.LessThanOrEqual => "<=",
        ConditionOperator.Contains => "contains",
        ConditionOperator.NotContains => "not contains",
        ConditionOperator.StartsWith => "starts with",
        ConditionOperator.EndsWith => "ends with",
        ConditionOperator.In => "in",
        ConditionOperator.NotIn => "not in",
        _ => op.ToString()
    };

    private async Task HandleSave()
    {
        // Convert ViewModels back to ConditionalRules
        var conditionalRules = rules.Select(r => new ConditionalRule
        {
            Id = r.Id,
            Description = string.IsNullOrWhiteSpace(r.Description) ? null : r.Description,
            Action = r.Action,
            TargetStepNumber = r.TargetStepNumber,
            TargetFieldId = r.TargetFieldId,
            Condition = r.Condition!,
            Priority = r.Priority,
            IsActive = r.IsActive
        }).ToList();

        await OnSave.InvokeAsync(conditionalRules);
        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await IsOpenChanged.InvokeAsync(false);
    }

    /// <summary>
    /// Tab enumeration for the rules modal.
    /// </summary>
    private enum RuleTab
    {
        Skip,
        GoTo,
        Completion,
        Validation
    }

    /// <summary>
    /// View model for workflow rules.
    /// </summary>
    private class WorkflowRuleViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Action { get; set; } = "skipStep";
        public string Description { get; set; } = "";
        public int? TargetStepNumber { get; set; }
        public string? TargetFieldId { get; set; }
        public Condition? Condition { get; set; }
        public int Priority { get; set; } = 100;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Information about an action type.
    /// </summary>
    private record ActionTypeInfo(string Id, string Name, string Description, string Icon, string CssClass);

    /// <summary>
    /// Field option for condition builder.
    /// </summary>
    public class WorkflowFieldOption
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
        public string Type { get; set; } = "";
        public string Group { get; set; } = "";
    }
}
