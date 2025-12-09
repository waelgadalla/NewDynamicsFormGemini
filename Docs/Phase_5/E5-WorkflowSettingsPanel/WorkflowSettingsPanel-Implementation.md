# E.5 WorkflowSettingsPanel - Implementation Guide

## Overview

The WorkflowSettingsPanel provides a comprehensive interface for configuring workflow-level settings including general properties, behavior options, triggers, data integration, appearance, validation, and access control. It appears as a collapsible sidebar panel.

## Component Architecture

```
WorkflowSettingsPanel/
├── WorkflowSettingsPanel.razor        # Main panel container
├── WorkflowSettingsPanel.razor.css    # Scoped styles
├── SettingsSection.razor              # Collapsible section component
├── Sections/
│   ├── GeneralSettings.razor          # Name, description, version
│   ├── BehaviorSettings.razor         # Navigation, auto-save options
│   ├── TriggerSettings.razor          # Start/completion triggers
│   ├── DataIntegrationSettings.razor  # API endpoints, webhooks
│   ├── AppearanceSettings.razor       # Theme, layout options
│   ├── ValidationSettings.razor       # Validation mode, status
│   └── AccessControlSettings.razor    # Authentication, roles
└── Models/
    └── WorkflowSettings.cs            # Settings model
```

## Data Models

### WorkflowSettings.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

/// <summary>
/// Complete workflow settings configuration
/// </summary>
public record WorkflowSettings
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
```

### BehaviorSettings.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

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
```

### TriggerSettings.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

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
    public CompletionAction OnComplete { get; init; } = CompletionAction.ShowMessage;

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

public enum CompletionAction
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
```

### DataIntegrationSettings.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

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
```

### AppearanceSettings.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

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
```

### ValidationSettings.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

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
```

### AccessControlSettings.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

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
```

### ValidationStatus.cs

```csharp
namespace VisualEditorOpus.Components.Workflow.Models;

/// <summary>
/// Workflow validation status information
/// </summary>
public record ValidationStatus
{
    public List<ValidationIssue> Issues { get; init; } = new();
    public bool IsValid => !Issues.Any(i => i.Severity == IssueSeverity.Error);
    public int ErrorCount => Issues.Count(i => i.Severity == IssueSeverity.Error);
    public int WarningCount => Issues.Count(i => i.Severity == IssueSeverity.Warning);
    public int InfoCount => Issues.Count(i => i.Severity == IssueSeverity.Info);
}

