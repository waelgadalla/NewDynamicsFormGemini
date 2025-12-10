using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Service for managing CodeSets with full CRUD operations, caching, and data loading
/// </summary>
public class CodeSetService : ICodeSetService
{
    private readonly ICodeSetCache _cache;
    private readonly ICodeSetLoader _loader;
    private readonly ILogger<CodeSetService> _logger;
    private readonly Dictionary<int, ManagedCodeSet> _codeSets = new();
    private int _nextId = 1;

    public event EventHandler<CodeSetChangedEventArgs>? CodeSetChanged;

    public CodeSetService(
        ICodeSetCache cache,
        ICodeSetLoader loader,
        ILogger<CodeSetService> logger)
    {
        _cache = cache;
        _loader = loader;
        _logger = logger;

        // Initialize with sample data
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        var countries = new ManagedCodeSet
        {
            Id = _nextId++,
            Code = "COUNTRIES",
            NameEn = "Countries",
            NameFr = "Pays",
            DescriptionEn = "ISO country codes",
            Category = "Geography",
            Items = new List<ManagedCodeSetItem>
            {
                new() { Id = Guid.NewGuid().ToString(), Code = "US", DisplayNameEn = "United States", DisplayNameFr = "États-Unis", Description = "United States of America", Order = 1, Status = CodeSetItemStatus.Active },
                new() { Id = Guid.NewGuid().ToString(), Code = "CA", DisplayNameEn = "Canada", DisplayNameFr = "Canada", Order = 2, Status = CodeSetItemStatus.Active },
                new() { Id = Guid.NewGuid().ToString(), Code = "MX", DisplayNameEn = "Mexico", DisplayNameFr = "Mexique", Description = "United Mexican States", Order = 3, Status = CodeSetItemStatus.Active },
                new() { Id = Guid.NewGuid().ToString(), Code = "GB", DisplayNameEn = "United Kingdom", DisplayNameFr = "Royaume-Uni", Description = "United Kingdom of Great Britain", Order = 4, Status = CodeSetItemStatus.Active },
                new() { Id = Guid.NewGuid().ToString(), Code = "DE", DisplayNameEn = "Germany", DisplayNameFr = "Allemagne", Description = "Federal Republic of Germany", Order = 5, Status = CodeSetItemStatus.Active },
                new() { Id = Guid.NewGuid().ToString(), Code = "FR", DisplayNameEn = "France", DisplayNameFr = "France", Description = "French Republic", Order = 6, Status = CodeSetItemStatus.Active },
                new() { Id = Guid.NewGuid().ToString(), Code = "JP", DisplayNameEn = "Japan", DisplayNameFr = "Japon", Order = 7, Status = CodeSetItemStatus.Active },
                new() { Id = Guid.NewGuid().ToString(), Code = "AU", DisplayNameEn = "Australia", DisplayNameFr = "Australie", Description = "Commonwealth of Australia", Order = 8, Status = CodeSetItemStatus.Active },
                new() { Id = Guid.NewGuid().ToString(), Code = "BR", DisplayNameEn = "Brazil", DisplayNameFr = "Brésil", Description = "Federative Republic of Brazil", Order = 9, Status = CodeSetItemStatus.Deprecated },
                new() { Id = Guid.NewGuid().ToString(), Code = "IN", DisplayNameEn = "India", DisplayNameFr = "Inde", Description = "Republic of India", Order = 10, Status = CodeSetItemStatus.Inactive },
            },
            Bindings = new List<CodeSetBinding>
            {
                new() { FieldId = "f1", FieldName = "Country Selector", FieldPath = "Step1.PersonalInfo.Country", FieldType = BoundFieldType.Dropdown },
                new() { FieldId = "f2", FieldName = "Billing Country", FieldPath = "Step2.BillingAddress.Country", FieldType = BoundFieldType.Dropdown },
                new() { FieldId = "f3", FieldName = "Shipping Country", FieldPath = "Step2.ShippingAddress.Country", FieldType = BoundFieldType.Dropdown },
            }
        };
        _codeSets[countries.Id] = countries;

        var states = new ManagedCodeSet
        {
            Id = _nextId++,
            Code = "US_STATES",
            NameEn = "US States",
            NameFr = "États américains",
            DescriptionEn = "US state codes",
            Category = "Geography",
            ParentCodeField = "countryCode",
            Items = new List<ManagedCodeSetItem>
            {
                new() { Id = Guid.NewGuid().ToString(), Code = "AL", DisplayNameEn = "Alabama", Order = 1, ParentCode = "US" },
                new() { Id = Guid.NewGuid().ToString(), Code = "AK", DisplayNameEn = "Alaska", Order = 2, ParentCode = "US" },
                new() { Id = Guid.NewGuid().ToString(), Code = "AZ", DisplayNameEn = "Arizona", Order = 3, ParentCode = "US" },
                new() { Id = Guid.NewGuid().ToString(), Code = "CA", DisplayNameEn = "California", Order = 4, ParentCode = "US" },
                new() { Id = Guid.NewGuid().ToString(), Code = "CO", DisplayNameEn = "Colorado", Order = 5, ParentCode = "US" },
                new() { Id = Guid.NewGuid().ToString(), Code = "FL", DisplayNameEn = "Florida", Order = 6, ParentCode = "US" },
                new() { Id = Guid.NewGuid().ToString(), Code = "NY", DisplayNameEn = "New York", Order = 7, ParentCode = "US" },
                new() { Id = Guid.NewGuid().ToString(), Code = "TX", DisplayNameEn = "Texas", Order = 8, ParentCode = "US" },
            }
        };
        _codeSets[states.Id] = states;

        var departments = new ManagedCodeSet
        {
            Id = _nextId++,
            Code = "DEPARTMENTS",
            NameEn = "Departments",
            NameFr = "Départements",
            DescriptionEn = "Organization departments",
            Category = "Organization",
            IsDirty = true,
            Items = new List<ManagedCodeSetItem>
            {
                new() { Id = Guid.NewGuid().ToString(), Code = "HR", DisplayNameEn = "Human Resources", DisplayNameFr = "Ressources humaines", Order = 1 },
                new() { Id = Guid.NewGuid().ToString(), Code = "IT", DisplayNameEn = "Information Technology", DisplayNameFr = "Technologies de l'information", Order = 2 },
                new() { Id = Guid.NewGuid().ToString(), Code = "FIN", DisplayNameEn = "Finance", DisplayNameFr = "Finance", Order = 3 },
                new() { Id = Guid.NewGuid().ToString(), Code = "MKT", DisplayNameEn = "Marketing", DisplayNameFr = "Marketing", Order = 4 },
                new() { Id = Guid.NewGuid().ToString(), Code = "OPS", DisplayNameEn = "Operations", DisplayNameFr = "Opérations", Order = 5 },
            }
        };
        _codeSets[departments.Id] = departments;

        var statusCodes = new ManagedCodeSet
        {
            Id = _nextId++,
            Code = "STATUS_CODES",
            NameEn = "Status Codes",
            NameFr = "Codes de statut",
            DescriptionEn = "Application status codes",
            Category = "System",
            Items = new List<ManagedCodeSetItem>
            {
                new() { Id = Guid.NewGuid().ToString(), Code = "PEND", DisplayNameEn = "Pending", DisplayNameFr = "En attente", Order = 1 },
                new() { Id = Guid.NewGuid().ToString(), Code = "APPR", DisplayNameEn = "Approved", DisplayNameFr = "Approuvé", Order = 2 },
                new() { Id = Guid.NewGuid().ToString(), Code = "REJ", DisplayNameEn = "Rejected", DisplayNameFr = "Rejeté", Order = 3 },
                new() { Id = Guid.NewGuid().ToString(), Code = "CANC", DisplayNameEn = "Cancelled", DisplayNameFr = "Annulé", Order = 4 },
            }
        };
        _codeSets[statusCodes.Id] = statusCodes;

        var priorities = new ManagedCodeSet
        {
            Id = _nextId++,
            Code = "PRIORITIES",
            NameEn = "Priority Levels",
            NameFr = "Niveaux de priorité",
            DescriptionEn = "Task priority levels",
            Category = "System",
            Source = new CodeSetSource { Type = CodeSetSourceType.Api, ApiEndpoint = "https://api.example.com/priorities" },
            Items = new List<ManagedCodeSetItem>
            {
                new() { Id = Guid.NewGuid().ToString(), Code = "LOW", DisplayNameEn = "Low", DisplayNameFr = "Faible", Order = 1 },
                new() { Id = Guid.NewGuid().ToString(), Code = "MED", DisplayNameEn = "Medium", DisplayNameFr = "Moyen", Order = 2 },
                new() { Id = Guid.NewGuid().ToString(), Code = "HIGH", DisplayNameEn = "High", DisplayNameFr = "Élevé", Order = 3 },
                new() { Id = Guid.NewGuid().ToString(), Code = "CRIT", DisplayNameEn = "Critical", DisplayNameFr = "Critique", Order = 4, Status = CodeSetItemStatus.Active },
            }
        };
        _codeSets[priorities.Id] = priorities;

        _logger.LogInformation("Initialized {Count} sample CodeSets", _codeSets.Count);
    }

