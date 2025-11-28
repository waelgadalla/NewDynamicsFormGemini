using DynamicForms.Core.Entities;
using DynamicForms.Core.Entities.Data;
using ValidationResult = DynamicForms.Core.Entities.ValidationResult;

namespace DynamicForms.Core.Interfaces;

/// <summary>
/// Service for managing form data submissions and persistence
/// </summary>
public interface IFormDataService
{
    /// <summary>
    /// Save module data (complete form submission)
    /// </summary>
    Task<int> SaveModuleDataAsync(int opportunityId, int moduleId, int applicationId, IEnumerable<FormDataItem> data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save draft module data (partial form submission)
    /// </summary>
    Task<int> SaveDraftAsync(int opportunityId, int moduleId, int applicationId, IEnumerable<FormDataItem> data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save modal data
    /// </summary>
    Task<int> SaveModalDataAsync(int opportunityId, int moduleId, int applicationId, string modalId, int recordId, IEnumerable<FormDataItem> data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get complete module data
    /// </summary>
    Task<ModuleFormData?> GetModuleDataAsync(int moduleId, int applicationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get data for multiple modules
    /// </summary>
    Task<IEnumerable<ModuleFormData>> GetDataForModulesAsync(int applicationId, int[] moduleIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get modal data
    /// </summary>
    Task<IEnumerable<ModalFormData>> GetModalDataAsync(string modalId, int applicationId, int? recordId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete modal data
    /// </summary>
    Task<bool> DeleteModalDataAsync(string modalId, int recordId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get submission status
    /// </summary>
    Task<string?> GetSubmissionStatusAsync(int applicationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update submission status
    /// </summary>
    Task<bool> UpdateSubmissionStatusAsync(int applicationId, string status, string? updatedBy = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search submissions by criteria
    /// </summary>
    Task<IEnumerable<ModuleDataSummary>> SearchSubmissionsAsync(SubmissionSearchCriteria criteria, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for form validation and business rules using  Enhanced architecture
/// </summary>
public interface IFormValidationService
{
    /// <summary>
    /// Validate entire module data
    /// </summary>
    Task<ValidationResult> ValidateModuleAsync(FormModule module, ModuleFormData data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate a single field
    /// </summary>
    Task<ValidationResult> ValidateFieldAsync(FormField field, FormDataItem data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate required fields in module
    /// </summary>
    Task<ValidationResult> ValidateRequiredFieldsAsync(FormModule module, ModuleFormData data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate conditional rules in module with hierarchy support
    /// </summary>
    Task<ValidationResult> ValidateConditionalRulesAsync(FormModule module, ModuleFormData data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate business rules with enhanced hierarchy awareness
    /// </summary>
    Task<ValidationResult> ValidateBusinessRulesAsync(FormModule module, ModuleFormData data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get validation rules for a field
    /// </summary>
    Task<IEnumerable<FieldValidationRule>> GetFieldValidationRulesAsync(FormField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate field hierarchy integrity
    /// </summary>
    Task<ValidationResult> ValidateHierarchyAsync(FormModule module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate parent-child relationships
    /// </summary>
    Task<ValidationResult> ValidateRelationshipsAsync(FormModule module, ModuleFormData data, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for rendering forms from JSON schemas using  Enhanced architecture
/// </summary>
public interface IFormRenderingService
{
    /// <summary>
    /// Render a single field
    /// </summary>
    Task<string> RenderFieldAsync(FormField field, FormDataItem? data = null, string language = "EN", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Render entire module with hierarchy support
    /// </summary>
    Task<string> RenderModuleAsync(FormModule module, ModuleFormData? data = null, string language = "EN", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Render modal form from field
    /// </summary>
    Task<string> RenderModalAsync(FormField modalField, IEnumerable<ModalRecord>? data = null, string language = "EN", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Render field for display (read-only) with hierarchy awareness
    /// </summary>
    Task<string> RenderDisplayFieldAsync(FormField field, FormDataItem? data = null, string language = "EN", CancellationToken cancellationToken = default);

    /// <summary>
    /// Render field hierarchy recursively
    /// </summary>
    Task<string> RenderFieldHierarchyAsync(FormField parentField, ModuleFormData? data = null, string language = "EN", int level = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Render only root fields of a module
    /// </summary>
    Task<string> RenderRootFieldsAsync(FormModule module, ModuleFormData? data = null, string language = "EN", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get available field templates
    /// </summary>
    Task<IEnumerable<string>> GetAvailableTemplatesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Register custom field renderer for fields
    /// </summary>
    void RegisterFieldRenderer(string fieldType, IFieldRenderer renderer);
}

/// <summary>
/// Service for managing form localization using  Enhanced architecture
/// </summary>
public interface IFormLocalizationService
{
    /// <summary>
    /// Get supported languages
    /// </summary>
    Task<IEnumerable<string>> GetSupportedLanguagesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get localized text
    /// </summary>
    Task<string> GetLocalizedTextAsync(string key, string language, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate module localization
    /// </summary>
    Task<LocalizationValidationResult> ValidateModuleLocalizationAsync(FormModule module, string[] requiredLanguages, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Export localization data from module
    /// </summary>
    Task<Dictionary<string, Dictionary<string, string>>> ExportLocalizationAsync(FormModule module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import localization data to module
    /// </summary>
    Task<bool> ImportLocalizationAsync(FormModule module, Dictionary<string, Dictionary<string, string>> localizationData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get missing translations for a module
    /// </summary>
    Task<Dictionary<string, List<string>>> GetMissingTranslationsAsync(FormModule module, string[] requiredLanguages, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for form schemas
/// This is the UNIFIED interface for all form schema operations in DynamicForms.
/// Uses FormModule exclusively for enhanced functionality with full hierarchy support.
/// Replaces all legacy repository interfaces for a clean, modern architecture.
/// </summary>
public interface IFormModuleRepository
{
    /// <summary>
    /// Get module metadata from storage
    /// </summary>
    Task<FormModule?> GetEnhancedMetadataAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all modules for listing
    /// </summary>
    Task<IEnumerable<ModuleSearchResult>> GetAllModulesAsync(int? opportunityId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get multiple modules metadata
    /// </summary>
    Task<IEnumerable<FormModule>> GetEnhancedMetadataForModulesAsync(int opportunityId, int[] moduleIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save module to storage
    /// </summary>
    Task<bool> SaveEnhancedModuleAsync(FormModule module, int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete module from storage
    /// </summary>
    Task<bool> DeleteEnhancedModuleAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if module exists
    /// </summary>
    Task<bool> EnhancedExistsAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get module versions
    /// </summary>
    Task<IEnumerable<ModuleVersionInfo>> GetEnhancedModuleVersionsAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get module with built hierarchy
    /// Automatically calls RebuildFieldHierarchy() on the returned module
    /// </summary>
    Task<FormModule?> GetEnhancedMetadataWithHierarchyAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save module with automatic hierarchy optimization
    /// Optimizes field ordering and validates relationships before saving
    /// </summary>
    Task<bool> SaveEnhancedModuleWithOptimizationAsync(FormModule module, int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get module statistics for reporting and analysis
    /// </summary>
    Task<ModuleStatistics?> GetModuleStatisticsAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search modules by hierarchy depth, complexity, or field count
    /// </summary>
    Task<IEnumerable<ModuleSearchResult>> SearchModulesByComplexityAsync(ModuleSearchCriteria criteria, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clone a module with optional field filtering
    /// Useful for creating module templates or variations
    /// </summary>
    Task<FormModule?> CloneModuleAsync(int sourceModuleId, int? sourceOpportunityId, int newModuleId, int? newOpportunityId, Func<FormField, bool>? fieldFilter = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for form data
/// </summary>
public interface IFormDataRepository
{
    /// <summary>
    /// Save module data to storage
    /// </summary>
    Task<int> SaveModuleDataAsync(int opportunityId, int moduleId, int applicationId, IEnumerable<FormDataItem> data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save modal data to storage
    /// </summary>
    Task<int> SaveModalDataAsync(int opportunityId, int moduleId, int applicationId, string modalId, int recordId, IEnumerable<FormDataItem> data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get module data from storage
    /// </summary>
    Task<ModuleFormData?> GetModuleDataAsync(int moduleId, int applicationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get data for multiple modules
    /// </summary>
    Task<IEnumerable<ModuleFormData>> GetDataForModulesAsync(int applicationId, int[] moduleIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get modal data from storage
    /// </summary>
    Task<IEnumerable<ModalFormData>> GetModalDataAsync(string modalId, int applicationId, int? recordId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete modal data from storage
    /// </summary>
    Task<bool> DeleteModalDataAsync(string modalId, int recordId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search submissions
    /// </summary>
    Task<IEnumerable<ModuleDataSummary>> SearchAsync(SubmissionSearchCriteria criteria, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get submission statistics
    /// </summary>
    Task<SubmissionStatistics> GetStatisticsAsync(int? opportunityId = null, int? moduleId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for custom field renderers using  Enhanced architecture
/// </summary>
public interface IFieldRenderer
{
    /// <summary>
    /// Supported field types
    /// </summary>
    IEnumerable<string> SupportedFieldTypes { get; }
    
    /// <summary>
    /// Render field HTML with hierarchy support
    /// </summary>
    Task<string> RenderAsync(FormField field, FormDataItem? data, string language, int hierarchyLevel = 0, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Render display-only field HTML with hierarchy support
    /// </summary>
    Task<string> RenderDisplayAsync(FormField field, FormDataItem? data, string language, int hierarchyLevel = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if this renderer can handle the given field type and relationship
    /// </summary>
    bool CanRender(FormField field);

    /// <summary>
    /// Get CSS classes for the field based on hierarchy and relationship
    /// </summary>
    string GetFieldCssClasses(FormField field);
}

#region Supporting Models

/// <summary>
/// Module version information
/// </summary>

/// <summary>
///  enhanced module version information
/// </summary>
public class ModuleVersionInfo
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? OpportunityId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public float Version { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime DateCreated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CreatedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsCurrent { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
    /// <summary>
    /// Enhanced version info specific to standalone modules
    /// </summary>
    public int TotalFields { get; set; }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int RootFields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int MaxDepth { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double ComplexityScore { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string[] FieldTypes { get; set; } = Array.Empty<string>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string[] RelationshipTypes { get; set; } = Array.Empty<string>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Search criteria for modules
/// </summary>
public class ModuleSearchCriteria
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MinFieldCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MaxFieldCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MinDepth { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MaxDepth { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double? MinComplexity { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double? MaxComplexity { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string[]? FieldTypes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string[]? RelationshipTypes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
    public int PageSize { get; set; } = 50;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int PageNumber { get; set; } = 1;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string SortBy { get; set; } = "DateCreated";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool SortDescending { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Search result for modules
/// </summary>
public class ModuleSearchResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ModuleId { get; set; }
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
    public DateTime DateCreated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CreatedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ModuleStatistics Statistics { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Search criteria for submissions
/// </summary>
public class SubmissionSearchCriteria
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? OpportunityId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? ModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Status { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime? DateFrom { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime? DateTo { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CreatedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? SearchText { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int PageSize { get; set; } = 50;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int PageNumber { get; set; } = 1;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string SortBy { get; set; } = "DateUpdated";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool SortDescending { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Submission statistics
/// </summary>
public class SubmissionStatistics
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalSubmissions { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int DraftSubmissions { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int CompletedSubmissions { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int UnderReviewSubmissions { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ApprovedSubmissions { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int RejectedSubmissions { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime? LastSubmissionDate { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double AverageCompletionTime { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Field validation rule
/// </summary>
public class FieldValidationRule
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string RuleType { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Pattern { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public object? Value { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ErrorMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsRequired { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
    /// <summary>
    /// Enhanced properties for fields
    /// </summary>
    public string[]? DependentFields { get; set; }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ParentFieldCondition { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ParentChildRelationshipType? RelationshipType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Localization validation result
/// </summary>
public class LocalizationValidationResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> MissingTranslations { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> IncompleteFields { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, List<string>> LanguageSpecificIssues { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
    /// <summary>
    /// Enhanced properties for hierarchy-aware localization validation
    /// </summary>
    public List<string> MissingHierarchicalTranslations { get; set; } = new();
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, int> FieldDepthTranslationIssues { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#endregion