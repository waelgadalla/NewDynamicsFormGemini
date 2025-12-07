using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Runtime;
using DynamicForms.Core.V4.Services; // Add this line
using DynamicForms.Editor.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DynamicForms.Core.V4.Builders; // For FormFieldBuilder.CreateTextField etc.

namespace DynamicForms.Editor.Services;

public class EditorStateService : IEditorStateService
{
    private readonly IFormHierarchyService _hierarchyService;
    private readonly ISchemaValidationService _validationService;
    private readonly IUndoRedoService _undoRedo;
    private readonly IToastService _toastService; // Inject toast service for notifications

    private EditorState _state = new();

    public event Action? OnStateChanged;
    public event Action<string>? OnFieldSelected;
    public event Action? OnModuleChanged;

    public EditorStateService(
        IFormHierarchyService hierarchyService,
        ISchemaValidationService validationService,
        IUndoRedoService undoRedo,
        IToastService toastService)
    {
        _hierarchyService = hierarchyService;
        _validationService = validationService;
        _undoRedo = undoRedo;
        _toastService = toastService;

        // Initialize with a default module if none is loaded
        if (_state.Module is null)
        {
            LoadModule(FormModuleSchema.Create(id: 1, titleEn: "New Form Module"));
        }
    }

    // === Accessors ===
    public FormWorkflowSchema? CurrentWorkflow => _state.Workflow;
    public FormModuleSchema? CurrentModule => _state.Module;
    public FormModuleRuntime? CurrentModuleRuntime => _state.ModuleRuntime;
    public string? SelectedFieldId => _state.SelectedFieldId;
    public FormFieldSchema? SelectedField => _state.SelectedField;
    public IReadOnlyList<ValidationIssue> ValidationIssues => _state.Issues;
    public bool CanUndo => _undoRedo.CanUndo;
    public bool CanRedo => _undoRedo.CanRedo;
    public bool HasClipboard => _state.ClipboardField is not null;

    // === State Management Helpers ===
    private void SetState(EditorState newState, bool suppressStateChanged = false)
    {
        _state = newState;
        if (!suppressStateChanged)
        {
            OnStateChanged?.Invoke();
        }
    }

    // === Workflow Operations (Placeholders for now, will be implemented in Phase 4) ===
    public void LoadWorkflow(FormWorkflowSchema workflow) { /* TODO: Implement in Phase 4 */ }
    public void UpdateWorkflow(FormWorkflowSchema workflow) { /* TODO: Implement in Phase 4 */ }
    public void AddModuleToWorkflow(int moduleId) { /* TODO: Implement in Phase 4 */ }
    public void RemoveModuleFromWorkflow(int moduleId) { /* TODO: Implement in Phase 4 */ }
    public void ReorderModules(int[] newOrder) { /* TODO: Implement in Phase 4 */ }

    // === Module Operations ===
    public void LoadModule(FormModuleSchema module)
    {
        _undoRedo.SaveState(module); // Save initial state for undo
        UpdateModuleState(module);
        _toastService.ShowInfo($"Module '{module.TitleEn}' loaded.");
    }

    public void UpdateModule(FormModuleSchema module)
    {
        _undoRedo.SaveState(_state.Module!); // Save current state before updating
        UpdateModuleState(module);
        _toastService.ShowSuccess($"Module '{module.TitleEn}' updated.");
    }

    private void UpdateModuleState(FormModuleSchema module)
    {
        var runtime = _hierarchyService.BuildHierarchyAsync(module).GetAwaiter().GetResult();
        var issues = _validationService.Validate(module);

        SetState(_state with
        {
            Module = module,
            ModuleRuntime = runtime,
            Issues = issues.ToImmutableList(),
            SelectedFieldId = null // Clear selection on full module reload/update
        });

        OnModuleChanged?.Invoke();
    }

    public FormModuleSchema CreateNewModule(string titleEn, string? titleFr = null)
    {
        var newModule = FormModuleSchema.Create(
            id: new Random().Next(1000, 9999), // Simple ID for now
            titleEn: titleEn,
            titleFr: titleFr
        );
        LoadModule(newModule);
        return newModule;
    }

    // === Field Operations ===
    public void SelectField(string? fieldId)
    {
        if (_state.SelectedFieldId == fieldId) return;

        SetState(_state with { SelectedFieldId = fieldId });
        OnFieldSelected?.Invoke(fieldId ?? "");
    }

    public void AddField(string fieldType, string? parentId = null)
    {
        if (_state.Module is null) return;

        var fieldDef = FieldTypeDefinition.GetByType(fieldType);
        if (fieldDef is null)
        {
            _toastService.ShowError($"Unknown field type: {fieldType}");
            return;
        }

        var newField = CreateDefaultField(fieldType, parentId);
        var newFields = _state.Module.Fields.Append(newField).ToArray();
        var newModule = _state.Module with { Fields = newFields };

        UpdateModule(newModule);
        SelectField(newField.Id);
        _toastService.ShowSuccess($"Added '{fieldDef.DisplayName}' field.");
    }

