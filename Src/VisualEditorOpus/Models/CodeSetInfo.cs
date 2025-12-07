namespace VisualEditorOpus.Models;

/// <summary>
/// Represents a CodeSet summary for display in the manager
/// </summary>
public record CodeSetInfo(
    string Id,
    string Name,
    string Description,
    int OptionCount,
    DateTime Modified
);
