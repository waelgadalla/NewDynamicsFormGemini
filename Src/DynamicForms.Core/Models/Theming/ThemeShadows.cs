using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Shadow settings for creating depth and elevation in form theming.
/// Shadow values should be valid CSS box-shadow values.
/// </summary>
public class ThemeShadows
{
    #region Shadow Scale

    /// <summary>
    /// No shadow (used to explicitly remove shadows).
    /// </summary>
    [JsonPropertyName("shadowNone")]
    public string ShadowNone { get; set; } = "none";

    /// <summary>
    /// Extra small/subtle shadow for minimal elevation.
    /// </summary>
    [JsonPropertyName("shadowXSmall")]
    public string ShadowXSmall { get; set; } = "0 1px 2px rgba(0, 0, 0, 0.05)";

    /// <summary>
    /// Small shadow for slight elevation.
    /// </summary>
    [JsonPropertyName("shadowSmall")]
    public string ShadowSmall { get; set; } = "0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)";

    /// <summary>
    /// Medium shadow for moderate elevation.
    /// </summary>
    [JsonPropertyName("shadowMedium")]
    public string ShadowMedium { get; set; } = "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)";

    /// <summary>
    /// Large shadow for prominent elevation.
    /// </summary>
    [JsonPropertyName("shadowLarge")]
    public string ShadowLarge { get; set; } = "0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)";

    /// <summary>
    /// Extra large shadow for maximum elevation.
    /// </summary>
    [JsonPropertyName("shadowXLarge")]
    public string ShadowXLarge { get; set; } = "0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)";

    /// <summary>
    /// 2XL shadow for floating elements like modals.
    /// </summary>
    [JsonPropertyName("shadow2XLarge")]
    public string Shadow2XLarge { get; set; } = "0 25px 50px -12px rgba(0, 0, 0, 0.25)";

    #endregion

    #region Component-Specific Shadows

    /// <summary>
    /// Shadow for form cards and panels.
    /// </summary>
    [JsonPropertyName("cardShadow")]
    public string CardShadow { get; set; } = "0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)";

    /// <summary>
    /// Shadow for dropdown menus and popovers.
    /// </summary>
    [JsonPropertyName("dropdownShadow")]
    public string DropdownShadow { get; set; } = "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)";

    /// <summary>
    /// Shadow for modal dialogs and overlays.
    /// </summary>
    [JsonPropertyName("modalShadow")]
    public string ModalShadow { get; set; } = "0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)";

    /// <summary>
    /// Shadow for focused input fields (focus glow effect).
    /// </summary>
    [JsonPropertyName("inputFocusShadow")]
    public string InputFocusShadow { get; set; } = "0 0 0 3px rgba(99, 102, 241, 0.15)";

    /// <summary>
    /// Shadow for buttons on hover.
    /// </summary>
    [JsonPropertyName("buttonHoverShadow")]
    public string ButtonHoverShadow { get; set; } = "0 4px 6px -1px rgba(0, 0, 0, 0.1)";

    /// <summary>
    /// Inner shadow for inset effects (e.g., pressed buttons).
    /// </summary>
    [JsonPropertyName("innerShadow")]
    public string InnerShadow { get; set; } = "inset 0 2px 4px rgba(0, 0, 0, 0.06)";

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep clone of this shadow configuration.
    /// </summary>
    public ThemeShadows Clone()
    {
        return new ThemeShadows
        {
            ShadowNone = ShadowNone,
            ShadowXSmall = ShadowXSmall,
            ShadowSmall = ShadowSmall,
            ShadowMedium = ShadowMedium,
            ShadowLarge = ShadowLarge,
            ShadowXLarge = ShadowXLarge,
            Shadow2XLarge = Shadow2XLarge,
            CardShadow = CardShadow,
            DropdownShadow = DropdownShadow,
            ModalShadow = ModalShadow,
            InputFocusShadow = InputFocusShadow,
            ButtonHoverShadow = ButtonHoverShadow,
            InnerShadow = InnerShadow
        };
    }

    #endregion
}
