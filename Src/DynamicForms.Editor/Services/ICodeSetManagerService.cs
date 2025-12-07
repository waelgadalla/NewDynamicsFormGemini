using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Services;

public interface ICodeSetManagerService
{
    // State
    CodeSetSchema? SelectedCodeSet { get; }
    IEnumerable<CodeSetSchema> CodeSets { get; }
    bool IsDirty { get; }

    // Events
    event Action? OnStateChanged;

    // Operations
    Task LoadCodeSetsAsync();
    void SelectCodeSet(int? id);
    void CreateNewCodeSet();
    void UpdateSelectedCodeSet(CodeSetSchema updatedCodeSet);
    void SaveCodeSet(CodeSetSchema codeSet);
    void DeleteCodeSet(int id);
    
    // Import/Export
    string ExportToJson(CodeSetSchema codeSet);
    void ImportFromJson(string json);
}
