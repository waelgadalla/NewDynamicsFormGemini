using DynamicForms.Core.V4.Runtime;
using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Enums;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Core.V4.Services;

/// <summary>
/// Default implementation of condition evaluator with support for cross-module references and workflow logic.
/// Evaluates simple and complex conditions using recursive logic.
/// </summary>
public class ConditionEvaluator : IConditionEvaluator
{
    private readonly ILogger<ConditionEvaluator> _logger;

public ConditionEvaluator(ILogger<ConditionEvaluator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
  public bool Evaluate(Condition condition, WorkflowFormData workflowData)
    {
        try
      {
            // Simple condition (leaf node)
            if (condition.IsSimpleCondition)
        {
      return EvaluateSimpleCondition(condition, workflowData);
  }

            // Complex condition (branch node)
 if (condition.IsComplexCondition)
            {
     return EvaluateComplexCondition(condition, workflowData);
   }

            _logger.LogWarning("Invalid condition: neither simple nor complex");
   return false;
        }
   catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating condition");
          return false;
        }
    }

    /// <inheritdoc/>
    public bool Evaluate(Condition condition, Dictionary<string, object?> fieldData, string? moduleKey = null)
    {
        // Convert to WorkflowFormData for unified evaluation
        var workflowData = WorkflowFormData.FromSingleModule(moduleKey ?? "current", fieldData);
        return Evaluate(condition, workflowData);
    }

    /// <inheritdoc/>
    public RuleEvaluationResult EvaluateRule(ConditionalRule rule, WorkflowFormData workflowData)
    {
 try
    {
            if (!rule.IsActive)
            {
        return new RuleEvaluationResult
   {
         Rule = rule,
         IsTriggered = false,
          ErrorMessage = "Rule is inactive"
     };
    }

    bool isTriggered = Evaluate(rule.Condition, workflowData);

            return new RuleEvaluationResult
        {
     Rule = rule,
       IsTriggered = isTriggered
     };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating rule {RuleId}", rule.Id);
      return new RuleEvaluationResult
            {
   Rule = rule,
       IsTriggered = false,
           ErrorMessage = ex.Message
            };
        }
    }

    /// <inheritdoc/>
    public RuleEvaluationResult[] EvaluateRules(ConditionalRule[] rules, WorkflowFormData workflowData)
    {
        return rules
       .Where(r => r.IsActive)
      .OrderBy(r => r.Priority)
    .Select(rule => EvaluateRule(rule, workflowData))
      .Where(result => result.IsTriggered)
            .ToArray();
    }

    /// <inheritdoc/>
  public (string? moduleKey, string fieldId) ParseFieldReference(string fieldReference)
    {
        if (string.IsNullOrWhiteSpace(fieldReference))
        {
throw new ArgumentException("Field reference cannot be null or empty", nameof(fieldReference));
        }

   // Check for module prefix (e.g., "ModuleKey.FieldId" or "1.age")
        var dotIndex = fieldReference.IndexOf('.');
      if (dotIndex > 0 && dotIndex < fieldReference.Length - 1)
      {
       var moduleKey = fieldReference.Substring(0, dotIndex);
        var fieldId = fieldReference.Substring(dotIndex + 1);
return (moduleKey, fieldId);
        }

      // No module prefix - field in current module
        return (null, fieldReference);
    }

    // ===== Private Helper Methods =====

    private bool EvaluateSimpleCondition(Condition condition, WorkflowFormData workflowData)
    {
        if (condition.Field == null || !condition.Operator.HasValue)
        {
            _logger.LogWarning("Invalid simple condition: missing Field or Operator");
            return false;
        }

        // Parse field reference to get module and field ID
        var (moduleKey, fieldId) = ParseFieldReference(condition.Field);

        // Get field value from workflow data
        var fieldValue = workflowData.GetFieldValue(moduleKey, fieldId);

        // Evaluate operator
        return EvaluateOperator(fieldValue, condition.Operator.Value, condition.Value);
    }

    private bool EvaluateComplexCondition(Condition condition, WorkflowFormData workflowData)
    {
        if (condition.LogicalOp == null || condition.Conditions == null || condition.Conditions.Length == 0)
        {
            _logger.LogWarning("Invalid complex condition: missing LogicalOp or Conditions");
            return false;
        }

        return condition.LogicalOp switch
        {
       LogicalOperator.And => condition.Conditions.All(c => Evaluate(c, workflowData)),
   LogicalOperator.Or => condition.Conditions.Any(c => Evaluate(c, workflowData)),
            LogicalOperator.Not => condition.Conditions.Length > 0 && !Evaluate(condition.Conditions[0], workflowData),
          _ => false
        };
    }

