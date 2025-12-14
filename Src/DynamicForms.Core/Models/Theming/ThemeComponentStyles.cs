using System.Text.Json.Serialization;

namespace DynamicForms.Models.Theming;

/// <summary>
/// Component-specific style overrides for advanced theme customization.
/// These override the global theme settings for specific component types.
/// </summary>
public class ThemeComponentStyles
{
    /// <summary>
    /// Style overrides for buttons.
    /// </summary>
    [JsonPropertyName("buttons")]
    public ButtonStyles Buttons { get; set; } = new();

    /// <summary>
    /// Style overrides for text inputs.
    /// </summary>
    [JsonPropertyName("inputs")]
    public InputStyles Inputs { get; set; } = new();

    /// <summary>
    /// Style overrides for dropdown/select elements.
    /// </summary>
    [JsonPropertyName("dropdowns")]
    public DropdownStyles Dropdowns { get; set; } = new();

    /// <summary>
    /// Style overrides for checkboxes.
    /// </summary>
    [JsonPropertyName("checkboxes")]
    public CheckboxStyles Checkboxes { get; set; } = new();

    /// <summary>
    /// Style overrides for radio buttons.
    /// </summary>
    [JsonPropertyName("radioButtons")]
    public RadioStyles RadioButtons { get; set; } = new();

    /// <summary>
    /// Style overrides for panels and sections.
    /// </summary>
    [JsonPropertyName("panels")]
    public PanelStyles Panels { get; set; } = new();

    /// <summary>
    /// Style overrides for progress indicators.
    /// </summary>
    [JsonPropertyName("progressBar")]
    public ProgressBarStyles ProgressBar { get; set; } = new();

    /// <summary>
    /// Style overrides for navigation buttons.
    /// </summary>
    [JsonPropertyName("navigation")]
    public NavigationStyles Navigation { get; set; } = new();

    /// <summary>
    /// Creates a deep clone of all component styles.
    /// </summary>
    public ThemeComponentStyles Clone()
    {
        return new ThemeComponentStyles
        {
            Buttons = Buttons.Clone(),
            Inputs = Inputs.Clone(),
            Dropdowns = Dropdowns.Clone(),
            Checkboxes = Checkboxes.Clone(),
            RadioButtons = RadioButtons.Clone(),
            Panels = Panels.Clone(),
            ProgressBar = ProgressBar.Clone(),
            Navigation = Navigation.Clone()
        };
    }
}

/// <summary>
/// Style overrides for button components.
/// </summary>
public class ButtonStyles
{
    [JsonPropertyName("primaryBackground")]
    public string PrimaryBackground { get; set; } = "";

    [JsonPropertyName("primaryText")]
    public string PrimaryText { get; set; } = "";

    [JsonPropertyName("primaryBorder")]
    public string PrimaryBorder { get; set; } = "";

    [JsonPropertyName("primaryHoverBackground")]
    public string PrimaryHoverBackground { get; set; } = "";

    [JsonPropertyName("secondaryBackground")]
    public string SecondaryBackground { get; set; } = "";

    [JsonPropertyName("secondaryText")]
    public string SecondaryText { get; set; } = "";

    [JsonPropertyName("secondaryBorder")]
    public string SecondaryBorder { get; set; } = "";

    [JsonPropertyName("borderRadius")]
    public string BorderRadius { get; set; } = "";

    [JsonPropertyName("padding")]
    public string Padding { get; set; } = "";

    [JsonPropertyName("fontSize")]
    public string FontSize { get; set; } = "";

    [JsonPropertyName("fontWeight")]
    public string FontWeight { get; set; } = "";

    public ButtonStyles Clone() => new()
    {
        PrimaryBackground = PrimaryBackground,
        PrimaryText = PrimaryText,
        PrimaryBorder = PrimaryBorder,
        PrimaryHoverBackground = PrimaryHoverBackground,
        SecondaryBackground = SecondaryBackground,
        SecondaryText = SecondaryText,
        SecondaryBorder = SecondaryBorder,
        BorderRadius = BorderRadius,
        Padding = Padding,
        FontSize = FontSize,
        FontWeight = FontWeight
    };
}

