using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.Json;
using VisualEditorOpus.Models;
using DynamicForms.Core.V4.Enums;
using DynamicForms.Core.V4.Schemas;

namespace VisualEditorOpus.Components.Editor.Modals;

/// <summary>
/// Code-behind for the ConditionBuilderModal component.
/// </summary>
public partial class ConditionBuilderModal : ComponentBase
{
    private static readonly Dictionary<string, (string Label, string Icon)> Actions = new()
    {
        ["show"] = ("Show", "bi-eye"),
        ["hide"] = ("Hide", "bi-eye-slash"),
        ["enable"] = ("Enable", "bi-unlock"),
        ["disable"] = ("Disable", "bi-lock"),
        ["setRequired"] = ("Set Required", "bi-asterisk"),
        ["setOptional"] = ("Set Optional", "bi-dash")
    };

    private List<ConditionGroupModel> rootGroups = new();
    private List<FieldReference> availableFields = new();
    private string selectedAction = "show";
    private string? selectedTargetFieldId;
    private string previewTab = "human";
    private List<string> validationErrors = new();

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
    /// Existing rule to edit (null for new rule).
    /// </summary>
    [Parameter]
    public ConditionalRule? ExistingRule { get; set; }

    /// <summary>
    /// Callback when the rule is saved.
    /// </summary>
    [Parameter]
    public EventCallback<ConditionalRule> OnSave { get; set; }

    /// <summary>
    /// Callback when the modal is cancelled.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// The current module for field references.
    /// </summary>
    [Parameter]
    public FormModuleSchema? CurrentModule { get; set; }

    /// <summary>
    /// The workflow containing all modules (for cross-module references).
    /// </summary>
    [Parameter]
    public FormWorkflowSchema? Workflow { get; set; }

    /// <summary>
    /// Dictionary of workflow modules keyed by step number.
    /// </summary>
    [Parameter]
    public Dictionary<int, FormModuleSchema>? WorkflowModules { get; set; }

    /// <summary>
    /// Default target field ID when creating a new rule.
    /// </summary>
    [Parameter]
    public string? DefaultTargetFieldId { get; set; }

    /// <summary>
    /// Default action when creating a new rule.
    /// </summary>
    [Parameter]
    public string DefaultAction { get; set; } = "show";

    private bool CanSave => !string.IsNullOrEmpty(selectedTargetFieldId) &&
                            !string.IsNullOrEmpty(selectedAction) &&
                            rootGroups.Any(g => !g.IsEmpty) &&
                            !validationErrors.Any();

    private Dictionary<string, List<FieldReference>> TargetFieldsByModule =>
        availableFields
            .GroupBy(f => f.ModuleName ?? "Current Module")
            .ToDictionary(g => g.Key, g => g.ToList());

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
        // Build field references
        availableFields = BuildFieldReferences();

        // Initialize from existing rule or defaults
        if (ExistingRule != null)
        {
            selectedAction = ExistingRule.Action;
            selectedTargetFieldId = ExistingRule.TargetFieldId;
            rootGroups = new List<ConditionGroupModel> { FromCondition(ExistingRule.Condition) };
        }
        else
        {
            selectedAction = DefaultAction;
            selectedTargetFieldId = DefaultTargetFieldId;

            // Start with one empty group with one empty condition
            var initialGroup = new ConditionGroupModel();
            initialGroup.Conditions.Add(new ConditionRowModel());
            rootGroups = new List<ConditionGroupModel> { initialGroup };
        }

