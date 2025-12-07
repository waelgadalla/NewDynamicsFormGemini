using DynamicForms.Core.V4.Schemas;
using DynamicForms.Editor.Models;
using DynamicForms.Core.V4.Enums;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace DynamicForms.Editor.Services;

public class WorkflowGraphService : IWorkflowGraphService
{
    public FormWorkflowSchema CompileGraphToSchema(
        FormWorkflowSchema originalSchema,
        List<WorkflowVisualNode> nodes,
        List<WorkflowVisualConnection> connections)
    {
        // 1. Extract Module IDs from nodes (All modules present in the graph)
        // We attempt to order them by following the graph from Start, but simple extraction is safe for now.
        var moduleIds = nodes
            .Where(n => n.Type == "module")
            .Select(n => int.Parse(n.Id.Replace("module_", "")))
            .ToArray();

        // 2. Build WorkflowRules from Decision Nodes
        var newRules = new List<ConditionalRule>();
        var decisionNodes = nodes.Where(n => n.Type == "decision");

        foreach (var decisionNode in decisionNodes)
        {
            // Find outgoing connections from this decision node
            var outgoing = connections.Where(c => c.SourceNodeId == decisionNode.Id).ToList();
            
            // We need to find the target of the "True" path.
            // For MVP, we assume the FIRST connection is the True path.
            var trueConnection = outgoing.FirstOrDefault();
            
            if (trueConnection != null)
            {
                var targetNode = nodes.FirstOrDefault(n => n.Id == trueConnection.TargetNodeId);
                if (targetNode != null)
                {
                    // Extract Condition from Node Data
                    Condition? conditionSchema = null;
                    if (decisionNode.Data.TryGetValue("Condition", out var conditionJson))
                    {
                         try
                         {
                             var model = JsonSerializer.Deserialize<ConditionModel>(conditionJson.ToString()!);
                             conditionSchema = model?.ToSchema();
                         }
                         catch { /* Ignore invalid JSON */ }
                    }

                    // Fallback if no condition set
                    if (conditionSchema == null)
                    {
                        conditionSchema = new Condition { Field = "IsActive", Operator = ConditionOperator.Equals, Value = "True" };
                    }

                    var rule = new ConditionalRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        Condition = conditionSchema,
                        Action = "goToStep" // Default action, will be updated below
                    };

                    // Determine Target and update Action/TargetModuleKey
                    if (targetNode.Type == "module")
                    {
                        rule = rule with { TargetModuleKey = targetNode.Id.Replace("module_", "") };
                    }
                    else if (targetNode.Type == "end")
                    {
                        rule = rule with { Action = "completeWorkflow" };
                    }

                    newRules.Add(rule);
                }
            }
        }

        // 3. Return updated schema
        return originalSchema with
        {
            ModuleIds = moduleIds,
            WorkflowRules = newRules.ToArray()
        };
    }
}
