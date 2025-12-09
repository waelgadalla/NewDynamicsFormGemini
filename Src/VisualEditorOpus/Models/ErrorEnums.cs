namespace VisualEditorOpus.Models;

/// <summary>
/// Alert style variants
/// </summary>
public enum AlertVariant
{
    /// <summary>
    /// Error alert (red)
    /// </summary>
    Error,

    /// <summary>
    /// Warning alert (yellow/orange)
    /// </summary>
    Warning,

    /// <summary>
    /// Info alert (blue/primary)
    /// </summary>
    Info,

    /// <summary>
    /// Success alert (green)
    /// </summary>
    Success
}

/// <summary>
/// Network connection status states
/// </summary>
public enum NetworkConnectionStatus
{
    /// <summary>
    /// Connected to the server
    /// </summary>
    Connected,

    /// <summary>
    /// Disconnected from the server
    /// </summary>
    Disconnected,

    /// <summary>
    /// Attempting to reconnect
    /// </summary>
    Reconnecting
}

/// <summary>
/// Represents a validation error with field context
/// </summary>
/// <param name="FieldId">The ID of the field with the error (can be null for form-level errors)</param>
/// <param name="FieldLabel">The display label of the field (can be null)</param>
/// <param name="Message">The error message to display</param>
public record ValidationError(string? FieldId, string? FieldLabel, string Message);
