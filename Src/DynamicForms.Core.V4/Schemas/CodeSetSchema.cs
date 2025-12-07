namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Immutable schema definition for a reusable CodeSet.
/// CodeSets provide centralized, reusable collections of options for dropdown lists,
/// radio buttons, checkboxes, and other selection-based form fields.
/// </summary>
public record CodeSetSchema
{
    #region Core Identity

    /// <summary>
    /// Unique identifier for the CodeSet
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Unique code/key for the CodeSet (e.g., "PROVINCES_CA", "ORG_TYPES")
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Schema version for this CodeSet (supports evolution)
    /// </summary>
    public float Version { get; init; } = 1.0f;

    /// <summary>
    /// UTC timestamp when CodeSet was created
    /// </summary>
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when CodeSet was last updated (null if never updated)
    /// </summary>
    public DateTime? DateUpdated { get; init; }

    #endregion

    #region Multilingual Metadata

    /// <summary>
    /// English name/title for the CodeSet
    /// </summary>
    public required string NameEn { get; init; }

    /// <summary>
    /// French name/title for the CodeSet
    /// </summary>
    public string? NameFr { get; init; }

    /// <summary>
    /// English description of the CodeSet's purpose
    /// </summary>
    public string? DescriptionEn { get; init; }

    /// <summary>
    /// French description of the CodeSet's purpose
    /// </summary>
    public string? DescriptionFr { get; init; }

    #endregion

    #region Items

    /// <summary>
    /// Collection of items/options in this CodeSet
    /// </summary>
    public CodeSetItem[] Items { get; init; } = Array.Empty<CodeSetItem>();

    #endregion

    #region Metadata

    /// <summary>
    /// Category or group for organizing CodeSets (e.g., "Geography", "Organizations")
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Whether this CodeSet is currently active and available for use
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Whether this CodeSet is system-managed (cannot be deleted)
    /// </summary>
    public bool IsSystemManaged { get; init; }

    /// <summary>
    /// Tags for searching and filtering CodeSets
    /// </summary>
    public string[]? Tags { get; init; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new CodeSet with required fields
    /// </summary>
    /// <param name="id">Unique CodeSet identifier</param>
    /// <param name="code">Unique code/key</param>
    /// <param name="nameEn">English name</param>
    /// <param name="items">Array of CodeSet items</param>
    /// <returns>Configured CodeSetSchema</returns>
    public static CodeSetSchema Create(
        int id,
        string code,
        string nameEn,
        CodeSetItem[] items)
    {
        return new CodeSetSchema
        {
            Id = id,
            Code = code,
            NameEn = nameEn,
            Items = items
        };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Converts CodeSet items to FieldOption array for use in form fields
    /// </summary>
    /// <returns>Array of FieldOption objects</returns>
    public FieldOption[] ToFieldOptions()
    {
        return Items
            .Where(item => item.IsActive)
            .OrderBy(item => item.Order)
            .Select(item => new FieldOption(
                Value: item.Value,
                LabelEn: item.TextEn,
                LabelFr: item.TextFr,
                IsDefault: item.IsDefault,
                Order: item.Order
            ))
            .ToArray();
    }

    /// <summary>
    /// Gets a specific item by its value
    /// </summary>
    /// <param name="value">The item value to find</param>
    /// <returns>The matching item, or null if not found</returns>
    public CodeSetItem? GetItem(string value)
    {
        return Items.FirstOrDefault(item => 
            item.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    #endregion
}

/// <summary>
/// Represents a single item/option within a CodeSet
/// </summary>
public record CodeSetItem
{
    /// <summary>
    /// The underlying value of the item (submitted with form data)
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// English display text for the item
    /// </summary>
    public required string TextEn { get; init; }

    /// <summary>
    /// French display text for the item
    /// </summary>
    public string? TextFr { get; init; }

    /// <summary>
    /// Whether this item is selected by default
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Display order for the item (lower values appear first)
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Whether this item is currently active and available for selection
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Optional description or additional context for the item
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional parent value for hierarchical CodeSets
    /// </summary>
    public string? ParentValue { get; init; }

    /// <summary>
    /// Optional CSS class for custom styling
    /// </summary>
    public string? CssClass { get; init; }

    /// <summary>
    /// Optional metadata for extensibility
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}
