using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Spacing and layout settings for form theming.
/// All values should include units (e.g., "8px", "1rem").
/// </summary>
public class ThemeSpacing
{
    #region Base Unit

    /// <summary>
    /// Master spacing unit used as the foundation for all spacing calculations.
    /// Typically 8px following the 8-point grid system.
    /// </summary>
    [JsonPropertyName("baseUnit")]
    public string BaseUnit { get; set; } = "8px";

    #endregion

    #region Form Level Spacing

    /// <summary>
    /// Padding inside the form container (e.g., "32px", "24px").
    /// </summary>
    [JsonPropertyName("formPadding")]
    public string FormPadding { get; set; } = "32px";

    /// <summary>
    /// Vertical spacing between sections/panels (e.g., "24px", "32px").
    /// </summary>
    [JsonPropertyName("sectionSpacing")]
    public string SectionSpacing { get; set; } = "24px";

    #endregion

    #region Field Level Spacing

    /// <summary>
    /// Vertical spacing between questions/fields (e.g., "20px", "24px").
    /// </summary>
    [JsonPropertyName("questionSpacing")]
    public string QuestionSpacing { get; set; } = "20px";

    /// <summary>
    /// Spacing between a label and its input (e.g., "6px", "8px").
    /// </summary>
    [JsonPropertyName("labelSpacing")]
    public string LabelSpacing { get; set; } = "6px";

    /// <summary>
    /// Spacing between radio/checkbox options (e.g., "8px", "12px").
    /// </summary>
    [JsonPropertyName("optionSpacing")]
    public string OptionSpacing { get; set; } = "8px";

    #endregion

    #region Input Spacing

    /// <summary>
    /// Internal padding inside input fields (e.g., "12px", "10px 14px").
    /// </summary>
    [JsonPropertyName("inputPadding")]
    public string InputPadding { get; set; } = "12px";

    /// <summary>
    /// Horizontal padding for input fields when different from vertical.
    /// If empty, uses inputPadding value.
    /// </summary>
    [JsonPropertyName("inputPaddingHorizontal")]
    public string InputPaddingHorizontal { get; set; } = "14px";

    #endregion

    #region Button Spacing

    /// <summary>
    /// Vertical padding inside buttons (e.g., "10px", "12px").
    /// </summary>
    [JsonPropertyName("buttonPaddingVertical")]
    public string ButtonPaddingVertical { get; set; } = "10px";

    /// <summary>
    /// Horizontal padding inside buttons (e.g., "20px", "24px").
    /// </summary>
    [JsonPropertyName("buttonPaddingHorizontal")]
    public string ButtonPaddingHorizontal { get; set; } = "20px";

    /// <summary>
    /// Spacing between buttons in button groups (e.g., "12px", "16px").
    /// </summary>
    [JsonPropertyName("buttonGap")]
    public string ButtonGap { get; set; } = "12px";

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep clone of this spacing configuration.
    /// </summary>
    public ThemeSpacing Clone()
    {
        return new ThemeSpacing
        {
            BaseUnit = BaseUnit,
            FormPadding = FormPadding,
            SectionSpacing = SectionSpacing,
            QuestionSpacing = QuestionSpacing,
            LabelSpacing = LabelSpacing,
            OptionSpacing = OptionSpacing,
            InputPadding = InputPadding,
            InputPaddingHorizontal = InputPaddingHorizontal,
            ButtonPaddingVertical = ButtonPaddingVertical,
            ButtonPaddingHorizontal = ButtonPaddingHorizontal,
            ButtonGap = ButtonGap
        };
    }

    #endregion
}
