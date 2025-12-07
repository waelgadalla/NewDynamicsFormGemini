using DynamicForms.Core.V4.Runtime;
using DynamicForms.Core.V4.Schemas;
using DynamicForms.Core.V4.Services;
using DynamicForms.Core.V4.Enums; // Added for ConditionOperator
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DynamicForms.Core.V4.Tests;

/// <summary>
/// Comprehensive unit tests for the V4 conditional rules engine.
/// Tests single-module scenarios, cross-module references, workflow branching, and complex logic.
/// </summary>
public class ConditionEvaluatorTests
{
    private readonly IConditionEvaluator _evaluator;

    public ConditionEvaluatorTests()
    {
        _evaluator = new ConditionEvaluator(NullLogger<ConditionEvaluator>.Instance);
    }

    #region Simple Condition Tests

    [Fact]
    public void Evaluate_SimpleEquality_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            Field = "org_type",
            Operator = ConditionOperator.Equals,
            Value = "Business"
        };

        var data = new Dictionary<string, object?>
        {
            { "org_type", "Business" }
        };

        // Act
        var result = _evaluator.Evaluate(condition, data);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_NumericLessThan_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            Field = "age",
            Operator = ConditionOperator.LessThan,
            Value = 18
        };

        var data = new Dictionary<string, object?>
        {
            { "age", 16 }
        };

        // Act
        var result = _evaluator.Evaluate(condition, data);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_StringContains_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            Field = "email",
            Operator = ConditionOperator.Contains,
            Value = "@example.com"
        };

        var data = new Dictionary<string, object?>
        {
            { "email", "user@example.com" }
        };

        // Act
        var result = _evaluator.Evaluate(condition, data);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_InOperator_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            Field = "province",
            Operator = ConditionOperator.In,
            Value = new[] { "ON", "QC", "BC" }
        };

        var data = new Dictionary<string, object?>
        {
            { "province", "ON" }
        };

        // Act
        var result = _evaluator.Evaluate(condition, data);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Complex Condition Tests

    [Fact]
    public void Evaluate_AndCondition_AllTrue_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            LogicalOp = LogicalOperator.And,
            Conditions = new[]
            {
                new Condition { Field = "age", Operator = ConditionOperator.LessThan, Value = 18 },
                new Condition { Field = "province", Operator = ConditionOperator.Equals, Value = "ON" }
            }
        };

        var data = new Dictionary<string, object?>
        {
            { "age", 16 },
            { "province", "ON" }
        };

        // Act
        var result = _evaluator.Evaluate(condition, data);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_OrCondition_OneTrue_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            LogicalOp = LogicalOperator.Or,
            Conditions = new[]
            {
                new Condition { Field = "province", Operator = ConditionOperator.Equals, Value = "ON" },
                new Condition { Field = "province", Operator = ConditionOperator.Equals, Value = "QC" }
            }
        };

        var data = new Dictionary<string, object?>
        {
            { "province", "ON" }
        };

        // Act
        var result = _evaluator.Evaluate(condition, data);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_NotCondition_ReturnsTrueWhenInnerIsFalse()
    {
        // Arrange
        var condition = new Condition
        {
            LogicalOp = LogicalOperator.Not,
            Conditions = new[]
            {
                new Condition { Field = "is_student", Operator = ConditionOperator.Equals, Value = true }
            }
        };

        var data = new Dictionary<string, object?>
        {
            { "is_student", false }
        };

        // Act
        var result = _evaluator.Evaluate(condition, data);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_NestedAndOr_ReturnsTrue()
    {
        // Scenario: Age < 18 AND (Province = ON OR Province = QC)
        // Arrange
        var condition = new Condition
        {
            LogicalOp = LogicalOperator.And,
            Conditions = new[]
            {
                new Condition { Field = "age", Operator = ConditionOperator.LessThan, Value = 18 },
                new Condition
                {
                    LogicalOp = LogicalOperator.Or,
                    Conditions = new[]
                    {
                        new Condition { Field = "province", Operator = ConditionOperator.Equals, Value = "ON" },
                        new Condition { Field = "province", Operator = ConditionOperator.Equals, Value = "QC" }
                    }
                }
            }
        };

        var data = new Dictionary<string, object?>
        {
            { "age", 16 },
            { "province", "QC" }
        };

        // Act
        var result = _evaluator.Evaluate(condition, data);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Cross-Module Field Reference Tests

    [Fact]
    public void ParseFieldReference_SimpleField_ReturnsNoModule()
    {
        // Act
        var (moduleKey, fieldId) = _evaluator.ParseFieldReference("age");

        // Assert
        Assert.Null(moduleKey);
        Assert.Equal("age", fieldId);
    }

    [Fact]
    public void ParseFieldReference_ModulePrefix_ReturnsModuleAndField()
    {
        // Act
        var (moduleKey, fieldId) = _evaluator.ParseFieldReference("PersonalInfo.age");

        // Assert
        Assert.Equal("PersonalInfo", moduleKey);
        Assert.Equal("age", fieldId);
    }

    [Fact]
    public void ParseFieldReference_NumericModuleId_ReturnsModuleAndField()
    {
        // Act
        var (moduleKey, fieldId) = _evaluator.ParseFieldReference("1.applicant_age");

        // Assert
        Assert.Equal("1", moduleKey);
        Assert.Equal("applicant_age", fieldId);
    }

    [Fact]
    public void Evaluate_CrossModuleReference_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            Field = "PersonalInfo.age",  // Cross-module reference
            Operator = ConditionOperator.LessThan,
            Value = 18
        };

        var workflowData = new WorkflowFormData
        {
            Modules = new Dictionary<string, Dictionary<string, object?>>
            {
                { "PersonalInfo", new Dictionary<string, object?> { { "age", 16 } } },
                { "ContactInfo", new Dictionary<string, object?> { { "email", "test@example.com" } } }
            }
        };

        // Act
        var result = _evaluator.Evaluate(condition, workflowData);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_CrossModuleWithCurrentModuleFallback_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            Field = "age",  // No module prefix - should use CurrentModuleKey
            Operator = ConditionOperator.LessThan,
            Value = 18
        };

        var workflowData = new WorkflowFormData
        {
            CurrentModuleKey = "PersonalInfo",
            Modules = new Dictionary<string, Dictionary<string, object?>>
            {
                { "PersonalInfo", new Dictionary<string, object?> { { "age", 16 } } }
            }
        };

        // Act
        var result = _evaluator.Evaluate(condition, workflowData);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_ComplexCrossModuleCondition_ReturnsTrue()
    {
        // Scenario: Show parental consent if age < 18 in Module 1 AND province = ON in Module 2
        // Arrange
        var condition = new Condition
        {
            LogicalOp = LogicalOperator.And,
            Conditions = new[]
            {
                new Condition { Field = "1.applicant_age", Operator = ConditionOperator.LessThan, Value = 18 },
                new Condition { Field = "2.province", Operator = ConditionOperator.Equals, Value = "ON" }
            }
        };

        var workflowData = new WorkflowFormData
        {
            Modules = new Dictionary<string, Dictionary<string, object?>>
            {
                { "1", new Dictionary<string, object?> { { "applicant_age", 16 } } },
                { "2", new Dictionary<string, object?> { { "province", "ON" } } }
            }
        };

        // Act
        var result = _evaluator.Evaluate(condition, workflowData);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Workflow Rule Evaluation Tests

    [Fact]
    public void EvaluateRule_FieldLevelAction_ReturnsTriggered()
    {
        // Arrange
        var rule = new ConditionalRule
        {
            Id = "show_business_number",
            TargetFieldId = "business_number",
            Action = "show",
            Condition = new Condition
            {
                Field = "org_type",
                Operator = ConditionOperator.Equals,
                Value = "Business"
            }
        };

        var workflowData = WorkflowFormData.FromSingleModule("current", new Dictionary<string, object?>
        {
            { "org_type", "Business" }
        });

        // Act
        var result = _evaluator.EvaluateRule(rule, workflowData);

        // Assert
        Assert.True(result.IsTriggered);
        Assert.Equal("show", result.ActionToPerform);
        Assert.Equal("business_number", result.TargetFieldId);
    }

    [Fact]
    public void EvaluateRule_WorkflowLevelAction_ReturnsTriggered()
    {
        // Arrange
        var rule = new ConditionalRule
        {
            Id = "skip_financial_review",
            TargetStepNumber = 3,
            Action = "skipStep",
            Condition = new Condition
            {
                Field = "1.total_amount",
                Operator = ConditionOperator.LessThan,
                Value = 10000
            }
        };

        var workflowData = new WorkflowFormData
        {
            Modules = new Dictionary<string, Dictionary<string, object?>>
            {
                { "1", new Dictionary<string, object?> { { "total_amount", 5000 } } }
            }
        };

        // Act
        var result = _evaluator.EvaluateRule(rule, workflowData);

        // Assert
        Assert.True(result.IsTriggered);
        Assert.Equal("skipStep", result.ActionToPerform);
        Assert.Equal(3, result.TargetStepNumber);
    }

    [Fact]
    public void EvaluateRules_MultiplePriorities_ReturnsInPriorityOrder()
    {
        // Arrange
        var rules = new[]
        {
            new ConditionalRule
            {
                Id = "rule3",
                Priority = 300,
                TargetFieldId = "field1",
                Action = "show",
                Condition = new Condition { Field = "trigger", Operator = ConditionOperator.Equals, Value = true }
            },
            new ConditionalRule
            {
                Id = "rule1",
                Priority = 100,
                TargetFieldId = "field2",
                Action = "hide",
                Condition = new Condition { Field = "trigger", Operator = ConditionOperator.Equals, Value = true }
            },
            new ConditionalRule
            {
                Id = "rule2",
                Priority = 200,
                TargetFieldId = "field3",
                Action = "disable",
                Condition = new Condition { Field = "trigger", Operator = ConditionOperator.Equals, Value = true }
            }
        };

        var workflowData = WorkflowFormData.FromSingleModule("current", new Dictionary<string, object?>
        {
            { "trigger", true }
        });

        // Act
        var results = _evaluator.EvaluateRules(rules, workflowData);

        // Assert
        Assert.Equal(3, results.Length);
        Assert.Equal("rule1", results[0].Rule.Id);  // Priority 100
        Assert.Equal("rule2", results[1].Rule.Id);  // Priority 200
        Assert.Equal("rule3", results[2].Rule.Id);  // Priority 300
    }

    [Fact]
    public void EvaluateRule_InactiveRule_ReturnsNotTriggered()
    {
        // Arrange
        var rule = new ConditionalRule
        {
            Id = "inactive_rule",
            IsActive = false,
            TargetFieldId = "field1",
            Action = "show",
            Condition = new Condition { Field = "trigger", Operator = ConditionOperator.Equals, Value = true }
        };

        var workflowData = WorkflowFormData.FromSingleModule("current", new Dictionary<string, object?>
        {
            { "trigger", true }
        });

        // Act
        var result = _evaluator.EvaluateRule(rule, workflowData);

        // Assert
        Assert.False(result.IsTriggered);
    }

    #endregion

    #region Real-World Scenario Tests

    [Fact]
    public void Scenario_ParentalConsentForMinors_ReturnsTrue()
    {
        // Real scenario: Show parental consent field if applicant is under 18 AND in ON or QC
        // Arrange
        var rule = new ConditionalRule
        {
            Id = "show_parental_consent",
            TargetFieldId = "sec_parental_consent",
            Action = "show",
            Condition = new Condition
            {
                LogicalOp = LogicalOperator.And,
                Conditions = new[]
                {
                    new Condition { Field = "PersonalInfo.applicant_age", Operator = ConditionOperator.LessThan, Value = 18 },
                    new Condition
                    {
                        LogicalOp = LogicalOperator.Or,
                        Conditions = new[]
                        {
                            new Condition { Field = "PersonalInfo.province", Operator = ConditionOperator.Equals, Value = "ON" },
                            new Condition { Field = "PersonalInfo.province", Operator = ConditionOperator.Equals, Value = "QC" }
                        }
                    }
                }
            }
        };

        var workflowData = new WorkflowFormData
        {
            Modules = new Dictionary<string, Dictionary<string, object?>>
            {
                { "PersonalInfo", new Dictionary<string, object?> 
                    { 
                        { "applicant_age", 16 }, 
                        { "province", "ON" } 
                    } 
                }
            }
        };

        // Act
        var result = _evaluator.EvaluateRule(rule, workflowData);

        // Assert
        Assert.True(result.IsTriggered);
        Assert.Equal("show", result.ActionToPerform);
    }

    [Fact]
    public void Scenario_SkipFinancialReviewForSmallAmounts_ReturnsTrue()
    {
        // Real scenario: Skip financial review step if total amount < $10,000
        // Arrange
        var rule = new ConditionalRule
        {
            Id = "skip_financial_review",
            Description = "Skip financial review for amounts under $10,000",
            TargetStepNumber = 3,
            Action = "skipStep",
            Condition = new Condition
            {
                Field = "Step1.total_request_amount",
                Operator = ConditionOperator.LessThan,
                Value = 10000
            }
        };

        var workflowData = new WorkflowFormData
        {
            Modules = new Dictionary<string, Dictionary<string, object?>>
            {
                { "Step1", new Dictionary<string, object?> { { "total_request_amount", 7500 } } }
            }
        };

        // Act
        var result = _evaluator.EvaluateRule(rule, workflowData);

        // Assert
        Assert.True(result.IsTriggered);
        Assert.Equal("skipStep", result.ActionToPerform);
        Assert.Equal(3, result.TargetStepNumber);
    }

    [Fact]
    public void Scenario_CompleteWorkflowForPreApproved_ReturnsTrue()
    {
        // Real scenario: Complete workflow immediately if user is pre-approved
        // Arrange
        var rule = new ConditionalRule
        {
            Id = "complete_for_preapproved",
            Action = "completeWorkflow",
            Condition = new Condition
            {
                LogicalOp = LogicalOperator.And,
                Conditions = new[]
                {
                    new Condition { Field = "Step1.approval_status", Operator = ConditionOperator.Equals, Value = "pre_approved" },
                    new Condition { Field = "Step1.credit_score", Operator = ConditionOperator.GreaterThanOrEqual, Value = 750 }
                }
            }
        };

        var workflowData = new WorkflowFormData
        {
            Modules = new Dictionary<string, Dictionary<string, object?>>
            {
                { "Step1", new Dictionary<string, object?> 
                    { 
                        { "approval_status", "pre_approved" }, 
                        { "credit_score", 800 } 
                    } 
                }
            }
        };

        // Act
        var result = _evaluator.EvaluateRule(rule, workflowData);

        // Assert
        Assert.True(result.IsTriggered);
        Assert.Equal("completeWorkflow", result.ActionToPerform);
    }

    #endregion
}