        validationErrors.Clear();
    }

    private List<FieldReference> BuildFieldReferences()
    {
        var refs = new List<FieldReference>();

        // Current module fields
        if (CurrentModule?.Fields != null)
        {
            foreach (var field in CurrentModule.Fields)
            {
                refs.Add(new FieldReference(
                    FieldId: field.Id,
                    DisplayName: $"{field.Id} ({field.FieldType})",
                    FieldType: field.FieldType,
                    ModuleKey: null,
                    ModuleName: "Current Module",
                    Options: field.Options?.Select(o => new Models.FieldOption(o.Value, o.LabelEn ?? o.Value)).ToList()
                ));
            }
        }

        // Cross-module fields
        if (WorkflowModules != null)
        {
            foreach (var (stepNumber, module) in WorkflowModules)
            {
                if (module.Id == CurrentModule?.Id) continue;

                var moduleKey = $"Step{stepNumber}";
                foreach (var field in module.Fields ?? Array.Empty<FormFieldSchema>())
                {
                    refs.Add(new FieldReference(
                        FieldId: $"{moduleKey}.{field.Id}",
                        DisplayName: $"{field.Id} ({field.FieldType})",
                        FieldType: field.FieldType,
                        ModuleKey: moduleKey,
                        ModuleName: $"{moduleKey} - {module.TitleEn ?? $"Module {stepNumber}"}",
                        Options: field.Options?.Select(o => new Models.FieldOption(o.Value, o.LabelEn ?? o.Value)).ToList()
                    ));
                }
            }
        }

        return refs;
    }

    private string GetModalTitle()
    {
        return ExistingRule != null ? "Edit Condition" : "Create Condition";
    }

    private void SelectAction(string action)
    {
        selectedAction = action;
        StateHasChanged();
    }

    private string GetActionPastTense(string action)
    {
        return action switch
        {
            "show" => "shown",
            "hide" => "hidden",
            "enable" => "enabled",
            "disable" => "disabled",
            "setRequired" => "set as required",
            "setOptional" => "set as optional",
            _ => action
        };
    }

    private void AddGroup()
    {
        var newGroup = new ConditionGroupModel();
        newGroup.Conditions.Add(new ConditionRowModel());
        rootGroups.Add(newGroup);
        StateHasChanged();
    }

    private void RemoveGroup(ConditionGroupModel group)
    {
        rootGroups.Remove(group);
        // Ensure at least one group exists
        if (!rootGroups.Any())
        {
            var newGroup = new ConditionGroupModel();
            newGroup.Conditions.Add(new ConditionRowModel());
            rootGroups.Add(newGroup);
        }
        StateHasChanged();
    }

    private void HandleConditionChanged()
    {
        Validate();
        StateHasChanged();
    }

    private void Validate()
    {
        validationErrors.Clear();

        if (string.IsNullOrEmpty(selectedTargetFieldId))
        {
            validationErrors.Add("Please select a target field.");
        }

        if (string.IsNullOrEmpty(selectedAction))
        {
            validationErrors.Add("Please select an action.");
        }

        var allConditions = rootGroups.SelectMany(GetAllConditions).ToList();
        if (!allConditions.Any())
        {
            validationErrors.Add("Please add at least one condition.");
        }
        else
        {
            var incompleteCount = allConditions.Count(c => !c.IsComplete);
            if (incompleteCount > 0)
            {
                validationErrors.Add($"{incompleteCount} condition(s) are incomplete.");
            }
        }
    }

    private IEnumerable<ConditionRowModel> GetAllConditions(ConditionGroupModel group)
    {
        foreach (var condition in group.Conditions)
        {
            yield return condition;
        }
        foreach (var nestedGroup in group.NestedGroups)
        {
            foreach (var condition in GetAllConditions(nestedGroup))
            {
                yield return condition;
            }
        }
    }

    private async Task HandleSave()
    {
        Validate();
        if (validationErrors.Any()) return;

        var rule = BuildRule();
        await OnSave.InvokeAsync(rule);
        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await IsOpenChanged.InvokeAsync(false);
    }

    private ConditionalRule BuildRule()
    {
        var condition = BuildConditionFromGroups();

        return new ConditionalRule
        {
            Id = ExistingRule?.Id ?? $"rule_{Guid.NewGuid():N}",
            Description = GenerateDescription(),
            TargetFieldId = selectedTargetFieldId,
            Action = selectedAction,
            Condition = condition,
            Priority = ExistingRule?.Priority ?? 100,
            IsActive = ExistingRule?.IsActive ?? true
        };
    }

    private Condition BuildConditionFromGroups()
    {
        if (rootGroups.Count == 1)
        {
            return ToCondition(rootGroups[0]);
        }

        // Multiple root groups are ANDed together
        var conditions = rootGroups
            .Where(g => !g.IsEmpty)
            .Select(ToCondition)
            .ToArray();

        return new Condition
        {
            LogicalOp = LogicalOperator.And,
            Conditions = conditions
        };
    }

    private Condition ToCondition(ConditionGroupModel group)
    {
        var allItems = new List<Condition>();

        // Add simple conditions
        foreach (var row in group.Conditions.Where(c => c.IsComplete))
        {
            allItems.Add(new Condition
            {
                Field = row.FieldId,
                Operator = row.Operator,
                Value = ParseValue(row.Value, row.FieldType)
            });
        }

        // Add nested groups
        foreach (var nested in group.NestedGroups.Where(g => !g.IsEmpty))
        {
            allItems.Add(ToCondition(nested));
        }

        // Single item - return as-is (unless it's a NOT group)
        if (allItems.Count == 1 && group.LogicalOp != LogicalOperator.Not)
        {
            return allItems[0];
        }

        return new Condition
        {
            LogicalOp = group.LogicalOp,
            Conditions = allItems.ToArray()
        };
    }

    private static ConditionGroupModel FromCondition(Condition condition)
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

    private static object? ParseValue(string? value, string? fieldType)
    {
        if (string.IsNullOrEmpty(value)) return null;

        return fieldType switch
        {
            "Number" or "Currency" => decimal.TryParse(value, out var num) ? num : value,
            "Checkbox" => bool.TryParse(value, out var b) ? b : value,
            _ => value
        };
    }

    private string GenerateDescription()
    {
        var targetField = availableFields.FirstOrDefault(f => f.FieldId == selectedTargetFieldId);
        var targetName = targetField?.DisplayName ?? selectedTargetFieldId ?? "field";
        return $"{Actions[selectedAction].Label} {targetName} based on conditions";
    }

    private string GenerateHumanReadablePreview()
    {
        var sb = new StringBuilder();

        var targetField = availableFields.FirstOrDefault(f => f.FieldId == selectedTargetFieldId);
        var targetName = targetField?.FieldId ?? selectedTargetFieldId ?? "?";

        sb.Append($"<span class=\"action\">{selectedAction.ToUpper()}</span> ");
        sb.Append($"field <span class=\"field\">\"{targetName}\"</span> ");
        sb.Append("<span class=\"keyword\">WHEN</span><br><br>");

        var groupTexts = rootGroups
            .Where(g => !g.IsEmpty)
            .Select(g => GenerateGroupPreview(g, 1))
            .ToList();

        sb.Append(string.Join("<br><span class=\"keyword\">AND</span><br>", groupTexts));

        return sb.ToString();
    }

    private string GenerateGroupPreview(ConditionGroupModel group, int indent)
    {
        var sb = new StringBuilder();
        var padding = new string(' ', indent * 2);

        var items = new List<string>();

        foreach (var condition in group.Conditions.Where(c => c.IsComplete))
        {
            var opDisplay = GetOperatorSymbol(condition.Operator);
            var valueDisplay = condition.RequiresValue
                ? $"<span class=\"value\">\"{condition.Value}\"</span>"
                : "";

            items.Add($"<span class=\"field\">{condition.FieldId}</span> " +
                      $"<span class=\"operator\">{opDisplay}</span> {valueDisplay}");
        }

        foreach (var nested in group.NestedGroups.Where(g => !g.IsEmpty))
        {
            items.Add("(" + GenerateGroupPreview(nested, indent + 1) + ")");
        }

        var separator = group.LogicalOp switch
        {
            LogicalOperator.And => $" <span class=\"keyword\">AND</span> ",
            LogicalOperator.Or => $" <span class=\"keyword\">OR</span> ",
            LogicalOperator.Not => $" <span class=\"keyword\">NOT</span> ",
            _ => " AND "
        };

        if (group.LogicalOp == LogicalOperator.Not && items.Count == 1)
        {
            sb.Append($"<span class=\"keyword\">NOT</span> ({items[0]})");
        }
        else
        {
            sb.Append($"{padding}({string.Join(separator, items)})");
        }

        return sb.ToString();
    }

    private static string GetOperatorSymbol(ConditionOperator op)
    {
        return op switch
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
            ConditionOperator.IsNull => "is null",
            ConditionOperator.IsNotNull => "is not null",
            ConditionOperator.IsEmpty => "is empty",
            ConditionOperator.IsNotEmpty => "is not empty",
            _ => op.ToString()
        };
    }

    private string GenerateJsonPreview()
    {
        try
        {
            var rule = BuildRule();
            return JsonSerializer.Serialize(rule, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return "{ \"error\": \"Unable to generate preview\" }";
        }
    }
}
