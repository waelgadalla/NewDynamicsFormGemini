using DynamicForms.Core.V4.Runtime;
using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Core.V4.Services;

/// <summary>
/// Service interface for evaluating conditional logic.
/// Supports both single-module and multi-module (workflow) scenarios.
/// </summary>
public interface IConditionEvaluator
{
    /// <summary>
    /// Evaluates a condition against workflow data (multi-module support).
    /// Use this method for workflow scenarios where conditions may reference fields from multiple modules.
    /// </summary>
    /// <param name="condition">Condition to evaluate</param>
    /// <param name="workflowData">Workflow form data containing all module data</param>
    /// <returns>True if condition is met, false otherwise</returns>
    bool Evaluate(Condition condition, WorkflowFormData workflowData);

    /// <summary>
    /// Evaluates a condition against single-module field data (backward compatibility).
    /// Use this method for single-module scenarios. Field references without module prefix are supported.
    /// </summary>
    /// <param name="condition">Condition to evaluate</param>
    /// <param name="fieldData">Dictionary of field values (FieldId => Value)</param>
    /// <param name="moduleKey">Optional module key for context (defaults to "current")</param>
    /// <returns>True if condition is met, false otherwise</returns>
    bool Evaluate(Condition condition, Dictionary<string, object?> fieldData, string? moduleKey = null);

    /// <summary>
    /// Evaluates a conditional rule and returns the action to perform.
    /// Checks if the rule's condition is met and returns the associated action if true.
    /// </summary>
    /// <param name="rule">Conditional rule to evaluate</param>
    /// <param name="workflowData">Workflow form data</param>
    /// <returns>RuleEvaluationResult containing whether rule triggered and which action to perform</returns>
    RuleEvaluationResult EvaluateRule(ConditionalRule rule, WorkflowFormData workflowData);

    /// <summary>
    /// Evaluates multiple rules and returns all triggered actions.
    /// Rules are evaluated in priority order (lower priority value = higher precedence).
    /// </summary>
    /// <param name="rules">Array of conditional rules to evaluate</param>
    /// <param name="workflowData">Workflow form data</param>
    /// <returns>Array of RuleEvaluationResult for all triggered rules</returns>
    RuleEvaluationResult[] EvaluateRules(ConditionalRule[] rules, WorkflowFormData workflowData);

    /// <summary>
    /// Parses a field reference string into module key and field ID components.
    /// Supports formats: "fieldId", "moduleKey.fieldId", "1.fieldId"
    /// </summary>
    /// <param name="fieldReference">Field reference string</param>
 /// <returns>Tuple of (moduleKey, fieldId). ModuleKey is null if no prefix.</returns>
    (string? moduleKey, string fieldId) ParseFieldReference(string fieldReference);
}

/// <summary>
/// Result of evaluating a conditional rule.
/// </summary>
public record RuleEvaluationResult
{
    /// <summary>
    /// The rule that was evaluated
    /// </summary>
    public required ConditionalRule Rule { get; init; }

    /// <summary>
    /// Whether the rule's condition evaluated to true and the rule was triggered
    /// </summary>
    public bool IsTriggered { get; init; }

    /// <summary>
    /// The action to perform if the rule was triggered.
    /// Null if IsTriggered is false.
    /// </summary>
    public string? ActionToPerform => IsTriggered ? Rule.Action : null;

    /// <summary>
    /// Target field ID if this is a field-level action.
    /// </summary>
    public string? TargetFieldId => Rule.TargetFieldId;

    /// <summary>
    /// Target step number if this is a workflow-level action.
    /// </summary>
    public int? TargetStepNumber => Rule.TargetStepNumber;

    /// <summary>
    /// Target module key if this is a module-level action.
    /// </summary>
 public string? TargetModuleKey => Rule.TargetModuleKey;

    /// <summary>
    /// Error message if evaluation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Whether evaluation encountered an error.
 /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
}
