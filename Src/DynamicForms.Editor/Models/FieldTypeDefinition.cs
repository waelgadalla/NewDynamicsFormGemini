using DynamicForms.Core.V4.Schemas; // For TypeConfig types

namespace DynamicForms.Editor.Models;

public record FieldTypeDefinition(
    string FieldType,
    string DisplayName,
    string Icon,
    string Category,
    bool RequiresTypeConfig = false,
    Type? TypeConfigType = null
)
{
    public static readonly FieldTypeDefinition[] AllTypes = new[]
    {
        // Basic
        new FieldTypeDefinition("TextBox", "Text Input", "bi-input-cursor-text", "Basic"),
        new FieldTypeDefinition("TextArea", "Text Area", "bi-textarea-t", "Basic"),
        new FieldTypeDefinition("Number", "Number", "bi-123", "Basic"),
        new FieldTypeDefinition("Currency", "Currency", "bi-currency-dollar", "Basic"),
        
        // Choice
        new FieldTypeDefinition("DropDown", "Dropdown", "bi-menu-button-wide", "Choice"),
        new FieldTypeDefinition("RadioGroup", "Radio Group", "bi-ui-radios", "Choice"),
        new FieldTypeDefinition("CheckboxList", "Checkboxes", "bi-ui-checks", "Choice"),
        new FieldTypeDefinition("Checkbox", "Single Checkbox", "bi-check-square", "Choice"),
        
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
        new FieldTypeDefinition("AutoComplete", "AutoComplete", "bi-search", "Advanced",
            true, typeof(AutoCompleteConfig)),
        
        // Layout
        new FieldTypeDefinition("Section", "Section", "bi-layout-three-columns", "Layout"),
        new FieldTypeDefinition("Panel", "Panel", "bi-window", "Layout"),
        new FieldTypeDefinition("Divider", "Divider", "bi-dash-lg", "Layout"),
        new FieldTypeDefinition("Label", "Label", "bi-fonts", "Layout"),
        new FieldTypeDefinition("Html", "HTML Content", "bi-code-slash", "Layout"), // Added from Appendix A
    };
    
    public static FieldTypeDefinition? GetByType(string fieldType) 
        => AllTypes.FirstOrDefault(t => t.FieldType == fieldType);
    
    public static IEnumerable<IGrouping<string, FieldTypeDefinition>> GetByCategory()
        => AllTypes.GroupBy(t => t.Category);
}
