namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Configuration for field-level validation rules.
/// </summary>
public record FieldValidationConfig
{
    /// <summary>
    /// Whether a value is required.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Custom error message for required validation (English).
    /// </summary>
    public string? RequiredMessageEn { get; init; }

    /// <summary>
    /// Custom error message for required validation (French).
    /// </summary>
    public string? RequiredMessageFr { get; init; }

    /// <summary>
    /// Minimum string length.
    /// </summary>
    public int? MinLength { get; init; }

    /// <summary>
    /// Maximum string length.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Regular expression pattern.
    /// </summary>
    public string? Pattern { get; init; }

    /// <summary>
    /// Error message if pattern validation fails (English).
    /// </summary>
    public string? PatternMessageEn { get; init; }

    /// <summary>
    /// Error message if pattern validation fails (French).
    /// </summary>
    public string? PatternMessageFr { get; init; }

    /// <summary>
    /// Minimum numeric value.
    /// </summary>
    public double? MinValue { get; init; }

    /// <summary>
    /// Maximum numeric value.
    /// </summary>
    public double? MaxValue { get; init; }

    /// <summary>
    /// List of custom rule IDs to apply.
    /// </summary>
    public string[]? CustomRuleIds { get; init; }
}
