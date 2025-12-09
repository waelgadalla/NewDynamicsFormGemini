# ConfirmDeleteModal Component - Implementation Plan

> **Component**: `ConfirmDeleteModal.razor`
> **Location**: `Src/VisualEditorOpus/Components/Shared/ConfirmDeleteModal.razor`
> **Priority**: Critical (Used throughout the application)
> **Estimated Effort**: 1-1.5 hours
> **Depends On**: ModalBase.razor

---

## Overview

A simple, reusable confirmation dialog for delete operations. Provides clear messaging, danger styling, and optional warning information about cascading deletes.

---

## Features

| Feature | Description |
|---------|-------------|
| Clear Messaging | Title, message, and item name clearly displayed |
| Danger Styling | Red/danger color scheme signals destructive action |
| Warning Section | Optional warning for cascading deletes |
| Children List | Shows items that will be deleted along with parent |
| Loading State | Shows spinner while delete operation is in progress |
| Keyboard Support | ESC to cancel, Enter to confirm |

---

## Component API

### Parameters

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

[Parameter] public string Title { get; set; } = "Delete Item";
[Parameter] public string Message { get; set; } = "Are you sure you want to delete this item? This action cannot be undone.";
[Parameter] public string ItemName { get; set; } = "";

[Parameter] public string? WarningMessage { get; set; }
[Parameter] public IEnumerable<DeleteChildItem>? ChildItems { get; set; }

[Parameter] public string ConfirmButtonText { get; set; } = "Delete";
[Parameter] public string CancelButtonText { get; set; } = "Cancel";

