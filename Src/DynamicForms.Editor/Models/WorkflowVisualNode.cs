using System.Text.Json.Serialization;

namespace DynamicForms.Editor.Models;

public class WorkflowVisualNode
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "module"; // start, module, decision, end
    public double X { get; set; }
    public double Y { get; set; }
    
    // Store additional data like ConditionModel for decisions, or specialized settings
    public Dictionary<string, object> Data { get; set; } = new();

    public WorkflowVisualNode() { }

    public WorkflowVisualNode(string id, string title, string type, double x, double y)
    {
        Id = id;
        Title = title;
        Type = type;
        X = x;
        Y = y;
    }
}
