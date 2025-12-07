namespace VisualEditorOpus.Models;

/// <summary>
/// Represents an option in a CodeSet for editing
/// </summary>
public record CodeSetOption(
    string Id,
    string Value,
    string LabelEn,
    string? LabelFr,
    int Order
);
