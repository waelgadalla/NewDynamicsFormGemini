using DynamicForms.Core.V4.Schemas;
using DynamicForms.Editor.Models;
using System.Collections.Generic;
using System.Linq;

namespace DynamicForms.Editor.Services;

public class SchemaValidationService : ISchemaValidationService
{
    public IEnumerable<ValidationIssue> Validate(FormModuleSchema module)
    {
        var issues = new List<ValidationIssue>();
        
        // Check for duplicate IDs
        var duplicateIds = module.Fields
            .GroupBy(f => f.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        
        foreach (var id in duplicateIds)
        {
            issues.Add(new ValidationIssue(
                id,
                ValidationSeverity.Error,
                "Duplicate Field ID",
                $"Multiple fields have the ID '{id}'"
            ));
        }
        
        // Check for orphaned fields
        var allIds = module.Fields.Select(f => f.Id).ToHashSet();
        foreach (var field in module.Fields.Where(f => f.ParentId is not null))
        {
            if (!allIds.Contains(field.ParentId!))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Error,
                    "Orphaned Field",
                    $"Parent '{field.ParentId}' does not exist"
                ));
            }
        }
        
        // Check for circular references
        foreach (var field in module.Fields)
        {
            if (HasCircularReference(field, module))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Error,
                    "Circular Reference",
                    "Field has a circular parent reference"
                ));
            }
        }
        
        // Field-level validations
        foreach (var field in module.Fields)
        {
            issues.AddRange(ValidateField(field, module));
        }
        
        return issues;
    }
    
    public IEnumerable<ValidationIssue> ValidateField(FormFieldSchema field, FormModuleSchema module)
    {
        var issues = new List<ValidationIssue>();
        
        // Required fields should have error messages
        if (field.Validation?.IsRequired == true)
        {
            if (string.IsNullOrWhiteSpace(field.Validation.RequiredMessageEn))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Warning,
                    "Missing Error Message",
                    "Required field should have an error message"
                ));
            }
        }
        
        // Fields should have labels
        if (string.IsNullOrWhiteSpace(field.LabelEn) && field.FieldType != "Divider")
        {
            issues.Add(new ValidationIssue(
                field.Id,
                ValidationSeverity.Warning,
                "Missing Label",
                "Field has no English label"
            ));
        }
        
        // Choice fields should have options
        if (field.FieldType is "DropDown" or "RadioGroup" or "CheckboxList")
        {
            if (field.CodeSetId is null && (field.Options is null || field.Options.Length == 0))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Error,
                    "Missing Options",
                    "Choice field has no options or CodeSet"
                ));
            }
        }
        
        // TypeConfig validation
        if (field.FieldType == "AutoComplete" && field.TypeConfig is AutoCompleteConfig ac)
        {
            if (string.IsNullOrWhiteSpace(ac.DataSourceUrl))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Error,
                    "Missing Data Source",
                    "AutoComplete requires a data source URL"
                ));
            }
        }
        
        return issues;
    }
    
    private bool HasCircularReference(FormFieldSchema field, FormModuleSchema module)
    {
        var visited = new HashSet<string>();
        var current = field;
        
        while (current?.ParentId is not null)
        {
            if (!visited.Add(current.Id))
                return true;
            
            current = module.Fields.FirstOrDefault(f => f.Id == current.ParentId);
        }
        
        return false;
    }
}
