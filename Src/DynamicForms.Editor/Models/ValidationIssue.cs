namespace DynamicForms.Editor.Models;

public record ValidationIssue(
    string FieldId,
    ValidationSeverity Severity,
    string Title,
    string Description
);

public enum ValidationSeverity { Error, Warning, Info }
