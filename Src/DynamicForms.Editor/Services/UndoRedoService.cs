using DynamicForms.Core.V4.Schemas;
using System.Collections.Generic;
using System;

namespace DynamicForms.Editor.Services;

public class UndoRedoService : IUndoRedoService
{
    private readonly Stack<FormModuleSchema> _undoStack = new();
    private readonly Stack<FormModuleSchema> _redoStack = new();
    private const int MaxStackSize = 50;
    
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    
    public void SaveState(FormModuleSchema module)
    {
        // Clear redo stack on new action
        _redoStack.Clear();
        
        // Push current state to undo stack
        // IMPORTANT: Create a deep clone if FormModuleSchema is mutable in any way beyond 'with' expressions
        // For immutable records, a direct copy is fine.
        _undoStack.Push(module); 
        
        // Limit stack size
        if (_undoStack.Count > MaxStackSize)
        {
            // Remove oldest item if stack exceeds max size
            // This is a simplified approach, in a real scenario, you might want a custom stack implementation
            // that automatically drops the oldest item without copying to an array.
            var items = _undoStack.ToList();
            items.RemoveAt(0); // Remove the oldest entry
            _undoStack.Clear();
            foreach (var item in items)
            {
                _undoStack.Push(item);
            }
        }
    }
    
    public FormModuleSchema Undo(FormModuleSchema current)
    {
        if (!CanUndo) throw new InvalidOperationException("Nothing to undo");
        
        _redoStack.Push(current);
        return _undoStack.Pop();
    }
    
    public FormModuleSchema Redo(FormModuleSchema current)
    {
        if (!CanRedo) throw new InvalidOperationException("Nothing to redo");
        
        _undoStack.Push(current);
        return _redoStack.Pop();
    }
    
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
