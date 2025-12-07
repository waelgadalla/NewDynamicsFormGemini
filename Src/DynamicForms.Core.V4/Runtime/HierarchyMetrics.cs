namespace DynamicForms.Core.V4.Runtime;

/// <summary>
/// Statistical metrics about a form module's hierarchy structure.
/// Used for monitoring, optimization, and complexity analysis.
/// </summary>
/// <param name="TotalFields">Total number of fields in the module</param>
/// <param name="RootFields">Number of root-level fields (fields without parents)</param>
/// <param name="MaxDepth">Maximum depth of the hierarchy tree (0 = only root fields)</param>
/// <param name="AverageDepth">Average depth of all fields in the hierarchy</param>
/// <param name="ConditionalFields">Number of fields with conditional logic</param>
/// <param name="ComplexityScore">Calculated complexity score (higher = more complex)</param>
public record HierarchyMetrics(
    int TotalFields,
    int RootFields,
    int MaxDepth,
    double AverageDepth,
    int ConditionalFields,
    double ComplexityScore
);
