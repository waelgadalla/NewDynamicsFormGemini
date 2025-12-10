namespace VisualEditorOpus.Models;

using DynamicForms.Core.V4.Enums;

/// <summary>
/// UI model for a single condition row in the condition builder.
/// </summary>
public class ConditionRowModel
{
    /// <summary>
    /// Unique identifier for this row.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Field ID to evaluate. Can include module prefix (e.g., "Step1.age").
    /// </summary>
    public string? FieldId { get; set; }

    /// <summary>
    /// Comparison operator to use.
    /// </summary>
    public ConditionOperator Operator { get; set; } = ConditionOperator.Equals;

    /// <summary>
    /// Value to compare against (stored as string, parsed on save).
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Field type for rendering appropriate value input.
    /// </summary>
    public string? FieldType { get; set; }

    /// <summary>
    /// Whether the operator requires a value (IsNull, IsNotNull, etc. don't).
    /// </summary>
    public bool RequiresValue => Operator switch
    {
        ConditionOperator.IsNull => false,
        ConditionOperator.IsNotNull => false,
        ConditionOperator.IsEmpty => false,
        ConditionOperator.IsNotEmpty => false,
        _ => true
    };

    /// <summary>
    /// Whether this row has all required fields filled.
    /// </summary>
    public bool IsComplete => !string.IsNullOrEmpty(FieldId) && (!RequiresValue || !string.IsNullOrEmpty(Value));
}
