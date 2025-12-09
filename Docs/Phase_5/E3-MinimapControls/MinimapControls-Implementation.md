# E.3 Minimap & Controls - Implementation Plan

> **Task**: Workflow Minimap and Navigation Controls
> **Location**: `Src/VisualEditorOpus/Components/Workflow/`
> **Priority**: Medium
> **Estimated Effort**: 2-3 hours
> **Delegation**: 85% AI

---

## Overview

The Minimap and Controls provide navigation assistance for large workflow canvases. The minimap shows a bird's-eye view with a draggable viewport indicator, while zoom controls allow precise canvas scaling.

---

## Components to Create

### WorkflowMinimap.razor

```razor
@namespace VisualEditorOpus.Components.Workflow
@inject IJSRuntime JS

<div class="minimap @(IsCollapsed ? "collapsed" : "")">
    <div class="minimap-header">
        <span>Minimap</span>
        <button class="minimap-toggle" @onclick="ToggleCollapse">
            <i class="bi bi-chevron-@(IsCollapsed ? "up" : "down")"></i>
        </button>
    </div>

    @if (!IsCollapsed)
    {
        <div class="minimap-content" @ref="minimapRef"
             @onmousedown="HandleMinimapClick">
            @* Mini representations of nodes *@
            @foreach (var node in Nodes)
            {
                <div class="minimap-node @node.Type.ToString().ToLower()"
                     style="left: @(node.Position.X * Scale)px;
                            top: @(node.Position.Y * Scale)px;
                            width: @(NodeWidth * Scale)px;
                            height: @(NodeHeight * Scale)px;">
                </div>
            }

            @* Viewport indicator *@
            <div class="minimap-viewport"
                 style="left: @ViewportLeft px;
                        top: @ViewportTop px;
                        width: @ViewportWidth px;
                        height: @ViewportHeight px;"
                 @onmousedown="StartViewportDrag"
                 @onmousedown:stopPropagation>
            </div>
        </div>
    }
</div>

@code {
    [Parameter] public List<WorkflowNode> Nodes { get; set; } = new();
    [Parameter] public double CanvasWidth { get; set; } = 2000;
    [Parameter] public double CanvasHeight { get; set; } = 1500;
    [Parameter] public double ViewportX { get; set; }
    [Parameter] public double ViewportY { get; set; }
    [Parameter] public double ViewportW { get; set; }
    [Parameter] public double ViewportH { get; set; }
    [Parameter] public double Zoom { get; set; } = 1;
    [Parameter] public EventCallback<(double X, double Y)> OnNavigate { get; set; }

    private ElementReference minimapRef;
    private bool IsCollapsed { get; set; } = false;
    private bool IsDragging { get; set; } = false;

    private const double MinimapWidth = 200;
    private const double MinimapHeight = 150;
    private const double NodeWidth = 160;
    private const double NodeHeight = 80;

    private double Scale => Math.Min(MinimapWidth / CanvasWidth, MinimapHeight / CanvasHeight);

    private double ViewportLeft => (-ViewportX / Zoom) * Scale;
    private double ViewportTop => (-ViewportY / Zoom) * Scale;
    private double ViewportWidth => (ViewportW / Zoom) * Scale;
    private double ViewportHeight => (ViewportH / Zoom) * Scale;

    private void ToggleCollapse()
    {
        IsCollapsed = !IsCollapsed;
    }

    private async Task HandleMinimapClick(MouseEventArgs e)
    {
        var rect = await JS.InvokeAsync<BoundingClientRect>("getBoundingClientRect", minimapRef);
        var clickX = (e.ClientX - rect.Left) / Scale;
        var clickY = (e.ClientY - rect.Top) / Scale;

        var newX = -clickX * Zoom + ViewportW / 2;
        var newY = -clickY * Zoom + ViewportH / 2;

        await OnNavigate.InvokeAsync((newX, newY));
    }

    private void StartViewportDrag(MouseEventArgs e)
    {
        IsDragging = true;
    }

    public record BoundingClientRect(double Left, double Top, double Width, double Height);
}
```

