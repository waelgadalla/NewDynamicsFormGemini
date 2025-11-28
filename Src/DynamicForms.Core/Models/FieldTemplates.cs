using DynamicForms.Core.Enums;
using DynamicForms.Core.Entities;

namespace DynamicForms.Core.Models.Enhanced;

/// <summary>
/// Template for creating field hierarchies using  Enhanced architecture
/// </summary>
public class HierarchyTemplate
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FieldHierarchyTemplate RootField { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Unified template for field creation - handles both root fields and individual fields
/// Uses FormField for enhanced functionality
/// </summary>
public class FieldHierarchyTemplate
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FieldId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FieldType { get; set; } = "TextBox";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Label { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? HelpText { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? Order { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsRequired { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsVisible { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ParentChildRelationshipType RelationshipType { get; set; } = ParentChildRelationshipType.GroupContainer;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<FieldHierarchyTemplate> ChildFields { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Convert this template to a FormField
    /// </summary>
    public FormField ToField()
    {
        var field = new FormField
        {
            Id = FieldId ?? GenerateFieldId(),
            FieldType = new FieldType { Type = FieldType ?? "TextBox" },
            Order = Order,
            IsRequired = IsRequired,
            IsVisible = IsVisible,
            RelationshipType = RelationshipType,
            Text = new FormField.TextResource()
        };

        // Set text resources if provided
        if (!string.IsNullOrEmpty(Label))
        {
            field.Text.Description = new TextClass
            {
                EN = Label,
                FR = Label // Default to same value, can be overridden
            };
        }

        if (!string.IsNullOrEmpty(HelpText))
        {
            field.Text.Help = new TextClass
            {
                EN = HelpText,
                FR = HelpText // Default to same value, can be overridden
            };
        }

        return field;
    }

    /// <summary>
    /// Convert this template hierarchy to a FormModule
    /// </summary>
    public FormModule ToModule(int moduleId, int? opportunityId = null)
    {
        var module = new FormModule
        {
            Id = moduleId,
            OpportunityId = opportunityId,
            Version = 1.0f,
            DateGenerated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // Set default title
        module.Text.Title.EN = Label ?? "Generated Module";
        module.Text.Title.FR = Label ?? "Module Généré";

        // Convert root field to  and get all descendants
        var rootField = ToField();
        var allFields = new List<FormField> { rootField };
        
        // Process child fields recursively
        AddChildFieldsRecursively(rootField, ChildFields, allFields);

        module.Fields = allFields.ToArray();
        
        // Build field hierarchy
        module.RebuildFieldHierarchy();

        return module;
    }

    /// <summary>
    /// Get all fields in this template hierarchy (including children)
    /// </summary>
    public IEnumerable<FieldHierarchyTemplate> GetAllFields()
    {
        yield return this;
        
        foreach (var child in ChildFields)
        {
            foreach (var descendant in child.GetAllFields())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Add child template to this template
    /// </summary>
    public void AddChild(FieldHierarchyTemplate child)
    {
        ChildFields.Add(child);
    }

    /// <summary>
    /// Remove child template by field ID
    /// </summary>
    public bool RemoveChild(string fieldId)
    {
        var child = ChildFields.FirstOrDefault(c => c.FieldId == fieldId);
        if (child != null)
        {
            ChildFields.Remove(child);
            return true;
        }
        return false;
    }

    private void AddChildFieldsRecursively(FormField parent, List<FieldHierarchyTemplate> childTemplates, List<FormField> allFields)
    {
        foreach (var childTemplate in childTemplates)
        {
            var childField = childTemplate.ToField();
            childField.ParentId = parent.Id;
            
            allFields.Add(childField);
            
            // Process grandchildren
            if (childTemplate.ChildFields.Any())
            {
                AddChildFieldsRecursively(childField, childTemplate.ChildFields, allFields);
            }
        }
    }

    private static string GenerateFieldId()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }
}

/// <summary>
/// Pre-built field templates for common form patterns
/// </summary>
public static class CommonFieldTemplates
{
    /// <summary>
    /// Contact information section template
    /// </summary>
    public static FieldHierarchyTemplate ContactInformation => new()
    {
        FieldId = "contact_section",
        FieldType = "Section",
        Label = "Contact Information",
        Order = 1,
        ChildFields = new List<FieldHierarchyTemplate>
        {
            new() { FieldId = "first_name", FieldType = "TextBox", Label = "First Name", IsRequired = true, Order = 1 },
            new() { FieldId = "last_name", FieldType = "TextBox", Label = "Last Name", IsRequired = true, Order = 2 },
            new() { FieldId = "email", FieldType = "Email", Label = "Email Address", IsRequired = true, Order = 3 },
            new() { FieldId = "phone", FieldType = "Phone", Label = "Phone Number", Order = 4 }
        }
    };

    /// <summary>
    /// Address section template
    /// </summary>
    public static FieldHierarchyTemplate AddressInformation => new()
    {
        FieldId = "address_section",
        FieldType = "Section", 
        Label = "Address Information",
        Order = 2,
        ChildFields = new List<FieldHierarchyTemplate>
        {
            new() { FieldId = "street_address", FieldType = "TextBox", Label = "Street Address", IsRequired = true, Order = 1 },
            new() { FieldId = "city", FieldType = "TextBox", Label = "City", IsRequired = true, Order = 2 },
            new() { FieldId = "province", FieldType = "DropDownList", Label = "Province", IsRequired = true, Order = 3 },
            new() { FieldId = "postal_code", FieldType = "TextBox", Label = "Postal Code", IsRequired = true, Order = 4 }
        }
    };

    /// <summary>
    /// Organization information template
    /// </summary>
    public static FieldHierarchyTemplate OrganizationInformation => new()
    {
        FieldId = "organization_section",
        FieldType = "Section",
        Label = "Organization Information", 
        Order = 3,
        ChildFields = new List<FieldHierarchyTemplate>
        {
            new() { FieldId = "org_name", FieldType = "TextBox", Label = "Organization Name", IsRequired = true, Order = 1 },
            new() { FieldId = "org_type", FieldType = "DropDownList", Label = "Organization Type", IsRequired = true, Order = 2 },
            new() { FieldId = "org_size", FieldType = "DropDownList", Label = "Organization Size", Order = 3 },
            new() { FieldId = "org_description", FieldType = "TextArea", Label = "Organization Description", Order = 4 }
        }
    };

    /// <summary>
    /// Complete application template with conditional fields
    /// </summary>
    public static FieldHierarchyTemplate CompleteApplicationForm => new()
    {
        FieldId = "application_form",
        FieldType = "Section",
        Label = "Application Form",
        Order = 1,
        ChildFields = new List<FieldHierarchyTemplate>
        {
            ContactInformation,
            AddressInformation,
            OrganizationInformation,
            new FieldHierarchyTemplate
            {
                FieldId = "additional_info_section",
                FieldType = "Section",
                Label = "Additional Information",
                Order = 4,
                ChildFields = new List<FieldHierarchyTemplate>
                {
                    new() { FieldId = "has_previous_funding", FieldType = "RadioButtonList", Label = "Have you received funding before?", IsRequired = true, Order = 1 },
                    new() { FieldId = "previous_funding_details", FieldType = "TextArea", Label = "Previous Funding Details", RelationshipType = ParentChildRelationshipType.ConditionalShow, Order = 2 },
                    new() { FieldId = "project_description", FieldType = "TextArea", Label = "Project Description", IsRequired = true, Order = 3 },
                    new() { FieldId = "supporting_documents", FieldType = "FileUpload", Label = "Supporting Documents", Order = 4 }
                }
            }
        }
    };

    /// <summary>
    /// Get all available templates
    /// </summary>
    public static Dictionary<string, FieldHierarchyTemplate> GetAllTemplates() => new()
    {
        { "contact", ContactInformation },
        { "address", AddressInformation },
        { "organization", OrganizationInformation },
        { "complete_application", CompleteApplicationForm }
    };

    /// <summary>
    /// Get template by name
    /// </summary>
    public static FieldHierarchyTemplate? GetTemplate(string templateName)
    {
        return GetAllTemplates().TryGetValue(templateName.ToLowerInvariant(), out var template) ? template : null;
    }
}