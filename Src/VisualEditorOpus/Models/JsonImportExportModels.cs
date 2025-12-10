using DynamicForms.Core.V4.Schemas;

namespace VisualEditorOpus.Models;

#region Export Models

/// <summary>
/// Options for exporting JSON form schemas
/// </summary>
public record ExportOptions
{
    /// <summary>
    /// Export format
    /// </summary>
    public ExportFormat Format { get; init; } = ExportFormat.Json;

    /// <summary>
    /// Pretty print the JSON output
    /// </summary>
    public bool PrettyPrint { get; init; } = true;

    /// <summary>
    /// Minify the JSON output (overrides PrettyPrint)
    /// </summary>
    public bool Minify { get; init; } = false;

    /// <summary>
    /// Include field IDs in the export
    /// </summary>
    public bool IncludeFieldIds { get; init; } = true;

    /// <summary>
    /// Include metadata in the export
    /// </summary>
    public bool IncludeMetadata { get; init; } = true;

    /// <summary>
    /// Include validation rules
    /// </summary>
    public bool IncludeValidation { get; init; } = true;
}

/// <summary>
/// Export format options
/// </summary>
public enum ExportFormat
{
    Json,
    JsonSchema,
    TypeScript
}

/// <summary>
/// Result of a JSON form export operation
/// </summary>
public record ExportResult
{
    public bool Success { get; init; }
    public string? Content { get; init; }
    public string? FileName { get; init; }
    public string? Error { get; init; }
    public string? ErrorMessage { get; init; }
    public long Size { get; init; }
    public DateTime ExportedAt { get; init; } = DateTime.UtcNow;
}

#endregion

#region Import Models

/// <summary>
/// Options for importing JSON form schemas
/// </summary>
public record ImportOptions
{
    /// <summary>
    /// Import mode
    /// </summary>
    public ImportMode Mode { get; init; } = ImportMode.CreateNew;

    /// <summary>
    /// File name being imported
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Validate schema during import
    /// </summary>
    public bool ValidateSchema { get; init; } = true;

    /// <summary>
    /// Generate new IDs for imported fields
    /// </summary>
    public bool GenerateNewIds { get; init; } = false;

    /// <summary>
    /// Preserve field IDs from source
    /// </summary>
    public bool PreserveFieldIds { get; init; } = true;

    /// <summary>
    /// Preserve field IDs from source (alias for PreserveFieldIds)
    /// </summary>
    public bool PreserveIds { get; init; } = true;
}

/// <summary>
/// Import mode options
/// </summary>
public enum ImportMode
{
    CreateNew,      // Create a new form module
    Replace,        // Replace existing form module
    Merge           // Merge with existing form module
}

/// <summary>
/// Result of a JSON form import operation
/// </summary>
public record ImportResult
{
    public bool Success { get; init; }
    public FormModuleSchema? Module { get; init; }
    public string? Error { get; init; }
    public string? ErrorMessage { get; init; }
    public int FieldCount { get; init; }
    public List<ImportValidationWarning> Warnings { get; init; } = new();
    public List<ImportValidationError> Errors { get; init; } = new();
    public DateTime ImportedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a JSON form import validation
/// </summary>
public record ImportValidationResult
{
    public bool IsValid { get; init; }
    public List<ImportValidationIssue> Issues { get; init; } = new();
    public List<ImportValidationError> Errors { get; init; } = new();
    public List<ImportValidationWarning> Warnings { get; init; } = new();
    public FormModuleSchema? ParsedModule { get; init; }
}

/// <summary>
/// Issue found during JSON import validation
/// </summary>
public record ImportValidationIssue
{
    public ImportValidationSeverity Severity { get; init; }
    public string Message { get; init; } = "";
    public string? Path { get; init; }
    public int? Line { get; init; }
}

/// <summary>
/// Error found during JSON import validation
/// </summary>
public record ImportValidationError
{
    public ImportValidationSeverity Severity { get; init; } = ImportValidationSeverity.Error;
    public string Message { get; init; } = "";
    public string? Path { get; init; }
    public string? Column { get; init; }
    public int? Line { get; init; }
}

/// <summary>
/// Warning found during JSON import validation
/// </summary>
public record ImportValidationWarning
{
    public ImportValidationSeverity Severity { get; init; } = ImportValidationSeverity.Warning;
    public string Message { get; init; } = "";
    public string? Path { get; init; }
    public int? Line { get; init; }
}

/// <summary>
/// Severity of an import validation issue
/// </summary>
public enum ImportValidationSeverity
{
    Info,
    Warning,
    Error
}

#endregion

#region History Models

/// <summary>
/// History item for import/export operations
/// </summary>
public record ImportExportHistoryItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public ImportExportOperationType OperationType { get; init; }
    public ImportExportOperationType Operation { get; init; }
    public string? FileName { get; init; }
    public string? ModuleName { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? ErrorMessage { get; init; }
    public long Size { get; init; }
    public string? Content { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Type of import/export operation
/// </summary>
public enum ImportExportOperationType
{
    Import,
    Export
}

#endregion
