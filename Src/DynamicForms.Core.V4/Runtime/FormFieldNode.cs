using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Core.V4.Runtime;

/// <summary>
/// Runtime representation of a form field with hierarchy navigation.
/// This class is NOT serialized - it's built from FormFieldSchema for runtime use.
/// Contains parent-child relationships for efficient hierarchy traversal.
/// </summary>
public class FormFieldNode
{
    /// <summary>
    /// The immutable schema definition for this field
    /// </summary>
    public required FormFieldSchema Schema { get; init; }

    /// <summary>
    /// Reference to the parent node in the hierarchy (null for root fields)
    /// </summary>
    public FormFieldNode? Parent { get; set; }

    /// <summary>
    /// Collection of child nodes in the hierarchy
    /// </summary>
    public List<FormFieldNode> Children { get; } = new();

    /// <summary>
    /// Resolved options from CodeSet (if field uses CodeSetId).
    /// Populated during hierarchy building when CodeSet is resolved.
    /// </summary>
    public FieldOption[]? ResolvedOptions { get; set; }

    /// <summary>
    /// Gets the effective options for this field (resolved CodeSet or inline options)
    /// </summary>
    /// <returns>Array of FieldOptions, or null if field doesn't support options</returns>
    public FieldOption[]? GetEffectiveOptions()
    {
        // Priority: ResolvedOptions from CodeSet, then Schema.Options
        return ResolvedOptions ?? Schema.Options;
    }

    /// <summary>
    /// Computed depth level in the hierarchy (0 = root, 1 = first level child, etc.)
    /// </summary>
    public int Level => Parent?.Level + 1 ?? 0;

    /// <summary>
    /// Computed full path from root to this node (e.g., "section1.group1.field1")
    /// </summary>
    public string Path => Parent != null ? $"{Parent.Path}.{Schema.Id}" : Schema.Id;

    /// <summary>
    /// Recursively gets all descendant nodes (children, grandchildren, etc.)
    /// </summary>
    /// <returns>Enumerable of all descendants in depth-first order</returns>
    public IEnumerable<FormFieldNode> GetAllDescendants()
    {
        foreach (var child in Children)
        {
            yield return child;
            foreach (var descendant in child.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Gets all ancestor nodes (parent, grandparent, etc.) from immediate parent to root
    /// </summary>
    /// <returns>Enumerable of ancestors from closest to root</returns>
    public IEnumerable<FormFieldNode> GetAllAncestors()
    {
        var current = Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Returns a string representation of this node for debugging
    /// </summary>
    public override string ToString() => $"{Schema.FieldType} [{Schema.Id}] at Level {Level}";
}
