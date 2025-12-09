# E.1 Workflow Node Types - Implementation Plan

> **Task**: Workflow Node Type Components
> **Location**: `Src/VisualEditorOpus/Components/Workflow/Nodes/`
> **Priority**: Medium
> **Estimated Effort**: 4-5 hours
> **Delegation**: 90% AI

---

## Overview

The Workflow Node Types are specialized components that represent different stages and decision points in a workflow. Each node type has unique styling, connection handles, and configuration options.

---

## Node Types

| Node Type | Purpose | Handles | Color |
|-----------|---------|---------|-------|
| **Start** | Entry point, triggers workflow | Bottom only | Green |
| **End** | Exit point, completion action | Top only | Gray |
| **Step** | Form module/stage | Top + Bottom | Indigo |
| **Decision** | Conditional branching | All 4 sides | Orange |
| **Action** | Automated action (email, API) | Top + Bottom | Purple |

---

## Components to Create

### WfNodeBase.razor (Base Component)

```razor
@namespace VisualEditorOpus.Components.Workflow.Nodes

<div class="wf-node @NodeTypeClass @(IsSelected ? "selected" : "")"
     style="left: @(Position.X)px; top: @(Position.Y)px;"
     @onclick="HandleClick"
     @onclick:stopPropagation
     @onmousedown="HandleMouseDown">

    <div class="wf-node-header">
        <div class="wf-node-icon">
            <i class="bi bi-@IconName"></i>
        </div>
        <span class="wf-node-title">@Title</span>
        <button class="wf-node-menu" @onclick="ShowMenu" @onclick:stopPropagation>
            <i class="bi bi-three-dots-vertical"></i>
        </button>
    </div>

    <div class="wf-node-body">
        @ChildContent
    </div>

    @if (ShowFooter)
    {
        <div class="wf-node-footer">
            @FooterContent
        </div>
    }

    @* Connection Handles *@
    @if (HasTopHandle)
    {
        <div class="wf-handle top"
             @onmousedown="e => StartConnection(e, HandlePosition.Top)"
             @onmousedown:stopPropagation></div>
    }
    @if (HasBottomHandle)
    {
        <div class="wf-handle bottom"
             @onmousedown="e => StartConnection(e, HandlePosition.Bottom)"
             @onmousedown:stopPropagation></div>
    }
    @if (HasLeftHandle)
    {
        <div class="wf-handle left"
             @onmousedown="e => StartConnection(e, HandlePosition.Left)"
             @onmousedown:stopPropagation></div>
    }
    @if (HasRightHandle)
    {
        <div class="wf-handle right"
             @onmousedown="e => StartConnection(e, HandlePosition.Right)"
             @onmousedown:stopPropagation></div>
    }
</div>

@code {
    [Parameter] public string Id { get; set; } = "";
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string IconName { get; set; } = "circle";
    [Parameter] public Point Position { get; set; } = new(0, 0);
    [Parameter] public bool IsSelected { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }
    [Parameter] public bool ShowFooter { get; set; } = true;

    [Parameter] public bool HasTopHandle { get; set; } = true;
    [Parameter] public bool HasBottomHandle { get; set; } = true;
    [Parameter] public bool HasLeftHandle { get; set; } = false;
    [Parameter] public bool HasRightHandle { get; set; } = false;

    [Parameter] public EventCallback<string> OnSelected { get; set; }
    [Parameter] public EventCallback<(string NodeId, Point Position)> OnPositionChanged { get; set; }
    [Parameter] public EventCallback<(string NodeId, HandlePosition Handle)> OnConnectionStart { get; set; }
    [Parameter] public EventCallback OnMenuRequested { get; set; }

    protected virtual string NodeTypeClass => "wf-node-base";

    private async Task HandleClick()
    {
        await OnSelected.InvokeAsync(Id);
    }

    private void HandleMouseDown(MouseEventArgs e)
    {
        // Initiate drag - handled by parent canvas
    }

    private async Task StartConnection(MouseEventArgs e, HandlePosition handle)
    {
        await OnConnectionStart.InvokeAsync((Id, handle));
    }

    private async Task ShowMenu()
    {
        await OnMenuRequested.InvokeAsync();
    }
}
```

### WfNodeStart.razor

