# DynamicForms vs SurveyJS Feature Comparison
## Comprehensive Analysis for Enterprise & Government Market

**Document Version:** 1.0
**Date:** December 12, 2025
**Purpose:** Gap analysis between DynamicForms (VisualEditorOpus + Core.V4) and SurveyJS
**Target Market:** .NET Enterprise and Government applications

---

## Executive Summary

| Metric | DynamicForms | SurveyJS |
|--------|--------------|----------|
| **Field Types** | 17 | 25+ |
| **Multi-Language** | EN/FR (built-in) | 50+ languages |
| **Conditional Logic** | AND/OR/NOT, cross-module | Full expression syntax |
| **PDF Export** | Not implemented | Yes (licensed) |
| **Theme Editor** | Not implemented | Yes (GUI) |
| **Accessibility** | ARIA config available | WCAG 2.1 AA certified |
| **Platform** | Blazor/.NET | JavaScript (all frameworks) |
| **Licensing** | Proprietary | MIT (library) + Commercial (creator/PDF) |

**Overall Assessment:** DynamicForms covers ~70% of SurveyJS functionality. Critical gaps exist in question types, PDF export, and visual theming. However, DynamicForms has strong advantages in .NET integration, SQL Server persistence, and bilingual government forms.

---

## Feature Comparison by Category

### 1. Question/Field Types

#### ENTERPRISE CRITICAL - Missing in DynamicForms

| Feature | SurveyJS | DynamicForms | Priority | Notes |
|---------|----------|--------------|----------|-------|
| **Signature Pad** | Yes | Schema only | CRITICAL | Required for legal documents, consent forms |
| **Matrix Single-Select** | Yes | No | CRITICAL | Essential for Likert scales, surveys |
| **Matrix Multi-Select** | Yes | No | HIGH | Complex data collection grids |
| **Dynamic Matrix** | Yes | No | HIGH | Add/remove rows dynamically |
| **Rating/Stars** | Yes | No | HIGH | User satisfaction, feedback |
| **Ranking (Drag & Drop)** | Yes | No | HIGH | Priority ordering, preferences |
| **Slider** | Yes | No | MEDIUM | Numeric ranges, satisfaction scales |
| **Range Slider** | Yes | No | MEDIUM | Min-max selection |
| **Boolean Toggle** | Yes | Checkbox only | LOW | UX preference |

#### Currently Supported - Parity Exists

| Feature | SurveyJS | DynamicForms | Status |
|---------|----------|--------------|--------|
| Text Input | Yes | TextBox | PARITY |
| Text Area | Yes | TextArea | PARITY |
| Number | Yes | Number | PARITY |
| Currency | No (use Number) | Currency | ADVANTAGE |
| Dropdown | Yes | DropDown | PARITY |
| Radio Group | Yes | RadioGroup | PARITY |
| Checkbox List | Yes | CheckboxList | PARITY |
| Single Checkbox | Yes | Checkbox | PARITY |
| Date Picker | Yes | DatePicker | PARITY |
| Time Picker | Yes | TimePicker | PARITY |
| DateTime | Yes | DateTimePicker | PARITY |
| File Upload | Yes | FileUpload | PARITY |
| Data Grid | Yes (Dynamic Panel) | DataGrid | PARITY |
| AutoComplete | Yes | AutoComplete | PARITY |
| Section/Panel | Yes | Section, Panel | PARITY |
| HTML/Label | Yes | Label | PARITY |
| Divider | No | Divider | ADVANTAGE |

#### PUBLIC-FACING (Less Critical for Enterprise)

| Feature | SurveyJS | DynamicForms | Priority | Notes |
|---------|----------|--------------|----------|-------|
| Image Picker | Yes | No | LOW | Consumer surveys, visual selection |
| Video Display | Yes | No | LOW | Marketing surveys |
| Multiple Textboxes | Yes | No | LOW | Can use Section + TextBox |
| Expression (calculated display) | Yes | ComputedValue | PARITY | Different implementation |

---

