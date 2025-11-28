using DynamicForms.Core.Entities.Data;
using DynamicForms.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;
using Dapper;

namespace DynamicForms.SqlServer.Repositories;

/// <summary>
/// High-performance SQL Server implementation of IFormDataRepository
/// Uses Dapper for optimized SQL operations and bulk operations
/// </summary>
public class SqlServerFormDataRepository : IFormDataRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerFormDataRepository> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public SqlServerFormDataRepository(string connectionString, ILogger<SqlServerFormDataRepository> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _connectionString = connectionString;
        _logger = logger;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<int> SaveModuleDataAsync(int opportunityId, int moduleId, int applicationId, IEnumerable<FormDataItem> data, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string upsertSubmissionSql = @"
            MERGE FormSubmissions AS target
            USING (SELECT @ApplicationId AS ApplicationId) AS source
            ON target.ApplicationId = source.ApplicationId
            WHEN MATCHED THEN
                UPDATE SET DateUpdated = GETUTCDATE(), ModuleId = @ModuleId, OpportunityId = @OpportunityId
            WHEN NOT MATCHED THEN
                INSERT (ApplicationId, OpportunityId, ModuleId, Status, Language, DateCreated, DateUpdated)
                VALUES (@ApplicationId, @OpportunityId, @ModuleId, 'Draft', 'EN', GETUTCDATE(), GETUTCDATE());";

        const string deleteFieldDataSql = @"
            DELETE FROM FieldData WHERE ApplicationId = @ApplicationId";

        const string insertFieldDataSql = @"
            INSERT INTO FieldData (ApplicationId, FieldId, Value, JsonValue, DateCreated, DateUpdated, CreatedBy, UpdatedBy)
            VALUES (@ApplicationId, @FieldId, @Value, @JsonValue, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @UpdatedBy)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Upsert form submission
            await connection.ExecuteAsync(upsertSubmissionSql, new
            {
                ApplicationId = applicationId,
                OpportunityId = opportunityId,
                ModuleId = moduleId
            }, transaction);

            // Delete existing field data
            await connection.ExecuteAsync(deleteFieldDataSql, new { ApplicationId = applicationId }, transaction);

            // Bulk insert new field data
            var fieldDataList = data.Select(item => new
            {
                ApplicationId = applicationId,
                FieldId = item.Id,
                Value = item.Value,
                JsonValue = item.Values?.Any() == true || item.SpeciesAutoCompleteData != null || item.FileUploadData != null
                    ? JsonSerializer.Serialize(item)
                    : null,
                CreatedBy = item.CreatedBy,
                UpdatedBy = item.UpdatedBy
            }).ToList();

            if (fieldDataList.Any())
            {
                await connection.ExecuteAsync(insertFieldDataSql, fieldDataList, transaction);
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Successfully saved {FieldCount} fields for ApplicationId: {ApplicationId}, ModuleId: {ModuleId}", 
                data.Count(), applicationId, moduleId);

            return applicationId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error saving module data for ApplicationId: {ApplicationId}, ModuleId: {ModuleId}", 
                applicationId, moduleId);
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<int> SaveModalDataAsync(int opportunityId, int moduleId, int applicationId, string modalId, int recordId, IEnumerable<FormDataItem> data, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string deleteModalDataSql = @"
            DELETE FROM ModalData 
            WHERE ApplicationId = @ApplicationId AND ModalId = @ModalId AND RecordId = @RecordId";

        const string insertModalDataSql = @"
            INSERT INTO ModalData (ApplicationId, ModalId, RecordId, FormData, [Order], DateCreated, CreatedBy)
            VALUES (@ApplicationId, @ModalId, @RecordId, @FormData, 0, GETUTCDATE(), @CreatedBy)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Delete existing modal data for this record
            await connection.ExecuteAsync(deleteModalDataSql, new
            {
                ApplicationId = applicationId,
                ModalId = modalId,
                RecordId = recordId.ToString()
            }, transaction);

            // Insert new modal data
            var formData = JsonSerializer.Serialize(data);
            await connection.ExecuteAsync(insertModalDataSql, new
            {
                ApplicationId = applicationId,
                ModalId = modalId,
                RecordId = recordId.ToString(),
                FormData = formData,
                CreatedBy = data.FirstOrDefault()?.CreatedBy
            }, transaction);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Successfully saved modal data for ApplicationId: {ApplicationId}, ModalId: {ModalId}, RecordId: {RecordId}", 
                applicationId, modalId, recordId);

            return recordId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error saving modal data for ApplicationId: {ApplicationId}, ModalId: {ModalId}, RecordId: {RecordId}", 
                applicationId, modalId, recordId);
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ModuleFormData?> GetModuleDataAsync(int moduleId, int applicationId, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string submissionSql = @"
            SELECT ApplicationId, OpportunityId, ModuleId, Status, Language, IsComplete, IsValid, 
                   SchemaVersion, DateCreated, DateUpdated, CreatedBy, UpdatedBy
            FROM FormSubmissions 
            WHERE ApplicationId = @ApplicationId AND ModuleId = @ModuleId";

        const string fieldDataSql = @"
            SELECT FieldId, Value, JsonValue
            FROM FieldData 
            WHERE ApplicationId = @ApplicationId";

        const string modalDataSql = @"
            SELECT ModalId, RecordId, FormData, DateCreated, CreatedBy
            FROM ModalData 
            WHERE ApplicationId = @ApplicationId AND IsDeleted = 0";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get submission info
            var submission = await connection.QueryFirstOrDefaultAsync<dynamic>(submissionSql, new
            {
                ApplicationId = applicationId,
                ModuleId = moduleId
            });

            if (submission == null)
                return null;

            // Get field data
            var fieldData = await connection.QueryAsync<dynamic>(fieldDataSql, new { ApplicationId = applicationId });

            // Get modal data
            var modalData = await connection.QueryAsync<dynamic>(modalDataSql, new { ApplicationId = applicationId });

            // Convert to FormDataItems
            var dataItems = fieldData.Select(fd =>
            {
                FormDataItem item;
                if (!string.IsNullOrEmpty(fd.JsonValue))
                {
                    try
                    {
                        item = JsonSerializer.Deserialize<FormDataItem>(fd.JsonValue) ?? new FormDataItem();
                    }
                    catch
                    {
                        item = new FormDataItem
                        {
                            Id = fd.FieldId,
                            Value = fd.Value
                        };
                    }
                }
                else
                {
                    item = new FormDataItem
                    {
                        Id = fd.FieldId,
                        Value = fd.Value
                    };
                }
                return item;
            }).ToList();

            // Convert modal data
            var modals = modalData.GroupBy(md => (string)md.ModalId).Select(g => new ModalFormData
            {
                Id = g.Key,
                OpportunityId = (int)submission.OpportunityId,
                ModuleId = (int)submission.ModuleId,
                ModalId = g.Key,
                ApplicationId = applicationId.ToString(),
                DataItems = g.SelectMany(md =>
                {
                    try
                    {
                        return JsonSerializer.Deserialize<IEnumerable<FormDataItem>>((string)md.FormData) ?? Array.Empty<FormDataItem>();
                    }
                    catch
                    {
                        return Array.Empty<FormDataItem>();
                    }
                }).ToList(),
                DateCreated = g.First().DateCreated,
                CreatedBy = g.First().CreatedBy
            }).ToList();

            return new ModuleFormData
            {
                OpportunityId = (int)submission.OpportunityId,
                ModuleId = (int)submission.ModuleId,
                ApplicationId = applicationId,
                DataItems = dataItems,
                Modals = modals,
                Status = (string)submission.Status,
                Language = (string)submission.Language,
                IsComplete = (bool)submission.IsComplete,
                IsValid = (bool)submission.IsValid,
                SchemaVersion = (float)submission.SchemaVersion,
                DateCreated = (DateTime)submission.DateCreated,
                DateUpdated = (DateTime)submission.DateUpdated,
                CreatedBy = (string?)submission.CreatedBy,
                UpdatedBy = (string?)submission.UpdatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module data for ModuleId: {ModuleId}, ApplicationId: {ApplicationId}", 
                moduleId, applicationId);
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<IEnumerable<ModuleFormData>> GetDataForModulesAsync(int applicationId, int[] moduleIds, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        try
        {
            var results = new List<ModuleFormData>();

            // Use parallel execution for multiple modules
            var tasks = moduleIds.Select(moduleId => GetModuleDataAsync(moduleId, applicationId, cancellationToken));
            var moduleDataResults = await Task.WhenAll(tasks);

            foreach (var moduleData in moduleDataResults.Where(md => md != null))
            {
                results.Add(moduleData!);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data for multiple modules for ApplicationId: {ApplicationId}", applicationId);
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<IEnumerable<ModalFormData>> GetModalDataAsync(string modalId, int applicationId, int? recordId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string sql = @"
            SELECT Id, ModalId, RecordId, FormData, [Order], DateCreated, CreatedBy
            FROM ModalData 
            WHERE ApplicationId = @ApplicationId 
              AND ModalId = @ModalId 
              AND (@RecordId IS NULL OR RecordId = @RecordId)
              AND IsDeleted = 0
            ORDER BY [Order], DateCreated";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<dynamic>(sql, new
            {
                ApplicationId = applicationId,
                ModalId = modalId,
                RecordId = recordId?.ToString()
            });

            return results.Select(md => new ModalFormData
            {
                Id = ((int)md.Id).ToString(),
                OpportunityId = 0, // Would need additional query to get this
                ModuleId = 0,      // Would need additional query to get this
                ModalId = (string)md.ModalId,
                ApplicationId = applicationId.ToString(),
                DataItems = JsonSerializer.Deserialize<IEnumerable<FormDataItem>>((string)md.FormData) ?? Array.Empty<FormDataItem>(),
                Order = (int)md.Order,
                DateCreated = (DateTime)md.DateCreated,
                CreatedBy = (string?)md.CreatedBy
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving modal data for ModalId: {ModalId}, ApplicationId: {ApplicationId}", 
                modalId, applicationId);
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<bool> DeleteModalDataAsync(string modalId, int recordId, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string sql = @"
            UPDATE ModalData 
            SET IsDeleted = 1 
            WHERE ModalId = @ModalId AND RecordId = @RecordId";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                ModalId = modalId,
                RecordId = recordId.ToString()
            });

            _logger.LogInformation("Successfully soft-deleted {RowsAffected} modal data records for ModalId: {ModalId}, RecordId: {RecordId}", 
                rowsAffected, modalId, recordId);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting modal data for ModalId: {ModalId}, RecordId: {RecordId}", 
                modalId, recordId);
            return false;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<IEnumerable<ModuleDataSummary>> SearchAsync(SubmissionSearchCriteria criteria, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        // Build dynamic WHERE clause
        if (criteria.OpportunityId.HasValue)
        {
            whereConditions.Add("OpportunityId = @OpportunityId");
            parameters.Add("OpportunityId", criteria.OpportunityId.Value);
        }

        if (criteria.ModuleId.HasValue)
        {
            whereConditions.Add("ModuleId = @ModuleId");
            parameters.Add("ModuleId", criteria.ModuleId.Value);
        }

        if (!string.IsNullOrEmpty(criteria.Status))
        {
            whereConditions.Add("Status = @Status");
            parameters.Add("Status", criteria.Status);
        }

        if (criteria.DateFrom.HasValue)
        {
            whereConditions.Add("DateCreated >= @DateFrom");
            parameters.Add("DateFrom", criteria.DateFrom.Value);
        }

        if (criteria.DateTo.HasValue)
        {
            whereConditions.Add("DateCreated <= @DateTo");
            parameters.Add("DateTo", criteria.DateTo.Value);
        }

        if (!string.IsNullOrEmpty(criteria.CreatedBy))
        {
            whereConditions.Add("CreatedBy = @CreatedBy");
            parameters.Add("CreatedBy", criteria.CreatedBy);
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        // Build ORDER BY clause
        var orderBy = criteria.SortBy?.ToLower() switch
        {
            "datecreated" => criteria.SortDescending ? "ORDER BY DateCreated DESC" : "ORDER BY DateCreated ASC",
            "dateupdated" => criteria.SortDescending ? "ORDER BY DateUpdated DESC" : "ORDER BY DateUpdated ASC",
            "status" => criteria.SortDescending ? "ORDER BY Status DESC" : "ORDER BY Status ASC",
            _ => "ORDER BY DateUpdated DESC"
        };

        var sql = $@"
            SELECT OpportunityId, ModuleId, ApplicationId, Status, IsComplete, IsValid,
                   DateCreated, DateUpdated, CreatedBy, UpdatedBy,
                   0 AS FieldCount, 0 AS ModalCount
            FROM FormSubmissions 
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS 
            FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("Offset", (criteria.PageNumber - 1) * criteria.PageSize);
        parameters.Add("PageSize", criteria.PageSize);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<ModuleDataSummary>(sql, parameters);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching submissions with criteria: {@Criteria}", criteria);
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<SubmissionStatistics> GetStatisticsAsync(int? opportunityId = null, int? moduleId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string sql = @"
            SELECT 
                COUNT(*) AS TotalSubmissions,
                COUNT(CASE WHEN Status = 'Draft' THEN 1 END) AS DraftSubmissions,
                COUNT(CASE WHEN Status IN ('Completed', 'Submitted') THEN 1 END) AS CompletedSubmissions,
                COUNT(CASE WHEN Status = 'UnderReview' THEN 1 END) AS UnderReviewSubmissions,
                COUNT(CASE WHEN Status = 'Approved' THEN 1 END) AS ApprovedSubmissions,
                COUNT(CASE WHEN Status = 'Rejected' THEN 1 END) AS RejectedSubmissions,
                MAX(DateSubmitted) AS LastSubmissionDate
            FROM FormSubmissions
            WHERE (@OpportunityId IS NULL OR OpportunityId = @OpportunityId)
              AND (@ModuleId IS NULL OR ModuleId = @ModuleId)";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var stats = await connection.QueryFirstOrDefaultAsync<SubmissionStatistics>(sql, new
            {
                OpportunityId = opportunityId,
                ModuleId = moduleId
            });

            return stats ?? new SubmissionStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving submission statistics for OpportunityId: {OpportunityId}, ModuleId: {ModuleId}", 
                opportunityId, moduleId);
            throw;
        }
    }

    /// <summary>
    /// Bulk save multiple form submissions for high-performance scenarios
    /// </summary>
    public async Task<bool> BulkSaveSubmissionsAsync(IEnumerable<ModuleFormData> submissions, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var submission in submissions)
            {
                await SaveModuleDataAsync(submission.OpportunityId, submission.ModuleId, submission.ApplicationId, 
                    submission.DataItems, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Successfully bulk saved {Count} submissions", submissions.Count());
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error bulk saving submissions");
            return false;
        }
    }
}