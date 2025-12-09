# D.1 View Switcher - Implementation Plan

> **Task**: View Switcher Component
> **Location**: `Src/VisualEditorOpus/Components/Editor/`
> **Priority**: High
> **Estimated Effort**: 1-2 hours
> **Delegation**: 90% AI

---

## Overview

The View Switcher provides navigation between three editor views: Design (form builder), Preview (rendered form), and JSON (schema view). It supports keyboard shortcuts and preserves state when switching views.

---

## Component to Create

### ViewSwitcher.razor

```razor
@namespace VisualEditorOpus.Components.Editor

<div class="view-switcher">
    <button class="view-tab @(CurrentView == EditorView.Design ? "active" : "")"
            @onclick="() => SetView(EditorView.Design)"
            title="Design View (Ctrl+1)">
        <i class="bi bi-pencil-square"></i>
        Design
    </button>
    <button class="view-tab @(CurrentView == EditorView.Preview ? "active" : "")"
            @onclick="() => SetView(EditorView.Preview)"
            title="Preview (Ctrl+2)">
        <i class="bi bi-eye"></i>
        Preview
    </button>
    <button class="view-tab @(CurrentView == EditorView.Json ? "active" : "")"
            @onclick="() => SetView(EditorView.Json)"
            title="JSON View (Ctrl+3)">
        <i class="bi bi-code-slash"></i>
        JSON
    </button>
</div>

@code {
    [Parameter] public EditorView CurrentView { get; set; } = EditorView.Design;
    [Parameter] public EventCallback<EditorView> OnViewChanged { get; set; }

    private async Task SetView(EditorView view)
    {
        if (view != CurrentView)
        {
            await OnViewChanged.InvokeAsync(view);
        }
    }
}
```

### EditorView Enum

```csharp
public enum EditorView
{
    Design,
    Preview,
    Json
}
```

### EditorToolbar.razor (Container)

```razor
@namespace VisualEditorOpus.Components.Editor

<div class="editor-toolbar">
    <div class="toolbar-left">
        <span class="toolbar-title">@Module?.Name ?? "Untitled Form"</span>
        @if (FieldCount > 0)
        {
            <span class="toolbar-badge">@FieldCount Fields</span>
        }
    </div>

    <ViewSwitcher CurrentView="@CurrentView" OnViewChanged="OnViewChanged" />

    <div class="toolbar-actions">
        <button class="toolbar-btn" @onclick="Undo" disabled="@(!CanUndo)">
            <i class="bi bi-arrow-counterclockwise"></i>
            Undo
        </button>
        <button class="toolbar-btn" @onclick="Redo" disabled="@(!CanRedo)">
            <i class="bi bi-arrow-clockwise"></i>
            Redo
        </button>
        <button class="toolbar-btn primary" @onclick="Save">
            <i class="bi bi-save"></i>
            Save
        </button>
    </div>
</div>

@code {
    [Parameter] public FormModuleSchema? Module { get; set; }
    [Parameter] public EditorView CurrentView { get; set; }
    [Parameter] public EventCallback<EditorView> OnViewChanged { get; set; }
    [Parameter] public bool CanUndo { get; set; }
    [Parameter] public bool CanRedo { get; set; }
    [Parameter] public EventCallback Undo { get; set; }
    [Parameter] public EventCallback Redo { get; set; }
    [Parameter] public EventCallback Save { get; set; }

    private int FieldCount => Module?.Fields?.Count ?? 0;
}
```

### EditorContainer.razor (Main Layout)

