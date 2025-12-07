using System.Text.Json.Serialization;
using DynamicForms.Core.V4.Enums;

namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Represents a logical condition for rules (e.g. Visibility, Validation).
/// Recursive structure supports nested groups (AND/OR).
/// Enables cross-module field references using dot notation (ModuleKey.FieldId).
/// </summary>
public record Condition
{
    // ===== Simple Condition (Leaf Node) =====

    /// <summary>
    /// The field ID to evaluate (e.g. "age", "Step1.total").
    /// Null if this is a complex group.
    /// </summary>
    public string? Field { get; init; }

    /// <summary>
    /// The operator to apply (Equals, GreaterThan, etc.).
    /// </summary>
    public ConditionOperator? Operator { get; init; }

    /// <summary>
    /// The value to compare against.
    /// </summary>
    public object? Value { get; init; }

    // ===== Complex Condition (Branch Node) =====

    /// <summary>
    /// Logical operator for combining multiple sub-conditions.
    /// Only used for complex conditions (when Conditions array is populated).
    /// </summary>
    public LogicalOperator? LogicalOp { get; init; }

    /// <summary>
    /// Child conditions for this group.
    /// </summary>
    public Condition[]? Conditions { get; init; }

    // ===== Helpers =====
    
    [JsonIgnore]
    public bool IsSimpleCondition => !string.IsNullOrWhiteSpace(Field) && Operator.HasValue;

    [JsonIgnore]
    public bool IsComplexCondition => LogicalOp.HasValue && Conditions != null && Conditions.Length > 0;
}