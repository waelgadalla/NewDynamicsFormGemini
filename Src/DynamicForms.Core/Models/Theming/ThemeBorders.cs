using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Border and corner radius settings for form theming.
/// </summary>
public class ThemeBorders
{
    #region Border Properties

    /// <summary>
    /// Default border width for inputs and containers (e.g., "1px", "2px").
    /// </summary>
    [JsonPropertyName("borderWidth")]
    public string BorderWidth { get; set; } = "1px";

    /// <summary>
    /// Border style for inputs and containers (solid, dashed, dotted, none).
    /// </summary>
    [JsonPropertyName("borderStyle")]
    public string BorderStyle { get; set; } = "solid";

    #endregion

    #region Corner Radius Scale

    /// <summary>
    /// Small border radius for subtle rounding (e.g., checkboxes, tags).
    /// </summary>
    [JsonPropertyName("radiusSmall")]
    public string RadiusSmall { get; set; } = "4px";

    /// <summary>
    /// Medium border radius for inputs and buttons.
    /// </summary>
    [JsonPropertyName("radiusMedium")]
    public string RadiusMedium { get; set; } = "6px";

    /// <summary>
    /// Large border radius for cards and panels.
    /// </summary>
    [JsonPropertyName("radiusLarge")]
    public string RadiusLarge { get; set; } = "8px";

    /// <summary>
    /// Extra large border radius for modals and prominent containers.
    /// </summary>
    [JsonPropertyName("radiusXLarge")]
    public string RadiusXLarge { get; set; } = "12px";

    /// <summary>
    /// Full/pill border radius for rounded buttons and badges.
    /// </summary>
    [JsonPropertyName("radiusFull")]
    public string RadiusFull { get; set; } = "9999px";

    #endregion

    #region Focus Ring

    /// <summary>
    /// Width of the focus ring around focused elements (e.g., "2px", "3px").
    /// </summary>
    [JsonPropertyName("focusRingWidth")]
    public string FocusRingWidth { get; set; } = "2px";

    /// <summary>
    /// Offset/gap between the element and focus ring (e.g., "2px", "1px").
    /// </summary>
    [JsonPropertyName("focusRingOffset")]
    public string FocusRingOffset { get; set; } = "2px";

    /// <summary>
    /// Style of the focus ring (solid, dashed, dotted).
    /// </summary>
    [JsonPropertyName("focusRingStyle")]
    public string FocusRingStyle { get; set; } = "solid";

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep clone of this border configuration.
    /// </summary>
    public ThemeBorders Clone()
    {
        return new ThemeBorders
        {
            BorderWidth = BorderWidth,
            BorderStyle = BorderStyle,
            RadiusSmall = RadiusSmall,
            RadiusMedium = RadiusMedium,
            RadiusLarge = RadiusLarge,
            RadiusXLarge = RadiusXLarge,
            RadiusFull = RadiusFull,
            FocusRingWidth = FocusRingWidth,
            FocusRingOffset = FocusRingOffset,
            FocusRingStyle = FocusRingStyle
        };
    }

    #endregion
}
