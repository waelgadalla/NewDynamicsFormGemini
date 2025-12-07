using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Services;

public interface IUndoRedoService
{
    bool CanUndo { get; }
    bool CanRedo { get; }
    void SaveState(FormModuleSchema module);
    FormModuleSchema Undo(FormModuleSchema current);
    FormModuleSchema Redo(FormModuleSchema current);
    void Clear();
}
