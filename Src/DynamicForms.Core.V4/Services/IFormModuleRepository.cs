using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Core.V4.Services;

/// <summary>
/// Repository interface for persisting and retrieving form module schemas.
/// Implementations handle database, file system, or other storage mechanisms.
/// </summary>
public interface IFormModuleRepository
{
    /// <summary>
    /// Saves a module schema to storage
    /// </summary>
    /// <param name="schema">The module schema to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if save was successful, false otherwise</returns>
    Task<bool> SaveAsync(FormModuleSchema schema, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a module schema by ID
    /// </summary>
    /// <param name="moduleId">The module identifier</param>
    /// <param name="opportunityId">Optional opportunity context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The module schema, or null if not found</returns>
    Task<FormModuleSchema?> GetByIdAsync(
        int moduleId,
        int? opportunityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple module schemas by IDs
    /// </summary>
    /// <param name="moduleIds">Array of module identifiers</param>
    /// <param name="opportunityId">Optional opportunity context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of module schemas (may be empty)</returns>
    Task<FormModuleSchema[]> GetByIdsAsync(
        int[] moduleIds,
        int? opportunityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a module schema from storage
    /// </summary>
    /// <param name="moduleId">The module identifier</param>
    /// <param name="opportunityId">Optional opportunity context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteAsync(
        int moduleId,
        int? opportunityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a module exists in storage
    /// </summary>
    /// <param name="moduleId">The module identifier</param>
    /// <param name="opportunityId">Optional opportunity context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if module exists, false otherwise</returns>
    Task<bool> ExistsAsync(
        int moduleId,
        int? opportunityId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Metadata about a module version
/// </summary>
/// <param name="ModuleId">Module identifier</param>
/// <param name="OpportunityId">Opportunity context</param>
/// <param name="Version">Schema version</param>
/// <param name="DateCreated">Creation timestamp</param>
/// <param name="CreatedBy">Creator identifier</param>
/// <param name="IsCurrent">Whether this is the current version</param>
/// <param name="TotalFields">Number of fields in the module</param>
/// <param name="TitleEn">English title</param>
public record ModuleVersionInfo(
    int ModuleId,
    int? OpportunityId,
    float Version,
    DateTime DateCreated,
    string? CreatedBy,
    bool IsCurrent,
    int TotalFields,
    string TitleEn
);

/// <summary>
/// Search criteria for finding modules
/// </summary>
/// <param name="OpportunityId">Filter by opportunity</param>
/// <param name="SearchText">Text search in title/description</param>
/// <param name="CreatedAfter">Filter by creation date</param>
/// <param name="CreatedBefore">Filter by creation date</param>
/// <param name="PageSize">Results per page</param>
/// <param name="PageNumber">Page number (1-based)</param>
public record ModuleSearchCriteria(
    int? OpportunityId = null,
    string? SearchText = null,
    DateTime? CreatedAfter = null,
    DateTime? CreatedBefore = null,
    int PageSize = 20,
    int PageNumber = 1
);

/// <summary>
/// Search results for module queries
/// </summary>
/// <param name="Modules">Array of matching modules</param>
/// <param name="TotalCount">Total number of matches</param>
/// <param name="PageNumber">Current page number</param>
/// <param name="PageSize">Results per page</param>
public record ModuleSearchResult(
    FormModuleSchema[] Modules,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there are more pages
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
};
