using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Text.Json;
using DynamicForms.Core.V4.Schemas;

namespace VisualEditorOpus.Components.Editor.Modals;

/// <summary>
/// Code-behind for the ImportJsonModal component.
/// Allows importing form modules or workflows from JSON files or pasted content.
/// </summary>
public partial class ImportJsonModal : ComponentBase
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

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
    /// Callback when a module is imported.
    /// </summary>
    [Parameter]
    public EventCallback<ImportResult<FormModuleSchema>> OnImportModule { get; set; }

    /// <summary>
    /// Callback when a workflow is imported.
    /// </summary>
    [Parameter]
    public EventCallback<ImportResult<FormWorkflowSchema>> OnImportWorkflow { get; set; }

    /// <summary>
    /// Callback when the modal is cancelled.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    // State
    private string activeTab = "file";
    private string jsonContent = "";
    private bool isValid;
    private string? validationMessage;
    private string? detectedType; // "module" or "workflow"
    private FormModuleSchema? parsedModule;
    private FormWorkflowSchema? parsedWorkflow;
    private ImportMode importMode = ImportMode.Replace;

    // File upload state
    private InputFile? fileInput;
    private bool isDragOver;
    private bool hasFile;
    private string? fileName;

    // JSON serialization options
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (IsOpen)
        {
            // Reset state when modal opens
            ResetState();
        }
    }

    private void ResetState()
    {
        activeTab = "file";
        jsonContent = "";
        isValid = false;
        validationMessage = null;
        detectedType = null;
        parsedModule = null;
        parsedWorkflow = null;
        importMode = ImportMode.Replace;
        isDragOver = false;
        hasFile = false;
        fileName = null;
    }

    private void SetActiveTab(string tab)
    {
        activeTab = tab;
        // Clear validation when switching tabs
        isValid = false;
        validationMessage = null;
        detectedType = null;
        parsedModule = null;
        parsedWorkflow = null;
    }

    private async Task TriggerFileInput()
    {
        // Use JS interop to trigger the hidden file input
        await JS.InvokeVoidAsync("eval", "document.querySelector('.file-input-hidden').click()");
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;

        // Validate file size (5MB max)
        const long maxSize = 5 * 1024 * 1024;
        if (file.Size > maxSize)
        {
            isValid = false;
            validationMessage = "File is too large. Maximum size is 5MB.";
            return;
        }

        // Validate file type
        if (!file.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            isValid = false;
            validationMessage = "Invalid file type. Please select a .json file.";
            return;
        }

        try
        {
            using var stream = file.OpenReadStream(maxSize);
            using var reader = new StreamReader(stream);
            jsonContent = await reader.ReadToEndAsync();
            hasFile = true;
            fileName = file.Name;

            await ValidateJson();
        }
        catch (Exception ex)
        {
            isValid = false;
            validationMessage = $"Error reading file: {ex.Message}";
        }
    }

    private void HandleDragEnter()
    {
        isDragOver = true;
    }

    private void HandleDragOver()
    {
        isDragOver = true;
    }

    private void HandleDragLeave()
    {
        isDragOver = false;
    }

    private void HandleDrop()
    {
        isDragOver = false;
        // The actual file handling will be done by HandleFileSelected
        // which is triggered by the InputFile component
    }

    private Task ValidateJson()
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            isValid = false;
            validationMessage = "Please enter JSON content";
            return Task.CompletedTask;
        }

        // Try to parse as FormModuleSchema first
        try
        {
            var module = JsonSerializer.Deserialize<FormModuleSchema>(jsonContent, JsonOptions);
            if (module != null && !string.IsNullOrEmpty(module.TitleEn))
            {
                detectedType = "module";
                parsedModule = module;
                parsedWorkflow = null;
                isValid = true;
                var fieldCount = module.Fields?.Length ?? 0;
                validationMessage = $"FormModuleSchema detected with {fieldCount} field{(fieldCount != 1 ? "s" : "")}";
                return Task.CompletedTask;
            }
        }
        catch (JsonException)
        {
            // Not a valid module, try workflow
        }

        // Try to parse as FormWorkflowSchema
        try
        {
            var workflow = JsonSerializer.Deserialize<FormWorkflowSchema>(jsonContent, JsonOptions);
            if (workflow != null && !string.IsNullOrEmpty(workflow.TitleEn))
            {
                detectedType = "workflow";
                parsedWorkflow = workflow;
                parsedModule = null;
                isValid = true;
                var moduleCount = workflow.ModuleIds?.Length ?? 0;
                validationMessage = $"FormWorkflowSchema detected with {moduleCount} module{(moduleCount != 1 ? "s" : "")}";
                return Task.CompletedTask;
            }
        }
        catch (JsonException)
        {
            // Not a valid workflow either
        }

        // Check if it's valid JSON at all
        try
        {
            JsonDocument.Parse(jsonContent);
            isValid = false;
            validationMessage = "Valid JSON but unrecognized schema. Expected FormModuleSchema or FormWorkflowSchema.";
        }
        catch (JsonException ex)
        {
            isValid = false;
            validationMessage = $"Invalid JSON: {ex.Message}";
        }

        return Task.CompletedTask;
    }

    private async Task HandleImport()
    {
        if (!isValid) return;

        if (detectedType == "module" && parsedModule != null)
        {
            var result = new ImportResult<FormModuleSchema>(parsedModule, importMode);
            await OnImportModule.InvokeAsync(result);
        }
        else if (detectedType == "workflow" && parsedWorkflow != null)
        {
            var result = new ImportResult<FormWorkflowSchema>(parsedWorkflow, importMode);
            await OnImportWorkflow.InvokeAsync(result);
        }

        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await IsOpenChanged.InvokeAsync(false);
    }
}

/// <summary>
/// Import mode options.
/// </summary>
public enum ImportMode
{
    /// <summary>
    /// Replace the current module/workflow entirely.
    /// </summary>
    Replace,

    /// <summary>
    /// Merge with the current module/workflow (add new fields/modules only).
    /// </summary>
    Merge
}

/// <summary>
/// Result of an import operation.
/// </summary>
/// <typeparam name="T">The schema type (FormModuleSchema or FormWorkflowSchema).</typeparam>
public record ImportResult<T>(T Schema, ImportMode Mode) where T : class;
