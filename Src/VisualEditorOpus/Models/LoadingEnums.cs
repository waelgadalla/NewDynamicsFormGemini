namespace VisualEditorOpus.Models;

/// <summary>
/// Size variants for loading spinner
/// </summary>
public enum SpinnerSize
{
    /// <summary>
    /// Small spinner (20px)
    /// </summary>
    Small,

    /// <summary>
    /// Medium spinner (32px) - Default
    /// </summary>
    Medium,

    /// <summary>
    /// Large spinner (48px)
    /// </summary>
    Large
}

/// <summary>
/// Color variants for loading spinner
/// </summary>
public enum SpinnerColor
{
    /// <summary>
    /// Primary color (indigo)
    /// </summary>
    Primary,

    /// <summary>
    /// White color (for dark backgrounds)
    /// </summary>
    White
}

/// <summary>
/// Skeleton loader shape variants
/// </summary>
public enum SkeletonVariant
{
    /// <summary>
    /// Text line placeholder (14px height)
    /// </summary>
    Text,

    /// <summary>
    /// Title placeholder (20px height, 60% width)
    /// </summary>
    Title,

    /// <summary>
    /// Avatar circle placeholder (48px)
    /// </summary>
    Avatar,

    /// <summary>
    /// Button placeholder (36px height)
    /// </summary>
    Button,

    /// <summary>
    /// Icon placeholder (44px square)
    /// </summary>
    Icon,

    /// <summary>
    /// Full card skeleton with header and body
    /// </summary>
    Card,

    /// <summary>
    /// Form field skeleton with label and input
    /// </summary>
    Field,

    /// <summary>
    /// List item skeleton with avatar and text
    /// </summary>
    ListItem,

    /// <summary>
    /// Table row skeleton
    /// </summary>
    TableRow
}

/// <summary>
/// Size variants for skeleton elements
/// </summary>
public enum SkeletonSize
{
    /// <summary>
    /// Small skeleton element
    /// </summary>
    Small,

    /// <summary>
    /// Medium skeleton element - Default
    /// </summary>
    Medium
}

/// <summary>
/// Button style variants
/// </summary>
public enum ButtonVariant
{
    /// <summary>
    /// Primary filled button
    /// </summary>
    Primary,

    /// <summary>
    /// Outline button
    /// </summary>
    Outline,

    /// <summary>
    /// Ghost/text button
    /// </summary>
    Ghost,

    /// <summary>
    /// Success button (green)
    /// </summary>
    Success,

    /// <summary>
    /// Danger button (red)
    /// </summary>
    Danger
}
