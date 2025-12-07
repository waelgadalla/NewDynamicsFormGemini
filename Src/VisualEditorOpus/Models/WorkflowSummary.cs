namespace VisualEditorOpus.Models;

/// <summary>
/// Summary information for a workflow displayed on the dashboard
/// </summary>
public record WorkflowSummary(
    int Id,
    string Title,
    string Description,
    int ModuleCount,
    string Status,
    DateTime Modified
);
