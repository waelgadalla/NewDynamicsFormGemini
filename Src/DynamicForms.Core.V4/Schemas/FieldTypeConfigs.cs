using System.Text.Json.Serialization;

namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Base class for type-specific configuration.
/// Uses polymorphic serialization to handle derived types.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(AutoCompleteConfig), typeDiscriminator: "autocomplete")]
[JsonDerivedType(typeof(DataGridConfig), typeDiscriminator: "datagrid")]
[JsonDerivedType(typeof(FileUploadConfig), typeDiscriminator: "fileupload")]
[JsonDerivedType(typeof(DateConfig), typeDiscriminator: "date")]
public abstract record FieldTypeConfig { }

/// <summary>
/// Configuration for an Autocomplete / Typeahead field.
/// Used for selecting from large datasets via API.
/// </summary>
public record AutoCompleteConfig : FieldTypeConfig
{
    /// <summary>
    /// The API endpoint to query for results.
    /// Example: "/api/v1/species/search"
    /// </summary>
    public required string DataSourceUrl { get; init; }

    /// <summary>
    /// The query parameter name to send to the API.
    /// Default: "q" -> /api...?q=searchTerm
    /// </summary>
    public string QueryParameter { get; init; } = "q";

    /// <summary>
    /// Minimum characters required before triggering a search.
    /// </summary>
    public int MinCharacters { get; init; } = 3;

    /// <summary>
    /// The property name in the API response object to use as the saved value.
    /// Example: "Id" or "Code"
    /// </summary>
    public required string ValueField { get; init; }

    /// <summary>
    /// The property name in the API response object to display in the input.
    /// Example: "Name"
    /// </summary>
    public required string DisplayField { get; init; }

    /// <summary>
    /// Optional Handlebars-style template for the dropdown list items.
    /// Example: "{{Name}} <span class='text-muted'>({{ScientificName}})</span>"
    /// </summary>
    public string? ItemTemplate { get; init; }
}

/// <summary>
/// Configuration for a DataGrid / Repeater field.
/// Allows users to add multiple rows of structured data (e.g. Line Items).
/// </summary>
public record DataGridConfig : FieldTypeConfig
{
    /// <summary>
    /// Allows adding new rows.
    /// </summary>
    public bool AllowAdd { get; init; } = true;

    /// <summary>
    /// Allows editing existing rows.
    /// </summary>
    public bool AllowEdit { get; init; } = true;

    /// <summary>
    /// Allows deleting rows.
    /// </summary>
    public bool AllowDelete { get; init; } = true;

    /// <summary>
    /// Maximum number of rows allowed. Null = unlimited.
    /// </summary>
    public int? MaxRows { get; init; }

    /// <summary>
    /// How the editor should appear.
    /// "Modal" = Popup dialog.
    /// "Inline" = Edit directly in the table row.
    /// </summary>
    public string EditorMode { get; init; } = "Modal"; // "Modal" | "Inline"

    /// <summary>
    /// The definitions of the fields that make up a single row.
    /// These form the columns of the grid and the inputs of the editor.
    /// </summary>
    public FormFieldSchema[] Columns { get; init; } = Array.Empty<FormFieldSchema>();
}

/// <summary>
/// Configuration for File Upload fields.
/// </summary>
public record FileUploadConfig : FieldTypeConfig
{
    public string[] AllowedExtensions { get; init; } = Array.Empty<string>();
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024; // 10MB
    public bool AllowMultiple { get; init; }
    public bool ScanRequired { get; init; } = true;
}

/// <summary>
/// Configuration for Date/Time fields.
/// </summary>
public record DateConfig : FieldTypeConfig
{
    public bool AllowFuture { get; init; } = true;
    public bool AllowPast { get; init; } = true;
    public string? MinDate { get; init; } // ISO 8601 or "Now", "Now+30d"
    public string? MaxDate { get; init; }
}