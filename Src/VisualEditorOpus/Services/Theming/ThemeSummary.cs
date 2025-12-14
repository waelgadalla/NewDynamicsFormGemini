using DynamicForms.Models.Theming;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Summary information about a theme for list displays.
/// Contains only essential metadata without the full theme configuration.
/// </summary>
public sealed record ThemeSummary
{
    /// <summary>
    /// Unique identifier for the theme.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name of the theme.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional description of the theme.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The preset this theme was based on.
    /// </summary>
    public string? BasePreset { get; init; }

    /// <summary>
    /// Primary color for quick preview (hex value).
    /// </summary>
    public string? PreviewColor { get; init; }

    /// <summary>
    /// The theme mode (Light, Dark, Auto).
    /// </summary>
    public ThemeMode Mode { get; init; }

    /// <summary>
    /// Whether this is the default theme.
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Whether this theme is locked from editing.
    /// </summary>
    public bool IsLocked { get; init; }

    /// <summary>
    /// Organization this theme belongs to (for multi-tenant).
    /// </summary>
    public string? OrganizationId { get; init; }

    /// <summary>
    /// User who created this theme.
    /// </summary>
    public string? CreatedBy { get; init; }

    /// <summary>
    /// When the theme was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the theme was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// Version number for optimistic concurrency.
    /// </summary>
    public int Version { get; init; }
}
