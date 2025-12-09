# D.4 JSON Import/Export Service - Implementation Plan

> **Task**: JSON Import/Export Service & UI Components
> **Location**: `Src/VisualEditorOpus/Services/` and `Src/VisualEditorOpus/Components/ImportExport/`
> **Priority**: High
> **Estimated Effort**: 5-6 hours
> **Delegation**: 75% AI

---

## Overview

The JSON Import/Export Service provides functionality to export form schemas to JSON files and import JSON files back into the form editor. It includes validation, format options, merge strategies, and a history of recent operations.

---

## Service to Create

### IJsonImportExportService.cs (Interface)

```csharp
namespace VisualEditorOpus.Services;

public interface IJsonImportExportService
{
    // Export
    Task<ExportResult> ExportAsync(FormModuleSchema module, ExportOptions options);
    Task<string> SerializeAsync(FormModuleSchema module, ExportOptions options);

    // Import
    Task<ImportResult> ImportAsync(Stream fileStream, ImportOptions options);
    Task<ImportResult> ImportAsync(string jsonContent, ImportOptions options);
    Task<ValidationResult> ValidateAsync(string jsonContent);

    // History
    IReadOnlyList<ImportExportHistoryItem> GetHistory();
    void ClearHistory();
}
```

### JsonImportExportService.cs (Implementation)

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using DynamicForms.Core.V4;

namespace VisualEditorOpus.Services;

public class JsonImportExportService : IJsonImportExportService
{
    private readonly List<ImportExportHistoryItem> _history = new();
    private readonly JsonSerializerOptions _serializerOptions;

    public JsonImportExportService()
    {
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }

    #region Export

