# C.5 Enhanced Options Section - Implementation Plan

> **Task**: Enhanced Options Section with Drag-to-Reorder
> **Location**: `Src/VisualEditorOpus/Components/PropertyPanel/Sections/`
> **Priority**: High
> **Estimated Effort**: 3-4 hours
> **Delegation**: 85% AI

---

## Overview

The Enhanced Options Section manages dropdown/radio/checkbox options with two modes: Inline (custom options) and CodeSet (reference to shared option sets). It supports drag-to-reorder, bilingual labels, quick add, and bulk actions.

---

## Schema Reference

From `DynamicForms.Core.V4`:

```csharp
// FormFieldSchema.cs
public ImmutableList<FieldOption>? Options { get; init; }
public string? CodeSetId { get; init; }

// FieldOption.cs
public record FieldOption
{
    public required string Value { get; init; }
    public required string LabelEn { get; init; }
    public string? LabelFr { get; init; }
    public bool IsDefault { get; init; }
    public int SortOrder { get; init; }
}
```

---

## Component to Create

### EnhancedOptionsSection.razor

```razor
@namespace VisualEditorOpus.Components.PropertyPanel.Sections
@inject IJSRuntime JS

<div class="property-section">
    <div class="section-header @(IsExpanded ? "expanded" : "")" @onclick="ToggleExpanded">
        <div class="section-header-left">
            <i class="bi bi-list-check"></i>
            <span>Options</span>
        </div>
        <i class="bi bi-chevron-down section-chevron"></i>
    </div>

    @if (IsExpanded)
    {
        <div class="section-content">
            @* Mode Switcher *@
            <div class="mode-switcher">
                <button class="mode-btn @(Mode == OptionsMode.Inline ? "active" : "")"
                        @onclick="() => SetMode(OptionsMode.Inline)">
                    <i class="bi bi-list-ul"></i>
                    Inline
                </button>
                <button class="mode-btn @(Mode == OptionsMode.CodeSet ? "active" : "")"
                        @onclick="() => SetMode(OptionsMode.CodeSet)">
                    <i class="bi bi-database"></i>
                    CodeSet
                </button>
            </div>

            @if (Mode == OptionsMode.Inline)
            {
                @* Inline Options List *@
                <div class="options-list">
                    <div class="options-header">
                        <div class="options-header-left">
                            <span class="options-count">@Options.Count Options</span>
                            <span class="options-badge">Inline</span>
                        </div>
                        <button class="add-option-btn" @onclick="AddOption">
                            <i class="bi bi-plus"></i>
                            Add
                        </button>
                    </div>

                    @foreach (var option in Options.OrderBy(o => o.SortOrder))
                    {
                        <div class="option-item @(DraggedOption?.Value == option.Value ? "dragging" : "")"
                             draggable="true"
                             @ondragstart="() => StartDrag(option)"
                             @ondragover:preventDefault
                             @ondrop="() => DropOn(option)">
                            <i class="bi bi-grip-vertical option-drag-handle"></i>
                            <span class="option-value-badge">@option.Value</span>
                            <div class="option-labels">
                                <div class="option-label-en">@option.LabelEn</div>
                                @if (!string.IsNullOrEmpty(option.LabelFr))
                                {
                                    <div class="option-label-fr">@option.LabelFr</div>
                                }
                            </div>
                            @if (option.IsDefault)
                            {
                                <span class="option-default">Default</span>
                            }
                            <div class="option-actions">
                                <button class="option-action-btn" @onclick="() => EditOption(option)">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="option-action-btn danger" @onclick="() => DeleteOption(option)">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        </div>
                    }

                    @* Quick Add *@
                    <div class="quick-add-row">
                        <input type="text"
                               class="quick-add-input"
                               placeholder="Add new option..."
                               @bind="QuickAddValue"
                               @bind:event="oninput"
                               @onkeypress="HandleQuickAddKeyPress" />
                        <button class="quick-add-btn" @onclick="QuickAdd">Add</button>
                    </div>
                </div>

                @* Bulk Actions *@
                <div class="bulk-actions">
                    <button class="bulk-btn" @onclick="SortAlphabetically">
                        <i class="bi bi-sort-alpha-down"></i>
                        Sort A-Z
                    </button>
                    <button class="bulk-btn" @onclick="RenumberOptions">
                        <i class="bi bi-123"></i>
                        Re-number
                    </button>
                    <button class="bulk-btn" @onclick="ImportOptions">
                        <i class="bi bi-upload"></i>
                        Import
                    </button>
                </div>
            }
            else
            {
                @* CodeSet Mode *@
                <div class="codeset-selector">
                    @if (SelectedCodeSet != null)
                    {
                        <div class="codeset-current">
                            <div class="codeset-icon">
                                <i class="bi bi-collection"></i>
                            </div>
                            <div class="codeset-info">
                                <div class="codeset-name">@SelectedCodeSet.Name</div>
                                <div class="codeset-meta">@SelectedCodeSet.Items.Count items</div>
                            </div>
                            <button class="codeset-change-btn" @onclick="OpenCodeSetSelector">
                                Change
                            </button>
                        </div>

                        <div class="codeset-preview">
                            @foreach (var item in SelectedCodeSet.Items.Take(6))
                            {
                                <div class="codeset-preview-item">
                                    <span class="codeset-preview-value">@item.Value</span>
                                    <span>@item.LabelEn</span>
                                </div>
                            }
                            @if (SelectedCodeSet.Items.Count > 6)
                            {
                                <div class="codeset-preview-item" style="color: var(--text-muted); font-style: italic;">
                                    +@(SelectedCodeSet.Items.Count - 6) more items
                                </div>
                            }
                        </div>
                    }
                    else
                    {
                        <div class="codeset-empty">
                            <i class="bi bi-database-add"></i>
                            <span>No CodeSet selected</span>
                            <button class="codeset-change-btn" @onclick="OpenCodeSetSelector">
                                Select CodeSet
                            </button>
                        </div>
                    }
                </div>

                @* Bulk Actions for CodeSet *@
                <div class="bulk-actions">
                    <button class="bulk-btn" @onclick="EditCodeSet">
                        <i class="bi bi-pencil-square"></i>
                        Edit CodeSet
                    </button>
                    <button class="bulk-btn" @onclick="RefreshCodeSet">
                        <i class="bi bi-arrow-repeat"></i>
                        Refresh
                    </button>
                </div>
            }
        </div>
    }
</div>

@* Edit Option Modal *@
<OptionEditorModal @ref="optionEditorModal"
                   OnSave="SaveOption" />

@* CodeSet Selector Modal *@
<CodeSetSelectorModal @ref="codeSetSelectorModal"
                      OnSelect="SelectCodeSet" />

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = default!;
    [Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }
    [Parameter] public IEnumerable<CodeSet> AvailableCodeSets { get; set; } = [];

    private OptionEditorModal? optionEditorModal;
    private CodeSetSelectorModal? codeSetSelectorModal;

    private bool IsExpanded { get; set; } = true;
    private OptionsMode Mode { get; set; } = OptionsMode.Inline;
    private string QuickAddValue { get; set; } = "";
    private FieldOption? DraggedOption { get; set; }

    private List<FieldOption> Options => Field.Options?.ToList() ?? new();
    private CodeSet? SelectedCodeSet { get; set; }

    protected override void OnParametersSet()
    {
        // Determine mode based on field data
        Mode = !string.IsNullOrEmpty(Field.CodeSetId)
            ? OptionsMode.CodeSet
            : OptionsMode.Inline;

        // Load selected CodeSet if applicable
        if (Mode == OptionsMode.CodeSet && !string.IsNullOrEmpty(Field.CodeSetId))
        {
            SelectedCodeSet = AvailableCodeSets.FirstOrDefault(c => c.Id == Field.CodeSetId);
        }
    }

    private void ToggleExpanded() => IsExpanded = !IsExpanded;

    private async Task SetMode(OptionsMode mode)
    {
        Mode = mode;

        if (mode == OptionsMode.Inline)
        {
            // Clear CodeSet reference
            var updated = Field with { CodeSetId = null };
            await OnFieldChanged.InvokeAsync(updated);
        }
        else
        {
            // Clear inline options when switching to CodeSet
            var updated = Field with { Options = null };
            await OnFieldChanged.InvokeAsync(updated);
        }
    }

    #region Inline Options

    private async Task AddOption()
    {
        var newOption = new FieldOption
        {
            Value = (Options.Count + 1).ToString(),
            LabelEn = $"Option {Options.Count + 1}",
            SortOrder = Options.Count
        };

        await optionEditorModal?.Open(newOption)!;
    }

    private async Task EditOption(FieldOption option)
    {
        await optionEditorModal?.Open(option)!;
    }

    private async Task SaveOption(FieldOption option)
    {
        var newOptions = Options.ToList();
        var existingIndex = newOptions.FindIndex(o => o.Value == option.Value);

        if (existingIndex >= 0)
        {
            newOptions[existingIndex] = option;
        }
        else
        {
            newOptions.Add(option);
        }

        var updated = Field with { Options = newOptions.ToImmutableList() };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task DeleteOption(FieldOption option)
    {
        var newOptions = Options.Where(o => o.Value != option.Value).ToList();
        var updated = Field with { Options = newOptions.ToImmutableList() };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task HandleQuickAddKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(QuickAddValue))
        {
            await QuickAdd();
        }
    }

    private async Task QuickAdd()
    {
        if (string.IsNullOrWhiteSpace(QuickAddValue)) return;

        var newOption = new FieldOption
        {
            Value = (Options.Count + 1).ToString(),
            LabelEn = QuickAddValue.Trim(),
            SortOrder = Options.Count
        };

        var newOptions = Options.ToList();
        newOptions.Add(newOption);

        var updated = Field with { Options = newOptions.ToImmutableList() };
        await OnFieldChanged.InvokeAsync(updated);

        QuickAddValue = "";
    }

    #endregion

    #region Drag and Drop

    private void StartDrag(FieldOption option)
    {
        DraggedOption = option;
    }

    private async Task DropOn(FieldOption target)
    {
        if (DraggedOption == null || DraggedOption.Value == target.Value) return;

        var newOptions = Options.ToList();
        var draggedIndex = newOptions.FindIndex(o => o.Value == DraggedOption.Value);
        var targetIndex = newOptions.FindIndex(o => o.Value == target.Value);

        if (draggedIndex >= 0 && targetIndex >= 0)
        {
            // Remove and insert at new position
            newOptions.RemoveAt(draggedIndex);
            newOptions.Insert(targetIndex, DraggedOption);

            // Update sort orders
            for (int i = 0; i < newOptions.Count; i++)
            {
                newOptions[i] = newOptions[i] with { SortOrder = i };
            }

            var updated = Field with { Options = newOptions.ToImmutableList() };
            await OnFieldChanged.InvokeAsync(updated);
        }

        DraggedOption = null;
    }

    #endregion

    #region Bulk Actions

    private async Task SortAlphabetically()
    {
        var sorted = Options
            .OrderBy(o => o.LabelEn)
            .Select((o, i) => o with { SortOrder = i })
            .ToImmutableList();

        var updated = Field with { Options = sorted };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task RenumberOptions()
    {
        var renumbered = Options
            .Select((o, i) => o with { Value = (i + 1).ToString() })
            .ToImmutableList();

        var updated = Field with { Options = renumbered };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task ImportOptions()
    {
        // TODO: Open import modal
    }

    #endregion

    #region CodeSet

    private async Task OpenCodeSetSelector()
    {
        await codeSetSelectorModal?.Open()!;
    }

    private async Task SelectCodeSet(CodeSet codeSet)
    {
        SelectedCodeSet = codeSet;
        var updated = Field with { CodeSetId = codeSet.Id, Options = null };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task EditCodeSet()
    {
        // TODO: Navigate to CodeSet editor
    }

    private async Task RefreshCodeSet()
    {
        if (!string.IsNullOrEmpty(Field.CodeSetId))
        {
            SelectedCodeSet = AvailableCodeSets.FirstOrDefault(c => c.Id == Field.CodeSetId);
            StateHasChanged();
        }
    }

    #endregion

    private enum OptionsMode { Inline, CodeSet }
}
```

