using DynamicForms.Core.V4.Schemas;
using DynamicForms.Editor.Models;
using System.Collections.Generic;
using System.Linq;

namespace DynamicForms.Editor.Services;

public interface ISchemaValidationService
{
    IEnumerable<ValidationIssue> Validate(FormModuleSchema module);
    IEnumerable<ValidationIssue> ValidateField(FormFieldSchema field, FormModuleSchema module);
}
