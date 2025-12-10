using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Service interface for managing CodeSets with full CRUD operations,
/// caching, and data loading from various sources.
/// </summary>
public interface ICodeSetService
{
    #region CRUD Operations

    /// <summary>
    /// Gets all CodeSets
    /// </summary>
    Task<List<ManagedCodeSet>> GetAllCodeSetsAsync();

    /// <summary>
    /// Gets a CodeSet by its unique identifier
    /// </summary>
    Task<ManagedCodeSet?> GetCodeSetByIdAsync(int id);

    /// <summary>
    /// Gets a CodeSet by its code/key
    /// </summary>
    Task<ManagedCodeSet?> GetCodeSetByCodeAsync(string code);

    /// <summary>
    /// Creates a new CodeSet
    /// </summary>
    Task<ManagedCodeSet> CreateCodeSetAsync(ManagedCodeSet codeSet);

    /// <summary>
    /// Updates an existing CodeSet
    /// </summary>
    Task<ManagedCodeSet> UpdateCodeSetAsync(ManagedCodeSet codeSet);

    /// <summary>
    /// Deletes a CodeSet by ID
    /// </summary>
    Task DeleteCodeSetAsync(int id);

    #endregion

    #region Item Operations

    /// <summary>
    /// Gets all items for a CodeSet
    /// </summary>
    Task<List<ManagedCodeSetItem>> GetItemsAsync(int codeSetId);

    /// <summary>
    /// Adds a new item to a CodeSet
    /// </summary>
    Task<ManagedCodeSetItem> AddItemAsync(int codeSetId, ManagedCodeSetItem item);

    /// <summary>
    /// Updates an item in a CodeSet
    /// </summary>
    Task<ManagedCodeSetItem> UpdateItemAsync(int codeSetId, ManagedCodeSetItem item);

    /// <summary>
    /// Deletes an item from a CodeSet
    /// </summary>
    Task DeleteItemAsync(int codeSetId, string itemId);

    /// <summary>
    /// Reorders items in a CodeSet
    /// </summary>
    Task ReorderItemsAsync(int codeSetId, List<string> itemIds);

    #endregion

    #region Data Loading

    /// <summary>
    /// Loads items from a data source configuration
    /// </summary>
    Task<List<ManagedCodeSetItem>> LoadFromSourceAsync(CodeSetSource source);

    /// <summary>
    /// Refreshes a CodeSet's data from its source
    /// </summary>
    Task RefreshCodeSetAsync(int id);

    /// <summary>
    /// Refreshes all CodeSets
    /// </summary>
    Task RefreshAllAsync();

    #endregion

    #region Binding Operations

    /// <summary>
    /// Gets all bindings for a CodeSet
    /// </summary>
    Task<List<CodeSetBinding>> GetBindingsAsync(int codeSetId);

    /// <summary>
    /// Binds a field to a CodeSet
    /// </summary>
    Task BindFieldAsync(int codeSetId, CodeSetBinding binding);

    /// <summary>
    /// Unbinds a field from a CodeSet
    /// </summary>
    Task UnbindFieldAsync(int codeSetId, string fieldId);

    #endregion

    #region Filtering

    /// <summary>
    /// Gets filtered items for cascading dropdowns
    /// </summary>
    Task<List<ManagedCodeSetItem>> GetFilteredItemsAsync(
        int codeSetId,
        string? parentCode = null,
        string? searchTerm = null);

    /// <summary>
    /// Searches CodeSets by name or description
    /// </summary>
    Task<List<ManagedCodeSet>> SearchCodeSetsAsync(string searchTerm);

    #endregion

    #region Import/Export

    /// <summary>
    /// Imports a CodeSet from JSON
    /// </summary>
    Task<ManagedCodeSet> ImportFromJsonAsync(string json);

    /// <summary>
    /// Imports items from CSV
    /// </summary>
    Task<List<ManagedCodeSetItem>> ImportItemsFromCsvAsync(Stream csvStream);

    /// <summary>
    /// Exports a CodeSet to JSON
    /// </summary>
    Task<string> ExportToJsonAsync(int codeSetId);

    /// <summary>
    /// Exports CodeSet items to CSV
    /// </summary>
    Task<byte[]> ExportToCsvAsync(int codeSetId);

    #endregion

    #region Events

    /// <summary>
    /// Event fired when a CodeSet is changed
    /// </summary>
    event EventHandler<CodeSetChangedEventArgs>? CodeSetChanged;

    #endregion
}

/// <summary>
/// Event args for CodeSet changes
/// </summary>
public class CodeSetChangedEventArgs : EventArgs
{
    public int CodeSetId { get; }
    public CodeSetChangeType ChangeType { get; }

    public CodeSetChangedEventArgs(int codeSetId, CodeSetChangeType changeType)
    {
        CodeSetId = codeSetId;
        ChangeType = changeType;
    }
}

public enum CodeSetChangeType
{
    Created,
    Updated,
    Deleted,
    ItemsChanged,
    Refreshed
}