### ZoomControls.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="zoom-controls">
    <button class="zoom-btn" @onclick="ZoomIn" disabled="@(Zoom >= MaxZoom)" title="Zoom In (+)">
        <i class="bi bi-plus-lg"></i>
    </button>

    <div class="zoom-level">@(Math.Round(Zoom * 100))%</div>

    <button class="zoom-btn" @onclick="ZoomOut" disabled="@(Zoom <= MinZoom)" title="Zoom Out (-)">
        <i class="bi bi-dash-lg"></i>
    </button>

    <div class="zoom-divider"></div>

    <button class="zoom-btn" @onclick="FitView" title="Fit to View">
        <i class="bi bi-aspect-ratio"></i>
    </button>

    <button class="zoom-btn" @onclick="ResetView" title="Reset View (Ctrl+0)">
        <i class="bi bi-arrow-counterclockwise"></i>
    </button>
</div>

@code {
    [Parameter] public double Zoom { get; set; } = 1;
    [Parameter] public double MinZoom { get; set; } = 0.25;
    [Parameter] public double MaxZoom { get; set; } = 2;
    [Parameter] public double ZoomStep { get; set; } = 0.1;

    [Parameter] public EventCallback<double> OnZoomChanged { get; set; }
    [Parameter] public EventCallback OnFitView { get; set; }
    [Parameter] public EventCallback OnResetView { get; set; }

    private async Task ZoomIn()
    {
        var newZoom = Math.Min(MaxZoom, Zoom + ZoomStep);
        await OnZoomChanged.InvokeAsync(newZoom);
    }

    private async Task ZoomOut()
    {
        var newZoom = Math.Max(MinZoom, Zoom - ZoomStep);
        await OnZoomChanged.InvokeAsync(newZoom);
    }

    private async Task FitView()
    {
        await OnFitView.InvokeAsync();
    }

    private async Task ResetView()
    {
        await OnResetView.InvokeAsync();
    }
}
```

### FitControls.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="fit-controls">
    <button class="fit-btn" @onclick="OnFitView" title="Fit all nodes in view">
        <i class="bi bi-arrows-fullscreen"></i>
        Fit View
    </button>

    <button class="fit-btn" @onclick="OnCenterView" title="Center the canvas">
        <i class="bi bi-fullscreen"></i>
        Center
    </button>

    <button class="fit-btn" @onclick="OnResetView" title="Reset zoom and pan">
        <i class="bi bi-arrow-counterclockwise"></i>
        Reset
    </button>
</div>

@code {
    [Parameter] public EventCallback OnFitView { get; set; }
    [Parameter] public EventCallback OnCenterView { get; set; }
    [Parameter] public EventCallback OnResetView { get; set; }
}
```

### PanIndicator.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="pan-indicator @(IsVisible ? "visible" : "")">
    <i class="bi bi-arrows-move"></i>
    <span>Panning</span>
</div>

@code {
    [Parameter] public bool IsVisible { get; set; }
}
```

### KeyboardHints.razor

```razor
@namespace VisualEditorOpus.Components.Workflow

<div class="keyboard-hint">
    <span><kbd>Scroll</kbd> to zoom</span>
    <span><kbd>Space</kbd> + drag to pan</span>
    <span><kbd>Ctrl</kbd> + <kbd>0</kbd> reset</span>
    <span><kbd>Ctrl</kbd> + <kbd>1</kbd> fit view</span>
</div>

@code {
}
```

---

## WorkflowCanvas Integration

```razor
@namespace VisualEditorOpus.Components.Workflow
@inject IJSRuntime JS
@implements IAsyncDisposable

<div class="workflow-container" @ref="containerRef"
     @onwheel="HandleWheel"
     @onmousedown="HandleMouseDown"
     @onmousemove="HandleMouseMove"
     @onmouseup="HandleMouseUp"
     @onmouseleave="HandleMouseUp">

    <FitControls
        OnFitView="FitView"
        OnCenterView="CenterView"
        OnResetView="ResetView" />

    <ZoomControls
        Zoom="@Zoom"
        OnZoomChanged="SetZoom"
        OnFitView="FitView"
        OnResetView="ResetView" />

    <PanIndicator IsVisible="@IsPanning" />

    <div class="workflow-canvas"
         style="transform: translate(@(PanX)px, @(PanY)px) scale(@Zoom);">

        <ConnectionLayer
            Connections="@Connections"
            NodePositions="@NodePositions" />

        @foreach (var node in Nodes)
        {
            @switch (node.Type)
            {
                case WorkflowNodeType.Start:
                    <WfNodeStart Node="@node" Position="@node.Position" />
                    break;
                case WorkflowNodeType.End:
                    <WfNodeEnd Node="@node" Position="@node.Position" />
                    break;
                case WorkflowNodeType.Step:
                    <WfNodeStep Node="@node" Position="@node.Position" />
                    break;
                case WorkflowNodeType.Decision:
                    <WfNodeDecision Node="@node" Position="@node.Position" />
                    break;
                case WorkflowNodeType.Action:
                    <WfNodeAction Node="@node" Position="@node.Position" />
                    break;
            }
        }
    </div>

    <WorkflowMinimap
        Nodes="@Nodes"
        ViewportX="@PanX"
        ViewportY="@PanY"
        ViewportW="@ContainerWidth"
        ViewportH="@ContainerHeight"
        Zoom="@Zoom"
        OnNavigate="NavigateTo" />

    <KeyboardHints />
