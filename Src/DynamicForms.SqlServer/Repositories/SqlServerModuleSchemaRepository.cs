using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using DynamicForms.Core.V4.Schemas;
using DynamicForms.SqlServer.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DynamicForms.SqlServer.Repositories;

/// <summary>
/// SQL Server implementation of IModuleSchemaRepository.
/// Uses Dapper for high-performance database operations.
/// Stores FormModuleSchema as JSON in the ModuleSchemas table.
/// </summary>
public class SqlServerModuleSchemaRepository : IModuleSchemaRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerModuleSchemaRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqlServerModuleSchemaRepository(
        string connectionString,
        ILogger<SqlServerModuleSchemaRepository> logger)
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
    public async Task<bool> SaveAsync(FormModuleSchema schema, CancellationToken cancellationToken = default)
    {
        const string checkExistsSql = @"
            SELECT COUNT(1) FROM ModuleSchemas
            WHERE ModuleId = @ModuleId AND IsActive = 1";

        const string updateSql = @"
            UPDATE ModuleSchemas
            SET Version = @Version,
                SchemaJson = @SchemaJson,
                TitleEn = @TitleEn,
                TitleFr = @TitleFr,
                DescriptionEn = @DescriptionEn,
                DateUpdated = GETUTCDATE(),
                UpdatedBy = @UpdatedBy,
                IsCurrent = 1
            WHERE ModuleId = @ModuleId AND IsActive = 1 AND IsCurrent = 1";

        const string insertSql = @"
            INSERT INTO ModuleSchemas
                (ModuleId, Version, SchemaJson, TitleEn, TitleFr, DescriptionEn, IsActive, IsCurrent, DateCreated, DateUpdated, CreatedBy, UpdatedBy)
            VALUES
                (@ModuleId, @Version, @SchemaJson, @TitleEn, @TitleFr, @DescriptionEn, 1, 1, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @UpdatedBy)";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var schemaJson = JsonSerializer.Serialize(schema, _jsonOptions);

                var exists = await connection.ExecuteScalarAsync<int>(
                    checkExistsSql,
                    new { ModuleId = schema.Id },
                    transaction) > 0;

                var parameters = new
                {
                    ModuleId = schema.Id,
                    schema.Version,
                    SchemaJson = schemaJson,
                    schema.TitleEn,
                    schema.TitleFr,
                    schema.DescriptionEn,
                    schema.CreatedBy,
                    UpdatedBy = schema.CreatedBy
                };

                if (exists)
                {
                    await connection.ExecuteAsync(updateSql, parameters, transaction);
                    _logger.LogInformation("Updated module schema {ModuleId} v{Version}", schema.Id, schema.Version);
                }
                else
                {
                    await connection.ExecuteAsync(insertSql, parameters, transaction);
                    _logger.LogInformation("Created new module schema {ModuleId} v{Version}", schema.Id, schema.Version);
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
            _logger.LogError(ex, "Error saving module schema {ModuleId}", schema.Id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<FormModuleSchema?> GetByIdAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT SchemaJson
            FROM ModuleSchemas
            WHERE ModuleId = @ModuleId AND IsActive = 1 AND IsCurrent = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var schemaJson = await connection.QueryFirstOrDefaultAsync<string>(
                sql,
                new { ModuleId = moduleId });

            if (string.IsNullOrEmpty(schemaJson))
            {
                _logger.LogDebug("Module schema {ModuleId} not found", moduleId);
                return null;
            }

            var schema = JsonSerializer.Deserialize<FormModuleSchema>(schemaJson, _jsonOptions);
            _logger.LogDebug("Retrieved module schema {ModuleId} with {FieldCount} fields",
                moduleId, schema?.Fields.Length ?? 0);

            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module schema {ModuleId}", moduleId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModuleSchemaSummary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                ModuleId,
                TitleEn,
                TitleFr,
                DescriptionEn,
                Version,
                DateCreated,
                DateUpdated,
                CreatedBy,
                JSON_VALUE(SchemaJson, '$.fields') as FieldsJson
            FROM ModuleSchemas
            WHERE IsActive = 1 AND IsCurrent = 1
            ORDER BY DateUpdated DESC";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<dynamic>(sql);

            var summaries = new List<ModuleSchemaSummary>();
            foreach (var row in results)
            {
                int fieldCount = 0;
                if (!string.IsNullOrEmpty((string?)row.FieldsJson))
                {
                    try
                    {
                        var fields = JsonSerializer.Deserialize<JsonElement[]>((string)row.FieldsJson);
                        fieldCount = fields?.Length ?? 0;
                    }
                    catch
                    {
                        // If parsing fails, just use 0
                    }
                }

                summaries.Add(new ModuleSchemaSummary
                {
                    ModuleId = (int)row.ModuleId,
                    TitleEn = (string)row.TitleEn ?? $"Module {row.ModuleId}",
                    TitleFr = (string?)row.TitleFr,
                    DescriptionEn = (string?)row.DescriptionEn,
                    Version = (float)(double)row.Version,
                    DateCreated = (DateTime)row.DateCreated,
                    DateUpdated = (DateTime)row.DateUpdated,
                    CreatedBy = (string?)row.CreatedBy,
                    FieldCount = fieldCount
                });
            }

            _logger.LogDebug("Retrieved {Count} module schema summaries", summaries.Count);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all module schemas");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<FormModuleSchema[]> GetByIdsAsync(int[] moduleIds, CancellationToken cancellationToken = default)
    {
        if (moduleIds.Length == 0)
            return Array.Empty<FormModuleSchema>();

        const string sql = @"
            SELECT SchemaJson
            FROM ModuleSchemas
            WHERE ModuleId IN @ModuleIds AND IsActive = 1 AND IsCurrent = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var schemaJsons = await connection.QueryAsync<string>(
                sql,
                new { ModuleIds = moduleIds });

            var schemas = new List<FormModuleSchema>();
            foreach (var json in schemaJsons)
            {
                if (!string.IsNullOrEmpty(json))
                {
                    var schema = JsonSerializer.Deserialize<FormModuleSchema>(json, _jsonOptions);
                    if (schema != null)
                        schemas.Add(schema);
                }
            }

            _logger.LogDebug("Retrieved {Count} module schemas for {RequestedCount} IDs",
                schemas.Count, moduleIds.Length);

            return schemas.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module schemas by IDs");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE ModuleSchemas
            SET IsActive = 0, DateUpdated = GETUTCDATE()
            WHERE ModuleId = @ModuleId AND IsActive = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var rowsAffected = await connection.ExecuteAsync(sql, new { ModuleId = moduleId });

            _logger.LogInformation("Soft-deleted module schema {ModuleId}, rows affected: {RowsAffected}",
                moduleId, rowsAffected);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting module schema {ModuleId}", moduleId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS(
                SELECT 1 FROM ModuleSchemas
                WHERE ModuleId = @ModuleId AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            return await connection.ExecuteScalarAsync<bool>(sql, new { ModuleId = moduleId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of module schema {ModuleId}", moduleId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetNextModuleIdAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT ISNULL(MAX(ModuleId), 0) + 1 FROM ModuleSchemas";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            return await connection.ExecuteScalarAsync<int>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next module ID");
            throw;
        }
    }
}