public record ValidationIssue
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
```

## Blazor Components

### WorkflowSettingsPanel.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="settings-panel @(IsCollapsed ? "collapsed" : "")">
    <div class="panel-header">
        <div class="panel-title">
            <i class="bi bi-gear"></i>
            <span>Workflow Settings</span>
        </div>
        <button class="btn-icon" @onclick="ToggleCollapse" title="@(IsCollapsed ? "Expand" : "Collapse")">
            <i class="bi bi-chevron-@(IsCollapsed ? "left" : "right")"></i>
        </button>
    </div>

    @if (!IsCollapsed)
    {
        <div class="panel-content">
            <SettingsSection Title="General" Icon="bi-sliders" IsExpanded="true">
                <GeneralSettings Settings="Settings"
                                 OnSettingsChanged="HandleSettingsChanged" />
            </SettingsSection>

            <SettingsSection Title="Behavior" Icon="bi-lightning">
                <BehaviorSettings Settings="Settings.Behavior"
                                  OnSettingsChanged="HandleBehaviorChanged" />
            </SettingsSection>

            <SettingsSection Title="Triggers" Icon="bi-play-circle">
                <TriggerSettings Settings="Settings.Triggers"
                                 OnSettingsChanged="HandleTriggersChanged" />
            </SettingsSection>

            <SettingsSection Title="Data Integration" Icon="bi-database">
                <DataIntegrationSettings Settings="Settings.DataIntegration"
                                         OnSettingsChanged="HandleDataIntegrationChanged" />
            </SettingsSection>

            <SettingsSection Title="Appearance" Icon="bi-palette">
                <AppearanceSettings Settings="Settings.Appearance"
                                    OnSettingsChanged="HandleAppearanceChanged" />
            </SettingsSection>

            <SettingsSection Title="Validation" Icon="bi-shield-check">
                <ValidationSettingsSection Settings="Settings.Validation"
                                           Status="ValidationStatus"
                                           OnSettingsChanged="HandleValidationChanged" />
            </SettingsSection>

            <SettingsSection Title="Access Control" Icon="bi-lock" IsExpanded="false">
                <AccessControlSettings Settings="Settings.AccessControl"
                                       OnSettingsChanged="HandleAccessControlChanged" />
            </SettingsSection>
        </div>

        <div class="panel-footer">
            <div class="footer-actions">
                <button class="btn btn-secondary" @onclick="ResetSettings">
                    <i class="bi bi-arrow-counterclockwise"></i>
                    Reset
                </button>
                <button class="btn btn-primary" @onclick="ApplySettings">
                    <i class="bi bi-check-lg"></i>
                    Apply
                </button>
            </div>
        </div>
    }
</div>

@code {
    [Parameter] public WorkflowSettings Settings { get; set; } = new();
    [Parameter] public EventCallback<WorkflowSettings> SettingsChanged { get; set; }
    [Parameter] public ValidationStatus ValidationStatus { get; set; } = new();
    [Parameter] public bool IsCollapsed { get; set; } = false;
    [Parameter] public EventCallback<bool> IsCollapsedChanged { get; set; }
    [Parameter] public EventCallback OnApply { get; set; }
    [Parameter] public EventCallback OnReset { get; set; }

    private WorkflowSettings _originalSettings = new();

    protected override void OnParametersSet()
    {
        _originalSettings = Settings;
    }

    private void ToggleCollapse()
    {
        IsCollapsed = !IsCollapsed;
        IsCollapsedChanged.InvokeAsync(IsCollapsed);
    }

    private async Task HandleSettingsChanged(WorkflowSettings settings)
    {
        Settings = settings;
        await SettingsChanged.InvokeAsync(settings);
    }

    private async Task HandleBehaviorChanged(BehaviorSettings behavior)
    {
        Settings = Settings with { Behavior = behavior };
        await SettingsChanged.InvokeAsync(Settings);
    }

    private async Task HandleTriggersChanged(TriggerSettings triggers)
    {
        Settings = Settings with { Triggers = triggers };
        await SettingsChanged.InvokeAsync(Settings);
    }

    private async Task HandleDataIntegrationChanged(DataIntegrationSettings dataIntegration)
    {
        Settings = Settings with { DataIntegration = dataIntegration };
        await SettingsChanged.InvokeAsync(Settings);
    }

    private async Task HandleAppearanceChanged(AppearanceSettings appearance)
    {
        Settings = Settings with { Appearance = appearance };
        await SettingsChanged.InvokeAsync(Settings);
    }

    private async Task HandleValidationChanged(ValidationSettings validation)
    {
        Settings = Settings with { Validation = validation };
        await SettingsChanged.InvokeAsync(Settings);
    }

    private async Task HandleAccessControlChanged(AccessControlSettings accessControl)
    {
        Settings = Settings with { AccessControl = accessControl };
        await SettingsChanged.InvokeAsync(Settings);
    }

    private async Task ApplySettings()
    {
        await OnApply.InvokeAsync();
    }

    private async Task ResetSettings()
    {
        Settings = _originalSettings;
        await SettingsChanged.InvokeAsync(Settings);
        await OnReset.InvokeAsync();
    }
}
```

### WorkflowSettingsPanel.razor.css

