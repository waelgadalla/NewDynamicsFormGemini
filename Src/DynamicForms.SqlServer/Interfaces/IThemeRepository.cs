using DynamicForms.Models.Theming;

namespace DynamicForms.SqlServer.Interfaces;

/// <summary>
/// Repository interface for theme persistence operations.
/// </summary>
public interface IThemeRepository
{
    /// <summary>
    /// Get a theme by its ID.
    /// </summary>
    Task<FormTheme?> GetByIdAsync(string themeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the default theme, optionally filtered by organization.
    /// </summary>
    Task<FormTheme?> GetDefaultAsync(string? organizationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active themes, optionally filtered by organization.
    /// </summary>
    Task<IEnumerable<ThemeSummaryDto>> GetAllAsync(string? organizationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search themes by name or description.
    /// </summary>
    Task<IEnumerable<ThemeSummaryDto>> SearchAsync(string searchTerm, string? organizationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get themes filtered by mode (Light, Dark, Auto).
    /// </summary>
    Task<IEnumerable<ThemeSummaryDto>> GetByModeAsync(ThemeMode mode, string? organizationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save a theme (insert or update).
    /// </summary>
    Task<bool> SaveAsync(FormTheme theme, string? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a theme (soft delete).
    /// </summary>
    Task<bool> DeleteAsync(string themeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set a theme as the default.
    /// </summary>
    Task<bool> SetDefaultAsync(string themeId, string? organizationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a theme exists.
    /// </summary>
    Task<bool> ExistsAsync(string themeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Duplicate a theme with a new ID and name.
    /// </summary>
    Task<FormTheme?> DuplicateAsync(string themeId, string newName, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for theme summary information from database.
/// </summary>
public record ThemeSummaryDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? BasePreset { get; init; }
    public string? PreviewColor { get; init; }
    public required string Mode { get; init; }
    public bool IsDefault { get; init; }
    public bool IsLocked { get; init; }
    public string? OrganizationId { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ModifiedAt { get; init; }
    public int Version { get; init; }
}
