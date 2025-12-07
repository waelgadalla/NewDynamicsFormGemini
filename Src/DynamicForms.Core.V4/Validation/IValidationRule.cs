using DynamicForms.Core.V4.Runtime;

namespace DynamicForms.Core.V4.Validation;

/// <summary>
/// Interface for composable validation rules.
/// Validation rules can be registered with the validation service and applied to fields.
/// </summary>
public interface IValidationRule
{
    /// <summary>
    /// Unique identifier for this validation rule (e.g., "required", "email", "pattern")
    /// </summary>
    string RuleId { get; }

    /// <summary>
    /// Performs validation on a field value
    /// </summary>
    /// <param name="field">The field node being validated (contains schema and hierarchy context)</param>
    /// <param name="value">The value to validate (may be null)</param>
    /// <param name="formData">All form data for cross-field validation (key = field ID, value = field value)</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Validation result indicating success or failure with errors</returns>
    Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default);
}