```css
/* Panel Container */
.settings-panel {
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-color, #e5e7eb);
    border-radius: 12px;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    transition: width 0.2s ease;
    width: 360px;
}

.settings-panel.collapsed {
    width: 48px;
}

/* Panel Header */
.panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 1rem 1.25rem;
    border-bottom: 1px solid var(--border-color, #e5e7eb);
    background: var(--bg-secondary, #f9fafb);
}

.collapsed .panel-header {
    padding: 1rem 0.75rem;
    justify-content: center;
}

.panel-title {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.9375rem;
    font-weight: 600;
    color: var(--text-primary, #1f2937);
}

.panel-title i {
    color: var(--primary, #6366f1);
}

.collapsed .panel-title span {
    display: none;
}

.btn-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 2rem;
    height: 2rem;
    background: transparent;
    border: none;
    border-radius: 6px;
    color: var(--text-secondary, #6b7280);
    cursor: pointer;
    transition: all 0.15s ease;
}

.btn-icon:hover {
    background: var(--bg-tertiary, #f3f4f6);
    color: var(--text-primary, #1f2937);
}

.collapsed .btn-icon {
    display: none;
}

/* Panel Content */
.panel-content {
    flex: 1;
    overflow-y: auto;
    padding: 0.5rem 0;
}

/* Panel Footer */
.panel-footer {
    padding: 1rem 1.25rem;
    border-top: 1px solid var(--border-color, #e5e7eb);
    background: var(--bg-secondary, #f9fafb);
}

.footer-actions {
    display: flex;
    gap: 0.75rem;
}

.footer-actions .btn {
    flex: 1;
    justify-content: center;
}

/* Buttons */
.btn {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.625rem 1.25rem;
    border: none;
    border-radius: 8px;
    font-size: 0.875rem;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.15s ease;
}

.btn-primary {
    background: var(--primary, #6366f1);
    color: white;
}

.btn-primary:hover {
    background: var(--primary-hover, #4f46e5);
}

.btn-secondary {
    background: var(--bg-primary, #ffffff);
    color: var(--text-primary, #1f2937);
    border: 1px solid var(--border-color, #e5e7eb);
}

.btn-secondary:hover {
    background: var(--bg-secondary, #f3f4f6);
}

/* Dark Mode */
:global([data-theme="dark"]) .settings-panel {
    background: var(--bg-primary, #1f2937);
    border-color: var(--border-color, #4b5563);
}

:global([data-theme="dark"]) .panel-header,
:global([data-theme="dark"]) .panel-footer {
    background: var(--bg-tertiary, #111827);
    border-color: var(--border-color, #4b5563);
}
```

### SettingsSection.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="settings-section">
    <div class="section-header" @onclick="Toggle">
        <div class="section-title">
            <i class="bi @Icon"></i>
            <span>@Title</span>
        </div>
        <i class="bi bi-chevron-down section-toggle @(_isExpanded ? "" : "collapsed")"></i>
    </div>
    <div class="section-content @(_isExpanded ? "" : "collapsed")">
        @ChildContent
    </div>
</div>

@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Icon { get; set; } = "bi-gear";
    [Parameter] public bool IsExpanded { get; set; } = false;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool _isExpanded;

    protected override void OnInitialized()
    {
        _isExpanded = IsExpanded;
    }

    private void Toggle() => _isExpanded = !_isExpanded;
}
```

### SettingsSection.razor.css

```css
.settings-section {
    border-bottom: 1px solid var(--border-color, #e5e7eb);
}

.settings-section:last-child {
    border-bottom: none;
}

.section-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.875rem 1.25rem;
    cursor: pointer;
    transition: background 0.15s ease;
}

