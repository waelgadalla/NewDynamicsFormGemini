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
[JsonDerivedType(typeof(SignatureConfig), typeDiscriminator: "signature")]
[JsonDerivedType(typeof(ImageConfig), typeDiscriminator: "image")]
[JsonDerivedType(typeof(RichTextConfig), typeDiscriminator: "richtext")]
[JsonDerivedType(typeof(TextInputConfig), typeDiscriminator: "textinput")]
[JsonDerivedType(typeof(ToggleConfig), typeDiscriminator: "toggle")]
[JsonDerivedType(typeof(MatrixSingleSelectConfig), typeDiscriminator: "matrixsingle")]
[JsonDerivedType(typeof(MatrixMultiSelectConfig), typeDiscriminator: "matrixmulti")]
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

/// <summary>
/// Configuration for Signature Pad fields.
/// Used for capturing handwritten signatures on forms.
/// </summary>
public record SignatureConfig : FieldTypeConfig
{
    /// <summary>
    /// Width of the signature canvas in pixels.
    /// </summary>
    public int CanvasWidth { get; init; } = 400;

    /// <summary>
    /// Height of the signature canvas in pixels.
    /// </summary>
    public int CanvasHeight { get; init; } = 200;

    /// <summary>
    /// Stroke color for the signature pen (CSS color).
    /// </summary>
    public string StrokeColor { get; init; } = "#000000";

    /// <summary>
    /// Stroke width for the signature pen in pixels.
    /// </summary>
    public int StrokeWidth { get; init; } = 2;

    /// <summary>
    /// Background color of the canvas (CSS color).
    /// </summary>
    public string BackgroundColor { get; init; } = "#ffffff";

    /// <summary>
    /// Output format for the signature image.
    /// </summary>
    public string OutputFormat { get; init; } = "png"; // "png" | "jpeg" | "svg"

    /// <summary>
    /// Whether to show a "Type signature" option as alternative.
    /// </summary>
    public bool AllowTypedSignature { get; init; } = false;

    /// <summary>
    /// Whether to show date/time stamp with the signature.
    /// </summary>
    public bool ShowTimestamp { get; init; } = true;

    /// <summary>
    /// Legal text to display above the signature pad.
    /// </summary>
    public string? LegalTextEn { get; init; }

    /// <summary>
    /// Legal text to display above the signature pad (French).
    /// </summary>
    public string? LegalTextFr { get; init; }
}

/// <summary>
/// Configuration for Image fields.
/// Used for displaying static images or capturing image uploads with preview.
/// </summary>
public record ImageConfig : FieldTypeConfig
{
    /// <summary>
    /// Mode of the image field.
    /// "Display" = Show a static image.
    /// "Upload" = Allow user to upload an image with preview.
    /// </summary>
    public string Mode { get; init; } = "Upload"; // "Display" | "Upload"

    /// <summary>
    /// URL of the image to display (for Display mode).
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Alt text for accessibility (English).
    /// </summary>
    public string? AltTextEn { get; init; }

    /// <summary>
    /// Alt text for accessibility (French).
    /// </summary>
    public string? AltTextFr { get; init; }

    /// <summary>
    /// Maximum width in pixels. Null = no limit.
    /// </summary>
    public int? MaxWidth { get; init; }

    /// <summary>
    /// Maximum height in pixels. Null = no limit.
    /// </summary>
    public int? MaxHeight { get; init; }

    /// <summary>
    /// Allowed image formats for upload.
    /// </summary>
    public string[] AllowedFormats { get; init; } = new[] { "jpg", "jpeg", "png", "gif", "webp" };

    /// <summary>
    /// Maximum file size in bytes for uploads.
    /// </summary>
    public long MaxFileSizeBytes { get; init; } = 5 * 1024 * 1024; // 5MB

    /// <summary>
    /// Whether to show image cropping tool on upload.
    /// </summary>
    public bool AllowCrop { get; init; } = false;

    /// <summary>
    /// Aspect ratio for cropping (e.g., "16:9", "1:1"). Null = free crop.
    /// </summary>
    public string? CropAspectRatio { get; init; }
}

/// <summary>
/// Configuration for Rich Text Editor fields.
/// Used for formatted text input with HTML output.
/// </summary>
public record RichTextConfig : FieldTypeConfig
{
    /// <summary>
    /// Height of the editor in pixels.
    /// </summary>
    public int EditorHeight { get; init; } = 300;

    /// <summary>
    /// Toolbar configuration. Which buttons to show.
    /// </summary>
    public string[] Toolbar { get; init; } = new[]
    {
        "bold", "italic", "underline", "strikethrough",
        "heading", "bulletList", "orderedList",
        "link", "blockquote", "horizontalRule",
        "undo", "redo"
    };

    /// <summary>
    /// Whether to allow image insertion.
    /// </summary>
    public bool AllowImages { get; init; } = false;

