# F.3 CodeSetImportExport - Implementation Guide

## Overview

The CodeSetImportExport system provides functionality to import CodeSet data from various file formats (JSON, CSV, Excel) and export CodeSets to these formats. It includes data preview, field mapping, validation, and progress tracking.

## Component Architecture

```
CodeSetImportExport/
├── ImportExportModal.razor           # Main modal component
├── ImportExportModal.razor.css       # Scoped styles
├── ImportPanel.razor                 # Import UI
├── ExportPanel.razor                 # Export UI
├── Components/
│   ├── FileDropZone.razor            # Drag-and-drop file upload
│   ├── FilePreview.razor             # Preview uploaded data
│   ├── FieldMapper.razor             # Column mapping UI
│   ├── ImportProgress.razor          # Progress indicator
│   └── FormatSelector.razor          # Export format selection
└── Services/
    ├── IImportExportService.cs       # Service interface
    ├── ImportExportService.cs        # Implementation
    ├── Parsers/
    │   ├── JsonParser.cs             # JSON parsing
    │   ├── CsvParser.cs              # CSV parsing
    │   └── ExcelParser.cs            # Excel parsing
    └── Exporters/
        ├── JsonExporter.cs           # JSON export
        ├── CsvExporter.cs            # CSV export
        └── ExcelExporter.cs          # Excel export
```

## Data Models

### ImportOptions.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// Options for importing CodeSet data
/// </summary>
public record ImportOptions
{
    /// <summary>
    /// How to handle the imported data
    /// </summary>
    public ImportMode Mode { get; init; } = ImportMode.CreateNew;

    /// <summary>
    /// Target CodeSet ID for merge/replace operations
    /// </summary>
    public string? TargetCodeSetId { get; init; }

    /// <summary>
    /// Name for new CodeSet
    /// </summary>
    public string NewCodeSetName { get; init; } = "";

    /// <summary>
    /// Field mapping configuration
    /// </summary>
    public FieldMapping Mapping { get; init; } = new();

    /// <summary>
    /// How to handle duplicate codes
    /// </summary>
    public DuplicateHandling DuplicateHandling { get; init; } = DuplicateHandling.Skip;

    /// <summary>
    /// Whether to validate data before importing
    /// </summary>
    public bool ValidateBeforeImport { get; init; } = true;

    /// <summary>
    /// Whether to trim whitespace from values
    /// </summary>
    public bool TrimWhitespace { get; init; } = true;

    /// <summary>
    /// Whether to skip empty rows
    /// </summary>
    public bool SkipEmptyRows { get; init; } = true;

    /// <summary>
    /// Character encoding for text files
    /// </summary>
    public string Encoding { get; init; } = "UTF-8";
}

public enum ImportMode
{
    CreateNew,      // Create a new CodeSet
    MergeWith,      // Add to existing CodeSet
    Replace         // Replace existing CodeSet data
}

public enum DuplicateHandling
{
    Skip,           // Skip duplicate codes
    Update,         // Update existing with new values
    Error           // Fail on duplicates
}
```

### FieldMapping.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// Maps source file columns to CodeSet fields
/// </summary>
public record FieldMapping
{
    /// <summary>
    /// Whether to auto-detect mapping
    /// </summary>
    public bool AutoDetect { get; init; } = true;

    /// <summary>
    /// Source column for Code field
    /// </summary>
    public string? CodeColumn { get; init; }

    /// <summary>
    /// Source column for DisplayName field
    /// </summary>
    public string? DisplayNameColumn { get; init; }

    /// <summary>
    /// Source column for Description field
    /// </summary>
    public string? DescriptionColumn { get; init; }

    /// <summary>
    /// Source column for Order field
    /// </summary>
    public string? OrderColumn { get; init; }

    /// <summary>
    /// Source column for Status field
    /// </summary>
    public string? StatusColumn { get; init; }

    /// <summary>
    /// Source column for ParentCode field
    /// </summary>
    public string? ParentCodeColumn { get; init; }

    /// <summary>
    /// Additional custom field mappings
    /// </summary>
    public Dictionary<string, string> CustomMappings { get; init; } = new();
}
```

