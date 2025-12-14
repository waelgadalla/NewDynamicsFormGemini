using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using DynamicForms.Models.Theming;
using DynamicForms.SqlServer.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DynamicForms.SqlServer.Repositories;

/// <summary>
/// SQL Server implementation of IThemeRepository.
/// Uses Dapper for high-performance database operations.
/// Stores FormTheme as JSON in the Themes table.
/// </summary>
public class SqlServerThemeRepository : IThemeRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerThemeRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqlServerThemeRepository(
        string connectionString,
        ILogger<SqlServerThemeRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }

    /// <inheritdoc />
    public async Task<FormTheme?> GetByIdAsync(string themeId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT ThemeJson
            FROM Themes
            WHERE Id = @ThemeId AND IsActive = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var themeJson = await connection.QueryFirstOrDefaultAsync<string>(
                sql,
                new { ThemeId = themeId });

            if (string.IsNullOrEmpty(themeJson))
            {
                _logger.LogDebug("Theme {ThemeId} not found", themeId);
                return null;
            }

            var theme = JsonSerializer.Deserialize<FormTheme>(themeJson, _jsonOptions);
            _logger.LogDebug("Retrieved theme {ThemeId}: {ThemeName}", themeId, theme?.Name);

            return theme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving theme {ThemeId}", themeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<FormTheme?> GetDefaultAsync(string? organizationId = null, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT ThemeJson
            FROM Themes
            WHERE IsDefault = 1 AND IsActive = 1";

        if (organizationId != null)
        {
            sql += " AND (OrganizationId = @OrganizationId OR OrganizationId IS NULL)";
        }

        sql += " ORDER BY CASE WHEN OrganizationId = @OrganizationId THEN 0 ELSE 1 END";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var themeJson = await connection.QueryFirstOrDefaultAsync<string>(
                sql,
                new { OrganizationId = organizationId });

            if (string.IsNullOrEmpty(themeJson))
            {
                _logger.LogDebug("No default theme found for organization {OrganizationId}", organizationId);
                return null;
            }

            return JsonSerializer.Deserialize<FormTheme>(themeJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default theme for organization {OrganizationId}", organizationId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ThemeSummaryDto>> GetAllAsync(string? organizationId = null, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT
                Id, Name, Description, BasePreset, PreviewColor, Mode,
                IsDefault, IsLocked, OrganizationId, CreatedBy,
                CreatedAt, ModifiedAt, Version
            FROM Themes
            WHERE IsActive = 1";

        if (organizationId != null)
        {
            sql += " AND (OrganizationId = @OrganizationId OR OrganizationId IS NULL)";
        }

        sql += " ORDER BY IsDefault DESC, ModifiedAt DESC";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<ThemeSummaryDto>(sql, new { OrganizationId = organizationId });

            _logger.LogDebug("Retrieved {Count} themes for organization {OrganizationId}",
                results.Count(), organizationId);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all themes for organization {OrganizationId}", organizationId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ThemeSummaryDto>> SearchAsync(string searchTerm, string? organizationId = null, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT
                Id, Name, Description, BasePreset, PreviewColor, Mode,
                IsDefault, IsLocked, OrganizationId, CreatedBy,
                CreatedAt, ModifiedAt, Version
            FROM Themes
            WHERE IsActive = 1
              AND (Name LIKE @SearchTerm OR Description LIKE @SearchTerm)";

        if (organizationId != null)
        {
            sql += " AND (OrganizationId = @OrganizationId OR OrganizationId IS NULL)";
        }

        sql += " ORDER BY IsDefault DESC, ModifiedAt DESC";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<ThemeSummaryDto>(
                sql,
                new { SearchTerm = $"%{searchTerm}%", OrganizationId = organizationId });

            _logger.LogDebug("Search for '{SearchTerm}' returned {Count} themes", searchTerm, results.Count());

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching themes with term '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ThemeSummaryDto>> GetByModeAsync(ThemeMode mode, string? organizationId = null, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT
                Id, Name, Description, BasePreset, PreviewColor, Mode,
                IsDefault, IsLocked, OrganizationId, CreatedBy,
                CreatedAt, ModifiedAt, Version
            FROM Themes
            WHERE IsActive = 1 AND Mode = @Mode";

        if (organizationId != null)
        {
            sql += " AND (OrganizationId = @OrganizationId OR OrganizationId IS NULL)";
        }

        sql += " ORDER BY IsDefault DESC, ModifiedAt DESC";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<ThemeSummaryDto>(
                sql,
                new { Mode = mode.ToString(), OrganizationId = organizationId });

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving themes by mode {Mode}", mode);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SaveAsync(FormTheme theme, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        const string checkExistsSql = @"
            SELECT COUNT(1) FROM Themes
            WHERE Id = @ThemeId AND IsActive = 1";

        const string updateSql = @"
            UPDATE Themes
            SET Name = @Name,
                Description = @Description,
                BasePreset = @BasePreset,
                ThemeJson = @ThemeJson,
                PreviewColor = @PreviewColor,
                Mode = @Mode,
                Version = Version + 1
            WHERE Id = @ThemeId AND IsActive = 1";

        const string insertSql = @"
            INSERT INTO Themes
                (Id, Name, Description, BasePreset, ThemeJson, PreviewColor, Mode, IsDefault, IsLocked, OrganizationId, CreatedBy, Version)
            VALUES
                (@ThemeId, @Name, @Description, @BasePreset, @ThemeJson, @PreviewColor, @Mode, 0, 0, @OrganizationId, @CreatedBy, 1)";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var themeJson = JsonSerializer.Serialize(theme, _jsonOptions);

                var exists = await connection.ExecuteScalarAsync<int>(
                    checkExistsSql,
                    new { ThemeId = theme.Id },
                    transaction) > 0;

                var parameters = new
                {
                    ThemeId = theme.Id,
                    theme.Name,
                    theme.Description,
                    theme.BasePreset,
                    ThemeJson = themeJson,
                    PreviewColor = theme.Colors.Primary,
                    Mode = theme.Mode.ToString(),
                    OrganizationId = (string?)null,
                    CreatedBy = createdBy ?? theme.CreatedBy
                };

                if (exists)
                {
                    await connection.ExecuteAsync(updateSql, parameters, transaction);
                    _logger.LogInformation("Updated theme {ThemeId}: {ThemeName}", theme.Id, theme.Name);
                }
                else
                {
                    await connection.ExecuteAsync(insertSql, parameters, transaction);
                    _logger.LogInformation("Created new theme {ThemeId}: {ThemeName}", theme.Id, theme.Name);
                }

                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving theme {ThemeId}", theme.Id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string themeId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Themes
            SET IsActive = 0
            WHERE Id = @ThemeId AND IsActive = 1 AND IsLocked = 0";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var rowsAffected = await connection.ExecuteAsync(sql, new { ThemeId = themeId });

            _logger.LogInformation("Soft-deleted theme {ThemeId}, rows affected: {RowsAffected}",
                themeId, rowsAffected);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting theme {ThemeId}", themeId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SetDefaultAsync(string themeId, string? organizationId = null, CancellationToken cancellationToken = default)
    {
        const string clearDefaultSql = @"
            UPDATE Themes
            SET IsDefault = 0
            WHERE IsDefault = 1 AND IsActive = 1
              AND (OrganizationId = @OrganizationId OR (@OrganizationId IS NULL AND OrganizationId IS NULL))";

        const string setDefaultSql = @"
            UPDATE Themes
            SET IsDefault = 1
            WHERE Id = @ThemeId AND IsActive = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                // Clear existing default
                await connection.ExecuteAsync(clearDefaultSql, new { OrganizationId = organizationId }, transaction);

                // Set new default
                var rowsAffected = await connection.ExecuteAsync(setDefaultSql, new { ThemeId = themeId }, transaction);

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Set theme {ThemeId} as default for organization {OrganizationId}",
                    themeId, organizationId);

                return rowsAffected > 0;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting theme {ThemeId} as default", themeId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string themeId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS(
                SELECT 1 FROM Themes
                WHERE Id = @ThemeId AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            return await connection.ExecuteScalarAsync<bool>(sql, new { ThemeId = themeId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of theme {ThemeId}", themeId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<FormTheme?> DuplicateAsync(string themeId, string newName, CancellationToken cancellationToken = default)
    {
        try
        {
            var original = await GetByIdAsync(themeId, cancellationToken);
            if (original == null)
            {
                _logger.LogWarning("Cannot duplicate theme {ThemeId}: not found", themeId);
                return null;
            }

            var duplicate = original.Clone();
            duplicate.Id = Guid.NewGuid().ToString();
            duplicate.Name = newName;
            duplicate.CreatedAt = DateTime.UtcNow;
            duplicate.ModifiedAt = DateTime.UtcNow;
            duplicate.Version = 1;

            var success = await SaveAsync(duplicate, cancellationToken: cancellationToken);

            if (success)
            {
                _logger.LogInformation("Duplicated theme {OriginalId} as {NewId}: {NewName}",
                    themeId, duplicate.Id, newName);
                return duplicate;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating theme {ThemeId}", themeId);
            throw;
        }
    }
}
