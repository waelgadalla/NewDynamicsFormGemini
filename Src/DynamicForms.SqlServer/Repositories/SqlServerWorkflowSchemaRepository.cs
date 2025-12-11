using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using DynamicForms.Core.V4.Schemas;
using DynamicForms.SqlServer.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DynamicForms.SqlServer.Repositories;

/// <summary>
/// SQL Server implementation of IWorkflowSchemaRepository.
/// Uses Dapper for high-performance database operations.
/// Stores FormWorkflowSchema as JSON in the WorkflowSchemas table.
/// </summary>
public class SqlServerWorkflowSchemaRepository : IWorkflowSchemaRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerWorkflowSchemaRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqlServerWorkflowSchemaRepository(
        string connectionString,
        ILogger<SqlServerWorkflowSchemaRepository> logger)
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
    public async Task<bool> SaveAsync(FormWorkflowSchema schema, CancellationToken cancellationToken = default)
    {
        const string checkExistsSql = @"
            SELECT COUNT(1) FROM WorkflowSchemas
            WHERE WorkflowId = @WorkflowId AND IsActive = 1";

        const string updateSql = @"
            UPDATE WorkflowSchemas
            SET Version = @Version,
                SchemaJson = @SchemaJson,
                TitleEn = @TitleEn,
                TitleFr = @TitleFr,
                DescriptionEn = @DescriptionEn,
                DateUpdated = GETUTCDATE(),
                UpdatedBy = @UpdatedBy,
                IsCurrent = 1
            WHERE WorkflowId = @WorkflowId AND IsActive = 1 AND IsCurrent = 1";

        const string insertSql = @"
            INSERT INTO WorkflowSchemas
                (WorkflowId, Version, SchemaJson, TitleEn, TitleFr, DescriptionEn, IsActive, IsCurrent, DateCreated, DateUpdated, CreatedBy, UpdatedBy)
            VALUES
                (@WorkflowId, @Version, @SchemaJson, @TitleEn, @TitleFr, @DescriptionEn, 1, 1, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @UpdatedBy)";

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
                    new { WorkflowId = schema.Id },
                    transaction) > 0;

                var parameters = new
                {
                    WorkflowId = schema.Id,
                    schema.Version,
                    SchemaJson = schemaJson,
                    schema.TitleEn,
                    schema.TitleFr,
                    schema.DescriptionEn,
                    CreatedBy = (string?)null,
                    UpdatedBy = (string?)null
                };

                if (exists)
                {
                    await connection.ExecuteAsync(updateSql, parameters, transaction);
                    _logger.LogInformation("Updated workflow schema {WorkflowId} v{Version}", schema.Id, schema.Version);
                }
                else
                {
                    await connection.ExecuteAsync(insertSql, parameters, transaction);
                    _logger.LogInformation("Created new workflow schema {WorkflowId} v{Version}", schema.Id, schema.Version);
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
            _logger.LogError(ex, "Error saving workflow schema {WorkflowId}", schema.Id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<FormWorkflowSchema?> GetByIdAsync(int workflowId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT SchemaJson
            FROM WorkflowSchemas
            WHERE WorkflowId = @WorkflowId AND IsActive = 1 AND IsCurrent = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var schemaJson = await connection.QueryFirstOrDefaultAsync<string>(
                sql,
                new { WorkflowId = workflowId });

            if (string.IsNullOrEmpty(schemaJson))
            {
                _logger.LogDebug("Workflow schema {WorkflowId} not found", workflowId);
                return null;
            }

            var schema = JsonSerializer.Deserialize<FormWorkflowSchema>(schemaJson, _jsonOptions);
            _logger.LogDebug("Retrieved workflow schema {WorkflowId} with {ModuleCount} modules",
                workflowId, schema?.ModuleIds.Length ?? 0);

            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow schema {WorkflowId}", workflowId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkflowSchemaSummary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                WorkflowId,
                TitleEn,
                TitleFr,
                DescriptionEn,
                Version,
                DateCreated,
                DateUpdated,
                CreatedBy,
                JSON_VALUE(SchemaJson, '$.moduleIds') as ModuleIdsJson
            FROM WorkflowSchemas
            WHERE IsActive = 1 AND IsCurrent = 1
            ORDER BY DateUpdated DESC";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<dynamic>(sql);

            var summaries = new List<WorkflowSchemaSummary>();
            foreach (var row in results)
            {
                int moduleCount = 0;
                if (!string.IsNullOrEmpty((string?)row.ModuleIdsJson))
                {
                    try
                    {
                        var moduleIds = JsonSerializer.Deserialize<int[]>((string)row.ModuleIdsJson);
                        moduleCount = moduleIds?.Length ?? 0;
                    }
                    catch
                    {
                        // If parsing fails, just use 0
                    }
                }

                summaries.Add(new WorkflowSchemaSummary
                {
                    WorkflowId = (int)row.WorkflowId,
                    TitleEn = (string)row.TitleEn ?? $"Workflow {row.WorkflowId}",
                    TitleFr = (string?)row.TitleFr,
                    DescriptionEn = (string?)row.DescriptionEn,
                    Version = (float)(double)row.Version,
                    DateCreated = (DateTime)row.DateCreated,
                    DateUpdated = (DateTime)row.DateUpdated,
                    CreatedBy = (string?)row.CreatedBy,
                    ModuleCount = moduleCount
                });
            }

            _logger.LogDebug("Retrieved {Count} workflow schema summaries", summaries.Count);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all workflow schemas");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int workflowId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE WorkflowSchemas
            SET IsActive = 0, DateUpdated = GETUTCDATE()
            WHERE WorkflowId = @WorkflowId AND IsActive = 1";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var rowsAffected = await connection.ExecuteAsync(sql, new { WorkflowId = workflowId });

            _logger.LogInformation("Soft-deleted workflow schema {WorkflowId}, rows affected: {RowsAffected}",
                workflowId, rowsAffected);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow schema {WorkflowId}", workflowId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int workflowId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS(
                SELECT 1 FROM WorkflowSchemas
                WHERE WorkflowId = @WorkflowId AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            return await connection.ExecuteScalarAsync<bool>(sql, new { WorkflowId = workflowId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of workflow schema {WorkflowId}", workflowId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetNextWorkflowIdAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT ISNULL(MAX(WorkflowId), 0) + 1 FROM WorkflowSchemas";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            return await connection.ExecuteScalarAsync<int>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next workflow ID");
            throw;
        }
    }
}
