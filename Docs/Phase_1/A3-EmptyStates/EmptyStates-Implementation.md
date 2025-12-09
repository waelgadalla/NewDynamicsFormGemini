# A.3 Empty States & Onboarding - Implementation Plan

> **Task**: Empty States & Onboarding
> **Location**: `Src/VisualEditorOpus/Components/Shared/`
> **Priority**: Critical
> **Estimated Effort**: 2-3 hours
> **Delegation**: 85% AI

---

## Overview

Professional empty states guide users and make the application feel intentional. They turn "nothing here" into "here's what you can do."

---

## Empty States Needed

| Location | Component | Purpose |
|----------|-----------|---------|
| Module Editor Canvas | `EmptyCanvas.razor` | Guide users to add first field |
| Workflow Designer | `EmptyWorkflow.razor` | Guide users to add first module |
| Outline Panel (No Fields) | `EmptyOutline.razor` | Small empty state in sidebar |
| Validation Panel (No Issues) | `ValidationSuccess.razor` | Celebrate success |
| Search (No Results) | `NoSearchResults.razor` | Help users find alternatives |
| CodeSet List (No Items) | `EmptyCodeSet.razor` | Guide users to add options |
| Dashboard (No Forms) | `EmptyDashboard.razor` | Welcome + quick start |

---

## Components to Create

### 1. EmptyState.razor (Base Component)

```razor
@* Reusable empty state component *@

<div class="empty-state @SizeClass @CssClass">
    @if (Icon != null)
    {
        <div class="empty-state-icon @IconVariant">
            <i class="bi bi-@Icon"></i>
        </div>
    }

    @if (Illustration != null)
    {
        <div class="empty-state-illustration">
            @Illustration
        </div>
    }

    <div class="empty-state-title">@Title</div>

    @if (Description != null)
    {
        <div class="empty-state-description">@Description</div>
    }

    @if (ChildContent != null || PrimaryAction != null)
    {
        <div class="empty-state-actions">
            @if (PrimaryAction != null)
            {
                <button class="btn btn-primary @(Size == EmptyStateSize.Small ? "btn-sm" : "")"
                        @onclick="PrimaryAction">
                    @if (PrimaryIcon != null)
                    {
                        <i class="bi bi-@PrimaryIcon"></i>
                    }
                    @PrimaryLabel
                </button>
            }
            @if (SecondaryAction != null)
            {
                <button class="btn btn-outline @(Size == EmptyStateSize.Small ? "btn-sm" : "")"
                        @onclick="SecondaryAction">
                    @if (SecondaryIcon != null)
                    {
                        <i class="bi bi-@SecondaryIcon"></i>
                    }
                    @SecondaryLabel
                </button>
            }
            @ChildContent
        </div>
    }
</div>

@code {
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string IconVariant { get; set; } = ""; // "success", "primary", "warning"
    [Parameter] public RenderFragment? Illustration { get; set; }
    [Parameter] public string Title { get; set; } = "No items";
    [Parameter] public string? Description { get; set; }
    [Parameter] public EmptyStateSize Size { get; set; } = EmptyStateSize.Medium;
    [Parameter] public string? CssClass { get; set; }

    [Parameter] public string? PrimaryLabel { get; set; }
    [Parameter] public string? PrimaryIcon { get; set; }
    [Parameter] public EventCallback PrimaryAction { get; set; }

    [Parameter] public string? SecondaryLabel { get; set; }
    [Parameter] public string? SecondaryIcon { get; set; }
    [Parameter] public EventCallback SecondaryAction { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string SizeClass => Size switch
    {
        EmptyStateSize.Small => "empty-state-sm",
        EmptyStateSize.Large => "empty-state-lg",
        _ => ""
    };
}
```

### 2. EmptyStateSize Enum

```csharp
public enum EmptyStateSize
{
    Small,   // For sidebars, small panels
    Medium,  // Default
    Large    // For main content areas
}
```

