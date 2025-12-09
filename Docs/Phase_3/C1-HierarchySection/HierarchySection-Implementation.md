# C.1 Hierarchy Section - Implementation Plan

> **Task**: Hierarchy Section for Property Panel
> **Location**: `Src/VisualEditorOpus/Components/PropertyPanel/Sections/`
> **Priority**: High
> **Estimated Effort**: 2-3 hours
> **Delegation**: 90% AI

---

## Overview

The Hierarchy Section allows users to manage parent-child relationships between form fields. It provides a tree selector for choosing parent fields and relationship type options for defining how fields interact.

---

## Schema Reference

From `DynamicForms.Core.V4`:

```csharp
// FormFieldSchema.cs
public string? ParentId { get; init; }
public RelationshipType Relationship { get; init; } = RelationshipType.Container;

// RelationshipType.cs
public enum RelationshipType
{
    Container = 0,    // Visually grouped within parent
    Conditional = 1,  // Visibility depends on parent value
    Cascade = 2,      // Options depend on parent selection
    Validation = 3    // Validation rules reference parent
}
```

---

## Component to Create

### HierarchySection.razor

```razor
@namespace VisualEditorOpus.Components.PropertyPanel.Sections

<div class="property-section">
    <div class="section-header @(IsExpanded ? "expanded" : "")" @onclick="ToggleExpanded">
        <div class="section-header-left">
            <i class="bi bi-diagram-3"></i>
            <span>Hierarchy</span>
        </div>
        <i class="bi bi-chevron-down section-chevron"></i>
    </div>

    @if (IsExpanded)
    {
        <div class="section-content">
            @* Status Badge *@
            <div class="hierarchy-status @(HasParent ? "has-parent" : "")">
                <i class="bi bi-@(HasParent ? "diagram-3" : "house")"></i>
                <span class="hierarchy-status-text">
                    @if (HasParent)
                    {
                        @($"Nested under \"{ParentFieldLabel}\"")
                    }
                    else
                    {
                        @("This field is at the root level")
                    }
                </span>
            </div>

            @* Breadcrumb Path (when has parent) *@
            @if (HasParent)
            {
                <div class="hierarchy-breadcrumb">
                    @foreach (var (item, index) in BreadcrumbPath.Select((x, i) => (x, i)))
                    {
                        @if (index > 0)
                        {
                            <i class="bi bi-chevron-right breadcrumb-separator"></i>
                        }
                        <div class="breadcrumb-item @(item.IsRoot ? "root" : "") @(item.IsCurrent ? "current" : "")">
                            <i class="bi bi-@item.Icon"></i>
                            @item.Label
                        </div>
                    }
                </div>
            }

            @* Parent Field Selector *@
            <div class="form-group">
                <label class="form-label">
                    <i class="bi bi-arrow-up-square"></i>
                    Parent Field
                </label>
                <div class="tree-selector">
                    @* Root Option *@
                    <div class="tree-node root-option @(Field.ParentId == null ? "selected" : "")"
                         @onclick="() => SelectParent(null)">
                        <div class="tree-node-icon">
                            <i class="bi bi-house"></i>
                        </div>
                        <div class="tree-node-content">
                            <div class="tree-node-label">Root (No Parent)</div>
                            <div class="tree-node-type">Top-level field</div>
                        </div>
                        @if (Field.ParentId == null)
                        {
                            <i class="bi bi-check-lg tree-node-check"></i>
                        }
                    </div>

                    @* Available Parent Fields *@
                    @foreach (var node in AvailableParents)
                    {
                        <div class="tree-node @(node.IsSelected ? "selected" : "") @(node.IsDisabled ? "disabled" : "")"
                             @onclick="() => SelectParent(node.FieldId)">
                            @if (node.Depth > 0)
                            {
                                <div class="tree-node-indent">
                                    @for (int i = 0; i < node.Depth; i++)
                                    {
                                        <div class="tree-indent-line"></div>
                                    }
                                </div>
                            }
                            <div class="tree-node-icon">
                                <i class="bi bi-@node.Icon"></i>
                            </div>
                            <div class="tree-node-content">
                                <div class="tree-node-label">@node.Label</div>
                                <div class="tree-node-type">@node.TypeLabel</div>
                            </div>
                            @if (node.IsSelected)
                            {
                                <i class="bi bi-check-lg tree-node-check"></i>
                            }
                        </div>
                    }
                </div>
            </div>

            @* Relationship Type (only when has parent) *@
            @if (HasParent)
            {
                <div class="form-group">
                    <label class="form-label">
                        <i class="bi bi-link-45deg"></i>
                        Relationship Type
                    </label>
                    <div class="relationship-selector">
                        @foreach (var rel in RelationshipOptions)
                        {
                            <div class="relationship-option @(Field.Relationship == rel.Value ? "selected" : "")"
                                 @onclick="() => SelectRelationship(rel.Value)">
                                <div class="relationship-option-icon">
                                    <i class="bi bi-@rel.Icon"></i>
                                </div>
                                <div class="relationship-option-content">
                                    <div class="relationship-option-label">@rel.Label</div>
                                    <div class="relationship-option-desc">@rel.Description</div>
                                </div>
                            </div>
                        }
                    </div>
                </div>
            }

            @* Children List (for parent fields) *@
            @if (ChildFields.Any())
            {
                <div class="children-list">
                    <div class="children-header">
                        <span class="children-header-title">Child Fields</span>
                        <span class="children-count">@ChildFields.Count() fields</span>
                    </div>
                    @foreach (var child in ChildFields)
                    {
                        <div class="child-item">
                            <div class="child-icon">
                                <i class="bi bi-@child.Icon"></i>
                            </div>
                            <span class="child-name">@child.Label</span>
                            <span class="child-relationship @child.Relationship.ToString().ToLower()">
                                @child.Relationship
                            </span>
                        </div>
                    }
                </div>
            }

            @* Quick Actions *@
            @if (HasParent)
            {
                <div class="hierarchy-actions">
                    <button class="action-btn" @onclick="MoveUp" disabled="@(!CanMoveUp)">
                        <i class="bi bi-arrow-up"></i>
                        Move Up
                    </button>
                    <button class="action-btn danger" @onclick="Unparent">
                        <i class="bi bi-box-arrow-up"></i>
                        Unparent
                    </button>
                </div>
            }
        </div>
    }
</div>

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = default!;
    [Parameter] public IEnumerable<FormFieldSchema> AllFields { get; set; } = [];
    [Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }

    private bool IsExpanded { get; set; } = true;
    private bool HasParent => !string.IsNullOrEmpty(Field.ParentId);
    private string ParentFieldLabel => AllFields.FirstOrDefault(f => f.Id == Field.ParentId)?.LabelEn ?? "Unknown";

    private IEnumerable<TreeNode> AvailableParents { get; set; } = [];
    private IEnumerable<ChildFieldInfo> ChildFields { get; set; } = [];
    private IEnumerable<BreadcrumbItem> BreadcrumbPath { get; set; } = [];
    private bool CanMoveUp => true; // TODO: Implement logic

    private static readonly RelationshipOption[] RelationshipOptions = new[]
    {
        new RelationshipOption(RelationshipType.Container, "box", "Container", "Grouped within parent"),
        new RelationshipOption(RelationshipType.Conditional, "question-circle", "Conditional", "Shown based on parent"),
        new RelationshipOption(RelationshipType.Cascade, "arrow-down-circle", "Cascade", "Value depends on parent"),
        new RelationshipOption(RelationshipType.Validation, "shield-check", "Validation", "Validates with parent")
    };

    protected override void OnParametersSet()
    {
        BuildAvailableParents();
        BuildChildFields();
        BuildBreadcrumbPath();
    }

    private void ToggleExpanded() => IsExpanded = !IsExpanded;

    private void BuildAvailableParents()
    {
        // Build tree of fields that can be parents
        // Exclude: current field, descendants of current field
        var descendants = GetDescendants(Field.Id).ToHashSet();

        AvailableParents = AllFields
            .Where(f => f.Id != Field.Id && !descendants.Contains(f.Id))
            .Select(f => new TreeNode
            {
                FieldId = f.Id,
                Label = f.LabelEn ?? f.Id,
                TypeLabel = f.Type.ToString(),
                Icon = GetFieldIcon(f.Type),
                Depth = GetFieldDepth(f),
                IsSelected = f.Id == Field.ParentId,
                IsDisabled = f.Id == Field.Id
            })
            .OrderBy(n => GetFieldPath(n.FieldId))
            .ToList();
    }

    private void BuildChildFields()
    {
        ChildFields = AllFields
            .Where(f => f.ParentId == Field.Id)
            .Select(f => new ChildFieldInfo
            {
                Label = f.LabelEn ?? f.Id,
                Icon = GetFieldIcon(f.Type),
                Relationship = f.Relationship
            })
            .ToList();
    }

    private void BuildBreadcrumbPath()
    {
        var path = new List<BreadcrumbItem>();
        path.Add(new BreadcrumbItem { Label = "Root", Icon = "house", IsRoot = true });

        var current = Field;
        var ancestors = new List<FormFieldSchema>();

        while (!string.IsNullOrEmpty(current.ParentId))
        {
            var parent = AllFields.FirstOrDefault(f => f.Id == current.ParentId);
            if (parent == null) break;
            ancestors.Insert(0, parent);
            current = parent;
        }

        foreach (var ancestor in ancestors)
        {
            path.Add(new BreadcrumbItem
            {
                Label = ancestor.LabelEn ?? ancestor.Id,
                Icon = GetFieldIcon(ancestor.Type)
            });
        }

        path.Add(new BreadcrumbItem
        {
            Label = Field.LabelEn ?? Field.Id,
            Icon = GetFieldIcon(Field.Type),
            IsCurrent = true
        });

        BreadcrumbPath = path;
    }

    private async Task SelectParent(string? parentId)
    {
        if (parentId == Field.Id) return;

        var updated = Field with { ParentId = parentId };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task SelectRelationship(RelationshipType relationship)
    {
        var updated = Field with { Relationship = relationship };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task Unparent()
    {
        var updated = Field with { ParentId = null, Relationship = RelationshipType.Container };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task MoveUp()
    {
        // Move to parent's parent
        if (string.IsNullOrEmpty(Field.ParentId)) return;

        var parent = AllFields.FirstOrDefault(f => f.Id == Field.ParentId);
        var updated = Field with { ParentId = parent?.ParentId };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private IEnumerable<string> GetDescendants(string fieldId)
    {
        var children = AllFields.Where(f => f.ParentId == fieldId);
        foreach (var child in children)
        {
            yield return child.Id;
            foreach (var grandchild in GetDescendants(child.Id))
            {
                yield return grandchild;
            }
        }
    }

    private int GetFieldDepth(FormFieldSchema field)
    {
        int depth = 0;
        var current = field;
        while (!string.IsNullOrEmpty(current.ParentId))
        {
            depth++;
            current = AllFields.FirstOrDefault(f => f.Id == current.ParentId);
            if (current == null) break;
        }
        return depth;
    }

    private string GetFieldPath(string fieldId)
    {
        var path = new List<string>();
        var field = AllFields.FirstOrDefault(f => f.Id == fieldId);
        while (field != null)
        {
            path.Insert(0, field.Id);
            field = AllFields.FirstOrDefault(f => f.Id == field.ParentId);
        }
        return string.Join("/", path);
    }

    private static string GetFieldIcon(FieldType type) => type switch
    {
        FieldType.Text => "input-cursor-text",
        FieldType.Number => "123",
        FieldType.Email => "envelope",
        FieldType.Phone => "telephone",
        FieldType.Date => "calendar",
        FieldType.Time => "clock",
        FieldType.DateTime => "calendar-event",
        FieldType.Select => "menu-down",
        FieldType.MultiSelect => "list-check",
        FieldType.Radio => "record-circle",
        FieldType.Checkbox => "check-square",
        FieldType.Toggle => "toggle-on",
        FieldType.TextArea => "textarea-resize",
        FieldType.RichText => "text-paragraph",
        FieldType.File => "paperclip",
        FieldType.Image => "image",
        FieldType.Signature => "pen",
        FieldType.Section => "card-list",
        FieldType.Repeater => "collection",
        FieldType.Calculated => "calculator",
        FieldType.Hidden => "eye-slash",
        _ => "square"
    };

    // Helper Records
    private record TreeNode
    {
        public string FieldId { get; init; } = "";
        public string Label { get; init; } = "";
        public string TypeLabel { get; init; } = "";
        public string Icon { get; init; } = "";
        public int Depth { get; init; }
        public bool IsSelected { get; init; }
        public bool IsDisabled { get; init; }
    }

    private record ChildFieldInfo
    {
        public string Label { get; init; } = "";
        public string Icon { get; init; } = "";
        public RelationshipType Relationship { get; init; }
    }

    private record BreadcrumbItem
    {
        public string Label { get; init; } = "";
        public string Icon { get; init; } = "";
        public bool IsRoot { get; init; }
        public bool IsCurrent { get; init; }
    }

    private record RelationshipOption(RelationshipType Value, string Icon, string Label, string Description);
}
```