</div>

@code {
    [Parameter] public List<WorkflowNode> Nodes { get; set; } = new();
    [Parameter] public List<WorkflowConnection> Connections { get; set; } = new();

    private ElementReference containerRef;
    private DotNetObjectReference<WorkflowCanvas>? objRef;

    private double Zoom { get; set; } = 1;
    private double PanX { get; set; } = 0;
    private double PanY { get; set; } = 0;
    private bool IsPanning { get; set; } = false;
    private bool IsSpacePressed { get; set; } = false;
    private double StartX { get; set; }
    private double StartY { get; set; }
    private double ContainerWidth { get; set; } = 800;
    private double ContainerHeight { get; set; } = 600;

    private Dictionary<string, NodePosition> NodePositions => Nodes.ToDictionary(
        n => n.Id,
        n => new NodePosition
        {
            X = n.Position.X,
            Y = n.Position.Y,
            Width = 160,
            Height = 80
        }
    );

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            objRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("initWorkflowCanvas", objRef, containerRef);
            await UpdateContainerSize();
        }
    }

    private async Task UpdateContainerSize()
    {
        var rect = await JS.InvokeAsync<BoundingClientRect>("getBoundingClientRect", containerRef);
        ContainerWidth = rect.Width;
        ContainerHeight = rect.Height;
    }

    private void SetZoom(double newZoom)
    {
        Zoom = Math.Clamp(newZoom, 0.25, 2);
    }

    private void HandleWheel(WheelEventArgs e)
    {
        var delta = e.DeltaY > 0 ? -0.1 : 0.1;
        SetZoom(Zoom + delta);
    }

    private void HandleMouseDown(MouseEventArgs e)
    {
        if (e.Button == 1 || IsSpacePressed) // Middle button or space
        {
            IsPanning = true;
            StartX = e.ClientX - PanX;
            StartY = e.ClientY - PanY;
        }
    }

    private void HandleMouseMove(MouseEventArgs e)
    {
        if (IsPanning)
        {
            PanX = e.ClientX - StartX;
            PanY = e.ClientY - StartY;
        }
    }

    private void HandleMouseUp(MouseEventArgs e)
    {
        IsPanning = false;
    }

    [JSInvokable]
    public void HandleKeyDown(string key)
    {
        if (key == " ") IsSpacePressed = true;
    }

    [JSInvokable]
    public void HandleKeyUp(string key)
    {
        if (key == " ") IsSpacePressed = false;
    }

    private async Task FitView()
    {
        if (!Nodes.Any()) return;

        var minX = Nodes.Min(n => n.Position.X);
        var minY = Nodes.Min(n => n.Position.Y);
        var maxX = Nodes.Max(n => n.Position.X) + 160;
        var maxY = Nodes.Max(n => n.Position.Y) + 80;

        var contentWidth = maxX - minX + 100;
        var contentHeight = maxY - minY + 100;

        var scaleX = ContainerWidth / contentWidth;
        var scaleY = ContainerHeight / contentHeight;
        Zoom = Math.Min(Math.Min(scaleX, scaleY), 1);

        PanX = (ContainerWidth - contentWidth * Zoom) / 2 - minX * Zoom + 50;
        PanY = (ContainerHeight - contentHeight * Zoom) / 2 - minY * Zoom + 50;
    }

    private void CenterView()
    {
        PanX = (ContainerWidth - 2000 * Zoom) / 2;
        PanY = (ContainerHeight - 1500 * Zoom) / 2;
    }

    private void ResetView()
    {
        Zoom = 1;
        PanX = 0;
        PanY = 0;
    }

    private void NavigateTo((double X, double Y) position)
    {
        PanX = position.X;
        PanY = position.Y;
    }

    public async ValueTask DisposeAsync()
    {
        if (objRef != null)
        {
            await JS.InvokeVoidAsync("disposeWorkflowCanvas");
            objRef.Dispose();
        }
    }

    private record BoundingClientRect(double Left, double Top, double Width, double Height);
}
```

---

## CSS Styles

```css
/* ===== WORKFLOW CONTAINER ===== */
.workflow-container {
    position: relative;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-lg);
    height: 100%;
    overflow: hidden;
}