### ExportOptions.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// Options for exporting CodeSet data
/// </summary>
public record ExportOptions
{
    /// <summary>
    /// Export file format
    /// </summary>
    public ExportFormat Format { get; init; } = ExportFormat.Json;

    /// <summary>
    /// Whether to include CodeSet metadata
    /// </summary>
    public bool IncludeMetadata { get; init; } = true;

    /// <summary>
    /// Whether to include inactive items
    /// </summary>
    public bool IncludeInactive { get; init; } = false;

    /// <summary>
    /// Whether to include deprecated items
    /// </summary>
    public bool IncludeDeprecated { get; init; } = false;

    /// <summary>
    /// Pretty print JSON output
    /// </summary>
    public bool PrettyPrint { get; init; } = true;

    /// <summary>
    /// Fields to include in export
    /// </summary>
    public List<string> IncludeFields { get; init; } = new()
    {
        "Code", "DisplayName", "Description", "Order", "Status"
    };

    /// <summary>
    /// Character encoding for text files
    /// </summary>
    public string Encoding { get; init; } = "UTF-8";

    /// <summary>
    /// CSV delimiter character
    /// </summary>
    public char CsvDelimiter { get; init; } = ',';

    /// <summary>
    /// Whether to include header row in CSV
    /// </summary>
    public bool IncludeHeader { get; init; } = true;
}

public enum ExportFormat
{
    Json,
    Csv,
    Excel,
    Xml
}
```

### ImportResult.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// Result of an import operation
/// </summary>
public record ImportResult
{
    public bool Success { get; init; }
    public string? CodeSetId { get; init; }
    public string? CodeSetName { get; init; }
    public int TotalRows { get; init; }
    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public int ErrorCount { get; init; }
    public List<ImportError> Errors { get; init; } = new();
    public List<ImportWarning> Warnings { get; init; } = new();
    public TimeSpan Duration { get; init; }
}

public record ImportError
{
    public int RowNumber { get; init; }
    public string Column { get; init; } = "";
    public string Value { get; init; } = "";
    public string Message { get; init; } = "";
}

public record ImportWarning
{
    public int RowNumber { get; init; }
    public string Message { get; init; } = "";
}
```

### ParsedData.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// Represents parsed file data before import
/// </summary>
public record ParsedData
{
    public string FileName { get; init; } = "";
    public long FileSize { get; init; }
    public string FileType { get; init; } = "";
    public List<string> Columns { get; init; } = new();
    public List<Dictionary<string, object>> Rows { get; init; } = new();
    public int TotalRows { get; init; }
    public FieldMapping DetectedMapping { get; init; } = new();
}
```

## Services

### IImportExportService.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Services;

public interface IImportExportService
{
    // Parsing
    Task<ParsedData> ParseFileAsync(Stream stream, string fileName);
    Task<ParsedData> ParseJsonAsync(Stream stream);
    Task<ParsedData> ParseCsvAsync(Stream stream, char delimiter = ',');
    Task<ParsedData> ParseExcelAsync(Stream stream, string sheetName = "");

    // Import
    Task<ImportResult> ImportAsync(ParsedData data, ImportOptions options, IProgress<ImportProgress>? progress = null);
    Task<ImportResult> ValidateAsync(ParsedData data, ImportOptions options);

    // Export
    Task<byte[]> ExportAsync(string codeSetId, ExportOptions options);
    Task<byte[]> ExportToJsonAsync(CodeSet codeSet, ExportOptions options);
    Task<byte[]> ExportToCsvAsync(CodeSet codeSet, ExportOptions options);
    Task<byte[]> ExportToExcelAsync(CodeSet codeSet, ExportOptions options);
    Task<byte[]> ExportToXmlAsync(CodeSet codeSet, ExportOptions options);

    // Field detection
    FieldMapping DetectFieldMapping(List<string> columns);
}

public record ImportProgress
{
    public int Current { get; init; }
    public int Total { get; init; }
    public string Status { get; init; } = "";
    public double Percentage => Total > 0 ? (double)Current / Total * 100 : 0;
}
```

### ImportExportService.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Services;

public class ImportExportService : IImportExportService
{
    private readonly ICodeSetService _codeSetService;
    private readonly ILogger<ImportExportService> _logger;

