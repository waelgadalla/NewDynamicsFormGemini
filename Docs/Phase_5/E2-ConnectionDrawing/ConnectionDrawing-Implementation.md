# E.2 Connection Drawing - Implementation Plan

> **Task**: Workflow Connection Drawing System
> **Location**: `Src/VisualEditorOpus/Components/Workflow/`
> **Priority**: Medium
> **Estimated Effort**: 3-4 hours
> **Delegation**: 80% AI

---

## Overview

The Connection Drawing system renders bezier curves between workflow nodes, handles user interaction for creating/deleting connections, and displays branch labels for decision nodes.

---

## Components to Create

### ConnectionLayer.razor (SVG Container)

```razor
@namespace VisualEditorOpus.Components.Workflow
@inject IJSRuntime JS

<svg class="connections-layer" @ref="svgRef">
    <defs>
        @* Arrow markers for different states *@
        <marker id="arrow-default" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto">
            <path d="M0,0 L0,6 L9,3 z" class="arrow-default"/>
        </marker>
        <marker id="arrow-success" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto">
            <path d="M0,0 L0,6 L9,3 z" class="arrow-success"/>
        </marker>
        <marker id="arrow-warning" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto">
            <path d="M0,0 L0,6 L9,3 z" class="arrow-warning"/>
        </marker>
        <marker id="arrow-active" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto">
            <path d="M0,0 L0,6 L9,3 z" class="arrow-active"/>
        </marker>
    </defs>

    @* Existing connections *@
    <g class="connection-paths">
        @foreach (var connection in Connections)
        {
            <ConnectionPath
                Connection="@connection"
                ConnectionType="@ConnectionType"
                IsSelected="@(SelectedConnectionId == connection.Id)"
                OnSelected="HandleConnectionSelected"
                OnDeleted="HandleConnectionDeleted" />
        }
    </g>

    @* Connection labels *@
    <g class="connection-labels">
        @foreach (var connection in Connections.Where(c => !string.IsNullOrEmpty(c.Label)))
        {
            <ConnectionLabel
                Connection="@connection"
                ConnectionType="@ConnectionType" />
        }
    </g>

    @* Drawing preview *@
    @if (IsDrawing)
    {
        <path class="connection-path connection-drawing"
              d="@DrawingPath"
              marker-end="url(#arrow-active)" />
    }
</svg>

@code {
    [Parameter] public List<WorkflowConnection> Connections { get; set; } = new();
    [Parameter] public Dictionary<string, NodePosition> NodePositions { get; set; } = new();
    [Parameter] public ConnectionType ConnectionType { get; set; } = ConnectionType.Bezier;
    [Parameter] public string? SelectedConnectionId { get; set; }

    [Parameter] public EventCallback<string> OnConnectionSelected { get; set; }
    [Parameter] public EventCallback<WorkflowConnection> OnConnectionCreated { get; set; }
    [Parameter] public EventCallback<string> OnConnectionDeleted { get; set; }

    private ElementReference svgRef;
    private bool IsDrawing { get; set; }
    private Point? DrawingStart { get; set; }
    private Point? DrawingEnd { get; set; }
    private string? DrawingSourceNodeId { get; set; }
    private HandlePosition? DrawingSourceHandle { get; set; }

    private string DrawingPath => IsDrawing && DrawingStart != null && DrawingEnd != null
        ? GeneratePath(DrawingStart.Value, DrawingEnd.Value)
        : "";

    public void StartDrawing(string nodeId, HandlePosition handle, Point position)
    {
        IsDrawing = true;
        DrawingSourceNodeId = nodeId;
        DrawingSourceHandle = handle;
        DrawingStart = position;
        DrawingEnd = position;
        StateHasChanged();
    }

    public void UpdateDrawing(Point position)
    {
        if (IsDrawing)
        {
            DrawingEnd = position;
            StateHasChanged();
        }
    }

    public async Task FinishDrawing(string? targetNodeId, HandlePosition? targetHandle, Point position)
    {
        if (IsDrawing && DrawingSourceNodeId != null && targetNodeId != null &&
            DrawingSourceNodeId != targetNodeId)
        {
            var connection = new WorkflowConnection
            {
                Id = Guid.NewGuid().ToString(),
                SourceNodeId = DrawingSourceNodeId,
                SourceHandle = DrawingSourceHandle!.Value,
                TargetNodeId = targetNodeId,
                TargetHandle = targetHandle!.Value
            };

            await OnConnectionCreated.InvokeAsync(connection);
        }

        CancelDrawing();
    }

    public void CancelDrawing()
    {
        IsDrawing = false;
        DrawingStart = null;
        DrawingEnd = null;
        DrawingSourceNodeId = null;
        DrawingSourceHandle = null;
        StateHasChanged();
    }

    private async Task HandleConnectionSelected(string connectionId)
    {
        await OnConnectionSelected.InvokeAsync(connectionId);
    }

    private async Task HandleConnectionDeleted(string connectionId)
    {
        await OnConnectionDeleted.InvokeAsync(connectionId);
    }

    private string GeneratePath(Point start, Point end)
    {
        return ConnectionType switch
        {
            ConnectionType.Bezier => GenerateBezierPath(start, end),
            ConnectionType.Step => GenerateStepPath(start, end),
            ConnectionType.SmoothStep => GenerateSmoothStepPath(start, end),
            ConnectionType.Straight => GenerateStraightPath(start, end),
            _ => GenerateBezierPath(start, end)
        };
    }

    private string GenerateBezierPath(Point start, Point end)
    {
        var midY = (start.Y + end.Y) / 2;
        return $"M {start.X},{start.Y} C {start.X},{midY} {end.X},{midY} {end.X},{end.Y}";
    }

    private string GenerateStepPath(Point start, Point end)
    {
        var midY = (start.Y + end.Y) / 2;
        return $"M {start.X},{start.Y} L {start.X},{midY} L {end.X},{midY} L {end.X},{end.Y}";
    }

    private string GenerateSmoothStepPath(Point start, Point end)
    {
        var midY = (start.Y + end.Y) / 2;
        var radius = 10;
        return $"M {start.X},{start.Y} L {start.X},{midY - radius} Q {start.X},{midY} {start.X + radius},{midY} L {end.X - radius},{midY} Q {end.X},{midY} {end.X},{midY + radius} L {end.X},{end.Y}";
    }

    private string GenerateStraightPath(Point start, Point end)
    {
        return $"M {start.X},{start.Y} L {end.X},{end.Y}";
    }
}
```