```razor
@namespace VisualEditorOpus.Components.Workflow.Nodes
@inherits WfNodeBase

<WfNodeBase
    Id="@Id"
    Title="@(string.IsNullOrEmpty(Node?.Name) ? "Start" : Node.Name)"
    IconName="play-fill"
    Position="@Position"
    IsSelected="@IsSelected"
    HasTopHandle="false"
    HasBottomHandle="true"
    HasLeftHandle="false"
    HasRightHandle="false"
    OnSelected="OnSelected"
    OnPositionChanged="OnPositionChanged"
    OnConnectionStart="OnConnectionStart"
    OnMenuRequested="OnMenuRequested">

    <ChildContent>
        <div class="start-trigger">
            @GetTriggerDescription()
        </div>
    </ChildContent>

    <FooterContent>
        <span>Trigger: @GetTriggerType()</span>
    </FooterContent>
</WfNodeBase>

@code {
    [Parameter] public WorkflowNode? Node { get; set; }

    protected override string NodeTypeClass => "wf-node-start";

    private string GetTriggerType()
    {
        return Node?.TriggerType switch
        {
            TriggerType.OnSubmit => "On Submit",
            TriggerType.OnSave => "On Save",
            TriggerType.OnLoad => "On Load",
            TriggerType.Manual => "Manual",
            TriggerType.Scheduled => "Scheduled",
            _ => "On Submit"
        };
    }

    private string GetTriggerDescription()
    {
        return Node?.TriggerType switch
        {
            TriggerType.OnSubmit => "Workflow begins when form is submitted",
            TriggerType.OnSave => "Workflow begins when form is saved",
            TriggerType.OnLoad => "Workflow begins when form loads",
            TriggerType.Manual => "Workflow is triggered manually",
            TriggerType.Scheduled => $"Runs on schedule: {Node?.ScheduleCron}",
            _ => "Workflow begins here"
        };
    }
}
```

### WfNodeEnd.razor

```razor
@namespace VisualEditorOpus.Components.Workflow.Nodes
@inherits WfNodeBase

<WfNodeBase
    Id="@Id"
    Title="@(string.IsNullOrEmpty(Node?.Name) ? "End" : Node.Name)"
    IconName="stop-fill"
    Position="@Position"
    IsSelected="@IsSelected"
    HasTopHandle="true"
    HasBottomHandle="false"
    HasLeftHandle="false"
    HasRightHandle="false"
    OnSelected="OnSelected"
    OnPositionChanged="OnPositionChanged"
    OnConnectionStart="OnConnectionStart"
    OnMenuRequested="OnMenuRequested">

    <ChildContent>
        <div class="end-action">
            @GetCompletionDescription()
        </div>
    </ChildContent>

    <FooterContent>
        <span>Action: @GetCompletionAction()</span>
    </FooterContent>
</WfNodeBase>

@code {
    [Parameter] public WorkflowNode? Node { get; set; }

    protected override string NodeTypeClass => "wf-node-end";

    private string GetCompletionAction()
    {
        return Node?.CompletionAction switch
        {
            CompletionAction.Submit => "Submit",
            CompletionAction.Save => "Save",
            CompletionAction.Redirect => "Redirect",
            CompletionAction.ShowMessage => "Show Message",
            CompletionAction.CallApi => "Call API",
            _ => "Submit"
        };
    }

    private string GetCompletionDescription()
    {
        return Node?.CompletionAction switch
        {
            CompletionAction.Submit => "Submit form data and complete",
            CompletionAction.Save => "Save as draft and exit",
            CompletionAction.Redirect => $"Redirect to: {Node?.RedirectUrl}",
            CompletionAction.ShowMessage => Node?.CompletionMessage ?? "Thank you!",
            CompletionAction.CallApi => $"POST to: {Node?.ApiEndpoint}",
            _ => "Workflow complete"
        };
    }
}
```

### WfNodeStep.razor

```razor
@namespace VisualEditorOpus.Components.Workflow.Nodes
@inherits WfNodeBase

<WfNodeBase
    Id="@Id"
    Title="@(Module?.TitleEn ?? Node?.Name ?? "Step")"
    IconName="ui-checks"
    Position="@Position"
    IsSelected="@IsSelected"
    HasTopHandle="true"
    HasBottomHandle="true"
    HasLeftHandle="false"
    HasRightHandle="false"
    OnSelected="OnSelected"
    OnPositionChanged="OnPositionChanged"
    OnConnectionStart="OnConnectionStart"
    OnMenuRequested="OnMenuRequested">

    <ChildContent>
        <div class="step-description">
            @(Module?.DescriptionEn ?? "Configure this step")
        </div>
        @if (Node?.SkipCondition != null)
        {
            <div class="step-skip-indicator">
                <i class="bi bi-skip-forward"></i>
                Has skip condition
            </div>
        }
    </ChildContent>

    <FooterContent>
        <span>@(Module?.Fields?.Count ?? 0) fields</span>
        @if (Node?.IsRequired == true)
        {
            <span class="step-required">Required</span>
        }
    </FooterContent>
</WfNodeBase>

@code {
    [Parameter] public WorkflowNode? Node { get; set; }
    [Parameter] public FormModuleSchema? Module { get; set; }

    protected override string NodeTypeClass => "wf-node-step";
}
```

