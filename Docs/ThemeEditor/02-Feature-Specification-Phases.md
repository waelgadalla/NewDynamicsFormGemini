# Theme Editor Specification - Part 2
# Feature Specification & Implementation Phases

**Document Version:** 2.0
**Date:** December 2025

---

## 1. Complete Feature Specification

### 1.1 Theme Data Model

The core theme model that will be serialized to JSON and stored in the database:

```csharp
namespace DynamicForms.Models.Theming
{
    /// <summary>
    /// Complete theme configuration - serializable to JSON for storage
    /// </summary>
    public class FormTheme
    {
        // === Metadata ===
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Custom Theme";
        public string Description { get; set; } = "";
        public string BasePreset { get; set; } = "default"; // Which preset this derives from
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "";
        public int Version { get; set; } = 1;

        // === Mode Settings ===
        public ThemeMode Mode { get; set; } = ThemeMode.Light;
        public bool IsPanelless { get; set; } = false; // Compact/panelless view

        // === Color Palette ===
        public ThemeColors Colors { get; set; } = new();

        // === Typography ===
        public ThemeTypography Typography { get; set; } = new();

        // === Spacing & Layout ===
        public ThemeSpacing Spacing { get; set; } = new();

        // === Borders & Corners ===
        public ThemeBorders Borders { get; set; } = new();

        // === Shadows ===
        public ThemeShadows Shadows { get; set; } = new();

        // === Header ===
        public ThemeHeader Header { get; set; } = new();

        // === Background ===
        public ThemeBackground Background { get; set; } = new();

        // === Component Overrides (Advanced) ===
        public ThemeComponentStyles Components { get; set; } = new();

        // === Accessibility ===
        public ThemeAccessibility Accessibility { get; set; } = new();
    }

    public enum ThemeMode
    {
        Light,
        Dark,
        Auto // Follow system preference
    }

    /// <summary>
    /// Color palette - all colors used throughout the form
    /// </summary>
    public class ThemeColors
    {
        // Primary Brand Colors
        public string Primary { get; set; } = "#6366F1";        // Main brand/accent
        public string PrimaryHover { get; set; } = "#4F46E5";   // Primary hover state
        public string PrimaryForeground { get; set; } = "#FFFFFF"; // Text on primary

        // Secondary Colors
        public string Secondary { get; set; } = "#64748B";
        public string SecondaryHover { get; set; } = "#475569";
        public string SecondaryForeground { get; set; } = "#FFFFFF";

        // Surface Colors
        public string Background { get; set; } = "#FFFFFF";      // Main form background
        public string BackgroundDim { get; set; } = "#F8FAFC";   // Secondary background
        public string Surface { get; set; } = "#FFFFFF";         // Card/panel surface
        public string SurfaceHover { get; set; } = "#F1F5F9";    // Hover state

        // Text Colors
        public string TextPrimary { get; set; } = "#0F172A";     // Primary text
        public string TextSecondary { get; set; } = "#64748B";   // Secondary/muted text
        public string TextDisabled { get; set; } = "#94A3B8";    // Disabled text
        public string TextPlaceholder { get; set; } = "#94A3B8"; // Placeholder text

        // Border Colors
        public string Border { get; set; } = "#E2E8F0";          // Default borders
        public string BorderHover { get; set; } = "#CBD5E1";     // Border hover
        public string BorderFocus { get; set; } = "#6366F1";     // Focus ring

        // State Colors
        public string Error { get; set; } = "#EF4444";           // Error/danger
        public string ErrorBackground { get; set; } = "#FEF2F2"; // Error background
        public string Success { get; set; } = "#10B981";         // Success
        public string SuccessBackground { get; set; } = "#F0FDF4";
        public string Warning { get; set; } = "#F59E0B";         // Warning
        public string WarningBackground { get; set; } = "#FFFBEB";
        public string Info { get; set; } = "#3B82F6";            // Info
        public string InfoBackground { get; set; } = "#EFF6FF";

        // Interactive States
        public string FocusRing { get; set; } = "#6366F1";       // Focus indicator
        public string Selection { get; set; } = "#DBEAFE";       // Selected item bg
    }

    /// <summary>
    /// Typography settings
    /// </summary>
    public class ThemeTypography
    {
        // Font Families
        public string FontFamily { get; set; } = "'DM Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif";
        public string HeadingFontFamily { get; set; } = ""; // Empty = inherit from FontFamily
        public string MonoFontFamily { get; set; } = "'JetBrains Mono', 'Fira Code', 'Consolas', monospace";

        // Base Sizes
        public string BaseFontSize { get; set; } = "14px";
        public string LineHeight { get; set; } = "1.5";

        // Heading Sizes
        public string FormTitleSize { get; set; } = "24px";
        public string SectionTitleSize { get; set; } = "18px";
        public string QuestionTitleSize { get; set; } = "14px";
        public string DescriptionSize { get; set; } = "13px";

        // Font Weights
        public string FontWeightNormal { get; set; } = "400";
        public string FontWeightMedium { get; set; } = "500";
        public string FontWeightSemibold { get; set; } = "600";
        public string FontWeightBold { get; set; } = "700";
    }

    /// <summary>
    /// Spacing and layout settings
    /// </summary>
    public class ThemeSpacing
    {
        public string BaseUnit { get; set; } = "8px";            // Master spacing unit
        public string FormPadding { get; set; } = "32px";        // Form container padding
        public string SectionSpacing { get; set; } = "24px";     // Between sections
        public string QuestionSpacing { get; set; } = "20px";    // Between questions
        public string InputPadding { get; set; } = "12px";       // Inside inputs
        public string LabelSpacing { get; set; } = "6px";        // Label to input gap
        public string OptionSpacing { get; set; } = "8px";       // Between radio/checkbox options
    }

    /// <summary>
    /// Border and corner settings
    /// </summary>
    public class ThemeBorders
    {
        public string BorderWidth { get; set; } = "1px";
        public string BorderStyle { get; set; } = "solid";       // solid, dashed, dotted

        // Corner Radius
        public string RadiusSmall { get; set; } = "4px";         // Small elements
        public string RadiusMedium { get; set; } = "6px";        // Inputs, buttons
        public string RadiusLarge { get; set; } = "8px";         // Cards, panels
        public string RadiusXLarge { get; set; } = "12px";       // Large containers

        // Focus Ring
        public string FocusRingWidth { get; set; } = "2px";
        public string FocusRingOffset { get; set; } = "2px";
    }

    /// <summary>
    /// Shadow settings
    /// </summary>
    public class ThemeShadows
    {
        public string ShadowNone { get; set; } = "none";
        public string ShadowSmall { get; set; } = "0 1px 2px rgba(0, 0, 0, 0.05)";
        public string ShadowMedium { get; set; } = "0 4px 6px -1px rgba(0, 0, 0, 0.1)";
        public string ShadowLarge { get; set; } = "0 10px 15px -3px rgba(0, 0, 0, 0.1)";
        public string ShadowXLarge { get; set; } = "0 20px 25px -5px rgba(0, 0, 0, 0.1)";

        // Component-specific
        public string CardShadow { get; set; } = "0 1px 3px rgba(0, 0, 0, 0.1)";
        public string DropdownShadow { get; set; } = "0 4px 6px -1px rgba(0, 0, 0, 0.1)";
        public string ModalShadow { get; set; } = "0 20px 25px -5px rgba(0, 0, 0, 0.1)";
        public string InputFocusShadow { get; set; } = "0 0 0 3px rgba(99, 102, 241, 0.1)";
    }

    /// <summary>
    /// Header customization (Advanced)
    /// </summary>
    public class ThemeHeader
    {
        public bool Enabled { get; set; } = true;

        // Logo
        public string LogoUrl { get; set; } = "";
        public string LogoPosition { get; set; } = "left";       // left, center, right
        public string LogoMaxHeight { get; set; } = "48px";

        // Background
        public string BackgroundType { get; set; } = "color";    // color, image, gradient
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string BackgroundImage { get; set; } = "";
        public string BackgroundGradient { get; set; } = "";
        public string BackgroundSize { get; set; } = "cover";    // cover, contain, auto
        public string BackgroundPosition { get; set; } = "center";

        // Overlay
        public bool OverlayEnabled { get; set; } = false;
        public string OverlayColor { get; set; } = "rgba(0, 0, 0, 0.3)";

        // Layout
        public string Height { get; set; } = "auto";             // auto or specific px
        public string Padding { get; set; } = "24px";
        public string ContentAlignment { get; set; } = "left";   // left, center, right
        public string VerticalAlignment { get; set; } = "center"; // top, center, bottom
        public bool FullWidth { get; set; } = false;             // Extend beyond form
        public bool OverlapContent { get; set; } = false;        // Overlap form content

        // Title & Description Styling
        public string TitleColor { get; set; } = "";             // Empty = inherit
        public string TitleSize { get; set; } = "";
        public string DescriptionColor { get; set; } = "";
        public string DescriptionSize { get; set; } = "";
    }

    /// <summary>
    /// Form background settings
    /// </summary>
    public class ThemeBackground
    {
        public string Type { get; set; } = "color";              // color, image, gradient
        public string Color { get; set; } = "#F8FAFC";           // Page background
        public string Image { get; set; } = "";
        public string ImageSize { get; set; } = "cover";
        public string ImagePosition { get; set; } = "center";
        public string ImageRepeat { get; set; } = "no-repeat";
        public string ImageAttachment { get; set; } = "fixed";   // scroll, fixed
        public string ImageOpacity { get; set; } = "1";
        public string Gradient { get; set; } = "";
    }

    /// <summary>
    /// Component-specific style overrides (Advanced Mode)
    /// </summary>
    public class ThemeComponentStyles
    {
        public ComponentStyle Buttons { get; set; } = new();
        public ComponentStyle Inputs { get; set; } = new();
        public ComponentStyle Dropdowns { get; set; } = new();
        public ComponentStyle Checkboxes { get; set; } = new();
        public ComponentStyle RadioButtons { get; set; } = new();
        public ComponentStyle Panels { get; set; } = new();
        public ComponentStyle ProgressBar { get; set; } = new();
        public ComponentStyle Navigation { get; set; } = new();
    }

    public class ComponentStyle
    {
        public string BackgroundColor { get; set; } = "";
        public string TextColor { get; set; } = "";
        public string BorderColor { get; set; } = "";
        public string BorderRadius { get; set; } = "";
        public string Shadow { get; set; } = "";
        public string Padding { get; set; } = "";
        public Dictionary<string, string> CustomProperties { get; set; } = new();
    }

    /// <summary>
    /// Accessibility settings
    /// </summary>
    public class ThemeAccessibility
    {
        public string ScaleFactor { get; set; } = "100%";        // 80% - 150%
        public string MinFontSize { get; set; } = "12px";
        public bool HighContrastMode { get; set; } = false;
        public bool ReduceMotion { get; set; } = false;
        public string FocusIndicatorStyle { get; set; } = "ring"; // ring, outline, underline
    }
}
```

