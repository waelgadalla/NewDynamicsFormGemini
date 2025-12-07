using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Models;

public class FieldSetValidationModel
{
    public string Type { get; set; } = "AtLeastOne"; // AtLeastOne, AllOrNone, MutuallyExclusive
    public List<string> FieldIds { get; set; } = new();
    public string? ErrorMessageEn { get; set; }
    public string? ErrorMessageFr { get; set; }

    public static FieldSetValidationModel FromSchema(FieldSetValidation rule)
    {
        return new FieldSetValidationModel
        {
            Type = rule.Type,
            FieldIds = rule.FieldIds.ToList(),
            ErrorMessageEn = rule.ErrorMessageEn,
            ErrorMessageFr = rule.ErrorMessageFr
        };
    }

    public FieldSetValidation ToSchema()
    {
        return new FieldSetValidation
        {
            Type = Type,
            FieldIds = FieldIds.ToArray(),
            ErrorMessageEn = ErrorMessageEn,
            ErrorMessageFr = ErrorMessageFr
        };
    }
}