    public async Task<ExportResult> ExportAsync(FormModuleSchema module, ExportOptions options)
    {
        try
        {
            var json = await SerializeAsync(module, options);
            var fileName = GenerateFileName(module, options);

            // Add to history
            _history.Insert(0, new ImportExportHistoryItem
            {
                Id = Guid.NewGuid().ToString(),
                FileName = fileName,
                Operation = OperationType.Export,
                Size = System.Text.Encoding.UTF8.GetByteCount(json),
                Timestamp = DateTime.UtcNow,
                Success = true
            });

            return new ExportResult
            {
                Success = true,
                FileName = fileName,
                Content = json,
                Size = System.Text.Encoding.UTF8.GetByteCount(json)
            };
        }
        catch (Exception ex)
        {
            return new ExportResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<string> SerializeAsync(FormModuleSchema module, ExportOptions options)
    {
        var serializeOptions = new JsonSerializerOptions(_serializerOptions)
        {
            WriteIndented = options.PrettyPrint && !options.Minify
        };

        object exportData = options.Format switch
        {
            ExportFormat.JsonSchema => CreateJsonSchemaExport(module, options),
            ExportFormat.TypeScript => throw new NotSupportedException("TypeScript export not yet implemented"),
            _ => CreateStandardExport(module, options)
        };

        var json = JsonSerializer.Serialize(exportData, serializeOptions);

        return Task.FromResult(json);
    }

    private object CreateStandardExport(FormModuleSchema module, ExportOptions options)
    {
        var export = new Dictionary<string, object?>
        {
            ["$schema"] = "https://forms.example.com/schema/v4.json",
            ["id"] = module.Id,
            ["name"] = module.Name,
            ["titleEn"] = module.TitleEn,
            ["titleFr"] = module.TitleFr,
            ["descriptionEn"] = module.DescriptionEn,
            ["descriptionFr"] = module.DescriptionFr,
            ["version"] = module.Version,
            ["tableName"] = module.TableName,
            ["schemaName"] = module.SchemaName
        };

        if (options.IncludeMetadata)
        {
            export["metadata"] = new Dictionary<string, object?>
            {
                ["createdAt"] = module.CreatedAt,
                ["updatedAt"] = module.UpdatedAt,
                ["createdBy"] = module.CreatedBy,
                ["status"] = module.Status?.ToString()
            };
        }

        // Export fields
        var fields = module.Fields?.Select(f => ExportField(f, options)).ToList();
        export["fields"] = fields;

        return export;
    }

    private Dictionary<string, object?> ExportField(FormFieldSchema field, ExportOptions options)
    {
        var fieldExport = new Dictionary<string, object?>
        {
            ["type"] = field.Type.ToString(),
            ["labelEn"] = field.LabelEn,
            ["labelFr"] = field.LabelFr,
            ["placeholderEn"] = field.PlaceholderEn,
            ["placeholderFr"] = field.PlaceholderFr,
            ["helpTextEn"] = field.HelpTextEn,
            ["helpTextFr"] = field.HelpTextFr,
            ["isRequired"] = field.IsRequired,
            ["isReadOnly"] = field.IsReadOnly,
            ["isHidden"] = field.IsHidden,
            ["sortOrder"] = field.SortOrder
        };

        if (options.IncludeFieldIds)
        {
            fieldExport["id"] = field.Id;
        }

        if (field.ParentId != null)
        {
            fieldExport["parentId"] = field.ParentId;
        }

        if (field.Options?.Any() == true)
        {
            fieldExport["options"] = field.Options.Select(o => new Dictionary<string, object?>
            {
                ["value"] = o.Value,
                ["labelEn"] = o.LabelEn,
                ["labelFr"] = o.LabelFr,
                ["isDefault"] = o.IsDefault,
                ["sortOrder"] = o.SortOrder
            }).ToList();
        }

        if (field.Validation != null)
        {
            fieldExport["validation"] = field.Validation;
        }

        if (field.TypeConfig != null)
        {
            fieldExport["typeConfig"] = field.TypeConfig;
        }

        if (field.ColumnName != null)
        {
            fieldExport["columnName"] = field.ColumnName;
            fieldExport["columnType"] = field.ColumnType;
        }

        if (field.Accessibility != null)
        {
            fieldExport["accessibility"] = field.Accessibility;
        }

        if (field.ComputedValue != null)
        {
            fieldExport["computedValue"] = field.ComputedValue;
        }

        return fieldExport;
    }

    private object CreateJsonSchemaExport(FormModuleSchema module, ExportOptions options)
    {
        // Generate JSON Schema format
        var schema = new Dictionary<string, object?>
        {
            ["$schema"] = "http://json-schema.org/draft-07/schema#",
            ["$id"] = $"https://forms.example.com/schemas/{module.Id}.json",
            ["title"] = module.TitleEn,
            ["description"] = module.DescriptionEn,
            ["type"] = "object"
        };

        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        foreach (var field in module.Fields ?? Enumerable.Empty<FormFieldSchema>())
        {
            var fieldName = field.ColumnName ?? field.Id;
            properties[fieldName] = GetJsonSchemaType(field);

            if (field.IsRequired)
            {
                required.Add(fieldName);
            }
        }

        schema["properties"] = properties;
        schema["required"] = required;

        return schema;
    }

    private object GetJsonSchemaType(FormFieldSchema field)
    {
        var schema = new Dictionary<string, object?>
        {
            ["title"] = field.LabelEn,
            ["description"] = field.HelpTextEn
        };

        switch (field.Type)
        {
            case FieldType.Text:
            case FieldType.TextArea:
            case FieldType.Email:
            case FieldType.Phone:
            case FieldType.RichText:
                schema["type"] = "string";
                if (field.Type == FieldType.Email)
                    schema["format"] = "email";
                break;

            case FieldType.Number:
                schema["type"] = "number";
                break;

            case FieldType.Date:
            case FieldType.DateTime:
                schema["type"] = "string";
                schema["format"] = field.Type == FieldType.Date ? "date" : "date-time";
                break;

            case FieldType.Time:
                schema["type"] = "string";
                schema["format"] = "time";
                break;

            case FieldType.Toggle:
                schema["type"] = "boolean";
                break;

            case FieldType.Select:
            case FieldType.Radio:
                schema["type"] = "string";
                if (field.Options?.Any() == true)
                {
                    schema["enum"] = field.Options.Select(o => o.Value).ToList();
                }
                break;

            case FieldType.Checkbox:
                schema["type"] = "array";
                schema["items"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["enum"] = field.Options?.Select(o => o.Value).ToList() ?? new List<string>()
                };
                break;

            default:
                schema["type"] = "string";
                break;
        }

        return schema;
    }

    private string GenerateFileName(FormModuleSchema module, ExportOptions options)
    {
        var baseName = !string.IsNullOrEmpty(module.Name)
            ? module.Name.ToLower().Replace(" ", "-")
            : "form-schema";

        var extension = options.Format switch
        {
            ExportFormat.TypeScript => ".d.ts",
            _ => ".json"
        };

        return $"{baseName}{extension}";
    }

    #endregion

    #region Import

    public async Task<ImportResult> ImportAsync(Stream fileStream, ImportOptions options)
    {
        using var reader = new StreamReader(fileStream);
        var json = await reader.ReadToEndAsync();
        return await ImportAsync(json, options);
    }

    public async Task<ImportResult> ImportAsync(string jsonContent, ImportOptions options)
    {
        var validation = await ValidateAsync(jsonContent);

        if (!validation.IsValid)
        {
            _history.Insert(0, new ImportExportHistoryItem
            {
                Id = Guid.NewGuid().ToString(),
                FileName = options.FileName ?? "unknown.json",
                Operation = OperationType.Import,
                Size = System.Text.Encoding.UTF8.GetByteCount(jsonContent),
                Timestamp = DateTime.UtcNow,
                Success = false,
                ErrorMessage = validation.Errors.FirstOrDefault()?.Message
            });

            return new ImportResult
            {
                Success = false,
                Errors = validation.Errors
            };
        }

        try
        {
            var importData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
            var module = ParseFormModule(importData, options);

            _history.Insert(0, new ImportExportHistoryItem
            {
                Id = Guid.NewGuid().ToString(),
                FileName = options.FileName ?? "imported.json",
                Operation = OperationType.Import,
                Size = System.Text.Encoding.UTF8.GetByteCount(jsonContent),
                Timestamp = DateTime.UtcNow,
                Success = true
            });

            return new ImportResult
            {
                Success = true,
                Module = module,
                Warnings = validation.Warnings,
                FieldCount = module.Fields?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            return new ImportResult
            {
                Success = false,
                Errors = new List<ValidationError>
                {
                    new() { Message = ex.Message, Severity = ValidationSeverity.Error }
                }
            };
        }
    }

    public Task<ValidationResult> ValidateAsync(string jsonContent)
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        // Check JSON syntax
        try
        {
            var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            // Check for required properties
            if (!root.TryGetProperty("fields", out _) && !root.TryGetProperty("Fields", out _))
            {
                warnings.Add(new ValidationWarning
                {
                    Message = "No 'fields' property found. Form will have no fields.",
                    Severity = ValidationSeverity.Warning
                });
            }

            // Check schema version
            if (root.TryGetProperty("$schema", out var schemaUrl))
            {
                var url = schemaUrl.GetString();
                if (!string.IsNullOrEmpty(url) && url.Contains("v4"))
                {
                    // Valid v4 schema
                }
                else
                {
                    warnings.Add(new ValidationWarning
                    {
                        Message = "Schema version not recognized. Import may have compatibility issues.",
                        Severity = ValidationSeverity.Warning
                    });
                }
            }

            // Check for fields with missing translations
            if (root.TryGetProperty("fields", out var fields) ||
                root.TryGetProperty("Fields", out fields))
            {
                var missingFrenchCount = 0;
                foreach (var field in fields.EnumerateArray())
                {
                    if (!field.TryGetProperty("labelFr", out var labelFr) ||
                        string.IsNullOrEmpty(labelFr.GetString()))
                    {
                        missingFrenchCount++;
                    }
                }

                if (missingFrenchCount > 0)
                {
                    warnings.Add(new ValidationWarning
                    {
                        Message = $"{missingFrenchCount} field(s) have missing French translations.",
                        Severity = ValidationSeverity.Warning
                    });
                }
            }
        }
        catch (JsonException ex)
        {
            errors.Add(new ValidationError
            {
                Message = $"Invalid JSON: {ex.Message}",
                Line = (int?)ex.LineNumber,
                Column = (int?)ex.BytePositionInLine,
                Severity = ValidationSeverity.Error
            });
        }

        return Task.FromResult(new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            Warnings = warnings
        });
    }

    private FormModuleSchema ParseFormModule(JsonElement element, ImportOptions options)
    {
        var moduleOptions = new JsonSerializerOptions(_serializerOptions)
        {
            PropertyNameCaseInsensitive = true
        };

        var module = JsonSerializer.Deserialize<FormModuleSchema>(
            element.GetRawText(), moduleOptions);

        if (module == null)
        {
            throw new InvalidOperationException("Failed to parse form module");
        }

        // Generate new IDs if not preserving
        if (!options.PreserveIds)
        {
            module = module with
            {
                Id = Guid.NewGuid().ToString(),
                Fields = module.Fields?.Select(f => f with
                {
                    Id = Guid.NewGuid().ToString()
                }).ToList()
            };
        }

        // Apply import mode
        if (options.Mode == ImportMode.CreateNew)
        {
            module = module with
            {
                Name = module.Name + " (Imported)",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        return module;
    }

    #endregion

    #region History

    public IReadOnlyList<ImportExportHistoryItem> GetHistory() => _history.AsReadOnly();

    public void ClearHistory() => _history.Clear();

    #endregion
}
```

---

## Supporting Types

```csharp
// Models/ExportOptions.cs
public record ExportOptions
{
    public ExportFormat Format { get; init; } = ExportFormat.Json;
    public bool IncludeMetadata { get; init; } = true;
    public bool IncludeFieldIds { get; init; } = false;
    public bool PrettyPrint { get; init; } = true;
    public bool Minify { get; init; } = false;
}

public enum ExportFormat
{
    Json,
    JsonSchema,
    TypeScript
}

// Models/ExportResult.cs
public record ExportResult
{
    public bool Success { get; init; }
    public string? FileName { get; init; }
    public string? Content { get; init; }
    public int Size { get; init; }
    public string? ErrorMessage { get; init; }
}

// Models/ImportOptions.cs
public record ImportOptions
{
    public ImportMode Mode { get; init; } = ImportMode.Replace;
    public bool PreserveIds { get; init; } = false;
    public string? FileName { get; init; }
}

public enum ImportMode
{
    Replace,
    Merge,
    CreateNew
}

// Models/ImportResult.cs
public record ImportResult
{
    public bool Success { get; init; }
    public FormModuleSchema? Module { get; init; }
    public List<ValidationError> Errors { get; init; } = new();
    public List<ValidationWarning> Warnings { get; init; } = new();
    public int FieldCount { get; init; }
}

// Models/ValidationResult.cs
public record ValidationResult
{
    public bool IsValid { get; init; }
    public List<ValidationError> Errors { get; init; } = new();
    public List<ValidationWarning> Warnings { get; init; } = new();
}

public record ValidationError
{
    public string Message { get; init; } = "";
    public int? Line { get; init; }
    public int? Column { get; init; }
    public ValidationSeverity Severity { get; init; }
}

public record ValidationWarning
{
    public string Message { get; init; } = "";
    public ValidationSeverity Severity { get; init; }
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}

// Models/ImportExportHistoryItem.cs
public record ImportExportHistoryItem
{
    public string Id { get; init; } = "";
    public string FileName { get; init; } = "";
    public OperationType Operation { get; init; }
    public int Size { get; init; }
    public DateTime Timestamp { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

public enum OperationType
{
    Import,
    Export
}
```

---

## UI Components

### ExportPanel.razor

```razor
@namespace VisualEditorOpus.Components.ImportExport
@inject IJsonImportExportService ImportExportService
@inject IJSRuntime JS

<div class="export-panel">
    <div class="panel-header">
        <span class="panel-title">
            <i class="bi bi-box-arrow-up"></i>
            Export Form Schema
        </span>
    </div>
    <div class="panel-content">
        <div class="export-options">
            <!-- Format Selection -->
            <div class="option-group">
                <span class="option-label">Export Format</span>
                <div class="option-cards">
                    @foreach (var format in Enum.GetValues<ExportFormat>())
                    {
                        <div class="option-card @(Options.Format == format ? "selected" : "")"
                             @onclick="() => SelectFormat(format)">
                            <div class="option-card-icon">
                                <i class="bi bi-@GetFormatIcon(format)"></i>
                            </div>
                            <span class="option-card-label">@GetFormatLabel(format)</span>
                            <span class="option-card-desc">@GetFormatDescription(format)</span>
                        </div>
                    }
                </div>
            </div>

            <!-- Export Options -->
            <div class="option-group">
                <span class="option-label">Options</span>
                <div class="checkbox-group">
                    <label class="checkbox-item">
                        <input type="checkbox" @bind="Options.IncludeMetadata" />
                        <div class="checkbox-label">
                            <div class="checkbox-label-main">Include Metadata</div>
                            <div class="checkbox-label-desc">Created date, author, version</div>
                        </div>
                    </label>
                    <label class="checkbox-item">
                        <input type="checkbox" @bind="Options.PrettyPrint" />
                        <div class="checkbox-label">
                            <div class="checkbox-label-main">Pretty Print</div>
                            <div class="checkbox-label-desc">Format with indentation</div>
                        </div>
                    </label>
                    <label class="checkbox-item">
                        <input type="checkbox" @bind="Options.Minify" />
                        <div class="checkbox-label">
                            <div class="checkbox-label-main">Minify Output</div>
                            <div class="checkbox-label-desc">Remove whitespace (smaller file)</div>
                        </div>
                    </label>
                    <label class="checkbox-item">
                        <input type="checkbox" @bind="Options.IncludeFieldIds" />
                        <div class="checkbox-label">
                            <div class="checkbox-label-main">Include Field IDs</div>
                            <div class="checkbox-label-desc">Preserve internal identifiers</div>
                        </div>
                    </label>
                </div>
            </div>
        </div>

        <div class="action-buttons">
            <button class="btn btn-secondary" @onclick="CopyToClipboard">
                <i class="bi bi-clipboard"></i>
                Copy to Clipboard
            </button>
            <button class="btn btn-primary" @onclick="DownloadJson">
                <i class="bi bi-download"></i>
                Download JSON
            </button>
        </div>
    </div>
</div>

@code {
    [Parameter] public FormModuleSchema? Module { get; set; }
    [Parameter] public EventCallback<string> OnExported { get; set; }

    private ExportOptions Options { get; set; } = new();

    private void SelectFormat(ExportFormat format)
    {
        Options = Options with { Format = format };
    }

    private async Task CopyToClipboard()
    {
        if (Module == null) return;

        var json = await ImportExportService.SerializeAsync(Module, Options);
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", json);
        await OnExported.InvokeAsync("Copied to clipboard!");
    }

    private async Task DownloadJson()
    {
        if (Module == null) return;

        var result = await ImportExportService.ExportAsync(Module, Options);

        if (result.Success)
        {
            await JS.InvokeVoidAsync("downloadJson", result.Content, result.FileName);
            await OnExported.InvokeAsync($"Downloaded {result.FileName}");
        }
    }

    private string GetFormatIcon(ExportFormat format) => format switch
    {
        ExportFormat.Json => "filetype-json",
        ExportFormat.JsonSchema => "filetype-json",
        ExportFormat.TypeScript => "code-square",
        _ => "file-code"
    };

    private string GetFormatLabel(ExportFormat format) => format switch
    {
        ExportFormat.Json => "JSON",
        ExportFormat.JsonSchema => "JSON Schema",
        ExportFormat.TypeScript => "TypeScript",
        _ => format.ToString()
    };

    private string GetFormatDescription(ExportFormat format) => format switch
    {
        ExportFormat.Json => "Standard format",
        ExportFormat.JsonSchema => "With validation",
        ExportFormat.TypeScript => "Type definitions",
        _ => ""
    };
}
```

### ImportPanel.razor

```razor
@namespace VisualEditorOpus.Components.ImportExport
@inject IJsonImportExportService ImportExportService
@inject IJSRuntime JS
@implements IAsyncDisposable

<div class="import-panel">
    <div class="panel-header">
        <span class="panel-title">
            <i class="bi bi-box-arrow-in-down"></i>
            Import Form Schema
        </span>
    </div>
    <div class="panel-content">
        @if (!HasFile)
        {
            <!-- Drop Zone -->
            <div class="drop-zone @(IsDragOver ? "dragover" : "")"
                 @ondragover="HandleDragOver"
                 @ondragover:preventDefault
                 @ondragleave="HandleDragLeave"
                 @ondrop="HandleDrop"
                 @ondrop:preventDefault
                 @onclick="TriggerFileInput">
                <div class="drop-zone-icon">
                    <i class="bi bi-cloud-arrow-up"></i>
                </div>
                <div class="drop-zone-text">
                    <div class="drop-zone-title">Drag and drop your JSON file here</div>
                    <div class="drop-zone-subtitle">Supports .json files up to 5MB</div>
                </div>
                <div class="drop-zone-or">or</div>
                <button class="browse-btn">Browse Files</button>
            </div>
            <InputFile @ref="fileInput" OnChange="HandleFileSelected" accept=".json" style="display:none" />
        }
        else
        {
            <!-- File Preview -->
            <div class="file-preview">
                <div class="file-preview-header">
                    <div class="file-info">
                        <div class="file-icon">
                            <i class="bi bi-file-earmark-code"></i>
                        </div>
                        <div class="file-details">
                            <span class="file-name">@FileName</span>
                            <span class="file-size">@FormatSize(FileSize)</span>
                        </div>
                    </div>
                    <button class="file-remove" @onclick="RemoveFile">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>

                <!-- Validation Preview -->
                @if (ValidationResult != null)
                {
                    <div class="validation-preview @(ValidationResult.IsValid ? "valid" : "invalid")">
                        <div class="validation-header">
                            <i class="bi bi-@(ValidationResult.IsValid ? "check-circle-fill" : "x-circle-fill")"></i>
                            @(ValidationResult.IsValid ? "Valid JSON Schema Detected" : "Validation Errors")
                        </div>
                        <div class="validation-details">
                            @if (ValidationResult.IsValid)
                            {
                                <div class="validation-item success">
                                    <i class="bi bi-check-circle-fill"></i>
                                    <span>Valid JSON syntax</span>
                                </div>
                            }
                            @foreach (var error in ValidationResult.Errors)
                            {
                                <div class="validation-item error">
                                    <i class="bi bi-x-circle-fill"></i>
                                    <span>@error.Message</span>
                                </div>
                            }
                            @foreach (var warning in ValidationResult.Warnings)
                            {
                                <div class="validation-item @warning.Severity.ToString().ToLower()">
                                    <i class="bi bi-@GetWarningIcon(warning.Severity)"></i>
                                    <span>@warning.Message</span>
                                </div>
                            }
                        </div>
                    </div>
                }

                <!-- Import Options -->
                <div class="import-options">
                    <div class="import-options-title">Import Mode</div>
                    <div class="radio-group">
                        @foreach (var mode in Enum.GetValues<ImportMode>())
                        {
                            <label class="radio-item @(Options.Mode == mode ? "selected" : "")"
                                   @onclick="() => SelectMode(mode)">
                                <input type="radio" name="importMode" checked="@(Options.Mode == mode)" />
                                <div>
                                    <div class="radio-label-main">@GetModeLabel(mode)</div>
                                    <div class="radio-label-desc">@GetModeDescription(mode)</div>
                                </div>
                            </label>
                        }
                    </div>
                </div>
            </div>

            <!-- Progress -->
            @if (IsImporting)
            {
                <div class="progress-container active">
                    <div class="progress-header">
                        <span class="progress-label">Importing form schema...</span>
                        <span class="progress-percent">@Progress%</span>
                    </div>
                    <div class="progress-bar">
                        <div class="progress-fill" style="width: @Progress%"></div>
                    </div>
                </div>
            }

            <div class="action-buttons">
                <button class="btn btn-secondary" @onclick="RemoveFile" disabled="@IsImporting">
                    Cancel
                </button>
                <button class="btn btn-primary" @onclick="ImportFile"
                        disabled="@(!CanImport || IsImporting)">
                    <i class="bi bi-upload"></i>
                    Import Form
                </button>
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public EventCallback<FormModuleSchema> OnImported { get; set; }
    [Parameter] public EventCallback<string> OnError { get; set; }

    private InputFile? fileInput;
    private DotNetObjectReference<ImportPanel>? objRef;

    private bool HasFile => !string.IsNullOrEmpty(FileContent);
    private string FileName { get; set; } = "";
    private long FileSize { get; set; }
    private string FileContent { get; set; } = "";
    private ValidationResult? ValidationResult { get; set; }
    private ImportOptions Options { get; set; } = new();
    private bool IsDragOver { get; set; }
    private bool IsImporting { get; set; }
    private int Progress { get; set; }
    private bool CanImport => ValidationResult?.IsValid == true && !IsImporting;

    protected override void OnInitialized()
    {
        objRef = DotNetObjectReference.Create(this);
    }

    private void HandleDragOver()
    {
        IsDragOver = true;
    }

    private void HandleDragLeave()
    {
        IsDragOver = false;
    }

    private async Task HandleDrop()
    {
        IsDragOver = false;
        // File will be handled by InputFile component
    }

    private async Task TriggerFileInput()
    {
        await JS.InvokeVoidAsync("triggerInputFile", fileInput?.Element);
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;

        FileName = file.Name;
        FileSize = file.Size;

        using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
        using var reader = new StreamReader(stream);
        FileContent = await reader.ReadToEndAsync();

        ValidationResult = await ImportExportService.ValidateAsync(FileContent);
        Options = Options with { FileName = FileName };
    }

    private void RemoveFile()
    {
        FileName = "";
        FileSize = 0;
        FileContent = "";
        ValidationResult = null;
        Progress = 0;
    }

    private void SelectMode(ImportMode mode)
    {
        Options = Options with { Mode = mode };
    }

    private async Task ImportFile()
    {
        if (!CanImport) return;

        IsImporting = true;
        Progress = 0;

        try
        {
            // Simulate progress
            for (int i = 0; i <= 100; i += 25)
            {
                Progress = i;
                StateHasChanged();
                await Task.Delay(200);
            }

            var result = await ImportExportService.ImportAsync(FileContent, Options);

            if (result.Success && result.Module != null)
            {
                await OnImported.InvokeAsync(result.Module);
                RemoveFile();
            }
            else
            {
                await OnError.InvokeAsync(result.Errors.FirstOrDefault()?.Message ?? "Import failed");
            }
        }
        finally
        {
            IsImporting = false;
        }
    }

    private string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    private string GetWarningIcon(ValidationSeverity severity) => severity switch
    {
        ValidationSeverity.Info => "info-circle-fill",
        ValidationSeverity.Warning => "exclamation-triangle-fill",
        ValidationSeverity.Error => "x-circle-fill",
        _ => "info-circle-fill"
    };

    private string GetModeLabel(ImportMode mode) => mode switch
    {
        ImportMode.Replace => "Replace Current Form",
        ImportMode.Merge => "Merge Fields",
        ImportMode.CreateNew => "Create New Form",
        _ => mode.ToString()
    };

    private string GetModeDescription(ImportMode mode) => mode switch
    {
        ImportMode.Replace => "Completely replace with imported schema",
        ImportMode.Merge => "Add new fields, update existing ones",
        ImportMode.CreateNew => "Import as a separate form module",
        _ => ""
    };

    public void Dispose()
    {
        objRef?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        objRef?.Dispose();
    }
}
```

### HistoryPanel.razor

```razor
@namespace VisualEditorOpus.Components.ImportExport
@inject IJsonImportExportService ImportExportService

<div class="history-section">
    <div class="history-header">
        <span class="history-title">
            <i class="bi bi-clock-history"></i>
            Recent Activity
        </span>
        @if (History.Any())
        {
            <button class="btn-link" @onclick="ClearHistory">Clear</button>
        }
    </div>
    <div class="history-list">
        @if (!History.Any())
        {
            <div class="history-empty">
                <i class="bi bi-inbox"></i>
                <span>No recent activity</span>
            </div>
        }
        else
        {
            @foreach (var item in History.Take(10))
            {
                <div class="history-item">
                    <div class="history-icon @item.Operation.ToString().ToLower()">
                        <i class="bi bi-@(item.Operation == OperationType.Export ? "box-arrow-up" : "box-arrow-in-down")"></i>
                    </div>
                    <div class="history-content">
                        <div class="history-name">@item.FileName</div>
                        <div class="history-meta">
                            <span>@item.Operation</span>
                            <span>@FormatSize(item.Size)</span>
                            <span>@GetRelativeTime(item.Timestamp)</span>
                        </div>
                    </div>
                    @if (item.Operation == OperationType.Export)
                    {
                        <button class="history-action" @onclick="() => RedownloadItem(item)">
                            Download Again
                        </button>
                    }
                </div>
            }
        }
    </div>
</div>

@code {
    [Parameter] public EventCallback<ImportExportHistoryItem> OnRedownload { get; set; }

    private IReadOnlyList<ImportExportHistoryItem> History => ImportExportService.GetHistory();

    private void ClearHistory()
    {
        ImportExportService.ClearHistory();
        StateHasChanged();
    }

    private async Task RedownloadItem(ImportExportHistoryItem item)
    {
        await OnRedownload.InvokeAsync(item);
    }

    private string FormatSize(int bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    private string GetRelativeTime(DateTime time)
    {
        var diff = DateTime.UtcNow - time;

        if (diff.TotalSeconds < 60) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hour(s) ago";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} day(s) ago";
        return time.ToString("MMM d, yyyy");
    }
}
```

---

## CSS Styles

```css
/* ===== EXPORT/IMPORT PANELS ===== */
.export-panel,
.import-panel {
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-lg);
    overflow: hidden;
}

.panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    background: var(--bg-tertiary);
    border-bottom: 1px solid var(--border-color);
}

.panel-title {
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 16px;
    font-weight: 600;
}

.panel-title i {
    font-size: 20px;
    color: var(--primary);
}

.panel-content {
    padding: 20px;
}

/* ===== OPTION CARDS ===== */
.option-cards {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 10px;
}

.option-card {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    padding: 16px;
    border: 2px solid var(--border-color);
    border-radius: var(--radius-md);
    background: var(--bg-primary);
    cursor: pointer;
    transition: all 0.15s;
}

.option-card:hover,
.option-card.selected {
    border-color: var(--primary);
    background: var(--primary-light);
}

.option-card-icon {
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--radius-sm);
    background: var(--bg-tertiary);
    font-size: 20px;
    color: var(--text-secondary);
}

.option-card.selected .option-card-icon {
    background: var(--primary);
    color: white;
}

/* ===== DROP ZONE ===== */
.drop-zone {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 12px;
    padding: 40px;
    border: 2px dashed var(--border-color);
    border-radius: var(--radius-lg);
    background: var(--bg-secondary);
    cursor: pointer;
    transition: all 0.2s;
}

.drop-zone:hover,
.drop-zone.dragover {
    border-color: var(--primary);
    background: var(--primary-light);
}

.drop-zone-icon {
    width: 64px;
    height: 64px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    background: var(--bg-tertiary);
    color: var(--text-muted);
    font-size: 28px;
}

.drop-zone.dragover .drop-zone-icon {
    background: var(--primary);
    color: white;
}

/* ===== VALIDATION PREVIEW ===== */
.validation-preview {
    margin-top: 16px;
    padding: 12px;
    border-radius: var(--radius-md);
}

.validation-preview.valid {
    background: var(--success-light);
}

.validation-preview.invalid {
    background: var(--error-light);
}

.validation-item {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 12px;
    padding: 8px 10px;
    background: var(--bg-primary);
    border-radius: var(--radius-sm);
}

.validation-item.success i { color: var(--success); }
.validation-item.error i { color: var(--error); }
.validation-item.warning i { color: var(--warning); }
.validation-item.info i { color: var(--info); }

/* ===== PROGRESS ===== */
.progress-container {
    margin-top: 20px;
}

.progress-bar {
    height: 8px;
    background: var(--bg-tertiary);
    border-radius: 4px;
    overflow: hidden;
}

.progress-fill {
    height: 100%;
    background: var(--primary);
    border-radius: 4px;
    transition: width 0.3s;
}

/* ===== HISTORY ===== */
.history-section {
    margin-top: 24px;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-lg);
    overflow: hidden;
}

.history-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px 16px;
    border-bottom: 1px solid var(--border-color);
    cursor: pointer;
    transition: background 0.15s;
}

.history-item:hover {
    background: var(--bg-secondary);
}

.history-icon {
    width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--radius-sm);
    font-size: 16px;
}

