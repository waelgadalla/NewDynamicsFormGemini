using System.Text.Json.Serialization;

namespace DynamicForms.Core.Entities;

/// <summary>
/// Defines the type of parent-child relationship between fields
/// </summary>
public enum ParentChildRelationshipType
{
    /// <summary>
    /// Simple grouping/container relationship (sections, groups)
    /// </summary>
    GroupContainer,
    
    /// <summary>
    /// Conditional relationship where child appears when parent has specific value
    /// </summary>
    ConditionalShow,
    
    /// <summary>
    /// Conditional relationship where child is hidden when parent has specific value
    /// </summary>
    ConditionalHide,
    
    /// <summary>
    /// Cascade relationship where child options depend on parent selection
    /// </summary>
    Cascade,
    
    /// <summary>
    /// Validation relationship where parent validates child
    /// </summary>
    Validation,
    
    /// <summary>
    /// Repeater relationship where child can repeat based on parent
    /// </summary>
    Repeater,
    
    /// <summary>
    /// No specific relationship type
    /// </summary>
    None
}