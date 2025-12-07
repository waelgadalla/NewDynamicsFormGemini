namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Represents a module within a workflow with its metadata and branching logic.
/// Used for workflow editor display and management.
/// </summary>
public record WorkflowModuleInfo
{
    /// <summary>
    /// The module ID (references FormModuleSchema.Id)
    /// </summary>
    public required int ModuleId { get; init; }

    /// <summary>
    /// English title of the module
    /// </summary>
    public string? TitleEn { get; init; }

    /// <summary>
    /// French title of the module
    /// </summary>
    public string? TitleFr { get; init; }

    /// <summary>
    /// Order/position in the workflow (0-based)
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// Conditional branching rules for this module
    /// </summary>
    public ConditionalBranch? Branch { get; init; }

    /// <summary>
    /// Whether this module is required for workflow completion
    /// </summary>
    public bool IsRequired { get; init; } = true;

    /// <summary>
    /// Whether this module can be skipped by the user
    /// </summary>
    public bool IsSkippable { get; init; } = false;
}

/// <summary>
/// Defines conditional branching logic for a workflow module
/// </summary>
/// <param name="ConditionFieldId">Field ID to evaluate</param>
/// <param name="Operator">Comparison operator (Equals, NotEquals, etc.)</param>
/// <param name="Value">Value to compare against</param>
/// <param name="NextModuleIdIfTrue">Module ID to navigate to if condition is true</param>
/// <param name="NextModuleIdIfFalse">Module ID to navigate to if condition is false (null = continue to next)</param>
public record ConditionalBranch(
    string ConditionFieldId,
    string Operator,
    string Value,
    int? NextModuleIdIfTrue = null,
    int? NextModuleIdIfFalse = null
);
