using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.SqlServer.Interfaces;

/// <summary>
/// Repository interface for persisting and retrieving FormModuleSchema.
/// Designed for the Visual Editor - stores complete module schemas as JSON.
/// </summary>
public interface IModuleSchemaRepository
{
    /// <summary>
    /// Saves a module schema to storage. Creates new or updates existing based on ModuleId.
    /// </summary>
    /// <param name="schema">The module schema to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if save was successful</returns>
    Task<bool> SaveAsync(FormModuleSchema schema, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a module schema by its ModuleId
    /// </summary>
    /// <param name="moduleId">The module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The module schema, or null if not found</returns>
    Task<FormModuleSchema?> GetByIdAsync(int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active module schemas
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all active module schemas</returns>
    Task<IEnumerable<ModuleSchemaSummary>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple module schemas by their IDs
    /// </summary>
    /// <param name="moduleIds">Array of module identifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of module schemas</returns>
    Task<FormModuleSchema[]> GetByIdsAsync(int[] moduleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a module schema
    /// </summary>
    /// <param name="moduleId">The module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a module exists
    /// </summary>
    /// <param name="moduleId">The module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if module exists and is active</returns>
    Task<bool> ExistsAsync(int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next available module ID
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Next available module ID</returns>
    Task<int> GetNextModuleIdAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary information for module listing (avoids deserializing full schema)
/// </summary>
public record ModuleSchemaSummary
{
    public required int ModuleId { get; init; }
    public required string TitleEn { get; init; }
    public string? TitleFr { get; init; }
    public string? DescriptionEn { get; init; }
    public float Version { get; init; }
    public DateTime DateCreated { get; init; }
    public DateTime DateUpdated { get; init; }
    public string? CreatedBy { get; init; }
    public int FieldCount { get; init; }
}
