# A.2 Micro-interactions & Animations - Implementation Plan

> **Task**: Micro-interactions & Animations
> **Location**: `Src/VisualEditorOpus/wwwroot/css/animations.css`
> **Priority**: Critical
> **Estimated Effort**: 3-4 hours
> **Delegation**: 90% AI

---

## Overview

Add subtle, professional animations that signal quality and provide user feedback. These micro-interactions make the editor feel polished and responsive.

---

## Animation Categories

### 1. Entry Animations
Used when elements appear on screen.

| Animation | Use Case | Duration | Easing |
|-----------|----------|----------|--------|
| `fadeIn` | General appearance | 200ms | ease-out |
| `fadeInUp` | List items, cards | 300ms | ease-out |
| `fadeInDown` | Dropdowns, tooltips | 200ms | ease-out |
| `scaleIn` | Modals, popovers | 300ms | bounce |
| `slideInRight` | Toast notifications | 300ms | bounce |
| `slideInLeft` | Side panels | 300ms | ease-out |

### 2. Feedback Animations
Used for user interaction feedback.

| Animation | Use Case | Duration | Easing |
|-----------|----------|----------|--------|
| `pulse` | Selection confirmation | 400ms | ease |
| `bounce` | Success state | 600ms | ease |
| `shake` | Error/validation failure | 500ms | ease |
| `pop` | Checkbox/toggle activation | 300ms | bounce |

### 3. Loading Animations
Used for loading states.

| Animation | Use Case | Duration | Easing |
|-----------|----------|----------|--------|
| `spin` | Loading spinner | 1000ms | linear |
| `shimmer` | Skeleton loader | 1500ms | linear |

---

## CSS File: animations.css