### ConnectionPath.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<g class="connection-group" @onclick="HandleClick" @onclick:stopPropagation>
    <path class="connection-path @GetConnectionClass()"
          d="@PathData"
          marker-end="@GetMarkerUrl()"
          @onmouseover="() => IsHovered = true"
          @onmouseout="() => IsHovered = false" />

    @if (IsSelected || IsHovered)
    {
        <foreignObject x="@DeleteButtonPosition.X" y="@DeleteButtonPosition.Y" width="24" height="24">
            <button class="connection-delete-btn" @onclick="HandleDelete" @onclick:stopPropagation>
                <i class="bi bi-x"></i>
            </button>
        </foreignObject>
    }
</g>

@code {
    [Parameter] public WorkflowConnection Connection { get; set; } = default!;
    [Parameter] public ConnectionType ConnectionType { get; set; }
    [Parameter] public bool IsSelected { get; set; }
    [Parameter] public EventCallback<string> OnSelected { get; set; }
    [Parameter] public EventCallback<string> OnDeleted { get; set; }

    [CascadingParameter] public Dictionary<string, NodePosition>? NodePositions { get; set; }

    private bool IsHovered { get; set; }

    private string PathData
    {
        get
        {
            if (NodePositions == null) return "";

            var sourcePos = GetHandlePosition(Connection.SourceNodeId, Connection.SourceHandle);
            var targetPos = GetHandlePosition(Connection.TargetNodeId, Connection.TargetHandle);

            if (sourcePos == null || targetPos == null) return "";

            return GeneratePath(sourcePos.Value, targetPos.Value);
        }
    }

    private Point DeleteButtonPosition
    {
        get
        {
            // Calculate midpoint of path
            var sourcePos = GetHandlePosition(Connection.SourceNodeId, Connection.SourceHandle);
            var targetPos = GetHandlePosition(Connection.TargetNodeId, Connection.TargetHandle);

            if (sourcePos == null || targetPos == null) return new Point(0, 0);

            return new Point(
                (sourcePos.Value.X + targetPos.Value.X) / 2 - 12,
                (sourcePos.Value.Y + targetPos.Value.Y) / 2 - 12
            );
        }
    }

    private Point? GetHandlePosition(string nodeId, HandlePosition handle)
    {
        if (NodePositions == null || !NodePositions.TryGetValue(nodeId, out var nodePos))
            return null;

        return handle switch
        {
            HandlePosition.Top => new Point(nodePos.X + nodePos.Width / 2, nodePos.Y),
            HandlePosition.Bottom => new Point(nodePos.X + nodePos.Width / 2, nodePos.Y + nodePos.Height),
            HandlePosition.Left => new Point(nodePos.X, nodePos.Y + nodePos.Height / 2),
            HandlePosition.Right => new Point(nodePos.X + nodePos.Width, nodePos.Y + nodePos.Height / 2),
            _ => new Point(nodePos.X + nodePos.Width / 2, nodePos.Y + nodePos.Height / 2)
        };
    }

    private string GeneratePath(Point start, Point end)
    {
        return ConnectionType switch
        {
            ConnectionType.Bezier => GenerateBezierPath(start, end),
            ConnectionType.Step => GenerateStepPath(start, end),
            ConnectionType.SmoothStep => GenerateSmoothStepPath(start, end),
            ConnectionType.Straight => $"M {start.X},{start.Y} L {end.X},{end.Y}",
            _ => GenerateBezierPath(start, end)
        };
    }

    private string GenerateBezierPath(Point start, Point end)
    {
        var controlPointOffset = Math.Abs(end.Y - start.Y) / 2;
        return $"M {start.X},{start.Y} C {start.X},{start.Y + controlPointOffset} {end.X},{end.Y - controlPointOffset} {end.X},{end.Y}";
    }

    private string GenerateStepPath(Point start, Point end)
    {
        var midY = (start.Y + end.Y) / 2;
        return $"M {start.X},{start.Y} L {start.X},{midY} L {end.X},{midY} L {end.X},{end.Y}";
    }

    private string GenerateSmoothStepPath(Point start, Point end)
    {
        var midY = (start.Y + end.Y) / 2;
        var radius = Math.Min(10, Math.Abs(end.X - start.X) / 4);
        var dir = end.X > start.X ? 1 : -1;

        return $"M {start.X},{start.Y} " +
               $"L {start.X},{midY - radius} " +
               $"Q {start.X},{midY} {start.X + dir * radius},{midY} " +
               $"L {end.X - dir * radius},{midY} " +
               $"Q {end.X},{midY} {end.X},{midY + radius} " +
               $"L {end.X},{end.Y}";
    }

    private string GetConnectionClass()
    {
        var classes = new List<string>();

        if (IsSelected) classes.Add("selected");
        if (IsHovered) classes.Add("hovered");

        // Branch type styling
        classes.Add(Connection.BranchType switch
        {
            BranchType.Yes => "success",
            BranchType.No => "warning",
            BranchType.Error => "error",
            _ => "default"
        });

        return string.Join(" ", classes);
    }

    private string GetMarkerUrl()
    {
        if (IsSelected || IsHovered) return "url(#arrow-active)";

        return Connection.BranchType switch
        {
            BranchType.Yes => "url(#arrow-success)",
            BranchType.No => "url(#arrow-warning)",
            BranchType.Error => "url(#arrow-error)",
            _ => "url(#arrow-default)"
        };
    }

    private async Task HandleClick()
    {
        await OnSelected.InvokeAsync(Connection.Id);
    }

    private async Task HandleDelete()
    {
        await OnDeleted.InvokeAsync(Connection.Id);
    }
}
```

### ConnectionLabel.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

@if (!string.IsNullOrEmpty(Connection.Label))
{
    <g class="connection-label-group">
        <rect class="connection-label-bg"
              x="@(LabelPosition.X - 15)"
              y="@(LabelPosition.Y - 8)"
              width="30"
              height="16"
              rx="4" />
        <text class="connection-label @GetLabelClass()"
              x="@LabelPosition.X"
              y="@(LabelPosition.Y + 4)"
              text-anchor="middle">
            @Connection.Label
        </text>
    </g>
}

@code {
    [Parameter] public WorkflowConnection Connection { get; set; } = default!;
    [Parameter] public ConnectionType ConnectionType { get; set; }

    [CascadingParameter] public Dictionary<string, NodePosition>? NodePositions { get; set; }

    private Point LabelPosition
    {
        get
        {
            var sourcePos = GetHandlePosition(Connection.SourceNodeId, Connection.SourceHandle);
            var targetPos = GetHandlePosition(Connection.TargetNodeId, Connection.TargetHandle);

            if (sourcePos == null || targetPos == null) return new Point(0, 0);

            // Position label at 25% of the path (closer to source)
            return new Point(
                sourcePos.Value.X + (targetPos.Value.X - sourcePos.Value.X) * 0.25,
                sourcePos.Value.Y + (targetPos.Value.Y - sourcePos.Value.Y) * 0.25
            );
        }
    }

    private Point? GetHandlePosition(string nodeId, HandlePosition handle)
    {
        if (NodePositions == null || !NodePositions.TryGetValue(nodeId, out var nodePos))
            return null;

        return handle switch
        {
            HandlePosition.Top => new Point(nodePos.X + nodePos.Width / 2, nodePos.Y),
            HandlePosition.Bottom => new Point(nodePos.X + nodePos.Width / 2, nodePos.Y + nodePos.Height),
            HandlePosition.Left => new Point(nodePos.X, nodePos.Y + nodePos.Height / 2),
            HandlePosition.Right => new Point(nodePos.X + nodePos.Width, nodePos.Y + nodePos.Height / 2),
            _ => new Point(nodePos.X + nodePos.Width / 2, nodePos.Y + nodePos.Height / 2)
        };
    }

    private string GetLabelClass() => Connection.BranchType switch
    {
        BranchType.Yes => "label-success",
        BranchType.No => "label-warning",
        BranchType.Error => "label-error",
        _ => "label-default"
    };
}
```