---

## Specific Empty States

### EmptyCanvas.razor

```razor
<div class="canvas-empty">
    <EmptyState
        Size="EmptyStateSize.Large"
        Title="Start Building Your Form"
        Description="Drag fields from the left panel or click below to add your first field.">

        <Illustration>
            <div class="illustration-canvas">
                <div class="illustration-field"></div>
                <div class="illustration-field"></div>
                <div class="illustration-field"></div>
                <div class="illustration-plus"><i class="bi bi-plus"></i></div>
            </div>
        </Illustration>

        <ChildContent>
            <button class="btn btn-primary" @onclick="OnAddField">
                <i class="bi bi-plus-lg"></i> Add First Field
            </button>
            <button class="btn btn-outline" @onclick="OnImport">
                <i class="bi bi-cloud-arrow-down"></i> Import JSON
            </button>
        </ChildContent>
    </EmptyState>
</div>

@code {
    [Parameter] public EventCallback OnAddField { get; set; }
    [Parameter] public EventCallback OnImport { get; set; }
}
```

### EmptyWorkflow.razor

```razor
<EmptyState
    Icon="diagram-3"
    IconVariant="primary"
    Title="Create Your Workflow"
    Description="Add modules to build a multi-step workflow. Drag to arrange the order."
    PrimaryLabel="Add Module"
    PrimaryIcon="plus"
    PrimaryAction="OnAddModule" />

@code {
    [Parameter] public EventCallback OnAddModule { get; set; }
}
```

### ValidationSuccess.razor

```razor
<EmptyState
    Icon="check-lg"
    IconVariant="success"
    Title="All Checks Passed!"
    Description="Your form is valid and ready to publish. No issues detected."
    Size="EmptyStateSize.Small" />
```

### NoSearchResults.razor

```razor
<EmptyState
    Icon="search"
    Title="No Results Found"
    Description="@($"We couldn't find any fields matching \"{SearchTerm}\". Try a different search term.")"
    Size="EmptyStateSize.Small"
    SecondaryLabel="Clear Search"
    SecondaryIcon="x"
    SecondaryAction="OnClear" />

@code {
    [Parameter] public string SearchTerm { get; set; } = "";
    [Parameter] public EventCallback OnClear { get; set; }
}
```

---

## CSS Styles

