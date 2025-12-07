namespace DynamicForms.Core.V4.Enums;

/// <summary>
/// Defines the type of relationship between parent and child fields
/// </summary>
public enum RelationshipType
{
    /// <summary>
    /// Structural container relationship (Section, Group, Panel).
    /// Used when a parent field contains child fields for organizational purposes.
    /// </summary>
    Container = 0,

    /// <summary>
    /// Conditional relationship for show/hide logic.
    /// Used when a parent field's value determines whether child fields are displayed.
    /// </summary>
    Conditional = 1,

    /// <summary>
    /// Cascading relationship for dependent dropdowns.
    /// Used when a parent field's selection filters options available in child fields.
    /// </summary>
    Cascade = 2,

    /// <summary>
    /// Validation dependency relationship.
    /// Used when a parent field's value affects validation rules applied to child fields.
    /// </summary>
    Validation = 3
}
