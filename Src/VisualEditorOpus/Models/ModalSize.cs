namespace VisualEditorOpus.Models;

/// <summary>
/// Defines the available size variants for modal dialogs.
/// </summary>
public enum ModalSize
{
    /// <summary>
    /// Small modal with max-width of 400px
    /// </summary>
    Small,

    /// <summary>
    /// Medium modal with max-width of 560px (default)
    /// </summary>
    Medium,

    /// <summary>
    /// Large modal with max-width of 720px
    /// </summary>
    Large,

    /// <summary>
    /// Extra large modal with max-width of 900px
    /// </summary>
    ExtraLarge,

    /// <summary>
    /// Fullscreen modal that fills the viewport
    /// </summary>
    Fullscreen
}
