using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Runtime;
using DynamicForms.Core.V4.Services;
using DynamicForms.Editor.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DynamicForms.Core.V4.Builders;
using System.Text.Json;
using DynamicForms.Editor.Extensions;
using Microsoft.JSInterop;

namespace DynamicForms.Editor.Services;

public class EditorStateService : IEditorStateService
{
    private readonly IFormHierarchyService _hierarchyService;
    private readonly ISchemaValidationService _validationService;
    private readonly IUndoRedoService _undoRedo;
    private readonly IToastService _toastService;
    private readonly IClipboardService _clipboardService;
    private readonly IJsonImportExportService _jsonService;

    private EditorState _state = new();

    public event Action? OnStateChanged;
    public event Action<string>? OnFieldSelected;
    public event Action? OnModuleChanged;

    public EditorStateService(
        IFormHierarchyService hierarchyService,
        ISchemaValidationService validationService,
        IUndoRedoService undoRedo,
        IToastService toastService,
        IClipboardService clipboardService,
        IJsonImportExportService jsonService)
    {
        _hierarchyService = hierarchyService;
        _validationService = validationService;
        _undoRedo = undoRedo;
        _toastService = toastService;
        _clipboardService = clipboardService;
        _jsonService = jsonService;

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
    public string? SelectedNodeId => _state.SelectedNodeId;
    public IReadOnlyList<ValidationIssue> ValidationIssues => _state.Issues;
    public bool CanUndo => _undoRedo.CanUndo;
    public bool CanRedo => _undoRedo.CanRedo;
    public bool HasClipboard => _clipboardService.HasContent;

    // === JS Invokable Methods (Keyboard Shortcuts) ===
    [JSInvokable] public void OnSave() => _toastService.ShowInfo("Save triggered (Not implemented persistence yet)");
    [JSInvokable] public void OnUndo() => Undo();
    [JSInvokable] public void OnRedo() => Redo();
    [JSInvokable] public void OnDelete() {
        if (SelectedFieldId != null) DeleteField(SelectedFieldId);
        else if (SelectedNodeId != null) RemoveNode(SelectedNodeId);
    }
    [JSInvokable] public void OnCopy() {
        if (SelectedFieldId != null) CopyField(SelectedFieldId);
    }
    [JSInvokable] public void OnPaste() {
        if (HasClipboard) PasteField(SelectedField?.ParentId);
    }
    [JSInvokable] public void OnDuplicate() {
        if (SelectedFieldId != null) DuplicateField(SelectedFieldId);
    }
    [JSInvokable] public void OnEscape() {
        SelectField(null);
        SelectNode(null);
    }

    // === Import/Export ===
    public async Task ExportModuleJsonAsync()
    {
        if (CurrentModule == null) return;
        var json = _jsonService.SerializeModule(CurrentModule);
        var fileName = $"{CurrentModule.TitleEn.Replace(" ", "_")}_v{CurrentModule.Version}.json";
        await _jsonService.DownloadFileAsync(fileName, json);
        _toastService.ShowSuccess("Module exported.");
    }

    public async Task ExportWorkflowJsonAsync()
    {
        if (CurrentWorkflow == null) return;
        var json = _jsonService.SerializeWorkflow(CurrentWorkflow);
        var fileName = $"{CurrentWorkflow.TitleEn.Replace(" ", "_")}.json";
        await _jsonService.DownloadFileAsync(fileName, json);
        _toastService.ShowSuccess("Workflow exported.");
    }
    
    // === State Management Helpers ===
    private void SetState(EditorState newState, bool suppressStateChanged = false)
    {
        _state = newState;
        if (!suppressStateChanged)
        {
            OnStateChanged?.Invoke();
        }
    }

    // === Workflow Operations ===
    public void LoadWorkflow(FormWorkflowSchema workflow)
    {
        _undoRedo.SaveState(workflow); // Save initial workflow state
        
        var layout = DeserializeWorkflowLayout(workflow.ExtendedProperties);
        
        UpdateWorkflowState(workflow, layout.Nodes, layout.Connections);
        _toastService.ShowInfo($"Workflow '{workflow.TitleEn}' loaded.");
    }

    public void UpdateWorkflow(FormWorkflowSchema workflow)
    {
        _undoRedo.SaveState(_state.Workflow!); // Save current workflow state before updating
        
        // When updating workflow metadata, preserve current node positions
        UpdateWorkflowState(workflow, _state.WorkflowNodes, _state.WorkflowConnections);
        _toastService.ShowSuccess($"Workflow '{workflow.TitleEn}' updated.");
    }

    private void UpdateWorkflowState(FormWorkflowSchema workflow, List<WorkflowVisualNode>? nodes = null, List<WorkflowVisualConnection>? connections = null)
    {
        SetState(_state with
        {
            Workflow = workflow,
            SelectedNodeId = null, // Clear selection on full workflow reload/update
            WorkflowNodes = nodes ?? new List<WorkflowVisualNode>(),
            WorkflowConnections = connections ?? new List<WorkflowVisualConnection>()
        });
        OnStateChanged?.Invoke();
    }

    public void SelectNode(string? nodeId)
    {
        if (_state.SelectedNodeId == nodeId) return;
        SetState(_state with { SelectedNodeId = nodeId });
    }

    public void AddModuleToWorkflow(int moduleId) { /* TODO: Implement in Phase 4 */ }
    public void RemoveModuleFromWorkflow(int moduleId) { /* TODO: Implement in Phase 4 */ }
    public void ReorderModules(int[] newOrder) { /* TODO: Implement in Phase 4 */ }

    // === Workflow Layout Operations ===
    public List<WorkflowVisualNode> GetWorkflowNodes() => _state.WorkflowNodes;
    public List<WorkflowVisualConnection> GetWorkflowConnections() => _state.WorkflowConnections;

    public void UpdateWorkflowNode(WorkflowVisualNode node)
    {
        var currentNodes = _state.WorkflowNodes.ToList();
        var existing = currentNodes.FirstOrDefault(n => n.Id == node.Id);
        if (existing != null) currentNodes.Remove(existing);
        currentNodes.Add(node);
        SaveWorkflowLayout(currentNodes, _state.WorkflowConnections);
    }

    public void UpdateWorkflowNodePosition(string nodeId, double x, double y)
    {
        var currentNodes = _state.WorkflowNodes.ToList();
        var existing = currentNodes.FirstOrDefault(n => n.Id == nodeId);
        if (existing != null)
        {
            existing.X = x;
            existing.Y = y;
            SaveWorkflowLayout(currentNodes, _state.WorkflowConnections);
        }
    }

    public void AddConnection(string sourceId, string targetId)
    {
        if (_state.WorkflowConnections.Any(c => c.SourceNodeId == sourceId && c.TargetNodeId == targetId)) return;

        var newConnections = _state.WorkflowConnections.ToList();
        newConnections.Add(new WorkflowVisualConnection(sourceId, targetId));
        SaveWorkflowLayout(_state.WorkflowNodes, newConnections);
    }

    public void RemoveConnection(string sourceId, string targetId)
    {
        var newConnections = _state.WorkflowConnections.Where(c => !(c.SourceNodeId == sourceId && c.TargetNodeId == targetId)).ToList();
        SaveWorkflowLayout(_state.WorkflowNodes, newConnections);
    }

    public void RemoveNode(string nodeId)
    {
        var currentNodes = _state.WorkflowNodes.Where(n => n.Id != nodeId).ToList();
        // Also remove any connections involving this node
        var currentConnections = _state.WorkflowConnections
            .Where(c => c.SourceNodeId != nodeId && c.TargetNodeId != nodeId)
            .ToList();
            
        if (_state.SelectedNodeId == nodeId)
        {
            SetState(_state with { SelectedNodeId = null }, suppressStateChanged: true);
        }
        
        SaveWorkflowLayout(currentNodes, currentConnections);
        _toastService.ShowInfo("Node removed.");
    }

    private void SaveWorkflowLayout(List<WorkflowVisualNode> nodes, List<WorkflowVisualConnection> connections)
    {
        SetState(_state with { WorkflowNodes = nodes, WorkflowConnections = connections }, suppressStateChanged: true);

        if (_state.Workflow != null)
        {
             var layoutData = new WorkflowLayoutData { Nodes = nodes, Connections = connections };
             var updatedExtendedProps = SerializeWorkflowLayout(layoutData);
             var updatedWorkflow = _state.Workflow with { ExtendedProperties = updatedExtendedProps };
             _state = _state with { Workflow = updatedWorkflow };
        }
    }

    // === Helpers ===
    private JsonElement? SerializeWorkflowLayout(WorkflowLayoutData layout)
    {
        var json = JsonSerializer.Serialize(layout, JsonSerializerOptionsProvider.Default);
        return JsonDocument.Parse(json).RootElement;
    }

    private WorkflowLayoutData DeserializeWorkflowLayout(JsonElement? jsonElement)
    {
        if (jsonElement == null || jsonElement.Value.ValueKind == JsonValueKind.Null)
        {
            return new WorkflowLayoutData();
        }
        try
        {
            // Try to deserialize as new format
            return JsonSerializer.Deserialize<WorkflowLayoutData>(jsonElement.Value, JsonSerializerOptionsProvider.Default)
                ?? new WorkflowLayoutData();
        }
        catch
        {
            // Fallback for old format (list of nodes)
            try 
            {
                var nodes = JsonSerializer.Deserialize<List<WorkflowVisualNode>>(jsonElement.Value, JsonSerializerOptionsProvider.Default);
                return new WorkflowLayoutData { Nodes = nodes ?? new() };
            }
            catch { return new WorkflowLayoutData(); }
        }
    }

    public class WorkflowLayoutData
    {
        public List<WorkflowVisualNode> Nodes { get; set; } = new();
        public List<WorkflowVisualConnection> Connections { get; set; } = new();
    }

    // === Module Operations ===
    public void LoadModule(FormModuleSchema module)
    {
        _undoRedo.SaveState(module);
        UpdateModuleState(module);
        _toastService.ShowInfo($"Module '{module.TitleEn}' loaded.");
    }

    public void UpdateModule(FormModuleSchema module)
    {
        _undoRedo.SaveState(_state.Module!);
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
            SelectedFieldId = null
        });

