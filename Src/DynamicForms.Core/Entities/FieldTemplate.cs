namespace DynamicForms.Core.Entities;

/// <summary>
/// Pre-built field template for quick insertion
/// </summary>
public class FieldTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-file-text";
    public FormField[] Fields { get; set; } = Array.Empty<FormField>();
}

/// <summary>
/// Template categories
/// </summary>
public static class TemplateCategory
{
    public const string ContactInformation = "Contact Information";
    public const string Address = "Address";
    public const string Employment = "Employment";
    public const string PersonalInformation = "Personal Information";
    public const string Financial = "Financial";
    public const string Emergency = "Emergency";
}
