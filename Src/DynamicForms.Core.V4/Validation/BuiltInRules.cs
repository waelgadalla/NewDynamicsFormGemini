using System.Text.RegularExpressions;
using DynamicForms.Core.V4.Runtime;

namespace DynamicForms.Core.V4.Validation;

/// <summary>
/// Validates that a required field has a value
/// </summary>
public class RequiredFieldRule : IValidationRule
{
    public string RuleId => "required";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        if (field.Schema.Validation?.IsRequired != true)
            return Task.FromResult(ValidationResult.Success());

        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(
                    field.Schema.Id,
                    "REQUIRED",
                    field.Schema.Validation?.RequiredMessageEn ?? $"{field.Schema.LabelEn ?? field.Schema.Id} is required",
                    field.Schema.Validation?.RequiredMessageFr ?? $"{field.Schema.LabelFr ?? field.Schema.LabelEn} est requis"
                )
            ));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}

/// <summary>
/// Validates field value length against min/max constraints
/// </summary>
public class LengthValidationRule : IValidationRule
{
    public string RuleId => "length";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        if (value == null || field.Schema.Validation == null)
            return Task.FromResult(ValidationResult.Success());

        var stringValue = value.ToString() ?? string.Empty;
        var errors = new List<ValidationError>();
        var validation = field.Schema.Validation;

        if (validation.MinLength.HasValue && stringValue.Length < validation.MinLength.Value)
        {
            errors.Add(new ValidationError(
                field.Schema.Id,
                "MIN_LENGTH",
                $"{field.Schema.LabelEn ?? field.Schema.Id} must be at least {validation.MinLength} characters",
                $"{field.Schema.LabelFr ?? field.Schema.LabelEn} doit contenir au moins {validation.MinLength} caractères"
            ));
        }

        if (validation.MaxLength.HasValue && stringValue.Length > validation.MaxLength.Value)
        {
            errors.Add(new ValidationError(
                field.Schema.Id,
                "MAX_LENGTH",
                $"{field.Schema.LabelEn ?? field.Schema.Id} must not exceed {validation.MaxLength} characters",
                $"{field.Schema.LabelFr ?? field.Schema.LabelEn} ne doit pas dépasser {validation.MaxLength} caractères"
            ));
        }

        return errors.Any()
            ? Task.FromResult(ValidationResult.Failure(errors.ToArray()))
            : Task.FromResult(ValidationResult.Success());
    }
}

/// <summary>
/// Validates field value against a regular expression pattern
/// </summary>
public class PatternValidationRule : IValidationRule
{
    public string RuleId => "pattern";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        if (value == null || field.Schema.Validation == null || string.IsNullOrWhiteSpace(field.Schema.Validation.Pattern))
            return Task.FromResult(ValidationResult.Success());

        var stringValue = value.ToString() ?? string.Empty;
        var validation = field.Schema.Validation;

        try
        {
            var regex = new Regex(validation.Pattern);
            if (!regex.IsMatch(stringValue))
            {
                return Task.FromResult(ValidationResult.Failure(
                    new ValidationError(
                        field.Schema.Id,
                        "PATTERN_MISMATCH",
                        validation.PatternMessageEn ?? $"{field.Schema.LabelEn ?? field.Schema.Id} format is invalid",
                        validation.PatternMessageFr ?? $"Le format de {field.Schema.LabelFr ?? field.Schema.LabelEn} est invalide"
                    )
                ));
            }
        }
        catch (Exception)
        {
            // Invalid regex pattern - log but don't fail validation
            return Task.FromResult(ValidationResult.Success());
        }

        return Task.FromResult(ValidationResult.Success());
    }
}

/// <summary>
/// Validates that a field contains a valid email address
/// </summary>
public class EmailValidationRule : IValidationRule
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string RuleId => "email";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        if (value == null)
            return Task.FromResult(ValidationResult.Success());

        var stringValue = value.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(stringValue))
            return Task.FromResult(ValidationResult.Success());

        if (!EmailRegex.IsMatch(stringValue))
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(
                    field.Schema.Id,
                    "INVALID_EMAIL",
                    $"{field.Schema.LabelEn ?? field.Schema.Id} must be a valid email address",
                    $"{field.Schema.LabelFr ?? field.Schema.LabelEn} doit être une adresse courriel valide"
                )
            ));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}
