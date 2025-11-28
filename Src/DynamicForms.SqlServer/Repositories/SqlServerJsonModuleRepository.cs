using DynamicForms.Core.Entities;
using DynamicForms.Core.Entities.Data;
using DynamicForms.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;
using Dapper;

namespace DynamicForms.SqlServer.Repositories;

/// <summary>
/// High-performance SQL Server implementation of IFormModuleRepository
/// Uses Dapper for optimized SQL operations and leverages SQL Server JSON features
/// Works directly with FormModule without conversion layers
/// </summary>
public class SqlServerFormModuleRepository : IFormModuleRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerFormModuleRepository> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public SqlServerFormModuleRepository(string connectionString, ILogger<SqlServerFormModuleRepository> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    #region Core Methods (Direct operations)

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<FormModule?> GetEnhancedMetadataAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string sql = @"
            SELECT TOP 1 SchemaJson, Version, DateUpdated
            FROM ModuleSchemas 
            WHERE ModuleId = @ModuleId 
              AND (@OpportunityId IS NULL OR OpportunityId = @OpportunityId)
              AND IsActive = 1 
              AND IsCurrent = 1
            ORDER BY Version DESC";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var result = await connection.QueryFirstOrDefaultAsync<(string SchemaJson, float Version, DateTime DateUpdated)>(
                sql, 
                new { ModuleId = moduleId, OpportunityId = opportunityId });

            if (result.SchemaJson == null)
                return null;

            // Deserialize directly as FormModule
            var formModule = JsonSerializer.Deserialize<FormModule>(result.SchemaJson);
            if (formModule == null)
                return null;

            // Ensure all nested objects are initialized (constructors aren't called during deserialization)
            formModule.EnsureInitialized();

            formModule.Version = result.Version;
            formModule.DateUpdated = result.DateUpdated.ToString("yyyy-MM-dd HH:mm:ss");

            _logger.LogDebug("Retrieved module {ModuleId} with {FieldCount} fields",
                moduleId, formModule.Fields.Length);

            return formModule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module metadata for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}", 
                moduleId, opportunityId);
            throw;
        }
    }

    /// <summary>
    /// Get all modules for listing purposes
    /// </summary>
    public async Task<IEnumerable<ModuleSearchResult>> GetAllModulesAsync(int? opportunityId = null, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT ModuleId, OpportunityId, Version, SchemaJson, DateCreated, DateUpdated, CreatedBy, Description
            FROM ModuleSchemas 
            WHERE IsActive = 1 AND IsCurrent = 1";

        if (opportunityId.HasValue)
        {
            sql += " AND OpportunityId = @OpportunityId";
        }

        sql += " ORDER BY CASE WHEN DateUpdated > DateCreated THEN DateUpdated ELSE DateCreated END DESC";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<(int ModuleId, int? OpportunityId, float Version, string SchemaJson, DateTime DateCreated, DateTime DateUpdated, string? CreatedBy, string? Description)>(
                sql, 
                new { OpportunityId = opportunityId });

            var searchResults = new List<ModuleSearchResult>();

            foreach (var result in results)
            {
                try
                {
                    var formModule = JsonSerializer.Deserialize<FormModule>(result.SchemaJson);
                    if (formModule != null)
                    {
                        // Ensure all nested objects are initialized
                        formModule.EnsureInitialized();
                        formModule.RebuildFieldHierarchy();
                        var stats = formModule.GetModuleStatistics();

                        searchResults.Add(new ModuleSearchResult
                        {
                            ModuleId = result.ModuleId,
                            OpportunityId = result.OpportunityId,
                            Title = formModule.Text?.Title?.EN ?? $"Module {result.ModuleId}",
                            Description = formModule.Text?.Description?.EN ?? string.Empty,
                            Version = result.Version,
                            DateCreated = result.DateCreated,
                            CreatedBy = result.CreatedBy,
                            Statistics = stats
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error deserializing module {ModuleId} for list", result.ModuleId);
                    
                    // Add basic info even if deserialization fails
                    searchResults.Add(new ModuleSearchResult
                    {
                        ModuleId = result.ModuleId,
                        OpportunityId = result.OpportunityId,
                        Title = $"Module {result.ModuleId}",
                        Description = "Error loading module details",
                        Version = result.Version,
                        DateCreated = result.DateCreated,
                        CreatedBy = result.CreatedBy,
                        Statistics = new ModuleStatistics()
                    });
                }
            }

            _logger.LogInformation("Retrieved {Count} modules for listing", searchResults.Count);
            return searchResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all modules");
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<IEnumerable<FormModule>> GetEnhancedMetadataForModulesAsync(int opportunityId, int[] moduleIds, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string sql = @"
            SELECT SchemaJson, Version, DateUpdated, ModuleId
            FROM ModuleSchemas 
            WHERE ModuleId IN @ModuleIds
              AND OpportunityId = @OpportunityId
              AND IsActive = 1 
              AND IsCurrent = 1
            ORDER BY ModuleId";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<(string SchemaJson, float Version, DateTime DateUpdated, int ModuleId)>(
                sql, 
                new { ModuleIds = moduleIds, OpportunityId = opportunityId });

            var modules = new List<FormModule>();
            foreach (var result in results)
            {
                try
                {
                    // Deserialize directly as FormModule
                    var formModule = JsonSerializer.Deserialize<FormModule>(result.SchemaJson);
                    if (formModule != null)
                    {
                        // Ensure all nested objects are initialized
                        formModule.EnsureInitialized();
                        formModule.Id = result.ModuleId;
                        formModule.Version = result.Version;
                        formModule.DateUpdated = result.DateUpdated.ToString("yyyy-MM-dd HH:mm:ss");
                        modules.Add(formModule);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize module schema for ModuleId: {ModuleId}", result.ModuleId);
                }
            }

            _logger.LogDebug("Retrieved {ModuleCount} modules for OpportunityId: {OpportunityId}", 
                modules.Count, opportunityId);

            return modules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving multiple module metadata for OpportunityId: {OpportunityId}", opportunityId);
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<bool> SaveEnhancedModuleAsync(FormModule module, int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string checkExistsSql = @"
            SELECT COUNT(1)
            FROM ModuleSchemas
            WHERE ModuleId = @ModuleId
              AND (@OpportunityId IS NULL OR OpportunityId = @OpportunityId)";

        const string updateSql = @"
            UPDATE ModuleSchemas
            SET Version = @Version,
                SchemaJson = @SchemaJson,
                IsActive = 1,
                IsCurrent = 1,
                DateUpdated = GETUTCDATE(),
                UpdatedBy = @UpdatedBy,
                Description = @Description
            WHERE ModuleId = @ModuleId
              AND (@OpportunityId IS NULL OR OpportunityId = @OpportunityId)";

        const string insertSql = @"
            INSERT INTO ModuleSchemas (ModuleId, OpportunityId, Version, SchemaJson, IsActive, IsCurrent, DateCreated, DateUpdated, CreatedBy, UpdatedBy, Description)
            VALUES (@ModuleId, @OpportunityId, @Version, @SchemaJson, 1, 1, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @UpdatedBy, @Description)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Serialize FormModule directly
            var schemaJson = JsonSerializer.Serialize(module, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            // Check if schema already exists
            var exists = await connection.ExecuteScalarAsync<int>(checkExistsSql,
                new { ModuleId = moduleId, OpportunityId = opportunityId },
                transaction) > 0;

            if (exists)
            {
                // Update existing schema
                await connection.ExecuteAsync(updateSql, new
                {
                    ModuleId = moduleId,
                    OpportunityId = opportunityId,
                    Version = module.Version,
                    SchemaJson = schemaJson,
                    UpdatedBy = (object?)null,
                    Description = module.Text?.Description?.EN
                }, transaction);

                _logger.LogInformation("Updated existing module schema for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}",
                    moduleId, opportunityId);
            }
            else
            {
                // Insert new schema
                await connection.ExecuteAsync(insertSql, new
                {
                    ModuleId = moduleId,
                    OpportunityId = opportunityId,
                    Version = module.Version,
                    SchemaJson = schemaJson,
                    CreatedBy = (object?)null,
                    UpdatedBy = (object?)null,
                    Description = module.Text?.Description?.EN
                }, transaction);

                _logger.LogInformation("Created new module schema for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}",
                    moduleId, opportunityId);
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Successfully saved module schema for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}, Version: {Version}",
                moduleId, opportunityId, module.Version);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error saving module metadata for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}",
                moduleId, opportunityId);
            return false;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<bool> DeleteEnhancedModuleAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string sql = @"
            UPDATE ModuleSchemas 
            SET IsActive = 0, DateUpdated = GETUTCDATE()
            WHERE ModuleId = @ModuleId 
              AND (@OpportunityId IS NULL OR OpportunityId = @OpportunityId)";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var rowsAffected = await connection.ExecuteAsync(sql, 
                new { ModuleId = moduleId, OpportunityId = opportunityId });

            _logger.LogInformation("Successfully soft-deleted {RowsAffected} module schema(s) for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}",
                rowsAffected, moduleId, opportunityId);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting module metadata for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}", 
                moduleId, opportunityId);
            return false;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<bool> EnhancedExistsAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS(
                SELECT 1 FROM ModuleSchemas 
                WHERE ModuleId = @ModuleId 
                  AND (@OpportunityId IS NULL OR OpportunityId = @OpportunityId)
                  AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var exists = await connection.QuerySingleAsync<bool>(sql, 
                new { ModuleId = moduleId, OpportunityId = opportunityId });

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking module existence for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}", 
                moduleId, opportunityId);
            return false;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<IEnumerable<ModuleVersionInfo>> GetEnhancedModuleVersionsAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        const string sql = @"
            SELECT ModuleId, OpportunityId, Version, SchemaJson, DateCreated, CreatedBy, Description, IsCurrent
            FROM ModuleSchemas
            WHERE ModuleId = @ModuleId 
              AND (@OpportunityId IS NULL OR OpportunityId = @OpportunityId)
              AND IsActive = 1
            ORDER BY Version DESC";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<(int ModuleId, int? OpportunityId, float Version, string SchemaJson, DateTime DateCreated, string? CreatedBy, string? Description, bool IsCurrent)>(
                sql, 
                new { ModuleId = moduleId, OpportunityId = opportunityId });

            var versions = new List<ModuleVersionInfo>();

            foreach (var result in results)
            {
                try
                {
                    // Deserialize directly as FormModule and get enhanced metrics
                    var formModule = JsonSerializer.Deserialize<FormModule>(result.SchemaJson);
                    if (formModule != null)
                    {
                        // Ensure all nested objects are initialized
                        formModule.EnsureInitialized();
                        formModule.RebuildFieldHierarchy();
                        var stats = formModule.GetModuleStatistics();

                        var versionInfo = new ModuleVersionInfo
                        {
                            ModuleId = result.ModuleId,
                            OpportunityId = result.OpportunityId,
                            Version = result.Version,
                            DateCreated = result.DateCreated,
                            CreatedBy = result.CreatedBy,
                            Description = result.Description,
                            IsCurrent = result.IsCurrent,
                            TotalFields = stats.TotalFields,
                            RootFields = stats.RootFields,
                            MaxDepth = stats.MaxDepth,
                            ComplexityScore = stats.ComplexityScore,
                            FieldTypes = formModule.Fields.Select(f => f.FieldType.Type).Distinct().ToArray(),
                            RelationshipTypes = formModule.Fields.Select(f => f.RelationshipType.ToString()).Distinct().ToArray()
                        };

                        versions.Add(versionInfo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process enhanced version info for ModuleId: {ModuleId}, Version: {Version}", 
                        result.ModuleId, result.Version);
                    
                    // Fallback to basic version info
                    versions.Add(new ModuleVersionInfo
                    {
                        ModuleId = result.ModuleId,
                        OpportunityId = result.OpportunityId,
                        Version = result.Version,
                        DateCreated = result.DateCreated,
                        CreatedBy = result.CreatedBy,
                        Description = result.Description,
                        IsCurrent = result.IsCurrent
                    });
                }
            }

            return versions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module versions for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}", 
                moduleId, opportunityId);
            throw;
        }
    }

    #endregion

    #region Enhanced Methods (Advanced functionality)

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<FormModule?> GetEnhancedMetadataWithHierarchyAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        try
        {
            var module = await GetEnhancedMetadataAsync(moduleId, opportunityId, cancellationToken);
            
            if (module != null)
            {
                // Automatically rebuild hierarchy
                module.RebuildFieldHierarchy();
                
                _logger.LogDebug("Retrieved and built hierarchy for module {ModuleId} - {TotalFields} fields, {MaxDepth} max depth", 
                    moduleId, module.Fields.Length, module.FieldHierarchy.MaxDepth);
            }

            return module;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module with hierarchy for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}", 
                moduleId, opportunityId);
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<bool> SaveEnhancedModuleWithOptimizationAsync(FormModule module, int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        try
        {
            // Perform optimizations before saving
            module.RebuildFieldHierarchy();

            // Optimize field ordering
            var rootFields = module.GetRootFields().ToList();
            foreach (var rootField in rootFields)
            {
                OptimizeFieldOrdering(rootField);
            }

            // Validate and fix relationships
            ValidateAndFixRelationships(module);

            // Save the optimized module
            var result = await SaveEnhancedModuleAsync(module, moduleId, opportunityId, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Successfully saved optimized module {ModuleId} with {FieldCount} fields", 
                    moduleId, module.Fields.Length);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving optimized module for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}", 
                moduleId, opportunityId);
            return false;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ModuleStatistics?> GetModuleStatisticsAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        try
        {
            var module = await GetEnhancedMetadataWithHierarchyAsync(moduleId, opportunityId, cancellationToken);
            return module?.GetModuleStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module statistics for ModuleId: {ModuleId}, OpportunityId: {OpportunityId}", 
                moduleId, opportunityId);
            return null;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<IEnumerable<ModuleSearchResult>> SearchModulesByComplexityAsync(ModuleSearchCriteria criteria, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        // Build base SQL query with filters that can be applied at database level
        var whereConditions = new List<string> { "IsActive = 1", "IsCurrent = 1" };
        var parameters = new DynamicParameters();

        if (criteria.CreatedAfter.HasValue)
        {
            whereConditions.Add("DateCreated >= @CreatedAfter");
            parameters.Add("CreatedAfter", criteria.CreatedAfter.Value);
        }

        if (criteria.CreatedBefore.HasValue)
        {
            whereConditions.Add("DateCreated <= @CreatedBefore");
            parameters.Add("CreatedBefore", criteria.CreatedBefore.Value);
        }

        if (!string.IsNullOrEmpty(criteria.CreatedBy))
        {
            whereConditions.Add("CreatedBy = @CreatedBy");
            parameters.Add("CreatedBy", criteria.CreatedBy);
        }

        var sql = $@"
            SELECT ModuleId, OpportunityId, Version, SchemaJson, DateCreated, CreatedBy, Description
            FROM ModuleSchemas 
            WHERE {string.Join(" AND ", whereConditions)}
            ORDER BY DateCreated DESC";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<(int ModuleId, int? OpportunityId, float Version, string SchemaJson, DateTime DateCreated, string? CreatedBy, string? Description)>(
                sql, parameters);

            var searchResults = new List<ModuleSearchResult>();

            foreach (var result in results)
            {
                try
                {
                    var formModule = JsonSerializer.Deserialize<FormModule>(result.SchemaJson);
                    if (formModule == null) continue;

                    // Ensure all nested objects are initialized
                    formModule.EnsureInitialized();
                    formModule.RebuildFieldHierarchy();
                    var stats = formModule.GetModuleStatistics();

                    // Apply complexity filters
                    if (!PassesComplexityFilter(stats, criteria, formModule))
                        continue;

                    searchResults.Add(new ModuleSearchResult
                    {
                        ModuleId = result.ModuleId,
                        OpportunityId = result.OpportunityId,
                        Title = formModule.Text.Title?.EN ?? "Untitled",
                        Description = formModule.Text.Description?.EN ?? "",
                        Version = result.Version,
                        DateCreated = result.DateCreated,
                        CreatedBy = result.CreatedBy,
                        Statistics = stats
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process module search result for ModuleId: {ModuleId}", result.ModuleId);
                }
            }

            // Apply sorting and pagination
            searchResults = ApplySortingAndPagination(searchResults, criteria);

            _logger.LogDebug("Search completed. Found {ResultCount} modules matching criteria", searchResults.Count);

            return searchResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching modules by complexity");
            throw;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<FormModule?> CloneModuleAsync(int sourceModuleId, int? sourceOpportunityId, int newModuleId, int? newOpportunityId, Func<FormField, bool>? fieldFilter = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        try
        {
            var sourceModule = await GetEnhancedMetadataWithHierarchyAsync(sourceModuleId, sourceOpportunityId, cancellationToken);
            if (sourceModule == null)
            {
                _logger.LogWarning("Source module {SourceModuleId} not found for cloning", sourceModuleId);
                return null;
            }

            // Create cloned module
            var clonedModule = new FormModule
            {
                Id = newModuleId,
                OpportunityId = newOpportunityId,
                Version = 1.0f,
                DateGenerated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Clone text resources
            clonedModule.Text.Title.EN = sourceModule.Text.Title?.EN + " (Clone)";
            clonedModule.Text.Title.FR = sourceModule.Text.Title?.FR + " (Clone)";
            clonedModule.Text.Description.EN = sourceModule.Text.Description?.EN;
            clonedModule.Text.Description.FR = sourceModule.Text.Description?.FR;

            // Clone fields with optional filtering
            var fieldsToClone = sourceModule.Fields.AsEnumerable();
            if (fieldFilter != null)
            {
                fieldsToClone = fieldsToClone.Where(fieldFilter);
            }

            var clonedFields = new List<FormField>();
            foreach (var field in fieldsToClone)
            {
                var clonedField = CloneField(field, newModuleId, newOpportunityId);
                clonedFields.Add(clonedField);
            }

            clonedModule.Fields = clonedFields.ToArray();
            clonedModule.RebuildFieldHierarchy();

            // Save the cloned module
            var saveResult = await SaveEnhancedModuleAsync(clonedModule, newModuleId, newOpportunityId, cancellationToken);
            if (saveResult)
            {
                _logger.LogInformation("Successfully cloned module {SourceModuleId} to {NewModuleId} with {FieldCount} fields", 
                    sourceModuleId, newModuleId, clonedFields.Count);
                return clonedModule;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning module {SourceModuleId} to {NewModuleId}", sourceModuleId, newModuleId);
            return null;
        }
    }

    #endregion

    #region High-Performance Bulk Operations

    /// <summary>
    /// Bulk save multiple modules for performance
    /// Uses SQL Server Table-Valued Parameters for maximum efficiency
    /// </summary>
    public async Task<bool> BulkSaveEnhancedModulesAsync(IEnumerable<(FormModule Module, int ModuleId, int? OpportunityId)> modules, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            MERGE ModuleSchemas AS target
            USING @ModuleData AS source
            ON target.ModuleId = source.ModuleId AND target.OpportunityId = source.OpportunityId
            WHEN MATCHED THEN
                UPDATE SET IsCurrent = 0, DateUpdated = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT (ModuleId, OpportunityId, Version, SchemaJson, IsActive, IsCurrent, DateCreated, DateUpdated)
                VALUES (source.ModuleId, source.OpportunityId, source.Version, source.SchemaJson, 1, 1, GETUTCDATE(), GETUTCDATE());";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var dataTable = new DataTable();
            dataTable.Columns.Add("ModuleId", typeof(int));
            dataTable.Columns.Add("OpportunityId", typeof(int));
            dataTable.Columns.Add("Version", typeof(float));
            dataTable.Columns.Add("SchemaJson", typeof(string));

            foreach (var (module, moduleId, opportunityId) in modules)
            {
                // Serialize FormModule directly
                var schemaJson = JsonSerializer.Serialize(module);
                dataTable.Rows.Add(moduleId, opportunityId, module.Version, schemaJson);
            }

            var parameter = new SqlParameter("@ModuleData", SqlDbType.Structured)
            {
                TypeName = "ModuleDataTableType",
                Value = dataTable
            };

            await connection.ExecuteAsync(sql, new { ModuleData = parameter });

            _logger.LogInformation("Successfully bulk saved {Count} module schemas", modules.Count());

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk saving module schemas");
            return false;
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Clone a field for module cloning
    /// </summary>
    private FormField CloneField(FormField sourceField, int? newModuleId, int? newOpportunityId)
    {
        var clonedField = new FormField
        {
            Id = $"{sourceField.Id}_clone_{Guid.NewGuid():N}",
            ModuleId = newModuleId,
            OpportunityId = newOpportunityId,
            Order = sourceField.Order,
            Version = sourceField.Version,
            ParentId = sourceField.ParentId,
            FieldType = sourceField.FieldType,
            IsActive = sourceField.IsActive,
            IsVisible = sourceField.IsVisible,
            IsVisibleInEditor = sourceField.IsVisibleInEditor,
            IsVisibleInDisplay = sourceField.IsVisibleInDisplay,
            ReadOnly = sourceField.ReadOnly,
            IsRequired = sourceField.IsRequired,
            IsRequiredInParent = sourceField.IsRequiredInParent,
            MaximumLength = sourceField.MaximumLength,
            MinimumLength = sourceField.MinimumLength,
            IsConditionallyRequired = sourceField.IsConditionallyRequired,
            ValidatingFields = sourceField.ValidatingFields,
            ConditionalRules = sourceField.ConditionalRules,
            CodeSetId = sourceField.CodeSetId,
            Options = sourceField.Options,
            SpeciesAutoCompleteFields = sourceField.SpeciesAutoCompleteFields,
            WidthClass = sourceField.WidthClass,
            CssClasses = sourceField.CssClasses,
            InlineStyles = sourceField.InlineStyles,
            Modal = sourceField.Modal,
            FileUpload = sourceField.FileUpload,
            Database = sourceField.Database,
            CustomProperties = sourceField.CustomProperties,
            RelationshipType = sourceField.RelationshipType
        };

        // Copy text resources
        clonedField.Text.Description.EN = sourceField.Text.Description?.EN;
        clonedField.Text.Description.FR = sourceField.Text.Description?.FR;
        clonedField.Text.Help.EN = sourceField.Text.Help?.EN;
        clonedField.Text.Help.FR = sourceField.Text.Help?.FR;
        clonedField.Text.Placeholder.EN = sourceField.Text.Placeholder?.EN;
        clonedField.Text.Placeholder.FR = sourceField.Text.Placeholder?.FR;
        clonedField.Text.Label.EN = sourceField.Text.Label?.EN;
        clonedField.Text.Label.FR = sourceField.Text.Label?.FR;

        return clonedField;
    }

    /// <summary>
    /// Optimize field ordering within a parent
    /// </summary>
    private void OptimizeFieldOrdering(FormField parentField)
    {
        if (!parentField.ChildFields.Any())
            return;

        // Sort children by order, then by field type priority
        var sortedChildren = parentField.ChildFields
            .OrderBy(f => f.Order ?? int.MaxValue)
            .ThenBy(f => GetFieldTypePriority(f.FieldType.Type))
            .ToList();

        parentField.ChildFields.Clear();
        foreach (var child in sortedChildren)
        {
            parentField.ChildFields.Add(child);
            OptimizeFieldOrdering(child); // Recursive optimization
        }
    }

    /// <summary>
    /// Get priority for field type ordering (lower = higher priority)
    /// </summary>
    private int GetFieldTypePriority(string fieldType)
    {
        return fieldType?.ToLowerInvariant() switch
        {
            "section" => 1,
            "header" => 2,
            "textbox" => 3,
            "textarea" => 4,
            "dropdown" => 5,
            "checkbox" => 6,
            "radio" => 7,
            "datepicker" => 8,
            "fileupload" => 9,
            _ => 10
        };
    }

    /// <summary>
    /// Validate and fix relationships in the module
    /// </summary>
    private void ValidateAndFixRelationships(FormModule module)
    {
        var fieldDict = module.Fields.ToDictionary(f => f.Id, f => f);

        foreach (var field in module.Fields)
        {
            // Fix invalid parent references
            if (!string.IsNullOrEmpty(field.ParentId) && !fieldDict.ContainsKey(field.ParentId))
            {
                _logger.LogWarning("Field {FieldId} has invalid parent reference {ParentId}, clearing", field.Id, field.ParentId);
                field.ParentId = null;
            }
        }
    }

    /// <summary>
    /// Check if module statistics pass the complexity filter
    /// </summary>
    private bool PassesComplexityFilter(ModuleStatistics stats, ModuleSearchCriteria criteria, FormModule formModule)
    {
        if (criteria.MinFieldCount.HasValue && stats.TotalFields < criteria.MinFieldCount.Value)
            return false;

        if (criteria.MaxFieldCount.HasValue && stats.TotalFields > criteria.MaxFieldCount.Value)
            return false;

        if (criteria.MinDepth.HasValue && stats.MaxDepth < criteria.MinDepth.Value)
            return false;

        if (criteria.MaxDepth.HasValue && stats.MaxDepth > criteria.MaxDepth.Value)
            return false;

        if (criteria.MinComplexity.HasValue && stats.ComplexityScore < criteria.MinComplexity.Value)
            return false;

        if (criteria.MaxComplexity.HasValue && stats.ComplexityScore > criteria.MaxComplexity.Value)
            return false;

        if (criteria.FieldTypes?.Length > 0)
        {
            var hasRequiredFieldType = criteria.FieldTypes.Any(type => 
                formModule.Fields.Any(f => f.FieldType.Type.Equals(type, StringComparison.OrdinalIgnoreCase)));
            if (!hasRequiredFieldType)
                return false;
        }

        if (criteria.RelationshipTypes?.Length > 0)
        {
            var hasRequiredRelationshipType = criteria.RelationshipTypes.Any(type => 
                formModule.Fields.Any(f => f.RelationshipType.ToString().Equals(type, StringComparison.OrdinalIgnoreCase)));
            if (!hasRequiredRelationshipType)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Apply sorting and pagination to search results
    /// </summary>
    private List<ModuleSearchResult> ApplySortingAndPagination(List<ModuleSearchResult> results, ModuleSearchCriteria criteria)
    {
        // Apply sorting
        var sortedResults = criteria.SortBy?.ToLowerInvariant() switch
        {
            "title" => criteria.SortDescending
                ? results.OrderByDescending(r => r.Title)
                : results.OrderBy(r => r.Title),
            "datecreated" => criteria.SortDescending
                ? results.OrderByDescending(r => r.DateCreated)
                : results.OrderBy(r => r.DateCreated),
            "fieldcount" => criteria.SortDescending
                ? results.OrderByDescending(r => r.Statistics.TotalFields)
                : results.OrderBy(r => r.Statistics.TotalFields),
            "complexity" or "complexityscore" => criteria.SortDescending
                ? results.OrderByDescending(r => r.Statistics.ComplexityScore)
                : results.OrderBy(r => r.Statistics.ComplexityScore),
            "maxdepth" => criteria.SortDescending
                ? results.OrderByDescending(r => r.Statistics.MaxDepth)
                : results.OrderBy(r => r.Statistics.MaxDepth),
            _ => criteria.SortDescending
                ? results.OrderByDescending(r => r.DateCreated)
                : results.OrderBy(r => r.DateCreated)
        };

        // Apply pagination
        var skip = (criteria.PageNumber - 1) * criteria.PageSize;
        return sortedResults.Skip(skip).Take(criteria.PageSize).ToList();
    }

    #endregion
}