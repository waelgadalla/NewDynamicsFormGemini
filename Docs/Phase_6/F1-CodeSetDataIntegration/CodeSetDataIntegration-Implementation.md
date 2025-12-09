# F.1 CodeSetDataIntegration - Implementation Guide

## Overview

The CodeSetDataIntegration system provides a comprehensive solution for managing lookup data (CodeSets) that populates dropdown fields, radio groups, and other selection controls in forms. It supports static data, API endpoints, and real-time data synchronization.

## Component Architecture

```
CodeSetDataIntegration/
├── CodeSetManager.razor              # Main CodeSet management UI
├── CodeSetManager.razor.css          # Scoped styles
├── CodeSetList.razor                 # List of available CodeSets
├── CodeSetGrid.razor                 # Data grid for viewing/editing
├── CodeSetProperties.razor           # Properties panel
├── Services/
│   ├── ICodeSetService.cs            # Service interface
│   ├── CodeSetService.cs             # Implementation
│   ├── CodeSetCache.cs               # Caching layer
│   └── CodeSetLoader.cs              # Data loading strategies
└── Models/
    ├── CodeSet.cs                    # CodeSet definition
    ├── CodeSetItem.cs                # Individual code item
    ├── CodeSetSource.cs              # Data source configuration
    └── CodeSetBinding.cs             # Field binding info
```

## Data Models

### CodeSet.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// Represents a collection of code/value pairs for form fields
/// </summary>
public record CodeSet
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";

    /// <summary>
    /// Field name containing the code/key value
    /// </summary>
    public string CodeField { get; init; } = "code";

    /// <summary>
    /// Field name containing the display text
    /// </summary>
    public string DisplayField { get; init; } = "displayName";

    /// <summary>
    /// Optional field for descriptions/tooltips
    /// </summary>
    public string? DescriptionField { get; init; }

    /// <summary>
    /// Optional field for sort order
    /// </summary>
    public string? OrderField { get; init; }

    /// <summary>
    /// Optional field for parent code (hierarchical data)
    /// </summary>
    public string? ParentCodeField { get; init; }

    /// <summary>
    /// Data source configuration
    /// </summary>
    public CodeSetSource Source { get; init; } = new();

    /// <summary>
    /// The actual data items
    /// </summary>
    public List<CodeSetItem> Items { get; init; } = new();

    /// <summary>
    /// Fields bound to this CodeSet
    /// </summary>
    public List<CodeSetBinding> Bindings { get; init; } = new();

    /// <summary>
    /// Whether the CodeSet has been modified
    /// </summary>
    public bool IsDirty { get; init; } = false;

    /// <summary>
    /// Last refresh timestamp
    /// </summary>
    public DateTime? LastRefreshed { get; init; }

    /// <summary>
    /// Metadata
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; init; }
}
```

### CodeSetItem.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// A single item within a CodeSet
/// </summary>
public record CodeSetItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The code/key value
    /// </summary>
    public string Code { get; init; } = "";

    /// <summary>
    /// Display text shown to users
    /// </summary>
    public string DisplayName { get; init; } = "";

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Sort order
    /// </summary>
    public int Order { get; init; } = 0;

    /// <summary>
    /// Item status
    /// </summary>
    public CodeSetItemStatus Status { get; init; } = CodeSetItemStatus.Active;

    /// <summary>
    /// Parent code for hierarchical CodeSets
    /// </summary>
    public string? ParentCode { get; init; }

    /// <summary>
    /// Additional custom properties
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Whether this item should be shown
    /// </summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>
    /// Whether this is a default/pre-selected value
    /// </summary>
    public bool IsDefault { get; init; } = false;
}

public enum CodeSetItemStatus
{
    Active,
    Inactive,
    Deprecated
}
```

### CodeSetSource.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// Configuration for CodeSet data source
/// </summary>
public record CodeSetSource
{
    /// <summary>
    /// Type of data source
    /// </summary>
    public CodeSetSourceType Type { get; init; } = CodeSetSourceType.Static;

    /// <summary>
    /// API endpoint URL (for API type)
    /// </summary>
    public string? ApiEndpoint { get; init; }

