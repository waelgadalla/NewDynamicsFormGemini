using System.Collections.Immutable;
using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Runtime;
using DynamicForms.Core.V4.Services;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

public class EditorStateService : IEditorStateService
{
    private readonly IFormHierarchyService _hierarchyService;
    private readonly ISchemaValidationService _validationService;
    private readonly IUndoRedoService _undoRedo;

    private EditorState _state = new();

    public event Action? OnStateChanged;
    public event Action<string?>? OnFieldSelected;
    public event Action? OnModuleChanged;
    public event Action? OnWorkflowChanged;
    public event Action<EditorView>? OnViewChanged;

    private EditorView _currentView = EditorView.Design;

    public EditorStateService(
        IFormHierarchyService hierarchyService,
        ISchemaValidationService validationService,
        IUndoRedoService undoRedo)
    {
        _hierarchyService = hierarchyService;
        _validationService = validationService;
        _undoRedo = undoRedo;
    }

    // === Accessors ===
    public FormWorkflowSchema? CurrentWorkflow => _state.Workflow;
    public FormModuleSchema? CurrentModule => _state.Module;
    public FormModuleRuntime? CurrentModuleRuntime => _state.ModuleRuntime;
    public string? SelectedFieldId => _state.SelectedFieldId;
    public FormFieldSchema? SelectedField => _state.SelectedField;
    public FormFieldNode? SelectedFieldNode => _state.SelectedFieldNode;
    public IReadOnlyList<ValidationIssue> ValidationIssues => _state.Issues;
    public bool CanUndo => _undoRedo.CanUndo;
    public bool CanRedo => _undoRedo.CanRedo;
    public bool HasClipboard => _state.ClipboardField is not null;
    public EditorView CurrentView => _currentView;

    // === Workflow Operations ===
    public void LoadWorkflow(FormWorkflowSchema workflow)
    {
        _state = _state with { Workflow = workflow };
        OnWorkflowChanged?.Invoke();
        OnStateChanged?.Invoke();
    }

    public void UpdateWorkflow(FormWorkflowSchema workflow)
    {
        _state = _state with { Workflow = workflow };
        OnWorkflowChanged?.Invoke();
        OnStateChanged?.Invoke();
    }

    public void CreateNewWorkflow(string titleEn, string? titleFr = null)
    {
        var workflow = new FormWorkflowSchema
        {
            Id = GenerateNewId(),
            TitleEn = titleEn,
            TitleFr = titleFr,
            ModuleIds = Array.Empty<int>()
        };
        LoadWorkflow(workflow);
    }

    // === Module Operations ===
    public void LoadModule(FormModuleSchema module)
    {
        var runtime = _hierarchyService.BuildHierarchyAsync(module).GetAwaiter().GetResult();
        var issues = _validationService.Validate(module);

        _state = _state with
        {
            Module = module,
            ModuleRuntime = runtime,
            Issues = issues.ToImmutableList(),
            SelectedFieldId = null
        };

        OnModuleChanged?.Invoke();
        OnStateChanged?.Invoke();
    }

    public void UpdateModule(FormModuleSchema module)
    {
        if (_state.Module is not null)
        {
            _undoRedo.SaveState(_state.Module);
        }
        LoadModule(module);
    }

    public FormModuleSchema CreateNewModule(string titleEn, string? titleFr = null)
    {
        var module = new FormModuleSchema
        {
            Id = GenerateNewId(),
            TitleEn = titleEn,
            TitleFr = titleFr,
            Fields = Array.Empty<FormFieldSchema>()
        };
        LoadModule(module);
        return module;
    }

    public void ClearModule()
    {
        _state = _state with
        {
            Module = null,
            ModuleRuntime = null,
            SelectedFieldId = null,
            Issues = ImmutableList<ValidationIssue>.Empty
        };
        OnModuleChanged?.Invoke();
        OnStateChanged?.Invoke();
    }

    // === Field Operations ===
    public void SelectField(string? fieldId)
    {
        if (_state.SelectedFieldId == fieldId) return;

        _state = _state with { SelectedFieldId = fieldId };
        OnFieldSelected?.Invoke(fieldId);
        OnStateChanged?.Invoke();
    }

    public void AddField(string fieldType, string? parentId = null, int? insertAtOrder = null)
    {
        if (_state.Module is null) return;

        var newField = CreateDefaultField(fieldType, parentId, insertAtOrder);
        var newFields = _state.Module.Fields.Append(newField).ToArray();
        var newModule = _state.Module with { Fields = newFields };

        UpdateModule(newModule);
        SelectField(newField.Id);
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

        // Also delete children
        var idsToDelete = GetFieldAndDescendantIds(fieldId);
        var newFields = _state.Module.Fields
            .Where(f => !idsToDelete.Contains(f.Id))
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };

        if (_state.SelectedFieldId == fieldId || idsToDelete.Contains(_state.SelectedFieldId ?? ""))
        {
            _state = _state with { SelectedFieldId = null };
        }