    public void UpdateField(FormFieldSchema field)
    {
        if (_state.Module is null) return;

        var newFields = _state.Module.Fields
            .Select(f => f.Id == field.Id ? field : f)
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };

        UpdateModule(newModule);
    }

    public void DeleteField(string fieldId)
    {
        if (_state.Module is null) return;

        // Also delete children (recursive)
        var idsToDelete = GetFieldAndDescendantIds(fieldId);
        var newFields = _state.Module.Fields
            .Where(f => !idsToDelete.Contains(f.Id))
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };

        if (_state.SelectedFieldId == fieldId)
        {
            SetState(_state with { SelectedFieldId = null }, suppressStateChanged: true); // Update internally without triggering full refresh before module update
        }
        
        UpdateModule(newModule);
        _toastService.ShowWarning($"Deleted field '{fieldId}' and its descendants.");
    }

    public void DuplicateField(string fieldId)
    {
        if (_state.Module is null) return;
        var fieldToDuplicate = _state.Module.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (fieldToDuplicate is null)
        {
            _toastService.ShowError($"Field '{fieldId}' not found for duplication.");
            return;
        }

        var duplicatedField = fieldToDuplicate with
        {
            Id = GenerateFieldId(fieldToDuplicate.FieldType), // Generate new unique ID
            LabelEn = $"Copy of {fieldToDuplicate.LabelEn}",
            LabelFr = fieldToDuplicate.LabelFr is not null ? $"Copie de {fieldToDuplicate.LabelFr}" : null,
            Order = fieldToDuplicate.Order + 1 // Place right after original
        };

        var newFields = _state.Module.Fields.ToList();
        var originalIndex = newFields.FindIndex(f => f.Id == fieldId);
        if (originalIndex != -1)
        {
            newFields.Insert(originalIndex + 1, duplicatedField);
        }
        else
        {
            newFields.Add(duplicatedField);
        }

        var newModule = _state.Module with { Fields = newFields.ToArray() };
        UpdateModule(newModule);
        SelectField(duplicatedField.Id);
        _toastService.ShowSuccess($"Duplicated field '{fieldId}'. New ID: '{duplicatedField.Id}'");
    }


    public void MoveField(string fieldId, MoveDirection direction)
    {
        if (_state.Module is null) return;
        
        var field = _state.Module.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (field is null) return;
        
        // Get siblings (fields with same parent)
        var siblings = _state.Module.Fields
            .Where(f => f.ParentId == field.ParentId)
            .OrderBy(f => f.Order)
            .ToList();
        
        var index = siblings.FindIndex(f => f.Id == fieldId);
        var newIndex = direction == MoveDirection.Up ? index - 1 : index + 1;
        
        if (newIndex < 0 || newIndex >= siblings.Count) return;
        
        // Temporarily adjust orders and rebuild to ensure uniqueness and correct sorting
        // More robust approach would be to swap elements and then reassign orders based on new positions
        var fieldToMove = siblings[index];
        var otherField = siblings[newIndex];

        // Perform a swap by updating order property, then re-sort all fields
        var allFields = _state.Module.Fields.ToList();
        
        var updatedAllFields = allFields
            .Select(f =>
            {
                if (f.Id == fieldToMove.Id) return f with { Order = otherField.Order };
                if (f.Id == otherField.Id) return f with { Order = fieldToMove.Order };
                return f;
            })
            .ToList();

        // Re-sort all fields based on their new order properties to ensure a stable ordering
        updatedAllFields.Sort((a, b) => a.Order.CompareTo(b.Order));

        // Reassign explicit orders to be sequential (1, 2, 3...) within their parent groups
        var reorderedFields = new List<FormFieldSchema>();
        var groupedFields = updatedAllFields.GroupBy(f => f.ParentId);
        foreach (var group in groupedFields)
        {
            var currentOrder = 1;
            foreach (var f in group.OrderBy(f => f.Order))
            {
                reorderedFields.Add(f with { Order = currentOrder++ });
            }
        }

        var newModule = _state.Module with { Fields = reorderedFields.ToArray() };
        UpdateModule(newModule);
        _toastService.ShowInfo($"Moved field '{fieldId}' {(direction == MoveDirection.Up ? "up" : "down")}.");
    }


    public void ChangeFieldParent(string fieldId, string? newParentId)
    {
        if (_state.Module is null) return;
        var fieldToMove = _state.Module.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (fieldToMove is null) return;

        // Ensure new parent exists if not null
        if (newParentId is not null && !_state.Module.Fields.Any(f => f.Id == newParentId))
        {
            _toastService.ShowError($"New parent field '{newParentId}' not found.");
            return;
        }
        
        if (newParentId == fieldToMove.Id)
        {
            _toastService.ShowError("Cannot set a field as its own parent.");
            return;
        }

        // Prevent circular references
        if (newParentId is not null)
        {
            var tempModule = _state.Module with { Fields = _state.Module.Fields.Select(f => f.Id == fieldId ? f with { ParentId = newParentId } : f).ToArray() };
            if (HasCircularReference(fieldToMove with { ParentId = newParentId }, tempModule))
            {
                _toastService.ShowError("Changing parent would create a circular reference.");
                return;
            }
        }

        var newMaxOrder = _state.Module.Fields
                            .Where(f => f.ParentId == newParentId)
                            .Select(f => f.Order)
                            .DefaultIfEmpty(0)
                            .Max() + 1;

        var newFields = _state.Module.Fields
            .Select(f => f.Id == fieldId ? f with { ParentId = newParentId, Order = newMaxOrder } : f)
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };
        UpdateModule(newModule);
        _toastService.ShowInfo($"Moved field '{fieldId}' to parent '{newParentId ?? "root"}'.");
    }

    // === Clipboard Operations (Placeholders for now) ===
    public void CopyField(string fieldId)
    {
        if (_state.Module is null) return;
        var fieldToCopy = _state.Module.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (fieldToCopy is null) return;

        // Deep copy the field to clipboard
        // Need a better deep copy mechanism for records with nested records
        var json = System.Text.Json.JsonSerializer.Serialize(fieldToCopy);
        var clipboardField = System.Text.Json.JsonSerializer.Deserialize<FormFieldSchema>(json);

        SetState(_state with { ClipboardField = clipboardField });
        _toastService.ShowInfo($"Copied field '{fieldId}' to clipboard.");
    }

    public void PasteField(string? parentId = null)
    {
        if (_state.ClipboardField is null)
        {
            _toastService.ShowWarning("Clipboard is empty.");
            return;
        }
        if (_state.Module is null) return;

        var pastedField = _state.ClipboardField with
        {
            Id = GenerateFieldId(_state.ClipboardField.FieldType), // New ID
            ParentId = parentId,
            LabelEn = $"Pasted {_state.ClipboardField.LabelEn}",
            LabelFr = _state.ClipboardField.LabelFr is not null ? $"Collé {_state.ClipboardField.LabelFr}" : null,
            Order = (_state.Module.Fields.Where(f => f.ParentId == parentId).Select(f => f.Order).DefaultIfEmpty(0).Max() + 1)
        };

        var newFields = _state.Module.Fields.Append(pastedField).ToArray();
        var newModule = _state.Module with { Fields = newFields };
        UpdateModule(newModule);
        SelectField(pastedField.Id);
        _toastService.ShowSuccess($"Pasted field '{pastedField.Id}'.");
    }

    // === Validation ===
    public void RefreshValidation()
    {
        if (_state.Module is null) return;
        var issues = _validationService.Validate(_state.Module);
        SetState(_state with { Issues = issues.ToImmutableList() });
        _toastService.ShowInfo("Validation refreshed.");
    }

    // === Undo/Redo ===
    public void Undo()
    {
        if (!CanUndo)
        {
            _toastService.ShowWarning("Nothing to undo.");
            return;
        }
        var previousModule = _undoRedo.Undo(_state.Module!);
        UpdateModuleState(previousModule); // Update state without saving to undo again
        _toastService.ShowInfo("Undo successful.");
    }

    public void Redo()
    {
        if (!CanRedo)
        {
            _toastService.ShowWarning("Nothing to redo.");
            return;
        }
        var nextModule = _undoRedo.Redo(_state.Module!);
        UpdateModuleState(nextModule); // Update state without saving to undo again
        _toastService.ShowInfo("Redo successful.");
    }

    // === Helpers ===
    private FormFieldSchema CreateDefaultField(string fieldType, string? parentId)
    {
        var maxOrder = _state.Module!.Fields
            .Where(f => f.ParentId == parentId)
            .Select(f => f.Order)
            .DefaultIfEmpty(0)
            .Max();

        var id = GenerateFieldId(fieldType);

        return new FormFieldSchema
        {
            Id = id,
            FieldType = fieldType,
            LabelEn = $"New {fieldType}",
            ParentId = parentId,
            Order = maxOrder + 1
        };
    }

    private string GenerateFieldId(string fieldType)
    {
        var baseName = fieldType.ToLowerInvariant().Replace(" ", "");
        var count = 1;
        string id;
        do
        {
            id = $"{baseName}_{count}";
            count++;
        } while (_state.Module!.Fields.Any(f => f.Id == id));
        
        return id;
    }

    private HashSet<string> GetFieldAndDescendantIds(string fieldId)
    {
        var ids = new HashSet<string> { fieldId };
        var queue = new Queue<string>();
        queue.Enqueue(fieldId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = _state.Module!.Fields
                .Where(f => f.ParentId == currentId)
                .Select(f => f.Id);

            foreach (var childId in children)
            {
                ids.Add(childId);
                queue.Enqueue(childId);
            }
        }
        return ids;
    }
    
    // Helper to check for circular references (copied from SchemaValidationService)
    private bool HasCircularReference(FormFieldSchema field, FormModuleSchema module)
    {
        var visited = new HashSet<string>();
        var current = field;
        
        while (current?.ParentId is not null)
        {
            if (!visited.Add(current.Id))
                return true;
            
            current = module.Fields.FirstOrDefault(f => f.Id == current.ParentId);
        }
        
        return false;
    }
}