    #region CRUD Operations

    public Task<List<ManagedCodeSet>> GetAllCodeSetsAsync()
    {
        return Task.FromResult(_codeSets.Values.ToList());
    }

    public Task<ManagedCodeSet?> GetCodeSetByIdAsync(int id)
    {
        _codeSets.TryGetValue(id, out var codeSet);
        return Task.FromResult(codeSet);
    }

    public Task<ManagedCodeSet?> GetCodeSetByCodeAsync(string code)
    {
        var codeSet = _codeSets.Values.FirstOrDefault(cs =>
            cs.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(codeSet);
    }

    public Task<ManagedCodeSet> CreateCodeSetAsync(ManagedCodeSet codeSet)
    {
        var newCodeSet = codeSet with
        {
            Id = _nextId++,
            DateCreated = DateTime.UtcNow
        };
        _codeSets[newCodeSet.Id] = newCodeSet;

        _logger.LogInformation("Created CodeSet: {Id} - {Code}", newCodeSet.Id, newCodeSet.Code);
        OnCodeSetChanged(newCodeSet.Id, CodeSetChangeType.Created);

        return Task.FromResult(newCodeSet);
    }

    public Task<ManagedCodeSet> UpdateCodeSetAsync(ManagedCodeSet codeSet)
    {
        var updated = codeSet with { DateUpdated = DateTime.UtcNow };
        _codeSets[codeSet.Id] = updated;
        _cache.Invalidate(codeSet.Id);

        _logger.LogInformation("Updated CodeSet: {Id} - {Code}", codeSet.Id, codeSet.Code);
        OnCodeSetChanged(codeSet.Id, CodeSetChangeType.Updated);

        return Task.FromResult(updated);
    }

    public Task DeleteCodeSetAsync(int id)
    {
        if (_codeSets.Remove(id))
        {
            _cache.Invalidate(id);
            _logger.LogInformation("Deleted CodeSet: {Id}", id);
            OnCodeSetChanged(id, CodeSetChangeType.Deleted);
        }
        return Task.CompletedTask;
    }

    #endregion

    #region Item Operations

    public async Task<List<ManagedCodeSetItem>> GetItemsAsync(int codeSetId)
    {
        // Check cache first
        var cached = await _cache.GetAsync(codeSetId);
        if (cached != null)
            return cached;

        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null)
            return new List<ManagedCodeSetItem>();

        // If source is not static, try to load from source
        if (codeSet.Source.Type != CodeSetSourceType.Static)
        {
            try
            {
                var items = await _loader.LoadAsync(codeSet.Source);
                if (items.Any())
                {
                    await _cache.SetAsync(codeSetId, items, codeSet.Source.CacheDurationSeconds);
                    return items;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load from source, using stored items");
            }
        }

        return codeSet.Items;
    }

    public async Task<ManagedCodeSetItem> AddItemAsync(int codeSetId, ManagedCodeSetItem item)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null)
            throw new InvalidOperationException($"CodeSet {codeSetId} not found");