---

## Supporting Types

```csharp
// Models/WorkflowConnection.cs
public record WorkflowConnection
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string SourceNodeId { get; init; } = "";
    public HandlePosition SourceHandle { get; init; }
    public string TargetNodeId { get; init; } = "";
    public HandlePosition TargetHandle { get; init; }
    public string? Label { get; init; }
    public BranchType BranchType { get; init; } = BranchType.Default;
}

// Models/NodePosition.cs
public record NodePosition
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
}

// Enums/ConnectionType.cs
public enum ConnectionType
{
    Bezier,
    Step,
    SmoothStep,
    Straight
}

// Enums/BranchType.cs
public enum BranchType
{
    Default,
    Yes,
    No,
    Error
}
```

---

## CSS Styles

```css
/* ===== CONNECTION LAYER ===== */
.connections-layer {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    overflow: visible;
}

/* ===== CONNECTION PATH ===== */
.connection-path {
    fill: none;
    stroke: var(--connection-default, #6B7280);
    stroke-width: 2;
    pointer-events: stroke;
    cursor: pointer;
    transition: stroke 0.2s, stroke-width 0.2s;
}

.connection-path:hover,
.connection-path.hovered {
    stroke: var(--connection-active, #6366F1);
    stroke-width: 3;
}

.connection-path.selected {
    stroke: var(--connection-active, #6366F1);
    stroke-width: 3;
}

.connection-path.success {
    stroke: var(--connection-success, #10B981);
}

.connection-path.warning {
    stroke: var(--connection-warning, #F59E0B);
}

.connection-path.error {
    stroke: var(--connection-error, #EF4444);
}

/* ===== ARROW MARKERS ===== */
.arrow-default {
    fill: var(--connection-default, #6B7280);
}

.arrow-success {
    fill: var(--connection-success, #10B981);
}

.arrow-warning {
    fill: var(--connection-warning, #F59E0B);
}

.arrow-error {
    fill: var(--connection-error, #EF4444);
}

.arrow-active {
    fill: var(--connection-active, #6366F1);
}

/* ===== DRAWING ANIMATION ===== */
.connection-drawing {
    stroke-dasharray: 5;
    animation: connection-dash 0.5s linear infinite;
}

@keyframes connection-dash {
    to {
        stroke-dashoffset: -10;
    }
}

/* ===== CONNECTION LABELS ===== */
.connection-label-bg {
    fill: var(--bg-primary);
    stroke: var(--border-color);
    stroke-width: 1;
}

.connection-label {
    font-size: 10px;
    font-weight: 600;
    fill: var(--text-secondary);
    pointer-events: none;
    user-select: none;
}

.connection-label.label-success {
    fill: var(--success);
}

.connection-label.label-warning {
    fill: var(--warning);
}

.connection-label.label-error {
    fill: var(--error);
}

/* ===== DELETE BUTTON ===== */
.connection-delete-btn {
    width: 24px;
    height: 24px;
    background: var(--error);
    border: 2px solid var(--bg-primary);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 12px;
    cursor: pointer;
    pointer-events: auto;
    transition: transform 0.15s;
}

.connection-delete-btn:hover {
    transform: scale(1.1);
}

/* ===== CONNECTION HOVER AREA ===== */
.connection-group {
    pointer-events: auto;
}
```