[Parameter] public bool IsDeleting { get; set; } = false;
[Parameter] public EventCallback OnConfirm { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

### Supporting Types

```csharp
public record DeleteChildItem(string Name, string Icon = "bi-file");
```

---

## Visual Structure

```
┌─────────────────────────────────────────┐
│ ⚠ Delete Field                      [×] │  <- Header
├─────────────────────────────────────────┤
│                                         │
│            [🗑️ Icon]                    │  <- Danger icon in circle
│                                         │
│          Are you sure?                  │  <- Title
│                                         │
│   You are about to delete this field.   │  <- Message
│   This action cannot be undone.         │
│                                         │
│   ┌─────────────────────────────────┐   │
│   │ firstName                       │   │  <- Item name (monospace)
│   └─────────────────────────────────┘   │
│                                         │
│   ┌─────────────────────────────────┐   │  <- Warning (optional)
│   │ ⚠ This field has 3 children... │   │
│   └─────────────────────────────────┘   │
│                                         │
│   ┌─────────────────────────────────┐   │  <- Children list (optional)
│   │ Fields to be deleted (3):       │   │
│   │ ─ firstName                     │   │
│   │ ─ lastName                      │   │
│   │ ─ email                         │   │
│   └─────────────────────────────────┘   │
│                                         │
├─────────────────────────────────────────┤
│ [Esc] to cancel    [Cancel] [🗑Delete]  │  <- Footer
└─────────────────────────────────────────┘
```

---

## Usage Examples

### Simple Delete

```razor
<ConfirmDeleteModal @bind-IsOpen="showDeleteModal"
                    Title="Delete Field"
                    Message="You are about to delete this field. This action cannot be undone."
                    ItemName="@selectedField.Id"
                    OnConfirm="HandleDelete"
                    OnCancel="() => showDeleteModal = false" />
```

### Delete with Children

```razor
<ConfirmDeleteModal @bind-IsOpen="showDeleteModal"
                    Title="Delete Section"
                    Message="You are about to delete this section and all its contents."
                    ItemName="@selectedSection.Id"
                    WarningMessage="This section contains 5 child fields that will also be deleted."
                    ChildItems="@childFields"
                    OnConfirm="HandleDeleteWithChildren"
                    IsDeleting="@isDeleting" />

@code {
    private IEnumerable<DeleteChildItem> childFields =>
        selectedSection.Children.Select(c => new DeleteChildItem(c.Id, GetFieldIcon(c.FieldType)));
}
```

### Delete Module

```razor
<ConfirmDeleteModal @bind-IsOpen="showDeleteModule"
                    Title="Delete Module"
                    Message="You are about to delete this entire module including all fields and rules."
                    ItemName="@module.TitleEn"
                    WarningMessage="@($"This module contains {module.Fields.Length} fields that will be permanently deleted.")"
                    OnConfirm="HandleDeleteModule" />
```

---

## Implementation Details

### 1. Extends ModalBase
This component should use ModalBase internally, passing appropriate parameters.

### 2. Icon Styling
The trash icon should be displayed in a circular background with danger-light color.

### 3. Loading State
When `IsDeleting` is true:
- Show spinner on confirm button
- Disable both buttons
- Prevent backdrop click close

### 4. Keyboard Handling
- ESC: Close modal (unless deleting)
- Enter: Trigger confirm (unless deleting)

---

## File Structure

```
Components/Shared/
├── ConfirmDeleteModal.razor
├── ConfirmDeleteModal.razor.cs (optional, can be inline)
└── DeleteChildItem.cs (in Models folder)
```

---

## CSS Classes

Uses existing classes from `app.css`:
- `.modal-backdrop`, `.modal`, `.modal-header`, `.modal-body`, `.modal-footer`
- `.btn`, `.btn-danger`, `.btn-ghost`

New scoped styles needed:
```css
.delete-icon { /* Circular danger icon container */ }
.delete-title { /* "Are you sure?" text */ }
.delete-message { /* Explanation text */ }
.delete-item-name { /* Monospace item name box */ }
.delete-warning { /* Warning box with icon */ }
.children-list { /* List of child items */ }
```

---

## Testing Checklist

- [ ] Modal opens and closes correctly
- [ ] ESC key closes modal
- [ ] Cancel button closes modal
- [ ] Confirm button triggers OnConfirm
- [ ] Loading state shows spinner
- [ ] Loading state disables buttons
- [ ] Warning message displays when provided
- [ ] Children list displays when provided
- [ ] Item name displays correctly
- [ ] Dark mode styling works
- [ ] Focus is on Cancel button by default (safer default)

---

## Claude Implementation Prompt

Copy and paste the following prompt to Claude to implement this component:

---

### PROMPT START

```
I need you to implement the ConfirmDeleteModal component for my Blazor application. This is a reusable confirmation dialog for delete operations.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Shared/
- Depends on: ModalBase.razor (already implemented)
- Styling: Uses CSS variables from variables.css

## Files to Create

### 1. Models/DeleteChildItem.cs
```csharp
namespace VisualEditorOpus.Models;

public record DeleteChildItem(string Name, string Icon = "bi-file");
```

### 2. ConfirmDeleteModal.razor
Create a component that:
- Uses ModalBase internally with Size="ModalSize.Small"
- Displays a circular danger icon at the top
- Shows "Are you sure?" title
- Shows configurable message
- Shows item name in a monospace box
- Optionally shows warning message with warning icon
- Optionally shows list of child items to be deleted
- Has Cancel and Delete buttons in footer

### 3. ConfirmDeleteModal.razor.css (scoped styles)
Add styles for:
- .delete-icon (64px circular bg with danger-light, centered icon)
- .delete-title (18px bold centered)
- .delete-message (text-secondary, centered)
- .delete-item-name (monospace, bg-tertiary, padded)
- .delete-warning (warning background, flex with icon)
- .children-list (bg-tertiary list container)
- .children-item (individual item row)

## Parameters Required

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public string Title { get; set; } = "Delete Item";
[Parameter] public string Message { get; set; } = "Are you sure you want to delete this item? This action cannot be undone.";
[Parameter] public string ItemName { get; set; } = "";
[Parameter] public string? WarningMessage { get; set; }
[Parameter] public IEnumerable<DeleteChildItem>? ChildItems { get; set; }
[Parameter] public string ConfirmButtonText { get; set; } = "Delete";
[Parameter] public string CancelButtonText { get; set; } = "Cancel";
[Parameter] public bool IsDeleting { get; set; } = false;
[Parameter] public EventCallback OnConfirm { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

## Razor Structure

```razor
<ModalBase @bind-IsOpen="IsOpen"
           Title="@Title"
           Icon="bi-trash"
           Size="ModalSize.Small"
           CloseOnBackdropClick="@(!IsDeleting)"
           CloseOnEscape="@(!IsDeleting)"
           OnClose="HandleCancel">

    <div class="delete-content">
        <!-- Circular icon -->
        <div class="delete-icon">
            <i class="bi bi-trash"></i>
        </div>

        <!-- Title -->
        <div class="delete-title">Are you sure?</div>

        <!-- Message -->
        <div class="delete-message">@Message</div>

        <!-- Item name -->
        @if (!string.IsNullOrEmpty(ItemName))
        {
            <div class="delete-item-name">@ItemName</div>
        }

        <!-- Warning (conditional) -->
        @if (!string.IsNullOrEmpty(WarningMessage))
        {
            <div class="delete-warning">
                <i class="bi bi-exclamation-triangle-fill"></i>
                <span>@WarningMessage</span>
            </div>
        }

        <!-- Children list (conditional) -->
        @if (ChildItems?.Any() == true)
        {
            <div class="children-list">
                <div class="children-list-title">Items to be deleted (@ChildItems.Count())</div>
                @foreach (var child in ChildItems)
                {
                    <div class="children-item">
                        <i class="bi @child.Icon"></i>
                        <span>@child.Name</span>
                    </div>
                }
            </div>
        }
    </div>

    <FooterContent>
        <span class="keyboard-hint"><kbd>Esc</kbd> to cancel</span>
        <button class="btn btn-ghost" @onclick="HandleCancel" disabled="@IsDeleting">
            @CancelButtonText
        </button>
        <button class="btn btn-danger @(IsDeleting ? "btn-loading" : "")"
                @onclick="HandleConfirm"
                disabled="@IsDeleting">
            <i class="bi bi-trash"></i> @ConfirmButtonText
        </button>
    </FooterContent>

</ModalBase>
```

## Code-Behind Methods

```csharp
private async Task HandleConfirm()
{
    await OnConfirm.InvokeAsync();
}

private async Task HandleCancel()
{
    await OnCancel.InvokeAsync();
    await IsOpenChanged.InvokeAsync(false);
}
```

## CSS Styles (scoped)

```css
.delete-content {
    text-align: center;
    padding: 8px 0;
}

.delete-icon {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background: var(--danger-light);
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 16px;
}

.delete-icon i {
    font-size: 28px;
    color: var(--danger);
}

.delete-title {
    font-size: 18px;
    font-weight: 700;
    margin-bottom: 8px;
}

.delete-message {
    color: var(--text-secondary);
    line-height: 1.5;
    margin-bottom: 16px;
}

.delete-item-name {
    background: var(--bg-tertiary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    padding: 10px 16px;
    font-family: var(--font-mono);
    font-size: 13px;
    word-break: break-all;
}

.delete-warning {
    display: flex;
    align-items: flex-start;
    gap: 10px;
    background: var(--warning-light);
    border: 1px solid var(--warning);
    border-radius: var(--radius-md);
    padding: 12px;
    margin-top: 16px;
    text-align: left;
}

.delete-warning i {
    color: var(--warning);
    font-size: 18px;
    flex-shrink: 0;
}

.delete-warning span {
    font-size: 13px;
}

.children-list {
    background: var(--bg-tertiary);
    border-radius: var(--radius-md);
    padding: 12px;
    margin-top: 12px;
    text-align: left;
}

.children-list-title {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-secondary);
    text-transform: uppercase;
    margin-bottom: 8px;
}

.children-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 0;
    font-size: 13px;
    border-bottom: 1px solid var(--border-color);
}