### 2. Conditional Logic & Expressions

#### Current DynamicForms Capabilities

```
Operators Supported:
- Equals, NotEquals
- GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
- Contains, NotContains, StartsWith, EndsWith
- In, NotIn
- IsNull, IsNotNull, IsEmpty, IsNotEmpty

Logical Operators:
- AND, OR, NOT (via LogicalOperator enum)

Actions:
- show, hide, enable, disable
- setRequired, setOptional
- skipStep, goToStep, completeWorkflow
```

#### SurveyJS Additional Capabilities - GAPS

| Feature | SurveyJS | DynamicForms | Priority |
|---------|----------|--------------|----------|
| **Expression Syntax** | Full formula syntax | Basic operators | MEDIUM |
| **clearInvisibleValues** | Yes | No | HIGH |
| **requiredIf** | Yes | Via ConditionalRule | PARITY |
| **enableIf** | Yes | Via ConditionalRule | PARITY |
| **visibleIf in JSON** | Inline expression | Separate rule | PARITY |
| **setValueIf** | Yes | Via ComputedValue | PARITY |
| **Cascade Conditions** | Automatic | Manual | MEDIUM |

---

### 3. Validation

#### Current DynamicForms Validation

```csharp
FieldValidationConfig:
- IsRequired
- RequiredMessageEn/Fr
- MinLength, MaxLength
- Pattern (Regex)
- PatternMessageEn/Fr
- MinValue, MaxValue
- CustomRuleIds

Cross-Field Validation:
- AtLeastOne
- AllOrNone
- MutuallyExclusive
```

#### SurveyJS Additional Validators - GAPS

| Feature | SurveyJS | DynamicForms | Priority |
|---------|----------|--------------|----------|
| **Email Validator** | Built-in | Via Pattern | LOW |
| **URL Validator** | Built-in | Via Pattern | LOW |
| **Async Validation** | Yes | No | MEDIUM |
| **Real-time Validation** | onValueChanged | On blur only | MEDIUM |
| **Validation Expressions** | Custom functions | Via CustomRuleIds | PARITY |
| **Date Range Validation** | Min/Max Date | Via TypeConfig | PARITY |
| **File Size/Type Validation** | Yes | Via TypeConfig | PARITY |

---

### 4. Localization & Internationalization

#### ENTERPRISE CRITICAL

| Feature | SurveyJS | DynamicForms | Priority | Notes |
|---------|----------|--------------|----------|-------|
| **UI Languages** | 50+ | EN only | CRITICAL | Government requires multi-language UI |
| **Survey Content** | Translation tab | EN/FR built-in | ADVANTAGE | Bilingual by design |
| **RTL Support** | Yes | Not tested | HIGH | Arabic, Hebrew markets |
| **Machine Translation** | Azure AI | No | LOW | Nice to have |
| **Custom Locale Creation** | Yes | N/A | LOW | 50 languages sufficient |

**Recommendation:** DynamicForms has an advantage for Canadian bilingual requirements (EN/FR built-in at schema level). Add UI localization for international enterprise.

---

### 5. Theming & Styling

#### GAPS - Medium Priority

| Feature | SurveyJS | DynamicForms | Priority |
|---------|----------|--------------|----------|
| **Theme Editor GUI** | Yes | No | MEDIUM |
| **Predefined Themes** | 40 variations | 1 (CSS variables) | MEDIUM |
| **Dark Mode** | Yes | Via CSS variables | LOW |
| **High Contrast** | Yes | Via CSS variables | HIGH (Accessibility) |
| **Custom CSS Classes** | Per element | CssClasses property | PARITY |
| **Logo/Header Customization** | Yes | Via settings | PARITY |
| **Theme JSON Export** | Yes | No | LOW |

---

### 6. Accessibility (WCAG/Section 508)

#### ENTERPRISE CRITICAL for Government

