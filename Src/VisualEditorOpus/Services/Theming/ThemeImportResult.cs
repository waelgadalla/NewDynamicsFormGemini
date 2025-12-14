namespace VisualEditorOpus.Services.Theming;

using DynamicForms.Models.Theming;

/// <summary>
/// Result of a theme import operation.
/// </summary>
public sealed record ThemeImportResult
{
    /// <summary>
    /// Whether the import was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The imported theme if successful, null otherwise.
    /// </summary>
    public FormTheme? Theme { get; init; }

    /// <summary>
    /// List of validation errors that prevented import.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>
    /// List of warnings about the imported theme (e.g., deprecated properties, missing values).
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];

    /// <summary>
    /// Creates a successful import result.
    /// </summary>
    public static ThemeImportResult Succeeded(FormTheme theme, IReadOnlyList<string>? warnings = null) => new()
    {
        Success = true,
        Theme = theme,
        Warnings = warnings ?? []
    };

    /// <summary>
    /// Creates a failed import result.
    /// </summary>
    public static ThemeImportResult Failed(IReadOnlyList<string> errors, IReadOnlyList<string>? warnings = null) => new()
    {
        Success = false,
        Theme = null,
        Errors = errors,
        Warnings = warnings ?? []
    };

    /// <summary>
    /// Creates a failed import result with a single error message.
    /// </summary>
    public static ThemeImportResult Failed(string error) => new()
    {
        Success = false,
        Theme = null,
        Errors = [error]
    };
}
