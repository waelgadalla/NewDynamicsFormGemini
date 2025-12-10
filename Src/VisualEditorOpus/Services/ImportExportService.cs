using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Service for importing and exporting CodeSet data
/// </summary>
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

    #region Parsing

    public async Task<CodeSetParsedFileData> ParseFileAsync(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".json" => await ParseJsonAsync(stream, fileName),
            ".csv" => await ParseCsvAsync(stream, fileName),
            _ => throw new NotSupportedException($"File type {extension} is not supported. Supported formats: JSON, CSV")
        };
    }

    public async Task<CodeSetParsedFileData> ParseJsonAsync(Stream stream, string fileName)
    {
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Try to parse as array of objects
        List<Dictionary<string, JsonElement>>? items;
        try
        {
            items = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json, options);
        }
        catch (JsonException)
        {
            // Try to parse as object with Items array
            try
            {
                var wrapper = JsonSerializer.Deserialize<JsonElement>(json, options);
                if (wrapper.TryGetProperty("items", out var itemsElement) ||
                    wrapper.TryGetProperty("Items", out itemsElement))
                {
                    items = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(
                        itemsElement.GetRawText(), options);
                }
                else
                {
                    items = new List<Dictionary<string, JsonElement>>();
                }
            }
            catch
            {
                items = new List<Dictionary<string, JsonElement>>();
            }
        }

        items ??= new List<Dictionary<string, JsonElement>>();

        // Get columns from first item
        var columns = items.FirstOrDefault()?.Keys.ToList() ?? new List<string>();

        // Convert to generic dictionary
        var rows = items.Select(item => item.ToDictionary(
            kvp => kvp.Key,
            kvp => GetJsonValue(kvp.Value)
        )).ToList();

        return new CodeSetParsedFileData
        {
            FileName = fileName,
            FileSize = stream.Length,
            FileType = "JSON",
            Columns = columns,
            Rows = rows,
            TotalRows = rows.Count,
            DetectedMapping = DetectFieldMapping(columns)
        };
    }

    public async Task<CodeSetParsedFileData> ParseCsvAsync(Stream stream, string fileName, char delimiter = ',')
    {
        var rows = new List<Dictionary<string, object?>>();
        var columns = new List<string>();

        using var reader = new StreamReader(stream);

        // Read header
        var headerLine = await reader.ReadLineAsync();
        if (!string.IsNullOrEmpty(headerLine))
        {
            columns = ParseCsvLine(headerLine, delimiter);
        }

        // Read data rows
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line, delimiter);
            var row = new Dictionary<string, object?>();

            for (int i = 0; i < Math.Min(columns.Count, values.Count); i++)
            {
                row[columns[i]] = values[i];
            }

            rows.Add(row);
        }

        return new CodeSetParsedFileData
        {
            FileName = fileName,
            FileSize = stream.Length,
            FileType = "CSV",
            Columns = columns,
            Rows = rows,
            TotalRows = rows.Count,
            DetectedMapping = DetectFieldMapping(columns)
        };
    }

    #endregion

    #region Import

    public async Task<CodeSetImportResult> ImportAsync(
        CodeSetParsedFileData data,
        CodeSetImportOptions options,
        IProgress<CodeSetImportProgress>? progress = null)
    {
        var startTime = DateTime.UtcNow;
        var errors = new List<CodeSetImportError>();
        var warnings = new List<CodeSetImportWarning>();
        var importedCount = 0;
        var skippedCount = 0;

        _logger.LogInformation("Starting import of {RowCount} rows from {FileName}", data.TotalRows, data.FileName);

        // Validate first if requested
        if (options.ValidateBeforeImport)
        {
            var validationResult = await ValidateAsync(data, options);
            if (!validationResult.Success)
            {
                _logger.LogWarning("Import validation failed with {ErrorCount} errors", validationResult.Errors.Count);
                return validationResult;
            }
        }

        // Create or get target CodeSet
        ManagedCodeSet codeSet;
        if (options.Mode == CodeSetImportMode.CreateNew)
        {
            var newCodeSet = new ManagedCodeSet
            {
                Id = 0,
                Code = $"imported_{DateTime.UtcNow.Ticks}",
                NameEn = options.NewCodeSetName,
                NameFr = options.NewCodeSetName,
                Category = "imported",
                CodeField = options.Mapping.CodeColumn ?? "code",
                DisplayField = options.Mapping.DisplayNameColumn ?? "displayName"
            };
            codeSet = await _codeSetService.CreateCodeSetAsync(newCodeSet);
            _logger.LogInformation("Created new CodeSet {CodeSetId}: {CodeSetName}", codeSet.Id, codeSet.NameEn);
        }
        else
        {
            var existing = await _codeSetService.GetCodeSetByIdAsync(options.TargetCodeSetId!.Value);
            if (existing == null)
            {
                return new CodeSetImportResult
                {
                    Success = false,
                    Errors = new List<CodeSetImportError>
                    {
                        new CodeSetImportError { Message = "Target CodeSet not found" }
                    }
                };
            }
            codeSet = existing;

            if (options.Mode == CodeSetImportMode.Replace)
            {
                // Clear existing items
                var itemsToClear = await _codeSetService.GetItemsAsync(codeSet.Id);
                foreach (var item in itemsToClear)
                {
                    await _codeSetService.DeleteItemAsync(codeSet.Id, item.Id);
                }
                _logger.LogInformation("Cleared {ItemCount} existing items from CodeSet {CodeSetId}", itemsToClear.Count, codeSet.Id);
            }
        }

        // Get existing codes for duplicate detection
        var existingItems = await _codeSetService.GetItemsAsync(codeSet.Id);
        var existingCodes = new HashSet<string>(
            existingItems.Select(i => i.Code.ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase
        );

        // Import rows
        for (int i = 0; i < data.Rows.Count; i++)
        {
            var row = data.Rows[i];
            progress?.Report(new CodeSetImportProgress
            {
                Current = i + 1,
                Total = data.Rows.Count,
                Status = $"Importing row {i + 1} of {data.Rows.Count}"
            });

            try
            {
                // Skip empty rows if configured
                if (options.SkipEmptyRows && IsEmptyRow(row, options.Mapping))
                {
                    skippedCount++;
                    continue;
                }

                var item = MapRowToItem(row, options.Mapping, i + 1);

                if (options.TrimWhitespace)
                {
                    item = item with
                    {
                        Code = item.Code.Trim(),
                        DisplayNameEn = item.DisplayNameEn.Trim(),
                        DisplayNameFr = item.DisplayNameFr?.Trim(),
                        Description = item.Description?.Trim()
                    };
                }

                // Check for duplicates
                var codeLower = item.Code.ToLowerInvariant();
                if (existingCodes.Contains(codeLower))
                {
                    switch (options.DuplicateHandling)
                    {
                        case CodeSetDuplicateHandling.Skip:
                            skippedCount++;
                            warnings.Add(new CodeSetImportWarning
                            {
                                RowNumber = i + 2,
                                Message = $"Skipped duplicate code: {item.Code}"
                            });
                            continue;

                        case CodeSetDuplicateHandling.Update:
                            var existing = existingItems.FirstOrDefault(
                                x => x.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                item = item with { Id = existing.Id };
                                await _codeSetService.UpdateItemAsync(codeSet.Id, item);
                                importedCount++;
                            }
                            continue;

                        case CodeSetDuplicateHandling.Error:
                            errors.Add(new CodeSetImportError
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
                _logger.LogError(ex, "Error importing row {RowNumber}", i + 2);
                errors.Add(new CodeSetImportError
                {
                    RowNumber = i + 2,
                    Message = ex.Message
                });
            }
        }

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation(
            "Import completed: {ImportedCount} imported, {SkippedCount} skipped, {ErrorCount} errors in {Duration}ms",
            importedCount, skippedCount, errors.Count, duration.TotalMilliseconds);

        return new CodeSetImportResult
        {
            Success = errors.Count == 0,
            CodeSetId = codeSet.Id,
            CodeSetName = codeSet.NameEn,
            TotalRows = data.Rows.Count,
            ImportedCount = importedCount,
            SkippedCount = skippedCount,
            ErrorCount = errors.Count,
            Errors = errors,
            Warnings = warnings,
            Duration = duration
        };
    }

    public Task<CodeSetImportResult> ValidateAsync(CodeSetParsedFileData data, CodeSetImportOptions options)
    {
        var errors = new List<CodeSetImportError>();
        var warnings = new List<CodeSetImportWarning>();

        // Check required mapping
        if (string.IsNullOrEmpty(options.Mapping.CodeColumn))
        {
            errors.Add(new CodeSetImportError { Message = "Code column mapping is required" });
        }

        if (string.IsNullOrEmpty(options.Mapping.DisplayNameColumn))
        {
            warnings.Add(new CodeSetImportWarning { Message = "Display Name column mapping not set, using Code as display name" });
        }

        // Validate each row (limit to first 1000 for performance)
        var rowsToValidate = Math.Min(data.Rows.Count, 1000);
        for (int i = 0; i < rowsToValidate; i++)
        {
            var row = data.Rows[i];

            // Check Code field
            if (!string.IsNullOrEmpty(options.Mapping.CodeColumn))
            {
                var code = GetStringValue(row, options.Mapping.CodeColumn);
                if (string.IsNullOrWhiteSpace(code))
                {
                    errors.Add(new CodeSetImportError
                    {
                        RowNumber = i + 2,
                        Column = options.Mapping.CodeColumn,
                        Message = "Code is required"
                    });
                }
            }
        }

        if (data.Rows.Count > 1000 && errors.Count == 0)
        {
            warnings.Add(new CodeSetImportWarning
            {
                Message = $"Only first 1000 of {data.Rows.Count} rows were validated"
            });
        }

        return Task.FromResult(new CodeSetImportResult
        {
            Success = errors.Count == 0,
            TotalRows = data.Rows.Count,
            Errors = errors,
            Warnings = warnings
        });
    }

    #endregion

    #region Export

    public async Task<byte[]> ExportAsync(int codeSetId, CodeSetExportOptions options)
    {
        var codeSet = await _codeSetService.GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null)
        {
            throw new InvalidOperationException($"CodeSet {codeSetId} not found");
        }

        // Load items
        var items = await _codeSetService.GetItemsAsync(codeSetId);
        codeSet = codeSet with { Items = items };

        _logger.LogInformation("Exporting CodeSet {CodeSetId} to {Format} with {ItemCount} items",
            codeSetId, options.Format, items.Count);

        return options.Format switch
        {
            CodeSetExportFormat.Json => await ExportToJsonAsync(codeSet, options),
            CodeSetExportFormat.Csv => await ExportToCsvAsync(codeSet, options),
            CodeSetExportFormat.Xml => await ExportToXmlAsync(codeSet, options),
            _ => throw new NotSupportedException($"Export format {options.Format} is not supported")
        };
    }

    public Task<byte[]> ExportToJsonAsync(ManagedCodeSet codeSet, CodeSetExportOptions options)
    {
        var items = FilterItems(codeSet.Items, options);

        object exportData;
        if (options.IncludeMetadata)
        {
            exportData = new
            {
                codeSet.Id,
                Name = codeSet.NameEn,
                NameFr = codeSet.NameFr,
                Description = codeSet.DescriptionEn,
                codeSet.Category,
                codeSet.CodeField,
                codeSet.DisplayField,
                ExportedAt = DateTime.UtcNow,
                ItemCount = items.Count,
                Items = items.Select(i => new
                {
                    i.Code,
                    DisplayName = i.DisplayNameEn,
                    DisplayNameFr = i.DisplayNameFr,
                    i.Description,
                    i.Order,
                    Status = i.Status.ToString(),
                    i.ParentCode
                })
            };
        }
        else
        {
            exportData = items.Select(i => new
            {
                i.Code,
                DisplayName = i.DisplayNameEn,
                DisplayNameFr = i.DisplayNameFr,
                i.Description,
                i.Order,
                Status = i.Status.ToString()
            });
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = options.PrettyPrint,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(exportData, jsonOptions);
        return Task.FromResult(Encoding.UTF8.GetBytes(json));
    }

    public Task<byte[]> ExportToCsvAsync(ManagedCodeSet codeSet, CodeSetExportOptions options)
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
                "Code" => EscapeCsvValue(item.Code, options.CsvDelimiter),
                "DisplayName" => EscapeCsvValue(item.DisplayNameEn, options.CsvDelimiter),
                "DisplayNameFr" => EscapeCsvValue(item.DisplayNameFr ?? "", options.CsvDelimiter),
                "Description" => EscapeCsvValue(item.Description ?? "", options.CsvDelimiter),
                "Order" => item.Order.ToString(),
                "Status" => item.Status.ToString(),
                "ParentCode" => EscapeCsvValue(item.ParentCode ?? "", options.CsvDelimiter),
                _ => ""
            });
            sb.AppendLine(string.Join(options.CsvDelimiter, values));
        }

        return Task.FromResult(Encoding.GetEncoding(options.Encoding).GetBytes(sb.ToString()));
    }

    public Task<byte[]> ExportToXmlAsync(ManagedCodeSet codeSet, CodeSetExportOptions options)
    {
        var items = FilterItems(codeSet.Items, options);

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("CodeSet",
                new XAttribute("id", codeSet.Id),
                new XAttribute("name", codeSet.NameEn),
                new XAttribute("exportedAt", DateTime.UtcNow.ToString("O")),
                options.IncludeMetadata ? new object[]
                {
                    new XElement("Metadata",
                        new XElement("NameFr", codeSet.NameFr ?? ""),
                        new XElement("Description", codeSet.DescriptionEn ?? ""),
                        new XElement("Category", codeSet.Category ?? ""),
                        new XElement("CodeField", codeSet.CodeField),
                        new XElement("DisplayField", codeSet.DisplayField)
                    )
                } : Array.Empty<object>(),
                new XElement("Items",
                    items.Select(item => new XElement("Item",
                        new XElement("Code", item.Code),
                        new XElement("DisplayName", item.DisplayNameEn),
                        new XElement("DisplayNameFr", item.DisplayNameFr ?? ""),
                        new XElement("Description", item.Description ?? ""),
                        new XElement("Order", item.Order),
                        new XElement("Status", item.Status.ToString()),
                        new XElement("ParentCode", item.ParentCode ?? "")
                    ))
                )
            )
        );

        using var stream = new MemoryStream();
        doc.Save(stream);
        return Task.FromResult(stream.ToArray());
    }

    #endregion

    #region Field Detection

    public CodeSetFieldMapping DetectFieldMapping(List<string> columns)
    {
        var mapping = new CodeSetFieldMapping { AutoDetect = true };
        var lowerColumns = columns.Select(c => c.ToLowerInvariant()).ToList();

        // Detect Code field
        var codePatterns = new[] { "code", "id", "key", "value" };
        var codeIndex = FindColumnIndex(lowerColumns, codePatterns);
        if (codeIndex >= 0)
            mapping = mapping with { CodeColumn = columns[codeIndex] };

        // Detect DisplayName field
        var namePatterns = new[] { "name", "displayname", "display_name", "label", "text", "title", "displaynameen" };
        var nameIndex = FindColumnIndex(lowerColumns, namePatterns);
        if (nameIndex >= 0)
            mapping = mapping with { DisplayNameColumn = columns[nameIndex] };

        // Detect French DisplayName field
        var nameFrPatterns = new[] { "namefr", "displaynamefr", "display_name_fr", "labelfr", "nom" };
        var nameFrIndex = FindColumnIndex(lowerColumns, nameFrPatterns);
        if (nameFrIndex >= 0)
            mapping = mapping with { DisplayNameFrColumn = columns[nameFrIndex] };

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

        // Detect ParentCode field
        var parentPatterns = new[] { "parentcode", "parent_code", "parent", "parentid" };
        var parentIndex = FindColumnIndex(lowerColumns, parentPatterns);
        if (parentIndex >= 0)
            mapping = mapping with { ParentCodeColumn = columns[parentIndex] };

        _logger.LogDebug("Detected field mapping: Code={Code}, DisplayName={DisplayName}, Description={Description}",
            mapping.CodeColumn, mapping.DisplayNameColumn, mapping.DescriptionColumn);

        return mapping;
    }

    #endregion

    #region Helper Methods

    private List<ManagedCodeSetItem> FilterItems(List<ManagedCodeSetItem> items, CodeSetExportOptions options)
    {
        var filtered = items.AsEnumerable();

        if (!options.IncludeInactive)
            filtered = filtered.Where(i => i.Status != CodeSetItemStatus.Inactive);

        if (!options.IncludeDeprecated)
            filtered = filtered.Where(i => i.Status != CodeSetItemStatus.Deprecated);

        return filtered.OrderBy(i => i.Order).ToList();
    }

    private ManagedCodeSetItem MapRowToItem(Dictionary<string, object?> row, CodeSetFieldMapping mapping, int rowNumber)
    {
        var code = GetStringValue(row, mapping.CodeColumn) ?? $"ROW{rowNumber}";
        var displayName = GetStringValue(row, mapping.DisplayNameColumn) ?? code;

        return new ManagedCodeSetItem
        {
            Code = code,
            DisplayNameEn = displayName,
            DisplayNameFr = GetStringValue(row, mapping.DisplayNameFrColumn),
            Description = GetStringValue(row, mapping.DescriptionColumn),
            Order = GetIntValue(row, mapping.OrderColumn) ?? rowNumber,
            Status = ParseStatus(GetStringValue(row, mapping.StatusColumn)),
            ParentCode = GetStringValue(row, mapping.ParentCodeColumn),
            IsVisible = true
        };
    }

    private bool IsEmptyRow(Dictionary<string, object?> row, CodeSetFieldMapping mapping)
    {
        var code = GetStringValue(row, mapping.CodeColumn);
        var name = GetStringValue(row, mapping.DisplayNameColumn);
        return string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name);
    }

    private static string? GetStringValue(Dictionary<string, object?> row, string? column)
    {
        if (string.IsNullOrEmpty(column)) return null;
        return row.TryGetValue(column, out var value) ? value?.ToString() : null;
    }

    private static int? GetIntValue(Dictionary<string, object?> row, string? column)
    {
        var str = GetStringValue(row, column);
        return int.TryParse(str, out var val) ? val : null;
    }

    private static CodeSetItemStatus ParseStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return CodeSetItemStatus.Active;
        return status.ToLowerInvariant() switch
        {
            "active" or "1" or "true" or "yes" => CodeSetItemStatus.Active,
            "inactive" or "0" or "false" or "no" => CodeSetItemStatus.Inactive,
            "deprecated" => CodeSetItemStatus.Deprecated,
            _ => CodeSetItemStatus.Active
        };
    }

    private static int FindColumnIndex(List<string> columns, string[] patterns)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            if (patterns.Any(p => columns[i].Contains(p, StringComparison.OrdinalIgnoreCase)))
                return i;
        }
        return -1;
    }

    private static List<string> ParseCsvLine(string line, char delimiter)
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

    private static string EscapeCsvValue(string value, char delimiter)
    {
        if (value.Contains(delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    private static object? GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    #endregion
}
