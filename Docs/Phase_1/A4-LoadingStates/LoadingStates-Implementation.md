# A.4 Loading States - Implementation Plan

> **Task**: Loading States
> **Location**: `Src/VisualEditorOpus/Components/Shared/`
> **Priority**: Critical
> **Estimated Effort**: 2 hours
> **Delegation**: 90% AI

---

## Overview

Loading states provide visual feedback during asynchronous operations. They reduce perceived wait time and reassure users that the system is working.

---

## Components to Create

| Component | Purpose | Size |
|-----------|---------|------|
| `LoadingSpinner.razor` | Animated spinner for general loading | sm, md, lg |
| `SkeletonLoader.razor` | Placeholder content while loading | Configurable |
| `ButtonLoading.razor` | Button with loading state | - |
| `PageLoader.razor` | Full page loading overlay | - |
| `InlineLoader.razor` | Inline loading with text | - |
| `ProgressBar.razor` | Determinate/indeterminate progress | - |

---

## 1. LoadingSpinner.razor

```razor
<div class="spinner @SizeClass @ColorClass @CssClass" role="status" aria-label="Loading">
    <span class="visually-hidden">Loading...</span>
</div>

@code {
    [Parameter] public SpinnerSize Size { get; set; } = SpinnerSize.Medium;
    [Parameter] public SpinnerColor Color { get; set; } = SpinnerColor.Primary;
    [Parameter] public string? CssClass { get; set; }

    private string SizeClass => Size switch
    {
        SpinnerSize.Small => "spinner-sm",
        SpinnerSize.Large => "spinner-lg",
        _ => ""
    };

    private string ColorClass => Color switch
    {
        SpinnerColor.White => "spinner-white",
        _ => ""
    };
}
```

### Enums

```csharp
public enum SpinnerSize { Small, Medium, Large }
public enum SpinnerColor { Primary, White }
```

---

## 2. SkeletonLoader.razor

```razor
@switch (Variant)
{
    case SkeletonVariant.Text:
        <div class="skeleton skeleton-text @CssClass" style="@WidthStyle"></div>
        break;

    case SkeletonVariant.Title:
        <div class="skeleton skeleton-title @CssClass" style="@WidthStyle"></div>
        break;

    case SkeletonVariant.Avatar:
        <div class="skeleton skeleton-avatar @(Size == SkeletonSize.Small ? "skeleton-avatar-sm" : "") @CssClass"></div>
        break;

    case SkeletonVariant.Button:
        <div class="skeleton skeleton-btn @CssClass"></div>
        break;

    case SkeletonVariant.Icon:
        <div class="skeleton skeleton-icon @CssClass"></div>
        break;

    case SkeletonVariant.Card:
        <div class="skeleton-card @CssClass">
            <div class="skeleton-card-header">
                <div class="skeleton skeleton-icon"></div>
                <div style="flex: 1;">
                    <div class="skeleton skeleton-title"></div>
                    <div class="skeleton skeleton-text-sm" style="width: 40%;"></div>
                </div>
            </div>
            <div class="skeleton-card-body">
                <div class="skeleton skeleton-text"></div>
                <div class="skeleton skeleton-text"></div>
                <div class="skeleton skeleton-text" style="width: 70%;"></div>
            </div>
        </div>
        break;

    case SkeletonVariant.Field:
        <div class="skeleton-field @CssClass">
            <div class="skeleton skeleton-field-label"></div>
            <div class="skeleton skeleton-field-input"></div>
        </div>
        break;

    case SkeletonVariant.ListItem:
        <div class="skeleton-list-item @CssClass">
            <div class="skeleton skeleton-avatar-sm"></div>
            <div style="flex: 1;">
                <div class="skeleton skeleton-text" style="width: 60%; margin-bottom: 4px;"></div>
                <div class="skeleton skeleton-text-sm" style="width: 40%;"></div>
            </div>
        </div>
        break;
}

@code {
    [Parameter] public SkeletonVariant Variant { get; set; } = SkeletonVariant.Text;
    [Parameter] public SkeletonSize Size { get; set; } = SkeletonSize.Medium;
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private string? WidthStyle => Width != null ? $"width: {Width}" : null;
}
```

### Enums

```csharp
public enum SkeletonVariant { Text, Title, Avatar, Button, Icon, Card, Field, ListItem }
public enum SkeletonSize { Small, Medium }
```

---

## 3. ButtonLoading Component

Extend existing button with loading state:

