namespace VisualEditorOpus.Models;

/// <summary>
/// Represents the different views available in the form editor
/// </summary>
public enum EditorView
{
    /// <summary>
    /// Design view - drag-and-drop form builder
    /// </summary>
    Design = 0,

    /// <summary>
    /// Preview view - rendered form preview
    /// </summary>
    Preview = 1,

    /// <summary>
    /// JSON view - raw schema editor
    /// </summary>
    Json = 2
}
