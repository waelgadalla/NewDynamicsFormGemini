# A.1 CSS Architecture Overhaul - Implementation Plan

> **Task**: CSS Architecture Overhaul
> **Location**: `Src/VisualEditorOpus/wwwroot/css/`
> **Priority**: Critical
> **Estimated Effort**: 4-6 hours
> **Delegation**: 70% AI, 30% Manual Review

---

## Overview

Transform the current CSS structure from functional to professional-grade by creating a proper CSS architecture with design tokens, consistent spacing, shadow hierarchy, and animation library.

---

## Current State Analysis

### Existing Files
```
wwwroot/css/
├── variables.css    # Design tokens (partially complete)
├── app.css          # Main styles (large, ~400 lines)
└── components.css   # Component-specific styles (~950 lines)
```

### Issues to Address
1. No dedicated reset/normalize file
2. Typography styles mixed with component styles
3. Layout utilities scattered across files
4. No dedicated animation/transition library
5. Dark theme partially implemented
6. Some inline styles still present in components

---

## Target CSS Architecture

```
wwwroot/css/
├── variables.css      # Design tokens (colors, spacing, shadows) [ENHANCE]
├── reset.css          # CSS reset/normalize [CREATE]
├── typography.css     # Font styles, headings, text utilities [CREATE]
├── components.css     # Shared component styles [REFACTOR]
├── layouts.css        # Layout utilities (flex, grid, spacing) [CREATE]
├── animations.css     # Transitions, micro-interactions [CREATE]
└── themes/
    ├── light.css      # Light theme (default) [CREATE]
    └── dark.css       # Dark theme overrides [CREATE]
```

---

## Files to Create/Modify

### 1. reset.css (New)

```css
/* CSS Reset - Modern Reset */
*, *::before, *::after {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
}

html {
    line-height: 1.5;
    -webkit-text-size-adjust: 100%;
    tab-size: 4;
}

body {
    min-height: 100vh;
    text-rendering: optimizeSpeed;
}

img, picture, video, canvas, svg {
    display: block;
    max-width: 100%;
}

input, button, textarea, select {
    font: inherit;
}

button {
    cursor: pointer;
    background: transparent;
    border: none;
}

a {
    color: inherit;
    text-decoration: none;
}

ul, ol {
    list-style: none;
}

/* Remove animations for people who've turned them off */
@media (prefers-reduced-motion: reduce) {
    *, *::before, *::after {
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.01ms !important;
        scroll-behavior: auto !important;
    }
}
```

### 2. variables.css (Enhanced)

