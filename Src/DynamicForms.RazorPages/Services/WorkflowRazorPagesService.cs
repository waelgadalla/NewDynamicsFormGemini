using DynamicForms.Core.Entities;
using DynamicForms.Core.Interfaces;
using Microsoft.Extensions.Logging;
using DynamicForms.Core.Entities.Data;

namespace DynamicForms.RazorPages.Services;

/// <summary>
/// Razor Pages service for working with multi-module workflows
/// Enhanced with single document storage support for optimal performance
/// </summary>
public class WorkflowRazorPagesService
{
    private readonly IFormWorkflowRepository _workflowRepository;
    private readonly RazorPagesService _moduleService;
    private readonly ILogger<WorkflowRazorPagesService> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public WorkflowRazorPagesService(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        IFormWorkflowRepository workflowRepository,
        RazorPagesService moduleService,
        ILogger<WorkflowRazorPagesService> logger)
    {
        _workflowRepository = workflowRepository;
        _moduleService = moduleService;
        _logger = logger;
    }

    #region Workflow Loading and Management

    /// <summary>
    /// Load a complete workflow for Razor Pages with built hierarchy and navigation
    /// Uses smart loading to automatically choose the best storage mode
    /// </summary>
    public async Task<FormWorkflow?> GetWorkflowAsync(int workflowId, int? opportunityId = null)
    {
        try
        {
            // Use smart loading for optimal performance
            var workflow = await _workflowRepository.GetWorkflowSmartAsync(workflowId, opportunityId);
            
            if (workflow == null)
            {
                _logger.LogWarning("Workflow {WorkflowId} not found", workflowId);
                return null;
            }

            _logger.LogDebug("Loaded workflow {WorkflowId} with {ModuleCount} modules and {TotalFields} total fields using {StorageMode} storage", 
                workflowId, workflow.ModuleCount, workflow.TotalFieldCount, 
                workflow.PreferSingleDocumentStorage ? "single document" : "multi-document");

            return workflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow {WorkflowId}", workflowId);
            throw;
        }
    }

    /// <summary>
    /// Load workflow with specified storage mode preference
    /// </summary>
    public async Task<FormWorkflow?> GetWorkflowWithStorageModeAsync(int workflowId, int? opportunityId = null, WorkflowStorageMode preferredMode = WorkflowStorageMode.Adaptive)
    {
        try
        {
            var workflow = await _workflowRepository.GetWorkflowWithStorageModeAsync(workflowId, opportunityId, preferredMode);
            
            if (workflow != null)
            {
                _logger.LogDebug("Loaded workflow {WorkflowId} with preferred storage mode {StorageMode}", 
                    workflowId, preferredMode);
            }

            return workflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow {WorkflowId} with storage mode {StorageMode}", workflowId, preferredMode);
            throw;
        }
    }

