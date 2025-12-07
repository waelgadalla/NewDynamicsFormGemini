using DynamicForms.Core.V4.Schemas;
using Microsoft.JSInterop;

namespace DynamicForms.Editor.Services;

public class ClipboardService : IClipboardService
{
    private readonly IJSRuntime _jsRuntime;
    private FormFieldSchema? _copiedField;

    public ClipboardService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool HasContent => _copiedField != null;

    public void CopyField(FormFieldSchema field)
    {
        // Deep copy via record clone to prevent reference issues
        _copiedField = field with { }; 
    }

    public FormFieldSchema? GetField()
    {
        if (_copiedField == null) return null;
        
        // Return a fresh clone so the clipboard template remains untouched
        // We must generate a new ID for the pasted item (logic usually handled by caller, 
        // but we ensure we don't pass the exact same reference)
        return _copiedField with { }; 
    }

    public async Task CopyTextToSystemClipboardAsync(string text)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
        catch
        {
            // Fallback or ignore if permission denied/not secure context
        }
    }

    public async Task<string> ReadTextFromSystemClipboardAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("navigator.clipboard.readText");
        }
        catch
        {
            return string.Empty;
        }
    }
}