```css
/* ===== DESIGN TOKENS ===== */
:root {
    /* === Colors === */
    --primary: #6366F1;
    --primary-dark: #4F46E5;
    --primary-darker: #4338CA;
    --primary-light: #EEF2FF;
    --primary-muted: #A5B4FC;

    --success: #10B981;
    --success-dark: #059669;
    --success-light: #D1FAE5;

    --danger: #EF4444;
    --danger-dark: #DC2626;
    --danger-light: #FEE2E2;

    --warning: #F59E0B;
    --warning-dark: #D97706;
    --warning-light: #FEF3C7;

    --info: #3B82F6;
    --info-dark: #2563EB;
    --info-light: #DBEAFE;

    /* === Spacing Scale === */
    --space-0: 0;
    --space-1: 4px;
    --space-2: 8px;
    --space-3: 12px;
    --space-4: 16px;
    --space-5: 20px;
    --space-6: 24px;
    --space-8: 32px;
    --space-10: 40px;
    --space-12: 48px;
    --space-16: 64px;

    /* === Layout Sizes === */
    --sidebar-width: 320px;
    --header-height: 56px;
    --toolbar-height: 48px;
    --global-nav-width: 64px;

    /* === Light Theme === */
    --bg-primary: #FFFFFF;
    --bg-secondary: #F8FAFC;
    --bg-tertiary: #F1F5F9;
    --bg-canvas: #F0F4F8;
    --bg-overlay: rgba(0, 0, 0, 0.5);

    --text-primary: #0F172A;
    --text-secondary: #64748B;
    --text-muted: #94A3B8;
    --text-inverse: #FFFFFF;

    --border-color: #E2E8F0;
    --border-strong: #CBD5E1;
    --border-focus: var(--primary);

    /* === Shadows === */
    --shadow-xs: 0 1px 2px rgba(0,0,0,0.04);
    --shadow-sm: 0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);
    --shadow-md: 0 4px 6px rgba(0,0,0,0.06), 0 2px 4px rgba(0,0,0,0.04);
    --shadow-lg: 0 10px 15px rgba(0,0,0,0.08), 0 4px 6px rgba(0,0,0,0.04);
    --shadow-xl: 0 20px 25px rgba(0,0,0,0.10), 0 10px 10px rgba(0,0,0,0.04);
    --shadow-2xl: 0 25px 50px rgba(0,0,0,0.20);
    --shadow-inner: inset 0 2px 4px rgba(0,0,0,0.06);
    --shadow-focus: 0 0 0 3px rgba(99, 102, 241, 0.25);

    /* === Grid === */
    --grid-color: #E1E7EF;

    /* === Typography === */
    --font-sans: 'DM Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    --font-mono: 'JetBrains Mono', 'Fira Code', 'Consolas', monospace;

    --text-xs: 11px;
    --text-sm: 12px;
    --text-base: 14px;
    --text-md: 15px;
    --text-lg: 16px;
    --text-xl: 18px;
    --text-2xl: 20px;
    --text-3xl: 24px;

    --font-normal: 400;
    --font-medium: 500;
    --font-semibold: 600;
    --font-bold: 700;

    --leading-tight: 1.25;
    --leading-normal: 1.5;
    --leading-relaxed: 1.75;

    /* === Border Radius === */
    --radius-none: 0;
    --radius-sm: 4px;
    --radius-md: 6px;
    --radius-lg: 8px;
    --radius-xl: 10px;
    --radius-2xl: 12px;
    --radius-full: 9999px;

    /* === Transitions === */
    --duration-fast: 100ms;
    --duration-normal: 200ms;
    --duration-slow: 300ms;
    --duration-slower: 500ms;

    --ease-in: cubic-bezier(0.4, 0, 1, 1);
    --ease-out: cubic-bezier(0, 0, 0.2, 1);
    --ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
    --ease-bounce: cubic-bezier(0.34, 1.56, 0.64, 1);

    /* === Z-Index Scale === */
    --z-dropdown: 100;
    --z-sticky: 200;
    --z-fixed: 300;
    --z-modal-backdrop: 400;
    --z-modal: 500;
    --z-popover: 600;
    --z-tooltip: 700;
    --z-toast: 800;
}
```

### 3. typography.css (New)

```css
/* ===== TYPOGRAPHY ===== */

/* Base */
body {
    font-family: var(--font-sans);
    font-size: var(--text-base);
    line-height: var(--leading-normal);
    color: var(--text-primary);
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
}

/* Headings */
h1, h2, h3, h4, h5, h6 {
    font-weight: var(--font-bold);
    line-height: var(--leading-tight);
    color: var(--text-primary);
}

h1, .text-3xl { font-size: var(--text-3xl); }
h2, .text-2xl { font-size: var(--text-2xl); }
h3, .text-xl { font-size: var(--text-xl); }
h4, .text-lg { font-size: var(--text-lg); }
h5, .text-md { font-size: var(--text-md); }
h6, .text-base { font-size: var(--text-base); }

/* Text Sizes */
.text-xs { font-size: var(--text-xs); }
.text-sm { font-size: var(--text-sm); }

/* Font Weights */
.font-normal { font-weight: var(--font-normal); }
.font-medium { font-weight: var(--font-medium); }
.font-semibold { font-weight: var(--font-semibold); }
.font-bold { font-weight: var(--font-bold); }

/* Text Colors */
.text-primary { color: var(--text-primary); }
.text-secondary { color: var(--text-secondary); }
.text-muted { color: var(--text-muted); }
.text-inverse { color: var(--text-inverse); }
.text-success { color: var(--success); }
.text-danger { color: var(--danger); }
.text-warning { color: var(--warning); }
.text-info { color: var(--info); }

/* Monospace */
.font-mono, code, pre, .text-mono {
    font-family: var(--font-mono);
}

/* Text Transform */
.uppercase { text-transform: uppercase; }
.lowercase { text-transform: lowercase; }
.capitalize { text-transform: capitalize; }

/* Letter Spacing */
.tracking-tight { letter-spacing: -0.025em; }
.tracking-normal { letter-spacing: 0; }
.tracking-wide { letter-spacing: 0.025em; }
.tracking-wider { letter-spacing: 0.05em; }

/* Line Height */
.leading-tight { line-height: var(--leading-tight); }
.leading-normal { line-height: var(--leading-normal); }
.leading-relaxed { line-height: var(--leading-relaxed); }

/* Text Alignment */
.text-left { text-align: left; }
.text-center { text-align: center; }
.text-right { text-align: right; }

/* Truncation */
.truncate {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.line-clamp-2 {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.line-clamp-3 {
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

/* Labels (Form) */
.label {
    display: block;
    font-size: var(--text-xs);
    font-weight: var(--font-semibold);
    color: var(--text-secondary);
    text-transform: uppercase;
    letter-spacing: 0.3px;
    margin-bottom: var(--space-1);
}
```

