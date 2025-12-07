using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Runtime;
using DynamicForms.Editor.Models;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DynamicForms.Editor.Services;

public interface IEditorStateService
{
    // === Current State ===
    FormWorkflowSchema? CurrentWorkflow { get; }
    FormModuleSchema? CurrentModule { get; }
    FormModuleRuntime? CurrentModuleRuntime { get; }
    string? SelectedFieldId { get; }
    FormFieldSchema? SelectedField { get; }
    string? SelectedNodeId { get; }
    
    // === Events ===
    event Action? OnStateChanged;
    event Action<string>? OnFieldSelected;
    event Action? OnModuleChanged;
    
    // === Workflow Operations ===
    void LoadWorkflow(FormWorkflowSchema workflow);
    void UpdateWorkflow(FormWorkflowSchema workflow);
    void SelectNode(string? nodeId);
    void AddModuleToWorkflow(int moduleId);
    void RemoveModuleFromWorkflow(int moduleId);
    void ReorderModules(int[] newOrder);
    
    // === Workflow Layout Operations ===
    List<WorkflowVisualNode> GetWorkflowNodes();
    List<WorkflowVisualConnection> GetWorkflowConnections(); // New
    void UpdateWorkflowNode(WorkflowVisualNode node); // New
    void UpdateWorkflowNodePosition(string nodeId, double x, double y);
    void AddConnection(string sourceId, string targetId); // New
    void RemoveConnection(string sourceId, string targetId); // New
    void RemoveNode(string nodeId); // New

    // === Module Operations ===
    void LoadModule(FormModuleSchema module);
    void UpdateModule(FormModuleSchema module);
    FormModuleSchema CreateNewModule(string titleEn, string? titleFr = null);
    
    // === Field Operations ===
    void SelectField(string? fieldId);
    void AddField(string fieldType, string? parentId = null);
    void UpdateField(FormFieldSchema field);
    void DeleteField(string fieldId);
    void DuplicateField(string fieldId);
    void MoveField(string fieldId, MoveDirection direction);
    void ChangeFieldParent(string fieldId, string? newParentId);
    
    // === Clipboard ===
    void CopyField(string fieldId);
    void PasteField(string? parentId = null);
    bool HasClipboard { get; }

    // === Import/Export ===
    Task ExportModuleJsonAsync();
    Task ExportWorkflowJsonAsync();
    
    // === Validation ===
    IReadOnlyList<ValidationIssue> ValidationIssues { get; }
    void RefreshValidation();
    
    // === Undo/Redo ===
    bool CanUndo { get; }
    bool CanRedo { get; }
    void Undo();
    void Redo();
}

public enum MoveDirection { Up, Down }