.children-item:last-child {
    border-bottom: none;
}

.children-item i {
    color: var(--text-muted);
    font-size: 14px;
}

.keyboard-hint {
    font-size: 11px;
    color: var(--text-muted);
    margin-right: auto;
}

kbd {
    background: var(--bg-tertiary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-sm);
    padding: 2px 6px;
    font-size: 10px;
}

.btn-loading {
    position: relative;
    color: transparent !important;
}

.btn-loading::after {
    content: "";
    position: absolute;
    width: 16px;
    height: 16px;
    border: 2px solid rgba(255,255,255,0.3);
    border-top-color: white;
    border-radius: 50%;
    animation: spin 0.6s linear infinite;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}
```

## Important Notes

1. The modal should use ModalBase - do not recreate modal infrastructure
2. Focus should be on Cancel button when opened (safer default)
3. When IsDeleting is true, disable all interactions
4. The .btn-loading class shows a spinner and hides text
5. Use the existing CSS variables for colors
6. The keyboard hint in footer should use margin-right: auto to push buttons right

Please implement all files with complete, production-ready code.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `ConfirmDeleteModal-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing simple delete confirmation
- Delete with warning message testing
- Delete with child items list testing
- Loading state (IsDeleting) behavior testing
- Keyboard interaction testing (ESC to cancel, Enter to confirm)
- Button state testing (disabled during delete)
- Visual styling verification (danger icon, warning box)
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Model file location setup (DeleteChildItem.cs)
- Integration with EditorStateService for delete operations
- Integration with ToastService for success/error messages
- Wiring up delete prompts in parent components
- Event handler implementation patterns
- CSS imports if separate file needed

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens and closes correctly
- [ ] ESC key closes modal (when not deleting)
- [ ] Cancel button closes modal
- [ ] Confirm button triggers OnConfirm callback
- [ ] Loading state shows spinner on button
- [ ] Loading state disables both buttons
- [ ] Loading state prevents backdrop click close
- [ ] Danger icon displays in circular background
- [ ] Warning message displays when provided
- [ ] Children list displays when provided
- [ ] Item name displays in monospace box
- [ ] Keyboard hint shows in footer
- [ ] Focus on Cancel button by default
- [ ] Dark mode styling correct
- [ ] Integration with ModalBase works

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Integration Points

This modal will be used by:
- **EditorStateService**: When deleting fields
- **ModuleEditor**: When deleting the current module
- **WorkflowEditor**: When deleting modules from workflow
- **CodeSetManager**: When deleting CodeSets
- **RightSidebar**: When deleting from property panel

---

## Common Integration Pattern

```csharp
// In parent component
private bool showDeleteModal = false;
private FormFieldSchema? fieldToDelete;
private bool isDeleting = false;

private void PromptDelete(FormFieldSchema field)
{
    fieldToDelete = field;
    showDeleteModal = true;
}

private async Task HandleDelete()
{
    if (fieldToDelete == null) return;

    isDeleting = true;
    StateHasChanged();

    try
    {
        await EditorStateService.DeleteFieldAsync(fieldToDelete.Id);
        showDeleteModal = false;
        ToastService.ShowSuccess("Field deleted");
    }
    catch (Exception ex)
    {
        ToastService.ShowError($"Delete failed: {ex.Message}");
    }
    finally
    {
        isDeleting = false;
        fieldToDelete = null;
    }
}
```