```css
/* ===== ANIMATIONS.CSS ===== */

/* === Timing Functions === */
:root {
    --ease-in: cubic-bezier(0.4, 0, 1, 1);
    --ease-out: cubic-bezier(0, 0, 0.2, 1);
    --ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
    --ease-bounce: cubic-bezier(0.34, 1.56, 0.64, 1);
}

/* === KEYFRAMES === */

/* Fade In */
@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

/* Fade In Up */
@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

/* Fade In Down */
@keyframes fadeInDown {
    from {
        opacity: 0;
        transform: translateY(-10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

/* Scale In */
@keyframes scaleIn {
    from {
        opacity: 0;
        transform: scale(0.95);
    }
    to {
        opacity: 1;
        transform: scale(1);
    }
}

/* Slide In Right */
@keyframes slideInRight {
    from {
        opacity: 0;
        transform: translateX(20px);
    }
    to {
        opacity: 1;
        transform: translateX(0);
    }
}

/* Slide In Left */
@keyframes slideInLeft {
    from {
        opacity: 0;
        transform: translateX(-20px);
    }
    to {
        opacity: 1;
        transform: translateX(0);
    }
}

/* Bounce */
@keyframes bounce {
    0%, 100% { transform: translateY(0); }
    50% { transform: translateY(-8px); }
}

/* Pulse */
@keyframes pulse {
    0%, 100% { transform: scale(1); }
    50% { transform: scale(1.05); }
}

/* Shake */
@keyframes shake {
    0%, 100% { transform: translateX(0); }
    10%, 30%, 50%, 70%, 90% { transform: translateX(-4px); }
    20%, 40%, 60%, 80% { transform: translateX(4px); }
}

/* Pop */
@keyframes pop {
    0% { transform: scale(1); }
    50% { transform: scale(1.1); }
    100% { transform: scale(1); }
}

/* Spin */
@keyframes spin {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
}

/* Shimmer (Skeleton Loading) */
@keyframes shimmer {
    0% { background-position: -200% 0; }
    100% { background-position: 200% 0; }
}

/* === ANIMATION UTILITY CLASSES === */

.animate-fadeIn { animation: fadeIn 0.2s var(--ease-out); }
.animate-fadeInUp { animation: fadeInUp 0.3s var(--ease-out); }
.animate-fadeInDown { animation: fadeInDown 0.2s var(--ease-out); }
.animate-scaleIn { animation: scaleIn 0.3s var(--ease-bounce); }
.animate-slideInRight { animation: slideInRight 0.3s var(--ease-bounce); }
.animate-slideInLeft { animation: slideInLeft 0.3s var(--ease-out); }
.animate-bounce { animation: bounce 0.6s ease; }
.animate-pulse { animation: pulse 0.4s ease; }
.animate-shake { animation: shake 0.5s ease; }
.animate-pop { animation: pop 0.3s var(--ease-bounce); }
.animate-spin { animation: spin 1s linear infinite; }

/* === TRANSITION UTILITIES === */

.transition-none { transition: none; }
.transition-all { transition: all 0.2s var(--ease-out); }
.transition-colors { transition: color 0.15s, background-color 0.15s, border-color 0.15s; }
.transition-opacity { transition: opacity 0.2s var(--ease-out); }
.transition-transform { transition: transform 0.2s var(--ease-out); }
.transition-shadow { transition: box-shadow 0.2s var(--ease-out); }

/* Duration modifiers */
.duration-fast { transition-duration: 100ms; }
.duration-normal { transition-duration: 200ms; }
.duration-slow { transition-duration: 300ms; }
.duration-slower { transition-duration: 500ms; }

/* === COMPONENT ANIMATIONS === */

/* Button Hover/Active */
.btn {
    transition: all 0.15s var(--ease-out);
}
.btn:hover:not(:disabled) {
    transform: translateY(-1px);
}
.btn:active:not(:disabled) {
    transform: translateY(0) scale(0.98);
}

/* Button Ripple Effect */
.btn-ripple {
    position: relative;
    overflow: hidden;
}
.btn-ripple::after {
    content: "";
    position: absolute;
    width: 100%;
    height: 100%;
    top: 0;
    left: 0;
    pointer-events: none;
    background-image: radial-gradient(circle, rgba(255,255,255,0.3) 10%, transparent 10.01%);
    background-repeat: no-repeat;
    background-position: 50%;
    transform: scale(10, 10);
    opacity: 0;
    transition: transform 0.5s, opacity 0.5s;
}
.btn-ripple:active::after {
    transform: scale(0, 0);
    opacity: 1;
    transition: 0s;
}

/* Card Hover */
.card-hover {
    transition: transform 0.3s var(--ease-bounce), box-shadow 0.3s ease, border-color 0.3s ease;
}
.card-hover:hover {
    transform: translateY(-4px);
    box-shadow: var(--shadow-lg);
}

/* Input Focus */
.input-focus {
    transition: border-color 0.2s, box-shadow 0.2s;
}
.input-focus:focus {
    border-color: var(--primary);
    box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
}

/* Toggle Switch */
.toggle-animated {
    transition: background-color 0.3s ease;
}
.toggle-animated::after {
    transition: transform 0.3s var(--ease-bounce);
}

/* Panel Expand/Collapse */
.panel-animated .panel-body {
    transition: max-height 0.3s ease, padding 0.3s ease, opacity 0.2s ease;
}
.panel-animated .panel-chevron {
    transition: transform 0.3s var(--ease-bounce);
}

/* Field Selection */
.field-selectable {
    transition: border-color 0.2s, background-color 0.2s, transform 0.2s;
}
.field-selectable.selected {
    animation: pop 0.3s var(--ease-bounce);
}

/* Modal Entrance */
.modal-animated {
    transition: transform 0.3s var(--ease-bounce), opacity 0.2s ease;
}

/* Toast Slide */
.toast-animated {
    animation: slideInRight 0.3s var(--ease-bounce);
}
.toast-animated.hiding {
    animation: slideInRight 0.3s var(--ease-bounce) reverse forwards;
}

/* Skeleton Shimmer */
.skeleton {
    background: linear-gradient(
        90deg,
        var(--bg-tertiary) 25%,
        var(--bg-secondary) 50%,
        var(--bg-tertiary) 75%
    );
    background-size: 200% 100%;
    animation: shimmer 1.5s infinite linear;
}

/* Checkbox Check Animation */
.checkbox-animated.checked {
    animation: pop 0.3s var(--ease-bounce);
}

/* Icon Hover Rotation */
.icon-rotate-hover {
    transition: transform 0.3s var(--ease-bounce);
}
.icon-rotate-hover:hover {
    transform: rotate(90deg);
}

/* === REDUCED MOTION === */
@media (prefers-reduced-motion: reduce) {
    *,
    *::before,
    *::after {
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.01ms !important;
        scroll-behavior: auto !important;
    }
}
```

---

## Implementation Guide

### Button Animations

```razor
<!-- Primary button with hover lift and ripple -->
<button class="btn btn-primary btn-ripple">
    <i class="bi bi-plus"></i> Add Field
</button>
```

### Card Hover

```razor
<div class="card card-hover">
    <div class="card-icon"><i class="bi bi-file-text"></i></div>
    <div class="card-title">Form Module</div>
</div>
```

### Modal Animation

```razor
<div class="modal-backdrop @(IsOpen ? "active" : "")">
    <div class="modal modal-animated">
        <!-- Modal content -->
    </div>
</div>
```

### Toast Notification

