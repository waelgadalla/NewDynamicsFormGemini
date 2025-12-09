# ModalBase Component - Implementation Plan

> **Component**: `ModalBase.razor`
> **Location**: `Src/VisualEditorOpus/Components/Shared/ModalBase.razor`
> **Priority**: Critical (Foundation for all other modals)
> **Estimated Effort**: 3-4 hours

---

## Overview

ModalBase is the foundational modal component that all other modals in the application will extend. It provides consistent styling, animations, accessibility features, and behavior across the entire application.

---

## Features

| Feature | Description |
|---------|-------------|
| Size Variants | `sm` (400px), `md` (560px), `lg` (720px), `xl` (900px), `fullscreen` |
| Backdrop | Semi-transparent with blur effect, optional click-to-close |
| Animations | Smooth fade + scale entry/exit animations |
| Keyboard | ESC to close, focus trap within modal |
| Accessibility | ARIA attributes, focus management, screen reader support |
| Slots | Header, Body, Footer as RenderFragments |
| Customization | Custom CSS class support, icon, badge |

---

## Component API

### Parameters

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public EventCallback OnClose { get; set; }

[Parameter] public string Title { get; set; } = "Modal";
[Parameter] public string? Icon { get; set; }  // Bootstrap icon class e.g., "bi-gear"
[Parameter] public string? Badge { get; set; } // Optional badge text next to title

[Parameter] public ModalSize Size { get; set; } = ModalSize.Medium;
[Parameter] public bool CloseOnBackdropClick { get; set; } = true;
[Parameter] public bool CloseOnEscape { get; set; } = true;
[Parameter] public bool ShowCloseButton { get; set; } = true;

[Parameter] public RenderFragment? ChildContent { get; set; }  // Body content
[Parameter] public RenderFragment? FooterContent { get; set; } // Footer content
[Parameter] public RenderFragment? HeaderExtra { get; set; }   // Extra header content

[Parameter] public string? CssClass { get; set; }  // Additional CSS classes
[Parameter] public bool IsLoading { get; set; } = false;
```

### Enums

```csharp
public enum ModalSize
{
    Small,      // 400px
    Medium,     // 560px (default)
    Large,      // 720px
    ExtraLarge, // 900px
    Fullscreen  // 100vw - 40px
}
```

---

## File Structure

```
Components/Shared/
├── ModalBase.razor
├── ModalBase.razor.cs
└── ModalBase.razor.css
```

---

## Implementation Details

### 1. Focus Trap
When the modal opens, focus should be trapped within the modal. Tab and Shift+Tab should cycle through focusable elements only within the modal.

### 2. Focus Restoration
When the modal closes, focus should return to the element that was focused before the modal opened.

### 3. Body Scroll Lock
When modal is open, the page body should not scroll. Add `overflow: hidden` to body.

### 4. Animation Timing
- Backdrop fade: 200ms
- Modal scale/translate: 200ms
- Use CSS transitions, not JS animations

### 5. Z-Index Management
- Backdrop: 2000
- Modal: 2001
- Nested modals should stack properly

---

## CSS Classes Reference

These CSS classes already exist in `wwwroot/css/app.css`:

```css
.modal-backdrop    /* Fixed backdrop with blur */
.modal             /* Modal container */
.modal-sm/md/lg/xl /* Size variants */
.modal-header      /* Header with border */
.modal-title       /* Title with icon */
.modal-close       /* Close button */
.modal-body        /* Scrollable content area */
.modal-footer      /* Footer with buttons */
```

---

## Dependencies

- **CSS Variables**: Uses design tokens from `variables.css`
- **Bootstrap Icons**: For close button and optional title icon
- **No JS Interop Required**: Pure Blazor implementation

---

## Usage Example

```razor
<ModalBase @bind-IsOpen="showModal"
           Title="Edit Field"
           Icon="bi-pencil"
           Size="ModalSize.Medium"
           OnClose="HandleClose">

    <div class="form-group">
        <label class="form-label">Field Name</label>
        <input type="text" class="form-input" @bind="fieldName" />
    </div>

    <FooterContent>
        <button class="btn btn-ghost" @onclick="Cancel">Cancel</button>
        <button class="btn btn-primary" @onclick="Save">
            <i class="bi bi-check-lg"></i> Save
        </button>
    </FooterContent>
</ModalBase>
```

---

## Accessibility Requirements

| Requirement | Implementation |
|-------------|----------------|
| `role="dialog"` | On modal container |
| `aria-modal="true"` | On modal container |
| `aria-labelledby` | Reference to title element |
| `aria-describedby` | Optional, reference to description |
| Focus trap | Tab cycles within modal only |
| ESC to close | Keyboard event handler |
| Screen reader announcement | Live region for open/close |

---

## Testing Checklist

- [ ] Modal opens with animation
- [ ] Modal closes with animation
- [ ] ESC key closes modal
- [ ] Click outside closes modal (when enabled)
- [ ] Focus trapped within modal
- [ ] Focus returns to trigger on close
- [ ] All size variants render correctly
- [ ] Dark mode styling works
- [ ] Loading state displays spinner
- [ ] Scrollable content works in body
- [ ] Footer buttons align correctly
- [ ] Screen reader announces modal

---

## Claude Implementation Prompt

Copy and paste the following prompt to Claude to implement this component:

---

### PROMPT START

```
I need you to implement the ModalBase component for my Blazor application. This is the foundation modal that all other modals will extend.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Shared/
- Styling: CSS already exists in wwwroot/css/app.css (modal-backdrop, modal, modal-header, etc.)
- Design System: Uses CSS variables from variables.css (--primary, --bg-primary, etc.)