---

## 2. Theme Presets Specification

### 2.1 Preset Definitions

```csharp
public static class ThemePresets
{
    public static readonly Dictionary<string, FormTheme> Presets = new()
    {
        // === PROFESSIONAL THEMES ===

        ["default"] = new FormTheme
        {
            Name = "Default",
            Description = "Clean, professional default theme",
            Colors = new ThemeColors
            {
                Primary = "#6366F1",
                Background = "#FFFFFF",
                TextPrimary = "#0F172A"
            }
        },

        ["corporate"] = new FormTheme
        {
            Name = "Corporate",
            Description = "Traditional business styling",
            Colors = new ThemeColors
            {
                Primary = "#1E40AF",
                Background = "#FFFFFF",
                TextPrimary = "#1E293B"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Segoe UI', 'Roboto', sans-serif"
            },
            Borders = new ThemeBorders
            {
                RadiusMedium = "4px"
            }
        },

        ["modern"] = new FormTheme
        {
            Name = "Modern",
            Description = "Contemporary, minimalist design",
            Colors = new ThemeColors
            {
                Primary = "#8B5CF6",
                Background = "#FAFAFA",
                TextPrimary = "#18181B"
            },
            Borders = new ThemeBorders
            {
                RadiusMedium = "12px",
                RadiusLarge = "16px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "0 4px 20px rgba(0, 0, 0, 0.08)"
            }
        },

        // === GOVERNMENT THEMES ===

        ["government-federal"] = new FormTheme
        {
            Name = "Federal Government",
            Description = "U.S. Federal agency styling (USWDS inspired)",
            Colors = new ThemeColors
            {
                Primary = "#005EA2",
                Secondary = "#1A4480",
                Background = "#FFFFFF",
                TextPrimary = "#1B1B1B",
                Error = "#B50909",
                Success = "#00A91C"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Public Sans', 'Helvetica Neue', sans-serif",
                BaseFontSize = "16px"
            },
            Borders = new ThemeBorders
            {
                RadiusMedium = "0px" // Sharp corners
            }
        },

        ["government-canada"] = new FormTheme
        {
            Name = "Government of Canada",
            Description = "Canada.ca Web Experience Toolkit styling",
            Colors = new ThemeColors
            {
                Primary = "#26374A",
                Secondary = "#335075",
                Background = "#FFFFFF",
                TextPrimary = "#333333",
                Error = "#D3080C"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Noto Sans', sans-serif"
            }
        },

        // === ACCESSIBILITY THEMES ===

        ["high-contrast"] = new FormTheme
        {
            Name = "High Contrast",
            Description = "Maximum contrast for accessibility",
            Colors = new ThemeColors
            {
                Primary = "#0000FF",
                Background = "#FFFFFF",
                TextPrimary = "#000000",
                Border = "#000000",
                FocusRing = "#FF0000"
            },
            Borders = new ThemeBorders
            {
                BorderWidth = "2px",
                FocusRingWidth = "3px"
            },
            Accessibility = new ThemeAccessibility
            {
                HighContrastMode = true,
                MinFontSize = "16px"
            }
        },

        // === DARK THEMES ===

        ["dark"] = new FormTheme
        {
            Name = "Dark",
            Description = "Dark mode theme",
            Mode = ThemeMode.Dark,
            Colors = new ThemeColors
            {
                Primary = "#818CF8",
                Background = "#0F172A",
                BackgroundDim = "#1E293B",
                Surface = "#1E293B",
                TextPrimary = "#F1F5F9",
                TextSecondary = "#94A3B8",
                Border = "#334155"
            }
        },

        ["dark-modern"] = new FormTheme
        {
            Name = "Dark Modern",
            Description = "Sleek dark theme with purple accents",
            Mode = ThemeMode.Dark,
            Colors = new ThemeColors
            {
                Primary = "#A78BFA",
                Background = "#18181B",
                Surface = "#27272A",
                TextPrimary = "#FAFAFA",
                Border = "#3F3F46"
            },
            Borders = new ThemeBorders
            {
                RadiusMedium = "12px"
            }
        },

        // === SPECIALIZED THEMES ===

        ["healthcare"] = new FormTheme
        {
            Name = "Healthcare",
            Description = "Medical and healthcare forms",
            Colors = new ThemeColors
            {
                Primary = "#0D9488",
                Secondary = "#0891B2",
                Background = "#FFFFFF",
                TextPrimary = "#134E4A"
            }
        },

        ["education"] = new FormTheme
        {
            Name = "Education",
            Description = "Schools and universities",
            Colors = new ThemeColors
            {
                Primary = "#7C3AED",
                Secondary = "#2563EB",
                Background = "#FFFFFF",
                TextPrimary = "#1E1B4B"
            }
        },

        ["financial"] = new FormTheme
        {
            Name = "Financial",
            Description = "Banking and finance",
            Colors = new ThemeColors
            {
                Primary = "#047857",
                Secondary = "#0F766E",
                Background = "#FFFFFF",
                TextPrimary = "#064E3B"
            },
            Typography = new ThemeTypography
            {
                FontFamily = "'Inter', 'Helvetica Neue', sans-serif"
            }
        },

        // === MINIMAL THEMES ===

        ["borderless"] = new FormTheme
        {
            Name = "Borderless",
            Description = "Clean, border-free design",
            Colors = new ThemeColors
            {
                Primary = "#3B82F6",
                Border = "transparent",
                BorderHover = "#E5E7EB"
            },
            Borders = new ThemeBorders
            {
                BorderWidth = "0px"
            },
            Shadows = new ThemeShadows
            {
                CardShadow = "none"
            }
        },

        ["flat"] = new FormTheme
        {
            Name = "Flat",
            Description = "No shadows, flat design",
            Shadows = new ThemeShadows
            {
                ShadowSmall = "none",
                ShadowMedium = "none",
                ShadowLarge = "none",
                CardShadow = "none"
            }
        }
    };
}
```