### 4. layouts.css (New)

```css
/* ===== LAYOUT UTILITIES ===== */

/* Display */
.hidden { display: none !important; }
.block { display: block; }
.inline-block { display: inline-block; }
.flex { display: flex; }
.inline-flex { display: inline-flex; }
.grid { display: grid; }

/* Flexbox */
.flex-row { flex-direction: row; }
.flex-col { flex-direction: column; }
.flex-wrap { flex-wrap: wrap; }
.flex-1 { flex: 1; }
.flex-shrink-0 { flex-shrink: 0; }
.flex-grow-0 { flex-grow: 0; }

/* Alignment */
.items-start { align-items: flex-start; }
.items-center { align-items: center; }
.items-end { align-items: flex-end; }
.items-stretch { align-items: stretch; }

.justify-start { justify-content: flex-start; }
.justify-center { justify-content: center; }
.justify-end { justify-content: flex-end; }
.justify-between { justify-content: space-between; }
.justify-around { justify-content: space-around; }

.self-start { align-self: flex-start; }
.self-center { align-self: center; }
.self-end { align-self: flex-end; }

/* Gap */
.gap-0 { gap: var(--space-0); }
.gap-1 { gap: var(--space-1); }
.gap-2 { gap: var(--space-2); }
.gap-3 { gap: var(--space-3); }
.gap-4 { gap: var(--space-4); }
.gap-5 { gap: var(--space-5); }
.gap-6 { gap: var(--space-6); }
.gap-8 { gap: var(--space-8); }

/* Margin */
.m-0 { margin: var(--space-0); }
.m-1 { margin: var(--space-1); }
.m-2 { margin: var(--space-2); }
.m-3 { margin: var(--space-3); }
.m-4 { margin: var(--space-4); }

.mt-0 { margin-top: var(--space-0); }
.mt-1 { margin-top: var(--space-1); }
.mt-2 { margin-top: var(--space-2); }
.mt-3 { margin-top: var(--space-3); }
.mt-4 { margin-top: var(--space-4); }
.mt-6 { margin-top: var(--space-6); }
.mt-8 { margin-top: var(--space-8); }

.mb-0 { margin-bottom: var(--space-0); }
.mb-1 { margin-bottom: var(--space-1); }
.mb-2 { margin-bottom: var(--space-2); }
.mb-3 { margin-bottom: var(--space-3); }
.mb-4 { margin-bottom: var(--space-4); }
.mb-6 { margin-bottom: var(--space-6); }
.mb-8 { margin-bottom: var(--space-8); }

.ml-auto { margin-left: auto; }
.mr-auto { margin-right: auto; }
.mx-auto { margin-left: auto; margin-right: auto; }

/* Padding */
.p-0 { padding: var(--space-0); }
.p-1 { padding: var(--space-1); }
.p-2 { padding: var(--space-2); }
.p-3 { padding: var(--space-3); }
.p-4 { padding: var(--space-4); }
.p-5 { padding: var(--space-5); }
.p-6 { padding: var(--space-6); }

.px-2 { padding-left: var(--space-2); padding-right: var(--space-2); }
.px-3 { padding-left: var(--space-3); padding-right: var(--space-3); }
.px-4 { padding-left: var(--space-4); padding-right: var(--space-4); }
.px-6 { padding-left: var(--space-6); padding-right: var(--space-6); }

.py-2 { padding-top: var(--space-2); padding-bottom: var(--space-2); }
.py-3 { padding-top: var(--space-3); padding-bottom: var(--space-3); }
.py-4 { padding-top: var(--space-4); padding-bottom: var(--space-4); }

/* Width/Height */
.w-full { width: 100%; }
.h-full { height: 100%; }
.min-h-screen { min-height: 100vh; }

/* Position */
.relative { position: relative; }
.absolute { position: absolute; }
.fixed { position: fixed; }
.sticky { position: sticky; }

.inset-0 { top: 0; right: 0; bottom: 0; left: 0; }
.top-0 { top: 0; }
.right-0 { right: 0; }
.bottom-0 { bottom: 0; }
.left-0 { left: 0; }

/* Overflow */
.overflow-hidden { overflow: hidden; }
.overflow-auto { overflow: auto; }
.overflow-y-auto { overflow-y: auto; }
.overflow-x-auto { overflow-x: auto; }

/* Borders */
.border { border: 1px solid var(--border-color); }
.border-t { border-top: 1px solid var(--border-color); }
.border-b { border-bottom: 1px solid var(--border-color); }
.border-l { border-left: 1px solid var(--border-color); }
.border-r { border-right: 1px solid var(--border-color); }
.border-0 { border: none; }

.rounded-none { border-radius: var(--radius-none); }
.rounded-sm { border-radius: var(--radius-sm); }
.rounded { border-radius: var(--radius-md); }
.rounded-lg { border-radius: var(--radius-lg); }
.rounded-xl { border-radius: var(--radius-xl); }
.rounded-2xl { border-radius: var(--radius-2xl); }
.rounded-full { border-radius: var(--radius-full); }

/* Shadows */
.shadow-none { box-shadow: none; }
.shadow-xs { box-shadow: var(--shadow-xs); }
.shadow-sm { box-shadow: var(--shadow-sm); }
.shadow { box-shadow: var(--shadow-md); }
.shadow-lg { box-shadow: var(--shadow-lg); }
.shadow-xl { box-shadow: var(--shadow-xl); }

/* Background */
.bg-primary { background-color: var(--bg-primary); }
.bg-secondary { background-color: var(--bg-secondary); }
.bg-tertiary { background-color: var(--bg-tertiary); }
.bg-transparent { background-color: transparent; }

/* Cursor */
.cursor-pointer { cursor: pointer; }
.cursor-default { cursor: default; }
.cursor-not-allowed { cursor: not-allowed; }
.cursor-grab { cursor: grab; }
.cursor-grabbing { cursor: grabbing; }

/* Pointer Events */
.pointer-events-none { pointer-events: none; }
.pointer-events-auto { pointer-events: auto; }

/* User Select */
.select-none { user-select: none; }
.select-text { user-select: text; }
.select-all { user-select: all; }

/* Z-Index */
.z-0 { z-index: 0; }
.z-10 { z-index: 10; }
.z-20 { z-index: 20; }
.z-50 { z-index: 50; }
.z-dropdown { z-index: var(--z-dropdown); }
.z-modal { z-index: var(--z-modal); }
.z-tooltip { z-index: var(--z-tooltip); }
.z-toast { z-index: var(--z-toast); }

/* Visibility */
.visible { visibility: visible; }
.invisible { visibility: hidden; }

/* Opacity */
.opacity-0 { opacity: 0; }
.opacity-50 { opacity: 0.5; }
.opacity-75 { opacity: 0.75; }
.opacity-100 { opacity: 1; }
```

