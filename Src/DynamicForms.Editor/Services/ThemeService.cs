using Microsoft.JSInterop;

namespace DynamicForms.Editor.Services;

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _js;
    private bool _isDarkMode;

    public bool IsDarkMode => _isDarkMode;
    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task Initialize()
    {
        var theme = await _js.InvokeAsync<string>("getTheme");
        _isDarkMode = theme == "dark";
        NotifyStateChanged();
    }

    public async Task Toggle()
    {
        await SetTheme(!_isDarkMode);
    }

    public async Task SetTheme(bool isDark)
    {
        _isDarkMode = isDark;
        await _js.InvokeVoidAsync("setTheme", isDark ? "dark" : "light");
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnThemeChanged?.Invoke();
}