```razor
<div class="toast toast-animated @(IsHiding ? "hiding" : "")">
    <i class="bi bi-check-circle-fill"></i>
    <span>Changes saved successfully</span>
</div>
```

### Field Selection

```razor
<div class="field-item field-selectable @(IsSelected ? "selected" : "")"
     @onclick="OnSelect">
    <!-- Field content -->
</div>
```

### Skeleton Loading

```razor
<div class="skeleton-card">
    <div class="skeleton skeleton-avatar"></div>
    <div class="skeleton skeleton-title"></div>
    <div class="skeleton skeleton-text"></div>
</div>
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to create the animations.css file for my Blazor application to add professional micro-interactions.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/wwwroot/css/animations.css
- Design System: Uses CSS custom properties from variables.css

## Requirements

### 1. Create Animation Keyframes
- fadeIn, fadeInUp, fadeInDown
- scaleIn (for modals)
- slideInRight, slideInLeft
- bounce, pulse, shake, pop
- spin (loading spinner)
- shimmer (skeleton loader)

### 2. Create Utility Classes
Animation classes:
- .animate-fadeIn, .animate-fadeInUp, etc.
- .animate-spin (infinite)

Transition classes:
- .transition-all, .transition-colors, .transition-transform
- Duration modifiers: .duration-fast (100ms), .duration-normal (200ms), .duration-slow (300ms)

### 3. Component-Specific Animations

**Buttons**:
- Hover: translateY(-1px) lift effect
- Active: scale(0.98) press effect
- Optional ripple effect on click

**Cards**:
- Hover: translateY(-4px) + shadow-lg

**Inputs**:
- Focus: border-color transition + focus ring (box-shadow)

**Toggles**:
- Smooth transition for switch movement

**Panels**:
- Expand/collapse with max-height transition
- Chevron rotation

**Modals**:
- Scale + fade entrance
- Backdrop fade

**Toasts**:
- Slide in from right
- Slide out on dismiss

**Skeleton Loading**:
- Shimmer gradient animation

**Field Selection**:
- Pop animation on select

### 4. Timing Variables
Use these timing functions:
```css
--ease-in: cubic-bezier(0.4, 0, 1, 1);
--ease-out: cubic-bezier(0, 0, 0.2, 1);
--ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
--ease-bounce: cubic-bezier(0.34, 1.56, 0.64, 1);
```

### 5. Accessibility
Include reduced motion media query:
```css
@media (prefers-reduced-motion: reduce) {
    /* Disable all animations */
}
```

## Usage Patterns
Provide examples of how to apply each animation in Blazor components.

Please create the complete animations.css file with all keyframes, utility classes, and component animations.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `MicroInteractions-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing each animation
- Expected animation behaviors and timing
- Visual indicators to look for
- Browser compatibility testing steps
- Performance testing (60fps verification)
- prefers-reduced-motion testing steps

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- CSS file imports to add
- Class names to apply to existing components
- JavaScript integrations if needed
- Configuration changes required
- Integration with existing button/card/modal components

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] All keyframe animations defined
- [ ] Utility classes working
- [ ] Button hover/active animations smooth
- [ ] Card hover effects working
- [ ] Modal entrance animations working
- [ ] Toast slide animations working
- [ ] Skeleton shimmer animating
- [ ] Reduced motion respected
- [ ] Dark mode animations working
- [ ] No performance issues

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] All keyframe animations play correctly
- [ ] Button hover/active states work
- [ ] Card hover lift effect works
- [ ] Input focus ring appears
- [ ] Toggle switch animates smoothly
- [ ] Panel expand/collapse is smooth
- [ ] Modal entrance animation works
- [ ] Toast slides in/out correctly
- [ ] Skeleton shimmer animates
- [ ] Field selection has pop effect
- [ ] Reduced motion disables animations
- [ ] Animations work in dark mode
- [ ] No janky/stuttering animations
- [ ] Performance is smooth (60fps)

---

## Integration with Existing Components

After creating animations.css, update these components:

1. **Buttons**: Add `btn-ripple` class to primary action buttons
2. **Cards**: Add `card-hover` class to interactive cards
3. **Inputs**: Add `input-focus` class to form inputs
4. **Modals**: Add `modal-animated` class to modal containers
5. **Toasts**: Add `toast-animated` class to toast notifications
6. **Fields**: Add `field-selectable` class to canvas field items

---

## Notes

- Keep animations subtle - they should enhance, not distract
- Use `ease-bounce` sparingly for emphasis
- All animations should complete in under 500ms
- Test on low-end devices for performance
- Consider adding `will-change` for frequently animated properties
