using DynamicForms.Core.V4.Schemas;
using DynamicForms.Editor.Models;
using DynamicForms.Editor.Components.Workflow;

namespace DynamicForms.Editor.Services;

public interface IWorkflowGraphService
{
    FormWorkflowSchema CompileGraphToSchema(
        FormWorkflowSchema originalSchema, 
        List<WorkflowVisualNode> nodes, 
        List<WorkflowVisualConnection> connections);
}
