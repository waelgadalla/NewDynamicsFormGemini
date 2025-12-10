using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Interface for loading CodeSet items from various data sources
/// </summary>
public interface ICodeSetLoader
{
    /// <summary>
    /// Loads items from a data source configuration
    /// </summary>
    Task<List<ManagedCodeSetItem>> LoadAsync(CodeSetSource source);

    /// <summary>
    /// Tests if a data source is accessible and returns sample data
    /// </summary>
    Task<CodeSetLoaderResult> TestSourceAsync(CodeSetSource source);
}

/// <summary>
/// Result of a CodeSet load operation
/// </summary>
public record CodeSetLoaderResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public List<ManagedCodeSetItem> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public TimeSpan LoadTime { get; init; }
}
