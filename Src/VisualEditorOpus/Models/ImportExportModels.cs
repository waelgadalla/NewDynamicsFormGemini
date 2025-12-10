namespace VisualEditorOpus.Models;

/// <summary>
/// Options for exporting form schemas
/// </summary>
public record ExportOptions
{
    /// <summary>
    /// The export format to use
    /// </summary>
    public ExportFormat Format { get; init; } = ExportFormat.Json;

    /// <summary>
    /// Include metadata like created date, author, version
    /// </summary>
    public bool IncludeMetadata { get; init; } = true;

    /// <summary>
    /// Include internal field IDs
    /// </summary>
    public bool IncludeFieldIds { get; init; } = false;

    /// <summary>
    /// Format with indentation for readability
    /// </summary>
    public bool PrettyPrint { get; init; } = true;

    /// <summary>
    /// Remove whitespace for smaller file size
    /// </summary>
    public bool Minify { get; init; } = false;
}

/// <summary>
/// Available export formats
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Standard JSON format
    /// </summary>
    Json,

    /// <summary>
    /// JSON Schema format with validation rules
    /// </summary>
    JsonSchema,

    /// <summary>
    /// TypeScript type definitions (future)
    /// </summary>
    TypeScript
}

/// <summary>
/// Result of an export operation
/// </summary>
public record ExportResult
{
    /// <summary>
    /// Whether the export succeeded
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Generated filename
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Exported JSON content
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Size in bytes
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// Error message if export failed
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Options for importing form schemas
/// </summary>
public record ImportOptions
{
    /// <summary>
    /// How to handle the import
    /// </summary>
    public ImportMode Mode { get; init; } = ImportMode.Replace;

    /// <summary>
    /// Whether to preserve original IDs
    /// </summary>
    public bool PreserveIds { get; init; } = false;

    /// <summary>
    /// Name of the imported file
    /// </summary>
    public string? FileName { get; init; }
}

/// <summary>
/// Import modes
/// </summary>
public enum ImportMode
{
    /// <summary>
    /// Replace current form completely
    /// </summary>
    Replace,

    /// <summary>
    /// Merge fields with existing form
    /// </summary>
    Merge,

    /// <summary>
    /// Create as a new form module
    /// </summary>
    CreateNew
}

/// <summary>
/// Result of an import operation
/// </summary>
public record ImportResult
{
    /// <summary>
    /// Whether the import succeeded
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The imported module (if successful)
    /// </summary>
    public DynamicForms.Core.V4.Schemas.FormModuleSchema? Module { get; init; }

    /// <summary>
    /// Validation errors
    /// </summary>
    public List<ImportValidationError> Errors { get; init; } = new();

    /// <summary>
    /// Validation warnings
    /// </summary>
    public List<ImportValidationWarning> Warnings { get; init; } = new();

    /// <summary>
    /// Number of fields imported
    /// </summary>
    public int FieldCount { get; init; }
}

/// <summary>
/// Result of JSON validation
/// </summary>
public record ImportValidationResult
{
    /// <summary>
    /// Whether the JSON is valid
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Validation errors
    /// </summary>
    public List<ImportValidationError> Errors { get; init; } = new();

    /// <summary>
    /// Validation warnings
    /// </summary>
    public List<ImportValidationWarning> Warnings { get; init; } = new();
}

/// <summary>
/// A validation error
/// </summary>
public record ImportValidationError
{
    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; init; } = "";

    /// <summary>
    /// Line number where error occurred
    /// </summary>
    public int? Line { get; init; }

    /// <summary>
    /// Column number where error occurred
    /// </summary>
    public int? Column { get; init; }

    /// <summary>
    /// Severity of the error
    /// </summary>
    public ImportValidationSeverity Severity { get; init; } = ImportValidationSeverity.Error;
}

/// <summary>
/// A validation warning
/// </summary>
public record ImportValidationWarning
{
    /// <summary>
    /// Warning message
    /// </summary>
    public string Message { get; init; } = "";

    /// <summary>
    /// Severity of the warning
    /// </summary>
    public ImportValidationSeverity Severity { get; init; } = ImportValidationSeverity.Warning;
}

/// <summary>
/// Severity levels for validation messages
/// </summary>
public enum ImportValidationSeverity
{
    /// <summary>
    /// Informational message
    /// </summary>
    Info,

    /// <summary>
    /// Warning that doesn't prevent import
    /// </summary>
    Warning,

    /// <summary>
    /// Error that prevents import
    /// </summary>
    Error
}

/// <summary>
/// History item for import/export operations
/// </summary>
public record ImportExportHistoryItem
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Name of the file
    /// </summary>
    public string FileName { get; init; } = "";

    /// <summary>
    /// Type of operation
    /// </summary>
    public ImportExportOperationType Operation { get; init; }

    /// <summary>
    /// Size in bytes
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// When the operation occurred
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Whether the operation succeeded
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The exported content (for re-download)
    /// </summary>
    public string? Content { get; init; }
}

/// <summary>
/// Type of import/export operation
/// </summary>
public enum ImportExportOperationType
{
    /// <summary>
    /// Import operation
    /// </summary>
    Import,

    /// <summary>
    /// Export operation
    /// </summary>
    Export
}