    /// <summary>
    /// Whether to allow table insertion.
    /// </summary>
    public bool AllowTables { get; init; } = false;

    /// <summary>
    /// Whether to allow raw HTML editing.
    /// </summary>
    public bool AllowHtmlSource { get; init; } = false;

    /// <summary>
    /// Maximum character count. Null = unlimited.
    /// </summary>
    public int? MaxCharacters { get; init; }

    /// <summary>
    /// Placeholder text (English).
    /// </summary>
    public string? PlaceholderEn { get; init; }

    /// <summary>
    /// Placeholder text (French).
    /// </summary>
    public string? PlaceholderFr { get; init; }
}

/// <summary>
/// Configuration for specialized text input fields (Email, Phone, URL, etc.).
/// Extends basic text input with format-specific options.
/// </summary>
public record TextInputConfig : FieldTypeConfig
{
    /// <summary>
    /// The input subtype for specialized validation and formatting.
    /// </summary>
    public string InputType { get; init; } = "text"; // "text" | "email" | "phone" | "url"

    /// <summary>
    /// Input mask pattern (e.g., "(###) ###-####" for phone).
    /// </summary>
    public string? InputMask { get; init; }

    /// <summary>
    /// Default country code for phone numbers (e.g., "+1", "+33").
    /// </summary>
    public string? DefaultCountryCode { get; init; }

    /// <summary>
    /// Whether to show country code selector for phone fields.
    /// </summary>
    public bool ShowCountrySelector { get; init; } = false;

    /// <summary>
    /// Whether to validate format on input (real-time).
    /// </summary>
    public bool ValidateOnInput { get; init; } = false;

    /// <summary>
    /// Custom regex pattern for validation (overrides default).
    /// </summary>
    public string? CustomPattern { get; init; }

    /// <summary>
    /// Whether to show a "mailto:" link for email fields in read-only mode.
    /// </summary>
    public bool ShowAsLink { get; init; } = true;

    /// <summary>
    /// Autocomplete hint for browsers (e.g., "email", "tel", "url").
    /// </summary>
    public string? AutocompleteHint { get; init; }
}

/// <summary>
/// Configuration for Toggle/Switch fields.
/// A boolean input displayed as a toggle switch instead of checkbox.
/// </summary>
public record ToggleConfig : FieldTypeConfig
{
    /// <summary>
    /// Label to show when toggle is ON (English).
    /// </summary>
    public string OnLabelEn { get; init; } = "Yes";

    /// <summary>
    /// Label to show when toggle is ON (French).
    /// </summary>
    public string OnLabelFr { get; init; } = "Oui";

    /// <summary>
    /// Label to show when toggle is OFF (English).
    /// </summary>
    public string OffLabelEn { get; init; } = "No";

    /// <summary>
    /// Label to show when toggle is OFF (French).
    /// </summary>
    public string OffLabelFr { get; init; } = "Non";

    /// <summary>
    /// Color when toggle is ON (CSS color).
    /// </summary>
    public string OnColor { get; init; } = "#22c55e"; // green

    /// <summary>
    /// Color when toggle is OFF (CSS color).
    /// </summary>
    public string OffColor { get; init; } = "#e5e7eb"; // gray

    /// <summary>
    /// Size of the toggle switch.
    /// </summary>
    public string Size { get; init; } = "medium"; // "small" | "medium" | "large"

    /// <summary>
    /// Whether to show the on/off labels next to the toggle.
    /// </summary>
    public bool ShowLabels { get; init; } = true;

    /// <summary>
    /// Default value when field is first rendered.
    /// </summary>
    public bool DefaultValue { get; init; } = false;
}

/// <summary>
/// Defines a row in a Matrix question.
/// Rows typically represent statements or items to be rated/answered.
/// </summary>
public record MatrixRowDefinition
{
    /// <summary>
    /// Unique identifier for this row (saved in form data).
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Display text for this row (English).
    /// </summary>
    public required string TextEn { get; init; }

    /// <summary>
    /// Display text for this row (French).
    /// </summary>
    public string? TextFr { get; init; }

    /// <summary>
    /// Sort order for display.
    /// </summary>
    public int Order { get; init; } = 0;

    /// <summary>
    /// Whether this row is visible.
    /// </summary>
    public bool IsVisible { get; init; } = true;
}

/// <summary>
/// Defines a column in a Matrix question.
/// For Single-Select: columns are the rating scale points.
/// For Multi-Select: columns can have different input types per column.
/// </summary>
public record MatrixColumnDefinition
{
    /// <summary>
    /// Unique identifier for this column (saved in form data).
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Display text for this column header (English).
    /// </summary>
    public required string TextEn { get; init; }

    /// <summary>
    /// Display text for this column header (French).
    /// </summary>
    public string? TextFr { get; init; }

    /// <summary>
    /// Sort order for display.
    /// </summary>
    public int Order { get; init; } = 0;

