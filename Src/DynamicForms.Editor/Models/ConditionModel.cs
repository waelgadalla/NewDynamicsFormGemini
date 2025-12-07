using DynamicForms.Core.V4.Enums;
using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Models;

public class ConditionModel
{
    // Simple Condition
    public string? Field { get; set; }
    public ConditionOperator Operator { get; set; }
    public object? Value { get; set; }

    // Complex Condition
    public LogicalOperator? LogicalOp { get; set; }
    public List<ConditionModel> Conditions { get; set; } = new();

    public bool IsComplex => LogicalOp.HasValue;

    public Condition ToSchema()
    {
        if (IsComplex)
        {
            return new Condition
            {
                LogicalOp = LogicalOp,
                Conditions = Conditions.Select(c => c.ToSchema()).ToArray()
            };
        }
        else
        {
            return new Condition
            {
                Field = Field,
                Operator = Operator,
                Value = Value
            };
        }
    }

    public static ConditionModel FromSchema(Condition condition)
    {
        if (condition.IsComplexCondition)
        {
            return new ConditionModel
            {
                LogicalOp = condition.LogicalOp,
                Conditions = condition.Conditions?.Select(FromSchema).ToList() ?? new()
            };
        }
        else
        {
            return new ConditionModel
            {
                Field = condition.Field,
                Operator = condition.Operator ?? ConditionOperator.Equals,
                Value = condition.Value
            };
        }
    }
}
