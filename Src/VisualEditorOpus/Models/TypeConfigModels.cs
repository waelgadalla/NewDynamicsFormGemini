using DynamicForms.Core.V4.Schemas;

namespace VisualEditorOpus.Models;

/// <summary>
/// Mutable model for DateConfig editing.
/// </summary>
public class DateConfigModel
{
    public bool AllowFuture { get; set; } = true;
    public bool AllowPast { get; set; } = true;
    public string MinDatePreset { get; set; } = ""; // "", "Now", "Now-30d", "Now-1y", "custom"
    public string MaxDatePreset { get; set; } = ""; // "", "Now", "Now+30d", "Now+1y", "custom"
    public string? CustomMinDate { get; set; }
    public string? CustomMaxDate { get; set; }

    public string? MinDate => MinDatePreset == "custom" ? CustomMinDate : (string.IsNullOrEmpty(MinDatePreset) ? null : MinDatePreset);
    public string? MaxDate => MaxDatePreset == "custom" ? CustomMaxDate : (string.IsNullOrEmpty(MaxDatePreset) ? null : MaxDatePreset);

    public DateConfig ToConfig() => new()
    {
        AllowFuture = AllowFuture,
        AllowPast = AllowPast,
        MinDate = MinDate,
        MaxDate = MaxDate
    };

    public static DateConfigModel FromConfig(DateConfig config)
    {
        var model = new DateConfigModel
        {
            AllowFuture = config.AllowFuture,
            AllowPast = config.AllowPast
        };

        // Determine preset for MinDate
        if (string.IsNullOrEmpty(config.MinDate))
            model.MinDatePreset = "";
        else if (config.MinDate is "Now" or "Now-30d" or "Now-1y")
            model.MinDatePreset = config.MinDate;
        else
        {
            model.MinDatePreset = "custom";
            model.CustomMinDate = config.MinDate;
        }

        // Determine preset for MaxDate
        if (string.IsNullOrEmpty(config.MaxDate))
            model.MaxDatePreset = "";
        else if (config.MaxDate is "Now" or "Now+30d" or "Now+1y")
            model.MaxDatePreset = config.MaxDate;
        else
        {
            model.MaxDatePreset = "custom";
            model.CustomMaxDate = config.MaxDate;
        }

        return model;
    }
}

/// <summary>
/// Mutable model for FileUploadConfig editing.
/// </summary>
public class FileUploadConfigModel
{
    public List<string> AllowedExtensions { get; set; } = new();
    public decimal FileSizeValue { get; set; } = 10;
    public string FileSizeUnit { get; set; } = "MB"; // KB, MB, GB
    public bool AllowMultiple { get; set; }
    public bool ScanRequired { get; set; } = true;

    public long MaxFileSizeBytes => FileSizeUnit switch
    {
        "KB" => (long)(FileSizeValue * 1024),
        "MB" => (long)(FileSizeValue * 1024 * 1024),
        "GB" => (long)(FileSizeValue * 1024 * 1024 * 1024),
        _ => (long)(FileSizeValue * 1024 * 1024)
    };

    public FileUploadConfig ToConfig() => new()
    {
        AllowedExtensions = AllowedExtensions.ToArray(),
        MaxFileSizeBytes = MaxFileSizeBytes,
        AllowMultiple = AllowMultiple,
        ScanRequired = ScanRequired
    };

    public static FileUploadConfigModel FromConfig(FileUploadConfig config)
    {
        var model = new FileUploadConfigModel
        {
            AllowedExtensions = config.AllowedExtensions.ToList(),
            AllowMultiple = config.AllowMultiple,
            ScanRequired = config.ScanRequired
        };

        // Convert bytes to best unit
        if (config.MaxFileSizeBytes >= 1024 * 1024 * 1024)
        {
            model.FileSizeValue = config.MaxFileSizeBytes / (1024m * 1024m * 1024m);
            model.FileSizeUnit = "GB";
        }
        else if (config.MaxFileSizeBytes >= 1024 * 1024)
        {
            model.FileSizeValue = config.MaxFileSizeBytes / (1024m * 1024m);
            model.FileSizeUnit = "MB";
        }
        else
        {
            model.FileSizeValue = config.MaxFileSizeBytes / 1024m;
            model.FileSizeUnit = "KB";
        }

        return model;
    }

    public void AddExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return;