```razor
<button class="btn @ButtonClass @(IsLoading ? "btn-loading" : "")"
        disabled="@(IsLoading || Disabled)"
        @onclick="OnClick">
    @if (IsLoading)
    {
        <span class="btn-text" style="opacity: 0;">@ChildContent</span>
        <LoadingSpinner Size="SpinnerSize.Small"
                        Color="@(Variant == ButtonVariant.Primary ? SpinnerColor.White : SpinnerColor.Primary)"
                        CssClass="btn-spinner" />
    }
    else
    {
        @ChildContent
    }
</button>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }

    private string ButtonClass => Variant switch
    {
        ButtonVariant.Outline => "btn-outline",
        ButtonVariant.Ghost => "btn-ghost",
        ButtonVariant.Success => "btn-success",
        ButtonVariant.Danger => "btn-danger",
        _ => "btn-primary"
    };
}
```

---

## 4. ProgressBar.razor

```razor
<div class="progress-bar @(IsIndeterminate ? "progress-bar-indeterminate" : "") @CssClass">
    <div class="progress-bar-fill" style="@FillStyle"></div>
</div>

@code {
    [Parameter] public int Value { get; set; } = 0;
    [Parameter] public int Max { get; set; } = 100;
    [Parameter] public bool IsIndeterminate { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private string FillStyle => IsIndeterminate ? "" : $"width: {Percentage}%";
    private int Percentage => Max > 0 ? (int)((double)Value / Max * 100) : 0;
}
```

---

## 5. InlineLoader.razor

```razor
<div class="inline-loader @CssClass">
    <LoadingSpinner Size="SpinnerSize.Small" />
    <span>@Text</span>
</div>

@code {
    [Parameter] public string Text { get; set; } = "Loading...";
    [Parameter] public string? CssClass { get; set; }
}
```

---

## 6. PageLoader.razor

```razor
@if (IsVisible)
{
    <div class="page-loader">
        <div class="page-loader-icon">
            <i class="bi bi-grid-1x2-fill"></i>
        </div>
        <LoadingSpinner Size="SpinnerSize.Large" />
        <div class="page-loader-text">@Text</div>
    </div>
}

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public string Text { get; set; } = "Loading...";
}
```

---

## CSS Styles

