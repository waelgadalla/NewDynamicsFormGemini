using DynamicForms.Core.V4.Schemas;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

public interface ISchemaValidationService
{
    IEnumerable<ValidationIssue> Validate(FormModuleSchema module);
    IEnumerable<ValidationIssue> ValidateField(FormFieldSchema field, FormModuleSchema module);
}
