using DynamicForms.Core.Entities;
using DynamicForms.Core.Enums;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Core.Services.Enhanced;

/// <summary>
///  service for managing enhanced field relationships and hierarchies
/// Uses only * classes without any inheritance dependencies
/// </summary>
public class FieldHierarchyManagerService
{
    private readonly ILogger<FieldHierarchyManagerService> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FieldHierarchyManagerService(ILogger<FieldHierarchyManagerService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        _logger = logger;
    }

    #region Hierarchy Building and Analysis

    /// <summary>
    /// Build field hierarchy from a flat collection of fields
    /// </summary>
    public HierarchyBuildResult BuildFieldHierarchy(IEnumerable<FormField> fields)
    {
        var result = new HierarchyBuildResult();
        var fieldList = fields.ToList();

        try
        {
            _logger.LogDebug("Building hierarchy for {FieldCount} fields", fieldList.Count);

            // Step 1: Clear existing relationships
            foreach (var field in fieldList)
            {
                field.Parent = null;
                field.ChildFields.Clear();
            }

            // Step 2: Create field lookup for performance
            var fieldLookup = fieldList.ToDictionary(f => f.Id, f => f);

            // Step 3: Build parent-child relationships
            var circularRefResult = BuildParentChildRelationships(fieldList, fieldLookup);
            result.Errors.AddRange(circularRefResult.Errors);
            result.Warnings.AddRange(circularRefResult.Warnings);
            result.FixedCount += circularRefResult.FixedCount;

            // Step 4: Validate relationship types
            var typeValidationResults = ValidateRelationshipTypes(fieldList);
            result.Warnings.AddRange(typeValidationResults.Warnings);

            // Step 5: Build final hierarchy structure
            result.RootFields = fieldList.Where(f => f.Parent == null).OrderBy(f => f.Order ?? 0).ToList();
            result.MaxDepth = fieldList.Count > 0 ? fieldList.Max(f => f.HierarchicalLevel) : 0;
            result.TotalFields = fieldList.Count;

            result.IsSuccessful = !result.Errors.Any();
            
            _logger.LogInformation("Hierarchy build completed. Success: {IsSuccessful}, Root fields: {RootCount}, Max depth: {MaxDepth}", 
                result.IsSuccessful, result.RootFields.Count, result.MaxDepth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hierarchy building");
            result.Errors.Add($"Hierarchy building failed: {ex.Message}");
            result.IsSuccessful = false;
        }

        return result;
    }

    /// <summary>
    /// Analyze field hierarchy for optimization and reporting
    /// </summary>
    public HierarchyAnalysisResult AnalyzeHierarchy(IEnumerable<FormField> fields)
    {
        var result = new HierarchyAnalysisResult();
        var fieldList = fields.ToList();

        try
        {
            // Basic statistics
            result.TotalFields = fieldList.Count;
            result.RootFields = fieldList.Count(f => f.Parent == null);
            result.MaxDepth = fieldList.Count > 0 ? fieldList.Max(f => f.HierarchicalLevel) : 0;
            result.AverageDepth = fieldList.Count > 0 ? fieldList.Average(f => f.HierarchicalLevel) : 0;

            // Relationship type analysis
            result.RelationshipTypeDistribution = fieldList
                .GroupBy(f => f.RelationshipType)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            // Field type analysis
            result.FieldTypeDistribution = fieldList
                .GroupBy(f => f.FieldType.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            // Complexity analysis
            result.ComplexityScore = CalculateComplexityScore(fieldList);
            result.CircularReferences = DetectCircularReferences(fieldList);
            result.OrphanedFields = DetectOrphanedFields(fieldList);

            // Performance metrics
            result.LargestFieldGroup = fieldList
                .Where(f => f.ChildFields.Any())
                .Max(f => f.ChildFields.Count);

            result.DeepestNesting = result.MaxDepth;

            _logger.LogDebug("Hierarchy analysis completed. Fields: {Total}, Complexity: {Complexity}, Issues: {Issues}",
                result.TotalFields, result.ComplexityScore, result.CircularReferences.Count + result.OrphanedFields.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hierarchy analysis");
            result.Errors.Add($"Analysis failed: {ex.Message}");
        }

        return result;
    }

    #endregion

    #region Field Movement and Management

    /// <summary>
    /// Safely move a field to a new parent with validation
    /// </summary>
    public FieldMoveResult MoveFieldToParent(
        FormField field, 
        FormField? newParent, 
        IEnumerable<FormField> allFields)
    {
        var result = new FieldMoveResult();

        try
        {
            // Validate the move operation
            var validationResult = ValidateFieldMove(field, newParent, allFields);
            if (!validationResult.IsValid)
            {
                result.Errors.AddRange(validationResult.Errors);
                return result;
            }

            var oldParent = field.Parent;
            
            // Perform the move
            var moveSuccessful = field.MoveTo(newParent);
            
            if (moveSuccessful)
            {
                result.IsSuccessful = true;
                result.OldParentId = oldParent?.Id;
                result.NewParentId = newParent?.Id;
                
                _logger.LogDebug("Successfully moved field {FieldId} from parent {OldParent} to {NewParent}", 
                    field.Id, oldParent?.Id ?? "none", newParent?.Id ?? "none");
            }
            else
            {
                result.Errors.Add("Move operation failed for unknown reasons");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving field {FieldId}", field.Id);
            result.Errors.Add($"Move failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Create a field hierarchy from a template
    /// </summary>
    public FormField CreateFieldHierarchy(FieldHierarchyTemplate template)
    {
        var rootField = new FormField
        {
            Id = template.FieldId ?? $"section_{Guid.NewGuid():N}"[..12],
            FieldType = new DynamicForms.Core.Entities.FieldType { Type = template.FieldType ?? "Section" },
            Text = new FormField.TextResource
            {
                Description = new TextClass { EN = template.Label ?? "Section" }
            },
            Order = template.Order ?? 1,
            RelationshipType = template.RelationshipType
        };

        // Add child fields
        int childOrder = 1;
        foreach (var childTemplate in template.ChildFields)
        {
            var childField = CreateFieldFromTemplate(childTemplate, childOrder++);
            rootField.AddChildField(childField);
        }

        return rootField;
    }

    /// <summary>
    /// Clone a field hierarchy with new IDs
    /// </summary>
    public FormField CloneFieldHierarchy(
        FormField sourceField, 
        CloneOptions? options = null)
    {
        options ??= new CloneOptions();
        var idMapping = new Dictionary<string, string>();

        var clonedField = CloneFieldRecursive(sourceField, idMapping, options);
        
        // Update any internal references if needed
        UpdateClonedReferences(clonedField, idMapping);

        return clonedField;
    }

    #endregion

    #region Validation and Relationship Management

    /// <summary>
    /// Validate all relationships in a field collection
    /// </summary>
    public RelationshipValidationResult ValidateAllRelationships(IEnumerable<FormField> fields)
    {
        var result = new RelationshipValidationResult();
        var fieldList = fields.ToList();

        foreach (var field in fieldList)
        {
            ValidateFieldRelationships(field, fieldList, result);
        }

        return result;
    }

    /// <summary>
    /// Fix common relationship issues automatically
    /// </summary>
    public IssueFixResult FixCommonIssues(IEnumerable<FormField> fields)
    {
        var result = new IssueFixResult();
        var fieldList = fields.ToList();

        try
        {
            // Fix orphaned parent references
            var orphanCount = FixOrphanedParentReferences(fieldList);
            if (orphanCount > 0)
            {
                result.FixesApplied.Add($"Cleared {orphanCount} orphaned parent references");
            }

            // Fix invalid relationship types
            var relationshipCount = FixInvalidRelationshipTypes(fieldList);
            if (relationshipCount > 0)
            {
                result.FixesApplied.Add($"Fixed {relationshipCount} invalid relationship types");
            }

            // Fix circular references
            var circularCount = FixCircularReferences(fieldList);
            if (circularCount > 0)
            {
                result.FixesApplied.Add($"Fixed {circularCount} circular references");
            }

            result.IsSuccessful = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing issues in field collection");
            result.Errors.Add($"Fix operation failed: {ex.Message}");
        }

        return result;
    }

    #endregion

    #region Private Helper Methods

    private CircularReferenceResult BuildParentChildRelationships(
        List<FormField> fields, 
        Dictionary<string, FormField> fieldLookup)
    {
        var result = new CircularReferenceResult();

        foreach (var field in fields)
        {
            if (!string.IsNullOrEmpty(field.ParentId))
            {
                if (fieldLookup.TryGetValue(field.ParentId, out var parent))
                {
                    try
                    {
                        // Check for circular reference before adding
                        if (WouldCreateCircularReference(parent, field))
                        {
                            result.Errors.Add($"Circular reference detected: field '{field.Id}' cannot be child of '{parent.Id}'");
                            field.ParentId = null; // Clear the problematic reference
                            result.FixedCount++;
                        }
                        else
                        {
                            parent.AddChildField(field);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"Could not establish relationship between {parent.Id} and {field.Id}: {ex.Message}");
                        field.ParentId = null;
                    }
                }
                else
                {
                    result.Warnings.Add($"Field '{field.Id}' references non-existent parent '{field.ParentId}'");
                    field.ParentId = null; // Clear invalid reference
                    result.FixedCount++;
                }
            }
        }

        return result;
    }

    private bool WouldCreateCircularReference(FormField parent, FormField child)
    {
        // Check if parent is a descendant of child
        return child.GetAllDescendants().Any(d => d.Id == parent.Id);
    }

    private RelationshipTypeValidationResult ValidateRelationshipTypes(List<FormField> fields)
    {
        var result = new RelationshipTypeValidationResult();

        foreach (var field in fields.Where(f => f.Parent != null))
        {
            var parent = field.Parent!;
            var isValidRelationship = (parent.FieldType.Type, field.RelationshipType) switch
            {
                ("Section" or "Group" or "Panel", ParentChildRelationshipType.GroupContainer) => true,
                ("Conditional" or "ConditionalGroup", 
                    ParentChildRelationshipType.ConditionalShow or ParentChildRelationshipType.ConditionalHide) => true,
                ("DropDownList" or "RadioButtonList", ParentChildRelationshipType.Cascade) => true,
                (_, ParentChildRelationshipType.Validation) => true, // Validation can work with any parent type
                _ => false
            };

            if (!isValidRelationship)
            {
                result.Warnings.Add($"Field '{field.Id}' has potentially incompatible relationship type " +
                                   $"'{field.RelationshipType}' with parent field type '{parent.FieldType.Type}'");
            }
        }

        return result;
    }

    private FieldMoveValidationResult ValidateFieldMove(
        FormField field, 
        FormField? newParent, 
        IEnumerable<FormField> allFields)
    {
        var result = new FieldMoveValidationResult();

        // Check if new parent can have children
        if (newParent != null && !newParent.CanHaveChildren)
        {
            result.Errors.Add($"Parent field '{newParent.Id}' of type '{newParent.FieldType.Type}' cannot have children");
            return result;
        }

        // Check for circular reference
        if (newParent != null && newParent.GetAllAncestors().Any(a => a.Id == field.Id))
        {
            result.Errors.Add($"Moving field '{field.Id}' to parent '{newParent.Id}' would create a circular reference");
            return result;
        }

        // Check hierarchy depth limits
        if (newParent != null && newParent.HierarchicalLevel + 1 + GetMaxDescendantDepth(field) > 10)
        {
            result.Warnings.Add($"Moving field '{field.Id}' would create a very deep hierarchy (>10 levels)");
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    private FormField CreateFieldFromTemplate(FieldHierarchyTemplate template, int order)
    {
        var field = new FormField
        {
            Id = template.FieldId ?? FormField.GenerateFieldId(),
            FieldType = new DynamicForms.Core.Entities.FieldType { Type = template.FieldType ?? "TextBox" },
            Text = new FormField.TextResource
            {
                Description = new TextClass { EN = template.Label ?? "Field" },
                Help = new TextClass { EN = template.HelpText }
            },
            Order = order,
            IsRequired = template.IsRequired,
            IsVisible = template.IsVisible,
            RelationshipType = template.RelationshipType
        };

        return field;
    }

    private FormField CloneFieldRecursive(
        FormField source, 
        Dictionary<string, string> idMapping, 
        CloneOptions options)
    {
        var newId = options.GenerateNewIds ? FormField.GenerateFieldId() : source.Id;
        idMapping[source.Id] = newId;

        var cloned = new FormField
        {
            Id = newId,
            ModuleId = source.ModuleId,
            OpportunityId = source.OpportunityId,
            Order = source.Order,
            FieldType = source.FieldType,
            Version = source.Version,
            IsActive = source.IsActive,
            IsVisible = source.IsVisible,
            IsVisibleInEditor = source.IsVisibleInEditor,
            IsVisibleInDisplay = source.IsVisibleInDisplay,
            ReadOnly = source.ReadOnly,
            IsRequired = source.IsRequired,
            MaximumLength = source.MaximumLength,
            MinimumLength = source.MinimumLength,
            IsConditionallyRequired = source.IsConditionallyRequired,
            ValidatingFields = source.ValidatingFields,
            ConditionalRules = source.ConditionalRules,
            CodeSetId = source.CodeSetId,
            Options = source.Options,
            WidthClass = source.WidthClass,
            CssClasses = source.CssClasses,
            Text = source.Text, // Deep clone would be better
            Database = source.Database, // Deep clone would be better
            RelationshipType = source.RelationshipType,
            DependentFields = source.DependentFields.ToList()
        };

        // Clone children recursively
        foreach (var child in source.ChildFields)
        {
            var clonedChild = CloneFieldRecursive(child, idMapping, options);
            cloned.AddChildField(clonedChild);
        }

        return cloned;
    }

    private void UpdateClonedReferences(FormField field, Dictionary<string, string> idMapping)
    {
        // Update dependent field references
        for (int i = 0; i < field.DependentFields.Count; i++)
        {
            if (idMapping.ContainsKey(field.DependentFields[i]))
            {
                field.DependentFields[i] = idMapping[field.DependentFields[i]];
            }
        }

        // Update validating field references
        foreach (var validatingField in field.ValidatingFields)
        {
            if (!string.IsNullOrEmpty(validatingField.FieldId) && idMapping.ContainsKey(validatingField.FieldId))
            {
                validatingField.FieldId = idMapping[validatingField.FieldId];
            }
        }

        // Recursively update children
        foreach (var child in field.ChildFields)
        {
            UpdateClonedReferences(child, idMapping);
        }
    }

    private void ValidateFieldRelationships(
        FormField field, 
        List<FormField> allFields, 
        RelationshipValidationResult result)
    {
        // Check for circular references
        if (field.HasCircularReference())
        {
            result.Errors.Add($"Circular reference detected in field hierarchy starting at '{field.Id}'");
        }

        // Check parent existence and validity
        if (!string.IsNullOrEmpty(field.ParentId))
        {
            var parent = allFields.FirstOrDefault(f => f.Id == field.ParentId);
            if (parent == null)
            {
                result.Warnings.Add($"Field '{field.Id}' references non-existent parent '{field.ParentId}'");
            }
            else if (!parent.CanHaveChildren)
            {
                result.Errors.Add($"Field '{field.Id}' has parent '{parent.Id}' that cannot have children");
            }
            else if (field.Parent != parent)
            {
                result.Warnings.Add($"Field '{field.Id}' has mismatched parent reference (ParentId vs Parent object)");
            }
        }
    }

    private double CalculateComplexityScore(List<FormField> fields)
    {
        var totalFields = fields.Count;
        var totalRelationships = fields.Count(f => f.Parent != null);
        var conditionalFields = fields.Count(f => f.IsConditionallyRequired || f.ConditionalRules.Any());
        var maxDepth = fields.Count > 0 ? fields.Max(f => f.HierarchicalLevel) : 0;

        var baseComplexity = totalFields * 1.0;
        var relationshipComplexity = totalRelationships * 2.0;
        var conditionalComplexity = conditionalFields * 3.0;
        var depthComplexity = maxDepth * maxDepth * 1.5;

        return baseComplexity + relationshipComplexity + conditionalComplexity + depthComplexity;
    }

    private List<string> DetectCircularReferences(List<FormField> fields)
    {
        return fields.Where(f => f.HasCircularReference()).Select(f => f.Id).ToList();
    }

    private List<string> DetectOrphanedFields(List<FormField> fields)
    {
        var fieldIds = fields.Select(f => f.Id).ToHashSet();
        return fields
            .Where(f => !string.IsNullOrEmpty(f.ParentId) && !fieldIds.Contains(f.ParentId))
            .Select(f => f.Id)
            .ToList();
    }

    private int GetMaxDescendantDepth(FormField field)
    {
        if (!field.ChildFields.Any()) return 0;
        return 1 + field.ChildFields.Max(GetMaxDescendantDepth);
    }

    private int FixOrphanedParentReferences(List<FormField> fields)
    {
        var fieldIds = fields.Select(f => f.Id).ToHashSet();
        var fixCount = 0;

        foreach (var field in fields)
        {
            if (!string.IsNullOrEmpty(field.ParentId) && !fieldIds.Contains(field.ParentId))
            {
                field.ParentId = null;
                field.Parent = null;
                fixCount++;
            }
        }

        return fixCount;
    }

    private int FixInvalidRelationshipTypes(List<FormField> fields)
    {
        var fixCount = 0;

        foreach (var field in fields.Where(f => f.Parent != null))
        {
            var suggestedType = SuggestRelationshipType(field.Parent!, field);
            if (suggestedType != field.RelationshipType)
            {
                field.RelationshipType = suggestedType;
                fixCount++;
            }
        }

        return fixCount;
    }

    private int FixCircularReferences(List<FormField> fields)
    {
        var fixCount = 0;

        foreach (var field in fields.Where(f => f.HasCircularReference()))
        {
            // Break the circular reference by removing the parent relationship
            if (field.Parent != null)
            {
                field.Parent.RemoveChildField(field.Id);
                fixCount++;
            }
        }

        return fixCount;
    }

    private ParentChildRelationshipType SuggestRelationshipType(
        FormField parent, 
        FormField child)
    {
        return (parent.FieldType.Type, child.FieldType.Type) switch
        {
            ("Section" or "Group" or "Panel", _) => ParentChildRelationshipType.GroupContainer,
            ("Conditional", _) => ParentChildRelationshipType.ConditionalShow,
            ("DropDownList" or "RadioButtonList", "DropDownList" or "RadioButtonList") => ParentChildRelationshipType.Cascade,
            (_, _) when child.IsConditionallyRequired => ParentChildRelationshipType.Validation,
            _ => ParentChildRelationshipType.GroupContainer
        };
    }

    #endregion
}

#region Supporting Classes for  Service

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class HierarchyBuildResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSuccessful { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
    public int FixedCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class HierarchyAnalysisResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
    public double AverageDepth { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double ComplexityScore { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, int> RelationshipTypeDistribution { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, int> FieldTypeDistribution { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> CircularReferences { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> OrphanedFields { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int LargestFieldGroup { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int DeepestNesting { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class FieldMoveResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSuccessful { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? OldParentId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? NewParentId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class CircularReferenceResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int FixedCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class RelationshipTypeValidationResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class FieldMoveValidationResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class RelationshipValidationResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasErrors => Errors.Any();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasWarnings => Warnings.Any();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Warnings { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class IssueFixResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSuccessful { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool HasErrors => Errors.Any();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> FixesApplied { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class FieldHierarchyTemplate
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FieldId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FieldType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Label { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? HelpText { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? Order { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsRequired { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsVisible { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ParentChildRelationshipType RelationshipType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<FieldHierarchyTemplate> ChildFields { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class CloneOptions
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool GenerateNewIds { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool PreserveOrder { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool CloneRelationshipTypes { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#endregion