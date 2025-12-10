namespace VisualEditorOpus.Models;

#region Import Options and Field Mapping

/// <summary>
/// Options for importing CodeSet data
/// </summary>
public record CodeSetImportOptions
{
    /// <summary>
    /// How to handle the imported data
    /// </summary>
    public CodeSetImportMode Mode { get; init; } = CodeSetImportMode.CreateNew;

    /// <summary>
    /// Target CodeSet ID for merge/replace operations
    /// </summary>
    public int? TargetCodeSetId { get; init; }

    /// <summary>
    /// Name for new CodeSet
    /// </summary>
    public string NewCodeSetName { get; init; } = "";

    /// <summary>
    /// Field mapping configuration
    /// </summary>
    public CodeSetFieldMapping Mapping { get; init; } = new();

    /// <summary>
    /// How to handle duplicate codes
    /// </summary>
    public CodeSetDuplicateHandling DuplicateHandling { get; init; } = CodeSetDuplicateHandling.Skip;

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

/// <summary>
/// Maps source file columns to CodeSet fields
/// </summary>
public record CodeSetFieldMapping
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
    /// Source column for French DisplayName field
    /// </summary>
    public string? DisplayNameFrColumn { get; init; }

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

#endregion

#region Export Options

/// <summary>
/// Options for exporting CodeSet data
/// </summary>
public record CodeSetExportOptions
{
    /// <summary>
    /// Export file format
    /// </summary>
    public CodeSetExportFormat Format { get; init; } = CodeSetExportFormat.Json;

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

#endregion

#region Import Result and Status

/// <summary>
/// Result of a CodeSet import operation
/// </summary>
public record CodeSetImportResult
{
    public bool Success { get; init; }
    public int? CodeSetId { get; init; }
    public string? CodeSetName { get; init; }
    public int TotalRows { get; init; }
    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public int ErrorCount { get; init; }
    public List<CodeSetImportError> Errors { get; init; } = new();
    public List<CodeSetImportWarning> Warnings { get; init; } = new();
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Error during CodeSet import
/// </summary>
public record CodeSetImportError
{
    public int RowNumber { get; init; }
    public string Column { get; init; } = "";
    public string Value { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>
/// Warning during CodeSet import
/// </summary>
public record CodeSetImportWarning
{
    public int RowNumber { get; init; }
    public string Message { get; init; } = "";
}

/// <summary>
/// Represents parsed file data before CodeSet import
/// </summary>
public record CodeSetParsedFileData
{
    public string FileName { get; init; } = "";
    public long FileSize { get; init; }
    public string FileType { get; init; } = "";
    public List<string> Columns { get; init; } = new();
    public List<Dictionary<string, object?>> Rows { get; init; } = new();
    public int TotalRows { get; init; }
    public CodeSetFieldMapping DetectedMapping { get; init; } = new();
}

/// <summary>
/// Progress during CodeSet import
/// </summary>
public record CodeSetImportProgress
{
    public int Current { get; init; }
    public int Total { get; init; }
    public string Status { get; init; } = "";
    public double Percentage => Total > 0 ? (double)Current / Total * 100 : 0;
}

#endregion

#region Enums

/// <summary>
/// Import mode for CodeSet import
/// </summary>
public enum CodeSetImportMode
{
    CreateNew,      // Create a new CodeSet
    MergeWith,      // Add to existing CodeSet
    Replace         // Replace existing CodeSet data
}

/// <summary>
/// How to handle duplicate codes during import
/// </summary>
public enum CodeSetDuplicateHandling
{
    Skip,           // Skip duplicate codes
    Update,         // Update existing with new values
    Error           // Fail on duplicates
}

/// <summary>
/// Export file format for CodeSets
/// </summary>
public enum CodeSetExportFormat
{
    Json,
    Csv,
    Excel,
    Xml
}

/// <summary>
/// Mode for the CodeSet Import/Export modal
/// </summary>
public enum CodeSetImportExportMode
{
    Import,
    Export
}

#endregion