---

## CSS Styles

Add to `options-section.css`:

```css
/* ===== OPTIONS SECTION ===== */

/* Mode Switcher */
.mode-switcher {
    display: flex;
    gap: 4px;
    padding: 4px;
    background: var(--bg-tertiary);
    border-radius: var(--radius-md);
    margin-bottom: 16px;
}

.mode-btn {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
    padding: 10px 16px;
    border: none;
    background: transparent;
    border-radius: var(--radius-sm);
    font-size: 13px;
    font-weight: 500;
    color: var(--text-secondary);
    cursor: pointer;
    transition: all 0.15s;
}

.mode-btn:hover {
    color: var(--text-primary);
    background: var(--bg-secondary);
}

.mode-btn.active {
    background: var(--bg-primary);
    color: var(--primary);
    box-shadow: var(--shadow-sm);
}

/* Options List */
.options-list {
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    overflow: hidden;
}

.options-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px 14px;
    background: var(--bg-secondary);
    border-bottom: 1px solid var(--border-color);
}

.options-count {
    font-size: 12px;
    font-weight: 600;
    color: var(--text-secondary);
}

.options-badge {
    font-size: 11px;
    padding: 2px 8px;
    margin-left: 8px;
    border-radius: 10px;
    background: var(--primary-light);
    color: var(--primary);
}

.add-option-btn {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 6px 10px;
    border: none;
    background: var(--primary);
    color: white;
    border-radius: var(--radius-sm);
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
}

.add-option-btn:hover {
    background: var(--primary-hover);
}

/* Option Item */
.option-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px 14px;
    background: var(--bg-primary);
    border-bottom: 1px solid var(--border-color);
    cursor: grab;
    transition: background 0.15s;
}

.option-item:last-child {
    border-bottom: none;
}

.option-item:hover {
    background: var(--bg-secondary);
}

.option-item.dragging {
    background: var(--primary-light);
    border: 1px dashed var(--primary);
    opacity: 0.7;
}

.option-drag-handle {
    color: var(--text-muted);
    cursor: grab;
}

.option-value-badge {
    min-width: 40px;
    height: 24px;
    padding: 0 8px;
    background: var(--bg-tertiary);
    border-radius: var(--radius-sm);
    font-size: 11px;
    font-weight: 600;
    font-family: monospace;
    color: var(--text-secondary);
    display: flex;
    align-items: center;
    justify-content: center;
}

.option-labels {
    flex: 1;
    min-width: 0;
}

.option-label-en {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-primary);
}

.option-label-fr {
    font-size: 11px;
    color: var(--text-muted);
}

.option-default {
    font-size: 10px;
    padding: 2px 6px;
    background: var(--success-light);
    color: var(--success);
    border-radius: 10px;
    font-weight: 600;
}

.option-actions {
    display: flex;
    gap: 4px;
    opacity: 0;
    transition: opacity 0.15s;
}

.option-item:hover .option-actions {
    opacity: 1;
}

.option-action-btn {
    width: 28px;
    height: 28px;
    border: none;
    background: var(--bg-tertiary);
    border-radius: var(--radius-sm);
    color: var(--text-muted);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
}

.option-action-btn:hover {
    background: var(--primary-light);
    color: var(--primary);
}

.option-action-btn.danger:hover {
    background: var(--danger-light);
    color: var(--danger);
}

/* Quick Add */
.quick-add-row {
    display: flex;
    gap: 8px;
    padding: 12px;
    background: var(--bg-secondary);
    border-top: 1px solid var(--border-color);
}

.quick-add-input {
    flex: 1;
    padding: 8px 12px;
    border: 1px solid var(--border-color);
    border-radius: var(--radius-sm);
    font-size: 13px;
    background: var(--bg-primary);
    color: var(--text-primary);
}

.quick-add-input:focus {
    outline: none;
    border-color: var(--primary);
}

.quick-add-btn {
    padding: 8px 16px;
    background: var(--primary);
    color: white;
    border: none;
    border-radius: var(--radius-sm);
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
}

/* Bulk Actions */
.bulk-actions {
    display: flex;
    gap: 8px;
    margin-top: 12px;
}

.bulk-btn {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 8px 12px;
    border: 1px solid var(--border-color);
    background: var(--bg-primary);
    border-radius: var(--radius-sm);
    font-size: 12px;
    color: var(--text-secondary);
    cursor: pointer;
}

.bulk-btn:hover {
    border-color: var(--primary);
    color: var(--primary);
    background: var(--primary-light);
}

/* CodeSet Selector */
.codeset-selector {
    padding: 16px;
    background: var(--bg-secondary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
}

.codeset-current {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 14px;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    margin-bottom: 12px;
}

.codeset-icon {
    width: 40px;
    height: 40px;
    border-radius: var(--radius-md);
    background: var(--primary-light);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 18px;
    color: var(--primary);
}

.codeset-info { flex: 1; }

.codeset-name {
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary);
}

.codeset-meta {
    font-size: 12px;
    color: var(--text-muted);
}

.codeset-change-btn {
    padding: 8px 14px;
    border: 1px solid var(--border-color);
    background: var(--bg-primary);
    border-radius: var(--radius-sm);
    font-size: 12px;
    font-weight: 500;
    color: var(--text-secondary);
    cursor: pointer;
}

.codeset-change-btn:hover {
    border-color: var(--primary);
    color: var(--primary);
}

.codeset-preview {
    max-height: 160px;
    overflow-y: auto;
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    background: var(--bg-primary);
}

.codeset-preview-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 8px 12px;
    border-bottom: 1px solid var(--border-color);
    font-size: 13px;
}

.codeset-preview-value {
    font-family: monospace;
    font-size: 11px;
    color: var(--text-muted);
    background: var(--bg-tertiary);
    padding: 2px 6px;
    border-radius: 4px;
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the Enhanced Options Section for the property panel in my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/PropertyPanel/Sections/
- Schema: DynamicForms.Core.V4 (FieldOption record, CodeSetId property)

## Component: EnhancedOptionsSection.razor

This section manages select/radio/checkbox options.

### Features Required:

1. **Mode Switcher**
   - Inline: Custom options list
   - CodeSet: Reference to shared option set

2. **Inline Options List**
   - Header with count and badge
   - Add button in header
   - Drag handle for reordering
   - Value badge (monospace)
   - English/French labels
   - Default badge
   - Edit/Delete buttons (show on hover)
   - Quick add input at bottom

3. **Drag-to-Reorder**
   - Grab by handle
   - Visual feedback during drag
   - Update SortOrder on drop

4. **CodeSet Mode**
   - Current CodeSet display with icon
   - Item count and metadata
   - Change button
   - Preview of first 6 items
   - "+N more items" indicator

5. **Bulk Actions**
   - Inline: Sort A-Z, Re-number, Import
   - CodeSet: Edit CodeSet, Refresh

### Parameters:
```csharp
[Parameter] public FormFieldSchema Field { get; set; }
[Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }
[Parameter] public IEnumerable<CodeSet> AvailableCodeSets { get; set; }
```

### Schema Reference:
```csharp
public record FieldOption
{
    public required string Value { get; init; }
    public required string LabelEn { get; init; }
    public string? LabelFr { get; init; }
    public bool IsDefault { get; init; }
    public int SortOrder { get; init; }
}
```

Please implement complete, production-ready code with proper CSS styling.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `EnhancedOptionsSection-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for mode switcher testing (Inline vs CodeSet)
- Inline options list display testing
- Add button and option editor modal testing
- Edit button with existing option testing
- Delete button testing
- Drag-to-reorder testing (grab handle, drag, drop)
- Quick add input testing (type + Enter)
- Sort A-Z bulk action testing
- Re-number bulk action testing
- CodeSet mode selection testing
- CodeSet preview display testing
- Change CodeSet button testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- OptionEditorModal component creation
- CodeSetSelectorModal component creation
- AvailableCodeSets provisioning (from service or context)
- Import options modal (TODO in code)
- Edit CodeSet navigation implementation
- CSS imports for options-section.css
- ImmutableList usage from System.Collections.Immutable

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Mode switcher toggles between Inline and CodeSet
- [ ] Mode persists based on field data
- [ ] Inline mode clears CodeSetId
- [ ] CodeSet mode clears Options
- [ ] Options list shows count and "Inline" badge
- [ ] Add button opens OptionEditorModal
- [ ] Edit button loads option into modal
- [ ] Delete removes option from list
- [ ] Drag handle cursor is "grab"
- [ ] Dragging option shows visual feedback
- [ ] Drop reorders options correctly
- [ ] SortOrder updated after drag
- [ ] Quick add works with Enter key
- [ ] Quick add clears input after adding
- [ ] Sort A-Z alphabetizes by LabelEn
- [ ] Re-number updates Value sequentially
- [ ] CodeSet mode shows selected set info
- [ ] CodeSet preview shows first 6 items
- [ ] "+N more items" shows when > 6
- [ ] Change button opens CodeSetSelectorModal
- [ ] Section expands/collapses correctly
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Mode switcher toggles correctly
- [ ] Options list displays with all elements
- [ ] Add button opens option editor
- [ ] Edit button opens option editor with data
- [ ] Delete button removes option
- [ ] Drag reorders options correctly
- [ ] Quick add works with Enter key
- [ ] Sort A-Z alphabetizes options
- [ ] Re-number updates values sequentially
- [ ] CodeSet mode displays selected set
- [ ] Change button opens CodeSet selector
- [ ] CodeSet preview shows items
- [ ] Dark mode styling correct

---

## Notes

- Drag-and-drop uses native HTML5 API
- Consider adding keyboard accessibility for reordering
- OptionEditorModal and CodeSetSelectorModal are separate components
- CodeSet model should be defined in Core.V4