    /// <summary>
    /// HTTP method for API calls
    /// </summary>
    public HttpMethod HttpMethod { get; init; } = HttpMethod.Get;

    /// <summary>
    /// Custom headers for API calls
    /// </summary>
    public Dictionary<string, string> Headers { get; init; } = new();

    /// <summary>
    /// Request body template (for POST)
    /// </summary>
    public string? RequestBody { get; init; }

    /// <summary>
    /// JSON path to extract items from response
    /// </summary>
    public string? ResponsePath { get; init; }

    /// <summary>
    /// Database connection string (for Database type)
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// SQL query (for Database type)
    /// </summary>
    public string? Query { get; init; }

    /// <summary>
    /// File path (for File type)
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// How often to refresh data
    /// </summary>
    public RefreshMode RefreshMode { get; init; } = RefreshMode.OnLoad;

    /// <summary>
    /// Refresh interval in seconds (for Periodic mode)
    /// </summary>
    public int RefreshIntervalSeconds { get; init; } = 300;

    /// <summary>
    /// Whether to cache the data
    /// </summary>
    public bool EnableCaching { get; init; } = true;

    /// <summary>
    /// Cache duration in seconds
    /// </summary>
    public int CacheDurationSeconds { get; init; } = 3600;
}

public enum CodeSetSourceType
{
    Static,      // Data stored locally in the form definition
    Api,         // Load from REST API
    Database,    // Load from database query
    File         // Load from file (JSON, CSV)
}

public enum RefreshMode
{
    OnDemand,    // Only refresh when explicitly requested
    OnLoad,      // Refresh when form loads
    Periodic,    // Refresh at regular intervals
    RealTime     // Live updates via SignalR/WebSocket
}
```

### CodeSetBinding.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// Represents a binding between a CodeSet and a form field
/// </summary>
public record CodeSetBinding
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The form field ID that uses this CodeSet
    /// </summary>
    public string FieldId { get; init; } = "";

    /// <summary>
    /// Display name of the field
    /// </summary>
    public string FieldName { get; init; } = "";

    /// <summary>
    /// Full path to the field (e.g., Step1.Section1.Field1)
    /// </summary>
    public string FieldPath { get; init; } = "";

    /// <summary>
    /// Type of field component
    /// </summary>
    public BoundFieldType FieldType { get; init; } = BoundFieldType.Dropdown;

    /// <summary>
    /// Filter expression for dependent dropdowns
    /// </summary>
    public string? FilterExpression { get; init; }

    /// <summary>
    /// Parent field for cascading dropdowns
    /// </summary>
    public string? ParentFieldId { get; init; }

    /// <summary>
    /// Whether this binding is active
    /// </summary>
    public bool IsActive { get; init; } = true;
}

public enum BoundFieldType
{
    Dropdown,
    RadioGroup,
    CheckboxGroup,
    Autocomplete,
    ListBox,
    Combobox
}
```

## Services

### ICodeSetService.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Services;

public interface ICodeSetService
{
    // CRUD Operations
    Task<List<CodeSet>> GetAllCodeSetsAsync();
    Task<CodeSet?> GetCodeSetByIdAsync(string id);
    Task<CodeSet?> GetCodeSetByNameAsync(string name);
    Task<CodeSet> CreateCodeSetAsync(CodeSet codeSet);
    Task<CodeSet> UpdateCodeSetAsync(CodeSet codeSet);
    Task DeleteCodeSetAsync(string id);

    // Item Operations
    Task<List<CodeSetItem>> GetItemsAsync(string codeSetId);
    Task<CodeSetItem> AddItemAsync(string codeSetId, CodeSetItem item);
    Task<CodeSetItem> UpdateItemAsync(string codeSetId, CodeSetItem item);
    Task DeleteItemAsync(string codeSetId, string itemId);

    // Data Loading
    Task<List<CodeSetItem>> LoadFromSourceAsync(CodeSetSource source);
    Task RefreshCodeSetAsync(string id);
    Task RefreshAllAsync();

    // Binding Operations
    Task<List<CodeSetBinding>> GetBindingsAsync(string codeSetId);
    Task BindFieldAsync(string codeSetId, CodeSetBinding binding);
    Task UnbindFieldAsync(string codeSetId, string fieldId);

