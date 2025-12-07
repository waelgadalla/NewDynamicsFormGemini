namespace VisualEditorOpus.Services;

public class ThemeService : IThemeService
{
    private bool _isDarkMode;

    public bool IsDarkMode => _isDarkMode;

    public event Action? OnThemeChanged;

    public void Toggle()
    {
        _isDarkMode = !_isDarkMode;
        OnThemeChanged?.Invoke();
    }

    public void SetDarkMode(bool isDark)
    {
        if (_isDarkMode != isDark)
        {
            _isDarkMode = isDark;
            OnThemeChanged?.Invoke();
        }
    }
}