```css
/* ===== EMPTY STATE STYLES ===== */

.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 48px 24px;
    text-align: center;
}

.empty-state-sm {
    padding: 24px 16px;
}

.empty-state-lg {
    padding: 64px 32px;
}

/* Icon */
.empty-state-icon {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    background: var(--bg-tertiary);
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 20px;
}

.empty-state-icon i {
    font-size: 32px;
    color: var(--text-muted);
}

.empty-state-sm .empty-state-icon {
    width: 56px;
    height: 56px;
    margin-bottom: 16px;
}

.empty-state-sm .empty-state-icon i {
    font-size: 24px;
}

/* Icon Variants */
.empty-state-icon.success {
    background: var(--success-light);
}
.empty-state-icon.success i {
    color: var(--success);
}

.empty-state-icon.primary {
    background: var(--primary-light);
}
.empty-state-icon.primary i {
    color: var(--primary);
}

.empty-state-icon.warning {
    background: var(--warning-light);
}
.empty-state-icon.warning i {
    color: var(--warning);
}

/* Title */
.empty-state-title {
    font-size: 18px;
    font-weight: 700;
    margin-bottom: 8px;
    color: var(--text-primary);
}

.empty-state-sm .empty-state-title {
    font-size: 15px;
}

/* Description */
.empty-state-description {
    font-size: 14px;
    color: var(--text-secondary);
    max-width: 320px;
    margin-bottom: 20px;
    line-height: 1.6;
}

.empty-state-sm .empty-state-description {
    font-size: 13px;
    max-width: 280px;
    margin-bottom: 16px;
}

/* Actions */
.empty-state-actions {
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
    justify-content: center;
}

/* Illustration */
.empty-state-illustration {
    margin-bottom: 20px;
}

/* Canvas-specific empty state */
.canvas-empty {
    background: var(--bg-secondary);
    border: 2px dashed var(--border-color);
    border-radius: var(--radius-xl);
    min-height: 300px;
    transition: border-color 0.2s;
}

.canvas-empty:hover {
    border-color: var(--primary);
}

/* Workflow-specific empty state */
.workflow-empty {
    background: radial-gradient(var(--border-color) 1px, transparent 1px);
    background-size: 20px 20px;
    background-color: var(--bg-tertiary);
    border-radius: var(--radius-lg);
    min-height: 300px;
}

/* Illustration: Canvas with fields */
.illustration-canvas {
    width: 120px;
    height: 100px;
    background: var(--bg-primary);
    border: 2px solid var(--border-color);
    border-radius: var(--radius-lg);
    position: relative;
    overflow: hidden;
}

.illustration-field {
    position: absolute;
    height: 16px;
    background: var(--bg-tertiary);
    border-radius: 4px;
    left: 10%;
    right: 10%;
}

.illustration-field:nth-child(1) { top: 15%; width: 50%; }
.illustration-field:nth-child(2) { top: 40%; }
.illustration-field:nth-child(3) { top: 65%; width: 70%; }

.illustration-plus {
    position: absolute;
    bottom: -8px;
    right: -8px;
    width: 32px;
    height: 32px;
    background: var(--primary);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 16px;
    box-shadow: var(--shadow-md);
}
```

---

## Onboarding Component

### FirstTimeOnboarding.razor

