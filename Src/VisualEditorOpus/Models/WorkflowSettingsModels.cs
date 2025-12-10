namespace VisualEditorOpus.Models;

#region Main Settings

/// <summary>
/// Complete workflow settings configuration for the settings panel
/// </summary>
public record WorkflowPanelSettings
{
    // General
    public string Name { get; init; } = "Untitled Workflow";
    public string Description { get; init; } = "";
    public string Version { get; init; } = "1.0.0";
    public WorkflowStatus Status { get; init; } = WorkflowStatus.Draft;

    // Behavior
    public BehaviorSettings Behavior { get; init; } = new();

    // Triggers
    public TriggerSettings Triggers { get; init; } = new();

    // Data Integration
    public DataIntegrationSettings DataIntegration { get; init; } = new();

    // Appearance
    public AppearanceSettings Appearance { get; init; } = new();

    // Validation
    public ValidationSettings Validation { get; init; } = new();

    // Access Control
    public AccessControlSettings AccessControl { get; init; } = new();

    // Metadata
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public enum WorkflowStatus
{
    Draft,
    Published,
    Archived,
    Disabled
}

#endregion

#region Behavior Settings

/// <summary>
/// Workflow behavior configuration
/// </summary>
public record BehaviorSettings
{
    /// <summary>
    /// Allow users to navigate between steps freely
    /// </summary>
    public bool AllowStepNavigation { get; init; } = true;

    /// <summary>
    /// Show progress bar during workflow execution
    /// </summary>
    public bool ShowProgressBar { get; init; } = true;

    /// <summary>
    /// Enable automatic saving of form data
    /// </summary>
    public bool EnableAutoSave { get; init; } = true;

    /// <summary>
    /// Auto-save interval in seconds
    /// </summary>
    public int AutoSaveIntervalSeconds { get; init; } = 30;

    /// <summary>
    /// Require confirmation before leaving workflow
    /// </summary>
    public bool RequireExitConfirmation { get; init; } = true;

    /// <summary>
    /// Allow saving incomplete workflows
    /// </summary>
    public bool AllowPartialSave { get; init; } = true;

    /// <summary>
    /// Maximum time allowed to complete workflow (minutes, 0 = unlimited)
    /// </summary>
    public int TimeoutMinutes { get; init; } = 0;

    /// <summary>
    /// Show help tooltips on fields
    /// </summary>
    public bool ShowFieldHelp { get; init; } = true;
}

#endregion

#region Trigger Settings

/// <summary>
/// Workflow trigger configuration
/// </summary>
public record TriggerSettings
{
    /// <summary>
    /// How the workflow is started
    /// </summary>
    public StartTriggerType StartTrigger { get; init; } = StartTriggerType.Manual;

    /// <summary>
    /// URL path for URL-triggered workflows
    /// </summary>
    public string? TriggerUrl { get; init; }

    /// <summary>
    /// Cron expression for scheduled workflows
    /// </summary>
    public string? ScheduleCron { get; init; }

    /// <summary>
    /// Event name for event-triggered workflows
    /// </summary>
    public string? TriggerEvent { get; init; }

    /// <summary>
    /// What happens when workflow completes
    /// </summary>
    public SettingsCompletionAction OnComplete { get; init; } = SettingsCompletionAction.ShowMessage;

    /// <summary>
    /// Success message to display
    /// </summary>
    public string SuccessMessage { get; init; } = "Thank you! Your submission has been received.";

    /// <summary>
    /// URL to redirect to on completion
    /// </summary>
    public string? RedirectUrl { get; init; }

    /// <summary>
    /// Workflow ID to trigger on completion
    /// </summary>
    public string? NextWorkflowId { get; init; }

    /// <summary>
    /// What happens when workflow is cancelled
    /// </summary>
    public CancellationAction OnCancel { get; init; } = CancellationAction.ConfirmAndExit;
}

public enum StartTriggerType
{
    Manual,
    Url,
    Scheduled,
    Event,
    Api
}

public enum SettingsCompletionAction
{
    ShowMessage,
    Redirect,
    SubmitToApi,
    TriggerWorkflow,
    Custom
}

public enum CancellationAction
{
    ConfirmAndExit,
    SaveDraftAndExit,
    DiscardAndExit,
    PreventCancellation
}

#endregion

#region Data Integration Settings

/// <summary>
/// Data integration configuration
/// </summary>
public record DataIntegrationSettings
{
    /// <summary>
    /// API endpoint for form submission
    /// </summary>
    public string? SubmitEndpoint { get; init; }

    /// <summary>
    /// HTTP method for submission
    /// </summary>
    public HttpMethodType SubmitMethod { get; init; } = HttpMethodType.Post;

    /// <summary>
    /// Include workflow metadata in submission
    /// </summary>
    public bool IncludeMetadata { get; init; } = true;

    /// <summary>
    /// Enable webhook notifications
    /// </summary>
    public bool EnableWebhooks { get; init; } = false;

