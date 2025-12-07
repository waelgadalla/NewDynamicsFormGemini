namespace DynamicForms.Editor.Services;

public interface IThemeService
{
    bool IsDarkMode { get; }
    event Action? OnThemeChanged;
    Task Toggle();
    Task SetTheme(bool isDark);
    Task Initialize();
}
