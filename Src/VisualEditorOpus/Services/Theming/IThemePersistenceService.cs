using DynamicForms.Models.Theming;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Service for persisting themes to the database.
/// </summary>
public interface IThemePersistenceService
{
    /// <summary>
    /// Get a theme by its ID.
    /// </summary>
    Task<FormTheme?> GetThemeAsync(string themeId);

    /// <summary>
    /// Get the default theme.
    /// </summary>
    Task<FormTheme?> GetDefaultThemeAsync();

    /// <summary>
    /// Get all themes as summaries for list display.
    /// </summary>
    Task<IReadOnlyList<ThemeSummary>> ListThemesAsync();

    /// <summary>
    /// Search themes by name or description.
    /// </summary>
    Task<IReadOnlyList<ThemeSummary>> SearchThemesAsync(string searchTerm);

    /// <summary>
    /// Get themes filtered by mode.
    /// </summary>
    Task<IReadOnlyList<ThemeSummary>> GetThemesByModeAsync(ThemeMode mode);

    /// <summary>
    /// Save a theme (creates new or updates existing).
    /// Returns the theme ID on success.
    /// </summary>
    Task<string?> SaveThemeAsync(FormTheme theme);

    /// <summary>
    /// Update an existing theme.
    /// </summary>
    Task<bool> UpdateThemeAsync(FormTheme theme);

    /// <summary>
    /// Delete a theme by ID.
    /// </summary>
    Task<bool> DeleteThemeAsync(string themeId);

    /// <summary>
    /// Set a theme as the default.
    /// </summary>
    Task<bool> SetDefaultThemeAsync(string themeId);

    /// <summary>
    /// Duplicate a theme with a new name.
    /// Returns the new theme.
    /// </summary>
    Task<FormTheme?> DuplicateThemeAsync(string themeId, string newName);

    /// <summary>
    /// Check if a theme exists.
    /// </summary>
    Task<bool> ThemeExistsAsync(string themeId);
}
