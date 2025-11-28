using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DynamicForms.Core.Entities;
using System.IO.Compression;
using System.Text;

namespace DynamicForms.Core.Entities;

/// <summary>
/// Container for multiple FormModules representing a complete workflow or application process
/// Supports multi-module forms, wizards, and complex business workflows
/// Enhanced with single document storage capabilities for improved performance
/// </summary>
public class FormWorkflow
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FormWorkflow()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Text = new TextResource();
        Settings = new WorkflowSettings();
        Modules = Array.Empty<FormModule>();
        Navigation = new NavigationConfig();
        DateGenerated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    #region Core Properties
    /// <summary>
    /// Unique workflow identifier
    /// </summary>
    public int? WorkflowId { get; set; }

    /// <summary>
    /// Associated opportunity/program identifier
    /// </summary>
    public int? OpportunityId { get; set; }

    /// <summary>
    /// Workflow schema version for backwards compatibility
    /// </summary>
    public float Version { get; set; } = 1.0f;

    /// <summary>
    /// UTC timestamp when the workflow schema was generated
    /// </summary>
    public string? DateGenerated { get; set; }

    /// <summary>
    /// UTC timestamp when the workflow schema was last updated
    /// </summary>
    public string? DateUpdated { get; set; }

    /// <summary>
    /// Multilingual text resources for the workflow
    /// </summary>
    public TextResource Text { get; set; }

    /// <summary>
    /// Collection of modules in this workflow
    /// </summary>
    public FormModule[] Modules { get; set; }

    /// <summary>
    /// Workflow-specific settings and configuration
    /// </summary>
    public WorkflowSettings Settings { get; set; }

    /// <summary>
    /// Navigation configuration between modules
    /// </summary>
    public NavigationConfig Navigation { get; set; }
    #endregion

    #region Enhanced Properties
    /// <summary>
    /// Workflow hierarchy information (calculated at runtime)
    /// </summary>
    [JsonIgnore]
    public WorkflowHierarchyInfo WorkflowHierarchy { get; private set; } = new();

    /// <summary>
    /// Total number of fields across all modules
    /// </summary>
    [JsonIgnore]
    public int TotalFieldCount => Modules.Sum(m => m.Fields.Length);

    /// <summary>
    /// Total number of modules in workflow
    /// </summary>
    [JsonIgnore]
    public int ModuleCount => Modules.Length;

    /// <summary>
    /// Estimated serialized size in bytes (calculated when needed)
    /// </summary>
    [JsonIgnore]
    public long EstimatedSize { get; private set; }

    /// <summary>
    /// Whether this workflow should use single document storage
    /// </summary>
    [JsonIgnore]
    public bool PreferSingleDocumentStorage => DetermineOptimalStorageMode() == WorkflowStorageMode.SingleDocument;
    #endregion

    #region Workflow Management
    /// <summary>
    /// Add a module to the workflow
    /// </summary>
    public void AddModule(FormModule module, int? position = null)
    {
        if (module == null) throw new ArgumentNullException(nameof(module));

        // Set workflow context
        module.OpportunityId = OpportunityId;

        var modulesList = Modules?.ToList() ?? new List<FormModule>();
        
        if (position.HasValue && position.Value >= 0 && position.Value < modulesList.Count)
        {
            modulesList.Insert(position.Value, module);
        }
        else
        {
            modulesList.Add(module);
        }

        Modules = modulesList.ToArray();
        RebuildWorkflowHierarchy();
    }

    /// <summary>
    /// Remove a module from the workflow
    /// </summary>
    public bool RemoveModule(int moduleId)
    {
        var modulesList = Modules?.ToList() ?? new List<FormModule>();
        var module = modulesList.FirstOrDefault(m => m.Id == moduleId);
        
        if (module == null) return false;

        modulesList.Remove(module);
        Modules = modulesList.ToArray();
        
        RebuildWorkflowHierarchy();
        return true;
    }

    /// <summary>
    /// Get a module by ID
    /// </summary>
    public FormModule? GetModule(int moduleId)
    {
        return Modules.FirstOrDefault(m => m.Id == moduleId);
    }

    /// <summary>
    /// Get modules for a specific step/stage
    /// </summary>
    public IEnumerable<FormModule> GetModulesForStep(int stepNumber)
    {
        return Modules.Where(m => GetModuleStep(m) == stepNumber);
    }

    /// <summary>
    /// Rebuild workflow hierarchy and navigation
    /// </summary>
    public void RebuildWorkflowHierarchy()
    {
        var result = new WorkflowHierarchyBuildResult();

        try
        {
            // Build module hierarchy for each module
            foreach (var module in Modules)
            {
                module.RebuildFieldHierarchy();
            }

            // Calculate workflow-level statistics
            WorkflowHierarchy.TotalModules = Modules.Length;
            WorkflowHierarchy.TotalFields = TotalFieldCount;
            WorkflowHierarchy.MaxModuleDepth = Modules.Any() ? Modules.Max(m => m.FieldHierarchy.MaxDepth) : 0;
            WorkflowHierarchy.ComplexityScore = CalculateWorkflowComplexity();
            
            // Build navigation paths
            BuildNavigationPaths();

            // Calculate estimated size for storage optimization
            EstimatedSize = EstimateSerializedSize();

            result.IsSuccessful = true;
            WorkflowHierarchy.BuildResult = result;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to rebuild workflow hierarchy: {ex.Message}");
            WorkflowHierarchy.BuildResult = result;
        }
    }

    /// <summary>
    /// Validate the entire workflow
    /// </summary>
    public WorkflowValidationResult ValidateWorkflow(Dictionary<int, Dictionary<string, object>>? workflowData = null)
    {
        var result = new WorkflowValidationResult();

        // Validate each module
        int index = 0;
        foreach (var module in Modules)
        {
            var moduleData = workflowData?.GetValueOrDefault(module.Id ?? 0);
            var moduleValidation = module.ValidateModuleEnhanced(moduleData);

            if (!moduleValidation.IsValid)
            {
                // Use module ID if available, otherwise use a unique negative index to avoid key collisions
                int key = module.Id ?? -(++index);

                // Only add if key doesn't already exist
                if (!result.ModuleValidationResults.ContainsKey(key))
                {
                    result.ModuleValidationResults[key] = moduleValidation;
                }
            }
        }

        // Workflow-level validations
        ValidateWorkflowLevelRules(workflowData, result);

        return result;
    }

    /// <summary>
    /// Get workflow statistics
    /// </summary>
    public WorkflowStatistics GetWorkflowStatistics()
    {
        var moduleStats = Modules.Select(m => m.GetModuleStatistics()).ToList();

        return new WorkflowStatistics
        {
            TotalModules = Modules.Length,
            TotalFields = TotalFieldCount,
            MaxDepth = moduleStats.Any() ? moduleStats.Max(s => s.MaxDepth) : 0,
            AverageFieldsPerModule = Modules.Any() ? TotalFieldCount / (double)Modules.Length : 0,
            ComplexityScore = WorkflowHierarchy.ComplexityScore,
            // Handle duplicate field counts by using GroupBy
            ModuleComplexityDistribution = moduleStats
                .GroupBy(s => s.TotalFields.ToString())
                .ToDictionary(g => g.Key, g => g.Average(s => s.ComplexityScore)),
            FieldTypeDistribution = moduleStats
                .SelectMany(s => s.FieldTypes)
                .GroupBy(ft => ft.Key)
                .ToDictionary(g => g.Key, g => g.Sum(ft => ft.Value))
        };
    }

    /// <summary>
    /// Calculate completion percentage across all modules
    /// </summary>
    public double GetCompletionPercentage(Dictionary<int, Dictionary<string, object>>? workflowData = null)
    {
        if (!Modules.Any()) return 100.0;

        var totalRequiredFields = 0;
        var completedFields = 0;

        foreach (var module in Modules)
        {
            var moduleData = workflowData?.GetValueOrDefault(module.Id ?? 0);
            var requiredFields = module.Fields.Where(f => f.IsRequired).ToList();
            totalRequiredFields += requiredFields.Count;

            if (moduleData != null)
            {
                completedFields += requiredFields.Count(f => 
                {
                    var value = moduleData.GetValueOrDefault(f.Id);
                    return value != null && !string.IsNullOrWhiteSpace(value.ToString());
                });
            }
        }

        return totalRequiredFields > 0 ? (completedFields / (double)totalRequiredFields) * 100.0 : 100.0;
    }
    #endregion

    #region Single Document Storage Optimization
    /// <summary>
    /// Determine the optimal storage mode for this workflow
    /// </summary>
    public WorkflowStorageMode DetermineOptimalStorageMode()
    {
        // Check if settings override storage mode
        if (Settings.StorageMode != WorkflowStorageMode.Adaptive)
            return Settings.StorageMode;

        var complexity = GetWorkflowStatistics();
        
        // Single document for simple workflows
        if (complexity.TotalFields <= Settings.SingleDocumentThreshold && 
            complexity.TotalModules <= Settings.SingleDocumentModuleThreshold)
            return WorkflowStorageMode.SingleDocument;
        
        // Multi-document for very complex workflows
        if (complexity.TotalFields > Settings.MultiDocumentThreshold || 
            complexity.TotalModules > Settings.MultiDocumentModuleThreshold)
            return WorkflowStorageMode.MultiDocument;
        
        // Evaluate serialized size
        var estimatedSize = EstimatedSize > 0 ? EstimatedSize : EstimateSerializedSize();
        
        return estimatedSize < Settings.SingleDocumentSizeThreshold ?
            WorkflowStorageMode.SingleDocument : 
            WorkflowStorageMode.MultiDocument;
    }

    /// <summary>
    /// Estimate the serialized size of this workflow
    /// </summary>
    public long EstimateSerializedSize()
    {
        try
        {
            // Quick estimation based on field count and content
            var baseSize = 1000; // Base workflow metadata
            var moduleSize = Modules.Sum(m => EstimateModuleSize(m));
            var navigationSize = Navigation.NavigationPaths.Length * 200; // Estimated navigation size
            
            EstimatedSize = baseSize + moduleSize + navigationSize;
            return EstimatedSize;
        }
        catch
        {
            // Fallback estimation
            EstimatedSize = TotalFieldCount * 500; // 500 bytes per field average
            return EstimatedSize;
        }
    }

    /// <summary>
    /// Create an optimized JSON representation for single document storage
    /// </summary>
    public string ToOptimizedJson(bool useCompression = false)
    {
        var options = GetOptimizedSerializationOptions();
        var formString = System.Text.Json.JsonSerializer.Serialize(this, options);
        
        if (useCompression && formString.Length > Settings.CompressionThreshold)
        {
            return CompressJson(formString);
        }
        
        return formString;
    }

    /// <summary>
    /// Create workflow from optimized JSON string
    /// </summary>
    public static FormWorkflow? FromOptimizedJson(string formData, bool isCompressed = false)
    {
        try
        {
            var actualJson = isCompressed ? DecompressJson(formData) : formData;
            var options = GetOptimizedSerializationOptions();
            
            var workflow = System.Text.Json.JsonSerializer.Deserialize<FormWorkflow>(actualJson, options);
            workflow?.RebuildWorkflowHierarchy();
            
            return workflow;
        }
        catch
        {
            return null;
        }
    }
    #endregion

    #region Private Helper Methods
    private int GetModuleStep(FormModule module)
    {
        // Default step calculation - can be overridden with custom logic
        var index = Array.IndexOf(Modules, module);
        return index + 1;
    }

    private double CalculateWorkflowComplexity()
    {
        var moduleComplexities = Modules.Select(m => m.GetModuleStatistics().ComplexityScore);
        var baseComplexity = moduleComplexities.Sum();
        var interModuleDependencies = CalculateInterModuleDependencies();
        
        return baseComplexity + (interModuleDependencies * 10.0); // Inter-module complexity multiplier
    }

    private int CalculateInterModuleDependencies()
    {
        // Calculate dependencies between modules (placeholder for future enhancement)
        // Could look for cross-module field references, conditional logic, etc.
        return 0;
    }

    private void BuildNavigationPaths()
    {
        // Build navigation configuration based on module order and dependencies
        var paths = new List<NavigationPath>();

        for (int i = 0; i < Modules.Length; i++)
        {
            var module = Modules[i];

            // Skip modules without valid IDs
            if (!module.Id.HasValue || module.Id.Value == 0)
            {
                continue;
            }

            var path = new NavigationPath
            {
                ModuleId = module.Id.Value,
                StepNumber = i + 1,
                PreviousModuleId = i > 0 ? Modules[i - 1].Id : null,
                NextModuleId = i < Modules.Length - 1 ? Modules[i + 1].Id : null,
                IsRequired = true, // Default - can be customized
                RequiredCompletionPercentage = 100.0 // Default - can be customized
            };

            paths.Add(path);
        }

        Navigation.NavigationPaths = paths.ToArray();
    }

    private void ValidateWorkflowLevelRules(Dictionary<int, Dictionary<string, object>>? workflowData, WorkflowValidationResult result)
    {
        // Workflow-level validation rules can be implemented here
        // Examples: Cross-module field dependencies, workflow completion requirements, etc.
        
        if (Settings.RequireAllModulesComplete)
        {
            var incompleteModules = Modules.Where(m => 
            {
                var moduleData = workflowData?.GetValueOrDefault(m.Id ?? 0);
                var moduleValidation = m.ValidateModuleEnhanced(moduleData);
                return !moduleValidation.IsValid;
            }).ToList();

            if (incompleteModules.Any())
            {
                result.WorkflowErrors.Add($"Workflow requires all modules to be complete. Incomplete modules: {string.Join(", ", incompleteModules.Select(m => m.Text.Title.EN))}");
            }
        }
    }

    private long EstimateModuleSize(FormModule module)
    {
        // Estimate size based on module content
        var baseModuleSize = 500; // Base module metadata
        var fieldSize = module.Fields.Sum(f => EstimateFieldSize(f));
        
        return baseModuleSize + fieldSize;
    }

    private long EstimateFieldSize(FormField field)
    {
        // Basic estimation: field metadata + text content + options
        var baseSize = 200; // Basic field properties
        var textSize = (field.Text.Description.EN?.Length ?? 0) + (field.Text.Description.FR?.Length ?? 0);
        var optionSize = (field.Options?.Length ?? 0) * 100; // Average option size
        
        return baseSize + textSize + optionSize;
    }

    private static System.Text.Json.JsonSerializerOptions GetOptimizedSerializationOptions()
    {
        return new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            // Add custom converters if needed for large arrays
        };
    }

    private string CompressJson(string formString)
    {
        var bytes = Encoding.UTF8.GetBytes(formString);
        using var memoryStream = new MemoryStream();
        using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
        {
            gzipStream.Write(bytes);
        }
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static string DecompressJson(string compressedData)
    {
        var compressedBytes = Convert.FromBase64String(compressedData);
        using var memoryStream = new MemoryStream(compressedBytes);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream);
        return reader.ReadToEnd();
    }
    #endregion

    #region Nested Classes
    /// <summary>
    /// Workflow settings and configuration with single document storage options
    /// </summary>
    public class WorkflowSettings
    {
        /// <summary>
        /// Whether all modules must be completed before workflow submission
        /// </summary>
        public bool RequireAllModulesComplete { get; set; } = true;

        /// <summary>
        /// Allow users to skip optional modules
        /// </summary>
        public bool AllowModuleSkipping { get; set; } = false;

        /// <summary>
        /// Auto-save interval in seconds (0 = disabled)
        /// </summary>
        public int AutoSaveIntervalSeconds { get; set; } = 300; // 5 minutes

        /// <summary>
        /// Whether to show progress across all modules
        /// </summary>
        public bool ShowOverallProgress { get; set; } = true;

        /// <summary>
        /// Storage mode for this workflow
        /// </summary>
        public WorkflowStorageMode StorageMode { get; set; } = WorkflowStorageMode.Adaptive;

        /// <summary>
        /// Maximum fields for single document storage (default: 100)
        /// </summary>
        public int SingleDocumentThreshold { get; set; } = 100;

        /// <summary>
        /// Maximum modules for single document storage (default: 5)
        /// </summary>
        public int SingleDocumentModuleThreshold { get; set; } = 5;

        /// <summary>
        /// Minimum fields to force multi-document storage (default: 500)
        /// </summary>
        public int MultiDocumentThreshold { get; set; } = 500;

        /// <summary>
        /// Minimum modules to force multi-document storage (default: 20)
        /// </summary>
        public int MultiDocumentModuleThreshold { get; set; } = 20;

        /// <summary>
        /// Maximum size in bytes for single document (default: 1MB)
        /// </summary>
        public long SingleDocumentSizeThreshold { get; set; } = 1_048_576; // 1MB

        /// <summary>
        /// Minimum size in bytes to enable compression (default: 50KB)
        /// </summary>
        public int CompressionThreshold { get; set; } = 51_200; // 50KB

        /// <summary>
        /// Custom workflow behavior settings
        /// </summary>
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }

    /// <summary>
    /// Navigation configuration for workflow
    /// </summary>
    public class NavigationConfig
    {
        /// <summary>
        /// Navigation paths between modules
        /// </summary>
        public NavigationPath[] NavigationPaths { get; set; } = Array.Empty<NavigationPath>();

        /// <summary>
        /// Whether to show step numbers
        /// </summary>
        public bool ShowStepNumbers { get; set; } = true;

        /// <summary>
        /// Whether steps are clickable (allow jumping)
        /// </summary>
        public bool AllowStepJumping { get; set; } = false;

        /// <summary>
        /// Custom navigation settings
        /// </summary>
        public Dictionary<string, object> CustomNavigationSettings { get; set; } = new();
    }

    /// <summary>
    /// Navigation path between modules
    /// </summary>
    public class NavigationPath
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int ModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int StepNumber { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int? PreviousModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int? NextModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool IsRequired { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public double RequiredCompletionPercentage { get; set; } = 100.0;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string[]? ConditionalRequirements { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Multilingual text resources for workflow metadata
    /// </summary>
    public class TextResource
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextResource()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Title = new TextClass();
            Description = new TextClass();
            Instructions = new TextClass();
            CompletionMessage = new TextClass();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Title { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Instructions { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass CompletionMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
    #endregion
}

#region Supporting Classes for FormWorkflow
/// <summary>
/// Storage mode options for FormWorkflow
/// </summary>
public enum WorkflowStorageMode
{
    /// <summary>
    /// Store as single JSON document (optimal for small-medium workflows)
    /// </summary>
    SingleDocument,

    /// <summary>
    /// Store as multiple documents (optimal for large/complex workflows)
    /// </summary>
    MultiDocument,

    /// <summary>
    /// System automatically chooses based on size and complexity
    /// </summary>
    Adaptive
}

/// <summary>
/// Information about workflow hierarchy structure
/// </summary>
public class WorkflowHierarchyInfo
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalModules { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalFields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int MaxModuleDepth { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double ComplexityScore { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public WorkflowHierarchyBuildResult BuildResult { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Result of building workflow hierarchy
/// </summary>
public class WorkflowHierarchyBuildResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSuccessful { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Workflow validation result with module-specific results
/// </summary>
public class WorkflowValidationResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<int, ModuleValidationResult> ModuleValidationResults { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> WorkflowErrors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid => !ModuleValidationResults.Any(mvr => !mvr.Value.IsValid) && !WorkflowErrors.Any();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalErrors => ModuleValidationResults.Sum(mvr => mvr.Value.FieldErrors.Count + mvr.Value.ModuleErrors.Count) + WorkflowErrors.Count;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Workflow statistics for analysis and reporting
/// </summary>
public class WorkflowStatistics
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalModules { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalFields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int MaxDepth { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double AverageFieldsPerModule { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double ComplexityScore { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, double> ModuleComplexityDistribution { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, int> FieldTypeDistribution { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
#endregion