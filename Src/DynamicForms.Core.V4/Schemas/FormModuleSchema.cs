using System.Text.Json;

namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Immutable schema definition for a form module.
/// A module represents a complete form with fields, validation rules, and metadata.
/// Serializable to/from JSON for storage and transmission.
/// </summary>
public record FormModuleSchema
{
    #region Core Identity

    /// <summary>
    /// Unique identifier for the module
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Associated opportunity ID (null if not opportunity-specific)
    /// </summary>
    public int? OpportunityId { get; init; }

    /// <summary>
    /// Schema version for this module (supports evolution)
    /// </summary>
    public float Version { get; init; } = 1.0f;

    /// <summary>
    /// UTC timestamp when module was created
    /// </summary>
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when module was last updated (null if never updated)
    /// </summary>
    public DateTime? DateUpdated { get; init; }

    /// <summary>
    /// User ID or name of the creator
    /// </summary>
    public string? CreatedBy { get; init; }

    #endregion

    #region Multilingual Metadata

    /// <summary>
    /// English title for the module
    /// </summary>
    public required string TitleEn { get; init; }

    /// <summary>
    /// French title for the module
    /// </summary>
    public string? TitleFr { get; init; }

    /// <summary>
    /// English description of the module's purpose
    /// </summary>
    public string? DescriptionEn { get; init; }

    /// <summary>
    /// French description of the module's purpose
    /// </summary>
    public string? DescriptionFr { get; init; }

    /// <summary>
    /// English instructions for completing the module
    /// </summary>
    public string? InstructionsEn { get; init; }

    /// <summary>
    /// French instructions for completing the module
    /// </summary>
    public string? InstructionsFr { get; init; }

    #endregion

    #region Fields

    /// <summary>
    /// Array of all fields in this module (flat list, hierarchy built at runtime)
    /// </summary>
    public FormFieldSchema[] Fields { get; init; } = Array.Empty<FormFieldSchema>();

    #endregion

    #region Validation Rules

    /// <summary>
    /// Cross-field validation rules (e.g., "At least one of these fields must be filled").
    /// </summary>
    public FieldSetValidation[]? CrossFieldValidations { get; init; }

    /// <summary>
    /// Custom validation rules specific to this module
    /// </summary>
    public ModuleValidationRule[]? CustomValidationRules { get; init; }

    #endregion

    #region Database Configuration

    /// <summary>
    /// Database table name for storing module data (null = use default naming)
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Database schema name (default: "dbo")
    /// </summary>
    public string? SchemaName { get; init; } = "dbo";

    #endregion

    #region Extensibility

    /// <summary>
    /// Extended properties for custom data not covered by the schema.
    /// Stored as raw JSON for maximum flexibility.
    /// </summary>
    public JsonElement? ExtendedProperties { get; init; }

    #endregion

    #region Factory Method

    /// <summary>
    /// Creates a new form module with required fields
    /// </summary>
    /// <param name="id">Unique module identifier</param>
    /// <param name="titleEn">English title</param>
    /// <param name="titleFr">French title (optional)</param>
    /// <param name="opportunityId">Associated opportunity ID (optional)</param>
    /// <returns>Configured FormModuleSchema</returns>
    public static FormModuleSchema Create(
        int id,
        string titleEn,
        string? titleFr = null,
        int? opportunityId = null)
    {
        return new FormModuleSchema
        {
            Id = id,
            TitleEn = titleEn,
            TitleFr = titleFr,
            OpportunityId = opportunityId
        };
    }

    #endregion
}

/// <summary>
/// Represents a custom validation rule for a module
/// </summary>
/// <param name="RuleId">Unique identifier for the rule</param>
/// <param name="RuleType">Type of validation rule (e.g., "crossFieldValidation", "businessRule")</param>
/// <param name="Configuration">JSON configuration for the rule (rule-specific)</param>
public record ModuleValidationRule(
    string RuleId,
    string RuleType,
    JsonElement Configuration
);