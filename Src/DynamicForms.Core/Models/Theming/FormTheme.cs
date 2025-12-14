using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Defines the available theme modes for form display.
/// </summary>
public enum ThemeMode
{
    /// <summary>Light theme with light backgrounds and dark text.</summary>
    Light,

    /// <summary>Dark theme with dark backgrounds and light text.</summary>
    Dark,

    /// <summary>Automatically follow the user's system preference.</summary>
    Auto
}

/// <summary>
/// Complete theme configuration for form styling.
/// This is the root model that contains all theme settings and is serializable to JSON for storage.
/// </summary>
public class FormTheme
{
    #region Metadata

    /// <summary>
    /// Unique identifier for this theme.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name of the theme.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Custom Theme";

    /// <summary>
    /// Optional description of the theme.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// The preset this theme was derived from (e.g., "default", "corporate").
    /// Empty if created from scratch.
    /// </summary>
    [JsonPropertyName("basePreset")]
    public string BasePreset { get; set; } = "default";

    /// <summary>
    /// When the theme was first created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the theme was last modified.
    /// </summary>
    [JsonPropertyName("modifiedAt")]
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User or system that created this theme.
    /// </summary>
    [JsonPropertyName("createdBy")]
    public string CreatedBy { get; set; } = "";

    /// <summary>
    /// Version number for optimistic concurrency control.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    #endregion

    #region Mode Settings

    /// <summary>
    /// The color mode for this theme (Light, Dark, or Auto).
    /// </summary>
    [JsonPropertyName("mode")]
    public ThemeMode Mode { get; set; } = ThemeMode.Light;

    /// <summary>
    /// When true, displays a compact/panelless view without card backgrounds.
    /// </summary>
    [JsonPropertyName("isPanelless")]
    public bool IsPanelless { get; set; } = false;

    #endregion

    #region Theme Components

    /// <summary>
    /// Color palette settings for the theme.
    /// </summary>
    [JsonPropertyName("colors")]
    public ThemeColors Colors { get; set; } = new();

    /// <summary>
    /// Typography settings including fonts and sizes.
    /// </summary>
    [JsonPropertyName("typography")]
    public ThemeTypography Typography { get; set; } = new();

    /// <summary>
    /// Spacing and layout settings.
    /// </summary>
    [JsonPropertyName("spacing")]
    public ThemeSpacing Spacing { get; set; } = new();

    /// <summary>
    /// Border and corner radius settings.
    /// </summary>
    [JsonPropertyName("borders")]
    public ThemeBorders Borders { get; set; } = new();

    /// <summary>
    /// Shadow settings for depth and elevation.
    /// </summary>
    [JsonPropertyName("shadows")]
    public ThemeShadows Shadows { get; set; } = new();

    /// <summary>
    /// Form header customization including logo and background.
    /// </summary>
    [JsonPropertyName("header")]
    public ThemeHeader Header { get; set; } = new();

    /// <summary>
    /// Page/form background settings.
    /// </summary>
    [JsonPropertyName("background")]
    public ThemeBackground Background { get; set; } = new();

    /// <summary>
    /// Component-specific style overrides (for advanced mode).
    /// </summary>
    [JsonPropertyName("components")]
    public ThemeComponentStyles Components { get; set; } = new();

    /// <summary>
    /// Accessibility settings including scale factor and contrast.
    /// </summary>
    [JsonPropertyName("accessibility")]
    public ThemeAccessibility Accessibility { get; set; } = new();

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep clone of this theme.
    /// </summary>
    public FormTheme Clone()
    {
        return new FormTheme
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{Name} (Copy)",
            Description = Description,
            BasePreset = BasePreset,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            CreatedBy = CreatedBy,
            Version = 1,
            Mode = Mode,
            IsPanelless = IsPanelless,
            Colors = Colors.Clone(),
            Typography = Typography.Clone(),
            Spacing = Spacing.Clone(),
            Borders = Borders.Clone(),
            Shadows = Shadows.Clone(),
            Header = Header.Clone(),
            Background = Background.Clone(),
            Components = Components.Clone(),
            Accessibility = Accessibility.Clone()
        };
    }

    /// <summary>
    /// Updates the ModifiedAt timestamp to now.
    /// </summary>
    public void Touch()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    #endregion
}
