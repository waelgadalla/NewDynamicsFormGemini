using DynamicForms.Core.Entities;

namespace DynamicForms.Core.Entities.Data;

/// <summary>
/// Information about how a workflow is stored in the database
/// </summary>
public class WorkflowStorageInfo
{
    /// <summary>
    /// Workflow identifier
    /// </summary>
    public int WorkflowId { get; set; }

    /// <summary>
    /// Associated opportunity identifier
    /// </summary>
    public int? OpportunityId { get; set; }

    /// <summary>
    /// Current storage mode
    /// </summary>
    public WorkflowStorageMode CurrentStorageMode { get; set; }

    /// <summary>
    /// Size of the workflow in bytes
    /// </summary>
    public long SizeInBytes { get; set; }

    /// <summary>
    /// Whether the workflow is compressed
    /// </summary>
    public bool IsCompressed { get; set; }

    /// <summary>
    /// Number of database records used for storage
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// Compression ratio if compressed (original size / compressed size)
    /// </summary>
    public double? CompressionRatio { get; set; }

    /// <summary>
    /// Last time the workflow was accessed
    /// </summary>
    public DateTime? LastAccessed { get; set; }

    /// <summary>
    /// Performance metrics for this storage configuration
    /// </summary>
    public WorkflowStoragePerformance Performance { get; set; } = new();
}

/// <summary>
/// Performance metrics for workflow storage
/// </summary>
public class WorkflowStoragePerformance
{
    /// <summary>
    /// Average load time in milliseconds
    /// </summary>
    public double AverageLoadTimeMs { get; set; }

    /// <summary>
    /// Average save time in milliseconds
    /// </summary>
    public double AverageSaveTimeMs { get; set; }

    /// <summary>
    /// Number of times this workflow has been loaded
    /// </summary>
    public int LoadCount { get; set; }

    /// <summary>
    /// Number of times this workflow has been saved
    /// </summary>
    public int SaveCount { get; set; }

    /// <summary>
    /// Whether this storage mode is optimal for this workflow
    /// </summary>
    public bool IsOptimal { get; set; }

    /// <summary>
    /// Recommended storage mode based on usage patterns
    /// </summary>
    public WorkflowStorageMode RecommendedMode { get; set; }
}

/// <summary>
/// Criteria for searching workflows by complexity and characteristics
/// Enhanced with storage mode filtering
/// </summary>
public class WorkflowSearchCriteria
{
    /// <summary>
    /// Minimum number of modules
    /// </summary>
    public int? MinModuleCount { get; set; }

    /// <summary>
    /// Maximum number of modules
    /// </summary>
    public int? MaxModuleCount { get; set; }

    /// <summary>
    /// Minimum total field count
    /// </summary>
    public int? MinFieldCount { get; set; }

    /// <summary>
    /// Maximum total field count
    /// </summary>
    public int? MaxFieldCount { get; set; }

    /// <summary>
    /// Minimum complexity score
    /// </summary>
    public double? MinComplexity { get; set; }

    /// <summary>
    /// Maximum complexity score
    /// </summary>
    public double? MaxComplexity { get; set; }

    /// <summary>
    /// Filter by storage mode
    /// </summary>
    public WorkflowStorageMode? StorageMode { get; set; }

    /// <summary>
    /// Filter by compression status
    /// </summary>
    public bool? IsCompressed { get; set; }

    /// <summary>
    /// Minimum workflow size in bytes
    /// </summary>
    public long? MinSize { get; set; }

    /// <summary>
    /// Maximum workflow size in bytes
    /// </summary>
    public long? MaxSize { get; set; }

    /// <summary>
    /// Opportunity identifier filter
    /// </summary>
    public int? OpportunityId { get; set; }

    /// <summary>
    /// Created by filter
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Created after date filter
    /// </summary>
    public DateTime? CreatedAfter { get; set; }

    /// <summary>
    /// Created before date filter
    /// </summary>
    public DateTime? CreatedBefore { get; set; }

    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort results in descending order
    /// </summary>
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Search result for workflow queries
/// Enhanced with storage information
/// </summary>
public class WorkflowSearchResult
{
    /// <summary>
    /// Workflow identifier
    /// </summary>
    public int WorkflowId { get; set; }

    /// <summary>
    /// Associated opportunity identifier
    /// </summary>
    public int? OpportunityId { get; set; }

    /// <summary>
    /// Workflow title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Workflow description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Workflow version
    /// </summary>
    public float Version { get; set; }

    /// <summary>
    /// Number of modules
    /// </summary>
    public int ModuleCount { get; set; }

    /// <summary>
    /// Total number of fields
    /// </summary>
    public int TotalFields { get; set; }

    /// <summary>
    /// Complexity score
    /// </summary>
    public double ComplexityScore { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Created by
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Field type distribution
    /// </summary>
    public Dictionary<string, int> FieldTypeDistribution { get; set; } = new();

    /// <summary>
    /// Module type distribution
    /// </summary>
    public Dictionary<string, int> ModuleTypeDistribution { get; set; } = new();

    /// <summary>
    /// Storage information
    /// </summary>
    public WorkflowStorageInfo? StorageInfo { get; set; }
}

/// <summary>
/// Version information for workflows
/// Enhanced with storage details
/// </summary>
public class WorkflowVersionInfo
{
    /// <summary>
    /// Workflow identifier
    /// </summary>
    public int WorkflowId { get; set; }

