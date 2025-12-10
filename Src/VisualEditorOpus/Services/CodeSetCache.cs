using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// In-memory cache implementation for CodeSet items
/// </summary>
public class CodeSetCache : ICodeSetCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CodeSetCache> _logger;
    private readonly HashSet<int> _cachedKeys = new();
    private readonly object _lockObject = new();

    public CodeSetCache(IMemoryCache cache, ILogger<CodeSetCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<List<ManagedCodeSetItem>?> GetAsync(int codeSetId)
    {
        var cacheKey = GetCacheKey(codeSetId);
        _cache.TryGetValue(cacheKey, out List<ManagedCodeSetItem>? items);

        if (items != null)
        {
            _logger.LogDebug("Cache hit for CodeSet {CodeSetId}", codeSetId);
        }
        else
        {
            _logger.LogDebug("Cache miss for CodeSet {CodeSetId}", codeSetId);
        }

        return Task.FromResult(items);
    }

    public Task SetAsync(int codeSetId, List<ManagedCodeSetItem> items, int durationSeconds)
    {
        var cacheKey = GetCacheKey(codeSetId);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(durationSeconds))
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                lock (_lockObject)
                {
                    _cachedKeys.Remove(codeSetId);
                }
                _logger.LogDebug("Cache entry evicted for CodeSet {CodeSetId}, reason: {Reason}",
                    codeSetId, reason);
            });

        _cache.Set(cacheKey, items, options);

        lock (_lockObject)
        {
            _cachedKeys.Add(codeSetId);
        }

        _logger.LogDebug("Cached {Count} items for CodeSet {CodeSetId} for {Duration}s",
            items.Count, codeSetId, durationSeconds);

        return Task.CompletedTask;
    }

    public void Invalidate(int codeSetId)
    {
        var cacheKey = GetCacheKey(codeSetId);
        _cache.Remove(cacheKey);

        lock (_lockObject)
        {
            _cachedKeys.Remove(codeSetId);
        }

        _logger.LogDebug("Invalidated cache for CodeSet {CodeSetId}", codeSetId);
    }

    public void InvalidateAll()
    {
        int[] keysToRemove;
        lock (_lockObject)
        {
            keysToRemove = _cachedKeys.ToArray();
            _cachedKeys.Clear();
        }

        foreach (var codeSetId in keysToRemove)
        {
            var cacheKey = GetCacheKey(codeSetId);
            _cache.Remove(cacheKey);
        }

        _logger.LogDebug("Invalidated all CodeSet caches ({Count} entries)", keysToRemove.Length);
    }

    public bool IsCached(int codeSetId)
    {
        var cacheKey = GetCacheKey(codeSetId);
        return _cache.TryGetValue(cacheKey, out _);
    }

    private static string GetCacheKey(int codeSetId) => $"codeset:items:{codeSetId}";
}
