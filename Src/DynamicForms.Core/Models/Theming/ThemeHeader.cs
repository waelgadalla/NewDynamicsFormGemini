using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Header customization settings for form theming.
/// Includes logo, background, and layout configuration.
/// </summary>
public class ThemeHeader
{
    #region Enable/Disable

    /// <summary>
    /// Whether the header section is enabled and visible.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    #endregion

    #region Logo Settings

    /// <summary>
    /// URL or data URI of the logo image.
    /// </summary>
    [JsonPropertyName("logoUrl")]
    public string LogoUrl { get; set; } = "";

    /// <summary>
    /// Horizontal position of the logo (left, center, right).
    /// </summary>
    [JsonPropertyName("logoPosition")]
    public string LogoPosition { get; set; } = "left";

    /// <summary>
    /// Maximum height for the logo image (e.g., "48px", "60px").
    /// </summary>
    [JsonPropertyName("logoMaxHeight")]
    public string LogoMaxHeight { get; set; } = "48px";

    /// <summary>
    /// Alt text for the logo image for accessibility.
    /// </summary>
    [JsonPropertyName("logoAltText")]
    public string LogoAltText { get; set; } = "Logo";

    #endregion

    #region Background Settings

    /// <summary>
    /// Type of background: "color", "image", or "gradient".
    /// </summary>
    [JsonPropertyName("backgroundType")]
    public string BackgroundType { get; set; } = "color";

    /// <summary>
    /// Background color when backgroundType is "color".
    /// </summary>
    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// URL or data URI of the background image.
    /// </summary>
    [JsonPropertyName("backgroundImage")]
    public string BackgroundImage { get; set; } = "";

    /// <summary>
    /// CSS gradient value when backgroundType is "gradient".
    /// </summary>
    [JsonPropertyName("backgroundGradient")]
    public string BackgroundGradient { get; set; } = "";

    /// <summary>
    /// Background size: "cover", "contain", or "auto".
    /// </summary>
    [JsonPropertyName("backgroundSize")]
    public string BackgroundSize { get; set; } = "cover";

    /// <summary>
    /// Background position: "center", "top", "bottom", etc.
    /// </summary>
    [JsonPropertyName("backgroundPosition")]
    public string BackgroundPosition { get; set; } = "center";

    #endregion

    #region Overlay Settings

    /// <summary>
    /// Whether to show a color overlay on top of the background.
    /// </summary>
    [JsonPropertyName("overlayEnabled")]
    public bool OverlayEnabled { get; set; } = false;

    /// <summary>
    /// Color and opacity of the overlay (e.g., "rgba(0, 0, 0, 0.3)").
    /// </summary>
    [JsonPropertyName("overlayColor")]
    public string OverlayColor { get; set; } = "rgba(0, 0, 0, 0.3)";

    #endregion

    #region Layout Settings

    /// <summary>
    /// Height of the header: "auto" or specific value (e.g., "200px").
    /// </summary>
    [JsonPropertyName("height")]
    public string Height { get; set; } = "auto";

    /// <summary>
    /// Internal padding of the header (e.g., "24px", "32px 48px").
    /// </summary>
    [JsonPropertyName("padding")]
    public string Padding { get; set; } = "24px";

    /// <summary>
    /// Horizontal content alignment: "left", "center", or "right".
    /// </summary>
    [JsonPropertyName("contentAlignment")]
    public string ContentAlignment { get; set; } = "left";

    /// <summary>
    /// Vertical content alignment: "top", "center", or "bottom".
    /// </summary>
    [JsonPropertyName("verticalAlignment")]
    public string VerticalAlignment { get; set; } = "center";

    /// <summary>
    /// Whether the header extends beyond the form container.
    /// </summary>
    [JsonPropertyName("fullWidth")]
    public bool FullWidth { get; set; } = false;

    /// <summary>
    /// Whether the header overlaps the form content below.
    /// </summary>
    [JsonPropertyName("overlapContent")]
    public bool OverlapContent { get; set; } = false;

    /// <summary>
    /// Amount of overlap when overlapContent is true (e.g., "40px").
    /// </summary>
    [JsonPropertyName("overlapAmount")]
    public string OverlapAmount { get; set; } = "40px";

    #endregion

    #region Title Styling

    /// <summary>
    /// Color override for the form title in the header.
    /// If empty, inherits from theme colors.
    /// </summary>
    [JsonPropertyName("titleColor")]
    public string TitleColor { get; set; } = "";

    /// <summary>
    /// Font size override for the title in the header.
    /// If empty, uses default form title size.
    /// </summary>
    [JsonPropertyName("titleSize")]
    public string TitleSize { get; set; } = "";

    /// <summary>
    /// Color override for the description text in the header.
    /// </summary>
    [JsonPropertyName("descriptionColor")]
    public string DescriptionColor { get; set; } = "";

    /// <summary>
    /// Font size override for the description in the header.
    /// </summary>
    [JsonPropertyName("descriptionSize")]
    public string DescriptionSize { get; set; } = "";

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep clone of this header configuration.
    /// </summary>
    public ThemeHeader Clone()
    {
        return new ThemeHeader
        {
            Enabled = Enabled,
            LogoUrl = LogoUrl,
            LogoPosition = LogoPosition,
            LogoMaxHeight = LogoMaxHeight,
            LogoAltText = LogoAltText,
            BackgroundType = BackgroundType,
            BackgroundColor = BackgroundColor,
            BackgroundImage = BackgroundImage,
            BackgroundGradient = BackgroundGradient,
            BackgroundSize = BackgroundSize,
            BackgroundPosition = BackgroundPosition,
            OverlayEnabled = OverlayEnabled,
            OverlayColor = OverlayColor,
            Height = Height,
            Padding = Padding,
            ContentAlignment = ContentAlignment,
            VerticalAlignment = VerticalAlignment,
            FullWidth = FullWidth,
            OverlapContent = OverlapContent,
            OverlapAmount = OverlapAmount,
            TitleColor = TitleColor,
            TitleSize = TitleSize,
            DescriptionColor = DescriptionColor,
            DescriptionSize = DescriptionSize
        };
    }

    #endregion
}
