namespace DynamicForms.Editor.Models;

public class WorkflowVisualConnection
{
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;

    public WorkflowVisualConnection() { }

    public WorkflowVisualConnection(string sourceNodeId, string targetNodeId)
    {
        SourceNodeId = sourceNodeId;
        TargetNodeId = targetNodeId;
    }
}
