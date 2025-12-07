namespace DynamicForms.Core.V4.Runtime;

/// <summary>
/// Result of validating a form module's hierarchy structure
/// </summary>
/// <param name="Errors">Critical errors that prevent hierarchy building</param>
/// <param name="Warnings">Non-critical issues that should be addressed</param>
public record HierarchyValidationResult(
    List<string> Errors,
    List<string> Warnings
)
{
    /// <summary>
    /// Whether the hierarchy is valid (no errors)
    /// </summary>
    public bool IsValid => !Errors.Any();

    /// <summary>
    /// Creates a successful validation result with no errors or warnings
    /// </summary>
    public static HierarchyValidationResult Success() => new(new(), new());

    /// <summary>
    /// Creates a validation result with errors
    /// </summary>
    public static HierarchyValidationResult WithErrors(params string[] errors)
        => new(new List<string>(errors), new());

    /// <summary>
    /// Creates a validation result with warnings
    /// </summary>
    public static HierarchyValidationResult WithWarnings(params string[] warnings)
        => new(new(), new List<string>(warnings));
}
