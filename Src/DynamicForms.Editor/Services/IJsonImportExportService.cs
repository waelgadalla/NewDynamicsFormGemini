using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Services;

public interface IJsonImportExportService
{
    // Export
    string SerializeModule(FormModuleSchema module);
    string SerializeWorkflow(FormWorkflowSchema workflow);
    byte[] GenerateDownloadBytes(string json);

    // Import
    FormModuleSchema? DeserializeModule(string json);
    FormWorkflowSchema? DeserializeWorkflow(string json);
    
    // File Download Trigger
    Task DownloadFileAsync(string fileName, string content);
}
