using System.Text.Json.Serialization;

namespace DynamicForms.Core.V4.Enums;

/// <summary>
/// Supported operators for conditional logic evaluation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConditionOperator
{
    // Equality
    Equals,
    NotEquals,

    // Numeric / Date Comparison
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,

    // String / Collection Operations
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    In,
    NotIn,

    // Null / Empty Checks
    IsNull,
    IsNotNull,
    IsEmpty,
    IsNotEmpty
}
