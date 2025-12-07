using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Models;

public class DataGridConfigModel
{
    public bool AllowAdd { get; set; } = true;
    public bool AllowEdit { get; set; } = true;
    public bool AllowDelete { get; set; } = true;
    public int? MaxRows { get; set; }
    public string EditorMode { get; set; } = "Modal";
    public List<FormFieldSchema> Columns { get; set; } = new();

    public static DataGridConfigModel From(DataGridConfig? config)
    {
        if (config == null) return new DataGridConfigModel();
        return new DataGridConfigModel
        {
            AllowAdd = config.AllowAdd,
            AllowEdit = config.AllowEdit,
            AllowDelete = config.AllowDelete,
            MaxRows = config.MaxRows,
            EditorMode = config.EditorMode,
            Columns = config.Columns.ToList()
        };
    }

    public DataGridConfig ToConfig()
    {
        return new DataGridConfig
        {
            AllowAdd = AllowAdd,
            AllowEdit = AllowEdit,
            AllowDelete = AllowDelete,
            MaxRows = MaxRows,
            EditorMode = EditorMode,
            Columns = Columns.ToArray()
        };
    }
}