    public ImportExportService(
        ICodeSetService codeSetService,
        ILogger<ImportExportService> logger)
    {
        _codeSetService = codeSetService;
        _logger = logger;
    }

    public async Task<ParsedData> ParseFileAsync(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".json" => await ParseJsonAsync(stream),
            ".csv" => await ParseCsvAsync(stream),
            ".xlsx" or ".xls" => await ParseExcelAsync(stream),
            _ => throw new NotSupportedException($"File type {extension} is not supported")
        };
    }

    public async Task<ParsedData> ParseJsonAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var items = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json, options)
            ?? new List<Dictionary<string, JsonElement>>();

        // Get columns from first item
        var columns = items.FirstOrDefault()?.Keys.ToList() ?? new List<string>();

        // Convert to generic dictionary
        var rows = items.Select(item => item.ToDictionary(
            kvp => kvp.Key,
            kvp => (object)GetJsonValue(kvp.Value)
        )).ToList();

        return new ParsedData
        {
            FileType = "JSON",
            Columns = columns,
            Rows = rows,
            TotalRows = rows.Count,
            DetectedMapping = DetectFieldMapping(columns)
        };
    }

    public async Task<ParsedData> ParseCsvAsync(Stream stream, char delimiter = ',')
    {
        var rows = new List<Dictionary<string, object>>();
        var columns = new List<string>();

        using var reader = new StreamReader(stream);

        // Read header
        var headerLine = await reader.ReadLineAsync();
        if (!string.IsNullOrEmpty(headerLine))
        {
            columns = ParseCsvLine(headerLine, delimiter);
        }

        // Read data rows
        int lineNumber = 1;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line, delimiter);
            var row = new Dictionary<string, object>();

            for (int i = 0; i < Math.Min(columns.Count, values.Count); i++)
            {
                row[columns[i]] = values[i];
            }

            rows.Add(row);
            lineNumber++;
        }

        return new ParsedData
        {
            FileType = "CSV",
            Columns = columns,
            Rows = rows,
            TotalRows = rows.Count,
            DetectedMapping = DetectFieldMapping(columns)
        };
    }

    public async Task<ParsedData> ParseExcelAsync(Stream stream, string sheetName = "")
    {
        // Using a library like EPPlus or ClosedXML
        // This is a simplified example
        var rows = new List<Dictionary<string, object>>();
        var columns = new List<string>();

        // Implementation would use EPPlus:
        // using var package = new ExcelPackage(stream);
        // var worksheet = string.IsNullOrEmpty(sheetName)
        //     ? package.Workbook.Worksheets.First()
        //     : package.Workbook.Worksheets[sheetName];
        // ...

        return new ParsedData
        {
            FileType = "Excel",
            Columns = columns,
            Rows = rows,
            TotalRows = rows.Count,
            DetectedMapping = DetectFieldMapping(columns)
        };
    }

    public FieldMapping DetectFieldMapping(List<string> columns)
    {
        var mapping = new FieldMapping { AutoDetect = true };
        var lowerColumns = columns.Select(c => c.ToLowerInvariant()).ToList();

        // Detect Code field
        var codePatterns = new[] { "code", "id", "key", "value" };
        var codeIndex = FindColumnIndex(lowerColumns, codePatterns);
        if (codeIndex >= 0)
            mapping = mapping with { CodeColumn = columns[codeIndex] };

        // Detect DisplayName field
        var namePatterns = new[] { "name", "displayname", "display_name", "label", "text", "title" };
        var nameIndex = FindColumnIndex(lowerColumns, namePatterns);
        if (nameIndex >= 0)
            mapping = mapping with { DisplayNameColumn = columns[nameIndex] };

        // Detect Description field
        var descPatterns = new[] { "description", "desc", "details", "notes" };
        var descIndex = FindColumnIndex(lowerColumns, descPatterns);
        if (descIndex >= 0)
            mapping = mapping with { DescriptionColumn = columns[descIndex] };

        // Detect Order field
        var orderPatterns = new[] { "order", "sort", "sequence", "position", "index" };
        var orderIndex = FindColumnIndex(lowerColumns, orderPatterns);
        if (orderIndex >= 0)
            mapping = mapping with { OrderColumn = columns[orderIndex] };

        // Detect Status field
        var statusPatterns = new[] { "status", "active", "enabled", "state" };
        var statusIndex = FindColumnIndex(lowerColumns, statusPatterns);
        if (statusIndex >= 0)
            mapping = mapping with { StatusColumn = columns[statusIndex] };

        return mapping;
    }

    public async Task<ImportResult> ImportAsync(
        ParsedData data,
        ImportOptions options,
        IProgress<ImportProgress>? progress = null)
    {
        var startTime = DateTime.UtcNow;
        var errors = new List<ImportError>();
        var warnings = new List<ImportWarning>();
        var importedCount = 0;
        var skippedCount = 0;

        // Validate first if requested
        if (options.ValidateBeforeImport)
        {
            var validationResult = await ValidateAsync(data, options);
            if (!validationResult.Success)
                return validationResult;
        }

        // Create or get target CodeSet
        CodeSet codeSet;
        if (options.Mode == ImportMode.CreateNew)
        {
            codeSet = await _codeSetService.CreateCodeSetAsync(new CodeSet
            {
                Name = options.NewCodeSetName,
                CodeField = options.Mapping.CodeColumn ?? "code",
                DisplayField = options.Mapping.DisplayNameColumn ?? "displayName"
            });
        }
        else
        {
            codeSet = await _codeSetService.GetCodeSetByIdAsync(options.TargetCodeSetId!)
                ?? throw new InvalidOperationException("Target CodeSet not found");

            if (options.Mode == ImportMode.Replace)
            {
                // Clear existing items
                var existingItems = await _codeSetService.GetItemsAsync(codeSet.Id);
                foreach (var item in existingItems)
                {
                    await _codeSetService.DeleteItemAsync(codeSet.Id, item.Id);
                }
            }
        }

        // Get existing codes for duplicate detection
        var existingCodes = new HashSet<string>(
            (await _codeSetService.GetItemsAsync(codeSet.Id))
                .Select(i => i.Code.ToLowerInvariant())
        );

        // Import rows
        for (int i = 0; i < data.Rows.Count; i++)
        {
            var row = data.Rows[i];
            progress?.Report(new ImportProgress
            {
                Current = i + 1,
                Total = data.Rows.Count,
                Status = $"Importing row {i + 1} of {data.Rows.Count}"
            });

            try
            {
                var item = MapRowToItem(row, options.Mapping, i + 1);

                if (options.TrimWhitespace)
                {
                    item = item with
                    {
                        Code = item.Code.Trim(),
                        DisplayName = item.DisplayName.Trim(),
                        Description = item.Description?.Trim()
                    };
                }

                // Check for duplicates
                var codeLower = item.Code.ToLowerInvariant();
                if (existingCodes.Contains(codeLower))
                {
                    switch (options.DuplicateHandling)
                    {
                        case DuplicateHandling.Skip:
                            skippedCount++;
                            warnings.Add(new ImportWarning
                            {
                                RowNumber = i + 2,
                                Message = $"Skipped duplicate code: {item.Code}"
                            });
                            continue;

                        case DuplicateHandling.Update:
                            // Find and update existing
                            var existing = (await _codeSetService.GetItemsAsync(codeSet.Id))
                                .FirstOrDefault(x => x.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                item = item with { Id = existing.Id };
                                await _codeSetService.UpdateItemAsync(codeSet.Id, item);
                                importedCount++;
                            }
                            continue;

                        case DuplicateHandling.Error:
                            errors.Add(new ImportError
                            {
                                RowNumber = i + 2,
                                Column = "Code",
                                Value = item.Code,
                                Message = "Duplicate code"
                            });
                            continue;
                    }
                }

                await _codeSetService.AddItemAsync(codeSet.Id, item);
                existingCodes.Add(codeLower);
                importedCount++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError
                {
                    RowNumber = i + 2,
                    Message = ex.Message
                });
            }
        }

        return new ImportResult
        {
            Success = errors.Count == 0,
            CodeSetId = codeSet.Id,
            CodeSetName = codeSet.Name,
            TotalRows = data.Rows.Count,
            ImportedCount = importedCount,
            SkippedCount = skippedCount,
            ErrorCount = errors.Count,
            Errors = errors,
            Warnings = warnings,
            Duration = DateTime.UtcNow - startTime
        };
    }

    public Task<ImportResult> ValidateAsync(ParsedData data, ImportOptions options)
    {
        var errors = new List<ImportError>();
        var warnings = new List<ImportWarning>();

        // Check required mapping
        if (string.IsNullOrEmpty(options.Mapping.CodeColumn))
        {
            errors.Add(new ImportError { Message = "Code column mapping is required" });
        }

        // Validate each row
        for (int i = 0; i < Math.Min(data.Rows.Count, 1000); i++)
        {
            var row = data.Rows[i];

            // Check Code field
            if (options.Mapping.CodeColumn != null)
            {
                var code = row.GetValueOrDefault(options.Mapping.CodeColumn)?.ToString();
                if (string.IsNullOrWhiteSpace(code))
                {
                    errors.Add(new ImportError
                    {
                        RowNumber = i + 2,
                        Column = options.Mapping.CodeColumn,
                        Message = "Code is required"
                    });
                }
            }
        }

        return Task.FromResult(new ImportResult
        {
            Success = errors.Count == 0,
            TotalRows = data.Rows.Count,
            Errors = errors,
            Warnings = warnings
        });
    }

    // Export methods
    public async Task<byte[]> ExportAsync(string codeSetId, ExportOptions options)
    {
        var codeSet = await _codeSetService.GetCodeSetByIdAsync(codeSetId)
            ?? throw new InvalidOperationException("CodeSet not found");

        return options.Format switch
        {
            ExportFormat.Json => await ExportToJsonAsync(codeSet, options),
            ExportFormat.Csv => await ExportToCsvAsync(codeSet, options),
            ExportFormat.Excel => await ExportToExcelAsync(codeSet, options),
            ExportFormat.Xml => await ExportToXmlAsync(codeSet, options),
            _ => throw new NotSupportedException($"Export format {options.Format} not supported")
        };
    }

    public Task<byte[]> ExportToJsonAsync(CodeSet codeSet, ExportOptions options)
    {
        var items = FilterItems(codeSet.Items, options);

        object exportData;
        if (options.IncludeMetadata)
        {
            exportData = new
            {
                codeSet.Id,
                codeSet.Name,
                codeSet.Description,
                codeSet.CodeField,
                codeSet.DisplayField,
                ExportedAt = DateTime.UtcNow,
                ItemCount = items.Count,
                Items = items
            };
        }
        else
        {
            exportData = items;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = options.PrettyPrint,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(exportData, jsonOptions);
        return Task.FromResult(Encoding.UTF8.GetBytes(json));
    }

    public Task<byte[]> ExportToCsvAsync(CodeSet codeSet, ExportOptions options)
    {
        var items = FilterItems(codeSet.Items, options);
        var sb = new StringBuilder();

        // Header
        if (options.IncludeHeader)
        {
            sb.AppendLine(string.Join(options.CsvDelimiter, options.IncludeFields));
        }

        // Data rows
        foreach (var item in items)
        {
            var values = options.IncludeFields.Select(field => field switch
            {
                "Code" => EscapeCsvValue(item.Code),
                "DisplayName" => EscapeCsvValue(item.DisplayName),
                "Description" => EscapeCsvValue(item.Description ?? ""),
                "Order" => item.Order.ToString(),
                "Status" => item.Status.ToString(),
                _ => ""
            });
            sb.AppendLine(string.Join(options.CsvDelimiter, values));
        }

        return Task.FromResult(Encoding.GetEncoding(options.Encoding).GetBytes(sb.ToString()));
    }

    public Task<byte[]> ExportToExcelAsync(CodeSet codeSet, ExportOptions options)
    {
        // Would use EPPlus library
        // using var package = new ExcelPackage();
        // var worksheet = package.Workbook.Worksheets.Add(codeSet.Name);
        // ...
        // return package.GetAsByteArray();

        throw new NotImplementedException("Excel export requires EPPlus library");
    }

    public Task<byte[]> ExportToXmlAsync(CodeSet codeSet, ExportOptions options)
    {
        var items = FilterItems(codeSet.Items, options);

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("CodeSet",
                new XAttribute("name", codeSet.Name),
                new XAttribute("exportedAt", DateTime.UtcNow.ToString("O")),
                new XElement("Items",
                    items.Select(item => new XElement("Item",
                        new XElement("Code", item.Code),
                        new XElement("DisplayName", item.DisplayName),
                        new XElement("Description", item.Description ?? ""),
                        new XElement("Order", item.Order),
                        new XElement("Status", item.Status.ToString())
                    ))
                )
            )
        );

        using var stream = new MemoryStream();
        doc.Save(stream);
        return Task.FromResult(stream.ToArray());
    }

    // Helper methods
    private List<CodeSetItem> FilterItems(List<CodeSetItem> items, ExportOptions options)
    {
        var filtered = items.AsEnumerable();

        if (!options.IncludeInactive)
            filtered = filtered.Where(i => i.Status != CodeSetItemStatus.Inactive);

        if (!options.IncludeDeprecated)
            filtered = filtered.Where(i => i.Status != CodeSetItemStatus.Deprecated);

        return filtered.OrderBy(i => i.Order).ToList();
    }

    private CodeSetItem MapRowToItem(Dictionary<string, object> row, FieldMapping mapping, int rowNumber)
    {
        return new CodeSetItem
        {
            Code = GetStringValue(row, mapping.CodeColumn) ?? $"ROW{rowNumber}",
            DisplayName = GetStringValue(row, mapping.DisplayNameColumn) ?? "",
            Description = GetStringValue(row, mapping.DescriptionColumn),
            Order = GetIntValue(row, mapping.OrderColumn) ?? rowNumber,
            Status = ParseStatus(GetStringValue(row, mapping.StatusColumn))
        };
    }

    private string? GetStringValue(Dictionary<string, object> row, string? column)
    {
        if (string.IsNullOrEmpty(column)) return null;
        return row.TryGetValue(column, out var value) ? value?.ToString() : null;
    }

    private int? GetIntValue(Dictionary<string, object> row, string? column)
    {
        var str = GetStringValue(row, column);
        return int.TryParse(str, out var val) ? val : null;
    }

    private CodeSetItemStatus ParseStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return CodeSetItemStatus.Active;
        return status.ToLowerInvariant() switch
        {
            "active" or "1" or "true" => CodeSetItemStatus.Active,
            "inactive" or "0" or "false" => CodeSetItemStatus.Inactive,
            "deprecated" => CodeSetItemStatus.Deprecated,
            _ => CodeSetItemStatus.Active
        };
    }

    private int FindColumnIndex(List<string> columns, string[] patterns)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            if (patterns.Any(p => columns[i].Contains(p)))
                return i;
        }
        return -1;
    }

    private List<string> ParseCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == delimiter && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result;
    }

    private string EscapeCsvValue(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    private object GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => element.GetRawText()
        };
    }
}
```

## Blazor Components

### ImportExportModal.razor

```razor
@namespace VisualEditorOpus.Components.CodeSet
@inject IImportExportService ImportExportService
@inject IJSRuntime JSRuntime

