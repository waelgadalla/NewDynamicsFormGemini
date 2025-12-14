using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Typography settings for form theming including fonts, sizes, and weights.
/// </summary>
public class ThemeTypography
{
    #region Font Families

    /// <summary>
    /// Primary font family for body text and inputs.
    /// Uses a font stack with fallbacks for cross-platform compatibility.
    /// </summary>
    [JsonPropertyName("fontFamily")]
    public string FontFamily { get; set; } = "'DM Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif";

    /// <summary>
    /// Font family for headings and titles.
    /// If empty, inherits from FontFamily.
    /// </summary>
    [JsonPropertyName("headingFontFamily")]
    public string HeadingFontFamily { get; set; } = "";

    /// <summary>
    /// Monospace font family for code and technical content.
    /// </summary>
    [JsonPropertyName("monoFontFamily")]
    public string MonoFontFamily { get; set; } = "'JetBrains Mono', 'Fira Code', 'Consolas', monospace";

    #endregion

    #region Base Sizes

    /// <summary>
    /// Base font size for body text (e.g., "14px", "16px").
    /// </summary>
    [JsonPropertyName("baseFontSize")]
    public string BaseFontSize { get; set; } = "14px";

    /// <summary>
    /// Line height multiplier for text readability (e.g., "1.5", "1.6").
    /// </summary>
    [JsonPropertyName("lineHeight")]
    public string LineHeight { get; set; } = "1.5";

    #endregion

    #region Heading Sizes

    /// <summary>
    /// Font size for the main form title (e.g., "24px", "28px").
    /// </summary>
    [JsonPropertyName("formTitleSize")]
    public string FormTitleSize { get; set; } = "24px";

    /// <summary>
    /// Font size for section/panel titles (e.g., "18px", "20px").
    /// </summary>
    [JsonPropertyName("sectionTitleSize")]
    public string SectionTitleSize { get; set; } = "18px";

    /// <summary>
    /// Font size for question/field labels (e.g., "14px", "15px").
    /// </summary>
    [JsonPropertyName("questionTitleSize")]
    public string QuestionTitleSize { get; set; } = "14px";

    /// <summary>
    /// Font size for descriptions and help text (e.g., "12px", "13px").
    /// </summary>
    [JsonPropertyName("descriptionSize")]
    public string DescriptionSize { get; set; } = "13px";

    #endregion

    #region Font Weights

    /// <summary>
    /// Normal/regular font weight (typically 400).
    /// </summary>
    [JsonPropertyName("fontWeightNormal")]
    public string FontWeightNormal { get; set; } = "400";

    /// <summary>
    /// Medium font weight for subtle emphasis (typically 500).
    /// </summary>
    [JsonPropertyName("fontWeightMedium")]
    public string FontWeightMedium { get; set; } = "500";

    /// <summary>
    /// Semibold font weight for headings (typically 600).
    /// </summary>
    [JsonPropertyName("fontWeightSemibold")]
    public string FontWeightSemibold { get; set; } = "600";

    /// <summary>
    /// Bold font weight for strong emphasis (typically 700).
    /// </summary>
    [JsonPropertyName("fontWeightBold")]
    public string FontWeightBold { get; set; } = "700";

    #endregion

    #region Methods

    /// <summary>
    /// Gets the effective heading font family, falling back to the main font if not specified.
    /// </summary>
    [JsonIgnore]
    public string EffectiveHeadingFontFamily =>
        string.IsNullOrWhiteSpace(HeadingFontFamily) ? FontFamily : HeadingFontFamily;

    /// <summary>
    /// Creates a deep clone of this typography configuration.
    /// </summary>
    public ThemeTypography Clone()
    {
        return new ThemeTypography
        {
            FontFamily = FontFamily,
            HeadingFontFamily = HeadingFontFamily,
            MonoFontFamily = MonoFontFamily,
            BaseFontSize = BaseFontSize,
            LineHeight = LineHeight,
            FormTitleSize = FormTitleSize,
            SectionTitleSize = SectionTitleSize,
            QuestionTitleSize = QuestionTitleSize,
            DescriptionSize = DescriptionSize,
            FontWeightNormal = FontWeightNormal,
            FontWeightMedium = FontWeightMedium,
            FontWeightSemibold = FontWeightSemibold,
            FontWeightBold = FontWeightBold
        };
    }

    #endregion
}
