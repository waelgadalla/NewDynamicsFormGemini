# A.5 Error Handling UI - Implementation Plan

> **Task**: Error Handling UI
> **Location**: `Src/VisualEditorOpus/Components/Shared/`
> **Priority**: Critical
> **Estimated Effort**: 2 hours
> **Delegation**: 85% AI

---

## Overview

Professional error handling makes users feel safe. Clear, friendly error messages with actionable recovery options build trust and reduce frustration.

---

## Components to Create

| Component | Purpose |
|-----------|---------|
| `ErrorBoundary.razor` | Catch and display component errors |
| `Alert.razor` | Dismissible alert banners |
| `ValidationSummary.razor` | List of validation errors |
| `InlineError.razor` | Field-level error messages |
| `NetworkStatus.razor` | Connection status indicator |
| `RetryPanel.razor` | Failed operation with retry |
| `ErrorPage.razor` | Full-page error (404, 500) |
| `ToastError.razor` | Toast notification for errors |

---

## 1. ErrorBoundary.razor

```razor
@inherits ErrorBoundaryBase

@if (CurrentException != null)
{
    <div class="error-boundary">
        <div class="error-boundary-icon">
            <i class="bi bi-exclamation-triangle"></i>
        </div>
        <div class="error-boundary-title">Something went wrong</div>
        <div class="error-boundary-message">
            @(CustomMessage ?? "We encountered an unexpected error. Please try refreshing the page.")
        </div>
        <div class="error-boundary-actions">
            <button class="btn btn-primary btn-sm" @onclick="Recover">
                <i class="bi bi-arrow-clockwise"></i> Try Again
            </button>
            @if (ShowContactSupport)
            {
                <button class="btn btn-outline btn-sm" @onclick="OnContactSupport">
                    <i class="bi bi-chat"></i> Contact Support
                </button>
            }
        </div>
        @if (ShowDetails)
        {
            <details class="error-boundary-details-toggle">
                <summary>Technical Details</summary>
                <div class="error-boundary-details">
                    @CurrentException.Message<br/>
                    @CurrentException.StackTrace
                </div>
            </details>
        }
    </div>
}
else
{
    @ChildContent
}

@code {
    [Parameter] public string? CustomMessage { get; set; }
    [Parameter] public bool ShowDetails { get; set; } = false;
    [Parameter] public bool ShowContactSupport { get; set; } = true;
    [Parameter] public EventCallback OnContactSupport { get; set; }

    protected override Task OnErrorAsync(Exception exception)
    {
        // Log error to service
        Console.Error.WriteLine($"ErrorBoundary caught: {exception}");
        return Task.CompletedTask;
    }
}
```

---

## 2. Alert.razor

```razor
<div class="alert alert-@Variant @(IsDismissing ? "dismissing" : "") @CssClass"
     role="alert"
     style="@(IsVisible ? "" : "display: none;")">
    <div class="alert-icon">
        <i class="bi bi-@IconName"></i>
    </div>
    <div class="alert-content">
        @if (!string.IsNullOrEmpty(Title))
        {
            <div class="alert-title">@Title</div>
        }
        <div class="alert-message">@Message</div>
    </div>
    @if (Dismissible)
    {
        <button class="alert-close" @onclick="Dismiss" aria-label="Dismiss">
            <i class="bi bi-x-lg"></i>
        </button>
    }
</div>

@code {
    [Parameter] public AlertVariant Variant { get; set; } = AlertVariant.Error;
    [Parameter] public string? Title { get; set; }
    [Parameter] public string Message { get; set; } = "";
    [Parameter] public bool Dismissible { get; set; } = true;
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public EventCallback OnDismiss { get; set; }

    private bool IsVisible = true;
    private bool IsDismissing = false;

    private string IconName => Variant switch
    {
        AlertVariant.Error => "x-circle",
        AlertVariant.Warning => "exclamation-triangle",
        AlertVariant.Info => "info-circle",
        AlertVariant.Success => "check-circle",
        _ => "info-circle"
    };

    private async Task Dismiss()
    {
        IsDismissing = true;
        await Task.Delay(200); // Animation duration
        IsVisible = false;
        await OnDismiss.InvokeAsync();
    }
}
```

### AlertVariant Enum

```csharp
public enum AlertVariant
{
    Error,
    Warning,
    Info,
    Success
}
```

---

## 3. ValidationSummary.razor