### 5. animations.css (New)

See separate file: `A2-MicroInteractions/Animations-Mockup.html`

### 6. themes/dark.css (New)

```css
/* ===== DARK THEME ===== */
[data-theme="dark"] {
    /* Colors */
    --primary: #818CF8;
    --primary-dark: #6366F1;
    --primary-darker: #4F46E5;
    --primary-light: rgba(99, 102, 241, 0.15);

    /* Backgrounds */
    --bg-primary: #1E293B;
    --bg-secondary: #0F172A;
    --bg-tertiary: #334155;
    --bg-canvas: #1E293B;
    --bg-overlay: rgba(0, 0, 0, 0.7);

    /* Text */
    --text-primary: #F1F5F9;
    --text-secondary: #94A3B8;
    --text-muted: #64748B;
    --text-inverse: #0F172A;

    /* Borders */
    --border-color: #334155;
    --border-strong: #475569;

    /* Grid */
    --grid-color: #334155;

    /* Shadows (darker in dark mode) */
    --shadow-sm: 0 1px 3px rgba(0,0,0,0.2), 0 1px 2px rgba(0,0,0,0.1);
    --shadow-md: 0 4px 6px rgba(0,0,0,0.25), 0 2px 4px rgba(0,0,0,0.15);
    --shadow-lg: 0 10px 15px rgba(0,0,0,0.3), 0 4px 6px rgba(0,0,0,0.2);
    --shadow-xl: 0 20px 25px rgba(0,0,0,0.35), 0 10px 10px rgba(0,0,0,0.2);

    /* Semantic Light Colors */
    --success-light: rgba(16, 185, 129, 0.15);
    --danger-light: rgba(239, 68, 68, 0.15);
    --warning-light: rgba(245, 158, 11, 0.15);
    --info-light: rgba(59, 130, 246, 0.15);
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to refactor the CSS architecture for my Blazor application to create a professional, maintainable structure.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/wwwroot/css/

## Current Files
- variables.css (partially complete)
- app.css (~400 lines)
- components.css (~950 lines)

## Tasks

### 1. Create reset.css
Modern CSS reset with:
- Box-sizing border-box
- Remove default margins/padding
- Normalize form elements
- Respect prefers-reduced-motion

### 2. Enhance variables.css
Add:
- Complete spacing scale (0, 4, 8, 12, 16, 20, 24, 32, 40, 48, 64)
- Typography scale (11, 12, 14, 15, 16, 18, 20, 24)
- Shadow hierarchy (xs, sm, md, lg, xl, 2xl, inner, focus)
- Z-index scale for layers
- Duration and easing tokens
- Complete dark theme variables

### 3. Create typography.css
Extract all typography from app.css:
- Heading styles (h1-h6)
- Text size utilities
- Font weight utilities
- Text color utilities
- Text alignment
- Truncation utilities
- Label styles

### 4. Create layouts.css
Extract all layout utilities:
- Display utilities
- Flexbox utilities
- Gap utilities
- Margin utilities
- Padding utilities
- Position utilities
- Border utilities
- Shadow utilities
- Background utilities
- Z-index utilities

### 5. Create themes/dark.css
Move dark theme overrides to dedicated file:
- All [data-theme="dark"] rules
- Semantic color overrides
- Shadow adjustments

### 6. Update app.css imports
```css
@import url('reset.css');
@import url('variables.css');
@import url('typography.css');
@import url('layouts.css');
@import url('animations.css');
@import url('themes/dark.css');
@import url('components.css');
```

## Spacing Scale Reference
```
--space-0: 0
--space-1: 4px
--space-2: 8px
--space-3: 12px
--space-4: 16px
--space-5: 20px
--space-6: 24px
--space-8: 32px
--space-10: 40px
--space-12: 48px
--space-16: 64px
```

## Shadow Hierarchy Reference
```
xs: Subtle elevation (buttons)
sm: Low elevation (cards, inputs)
md: Medium elevation (dropdowns)
lg: High elevation (modals, popovers)
xl: Highest elevation (toast notifications)
```

Please implement with complete, production-ready code. Maintain backward compatibility with existing class names.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `CSSArchitecture-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing each feature
- Expected results for each test
- Screenshots or visual indicators to look for
- Browser/device testing requirements
- Dark mode testing steps
- Accessibility testing steps

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Configuration changes needed
- Environment setup requirements
- File imports or references to add
- Dependencies to install
- Settings to configure
- Integration points with existing code

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] All files created successfully
- [ ] All CSS variables working
- [ ] Theme switching functional
- [ ] No console errors
- [ ] All components render correctly
- [ ] Responsive design working
- [ ] Performance acceptable

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] All existing components render correctly
- [ ] Dark theme switches properly
- [ ] Spacing is consistent (4px increments)
- [ ] Shadows create clear visual hierarchy
- [ ] Typography scale is readable
- [ ] All utility classes work
- [ ] No broken styles after refactor
- [ ] prefers-reduced-motion respected
- [ ] No CSS console errors

---

## Manual Review Points

1. **Color Accessibility**: Check contrast ratios meet WCAG AA (4.5:1 for text)
2. **Spacing Consistency**: Verify 4px grid alignment
3. **Shadow Usage**: Confirm shadows indicate proper elevation
4. **Dark Theme**: Test all components in dark mode
5. **Mobile**: Check styles on smaller screens

---

## Notes

- Keep backward compatibility with existing class names
- Extract, don't rewrite - maintain working styles
- Test incrementally after each file creation
- Use CSS custom properties for all values
- Consider CSS layers for better specificity control in future
