using DynamicForms.Models.Theming;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Service for importing and exporting themes in various formats.
/// </summary>
public interface IThemeImportExportService
{
    /// <summary>
    /// Export a theme to JSON string.
    /// </summary>
    /// <param name="theme">The theme to export.</param>
    /// <param name="prettyPrint">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the theme.</returns>
    string ExportToJson(FormTheme theme, bool prettyPrint = true);

    /// <summary>
    /// Export a theme to JSON bytes for file download.
    /// </summary>
    /// <param name="theme">The theme to export.</param>
    /// <returns>UTF-8 encoded JSON bytes.</returns>
    byte[] ExportToJsonBytes(FormTheme theme);

    /// <summary>
    /// Export a theme to a complete CSS stylesheet.
    /// </summary>
    /// <param name="theme">The theme to export.</param>
    /// <returns>Complete CSS stylesheet with variables and component styles.</returns>
    string ExportToCss(FormTheme theme);

    /// <summary>
    /// Export a theme to CSS bytes for file download.
    /// </summary>
    /// <param name="theme">The theme to export.</param>
    /// <returns>UTF-8 encoded CSS bytes.</returns>
    byte[] ExportToCssBytes(FormTheme theme);

    /// <summary>
    /// Export minified CSS bytes for production use.
    /// </summary>
    /// <param name="theme">The theme to export.</param>
    /// <returns>UTF-8 encoded minified CSS bytes.</returns>
    byte[] ExportToMinifiedCssBytes(FormTheme theme);

    /// <summary>
    /// Import a theme from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>Import result containing the theme or errors.</returns>
    ThemeImportResult ImportFromJson(string json);

    /// <summary>
    /// Import a theme from a stream (e.g., file upload).
    /// </summary>
    /// <param name="stream">The stream containing JSON data.</param>
    /// <returns>Import result containing the theme or errors.</returns>
    Task<ThemeImportResult> ImportFromJsonAsync(Stream stream);

    /// <summary>
    /// Get CSS variables formatted for clipboard copy.
    /// </summary>
    /// <param name="theme">The theme to get variables from.</param>
    /// <returns>CSS variables formatted for use in a :root block.</returns>
    string GetCssVariablesForClipboard(FormTheme theme);

    /// <summary>
    /// Get the suggested filename for exporting a theme.
    /// </summary>
    /// <param name="theme">The theme to get filename for.</param>
    /// <param name="extension">File extension (json, css).</param>
    /// <returns>Sanitized filename with extension.</returns>
    string GetExportFilename(FormTheme theme, string extension);

    /// <summary>
    /// Validate a theme without importing it.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    /// <returns>List of validation errors, empty if valid.</returns>
    IReadOnlyList<string> ValidateJson(string json);
}
