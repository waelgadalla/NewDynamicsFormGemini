using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Services;

public interface IUndoRedoService
{
    bool CanUndo { get; }
    bool CanRedo { get; }
    void SaveState(FormModuleSchema module);
    void SaveState(FormWorkflowSchema workflow);
    FormModuleSchema Undo(FormModuleSchema current);
    FormWorkflowSchema Undo(FormWorkflowSchema current);
    FormModuleSchema Redo(FormModuleSchema current);
    FormWorkflowSchema Redo(FormWorkflowSchema current);
    void Clear();
}
