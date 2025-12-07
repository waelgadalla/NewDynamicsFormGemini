using System.Text.Json.Serialization;

namespace DynamicForms.Core.V4.Enums;

/// <summary>
/// Logical operators for combining conditions in complex logic expressions.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogicalOperator
{
    /// <summary>
    /// All sub-conditions must be true (logical AND).
    /// Example: Age < 18 AND Province = ON
    /// </summary>
    And,

    /// <summary>
    /// At least one sub-condition must be true (logical OR).
    /// Example: Province = ON OR Province = QC
    /// </summary>
    Or,

    /// <summary>
    /// Negates the sub-condition (logical NOT).
    /// Example: NOT (Age < 18)
    /// </summary>
    Not
}