    /// <summary>
    /// Associated opportunity identifier
    /// </summary>
    public int? OpportunityId { get; set; }

    /// <summary>
    /// Version number
    /// </summary>
    public float Version { get; set; }

    /// <summary>
    /// Version title/name
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Number of modules in this version
    /// </summary>
    public int ModuleCount { get; set; }

    /// <summary>
    /// Total fields in this version
    /// </summary>
    public int TotalFields { get; set; }

    /// <summary>
    /// Complexity score for this version
    /// </summary>
    public double ComplexityScore { get; set; }

    /// <summary>
    /// Creation date for this version
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Created by
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Whether this is the current version
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Storage mode for this version
    /// </summary>
    public WorkflowStorageMode StorageMode { get; set; }

    /// <summary>
    /// Size of this version in bytes
    /// </summary>
    public long SizeInBytes { get; set; }
}

/// <summary>
/// Options for exporting workflows
/// </summary>
public class WorkflowExportOptions
{
    /// <summary>
    /// Export format (JSON, XML, Excel, etc.)
    /// </summary>
    public WorkflowExportFormat Format { get; set; } = WorkflowExportFormat.Json;

    /// <summary>
    /// Include module data in export
    /// </summary>
    public bool IncludeModuleData { get; set; } = true;

    /// <summary>
    /// Include field hierarchy information
    /// </summary>
    public bool IncludeHierarchy { get; set; } = true;

    /// <summary>
    /// Include workflow statistics
    /// </summary>
    public bool IncludeStatistics { get; set; } = false;

    /// <summary>
    /// Compress exported data
    /// </summary>
    public bool CompressOutput { get; set; } = false;

    /// <summary>
    /// Include version history
    /// </summary>
    public bool IncludeVersionHistory { get; set; } = false;

    /// <summary>
    /// Export language (for multilingual content)
    /// </summary>
    public string Language { get; set; } = "EN";

    /// <summary>
    /// Custom export settings
    /// </summary>
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Export formats supported for workflows
/// </summary>
public enum WorkflowExportFormat
{
    /// <summary>
    /// JSON format
    /// </summary>
    Json,

    /// <summary>
    /// XML format
    /// </summary>
    Xml,

    /// <summary>
    /// Excel format
    /// </summary>
    Excel,

    /// <summary>
    /// CSV format (flattened)
    /// </summary>
    Csv,

    /// <summary>
    /// PDF documentation
    /// </summary>
    Pdf
}

/// <summary>
/// Result of workflow export operation
/// </summary>
public class WorkflowExportResult
{
    /// <summary>
    /// Whether the export was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Exported data
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// Content type/MIME type of exported data
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Suggested filename for the export
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Size of exported data in bytes
    /// </summary>
    public long SizeInBytes { get; set; }

    /// <summary>
    /// Export format used
    /// </summary>
    public WorkflowExportFormat Format { get; set; }

    /// <summary>
    /// Number of workflows exported
    /// </summary>
    public int WorkflowCount { get; set; }

    /// <summary>
    /// Export errors if any
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Export warnings if any
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Data for importing workflows
/// </summary>
public class WorkflowImportData
{
    /// <summary>
    /// Raw import data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Format of the import data
    /// </summary>
    public WorkflowExportFormat Format { get; set; } = WorkflowExportFormat.Json;

    /// <summary>
    /// Target opportunity ID for imported workflows
    /// </summary>
    public int? TargetOpportunityId { get; set; }

    /// <summary>
    /// Whether to overwrite existing workflows
    /// </summary>
    public bool OverwriteExisting { get; set; } = false;

    /// <summary>
    /// Whether to validate workflows before import
    /// </summary>
    public bool ValidateBeforeImport { get; set; } = true;

    /// <summary>
    /// Preferred storage mode for imported workflows
    /// </summary>
    public WorkflowStorageMode PreferredStorageMode { get; set; } = WorkflowStorageMode.Adaptive;

    /// <summary>
    /// Custom import settings
    /// </summary>
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Result of workflow import operation
/// </summary>
public class WorkflowImportResult
{
    /// <summary>
    /// Whether the import was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Number of workflows successfully imported
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// Number of workflows that failed to import
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// IDs of successfully imported workflows
    /// </summary>
    public List<int> ImportedWorkflowIds { get; set; } = new();

    /// <summary>
    /// Import errors if any
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Import warnings if any
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Detailed results for each workflow
    /// </summary>
    public List<WorkflowImportItemResult> ItemResults { get; set; } = new();
}

/// <summary>
/// Import result for individual workflow
/// </summary>
public class WorkflowImportItemResult
{
    /// <summary>
    /// Original workflow ID from import data
    /// </summary>
    public int? OriginalWorkflowId { get; set; }

    /// <summary>
    /// New workflow ID after import
    /// </summary>
    public int? NewWorkflowId { get; set; }

    /// <summary>
    /// Workflow title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Whether this item was successfully imported
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Storage mode used for this workflow
    /// </summary>
    public WorkflowStorageMode StorageMode { get; set; }

    /// <summary>
    /// Errors specific to this workflow
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Warnings specific to this workflow
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}