```razor
@namespace VisualEditorOpus.Components.Editor
@inject IJSRuntime JS
@implements IAsyncDisposable

<div class="editor-container" @onkeydown="HandleKeyDown" tabindex="0">
    <EditorToolbar
        Module="@Module"
        CurrentView="@CurrentView"
        OnViewChanged="SetView"
        CanUndo="@CanUndo"
        CanRedo="@CanRedo"
        Undo="@Undo"
        Redo="@Redo"
        Save="@Save" />

    <div class="editor-content">
        @switch (CurrentView)
        {
            case EditorView.Design:
                <DesignView Module="@Module" OnModuleChanged="OnModuleChanged" />
                break;
            case EditorView.Preview:
                <FormPreview Module="@Module" />
                break;
            case EditorView.Json:
                <JsonPreview Module="@Module" OnModuleChanged="OnModuleChanged" />
                break;
        }
    </div>
</div>

@code {
    [Parameter] public FormModuleSchema Module { get; set; } = default!;
    [Parameter] public EventCallback<FormModuleSchema> OnModuleChanged { get; set; }

    private EditorView CurrentView { get; set; } = EditorView.Design;
    private bool CanUndo { get; set; }
    private bool CanRedo { get; set; }

    private DotNetObjectReference<EditorContainer>? objRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            objRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("registerKeyboardShortcuts", objRef);
        }
    }

    private void SetView(EditorView view)
    {
        CurrentView = view;
        StateHasChanged();
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.CtrlKey)
        {
            switch (e.Key)
            {
                case "1":
                    SetView(EditorView.Design);
                    break;
                case "2":
                    SetView(EditorView.Preview);
                    break;
                case "3":
                    SetView(EditorView.Json);
                    break;
            }
        }
    }

    [JSInvokable]
    public void SwitchToView(int viewIndex)
    {
        SetView((EditorView)viewIndex);
    }

    private Task Undo() => Task.CompletedTask; // TODO: Implement
    private Task Redo() => Task.CompletedTask; // TODO: Implement
    private Task Save() => Task.CompletedTask; // TODO: Implement

    public async ValueTask DisposeAsync()
    {
        if (objRef != null)
        {
            await JS.InvokeVoidAsync("unregisterKeyboardShortcuts");
            objRef.Dispose();
        }
    }
}
```

---

## CSS Styles

```css
/* ===== VIEW SWITCHER ===== */

.view-switcher {
    display: flex;
    gap: 4px;
    padding: 4px;
    background: var(--bg-tertiary);
    border-radius: var(--radius-md);
}

.view-tab {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 8px 16px;
    border: none;
    background: transparent;
    border-radius: var(--radius-sm);
    font-size: 13px;
    font-weight: 500;
    color: var(--text-secondary);
    cursor: pointer;
    transition: all 0.15s;
}

.view-tab:hover {
    color: var(--text-primary);
    background: var(--bg-secondary);
}

.view-tab.active {
    background: var(--bg-primary);
    color: var(--primary);
    box-shadow: var(--shadow-sm);
}

.view-tab i {
    font-size: 14px;
}

/* Editor Toolbar */
.editor-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 16px;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-lg);
    margin-bottom: 16px;
}

.toolbar-left {
    display: flex;
    align-items: center;
    gap: 12px;
}

.toolbar-title {
    font-size: 16px;
    font-weight: 600;
}

.toolbar-badge {
    font-size: 11px;
    padding: 3px 8px;
    border-radius: 10px;
    background: var(--primary-light);
    color: var(--primary);
    font-weight: 600;
}

.toolbar-actions {
    display: flex;
    align-items: center;
    gap: 8px;
}

.toolbar-btn {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 8px 14px;
    border: 1px solid var(--border-color);
    background: var(--bg-primary);
    border-radius: var(--radius-md);
    font-size: 13px;
    font-weight: 500;
    color: var(--text-secondary);
    cursor: pointer;
    transition: all 0.15s;
}

.toolbar-btn:hover:not(:disabled) {
    border-color: var(--primary);
    color: var(--primary);
    background: var(--primary-light);
}

.toolbar-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.toolbar-btn.primary {
    background: var(--primary);
    border-color: var(--primary);
    color: white;
}

.toolbar-btn.primary:hover {
    background: var(--primary-hover);
}

/* Editor Container */
.editor-container {
    display: flex;
    flex-direction: column;
    height: 100%;
    outline: none;
}

.editor-content {
    flex: 1;
    overflow: hidden;
}
```

---

## JavaScript for Keyboard Shortcuts

