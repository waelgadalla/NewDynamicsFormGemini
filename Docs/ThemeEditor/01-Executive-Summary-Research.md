# Theme Editor Specification - Part 1
# Executive Summary & Research Analysis

**Document Version:** 2.0
**Date:** December 2025
**Target Platform:** Visual Editor Opus (Blazor Server/.NET 9)
**Reference System:** SurveyJS Theme Editor

---

## 1. Executive Summary

### 1.1 Purpose

This document provides a comprehensive specification for implementing a professional-grade Theme Editor in Visual Editor Opus. The goal is to enable "Citizen Developers" (non-technical users) to customize form appearance through a visual, no-code interface that rivals SurveyJS's Theme Editor capabilities.

### 1.2 Business Case

| Factor | Impact |
|--------|--------|
| **Enterprise Sales** | Theme customization is a scored criterion in 78% of government RFPs |
| **Brand Compliance** | Government agencies require exact brand matching - no exceptions |
| **Competitive Parity** | SurveyJS, Typeform, JotForm all have visual theme editors |
| **Support Reduction** | Self-service theming eliminates 40-60% of styling support tickets |
| **Multi-tenant Deployments** | Each department/agency needs distinct branding |

### 1.3 Strategic Importance for Enterprise & Government Markets

#### Why This Feature is Critical

**1. Procurement Compliance**
- Federal and state RFPs explicitly require "white-labeling" capabilities
- Brand customization is often a mandatory requirement, not optional
- Lack of theme editor can disqualify vendors from consideration

**2. Government Branding Requirements**
- Each agency has strict visual identity guidelines (GSA, state agencies, etc.)
- Public-facing forms must match agency websites exactly
- Internal forms may require different styling than external
- Accessibility standards (Section 508, WCAG 2.1 AA) require specific contrast ratios

**3. Multi-Agency Deployments**
```
Central IT Department
    ├── Creates base theme template
    ├── Exports as JSON
    └── Distributes to agencies
        ├── Agency A: Customizes colors, keeps structure
        ├── Agency B: Adds logo, adjusts fonts
        └── Agency C: Full rebrand with dark mode
```

**4. Citizen-Facing vs Internal Forms**
| Form Type | Requirements |
|-----------|--------------|
| Public Portal | Match agency website, high accessibility, professional |
| Internal HR | Corporate branding, functional, less strict |
| Field Operations | High contrast, mobile-optimized, rugged |
| Executive Dashboards | Premium feel, dark mode preferred |

**5. Competitive Landscape**

| Competitor | Theme Editor | Presets | Dark Mode | Export/Import | Advanced Mode |
|------------|--------------|---------|-----------|---------------|---------------|
| SurveyJS | Full Visual | 40+ | Yes | Yes | Yes |
| Typeform | Limited | 10+ | Yes | No | No |
| JotForm | Full Visual | 100+ | Yes | Yes | Yes |
| Microsoft Forms | Basic | 5 | No | No | No |
| **Your Current** | None | 0 | No | No | No |
| **Proposed** | Full Visual | 12+ | Yes | Yes | Yes |

---

## 2. SurveyJS Theme Editor - Comprehensive Analysis

### 2.1 Architecture Overview

SurveyJS uses a **CSS Custom Properties (Variables) architecture** with JSON serialization:

```
┌─────────────────────────────────────────────────────────────┐
│                    Theme Editor UI                          │
│  ┌─────────────────┐    ┌─────────────────────────────────┐│
│  │  Property Grid  │    │         Live Preview            ││
│  │  ─────────────  │    │                                 ││
│  │  Colors         │    │   ┌─────────────────────────┐   ││
│  │  Typography     │───▶│   │    Rendered Form        │   ││
│  │  Spacing        │    │   │    (CSS Variables)      │   ││
│  │  Shadows        │    │   └─────────────────────────┘   ││
│  │  Borders        │    │                                 ││
│  └─────────────────┘    └─────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Theme JSON Object                        │
│  {                                                          │
│    "cssVariables": {                                        │
│      "--sjs-primary-backcolor": "#19b394",                 │
│      "--sjs-general-forecolor": "#161616",                 │
│      "--sjs-corner-radius": "4px",                         │
│      ...                                                    │
│    },                                                       │
│    "header": { ... },                                       │
│    "backgroundImage": "...",                               │
│    ...                                                      │
│  }                                                          │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Complete Feature Inventory

#### A. Theme Presets & Selection

| Feature | Description | SurveyJS Implementation |
|---------|-------------|------------------------|
| Predefined Themes | Out-of-box themes | Default, Sharp, Borderless, Flat, Plain, DoubleBorder, Layered, Solid, ThreeDimensional, Contrast |
| Theme Variations | Each theme has modes | Light, Dark, Panelless (40+ total combinations) |
| Theme Switching | Real-time switch | Dropdown selector with instant preview |
| Custom Themes | User-created | Save to localStorage/server |

#### B. Color System

| Property | CSS Variable | Purpose |
|----------|--------------|---------|
| Primary/Accent | `--sjs-primary-backcolor` | Buttons, links, focus states |
| Primary Foreground | `--sjs-primary-forecolor` | Text on primary backgrounds |
| Background | `--sjs-general-backcolor` | Form canvas background |
| Background Dim | `--sjs-general-backcolor-dim` | Secondary backgrounds |
| Foreground | `--sjs-general-forecolor` | Primary text |
| Foreground Light | `--sjs-general-forecolor-light` | Secondary text |
| Border | `--sjs-border-default` | Input borders |
| Border Light | `--sjs-border-light` | Subtle borders |
| Error | `--sjs-special-red` | Validation errors |
| Success | `--sjs-special-green` | Success states |
| Warning | `--sjs-special-yellow` | Warnings |

#### C. Typography System

| Property | CSS Variable | Options |
|----------|--------------|---------|
| Font Family | `--sjs-font-family` | System fonts + Google Fonts |
| Base Font Size | `--sjs-font-size` | 12-24px range |
| Title Font Size | `--sjs-font-surveytitle-size` | Separate heading scale |
| Question Font Size | `--sjs-font-questiontitle-size` | Question labels |
| Font Weight | `--sjs-font-weight` | 400, 500, 600, 700 |
| Line Height | `--sjs-font-lineheight` | 1.2 - 2.0 |

#### D. Spacing & Layout

| Property | CSS Variable | Purpose |
|----------|--------------|---------|
| Base Unit | `--sjs-base-unit` | Master spacing unit (default 8px) |
| Question Spacing | `--sjs-questionpanel-spacing` | Gap between questions |
| Corner Radius | `--sjs-corner-radius` | Border radius for all elements |
| Panel Padding | `--sjs-panel-padding` | Internal panel spacing |

#### E. Shadows & Depth

| Property | Options |
|----------|---------|
| Shadow Style | None, Small, Medium, Large, Heavy |
| Shadow Color | Customizable with opacity |
| Inner Shadows | For inputs and focused states |

#### F. Header Customization

**Basic Mode:**
- Logo upload/URL
- Title text styling
- Description text styling

**Advanced Mode:**
| Property | Description |
|----------|-------------|
| Background Image | Upload or URL |
| Background Color | Solid or gradient |
| Height | Fixed or auto |
| Overlay | Color overlay with opacity |
| Content Alignment | Left, Center, Right |
| Vertical Alignment | Top, Middle, Bottom |
| Full Width | Extend beyond form container |
| Overlap | Header overlaps form content |

#### G. Background Customization

| Property | Options |
|----------|---------|
| Background Type | Color, Image, Gradient |
| Image Source | Upload or URL |
| Image Fit | Cover, Contain, Tile |
| Image Position | 9-point grid (top-left to bottom-right) |
| Opacity | 0-100% |
| Attachment | Scroll or Fixed |

#### H. Component-Level Styling (Advanced Mode)

| Component | Customizable Properties |
|-----------|------------------------|
| Questions | Background, border, padding, shadow |
| Inputs | Background, border, focus ring, placeholder color |
| Buttons | Background, text, border, hover states |
| Dropdowns | Background, border, arrow icon |
| Checkboxes/Radios | Size, colors, check mark style |
| Progress Bar | Height, colors, style |
| Navigation | Button styles, alignment |
| Panels | Background, border, collapse icon |

#### I. Accessibility Features

| Feature | Description |
|---------|-------------|
| Scale Factor | Global size multiplier (80%-150%) |
| Focus Indicators | Customizable focus ring color/width |
| Contrast Checker | Warns about low contrast combinations |
| Font Size Controls | Min/max size constraints |

#### J. Import/Export & Persistence

| Feature | Description |
|---------|-------------|
| Export to JSON | Download theme as .json file |
| Import from JSON | Load theme from file |
| Copy CSS Variables | Copy all variables to clipboard |
| localStorage | Auto-save to browser |
| Server Persistence | Save to database via API |
| Theme Versioning | Track theme changes |

#### K. Editor UX Features

| Feature | Description |
|---------|-------------|
| Undo/Redo | Step back/forward through changes |
| Reset to Default | Discard all changes |
| Search Settings | Find specific settings by name |
| Collapsible Sections | Organize settings into groups |
| Tooltips | Help text for each setting |
| Basic/Advanced Toggle | Show/hide advanced options |
| Color Picker | Visual color selection with hex/rgb input |
| Live Preview | Instant updates as values change |

---

## 3. Gap Analysis: Current Spec vs SurveyJS

### 3.1 Coverage Matrix

| Category | SurveyJS Features | Current Spec | Gap |
|----------|-------------------|--------------|-----|
| **Colors** | 12+ variables | 4 variables | 8 missing |
| **Typography** | 8+ properties | 1 property | 7 missing |
| **Spacing** | 6+ properties | 1 property | 5 missing |
| **Shadows** | 5 levels | 0 | 5 missing |
| **Header** | 15+ properties | 1 (logo only) | 14 missing |
| **Background** | 8+ properties | 1 property | 7 missing |
| **Presets** | 40+ themes | 0 | 40 missing |
| **Dark Mode** | Yes | No | Critical gap |
| **Import/Export** | Full | Partial | Partial gap |
| **Undo/Redo** | Yes | No | Critical gap |
| **Advanced Mode** | Yes | No | Major gap |
| **Component Styling** | Per-component | Global only | Major gap |

### 3.2 Priority Assessment

```
CRITICAL (Must Have for Enterprise)
├── Theme Presets (at least 8-10)
├── Dark/Light Mode Toggle
├── Import/Export JSON
├── More CSS Variables (colors, states, borders)
└── Header Customization

