using System.Text.Json;
using System.Text.Json.Serialization;
using DynamicForms.Core.V4.Schemas;
using Microsoft.JSInterop;

namespace DynamicForms.Editor.Services;

public class JsonImportExportService : IJsonImportExportService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _options;

    public JsonImportExportService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        
        // Configure options to handle polymorphic types and pretty printing
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
        
        // Note: System.Text.Json requires derived types to be registered 
        // or usage of [JsonDerivedType] attributes on the base class (FieldTypeConfig).
        // If Core.V4 schemas don't have these attributes, we might need a custom converter 
        // or the user should ensure Core schemas are updated. 
        // Assuming Core V4 is set up correctly or we rely on standard deserialization for now.
    }

    public string SerializeModule(FormModuleSchema module)
    {
        return JsonSerializer.Serialize(module, _options);
    }

    public string SerializeWorkflow(FormWorkflowSchema workflow)
    {
        return JsonSerializer.Serialize(workflow, _options);
    }

    public byte[] GenerateDownloadBytes(string json)
    {
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public FormModuleSchema? DeserializeModule(string json)
    {
        try 
        {
            return JsonSerializer.Deserialize<FormModuleSchema>(json, _options);
        }
        catch
        {
            return null;
        }
    }

    public FormWorkflowSchema? DeserializeWorkflow(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<FormWorkflowSchema>(json, _options);
        }
        catch
        {
            return null;
        }
    }

    public async Task DownloadFileAsync(string fileName, string content)
    {
        await _jsRuntime.InvokeVoidAsync("editorInterop.downloadFile", fileName, content);
    }
}