---

## 3. Implementation Phases

### Phase 1: Foundation (MVP)
**Goal:** Basic theme editor that matches current spec + critical enterprise features

#### Phase 1.1: Core Theme Infrastructure
| Feature | Description | Priority |
|---------|-------------|----------|
| ThemeSettings Model | Extended model with all Phase 1 properties | P0 |
| ThemeEditorService | State management, CRUD operations | P0 |
| ThemeScope Component | CSS variable injection wrapper | P0 |
| CSS Variable System | 25+ variables for forms | P0 |
| Theme Persistence | Save/load from SQL Server | P0 |

#### Phase 1.2: Basic Editor UI
| Feature | Description | Priority |
|---------|-------------|----------|
| Theme Editor Page | `/admin/theme-editor` route | P0 |
| Color Pickers | Primary, secondary, background, text | P0 |
| Typography Section | Font family selector, sizes | P0 |
| Border Radius Control | Corner roundness slider | P0 |
| Live Preview Panel | Side-by-side preview | P0 |
| Save Button | Persist to database | P0 |

#### Phase 1.3: Theme Presets
| Feature | Description | Priority |
|---------|-------------|----------|
| Preset Selector | Dropdown with 8-10 presets | P0 |
| Preset Loading | Apply preset values to editor | P0 |
| Default Theme | Out-of-box professional theme | P0 |
| Government Presets | Federal, State templates | P0 |

