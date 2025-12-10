using Microsoft.AspNetCore.Components;
using System.Text.Json;
using VisualEditorOpus.Models;
using DynamicForms.Core.V4.Schemas;

namespace VisualEditorOpus.Components.Editor.Modals.TypeConfig;

/// <summary>
/// Dynamic modal that shows different configuration editors based on field type.
/// Handles DateConfig, FileUploadConfig, AutoCompleteConfig, and DataGridConfig.
/// </summary>
public partial class TypeConfigModal : ComponentBase
{
    private DateConfigModel dateConfig = new();
    private FileUploadConfigModel fileUploadConfig = new();
    private AutoCompleteConfigModel autoCompleteConfig = new();
    private DataGridConfigModel dataGridConfig = new();

    /// <summary>
    /// Gets or sets whether the modal is open.
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>
    /// Event callback for two-way binding of IsOpen.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsOpenChanged { get; set; }

    /// <summary>
    /// The field type to configure (e.g., "DatePicker", "FileUpload", etc.).
    /// </summary>
    [Parameter]
    public string FieldType { get; set; } = "";

    /// <summary>
    /// Existing configuration to edit (null for new configuration).
    /// </summary>
    [Parameter]
    public FieldTypeConfig? ExistingConfig { get; set; }

    /// <summary>
    /// Callback when the configuration is saved.
    /// </summary>
    [Parameter]
    public EventCallback<FieldTypeConfig> OnSave { get; set; }

    /// <summary>
    /// Callback when the modal is cancelled.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    private bool HasValidEditor => FieldType is "DatePicker" or "TimePicker" or "DateTimePicker"
                                           or "FileUpload" or "AutoComplete" or "DataGrid";

    private bool CanSave => HasValidEditor && IsConfigValid();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (IsOpen)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        // Convert existing config to mutable model
        if (ExistingConfig is DateConfig dc)
        {
            dateConfig = DateConfigModel.FromConfig(dc);
        }
        else if (ExistingConfig is FileUploadConfig fc)
        {
            fileUploadConfig = FileUploadConfigModel.FromConfig(fc);
        }
        else if (ExistingConfig is AutoCompleteConfig ac)
        {
            autoCompleteConfig = AutoCompleteConfigModel.FromConfig(ac);
        }
        else if (ExistingConfig is DataGridConfig dgc)
        {
            dataGridConfig = DataGridConfigModel.FromConfig(dgc);
        }
        else
        {
            // Initialize with defaults
            dateConfig = new DateConfigModel();
            fileUploadConfig = new FileUploadConfigModel();
            autoCompleteConfig = new AutoCompleteConfigModel();
            dataGridConfig = new DataGridConfigModel();
        }
    }

    private string GetTitle()
    {
        return FieldType switch
        {
            "DatePicker" => "Date Configuration",
            "TimePicker" => "Time Configuration",
            "DateTimePicker" => "Date/Time Configuration",
            "FileUpload" => "File Upload Configuration",
            "AutoComplete" => "AutoComplete Configuration",
            "DataGrid" => "DataGrid Configuration",
            _ => "Field Configuration"
        };
    }

    private string GetIcon()
    {
        return FieldType switch
        {
            "DatePicker" or "TimePicker" or "DateTimePicker" => "bi-calendar",
            "FileUpload" => "bi-upload",
            "AutoComplete" => "bi-search",
            "DataGrid" => "bi-table",
            _ => "bi-sliders"
        };
    }

    private ModalSize GetModalSize()
    {
        return FieldType == "DataGrid" ? ModalSize.Large : ModalSize.Medium;
    }

    private string GetTypeBadgeText()
    {
        return FieldType switch
        {
            "DatePicker" => "DatePicker Configuration",
            "TimePicker" => "TimePicker Configuration",
            "DateTimePicker" => "DateTimePicker Configuration",
            "FileUpload" => "FileUpload Configuration",
            "AutoComplete" => "AutoComplete Configuration",
            "DataGrid" => "DataGrid Configuration",
            _ => "Configuration"
        };
    }

    private bool IsConfigValid()
    {
        return FieldType switch
        {
            "DatePicker" or "TimePicker" or "DateTimePicker" => true, // Date config is always valid
            "FileUpload" => true, // File upload config is always valid
            "AutoComplete" => autoCompleteConfig.IsValid,
            "DataGrid" => true, // DataGrid config is always valid
            _ => false
        };
    }

    private string GetConfigPreview()
    {
        try
        {
            var config = BuildConfig();
            return JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return "{ \"error\": \"Unable to generate preview\" }";
        }
    }

    private FieldTypeConfig BuildConfig()
    {
        return FieldType switch
        {
            "DatePicker" or "TimePicker" or "DateTimePicker" => dateConfig.ToConfig(),
            "FileUpload" => fileUploadConfig.ToConfig(),
            "AutoComplete" => autoCompleteConfig.ToConfig(),
            "DataGrid" => dataGridConfig.ToConfig(),
            _ => throw new InvalidOperationException($"No config for field type: {FieldType}")
        };
    }

    private async Task HandleSave()
    {
        if (!CanSave) return;

        var config = BuildConfig();
        await OnSave.InvokeAsync(config);
        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await IsOpenChanged.InvokeAsync(false);
    }
}
