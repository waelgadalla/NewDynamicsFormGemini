namespace VisualEditorOpus.Models;

/// <summary>
/// Configuration for CodeSet data source
/// </summary>
public record CodeSetSource
{
    /// <summary>
    /// Type of data source
    /// </summary>
    public CodeSetSourceType Type { get; init; } = CodeSetSourceType.Static;

    /// <summary>
    /// API endpoint URL (for API type)
    /// </summary>
    public string? ApiEndpoint { get; init; }

    /// <summary>
    /// HTTP method for API calls
    /// </summary>
    public string HttpMethod { get; init; } = "GET";

    /// <summary>
    /// Custom headers for API calls
    /// </summary>
    public Dictionary<string, string> Headers { get; init; } = new();

    /// <summary>
    /// Request body template (for POST)
    /// </summary>
    public string? RequestBody { get; init; }

    /// <summary>
    /// JSON path to extract items from response
    /// </summary>
    public string? ResponsePath { get; init; }

    /// <summary>
    /// File path (for File type)
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// How often to refresh data
    /// </summary>
    public RefreshMode RefreshMode { get; init; } = RefreshMode.OnLoad;

    /// <summary>
    /// Refresh interval in seconds (for Periodic mode)
    /// </summary>
    public int RefreshIntervalSeconds { get; init; } = 300;

    /// <summary>
    /// Whether to cache the data
    /// </summary>
    public bool EnableCaching { get; init; } = true;

    /// <summary>
    /// Cache duration in seconds
    /// </summary>
    public int CacheDurationSeconds { get; init; } = 3600;
}

/// <summary>
/// Represents a binding between a CodeSet and a form field
/// </summary>
public record CodeSetBinding
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The form field ID that uses this CodeSet
    /// </summary>
    public string FieldId { get; init; } = "";

    /// <summary>
    /// Display name of the field
    /// </summary>
    public string FieldName { get; init; } = "";

    /// <summary>
    /// Full path to the field (e.g., Step1.Section1.Field1)
    /// </summary>
    public string FieldPath { get; init; } = "";

    /// <summary>
    /// Type of field component
    /// </summary>
    public BoundFieldType FieldType { get; init; } = BoundFieldType.Dropdown;

    /// <summary>
    /// Filter expression for dependent dropdowns
    /// </summary>
    public string? FilterExpression { get; init; }

    /// <summary>
    /// Parent field for cascading dropdowns
    /// </summary>
    public string? ParentFieldId { get; init; }

    /// <summary>
    /// Whether this binding is active
    /// </summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Extended CodeSet for management UI with additional tracking
/// </summary>
public record ManagedCodeSet
{
    public required int Id { get; init; }
    public required string Code { get; init; }
    public required string NameEn { get; init; }
    public string? NameFr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? DescriptionFr { get; init; }
    public string? Category { get; init; }
    public bool IsActive { get; init; } = true;
    public float Version { get; init; } = 1.0f;
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
    public DateTime? DateUpdated { get; init; }

    /// <summary>
    /// Field name containing the code/key value
    /// </summary>
    public string CodeField { get; init; } = "code";

    /// <summary>
    /// Field name containing the display text
    /// </summary>
    public string DisplayField { get; init; } = "displayName";

    /// <summary>
    /// Optional field for descriptions/tooltips
    /// </summary>
    public string? DescriptionField { get; init; }

    /// <summary>
    /// Optional field for sort order
    /// </summary>
    public string? OrderField { get; init; }

    /// <summary>
    /// Optional field for parent code (hierarchical data)
    /// </summary>
    public string? ParentCodeField { get; init; }

    /// <summary>
    /// Data source configuration
    /// </summary>
    public CodeSetSource Source { get; init; } = new();

    /// <summary>
    /// The actual data items
    /// </summary>
    public List<ManagedCodeSetItem> Items { get; init; } = new();

    /// <summary>
    /// Fields bound to this CodeSet
    /// </summary>
    public List<CodeSetBinding> Bindings { get; init; } = new();

    /// <summary>
    /// Whether the CodeSet has been modified
    /// </summary>
    public bool IsDirty { get; init; } = false;

    /// <summary>
    /// Last refresh timestamp
    /// </summary>
    public DateTime? LastRefreshed { get; init; }
}

/// <summary>
/// Extended CodeSet item for management UI
/// </summary>
public record ManagedCodeSetItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The code/key value
    /// </summary>
    public string Code { get; init; } = "";

    /// <summary>
    /// Display text (English)
    /// </summary>
    public string DisplayNameEn { get; init; } = "";

    /// <summary>
    /// Display text (French)
    /// </summary>
    public string? DisplayNameFr { get; init; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Sort order
    /// </summary>
    public int Order { get; init; } = 0;

    /// <summary>
    /// Item status
    /// </summary>
    public CodeSetItemStatus Status { get; init; } = CodeSetItemStatus.Active;

    /// <summary>
    /// Parent code for hierarchical CodeSets
    /// </summary>
    public string? ParentCode { get; init; }

    /// <summary>
    /// Whether this item should be shown
    /// </summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>
    /// Whether this is a default/pre-selected value
    /// </summary>
    public bool IsDefault { get; init; } = false;
}

#region Enums

public enum CodeSetSourceType
{
    Static,      // Data stored locally in the form definition
    Api,         // Load from REST API
    File         // Load from file (JSON, CSV)
}

public enum RefreshMode
{
    OnDemand,    // Only refresh when explicitly requested
    OnLoad,      // Refresh when form loads
    Periodic     // Refresh at regular intervals
}

public enum BoundFieldType
{
    Dropdown,
    RadioGroup,
    CheckboxGroup,
    Autocomplete,
    ListBox,
    Combobox
}

public enum CodeSetItemStatus
{
    Active,
    Inactive,
    Deprecated
}

#endregion
