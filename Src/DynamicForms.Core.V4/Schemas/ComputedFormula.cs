namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Defines a computed value formula for a field.
/// </summary>
public record ComputedFormula
{
    /// <summary>
    /// The formula expression (e.g. "Quantity * Price").
    /// Syntax depends on the evaluation engine (e.g. NCalc, simple parser).
    /// </summary>
    public required string Expression { get; init; }

    /// <summary>
    /// The IDs of fields referenced in the formula.
    /// </summary>
    public string[]? DependentFieldIds { get; init; }
}
