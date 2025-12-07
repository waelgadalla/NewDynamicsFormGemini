using DynamicForms.Core.V4.Runtime;
using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Core.V4.Services;

/// <summary>
/// Service for building and managing form field hierarchies.
/// Transforms flat field arrays into navigable tree structures.
/// </summary>
public interface IFormHierarchyService
{
    /// <summary>
    /// Builds the runtime hierarchy from a module schema.
    /// Creates parent-child relationships and calculates metrics.
    /// </summary>
    /// <param name="schema">The module schema to build from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Runtime module with fully built hierarchy</returns>
    Task<FormModuleRuntime> BuildHierarchyAsync(
        FormModuleSchema schema,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the hierarchy structure for errors and warnings.
    /// Checks for circular references, missing parents, duplicate IDs, etc.
    /// </summary>
    /// <param name="schema">The module schema to validate</param>
    /// <returns>Validation result with errors and warnings</returns>
    HierarchyValidationResult ValidateHierarchy(FormModuleSchema schema);

    /// <summary>
    /// Attempts to fix common hierarchy issues automatically.
    /// Clears invalid parent references and resolves conflicts.
    /// </summary>
    /// <param name="schema">The module schema to fix</param>
    /// <returns>New schema with issues resolved</returns>
    FormModuleSchema FixHierarchyIssues(FormModuleSchema schema);

    /// <summary>
    /// Calculates metrics about the hierarchy structure.
    /// Useful for monitoring complexity and performance.
    /// </summary>
    /// <param name="schema">The module schema to analyze</param>
    /// <returns>Calculated hierarchy metrics</returns>
    HierarchyMetrics CalculateMetrics(FormModuleSchema schema);
}
