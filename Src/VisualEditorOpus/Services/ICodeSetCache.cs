using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Cache interface for CodeSet items
/// </summary>
public interface ICodeSetCache
{
    /// <summary>
    /// Gets cached items for a CodeSet
    /// </summary>
    Task<List<ManagedCodeSetItem>?> GetAsync(int codeSetId);

    /// <summary>
    /// Caches items for a CodeSet
    /// </summary>
    Task SetAsync(int codeSetId, List<ManagedCodeSetItem> items, int durationSeconds);

    /// <summary>
    /// Invalidates cache for a specific CodeSet
    /// </summary>
    void Invalidate(int codeSetId);

    /// <summary>
    /// Invalidates all cached CodeSets
    /// </summary>
    void InvalidateAll();

    /// <summary>
    /// Checks if a CodeSet is cached
    /// </summary>
    bool IsCached(int codeSetId);
}