    private bool EvaluateOperator(object? fieldValue, ConditionOperator op, object? expectedValue)
    {
        return op switch
        {
            ConditionOperator.Equals => AreEqual(fieldValue, expectedValue),
            ConditionOperator.NotEquals => !AreEqual(fieldValue, expectedValue),
            
            ConditionOperator.LessThan => Compare(fieldValue, expectedValue) < 0,
            ConditionOperator.LessThanOrEqual => Compare(fieldValue, expectedValue) <= 0,
            ConditionOperator.GreaterThan => Compare(fieldValue, expectedValue) > 0,
            ConditionOperator.GreaterThanOrEqual => Compare(fieldValue, expectedValue) >= 0,
            
            ConditionOperator.In => IsIn(fieldValue, expectedValue),
            ConditionOperator.NotIn => !IsIn(fieldValue, expectedValue),
            
            ConditionOperator.Contains => Contains(fieldValue, expectedValue),
            ConditionOperator.NotContains => !Contains(fieldValue, expectedValue),
            ConditionOperator.StartsWith => StartsWith(fieldValue, expectedValue),
            ConditionOperator.EndsWith => EndsWith(fieldValue, expectedValue),
            
            ConditionOperator.IsEmpty => IsEmpty(fieldValue),
            ConditionOperator.IsNotEmpty => !IsEmpty(fieldValue),
            ConditionOperator.IsNull => fieldValue == null,
            ConditionOperator.IsNotNull => fieldValue != null,
            
            _ => throw new NotSupportedException($"Operator '{op}' is not supported")
        };
    }

    private bool AreEqual(object? fieldValue, object? expectedValue)
    {
        // Handle null cases
 if (fieldValue == null && expectedValue == null) return true;
        if (fieldValue == null || expectedValue == null) return false;

  // Handle string comparison (case-insensitive)
        if (fieldValue is string strField && expectedValue is string strExpected)
        {
            return string.Equals(strField, strExpected, StringComparison.OrdinalIgnoreCase);
        }

        // Handle numeric comparison
      if (IsNumeric(fieldValue) && IsNumeric(expectedValue))
        {
 return Convert.ToDouble(fieldValue) == Convert.ToDouble(expectedValue);
        }

      // Handle boolean comparison
     if (fieldValue is bool boolField && expectedValue is bool boolExpected)
        {
      return boolField == boolExpected;
        }

        // Default: use Equals
      return fieldValue.Equals(expectedValue);
    }

    private int Compare(object? fieldValue, object? expectedValue)
    {
        if (fieldValue == null || expectedValue == null)
        {
  throw new InvalidOperationException("Cannot compare null values");
  }

      // Numeric comparison
   if (IsNumeric(fieldValue) && IsNumeric(expectedValue))
        {
     var fieldNum = Convert.ToDouble(fieldValue);
            var expectedNum = Convert.ToDouble(expectedValue);
            return fieldNum.CompareTo(expectedNum);
        }

     // String comparison
 if (fieldValue is string strField && expectedValue is string strExpected)
        {
      return string.Compare(strField, strExpected, StringComparison.OrdinalIgnoreCase);
     }

   // DateTime comparison
 if (fieldValue is DateTime dtField && expectedValue is DateTime dtExpected)
  {
 return dtField.CompareTo(dtExpected);
        }

        throw new InvalidOperationException($"Cannot compare types {fieldValue.GetType().Name} and {expectedValue.GetType().Name}");
 }

    private bool IsIn(object? fieldValue, object? expectedValue)
    {
 if (fieldValue == null) return false;

 // expectedValue should be an array or collection
        if (expectedValue is System.Collections.IEnumerable enumerable and not string)
        {
 foreach (var item in enumerable)
       {
       if (AreEqual(fieldValue, item))
{
      return true;
         }
       }
  }

        return false;
    }

    private bool Contains(object? fieldValue, object? expectedValue)
    {
        if (fieldValue == null || expectedValue == null) return false;

  // String contains
 if (fieldValue is string strField && expectedValue is string strExpected)
        {
         return strField.Contains(strExpected, StringComparison.OrdinalIgnoreCase);
        }

        // Collection contains
        if (fieldValue is System.Collections.IEnumerable enumerable and not string)
        {
   foreach (var item in enumerable)
{
         if (AreEqual(item, expectedValue))
   {
         return true;
   }
         }
        }

 return false;
    }

    private bool StartsWith(object? fieldValue, object? expectedValue)
    {
        if (fieldValue is string strField && expectedValue is string strExpected)
        {
      return strField.StartsWith(strExpected, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private bool EndsWith(object? fieldValue, object? expectedValue)
    {
        if (fieldValue is string strField && expectedValue is string strExpected)
        {
            return strField.EndsWith(strExpected, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private bool IsEmpty(object? fieldValue)
    {
        if (fieldValue == null) return true;

      if (fieldValue is string str)
        {
   return string.IsNullOrWhiteSpace(str);
}

        if (fieldValue is System.Collections.ICollection collection)
{
         return collection.Count == 0;
        }

        return false;
    }

    private bool IsNumeric(object? value)
{
        return value is sbyte or byte or short or ushort or int or uint
     or long or ulong or float or double or decimal;
    }
}
