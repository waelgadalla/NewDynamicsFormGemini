using DynamicForms.Core.V4.Schemas;

namespace VisualEditorOpus.Services;

public class UndoRedoService : IUndoRedoService
{
    private readonly Stack<FormModuleSchema> _undoStack = new();
    private readonly Stack<FormModuleSchema> _redoStack = new();
    private const int MaxStackSize = 50;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void SaveState(FormModuleSchema module)
    {
        _undoStack.Push(module);
        _redoStack.Clear();

        // Limit stack size
        if (_undoStack.Count > MaxStackSize)
        {
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = 0; i < MaxStackSize; i++)
            {
                _undoStack.Push(items[i]);
            }
        }
    }

    public FormModuleSchema Undo(FormModuleSchema current)
    {
        if (!CanUndo)
            throw new InvalidOperationException("Nothing to undo");

        _redoStack.Push(current);
        return _undoStack.Pop();
    }

    public FormModuleSchema Redo(FormModuleSchema current)
    {
        if (!CanRedo)
            throw new InvalidOperationException("Nothing to redo");

        _undoStack.Push(current);
        return _redoStack.Pop();
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
