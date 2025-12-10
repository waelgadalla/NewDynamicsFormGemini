namespace VisualEditorOpus.Models;

/// <summary>
/// View modes available in the JSON Preview component
/// </summary>
public enum JsonViewMode
{
    /// <summary>
    /// Formatted view with syntax highlighting and indentation
    /// </summary>
    Formatted,

    /// <summary>
    /// Raw view showing minified JSON
    /// </summary>
    Raw,

    /// <summary>
    /// Diff view comparing original and current JSON
    /// </summary>
    Diff
}

/// <summary>
/// Statistics about the current JSON document
/// </summary>
public record JsonStats
{
    /// <summary>
    /// Size of the JSON in bytes
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// Number of lines in the formatted JSON
    /// </summary>
    public int Lines { get; init; }

    /// <summary>
    /// Number of fields in the schema
    /// </summary>
    public int FieldCount { get; init; }
}

/// <summary>
/// Represents a JSON validation error with location information
/// </summary>
public record JsonValidationError
{
    /// <summary>
    /// Error message describing the validation issue
    /// </summary>
    public string Message { get; init; } = "";

    /// <summary>
    /// Line number where the error occurred (1-based)
    /// </summary>
    public int Line { get; init; }

    /// <summary>
    /// Column number where the error occurred (1-based)
    /// </summary>
    public int Column { get; init; }
}

/// <summary>
/// Represents a search match in the JSON document
/// </summary>
public record SearchMatch
{
    /// <summary>
    /// Character index where the match starts
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Length of the matched text
    /// </summary>
    public int Length { get; init; }

    /// <summary>
    /// Line number where the match is located (1-based)
    /// </summary>
    public int Line { get; init; }

    /// <summary>
    /// Column number where the match starts (1-based)
    /// </summary>
    public int Column { get; init; }
}

/// <summary>
/// Represents a line in the diff view
/// </summary>
public record DiffLine
{
    /// <summary>
    /// Content from the original (left side)
    /// </summary>
    public string Content { get; init; } = "";

    /// <summary>
    /// Content from the current version (right side)
    /// </summary>
    public string NewContent { get; init; } = "";

    /// <summary>
    /// Type of change for this line
    /// </summary>
    public DiffLineType Type { get; init; }
}

/// <summary>
/// Types of changes in a diff view
/// </summary>
public enum DiffLineType
{
    /// <summary>
    /// Line is unchanged between versions
    /// </summary>
    Unchanged,

    /// <summary>
    /// Line was added in the current version
    /// </summary>
    Added,

    /// <summary>
    /// Line was removed from the original
    /// </summary>
    Removed,

    /// <summary>
    /// Line was modified between versions
    /// </summary>
    Modified
}
