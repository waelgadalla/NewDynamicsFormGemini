using DynamicForms.Core.V4.Schemas;

namespace VisualEditorOpus.Models;

/// <summary>
/// Defines metadata for each field type in the palette
/// </summary>
public record FieldTypeDefinition(
    string FieldType,
    string DisplayName,
    string Icon,
    string Category,
    bool RequiresTypeConfig = false,
    Type? TypeConfigType = null
)
{
    /// <summary>
    /// All available field types organized by category
    /// </summary>
    public static readonly FieldTypeDefinition[] AllTypes = new[]
    {
        // Basic
        new FieldTypeDefinition("TextBox", "Text Input", "bi-input-cursor-text", "Basic"),
        new FieldTypeDefinition("TextArea", "Text Area", "bi-textarea-t", "Basic"),
        new FieldTypeDefinition("Number", "Number", "bi-123", "Basic"),
        new FieldTypeDefinition("Currency", "Currency", "bi-currency-dollar", "Basic"),
        new FieldTypeDefinition("Email", "Email", "bi-envelope", "Basic",
            true, typeof(TextInputConfig)),
        new FieldTypeDefinition("Phone", "Phone", "bi-telephone", "Basic",
            true, typeof(TextInputConfig)),

        // Choice
        new FieldTypeDefinition("DropDown", "Dropdown", "bi-menu-button-wide", "Choice"),
        new FieldTypeDefinition("RadioGroup", "Radio Group", "bi-ui-radios", "Choice"),
        new FieldTypeDefinition("CheckboxList", "Checkboxes", "bi-ui-checks", "Choice"),
        new FieldTypeDefinition("Checkbox", "Single Checkbox", "bi-check-square", "Choice"),
        new FieldTypeDefinition("Toggle", "Toggle Switch", "bi-toggle-on", "Choice",
            true, typeof(ToggleConfig)),

        // Date & Time
        new FieldTypeDefinition("DatePicker", "Date Picker", "bi-calendar-event", "Date & Time",
            true, typeof(DateConfig)),
        new FieldTypeDefinition("TimePicker", "Time Picker", "bi-clock", "Date & Time"),
        new FieldTypeDefinition("DateTimePicker", "Date & Time", "bi-calendar-week", "Date & Time",
            true, typeof(DateConfig)),

        // Advanced
        new FieldTypeDefinition("FileUpload", "File Upload", "bi-cloud-upload", "Advanced",
            true, typeof(FileUploadConfig)),
        new FieldTypeDefinition("DataGrid", "Data Grid", "bi-table", "Advanced",
            true, typeof(DataGridConfig)),
        new FieldTypeDefinition("Repeater", "Repeater", "bi-arrow-repeat", "Advanced",
            true, typeof(DataGridConfig)),
        new FieldTypeDefinition("AutoComplete", "AutoComplete", "bi-search", "Advanced",
            true, typeof(AutoCompleteConfig)),
        new FieldTypeDefinition("RichText", "Rich Text Editor", "bi-file-richtext", "Advanced",
            true, typeof(RichTextConfig)),
        new FieldTypeDefinition("Signature", "Signature Pad", "bi-pen", "Advanced",
            true, typeof(SignatureConfig)),
        new FieldTypeDefinition("Image", "Image", "bi-image", "Advanced",
            true, typeof(ImageConfig)),
        new FieldTypeDefinition("MatrixSingle", "Matrix (Single)", "bi-grid-3x3", "Advanced",
            true, typeof(MatrixSingleSelectConfig)),
        new FieldTypeDefinition("MatrixMulti", "Matrix (Multi)", "bi-grid-3x3-gap", "Advanced",
            true, typeof(MatrixMultiSelectConfig)),

        // Layout
        new FieldTypeDefinition("Section", "Section", "bi-layout-three-columns", "Layout"),
        new FieldTypeDefinition("Panel", "Panel", "bi-window", "Layout"),
        new FieldTypeDefinition("Divider", "Divider", "bi-dash-lg", "Layout"),
        new FieldTypeDefinition("Label", "Label/HTML", "bi-fonts", "Layout"),
    };

    /// <summary>
    /// Get a field type definition by its type name
    /// </summary>
    public static FieldTypeDefinition? GetByType(string fieldType)
        => AllTypes.FirstOrDefault(t => t.FieldType.Equals(fieldType, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Get all field types grouped by category
    /// </summary>
    public static IEnumerable<IGrouping<string, FieldTypeDefinition>> GetByCategory()
        => AllTypes.GroupBy(t => t.Category);

    /// <summary>
    /// Get the icon class for a field type
    /// </summary>
    public static string GetIcon(string fieldType)
        => GetByType(fieldType)?.Icon ?? "bi-question-circle";
}