<div class="modal-backdrop @(IsOpen ? "show" : "")" @onclick="HandleBackdropClick">
    <div class="modal @(IsOpen ? "show" : "")" @onclick:stopPropagation>
        <div class="modal-header">
            <div class="modal-title">
                <i class="bi bi-@(Mode == ImportExportMode.Import ? "upload" : "download")"></i>
                @(Mode == ImportExportMode.Import ? "Import" : "Export") CodeSet
            </div>
            <button class="btn-close" @onclick="Close">
                <i class="bi bi-x-lg"></i>
            </button>
        </div>

        <div class="modal-body">
            <div class="ie-tabs">
                <button class="ie-tab @(Mode == ImportExportMode.Import ? "active" : "")"
                        @onclick="() => Mode = ImportExportMode.Import">
                    <i class="bi bi-upload"></i>
                    Import
                </button>
                <button class="ie-tab @(Mode == ImportExportMode.Export ? "active" : "")"
                        @onclick="() => Mode = ImportExportMode.Export">
                    <i class="bi bi-download"></i>
                    Export
                </button>
            </div>

            @if (Mode == ImportExportMode.Import)
            {
                <ImportPanel @ref="_importPanel"
                             OnFileSelected="HandleFileSelected"
                             OnImportComplete="HandleImportComplete" />
            }
            else
            {
                <ExportPanel CodeSet="CodeSet"
                             OnExport="HandleExport" />
            }
        </div>

        <div class="modal-footer">
            <button class="btn btn-secondary" @onclick="Close">Cancel</button>
            @if (Mode == ImportExportMode.Import)
            {
                <button class="btn btn-primary"
                        disabled="@(!CanImport)"
                        @onclick="StartImport">
                    <i class="bi bi-upload"></i>
                    Import
                </button>
            }
            else
            {
                <button class="btn btn-success" @onclick="StartExport">
                    <i class="bi bi-download"></i>
                    Download
                </button>
            }
        </div>
    </div>