### WfNodeDecision.razor

```razor
@namespace VisualEditorOpus.Components.Workflow.Nodes
@inherits WfNodeBase

<WfNodeBase
    Id="@Id"
    Title="@(Node?.Name ?? "Decision")"
    IconName="signpost-split"
    Position="@Position"
    IsSelected="@IsSelected"
    HasTopHandle="true"
    HasBottomHandle="true"
    HasLeftHandle="true"
    HasRightHandle="true"
    OnSelected="OnSelected"
    OnPositionChanged="OnPositionChanged"
    OnConnectionStart="OnConnectionStart"
    OnMenuRequested="OnMenuRequested">

    <ChildContent>
        <div class="decision-condition">
            @GetConditionSummary()
        </div>
        <div class="decision-branches">
            <span class="decision-branch yes">
                <i class="bi bi-check-circle-fill"></i>
                @(Node?.YesBranchLabel ?? "Yes")
            </span>
            <span class="decision-branch no">
                <i class="bi bi-x-circle-fill"></i>
                @(Node?.NoBranchLabel ?? "No")
            </span>
        </div>
    </ChildContent>

    <FooterContent>
        <span>@GetBranchCount() branches</span>
    </FooterContent>
</WfNodeBase>

@code {
    [Parameter] public WorkflowNode? Node { get; set; }

    protected override string NodeTypeClass => "wf-node-decision";

    private string GetConditionSummary()
    {
        if (Node?.Condition == null) return "No condition set";

        var condition = Node.Condition;
        if (condition.Groups?.Any() == true)
        {
            var firstGroup = condition.Groups.First();
            var firstRule = firstGroup.Rules?.FirstOrDefault();
            if (firstRule != null)
            {
                return $"If {firstRule.FieldId} {GetOperatorSymbol(firstRule.Operator)} {firstRule.Value}";
            }
        }

        return "Complex condition";
    }

    private string GetOperatorSymbol(ComparisonOperator op) => op switch
    {
        ComparisonOperator.Equals => "=",
        ComparisonOperator.NotEquals => "!=",
        ComparisonOperator.GreaterThan => ">",
        ComparisonOperator.LessThan => "<",
        ComparisonOperator.GreaterThanOrEquals => ">=",
        ComparisonOperator.LessThanOrEquals => "<=",
        ComparisonOperator.Contains => "contains",
        ComparisonOperator.StartsWith => "starts with",
        ComparisonOperator.EndsWith => "ends with",
        _ => "?"
    };

    private int GetBranchCount()
    {
        // Count connected branches
        return 2; // Default Yes/No
    }
}
```

### WfNodeAction.razor

