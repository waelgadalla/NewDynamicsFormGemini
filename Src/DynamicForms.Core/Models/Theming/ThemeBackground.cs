using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Page/form background settings for theming.
/// Controls the overall background of the form page.
/// </summary>
public class ThemeBackground
{
    #region Background Type

    /// <summary>
    /// Type of background: "color", "image", or "gradient".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "color";

    #endregion

    #region Color Settings

    /// <summary>
    /// Background color when type is "color".
    /// </summary>
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#F8FAFC";

    #endregion

    #region Image Settings

    /// <summary>
    /// URL or data URI of the background image.
    /// </summary>
    [JsonPropertyName("image")]
    public string Image { get; set; } = "";

    /// <summary>
    /// Background image size: "cover", "contain", "auto", or specific value.
    /// </summary>
    [JsonPropertyName("imageSize")]
    public string ImageSize { get; set; } = "cover";

    /// <summary>
    /// Background image position: "center", "top", "bottom", "left", "right",
    /// or compound values like "center top".
    /// </summary>
    [JsonPropertyName("imagePosition")]
    public string ImagePosition { get; set; } = "center";

    /// <summary>
    /// Background image repeat: "no-repeat", "repeat", "repeat-x", "repeat-y".
    /// </summary>
    [JsonPropertyName("imageRepeat")]
    public string ImageRepeat { get; set; } = "no-repeat";

    /// <summary>
    /// Background attachment: "scroll" or "fixed".
    /// Fixed keeps the background in place while scrolling.
    /// </summary>
    [JsonPropertyName("imageAttachment")]
    public string ImageAttachment { get; set; } = "fixed";

    /// <summary>
    /// Opacity of the background image (0-1).
    /// </summary>
    [JsonPropertyName("imageOpacity")]
    public string ImageOpacity { get; set; } = "1";

    #endregion

    #region Gradient Settings

    /// <summary>
    /// CSS gradient value when type is "gradient".
    /// Example: "linear-gradient(180deg, #667eea 0%, #764ba2 100%)"
    /// </summary>
    [JsonPropertyName("gradient")]
    public string Gradient { get; set; } = "";

    /// <summary>
    /// Gradient type for the editor: "linear" or "radial".
    /// </summary>
    [JsonPropertyName("gradientType")]
    public string GradientType { get; set; } = "linear";

    /// <summary>
    /// Angle for linear gradient in degrees.
    /// </summary>
    [JsonPropertyName("gradientAngle")]
    public int GradientAngle { get; set; } = 180;

    /// <summary>
    /// Color stops for the gradient.
    /// Each stop contains a color and position percentage.
    /// </summary>
    [JsonPropertyName("gradientStops")]
    public List<GradientStop> GradientStops { get; set; } = new()
    {
        new GradientStop { Color = "#667eea", Position = 0 },
        new GradientStop { Color = "#764ba2", Position = 100 }
    };

    #endregion

    #region Overlay

    /// <summary>
    /// Whether to show an overlay on the background.
    /// </summary>
    [JsonPropertyName("overlayEnabled")]
    public bool OverlayEnabled { get; set; } = false;

    /// <summary>
    /// Color and opacity of the overlay.
    /// </summary>
    [JsonPropertyName("overlayColor")]
    public string OverlayColor { get; set; } = "rgba(0, 0, 0, 0.1)";

    #endregion

    #region Methods

    /// <summary>
    /// Generates the computed CSS gradient string from gradient settings.
    /// </summary>
    [JsonIgnore]
    public string ComputedGradient
    {
        get
        {
            if (GradientStops.Count < 2)
                return Gradient;

            var stops = string.Join(", ", GradientStops
                .OrderBy(s => s.Position)
                .Select(s => $"{s.Color} {s.Position}%"));

            return GradientType == "radial"
                ? $"radial-gradient(circle, {stops})"
                : $"linear-gradient({GradientAngle}deg, {stops})";
        }
    }

    /// <summary>
    /// Creates a deep clone of this background configuration.
    /// </summary>
    public ThemeBackground Clone()
    {
        return new ThemeBackground
        {
            Type = Type,
            Color = Color,
            Image = Image,
            ImageSize = ImageSize,
            ImagePosition = ImagePosition,
            ImageRepeat = ImageRepeat,
            ImageAttachment = ImageAttachment,
            ImageOpacity = ImageOpacity,
            Gradient = Gradient,
            GradientType = GradientType,
            GradientAngle = GradientAngle,
            GradientStops = GradientStops.Select(s => new GradientStop
            {
                Color = s.Color,
                Position = s.Position
            }).ToList(),
            OverlayEnabled = OverlayEnabled,
            OverlayColor = OverlayColor
        };
    }

    #endregion
}

/// <summary>
/// Represents a color stop in a gradient.
/// </summary>
public class GradientStop
{
    /// <summary>
    /// Color value at this stop (hex or rgba).
    /// </summary>
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#000000";

    /// <summary>
    /// Position of this stop as a percentage (0-100).
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; set; } = 0;
}