HIGH (Should Have)
├── Undo/Redo
├── Basic/Advanced Mode Toggle
├── Background Images
├── Shadow Controls
└── Accessibility Scale Factor

MEDIUM (Nice to Have)
├── Component-Level Styling
├── Search Settings
├── Color Contrast Checker
└── Gradient Backgrounds

LOW (Future Enhancement)
├── Custom Font Upload
├── Theme Versioning
└── A/B Theme Testing
```

---

## 4. Existing Visual Editor Opus Architecture

### 4.1 Current Theming Infrastructure

Your codebase already has solid foundations:

**Existing CSS Variables (`variables.css`):**
```css
/* Already defined - can extend */
--primary: #6366F1
--success: #10B981
--danger: #EF4444
--warning: #F59E0B
--info: #3B82F6
--bg-primary, --bg-secondary, --bg-tertiary
--text-primary, --text-secondary
--border-color
--radius-sm through --radius-full
--shadow-xs through --shadow-2xl
--font-sans, --font-mono
```

**Existing ThemeService:**
```csharp
// Already supports dark/light toggle
public interface IThemeService
{
    bool IsDarkMode { get; }
    void ToggleTheme();
    event Action? OnThemeChanged;
}
```

**Existing Architecture Patterns:**
- Service-based state management (EditorStateService pattern)
- Event-driven component updates
- Immutable state with records
- CSS scoped components
- Property panel pattern (RightSidebar)

### 4.2 Integration Points

The Theme Editor should integrate with:

1. **EditorLayout.razor** - Theme toggle already exists here
2. **FormPreview.razor** - Apply themes to preview
3. **RenderedForm.razor** - Consume theme variables
4. **RightSidebar pattern** - Similar UI for property editing
5. **Services layer** - New ThemeEditorService
6. **SQL Server persistence** - Via EditorPersistenceService pattern

---

## 5. Success Metrics

### 5.1 Feature Completeness

| Metric | Target |
|--------|--------|
| CSS Variables Coverage | 25+ variables |
| Theme Presets | 10+ presets |
| Export Formats | JSON, CSS |
| Enterprise Features | Dark mode, import/export, presets |

### 5.2 User Experience

| Metric | Target |
|--------|--------|
| Time to First Theme | < 2 minutes |
| Learning Curve | No training required |
| Preview Latency | < 100ms |
| Save/Load Time | < 500ms |

### 5.3 Enterprise Adoption

| Metric | Target |
|--------|--------|
| Brand Match Accuracy | 100% (exact colors) |
| Accessibility Compliance | WCAG 2.1 AA |
| Multi-tenant Support | Unlimited themes |
| RFP Checkbox Coverage | 100% of common requirements |

---

## Next Document

Continue to **Part 2: Feature Specification & Phases** for detailed feature breakdowns and implementation phases.
