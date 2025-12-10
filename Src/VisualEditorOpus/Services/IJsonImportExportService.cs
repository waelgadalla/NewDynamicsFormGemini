using DynamicForms.Core.V4.Schemas;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Service for importing and exporting form schemas as JSON
/// </summary>
public interface IJsonImportExportService
{
    /// <summary>
    /// Export a form module to JSON
    /// </summary>
    /// <param name="module">The module to export</param>
    /// <param name="options">Export options</param>
    /// <returns>Export result with content and filename</returns>
    Task<ExportResult> ExportAsync(FormModuleSchema module, ExportOptions options);

    /// <summary>
    /// Serialize a form module to JSON string
    /// </summary>
    /// <param name="module">The module to serialize</param>
    /// <param name="options">Export options</param>
    /// <returns>JSON string</returns>
    Task<string> SerializeAsync(FormModuleSchema module, ExportOptions options);

    /// <summary>
    /// Import a form module from a file stream
    /// </summary>
    /// <param name="fileStream">The file stream to import from</param>
    /// <param name="options">Import options</param>
    /// <returns>Import result with the module</returns>
    Task<ImportResult> ImportAsync(Stream fileStream, ImportOptions options);

    /// <summary>
    /// Import a form module from JSON content
    /// </summary>
    /// <param name="jsonContent">The JSON content to import</param>
    /// <param name="options">Import options</param>
    /// <returns>Import result with the module</returns>
    Task<ImportResult> ImportAsync(string jsonContent, ImportOptions options);

    /// <summary>
    /// Validate JSON content without importing
    /// </summary>
    /// <param name="jsonContent">The JSON content to validate</param>
    /// <returns>Validation result</returns>
    Task<ImportValidationResult> ValidateAsync(string jsonContent);

    /// <summary>
    /// Get the history of import/export operations
    /// </summary>
    /// <returns>Read-only list of history items</returns>
    IReadOnlyList<ImportExportHistoryItem> GetHistory();

    /// <summary>
    /// Clear the operation history
    /// </summary>
    void ClearHistory();
}
