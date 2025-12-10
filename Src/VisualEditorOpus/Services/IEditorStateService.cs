using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Runtime;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Central state management service for the form editor.
/// Handles current workflow/module, field selection, clipboard, undo/redo, and validation.
/// </summary>
public interface IEditorStateService
{
    // === Current State ===
    FormWorkflowSchema? CurrentWorkflow { get; }
    FormModuleSchema? CurrentModule { get; }
    FormModuleRuntime? CurrentModuleRuntime { get; }
    string? SelectedFieldId { get; }
    FormFieldSchema? SelectedField { get; }
    FormFieldNode? SelectedFieldNode { get; }
    EditorView CurrentView { get; }

    // === Events ===
    event Action? OnStateChanged;
    event Action<string?>? OnFieldSelected;
    event Action? OnModuleChanged;
    event Action? OnWorkflowChanged;
    event Action<EditorView>? OnViewChanged;

    // === Workflow Operations ===
    void LoadWorkflow(FormWorkflowSchema workflow);
    void UpdateWorkflow(FormWorkflowSchema workflow);
    void CreateNewWorkflow(string titleEn, string? titleFr = null);

    // === Module Operations ===
    void LoadModule(FormModuleSchema module);
    void UpdateModule(FormModuleSchema module);
    FormModuleSchema CreateNewModule(string titleEn, string? titleFr = null);
    void ClearModule();

    // === Field Operations ===
    void SelectField(string? fieldId);
    void AddField(string fieldType, string? parentId = null, int? insertAtOrder = null);
    void UpdateField(FormFieldSchema field);
    void DeleteField(string fieldId);
    void DuplicateField(string fieldId);
    void MoveField(string fieldId, MoveDirection direction);
    void ChangeFieldParent(string fieldId, string? newParentId);

    // === Clipboard ===
    void CopyField(string fieldId);
    void PasteField(string? parentId = null);
    bool HasClipboard { get; }

    // === Validation ===
    IReadOnlyList<ValidationIssue> ValidationIssues { get; }
    void RefreshValidation();

    // === Undo/Redo ===
    bool CanUndo { get; }
    bool CanRedo { get; }
    void Undo();
    void Redo();

    // === View ===
    void SetView(EditorView view);
}

public enum MoveDirection { Up, Down }