/// <summary>
/// Style overrides for input components.
/// </summary>
public class InputStyles
{
    [JsonPropertyName("background")]
    public string Background { get; set; } = "";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("border")]
    public string Border { get; set; } = "";

    [JsonPropertyName("borderWidth")]
    public string BorderWidth { get; set; } = "";

    [JsonPropertyName("borderRadius")]
    public string BorderRadius { get; set; } = "";

    [JsonPropertyName("padding")]
    public string Padding { get; set; } = "";

    [JsonPropertyName("placeholderColor")]
    public string PlaceholderColor { get; set; } = "";

    [JsonPropertyName("focusBorder")]
    public string FocusBorder { get; set; } = "";

    [JsonPropertyName("focusShadow")]
    public string FocusShadow { get; set; } = "";

    [JsonPropertyName("disabledBackground")]
    public string DisabledBackground { get; set; } = "";

    [JsonPropertyName("disabledText")]
    public string DisabledText { get; set; } = "";

    public InputStyles Clone() => new()
    {
        Background = Background,
        Text = Text,
        Border = Border,
        BorderWidth = BorderWidth,
        BorderRadius = BorderRadius,
        Padding = Padding,
        PlaceholderColor = PlaceholderColor,
        FocusBorder = FocusBorder,
        FocusShadow = FocusShadow,
        DisabledBackground = DisabledBackground,
        DisabledText = DisabledText
    };
}

/// <summary>
/// Style overrides for dropdown/select components.
/// </summary>
public class DropdownStyles
{
    [JsonPropertyName("background")]
    public string Background { get; set; } = "";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("border")]
    public string Border { get; set; } = "";

    [JsonPropertyName("borderRadius")]
    public string BorderRadius { get; set; } = "";

    [JsonPropertyName("arrowColor")]
    public string ArrowColor { get; set; } = "";

    [JsonPropertyName("optionHoverBackground")]
    public string OptionHoverBackground { get; set; } = "";

    [JsonPropertyName("optionSelectedBackground")]
    public string OptionSelectedBackground { get; set; } = "";

    [JsonPropertyName("dropdownShadow")]
    public string DropdownShadow { get; set; } = "";

    public DropdownStyles Clone() => new()
    {
        Background = Background,
        Text = Text,
        Border = Border,
        BorderRadius = BorderRadius,
        ArrowColor = ArrowColor,
        OptionHoverBackground = OptionHoverBackground,
        OptionSelectedBackground = OptionSelectedBackground,
        DropdownShadow = DropdownShadow
    };
}

/// <summary>
/// Style overrides for checkbox components.
/// </summary>
public class CheckboxStyles
{
    [JsonPropertyName("size")]
    public string Size { get; set; } = "";

    [JsonPropertyName("borderColor")]
    public string BorderColor { get; set; } = "";

    [JsonPropertyName("borderRadius")]
    public string BorderRadius { get; set; } = "";

    [JsonPropertyName("checkedBackground")]
    public string CheckedBackground { get; set; } = "";

    [JsonPropertyName("checkMarkColor")]
    public string CheckMarkColor { get; set; } = "";

    [JsonPropertyName("labelSpacing")]
    public string LabelSpacing { get; set; } = "";

    public CheckboxStyles Clone() => new()
    {
        Size = Size,
        BorderColor = BorderColor,
        BorderRadius = BorderRadius,
        CheckedBackground = CheckedBackground,
        CheckMarkColor = CheckMarkColor,
        LabelSpacing = LabelSpacing
    };
}

/// <summary>
/// Style overrides for radio button components.
/// </summary>
public class RadioStyles
{
    [JsonPropertyName("size")]
    public string Size { get; set; } = "";

