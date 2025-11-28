using DynamicForms.Core.Entities;
using DynamicForms.Core.Entities.Data;

namespace DynamicForms.RazorPages.Models;

/// <summary>
/// View model for displaying dynamic forms in Razor Pages using  Enhanced architecture
/// </summary>
public class DynamicFormViewModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DynamicFormViewModel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        ExistingData = new Dictionary<string, FormDataItem>();
        ValidationErrors = new Dictionary<string, List<string>>();
        Metadata = new Dictionary<string, object>();
    }

    /// <summary>
    /// The JSON module schema defining the form structure
    /// </summary>
    public FormModule Module { get; set; } = new();

    /// <summary>
    /// Application ID for data persistence
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Opportunity ID for context
    /// </summary>
    public int OpportunityId { get; set; }

    /// <summary>
    /// Current language for localization (EN/FR)
    /// </summary>
    public string Language { get; set; } = "EN";

    /// <summary>
    /// Whether the form is in read-only mode
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Whether to enable auto-save functionality
    /// </summary>
    public bool EnableAutoSave { get; set; } = true;

    /// <summary>
    /// Auto-save interval in milliseconds
    /// </summary>
    public int AutoSaveInterval { get; set; } = 30000;

    /// <summary>
    /// Existing data for form fields (for editing)
    /// </summary>
    public Dictionary<string, FormDataItem> ExistingData { get; set; }

    /// <summary>
    /// Field-level validation errors
    /// </summary>
    public Dictionary<string, List<string>> ValidationErrors { get; set; }

    /// <summary>
    /// Additional metadata for the form
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; }

    /// <summary>
    /// Current step in multi-step forms
    /// </summary>
    public int CurrentStep { get; set; } = 1;

    /// <summary>
    /// Total steps in multi-step forms
    /// </summary>
    public int TotalSteps { get; set; } = 1;

    /// <summary>
    /// CSS classes to apply to the form container
    /// </summary>
    public string? CssClasses { get; set; }

    /// <summary>
    /// Get existing value for a field
    /// </summary>
    public FormDataItem? GetExistingValue(string fieldId)
    {
        return ExistingData.TryGetValue(fieldId, out var value) ? value : null;
    }

    /// <summary>
    /// Get validation errors for a field
    /// </summary>
    public List<string> GetFieldErrors(string fieldId)
    {
        return ValidationErrors.TryGetValue(fieldId, out var errors) ? errors : new List<string>();
    }

    /// <summary>
    /// Check if field has validation errors
    /// </summary>
    public bool HasFieldErrors(string fieldId)
    {
        return ValidationErrors.ContainsKey(fieldId) && ValidationErrors[fieldId].Any();
    }

    /// <summary>
    /// Get visible fields for current step using enhanced capabilities
    /// </summary>
    public IEnumerable<FormField> GetVisibleFields()
    {
        return Module.GetVisibleFields();
    }

    /// <summary>
    /// Get root fields for hierarchical rendering
    /// </summary>
    public IEnumerable<FormField> GetRootFields()
    {
        return Module.GetRootFields();
    }

    /// <summary>
    /// Get required field IDs
    /// </summary>
    public IEnumerable<string> GetRequiredFieldIds()
    {
        return Module.Fields
            .Where(f => f.IsRequired && f.IsVisible)
            .Select(f => f.Id);
    }

    /// <summary>
    /// Calculate form completion percentage
    /// </summary>
    public double GetCompletionPercentage()
    {
        var requiredFields = GetRequiredFieldIds().ToList();
        if (!requiredFields.Any()) return 100.0;

        var completedFields = requiredFields.Count(fieldId => 
        {
            var data = GetExistingValue(fieldId);
            return data?.HasValue() == true;
        });

        return (double)completedFields / requiredFields.Count * 100.0;
    }

    /// <summary>
    /// Get module statistics for enhanced features
    /// </summary>
    public ModuleStatistics GetModuleStatistics()
    {
        return Module.GetModuleStatistics();
    }

    /// <summary>
    /// Get field hierarchy depth for the module
    /// </summary>
    public int GetMaxHierarchyDepth()
    {
        return Module.FieldHierarchy?.MaxDepth ?? 0;
    }

    /// <summary>
    /// Check if the module supports hierarchy features
    /// </summary>
    public bool HasHierarchicalFields()
    {
        return Module.Fields.Any(f => !string.IsNullOrEmpty(f.ParentId) || f.ChildFields.Any());
    }
}

