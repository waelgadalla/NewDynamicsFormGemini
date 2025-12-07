namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Accessibility (WCAG) configuration for a field.
/// </summary>
public record AccessibilityConfig
{
    /// <summary>
    /// Explicit ARIA label (overrides the visual label if set). (English)
    /// </summary>
    public string? AriaLabelEn { get; init; }

    /// <summary>
    /// Explicit ARIA label (overrides the visual label if set). (French)
    /// </summary>
    public string? AriaLabelFr { get; init; }

    /// <summary>
    /// ID of the element describing this field (aria-describedby).
    /// usually auto-generated, but can be manually overridden.
    /// </summary>
    public string? AriaDescribedBy { get; init; }

    /// <summary>
    /// The ARIA role of the element (e.g., "alert", "status", "searchbox").
    /// </summary>
    public string? AriaRole { get; init; }

    /// <summary>
    /// Whether updates to this field should be announced by screen readers.
    /// </summary>
    public bool AriaLive { get; init; }
}
