using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DynamicForms.Core.Entities;

/// <summary>
///  Enhanced FormModule with improved field hierarchy management
/// </summary>
public class FormModule
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FormModule()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Text = new TextResource();
        Database = new ModuleDatabase();
        Validations = new Validation();
        Fields = Array.Empty<FormField>();
        DateGenerated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    #region Core Properties
    /// <summary>
    /// UTC timestamp when the module schema was generated
    /// </summary>
    public string? DateGenerated { get; set; }
    
    /// <summary>
    /// UTC timestamp when the module schema was last updated
    /// </summary>
    public string? DateUpdated { get; set; }
    
    /// <summary>
    /// Schema version for backwards compatibility
    /// </summary>
    public float Version { get; set; } = 1.0f;
    
    /// <summary>
    /// Unique module identifier
    /// </summary>
    public int? Id { get; set; }
    
    /// <summary>
    /// Associated opportunity/program identifier
    /// </summary>
    public int? OpportunityId { get; set; }
    
    /// <summary>
    /// Multilingual text resources for the module
    /// </summary>
    public TextResource Text { get; set; }
    
    /// <summary>
    /// Database configuration for data persistence
    /// </summary>
    public ModuleDatabase Database { get; set; }
    
    /// <summary>
    /// Module-level validation rules
    /// </summary>
    public Validation Validations { get; set; }
    
    /// <summary>
    /// Collection of enhanced form fields in this module
    /// </summary>
    public FormField[] Fields { get; set; }

    /// <summary>
    /// Conditional rules for visual rules builder (Phase 4)
    /// </summary>
    public List<ConditionalRuleV4>? ConditionalRulesV4 { get; set; }
    #endregion

    #region Enhanced Properties
    /// <summary>
    /// Field hierarchy information (calculated at runtime)
    /// </summary>
    [JsonIgnore]
    public FieldHierarchyInfo FieldHierarchy { get; private set; } = new();

    /// <summary>
    /// Module complexity metrics
    /// </summary>
    [JsonIgnore]
    public ModuleComplexityMetrics ComplexityMetrics { get; private set; } = new();

    
    [JsonIgnore]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IEnumerable<FormField> EnhancedFields => Fields;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion

    #region Hierarchy Management
    /// <summary>
    /// Add an enhanced field to the module with automatic relationship building
    /// </summary>
    public void AddEnhancedField(FormField field)
    {
        if (field == null) throw new ArgumentNullException(nameof(field));
        
        // Set module context
        field.ModuleId = Id;
        field.OpportunityId = OpportunityId;
        
        // Add to Fields collection (single source of truth)
        var fieldsList = Fields?.ToList() ?? new List<FormField>();
        if (!fieldsList.Any(f => f.Id == field.Id))
        {
            fieldsList.Add(field);
            Fields = fieldsList.ToArray();
        }
        
        // Rebuild hierarchy
        RebuildFieldHierarchy();
    }

    /// <summary>
    /// Remove a field and its descendants from the module
    /// </summary>
    public bool RemoveEnhancedField(string fieldId)
    {
        var field = GetEnhancedField(fieldId);
        if (field == null) return false;

        // Get all descendants to remove
        var toRemove = field.GetAllDescendants().ToList();
        toRemove.Add(field);

        // Remove from Fields collection
        var fieldsList = Fields?.ToList() ?? new List<FormField>();
        foreach (var fieldToRemove in toRemove)
        {
            var existingField = fieldsList.FirstOrDefault(f => f.Id == fieldToRemove.Id);
            if (existingField != null)
                fieldsList.Remove(existingField);
        }
        Fields = fieldsList.ToArray();

        // Remove from parent's children if applicable
        if (field.Parent != null)
        {
            field.Parent.RemoveChildField(fieldId);
        }

        // Rebuild hierarchy
        RebuildFieldHierarchy();
        
        return true;
    }

    /// <summary>
    /// Get enhanced field by ID
    /// </summary>
    public FormField? GetEnhancedField(string fieldId)
    {
        return Fields.FirstOrDefault(f => f.Id == fieldId);
    }

    /// <summary>
    /// Rebuild field hierarchy from current field collection
    /// </summary>
    public void RebuildFieldHierarchy()
    {
        var result = new FieldHierarchyBuildResult();

        try
        {
            // Clear existing hierarchy
            FieldHierarchy = new FieldHierarchyInfo();
            
            // Clear existing parent-child relationships
            foreach (var field in Fields)
            {
                field.Parent = null;
                field.ChildFields.Clear();
            }

            // Create lookup dictionary for performance
            // Use GroupBy to handle duplicate field IDs safely
            var fieldLookup = Fields
                .GroupBy(f => f.Id)
                .ToDictionary(g => g.Key, g => g.First());

            // Rebuild parent-child relationships
            foreach (var field in Fields)
            {
                if (!string.IsNullOrEmpty(field.ParentId))
                {
                    if (fieldLookup.TryGetValue(field.ParentId, out var parent))
                    {
                        try
                        {
                            parent.AddChildField(field);
                        }
                        catch (Exception ex)
                        {
                            result.Warnings.Add($"Could not establish relationship between {parent.Id} and {field.Id}: {ex.Message}");
                            field.ParentId = null; // Clear invalid reference
                        }
                    }
                    else
                    {
                        result.Warnings.Add($"Field '{field.Id}' references non-existent parent '{field.ParentId}'");
                        field.ParentId = null; // Clear invalid reference
                    }
                }
            }

            // Build hierarchy structure
            FieldHierarchy.RootFields = GetRootFields().ToList();
            FieldHierarchy.MaxDepth = CalculateMaxHierarchicalDepth();
            FieldHierarchy.TotalFields = Fields.Length;
            FieldHierarchy.BuildResult = result;
           
            
            // Calculate complexity metrics
            RecalculateComplexityMetrics();

            result.IsSuccessful = true;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to rebuild field hierarchy: {ex.Message}");
            FieldHierarchy.BuildResult = result;
        }
    }

    /// <summary>
    /// Ensure all required nested objects are initialized after deserialization
    /// This is necessary because JSON deserialization bypasses constructors
    /// </summary>
    public void EnsureInitialized()
    {
        // Initialize core properties if null
        Text ??= new TextResource();
        Database ??= new ModuleDatabase();
        Validations ??= new Validation();
        Fields ??= Array.Empty<FormField>();

        // Ensure TextResource nested properties are initialized
        if (Text != null)
        {
            Text.Description ??= new TextClass();
            Text.DescriptionHtml ??= new TextClass();
            Text.Title ??= new TextClass();
            Text.Name ??= new TextClass();
            Text.ShortName ??= new TextClass();
            Text.Instructions ??= new TextClass();
        }

        // Ensure each field is also initialized
        if (Fields != null)
        {
            foreach (var field in Fields)
            {
                field?.EnsureInitialized();
            }
        }
    }

    /// <summary>
    /// Get all root fields (fields without parents)
    /// </summary>
    public IEnumerable<FormField> GetRootFields()
    {
        return Fields.Where(f => f.Parent == null).OrderBy(f => f.Order ?? int.MaxValue);
    }

    /// <summary>
    /// Get all visible fields in proper display order (depth-first traversal)
    /// </summary>
    public IEnumerable<FormField> GetVisibleFields(Dictionary<string, object>? formData = null)
    {
        var result = new List<FormField>();
        
        foreach (var rootField in GetRootFields())
        {
            if (rootField.ShouldBeVisibleEnhanced(formData))
            {
                AddFieldAndDescendantsInOrder(rootField, result, formData);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Validate the entire module hierarchy
    /// </summary>
    public ModuleValidationResult ValidateModuleEnhanced(Dictionary<string, object>? formData = null)
    {
        var result = new ModuleValidationResult();

        // Validate each field
        foreach (var field in Fields)
        {
            var fieldValue = formData?.GetValueOrDefault(field.Id);
            var fieldValidation = field.ValidateFieldEnhanced(fieldValue, formData);
            
            if (!fieldValidation.IsValid)
            {
                result.FieldErrors.AddRange(
                    fieldValidation.Errors.Select(e => new FieldValidationError 
                    { 
                        FieldId = field.Id, 
                        FieldName = field.Text.Description.EN ?? field.Id,
                        ErrorMessage = e 
                    })
                );
            }

            // Validate hierarchy for this field
            var hierarchyValidation = field.ValidateHierarchy();
            result.HierarchyErrors.AddRange(hierarchyValidation.Errors);
            result.HierarchyWarnings.AddRange(hierarchyValidation.Warnings);
        }

        // Module-level validations
        ValidateModuleLevelRules(formData, result);

        return result;
    }

    /// <summary>
    /// Get module statistics
    /// </summary>
    public ModuleStatistics GetModuleStatistics()
    {
        return new ModuleStatistics
        {
            TotalFields = Fields.Length,
            RootFields = GetRootFields().Count(),
            MaxDepth = FieldHierarchy.MaxDepth,
            ComplexityScore = ComplexityMetrics.OverallComplexity,
            RelationshipTypes = Fields
                .GroupBy(f => f.RelationshipType)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            FieldTypes = Fields
                .GroupBy(f => f.FieldType.Type)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
    #endregion

    #region Private Helper Methods
    private void AddFieldAndDescendantsInOrder(FormField field, List<FormField> result, Dictionary<string, object>? formData)
    {
        result.Add(field);
        
        // Add children in order
        var visibleChildren = field.ChildFields
            .Where(c => c.ShouldBeVisibleEnhanced(formData))
            .OrderBy(c => c.Order ?? int.MaxValue);

        foreach (var child in visibleChildren)
        {
            AddFieldAndDescendantsInOrder(child, result, formData);
        }
    }

    private int CalculateMaxHierarchicalDepth()
    {
        return Fields.Length > 0 ? Fields.Max(f => f.HierarchicalLevel) : 0;
    }

    private void RecalculateComplexityMetrics()
    {
        var totalFields = Fields.Length;
        var totalRelationships = Fields.Count(f => f.Parent != null);
        var conditionalFields = Fields.Count(f => f.IsConditionallyRequired || f.ConditionalRules.Any());
        var maxDepth = FieldHierarchy.MaxDepth;

        ComplexityMetrics = new ModuleComplexityMetrics
        {
            TotalFields = totalFields,
            TotalRelationships = totalRelationships,
            ConditionalFields = conditionalFields,
            MaxHierarchyDepth = maxDepth,
            OverallComplexity = CalculateComplexityScore(totalFields, totalRelationships, conditionalFields, maxDepth)
        };
    }

    private static double CalculateComplexityScore(int totalFields, int totalRelationships, int conditionalFields, int maxDepth)
    {
        // Simple complexity calculation - can be made more sophisticated
        var baseComplexity = totalFields * 1.0;
        var relationshipComplexity = totalRelationships * 2.0;
        var conditionalComplexity = conditionalFields * 3.0;
        var depthComplexity = maxDepth * maxDepth * 1.5;

        return baseComplexity + relationshipComplexity + conditionalComplexity + depthComplexity;
    }

    private void ValidateModuleLevelRules(Dictionary<string, object>? formData, ModuleValidationResult result)
    {
        if (formData == null) return;

        // Validate OneFieldRequired rules
        foreach (var oneRequired in Validations.OneFieldRequired)
        {
            var fieldIds = oneRequired.Split(',');
            var hasValue = fieldIds.Any(id => 
            {
                var value = formData.GetValueOrDefault(id.Trim());
                return value != null && !string.IsNullOrWhiteSpace(value.ToString());
            });

            if (!hasValue)
            {
                var fieldNames = fieldIds.Select(id => GetEnhancedField(id.Trim())?.Text.Description.EN ?? id).ToArray();
                result.ModuleErrors.Add($"At least one of the following fields must be filled: {string.Join(", ", fieldNames)}");
            }
        }
    }
    #endregion

    #region Nested Classes
    /// <summary>
    /// Validation configuration for the entire module
    /// </summary>
    public class Validation
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string[] OneFieldRequired { get; set; } = Array.Empty<string>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string[] DateFields { get; set; } = Array.Empty<string>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string[] ConditionallyRequiredFields { get; set; } = Array.Empty<string>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string[] NumericFields { get; set; } = Array.Empty<string>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Multilingual text resources for module metadata
    /// </summary>
    public class TextResource
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextResource()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Description = new TextClass();
            DescriptionHtml = new TextClass();
            Title = new TextClass();
            Name = new TextClass();
            ShortName = new TextClass();
            Instructions = new TextClass();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass DescriptionHtml { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Title { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass ShortName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Instructions { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
    #endregion
}

#region Supporting Classes (with  prefix to avoid conflicts)
/// <summary>
/// Information about field hierarchy structure
/// </summary>
public class FieldHierarchyInfo
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<FormField> RootFields { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int MaxDepth { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalFields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FieldHierarchyBuildResult BuildResult { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Result of building field hierarchy
/// </summary>
public class FieldHierarchyBuildResult
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
/// Module complexity metrics
/// </summary>
public class ModuleComplexityMetrics
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalFields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalRelationships { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ConditionalFields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int MaxHierarchyDepth { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double OverallComplexity { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Module validation result with enhanced error reporting
/// </summary>
public class ModuleValidationResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<FieldValidationError> FieldErrors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> ModuleErrors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> HierarchyErrors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> HierarchyWarnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid => !FieldErrors.Any() && !ModuleErrors.Any() && !HierarchyErrors.Any();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Field-specific validation error
/// </summary>
public class FieldValidationError
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string FieldId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string FieldName { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string ErrorMessage { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Module statistics for analysis
/// </summary>
public class ModuleStatistics
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int TotalFields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
    public Dictionary<string, int> RelationshipTypes { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, int> FieldTypes { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Database configuration for module data storage
/// </summary>
public class ModuleDatabase
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? TableName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ConnectionString { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? SchemaName { get; set; } = "dbo";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool UseJsonColumns { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
#endregion