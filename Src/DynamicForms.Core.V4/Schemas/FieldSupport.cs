namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Represents an option in a dropdown, radio button group, or checkbox list
/// </summary>
public record FieldOption(
    string Value,
    string LabelEn,
    string? LabelFr = null,
    bool IsDefault = false,
    int Order = 0
);
