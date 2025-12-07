using DynamicForms.Core.V4.Schemas;
using System.Collections.Generic;
using System;

namespace DynamicForms.Editor.Services;

public class UndoRedoService : IUndoRedoService
{
    private readonly Stack<FormModuleSchema> _undoStack = new();
    private readonly Stack<FormModuleSchema> _redoStack = new();
    private readonly Stack<FormWorkflowSchema> _undoWorkflowStack = new();
    private readonly Stack<FormWorkflowSchema> _redoWorkflowStack = new();
    private const int MaxStackSize = 50;

    public bool CanUndo => _undoStack.Count > 0 || _undoWorkflowStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0 || _redoWorkflowStack.Count > 0;

    public void SaveState(FormModuleSchema module)
    {
        _redoStack.Clear();
        _undoStack.Push(module);
        LimitStack(_undoStack);
    }

    public void SaveState(FormWorkflowSchema workflow)
    {
        _redoWorkflowStack.Clear();
        _undoWorkflowStack.Push(workflow);
        LimitStack(_undoWorkflowStack);
    }

    public FormModuleSchema Undo(FormModuleSchema current)
    {
        if (_undoStack.Count == 0) throw new InvalidOperationException("Nothing to undo for Module");
        _redoStack.Push(current);
        return _undoStack.Pop();
    }

    public FormWorkflowSchema Undo(FormWorkflowSchema current)
    {
        if (_undoWorkflowStack.Count == 0) throw new InvalidOperationException("Nothing to undo for Workflow");
        _redoWorkflowStack.Push(current);
        return _undoWorkflowStack.Pop();
    }

    public FormModuleSchema Redo(FormModuleSchema current)
    {
        if (_redoStack.Count == 0) throw new InvalidOperationException("Nothing to redo for Module");
        _undoStack.Push(current);
        return _redoStack.Pop();
    }

    public FormWorkflowSchema Redo(FormWorkflowSchema current)
    {
        if (_redoWorkflowStack.Count == 0) throw new InvalidOperationException("Nothing to redo for Workflow");
        _undoWorkflowStack.Push(current);
        return _redoWorkflowStack.Pop();
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _undoWorkflowStack.Clear();
        _redoWorkflowStack.Clear();
    }

    private void LimitStack<T>(Stack<T> stack)
    {
        if (stack.Count > MaxStackSize)
        {
            var items = stack.ToList();
            items.RemoveAt(0);
            stack.Clear();
            foreach (var item in items) stack.Push(item);
        }
    }
}
