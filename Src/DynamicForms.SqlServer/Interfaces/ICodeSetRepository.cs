namespace DynamicForms.SqlServer.Interfaces;

/// <summary>
/// Repository interface for CodeSet persistence using SQL Server.
/// Stores CodeSet definitions with items as JSON in the SchemaJson column.
/// </summary>
public interface ICodeSetRepository
{
    /// <summary>
    /// Saves a CodeSet (insert if new, update if exists based on Code)
    /// </summary>
    Task<int> SaveAsync(CodeSetEntity codeSet, string? userId = null);

    /// <summary>
    /// Gets a CodeSet by its database ID
    /// </summary>
    Task<CodeSetEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Gets a CodeSet by its unique code
    /// </summary>
    Task<CodeSetEntity?> GetByCodeAsync(string code);

    /// <summary>
    /// Gets all active CodeSets (summary only, without full schema)
    /// </summary>
    Task<IEnumerable<CodeSetSummary>> GetAllAsync();

    /// <summary>
    /// Gets CodeSets by category
    /// </summary>
    Task<IEnumerable<CodeSetSummary>> GetByCategoryAsync(string category);

    /// <summary>
    /// Soft-deletes a CodeSet by ID
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a CodeSet with the given code exists
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code);

    /// <summary>
    /// Gets the next available CodeSet ID
    /// </summary>
    Task<int> GetNextIdAsync();

    /// <summary>
    /// Searches CodeSets by name, code, or description
    /// </summary>
    Task<IEnumerable<CodeSetSummary>> SearchAsync(string searchTerm);
}

/// <summary>
/// Full CodeSet entity for database storage
/// </summary>
public record CodeSetEntity
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string NameEn { get; init; }
    public string? NameFr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? DescriptionFr { get; init; }
    public string? Category { get; init; }
    public required string SchemaJson { get; init; }
    public bool IsActive { get; init; } = true;
    public float Version { get; init; } = 1.0f;
    public DateTime DateCreated { get; init; }
    public DateTime DateUpdated { get; init; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; init; }
}

/// <summary>
/// Summary of a CodeSet for list views (without full schema JSON)
/// </summary>
public record CodeSetSummary
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string NameEn { get; init; }
    public string? NameFr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? Category { get; init; }
    public int ItemCount { get; init; }
    public float Version { get; init; }
    public bool IsActive { get; init; }
    public DateTime DateCreated { get; init; }
    public DateTime DateUpdated { get; init; }
}
