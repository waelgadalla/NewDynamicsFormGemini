using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using DynamicForms.SqlServer.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DynamicForms.SqlServer.Repositories;

/// <summary>
/// SQL Server implementation of ICodeSetRepository.
/// Uses Dapper for high-performance database operations.
/// Stores ManagedCodeSet as JSON in the CodeSets table.
/// </summary>
public class SqlServerCodeSetRepository : ICodeSetRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerCodeSetRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqlServerCodeSetRepository(
        string connectionString,
        ILogger<SqlServerCodeSetRepository> logger)
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
    public async Task<int> SaveAsync(CodeSetEntity codeSet, string? userId = null)
    {
        const string checkExistsSql = @"
            SELECT Id FROM CodeSets
            WHERE Id = @Id AND IsActive = 1";

        const string updateSql = @"
            UPDATE CodeSets
            SET Code = @Code,
                NameEn = @NameEn,
                NameFr = @NameFr,
                DescriptionEn = @DescriptionEn,
                DescriptionFr = @DescriptionFr,
                Category = @Category,
                SchemaJson = @SchemaJson,
                Version = @Version,
                DateUpdated = GETUTCDATE(),
                UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsActive = 1;
            SELECT @Id;";

        const string insertSql = @"
            INSERT INTO CodeSets
                (Code, NameEn, NameFr, DescriptionEn, DescriptionFr, Category, SchemaJson,
                 IsActive, Version, DateCreated, DateUpdated, CreatedBy, UpdatedBy)
            VALUES
                (@Code, @NameEn, @NameFr, @DescriptionEn, @DescriptionFr, @Category, @SchemaJson,
                 1, @Version, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @UpdatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var existingId = await connection.QueryFirstOrDefaultAsync<int?>(
                    checkExistsSql,
                    new { codeSet.Id },
                    transaction);

                var parameters = new
                {
                    codeSet.Id,
                    codeSet.Code,
                    codeSet.NameEn,
                    codeSet.NameFr,
                    codeSet.DescriptionEn,
                    codeSet.DescriptionFr,
                    codeSet.Category,
                    codeSet.SchemaJson,
                    codeSet.Version,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };

                int resultId;
                if (existingId.HasValue)
                {
                    resultId = await connection.QuerySingleAsync<int>(updateSql, parameters, transaction);
                    _logger.LogInformation("Updated CodeSet {Id} - {Code}", resultId, codeSet.Code);
                }
                else
                {
                    resultId = await connection.QuerySingleAsync<int>(insertSql, parameters, transaction);
                    _logger.LogInformation("Created new CodeSet {Id} - {Code}", resultId, codeSet.Code);
                }

                await transaction.CommitAsync();
                return resultId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving CodeSet {Code}", codeSet.Code);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CodeSetEntity?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Code, NameEn, NameFr, DescriptionEn, DescriptionFr, Category,
                   SchemaJson, IsActive, Version, DateCreated, DateUpdated, CreatedBy, UpdatedBy
            FROM CodeSets
            WHERE Id = @Id AND IsActive = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var result = await connection.QueryFirstOrDefaultAsync<CodeSetEntity>(sql, new { Id = id });

            if (result == null)
            {
                _logger.LogDebug("CodeSet {Id} not found", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CodeSet {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CodeSetEntity?> GetByCodeAsync(string code)
    {
        const string sql = @"
            SELECT Id, Code, NameEn, NameFr, DescriptionEn, DescriptionFr, Category,
                   SchemaJson, IsActive, Version, DateCreated, DateUpdated, CreatedBy, UpdatedBy
            FROM CodeSets
            WHERE Code = @Code AND IsActive = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var result = await connection.QueryFirstOrDefaultAsync<CodeSetEntity>(sql, new { Code = code });

            if (result == null)
            {
                _logger.LogDebug("CodeSet with code '{Code}' not found", code);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CodeSet by code '{Code}'", code);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CodeSetSummary>> GetAllAsync()
    {
        const string sql = @"
            SELECT
                Id, Code, NameEn, NameFr, DescriptionEn, Category,
                Version, IsActive, DateCreated, DateUpdated,
                JSON_VALUE(SchemaJson, '$.items') as ItemsJson
            FROM CodeSets
            WHERE IsActive = 1
            ORDER BY Category, NameEn";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var results = await connection.QueryAsync<dynamic>(sql);

            var summaries = new List<CodeSetSummary>();
            foreach (var row in results)
            {
                int itemCount = 0;
                if (!string.IsNullOrEmpty((string?)row.ItemsJson))
                {
                    try
                    {
                        var items = JsonSerializer.Deserialize<JsonElement[]>((string)row.ItemsJson);
                        itemCount = items?.Length ?? 0;
                    }
                    catch
                    {
                        // If parsing fails, just use 0
                    }
                }

                summaries.Add(new CodeSetSummary
                {
                    Id = (int)row.Id,
                    Code = (string)row.Code,
                    NameEn = (string)row.NameEn,
                    NameFr = (string?)row.NameFr,
                    DescriptionEn = (string?)row.DescriptionEn,
                    Category = (string?)row.Category,
                    Version = (float)(double)row.Version,
                    IsActive = (bool)row.IsActive,
                    DateCreated = (DateTime)row.DateCreated,
                    DateUpdated = (DateTime)row.DateUpdated,
                    ItemCount = itemCount
                });
            }

            _logger.LogDebug("Retrieved {Count} CodeSet summaries", summaries.Count);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all CodeSets");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CodeSetSummary>> GetByCategoryAsync(string category)
    {
        const string sql = @"
            SELECT
                Id, Code, NameEn, NameFr, DescriptionEn, Category,
                Version, IsActive, DateCreated, DateUpdated,
                JSON_VALUE(SchemaJson, '$.items') as ItemsJson
            FROM CodeSets
            WHERE IsActive = 1 AND Category = @Category
            ORDER BY NameEn";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var results = await connection.QueryAsync<dynamic>(sql, new { Category = category });

            var summaries = new List<CodeSetSummary>();
            foreach (var row in results)
            {
                int itemCount = 0;
                if (!string.IsNullOrEmpty((string?)row.ItemsJson))
                {
                    try
                    {
                        var items = JsonSerializer.Deserialize<JsonElement[]>((string)row.ItemsJson);
                        itemCount = items?.Length ?? 0;
                    }
                    catch { }
                }

                summaries.Add(new CodeSetSummary
                {
                    Id = (int)row.Id,
                    Code = (string)row.Code,
                    NameEn = (string)row.NameEn,
                    NameFr = (string?)row.NameFr,
                    DescriptionEn = (string?)row.DescriptionEn,
                    Category = (string?)row.Category,
                    Version = (float)(double)row.Version,
                    IsActive = (bool)row.IsActive,
                    DateCreated = (DateTime)row.DateCreated,
                    DateUpdated = (DateTime)row.DateUpdated,
                    ItemCount = itemCount
                });
            }

            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CodeSets by category '{Category}'", category);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            UPDATE CodeSets
            SET IsActive = 0, DateUpdated = GETUTCDATE()
            WHERE Id = @Id AND IsActive = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

            _logger.LogInformation("Soft-deleted CodeSet {Id}, rows affected: {RowsAffected}", id, rowsAffected);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CodeSet {Id}", id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByCodeAsync(string code)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS(
                SELECT 1 FROM CodeSets
                WHERE Code = @Code AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.ExecuteScalarAsync<bool>(sql, new { Code = code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of CodeSet with code '{Code}'", code);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetNextIdAsync()
    {
        const string sql = @"SELECT ISNULL(MAX(Id), 0) + 1 FROM CodeSets";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.ExecuteScalarAsync<int>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next CodeSet ID");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CodeSetSummary>> SearchAsync(string searchTerm)
    {
        const string sql = @"
            SELECT
                Id, Code, NameEn, NameFr, DescriptionEn, Category,
                Version, IsActive, DateCreated, DateUpdated,
                JSON_VALUE(SchemaJson, '$.items') as ItemsJson
            FROM CodeSets
            WHERE IsActive = 1
              AND (Code LIKE @Search
                   OR NameEn LIKE @Search
                   OR NameFr LIKE @Search
                   OR DescriptionEn LIKE @Search
                   OR Category LIKE @Search)
            ORDER BY Category, NameEn";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var searchPattern = $"%{searchTerm}%";
            var results = await connection.QueryAsync<dynamic>(sql, new { Search = searchPattern });

            var summaries = new List<CodeSetSummary>();
            foreach (var row in results)
            {
                int itemCount = 0;
                if (!string.IsNullOrEmpty((string?)row.ItemsJson))
                {
                    try
                    {
                        var items = JsonSerializer.Deserialize<JsonElement[]>((string)row.ItemsJson);
                        itemCount = items?.Length ?? 0;
                    }
                    catch { }
                }

                summaries.Add(new CodeSetSummary
                {
                    Id = (int)row.Id,
                    Code = (string)row.Code,
                    NameEn = (string)row.NameEn,
                    NameFr = (string?)row.NameFr,
                    DescriptionEn = (string?)row.DescriptionEn,
                    Category = (string?)row.Category,
                    Version = (float)(double)row.Version,
                    IsActive = (bool)row.IsActive,
                    DateCreated = (DateTime)row.DateCreated,
                    DateUpdated = (DateTime)row.DateUpdated,
                    ItemCount = itemCount
                });
            }

            _logger.LogDebug("Search for '{SearchTerm}' returned {Count} CodeSets", searchTerm, summaries.Count);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching CodeSets for '{SearchTerm}'", searchTerm);
            throw;
        }
    }
}