    [JsonPropertyName("borderColor")]
    public string BorderColor { get; set; } = "";

    [JsonPropertyName("selectedBackground")]
    public string SelectedBackground { get; set; } = "";

    [JsonPropertyName("dotColor")]
    public string DotColor { get; set; } = "";

    [JsonPropertyName("dotSize")]
    public string DotSize { get; set; } = "";

    [JsonPropertyName("labelSpacing")]
    public string LabelSpacing { get; set; } = "";

    public RadioStyles Clone() => new()
    {
        Size = Size,
        BorderColor = BorderColor,
        SelectedBackground = SelectedBackground,
        DotColor = DotColor,
        DotSize = DotSize,
        LabelSpacing = LabelSpacing
    };
}

/// <summary>
/// Style overrides for panel/section components.
/// </summary>
public class PanelStyles
{
    [JsonPropertyName("background")]
    public string Background { get; set; } = "";

    [JsonPropertyName("border")]
    public string Border { get; set; } = "";

    [JsonPropertyName("borderRadius")]
    public string BorderRadius { get; set; } = "";

    [JsonPropertyName("shadow")]
    public string Shadow { get; set; } = "";

    [JsonPropertyName("padding")]
    public string Padding { get; set; } = "";

    [JsonPropertyName("headerBackground")]
    public string HeaderBackground { get; set; } = "";

    [JsonPropertyName("headerText")]
    public string HeaderText { get; set; } = "";

    [JsonPropertyName("collapseIconColor")]
    public string CollapseIconColor { get; set; } = "";

    public PanelStyles Clone() => new()
    {
        Background = Background,
        Border = Border,
        BorderRadius = BorderRadius,
        Shadow = Shadow,
        Padding = Padding,
        HeaderBackground = HeaderBackground,
        HeaderText = HeaderText,
        CollapseIconColor = CollapseIconColor
    };
}

/// <summary>
/// Style overrides for progress bar/indicator components.
/// </summary>
public class ProgressBarStyles
{
    [JsonPropertyName("height")]
    public string Height { get; set; } = "";

    [JsonPropertyName("trackBackground")]
    public string TrackBackground { get; set; } = "";

    [JsonPropertyName("fillBackground")]
    public string FillBackground { get; set; } = "";

    [JsonPropertyName("borderRadius")]
    public string BorderRadius { get; set; } = "";

    [JsonPropertyName("textColor")]
    public string TextColor { get; set; } = "";

    [JsonPropertyName("showPercentage")]
    public bool? ShowPercentage { get; set; }

    public ProgressBarStyles Clone() => new()
    {
        Height = Height,
        TrackBackground = TrackBackground,
        FillBackground = FillBackground,
        BorderRadius = BorderRadius,
        TextColor = TextColor,
        ShowPercentage = ShowPercentage
    };
}

/// <summary>
/// Style overrides for navigation button components.
/// </summary>
public class NavigationStyles
{
    [JsonPropertyName("buttonStyle")]
    public string ButtonStyle { get; set; } = ""; // "primary", "secondary", "text"

    [JsonPropertyName("alignment")]
    public string Alignment { get; set; } = ""; // "left", "center", "right", "space-between"

    [JsonPropertyName("previousButtonText")]
    public string PreviousButtonText { get; set; } = "";

    [JsonPropertyName("nextButtonText")]
    public string NextButtonText { get; set; } = "";

    [JsonPropertyName("submitButtonText")]
    public string SubmitButtonText { get; set; } = "";

    [JsonPropertyName("showIcons")]
    public bool? ShowIcons { get; set; }

    public NavigationStyles Clone() => new()
    {
        ButtonStyle = ButtonStyle,
        Alignment = Alignment,
        PreviousButtonText = PreviousButtonText,
        NextButtonText = NextButtonText,
        SubmitButtonText = SubmitButtonText,
        ShowIcons = ShowIcons
    };
}
