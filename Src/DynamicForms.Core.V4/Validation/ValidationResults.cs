namespace DynamicForms.Core.V4.Validation;

/// <summary>
/// Represents a validation error for a specific field
/// </summary>
/// <param name="FieldId">The ID of the field that failed validation</param>
/// <param name="ErrorCode">Machine-readable error code (e.g., "REQUIRED", "INVALID_EMAIL")</param>
/// <param name="Message">Human-readable English error message</param>
/// <param name="MessageFr">Human-readable French error message (optional)</param>
public record ValidationError(
    string FieldId,
    string ErrorCode,
    string Message,
    string? MessageFr = null
);

/// <summary>
/// Result of a validation operation
/// </summary>
/// <param name="IsValid">Whether the validation passed</param>
/// <param name="Errors">List of validation errors (empty if valid)</param>
public record ValidationResult(
    bool IsValid,
    List<ValidationError> Errors
)
{
    /// <summary>
    /// Creates a successful validation result with no errors
    /// </summary>
    public ValidationResult() : this(true, new()) { }

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    /// <returns>ValidationResult with IsValid = true</returns>
    public static ValidationResult Success() => new();

    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    /// <param name="errors">One or more validation errors</param>
    /// <returns>ValidationResult with IsValid = false and errors</returns>
    public static ValidationResult Failure(params ValidationError[] errors)
        => new(false, new List<ValidationError>(errors));
}