---

## JavaScript Interop

```javascript
// wwwroot/js/workflow-connections.js

window.workflowConnections = {
    getPathLength: (pathElement) => {
        return pathElement.getTotalLength();
    },

    getPointAtLength: (pathElement, length) => {
        const point = pathElement.getPointAtLength(length);
        return { x: point.x, y: point.y };
    },

    animatePath: (pathElement, duration) => {
        const length = pathElement.getTotalLength();
        pathElement.style.strokeDasharray = length;
        pathElement.style.strokeDashoffset = length;
        pathElement.style.animation = `draw-path ${duration}ms ease forwards`;
    }
};

// CSS animation for path drawing
const style = document.createElement('style');
style.textContent = `
    @keyframes draw-path {
        to {
            stroke-dashoffset: 0;
        }
    }
`;
document.head.appendChild(style);
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the Connection Drawing system for my Blazor workflow designer.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Workflow/

## Components to Create:

### 1. ConnectionLayer.razor
SVG container with:
- Arrow marker definitions
- Existing connections rendering
- Connection labels
- Drawing preview line
- Start/Update/Finish drawing methods

### 2. ConnectionPath.razor
Individual connection with:
- Bezier/Step/SmoothStep/Straight path generation
- Selection and hover states
- Delete button on hover/select
- Branch type coloring (Yes=green, No=orange)

### 3. ConnectionLabel.razor
Label for branches:
- Background rect
- Text label
- Position at 25% of path
- Color based on branch type

### Path Types:
- **Bezier**: Smooth curved path (default)
- **Step**: Right-angle path
- **SmoothStep**: Right-angle with rounded corners
- **Straight**: Direct line

### Features:
- Click connection to select
- Delete button appears on selection
- Hover highlights connection
- Arrow heads at target end
- Animated dashed line while drawing
- Branch labels (Yes/No) for decision nodes

### Parameters:
```csharp
// ConnectionLayer
[Parameter] public List<WorkflowConnection> Connections { get; set; }
[Parameter] public Dictionary<string, NodePosition> NodePositions { get; set; }
[Parameter] public ConnectionType ConnectionType { get; set; }
[Parameter] public EventCallback<WorkflowConnection> OnConnectionCreated { get; set; }
[Parameter] public EventCallback<string> OnConnectionDeleted { get; set; }
```

Please implement complete, production-ready code with CSS.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `ConnectionDrawing-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing each path type (Bezier, Step, SmoothStep, Straight)
- Connection creation by clicking and dragging from handles
- Connection selection and hover states
- Delete button appearance and functionality
- Arrow marker rendering verification
- Branch type coloring (Yes=green, No=orange)
- Connection label positioning and display
- Drawing preview animation during connection creation

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- WorkflowConnection model file creation
- NodePosition model file creation
- ConnectionType and BranchType enum creation
- JavaScript interop file (workflow-connections.js) registration
- CSS file imports
- SVG marker definitions customization
- Integration with parent WorkflowCanvas component
- Event wiring for connection creation/deletion

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Bezier curves render correctly
- [ ] Step paths render correctly
- [ ] Smooth step paths render correctly
- [ ] Straight lines render correctly
- [ ] Arrow heads point to target
- [ ] Clicking selects connection
- [ ] Delete button appears on selection
- [ ] Delete removes connection
- [ ] Hover highlights path
- [ ] Drawing shows animated dashed line
- [ ] Connection snaps to handles
- [ ] Labels display at correct position
- [ ] Yes branch shows green color
- [ ] No branch shows orange color
- [ ] Connection paths update when nodes move
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Bezier curves render correctly
- [ ] Step paths render correctly
- [ ] Smooth step paths render correctly
- [ ] Straight lines render correctly
- [ ] Arrow heads point to target
- [ ] Clicking selects connection
- [ ] Delete button appears on selection
- [ ] Delete removes connection
- [ ] Hover highlights path
- [ ] Drawing shows animated dashed line
- [ ] Connection snaps to handles
- [ ] Labels display at correct position
- [ ] Yes branch shows green
- [ ] No branch shows orange
- [ ] Dark mode styling correct

---

## Notes

- SVG paths use pointer-events: stroke for click detection
- Consider adding connection validation (prevent loops)
- Consider adding connection animation on create
- Consider supporting multiple paths between same nodes
- Path generation should handle horizontal/vertical directions
- Labels should avoid overlapping nodes