---

## CSS Styles

Add to `hierarchy-section.css`:

```css
/* ===== HIERARCHY SECTION ===== */

/* Status Badge */
.hierarchy-status {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 14px;
    background: var(--bg-tertiary);
    border-radius: var(--radius-md);
    margin-bottom: 16px;
}

.hierarchy-status.has-parent {
    background: var(--primary-light);
}

.hierarchy-status i {
    font-size: 16px;
    color: var(--text-muted);
}

.hierarchy-status.has-parent i {
    color: var(--primary);
}

.hierarchy-status-text {
    flex: 1;
    font-size: 13px;
    color: var(--text-secondary);
}

.hierarchy-status.has-parent .hierarchy-status-text {
    color: var(--primary);
    font-weight: 500;
}

/* Breadcrumb Path */
.hierarchy-breadcrumb {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 12px 14px;
    background: var(--bg-secondary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    margin-bottom: 16px;
    overflow-x: auto;
}

.breadcrumb-item {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 13px;
    color: var(--text-secondary);
    white-space: nowrap;
}

.breadcrumb-item i {
    font-size: 12px;
}

.breadcrumb-separator {
    color: var(--text-muted);
    font-size: 10px;
}

.breadcrumb-item.current {
    color: var(--primary);
    font-weight: 600;
}

.breadcrumb-item.root i {
    color: var(--warning);
}

/* Tree Selector */
.tree-selector {
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    max-height: 200px;
    overflow-y: auto;
}

.tree-node {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 12px;
    cursor: pointer;
    transition: all 0.15s;
    border-bottom: 1px solid var(--border-color);
}

.tree-node:last-child {
    border-bottom: none;
}

.tree-node:hover {
    background: var(--bg-secondary);
}

.tree-node.selected {
    background: var(--primary-light);
}

.tree-node.disabled {
    opacity: 0.5;
    cursor: not-allowed;
    pointer-events: none;
}

.tree-node-indent {
    display: flex;
    align-items: center;
}

.tree-indent-line {
    width: 20px;
    height: 100%;
    border-left: 1px solid var(--border-color);
    margin-left: 8px;
}

.tree-node-icon {
    width: 28px;
    height: 28px;
    border-radius: var(--radius-sm);
    background: var(--bg-tertiary);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    color: var(--text-muted);
    flex-shrink: 0;
}

.tree-node.selected .tree-node-icon {
    background: var(--primary);
    color: white;
}

.tree-node-content {
    flex: 1;
    min-width: 0;
}

.tree-node-label {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-primary);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.tree-node-type {
    font-size: 11px;
    color: var(--text-muted);
}

.tree-node-check {
    color: var(--primary);
    font-size: 14px;
}

.tree-node.root-option {
    background: var(--bg-secondary);
    border-bottom: 2px solid var(--border-color);
}

.tree-node.root-option .tree-node-icon {
    background: var(--warning-light);
    color: var(--warning);
}

/* Relationship Type Selector */
.relationship-selector {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 8px;
}

.relationship-option {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px;
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    cursor: pointer;
    transition: all 0.15s;
}

.relationship-option:hover {
    border-color: var(--primary);
    background: var(--bg-secondary);
}

.relationship-option.selected {
    border-color: var(--primary);
    background: var(--primary-light);
}

.relationship-option-icon {
    width: 36px;
    height: 36px;
    border-radius: var(--radius-sm);
    background: var(--bg-tertiary);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 16px;
    color: var(--text-muted);
}

.relationship-option.selected .relationship-option-icon {
    background: var(--primary);
    color: white;
}

.relationship-option-content {
    flex: 1;
}

.relationship-option-label {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-primary);
}

.relationship-option-desc {
    font-size: 11px;
    color: var(--text-muted);
}

/* Children List */
.children-list {
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    overflow: hidden;
    margin-top: 16px;
}

.children-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px 12px;
    background: var(--bg-secondary);
    border-bottom: 1px solid var(--border-color);
}

.children-header-title {
    font-size: 12px;
    font-weight: 600;
    color: var(--text-secondary);
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.children-count {
    font-size: 11px;
    color: var(--text-muted);
    background: var(--bg-tertiary);
    padding: 2px 8px;
    border-radius: 10px;
}

.child-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 10px 12px;
    border-bottom: 1px solid var(--border-color);
}

.child-item:last-child {
    border-bottom: none;
}

.child-icon {
    width: 24px;
    height: 24px;
    border-radius: var(--radius-sm);
    background: var(--bg-tertiary);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 12px;
    color: var(--text-muted);
}

.child-name {
    flex: 1;
    font-size: 13px;
    color: var(--text-primary);
}

.child-relationship {
    font-size: 11px;
    padding: 2px 8px;
    border-radius: 10px;
    background: var(--bg-tertiary);
    color: var(--text-secondary);
}

.child-relationship.container {
    background: var(--primary-light);
    color: var(--primary);
}

.child-relationship.conditional {
    background: var(--warning-light);
    color: var(--warning);
}

.child-relationship.cascade {
    background: var(--success-light);
    color: var(--success);
}

.child-relationship.validation {
    background: var(--danger-light);
    color: var(--danger);
}

/* Quick Actions */
.hierarchy-actions {
    display: flex;
    gap: 8px;
    margin-top: 16px;
    padding-top: 16px;
    border-top: 1px solid var(--border-color);
}

.action-btn {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
    padding: 10px;
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    background: var(--bg-primary);
    color: var(--text-secondary);
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.15s;
}

.action-btn:hover:not(:disabled) {
    border-color: var(--primary);
    color: var(--primary);
    background: var(--primary-light);
}

.action-btn.danger:hover:not(:disabled) {
    border-color: var(--danger);
    color: var(--danger);
    background: var(--danger-light);
}

.action-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the Hierarchy Section component for the property panel in my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/PropertyPanel/Sections/
- Schema: DynamicForms.Core.V4 (FormFieldSchema has ParentId and Relationship properties)

## Component: HierarchySection.razor

This section manages parent-child relationships between form fields.

### Features Required:

1. **Status Badge**
   - Shows if field is at root level or nested
   - "This field is at the root level" for root fields
   - "Nested under [ParentName]" for child fields
   - Different styling for each state

2. **Breadcrumb Path** (when has parent)
   - Shows: Root > Parent1 > Parent2 > Current
   - Icons for each level
   - Current field highlighted

3. **Parent Field Selector**
   - Tree view of all available parent fields
   - "Root (No Parent)" option at top
   - Indent child fields visually
   - Show field type label
   - Disable: current field, descendants of current field
   - Selected state with checkmark

4. **Relationship Type Selector** (only when has parent)
   - Grid of 4 options: Container, Conditional, Cascade, Validation
   - Each shows: icon, label, short description
   - Selected state styling

5. **Children List** (for parent fields)
   - Header with count
   - Each child: icon, name, relationship badge
   - Color-coded relationship badges

6. **Quick Actions** (when has parent)
   - "Move Up" - move to parent's parent
   - "Unparent" - move to root level

### Parameters:
```csharp
[Parameter] public FormFieldSchema Field { get; set; }
[Parameter] public IEnumerable<FormFieldSchema> AllFields { get; set; }
[Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }
```

### Schema Reference:
```csharp
// From FormFieldSchema
public string? ParentId { get; init; }
public RelationshipType Relationship { get; init; } = RelationshipType.Container;