.history-icon.export {
    background: var(--success-light);
    color: var(--success);
}

.history-icon.import {
    background: var(--info-light);
    color: var(--info);
}
```

---

## JavaScript Interop

```javascript
// wwwroot/js/import-export.js

window.downloadJson = (content, filename) => {
    const blob = new Blob([content], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

window.triggerInputFile = (element) => {
    if (element) {
        element.click();
    }
};
```

---

## Service Registration

```csharp
// Program.cs
builder.Services.AddScoped<IJsonImportExportService, JsonImportExportService>();
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the JSON Import/Export Service and UI components for my Blazor form editor.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Services Location: Src/VisualEditorOpus/Services/
- Components Location: Src/VisualEditorOpus/Components/ImportExport/

## Service to Create:

### IJsonImportExportService
Interface with:
- ExportAsync(module, options): Export form to JSON
- SerializeAsync(module, options): Serialize to string
- ImportAsync(stream/string, options): Import JSON file
- ValidateAsync(json): Validate JSON content
- GetHistory(): Get operation history
- ClearHistory(): Clear history

### JsonImportExportService
Implementation with:
- Export formats: JSON, JSON Schema, TypeScript (future)
- Export options: metadata, pretty print, minify, include IDs
- Import validation with errors and warnings
- Import modes: Replace, Merge, Create New
- Operation history tracking

## UI Components:

### 1. ExportPanel.razor
- Format selection (cards)
- Export options (checkboxes)
- Copy to clipboard button
- Download button

### 2. ImportPanel.razor
- Drag and drop zone
- File preview with validation results
- Import mode selection (radio)
- Progress indicator
- Import button

### 3. HistoryPanel.razor
- List of recent operations
- Re-download option for exports

## Features:
- System.Text.Json serialization
- JSON syntax validation
- Schema validation (warnings for missing translations)
- File size limit (5MB)
- Progress feedback during import

Please implement complete, production-ready code with CSS.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `JsonImportExportService-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for export format selection testing (JSON, JSON Schema)
- Export option toggles testing (metadata, pretty print, minify, include IDs)
- Copy to clipboard testing
- Download JSON file testing
- Drag and drop file import testing
- File browse import testing
- Invalid JSON error display testing
- Valid JSON success display testing
- Warning display for missing translations testing
- Import mode selection testing (Replace, Merge, Create New)
- Import progress indicator testing
- History panel display testing
- Re-download from history testing
- Clear history testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Service registration in Program.cs
- Model files creation (ExportOptions, ImportOptions, ValidationResult, etc.)
- JavaScript file import (import-export.js)
- Integration with existing module editor
- Merge import mode implementation (complex logic)
- TypeScript export format implementation
- CSS file imports

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Export panel displays format cards
- [ ] JSON format card selectable
- [ ] JSON Schema format card selectable
- [ ] TypeScript format shows "not implemented" or disabled
- [ ] Include Metadata checkbox works
- [ ] Pretty Print checkbox works
- [ ] Minify checkbox works
- [ ] Include Field IDs checkbox works
- [ ] Copy to Clipboard copies JSON
- [ ] Download JSON saves file
- [ ] File named correctly (module-name.json)
- [ ] Drop zone highlights on drag over
- [ ] File input accepts .json files
- [ ] File preview shows name and size
- [ ] Remove file button works
- [ ] Valid JSON shows success validation
- [ ] Invalid JSON shows error validation
- [ ] Missing French translations show warning
- [ ] Replace mode radio works
- [ ] Merge mode radio works
- [ ] Create New mode radio works
- [ ] Import button disabled when invalid
- [ ] Progress bar shows during import
- [ ] Import creates FormModuleSchema
- [ ] History shows recent operations
- [ ] Export operations show in history
- [ ] Import operations show in history
- [ ] Re-download button works
- [ ] Clear history button works
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Export creates valid JSON file
- [ ] Export includes metadata when option selected
- [ ] Export pretty prints when option selected
- [ ] Export minifies when option selected
- [ ] Copy to clipboard works
- [ ] Download triggers file save
- [ ] Drag and drop accepts JSON files
- [ ] File input accepts JSON files
- [ ] Invalid JSON shows error
- [ ] Valid JSON shows success
- [ ] Warnings display for missing translations
- [ ] Import mode selection works
- [ ] Progress indicator shows during import
- [ ] Import creates new form module
- [ ] History tracks operations
- [ ] Re-download works from history
- [ ] Dark mode styling correct

---

## Notes

- Consider adding schema versioning support
- Consider adding migration for older schema versions
- Consider adding batch export (multiple forms)
- Consider adding export to ZIP with related assets
- Consider adding import validation against schema
- Consider persisting history to local storage
- File size limit is 5MB for imports
