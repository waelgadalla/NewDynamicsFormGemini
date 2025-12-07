using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Core.V4.Runtime;

/// <summary>
/// Runtime representation of a form module with built hierarchy.
/// This is the object returned by IFormHierarchyService after building the hierarchy.
/// Contains both the original schema and the navigable tree structure.
/// </summary>
public class FormModuleRuntime
{
    /// <summary>
    /// The original immutable schema for this module
    /// </summary>
    public required FormModuleSchema Schema { get; init; }

    /// <summary>
    /// Dictionary of all field nodes indexed by field ID for O(1) lookup
    /// </summary>
    public Dictionary<string, FormFieldNode> FieldNodes { get; init; } = new();

    /// <summary>
    /// List of root-level fields (fields with no parent)
    /// </summary>
    public List<FormFieldNode> RootFields { get; init; } = new();

    /// <summary>
    /// Calculated metrics about the hierarchy structure
    /// </summary>
    public HierarchyMetrics Metrics { get; init; } = new(0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Gets a field node by its ID
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <returns>The field node, or null if not found</returns>
    public FormFieldNode? GetField(string fieldId)
    {
        return FieldNodes.TryGetValue(fieldId, out var node) ? node : null;
    }

    /// <summary>
    /// Gets all fields in depth-first traversal order
    /// </summary>
    /// <returns>Enumerable of all fields in hierarchy order</returns>
    public IEnumerable<FormFieldNode> GetFieldsInOrder()
    {
        foreach (var rootField in RootFields.OrderBy(f => f.Schema.Order))
        {
            yield return rootField;
            foreach (var descendant in rootField.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Returns a string representation for debugging
    /// </summary>
    public override string ToString() =>
        $"Module '{Schema.TitleEn}' - {Metrics.TotalFields} fields, {Metrics.RootFields} roots, max depth {Metrics.MaxDepth}";
}