// RelationshipType enum
Container = 0,    // Visually grouped within parent
Conditional = 1,  // Visibility depends on parent value
Cascade = 2,      // Options depend on parent selection
Validation = 3    // Validation rules reference parent
```

### Key Logic:
- Prevent circular references (can't set descendant as parent)
- GetDescendants(fieldId) to find all children recursively
- GetFieldDepth(field) to calculate nesting level
- BuildBreadcrumbPath() to show ancestry

Please implement complete, production-ready code with proper CSS styling.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `HierarchySection-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing root level field display
- Nested field status badge testing
- Breadcrumb path navigation testing
- Parent field selector tree view testing
- Circular reference prevention testing
- Relationship type selection testing (Container, Conditional, Cascade, Validation)
- Children list display testing
- Move Up action testing
- Unparent action testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Integration with RightSidebar property panel
- AllFields collection provisioning from EditorStateService
- OnFieldChanged callback wiring
- CSS imports for hierarchy-section.css
- RelationshipType enum import from Core.V4
- Tree node depth calculation verification

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Root level field shows "at root level" status
- [ ] Nested field shows parent name in status
- [ ] Breadcrumb shows correct ancestry path
- [ ] Root > Parent > Current navigation displays
- [ ] Tree selector shows all available fields
- [ ] Indent lines display for nested fields
- [ ] Current field disabled in parent selector
- [ ] Descendants disabled in parent selector (no circular refs)
- [ ] Selecting parent updates field.ParentId
- [ ] Relationship selector appears when has parent
- [ ] All 4 relationship types selectable
- [ ] Children list shows child fields
- [ ] Relationship badges are color-coded
- [ ] Move Up moves to grandparent
- [ ] Unparent moves to root level
- [ ] Section expands/collapses correctly
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Root level field shows "at root level" status
- [ ] Nested field shows parent name in status
- [ ] Breadcrumb shows correct path
- [ ] Tree selector shows all fields with proper indentation
- [ ] Current field is disabled in selector
- [ ] Descendants are disabled in selector
- [ ] Selecting parent updates field
- [ ] Relationship type selector appears when has parent
- [ ] Relationship selection updates field
- [ ] Children list shows child fields
- [ ] Relationship badges are color-coded
- [ ] Move Up moves field to grandparent
- [ ] Unparent moves field to root
- [ ] Dark mode styling correct
- [ ] Section collapses/expands

---

## Notes

- Always prevent circular references
- Update all related fields when moving (cascade updates)
- Consider warning when unparenting a field with conditional children
- Tree depth should be limited (max 5-6 levels)
- Consider adding drag-drop reordering in future