.section-header:hover {
    background: var(--bg-secondary, #f9fafb);
}

.section-title {
    display: flex;
    align-items: center;
    gap: 0.625rem;
    font-size: 0.8125rem;
    font-weight: 600;
    color: var(--text-primary, #1f2937);
    text-transform: uppercase;
    letter-spacing: 0.025em;
}

.section-title i {
    font-size: 1rem;
    color: var(--text-secondary, #6b7280);
}

.section-toggle {
    color: var(--text-muted, #9ca3af);
    font-size: 0.875rem;
    transition: transform 0.2s ease;
}

.section-toggle.collapsed {
    transform: rotate(-90deg);
}

.section-content {
    padding: 0.75rem 1.25rem 1.25rem;
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.section-content.collapsed {
    display: none;
}

/* Dark Mode */
:global([data-theme="dark"]) .section-header:hover {
    background: var(--bg-secondary, #374151);
}
```

### GeneralSettings.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="settings-fields">
    <div class="form-group">
        <label class="form-label">Workflow Name</label>
        <input type="text"
               class="form-input"
               value="@Settings.Name"
               @oninput="e => UpdateName(e.Value?.ToString())"
               placeholder="Enter workflow name" />
    </div>

    <div class="form-group">
        <label class="form-label">Description</label>
        <textarea class="form-textarea"
                  @oninput="e => UpdateDescription(e.Value?.ToString())"
                  placeholder="Describe this workflow...">@Settings.Description</textarea>
    </div>

    <div class="form-group">
        <label class="form-label">Version</label>
        <div class="version-group">
            <input type="text"
                   class="form-input"
                   value="@Settings.Version"
                   @oninput="e => UpdateVersion(e.Value?.ToString())" />
            <span class="status-badge @GetStatusClass()">
                <i class="bi @GetStatusIcon()"></i>
                @Settings.Status
            </span>
        </div>
    </div>
</div>

@code {
    [Parameter] public WorkflowSettings Settings { get; set; } = new();
    [Parameter] public EventCallback<WorkflowSettings> OnSettingsChanged { get; set; }

    private void UpdateName(string? value)
    {
        if (value != null)
            OnSettingsChanged.InvokeAsync(Settings with { Name = value });
    }

    private void UpdateDescription(string? value)
    {
        OnSettingsChanged.InvokeAsync(Settings with { Description = value ?? "" });
    }

    private void UpdateVersion(string? value)
    {
        if (value != null)
            OnSettingsChanged.InvokeAsync(Settings with { Version = value });
    }

    private string GetStatusClass() => Settings.Status switch
    {
        WorkflowStatus.Draft => "draft",
        WorkflowStatus.Published => "published",
        WorkflowStatus.Archived => "archived",
        WorkflowStatus.Disabled => "disabled",
        _ => ""
    };

    private string GetStatusIcon() => Settings.Status switch
    {
        WorkflowStatus.Draft => "bi-pencil",
        WorkflowStatus.Published => "bi-check-circle",
        WorkflowStatus.Archived => "bi-archive",
        WorkflowStatus.Disabled => "bi-x-circle",
        _ => "bi-circle"
    };
}
```

### BehaviorSettings.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="settings-fields">
    <div class="toggle-group">
        <span class="toggle-label">Allow Step Navigation</span>
        <label class="toggle-switch">
            <input type="checkbox"
                   checked="@Settings.AllowStepNavigation"
                   @onchange="e => Update(s => s with { AllowStepNavigation = (bool)e.Value! })" />
            <span class="toggle-slider"></span>
        </label>
    </div>

    <div class="toggle-group">
        <span class="toggle-label">Show Progress Bar</span>
        <label class="toggle-switch">
            <input type="checkbox"
                   checked="@Settings.ShowProgressBar"
                   @onchange="e => Update(s => s with { ShowProgressBar = (bool)e.Value! })" />
            <span class="toggle-slider"></span>
        </label>
    </div>

    <div class="toggle-group">
        <span class="toggle-label">Enable Auto-Save</span>
        <label class="toggle-switch">
            <input type="checkbox"
                   checked="@Settings.EnableAutoSave"
                   @onchange="e => Update(s => s with { EnableAutoSave = (bool)e.Value! })" />
            <span class="toggle-slider"></span>
        </label>
    </div>

    @if (Settings.EnableAutoSave)
    {
        <div class="form-group">
            <label class="form-label">Auto-Save Interval (seconds)</label>
            <div class="number-input">
                <button @onclick="() => AdjustInterval(-5)">
                    <i class="bi bi-dash"></i>
                </button>
                <input type="number"
                       class="form-input"
                       value="@Settings.AutoSaveIntervalSeconds"
                       @onchange="e => Update(s => s with { AutoSaveIntervalSeconds = int.Parse(e.Value?.ToString() ?? "30") })" />
                <button @onclick="() => AdjustInterval(5)">
                    <i class="bi bi-plus"></i>
                </button>
            </div>
        </div>
    }

    <div class="toggle-group">
        <span class="toggle-label">Require Confirmation on Exit</span>
        <label class="toggle-switch">
            <input type="checkbox"
                   checked="@Settings.RequireExitConfirmation"
                   @onchange="e => Update(s => s with { RequireExitConfirmation = (bool)e.Value! })" />
            <span class="toggle-slider"></span>
        </label>
    </div>

    <div class="toggle-group">
        <span class="toggle-label">Show Field Help</span>
        <label class="toggle-switch">
            <input type="checkbox"
                   checked="@Settings.ShowFieldHelp"
                   @onchange="e => Update(s => s with { ShowFieldHelp = (bool)e.Value! })" />
            <span class="toggle-slider"></span>
        </label>
    </div>
</div>

@code {
    [Parameter] public BehaviorSettings Settings { get; set; } = new();
    [Parameter] public EventCallback<BehaviorSettings> OnSettingsChanged { get; set; }

    private void Update(Func<BehaviorSettings, BehaviorSettings> updater)
    {
        OnSettingsChanged.InvokeAsync(updater(Settings));
    }

    private void AdjustInterval(int delta)
    {
        var newValue = Math.Max(5, Settings.AutoSaveIntervalSeconds + delta);
        Update(s => s with { AutoSaveIntervalSeconds = newValue });
    }
}
```

### TriggerSettings.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="settings-fields">
    <div class="form-group">
        <label class="form-label">Start Trigger</label>
        <div class="trigger-options">
            @foreach (var trigger in Enum.GetValues<StartTriggerType>())
            {
                <span class="trigger-chip @(Settings.StartTrigger == trigger ? "active" : "")"
                      @onclick="() => SetTrigger(trigger)">
                    <i class="bi @GetTriggerIcon(trigger)"></i>
                    @trigger
                </span>
            }
        </div>
    </div>

    @if (Settings.StartTrigger == StartTriggerType.Url)
    {
        <div class="form-group">
            <label class="form-label">Trigger URL Path</label>
            <input type="text"
                   class="form-input"
                   value="@Settings.TriggerUrl"
                   @oninput="e => Update(s => s with { TriggerUrl = e.Value?.ToString() })"
                   placeholder="/forms/my-workflow" />
        </div>
    }

    @if (Settings.StartTrigger == StartTriggerType.Scheduled)
    {
        <div class="form-group">
            <label class="form-label">Schedule (Cron Expression)</label>
            <input type="text"
                   class="form-input"
                   value="@Settings.ScheduleCron"
                   @oninput="e => Update(s => s with { ScheduleCron = e.Value?.ToString() })"
                   placeholder="0 9 * * MON-FRI" />
            <span class="help-text">Runs at 9 AM on weekdays</span>
        </div>
    }

    <div class="form-group">
        <label class="form-label">On Complete</label>
        <select class="form-select"
                value="@Settings.OnComplete"
                @onchange="e => Update(s => s with { OnComplete = Enum.Parse<CompletionAction>(e.Value?.ToString() ?? "ShowMessage") })">
            @foreach (var action in Enum.GetValues<CompletionAction>())
            {
                <option value="@action">@FormatAction(action)</option>
            }
        </select>
    </div>

    @if (Settings.OnComplete == CompletionAction.ShowMessage)
    {
        <div class="form-group">
            <label class="form-label">Success Message</label>
            <input type="text"
                   class="form-input"
                   value="@Settings.SuccessMessage"
                   @oninput="e => Update(s => s with { SuccessMessage = e.Value?.ToString() ?? "" })"
                   placeholder="Thank you for your submission!" />
        </div>
    }

    @if (Settings.OnComplete == CompletionAction.Redirect)
    {
        <div class="form-group">
            <label class="form-label">Redirect URL</label>
            <input type="text"
                   class="form-input"
                   value="@Settings.RedirectUrl"
                   @oninput="e => Update(s => s with { RedirectUrl = e.Value?.ToString() })"
                   placeholder="https://example.com/thank-you" />
        </div>
    }
</div>

@code {
    [Parameter] public TriggerSettings Settings { get; set; } = new();
    [Parameter] public EventCallback<TriggerSettings> OnSettingsChanged { get; set; }

    private void SetTrigger(StartTriggerType trigger)
    {
        Update(s => s with { StartTrigger = trigger });
    }

    private void Update(Func<TriggerSettings, TriggerSettings> updater)
    {
        OnSettingsChanged.InvokeAsync(updater(Settings));
    }

    private string GetTriggerIcon(StartTriggerType trigger) => trigger switch
    {
        StartTriggerType.Manual => "bi-hand-index",
        StartTriggerType.Url => "bi-link-45deg",
        StartTriggerType.Scheduled => "bi-calendar-event",
        StartTriggerType.Event => "bi-broadcast",
        StartTriggerType.Api => "bi-code-slash",
        _ => "bi-circle"
    };

    private string FormatAction(CompletionAction action) => action switch
    {
        CompletionAction.ShowMessage => "Show Success Message",
        CompletionAction.Redirect => "Redirect to URL",
        CompletionAction.SubmitToApi => "Submit to API",
        CompletionAction.TriggerWorkflow => "Trigger Another Workflow",
        CompletionAction.Custom => "Custom Action",
        _ => action.ToString()
    };
}
```

### ValidationSettingsSection.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="settings-fields">
    @if (Status.Issues.Any())
    {
        <div class="validation-list">
            @foreach (var issue in Status.Issues.Take(5))
            {
                <div class="validation-item">
                    <span class="validation-icon @GetSeverityClass(issue.Severity)">
                        <i class="bi @GetSeverityIcon(issue.Severity)"></i>
                    </span>
                    <span class="validation-text">@issue.Message</span>
                </div>
            }
            @if (Status.Issues.Count > 5)
            {
                <div class="validation-more">
                    +@(Status.Issues.Count - 5) more issues
                </div>
            }
        </div>
    }
    else
    {
        <div class="validation-success">
            <i class="bi bi-check-circle-fill"></i>
            <span>All validations passed</span>
        </div>
    }

    <div class="form-group">
        <label class="form-label">Validation Mode</label>
        <select class="form-select"
                value="@Settings.Mode"
                @onchange="e => Update(s => s with { Mode = Enum.Parse<ValidationMode>(e.Value?.ToString() ?? "OnStepChange") })">
            @foreach (var mode in Enum.GetValues<ValidationMode>())
            {
                <option value="@mode">@FormatMode(mode)</option>
            }
        </select>
    </div>

    <div class="toggle-group">
        <span class="toggle-label">Show Inline Errors</span>
        <label class="toggle-switch">
            <input type="checkbox"
                   checked="@Settings.ShowInlineErrors"
                   @onchange="e => Update(s => s with { ShowInlineErrors = (bool)e.Value! })" />
            <span class="toggle-slider"></span>
        </label>
    </div>

    <div class="toggle-group">
        <span class="toggle-label">Scroll to First Error</span>
        <label class="toggle-switch">
            <input type="checkbox"
                   checked="@Settings.ScrollToFirstError"
                   @onchange="e => Update(s => s with { ScrollToFirstError = (bool)e.Value! })" />
            <span class="toggle-slider"></span>
        </label>
    </div>
</div>

@code {
    [Parameter] public ValidationSettings Settings { get; set; } = new();
    [Parameter] public ValidationStatus Status { get; set; } = new();
    [Parameter] public EventCallback<ValidationSettings> OnSettingsChanged { get; set; }

    private void Update(Func<ValidationSettings, ValidationSettings> updater)
    {
        OnSettingsChanged.InvokeAsync(updater(Settings));
    }

    private string GetSeverityClass(IssueSeverity severity) => severity switch
    {
        IssueSeverity.Error => "error",
        IssueSeverity.Warning => "warning",
        IssueSeverity.Info => "info",
        _ => ""
    };

    private string GetSeverityIcon(IssueSeverity severity) => severity switch
    {
        IssueSeverity.Error => "bi-x",
        IssueSeverity.Warning => "bi-exclamation",
        IssueSeverity.Info => "bi-info",
        _ => "bi-circle"
    };

    private string FormatMode(ValidationMode mode) => mode switch
    {
        ValidationMode.OnStepChange => "Validate on Step Change",
        ValidationMode.OnSubmitOnly => "Validate on Submit Only",
        ValidationMode.RealTime => "Real-time Validation",
        ValidationMode.OnBlur => "Validate on Field Blur",
        _ => mode.ToString()
    };
}
```

## Shared Styles (form-controls.css)

```css
/* Form Controls */
.form-group {
    display: flex;
    flex-direction: column;
    gap: 0.375rem;
}

.form-label {
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--text-secondary, #6b7280);
}

.form-input,
.form-select,
.form-textarea {
    padding: 0.5rem 0.75rem;
    border: 1px solid var(--border-color, #e5e7eb);
    border-radius: 6px;
    font-size: 0.875rem;
    background: var(--bg-primary, #ffffff);
    color: var(--text-primary, #1f2937);
    transition: all 0.15s ease;
}

.form-input:focus,
.form-select:focus,
.form-textarea:focus {
    outline: none;
    border-color: var(--primary, #6366f1);
    box-shadow: 0 0 0 3px var(--primary-light, #eef2ff);
}

.form-textarea {
    resize: vertical;
    min-height: 80px;
    font-family: inherit;
}

.form-select {
    cursor: pointer;
}

.help-text {
    font-size: 0.75rem;
    color: var(--text-muted, #9ca3af);
}

/* Toggle Controls */
.toggle-group {
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.toggle-label {
    font-size: 0.8125rem;
    color: var(--text-secondary, #6b7280);
}

.toggle-switch {
    position: relative;
    width: 2.5rem;
    height: 1.5rem;
}

.toggle-switch input {
    opacity: 0;
    width: 0;
    height: 0;
}

.toggle-slider {
    position: absolute;
    cursor: pointer;
    inset: 0;
    background: var(--bg-tertiary, #e5e7eb);
    border-radius: 9999px;
    transition: 0.2s;
}

.toggle-slider::before {
    position: absolute;
    content: "";
    height: 1rem;
    width: 1rem;
    left: 0.25rem;
    bottom: 0.25rem;
    background: white;
    border-radius: 50%;
    transition: 0.2s;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
}

input:checked + .toggle-slider {
    background: var(--success, #10b981);
}

input:checked + .toggle-slider::before {
    transform: translateX(1rem);
}

/* Number Input */
.number-input {
    display: flex;
    align-items: center;
    gap: 0.25rem;
}

.number-input input {
    width: 4rem;
    text-align: center;
}

.number-input button {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 1.75rem;
    height: 1.75rem;
    background: var(--bg-secondary, #f3f4f6);
    border: 1px solid var(--border-color, #e5e7eb);
    border-radius: 4px;
    color: var(--text-secondary, #6b7280);
    cursor: pointer;
}

.number-input button:hover {
    background: var(--bg-tertiary, #e5e7eb);
}

/* Trigger Chips */
.trigger-options {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
}

.trigger-chip {
    display: inline-flex;
    align-items: center;
    gap: 0.375rem;
    padding: 0.375rem 0.75rem;
    background: var(--bg-secondary, #f3f4f6);
    border: 1px solid var(--border-color, #e5e7eb);
    border-radius: 9999px;
    font-size: 0.75rem;
    color: var(--text-secondary, #6b7280);
    cursor: pointer;
    transition: all 0.15s ease;
}

.trigger-chip:hover {
    border-color: var(--primary, #6366f1);
    color: var(--primary, #6366f1);
}

.trigger-chip.active {
    background: var(--primary-light, #eef2ff);
    border-color: var(--primary, #6366f1);
    color: var(--primary, #6366f1);
}

/* Validation List */
.validation-list {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}

.validation-item {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.5rem 0.75rem;
    background: var(--bg-secondary, #f3f4f6);
    border-radius: 6px;
}

.validation-icon {
    width: 1.5rem;
    height: 1.5rem;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    font-size: 0.75rem;
}

.validation-icon.success {
    background: var(--success-light, #d1fae5);
    color: var(--success, #10b981);
}

.validation-icon.warning {
    background: var(--warning-light, #fef3c7);
    color: var(--warning, #f59e0b);
}

.validation-icon.error {
    background: var(--danger-light, #fee2e2);
    color: var(--danger, #ef4444);
}

.validation-text {
    flex: 1;
    font-size: 0.8125rem;
    color: var(--text-secondary, #6b7280);
}
```

## Usage Example

```razor
@page "/workflow-designer"

<div class="editor-layout">
    <WorkflowCanvas @ref="_canvas"
                    Nodes="Nodes"
                    Connections="Connections" />

    <WorkflowSettingsPanel @bind-Settings="WorkflowSettings"
                           @bind-IsCollapsed="_settingsCollapsed"
                           ValidationStatus="ValidationStatus"
                           OnApply="SaveSettings"
                           OnReset="ResetSettings" />
</div>

@code {
    private bool _settingsCollapsed = false;
    private WorkflowSettings WorkflowSettings { get; set; } = new()
    {
        Name = "Customer Onboarding",
        Description = "Multi-step registration process"
    };
    private ValidationStatus ValidationStatus { get; set; } = new();

    private async Task SaveSettings()
    {
        // Validate and save settings
        await WorkflowService.SaveSettingsAsync(WorkflowSettings);
    }

    private void ResetSettings()
    {
        // Reset to original settings
    }
}
```

## Claude Prompt for Implementation

```
Implement the WorkflowSettingsPanel component for configuring workflow-level settings.

Requirements:
1. Collapsible sidebar panel with multiple settings sections
2. Sections: General, Behavior, Triggers, Data Integration, Appearance, Validation, Access Control
3. Each section is independently collapsible
4. Settings use appropriate controls: text inputs, toggles, dropdowns, number steppers
5. Validation status display with error/warning/success indicators
6. Reset and Apply buttons in footer
7. Dark mode support

Use the existing design system:
- CSS variables for colors (--primary: #6366f1, etc.)
- Consistent form control styling
- Toggle switches for boolean options
- Section headers with icons
- Bootstrap Icons for iconography

Data models should use C# records with immutability.
Each settings section should be a separate component for maintainability.
Use two-way binding for settings changes.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `WorkflowSettingsPanel-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing panel collapse/expand
- General settings input testing (name, description, version)
- Behavior settings toggle testing
- Auto-save interval adjustment testing
- Trigger type selection testing
- Completion action configuration testing
- Appearance settings testing
- Validation mode selection testing
- Access control settings testing
- Apply and Reset button functionality

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Settings model files creation (WorkflowSettings, BehaviorSettings, TriggerSettings, etc.)
- Enum files (WorkflowStatus, StartTriggerType, CompletionAction, etc.)
- ValidationStatus model creation
- CSS file imports (form-controls.css)
- Component registration in _Imports.razor
- Settings persistence/storage implementation
- WorkflowValidationService implementation
- Primary color dynamic CSS variable update

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Panel collapses and expands
- [ ] All sections are collapsible independently
- [ ] General section shows name, description, version
- [ ] Status badge displays correctly
- [ ] Behavior toggles work
- [ ] Auto-save interval adjusts with +/- buttons
- [ ] Trigger type chips are selectable
- [ ] URL trigger shows URL input
- [ ] Scheduled trigger shows cron input
- [ ] Completion action dropdown works
- [ ] Success message input appears for ShowMessage
- [ ] Redirect URL input appears for Redirect
- [ ] Validation mode dropdown works
- [ ] Validation status displays issues
- [ ] Apply button triggers save
- [ ] Reset button restores original settings
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

## Integration Notes

1. **Settings Persistence**: Settings should be serialized to JSON and stored with the workflow
2. **Validation Service**: Implement a WorkflowValidationService to check workflow integrity
3. **Real-time Updates**: Consider SignalR for multi-user editing scenarios
4. **Theming**: Primary color setting should update CSS variables dynamically
5. **API Integration**: Data integration settings should be validated on change