```razor
@namespace VisualEditorOpus.Components.Workflow.Nodes
@inherits WfNodeBase

<WfNodeBase
    Id="@Id"
    Title="@(Node?.Name ?? "Action")"
    IconName="@GetActionIcon()"
    Position="@Position"
    IsSelected="@IsSelected"
    HasTopHandle="true"
    HasBottomHandle="true"
    HasLeftHandle="false"
    HasRightHandle="false"
    OnSelected="OnSelected"
    OnPositionChanged="OnPositionChanged"
    OnConnectionStart="OnConnectionStart"
    OnMenuRequested="OnMenuRequested">

    <ChildContent>
        <div class="action-description">
            @GetActionDescription()
        </div>
        @if (Node?.ActionType == ActionType.SendEmail)
        {
            <div class="action-recipients">
                <i class="bi bi-envelope"></i>
                @Node?.EmailRecipients
            </div>
        }
    </ChildContent>

    <FooterContent>
        <span>@GetActionTypeLabel()</span>
    </FooterContent>
</WfNodeBase>

@code {
    [Parameter] public WorkflowNode? Node { get; set; }

    protected override string NodeTypeClass => "wf-node-action";

    private string GetActionIcon() => Node?.ActionType switch
    {
        ActionType.SendEmail => "envelope-fill",
        ActionType.CallApi => "globe",
        ActionType.SetFieldValue => "pencil-square",
        ActionType.CreateRecord => "plus-circle",
        ActionType.UpdateRecord => "arrow-repeat",
        ActionType.DeleteRecord => "trash",
        ActionType.SendNotification => "bell-fill",
        ActionType.RunScript => "code-slash",
        _ => "lightning-fill"
    };

    private string GetActionTypeLabel() => Node?.ActionType switch
    {
        ActionType.SendEmail => "Email Action",
        ActionType.CallApi => "API Call",
        ActionType.SetFieldValue => "Set Value",
        ActionType.CreateRecord => "Create Record",
        ActionType.UpdateRecord => "Update Record",
        ActionType.DeleteRecord => "Delete Record",
        ActionType.SendNotification => "Notification",
        ActionType.RunScript => "Script",
        _ => "Action"
    };

    private string GetActionDescription() => Node?.ActionType switch
    {
        ActionType.SendEmail => $"Send email to {Node?.EmailRecipients}",
        ActionType.CallApi => $"{Node?.ApiMethod} {Node?.ApiEndpoint}",
        ActionType.SetFieldValue => $"Set {Node?.TargetFieldId} = {Node?.FieldValue}",
        ActionType.CreateRecord => $"Create in {Node?.TargetTable}",
        ActionType.UpdateRecord => $"Update {Node?.TargetTable}",
        ActionType.DeleteRecord => $"Delete from {Node?.TargetTable}",
        ActionType.SendNotification => Node?.NotificationMessage ?? "Send notification",
        ActionType.RunScript => "Execute custom script",
        _ => "Configure action"
    };
}
```

---

## Supporting Types

```csharp
// Enums/HandlePosition.cs
public enum HandlePosition
{
    Top,
    Bottom,
    Left,
    Right
}

// Enums/TriggerType.cs
public enum TriggerType
{
    OnSubmit,
    OnSave,
    OnLoad,
    Manual,
    Scheduled
}

// Enums/CompletionAction.cs
public enum CompletionAction
{
    Submit,
    Save,
    Redirect,
    ShowMessage,
    CallApi
}

// Enums/ActionType.cs
public enum ActionType
{
    SendEmail,
    CallApi,
    SetFieldValue,
    CreateRecord,
    UpdateRecord,
    DeleteRecord,
    SendNotification,
    RunScript
}

// Models/WorkflowNode.cs
public record WorkflowNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "";
    public WorkflowNodeType Type { get; init; }
    public Point Position { get; init; } = new(0, 0);

    // Start node properties
    public TriggerType? TriggerType { get; init; }
    public string? ScheduleCron { get; init; }

    // End node properties
    public CompletionAction? CompletionAction { get; init; }
    public string? CompletionMessage { get; init; }
    public string? RedirectUrl { get; init; }
    public string? ApiEndpoint { get; init; }

    // Step node properties
    public string? ModuleId { get; init; }
    public bool IsRequired { get; init; }
    public ConditionGroup? SkipCondition { get; init; }

    // Decision node properties
    public ConditionGroup? Condition { get; init; }
    public string? YesBranchLabel { get; init; }
    public string? NoBranchLabel { get; init; }

    // Action node properties
    public ActionType? ActionType { get; init; }
    public string? EmailRecipients { get; init; }
    public string? EmailSubject { get; init; }
    public string? EmailBody { get; init; }
    public string? ApiMethod { get; init; }
    public string? TargetFieldId { get; init; }
    public string? FieldValue { get; init; }
    public string? TargetTable { get; init; }
    public string? NotificationMessage { get; init; }
    public string? ScriptContent { get; init; }
}

public enum WorkflowNodeType
{
    Start,
    End,
    Step,
    Decision,
    Action
}

// Models/Point.cs
public record Point(double X, double Y);
```

---

## CSS Styles

