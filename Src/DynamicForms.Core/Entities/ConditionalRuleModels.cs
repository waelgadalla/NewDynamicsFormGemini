namespace DynamicForms.Core.Entities;

/// <summary>
/// Conditional rule for visual rules builder
/// </summary>
public class ConditionalRuleV4
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "New Rule";
    public bool Enabled { get; set; } = true;
    public List<RuleCondition> Conditions { get; set; } = new();
    public List<RuleAction> Actions { get; set; } = new();
}

/// <summary>
/// Condition in a conditional rule
/// </summary>
public class RuleCondition
{
    public string FieldId { get; set; } = string.Empty;
    public RuleOperator Operator { get; set; }
    public string? Value { get; set; }
    public LogicOperator Logic { get; set; } = LogicOperator.AND;
}

/// <summary>
/// Action to perform when rule conditions are met
/// </summary>
public class RuleAction
{
    public ActionType Type { get; set; }
    public List<string> TargetFieldIds { get; set; } = new();
    public string? Value { get; set; }
}

/// <summary>
/// Operators for rule conditions
/// </summary>
public enum RuleOperator
{
    Equals,
    NotEquals,
    Contains,
    IsEmpty,
    IsNotEmpty,
    GreaterThan,
    LessThan,
    IsTrue,
    IsFalse
}

/// <summary>
/// Logic operators for combining conditions
/// </summary>
public enum LogicOperator
{
    AND,
    OR
}

/// <summary>
/// Action types for conditional rules
/// </summary>
public enum ActionType
{
    Show,
    Hide,
    Require,
    Unrequire,
    SetValue,
    ClearValue,
    Enable,
    Disable
}