        var newItem = item with
        {
            Id = Guid.NewGuid().ToString(),
            Order = codeSet.Items.Count + 1
        };

        var items = codeSet.Items.ToList();
        items.Add(newItem);

        var updated = codeSet with { Items = items, IsDirty = true };
        _codeSets[codeSetId] = updated;
        _cache.Invalidate(codeSetId);

        OnCodeSetChanged(codeSetId, CodeSetChangeType.ItemsChanged);
        return newItem;
    }

    public async Task<ManagedCodeSetItem> UpdateItemAsync(int codeSetId, ManagedCodeSetItem item)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null)
            throw new InvalidOperationException($"CodeSet {codeSetId} not found");

        var items = codeSet.Items.ToList();
        var index = items.FindIndex(i => i.Id == item.Id);
        if (index >= 0)
        {
            items[index] = item;
            var updated = codeSet with { Items = items, IsDirty = true };
            _codeSets[codeSetId] = updated;
            _cache.Invalidate(codeSetId);
            OnCodeSetChanged(codeSetId, CodeSetChangeType.ItemsChanged);
        }

        return item;
    }

    public async Task DeleteItemAsync(int codeSetId, string itemId)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null) return;

        var items = codeSet.Items.Where(i => i.Id != itemId).ToList();
        var updated = codeSet with { Items = items, IsDirty = true };
        _codeSets[codeSetId] = updated;
        _cache.Invalidate(codeSetId);

        OnCodeSetChanged(codeSetId, CodeSetChangeType.ItemsChanged);
    }

    public async Task ReorderItemsAsync(int codeSetId, List<string> itemIds)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null) return;

        var itemDict = codeSet.Items.ToDictionary(i => i.Id);
        var reordered = new List<ManagedCodeSetItem>();

        for (int i = 0; i < itemIds.Count; i++)
        {
            if (itemDict.TryGetValue(itemIds[i], out var item))
            {
                reordered.Add(item with { Order = i + 1 });
            }
        }

        var updated = codeSet with { Items = reordered, IsDirty = true };
        _codeSets[codeSetId] = updated;
        _cache.Invalidate(codeSetId);

        OnCodeSetChanged(codeSetId, CodeSetChangeType.ItemsChanged);
    }

    #endregion

    #region Data Loading

    public Task<List<ManagedCodeSetItem>> LoadFromSourceAsync(CodeSetSource source)
    {
        return _loader.LoadAsync(source);
    }

    public async Task RefreshCodeSetAsync(int id)
    {
        var codeSet = await GetCodeSetByIdAsync(id);
        if (codeSet == null) return;

        _cache.Invalidate(id);

        if (codeSet.Source.Type != CodeSetSourceType.Static)
        {
            try
            {
                var items = await _loader.LoadAsync(codeSet.Source);
                var updated = codeSet with
                {
                    Items = items,
                    LastRefreshed = DateTime.UtcNow,
                    IsDirty = false
                };
                _codeSets[id] = updated;
                OnCodeSetChanged(id, CodeSetChangeType.Refreshed);

                _logger.LogInformation("Refreshed CodeSet {Id} with {Count} items", id, items.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh CodeSet {Id}", id);
            }
        }
    }

    public async Task RefreshAllAsync()
    {
        foreach (var codeSet in _codeSets.Values.ToList())
        {
            if (codeSet.Source.Type != CodeSetSourceType.Static)
            {
                await RefreshCodeSetAsync(codeSet.Id);
            }
        }
    }

    #endregion

    #region Binding Operations

    public async Task<List<CodeSetBinding>> GetBindingsAsync(int codeSetId)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        return codeSet?.Bindings ?? new List<CodeSetBinding>();
    }

    public async Task BindFieldAsync(int codeSetId, CodeSetBinding binding)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null) return;

        var bindings = codeSet.Bindings.ToList();
        bindings.Add(binding);

        var updated = codeSet with { Bindings = bindings };
        _codeSets[codeSetId] = updated;
    }

    public async Task UnbindFieldAsync(int codeSetId, string fieldId)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null) return;

        var bindings = codeSet.Bindings.Where(b => b.FieldId != fieldId).ToList();
        var updated = codeSet with { Bindings = bindings };
        _codeSets[codeSetId] = updated;
    }

    #endregion

    #region Filtering

    public async Task<List<ManagedCodeSetItem>> GetFilteredItemsAsync(
        int codeSetId,
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
                i.DisplayNameEn.ToLowerInvariant().Contains(term) ||
                (i.DisplayNameFr?.ToLowerInvariant().Contains(term) ?? false) ||
                (i.Description?.ToLowerInvariant().Contains(term) ?? false)
            ).ToList();
        }

        // Only return visible and active items
        return items
            .Where(i => i.IsVisible && i.Status == CodeSetItemStatus.Active)
            .OrderBy(i => i.Order)
            .ThenBy(i => i.DisplayNameEn)
            .ToList();
    }

    public Task<List<ManagedCodeSet>> SearchCodeSetsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return GetAllCodeSetsAsync();

        var term = searchTerm.ToLowerInvariant();
        var results = _codeSets.Values.Where(cs =>
            cs.Code.ToLowerInvariant().Contains(term) ||
            cs.NameEn.ToLowerInvariant().Contains(term) ||
            (cs.NameFr?.ToLowerInvariant().Contains(term) ?? false) ||
            (cs.DescriptionEn?.ToLowerInvariant().Contains(term) ?? false) ||
            (cs.Category?.ToLowerInvariant().Contains(term) ?? false)
        ).ToList();

        return Task.FromResult(results);
    }

    #endregion

    #region Import/Export

    public async Task<ManagedCodeSet> ImportFromJsonAsync(string json)
    {
        var codeSet = JsonSerializer.Deserialize<ManagedCodeSet>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (codeSet == null)
            throw new InvalidOperationException("Failed to parse JSON");

        return await CreateCodeSetAsync(codeSet);
    }

    public async Task<List<ManagedCodeSetItem>> ImportItemsFromCsvAsync(Stream csvStream)
    {
        var items = new List<ManagedCodeSetItem>();
        using var reader = new StreamReader(csvStream);

        // Read header
        var header = await reader.ReadLineAsync();
        if (header == null) return items;

        var columns = header.Split(',')
            .Select(c => c.Trim().ToLowerInvariant())
            .ToArray();

        int order = 0;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line)) continue;

            var values = ParseCsvLine(line);
            if (values.Length >= 2)
            {
                items.Add(new ManagedCodeSetItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = values[0],
                    DisplayNameEn = values[1],
                    DisplayNameFr = values.Length > 2 ? values[2] : null,
                    Description = values.Length > 3 ? values[3] : null,
                    Order = order++
                });
            }
        }

        return items;
    }

    public async Task<string> ExportToJsonAsync(int codeSetId)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        return JsonSerializer.Serialize(codeSet, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task<byte[]> ExportToCsvAsync(int codeSetId)
    {
        var codeSet = await GetCodeSetByIdAsync(codeSetId);
        if (codeSet == null) return Array.Empty<byte>();

        var sb = new StringBuilder();
        sb.AppendLine("Code,DisplayNameEn,DisplayNameFr,Description,Status,Order");

        foreach (var item in codeSet.Items.OrderBy(i => i.Order))
        {
            sb.AppendLine($"\"{EscapeCsv(item.Code)}\",\"{EscapeCsv(item.DisplayNameEn)}\",\"{EscapeCsv(item.DisplayNameFr ?? "")}\",\"{EscapeCsv(item.Description ?? "")}\",{item.Status},{item.Order}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        return value.Replace("\"", "\"\"");
    }

    private static string[] ParseCsvLine(string line)
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
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString().Trim());

        return result.ToArray();
    }

    #endregion

    private void OnCodeSetChanged(int codeSetId, CodeSetChangeType changeType)
    {
        CodeSetChanged?.Invoke(this, new CodeSetChangedEventArgs(codeSetId, changeType));
    }
}
