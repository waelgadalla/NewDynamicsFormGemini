using DynamicForms.Models.Theming;
using DynamicForms.SqlServer.Interfaces;
using Microsoft.Extensions.Logging;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Implementation of IThemePersistenceService.
/// Provides persistence operations for themes.
/// </summary>
public class ThemePersistenceService : IThemePersistenceService
{
    private readonly IThemeRepository _repository;
    private readonly ILogger<ThemePersistenceService> _logger;

    public ThemePersistenceService(
        IThemeRepository repository,
        ILogger<ThemePersistenceService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<FormTheme?> GetThemeAsync(string themeId)
    {
        try
        {
            _logger.LogInformation("Loading theme {ThemeId}", themeId);
            var theme = await _repository.GetByIdAsync(themeId);

            if (theme != null)
            {
                _logger.LogInformation("Theme {ThemeId} loaded: {ThemeName}", themeId, theme.Name);
            }
            else
            {
                _logger.LogWarning("Theme {ThemeId} not found", themeId);
            }

            return theme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading theme {ThemeId}", themeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<FormTheme?> GetDefaultThemeAsync()
    {
        try
        {
            _logger.LogInformation("Loading default theme");
            var theme = await _repository.GetDefaultAsync();

            if (theme != null)
            {
                _logger.LogInformation("Default theme loaded: {ThemeName}", theme.Name);
            }
            else
            {
                _logger.LogInformation("No default theme found");
            }

            return theme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading default theme");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ThemeSummary>> ListThemesAsync()
    {
        try
        {
            var dtos = await _repository.GetAllAsync();
            var summaries = dtos.Select(MapToSummary).ToList();

            _logger.LogInformation("Retrieved {Count} themes", summaries.Count);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing themes");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ThemeSummary>> SearchThemesAsync(string searchTerm)
    {
        try
        {
            var dtos = await _repository.SearchAsync(searchTerm);
            var summaries = dtos.Select(MapToSummary).ToList();

            _logger.LogInformation("Search for '{SearchTerm}' returned {Count} themes", searchTerm, summaries.Count);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching themes with term '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ThemeSummary>> GetThemesByModeAsync(ThemeMode mode)
    {
        try
        {
            var dtos = await _repository.GetByModeAsync(mode);
            var summaries = dtos.Select(MapToSummary).ToList();

            _logger.LogInformation("Retrieved {Count} themes with mode {Mode}", summaries.Count, mode);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting themes by mode {Mode}", mode);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> SaveThemeAsync(FormTheme theme)
    {
        try
        {
            _logger.LogInformation("Saving theme {ThemeId}: {ThemeName}", theme.Id, theme.Name);

            theme.ModifiedAt = DateTime.UtcNow;

            var success = await _repository.SaveAsync(theme);

            if (success)
            {
                _logger.LogInformation("Theme {ThemeId} saved successfully", theme.Id);
                return theme.Id;
            }
            else
            {
                _logger.LogWarning("Failed to save theme {ThemeId}", theme.Id);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving theme {ThemeId}", theme.Id);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateThemeAsync(FormTheme theme)
    {
        try
        {
            _logger.LogInformation("Updating theme {ThemeId}: {ThemeName}", theme.Id, theme.Name);

            theme.ModifiedAt = DateTime.UtcNow;
            theme.Version++;

            var success = await _repository.SaveAsync(theme);

            if (success)
            {
                _logger.LogInformation("Theme {ThemeId} updated successfully", theme.Id);
            }
            else
            {
                _logger.LogWarning("Failed to update theme {ThemeId}", theme.Id);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating theme {ThemeId}", theme.Id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteThemeAsync(string themeId)
    {
        try
        {
            _logger.LogInformation("Deleting theme {ThemeId}", themeId);

            var success = await _repository.DeleteAsync(themeId);

            if (success)
            {
                _logger.LogInformation("Theme {ThemeId} deleted successfully", themeId);
            }
            else
            {
                _logger.LogWarning("Failed to delete theme {ThemeId}", themeId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting theme {ThemeId}", themeId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SetDefaultThemeAsync(string themeId)
    {
        try
        {
            _logger.LogInformation("Setting theme {ThemeId} as default", themeId);

            var success = await _repository.SetDefaultAsync(themeId);

            if (success)
            {
                _logger.LogInformation("Theme {ThemeId} set as default successfully", themeId);
            }
            else
            {
                _logger.LogWarning("Failed to set theme {ThemeId} as default", themeId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting theme {ThemeId} as default", themeId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<FormTheme?> DuplicateThemeAsync(string themeId, string newName)
    {
        try
        {
            _logger.LogInformation("Duplicating theme {ThemeId} as '{NewName}'", themeId, newName);

            var duplicate = await _repository.DuplicateAsync(themeId, newName);

            if (duplicate != null)
            {
                _logger.LogInformation("Theme duplicated: {NewThemeId}", duplicate.Id);
            }
            else
            {
                _logger.LogWarning("Failed to duplicate theme {ThemeId}", themeId);
            }

            return duplicate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating theme {ThemeId}", themeId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ThemeExistsAsync(string themeId)
    {
        try
        {
            return await _repository.ExistsAsync(themeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if theme {ThemeId} exists", themeId);
            return false;
        }
    }

    private static ThemeSummary MapToSummary(ThemeSummaryDto dto)
    {
        return new ThemeSummary
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            BasePreset = dto.BasePreset,
            PreviewColor = dto.PreviewColor,
            Mode = Enum.TryParse<ThemeMode>(dto.Mode, out var mode) ? mode : ThemeMode.Light,
            IsDefault = dto.IsDefault,
            IsLocked = dto.IsLocked,
            OrganizationId = dto.OrganizationId,
            CreatedBy = dto.CreatedBy,
            CreatedAt = dto.CreatedAt,
            ModifiedAt = dto.ModifiedAt,
            Version = dto.Version
        };
    }
}