.workflow-canvas {
    width: 2000px;
    height: 1500px;
    position: relative;
    background-image:
        radial-gradient(circle, var(--border-color) 1px, transparent 1px);
    background-size: 20px 20px;
    transform-origin: 0 0;
    transition: transform 0.1s ease-out;
}

/* ===== MINIMAP ===== */
.minimap {
    position: absolute;
    bottom: 16px;
    right: 16px;
    width: 200px;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-lg);
    overflow: hidden;
    z-index: 100;
}

.minimap.collapsed {
    height: auto;
}

.minimap-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 6px 10px;
    background: var(--bg-tertiary);
    border-bottom: 1px solid var(--border-color);
    font-size: 11px;
    font-weight: 600;
    color: var(--text-secondary);
}

.minimap-toggle {
    padding: 2px 4px;
    background: transparent;
    border: none;
    cursor: pointer;
    color: var(--text-muted);
    font-size: 12px;
}

.minimap-toggle:hover {
    color: var(--text-primary);
}

.minimap-content {
    position: relative;
    height: 150px;
    background: var(--bg-secondary);
}

.minimap-node {
    position: absolute;
    border-radius: 2px;
}

.minimap-node.start { background: var(--success); }
.minimap-node.step { background: var(--primary); }
.minimap-node.decision { background: var(--warning); }
.minimap-node.action { background: #8B5CF6; }
.minimap-node.end { background: #6B7280; }

.minimap-viewport {
    position: absolute;
    border: 2px solid var(--primary);
    background: rgba(99, 102, 241, 0.1);
    border-radius: 2px;
    cursor: move;
}

.minimap-viewport:hover {
    background: rgba(99, 102, 241, 0.2);
}

/* ===== ZOOM CONTROLS ===== */
.zoom-controls {
    position: absolute;
    bottom: 16px;
    left: 16px;
    display: flex;
    flex-direction: column;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-lg);
    overflow: hidden;
    z-index: 100;
}

.zoom-btn {
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--bg-primary);
    border: none;
    cursor: pointer;
    color: var(--text-secondary);
    font-size: 16px;
    transition: all 0.15s;
}

.zoom-btn:hover:not(:disabled) {
    background: var(--bg-tertiary);
    color: var(--primary);
}

.zoom-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.zoom-level {
    padding: 8px;
    text-align: center;
    font-size: 11px;
    font-weight: 600;
    color: var(--text-secondary);
    background: var(--bg-tertiary);
    border-top: 1px solid var(--border-color);
    border-bottom: 1px solid var(--border-color);
}

.zoom-divider {
    height: 1px;
    background: var(--border-color);
}

/* ===== FIT CONTROLS ===== */
.fit-controls {
    position: absolute;
    top: 16px;
    right: 16px;
    display: flex;
    gap: 8px;
    z-index: 100;
}

.fit-btn {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 8px 12px;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-md);
    cursor: pointer;
    font-size: 12px;
    font-weight: 500;
    color: var(--text-secondary);
    transition: all 0.15s;
}

.fit-btn:hover {
    background: var(--bg-tertiary);
    border-color: var(--primary);
    color: var(--primary);
}

/* ===== PAN INDICATOR ===== */
.pan-indicator {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    padding: 12px 16px;
    background: rgba(0, 0, 0, 0.7);
    border-radius: var(--radius-md);
    color: white;
    font-size: 12px;
    opacity: 0;
    transition: opacity 0.2s;
    pointer-events: none;
    z-index: 200;
}

.pan-indicator.visible {
    opacity: 1;
}

.pan-indicator i {
    font-size: 24px;
}

/* ===== KEYBOARD HINTS ===== */
.keyboard-hint {
    position: absolute;
    bottom: 16px;
    left: 50%;
    transform: translateX(-50%);
    display: flex;
    gap: 16px;
    padding: 8px 16px;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-md);
    font-size: 11px;
    color: var(--text-muted);
    z-index: 100;
}

