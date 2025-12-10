namespace VisualEditorOpus.Models;

/// <summary>
/// Represents device types for form preview simulation
/// </summary>
public enum PreviewDevice
{
    /// <summary>
    /// Desktop view - full width
    /// </summary>
    Desktop = 0,

    /// <summary>
    /// Tablet view - constrained to 768px
    /// </summary>
    Tablet = 1,

    /// <summary>
    /// Mobile view - constrained to 375px
    /// </summary>
    Mobile = 2
}