    /// <summary>
    /// Save a workflow back to the repository using optimal storage mode
    /// </summary>
    public async Task<bool> SaveWorkflowAsync(FormWorkflow workflow)
    {
        try
        {
            if (workflow.WorkflowId == null)
            {
                _logger.LogError("Cannot save workflow without ID");
                return false;
            }

            // The repository will automatically choose the optimal storage mode
            var result = await _workflowRepository.SaveWorkflowAsync(workflow);
            
            if (result)
            {
                var storageMode = workflow.PreferSingleDocumentStorage ? "single document" : "multi-document";
                _logger.LogInformation("Successfully saved workflow {WorkflowId} using {StorageMode} storage", 
                    workflow.WorkflowId, storageMode);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving workflow {WorkflowId}", workflow.WorkflowId);
            return false;
        }
    }

    /// <summary>
    /// Save workflow with specific storage mode
    /// </summary>
    public async Task<bool> SaveWorkflowWithStorageModeAsync(FormWorkflow workflow, WorkflowStorageMode storageMode)
    {
        try
        {
            if (workflow.WorkflowId == null)
            {
                _logger.LogError("Cannot save workflow without ID");
                return false;
            }

            bool result;
            if (storageMode == WorkflowStorageMode.SingleDocument)
            {
                result = await _workflowRepository.SaveWorkflowAsSingleDocumentAsync(workflow);
            }
            else
            {
                result = await _workflowRepository.SaveWorkflowWithOptimizationAsync(workflow);
            }
            
            if (result)
            {
                _logger.LogInformation("Successfully saved workflow {WorkflowId} using {StorageMode} storage", 
                    workflow.WorkflowId, storageMode);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving workflow {WorkflowId} with storage mode {StorageMode}", workflow.WorkflowId, storageMode);
            return false;
        }
    }

    /// <summary>
    /// Get workflow storage information for admin/debugging purposes
    /// </summary>
    public async Task<WorkflowStorageInfo?> GetWorkflowStorageInfoAsync(int workflowId, int? opportunityId = null)
    {
        try
        {
            var storageInfo = await _workflowRepository.GetWorkflowStorageInfoAsync(workflowId, opportunityId);
            
            if (storageInfo != null)
            {
                _logger.LogDebug("Workflow {WorkflowId} storage info: {StorageMode}, {SizeInBytes} bytes, {RecordCount} records, compressed: {IsCompressed}",
                    workflowId, storageInfo.CurrentStorageMode, storageInfo.SizeInBytes, storageInfo.RecordCount, storageInfo.IsCompressed);
            }

            return storageInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage info for workflow {WorkflowId}", workflowId);
            return null;
        }
    }

    /// <summary>
    /// Migrate workflow between storage modes
    /// </summary>
    public async Task<bool> MigrateWorkflowStorageModeAsync(int workflowId, int? opportunityId, WorkflowStorageMode targetMode)
    {
        try
        {
            var result = await _workflowRepository.MigrateWorkflowStorageModeAsync(workflowId, opportunityId, targetMode);
            
            if (result)
            {
                _logger.LogInformation("Successfully migrated workflow {WorkflowId} to {StorageMode} storage", 
                    workflowId, targetMode);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating workflow {WorkflowId} to {StorageMode} storage", workflowId, targetMode);
            return false;
        }
    }

    #endregion

    #region Multi-Module Form Rendering

    /// <summary>
    /// Get all modules in a workflow for rendering
    /// </summary>
    public IEnumerable<FormModule> GetWorkflowModules(FormWorkflow workflow)
    {
        return workflow.Modules.OrderBy(m => Array.IndexOf(workflow.Modules, m));
    }

    /// <summary>
    /// Get a specific module from a workflow
    /// </summary>
    public FormModule? GetWorkflowModule(FormWorkflow workflow, int moduleId)
    {
        return workflow.GetModule(moduleId);
    }

    /// <summary>
    /// Get modules for a specific step in the workflow
    /// </summary>
    public IEnumerable<FormModule> GetModulesForStep(FormWorkflow workflow, int stepNumber)
    {
        return workflow.GetModulesForStep(stepNumber);
    }

    /// <summary>
    /// Check if a module should be rendered in the current workflow context
    /// Enhanced with storage mode awareness
    /// </summary>
    public bool ShouldRenderModule(FormModule module, Dictionary<int, Dictionary<string, object>>? workflowData = null)
    {
        // Basic visibility check
        if (!module.Fields.Any(f => f.IsActive && f.IsVisible))
            return false;

        // Add workflow-specific visibility logic here
        // Could check dependencies on other modules, user permissions, etc.
        
        return true;
    }

    /// <summary>
    /// Get CSS classes for module rendering in workflow context
    /// </summary>
    public string GetModuleCssClasses(FormModule module, FormWorkflow workflow, int currentStep)
    {
        var classes = new List<string>();
        
        // Add base module classes
        classes.Add("workflow-module");
        classes.Add($"module-{module.Id}");
        
        // Add step-based classes
        var moduleStep = GetModuleStep(module, workflow);
        classes.Add($"workflow-step-{moduleStep}");
        
        if (moduleStep == currentStep)
        {
            classes.Add("current-step");
        }
        else if (moduleStep < currentStep)
        {
            classes.Add("completed-step");
        }
        else
        {
            classes.Add("future-step");
        }

        // Add storage mode indicator for debugging
        if (workflow.PreferSingleDocumentStorage)
        {
            classes.Add("single-doc-storage");
        }
        else
        {
            classes.Add("multi-doc-storage");
        }

        return string.Join(" ", classes.Where(c => !string.IsNullOrEmpty(c)));
    }

    #endregion

    #region Workflow Navigation

    /// <summary>
    /// Get the current step number for a module in the workflow
    /// </summary>
    public int GetModuleStep(FormModule module, FormWorkflow workflow)
    {
        var index = Array.IndexOf(workflow.Modules, module);
        return index >= 0 ? index + 1 : 1;
    }

    /// <summary>
    /// Get navigation information for workflow stepper
    /// </summary>
    public WorkflowNavigationInfo GetNavigationInfo(FormWorkflow workflow, int currentModuleId)
    {
        var currentModule = workflow.GetModule(currentModuleId);
        if (currentModule == null)
            throw new ArgumentException($"Module {currentModuleId} not found in workflow");

        var currentStep = GetModuleStep(currentModule, workflow);
        var totalSteps = workflow.ModuleCount;

        var navigationPath = workflow.Navigation.NavigationPaths.FirstOrDefault(p => p.ModuleId == currentModuleId);

        return new WorkflowNavigationInfo
        {
            CurrentStep = currentStep,
            TotalSteps = totalSteps,
            CurrentModuleId = currentModuleId,
            PreviousModuleId = navigationPath?.PreviousModuleId,
            NextModuleId = navigationPath?.NextModuleId,
            CanNavigatePrevious = navigationPath?.PreviousModuleId.HasValue ?? false,
            CanNavigateNext = navigationPath?.NextModuleId.HasValue ?? false,
            ProgressPercentage = (currentStep / (double)totalSteps) * 100.0
        };
    }

    /// <summary>
    /// Check if navigation to a specific module is allowed
    /// </summary>
    public bool CanNavigateToModule(FormWorkflow workflow, int targetModuleId, Dictionary<int, Dictionary<string, object>>? workflowData = null)
    {
        var navigationPath = workflow.Navigation.NavigationPaths.FirstOrDefault(p => p.ModuleId == targetModuleId);
        if (navigationPath == null) return false;

        // Check if step jumping is allowed
        if (!workflow.Navigation.AllowStepJumping) return false;

        // Check completion requirements for previous modules
        if (navigationPath.RequiredCompletionPercentage > 0 && workflowData != null)
        {
            var previousModules = workflow.Modules.Where(m => GetModuleStep(m, workflow) < navigationPath.StepNumber);
            foreach (var prevModule in previousModules)
            {
                var moduleData = workflowData.GetValueOrDefault(prevModule.Id ?? 0);
                var validation = prevModule.ValidateModuleEnhanced(moduleData);
                
                if (!validation.IsValid)
                    return false;
            }
        }

        return true;
    }

    #endregion

    #region Workflow Validation

    /// <summary>
    /// Validate workflow data using enhanced validation
    /// </summary>
    public WorkflowValidationResult ValidateWorkflowData(
        FormWorkflow workflow, 
        Dictionary<int, Dictionary<string, object>>? workflowData)
    {
        return workflow.ValidateWorkflow(workflowData);
    }

    /// <summary>
    /// Get validation errors for a specific module in workflow context
    /// </summary>
    public List<string> GetModuleValidationErrors(
        FormModule module, 
        Dictionary<string, object>? moduleData,
        FormWorkflow workflow,
        Dictionary<int, Dictionary<string, object>>? workflowData = null)
    {
        var validation = module.ValidateModuleEnhanced(moduleData);
        var errors = validation.FieldErrors.Select(e => e.ErrorMessage).ToList();
        errors.AddRange(validation.ModuleErrors);

        // Add workflow-context specific validation errors
        // Could check inter-module dependencies, workflow completion requirements, etc.

        return errors;
    }

    #endregion

    #region Workflow Management

    /// <summary>
    /// Add a module to an existing workflow
    /// </summary>
    public async Task<WorkflowUpdateResult> AddModuleToWorkflowAsync(FormWorkflow workflow, FormModule newModule, int? position = null)
    {
        var result = new WorkflowUpdateResult();

        try
        {
            workflow.AddModule(newModule, position);
            
            var saveSuccess = await _workflowRepository.SaveWorkflowAsync(workflow);
            
            if (saveSuccess)
            {
                result.IsSuccessful = true;
                result.ModulesAffected = 1;
                
                _logger.LogDebug("Added module {ModuleId} to workflow {WorkflowId}", newModule.Id, workflow.WorkflowId);
            }
            else
            {
                result.Errors.Add("Failed to save workflow after adding module");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding module {ModuleId} to workflow {WorkflowId}", newModule.Id, workflow.WorkflowId);
            result.Errors.Add($"Failed to add module: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Remove a module from a workflow
    /// </summary>
    public async Task<WorkflowUpdateResult> RemoveModuleFromWorkflowAsync(FormWorkflow workflow, int moduleId)
    {
        var result = new WorkflowUpdateResult();

        try
        {
            var module = workflow.GetModule(moduleId);
            if (module == null)
            {
                result.Errors.Add($"Module '{moduleId}' not found in workflow");
                return result;
            }

            var removeSuccess = workflow.RemoveModule(moduleId);
            
            if (removeSuccess)
            {
                var saveSuccess = await _workflowRepository.SaveWorkflowAsync(workflow);
                
                if (saveSuccess)
                {
                    result.IsSuccessful = true;
                    result.ModulesAffected = 1;
                    
                    _logger.LogDebug("Removed module {ModuleId} from workflow {WorkflowId}", moduleId, workflow.WorkflowId);
                }
                else
                {
                    result.Errors.Add("Failed to save workflow after removing module");
                }
            }
            else
            {
                result.Errors.Add($"Failed to remove module '{moduleId}' from workflow");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing module {ModuleId} from workflow {WorkflowId}", moduleId, workflow.WorkflowId);
            result.Errors.Add($"Failed to remove module: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Optimize workflow storage mode based on usage patterns and size
    /// </summary>
    public async Task<bool> OptimizeWorkflowStorageModeAsync(int workflowId, int? opportunityId = null)
    {
        try
        {
            var workflow = await GetWorkflowAsync(workflowId, opportunityId);
            if (workflow == null)
            {
                _logger.LogWarning("Cannot optimize storage for workflow {WorkflowId} - not found", workflowId);
                return false;
            }

            var currentStorageInfo = await GetWorkflowStorageInfoAsync(workflowId, opportunityId);
            var optimalMode = workflow.DetermineOptimalStorageMode();

            if (currentStorageInfo?.CurrentStorageMode != optimalMode)
            {
                _logger.LogInformation("Optimizing workflow {WorkflowId} storage from {CurrentMode} to {OptimalMode}", 
                    workflowId, currentStorageInfo?.CurrentStorageMode, optimalMode);
                
                return await MigrateWorkflowStorageModeAsync(workflowId, opportunityId, optimalMode);
            }

            _logger.LogDebug("Workflow {WorkflowId} storage is already optimal ({StorageMode})", workflowId, optimalMode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing storage mode for workflow {WorkflowId}", workflowId);
            return false;
        }
    }

    #endregion

    #region Analysis and Statistics

    /// <summary>
    /// Get workflow statistics for display
    /// Enhanced with storage information
    /// </summary>
    public async Task<WorkflowStatistics?> GetWorkflowStatisticsAsync(int workflowId, int? opportunityId = null)
    {
        try
        {
            var workflow = await GetWorkflowAsync(workflowId, opportunityId);
            var statistics = workflow?.GetWorkflowStatistics();
            
            if (statistics != null && workflow != null)
            {
                // Enhance statistics with storage information
                var storageInfo = await GetWorkflowStorageInfoAsync(workflowId, opportunityId);
                if (storageInfo != null)
                {
                    // Add storage-related metrics to custom settings
                    statistics.ModuleComplexityDistribution["StorageMode"] = (double)storageInfo.CurrentStorageMode;
                    statistics.ModuleComplexityDistribution["SizeInBytes"] = storageInfo.SizeInBytes;
                    statistics.ModuleComplexityDistribution["RecordCount"] = storageInfo.RecordCount;
                    statistics.ModuleComplexityDistribution["IsCompressed"] = storageInfo.IsCompressed ? 1.0 : 0.0;
                }
            }
            
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for workflow {WorkflowId}", workflowId);
            return null;
        }
    }

    /// <summary>
    /// Get workflow statistics for a loaded workflow
    /// </summary>
    public WorkflowStatistics GetWorkflowStatistics(FormWorkflow workflow)
    {
        return workflow.GetWorkflowStatistics();
    }

    /// <summary>
    /// Calculate completion percentage for the entire workflow
    /// </summary>
    public double GetWorkflowCompletionPercentage(FormWorkflow workflow, Dictionary<int, Dictionary<string, object>>? workflowData)
    {
        return workflow.GetCompletionPercentage(workflowData);
    }

    /// <summary>
    /// Get completion status for each module in workflow
    /// </summary>
    public Dictionary<int, ModuleCompletionStatus> GetModuleCompletionStatuses(
        FormWorkflow workflow, 
        Dictionary<int, Dictionary<string, object>>? workflowData)
    {
        var statuses = new Dictionary<int, ModuleCompletionStatus>();

        foreach (var module in workflow.Modules)
        {
            var moduleId = module.Id ?? 0;
            var moduleData = workflowData?.GetValueOrDefault(moduleId);
            var validation = module.ValidateModuleEnhanced(moduleData);

            var requiredFields = module.Fields.Count(f => f.IsRequired);
            var completedFields = 0;

            if (moduleData != null)
            {
                completedFields = module.Fields.Count(f => 
                    f.IsRequired && 
                    moduleData.ContainsKey(f.Id) && 
                    !string.IsNullOrWhiteSpace(moduleData[f.Id]?.ToString())
                );
            }

            statuses[moduleId] = new ModuleCompletionStatus
            {
                ModuleId = moduleId,
                ModuleName = module.Text.Title.EN ?? $"Module {moduleId}",
                IsValid = validation.IsValid,
                CompletionPercentage = requiredFields > 0 ? (completedFields / (double)requiredFields) * 100.0 : 100.0,
                ErrorCount = validation.FieldErrors.Count + validation.ModuleErrors.Count,
                RequiredFieldCount = requiredFields,
                CompletedFieldCount = completedFields
            };
        }

        return statuses;
    }

    /// <summary>
    /// Get storage recommendations for workflow optimization
    /// </summary>
    public async Task<WorkflowStorageRecommendation> GetStorageRecommendationAsync(int workflowId, int? opportunityId = null)
    {
        try
        {
            var workflow = await GetWorkflowAsync(workflowId, opportunityId);
            var storageInfo = await GetWorkflowStorageInfoAsync(workflowId, opportunityId);
            
            if (workflow == null || storageInfo == null)
            {
                return new WorkflowStorageRecommendation
                {
                    IsOptimal = false,
                    RecommendedMode = WorkflowStorageMode.Adaptive,
                    Reason = "Unable to analyze workflow"
                };
            }

            var optimalMode = workflow.DetermineOptimalStorageMode();
            var isOptimal = storageInfo.CurrentStorageMode == optimalMode;

            var recommendation = new WorkflowStorageRecommendation
            {
                WorkflowId = workflowId,
                CurrentMode = storageInfo.CurrentStorageMode,
                RecommendedMode = optimalMode,
                IsOptimal = isOptimal,
                CurrentSizeBytes = storageInfo.SizeInBytes,
                EstimatedOptimalSizeBytes = isOptimal ? storageInfo.SizeInBytes : workflow.EstimateSerializedSize(),
                PotentialSavingsBytes = isOptimal ? 0 : Math.Max(0, storageInfo.SizeInBytes - workflow.EstimateSerializedSize()),
                Reason = GetStorageRecommendationReason(workflow, storageInfo, optimalMode)
            };

            return recommendation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage recommendation for workflow {WorkflowId}", workflowId);
            return new WorkflowStorageRecommendation
            {
                IsOptimal = false,
                RecommendedMode = WorkflowStorageMode.Adaptive,
                Reason = $"Error analyzing workflow: {ex.Message}"
            };
        }
    }

    #endregion

    #region Private Helper Methods

    private string GetStorageRecommendationReason(FormWorkflow workflow, WorkflowStorageInfo storageInfo, WorkflowStorageMode optimalMode)
    {
        if (storageInfo.CurrentStorageMode == optimalMode)
        {
            return $"Current {storageInfo.CurrentStorageMode} storage is optimal for this workflow size and complexity.";
        }

        if (optimalMode == WorkflowStorageMode.SingleDocument)
        {
            return $"Single document storage would be more efficient due to small size ({workflow.ModuleCount} modules, {workflow.TotalFieldCount} fields).";
        }
        else
        {
            return $"Multi-document storage would be more efficient due to large size ({workflow.ModuleCount} modules, {workflow.TotalFieldCount} fields).";
        }
    }

    #endregion
}

#region Supporting Models for Enhanced Workflow Razor Pages Service

/// <summary>
/// Navigation information for workflow display
/// </summary>
public class WorkflowNavigationInfo
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int CurrentStep { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalSteps { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int CurrentModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? PreviousModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? NextModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool CanNavigatePrevious { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool CanNavigateNext { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double ProgressPercentage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Result of workflow update operations
/// </summary>
public class WorkflowUpdateResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSuccessful { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ModulesAffected { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Completion status for a module within a workflow
/// </summary>
public class ModuleCompletionStatus
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string ModuleName { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double CompletionPercentage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ErrorCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int RequiredFieldCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int CompletedFieldCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Storage mode recommendation for workflow optimization
/// </summary>
public class WorkflowStorageRecommendation
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int WorkflowId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public WorkflowStorageMode CurrentMode { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public WorkflowStorageMode RecommendedMode { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsOptimal { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public long CurrentSizeBytes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public long EstimatedOptimalSizeBytes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public long PotentialSavingsBytes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Reason { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double PotentialSavingsPercentage => CurrentSizeBytes > 0 ? (PotentialSavingsBytes / (double)CurrentSizeBytes) * 100.0 : 0.0;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#endregion