using DynamicForms.Core.Entities;
using DynamicForms.Core.Entities.Data;

namespace DynamicForms.Core.Interfaces;

/// <summary>
/// Repository interface for managing FormWorkflow entities with multiple modules
/// Extends the existing single-module repository pattern to support workflow scenarios
/// Enhanced with single document storage capabilities
/// </summary>
public interface IFormWorkflowRepository
{
    #region Core Workflow Operations
    /// <summary>
    /// Get all workflows for listing purposes
    /// </summary>
    Task<IEnumerable<FormWorkflow>> GetAllWorkflowsAsync(int? opportunityId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a complete workflow with all modules and hierarchy built
    /// </summary>
    Task<FormWorkflow?> GetWorkflowAsync(int workflowId, int? opportunityId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get multiple workflows for an opportunity
    /// </summary>
    Task<IEnumerable<FormWorkflow>> GetWorkflowsAsync(int opportunityId, int[] workflowIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save a complete workflow with all modules
    /// </summary>
    Task<bool> SaveWorkflowAsync(FormWorkflow workflow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a workflow and all associated modules
    /// </summary>
    Task<bool> DeleteWorkflowAsync(int workflowId, int? opportunityId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a workflow exists
    /// </summary>
    Task<bool> WorkflowExistsAsync(int workflowId, int? opportunityId = null, CancellationToken cancellationToken = default);
    #endregion

    #region Advanced Workflow Operations
    /// <summary>
    /// Get workflow with automatically built hierarchy and navigation
    /// </summary>
    Task<FormWorkflow?> GetWorkflowWithNavigationAsync(int workflowId, int? opportunityId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save workflow with optimization (field ordering, relationship validation)
    /// </summary>
    Task<bool> SaveWorkflowWithOptimizationAsync(FormWorkflow workflow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow statistics and analytics
    /// </summary>
    Task<WorkflowStatistics?> GetWorkflowStatisticsAsync(int workflowId, int? opportunityId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search workflows by complexity and characteristics
    /// </summary>
    Task<IEnumerable<DynamicForms.Core.Entities.Data.WorkflowSearchResult>> SearchWorkflowsByComplexityAsync(DynamicForms.Core.Entities.Data.WorkflowSearchCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clone a workflow with optional module filtering
    /// </summary>
    Task<FormWorkflow?> CloneWorkflowAsync(int sourceWorkflowId, int? sourceOpportunityId, int newWorkflowId, int? newOpportunityId, Func<FormModule, bool>? moduleFilter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow versions with history
    /// </summary>
    Task<IEnumerable<DynamicForms.Core.Entities.Data.WorkflowVersionInfo>> GetWorkflowVersionsAsync(int workflowId, CancellationToken cancellationToken = default);
    #endregion

    #region Single Document Storage Operations
    /// <summary>
    /// Save workflow as a single optimized JSON document (new approach)
    /// Automatically determines whether to use compression based on size
    /// </summary>
    Task<bool> SaveWorkflowAsSingleDocumentAsync(FormWorkflow workflow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow using smart loading (single document or multi-document based on stored format)
    /// Handles both storage modes transparently
    /// </summary>
    Task<FormWorkflow?> GetWorkflowSmartAsync(int workflowId, int? opportunityId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow with specified storage mode preference
    /// Falls back to available storage if preferred mode is not available
    /// </summary>
    Task<FormWorkflow?> GetWorkflowWithStorageModeAsync(int workflowId, int? opportunityId = null, WorkflowStorageMode preferredMode = WorkflowStorageMode.Adaptive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Migrate workflow between storage modes (single document -> multi-document)
    /// </summary>
    Task<bool> MigrateWorkflowStorageModeAsync(int workflowId, int? opportunityId, WorkflowStorageMode targetMode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow storage information (current mode, size, compression status)
    /// </summary>
    Task<DynamicForms.Core.Entities.Data.WorkflowStorageInfo?> GetWorkflowStorageInfoAsync(int workflowId, int? opportunityId = null, CancellationToken cancellationToken = default);
    #endregion

    #region Module-within-Workflow Operations
    /// <summary>
    /// Add a module to an existing workflow
    /// </summary>
    Task<bool> AddModuleToWorkflowAsync(int workflowId, FormModule module, int? position = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a module from a workflow
    /// </summary>
    Task<bool> RemoveModuleFromWorkflowAsync(int workflowId, int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a specific module within a workflow
    /// </summary>
    Task<bool> UpdateModuleInWorkflowAsync(int workflowId, FormModule module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorder modules within a workflow
    /// </summary>
    Task<bool> ReorderWorkflowModulesAsync(int workflowId, int[] moduleIdOrder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific module from a workflow without loading the entire workflow
    /// </summary>
    Task<FormModule?> GetWorkflowModuleAsync(int workflowId, int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);
    #endregion

    #region Bulk Operations
    /// <summary>
    /// Save multiple workflows in a single transaction
    /// </summary>
    Task<bool> BulkSaveWorkflowsAsync(IEnumerable<FormWorkflow> workflows, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export workflows to various formats
    /// </summary>
    Task<DynamicForms.Core.Entities.Data.WorkflowExportResult> ExportWorkflowsAsync(int[] workflowIds, DynamicForms.Core.Entities.Data.WorkflowExportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import workflows from external sources
    /// </summary>
    Task<DynamicForms.Core.Entities.Data.WorkflowImportResult> ImportWorkflowsAsync(DynamicForms.Core.Entities.Data.WorkflowImportData importData, CancellationToken cancellationToken = default);
    #endregion
}

#region Supporting Models for Workflow Repository

/// <summary>
/// Search criteria for finding workflows by characteristics
/// </summary>
public class WorkflowSearchCriteria
{
    // Database-level filters (applied in SQL)
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime? CreatedAfter { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime? CreatedBefore { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CreatedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? OpportunityId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    // Application-level filters (applied after retrieval)
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MinModuleCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MaxModuleCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MinFieldCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MaxFieldCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double? MinComplexity { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double? MaxComplexity { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string[]? RequiredFieldTypes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    // Sorting and pagination
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string SortBy { get; set; } = "DateCreated";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool SortDescending { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int PageNumber { get; set; } = 1;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int PageSize { get; set; } = 50;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Search result for workflow queries
/// </summary>
public class WorkflowSearchResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int WorkflowId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? OpportunityId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Title { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Description { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public float Version { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ModuleCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalFields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double ComplexityScore { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime DateCreated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CreatedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, int> FieldTypeDistribution { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, int> ModuleTypeDistribution { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Version information for workflow history
/// </summary>
public class WorkflowVersionInfo
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int WorkflowId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? OpportunityId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public float Version { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Title { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ModuleCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalFields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double ComplexityScore { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime DateCreated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CreatedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsCurrent { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ChangeDescription { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Export options for workflows
/// </summary>
public class WorkflowExportOptions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Format { get; set; } = "JSON"; // JSON, XML, Excel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IncludeData { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IncludeHistory { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string[]? ModuleFilter { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? OutputPath { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool CompressOutput { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Export result with file information
/// </summary>
public class WorkflowExportResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSuccessful { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FilePath { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public long FileSize { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ExportedWorkflowCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ExportedModuleCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, object> Metadata { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Import data container
/// </summary>
public class WorkflowImportData
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Format { get; set; } = "JSON";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string FilePath { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public byte[]? FileData { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool OverwriteExisting { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ValidateBeforeImport { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, object> ImportOptions { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Import result with statistics
/// </summary>
public class WorkflowImportResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSuccessful { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ImportedWorkflowCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ImportedModuleCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int SkippedWorkflowCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ErrorCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<int, int> WorkflowIdMapping { get; set; } = new(); // Old ID -> New ID
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#endregion