    /// <summary>
    /// Cell type for Multi-Select Matrix columns.
    /// Ignored for Single-Select Matrix (always radio).
    /// </summary>
    public string? CellType { get; init; } // "radio" | "checkbox" | "dropdown" | "text" | "rating"

    /// <summary>
    /// Options for dropdown/checkbox/radio cell types.
    /// </summary>
    public FieldOption[]? Choices { get; init; }

    /// <summary>
    /// Maximum value for rating cell type.
    /// </summary>
    public int? RatingMax { get; init; }

    /// <summary>
    /// Minimum width for this column (CSS value, e.g., "100px", "20%").
    /// </summary>
    public string? MinWidth { get; init; }
}

/// <summary>
/// Configuration for Single-Select Matrix fields (Likert Scale).
/// Each row allows one selection from the column options (radio button per row).
/// Example: Rating satisfaction on a 1-5 scale for multiple statements.
/// </summary>
public record MatrixSingleSelectConfig : FieldTypeConfig
{
    /// <summary>
    /// Row definitions (statements/items to rate).
    /// </summary>
    public MatrixRowDefinition[] Rows { get; init; } = Array.Empty<MatrixRowDefinition>();

    /// <summary>
    /// Column definitions (rating scale points).
    /// </summary>
    public MatrixColumnDefinition[] Columns { get; init; } = Array.Empty<MatrixColumnDefinition>();

    /// <summary>
    /// Whether all rows must be answered before form submission.
    /// </summary>
    public bool IsAllRowRequired { get; init; } = false;

    /// <summary>
    /// Whether to alternate row background colors for readability.
    /// </summary>
    public bool AlternateRowColors { get; init; } = true;

    /// <summary>
    /// Whether to show row numbers.
    /// </summary>
    public bool ShowRowNumbers { get; init; } = false;

    /// <summary>
    /// Position of column headers.
    /// </summary>
    public string HeaderPosition { get; init; } = "top"; // "top" | "left" | "both"

    /// <summary>
    /// Whether to transpose the table (swap rows and columns).
    /// </summary>
    public bool TransposeTable { get; init; } = false;

    /// <summary>
    /// How cells should be aligned.
    /// </summary>
    public string CellAlignment { get; init; } = "center"; // "left" | "center" | "right"

    /// <summary>
    /// Layout mode for mobile devices.
    /// </summary>
    public string MobileLayout { get; init; } = "stacked"; // "stacked" | "scroll"

    /// <summary>
    /// Whether to show column totals/summary.
    /// </summary>
    public bool ShowColumnTotals { get; init; } = false;
}

/// <summary>
/// Configuration for Multi-Select Matrix fields.
/// Each cell can have a different input type (dropdown, checkbox, text, rating).
/// Example: Rate multiple products across multiple criteria with different input types.
/// </summary>
public record MatrixMultiSelectConfig : FieldTypeConfig
{
    /// <summary>
    /// Row definitions (items being evaluated).
    /// </summary>
    public MatrixRowDefinition[] Rows { get; init; } = Array.Empty<MatrixRowDefinition>();

    /// <summary>
    /// Column definitions with cell types and options.
    /// Each column can have a different input type.
    /// </summary>
    public MatrixColumnDefinition[] Columns { get; init; } = Array.Empty<MatrixColumnDefinition>();

    /// <summary>
    /// Default cell type for columns that don't specify one.
    /// </summary>
    public string DefaultCellType { get; init; } = "dropdown"; // "radio" | "checkbox" | "dropdown" | "text" | "rating"

    /// <summary>
    /// Whether all rows must be completed before form submission.
    /// </summary>
    public bool IsAllRowRequired { get; init; } = false;

    /// <summary>
    /// Whether to alternate row background colors for readability.
    /// </summary>
    public bool AlternateRowColors { get; init; } = true;

    /// <summary>
    /// Minimum width for all columns (CSS value).
    /// Can be overridden per column.
    /// </summary>
    public string? ColumnMinWidth { get; init; }

    /// <summary>
    /// Whether users can resize columns.
    /// </summary>
    public bool AllowColumnResize { get; init; } = false;

    /// <summary>
    /// Whether to transpose the table (swap rows and columns).
    /// </summary>
    public bool TransposeTable { get; init; } = false;

    /// <summary>
    /// Layout mode for mobile devices.
    /// </summary>
    public string MobileLayout { get; init; } = "stacked"; // "stacked" | "scroll"

    /// <summary>
    /// Whether to show a summary row at the bottom.
    /// </summary>
    public bool ShowSummaryRow { get; init; } = false;

    /// <summary>
    /// Whether to allow adding dynamic rows at runtime.
    /// </summary>
    public bool AllowDynamicRows { get; init; } = false;

    /// <summary>
    /// Placeholder text for empty cells (English).
    /// </summary>
    public string? EmptyCellTextEn { get; init; }

    /// <summary>
    /// Placeholder text for empty cells (French).
    /// </summary>
    public string? EmptyCellTextFr { get; init; }
}