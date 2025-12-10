namespace VisualEditorOpus.Models;

using DynamicForms.Core.V4.Enums;

/// <summary>
/// UI model for a condition group in the condition builder.
/// Supports nested groups and multiple conditions.
/// </summary>
public class ConditionGroupModel
{
    /// <summary>
    /// Unique identifier for this group.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Logical operator for combining conditions (AND, OR, NOT).
    /// </summary>
    public LogicalOperator LogicalOp { get; set; } = LogicalOperator.And;

    /// <summary>
    /// List of condition rows in this group.
    /// </summary>
    public List<ConditionRowModel> Conditions { get; set; } = new();

    /// <summary>
    /// Nested condition groups for complex logic.
    /// </summary>
    public List<ConditionGroupModel> NestedGroups { get; set; } = new();

    /// <summary>
    /// Whether this group has no conditions or nested groups.
    /// </summary>
    public bool IsEmpty => Conditions.Count == 0 && NestedGroups.Count == 0;

    /// <summary>
    /// Total count of all conditions including nested groups.
    /// </summary>
    public int TotalConditionCount => Conditions.Count + NestedGroups.Sum(g => g.TotalConditionCount);
}