    // Filtering
    Task<List<CodeSetItem>> GetFilteredItemsAsync(string codeSetId, string? parentCode = null, string? searchTerm = null);

    // Import/Export
    Task<CodeSet> ImportFromJsonAsync(string json);
    Task<CodeSet> ImportFromCsvAsync(Stream csvStream);
    Task<string> ExportToJsonAsync(string codeSetId);
    Task<byte[]> ExportToCsvAsync(string codeSetId);
}
```

### CodeSetService.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Services;

public class CodeSetService : ICodeSetService
{
    private readonly ICodeSetCache _cache;
    private readonly ICodeSetLoader _loader;
    private readonly ILogger<CodeSetService> _logger;
    private readonly Dictionary<string, CodeSet> _codeSets = new();

    public CodeSetService(
        ICodeSetCache cache,
        ICodeSetLoader loader,
        ILogger<CodeSetService> logger)
    {
        _cache = cache;
        _loader = loader;
        _logger = logger;
    }

    public Task<List<CodeSet>> GetAllCodeSetsAsync()
    {
        return Task.FromResult(_codeSets.Values.ToList());
    }

    public Task<CodeSet?> GetCodeSetByIdAsync(string id)
    {
        _codeSets.TryGetValue(id, out var codeSet);
        return Task.FromResult(codeSet);
    }

    public Task<CodeSet?> GetCodeSetByNameAsync(string name)
    {
        var codeSet = _codeSets.Values.FirstOrDefault(cs =>
            cs.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(codeSet);
    }

    public Task<CodeSet> CreateCodeSetAsync(CodeSet codeSet)
    {
        var newCodeSet = codeSet with { CreatedAt = DateTime.UtcNow };
        _codeSets[newCodeSet.Id] = newCodeSet;
        return Task.FromResult(newCodeSet);
    }

    public Task<CodeSet> UpdateCodeSetAsync(CodeSet codeSet)
    {
        var updated = codeSet with { ModifiedAt = DateTime.UtcNow };
        _codeSets[codeSet.Id] = updated;
        _cache.Invalidate(codeSet.Id);
        return Task.FromResult(updated);
    }

    public Task DeleteCodeSetAsync(string id)
    {
        _codeSets.Remove(id);
        _cache.Invalidate(id);
        return Task.CompletedTask;
    }

    public async Task<List<CodeSetItem>> GetItemsAsync(string codeSetId)
    {
        // Check cache first
        var cached = await _cache.GetAsync(codeSetId);
        if (cached != null)
            return cached;

        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null)
            return new List<CodeSetItem>();

        // If source is not static, load from source
        if (codeSet.Source.Type != CodeSetSourceType.Static)
        {
            var items = await _loader.LoadAsync(codeSet.Source);
            await _cache.SetAsync(codeSetId, items, codeSet.Source.CacheDurationSeconds);
            return items;
        }

        return codeSet.Items;
    }

    public async Task<List<CodeSetItem>> GetFilteredItemsAsync(
        string codeSetId,
        string? parentCode = null,
        string? searchTerm = null)
    {
        var items = await GetItemsAsync(codeSetId);

        // Filter by parent code (for cascading)
        if (!string.IsNullOrEmpty(parentCode))
        {
            items = items.Where(i => i.ParentCode == parentCode).ToList();
        }

        // Filter by search term
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            items = items.Where(i =>
                i.Code.ToLowerInvariant().Contains(term) ||
                i.DisplayName.ToLowerInvariant().Contains(term) ||
                (i.Description?.ToLowerInvariant().Contains(term) ?? false)
            ).ToList();
        }

        // Only return visible and active items
        return items
            .Where(i => i.IsVisible && i.Status == CodeSetItemStatus.Active)
            .OrderBy(i => i.Order)
            .ThenBy(i => i.DisplayName)
            .ToList();
    }

    public async Task RefreshCodeSetAsync(string id)
    {
        var codeSet = await GetCodeSetByIdAsync(id);
        if (codeSet == null) return;

        _cache.Invalidate(id);

        if (codeSet.Source.Type != CodeSetSourceType.Static)
        {
            var items = await _loader.LoadAsync(codeSet.Source);
            var updated = codeSet with
            {
                Items = items,
                LastRefreshed = DateTime.UtcNow
            };
            _codeSets[id] = updated;
        }
    }

    public async Task RefreshAllAsync()
    {
        foreach (var codeSet in _codeSets.Values)
        {
            await RefreshCodeSetAsync(codeSet.Id);
        }
    }

    public async Task<List<CodeSetItem>> LoadFromSourceAsync(CodeSetSource source)
    {
        return await _loader.LoadAsync(source);
    }

    // Binding operations
    public Task<List<CodeSetBinding>> GetBindingsAsync(string codeSetId)
    {
        if (_codeSets.TryGetValue(codeSetId, out var codeSet))
            return Task.FromResult(codeSet.Bindings);
        return Task.FromResult(new List<CodeSetBinding>());
    }

    public async Task BindFieldAsync(string codeSetId, CodeSetBinding binding)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null) return;

        var bindings = codeSet.Bindings.ToList();
        bindings.Add(binding);

        var updated = codeSet with { Bindings = bindings };
        _codeSets[codeSetId] = updated;
    }

    public async Task UnbindFieldAsync(string codeSetId, string fieldId)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null) return;

        var bindings = codeSet.Bindings.Where(b => b.FieldId != fieldId).ToList();
        var updated = codeSet with { Bindings = bindings };
        _codeSets[codeSetId] = updated;
    }

    // Import/Export
    public Task<CodeSet> ImportFromJsonAsync(string json)
    {
        var codeSet = JsonSerializer.Deserialize<CodeSet>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return CreateCodeSetAsync(codeSet ?? new CodeSet());
    }

    public async Task<CodeSet> ImportFromCsvAsync(Stream csvStream)
    {
        var items = new List<CodeSetItem>();
        using var reader = new StreamReader(csvStream);

        // Read header
        var header = await reader.ReadLineAsync();
        var columns = header?.Split(',') ?? Array.Empty<string>();

        // Read data rows
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line)) continue;

            var values = ParseCsvLine(line);
            if (values.Length >= 2)
            {
                items.Add(new CodeSetItem
                {
                    Code = values[0],
                    DisplayName = values[1],
                    Description = values.Length > 2 ? values[2] : null,
                    Order = items.Count
                });
            }
        }

        return new CodeSet { Items = items };
    }

    public async Task<string> ExportToJsonAsync(string codeSetId)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        return JsonSerializer.Serialize(codeSet, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public async Task<byte[]> ExportToCsvAsync(string codeSetId)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null) return Array.Empty<byte>();

        var sb = new StringBuilder();
        sb.AppendLine("Code,DisplayName,Description,Status,Order");

        foreach (var item in codeSet.Items.OrderBy(i => i.Order))
        {
            sb.AppendLine($"\"{item.Code}\",\"{item.DisplayName}\",\"{item.Description}\",{item.Status},{item.Order}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = new StringBuilder();

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());

        return result.ToArray();
    }
}
```