#### Phase 1.4: Dark Mode
| Feature | Description | Priority |
|---------|-------------|----------|
| Mode Toggle | Light/Dark switch | P0 |
| Dark Theme Variables | Automatic dark mode colors | P0 |
| System Preference | Auto-detect OS preference | P1 |

#### Phase 1.5: Import/Export
| Feature | Description | Priority |
|---------|-------------|----------|
| Export to JSON | Download theme file | P0 |
| Import from JSON | Upload and apply theme | P0 |
| Copy CSS Variables | Copy to clipboard | P1 |

**Phase 1 Deliverables:**
- Working theme editor with live preview
- 10+ presets including government themes
- Dark/light mode support
- JSON import/export
- 25+ CSS variables

**Estimated Effort:** 3-4 weeks

---

### Phase 2: Enhanced Editor
**Goal:** Match SurveyJS feature parity for most common use cases

#### Phase 2.1: Undo/Redo System
| Feature | Description | Priority |
|---------|-------------|----------|
| Undo Stack | Track theme changes | P0 |
| Redo Stack | Restore undone changes | P0 |
| Keyboard Shortcuts | Ctrl+Z, Ctrl+Y | P0 |
| History Limit | 50 states max | P1 |

#### Phase 2.2: Basic/Advanced Mode Toggle
| Feature | Description | Priority |
|---------|-------------|----------|
| Mode Toggle | Switch between basic/advanced | P0 |
| Basic Mode | 8-10 essential controls only | P0 |
| Advanced Mode | Full 40+ controls | P0 |
| Mode Persistence | Remember user preference | P1 |