    /// <summary>
    /// Webhook URLs for different events
    /// </summary>
    public Dictionary<string, string> WebhookUrls { get; init; } = new();

    /// <summary>
    /// Custom headers for API requests
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; init; } = new();

    /// <summary>
    /// API endpoint for loading pre-filled data
    /// </summary>
    public string? DataSourceEndpoint { get; init; }

    /// <summary>
    /// Enable real-time data sync
    /// </summary>
    public bool EnableRealTimeSync { get; init; } = false;
}

public enum HttpMethodType
{
    Get,
    Post,
    Put,
    Patch
}

#endregion

#region Appearance Settings

/// <summary>
/// Workflow appearance configuration
/// </summary>
public record AppearanceSettings
{
    /// <summary>
    /// Primary theme color
    /// </summary>
    public string PrimaryColor { get; init; } = "#6366f1";

    /// <summary>
    /// Layout mode for displaying steps
    /// </summary>
    public LayoutMode LayoutMode { get; init; } = LayoutMode.Wizard;

    /// <summary>
    /// Show step numbers in navigation
    /// </summary>
    public bool ShowStepNumbers { get; init; } = true;

    /// <summary>
    /// Enable transition animations
    /// </summary>
    public bool AnimateTransitions { get; init; } = true;

    /// <summary>
    /// Custom CSS class to apply to workflow container
    /// </summary>
    public string? CustomCssClass { get; init; }

    /// <summary>
    /// Custom logo URL
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Hide header in runtime mode
    /// </summary>
    public bool HideHeader { get; init; } = false;

    /// <summary>
    /// Compact mode for smaller screens
    /// </summary>
    public bool CompactMode { get; init; } = false;
}

public enum LayoutMode
{
    Wizard,      // Step by step with navigation
    Accordion,   // Collapsible sections
    Tabs,        // Tabbed interface
    SinglePage   // All steps on one page
}

#endregion

#region Validation Settings

/// <summary>
/// Validation configuration
/// </summary>
public record ValidationSettings
{
    /// <summary>
    /// When to perform validation
    /// </summary>
    public ValidationMode Mode { get; init; } = ValidationMode.OnStepChange;

    /// <summary>
    /// Show validation errors inline
    /// </summary>
    public bool ShowInlineErrors { get; init; } = true;

    /// <summary>
    /// Show validation summary at top of form
    /// </summary>
    public bool ShowValidationSummary { get; init; } = false;

    /// <summary>
    /// Scroll to first error on validation failure
    /// </summary>
    public bool ScrollToFirstError { get; init; } = true;

    /// <summary>
    /// Prevent navigation on validation failure
    /// </summary>
    public bool BlockNavigationOnError { get; init; } = true;

    /// <summary>
    /// Custom validation error messages
    /// </summary>
    public Dictionary<string, string> CustomMessages { get; init; } = new();
}

public enum ValidationMode
{
    OnStepChange,   // Validate when changing steps
    OnSubmitOnly,   // Only validate on final submit
    RealTime,       // Validate as user types
    OnBlur          // Validate when field loses focus
}

#endregion

#region Access Control Settings

/// <summary>
/// Access control configuration
/// </summary>
public record AccessControlSettings
{
    /// <summary>
    /// Require user authentication
    /// </summary>
    public bool RequireAuthentication { get; init; } = false;

    /// <summary>
    /// Roles allowed to access this workflow
    /// </summary>
    public List<string> AllowedRoles { get; init; } = new();

    /// <summary>
    /// Allow anonymous/guest access
    /// </summary>
    public bool AllowAnonymous { get; init; } = true;

    /// <summary>
    /// Restrict to specific users
    /// </summary>
    public List<string> AllowedUserIds { get; init; } = new();

    /// <summary>
    /// IP address restrictions
    /// </summary>
    public List<string> AllowedIpRanges { get; init; } = new();

    /// <summary>
    /// Maximum submissions per user
    /// </summary>
    public int? MaxSubmissionsPerUser { get; init; }

    /// <summary>
    /// Require CAPTCHA
    /// </summary>
    public bool RequireCaptcha { get; init; } = false;
}

#endregion

#region Validation Status

/// <summary>
/// Workflow validation status information
/// </summary>
public record ValidationStatus
{
    public List<WorkflowValidationIssue> Issues { get; init; } = new();
    public bool IsValid => !Issues.Any(i => i.Severity == IssueSeverity.Error);
    public int ErrorCount => Issues.Count(i => i.Severity == IssueSeverity.Error);
    public int WarningCount => Issues.Count(i => i.Severity == IssueSeverity.Warning);
    public int InfoCount => Issues.Count(i => i.Severity == IssueSeverity.Info);
}

public record WorkflowValidationIssue
{
    public string Message { get; init; } = "";
    public IssueSeverity Severity { get; init; } = IssueSeverity.Warning;
    public string? NodeId { get; init; }
    public string? FieldId { get; init; }
}

public enum IssueSeverity
{
    Info,
    Warning,
    Error
}

#endregion