        OnModuleChanged?.Invoke();
    }

    public FormModuleSchema CreateNewModule(string titleEn, string? titleFr = null)
    {
        var newModule = FormModuleSchema.Create(
            id: new Random().Next(1000, 9999),
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

        var idsToDelete = GetFieldAndDescendantIds(fieldId);
        var newFields = _state.Module.Fields
            .Where(f => !idsToDelete.Contains(f.Id))
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };

        if (_state.SelectedFieldId == fieldId)
        {
            SetState(_state with { SelectedFieldId = null }, suppressStateChanged: true);
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
            Id = GenerateFieldId(fieldToDuplicate.FieldType),
            LabelEn = $"Copy of {fieldToDuplicate.LabelEn}",
            LabelFr = fieldToDuplicate.LabelFr is not null ? $"Copie de {fieldToDuplicate.LabelFr}" : null,
            Order = fieldToDuplicate.Order + 1
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
        
        var siblings = _state.Module.Fields
            .Where(f => f.ParentId == field.ParentId)
            .OrderBy(f => f.Order)
            .ToList();
        
        var index = siblings.FindIndex(f => f.Id == fieldId);
        var newIndex = direction == MoveDirection.Up ? index - 1 : index + 1;
        
        if (newIndex < 0 || newIndex >= siblings.Count) return;
        
        var fieldToMove = siblings[index];
        var otherField = siblings[newIndex];

        var allFields = _state.Module.Fields.ToList();
        
        var updatedAllFields = allFields
            .Select(f =>
            {
                if (f.Id == fieldToMove.Id) return f with { Order = otherField.Order };
                if (f.Id == otherField.Id) return f with { Order = fieldToMove.Order };
                return f;
            })
            .ToList();

        updatedAllFields.Sort((a, b) => a.Order.CompareTo(b.Order));

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

    public void CopyField(string fieldId)
    {
        if (_state.Module is null) return;
        var fieldToCopy = _state.Module.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (fieldToCopy is null) return;

        _clipboardService.CopyField(fieldToCopy);
        
        // Also copy JSON to system clipboard
        var json = JsonSerializer.Serialize(fieldToCopy, JsonSerializerOptionsProvider.Default);
        _clipboardService.CopyTextToSystemClipboardAsync(json); // Fire and forget
        
        // UI update
        OnStateChanged?.Invoke(); 
        _toastService.ShowInfo($"Copied field '{fieldId}'.");
    }

    public void PasteField(string? parentId = null)
    {
        var clipboardField = _clipboardService.GetField();
        
        if (clipboardField is null)
        {
            _toastService.ShowWarning("Clipboard is empty.");
            return;
        }
        if (_state.Module is null) return;

        var pastedField = clipboardField with
        {
            Id = GenerateFieldId(clipboardField.FieldType),
            ParentId = parentId,
            LabelEn = $"Pasted {clipboardField.LabelEn}",
            LabelFr = clipboardField.LabelFr is not null ? $"Collé {clipboardField.LabelFr}" : null,
            Order = (_state.Module.Fields.Where(f => f.ParentId == parentId).Select(f => f.Order).DefaultIfEmpty(0).Max() + 1)
        };

        var newFields = _state.Module.Fields.Append(pastedField).ToArray();
        var newModule = _state.Module with { Fields = newFields };
        UpdateModule(newModule);
        SelectField(pastedField.Id);
        _toastService.ShowSuccess($"Pasted field '{pastedField.Id}'.");
    }

    public void RefreshValidation()
    {
        if (_state.Module is null) return;
        var issues = _validationService.Validate(_state.Module);
        SetState(_state with { Issues = issues.ToImmutableList() });
        _toastService.ShowInfo("Validation refreshed.");
    }

    public void Undo()
    {
        if (!CanUndo)
        {
            _toastService.ShowWarning("Nothing to undo.");
            return;
        }
        var previousModule = _undoRedo.Undo(_state.Module!);
        UpdateModuleState(previousModule);
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
        UpdateModuleState(nextModule);
        _toastService.ShowInfo("Redo successful.");
    }

    // === Helpers ===
    private JsonElement? SerializeWorkflowNodes(List<WorkflowVisualNode> nodes)
    {
        if (nodes == null || !nodes.Any()) return null;
        var json = JsonSerializer.Serialize(nodes, JsonSerializerOptionsProvider.Default);
        return JsonDocument.Parse(json).RootElement;
    }

    private List<WorkflowVisualNode> DeserializeWorkflowNodes(JsonElement? jsonElement)
    {
        if (jsonElement == null || jsonElement.Value.ValueKind == JsonValueKind.Null)
        {
            return new List<WorkflowVisualNode>();
        }
        try
        {
            return JsonSerializer.Deserialize<List<WorkflowVisualNode>>(jsonElement.Value, JsonSerializerOptionsProvider.Default)
                ?? new List<WorkflowVisualNode>();
        }
        catch (JsonException ex)
        {
            _toastService.ShowError($"Error deserializing node positions: {ex.Message}");
            return new List<WorkflowVisualNode>();
        }
    }

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