/// <summary>
/// View model for individual dynamic fields using  Enhanced architecture
/// </summary>
public class DynamicFieldViewModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DynamicFieldViewModel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        ValidationErrors = new List<string>();
    }

    /// <summary>
    /// The field definition from JSON schema
    /// </summary>
    public FormField Field { get; set; } = new();

    /// <summary>
    /// Current value/data for the field
    /// </summary>
    public FormDataItem? Value { get; set; }

    /// <summary>
    /// Language for localization
    /// </summary>
    public string Language { get; set; } = "EN";

    /// <summary>
    /// Field index in the form (for naming)
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Whether the field is read-only
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Validation errors for this field
    /// </summary>
    public List<string> ValidationErrors { get; set; }

    /// <summary>
    /// Additional HTML attributes
    /// </summary>
    public Dictionary<string, object>? HtmlAttributes { get; set; }

    /// <summary>
    /// Get field label/description
    /// </summary>
    public string GetLabel()
    {
        return Field.Text.Description.ToString(Language);
    }

    /// <summary>
    /// Get field help text
    /// </summary>
    public string GetHelpText()
    {
        return Field.Text.Help.ToString(Language);
    }

    /// <summary>
    /// Get field placeholder text
    /// </summary>
    public string GetPlaceholder()
    {
        return Field.Text.Placeholder?.ToString(Language) ?? string.Empty;
    }

    /// <summary>
    /// Get validation error message
    /// </summary>
    public string? GetValidationError()
    {
        return ValidationErrors.FirstOrDefault();
    }

    /// <summary>
    /// Check if field has validation errors
    /// </summary>
    public bool HasValidationErrors => ValidationErrors.Any();

    /// <summary>
    /// Get field value as string
    /// </summary>
    public string GetStringValue()
    {
        return Value?.GetDisplayValue(Language) ?? string.Empty;
    }

    /// <summary>
    /// Get CSS classes for the field including hierarchy-aware classes
    /// </summary>
    public string GetCssClasses()
    {
        var classes = new List<string> { "form-control" };
        
        if (Field.IsRequired)
            classes.Add("required");
            
        if (HasValidationErrors)
            classes.Add("is-invalid");
            
        if (IsReadOnly || Field.ReadOnly)
            classes.Add("readonly");
            
        if (!string.IsNullOrEmpty(Field.CssClasses))
            classes.Add(Field.CssClasses);

        // Add hierarchy-aware CSS classes
        classes.Add($"field-level-{Field.HierarchicalLevel}");
        classes.Add($"relationship-{Field.RelationshipType.ToString().ToLowerInvariant()}");
        
        if (Field.Parent != null)
        {
            classes.Add("field-child");
            classes.Add($"parent-{Field.Parent.Id}");
        }
        else
        {
            classes.Add("field-root");
        }

        // Add field type class
        classes.Add($"field-type-{Field.FieldType.Type.ToLowerInvariant()}");
            
        return string.Join(" ", classes);
    }

    /// <summary>
    /// Get hierarchical path for debugging
    /// </summary>
    public string GetHierarchicalPath()
    {
        return Field.HierarchicalPath;
    }

    /// <summary>
    /// Check if field should be visible based on enhanced logic
    /// </summary>
    public bool ShouldBeVisible(Dictionary<string, object>? formData = null)
    {
        return Field.ShouldBeVisibleEnhanced(formData);
    }

    /// <summary>
    /// Get child fields for container types
    /// </summary>
    public IEnumerable<FormField> GetChildFields()
    {
        return Field.ChildFields.Where(child => child.IsVisible).OrderBy(child => child.Order);
    }
}

/// <summary>
/// View model for form submission
/// </summary>
public class FormSubmissionModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FormSubmissionModel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Fields = new List<FieldSubmissionModel>();
        Modals = new List<ModalSubmissionModel>();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int OpportunityId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ApplicationId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Language { get; set; } = "EN";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Action { get; set; } = "Save";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<FieldSubmissionModel> Fields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<ModalSubmissionModel> Modals { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Model for individual field submission
/// </summary>
public class FieldSubmissionModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FieldSubmissionModel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        MultiValues = new List<string>();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string FieldId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Value { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> MultiValues { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FormData { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Model for modal data submission
/// </summary>
public class ModalSubmissionModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ModalSubmissionModel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Records = new List<ModalRecordSubmissionModel>();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string ModalId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<ModalRecordSubmissionModel> Records { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Model for individual modal record submission
/// </summary>
public class ModalRecordSubmissionModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ModalRecordSubmissionModel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Fields = new List<FieldSubmissionModel>();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string RecordId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<FieldSubmissionModel> Fields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// View model for modal/table displays using  Enhanced architecture
/// </summary>
public class ModalDisplayViewModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ModalDisplayViewModel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Records = new List<ModalRecord>();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FormField Field { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IEnumerable<FormField> ModalFields { get; set; } = Array.Empty<FormField>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<ModalRecord> Records { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Language { get; set; } = "EN";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool CanAdd { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool CanEdit { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool CanDelete { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsReadOnly { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// View model for file upload fields using  Enhanced architecture
/// </summary>
public class FileUploadViewModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FormField Field { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FileUploadData? ExistingFile { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Language { get; set; } = "EN";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsReadOnly { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int MaxFiles { get; set; } = 1;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// View model for validation summary
/// </summary>
public class ValidationSummaryViewModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ValidationSummaryViewModel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Errors = new List<ValidationErrorViewModel>();
        Warnings = new List<ValidationWarningViewModel>();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<ValidationErrorViewModel> Errors { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<ValidationWarningViewModel> Warnings { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Language { get; set; } = "EN";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowFieldNames { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class ValidationErrorViewModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string FieldId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string FieldName { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Message { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Code { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class ValidationWarningViewModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string FieldId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string FieldName { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Message { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Code { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// View model for enhanced module statistics display
/// </summary>
public class ModuleStatisticsViewModel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ModuleStatistics Statistics { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Language { get; set; } = "EN";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowDetailedMetrics { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Get formatted complexity score
    /// </summary>
    public string GetFormattedComplexityScore()
    {
        return Statistics.ComplexityScore.ToString("F1");
    }

    /// <summary>
    /// Get field type distribution as percentages
    /// </summary>
    public Dictionary<string, double> GetFieldTypePercentages()
    {
        var total = Statistics.TotalFields;
        if (total == 0) return new Dictionary<string, double>();

        return Statistics.FieldTypes.ToDictionary(
            kvp => kvp.Key,
            kvp => (double)kvp.Value / total * 100.0
        );
    }

    /// <summary>
    /// Get complexity assessment text
    /// </summary>
    public string GetComplexityAssessment()
    {
        return Statistics.ComplexityScore switch
        {
            < 2.0 => "Simple",
            < 5.0 => "Moderate", 
            < 8.0 => "Complex",
            _ => "Very Complex"
        };
    }
}