</div>

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public CodeSet? CodeSet { get; set; }
    [Parameter] public ImportExportMode Mode { get; set; } = ImportExportMode.Import;
    [Parameter] public EventCallback<ImportResult> OnImportComplete { get; set; }

    private ImportPanel? _importPanel;
    private bool CanImport => _importPanel?.HasFile == true;

    private void HandleBackdropClick() => Close();

    private async Task Close()
    {
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
    }

    private void HandleFileSelected(ParsedData data)
    {
        StateHasChanged();
    }

    private async Task HandleImportComplete(ImportResult result)
    {
        await OnImportComplete.InvokeAsync(result);
        if (result.Success)
        {
            await Close();
        }
    }

    private async Task StartImport()
    {
        if (_importPanel != null)
        {
            await _importPanel.StartImportAsync();
        }
    }

    private async Task StartExport()
    {
        // Implementation would trigger file download
    }

    private async Task HandleExport(byte[] data, string fileName)
    {
        // Trigger download via JS interop
        await JSRuntime.InvokeVoidAsync("downloadFile", fileName, data);
    }
}

public enum ImportExportMode
{
    Import,
    Export
}
```

### FileDropZone.razor

```razor
@namespace VisualEditorOpus.Components.CodeSet
@inject IJSRuntime JSRuntime