```razor
@if (Errors?.Any() == true)
{
    <div class="validation-summary" role="alert">
        <div class="validation-summary-header">
            <i class="bi bi-exclamation-triangle"></i>
            <span>Please fix the following errors (@Errors.Count)</span>
        </div>
        <ul class="validation-summary-list">
            @foreach (var error in Errors)
            {
                <li @onclick="() => OnErrorClick.InvokeAsync(error.FieldId)">
                    <i class="bi bi-dot"></i>
                    @if (!string.IsNullOrEmpty(error.FieldLabel))
                    {
                        <span>Field "@error.FieldLabel" - </span>
                    }
                    @error.Message
                </li>
            }
        </ul>
    </div>
}

@code {
    [Parameter] public List<ValidationError>? Errors { get; set; }
    [Parameter] public EventCallback<string> OnErrorClick { get; set; }
}

public record ValidationError(string? FieldId, string? FieldLabel, string Message);
```

---

## 4. InlineError.razor

```razor
@if (!string.IsNullOrEmpty(Message))
{
    <div class="form-error" role="alert">
        <i class="bi bi-exclamation-circle"></i>
        <span>@Message</span>
    </div>
}

@code {
    [Parameter] public string? Message { get; set; }
}
```

---

## 5. NetworkStatus.razor

```razor
@if (Status != NetworkConnectionStatus.Connected || ShowOnReconnect)
{
    <div class="network-status @StatusClass" role="status">
        <i class="bi bi-@IconName"></i>
        <span>@StatusText</span>
        @if (Status == NetworkConnectionStatus.Reconnecting)
        {
            <div class="status-dot pulse"></div>
        }
    </div>
}

@code {
    [Parameter] public NetworkConnectionStatus Status { get; set; }

    private bool ShowOnReconnect = false;

    private string StatusClass => Status switch
    {
        NetworkConnectionStatus.Disconnected => "offline",
        NetworkConnectionStatus.Reconnecting => "reconnecting",
        NetworkConnectionStatus.Connected => "online",
        _ => ""
    };

    private string IconName => Status switch
    {
        NetworkConnectionStatus.Disconnected => "wifi-off",
        NetworkConnectionStatus.Reconnecting => "arrow-repeat",
        NetworkConnectionStatus.Connected => "wifi",
        _ => "wifi"
    };

    private string StatusText => Status switch
    {
        NetworkConnectionStatus.Disconnected => "You are offline",
        NetworkConnectionStatus.Reconnecting => "Reconnecting...",
        NetworkConnectionStatus.Connected => "Back online",
        _ => ""
    };

    protected override async Task OnParametersSetAsync()
    {
        if (Status == NetworkConnectionStatus.Connected)
        {
            ShowOnReconnect = true;
            StateHasChanged();
            await Task.Delay(3000);
            ShowOnReconnect = false;
        }
    }
}

public enum NetworkConnectionStatus
{
    Connected,
    Disconnected,
    Reconnecting
}
```

---

## 6. RetryPanel.razor

```razor
<div class="retry-panel @CssClass">
    <div class="retry-panel-icon">
        <i class="bi bi-@Icon"></i>
    </div>
    <div class="retry-panel-title">@Title</div>
    <div class="retry-panel-message">@Message</div>
    <button class="btn btn-primary btn-sm" @onclick="OnRetry" disabled="@IsRetrying">
        @if (IsRetrying)
        {
            <LoadingSpinner Size="SpinnerSize.Small" Color="SpinnerColor.White" />
            <span>Retrying...</span>
        }
        else
        {
            <i class="bi bi-arrow-clockwise"></i>
            <span>Try Again</span>
        }
    </button>
</div>

@code {
    [Parameter] public string Icon { get; set; } = "cloud-slash";
    [Parameter] public string Title { get; set; } = "Failed to load";
    [Parameter] public string Message { get; set; } = "Something went wrong. Please try again.";
    [Parameter] public EventCallback OnRetry { get; set; }
    [Parameter] public bool IsRetrying { get; set; }
    [Parameter] public string? CssClass { get; set; }
}
```

---

## 7. ErrorPage.razor

```razor
<div class="error-page">
    <div class="error-page-code">@Code</div>
    <div class="error-page-title">@Title</div>
    <div class="error-page-message">@Message</div>
    <div class="error-page-actions">
        <button class="btn btn-primary" @onclick="OnGoHome">
            <i class="bi bi-house"></i> Go to Dashboard
        </button>
        <button class="btn btn-ghost" @onclick="OnGoBack">
            <i class="bi bi-arrow-left"></i> Go Back
        </button>
    </div>
</div>

@code {
    [Parameter] public string Code { get; set; } = "404";
    [Parameter] public string Title { get; set; } = "Page Not Found";
    [Parameter] public string Message { get; set; } = "The page you're looking for doesn't exist.";
    [Parameter] public EventCallback OnGoHome { get; set; }
    [Parameter] public EventCallback OnGoBack { get; set; }
}
```

---

## CSS Styles

