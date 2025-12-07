namespace VisualEditorOpus.Services;

public interface IThemeService
{
    bool IsDarkMode { get; }
    event Action? OnThemeChanged;
    void Toggle();
    void SetDarkMode(bool isDark);
}