        UpdateModule(newModule);
    }

    public void DuplicateField(string fieldId)
    {
        if (_state.Module is null) return;

        var originalField = _state.Module.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (originalField is null) return;

        var newId = GenerateFieldId(originalField.FieldType);
        var duplicatedField = originalField with
        {
            Id = newId,
            Order = originalField.Order + 1,
            LabelEn = $"{originalField.LabelEn} (Copy)"
        };

        // Shift orders of siblings after this field
        var newFields = _state.Module.Fields
            .Select(f =>
            {
                if (f.ParentId == originalField.ParentId && f.Order > originalField.Order)
                    return f with { Order = f.Order + 1 };
                return f;
            })
            .Append(duplicatedField)
            .ToArray();

        var newModule = _state.Module with { Fields = newFields };
        UpdateModule(newModule);
        SelectField(newId);
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

        // Swap orders
        var otherField = siblings[newIndex];
        var newOrder = otherField.Order;
        var otherNewOrder = field.Order;

        var newFields = _state.Module.Fields
            .Select(f => f.Id == fieldId ? f with { Order = newOrder } :
                         f.Id == otherField.Id ? f with { Order = otherNewOrder } : f)
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };

        UpdateModule(newModule);
    }

    public void ChangeFieldParent(string fieldId, string? newParentId)
    {
        if (_state.Module is null) return;

        var field = _state.Module.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (field is null) return;

        // Prevent circular reference
        if (newParentId is not null)
        {
            var descendants = GetFieldAndDescendantIds(fieldId);
            if (descendants.Contains(newParentId)) return;
        }

        var maxOrder = _state.Module.Fields
            .Where(f => f.ParentId == newParentId)
            .Select(f => f.Order)
            .DefaultIfEmpty(0)
            .Max();

        var newFields = _state.Module.Fields
            .Select(f => f.Id == fieldId ? f with { ParentId = newParentId, Order = maxOrder + 1 } : f)
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };

        UpdateModule(newModule);
    }

    // === Clipboard ===
    public void CopyField(string fieldId)
    {
        var field = _state.Module?.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (field is not null)
        {
            _state = _state with { ClipboardField = field };
            OnStateChanged?.Invoke();
        }
    }

    public void PasteField(string? parentId = null)
    {
        if (_state.Module is null || _state.ClipboardField is null) return;

        var newId = GenerateFieldId(_state.ClipboardField.FieldType);
        var maxOrder = _state.Module.Fields
            .Where(f => f.ParentId == parentId)
            .Select(f => f.Order)
            .DefaultIfEmpty(0)
            .Max();

        var pastedField = _state.ClipboardField with
        {
            Id = newId,
            ParentId = parentId,
            Order = maxOrder + 1,
            LabelEn = $"{_state.ClipboardField.LabelEn} (Pasted)"
        };

        var newFields = _state.Module.Fields.Append(pastedField).ToArray();
        var newModule = _state.Module with { Fields = newFields };

        UpdateModule(newModule);
        SelectField(newId);
    }

    // === Validation ===
    public void RefreshValidation()
    {
        if (_state.Module is null) return;

        var issues = _validationService.Validate(_state.Module);
        _state = _state with { Issues = issues.ToImmutableList() };
        OnStateChanged?.Invoke();
    }

    // === Undo/Redo ===
    public void Undo()
    {
        if (!_undoRedo.CanUndo || _state.Module is null) return;

        var previousModule = _undoRedo.Undo(_state.Module);
        LoadModule(previousModule);
    }

    public void Redo()
    {
        if (!_undoRedo.CanRedo || _state.Module is null) return;

        var nextModule = _undoRedo.Redo(_state.Module);
        LoadModule(nextModule);
    }

    // === View ===
    public void SetView(EditorView view)
    {
        if (_currentView == view) return;

        _currentView = view;
        OnViewChanged?.Invoke(view);
        OnStateChanged?.Invoke();
    }

    // === Helpers ===
    private FormFieldSchema CreateDefaultField(string fieldType, string? parentId, int? insertAtOrder = null)
    {
        int order;
        if (insertAtOrder.HasValue)
        {
            order = insertAtOrder.Value;
            // Shift existing fields
        }
        else
        {
            order = (_state.Module?.Fields
                .Where(f => f.ParentId == parentId)
                .Select(f => f.Order)
                .DefaultIfEmpty(0)
                .Max() ?? 0) + 1;
        }

        var id = GenerateFieldId(fieldType);
        var definition = FieldTypeDefinition.GetByType(fieldType);

        return new FormFieldSchema
        {
            Id = id,
            FieldType = fieldType,
            LabelEn = definition?.DisplayName ?? $"New {fieldType}",
            ParentId = parentId,
            Order = order
        };
    }

    private string GenerateFieldId(string fieldType)
    {
        var baseName = fieldType.ToLowerInvariant();
        var count = 1;
        var id = $"{baseName}_{count}";

        while (_state.Module?.Fields.Any(f => f.Id == id) == true)
        {
            count++;
            id = $"{baseName}_{count}";
        }

        return id;
    }

    private int GenerateNewId()
    {
        return Random.Shared.Next(1, int.MaxValue);
    }

    private HashSet<string> GetFieldAndDescendantIds(string fieldId)
    {
        var ids = new HashSet<string> { fieldId };
        var queue = new Queue<string>();
        queue.Enqueue(fieldId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = _state.Module?.Fields
                .Where(f => f.ParentId == currentId)
                .Select(f => f.Id) ?? Enumerable.Empty<string>();

            foreach (var childId in children)
            {
                ids.Add(childId);
                queue.Enqueue(childId);
            }
        }

        return ids;
    }
}
