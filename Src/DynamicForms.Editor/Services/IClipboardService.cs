using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Services;

public interface IClipboardService
{
    // Internal Clipboard (Memory)
    bool HasContent { get; }
    void CopyField(FormFieldSchema field);
    FormFieldSchema? GetField();
    
    // System Clipboard (via JS)
    Task CopyTextToSystemClipboardAsync(string text);
    Task<string> ReadTextFromSystemClipboardAsync();
}
