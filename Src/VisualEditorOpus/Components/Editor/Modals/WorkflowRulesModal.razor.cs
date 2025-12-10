using Microsoft.AspNetCore.Components;
using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Enums;

namespace VisualEditorOpus.Components.Editor.Modals;

/// <summary>
/// Code-behind for the WorkflowRulesModal component.
/// Manages workflow-level conditional rules for step navigation and workflow completion.
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

    // State
    private List<WorkflowRuleViewModel> rules = new();
    private bool isAddingRule;
    private bool isEditingRule;
    private string? editingRuleId;

    // Form state for add/edit
    private string selectedAction = "skipStep";
    private string ruleDescription = "";
    private int? targetStepNumber;
    private Condition? currentCondition;
    private int rulePriority = 100;

    // Condition builder modal
    private bool showConditionBuilder;

    // Action type definitions
    private static readonly List<ActionTypeInfo> ActionTypes = new()
    {
        new("skipStep", "Skip Step", "Skip a specific step/module", "bi-skip-forward", "skip"),
        new("goToStep", "Go To Step", "Navigate to a specific step", "bi-arrow-right-circle", "goto"),
        new("completeWorkflow", "Complete", "Complete the workflow", "bi-check-circle", "complete")
    };

    private bool IsRuleValid =>
        !string.IsNullOrWhiteSpace(ruleDescription) &&
        currentCondition != null &&
        (selectedAction == "completeWorkflow" || targetStepNumber.HasValue);

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
                Condition = r.Condition,
                Priority = r.Priority,
                IsActive = r.IsActive
            })
            .ToList();

        ResetForm();
    }

    private static bool IsWorkflowRule(string action) =>
        action is "skipStep" or "goToStep" or "completeWorkflow";

    private void ResetForm()
    {
        isAddingRule = false;
        isEditingRule = false;
        editingRuleId = null;
        selectedAction = "skipStep";
        ruleDescription = "";
        targetStepNumber = null;
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
                rule.TargetStepNumber = selectedAction == "completeWorkflow" ? null : targetStepNumber;
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
                TargetStepNumber = selectedAction == "completeWorkflow" ? null : targetStepNumber,
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

    private void HandleConditionSaved(Condition condition)
    {
        currentCondition = condition;
        showConditionBuilder = false;
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
        "skipStep" => ("bi-skip-forward", "skip", "Skip Step"),
        "goToStep" => ("bi-arrow-right-circle", "goto", "Go To Step"),
        "completeWorkflow" => ("bi-check-circle", "complete", "Complete Workflow"),
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
    /// View model for workflow rules.
    /// </summary>
    private class WorkflowRuleViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Action { get; set; } = "skipStep";
        public string Description { get; set; } = "";
        public int? TargetStepNumber { get; set; }
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
