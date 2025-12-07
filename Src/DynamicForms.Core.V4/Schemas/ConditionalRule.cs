namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Enhanced conditional rule supporting workflow branching and cross-module field references.
/// Defines conditions that trigger actions on fields, modules, or workflow steps.
/// Supports single-module field visibility, cross-module dependencies, and workflow navigation.
/// </summary>
public record ConditionalRule
{
    /// <summary>
    /// Unique identifier for this rule
    /// </summary>
  public required string Id { get; init; }

    /// <summary>
 /// Human-readable description of what this rule does (for documentation and debugging)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Target field ID for field-level actions (show, hide, enable, disable, setRequired).
    /// Use this for actions that affect a specific field within a module.
    /// </summary>
    public string? TargetFieldId { get; init; }

    /// <summary>
    /// Target step number for workflow-level actions (skipStep, goToStep).
    /// Step numbers are 1-based (Step 1 = first module, Step 2 = second module, etc.)
    /// </summary>
    public int? TargetStepNumber { get; init; }

    /// <summary>
    /// Target module key or ID for module-level actions.
    /// Can be numeric ID ("1") or module name/key ("PersonalInfo").
    /// Used for module-scoped operations.
    /// </summary>
    public string? TargetModuleKey { get; init; }

    /// <summary>
    /// Action to perform when the condition evaluates to true.
    /// 
    /// Field-level actions:
    /// - "show" - Make field visible
    /// - "hide" - Make field hidden
    /// - "enable" - Make field editable
    /// - "disable" - Make field read-only
    /// - "setRequired" - Make field required
    /// - "setOptional" - Make field optional
    /// 
    /// Workflow-level actions:
  /// - "skipStep" - Skip a workflow step/module
    /// - "goToStep" - Navigate to a specific step
    /// - "completeWorkflow" - Mark workflow as complete
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Condition that determines whether the action should be triggered.
    /// Supports simple conditions (single field comparison) and complex conditions (AND/OR/NOT logic).
    /// Can reference fields from the current module or other modules using dot notation.
    /// </summary>
    public required Condition Condition { get; init; }

    /// <summary>
  /// Priority for rule execution order when multiple rules apply to the same target.
  /// Lower values have higher priority (e.g., Priority=10 runs before Priority=100).
    /// Default is 100. Use lower values for critical rules that should run first.
    /// </summary>
    public int Priority { get; init; } = 100;

    /// <summary>
    /// Whether this rule is currently active.
    /// Inactive rules are not evaluated. Useful for temporarily disabling rules without deleting them.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Optional group/category for organizing rules.
    /// Example: "validation", "workflow-navigation", "conditional-visibility"
    /// </summary>
    public string? Category { get; init; }

 /// <summary>
    /// Tags for searching and filtering rules.
    /// Example: ["minors", "parental-consent", "age-verification"]
    /// </summary>
  public string[]? Tags { get; init; }
}