<div class="drop-zone @(_isDragOver ? "dragover" : "")"
     @ondragover="HandleDragOver"
     @ondragover:preventDefault
     @ondragleave="HandleDragLeave"
     @ondrop="HandleDrop"
     @ondrop:preventDefault
     @onclick="TriggerFileInput">

    <input type="file"
           @ref="_fileInput"
           style="display: none;"
           accept="@AcceptedFormats"
           @onchange="HandleFileSelected" />

    <div class="drop-zone-icon">
        <i class="bi bi-cloud-upload"></i>
    </div>
    <div class="drop-zone-title">Drag & drop your file here</div>
    <div class="drop-zone-subtitle">or click to browse</div>
    <div class="drop-zone-formats">
        <span class="format-badge"><i class="bi bi-filetype-json"></i> JSON</span>
        <span class="format-badge"><i class="bi bi-filetype-csv"></i> CSV</span>
        <span class="format-badge"><i class="bi bi-file-earmark-excel"></i> Excel</span>
    </div>
</div>

@code {
    [Parameter] public string AcceptedFormats { get; set; } = ".json,.csv,.xlsx,.xls";
    [Parameter] public EventCallback<IBrowserFile> OnFileSelected { get; set; }

    private ElementReference _fileInput;
    private bool _isDragOver = false;

    private void HandleDragOver(DragEventArgs e)
    {
        _isDragOver = true;
    }

    private void HandleDragLeave(DragEventArgs e)
    {
        _isDragOver = false;
    }

    private async Task HandleDrop(DragEventArgs e)
    {
        _isDragOver = false;
        // File handling would be done via JS interop for drag-drop
    }

    private async Task TriggerFileInput()
    {
        await JSRuntime.InvokeVoidAsync("clickElement", _fileInput);
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        if (e.FileCount > 0)
        {
            await OnFileSelected.InvokeAsync(e.File);
        }
    }
}
```

## JavaScript Interop

```javascript
// wwwroot/js/importexport.js

