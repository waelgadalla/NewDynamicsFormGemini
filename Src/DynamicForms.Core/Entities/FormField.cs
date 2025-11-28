using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DynamicForms.Core.Enums;

namespace DynamicForms.Core.Entities;

/// <summary>
///  Enhanced FormField with robust parent-child relationship management
/// </summary>
public class FormField : IComparable<FormField>, IEquatable<FormField>
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FormField()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Id = Guid.NewGuid().ToString("N")[..8]; // Short unique ID
        Database = new FieldDatabase();
        Text = new TextResource();
        FieldType = new FieldType();
        IsVisibleInEditor = true;
        IsVisibleInDisplay = true;
        Options = Array.Empty<Option>();
        ValidatingFields = Array.Empty<ValidatingField>();
        SpeciesAutoCompleteFields = Array.Empty<SpeciesAutoCompleteField>();
        ConditionalRules = Array.Empty<ConditionalRule>();
        
        // Enhanced-specific initialization
        ChildFields = new List<FormField>();
        DependentFields = new List<string>();
        RelationshipType = ParentChildRelationshipType.GroupContainer;
    }

    #region Core Properties
    /// <summary>
    /// Schema version for field compatibility
    /// </summary>
    public float Version { get; set; } = 1.0f;
    
    /// <summary>
    /// Unique field identifier (8-character alphanumeric)
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Parent module identifier
    /// </summary>
    public int? ModuleId { get; set; }
    
    /// <summary>
    /// Associated opportunity identifier
    /// </summary>
    public int? OpportunityId { get; set; }
    
    /// <summary>
    /// Display order within the form (1-based)
    /// </summary>
    public int? Order { get; set; }
    
    /// <summary>
    /// Field type configuration
    /// </summary>
    public FieldType FieldType { get; set; }
    #endregion

    #region Visibility and State
    /// <summary>
    /// Whether the field is active in the system
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether the field is visible to users
    /// </summary>
    public bool IsVisible { get; set; } = true;
    
    /// <summary>
    /// Whether the field appears in edit/input forms
    /// </summary>
    public bool IsVisibleInEditor { get; set; } = true;
    
    /// <summary>
    /// Whether the field appears in display/read-only views
    /// </summary>
    public bool IsVisibleInDisplay { get; set; } = true;
    
    /// <summary>
    /// Whether the field is read-only
    /// </summary>
    public bool ReadOnly { get; set; }
    #endregion

    #region Validation
    /// <summary>
    /// Whether the field is required for form submission
    /// </summary>
    public bool IsRequired { get; set; }
    
    /// <summary>
    /// Whether the field is required when in a parent-child relationship
    /// </summary>
    public bool IsRequiredInParent { get; set; }
    
    /// <summary>
    /// Parent field ID for hierarchical relationships
    /// </summary>
    public string? ParentId { get; set; }
    
    /// <summary>
    /// Maximum character length for text inputs
    /// </summary>
    public int? MaximumLength { get; set; }
    
    /// <summary>
    /// Minimum character length for text inputs
    /// </summary>
    public int? MinimumLength { get; set; }
    
    /// <summary>
    /// Whether the field has conditional validation rules
    /// </summary>
    public bool IsConditionallyRequired { get; set; }
    
    /// <summary>
    /// Fields that affect this field's validation
    /// </summary>
    public ValidatingField[] ValidatingFields { get; set; }
    
    /// <summary>
    /// Complex conditional validation rules
    /// </summary>
    public ConditionalRule[] ConditionalRules { get; set; }
    #endregion

    #region Data Source
    /// <summary>
    /// Code set ID for dropdown/selection options
    /// </summary>
    public int? CodeSetId { get; set; }
    
    /// <summary>
    /// Predefined options for selection controls
    /// </summary>
    public Option[] Options { get; set; }
    
    /// <summary>
    /// Species autocomplete configuration
    /// </summary>
    public IEnumerable<SpeciesAutoCompleteField> SpeciesAutoCompleteFields { get; set; }
    #endregion

    #region Layout and Styling
    /// <summary>
    /// CSS width class (e.g., 1-12 for Bootstrap grid)
    /// </summary>
    public int? WidthClass { get; set; }
    
    /// <summary>
    /// CSS classes to apply to the field container
    /// </summary>
    public string? CssClasses { get; set; }
    
    /// <summary>
    /// Inline styles to apply
    /// </summary>
    public string? InlineStyles { get; set; }
    #endregion

    #region Complex Field Types
    /// <summary>
    /// Modal/popup configuration for table fields
    /// </summary>
    public Modal? Modal { get; set; }
    
    /// <summary>
    /// File upload configuration
    /// </summary>
    public FileUploadConfiguration? FileUpload { get; set; }
    #endregion

    #region Metadata
    /// <summary>
    /// Multilingual text resources
    /// </summary>
    public TextResource Text { get; set; }
    
    /// <summary>
    /// Database configuration
    /// </summary>
    public FieldDatabase Database { get; set; }
    
    /// <summary>
    /// Custom metadata for extensibility
    /// </summary>
    public Dictionary<string, object>? CustomProperties { get; set; }
    #endregion

    #region Enhanced Parent-Child Relationships
    /// <summary>
    /// Reference to the parent field object (populated at runtime)
    /// </summary>
    [JsonIgnore]
    public FormField? Parent { get; set; }

    /// <summary>
    /// Collection of direct child fields
    /// </summary>
    [JsonIgnore]
    public List<FormField> ChildFields { get; set; }

    /// <summary>
    /// Type of relationship with parent
    /// </summary>
    public ParentChildRelationshipType RelationshipType { get; set; }

    /// <summary>
    /// Fields that depend on this field's value
    /// </summary>
    public List<string> DependentFields { get; set; }

    /// <summary>
    /// Calculated hierarchical level (0 = root, 1 = first level child, etc.)
    /// </summary>
    [JsonIgnore]
    public int HierarchicalLevel => CalculateHierarchicalLevel();

    /// <summary>
    /// Full path from root to this field (e.g., "section1.group1.field1")
    /// </summary>
    [JsonIgnore]
    public string HierarchicalPath => CalculateHierarchicalPath();
    #endregion

    #region Hierarchy Navigation Methods
    /// <summary>
    /// Get all descendant fields (children, grandchildren, etc.)
    /// </summary>
    public IEnumerable<FormField> GetAllDescendants()
    {
        foreach (var child in ChildFields)
        {
            yield return child;
            foreach (var descendant in child.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Get all ancestor fields (parent, grandparent, etc.)
    /// </summary>
    public IEnumerable<FormField> GetAllAncestors()
    {
        var current = Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Get root field (field with no parent)
    /// </summary>
    public FormField GetRoot()
    {
        var current = this;
        while (current.Parent != null)
        {
            current = current.Parent;
        }
        return current;
    }

    /// <summary>
    /// Get sibling fields (fields with same parent)
    /// </summary>
    public IEnumerable<FormField> GetSiblings()
    {
        if (Parent == null) return Enumerable.Empty<FormField>();
        return Parent.ChildFields.Where(f => f.Id != Id);
    }

    /// <summary>
    /// Find a descendant field by ID
    /// </summary>
    public FormField? FindDescendant(string fieldId)
    {
        return GetAllDescendants().FirstOrDefault(f => f.Id == fieldId);
    }

    /// <summary>
    /// Find an ancestor field by ID
    /// </summary>
    public FormField? FindAncestor(string fieldId)
    {
        return GetAllAncestors().FirstOrDefault(f => f.Id == fieldId);
    }
    #endregion

    #region Hierarchy Management Methods
    /// <summary>
    /// Add a child field with relationship validation
    /// </summary>
    public bool AddChildField(FormField child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        
        // Validate relationship
        if (!CanHaveChildren)
        {
            throw new InvalidOperationException($"Field '{Id}' of type '{FieldType.Type}' cannot have children");
        }

        if (child.Id == Id)
        {
            throw new InvalidOperationException("Field cannot be its own child");
        }

        // Check for circular reference
        if (WouldCreateCircularReference(child))
        {
            throw new InvalidOperationException($"Adding field '{child.Id}' would create a circular reference");
        }

        // Remove from existing parent if any
        if (child.Parent != null)
        {
            child.Parent.RemoveChildField(child.Id);
        }

        // Establish relationship
        child.Parent = this;
        child.ParentId = Id;
        
        if (!ChildFields.Contains(child))
        {
            ChildFields.Add(child);
        }

        return true;
    }

    /// <summary>
    /// Remove a child field by ID
    /// </summary>
    public bool RemoveChildField(string childId)
    {
        var child = ChildFields.FirstOrDefault(c => c.Id == childId);
        if (child == null) return false;

        child.Parent = null;
        child.ParentId = null;
        ChildFields.Remove(child);
        
        return true;
    }

    /// <summary>
    /// Move this field to a new parent
    /// </summary>
    public bool MoveTo(FormField? newParent)
    {
        // Remove from current parent
        if (Parent != null)
        {
            Parent.RemoveChildField(Id);
        }

        // Add to new parent
        if (newParent != null)
        {
            return newParent.AddChildField(this);
        }
        else
        {
            Parent = null;
            ParentId = null;
            return true;
        }
    }

    /// <summary>
    /// Ensure all required nested objects are initialized after deserialization
    /// This is necessary because JSON deserialization bypasses constructors
    /// </summary>
    public void EnsureInitialized()
    {
        // Initialize core properties if null
        Database ??= new FieldDatabase();
        Text ??= new TextResource();
        FieldType ??= new FieldType();
        Options ??= Array.Empty<Option>();
        ValidatingFields ??= Array.Empty<ValidatingField>();
        SpeciesAutoCompleteFields ??= Array.Empty<SpeciesAutoCompleteField>();
        ConditionalRules ??= Array.Empty<ConditionalRule>();
        ChildFields ??= new List<FormField>();
        DependentFields ??= new List<string>();

        // Ensure TextResource nested properties are initialized
        if (Text != null)
        {
            Text.Description ??= new TextClass();
            Text.Help ??= new TextClass();
            Text.Placeholder ??= new TextClass();
            Text.Label ??= new TextClass();
        }
    }
    #endregion

    #region Validation Methods
    /// <summary>
    /// Check if this field type can have children
    /// </summary>
    [JsonIgnore]
    public bool CanHaveChildren => FieldType.Type switch
    {
        "Section" or "Group" or "Panel" or "Container" => true,
        "Conditional" or "ConditionalGroup" => true,
        "DropDownList" or "RadioButtonList" => RelationshipType == ParentChildRelationshipType.Cascade,
        "Modal" or "Table" => true,
        _ => false
    };

    /// <summary>
    /// Validate the current field hierarchy
    /// </summary>
    public FieldHierarchyValidationResult ValidateHierarchy()
    {
        var result = new FieldHierarchyValidationResult();

        // Check circular references
        if (HasCircularReference())
        {
            result.Errors.Add($"Circular reference detected in field '{Id}'");
        }

        // Check parent-child type compatibility
        if (Parent != null && !IsValidParentChildRelationship())
        {
            result.Warnings.Add($"Field '{Id}' has incompatible relationship type '{RelationshipType}' with parent '{Parent.Id}' of type '{Parent.FieldType.Type}'");
        }

        // Check maximum depth
        if (HierarchicalLevel > 10) // Configurable max depth
        {
            result.Warnings.Add($"Field '{Id}' is at depth {HierarchicalLevel} which may be too deep");
        }

        // Validate children recursively
        foreach (var child in ChildFields)
        {
            var childResult = child.ValidateHierarchy();
            result.Errors.AddRange(childResult.Errors);
            result.Warnings.AddRange(childResult.Warnings);
        }

        return result;
    }

    /// <summary>
    /// Enhanced visibility check considering parent-child relationships
    /// </summary>
    public bool ShouldBeVisibleEnhanced(Dictionary<string, object>? formData = null)
    {
        // Base visibility check
        if (!ShouldBeVisible(formData)) return false;

        // Check parent visibility
        if (Parent != null && !Parent.ShouldBeVisibleEnhanced(formData))
        {
            return false;
        }

        // Check conditional relationships
        if (RelationshipType == ParentChildRelationshipType.ConditionalShow && Parent != null)
        {
            return ShouldShowBasedOnParentValue(formData);
        }
        else if (RelationshipType == ParentChildRelationshipType.ConditionalHide && Parent != null)
        {
            return !ShouldHideBasedOnParentValue(formData);
        }

        return true;
    }

    /// <summary>
    /// Enhanced validation that considers hierarchy
    /// </summary>
    public ValidationResult ValidateFieldEnhanced(object? value, Dictionary<string, object>? formData = null)
    {
        var result = new ValidationResult();

        // Skip validation if not visible
        if (!ShouldBeVisibleEnhanced(formData))
        {
            return result;
        }

        // Basic field validation
        var baseRules = GetValidationRules();
        foreach (var rule in baseRules)
        {
            // Implement validation logic based on rules
            if (rule == "required" && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                result.AddError($"Field '{Text.Description.EN ?? Id}' is required");
            }
        }

        // Relationship-specific validation
        if (RelationshipType == ParentChildRelationshipType.Validation && Parent != null)
        {
            var parentValue = formData?.GetValueOrDefault(Parent.Id);
            if (!ValidateBasedOnParentValue(value, parentValue))
            {
                result.AddError($"Field '{Id}' validation failed based on parent field '{Parent.Id}'");
            }
        }

        return result;
    }
    #endregion

    #region Methods
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return $"Order: {Order} | Id: {Id} | Type: {FieldType.Type} | Text: {Text.Description.EN}";
    }

    /// <summary>
    /// Get display label with language fallback chain
    /// ONLY uses field.Text.Label (no Description fallback)
    /// If Label missing in requested language ? try English fallback
    /// If no Label exists ? display "[No Label]"
    /// </summary>
    /// <param name="language">Language code (EN or FR)</param>
    /// <returns>Display label text</returns>
    public string GetDisplayLabel(string language = "EN")
    {
        var label = language == "FR" ? Text?.Label?.FR : Text?.Label?.EN;
        if (!string.IsNullOrWhiteSpace(label)) return label;
        
        // Try English fallback for French
        if (language == "FR" && !string.IsNullOrWhiteSpace(Text?.Label?.EN))
            return Text.Label.EN;
        
        return "[No Label]";
    }

    /// <summary>
    /// Validate field labels for completeness
    /// </summary>
    /// <returns>Validation result with errors and warnings</returns>
    public FieldLabelValidationResult ValidateLabels()
    {
        var result = new FieldLabelValidationResult { FieldId = Id };

        // Only validate user-input fields (not structural/informational fields)
        var userInputTypes = new[] 
        { 
            "TextBox", "TextArea", "NumberTextBox", "EmailTextBox", 
            "TelephoneTextBox", "URLTextBox", "DateBox", "DateRangePicker",
            "DropDownList", "RadioButtonList", "CheckBoxList", "FileUpload"
        };

        if (userInputTypes.Contains(FieldType?.Type))
        {
            // English label is required for user-input fields
            if (string.IsNullOrWhiteSpace(Text?.Label?.EN))
            {
                result.Errors.Add($"Field '{Id}': English label is required for user-input fields");
            }

            // French label is optional but warn if missing
            if (string.IsNullOrWhiteSpace(Text?.Label?.FR))
            {
                result.Warnings.Add($"Field '{Id}': French label is missing (optional)");
            }
        }

        return result;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int CompareTo(FormField? other)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (other == null) return 1;
        return (Order ?? 0).CompareTo(other.Order ?? 0);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool Equals(FormField? other)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return other != null && Id == other.Id;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override bool Equals(object? obj)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return Equals(obj as FormField);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override int GetHashCode()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Check if field should be visible based on conditions
    /// </summary>
    public bool ShouldBeVisible(Dictionary<string, object>? formData = null)
    {
        if (!IsActive || !IsVisible) return false;
        
        // Add conditional visibility logic here if needed
        return true;
    }

    /// <summary>
    /// Get validation rules for this field
    /// </summary>
    public List<string> GetValidationRules()
    {
        var rules = new List<string>();
        
        if (IsRequired) rules.Add("required");
        if (MaximumLength.HasValue) rules.Add($"maxlength:{MaximumLength}");
        if (MinimumLength.HasValue) rules.Add($"minlength:{MinimumLength}");
        
        return rules;
    }
    #endregion

    #region Private Helper Methods
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int CalculateHierarchicalLevel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        int level = 0;
        var current = Parent;
        while (current != null)
        {
            level++;
            current = current.Parent;
        }
        return level;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string CalculateHierarchicalPath()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var path = new List<string>();
        var current = this;
        
        while (current != null)
        {
            path.Insert(0, current.Id);
            current = current.Parent;
        }
        
        return string.Join(".", path);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool WouldCreateCircularReference(FormField potentialChild)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        // Check if this field is a descendant of the potential child
        return potentialChild.GetAllDescendants().Any(d => d.Id == Id);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasCircularReference()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var visited = new HashSet<string>();
        var current = this;

        while (current != null)
        {
            if (visited.Contains(current.Id))
                return true;
            
            visited.Add(current.Id);
            current = current.Parent;
        }

        return false;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValidParentChildRelationship()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (Parent == null) return true;

        return (Parent.FieldType.Type, RelationshipType) switch
        {
            ("Section" or "Group" or "Panel", ParentChildRelationshipType.GroupContainer) => true,
            ("Conditional" or "ConditionalGroup", ParentChildRelationshipType.ConditionalShow or ParentChildRelationshipType.ConditionalHide) => true,
            ("DropDownList" or "RadioButtonList", ParentChildRelationshipType.Cascade) => true,
            (_, ParentChildRelationshipType.Validation) => true, // Validation can work with any parent
            _ => false
        };
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShouldShowBasedOnParentValue(Dictionary<string, object>? formData)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (Parent == null || formData == null) return true;
        
        var parentValue = formData.GetValueOrDefault(Parent.Id)?.ToString();
        
        // Check validating fields for conditional logic
        foreach (var validatingField in ValidatingFields)
        {
            if (validatingField.FieldId == Parent.Id)
            {
                return parentValue == validatingField.ExpectedValue;
            }
        }
        
        return true;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShouldHideBasedOnParentValue(Dictionary<string, object>? formData)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return ShouldShowBasedOnParentValue(formData);
    }

    private bool ValidateBasedOnParentValue(object? value, object? parentValue)
    {
        // Implement parent-based validation logic
        // This is a placeholder - implement based on your business rules
        return true;
    }
    #endregion

    #region Static Helper Methods
    /// <summary>
    /// Generate a unique field ID
    /// </summary>
    public static string GenerateFieldId()
    {
        return Guid.NewGuid().ToString("N")[..12]; // 12-character unique ID
    }
    #endregion

    #region Nested Classes
    /// <summary>
    /// Multilingual text resources for field metadata
    /// </summary>
    public class TextResource
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextResource()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            ReportColumnName = new TextClass();
            DescriptionHtml = new TextClass();
            Description = new TextClass();
            Help = new TextClass();
            HelpHtml = new TextClass();
            ErrorMessage = new TextClass();
            ErrorMessageHtml = new TextClass();
            Placeholder = new TextClass();
            Label = new TextClass();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass ReportColumnName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass DescriptionHtml { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass HelpHtml { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Help { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass ErrorMessageHtml { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass ErrorMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Placeholder { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TextClass Label { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public IDictionary<string, TextClass>? AlternateText { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
    #endregion
}

#region  Supporting Classes
/// <summary>
///  field type configuration
/// </summary>
public class FieldType
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Type { get; set; } = "TextBox";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasUIInput { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Category { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, object>? TypeSpecificProperties { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override string ToString() => Type;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
///  database mapping configuration
/// </summary>
public class FieldDatabase
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ColumnName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ColumnDataType { get; set; } = "NVARCHAR(MAX)";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsIndexed { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSearchable { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? DefaultValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
///  field validation dependency
/// </summary>
public class ValidatingField
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FieldId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ExpectedValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Operator { get; set; } = "equals";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
///  complex conditional validation rule
/// </summary>
public class ConditionalRule
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? RuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Condition { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Action { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, object>? Parameters { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
///  modal/popup form configuration
/// </summary>
public class Modal
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Modal()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Fields = Array.Empty<FormField>();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FormField[] Fields { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasAddButton { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasEditButton { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasDeleteButton { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasActions { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Title { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MaxRecords { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool AllowDuplicates { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
///  file upload configuration
/// </summary>
public class FileUploadConfiguration
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int MaxFiles { get; set; } = 1;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool AllowMultiple { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? StoragePath { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool VirusScanRequired { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
///  selection option for dropdowns, radio buttons, etc.
/// </summary>
public class Option
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Value { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? EN { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FR { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsDefault { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int Order { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsActive { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CssClass { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override bool Equals(object? obj)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (obj is not Option other) return false;
        return string.Equals(EN, other.EN, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(FR, other.FR, StringComparison.OrdinalIgnoreCase);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override int GetHashCode()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return HashCode.Combine(EN?.GetHashCode() ?? 0, FR?.GetHashCode() ?? 0);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string GetText(string language)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return language?.ToLower() switch
        {
            "fr" => FR ?? EN ?? Value ?? string.Empty,
            _ => EN ?? FR ?? Value ?? string.Empty
        };
    }
}

/// <summary>
///  species autocomplete field configuration
/// </summary>
public class SpeciesAutoCompleteField
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public SpeciesAutoCompleteField()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Name = new TextClass();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Key { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public TextClass Name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int Order { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsVisible { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? DataSource { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? DisplayFormat { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Result of field hierarchy validation
/// </summary>
public class FieldHierarchyValidationResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid => !Errors.Any();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public void AddError(string error)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Errors.Add(error);
    }
}

/// <summary>
/// Enhanced validation result with field-specific errors
/// </summary>
public class ValidationResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid => !Errors.Any();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public void AddError(string error)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Errors.Add(error);
    }
}

/// <summary>
/// Result of field label validation
/// </summary>
public class FieldLabelValidationResult
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string FieldId { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid => !Errors.Any();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
#endregion