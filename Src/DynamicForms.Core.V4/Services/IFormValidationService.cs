using DynamicForms.Core.V4.Runtime;
using DynamicForms.Core.V4.Validation;

namespace DynamicForms.Core.V4.Services;

/// <summary>
/// Service for validating form modules and fields.
/// Supports composable validation rules and cross-field validation.
/// </summary>
public interface IFormValidationService
{
    /// <summary>
    /// Validates all fields in a module against form data
    /// </summary>
    /// <param name="module">The runtime module to validate</param>
    /// <param name="formData">Form data to validate (key = field ID, value = field value)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated validation result for all fields</returns>
    Task<ValidationResult> ValidateModuleAsync(
        FormModuleRuntime module,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a single field value
    /// </summary>
    /// <param name="field">The field node to validate</param>
    /// <param name="value">The value to validate</param>
    /// <param name="formData">All form data for cross-field validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result for this field</returns>
    Task<ValidationResult> ValidateFieldAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a validation rule with the service.
    /// Rules can be built-in or custom.
    /// </summary>
    /// <param name="ruleId">Unique identifier for the rule</param>
    /// <param name="rule">The validation rule implementation</param>
    void RegisterRule(string ruleId, IValidationRule rule);
}