window.clickElement = (element) => {
    element.click();
};

window.downloadFile = (fileName, data) => {
    const blob = new Blob([data], { type: 'application/octet-stream' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};
```

## Usage Example

```razor
@page "/codesets/{CodeSetId}"
@inject ICodeSetService CodeSetService

<button class="btn btn-secondary" @onclick="() => _showImportExport = true">
    <i class="bi bi-arrow-down-up"></i>
    Import/Export
</button>

<ImportExportModal @bind-IsOpen="_showImportExport"
                   CodeSet="CodeSet"
                   Mode="ImportExportMode.Export"
                   OnImportComplete="HandleImportComplete" />

@code {
    [Parameter] public string CodeSetId { get; set; } = "";
    private CodeSet? CodeSet { get; set; }
    private bool _showImportExport = false;

    protected override async Task OnInitializedAsync()
    {
        CodeSet = await CodeSetService.GetCodeSetByIdAsync(CodeSetId);
    }

    private async Task HandleImportComplete(ImportResult result)
    {
        if (result.Success)
        {
            // Reload data
            CodeSet = await CodeSetService.GetCodeSetByIdAsync(CodeSetId);
        }
    }
}
```

## Claude Prompt for Implementation

```
Implement the CodeSetImportExport system for importing and exporting CodeSet data.

Requirements:
1. Modal with Import/Export tabs
2. Drag-and-drop file upload zone
3. Support for JSON, CSV, and Excel formats
4. Data preview with first few rows
5. Auto-detect field mapping from column names
6. Import options: create new, merge, replace
7. Duplicate handling: skip, update, error
8. Export format selection with cards
9. Export options: metadata, inactive items, pretty print
10. Progress indicator during import
11. Success/error status messages
12. Dark mode support

Use the existing design system:
- CSS variables for colors (--primary: #6366f1, etc.)
- Modal with header/body/footer
- Toggle switches for boolean options
- Status badges for file format
- Bootstrap Icons for iconography

Parsers should handle edge cases like quoted CSV fields.
Use streaming for large file imports.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `CodeSetImportExport-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing modal open/close
- Import/Export tab switching testing
- File drop zone drag-and-drop testing
- File input click-to-browse testing
- JSON file parsing testing
- CSV file parsing testing
- Excel file parsing testing (if implemented)
- Field mapping auto-detection testing
- Manual field mapping override testing
- Import mode selection (Create/Merge/Replace) testing
- Duplicate handling option testing
- Import progress indicator testing
- Export format selection testing
- Export options configuration testing
- File download functionality testing

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Model files creation (ImportOptions, ExportOptions, FieldMapping, ImportResult, ParsedData)
- Enum files (ImportMode, DuplicateHandling, ExportFormat)
- IImportExportService interface and implementation
- Service registration in DI
- JavaScript interop file (importexport.js) registration
- CSS file imports
- Component registration in _Imports.razor
- EPPlus or ClosedXML NuGet package for Excel support
- File download JS function implementation

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens and closes correctly
- [ ] Import tab shows file drop zone
- [ ] Export tab shows format selection
- [ ] Drag and drop file works
- [ ] Click to browse file works
- [ ] JSON files parse correctly
- [ ] CSV files parse correctly
- [ ] Field mapping auto-detects columns
- [ ] Data preview shows first rows
- [ ] Import mode options work
- [ ] Duplicate handling options work
- [ ] Progress indicator updates during import
- [ ] Import result shows success/error counts
- [ ] Export format cards are selectable
- [ ] Export options toggles work
- [ ] Download triggers file save
- [ ] JSON export produces valid JSON
- [ ] CSV export produces valid CSV
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

## Integration Notes

1. **File Size Limits**: Consider chunked uploads for large files
2. **Excel Support**: Requires EPPlus or ClosedXML NuGet package
3. **Progress Tracking**: Use IProgress<T> for import progress updates
4. **Error Handling**: Collect all errors before failing import
5. **Validation**: Pre-validate data to catch issues early
