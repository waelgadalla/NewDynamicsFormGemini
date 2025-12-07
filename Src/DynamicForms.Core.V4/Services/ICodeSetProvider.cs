using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Core.V4.Services;

/// <summary>
/// Abstraction for loading CodeSets from various data sources.
/// This interface allows flexibility in how CodeSets are stored and retrieved:
/// - In-memory collections
/// - JSON files
/// - Database
/// - External APIs
/// - Cached sources
/// </summary>
public interface ICodeSetProvider
{
    /// <summary>
    /// Gets a CodeSet by its unique identifier
    /// </summary>
    /// <param name="codeSetId">The CodeSet ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The CodeSet schema, or null if not found</returns>
    Task<CodeSetSchema?> GetCodeSetAsync(int codeSetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a CodeSet by its unique code/key
    /// </summary>
    /// <param name="code">The CodeSet code (e.g., "PROVINCES_CA")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The CodeSet schema, or null if not found</returns>
    Task<CodeSetSchema?> GetCodeSetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available CodeSets
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive CodeSets</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of all CodeSets</returns>
    Task<CodeSetSchema[]> GetAllCodeSetsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets CodeSets filtered by category
    /// </summary>
    /// <param name="category">Category to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of matching CodeSets</returns>
    Task<CodeSetSchema[]> GetCodeSetsByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets just the items/options from a CodeSet (convenience method)
    /// </summary>
    /// <param name="codeSetId">The CodeSet ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of CodeSet items, or empty array if CodeSet not found</returns>
    Task<CodeSetItem[]> GetCodeSetItemsAsync(int codeSetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets CodeSet items converted to FieldOptions (ready to use in form fields)
    /// </summary>
    /// <param name="codeSetId">The CodeSet ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of FieldOptions, or empty array if CodeSet not found</returns>
    Task<FieldOption[]> GetCodeSetAsFieldOptionsAsync(int codeSetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a CodeSet exists
    /// </summary>
    /// <param name="codeSetId">The CodeSet ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if CodeSet exists, false otherwise</returns>
    Task<bool> CodeSetExistsAsync(int codeSetId, CancellationToken cancellationToken = default);
}