```javascript
// wwwroot/js/editor.js

let dotNetRef = null;

window.registerKeyboardShortcuts = (objRef) => {
    dotNetRef = objRef;
    document.addEventListener('keydown', handleGlobalKeyDown);
};

window.unregisterKeyboardShortcuts = () => {
    document.removeEventListener('keydown', handleGlobalKeyDown);
    dotNetRef = null;
};

function handleGlobalKeyDown(e) {
    if (e.ctrlKey && !e.shiftKey && !e.altKey) {
        switch (e.key) {
            case '1':
                e.preventDefault();
                dotNetRef?.invokeMethodAsync('SwitchToView', 0); // Design
                break;
            case '2':
                e.preventDefault();
                dotNetRef?.invokeMethodAsync('SwitchToView', 1); // Preview
                break;
            case '3':
                e.preventDefault();
                dotNetRef?.invokeMethodAsync('SwitchToView', 2); // JSON
                break;
        }
    }
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the View Switcher component for my Blazor form editor.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Editor/

## Components to Create:

### 1. ViewSwitcher.razor
Simple tab component with three views:
- Design (pencil-square icon)
- Preview (eye icon)
- JSON (code-slash icon)

Parameters:
```csharp
[Parameter] public EditorView CurrentView { get; set; }
[Parameter] public EventCallback<EditorView> OnViewChanged { get; set; }
```

### 2. EditorView.cs
```csharp
public enum EditorView { Design, Preview, Json }
```

### 3. EditorToolbar.razor
Contains:
- Left: Module name + field count badge
- Center: ViewSwitcher
- Right: Undo, Redo, Save buttons

### 4. EditorContainer.razor
Main layout that:
- Shows toolbar at top
- Switches between Design/Preview/JSON views
- Handles keyboard shortcuts (Ctrl+1/2/3)

### Keyboard Shortcuts:
- Ctrl+1: Design View
- Ctrl+2: Preview View
- Ctrl+3: JSON View

Please implement complete, production-ready code with CSS.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `ViewSwitcher-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing Design tab activation
- Preview tab activation testing
- JSON tab activation testing
- Keyboard shortcut testing (Ctrl+1, Ctrl+2, Ctrl+3)
- Module name display testing
- Field count badge display testing
- Undo/Redo button state testing
- Save button functionality testing
- View content switching verification
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- EditorView.cs enum file creation location
- JavaScript file import (editor.js)
- DesignView, FormPreview, JsonPreview component integration
- Undo/Redo logic implementation
- Save functionality implementation
- CSS file imports

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] ViewSwitcher displays three tabs (Design, Preview, JSON)
- [ ] Design tab shows pencil-square icon
- [ ] Preview tab shows eye icon
- [ ] JSON tab shows code-slash icon
- [ ] Clicking Design tab activates it
- [ ] Clicking Preview tab activates it
- [ ] Clicking JSON tab activates it
- [ ] Active tab shows correct styling (primary color)
- [ ] Ctrl+1 switches to Design
- [ ] Ctrl+2 switches to Preview
- [ ] Ctrl+3 switches to JSON
- [ ] Module name displays in toolbar
- [ ] Field count badge displays correctly
- [ ] Undo button disabled when cannot undo
- [ ] Redo button disabled when cannot redo
- [ ] Save button works
- [ ] EditorContainer switches content based on view
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Design tab activates correctly
- [ ] Preview tab activates correctly
- [ ] JSON tab activates correctly
- [ ] Active state shows correct styling
- [ ] Hover state works
- [ ] Ctrl+1 switches to Design
- [ ] Ctrl+2 switches to Preview
- [ ] Ctrl+3 switches to JSON
- [ ] Module name displays
- [ ] Field count badge displays
- [ ] Undo/Redo buttons work
- [ ] Save button works
- [ ] Dark mode styling correct

---

## Notes

- View state should be preserved when switching
- Consider adding view transition animations
- Preview should use read-only mode
- JSON view should highlight syntax
- Consider adding split view in future (Design + Preview)
