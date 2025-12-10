using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Service interface for importing and exporting CodeSet data
/// </summary>
public interface IImportExportService
{
    #region Parsing

    /// <summary>
    /// Parses a file based on its extension
    /// </summary>
    Task<CodeSetParsedFileData> ParseFileAsync(Stream stream, string fileName);

    /// <summary>
    /// Parses JSON content
    /// </summary>
    Task<CodeSetParsedFileData> ParseJsonAsync(Stream stream, string fileName);

    /// <summary>
    /// Parses CSV content
    /// </summary>
    Task<CodeSetParsedFileData> ParseCsvAsync(Stream stream, string fileName, char delimiter = ',');

    #endregion

    #region Import

    /// <summary>
    /// Imports parsed data into a CodeSet
    /// </summary>
    Task<CodeSetImportResult> ImportAsync(CodeSetParsedFileData data, CodeSetImportOptions options, IProgress<CodeSetImportProgress>? progress = null);

    /// <summary>
    /// Validates parsed data before import
    /// </summary>
    Task<CodeSetImportResult> ValidateAsync(CodeSetParsedFileData data, CodeSetImportOptions options);

    #endregion

    #region Export

    /// <summary>
    /// Exports a CodeSet to the specified format
    /// </summary>
    Task<byte[]> ExportAsync(int codeSetId, CodeSetExportOptions options);

    /// <summary>
    /// Exports a CodeSet to JSON
    /// </summary>
    Task<byte[]> ExportToJsonAsync(ManagedCodeSet codeSet, CodeSetExportOptions options);

    /// <summary>
    /// Exports a CodeSet to CSV
    /// </summary>
    Task<byte[]> ExportToCsvAsync(ManagedCodeSet codeSet, CodeSetExportOptions options);

    /// <summary>
    /// Exports a CodeSet to XML
    /// </summary>
    Task<byte[]> ExportToXmlAsync(ManagedCodeSet codeSet, CodeSetExportOptions options);

    #endregion

    #region Field Detection

    /// <summary>
    /// Auto-detects field mapping from column names
    /// </summary>
    CodeSetFieldMapping DetectFieldMapping(List<string> columns);

    #endregion
}
