using DynamicForms.Core.V4.Schemas;
using DynamicForms.SqlServer.Interfaces;

namespace VisualEditorOpus.Services;

/// <summary>
/// Service for persisting and loading form modules and workflows from the database.
/// Bridges the Visual Editor UI with the SQL Server repositories.
/// </summary>
public interface IEditorPersistenceService
{
    /// <summary>
    /// Saves the current module to the database
    /// </summary>
    Task<SaveResult> SaveModuleAsync(FormModuleSchema module);

    /// <summary>
    /// Loads a module from the database by ID
    /// </summary>
    Task<FormModuleSchema?> LoadModuleAsync(int moduleId);

    /// <summary>
    /// Gets all saved modules (summary only, not full schemas)
    /// </summary>
    Task<IEnumerable<ModuleSchemaSummary>> GetAllModulesAsync();

    /// <summary>
    /// Deletes a module from the database
    /// </summary>
    Task<bool> DeleteModuleAsync(int moduleId);

    /// <summary>
    /// Gets the next available module ID
    /// </summary>
    Task<int> GetNextModuleIdAsync();

    /// <summary>
    /// Saves the current workflow to the database
    /// </summary>
    Task<SaveResult> SaveWorkflowAsync(FormWorkflowSchema workflow);

    /// <summary>
    /// Loads a workflow from the database by ID
    /// </summary>
    Task<FormWorkflowSchema?> LoadWorkflowAsync(int workflowId);

    /// <summary>
    /// Gets all saved workflows (summary only)
    /// </summary>
    Task<IEnumerable<WorkflowSchemaSummary>> GetAllWorkflowsAsync();

    /// <summary>
    /// Deletes a workflow from the database
    /// </summary>
    Task<bool> DeleteWorkflowAsync(int workflowId);

    /// <summary>
    /// Gets the next available workflow ID
    /// </summary>
    Task<int> GetNextWorkflowIdAsync();
}

/// <summary>
/// Result of a save operation
/// </summary>
public record SaveResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int? SavedId { get; init; }

    public static SaveResult Ok(int id) => new() { Success = true, SavedId = id };
    public static SaveResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
