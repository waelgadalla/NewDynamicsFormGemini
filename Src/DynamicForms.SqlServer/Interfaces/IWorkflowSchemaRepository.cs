using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.SqlServer.Interfaces;

/// <summary>
/// Repository interface for persisting and retrieving FormWorkflowSchema.
/// Designed for the Visual Editor - stores complete workflow schemas as JSON.
/// </summary>
public interface IWorkflowSchemaRepository
{
    /// <summary>
    /// Saves a workflow schema to storage. Creates new or updates existing based on WorkflowId.
    /// </summary>
    /// <param name="schema">The workflow schema to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if save was successful</returns>
    Task<bool> SaveAsync(FormWorkflowSchema schema, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a workflow schema by its Id
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow schema, or null if not found</returns>
    Task<FormWorkflowSchema?> GetByIdAsync(int workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active workflow schemas
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all active workflow schemas</returns>
    Task<IEnumerable<WorkflowSchemaSummary>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a workflow schema
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a workflow exists
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if workflow exists and is active</returns>
    Task<bool> ExistsAsync(int workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next available workflow ID
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Next available workflow ID</returns>
    Task<int> GetNextWorkflowIdAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary information for workflow listing (avoids deserializing full schema)
/// </summary>
public record WorkflowSchemaSummary
{
    public required int WorkflowId { get; init; }
    public required string TitleEn { get; init; }
    public string? TitleFr { get; init; }
    public string? DescriptionEn { get; init; }
    public float Version { get; init; }
    public DateTime DateCreated { get; init; }
    public DateTime DateUpdated { get; init; }
    public string? CreatedBy { get; init; }
    public int ModuleCount { get; init; }
}
