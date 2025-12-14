# Theme Editor CSS Variables Reference

This document describes all CSS custom properties (variables) used by the Dynamic Forms theming system. These variables can be customized through the Theme Editor to create consistent, branded form experiences.

## Variable Naming Convention

All theme variables use the `--df-` prefix (Dynamic Forms) to avoid conflicts with other CSS frameworks.

---

## Color Variables

### Primary Colors

| Variable | Description | Default (Light) | Default (Dark) |
|----------|-------------|-----------------|----------------|
| `--df-primary` | Primary brand color | `#3B82F6` | `#60A5FA` |
| `--df-primary-light` | Lighter shade of primary | `#EFF6FF` | `#1E3A5F` |
| `--df-primary-dark` | Darker shade of primary | `#2563EB` | `#3B82F6` |
| `--df-secondary` | Secondary accent color | `#6B7280` | `#9CA3AF` |

### Background Colors

| Variable | Description | Default (Light) | Default (Dark) |
|----------|-------------|-----------------|----------------|
| `--df-background` | Main page background | `#FFFFFF` | `#0F172A` |
| `--df-surface` | Card/panel background | `#F9FAFB` | `#1E293B` |

### Text Colors

| Variable | Description | Default (Light) | Default (Dark) |
|----------|-------------|-----------------|----------------|
| `--df-text-primary` | Main text color | `#111827` | `#F9FAFB` |
| `--df-text-secondary` | Secondary/muted text | `#6B7280` | `#9CA3AF` |
| `--df-text-tertiary` | Tertiary/hint text | `#9CA3AF` | `#6B7280` |
| `--df-text-placeholder` | Placeholder text | `#9CA3AF` | `#4B5563` |

### Border Colors

| Variable | Description | Default (Light) | Default (Dark) |
|----------|-------------|-----------------|----------------|
| `--df-border` | Default border color | `#E5E7EB` | `#334155` |
| `--df-border-focus` | Border color on focus | `#3B82F6` | `#60A5FA` |

### Status Colors

| Variable | Description | Default (Light) | Default (Dark) |
|----------|-------------|-----------------|----------------|
| `--df-success` | Success/valid state | `#10B981` | `#34D399` |
| `--df-warning` | Warning state | `#F59E0B` | `#FBBF24` |
| `--df-error` | Error/invalid state | `#EF4444` | `#F87171` |
| `--df-info` | Informational state | `#3B82F6` | `#60A5FA` |

---

## Typography Variables

### Font Families

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-font-family` | Primary font stack | `'Inter', -apple-system, BlinkMacSystemFont, sans-serif` |
| `--df-font-mono` | Monospace font | `'JetBrains Mono', 'Fira Code', monospace` |

### Font Sizes

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-font-size-xs` | Extra small text | `11px` |
| `--df-font-size-sm` | Small text | `12px` |
| `--df-font-size-base` | Base/body text | `14px` |
| `--df-font-size-lg` | Large text | `16px` |
| `--df-font-size-xl` | Extra large text | `18px` |
| `--df-font-size-2xl` | Section headers | `20px` |
| `--df-font-size-3xl` | Page headers | `24px` |

### Font Weights

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-font-weight-normal` | Normal weight | `400` |
| `--df-font-weight-medium` | Medium weight | `500` |
| `--df-font-weight-semibold` | Semi-bold weight | `600` |
| `--df-font-weight-bold` | Bold weight | `700` |

### Line Heights

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-line-height-tight` | Compact line height | `1.25` |
| `--df-line-height-normal` | Normal line height | `1.5` |
| `--df-line-height-relaxed` | Relaxed line height | `1.75` |

---

## Spacing Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-spacing-xs` | Extra small spacing | `4px` |
| `--df-spacing-sm` | Small spacing | `8px` |
| `--df-spacing-md` | Medium spacing | `12px` |
| `--df-spacing-lg` | Large spacing | `16px` |
| `--df-spacing-xl` | Extra large spacing | `24px` |
| `--df-spacing-2xl` | 2x extra large | `32px` |

---

## Border Variables

### Border Radius

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-radius-sm` | Small radius | `4px` |
| `--df-radius-md` | Medium radius | `6px` |
| `--df-radius-lg` | Large radius | `8px` |
| `--df-radius-xl` | Extra large radius | `12px` |
| `--df-radius-full` | Fully rounded | `9999px` |

### Border Width

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-border-width` | Default border width | `1px` |
| `--df-border-width-thick` | Thick border width | `2px` |

---

## Shadow Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-shadow-sm` | Small shadow | `0 1px 2px rgba(0,0,0,0.05)` |
| `--df-shadow-md` | Medium shadow | `0 4px 6px rgba(0,0,0,0.1)` |
| `--df-shadow-lg` | Large shadow | `0 10px 15px rgba(0,0,0,0.1)` |
| `--df-shadow-xl` | Extra large shadow | `0 20px 25px rgba(0,0,0,0.15)` |
| `--df-shadow-focus` | Focus ring shadow | `0 0 0 3px var(--df-primary-light)` |

---

## Component-Specific Variables

### Input Fields

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-input-height` | Input field height | `40px` |
| `--df-input-padding-x` | Horizontal padding | `12px` |
| `--df-input-padding-y` | Vertical padding | `10px` |

### Buttons

| Variable | Description | Default |
|----------|-------------|---------|
| `--df-btn-height` | Button height | `40px` |
| `--df-btn-padding-x` | Horizontal padding | `16px` |
| `--df-btn-font-weight` | Button font weight | `500` |

---

## Usage Example

```css
/* Using theme variables in custom CSS */
.my-custom-element {
    background: var(--df-surface);
    color: var(--df-text-primary);
    border: var(--df-border-width) solid var(--df-border);
    border-radius: var(--df-radius-md);
    padding: var(--df-spacing-md);
    font-family: var(--df-font-family);
}

.my-custom-element:focus {
    border-color: var(--df-border-focus);
    box-shadow: var(--df-shadow-focus);
}
```

---

## WCAG Accessibility Guidelines

The Theme Editor includes built-in contrast checking to ensure accessibility compliance:

### Contrast Requirements

| Level | Normal Text | Large Text |
|-------|-------------|------------|
| **AA (Minimum)** | 4.5:1 | 3:1 |
| **AAA (Enhanced)** | 7:1 | 4.5:1 |

**Large text** is defined as:
- 18pt (24px) or larger
- 14pt (18.67px) bold or larger

### Recommendations

1. Always verify contrast ratios using the built-in Accessibility checker
2. Use `--df-text-primary` for main content text
3. Reserve `--df-text-secondary` for supporting content
4. Avoid using `--df-text-tertiary` for essential information
