namespace VisualEditorOpus.Models;

/// <summary>
/// Represents a validation issue found in the schema
/// </summary>
public record ValidationIssue(
    string FieldId,
    ValidationSeverity Severity,
    string Title,
    string Description
);

public enum ValidationSeverity
{
    Error,
    Warning,
    Info
}