```css
/* ===== LOADING STATES ===== */

/* Spinner */
@keyframes spin {
    to { transform: rotate(360deg); }
}

.spinner {
    width: 32px;
    height: 32px;
    border: 3px solid var(--border-color);
    border-top-color: var(--primary);
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
}

.spinner-sm {
    width: 20px;
    height: 20px;
    border-width: 2px;
}

.spinner-lg {
    width: 48px;
    height: 48px;
    border-width: 4px;
}

.spinner-white {
    border-color: rgba(255, 255, 255, 0.3);
    border-top-color: white;
}

/* Skeleton Shimmer */
@keyframes shimmer {
    0% { background-position: -200% 0; }
    100% { background-position: 200% 0; }
}

.skeleton {
    background: linear-gradient(
        90deg,
        var(--bg-tertiary) 25%,
        var(--bg-secondary) 50%,
        var(--bg-tertiary) 75%
    );
    background-size: 200% 100%;
    animation: shimmer 1.5s infinite linear;
    border-radius: var(--radius-md);
}

.skeleton-text { height: 14px; margin-bottom: 8px; }
.skeleton-text-sm { height: 12px; margin-bottom: 6px; }
.skeleton-title { height: 20px; width: 60%; margin-bottom: 12px; }
.skeleton-avatar { width: 48px; height: 48px; border-radius: 50%; flex-shrink: 0; }
.skeleton-avatar-sm { width: 32px; height: 32px; }
.skeleton-btn { height: 36px; width: 100px; }
.skeleton-icon { width: 44px; height: 44px; border-radius: var(--radius-lg); }

.skeleton-card {
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-xl);
    padding: 20px;
}

.skeleton-card-header {
    display: flex;
    gap: 12px;
    margin-bottom: 16px;
    align-items: flex-start;
}

.skeleton-card-body {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.skeleton-field {
    padding: 14px;
    border: 1px solid var(--border-color);
    border-radius: var(--radius-lg);
    margin-bottom: 12px;
}

.skeleton-field-label { height: 14px; width: 30%; margin-bottom: 8px; }
.skeleton-field-input { height: 36px; }

.skeleton-list-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px;
    border-bottom: 1px solid var(--border-color);
}

/* Button Loading */
.btn-loading {
    position: relative;
}

.btn-loading .btn-spinner {
    position: absolute;
}

/* Progress Bar */
.progress-bar {
    height: 8px;
    background: var(--bg-tertiary);
    border-radius: 4px;
    overflow: hidden;
}

.progress-bar-fill {
    height: 100%;
    background: var(--primary);
    border-radius: 4px;
    transition: width 0.3s ease;
}

@keyframes bar-slide {
    0% { left: -40%; }
    100% { left: 100%; }
}

.progress-bar-indeterminate .progress-bar-fill {
    width: 30%;
    position: relative;
    animation: bar-slide 1.5s ease-in-out infinite;
}

/* Inline Loader */
.inline-loader {
    display: flex;
    align-items: center;
    gap: 8px;
    color: var(--text-secondary);
    font-size: 13px;
}

/* Page Loader */
.page-loader {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: var(--bg-primary);
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    z-index: 9999;
    gap: 20px;
}

.page-loader-icon {
    width: 64px;
    height: 64px;
    background: var(--primary);
    border-radius: var(--radius-xl);
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 28px;
}

.page-loader-text {
    color: var(--text-secondary);
    font-size: 14px;
}

/* Accessibility */
.visually-hidden {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement loading state components for my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Shared/

## Components to Create

### 1. LoadingSpinner.razor
Animated border spinner with:
- Size variants: Small (20px), Medium (32px), Large (48px)
- Color variants: Primary (indigo), White (for dark backgrounds)
- Accessible aria-label

### 2. SkeletonLoader.razor
Shimmer placeholder with variants:
- Text (14px height)
- Title (20px height, 60% width)
- Avatar (48px circle)
- Button (36px height, 100px width)
- Icon (44px square)
- Card (icon + title + text lines)
- Field (label + input)
- ListItem (avatar + text lines)

### 3. ButtonLoading.razor
Button wrapper that shows:
- Normal state with content
- Loading state with spinner, text hidden
- Disabled during loading

### 4. ProgressBar.razor
Progress indicator with:
- Determinate mode (Value/Max)
- Indeterminate mode (animated bar)

### 5. InlineLoader.razor
Inline text with spinner:
- Small spinner + custom text

### 6. PageLoader.razor
Full page overlay:
- Centered logo/icon
- Large spinner
- Loading text

## CSS
Create loading.css with:
- @keyframes spin (360 rotation)
- @keyframes shimmer (gradient slide)
- @keyframes bar-slide (progress bar)
- All spinner, skeleton, progress styles
- Visually hidden class for accessibility

## Enums
```csharp
public enum SpinnerSize { Small, Medium, Large }
public enum SpinnerColor { Primary, White }
public enum SkeletonVariant { Text, Title, Avatar, Button, Icon, Card, Field, ListItem }
public enum SkeletonSize { Small, Medium }
```

Please implement complete, production-ready code with proper accessibility.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `LoadingStates-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing each loading component
- Scenarios to trigger each loading state
- Expected animation behaviors
- Spinner size verification steps
- Skeleton shimmer animation testing
- Progress bar determinate/indeterminate testing
- Accessibility testing (screen reader announcements)

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Component registrations needed
- CSS file imports to add
- Where to use each loading component in the application
- Async operation integration patterns
- Loading state management in parent components

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] LoadingSpinner animates smoothly
- [ ] All spinner sizes correct (20, 32, 48px)
- [ ] White spinner visible on dark backgrounds
- [ ] SkeletonLoader shimmer animates
- [ ] All skeleton variants display correctly
- [ ] ButtonLoading state works
- [ ] ProgressBar shows correct percentage
- [ ] Indeterminate progress animates
- [ ] PageLoader covers full viewport
- [ ] Screen reader announces loading state
- [ ] prefers-reduced-motion respected

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Usage Examples

### Spinner in Button
```razor
<button class="btn btn-primary" disabled="@isSaving">
    @if (isSaving)
    {
        <LoadingSpinner Size="SpinnerSize.Small" Color="SpinnerColor.White" />
        <span>Saving...</span>
    }
    else
    {
        <i class="bi bi-save"></i> Save
    }
</button>
```

### Skeleton for Field List
```razor
@if (isLoading)
{
    @for (int i = 0; i < 3; i++)
    {
        <SkeletonLoader Variant="SkeletonVariant.Field" />
    }
}
else
{
    @foreach (var field in fields)
    {
        <FieldItem Field="field" />
    }
}
```

### Progress Bar for Upload
```razor
<ProgressBar Value="@uploadProgress" Max="100" />
<span>@uploadProgress%</span>
```

---

## Testing Checklist

- [ ] Spinner animates smoothly
- [ ] Spinner sizes correct (20, 32, 48px)
- [ ] White spinner visible on dark backgrounds
- [ ] Skeleton shimmer animates
- [ ] All skeleton variants display correctly
- [ ] Button loading state works
- [ ] Progress bar shows correct percentage
- [ ] Indeterminate progress animates
- [ ] Inline loader displays with text
- [ ] Page loader covers full viewport
- [ ] Dark mode styling correct
- [ ] Screen reader announces loading state
- [ ] Animations respect prefers-reduced-motion

---

## Notes

- Always include aria-label for accessibility
- Use skeleton loaders for content-heavy areas
- Use spinners for quick operations
- Use progress bars for determinable operations
- Avoid showing loading states for < 200ms (feels janky)
- Consider adding delay before showing loader for fast operations
