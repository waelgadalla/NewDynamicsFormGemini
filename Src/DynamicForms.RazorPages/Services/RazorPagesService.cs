using DynamicForms.Core.Entities;
using DynamicForms.Core.Interfaces;
using DynamicForms.Core.Services.Enhanced;
using Microsoft.Extensions.Logging;

namespace DynamicForms.RazorPages.Services;

/// <summary>
///  Razor Pages service for working with modules
/// Uses only * classes without any inheritance or conversion dependencies
/// </summary>
public class RazorPagesService
{
    private readonly IFormModuleRepository _repository;
    private readonly FieldHierarchyManagerService _hierarchyManager;
    private readonly ILogger<RazorPagesService> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public RazorPagesService(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        IFormModuleRepository repository,
        FieldHierarchyManagerService hierarchyManager,
        ILogger<RazorPagesService> logger)
    {
        _repository = repository;
        _hierarchyManager = hierarchyManager;
        _logger = logger;
    }

    #region Module Loading and Management

    /// <summary>
    /// Load a module for Razor Pages with built hierarchy
    /// </summary>
    public async Task<FormModule?> GetModuleAsync(int moduleId, int? opportunityId = null)
    {
        try
        {
            // Load module directly from repository
            var formModule = await _repository.GetEnhancedMetadataWithHierarchyAsync(moduleId, opportunityId);
            
            if (formModule == null)
            {
                _logger.LogWarning("Module {ModuleId} not found", moduleId);
                return null;
            }

            _logger.LogDebug("Loaded module {ModuleId} with {FieldCount} fields and {MaxDepth} max depth", 
                moduleId, formModule.Fields.Length, formModule.FieldHierarchy.MaxDepth);

            return formModule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading module {ModuleId}", moduleId);
            throw;
        }
    }