.keyboard-hint kbd {
    padding: 2px 6px;
    background: var(--bg-tertiary);
    border: 1px solid var(--border-color);
    border-radius: 4px;
    font-family: monospace;
    font-size: 10px;
}
```

---

## JavaScript Interop

```javascript
// wwwroot/js/workflow-canvas.js

let dotNetRef = null;
let containerElement = null;

window.initWorkflowCanvas = (objRef, element) => {
    dotNetRef = objRef;
    containerElement = element;

    document.addEventListener('keydown', handleKeyDown);
    document.addEventListener('keyup', handleKeyUp);
};

window.disposeWorkflowCanvas = () => {
    document.removeEventListener('keydown', handleKeyDown);
    document.removeEventListener('keyup', handleKeyUp);
    dotNetRef = null;
    containerElement = null;
};

function handleKeyDown(e) {
    if (dotNetRef) {
        dotNetRef.invokeMethodAsync('HandleKeyDown', e.key);
    }
}

function handleKeyUp(e) {
    if (dotNetRef) {
        dotNetRef.invokeMethodAsync('HandleKeyUp', e.key);
    }
}

window.getBoundingClientRect = (element) => {
    const rect = element.getBoundingClientRect();
    return {
        left: rect.left,
        top: rect.top,
        width: rect.width,
        height: rect.height
    };
};
```

---

## Testing Checklist

- [ ] Minimap shows all nodes
- [ ] Minimap viewport updates on pan
- [ ] Clicking minimap navigates canvas
- [ ] Dragging viewport pans canvas
- [ ] Minimap collapses/expands
- [ ] Zoom in button works
- [ ] Zoom out button works
- [ ] Zoom level displays correctly
- [ ] Fit view calculates correctly
- [ ] Center view works
- [ ] Reset view works
- [ ] Mouse wheel zooms
- [ ] Space + drag pans
- [ ] Middle mouse button pans
- [ ] Pan indicator shows during pan
- [ ] Keyboard hints display
- [ ] Dark mode styling correct

---

## Notes

- Consider adding zoom to cursor position
- Consider adding minimap connection lines
- Consider zoom presets (25%, 50%, 75%, 100%, 150%, 200%)
- Consider touch gestures for mobile
- Minimap scale should adjust to canvas content bounds

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the Minimap and Controls components for my Blazor workflow designer.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Workflow/

## Components to Create:

### 1. WorkflowMinimap.razor
Minimap component with:
- Bird's-eye view of all nodes
- Color-coded node representations by type
- Draggable viewport indicator
- Click to navigate functionality
- Collapsible header

### 2. ZoomControls.razor
Zoom control panel with:
- Zoom in/out buttons
- Current zoom level display
- Fit to view button
- Reset view button

### 3. FitControls.razor
Navigation controls with:
- Fit View button
- Center button
- Reset button

### 4. PanIndicator.razor
Visual indicator showing when canvas is being panned

### 5. KeyboardHints.razor
Keyboard shortcut hints display

### Features:
- Mouse wheel zoom
- Space + drag to pan
- Middle mouse button pan
- Ctrl+0 reset view
- Ctrl+1 fit view
- Minimap viewport updates on pan/zoom

Please implement complete, production-ready code with CSS.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `MinimapControls-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing minimap node display
- Viewport indicator dragging and navigation
- Minimap click to navigate functionality
- Zoom in/out button functionality
- Mouse wheel zoom testing
- Space + drag pan testing
- Middle mouse button pan testing
- Fit view calculation testing
- Reset view functionality

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- JavaScript interop file (workflow-canvas.js) registration
- CSS file imports
- Integration with WorkflowCanvas parent component
- Keyboard event handlers setup
- BoundingClientRect JS interop function
- Touch gesture support (optional)

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Minimap shows all nodes
- [ ] Minimap viewport updates on pan
- [ ] Clicking minimap navigates canvas
- [ ] Dragging viewport pans canvas
- [ ] Minimap collapses/expands
- [ ] Zoom in button works
- [ ] Zoom out button works
- [ ] Zoom level displays correctly
- [ ] Fit view calculates correctly
- [ ] Center view works
- [ ] Reset view works
- [ ] Mouse wheel zooms
- [ ] Space + drag pans
- [ ] Middle mouse button pans
- [ ] Pan indicator shows during pan
- [ ] Keyboard hints display
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END
