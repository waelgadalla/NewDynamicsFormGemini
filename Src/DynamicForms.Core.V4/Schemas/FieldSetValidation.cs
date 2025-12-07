namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Configuration for cross-field validation rules (e.g. one of these fields must be filled).
/// </summary>
public record FieldSetValidation
{
    /// <summary>
    /// The type of validation to apply to the set.
    /// </summary>
    public required string Type { get; init; } // "AtLeastOne", "AllOrNone", "MutuallyExclusive"

    /// <summary>
    /// The IDs of the fields involved in this validation.
    /// </summary>
    public required string[] FieldIds { get; init; }

    /// <summary>
    /// Error message (English).
    /// </summary>
    public string? ErrorMessageEn { get; init; }

    /// <summary>
    /// Error message (French).
    /// </summary>
    public string? ErrorMessageFr { get; init; }
}