| Feature | SurveyJS | DynamicForms | Priority |
|---------|----------|--------------|----------|
| **WCAG 2.1 AA Certified** | v2.1.0+ | Not certified | CRITICAL |
| **Section 508 Compliant** | Yes | Designed for | HIGH |
| **ARIA Labels** | Auto-generated | AccessibilityConfig | PARITY |
| **ARIA Describedby** | Auto-generated | Manual | HIGH |
| **Keyboard Navigation** | Full | Partial | HIGH |
| **Screen Reader Support** | Tested (Axe) | Not tested | CRITICAL |
| **Focus Management** | Yes | Partial | HIGH |
| **Skip Links** | Yes | No | MEDIUM |

**Recommendation:** Conduct WCAG audit and certification for government contracts.

---

### 7. Data Integration & Export

#### ENTERPRISE CRITICAL

| Feature | SurveyJS | DynamicForms | Priority |
|---------|----------|--------------|----------|
| **JSON Schema Storage** | JSON files | SQL Server | ADVANTAGE |
| **PDF Export (Blank)** | Yes (licensed) | No | CRITICAL |
| **PDF Export (Filled)** | Yes (licensed) | No | CRITICAL |
| **Fillable PDF Forms** | Yes | No | HIGH |
| **TypeScript Export** | Via API | Implemented | ADVANTAGE |
| **JSON Schema Export** | Yes | Yes | PARITY |
| **REST API Integration** | Client-side | Server-side | ADVANTAGE |
| **Database Mapping** | Manual | ColumnName/Type | ADVANTAGE |
| **Version History** | No | Version property | ADVANTAGE |

---

### 8. Workflow & Multi-Step Forms

#### Current DynamicForms Capabilities - STRONG

```
FormWorkflowSchema:
- Multi-module sequences
- Cross-module conditional rules
- Step jumping/skipping
- Progress indicator
- Auto-save
- Submit button customization (EN/FR)
```

#### SurveyJS Comparison

| Feature | SurveyJS | DynamicForms | Priority |
|---------|----------|--------------|----------|
| **Multi-Page Support** | Yes | Multi-module | PARITY |
| **Progress Bar** | Multiple types | Basic | MEDIUM |
| **Progress as Navigation** | Clickable steps | Not clickable | MEDIUM |
| **Table of Contents** | Yes | Outline panel | PARITY |
| **Conditional Navigation** | Yes | WorkflowRules | PARITY |
| **Cross-Module References** | No | Yes | ADVANTAGE |
| **Module Reuse** | No | Yes (separate modules) | ADVANTAGE |
| **Auto-Save** | Yes | Yes (configurable) | PARITY |

---

### 9. Visual Editor (Creator) Features

| Feature | SurveyJS Creator | VisualEditorOpus | Priority |
|---------|------------------|------------------|----------|
| **Drag & Drop Fields** | Yes | Yes | PARITY |
| **Property Panel** | Yes | RightSidebar | PARITY |
| **Field Outline Tree** | Yes | Yes | PARITY |
| **Condition Builder GUI** | Yes | ConditionBuilderModal | PARITY |
| **Formula Editor** | Yes | FormulaEditorModal | PARITY |
| **Theme Editor Tab** | Yes | No | MEDIUM |
| **Translation Tab** | Yes | No | LOW (EN/FR built-in) |
| **Logic Tab** | Yes | Via modal | PARITY |
| **Preview Mode** | Yes | Yes | PARITY |
| **JSON Editor** | Yes | Export only | LOW |
| **Undo/Redo** | Yes | No | HIGH |
| **Field Reorder (Drag)** | Yes | Yes (implemented) | PARITY |
| **Copy/Paste Fields** | Yes | Duplicate only | MEDIUM |
| **Field Search** | Yes | Yes | PARITY |
| **Keyboard Shortcuts** | Yes | Partial | LOW |

---

### 10. Enterprise-Specific Features

#### CRITICAL for Government/Enterprise

