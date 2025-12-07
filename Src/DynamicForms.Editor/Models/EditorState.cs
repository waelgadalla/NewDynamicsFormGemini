using System.Collections.Immutable;
using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Runtime;
using DynamicForms.Editor.Models; // For ValidationIssue

namespace DynamicForms.Editor.Models;

public record EditorState
{
    // Current data
    public FormWorkflowSchema? Workflow { get; init; }
    public FormModuleSchema? Module { get; init; }
    public FormModuleRuntime? ModuleRuntime { get; init; }
    
    // Selection
    public string? SelectedFieldId { get; init; }
    public string? SelectedNodeId { get; init; }  // For workflow designer
    
    // Clipboard
    public FormFieldSchema? ClipboardField { get; init; }
    
    // Validation
    public ImmutableList<ValidationIssue> Issues { get; init; } 
        = ImmutableList<ValidationIssue>.Empty;
    
    // UI State
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    // Workflow State
    public List<WorkflowVisualNode> WorkflowNodes { get; init; } = new();
    public List<WorkflowVisualConnection> WorkflowConnections { get; init; } = new();
    
    // Computed properties
    public FormFieldSchema? SelectedField => SelectedFieldId is not null && Module is not null
        ? Module.Fields.FirstOrDefault(f => f.Id == SelectedFieldId)
        : null;
    
    public FormFieldNode? SelectedFieldNode => SelectedFieldId is not null && ModuleRuntime is not null
        ? ModuleRuntime.GetField(SelectedFieldId)
        : null;
}