#### Phase 2.3: Header Customization
| Feature | Description | Priority |
|---------|-------------|----------|
| Logo Upload | File upload or URL input | P0 |
| Logo Positioning | Left, center, right alignment | P0 |
| Header Background | Color picker | P0 |
| Header Background Image | Image upload/URL | P1 |
| Header Height | Auto or fixed height | P1 |
| Overlay | Color overlay with opacity | P1 |

#### Phase 2.4: Background Customization
| Feature | Description | Priority |
|---------|-------------|----------|
| Background Color | Page background picker | P0 |
| Background Image | Upload or URL | P0 |
| Image Controls | Size, position, repeat, attachment | P1 |
| Background Opacity | Transparency slider | P1 |

#### Phase 2.5: Shadow Controls
| Feature | Description | Priority |
|---------|-------------|----------|
| Shadow Presets | None, Small, Medium, Large, Heavy | P0 |
| Card Shadows | Form card shadow control | P0 |
| Input Shadows | Focus shadow control | P1 |
| Custom Shadow Editor | Advanced shadow builder | P2 |

#### Phase 2.6: Additional Controls
| Feature | Description | Priority |
|---------|-------------|----------|
| Spacing Controls | Form padding, question spacing | P0 |
| State Colors | Error, success, warning, info | P0 |
| Border Style | Solid, dashed, dotted | P1 |
| Input Padding | Control internal padding | P1 |

#### Phase 2.7: UX Improvements
| Feature | Description | Priority |
|---------|-------------|----------|
| Reset to Default | Clear all changes | P0 |
| Collapsible Sections | Organize settings | P0 |
| Tooltips | Help text for each setting | P1 |
| Settings Search | Find settings by name | P2 |

