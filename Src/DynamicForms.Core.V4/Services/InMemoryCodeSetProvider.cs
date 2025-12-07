using DynamicForms.Core.V4.Schemas;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Core.V4.Services;

/// <summary>
/// In-memory implementation of ICodeSetProvider.
/// Stores CodeSets in a Dictionary for fast lookup without requiring a database.
/// Useful for development, testing, and scenarios where CodeSets are loaded at startup.
/// </summary>
public class InMemoryCodeSetProvider : ICodeSetProvider
{
    private readonly Dictionary<int, CodeSetSchema> _codeSetsById = new();
    private readonly Dictionary<string, CodeSetSchema> _codeSetsByCode = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<InMemoryCodeSetProvider> _logger;

    public InMemoryCodeSetProvider(ILogger<InMemoryCodeSetProvider> logger)
    {
        _logger = logger;
        _logger.LogDebug("InMemoryCodeSetProvider initialized");
    }

    #region ICodeSetProvider Implementation

    public Task<CodeSetSchema?> GetCodeSetAsync(int codeSetId, CancellationToken cancellationToken = default)
    {
        _codeSetsById.TryGetValue(codeSetId, out var codeSet);
        _logger.LogDebug("GetCodeSetAsync({CodeSetId}): {Found}", codeSetId, codeSet != null ? "Found" : "Not Found");
        return Task.FromResult(codeSet);
    }

    public Task<CodeSetSchema?> GetCodeSetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        _codeSetsByCode.TryGetValue(code, out var codeSet);
        _logger.LogDebug("GetCodeSetByCodeAsync('{Code}'): {Found}", code, codeSet != null ? "Found" : "Not Found");
        return Task.FromResult(codeSet);
    }

    public Task<CodeSetSchema[]> GetAllCodeSetsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var codeSets = _codeSetsById.Values
            .Where(cs => includeInactive || cs.IsActive)
            .ToArray();

        _logger.LogDebug("GetAllCodeSetsAsync(includeInactive: {IncludeInactive}): {Count} CodeSets",
            includeInactive, codeSets.Length);

        return Task.FromResult(codeSets);
    }

    public Task<CodeSetSchema[]> GetCodeSetsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var codeSets = _codeSetsById.Values
            .Where(cs => cs.IsActive && string.Equals(cs.Category, category, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        _logger.LogDebug("GetCodeSetsByCategoryAsync('{Category}'): {Count} CodeSets", category, codeSets.Length);

        return Task.FromResult(codeSets);
    }

    public async Task<CodeSetItem[]> GetCodeSetItemsAsync(int codeSetId, CancellationToken cancellationToken = default)
    {
        var codeSet = await GetCodeSetAsync(codeSetId, cancellationToken);
        return codeSet?.Items ?? Array.Empty<CodeSetItem>();
    }

    public async Task<FieldOption[]> GetCodeSetAsFieldOptionsAsync(int codeSetId, CancellationToken cancellationToken = default)
    {
        var codeSet = await GetCodeSetAsync(codeSetId, cancellationToken);
        return codeSet?.ToFieldOptions() ?? Array.Empty<FieldOption>();
    }

    public Task<bool> CodeSetExistsAsync(int codeSetId, CancellationToken cancellationToken = default)
    {
        var exists = _codeSetsById.ContainsKey(codeSetId);
        return Task.FromResult(exists);
    }

    #endregion

    #region Management Methods

    /// <summary>
    /// Registers a CodeSet in the in-memory store.
    /// This allows CodeSets to be added programmatically at startup or runtime.
    /// </summary>
    /// <param name="codeSet">The CodeSet to register</param>
    public void RegisterCodeSet(CodeSetSchema codeSet)
    {
        if (codeSet == null)
            throw new ArgumentNullException(nameof(codeSet));

        _codeSetsById[codeSet.Id] = codeSet;
        _codeSetsByCode[codeSet.Code] = codeSet;

        _logger.LogInformation("Registered CodeSet: {Id} - '{Code}' ({NameEn}) with {ItemCount} items",
            codeSet.Id, codeSet.Code, codeSet.NameEn, codeSet.Items.Length);
    }

    /// <summary>
    /// Registers multiple CodeSets at once
    /// </summary>
    /// <param name="codeSets">Array of CodeSets to register</param>
    public void RegisterCodeSets(params CodeSetSchema[] codeSets)
    {
        foreach (var codeSet in codeSets)
        {
            RegisterCodeSet(codeSet);
        }
    }

    /// <summary>
    /// Removes a CodeSet from the in-memory store
    /// </summary>
    /// <param name="codeSetId">The CodeSet ID to remove</param>
    public bool UnregisterCodeSet(int codeSetId)
    {
        if (_codeSetsById.TryGetValue(codeSetId, out var codeSet))
        {
            _codeSetsById.Remove(codeSetId);
            _codeSetsByCode.Remove(codeSet.Code);
            _logger.LogInformation("Unregistered CodeSet: {Id} - '{Code}'", codeSetId, codeSet.Code);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Clears all registered CodeSets
    /// </summary>
    public void Clear()
    {
        var count = _codeSetsById.Count;
        _codeSetsById.Clear();
        _codeSetsByCode.Clear();
        _logger.LogInformation("Cleared all CodeSets ({Count} removed)", count);
    }

    /// <summary>
    /// Gets statistics about the registered CodeSets
    /// </summary>
    public CodeSetProviderStats GetStats()
    {
        var totalItems = _codeSetsById.Values.Sum(cs => cs.Items.Length);
        var activeCodeSets = _codeSetsById.Values.Count(cs => cs.IsActive);

        return new CodeSetProviderStats(
            TotalCodeSets: _codeSetsById.Count,
            ActiveCodeSets: activeCodeSets,
            TotalItems: totalItems,
            Categories: _codeSetsById.Values
                .Select(cs => cs.Category)
                .Where(c => c != null)
                .Distinct()
                .Count()
        );
    }

    #endregion
}

/// <summary>
/// Statistics about the CodeSet provider
/// </summary>
/// <param name="TotalCodeSets">Total number of CodeSets</param>
/// <param name="ActiveCodeSets">Number of active CodeSets</param>
/// <param name="TotalItems">Total number of items across all CodeSets</param>
/// <param name="Categories">Number of unique categories</param>
public record CodeSetProviderStats(
    int TotalCodeSets,
    int ActiveCodeSets,
    int TotalItems,
    int Categories
);