```razor
@if (IsVisible)
{
    <div class="onboarding-overlay @(IsAnimating ? "active" : "")" @onclick="Skip">
        <div class="onboarding-card" @onclick:stopPropagation>
            <div class="onboarding-icon">
                <i class="bi bi-@Steps[CurrentStep].Icon"></i>
            </div>
            <div class="onboarding-title">@Steps[CurrentStep].Title</div>
            <div class="onboarding-description">@Steps[CurrentStep].Description</div>

            <div class="onboarding-steps">
                @for (int i = 0; i < Steps.Length; i++)
                {
                    var index = i;
                    <div class="onboarding-step @(index == CurrentStep ? "active" : "")"></div>
                }
            </div>

            <div class="onboarding-actions">
                <button class="btn btn-outline" @onclick="Skip">Skip Tour</button>
                @if (CurrentStep < Steps.Length - 1)
                {
                    <button class="btn btn-primary" @onclick="Next">
                        Next <i class="bi bi-arrow-right"></i>
                    </button>
                }
                else
                {
                    <button class="btn btn-primary" @onclick="Complete">
                        <i class="bi bi-check"></i> Get Started
                    </button>
                }
            </div>
        </div>
    </div>
}

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback OnComplete { get; set; }

    private bool IsAnimating = false;
    private int CurrentStep = 0;

    private OnboardingStep[] Steps = new[]
    {
        new OnboardingStep("stars", "Welcome to Form Builder!", "Create professional forms with our drag-and-drop editor."),
        new OnboardingStep("grid", "Drag & Drop Fields", "Choose from 15+ field types and drag them onto your canvas."),
        new OnboardingStep("sliders", "Configure Properties", "Click any field to customize its settings in the right panel."),
        new OnboardingStep("check-circle", "Preview & Publish", "Preview your form and export when ready.")
    };

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible)
        {
            await Task.Delay(50);
            IsAnimating = true;
        }
    }

    private void Next() => CurrentStep++;

    private async Task Skip()
    {
        IsAnimating = false;
        await Task.Delay(300);
        await OnComplete.InvokeAsync();
    }

    private async Task Complete() => await Skip();

    private record OnboardingStep(string Icon, string Title, string Description);
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement empty state components for my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Shared/

## Components to Create

### 1. EmptyState.razor (Base Component)
Reusable empty state with:
- Icon (with variants: default, success, primary, warning)
- Illustration slot
- Title
- Description
- Primary/Secondary action buttons
- Size variants (Small, Medium, Large)

Parameters:
```csharp
[Parameter] public string? Icon { get; set; }
[Parameter] public string IconVariant { get; set; } = "";
[Parameter] public RenderFragment? Illustration { get; set; }
[Parameter] public string Title { get; set; } = "No items";
[Parameter] public string? Description { get; set; }
[Parameter] public EmptyStateSize Size { get; set; } = EmptyStateSize.Medium;
[Parameter] public string? PrimaryLabel { get; set; }
[Parameter] public string? PrimaryIcon { get; set; }
[Parameter] public EventCallback PrimaryAction { get; set; }
[Parameter] public string? SecondaryLabel { get; set; }
[Parameter] public string? SecondaryIcon { get; set; }
[Parameter] public EventCallback SecondaryAction { get; set; }
[Parameter] public RenderFragment? ChildContent { get; set; }
```

### 2. Specific Empty States
Create these specific implementations:
- EmptyCanvas.razor - For empty module editor canvas
- EmptyWorkflow.razor - For empty workflow designer
- EmptyOutline.razor - For empty field outline (small)
- ValidationSuccess.razor - For no validation errors (success variant)
- NoSearchResults.razor - For search with no results
- EmptyCodeSet.razor - For empty code set items

### 3. FirstTimeOnboarding.razor
Multi-step onboarding overlay with:
- Step indicator dots
- Icon, title, description for each step
- Skip and Next/Complete buttons
- Smooth fade in/out animations

### 4. CSS
Create empty-states.css with:
- .empty-state base styles
- Size variants (.empty-state-sm, .empty-state-lg)
- Icon variants (.success, .primary, .warning)
- Illustration styles
- Onboarding overlay styles

## Design Guidelines
- Use Bootstrap Icons
- Match existing color scheme (primary: #6366F1, success: #10B981)
- Descriptions should be helpful and action-oriented
- Include call-to-action buttons where appropriate
- Small empty states for sidebars (24px padding)
- Large empty states for main content (64px padding)

Please implement complete, production-ready code.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `EmptyStates-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing each empty state component
- Scenarios to trigger each empty state
- Expected visual appearance and behavior
- Button/action click testing
- Onboarding flow testing steps
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Component registrations needed
- Where to place EmptyCanvas, EmptyWorkflow components
- LocalStorage setup for onboarding state
- Event handler wiring for actions
- Integration with existing canvas/workflow components

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] EmptyState base component renders correctly
- [ ] All size variants working (Small, Medium, Large)
- [ ] All icon variants working (success, primary, warning)
- [ ] EmptyCanvas displays in empty module editor
- [ ] EmptyWorkflow displays in empty workflow designer
- [ ] Onboarding overlay shows on first visit
- [ ] Onboarding navigation works
- [ ] Dark mode styling correct
- [ ] Accessibility attributes present

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] EmptyCanvas displays in empty module editor
- [ ] EmptyWorkflow displays in empty workflow designer
- [ ] Outline panel shows empty state when no fields
- [ ] Validation panel shows success when no issues
- [ ] Search shows empty state when no results
- [ ] CodeSet shows empty state when no items
- [ ] Primary action buttons work
- [ ] Secondary action buttons work
- [ ] Onboarding shows on first visit
- [ ] Onboarding step navigation works
- [ ] Onboarding skip works
- [ ] Dark mode styling correct
- [ ] Animations smooth

---

## Notes

- Store onboarding completion in localStorage/user preferences
- Empty states should feel encouraging, not discouraging
- Include illustrations where appropriate for visual appeal
- Actions should always be clear and specific
- Consider screen reader accessibility for empty states
