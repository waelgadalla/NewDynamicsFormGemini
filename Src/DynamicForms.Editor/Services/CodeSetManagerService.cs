using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Services;
using System.Text.Json;

namespace DynamicForms.Editor.Services;

public class CodeSetManagerService : ICodeSetManagerService
{
    private readonly InMemoryCodeSetProvider _provider;
    private readonly IToastService _toastService;
    
    public CodeSetSchema? SelectedCodeSet { get; private set; }
    public IEnumerable<CodeSetSchema> CodeSets { get; private set; } = Array.Empty<CodeSetSchema>();
    public bool IsDirty { get; private set; }

    public event Action? OnStateChanged;

    public CodeSetManagerService(ICodeSetProvider provider, IToastService toastService)
    {
        // We assume the provider is the InMemory one for this editor
        _provider = (InMemoryCodeSetProvider)provider;
        _toastService = toastService;
    }

    public async Task LoadCodeSetsAsync()
    {
        CodeSets = await _provider.GetAllCodeSetsAsync(includeInactive: true);
        OnStateChanged?.Invoke();
    }

    public void SelectCodeSet(int? id)
    {
        if (id.HasValue)
        {
            SelectedCodeSet = CodeSets.FirstOrDefault(c => c.Id == id.Value);
        }
        else
        {
            SelectedCodeSet = null;
        }
        OnStateChanged?.Invoke();
    }

    public void CreateNewCodeSet()
    {
        var newId = (CodeSets.Max(c => (int?)c.Id) ?? 0) + 1;
        var newCodeSet = new CodeSetSchema
        {
            Id = newId,
            Code = $"NEW_CODESET_{newId}",
            NameEn = "New CodeSet",
            Items = Array.Empty<CodeSetItem>(),
            IsActive = true
        };
        
        // We don't save immediately to provider, but we set it as selected
        SelectedCodeSet = newCodeSet;
        // In a real app we might want a "draft" state. 
        // For simplicity, we'll register it immediately so it appears in the list
        _provider.RegisterCodeSet(newCodeSet);
        LoadCodeSetsAsync().GetAwaiter().GetResult();
        SelectCodeSet(newId);
    }

    public void UpdateSelectedCodeSet(CodeSetSchema updatedCodeSet)
    {
        SelectedCodeSet = updatedCodeSet;
        OnStateChanged?.Invoke();
    }

    public void SaveCodeSet(CodeSetSchema codeSet)
    {
        _provider.RegisterCodeSet(codeSet); // Upsert
        LoadCodeSetsAsync().GetAwaiter().GetResult();
        SelectCodeSet(codeSet.Id);
        _toastService.ShowSuccess($"CodeSet '{codeSet.NameEn}' saved.");
    }

    public void DeleteCodeSet(int id)
    {
        _provider.UnregisterCodeSet(id);
        LoadCodeSetsAsync().GetAwaiter().GetResult();
        if (SelectedCodeSet?.Id == id)
        {
            SelectCodeSet(null);
        }
        _toastService.ShowInfo("CodeSet deleted.");
    }

    public string ExportToJson(CodeSetSchema codeSet)
    {
        return JsonSerializer.Serialize(codeSet, new JsonSerializerOptions { WriteIndented = true });
    }

    public void ImportFromJson(string json)
    {
        try
        {
            var codeSet = JsonSerializer.Deserialize<CodeSetSchema>(json);
            if (codeSet != null)
            {
                SaveCodeSet(codeSet);
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Import failed: {ex.Message}");
        }
    }
}