```css
/* ===== ERROR HANDLING STYLES ===== */

/* Error Boundary */
.error-boundary {
    background: var(--danger-light);
    border: 1px solid var(--danger);
    border-radius: var(--radius-lg);
    padding: 24px;
    text-align: center;
}

.error-boundary-icon {
    width: 64px;
    height: 64px;
    background: var(--bg-primary);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 16px;
    color: var(--danger);
    font-size: 28px;
    box-shadow: var(--shadow-md);
}

.error-boundary-title {
    font-size: 18px;
    font-weight: 700;
    color: var(--danger-dark);
    margin-bottom: 8px;
}

.error-boundary-message {
    color: var(--text-secondary);
    margin-bottom: 16px;
    max-width: 400px;
    margin-left: auto;
    margin-right: auto;
}

.error-boundary-actions {
    display: flex;
    gap: 8px;
    justify-content: center;
}

.error-boundary-details {
    background: rgba(0, 0, 0, 0.05);
    border-radius: var(--radius-md);
    padding: 12px;
    font-family: var(--font-mono);
    font-size: 12px;
    color: var(--danger-dark);
    text-align: left;
    margin-top: 12px;
    max-height: 100px;
    overflow-y: auto;
}

/* Alert Banners */
.alert {
    display: flex;
    align-items: flex-start;
    gap: 12px;
    padding: 14px 16px;
    border-radius: var(--radius-lg);
    margin-bottom: 16px;
    animation: fadeIn 0.2s ease;
}

.alert.dismissing {
    animation: fadeOut 0.2s ease forwards;
}

@keyframes fadeOut {
    to { opacity: 0; transform: translateY(-10px); }
}

.alert-icon {
    width: 24px;
    height: 24px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    font-size: 14px;
}

.alert-content { flex: 1; }
.alert-title { font-weight: 600; font-size: 14px; margin-bottom: 4px; }
.alert-message { font-size: 13px; opacity: 0.9; }

.alert-close {
    background: none;
    border: none;
    cursor: pointer;
    opacity: 0.7;
    padding: 4px;
    color: inherit;
}
.alert-close:hover { opacity: 1; }

/* Alert Variants */
.alert-error {
    background: var(--danger-light);
    border: 1px solid rgba(239, 68, 68, 0.3);
    color: #991B1B;
}
.alert-error .alert-icon { background: var(--danger); color: white; }

.alert-warning {
    background: var(--warning-light);
    border: 1px solid rgba(245, 158, 11, 0.3);
    color: #92400E;
}
.alert-warning .alert-icon { background: var(--warning); color: white; }

.alert-info {
    background: var(--primary-light);
    border: 1px solid rgba(99, 102, 241, 0.3);
    color: var(--primary-dark);
}
.alert-info .alert-icon { background: var(--primary); color: white; }

.alert-success {
    background: var(--success-light);
    border: 1px solid rgba(16, 185, 129, 0.3);
    color: #065F46;
}
.alert-success .alert-icon { background: var(--success); color: white; }

/* Inline Error */
.form-error {
    display: flex;
    align-items: center;
    gap: 6px;
    margin-top: 6px;
    font-size: 12px;
    color: var(--danger);
}
.form-error i { font-size: 14px; }

.form-input.error {
    border-color: var(--danger);
    background: var(--danger-light);
}

/* Validation Summary */
.validation-summary {
    background: var(--danger-light);
    border: 1px solid rgba(239, 68, 68, 0.3);
    border-radius: var(--radius-lg);
    padding: 16px;
    margin-bottom: 20px;
}

.validation-summary-header {
    display: flex;
    align-items: center;
    gap: 8px;
    font-weight: 600;
    color: #991B1B;
    margin-bottom: 12px;
}
.validation-summary-header i { font-size: 18px; color: var(--danger); }

.validation-summary-list { list-style: none; }
.validation-summary-list li {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 0;
    font-size: 13px;
    color: #991B1B;
    cursor: pointer;
}
.validation-summary-list li:hover { text-decoration: underline; }
.validation-summary-list li i { font-size: 12px; color: var(--danger); }

/* Network Status */
.network-status {
    position: fixed;
    bottom: 20px;
    left: 50%;
    transform: translateX(-50%);
    background: var(--text-primary);
    color: var(--bg-primary);
    padding: 12px 20px;
    border-radius: var(--radius-lg);
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 14px;
    font-weight: 500;
    box-shadow: var(--shadow-lg);
    animation: slideUp 0.3s ease;
    z-index: var(--z-toast);
}

.network-status.offline { background: var(--danger); color: white; }
.network-status.reconnecting { background: var(--warning); color: white; }
.network-status.online { background: var(--success); color: white; }

@keyframes slideUp {
    from { transform: translateX(-50%) translateY(20px); opacity: 0; }
    to { transform: translateX(-50%) translateY(0); opacity: 1; }
}

.status-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: currentColor;
}
.status-dot.pulse { animation: pulse 1.5s ease-in-out infinite; }

/* Retry Panel */
.retry-panel {
    background: var(--bg-tertiary);
    border-radius: var(--radius-lg);
    padding: 24px;
    text-align: center;
}

.retry-panel-icon {
    font-size: 32px;
    color: var(--text-muted);
    margin-bottom: 12px;
}

.retry-panel-title {
    font-weight: 600;
    margin-bottom: 8px;
}

.retry-panel-message {
    font-size: 13px;
    color: var(--text-secondary);
    margin-bottom: 16px;
}

/* Error Page */
.error-page {
    min-height: 400px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
    padding: 48px 24px;
}

.error-page-code {
    font-size: 80px;
    font-weight: 700;
    color: var(--text-muted);
    line-height: 1;
    margin-bottom: 16px;
}

.error-page-title {
    font-size: 24px;
    font-weight: 700;
    margin-bottom: 8px;
}

.error-page-message {
    color: var(--text-secondary);
    margin-bottom: 24px;
    max-width: 400px;
}

.error-page-actions {
    display: flex;
    gap: 12px;
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement error handling UI components for my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Shared/

## Components to Create

### 1. ErrorBoundary.razor
Wrap components to catch errors:
- Display friendly error message
- "Try Again" button that calls Recover()
- Optional "Contact Support" button
- Collapsible technical details (dev mode)
- Log errors to console/service

### 2. Alert.razor
Dismissible alert banner:
- Variants: Error, Warning, Info, Success
- Icon, title, message
- Dismiss button with animation
- Auto-dismiss option

### 3. ValidationSummary.razor
List validation errors:
- Error count header
- Clickable items to focus field
- Scroll to field on click

### 4. InlineError.razor
Field-level error:
- Error icon + message
- Pairs with .form-input.error class

### 5. NetworkStatus.razor
Connection indicator:
- States: Connected, Disconnected, Reconnecting
- Fixed position at bottom center
- Auto-hide on reconnect after 3s

### 6. RetryPanel.razor
Failed operation:
- Icon, title, message
- Retry button with loading state

### 7. ErrorPage.razor
Full-page error (404, 500):
- Large error code
- Title and message
- Go Home and Go Back buttons

## Enums
```csharp
public enum AlertVariant { Error, Warning, Info, Success }
public enum NetworkConnectionStatus { Connected, Disconnected, Reconnecting }
```

## Records
```csharp
public record ValidationError(string? FieldId, string? FieldLabel, string Message);
```

## CSS
Create errors.css with styles for all error components.

## Error Message Guidelines
- Be specific about what went wrong
- Suggest what user can do next
- Use friendly, non-technical language
- Avoid blame ("You did something wrong")
- Include retry options when possible

Please implement complete, production-ready code with proper accessibility (role="alert").

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `ErrorHandling-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing each error component
- Scenarios to trigger each error state
- Expected visual appearance and behavior
- ErrorBoundary recovery testing
- Alert dismiss animation testing
- ValidationSummary click-to-field testing
- NetworkStatus state transition testing
- Accessibility testing (ARIA roles, screen reader)

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Component registrations needed
- Where to wrap components with ErrorBoundary
- Network status detection implementation
- Validation error collection integration
- Error logging service integration
- Navigation service for ErrorPage

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] ErrorBoundary catches component errors
- [ ] ErrorBoundary Recover() resets state
- [ ] Alert displays all variants correctly
- [ ] Alert dismiss animation works
- [ ] ValidationSummary shows error count
- [ ] ValidationSummary click scrolls to field
- [ ] InlineError displays below input
- [ ] NetworkStatus shows for offline
- [ ] NetworkStatus auto-hides on reconnect
- [ ] RetryPanel retry button works
- [ ] ErrorPage navigation works
- [ ] Dark mode styling correct
- [ ] Screen reader announces alerts

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] ErrorBoundary catches component errors
- [ ] ErrorBoundary Recover() resets state
- [ ] Alert displays all variants correctly
- [ ] Alert dismiss animation works
- [ ] ValidationSummary shows error count
- [ ] ValidationSummary click scrolls to field
- [ ] InlineError displays below input
- [ ] NetworkStatus shows for offline
- [ ] NetworkStatus auto-hides on reconnect
- [ ] RetryPanel retry button works
- [ ] ErrorPage navigation works
- [ ] Dark mode styling correct
- [ ] Screen reader announces alerts
- [ ] Animations smooth

---

## Notes

- Always provide a way out (retry, go back, contact support)
- Error messages should be helpful, not scary
- Log errors for debugging but show friendly messages
- Use appropriate ARIA roles for accessibility
- Consider error analytics/reporting service integration