```css
/* ===== NODE BASE STYLES ===== */
.wf-node {
    position: absolute;
    display: flex;
    flex-direction: column;
    background: var(--bg-primary);
    border: 2px solid var(--border-color);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-md);
    cursor: move;
    transition: all 0.2s;
    min-width: 200px;
    z-index: 1;
}

.wf-node:hover {
    box-shadow: var(--shadow-lg);
    z-index: 10;
}

.wf-node.selected {
    box-shadow: 0 0 0 3px var(--primary-light), var(--shadow-lg);
    z-index: 20;
}

.wf-node-header {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px 16px;
    border-bottom: 1px solid var(--border-color);
    border-radius: var(--radius-lg) var(--radius-lg) 0 0;
}

.wf-node-icon {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--radius-md);
    font-size: 16px;
}

.wf-node-title {
    flex: 1;
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary);
}

.wf-node-menu {
    padding: 4px;
    background: transparent;
    border: none;
    cursor: pointer;
    color: var(--text-muted);
    border-radius: var(--radius-sm);
}

.wf-node-menu:hover {
    background: var(--bg-tertiary);
    color: var(--text-primary);
}

.wf-node-body {
    padding: 12px 16px;
    font-size: 12px;
    color: var(--text-secondary);
}

.wf-node-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 8px 16px;
    background: var(--bg-tertiary);
    border-radius: 0 0 var(--radius-lg) var(--radius-lg);
    font-size: 11px;
    color: var(--text-muted);
}

/* ===== CONNECTION HANDLES ===== */
.wf-handle {
    position: absolute;
    width: 12px;
    height: 12px;
    background: var(--bg-primary);
    border: 2px solid var(--border-color);
    border-radius: 50%;
    cursor: crosshair;
    transition: all 0.15s;
    z-index: 10;
}

.wf-handle:hover {
    transform: scale(1.3);
    border-color: var(--primary);
    background: var(--primary-light);
}

.wf-handle.top {
    top: -6px;
    left: 50%;
    transform: translateX(-50%);
}

.wf-handle.top:hover {
    transform: translateX(-50%) scale(1.3);
}

.wf-handle.bottom {
    bottom: -6px;
    left: 50%;
    transform: translateX(-50%);
}

.wf-handle.bottom:hover {
    transform: translateX(-50%) scale(1.3);
}

.wf-handle.left {
    left: -6px;
    top: 50%;
    transform: translateY(-50%);
}

.wf-handle.left:hover {
    transform: translateY(-50%) scale(1.3);
}

.wf-handle.right {
    right: -6px;
    top: 50%;
    transform: translateY(-50%);
}

.wf-handle.right:hover {
    transform: translateY(-50%) scale(1.3);
}

/* ===== START NODE ===== */
.wf-node-start {
    border-color: var(--success);
}

.wf-node-start .wf-node-header {
    background: rgba(16, 185, 129, 0.1);
}

.wf-node-start .wf-node-icon {
    background: var(--success);
    color: white;
}

.wf-node-start .wf-handle {
    border-color: var(--success);
}

/* ===== END NODE ===== */
.wf-node-end {
    border-color: #6B7280;
}

.wf-node-end .wf-node-header {
    background: rgba(107, 114, 128, 0.1);
}

.wf-node-end .wf-node-icon {
    background: #6B7280;
    color: white;
}

.wf-node-end .wf-handle {
    border-color: #6B7280;
}

/* ===== STEP NODE ===== */
.wf-node-step {
    border-color: var(--primary);
}

.wf-node-step .wf-node-header {
    background: var(--primary-light);
}

.wf-node-step .wf-node-icon {
    background: var(--primary);
    color: white;
}

.wf-node-step .wf-handle {
    border-color: var(--primary);
}

.step-required {
    color: var(--error);
    font-weight: 600;
}

.step-skip-indicator {
    display: flex;
    align-items: center;
    gap: 6px;
    margin-top: 8px;
    padding: 4px 8px;
    background: var(--warning-light);
    color: var(--warning);
    border-radius: var(--radius-sm);
    font-size: 11px;
}

/* ===== DECISION NODE ===== */
.wf-node-decision {
    border-color: #F59E0B;
    min-width: 220px;
}

.wf-node-decision .wf-node-header {
    background: rgba(245, 158, 11, 0.1);
}

.wf-node-decision .wf-node-icon {
    background: #F59E0B;
    color: white;
}

.wf-node-decision .wf-handle {
    border-color: #F59E0B;
}

.decision-condition {
    font-family: 'Monaco', 'Menlo', monospace;
    font-size: 12px;
    color: var(--text-primary);
}

.decision-branches {
    display: flex;
    gap: 8px;
    margin-top: 8px;
}

.decision-branch {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 4px 8px;
    border-radius: var(--radius-sm);
    font-size: 11px;
    font-weight: 500;
}

.decision-branch.yes {
    background: rgba(16, 185, 129, 0.1);
    color: var(--success);
}

.decision-branch.no {
    background: rgba(239, 68, 68, 0.1);
    color: var(--error);
}

/* ===== ACTION NODE ===== */
.wf-node-action {
    border-color: #8B5CF6;
}

.wf-node-action .wf-node-header {
    background: rgba(139, 92, 246, 0.1);
}

.wf-node-action .wf-node-icon {
    background: #8B5CF6;
    color: white;
}

.wf-node-action .wf-handle {
    border-color: #8B5CF6;
}

.action-recipients {
    display: flex;
    align-items: center;
    gap: 6px;
    margin-top: 8px;
    font-size: 11px;
    color: var(--text-muted);
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the Workflow Node Type components for my Blazor workflow designer.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Workflow/Nodes/

## Components to Create:

### 1. WfNodeBase.razor
Base component with:
- Draggable node container
- Header with icon, title, menu
- Body content slot
- Footer content slot
- Connection handles (configurable per side)
- Selection state

### 2. WfNodeStart.razor
Start node with:
- Green styling
- Only bottom handle
- Trigger type display (OnSubmit, OnSave, Scheduled, etc.)
- Trigger description

### 3. WfNodeEnd.razor
End node with:
- Gray styling
- Only top handle
- Completion action display (Submit, Save, Redirect, etc.)
- Action description

### 4. WfNodeStep.razor
Step/Form node with:
- Indigo styling
- Top and bottom handles
- Module name and description
- Field count display
- Required indicator
- Skip condition indicator

### 5. WfNodeDecision.razor
Decision/Branch node with:
- Orange styling
- All 4 handles (top, bottom, left, right)
- Condition summary
- Yes/No branch labels
- Branch count

### 6. WfNodeAction.razor
Action node with:
- Purple styling
- Top and bottom handles
- Action type icon and label
- Action description
- Recipients for email actions

### Parameters for all nodes:
```csharp
[Parameter] public string Id { get; set; }
[Parameter] public WorkflowNode Node { get; set; }
[Parameter] public Point Position { get; set; }
[Parameter] public bool IsSelected { get; set; }
[Parameter] public EventCallback<string> OnSelected { get; set; }
[Parameter] public EventCallback<(string, Point)> OnPositionChanged { get; set; }
[Parameter] public EventCallback<(string, HandlePosition)> OnConnectionStart { get; set; }
```

Please implement complete, production-ready code with CSS.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `WfNodeTypes-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing each node type (Start, End, Step, Decision, Action)
- Node selection and highlighting testing
- Connection handle visibility and hover states
- Node dragging functionality testing
- Menu button interaction testing
- Node type-specific content display testing
- Color scheme verification for each node type

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Enum file creation locations (HandlePosition, TriggerType, CompletionAction, ActionType, WorkflowNodeType)
- WorkflowNode model file creation
- Point record creation
- CSS file imports and organization
- Component registration in _Imports.razor
- Integration with parent WorkflowCanvas component
- Node drag-and-drop event handling in parent

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Start node displays with green styling
- [ ] Start node only shows bottom handle
- [ ] End node displays with gray styling
- [ ] End node only shows top handle
- [ ] Step node displays with indigo styling
- [ ] Step node shows top and bottom handles
- [ ] Step node shows field count
- [ ] Step node shows required indicator
- [ ] Decision node displays with orange styling
- [ ] Decision node shows all 4 handles
- [ ] Decision node shows Yes/No branches
- [ ] Action node displays with purple styling
- [ ] Action node shows correct action icon
- [ ] Nodes can be selected
- [ ] Selected state shows highlight
- [ ] Menu button shows on hover
- [ ] Handles highlight on hover
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Start node displays with green styling
- [ ] Start node only shows bottom handle
- [ ] End node displays with gray styling
- [ ] End node only shows top handle
- [ ] Step node displays with indigo styling
- [ ] Step node shows field count
- [ ] Decision node displays with orange styling
- [ ] Decision node shows all 4 handles
- [ ] Decision node shows Yes/No branches
- [ ] Action node displays with purple styling
- [ ] Action node shows correct action icon
- [ ] Nodes can be selected
- [ ] Selected state shows highlight
- [ ] Menu button shows on hover
- [ ] Handles highlight on hover
- [ ] Nodes are draggable
- [ ] Dark mode styling correct

---

## Notes

- Handles should initiate connection drawing when clicked
- Node positions should be persisted
- Consider adding node validation states
- Consider adding node execution status (running, completed, error)
- Consider adding node collapse/expand for complex workflows
- Node menu should offer: Edit, Duplicate, Delete, Disconnect
