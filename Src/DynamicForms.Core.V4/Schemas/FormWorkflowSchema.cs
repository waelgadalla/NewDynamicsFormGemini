using System.Text.Json;

namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Immutable schema definition for a multi-module workflow.
/// Defines the sequence and navigation rules for multiple form modules.
/// Serializable to/from JSON for storage and transmission.
/// </summary>
public record FormWorkflowSchema
{
    /// <summary>
    /// Unique identifier for the workflow
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Associated opportunity ID (null if not opportunity-specific)
    /// </summary>
    public int? OpportunityId { get; init; }

    /// <summary>
    /// Schema version for this workflow (supports evolution)
    /// </summary>
    public float Version { get; init; } = 1.0f;

    /// <summary>
    /// UTC timestamp when workflow was created
    /// </summary>
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// English title for the workflow
    /// </summary>
    public required string TitleEn { get; init; }

    /// <summary>
    /// French title for the workflow
    /// </summary>
    public string? TitleFr { get; init; }

    /// <summary>
    /// English description of the workflow
    /// </summary>
    public string? DescriptionEn { get; init; }

    /// <summary>
    /// French description of the workflow
    /// </summary>
    public string? DescriptionFr { get; init; }

    /// <summary>
    /// Array of module IDs that comprise this workflow, in sequence order
    /// </summary>
    public int[] ModuleIds { get; init; } = Array.Empty<int>();

    /// <summary>
    /// Workflow-level conditional rules that control step navigation and branching.
    /// These rules can skip steps, navigate to specific steps, or complete the workflow based on conditions.
    /// Uses cross-module field references to evaluate data from multiple modules.
    /// </summary>
    public ConditionalRule[]? WorkflowRules { get; init; }

    /// <summary>
    /// Navigation settings for the workflow
    /// </summary>
    public WorkflowNavigation Navigation { get; init; } = new();

    /// <summary>
    /// General settings for workflow behavior
    /// </summary>
    public WorkflowSettings Settings { get; init; } = new();

    /// <summary>
    /// Extended properties for custom data not covered by the schema
    /// </summary>
    public JsonElement? ExtendedProperties { get; init; }
}

/// <summary>
/// Navigation settings for workflow step progression
/// </summary>
/// <param name="AllowStepJumping">Whether users can jump to non-sequential steps (default: false)</param>
/// <param name="ShowProgress">Whether to display progress indicator (default: true)</param>
/// <param name="ShowStepNumbers">Whether to show step numbers in navigation (default: true)</param>
public record WorkflowNavigation(
    bool AllowStepJumping = false,
    bool ShowProgress = true,
    bool ShowStepNumbers = true
);

/// <summary>
/// General behavior settings for workflows
/// </summary>
/// <param name="RequireAllModulesComplete">Whether all modules must be completed (default: true)</param>
/// <param name="AllowModuleSkipping">Whether users can skip optional modules (default: false)</param>
/// <param name="AutoSaveIntervalSeconds">Interval for automatic save in seconds (default: 300 = 5 minutes)</param>
public record WorkflowSettings(
    bool RequireAllModulesComplete = true,
    bool AllowModuleSkipping = false,
    int AutoSaveIntervalSeconds = 300
);