**Phase 2 Deliverables:**
- Undo/redo with keyboard shortcuts
- Basic/Advanced mode toggle
- Full header customization
- Background images
- Shadow controls
- 40+ controls total
- Improved UX with collapsible sections

**Estimated Effort:** 3-4 weeks

---

### Phase 3: Advanced Features
**Goal:** Enterprise-grade features and component-level customization

#### Phase 3.1: Component-Level Styling
| Feature | Description | Priority |
|---------|-------------|----------|
| Button Styles | Primary, secondary, text buttons | P0 |
| Input Styles | Text, number, date inputs | P0 |
| Dropdown Styles | Select appearance | P0 |
| Checkbox/Radio Styles | Check mark, size, colors | P1 |
| Panel Styles | Section panels | P1 |
| Progress Bar | Completion indicator | P1 |
| Navigation Buttons | Next/prev/submit | P1 |

#### Phase 3.2: Accessibility Enhancements
| Feature | Description | Priority |
|---------|-------------|----------|
| Scale Factor | Global size multiplier | P0 |
| Focus Indicator Options | Ring, outline, underline | P0 |
| Contrast Checker | WCAG compliance warnings | P1 |
| Reduce Motion | Disable animations | P1 |
| Min Font Size | Enforce minimum text size | P1 |

#### Phase 3.3: Theme Management
| Feature | Description | Priority |
|---------|-------------|----------|
| Theme Library | List all saved themes | P0 |
| Duplicate Theme | Clone existing theme | P0 |
| Delete Theme | Remove theme with confirmation | P0 |
| Theme Versioning | Track theme history | P1 |
| Theme Categories | Organize themes by type | P2 |

#### Phase 3.4: Advanced Backgrounds
| Feature | Description | Priority |
|---------|-------------|----------|
| Gradient Editor | Linear/radial gradients | P1 |
| Pattern Library | Predefined patterns | P2 |
| Multi-layer Backgrounds | Stack backgrounds | P2 |

#### Phase 3.5: Enterprise Features
| Feature | Description | Priority |
|---------|-------------|----------|
| Theme Locking | Prevent modification | P1 |
| Theme Sharing | Share between users/orgs | P1 |
| Theme Approval | Workflow for theme approval | P2 |
| Brand Guidelines | Lock certain colors | P2 |
| Theme Templates | Organization templates | P2 |

#### Phase 3.6: Preview Enhancements
| Feature | Description | Priority |
|---------|-------------|----------|
| Multiple Preview Forms | Different form types | P1 |
| Device Preview | Desktop/tablet/mobile | P1 |
| Print Preview | Print stylesheet | P2 |
| PDF Preview | How PDF will look | P2 |

**Phase 3 Deliverables:**
- Component-level styling for all elements
- Accessibility features with WCAG checker
- Theme library management
- Gradient backgrounds
- Enterprise management features
- Enhanced preview options

**Estimated Effort:** 4-5 weeks

---

## 4. Phase Summary Table

| Phase | Features | CSS Variables | Presets | Effort |
|-------|----------|---------------|---------|--------|
| **Phase 1** | Core editor, presets, dark mode, import/export | 25+ | 10+ | 3-4 weeks |
| **Phase 2** | Undo/redo, advanced mode, header, backgrounds, shadows | 35+ | 12+ | 3-4 weeks |
| **Phase 3** | Component styling, accessibility, management, enterprise | 50+ | 15+ | 4-5 weeks |
| **Total** | Full SurveyJS parity | 50+ | 15+ | 10-13 weeks |

---

## 5. Feature Dependency Graph

```
Phase 1 (Foundation)
├── Theme Model ─────────────────┐
├── ThemeScope Component ────────┤
├── CSS Variable System ─────────┼──▶ Phase 2 (Enhanced)
├── Theme Persistence ───────────┤    ├── Undo/Redo (requires persistence)
├── Basic Editor UI ─────────────┤    ├── Basic/Advanced Toggle
├── Preset System ───────────────┤    ├── Header Customization
├── Dark Mode ───────────────────┘    ├── Background Images
└── Import/Export                     ├── Shadow Controls
                                      └── UX Improvements
                                              │
                                              ▼
                                      Phase 3 (Advanced)
                                      ├── Component Styling
                                      ├── Accessibility
                                      ├── Theme Management
                                      └── Enterprise Features
```

---

## Next Document

Continue to **Part 3: Technical Design & Architecture** for detailed implementation specifications.