## Files to Create

### 1. ModalBase.razor
Create the Razor markup with:
- Conditional rendering based on IsOpen
- Backdrop div with click handler
- Modal container with size class
- Header with icon, title, optional badge, close button
- Body with ChildContent RenderFragment
- Footer with FooterContent RenderFragment
- Loading state overlay

### 2. ModalBase.razor.cs
Create the code-behind with:
- All parameters listed below
- Open/Close methods
- Keyboard event handling (ESC)
- Focus management (trap and restore)

### 3. ModalBase.razor.css (scoped styles)
Add any component-specific styles not in app.css:
- Animation keyframes
- Loading overlay styles
- Focus visible states

## Parameters Required

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public EventCallback OnClose { get; set; }
[Parameter] public string Title { get; set; } = "Modal";
[Parameter] public string? Icon { get; set; }
[Parameter] public string? Badge { get; set; }
[Parameter] public ModalSize Size { get; set; } = ModalSize.Medium;
[Parameter] public bool CloseOnBackdropClick { get; set; } = true;
[Parameter] public bool CloseOnEscape { get; set; } = true;
[Parameter] public bool ShowCloseButton { get; set; } = true;
[Parameter] public RenderFragment? ChildContent { get; set; }
[Parameter] public RenderFragment? FooterContent { get; set; }
[Parameter] public RenderFragment? HeaderExtra { get; set; }
[Parameter] public string? CssClass { get; set; }
[Parameter] public bool IsLoading { get; set; } = false;
```

## ModalSize Enum
Create in Models folder or same file:
```csharp
public enum ModalSize { Small, Medium, Large, ExtraLarge, Fullscreen }
```

## Key Implementation Requirements

1. **Two-way binding**: Support @bind-IsOpen pattern
2. **Animations**: Use CSS transitions (already defined in app.css), toggle 'active' class
3. **ESC handling**: Use @onkeydown on the backdrop, check for Escape key
4. **Backdrop click**: Only close if clicking directly on backdrop (not modal content)
5. **Focus trap**: On open, focus first focusable element; trap Tab key within modal
6. **Body scroll lock**: When open, add overflow:hidden to body (use JS interop if needed)
7. **Loading state**: Show spinner overlay when IsLoading=true, disable interactions

## CSS Class Mapping
```
ModalSize.Small -> "modal-sm"
ModalSize.Medium -> "modal-md" (or no class, as md is default)
ModalSize.Large -> "modal-lg"
ModalSize.ExtraLarge -> "modal-xl"
ModalSize.Fullscreen -> "modal-fullscreen"
```

## Existing CSS (from app.css) - DO NOT recreate these:
- .modal-backdrop (fixed positioning, blur)
- .modal (flex container, shadow)
- .modal-header, .modal-title, .modal-close
- .modal-body (scrollable)
- .modal-footer
- .btn, .btn-primary, .btn-ghost, etc.

## Accessibility
- Add role="dialog" and aria-modal="true" to modal
- Add aria-labelledby pointing to title
- Ensure close button has aria-label="Close"

## Example Usage (for reference)
```razor
<ModalBase @bind-IsOpen="showSettings"
           Title="Settings"
           Icon="bi-gear"
           Size="ModalSize.Large">
    <p>Modal content here</p>

    <FooterContent>
        <button class="btn btn-ghost" @onclick="() => showSettings = false">Cancel</button>
        <button class="btn btn-primary" @onclick="Save">Save</button>
    </FooterContent>
</ModalBase>
```

Please implement all three files with complete, production-ready code. Include XML documentation comments on public members.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `ModalBase-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing modal open/close animations
- Keyboard interaction testing (ESC key, Tab key focus trap)
- Backdrop click behavior testing
- Size variant testing (Small, Medium, Large, ExtraLarge, Fullscreen)
- Loading state testing
- Focus management verification (trap and restore)
- Body scroll lock verification
- Accessibility testing (screen reader, ARIA attributes)
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- CSS file imports to add
- Component registration in _Imports.razor
- Integration with existing components that need modals
- Body scroll lock JS interop setup (if required)
- Focus management service setup (if required)
- Testing modal with sample content

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens with fade + scale animation
- [ ] Modal closes with reverse animation
- [ ] ESC key closes modal when enabled
- [ ] Click outside closes modal when enabled
- [ ] Focus trapped within modal
- [ ] Focus returns to trigger on close
- [ ] All 5 size variants render correctly (sm, md, lg, xl, fullscreen)
- [ ] Loading state displays spinner overlay
- [ ] Body scroll locked when modal open
- [ ] Header icon displays correctly
- [ ] Header badge displays correctly
- [ ] Footer content renders correctly
- [ ] Dark mode styling correct
- [ ] Screen reader announces modal open/close
- [ ] ARIA attributes present and correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Modal doesn't animate | Ensure 'active' class is toggled, not display:none |
| Focus escapes modal | Implement proper focus trap with Tab key handling |
| Body still scrolls | Use JS interop to set body overflow |
| Multiple ESC handlers | Only handle ESC in topmost modal |
| Click on modal closes it | Use `@onclick:stopPropagation` on modal container |

---

## Related Components

After implementing ModalBase, these modals will use it:
- ConfirmDeleteModal
- ConditionBuilderModal
- FormulaEditorModal
- TypeConfigModal
- MetadataModal
- ImportJsonModal
- CrossFieldValidationModal
- WorkflowRulesModal