    /// <summary>
    /// Save a module back to the repository
    /// </summary>
    public async Task<bool> SaveModuleAsync(FormModule formModule)
    {
        try
        {
            if (formModule.Id == null)
            {
                _logger.LogError("Cannot save module without ID");
                return false;
            }

            // Save directly using the enhanced repository
            var result = await _repository.SaveEnhancedModuleWithOptimizationAsync(
                formModule, formModule.Id.Value, formModule.OpportunityId);
            
            if (result)
            {
                _logger.LogInformation("Successfully saved module {ModuleId}", formModule.Id);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving module {ModuleId}", formModule.Id);
            return false;
        }
    }

    #endregion

    #region Form Rendering Support

    /// <summary>
    /// Get fields for rendering in a Razor Page with proper hierarchy
    /// </summary>
    public IEnumerable<FormField> GetFieldsForRazorPageRendering(
        FormModule module, 
        Dictionary<string, object>? formData = null)
    {
        return module.GetVisibleFields(formData);
    }

    /// <summary>
    /// Get root fields only (for rendering the top level)
    /// </summary>
    public IEnumerable<FormField> GetRootFields(FormModule module)
    {
        return module.GetRootFields();
    }

    /// <summary>
    /// Get child fields for a parent (for rendering nested structures)
    /// </summary>
    public IEnumerable<FormField> GetChildFields(
        FormField parentField, 
        Dictionary<string, object>? formData = null)
    {
        return parentField.ChildFields
            .Where(child => child.ShouldBeVisibleEnhanced(formData))
            .OrderBy(child => child.Order ?? int.MaxValue);
    }

    /// <summary>
    /// Check if a field should be rendered in the current context
    /// </summary>
    public bool ShouldRenderField(FormField field, Dictionary<string, object>? formData = null)
    {
        return field.ShouldBeVisibleEnhanced(formData);
    }

    /// <summary>
    /// Get CSS classes for field rendering with hierarchy context
    /// </summary>
    public string GetFieldCssClasses(FormField field)
    {
        var classes = new List<string>();
        
        // Add base CSS classes
        if (!string.IsNullOrEmpty(field.CssClasses))
        {
            classes.Add(field.CssClasses);
        }

        // Add hierarchy-based classes
        classes.Add($"field-level-{field.HierarchicalLevel}");
        classes.Add($"relationship-{field.RelationshipType.ToString().ToLowerInvariant()}");
        
        if (field.Parent != null)
        {
            classes.Add("field-child");
            classes.Add($"parent-{field.Parent.Id}");
        }
        else
        {
            classes.Add("field-root");
        }

        // Add field type class
        classes.Add($"field-type-{field.FieldType.Type.ToLowerInvariant()}");

        return string.Join(" ", classes.Where(c => !string.IsNullOrEmpty(c)));
    }

    #endregion

    #region Form Validation

    /// <summary>
    /// Validate form data using validation
    /// </summary>
    public RazorPageValidationResult ValidateFormData(
        FormModule module, 
        Dictionary<string, object> formData)
    {
        var result = new RazorPageValidationResult();

        try
        {
            var moduleValidation = module.ValidateModuleEnhanced(formData);
            
            // Convert to Razor Pages friendly format
            result.IsValid = moduleValidation.IsValid;
            
            // Field errors
            foreach (var fieldError in moduleValidation.FieldErrors)
            {
                result.FieldErrors.Add(fieldError.FieldId, fieldError.ErrorMessage);
            }

            // Module-level errors
            result.ModuleErrors.AddRange(moduleValidation.ModuleErrors);

            // Hierarchy warnings (for debugging)
            result.HierarchyWarnings.AddRange(moduleValidation.HierarchyWarnings);

            _logger.LogDebug("Validation completed for module {ModuleId}. Valid: {IsValid}, Errors: {ErrorCount}", 
                module.Id, result.IsValid, result.FieldErrors.Count + result.ModuleErrors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during form validation for module {ModuleId}", module.Id);
            result.ModuleErrors.Add("An error occurred during validation. Please try again.");
        }

        return result;
    }

    /// <summary>
    /// Get validation errors for a specific field
    /// </summary>
    public List<string> GetFieldValidationErrors(
        FormField field, 
        object? fieldValue, 
        Dictionary<string, object>? formData = null)
    {
        var validation = field.ValidateFieldEnhanced(fieldValue, formData);
        return validation.Errors;
    }

    #endregion

    #region Hierarchy Management

    /// <summary>
    /// Add a new field to a module with automatic hierarchy integration
    /// </summary>
    public HierarchyUpdateResult AddFieldToModule(
        FormModule module, 
        FormField newField, 
        string? parentFieldId = null)
    {
        var result = new HierarchyUpdateResult();

        try
        {
            // Find parent if specified
            FormField? parent = null;
            if (!string.IsNullOrEmpty(parentFieldId))
            {
                parent = module.GetEnhancedField(parentFieldId);
                if (parent == null)
                {
                    result.Errors.Add($"Parent field '{parentFieldId}' not found");
                    return result;
                }
            }

            // Set parent relationship
            if (parent != null)
            {
                try
                {
                    parent.AddChildField(newField);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to establish parent relationship: {ex.Message}");
                    return result;
                }
            }

            // Add to module
            module.AddEnhancedField(newField);
            
            result.IsSuccessful = true;
            result.FieldsAffected = 1;
            
            _logger.LogDebug("Added field {FieldId} to module {ModuleId}", newField.Id, module.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding field {FieldId} to module {ModuleId}", newField.Id, module.Id);
            result.Errors.Add($"Failed to add field: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Remove a field from a module with cleanup of relationships
    /// </summary>
    public HierarchyUpdateResult RemoveFieldFromModule(FormModule module, string fieldId)
    {
        var result = new HierarchyUpdateResult();

        try
        {
            var field = module.GetEnhancedField(fieldId);
            if (field == null)
            {
                result.Errors.Add($"Field '{fieldId}' not found in module");
                return result;
            }

            // Count affected fields (field + all descendants)
            var affectedCount = 1 + field.GetAllDescendants().Count();

            // Remove field and descendants
            var removeSuccess = module.RemoveEnhancedField(fieldId);
            
            if (removeSuccess)
            {
                result.IsSuccessful = true;
                result.FieldsAffected = affectedCount;
                
                _logger.LogDebug("Removed field {FieldId} and {DescendantCount} descendants from module {ModuleId}", 
                    fieldId, affectedCount - 1, module.Id);
            }
            else
            {
                result.Errors.Add($"Failed to remove field '{fieldId}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing field {FieldId} from module {ModuleId}", fieldId, module.Id);
            result.Errors.Add($"Failed to remove field: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Move a field to a different parent with validation
    /// </summary>
    public HierarchyUpdateResult MoveField(FormModule module, string fieldId, string? newParentId = null)
    {
        var result = new HierarchyUpdateResult();

        try
        {
            var field = module.GetEnhancedField(fieldId);
            if (field == null)
            {
                result.Errors.Add($"Field '{fieldId}' not found");
                return result;
            }

            FormField? newParent = null;
            if (!string.IsNullOrEmpty(newParentId))
            {
                newParent = module.GetEnhancedField(newParentId);
                if (newParent == null)
                {
                    result.Errors.Add($"New parent field '{newParentId}' not found");
                    return result;
                }
            }

            var oldParentId = field.Parent?.Id;
            
            // Perform the move
            var moveSuccess = field.MoveTo(newParent);
            
            if (moveSuccess)
            {
                // Rebuild module hierarchy
                module.RebuildFieldHierarchy();
                
                result.IsSuccessful = true;
                result.FieldsAffected = 1;
                
                _logger.LogDebug("Moved field {FieldId} from {OldParent} to {NewParent}", 
                    fieldId, oldParentId ?? "root", newParentId ?? "root");
            }
            else
            {
                result.Errors.Add("Move operation failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving field {FieldId} in module {ModuleId}", fieldId, module.Id);
            result.Errors.Add($"Failed to move field: {ex.Message}");
        }

        return result;
    }

    #endregion

    #region Analysis and Debugging

    /// <summary>
    /// Get module statistics for Razor Pages display
    /// </summary>
    public ModuleStatistics GetModuleStatistics(FormModule module)
    {
        return module.GetModuleStatistics();
    }

    /// <summary>
    /// Get cascade options for a field based on parent value
    /// </summary>
    public IEnumerable<Option> GetCascadeOptions(FormField field, string parentFieldId, string parentValue)
    {
        if (field.Options == null || !field.Options.Any())
            return Enumerable.Empty<Option>();

        // Filter options based on parent value
        // This is a simple example - implement based on your business logic
        return field.Options.Where(o => o.IsActive);
    }

    #endregion
}

#region Result Classes for Razor Pages ( versions)

/// <summary>
/// Validation result tailored for Razor Pages ( version)
/// </summary>
public class RazorPageValidationResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, string> FieldErrors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> ModuleErrors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> HierarchyWarnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IEnumerable<string> GetAllErrors()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return FieldErrors.Values.Concat(ModuleErrors);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasFieldError(string fieldId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return FieldErrors.ContainsKey(fieldId);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? GetFieldError(string fieldId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return FieldErrors.GetValueOrDefault(fieldId);
    }
}

/// <summary>
/// Result of hierarchy update operations ( version)
/// </summary>
public class HierarchyUpdateResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSuccessful { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int FieldsAffected { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#endregion