namespace VisualEditorOpus.Models;

/// <summary>
/// Represents a field reference for use in the condition builder field picker.
/// </summary>
/// <param name="FieldId">Full field reference (e.g., "age" or "Step1.country")</param>
/// <param name="DisplayName">Display name shown in picker (e.g., "age (Number)")</param>
/// <param name="FieldType">Field type for operator filtering and value input</param>
/// <param name="ModuleKey">Module key prefix if cross-module (e.g., "Step1") or null for current module</param>
/// <param name="ModuleName">Human-readable module name for grouping</param>
/// <param name="Options">Dropdown options if field is a selection type</param>
public record FieldReference(
    string FieldId,
    string DisplayName,
    string FieldType,
    string? ModuleKey,
    string? ModuleName,
    List<FieldOption>? Options
);

/// <summary>
/// Represents an option for dropdown/selection fields.
/// </summary>
/// <param name="Value">The option value</param>
/// <param name="Label">The display label</param>
public record FieldOption(string Value, string Label);