### CodeSetLoader.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Services;

public interface ICodeSetLoader
{
    Task<List<CodeSetItem>> LoadAsync(CodeSetSource source);
}

public class CodeSetLoader : ICodeSetLoader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CodeSetLoader> _logger;

    public CodeSetLoader(HttpClient httpClient, ILogger<CodeSetLoader> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CodeSetItem>> LoadAsync(CodeSetSource source)
    {
        return source.Type switch
        {
            CodeSetSourceType.Api => await LoadFromApiAsync(source),
            CodeSetSourceType.File => await LoadFromFileAsync(source),
            CodeSetSourceType.Database => await LoadFromDatabaseAsync(source),
            _ => new List<CodeSetItem>()
        };
    }

    private async Task<List<CodeSetItem>> LoadFromApiAsync(CodeSetSource source)
    {
        try
        {
            var request = new HttpRequestMessage(source.HttpMethod, source.ApiEndpoint);

            // Add custom headers
            foreach (var header in source.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Add request body if POST
            if (source.HttpMethod == HttpMethod.Post && !string.IsNullOrEmpty(source.RequestBody))
            {
                request.Content = new StringContent(source.RequestBody, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // Extract data using response path if specified
            if (!string.IsNullOrEmpty(source.ResponsePath))
            {
                var doc = JsonDocument.Parse(json);
                var element = NavigateJsonPath(doc.RootElement, source.ResponsePath);
                json = element.GetRawText();
            }

            return JsonSerializer.Deserialize<List<CodeSetItem>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<CodeSetItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load CodeSet from API: {Endpoint}", source.ApiEndpoint);
            return new List<CodeSetItem>();
        }
    }

    private async Task<List<CodeSetItem>> LoadFromFileAsync(CodeSetSource source)
    {
        if (string.IsNullOrEmpty(source.FilePath))
            return new List<CodeSetItem>();

        try
        {
            var json = await File.ReadAllTextAsync(source.FilePath);
            return JsonSerializer.Deserialize<List<CodeSetItem>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<CodeSetItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load CodeSet from file: {Path}", source.FilePath);
            return new List<CodeSetItem>();
        }
    }

    private Task<List<CodeSetItem>> LoadFromDatabaseAsync(CodeSetSource source)
    {
        // Database loading would require additional dependencies
        // Placeholder for implementation
        _logger.LogWarning("Database loading not implemented");
        return Task.FromResult(new List<CodeSetItem>());
    }

    private JsonElement NavigateJsonPath(JsonElement element, string path)
    {
        var parts = path.Split('.');
        foreach (var part in parts)
        {
            if (element.TryGetProperty(part, out var child))
                element = child;
            else
                break;
        }
        return element;
    }
}
```

### CodeSetCache.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Services;

public interface ICodeSetCache
{
    Task<List<CodeSetItem>?> GetAsync(string key);
    Task SetAsync(string key, List<CodeSetItem> items, int durationSeconds);
    void Invalidate(string key);
    void InvalidateAll();
}

public class CodeSetCache : ICodeSetCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CodeSetCache> _logger;

    public CodeSetCache(IMemoryCache cache, ILogger<CodeSetCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<List<CodeSetItem>?> GetAsync(string key)
    {
        var cacheKey = GetCacheKey(key);
        _cache.TryGetValue(cacheKey, out List<CodeSetItem>? items);
        return Task.FromResult(items);
    }

    public Task SetAsync(string key, List<CodeSetItem> items, int durationSeconds)
    {
        var cacheKey = GetCacheKey(key);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(durationSeconds));

        _cache.Set(cacheKey, items, options);
        _logger.LogDebug("Cached CodeSet {Key} for {Duration}s", key, durationSeconds);

        return Task.CompletedTask;
    }

    public void Invalidate(string key)
    {
        var cacheKey = GetCacheKey(key);
        _cache.Remove(cacheKey);
        _logger.LogDebug("Invalidated CodeSet cache: {Key}", key);
    }

    public void InvalidateAll()
    {
        // Note: IMemoryCache doesn't support clearing all entries
        // In production, use a distributed cache or track keys
        _logger.LogDebug("CodeSet cache invalidation requested");
    }

    private string GetCacheKey(string key) => $"codeset:{key}";
}
```

## Blazor Components

### CodeSetManager.razor

```razor
@namespace VisualEditorOpus.Components.CodeSet
@inject ICodeSetService CodeSetService

<div class="codeset-manager">
    <div class="codeset-list-panel">
        <CodeSetList CodeSets="CodeSets"
                     SelectedCodeSet="SelectedCodeSet"
                     OnSelect="SelectCodeSet"
                     OnAdd="AddCodeSet"
                     OnRefresh="RefreshAll" />
    </div>

    <div class="codeset-data-panel">
        @if (SelectedCodeSet != null)
        {
            <CodeSetGrid CodeSet="SelectedCodeSet"
                         Items="Items"
                         OnItemAdd="AddItem"
                         OnItemUpdate="UpdateItem"
                         OnItemDelete="DeleteItem"
                         OnSearch="SearchItems"
                         OnSort="SortItems" />
        }
        else
        {
            <div class="empty-state">
                <div class="empty-icon">
                    <i class="bi bi-collection"></i>
                </div>
                <h3 class="empty-title">No CodeSet Selected</h3>
                <p class="empty-description">
                    Select a CodeSet from the list or create a new one
                </p>
                <button class="btn btn-primary" @onclick="AddCodeSet">
                    <i class="bi bi-plus-lg"></i>
                    Create CodeSet
                </button>
            </div>
        }
    </div>

    <div class="codeset-properties-panel">
        @if (SelectedCodeSet != null)
        {
            <CodeSetProperties CodeSet="SelectedCodeSet"
                               OnCodeSetChanged="UpdateCodeSet"
                               OnBindingRemove="RemoveBinding" />
        }
    </div>
</div>

@code {
    private List<CodeSet> CodeSets { get; set; } = new();
    private CodeSet? SelectedCodeSet { get; set; }
    private List<CodeSetItem> Items { get; set; } = new();
    private string _searchTerm = "";

    protected override async Task OnInitializedAsync()
    {
        CodeSets = await CodeSetService.GetAllCodeSetsAsync();
        if (CodeSets.Any())
        {
            await SelectCodeSet(CodeSets.First());
        }
    }

    private async Task SelectCodeSet(CodeSet codeSet)
    {
        SelectedCodeSet = codeSet;
        Items = await CodeSetService.GetItemsAsync(codeSet.Id);
    }

    private async Task AddCodeSet()
    {
        var newCodeSet = new CodeSet
        {
            Name = "New CodeSet",
            Description = ""
        };
        var created = await CodeSetService.CreateCodeSetAsync(newCodeSet);
        CodeSets.Add(created);
        await SelectCodeSet(created);
    }

    private async Task UpdateCodeSet(CodeSet codeSet)
    {
        var updated = await CodeSetService.UpdateCodeSetAsync(codeSet);
        var index = CodeSets.FindIndex(cs => cs.Id == updated.Id);
        if (index >= 0)
        {
            CodeSets[index] = updated;
        }
        SelectedCodeSet = updated;
    }

    private async Task RefreshAll()
    {
        await CodeSetService.RefreshAllAsync();
        CodeSets = await CodeSetService.GetAllCodeSetsAsync();
        if (SelectedCodeSet != null)
        {
            await SelectCodeSet(SelectedCodeSet);
        }
    }

    private async Task AddItem()
    {
        if (SelectedCodeSet == null) return;

        var newItem = new CodeSetItem
        {
            Code = "",
            DisplayName = "New Item",
            Order = Items.Count
        };
        var added = await CodeSetService.AddItemAsync(SelectedCodeSet.Id, newItem);
        Items.Add(added);
    }

    private async Task UpdateItem(CodeSetItem item)
    {
        if (SelectedCodeSet == null) return;
        await CodeSetService.UpdateItemAsync(SelectedCodeSet.Id, item);
    }

    private async Task DeleteItem(CodeSetItem item)
    {
        if (SelectedCodeSet == null) return;
        await CodeSetService.DeleteItemAsync(SelectedCodeSet.Id, item.Id);
        Items.Remove(item);
    }

    private async Task SearchItems(string searchTerm)
    {
        if (SelectedCodeSet == null) return;
        _searchTerm = searchTerm;
        Items = await CodeSetService.GetFilteredItemsAsync(
            SelectedCodeSet.Id,
            searchTerm: searchTerm);
    }

    private void SortItems(string column, bool ascending)
    {
        Items = column switch
        {
            "Code" => ascending
                ? Items.OrderBy(i => i.Code).ToList()
                : Items.OrderByDescending(i => i.Code).ToList(),
            "DisplayName" => ascending
                ? Items.OrderBy(i => i.DisplayName).ToList()
                : Items.OrderByDescending(i => i.DisplayName).ToList(),
            "Order" => ascending
                ? Items.OrderBy(i => i.Order).ToList()
                : Items.OrderByDescending(i => i.Order).ToList(),
            _ => Items
        };
    }

    private async Task RemoveBinding(CodeSetBinding binding)
    {
        if (SelectedCodeSet == null) return;
        await CodeSetService.UnbindFieldAsync(SelectedCodeSet.Id, binding.FieldId);
        var updated = await CodeSetService.GetCodeSetByIdAsync(SelectedCodeSet.Id);
        if (updated != null)
            SelectedCodeSet = updated;
    }
}
```

### CodeSetGrid.razor

```razor
@namespace VisualEditorOpus.Components.CodeSet

<div class="data-grid-container">
    <div class="grid-toolbar">
        <div class="search-box">
            <i class="bi bi-search"></i>
            <input type="text"
                   placeholder="Search codes..."
                   value="@_searchTerm"
                   @oninput="HandleSearch" />
        </div>
        <div class="toolbar-actions">
            <button class="btn-icon" @onclick="OnItemAdd" title="Add Row">
                <i class="bi bi-plus-lg"></i>
            </button>
            <button class="btn-icon"
                    @onclick="DeleteSelected"
                    disabled="@(!SelectedItems.Any())"
                    title="Delete Selected">
                <i class="bi bi-trash"></i>
            </button>
            <button class="btn-icon" title="Export">
                <i class="bi bi-download"></i>
            </button>
        </div>
    </div>

    <div class="data-table-wrapper">
        <table class="data-table">
            <thead>
                <tr>
                    <th style="width: 40px;">
                        <input type="checkbox"
                               class="row-checkbox"
                               checked="@_selectAll"
                               @onchange="ToggleSelectAll" />
                    </th>
                    <th class="sortable @(_sortColumn == "Code" ? "sorted" : "")"
                        @onclick="() => Sort(\"Code\")">
                        Code
                        <i class="bi bi-caret-@(_sortAscending ? "up" : "down")-fill sort-icon"></i>
                    </th>
                    <th class="sortable @(_sortColumn == "DisplayName" ? "sorted" : "")"
                        @onclick="() => Sort(\"DisplayName\")">
                        Display Name
                        <i class="bi bi-caret-@(_sortAscending ? "up" : "down")-fill sort-icon"></i>
                    </th>
                    <th>Description</th>
                    <th>Status</th>
                    <th style="width: 80px;">Order</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var item in Items)
                {
                    <tr class="@(SelectedItems.Contains(item) ? "selected" : "")">
                        <td>
                            <input type="checkbox"
                                   class="row-checkbox"
                                   checked="@SelectedItems.Contains(item)"
                                   @onchange="e => ToggleSelect(item, (bool)e.Value!)" />
                        </td>
                        <td class="code-cell">
                            <input type="text"
                                   class="inline-edit"
                                   value="@item.Code"
                                   @onchange="e => UpdateCode(item, e.Value?.ToString())" />
                        </td>
                        <td>
                            <input type="text"
                                   class="inline-edit"
                                   value="@item.DisplayName"
                                   @onchange="e => UpdateDisplayName(item, e.Value?.ToString())" />
                        </td>
                        <td class="description-cell">@item.Description</td>
                        <td>
                            <span class="status-badge @item.Status.ToString().ToLower()">
                                @item.Status
                            </span>
                        </td>
                        <td>@item.Order</td>
                    </tr>
                }
            </tbody>
        </table>
    </div>

    <div class="grid-footer">
        <span class="page-info">@Items.Count items</span>
    </div>
</div>

@code {
    [Parameter] public CodeSet CodeSet { get; set; } = default!;
    [Parameter] public List<CodeSetItem> Items { get; set; } = new();
    [Parameter] public EventCallback OnItemAdd { get; set; }
    [Parameter] public EventCallback<CodeSetItem> OnItemUpdate { get; set; }
    [Parameter] public EventCallback<CodeSetItem> OnItemDelete { get; set; }
    [Parameter] public EventCallback<string> OnSearch { get; set; }
    [Parameter] public EventCallback<(string, bool)> OnSort { get; set; }

    private HashSet<CodeSetItem> SelectedItems { get; set; } = new();
    private bool _selectAll = false;
    private string _searchTerm = "";
    private string _sortColumn = "Code";
    private bool _sortAscending = true;

    private async Task HandleSearch(ChangeEventArgs e)
    {
        _searchTerm = e.Value?.ToString() ?? "";
        await OnSearch.InvokeAsync(_searchTerm);
    }

    private void Sort(string column)
    {
        if (_sortColumn == column)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
        OnSort.InvokeAsync((column, _sortAscending));
    }

    private void ToggleSelectAll(ChangeEventArgs e)
    {
        _selectAll = (bool)e.Value!;
        if (_selectAll)
            SelectedItems = new HashSet<CodeSetItem>(Items);
        else
            SelectedItems.Clear();
    }

    private void ToggleSelect(CodeSetItem item, bool selected)
    {
        if (selected)
            SelectedItems.Add(item);
        else
            SelectedItems.Remove(item);
        _selectAll = SelectedItems.Count == Items.Count;
    }

    private async Task DeleteSelected()
    {
        foreach (var item in SelectedItems.ToList())
        {
            await OnItemDelete.InvokeAsync(item);
        }
        SelectedItems.Clear();
        _selectAll = false;
    }

    private async Task UpdateCode(CodeSetItem item, string? newCode)
    {
        if (newCode != null && newCode != item.Code)
        {
            var updated = item with { Code = newCode };
            await OnItemUpdate.InvokeAsync(updated);
        }
    }

    private async Task UpdateDisplayName(CodeSetItem item, string? newName)
    {
        if (newName != null && newName != item.DisplayName)
        {
            var updated = item with { DisplayName = newName };
            await OnItemUpdate.InvokeAsync(updated);
        }
    }
}
```

## Usage Example

```razor
@page "/form-designer"
@inject ICodeSetService CodeSetService

<FormDesigner>
    <DropdownField Label="Country"
                   @bind-Value="SelectedCountry"
                   CodeSetName="Countries" />

    <DropdownField Label="State"
                   @bind-Value="SelectedState"
                   CodeSetName="States"
                   ParentFieldValue="@SelectedCountry" />
</FormDesigner>

@code {
    private string? SelectedCountry { get; set; }
    private string? SelectedState { get; set; }
}
```

## Claude Prompt for Implementation

```
Implement the CodeSetDataIntegration system for managing lookup data in forms.

Requirements:
1. CodeSet management UI with list, grid, and properties panels
2. Support for static data, API endpoints, and file-based sources
3. CRUD operations for CodeSets and individual items
4. Search, sort, and filter functionality in data grid
5. Field binding tracking to show which fields use each CodeSet
6. Caching layer with configurable duration
7. Import from JSON/CSV and export functionality
8. Dark mode support

Use the existing design system:
- CSS variables for colors (--primary: #6366f1, etc.)
- Data grid with sortable columns and row selection
- Inline editing for code and display name
- Status badges for item status

Services should use dependency injection and interface-based design.
Support cascading/dependent dropdowns via ParentCode filtering.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `CodeSetDataIntegration-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing CodeSet CRUD operations
- Data grid inline editing functionality testing
- Search and filter functionality testing
- Sort by column functionality testing
- Static data source testing
- API data source loading testing
- Caching behavior verification
- Field binding tracking testing
- Cascading dropdown (ParentCode filtering) testing
- Import from JSON/CSV testing
- Export to JSON/CSV testing

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Model files creation (CodeSet, CodeSetItem, CodeSetSource, CodeSetBinding)
- Enum files (CodeSetSourceType, RefreshMode, BoundFieldType, CodeSetItemStatus)
- Service interface and implementation registration in DI
- ICodeSetCache implementation with IMemoryCache
- ICodeSetLoader implementation for API/File loading
- CSS file imports
- Component registration in _Imports.razor
- Form field integration (DropdownField component)
- HttpClient configuration for API loading

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] CodeSet list displays correctly
- [ ] CodeSet selection works
- [ ] Data grid displays items
- [ ] Inline editing updates items
- [ ] Search filters items correctly
- [ ] Column sorting works
- [ ] Add item creates new row
- [ ] Delete item removes row
- [ ] Static source returns stored items
- [ ] API source loads from endpoint
- [ ] Caching prevents repeated API calls
- [ ] Binding tracking shows field usage
- [ ] ParentCode filtering works for cascading
- [ ] JSON import parses correctly
- [ ] CSV import parses correctly
- [ ] Export generates valid files
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

## Integration Notes

1. **Form Field Integration**: Form fields should reference CodeSets by name
2. **Cascading Dropdowns**: Use ParentCode field for dependent dropdown filtering
3. **Caching**: Default 1-hour cache with configurable duration per CodeSet
4. **Real-time Updates**: Use SignalR for live data sync when enabled
5. **Performance**: Paginate large CodeSets in the grid view