        var ext = extension.Trim();
        if (!ext.StartsWith('.'))
            ext = "." + ext;

        ext = ext.ToLowerInvariant();

        if (!AllowedExtensions.Contains(ext))
            AllowedExtensions.Add(ext);
    }

    public void RemoveExtension(string extension)
    {
        AllowedExtensions.Remove(extension);
    }

    public void AddDocumentPreset()
    {
        foreach (var ext in new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" })
        {
            if (!AllowedExtensions.Contains(ext))
                AllowedExtensions.Add(ext);
        }
    }

    public void AddImagePreset()
    {
        foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" })
        {
            if (!AllowedExtensions.Contains(ext))
                AllowedExtensions.Add(ext);
        }
    }

    public void AddAllCommonPreset()
    {
        AddDocumentPreset();
        AddImagePreset();
    }
}

/// <summary>
/// Mutable model for AutoCompleteConfig editing.
/// </summary>
public class AutoCompleteConfigModel
{
    public string DataSourceUrl { get; set; } = "";
    public string QueryParameter { get; set; } = "q";
    public int MinCharacters { get; set; } = 3;
    public string ValueField { get; set; } = "";
    public string DisplayField { get; set; } = "";
    public string? ItemTemplate { get; set; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(DataSourceUrl) &&
        !string.IsNullOrWhiteSpace(ValueField) &&
        !string.IsNullOrWhiteSpace(DisplayField) &&
        MinCharacters >= 1 && MinCharacters <= 10;

    public AutoCompleteConfig ToConfig() => new()
    {
        DataSourceUrl = DataSourceUrl,
        QueryParameter = QueryParameter,
        MinCharacters = MinCharacters,
        ValueField = ValueField,
        DisplayField = DisplayField,
        ItemTemplate = string.IsNullOrWhiteSpace(ItemTemplate) ? null : ItemTemplate
    };

    public static AutoCompleteConfigModel FromConfig(AutoCompleteConfig config) => new()
    {
        DataSourceUrl = config.DataSourceUrl,
        QueryParameter = config.QueryParameter,
        MinCharacters = config.MinCharacters,
        ValueField = config.ValueField,
        DisplayField = config.DisplayField,
        ItemTemplate = config.ItemTemplate
    };
}

/// <summary>
/// Mutable model for DataGridConfig editing.
/// </summary>
public class DataGridConfigModel
{
    public bool AllowAdd { get; set; } = true;
    public bool AllowEdit { get; set; } = true;
    public bool AllowDelete { get; set; } = true;
    public int? MaxRows { get; set; }
    public string EditorMode { get; set; } = "Modal"; // "Modal" | "Inline"
    public List<DataGridColumnModel> Columns { get; set; } = new();

    public DataGridConfig ToConfig() => new()
    {
        AllowAdd = AllowAdd,
        AllowEdit = AllowEdit,
        AllowDelete = AllowDelete,
        MaxRows = MaxRows,
        EditorMode = EditorMode,
        Columns = Columns.Select(c => c.ToSchema()).ToArray()
    };

    public static DataGridConfigModel FromConfig(DataGridConfig config) => new()
    {
        AllowAdd = config.AllowAdd,
        AllowEdit = config.AllowEdit,
        AllowDelete = config.AllowDelete,
        MaxRows = config.MaxRows,
        EditorMode = config.EditorMode,
        Columns = config.Columns.Select(DataGridColumnModel.FromSchema).ToList()
    };
}

/// <summary>
/// Model for a DataGrid column definition.
/// </summary>
public class DataGridColumnModel
{
    public string Id { get; set; } = "";
    public string LabelEn { get; set; } = "";
    public string? LabelFr { get; set; }
    public string FieldType { get; set; } = "TextBox";
    public bool IsRequired { get; set; }
    public string? Width { get; set; }

    public FormFieldSchema ToSchema() => new()
    {
        Id = Id,
        LabelEn = LabelEn,
        LabelFr = LabelFr,
        FieldType = FieldType,
        Validation = new FieldValidationConfig { IsRequired = IsRequired }
    };

    public static DataGridColumnModel FromSchema(FormFieldSchema schema) => new()
    {
        Id = schema.Id,
        LabelEn = schema.LabelEn ?? "",
        LabelFr = schema.LabelFr,
        FieldType = schema.FieldType,
        IsRequired = schema.Validation?.IsRequired ?? false
    };
}
