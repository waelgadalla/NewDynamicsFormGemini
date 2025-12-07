using DynamicForms.Core.V4.Schemas;
using System.Text.Json;

namespace DynamicForms.Editor.Services;

public class EditorStateService
{
    private FormModuleSchema _currentModule;
    public FormModuleSchema CurrentModule
    {
        get => _currentModule;
        private set
        {
            _currentModule = value;
            OnModuleChanged?.Invoke();
        }
    }

    public FormFieldSchema? SelectedField { get; private set; }

    public event Action? OnModuleChanged;
    public event Action? OnSelectionChanged;

    public EditorStateService()
    {
        // Initialize with a blank module
        _currentModule = FormModuleSchema.Create(
            id: 1,
            titleEn: "New Form Module"
        );
    }

    public void SelectField(string? fieldId)
    {
        if (fieldId == null)
        {
            SelectedField = null;
        }
        else
        {
            SelectedField = FindFieldRecursive(_currentModule.Fields, fieldId);
        }
        OnSelectionChanged?.Invoke();
    }

    public void AddField(FormFieldSchema field)
    {
        var newFields = _currentModule.Fields.ToList();
        
        // Ensure ID Uniqueness
        if (FindFieldRecursive(newFields, field.Id) != null)
        {
            field = field with { Id = $"{field.Id}_{Guid.NewGuid().ToString().Substring(0, 4)}" };
        }

        newFields.Add(field);
        
        CurrentModule = _currentModule with { Fields = newFields.ToArray() };
        SelectField(field.Id);
    }

    public void RemoveField(string fieldId)
    {
        // TODO: Handle recursive remove if needed
        var newFields = _currentModule.Fields.Where(f => f.Id != fieldId).ToList();
        
        // Also remove children if any (cascade delete for simplicity in editor)
        var children = GetChildren(fieldId);
        foreach(var child in children)
        {
             newFields.RemoveAll(f => f.Id == child.Id);
        }

        CurrentModule = _currentModule with { Fields = newFields.ToArray() };
        
        if (SelectedField?.Id == fieldId)
        {
            SelectField(null);
        }
    }

    public void UpdateField(FormFieldSchema updatedField)
    {
        bool found;
        var newFields = UpdateFieldListRecursive(_currentModule.Fields, updatedField, out found);
        
        if (found)
        {
            CurrentModule = _currentModule with { Fields = newFields };
            SelectedField = updatedField; // Update selection reference
            OnSelectionChanged?.Invoke();
        }
    }

    public IEnumerable<FormFieldSchema> GetChildren(string? parentId)
    {
        return _currentModule.Fields
            .Where(f => f.ParentId == parentId)
            .OrderBy(f => f.Order);
    }

    public void LoadModule(FormModuleSchema module)
    {
        CurrentModule = module;
        SelectField(null);
    }

    // --- Private Helpers ---

    private FormFieldSchema? FindFieldRecursive(IEnumerable<FormFieldSchema> fields, string id)
    {
        foreach (var f in fields)
        {
            if (f.Id == id) return f;
            
            if (f.TypeConfig is DataGridConfig grid)
            {
                var found = FindFieldRecursive(grid.Columns, id);
                if (found != null) return found;
            }
        }
        return null;
    }

    private FormFieldSchema[] UpdateFieldListRecursive(IEnumerable<FormFieldSchema> fields, FormFieldSchema updatedField, out bool found)
    {
        found = false;
        var list = fields.ToList();
        
        for (int i = 0; i < list.Count; i++)
        {
            var f = list[i];
            if (f.Id == updatedField.Id)
            {
                list[i] = updatedField;
                found = true;
                return list.ToArray();
            }
            
            if (f.TypeConfig is DataGridConfig grid)
            {
                bool subFound;
                var newColumns = UpdateFieldListRecursive(grid.Columns, updatedField, out subFound);
                if (subFound)
                {
                    var newGrid = grid with { Columns = newColumns };
                    list[i] = f with { TypeConfig = newGrid };
                    found = true;
                    return list.ToArray();
                }
            }
        }
        return list.ToArray();
    }
}