| Feature | SurveyJS | DynamicForms | Priority | Notes |
|---------|----------|--------------|----------|-------|
| **Role-Based Access** | No | ExtendedProperties | HIGH | Add RBAC layer |
| **Audit Trail** | No | DateCreated/Updated | MEDIUM | Add comprehensive logging |
| **Digital Signatures** | Signature pad | Schema only | CRITICAL | Implement or integrate |
| **E-Signature Integration** | No | No | HIGH | DocuSign, Adobe Sign |
| **Offline Support** | No | No | MEDIUM | PWA consideration |
| **Data Encryption** | Manual | Manual | HIGH | At-rest encryption |
| **Draft/Published Status** | No | Version + IsCurrent | PARITY |
| **Approval Workflow** | No | No | MEDIUM | Add workflow states |
| **Compliance Reporting** | Dashboard | No | HIGH | Submission analytics |
| **SLA Tracking** | No | No | LOW | Integration point |

---

## Priority Recommendations

### CRITICAL (Must Have for Enterprise/Government)

1. **Signature Pad Implementation** - Legal documents require it
2. **PDF Export** - Archival, printing, offline distribution
3. **WCAG/508 Certification** - Government contracts require it
4. **Matrix Question Types** - Surveys, assessments, evaluations
5. **UI Localization** - International enterprise markets

### HIGH Priority

1. **Rating/Stars** - User feedback, satisfaction surveys
2. **Ranking Questions** - Priority setting, preference ordering
3. **Undo/Redo in Editor** - Essential UX for form designers
4. **Real-time Validation** - Better user experience
5. **RTL Language Support** - Middle East market expansion

### MEDIUM Priority

1. **Theme Editor GUI** - Brand customization without developers
2. **Progress Bar Navigation** - Clickable step indicator
3. **Slider Controls** - Numeric range selection
4. **Async Validation** - Server-side validation
5. **Copy/Paste Fields** - Editor productivity

### LOW Priority (Public-Facing Focus)

1. **Image Picker** - Consumer surveys
2. **Video Display** - Marketing forms
3. **50+ Languages** - EN/FR sufficient for Canada
4. **Machine Translation** - Manual translation preferred
5. **Quiz Scoring** - Educational use cases

---

## Competitive Advantages of DynamicForms

| Feature | Advantage |
|---------|-----------|
| **Native .NET Integration** | No JavaScript bridge, full type safety |
| **SQL Server Persistence** | Enterprise-grade data storage with versioning |
| **Bilingual by Design** | EN/FR at schema level, not as translation layer |
| **Cross-Module References** | Workflow conditions can reference any module |
| **Module Reuse** | Same module in multiple workflows |
| **Currency Field Type** | Built-in, not workaround |
| **CodeSet Management** | Centralized option lists with DB storage |
| **Server-Side Rendering** | Blazor Server, no client-side vulnerabilities |
| **Database Column Mapping** | Direct SQL column configuration |

---

## Implementation Roadmap Suggestion

### Phase 1: Critical Gaps (Enterprise/Government Ready)
- Signature Pad field type
- PDF Export (basic)
- Matrix Single-Select
- WCAG accessibility audit

### Phase 2: Enhanced Functionality
- Rating/Stars
- Ranking questions
- Undo/Redo
- Progress bar navigation

### Phase 3: Polish & Differentiation
- Theme Editor
- Additional matrix types
- Advanced PDF features
- Dashboard analytics

---

## Sources

- [SurveyJS Major Updates 2024](https://surveyjs.io/stay-updated/major-updates/2024)
- [SurveyJS Documentation](https://surveyjs.io/form-library/documentation)
- [SurveyJS Accessibility Statement](https://surveyjs.io/accessibility-statement)
- [SurveyJS Pricing](https://surveyjs.io/pricing)
- [SurveyJS Localization](https://surveyjs.io/form-library/documentation/survey-localization)
- [SurveyJS Conditional Logic](https://surveyjs.io/form-library/documentation/design-survey/conditional-logic)
- [SurveyJS PDF Generator](https://surveyjs.io/pdf-generator/documentation/overview)
- [GitHub - SurveyJS Creator](https://github.com/surveyjs/survey-creator)
