using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DynamicForms.Core.V4.Schemas;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Implementation of the JSON import/export service
/// </summary>
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
            var size = Encoding.UTF8.GetByteCount(json);

            // Add to history
            _history.Insert(0, new ImportExportHistoryItem
            {
                Id = Guid.NewGuid().ToString(),
                FileName = fileName,
                Operation = ImportExportOperationType.Export,
                Size = size,
                Timestamp = DateTime.UtcNow,
                Success = true,
                Content = json
            });

            // Keep only last 20 items
            if (_history.Count > 20)
            {
                _history.RemoveRange(20, _history.Count - 20);
            }

            return new ExportResult
            {
                Success = true,
                FileName = fileName,
                Content = json,
                Size = size
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
                ["dateCreated"] = module.DateCreated,
                ["dateUpdated"] = module.DateUpdated,
                ["createdBy"] = module.CreatedBy
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
            ["fieldType"] = field.FieldType,
            ["labelEn"] = field.LabelEn,
            ["labelFr"] = field.LabelFr,
            ["placeholderEn"] = field.PlaceholderEn,
            ["placeholderFr"] = field.PlaceholderFr,
            ["helpEn"] = field.HelpEn,
            ["helpFr"] = field.HelpFr,
            ["descriptionEn"] = field.DescriptionEn,
            ["descriptionFr"] = field.DescriptionFr,
            ["isVisible"] = field.IsVisible,
            ["isReadOnly"] = field.IsReadOnly,
            ["order"] = field.Order
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
                ["order"] = o.Order
            }).ToList();
        }

        if (field.Validation != null)
        {
            fieldExport["validation"] = new Dictionary<string, object?>
            {
                ["isRequired"] = field.Validation.IsRequired,
                ["minLength"] = field.Validation.MinLength,
                ["maxLength"] = field.Validation.MaxLength,
                ["pattern"] = field.Validation.Pattern,
                ["requiredMessageEn"] = field.Validation.RequiredMessageEn,
                ["requiredMessageFr"] = field.Validation.RequiredMessageFr,
                ["patternMessageEn"] = field.Validation.PatternMessageEn,
                ["patternMessageFr"] = field.Validation.PatternMessageFr,
                ["minValue"] = field.Validation.MinValue,
                ["maxValue"] = field.Validation.MaxValue
            };
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

        if (field.ConditionalRules?.Any() == true)
        {
            fieldExport["conditionalRules"] = field.ConditionalRules;
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

            if (field.Validation?.IsRequired == true)
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
            ["description"] = field.HelpEn
        };

        switch (field.FieldType?.ToLower())
        {
            case "textbox":
            case "textarea":
            case "email":
            case "phone":
            case "richtext":
                schema["type"] = "string";
                if (field.FieldType?.ToLower() == "email")
                    schema["format"] = "email";
                if (field.Validation?.MaxLength.HasValue == true)
                    schema["maxLength"] = field.Validation.MaxLength;
                if (field.Validation?.MinLength.HasValue == true)
                    schema["minLength"] = field.Validation.MinLength;
                break;

            case "number":
            case "currency":
                schema["type"] = "number";
                break;

            case "date":
            case "datepicker":
                schema["type"] = "string";
                schema["format"] = "date";
                break;

            case "datetime":
                schema["type"] = "string";
                schema["format"] = "date-time";
                break;

            case "time":
                schema["type"] = "string";
                schema["format"] = "time";
                break;

            case "toggle":
            case "checkbox":
                schema["type"] = "boolean";
                break;

            case "dropdown":
            case "select":
            case "radio":
                schema["type"] = "string";
                if (field.Options?.Any() == true)
                {
                    schema["enum"] = field.Options.Select(o => o.Value).ToList();
                }
                break;

            case "checkboxlist":
            case "multiselect":
                schema["type"] = "array";
                schema["items"] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["enum"] = field.Options?.Select(o => o.Value).ToList() ?? new List<string?>()
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
        var baseName = !string.IsNullOrEmpty(module.TitleEn)
            ? module.TitleEn.ToLower().Replace(" ", "-")
            : "form-schema";

        // Remove invalid characters
        baseName = string.Join("", baseName.Split(Path.GetInvalidFileNameChars()));

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
                Operation = ImportExportOperationType.Import,
                Size = Encoding.UTF8.GetByteCount(jsonContent),
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
                Operation = ImportExportOperationType.Import,
                Size = Encoding.UTF8.GetByteCount(jsonContent),
                Timestamp = DateTime.UtcNow,
                Success = true
            });

            // Keep only last 20 items
            if (_history.Count > 20)
            {
                _history.RemoveRange(20, _history.Count - 20);
            }

            return new ImportResult
            {
                Success = true,
                Module = module,
                Warnings = validation.Warnings,
                FieldCount = module.Fields?.Length ?? 0
            };
        }
        catch (Exception ex)
        {
            return new ImportResult
            {
                Success = false,
                Errors = new List<ImportValidationError>
                {
                    new() { Message = ex.Message, Severity = ImportValidationSeverity.Error }
                }
            };
        }
    }

    public Task<ImportValidationResult> ValidateAsync(string jsonContent)
    {
        var errors = new List<ImportValidationError>();
        var warnings = new List<ImportValidationWarning>();

        // Check JSON syntax
        try
        {
            var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            // Check for required properties
            if (!root.TryGetProperty("fields", out _) && !root.TryGetProperty("Fields", out _))
            {
                warnings.Add(new ImportValidationWarning
                {
                    Message = "No 'fields' property found. Form will have no fields.",
                    Severity = ImportValidationSeverity.Warning
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
                    warnings.Add(new ImportValidationWarning
                    {
                        Message = "Schema version not recognized. Import may have compatibility issues.",
                        Severity = ImportValidationSeverity.Warning
                    });
                }
            }

            // Check for fields with missing translations
            if (root.TryGetProperty("fields", out var fields) ||
                root.TryGetProperty("Fields", out fields))
            {
                var fieldCount = 0;
                var missingFrenchCount = 0;

                foreach (var field in fields.EnumerateArray())
                {
                    fieldCount++;
                    if (!field.TryGetProperty("labelFr", out var labelFr) ||
                        string.IsNullOrEmpty(labelFr.GetString()))
                    {
                        missingFrenchCount++;
                    }
                }

                if (missingFrenchCount > 0)
                {
                    warnings.Add(new ImportValidationWarning
                    {
                        Message = $"{missingFrenchCount} field(s) have missing French translations.",
                        Severity = ImportValidationSeverity.Warning
                    });
                }

                // Add info about field count
                warnings.Add(new ImportValidationWarning
                {
                    Message = $"Contains {fieldCount} field(s).",
                    Severity = ImportValidationSeverity.Info
                });
            }
        }
        catch (JsonException ex)
        {
            errors.Add(new ImportValidationError
            {
                Message = $"Invalid JSON: {ex.Message}",
                Line = (int?)ex.LineNumber,
                Column = ex.BytePositionInLine?.ToString(),
                Severity = ImportValidationSeverity.Error
            });
        }

        return Task.FromResult(new ImportValidationResult
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

        // Try to parse directly first
        FormModuleSchema? module = null;

        try
        {
            module = JsonSerializer.Deserialize<FormModuleSchema>(
                element.GetRawText(), moduleOptions);
        }
        catch
        {
            // If direct parsing fails, manually construct
        }

        if (module == null)
        {
            // Manual parsing for non-standard formats
            var id = element.TryGetProperty("id", out var idProp)
                ? idProp.GetInt32()
                : 0;

            var titleEn = element.TryGetProperty("titleEn", out var titleProp)
                ? titleProp.GetString() ?? "Imported Form"
                : "Imported Form";

            module = new FormModuleSchema
            {
                Id = id,
                TitleEn = titleEn,
                TitleFr = element.TryGetProperty("titleFr", out var titleFrProp)
                    ? titleFrProp.GetString()
                    : null,
                DescriptionEn = element.TryGetProperty("descriptionEn", out var descProp)
                    ? descProp.GetString()
                    : null,
                Fields = ParseFields(element, options)
            };
        }

        // Apply import mode
        if (options.Mode == ImportMode.CreateNew)
        {
            module = module with
            {
                TitleEn = module.TitleEn + " (Imported)",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };
        }

        return module;
    }

    private FormFieldSchema[] ParseFields(JsonElement element, ImportOptions options)
    {
        var fields = new List<FormFieldSchema>();

        if (element.TryGetProperty("fields", out var fieldsElement) ||
            element.TryGetProperty("Fields", out fieldsElement))
        {
            var moduleOptions = new JsonSerializerOptions(_serializerOptions)
            {
                PropertyNameCaseInsensitive = true
            };

            foreach (var fieldElement in fieldsElement.EnumerateArray())
            {
                try
                {
                    var field = JsonSerializer.Deserialize<FormFieldSchema>(
                        fieldElement.GetRawText(), moduleOptions);

                    if (field != null)
                    {
                        // Generate new ID if not preserving
                        if (!options.PreserveIds)
                        {
                            field = field with { Id = Guid.NewGuid().ToString() };
                        }
                        fields.Add(field);
                    }
                }
                catch
                {
                    // Skip invalid fields
                }
            }
        }

        return fields.ToArray();
    }

    #endregion

    #region History

    public IReadOnlyList<ImportExportHistoryItem> GetHistory() => _history.AsReadOnly();

    public void ClearHistory() => _history.Clear();

    #endregion
}
