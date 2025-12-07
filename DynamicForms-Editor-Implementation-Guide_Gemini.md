# DynamicForms Visual Editor - Implementation Guide (Gemini)

> **Version**: 1.1 (Gemini Enhanced)
> **Target Platform**: Blazor Server (.NET 8+)  
> **Based On**: V12 HTML Mockup + DynamicForms-Schema-Reference.md  
> **Last Updated**: December 2025

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Architecture Overview](#2-architecture-overview)
3. [Project Structure](#3-project-structure)
4. [Implementation Phases](#4-implementation-phases)
5. [Phase 1: Foundation](#5-phase-1-foundation)
6. [Phase 2: Module Editor Core](#6-phase-2-module-editor-core)
7. [Phase 3: Advanced Field Features](#7-phase-3-advanced-field-features)
8. [Phase 4: Workflow Designer](#8-phase-4-workflow-designer)
9. [Phase 5: CodeSet Manager](#9-phase-5-codeset-manager)
10. [Phase 6: Polish & Integration](#10-phase-6-polish--integration)
11. [Component Specifications](#11-component-specifications)
12. [State Management](#12-state-management)
13. [AI Prompt Templates](#13-ai-prompt-templates)

---

## 1. Executive Summary

### Goal
Build a complete visual schema editor for the DynamicForms V4 system that allows users to:
- Design multi-module workflows with visual flowchart interface
- Create and edit form modules with drag-free field management
- Configure field properties including bilingual labels, validation, and conditional logic
- Manage CodeSets (reusable option lists)
- Import/Export schemas as JSON
- Real-time validation and preview

### Key Design Decisions
1. **No Drag-and-Drop**: Use button-based interactions (Up/Down arrows, Add buttons) for accessibility and simplicity
2. **Three-Panel Layout**: Left sidebar (Fields/Outline/Validation) | Canvas | Right sidebar (Properties)
3. **Global Navigation**: Fixed left icon bar for switching between Dashboard, Workflow, Editor, CodeSets
4. **Bilingual First**: All text inputs have EN/FR tabs built-in
5. **Immutable State**: Use record types with `with` expressions for all schema modifications
6. **Workflow Persistence**: Store visual layout data in `ExtendedProperties` to separate visual presentation from business logic.

### Technology Stack
- **Framework**: Blazor Server (.NET 8+)
- **UI Components**: Custom components (no external library dependency)
- **CSS**: Custom CSS with CSS variables for theming (light/dark)
- **Icons**: Bootstrap Icons
- **Fonts**: DM Sans (UI), JetBrains Mono (code/IDs)
- **State**: Fluxor or custom state container with undo/redo
- **Serialization**: System.Text.Json with polymorphic type support

---

## 2. Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Blazor Server Application                          │
├─────────────────────────────────────────────────────────────────────────────┤
│  Pages                                                                       │
│  ├── Dashboard.razor          - Landing page, recent workflows              │
│  ├── WorkflowDesigner.razor   - Node-based workflow editor                  │
│  ├── ModuleEditor.razor       - Field-level form designer                   │
│  ├── CodeSetManager.razor     - CodeSet CRUD interface                      │
│  └── Settings.razor           - Application settings                        │
├─────────────────────────────────────────────────────────────────────────────┤
│  Layouts                                                                     │
│  ├── MainLayout.razor         - Global navigation + theme                   │
│  └── EditorLayout.razor       - Three-panel editor layout                   │
├─────────────────────────────────────────────────────────────────────────────┤
│  Components                                                                  │
│  ├── Navigation/              - GlobalNav, Breadcrumbs                      │
│  ├── Dashboard/               - WorkflowCard, TemplateCard                  │
│  ├── Workflow/                - WorkflowCanvas, WfNode, WfConnection        │
│  ├── Editor/                  - FieldPalette, Canvas, OutlineTree          │
│  ├── Properties/              - PropertyPanel, sections, editors            │
│  ├── CodeSet/                 - CodeSetList, CodeSetEditor, ItemsTable     │
│  ├── Modals/                  - ConditionBuilder, FormulaEditor, etc.      │
│  └── Shared/                  - Buttons, Inputs, Badges, Toasts            │
├─────────────────────────────────────────────────────────────────────────────┤
│  Services                                                                    │
│  ├── EditorStateService       - Current selection, clipboard, undo/redo    │
│  ├── SchemaValidationService  - Real-time schema validation                │
│  ├── JsonImportExportService  - JSON serialization/deserialization         │
│  ├── WorkflowGraphService     - Compiles graph to schema rules             │
│  └── (from Core.V4)           - IFormHierarchyService, ICodeSetProvider    │
├─────────────────────────────────────────────────────────────────────────────┤
│  DynamicForms.Core.V4 (existing library)                                    │
│  ├── Schemas                  - FormWorkflowSchema, FormModuleSchema, etc. │
│  ├── Services                 - FormHierarchyService, ConditionEvaluator   │
│  ├── Builders                 - FormFieldBuilder, FormModuleBuilder        │
│  └── Runtime                  - FormFieldNode, FormModuleRuntime           │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Data Flow

```
User Action → Component Event → EditorStateService → Schema Mutation → Re-render
                                      ↓
                              Undo/Redo Stack
                                      ↓
                              Validation Service
                                      ↓
                              Validation Errors (displayed in Issues tab)
```

---

## 3. Project Structure

```
DynamicForms.Editor/
├── DynamicForms.Editor.csproj
├── Program.cs
├── appsettings.json
│
├── wwwroot/
│   ├── css/
│   │   ├── app.css                 # Main styles
│   │   ├── variables.css           # CSS custom properties
│   │   ├── components.css          # Component-specific styles
│   │   └── themes/
│   │       ├── light.css
│   │       └── dark.css
│   ├── js/
│   │   └── interop.js              # JS interop (keyboard shortcuts, etc.)
│   └── fonts/
│
├── Pages/
│   ├── Dashboard.razor
│   ├── WorkflowDesigner.razor
│   ├── ModuleEditor.razor
│   ├── CodeSetManager.razor
│   └── Settings.razor
│
├── Layouts/
│   ├── MainLayout.razor
│   ├── MainLayout.razor.css
│   └── EmptyLayout.razor
│
├── Components/
│   ├── Navigation/
│   │   ├── GlobalNav.razor
│   │   ├── GlobalNavItem.razor
│   │   └── Breadcrumbs.razor
│   │
│   ├── Dashboard/
│   │   ├── WorkflowGrid.razor
│   │   ├── WorkflowCard.razor
│   │   ├── TemplateRow.razor
│   │   └── TemplateCard.razor
│   │
│   ├── Workflow/
│   │   ├── WorkflowCanvas.razor
│   │   ├── WorkflowToolbar.razor
│   │   ├── WfNode.razor
│   │   ├── WfNodeStart.razor
│   │   ├── WfNodeModule.razor
│   │   ├── WfNodeDecision.razor
│   │   ├── WfNodeEnd.razor
│   │   ├── WfConnection.razor
│   │   ├── WfMinimap.razor
│   │   ├── WfZoomControls.razor
│   │   └── WfPropertiesPanel.razor
│   │
│   ├── Editor/
│   │   ├── EditorHeader.razor
│   │   ├── EditorToolbar.razor
│   │   ├── LeftSidebar.razor
│   │   ├── FieldPalette.razor
│   │   ├── FieldPaletteItem.razor
│   │   ├── OutlineTree.razor
│   │   ├── OutlineTreeNode.razor
│   │   ├── ValidationIssues.razor
│   │   ├── ValidationIssueItem.razor
│   │   ├── FormCanvas.razor
│   │   ├── CanvasField.razor
│   │   ├── CanvasSection.razor
│   │   └── FieldActions.razor
│   │
│   ├── Properties/
│   │   ├── PropertiesPanel.razor
│   │   ├── PropertySection.razor
│   │   ├── GeneralSection.razor
│   │   ├── LabelsSection.razor
│   │   ├── OptionsSection.razor
│   │   ├── HierarchySection.razor
│   │   ├── ValidationSection.razor
│   │   ├── ConditionalSection.razor
│   │   ├── ComputedSection.razor
│   │   ├── LayoutSection.razor
│   │   ├── AccessibilitySection.razor
│   │   ├── BilingualInput.razor
│   │   ├── InlineOptionsEditor.razor
│   │   └── CodeSetPicker.razor
│   │
│   ├── CodeSet/
│   │   ├── CodeSetSidebar.razor
│   │   ├── CodeSetListItem.razor
│   │   ├── CodeSetEditorPanel.razor
│   │   ├── CodeSetTabs.razor
│   │   ├── CodeSetConfigTab.razor
│   │   ├── CodeSetItemsTab.razor
│   │   ├── CodeSetJsonTab.razor
│   │   ├── CodeSetUsageTab.razor
│   │   └── CodeSetItemRow.razor
│   │
│   ├── Modals/
│   │   ├── ModalBase.razor
│   │   ├── ConditionBuilderModal.razor
│   │   ├── ConditionGroup.razor
│   │   ├── ConditionRow.razor
│   │   ├── FormulaEditorModal.razor
│   │   ├── CrossFieldValidationModal.razor
│   │   ├── MetadataModal.razor
│   │   ├── TypeConfigModal.razor
│   │   ├── AutoCompleteConfigEditor.razor
│   │   ├── DataGridConfigEditor.razor
│   │   ├── FileUploadConfigEditor.razor
│   │   ├── DateConfigEditor.razor
│   │   ├── ImportJsonModal.razor
│   │   └── ConfirmDeleteModal.razor
│   │
│   └── Shared/
│       ├── Button.razor
│       ├── IconButton.razor
│       ├── TextInput.razor
│       ├── NumberInput.razor
│       ├── SelectInput.razor
│       ├── Checkbox.razor
│       ├── Badge.razor
│       ├── Toast.razor
│       ├── ToastContainer.razor
│       ├── Tooltip.razor
│       ├── SearchBox.razor
│       ├── TabBar.razor
│       ├── TabPanel.razor
│       └── LoadingSpinner.razor
│
├── Services/
│   ├── EditorStateService.cs
│   ├── IEditorStateService.cs
│   ├── UndoRedoService.cs
│   ├── IUndoRedoService.cs
│   ├── SchemaValidationService.cs
│   ├── ISchemaValidationService.cs
│   ├── JsonImportExportService.cs
│   ├── IJsonImportExportService.cs
│   ├── WorkflowGraphService.cs
│   ├── IWorkflowGraphService.cs
│   ├── ClipboardService.cs
│   ├── IClipboardService.cs
│   ├── ToastService.cs
│   ├── IToastService.cs
│   └── ThemeService.cs
│
├── Models/
│   ├── EditorState.cs
│   ├── SelectionInfo.cs
│   ├── EditorCommand.cs
│   ├── ValidationIssue.cs
│   ├── WorkflowNodePosition.cs
│   └── FieldTypeDefinition.cs
│
└── Extensions/
    ├── SchemaExtensions.cs
    └── ServiceCollectionExtensions.cs
```

---

## 4. Implementation Phases

### Phase Overview

| Phase | Name | Duration | Key Deliverables |
|-------|------|----------|------------------|
| 1 | Foundation | 2 weeks | Project setup, shared components, navigation, theming |
| 2 | Module Editor Core | 3 weeks | Field palette, canvas, outline, basic properties |
| 3 | Advanced Field Features | 2 weeks | Options editor, conditions, formulas, TypeConfig |
| 4 | Workflow Designer | 2 weeks | Node canvas, layout persistence, graph logic compiler |
| 5 | CodeSet Manager | 1 week | CRUD interface, import/export |
| 6 | Polish & Integration | 2 weeks | Undo/redo, validation, testing, optimization |

### Dependency Graph

```
Phase 1 (Foundation)
    ↓
Phase 2 (Module Editor Core)
    ↓
    ├── Phase 3 (Advanced Features)
    │       ↓
    └── Phase 4 (Workflow Designer)
            ↓
        Phase 5 (CodeSet Manager)
            ↓
        Phase 6 (Polish)
```

---

## 5. Phase 1: Foundation (Weeks 1-2)

### Goals
- Set up project structure and dependencies
- Implement shared UI components
- Create layout system with global navigation
- Implement theming (light/dark)
- Create Dashboard page

### Tasks

#### 1.1 Project Setup
- [ ] Create Blazor Server project
- [ ] Add reference to DynamicForms.Core.V4
- [ ] Configure services in Program.cs
- [ ] Set up CSS architecture (variables, components, themes)
- [ ] Add Bootstrap Icons, fonts

#### 1.2 Shared Components
- [ ] Button, IconButton (variants: primary, outline, ghost)
- [ ] TextInput, NumberInput, SelectInput
- [ ] Checkbox, CheckboxGroup
- [ ] Badge (variants: draft, published, primary, danger, etc.)
- [ ] SearchBox
- [ ] TabBar, TabPanel
- [ ] Toast, ToastContainer, ToastService
- [ ] ModalBase
- [ ] LoadingSpinner
- [ ] Tooltip

#### 1.3 Layout & Navigation
- [ ] MainLayout with GlobalNav
- [ ] GlobalNav component (icon bar)
- [ ] GlobalNavItem with tooltip
- [ ] Theme toggle functionality
- [ ] ThemeService (persists preference)

#### 1.4 Dashboard Page
- [ ] Dashboard layout
- [ ] TemplateRow with TemplateCard components
- [ ] WorkflowGrid with WorkflowCard components
- [ ] Search/filter functionality
- [ ] "New Workflow" action

### Deliverables
- Working navigation between all pages (even if pages are stubs)
- Complete set of shared components
- Light/dark theme switching
- Dashboard with mock data

---

## 6. Phase 2: Module Editor Core (Weeks 3-5)

### Goals
- Implement three-panel editor layout
- Create field palette with all field types
- Build form canvas with field rendering
- Implement outline tree view
- Create basic properties panel

### Tasks

#### 2.1 Editor Layout
- [ ] EditorHeader (back button, title input, view switcher, actions)
- [ ] EditorToolbar (undo, redo, duplicate, delete, move up/down)
- [ ] Three-panel layout (left sidebar, canvas, right sidebar)
- [ ] Left sidebar with tabs (Fields, Outline, Issues)

#### 2.2 Field Palette
- [ ] FieldPalette component with categories
- [ ] FieldPaletteItem component
- [ ] Field type definitions (icon, label, default config)
- [ ] Search/filter functionality
- [ ] Click-to-add functionality

#### 2.3 Form Canvas
- [ ] FormCanvas container with scroll
- [ ] Canvas header (title, description)
- [ ] CanvasField component (renders field preview)
- [ ] CanvasSection component (renders section with children)
- [ ] FieldActions overlay (settings, duplicate, move, delete)
- [ ] Selection state (highlight selected field)
- [ ] Empty state ("Add your first field")

#### 2.4 Outline Tree
- [ ] OutlineTree component
- [ ] OutlineTreeNode (recursive, shows hierarchy)
- [ ] Icons for field types
- [ ] Selection sync with canvas
- [ ] Move up/down actions
- [ ] Expand/collapse sections

#### 2.5 Validation Issues Tab
- [ ] ValidationIssues list component
- [ ] ValidationIssueItem (error/warning styling)
- [ ] Click to select problematic field
- [ ] Real-time validation updates

#### 2.6 Basic Properties Panel
- [ ] PropertiesPanel container
- [ ] PropertySection (collapsible)
- [ ] GeneralSection (ID, type, order)
- [ ] LabelsSection with BilingualInput (EN/FR tabs)
- [ ] Basic ValidationSection (required, min/max length)
- [ ] LayoutSection (width, visible, readonly)

#### 2.7 State Management
- [ ] EditorStateService
- [ ] Selection management
- [ ] Schema mutation methods
- [ ] Field CRUD operations

### Deliverables
- Functional module editor
- Add, select, delete, reorder fields
- Edit basic properties
- View hierarchy in outline
- See validation issues

---

## 7. Phase 3: Advanced Field Features (Weeks 6-7)

### Goals
- Complete all property panel sections
- Implement options editor (inline + CodeSet)
- Build condition builder modal
- Create formula editor modal
- Add TypeConfig editors

### Tasks

#### 3.1 Options Section
- [ ] InlineOptionsEditor (grid: value, labelEN, labelFR, delete)
- [ ] Add option button
- [ ] Drag handle for reordering (or up/down buttons)
- [ ] CodeSetPicker dropdown (injected with ICodeSetProvider)
- [ ] CodeSet preview (shows items)
- [ ] Toggle between Inline/CodeSet modes

#### 3.2 Hierarchy Section
- [ ] Parent selector tree
- [ ] Current parent display
- [ ] RelationshipType dropdown
- [ ] Visual indicator for current field

#### 3.3 Condition Builder Modal
- [ ] ConditionBuilderModal container
- [ ] ConditionGroup (AND/OR/NOT toggle)
- [ ] ConditionRow (field, operator, value, delete)
- [ ] Add Condition / Add Group buttons
- [ ] Nested groups support
- [ ] Cross-module field picker (ModuleKey.FieldId)
- [ ] Action configuration (THEN section)
- [ ] Rule metadata (ID, priority, active)

#### 3.4 Formula Editor Modal
- [ ] FormulaEditorModal container
- [ ] Expression input (monospace)
- [ ] Available fields as clickable chips
- [ ] Operator chips (+, -, *, /, parens)
- [ ] Auto-detected dependencies display
- [ ] Basic syntax validation

#### 3.5 Cross-Field Validation Modal
- [ ] CrossFieldValidationModal container
- [ ] Validation type selector (AtLeastOne, AllOrNone, MutuallyExclusive)
- [ ] Multi-select field list
- [ ] Error message inputs (EN/FR)

#### 3.6 TypeConfig Editors
- [ ] TypeConfigModal with dynamic content
- [ ] AutoCompleteConfigEditor (URL, params, fields)
- [ ] DataGridConfigEditor (add/edit/delete flags, max rows, columns)
- [ ] FileUploadConfigEditor (extensions, size, multiple)
- [ ] DateConfigEditor (allow future/past, min/max)

#### 3.7 Metadata Modal
- [ ] MetadataModal
- [ ] Module info grid (ID, version, dates, created by)
- [ ] Database mapping (table name, schema)

### Deliverables
- Complete property editing for all field types
- Working condition builder with nested logic
- Formula editor with field insertion
- Cross-field validation configuration
- TypeConfig editors for all types

---

## 8. Phase 4: Workflow Designer (Weeks 8-9)

### Goals
- Implement node-based workflow canvas
- Create node components for all types
- Add connection visualization
- Build workflow properties panel
- Integrate with module editor
- **Implement Layout Persistence**
- **Implement Graph-to-Logic Compiler**

### Tasks

#### 4.1 Workflow Canvas
- [ ] WorkflowCanvas with grid background
- [ ] Pan and zoom functionality (JS interop)
- [ ] Node positioning (absolute)
- [ ] Canvas bounds calculation

#### 4.2 Workflow Toolbar
- [ ] WorkflowToolbar component
- [ ] Title input
- [ ] Status badge
- [ ] Simulate button (stub for now)
- [ ] JSON Export button
- [ ] Save buttons

#### 4.3 Node Components
- [ ] WfNode base component
- [ ] WfNodeStart (green header, trigger config)
- [ ] WfNodeModule (white, edit button → opens module editor)
- [ ] WfNodeDecision (diamond, rotated, condition display)
- [ ] WfNodeEnd (gray, completion action)
- [ ] Connection handles (top, bottom, left, right)
- [ ] Selection state

#### 4.4 Connections
- [ ] WfConnection (SVG path with arrow)
- [ ] Connection routing (bezier curves)
- [ ] Connection labels (Yes/No for decisions)
- [ ] Click to select connection

#### 4.5 Layout Persistence
- [ ] Define `WorkflowNodePosition` schema (NodeId, X, Y)
- [ ] Serialize/Deserialize layout data to `FormWorkflowSchema.ExtendedProperties`
- [ ] Restore node positions on load

#### 4.6 Graph-to-Logic Compiler
- [ ] Implement `WorkflowGraphService`
- [ ] Algorithm to traverse visual graph (nodes + connections)
- [ ] Convert "Decision Node -> Connection -> Module" patterns into `ConditionalRule` objects in schema
- [ ] Validate graph connectivity (detect disconnected nodes)

#### 4.7 Workflow Properties Panel
- [ ] WfPropertiesPanel
- [ ] Module properties (title, ID, step number)
- [ ] Permissions checkboxes
- [ ] Navigation type selector
- [ ] "Edit Form Layout" button

### Deliverables
- Visual workflow designer with nodes
- Add/remove/connect modules
- Edit module properties
- Navigate to module editor
- Save workflow schema with persisted layout
- Graph compilation to runtime rules

---

## 9. Phase 5: CodeSet Manager (Week 10)

### Goals
- Implement CodeSet CRUD interface
- Create items table editor
- Add import/export functionality
- Show usage information

### Tasks

#### 5.1 CodeSet Sidebar
- [ ] CodeSetSidebar with list
- [ ] CodeSetListItem (name, count, category badge)
- [ ] Search/filter
- [ ] New CodeSet button
- [ ] Selection state

#### 5.2 CodeSet Editor Panel
- [ ] CodeSetEditorPanel container
- [ ] Toolbar (title, ID, import, export, save)
- [ ] CodeSetTabs (Configuration, Items, JSON, Usage)

#### 5.3 Configuration Tab
- [ ] Code input (unique identifier)
- [ ] Category selector
- [ ] Display name EN/FR
- [ ] Active checkbox

#### 5.4 Items Tab
- [ ] CodeSetItemsTab with table
- [ ] CodeSetItemRow (drag handle, value, textEN, textFR, default, active, delete)
- [ ] Add item button
- [ ] Reorder functionality

#### 5.5 JSON Preview Tab
- [ ] Read-only JSON display
- [ ] Copy button
- [ ] Syntax highlighting

#### 5.6 Usage Tab
- [ ] List of fields using this CodeSet
- [ ] Links to open those fields

#### 5.7 Import/Export
- [ ] CSV import functionality
- [ ] JSON export
- [ ] Validation on import

### Deliverables
- Complete CodeSet management
- Create, edit, delete CodeSets
- Add/remove items
- Import from CSV
- Export to JSON

---

## 10. Phase 6: Polish & Integration (Weeks 11-12)

### Goals
- Implement undo/redo system
- Complete validation system
- Add keyboard shortcuts
- Performance optimization
- Testing and bug fixes

### Tasks

#### 6.1 Undo/Redo System
- [ ] UndoRedoService with command stack
- [ ] EditorCommand record
- [ ] Capture state before mutations
- [ ] Undo/Redo actions
- [ ] Keyboard shortcuts (Ctrl+Z, Ctrl+Y)

#### 6.2 Clipboard Operations
- [ ] ClipboardService
- [ ] Copy field (Ctrl+C)
- [ ] Paste field (Ctrl+V)
- [ ] Duplicate (Ctrl+D)

#### 6.3 Keyboard Shortcuts
- [ ] JS interop for global shortcuts
- [ ] Save (Ctrl+S)
- [ ] Delete (Delete key)
- [ ] Escape (close modals, deselect)
- [ ] Arrow keys (navigate outline)

#### 6.4 Validation System
- [ ] SchemaValidationService
- [ ] Required field checks
- [ ] Unique ID validation
- [ ] Circular reference detection
- [ ] Orphan field detection
- [ ] Real-time validation on changes
- [ ] Validation issues display

#### 6.5 Import/Export
- [ ] JsonImportExportService
- [ ] Import from JSON file
- [ ] Export to JSON file
- [ ] Pretty-print formatting
- [ ] Validation on import

#### 6.6 Performance
- [ ] Virtualization for large field lists
- [ ] Debounced property updates
- [ ] Optimized re-renders

#### 6.7 Testing
- [ ] Unit tests for services
- [ ] Component tests
- [ ] Integration tests
- [ ] Manual testing checklist

### Deliverables
- Complete, polished editor
- Undo/redo working
- Keyboard shortcuts
- Import/export JSON
- All validations working
- Performance optimized


---

## 11. Component Specifications

### 11.1 GlobalNav Component

```razor
@* Components/Navigation/GlobalNav.razor *@

<nav class="global-nav">
    <div class="nav-logo">
        <i class="bi bi-ui-checks-grid"></i>
    </div>
    
    <GlobalNavItem Icon="bi-house-door" Label="Dashboard" 
                   Route="/" IsActive="@(CurrentRoute == "/")" />
    <GlobalNavItem Icon="bi-diagram-3" Label="Workflow Designer" 
                   Route="/workflow" IsActive="@(CurrentRoute.StartsWith("/workflow"))" />
    <GlobalNavItem Icon="bi-pencil-square" Label="Module Editor" 
                   Route="/editor" IsActive="@(CurrentRoute.StartsWith("/editor"))" />
    <GlobalNavItem Icon="bi-list-check" Label="CodeSet Manager" 
                   Route="/codesets" IsActive="@(CurrentRoute.StartsWith("/codesets"))" />
    
    <div class="nav-divider"></div>
    
    <GlobalNavItem Icon="bi-gear" Label="Settings" 
                   Route="/settings" IsActive="@(CurrentRoute == "/settings")" />
    
    <div class="nav-spacer"></div>
    
    <GlobalNavItem Icon="@ThemeIcon" Label="Toggle Theme" 
                   OnClick="ToggleTheme" />
</nav>

@code {
    [Inject] private NavigationManager Navigation { get; set; }
    [Inject] private IThemeService ThemeService { get; set; }
    
    private string CurrentRoute => Navigation.ToBaseRelativePath(Navigation.Uri);
    private string ThemeIcon => ThemeService.IsDarkMode ? "bi-sun" : "bi-moon-stars";
    
    private void ToggleTheme() => ThemeService.Toggle();
}
```

### 11.2 EditorStateService Interface

```csharp
// Services/IEditorStateService.cs

public interface IEditorStateService
{
    // === Current State ===
    FormWorkflowSchema? CurrentWorkflow { get; }
    FormModuleSchema? CurrentModule { get; }
    string? SelectedFieldId { get; }
    FormFieldSchema? SelectedField { get; }
    
    // === Events ===
    event Action? OnStateChanged;
    event Action<string>? OnFieldSelected;
    event Action? OnModuleChanged;
    
    // === Workflow Operations ===
    void LoadWorkflow(FormWorkflowSchema workflow);
    void UpdateWorkflow(FormWorkflowSchema workflow);
    void AddModuleToWorkflow(int moduleId);
    void RemoveModuleFromWorkflow(int moduleId);
    void ReorderModules(int[] newOrder);
    
    // === Module Operations ===
    void LoadModule(FormModuleSchema module);
    void UpdateModule(FormModuleSchema module);
    FormModuleSchema CreateNewModule(string titleEn, string? titleFr = null);
    
    // === Field Operations ===
    void SelectField(string? fieldId);
    void AddField(string fieldType, string? parentId = null);
    void UpdateField(FormFieldSchema field);
    void DeleteField(string fieldId);
    void DuplicateField(string fieldId);
    void MoveField(string fieldId, MoveDirection direction);
    void ChangeFieldParent(string fieldId, string? newParentId);
    
    // === Clipboard ===
    void CopyField(string fieldId);
    void PasteField(string? parentId = null);
    bool HasClipboard { get; }
    
    // === Validation ===
    IReadOnlyList<ValidationIssue> ValidationIssues { get; }
    void RefreshValidation();
    
    // === Undo/Redo ===
    bool CanUndo { get; }
    bool CanRedo { get; }
    void Undo();
    void Redo();
}

public enum MoveDirection { Up, Down }

public record ValidationIssue(
    string FieldId,
    ValidationSeverity Severity,
    string Title,
    string Description
);

public enum ValidationSeverity { Error, Warning, Info }
```

### 11.3 Field Palette Item Definition

```csharp
// Models/FieldTypeDefinition.cs

public record FieldTypeDefinition(
    string FieldType,
    string DisplayName,
    string Icon,
    string Category,
    bool RequiresTypeConfig = false,
    Type? TypeConfigType = null
)
{
    public static readonly FieldTypeDefinition[] AllTypes = new[]
    {
        // Basic
        new FieldTypeDefinition("TextBox", "Text Input", "bi-input-cursor-text", "Basic"),
        new FieldTypeDefinition("TextArea", "Text Area", "bi-textarea-t", "Basic"),
        new FieldTypeDefinition("Number", "Number", "bi-123", "Basic"),
        new FieldTypeDefinition("Currency", "Currency", "bi-currency-dollar", "Basic"),
        
        // Choice
        new FieldTypeDefinition("DropDown", "Dropdown", "bi-menu-button-wide", "Choice"),
        new FieldTypeDefinition("RadioGroup", "Radio Group", "bi-ui-radios", "Choice"),
        new FieldTypeDefinition("CheckboxList", "Checkboxes", "bi-ui-checks", "Choice"),
        new FieldTypeDefinition("Checkbox", "Single Checkbox", "bi-check-square", "Choice"),
        
        // Date & Time
        new FieldTypeDefinition("DatePicker", "Date Picker", "bi-calendar-event", "Date & Time", 
            true, typeof(DateConfig)),
        new FieldTypeDefinition("TimePicker", "Time Picker", "bi-clock", "Date & Time"),
        new FieldTypeDefinition("DateTimePicker", "Date & Time", "bi-calendar-week", "Date & Time",
            true, typeof(DateConfig)),
        
        // Advanced
        new FieldTypeDefinition("FileUpload", "File Upload", "bi-cloud-upload", "Advanced",
            true, typeof(FileUploadConfig)),
        new FieldTypeDefinition("DataGrid", "Data Grid", "bi-table", "Advanced",
            true, typeof(DataGridConfig)),
        new FieldTypeDefinition("AutoComplete", "AutoComplete", "bi-search", "Advanced",
            true, typeof(AutoCompleteConfig)),
        
        // Layout
        new FieldTypeDefinition("Section", "Section", "bi-layout-three-columns", "Layout"),
        new FieldTypeDefinition("Panel", "Panel", "bi-window", "Layout"),
        new FieldTypeDefinition("Divider", "Divider", "bi-dash-lg", "Layout"),
        new FieldTypeDefinition("Label", "Label", "bi-fonts", "Layout"),
    };
    
    public static FieldTypeDefinition? GetByType(string fieldType) 
        => AllTypes.FirstOrDefault(t => t.FieldType == fieldType);
    
    public static IEnumerable<IGrouping<string, FieldTypeDefinition>> GetByCategory()
        => AllTypes.GroupBy(t => t.Category);
}
```

### 11.4 BilingualInput Component

```razor
@* Components/Properties/BilingualInput.razor *@

<div class="bilingual-input">
    <div class="lang-tabs">
        <button class="lang-tab @(ActiveLang == "en" ? "active" : "")" 
                @onclick="() => ActiveLang = \"en\"">EN</button>
        <button class="lang-tab @(ActiveLang == "fr" ? "active" : "")" 
                @onclick="() => ActiveLang = \"fr\"">FR</button>
    </div>
    
    @if (ActiveLang == "en")
    {
        @if (Multiline)
        {
            <textarea class="prop-input" 
                      @bind="ValueEn" 
                      @bind:event="oninput"
                      placeholder="@PlaceholderEn"></textarea>
        }
        else
        {
            <input type="text" class="prop-input" 
                   @bind="ValueEn" 
                   @bind:event="oninput"
                   placeholder="@PlaceholderEn" />
        }
    }
    else
    {
        @if (Multiline)
        {
            <textarea class="prop-input" 
                      @bind="ValueFr" 
                      @bind:event="oninput"
                      placeholder="@PlaceholderFr"></textarea>
        }
        else
        {
            <input type="text" class="prop-input" 
                   @bind="ValueFr" 
                   @bind:event="oninput"
                   placeholder="@PlaceholderFr" />
        }
    }
</div>

@code {
    [Parameter] public string? ValueEn { get; set; }
    [Parameter] public EventCallback<string?> ValueEnChanged { get; set; }
    
    [Parameter] public string? ValueFr { get; set; }
    [Parameter] public EventCallback<string?> ValueFrChanged { get; set; }
    
    [Parameter] public string PlaceholderEn { get; set; } = "English";
    [Parameter] public string PlaceholderFr { get; set; } = "Français";
    [Parameter] public bool Multiline { get; set; }
    
    private string ActiveLang { get; set; } = "en";
}
```

### 11.5 ConditionRow Component

```razor
@* Components/Modals/ConditionRow.razor *@

<div class="condition-row">
    <select class="prop-select" @bind="Condition.Field">
        <option value="">Select Field...</option>
        @foreach (var field in AvailableFields)
        {
            <option value="@field.Id">@field.Label</option>
        }
        <optgroup label="Other Modules">
            @foreach (var moduleField in CrossModuleFields)
            {
                <option value="@moduleField.Reference">@moduleField.Label</option>
            }
        </optgroup>
    </select>
    
    <select class="prop-select" @bind="Condition.Operator">
        @foreach (var op in Operators)
        {
            <option value="@op">@GetOperatorLabel(op)</option>
        }
    </select>
    
    @if (!IsNullOperator)
    {
        <input type="text" class="prop-input" 
               @bind="Condition.Value" 
               placeholder="Value" />
    }
    else
    {
        <input type="text" class="prop-input" disabled placeholder="(not required)" />
    }
    
    <button class="btn btn-ghost btn-icon-sm" @onclick="OnDelete">
        <i class="bi bi-x"></i>
    </button>
</div>

@code {
    [Parameter] public SimpleConditionModel Condition { get; set; } = new();
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public IEnumerable<FieldReference> AvailableFields { get; set; } = [];
    [Parameter] public IEnumerable<CrossModuleFieldReference> CrossModuleFields { get; set; } = [];
    
    private bool IsNullOperator => Condition.Operator is 
        ConditionOperator.IsNull or ConditionOperator.IsNotNull or 
        ConditionOperator.IsEmpty or ConditionOperator.IsNotEmpty;
    
    private static readonly ConditionOperator[] Operators = Enum.GetValues<ConditionOperator>();
    
    private string GetOperatorLabel(ConditionOperator op) => op switch
    {
        ConditionOperator.Equals => "Equals",
        ConditionOperator.NotEquals => "Not Equals",
        ConditionOperator.Contains => "Contains",
        ConditionOperator.StartsWith => "Starts With",
        ConditionOperator.EndsWith => "Ends With",
        ConditionOperator.GreaterThan => "Greater Than",
        ConditionOperator.LessThan => "Less Than",
        ConditionOperator.In => "In List",
        ConditionOperator.NotIn => "Not In List",
        ConditionOperator.IsNull => "Is Empty",
        ConditionOperator.IsNotNull => "Is Not Empty",
        _ => op.ToString()
    };
}

public class SimpleConditionModel
{
    public string? Field { get; set; }
    public ConditionOperator Operator { get; set; } = ConditionOperator.Equals;
    public object? Value { get; set; }
}

public record FieldReference(string Id, string Label);
public record CrossModuleFieldReference(string Reference, string Label);
```

---

## 12. State Management

### 12.1 EditorState Record

```csharp
// Models/EditorState.cs

public record EditorState
{
    // Current data
    public FormWorkflowSchema? Workflow { get; init; }
    public FormModuleSchema? Module { get; init; }
    public FormModuleRuntime? ModuleRuntime { get; init; }
    
    // Selection
    public string? SelectedFieldId { get; init; }
    public string? SelectedNodeId { get; init; }  // For workflow designer
    
    // Clipboard
    public FormFieldSchema? ClipboardField { get; init; }
    
    // Validation
    public ImmutableList<ValidationIssue> Issues { get; init; } 
        = ImmutableList<ValidationIssue>.Empty;
    
    // UI State
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    
    // Computed
    public FormFieldSchema? SelectedField => SelectedFieldId is not null && Module is not null
        ? Module.Fields.FirstOrDefault(f => f.Id == SelectedFieldId)
        : null;
    
    public FormFieldNode? SelectedFieldNode => SelectedFieldId is not null && ModuleRuntime is not null
        ? ModuleRuntime.GetField(SelectedFieldId)
        : null;
}
```

### 12.2 EditorStateService Implementation

```csharp
// Services/EditorStateService.cs

public class EditorStateService : IEditorStateService
{
    private readonly IFormHierarchyService _hierarchyService;
    private readonly ISchemaValidationService _validationService;
    private readonly IUndoRedoService _undoRedo;
    
    private EditorState _state = new();
    
    public event Action? OnStateChanged;
    public event Action<string>? OnFieldSelected;
    public event Action? OnModuleChanged;
    
    public EditorStateService(
        IFormHierarchyService hierarchyService,
        ISchemaValidationService validationService,
        IUndoRedoService undoRedo)
    {
        _hierarchyService = hierarchyService;
        _validationService = validationService;
        _undoRedo = undoRedo;
    }
    
    // === Accessors ===
    public FormWorkflowSchema? CurrentWorkflow => _state.Workflow;
    public FormModuleSchema? CurrentModule => _state.Module;
    public string? SelectedFieldId => _state.SelectedFieldId;
    public FormFieldSchema? SelectedField => _state.SelectedField;
    public IReadOnlyList<ValidationIssue> ValidationIssues => _state.Issues;
    public bool CanUndo => _undoRedo.CanUndo;
    public bool CanRedo => _undoRedo.CanRedo;
    public bool HasClipboard => _state.ClipboardField is not null;
    
    // === Module Operations ===
    public void LoadModule(FormModuleSchema module)
    {
        var runtime = _hierarchyService.BuildHierarchyAsync(module).GetAwaiter().GetResult();
        var issues = _validationService.Validate(module);
        
        _state = _state with 
        { 
            Module = module, 
            ModuleRuntime = runtime,
            Issues = issues.ToImmutableList(),
            SelectedFieldId = null 
        };
        
        OnModuleChanged?.Invoke();
        OnStateChanged?.Invoke();
    }
    
    public void UpdateModule(FormModuleSchema module)
    {
        _undoRedo.SaveState(_state.Module!);
        LoadModule(module);
    }
    
    // === Field Operations ===
    public void SelectField(string? fieldId)
    {
        if (_state.SelectedFieldId == fieldId) return;
        
        _state = _state with { SelectedFieldId = fieldId };
        OnFieldSelected?.Invoke(fieldId ?? "");
        OnStateChanged?.Invoke();
    }
    
    public void AddField(string fieldType, string? parentId = null)
    {
        if (_state.Module is null) return;
        
        var newField = CreateDefaultField(fieldType, parentId);
        var newFields = _state.Module.Fields.Append(newField).ToArray();
        var newModule = _state.Module with { Fields = newFields };
        
        UpdateModule(newModule);
        SelectField(newField.Id);
    }
    
    public void UpdateField(FormFieldSchema field)
    {
        if (_state.Module is null) return;
        
        var newFields = _state.Module.Fields
            .Select(f => f.Id == field.Id ? field : f)
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };
        
        UpdateModule(newModule);
    }
    
    public void DeleteField(string fieldId)
    {
        if (_state.Module is null) return;
        
        // Also delete children
        var idsToDelete = GetFieldAndDescendantIds(fieldId);
        var newFields = _state.Module.Fields
            .Where(f => !idsToDelete.Contains(f.Id))
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };
        
        if (_state.SelectedFieldId == fieldId)
        {
            _state = _state with { SelectedFieldId = null };
        }
        
        UpdateModule(newModule);
    }
    
    public void MoveField(string fieldId, MoveDirection direction)
    {
        if (_state.Module is null) return;
        
        var field = _state.Module.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (field is null) return;
        
        // Get siblings (fields with same parent)
        var siblings = _state.Module.Fields
            .Where(f => f.ParentId == field.ParentId)
            .OrderBy(f => f.Order)
            .ToList();
        
        var index = siblings.FindIndex(f => f.Id == fieldId);
        var newIndex = direction == MoveDirection.Up ? index - 1 : index + 1;
        
        if (newIndex < 0 || newIndex >= siblings.Count) return;
        
        // Swap orders
        var otherField = siblings[newIndex];
        var newOrder = otherField.Order;
        var otherNewOrder = field.Order;
        
        var newFields = _state.Module.Fields
            .Select(f => f.Id == fieldId ? f with { Order = newOrder } :
                         f.Id == otherField.Id ? f with { Order = otherNewOrder } : f)
            .ToArray();
        var newModule = _state.Module with { Fields = newFields };
        
        UpdateModule(newModule);
    }
    
    // === Undo/Redo ===
    public void Undo()
    {
        if (!_undoRedo.CanUndo) return;
        var previousModule = _undoRedo.Undo(_state.Module!);
        LoadModule(previousModule);
    }
    
    public void Redo()
    {
        if (!_undoRedo.CanRedo) return;
        var nextModule = _undoRedo.Redo(_state.Module!);
        LoadModule(nextModule);
    }
    
    // === Helpers ===
    private FormFieldSchema CreateDefaultField(string fieldType, string? parentId)
    {
        var maxOrder = _state.Module!.Fields
            .Where(f => f.ParentId == parentId)
            .Select(f => f.Order)
            .DefaultIfEmpty(0)
            .Max();
        
        var id = GenerateFieldId(fieldType);
        
        return new FormFieldSchema
        {
            Id = id,
            FieldType = fieldType,
            LabelEn = $"New {fieldType}",
            ParentId = parentId,
            Order = maxOrder + 1
        };
    }
    
    private string GenerateFieldId(string fieldType)
    {
        var baseName = fieldType.ToLowerInvariant();
        var count = 1;
        var id = $"{baseName}_{count}";
        
        while (_state.Module!.Fields.Any(f => f.Id == id))
        {
            count++;
            id = $"{baseName}_{count}";
        }
        
        return id;
    }
    
    private HashSet<string> GetFieldAndDescendantIds(string fieldId)
    {
        var ids = new HashSet<string> { fieldId };
        var queue = new Queue<string>();
        queue.Enqueue(fieldId);
        
        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = _state.Module!.Fields
                .Where(f => f.ParentId == currentId)
                .Select(f => f.Id);
            
            foreach (var childId in children)
            {
                ids.Add(childId);
                queue.Enqueue(childId);
            }
        }
        
        return ids;
    }
}
```

### 12.3 UndoRedoService

```csharp
// Services/UndoRedoService.cs

public interface IUndoRedoService
{
    bool CanUndo { get; }
    bool CanRedo { get; }
    void SaveState(FormModuleSchema module);
    FormModuleSchema Undo(FormModuleSchema current);
    FormModuleSchema Redo(FormModuleSchema current);
    void Clear();
}

public class UndoRedoService : IUndoRedoService
{
    private readonly Stack<FormModuleSchema> _undoStack = new();
    private readonly Stack<FormModuleSchema> _redoStack = new();
    private const int MaxStackSize = 50;
    
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    
    public void SaveState(FormModuleSchema module)
    {
        _undoStack.Push(module);
        _redoStack.Clear();
        
        // Limit stack size
        if (_undoStack.Count > MaxStackSize)
        {
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = 0; i < MaxStackSize; i++)
            {
                _undoStack.Push(items[i]);
            }
        }
    }
    
    public FormModuleSchema Undo(FormModuleSchema current)
    {
        if (!CanUndo) throw new InvalidOperationException("Nothing to undo");
        
        _redoStack.Push(current);
        return _undoStack.Pop();
    }
    
    public FormModuleSchema Redo(FormModuleSchema current)
    {
        if (!CanRedo) throw new InvalidOperationException("Nothing to redo");
        
        _undoStack.Push(current);
        return _redoStack.Pop();
    }
    
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
```

### 12.4 SchemaValidationService

```csharp
// Services/SchemaValidationService.cs

public interface ISchemaValidationService
{
    IEnumerable<ValidationIssue> Validate(FormModuleSchema module);
    IEnumerable<ValidationIssue> ValidateField(FormFieldSchema field, FormModuleSchema module);
}

public class SchemaValidationService : ISchemaValidationService
{
    public IEnumerable<ValidationIssue> Validate(FormModuleSchema module)
    {
        var issues = new List<ValidationIssue>();
        
        // Check for duplicate IDs
        var duplicateIds = module.Fields
            .GroupBy(f => f.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        
        foreach (var id in duplicateIds)
        {
            issues.Add(new ValidationIssue(
                id,
                ValidationSeverity.Error,
                "Duplicate Field ID",
                $"Multiple fields have the ID '{id}'"
            ));
        }
        
        // Check for orphaned fields
        var allIds = module.Fields.Select(f => f.Id).ToHashSet();
        foreach (var field in module.Fields.Where(f => f.ParentId is not null))
        {
            if (!allIds.Contains(field.ParentId!))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Error,
                    "Orphaned Field",
                    $"Parent '{field.ParentId}' does not exist"
                ));
            }
        }
        
        // Check for circular references
        foreach (var field in module.Fields)
        {
            if (HasCircularReference(field, module))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Error,
                    "Circular Reference",
                    "Field has a circular parent reference"
                ));
            }
        }
        
        // Field-level validations
        foreach (var field in module.Fields)
        {
            issues.AddRange(ValidateField(field, module));
        }
        
        return issues;
    }
    
    public IEnumerable<ValidationIssue> ValidateField(FormFieldSchema field, FormModuleSchema module)
    {
        var issues = new List<ValidationIssue>();
        
        // Required fields should have error messages
        if (field.Validation?.IsRequired == true)
        {
            if (string.IsNullOrWhiteSpace(field.Validation.RequiredMessageEn))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Warning,
                    "Missing Error Message",
                    "Required field should have an error message"
                ));
            }
        }
        
        // Fields should have labels
        if (string.IsNullOrWhiteSpace(field.LabelEn) && field.FieldType != "Divider")
        {
            issues.Add(new ValidationIssue(
                field.Id,
                ValidationSeverity.Warning,
                "Missing Label",
                "Field has no English label"
            ));
        }
        
        // Choice fields should have options
        if (field.FieldType is "DropDown" or "RadioGroup" or "CheckboxList")
        {
            if (field.CodeSetId is null && (field.Options is null || field.Options.Length == 0))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Error,
                    "Missing Options",
                    "Choice field has no options or CodeSet"
                ));
            }
        }
        
        // TypeConfig validation
        if (field.FieldType == "AutoComplete" && field.TypeConfig is AutoCompleteConfig ac)
        {
            if (string.IsNullOrWhiteSpace(ac.DataSourceUrl))
            {
                issues.Add(new ValidationIssue(
                    field.Id,
                    ValidationSeverity.Error,
                    "Missing Data Source",
                    "AutoComplete requires a data source URL"
                ));
            }
        }
        
        return issues;
    }
    
    private bool HasCircularReference(FormFieldSchema field, FormModuleSchema module)
    {
        var visited = new HashSet<string>();
        var current = field;
        
        while (current?.ParentId is not null)
        {
            if (!visited.Add(current.Id))
                return true;
            
            current = module.Fields.FirstOrDefault(f => f.Id == current.ParentId);
        }
        
        return false;
    }
}
```

---

## 13. AI Prompt Templates

This section contains detailed, well-formed prompts to issue to Claude or Gemini to create each component of the editor. Each prompt is self-contained with context, requirements, and expected output format.

---

### Prompt 1: Project Setup and Shared Components

```markdown
# Task: Set Up DynamicForms Visual Editor Blazor Project

## Context
I'm building a visual schema editor for DynamicForms V4, a form builder system. The editor will be a Blazor Server application that allows users to design multi-module form workflows.

## Requirements

### 1. Create Blazor Server Project Structure
- Pages: Dashboard, WorkflowDesigner, ModuleEditor, CodeSetManager, Settings
- Layouts: MainLayout with global navigation
- Components/Shared: Button, IconButton, TextInput, SelectInput, Checkbox, Badge, SearchBox, Toast

### 2. CSS Variables (Design Tokens)
--primary: #6366F1; --success: #10B981; --danger: #EF4444; --warning: #F59E0B;
Include light/dark theme variants.

### 3. MainLayout with Global Navigation
- Fixed left sidebar (64px) with icon navigation
- Nav items: Dashboard, Workflow, Editor, CodeSets, Settings
- Theme toggle at bottom

### Expected Output
1. Complete Program.cs with service registration
2. CSS files with light/dark theme support
3. All shared components
4. MainLayout with working navigation
```

---

### Prompt 2: Editor State Service

```markdown
# Task: Implement EditorStateService for DynamicForms Visual Editor

## Context
Central state management service handling current workflow/module, field selection, clipboard, undo/redo, and real-time validation.

## Interface Requirements
- Properties: CurrentWorkflow, CurrentModule, SelectedFieldId, SelectedField, ValidationIssues
- Events: OnStateChanged, OnFieldSelected, OnModuleChanged
- Operations: LoadModule, UpdateModule, SelectField, AddField, UpdateField, DeleteField, DuplicateField, MoveField
- Clipboard: CopyField, PasteField, HasClipboard
- Undo/Redo: CanUndo, CanRedo, Undo(), Redo()
- **JSON Serialization**: Use `System.Text.Json` with correct polymorphic type configuration for `FieldTypeConfig`.

## Implementation Notes
- Use immutable records with `with` expressions
- Stack-based undo/redo (max 50 items)
- Validate on every module change
- Ensure `JsonImportExportService` is configured with `JsonDerivedType` attributes (or equivalent options) so `FieldTypeConfig` is correctly serialized/deserialized.

### Expected Output
1. IEditorStateService.cs interface
2. EditorStateService.cs implementation
3. IUndoRedoService.cs and UndoRedoService.cs
4. ISchemaValidationService.cs and SchemaValidationService.cs
5. IJsonImportExportService.cs and implementation with polymorphic support.
```

---

### Prompt 3: Module Editor - Left Sidebar

```markdown
# Task: Create Left Sidebar for Module Editor

## Components Required
1. LeftSidebar.razor - Container with tab switching (Fields/Outline/Issues)
2. FieldPalette.razor - Field types organized by category with search
3. OutlineTree.razor - Hierarchical tree view with expand/collapse
4. ValidationIssues.razor - Error/warning list with click-to-select

## Field Categories
- Basic: TextBox, TextArea, Number, Currency
- Choice: DropDown, RadioGroup, CheckboxList, Checkbox
- Date & Time: DatePicker, TimePicker, DateTimePicker
- Advanced: FileUpload, DataGrid, AutoComplete
- Layout: Section, Panel, Divider, Label

### Expected Output
All components with CSS styles
```

---

### Prompt 4: Properties Panel

```markdown
# Task: Create Properties Panel for Module Editor

## Sections Required
1. General - ID, Type, Order
2. Labels - BilingualInput for Label, Description, Help, Placeholder (EN/FR tabs)
3. Options - InlineOptionsEditor or CodeSetPicker
4. Hierarchy - Parent selector, RelationshipType
5. Validation - Required, min/max length, pattern, error messages
6. Conditional Logic - Rules list, "Add Rule" button
7. Computed Value - Formula display, "Edit Formula" button
8. Layout - WidthClass, IsVisible, IsReadOnly
9. Accessibility - ARIA labels, role

## Key Components
- PropertySection.razor (collapsible wrapper)
- BilingualInput.razor (EN/FR tab switcher)
- InlineOptionsEditor.razor (grid: value, labelEN, labelFR, delete)
- CodeSetPicker.razor (dropdown with preview)

## Dependency Injection
- Inject `ICodeSetProvider` into `CodeSetPicker` to populate available CodeSets.

### Expected Output
All section components and supporting components
```

---

### Prompt 5: Condition Builder Modal

```markdown
# Task: Create Condition Builder Modal

## Schema Classes
ConditionalRule with: Id, Description, TargetFieldId, Action, Condition, Priority, IsActive
Condition (recursive): Simple (Field, Operator, Value) or Complex (LogicalOp, Conditions[])

## Components Required
1. ConditionBuilderModal.razor - Modal container
2. ConditionGroup.razor - AND/OR/NOT toggle with nested conditions
3. ConditionRow.razor - Field selector, operator, value input

## Features
- Nested groups support
- Cross-module field picker (ModuleKey.FieldId format)
- Action configuration (show/hide/enable/disable/setRequired)
- Rule metadata (ID, priority, active)

### Expected Output
All modal components with mutable model classes for editing
```

---

### Prompt 6: Workflow Designer Canvas

```markdown
# Task: Create Workflow Designer Canvas

## Node Types
1. WfNodeStart - Green header, trigger config, bottom handle only
2. WfNodeModule - White, edit button, step badge, top/bottom handles
3. WfNodeDecision - Diamond shape, yellow, condition summary, all handles
4. WfNodeEnd - Gray, completion action, top handle only

## Components Required
1. WorkflowCanvas.razor - Grid background, pan/zoom
2. WfNode*.razor - Each node type
3. WfConnection.razor - SVG bezier curves with arrows
4. WfZoomControls.razor - +, -, fit-to-view
5. WfMinimap.razor - Overview with viewport
6. WorkflowPropertiesPanel.razor - Node properties editor

## Logic Requirements
1. **Layout Persistence:** Serialize node positions (X/Y) and visual connections to `FormWorkflowSchema.ExtendedProperties`.
2. **Graph Logic:** Implement a `WorkflowGraphService` that compiles the visual graph into `WorkflowRules` (e.g., converting "Decision -> Yes -> Module B" into a conditional navigation rule).
3. **Simulation:** The "Simulate" button should be a visual stub for now.

### Expected Output
Complete workflow designer with node rendering, connections, layout persistence logic, and basic graph compiler service.
```

---

### Prompt 7: CodeSet Manager

```markdown
# Task: Create CodeSet Manager

## Layout
- Left: CodeSet list (280px) with search and "New" button
- Right: CodeSet editor with tabs (Configuration, Items, JSON, Usage)

## Components Required
1. CodeSetSidebar.razor - List with search/filter
2. CodeSetEditorPanel.razor - Toolbar and tabs
3. CodeSetConfigTab.razor - Code, category, display names
4. CodeSetItemsTab.razor - Table with drag-to-reorder
5. CodeSetJsonTab.razor - Read-only preview with copy
6. CodeSetUsageTab.razor - Fields using this CodeSet

## Features
- CSV import functionality
- JSON export
- Inline table editing

### Expected Output
Complete CodeSet management interface
```

---

### Phase 2 Checklist (continued)
- [ ] OutlineTree showing hierarchy
- [ ] Field selection syncing
- [ ] PropertiesPanel with basic sections
- [ ] Add/Delete/Move field operations
- [ ] Validation issues displaying

### Phase 3 Checklist
- [ ] InlineOptionsEditor complete
- [ ] CodeSetPicker working
- [ ] ConditionBuilderModal functional
- [ ] FormulaEditorModal functional
- [ ] All TypeConfig editors (AutoComplete, DataGrid, FileUpload, Date)
- [ ] Cross-field validation modal
- [ ] All property sections complete

### Phase 4 Checklist
- [ ] WorkflowCanvas rendering
- [ ] All node types implemented
- [ ] SVG connections drawing
- [ ] Node selection working
- [ ] Properties panel for workflow
- [ ] Navigation to Module Editor
- [ ] Layout data saving/loading correctly
- [ ] Graph-to-Rule compilation working

### Phase 5 Checklist
- [ ] CodeSet CRUD operations
- [ ] Items table editing
- [ ] CSV import working
- [ ] JSON export working
- [ ] Usage tracking displayed

### Phase 6 Checklist
- [ ] Undo/Redo functional
- [ ] Clipboard operations working
- [ ] Keyboard shortcuts implemented
- [ ] All validations running
- [ ] JSON import/export for modules
- [ ] Performance optimized
- [ ] All manual tests passing

---

## Prompt 8: Form Canvas and Field Rendering

```markdown
# Task: Create Form Canvas for Module Editor

## Context
The Form Canvas is the central area of the Module Editor that displays a preview of the form being built. It shows fields organized in their hierarchy with selection and action overlays.

## Requirements

### 1. FormCanvas.razor
Main canvas container.

**Features:**
- Scroll container for long forms
- Form header (title, description from module)
- Render root fields in order
- Empty state when no fields
- Click on canvas background to deselect

**Structure:**
```razor
<div class="form-canvas" @onclick="HandleCanvasClick">
    <div class="form-canvas-content">
        <div class="canvas-header">
            <h2>@Module?.TitleEn</h2>
            @if (!string.IsNullOrEmpty(Module?.DescriptionEn))
            {
                <p>@Module.DescriptionEn</p>
            }
        </div>
        
        @if (RootFields.Any())
        {
            @foreach (var field in RootFields)
            {
                <CanvasField Field="@field" />
            }
        }
        else
        {
            <div class="empty-canvas">
                <i class="bi bi-plus-square-dotted"></i>
                <p>Add your first field from the palette</p>
            </div>
        }
    </div>
</div>

@code {
    [Inject] private IEditorStateService EditorState { get; set; }
    
    private FormModuleRuntime? Runtime => EditorState.CurrentModuleRuntime;
    private IEnumerable<FormFieldNode> RootFields => Runtime?.RootFields
        .OrderBy(f => f.Schema.Order) ?? Enumerable.Empty<FormFieldNode>();
    
    private void HandleCanvasClick(MouseEventArgs e)
    {
        // Deselect if clicking on canvas background
        EditorState.SelectField(null);
    }
}
```

### 2. CanvasField.razor
Renders a single field with its children (recursive).

**Features:**
- Different rendering based on FieldType
- Selection highlight when selected
- Hover shows FieldActions overlay
- Renders children for Section/Panel types

**Structure:**
```razor
<div class="canvas-field @GetFieldClasses()" 
     @onclick="SelectField" 
     @onclick:stopPropagation="true"
     @onmouseenter="() => IsHovered = true"
     @onmouseleave="() => IsHovered = false">
    
    @* Field content based on type *@
    @switch (Field.Schema.FieldType)
    {
        case "Section":
            <CanvasSection Field="@Field" />
            break;
        case "TextBox":
        case "TextArea":
        case "Number":
        case "Currency":
            <CanvasTextInput Field="@Field" />
            break;
        case "DropDown":
        case "RadioGroup":
        case "CheckboxList":
            <CanvasChoiceField Field="@Field" />
            break;
        case "DatePicker":
        case "TimePicker":
        case "DateTimePicker":
            <CanvasDateField Field="@Field" />
            break;
        case "FileUpload":
            <CanvasFileUpload Field="@Field" />
            break;
        case "DataGrid":
            <CanvasDataGrid Field="@Field" />
            break;
        case "Divider":
            <hr class="canvas-divider" />
            break;
        case "Label":
            <CanvasLabel Field="@Field" />
            break;
        default:
            <CanvasGenericField Field="@Field" />
            break;
    }
    
    @* Actions overlay *@
    @if (IsHovered || IsSelected)
    {
        <FieldActions Field="@Field" />
    }
</div>

@code {
    [Parameter] public FormFieldNode Field { get; set; }
    [Inject] private IEditorStateService EditorState { get; set; }
    
    private bool IsHovered { get; set; }
    private bool IsSelected => EditorState.SelectedFieldId == Field.Schema.Id;
    
    private string GetFieldClasses()
    {
        var classes = new List<string> { "field-item" };
        if (IsSelected) classes.Add("selected");
        if (Field.Schema.WidthClass.HasValue)
            classes.Add($"width-{Field.Schema.WidthClass}");
        return string.Join(" ", classes);
    }
    
    private void SelectField() => EditorState.SelectField(Field.Schema.Id);
}
```

### 3. CanvasSection.razor
Renders a section container with children.

```razor
<div class="canvas-section">
    <div class="section-header">
        <i class="bi bi-layout-three-columns"></i>
        <span>@(Field.Schema.LabelEn ?? Field.Schema.Id)</span>
    </div>
    <div class="section-body">
        @if (Field.Children.Any())
        {
            @foreach (var child in Field.Children.OrderBy(c => c.Schema.Order))
            {
                <CanvasField Field="@child" />
            }
        }
        else
        {
            <div class="section-empty">Drop fields here</div>
        }
    </div>
</div>
```

### 4. CanvasTextInput.razor
Preview for text input fields.

```razor
<div class="canvas-input-field">
    <label class="canvas-label">
        @(Field.Schema.LabelEn ?? Field.Schema.Id)
        @if (Field.Schema.Validation?.IsRequired == true)
        {
            <span class="required-marker">*</span>
        }
    </label>
    @if (Field.Schema.FieldType == "TextArea")
    {
        <textarea class="canvas-input" placeholder="@Field.Schema.PlaceholderEn" disabled></textarea>
    }
    else
    {
        <input type="@GetInputType()" class="canvas-input" placeholder="@Field.Schema.PlaceholderEn" disabled />
    }
    @if (!string.IsNullOrEmpty(Field.Schema.HelpEn))
    {
        <div class="canvas-help">@Field.Schema.HelpEn</div>
    }
</div>

@code {
    [Parameter] public FormFieldNode Field { get; set; }
    
    private string GetInputType() => Field.Schema.FieldType switch
    {
        "Number" => "number",
        "Currency" => "text",
        _ => "text"
    };
}
```

### 5. CanvasChoiceField.razor
Preview for dropdown, radio, checkbox fields.

```razor
<div class="canvas-choice-field">
    <label class="canvas-label">
        @(Field.Schema.LabelEn ?? Field.Schema.Id)
        @if (Field.Schema.Validation?.IsRequired == true)
        {
            <span class="required-marker">*</span>
        }
    </label>
    
    @if (Field.Schema.FieldType == "DropDown")
    {
        <select class="canvas-input" disabled>
            <option>@(Field.Schema.PlaceholderEn ?? "Select...")</option>
            @foreach (var option in GetOptions().Take(3))
            {
                <option>@option.LabelEn</option>
            }
            @if (GetOptions().Count() > 3)
            {
                <option>... (@(GetOptions().Count() - 3) more)</option>
            }
        </select>
    }
    else if (Field.Schema.FieldType == "RadioGroup")
    {
        <div class="canvas-radio-group">
            @foreach (var option in GetOptions().Take(4))
            {
                <label class="canvas-radio">
                    <input type="radio" disabled />
                    <span>@option.LabelEn</span>
                </label>
            }
        </div>
    }
    else if (Field.Schema.FieldType == "CheckboxList")
    {
        <div class="canvas-checkbox-group">
            @foreach (var option in GetOptions().Take(4))
            {
                <label class="canvas-checkbox">
                    <input type="checkbox" disabled />
                    <span>@option.LabelEn</span>
                </label>
            }
        </div>
    }
</div>

@code {
    [Parameter] public FormFieldNode Field { get; set; }
    
    private IEnumerable<FieldOption> GetOptions()
    {
        return Field.GetEffectiveOptions() ?? Enumerable.Empty<FieldOption>();
    }
}
```

### 6. FieldActions.razor
Floating action buttons overlay.

```razor
<div class="field-actions">
    <button class="action-btn" title="Settings" @onclick="OpenSettings" @onclick:stopPropagation="true">
        <i class="bi bi-gear"></i>
    </button>
    <button class="action-btn" title="Duplicate" @onclick="Duplicate" @onclick:stopPropagation="true">
        <i class="bi bi-copy"></i>
    </button>
    <button class="action-btn" title="Move Up" @onclick="MoveUp" @onclick:stopPropagation="true" disabled="@(!CanMoveUp)">
        <i class="bi bi-arrow-up"></i>
    </button>
    <button class="action-btn" title="Move Down" @onclick="MoveDown" @onclick:stopPropagation="true" disabled="@(!CanMoveDown)">
        <i class="bi bi-arrow-down"></i>
    </button>
    <button class="action-btn danger" title="Delete" @onclick="Delete" @onclick:stopPropagation="true">
        <i class="bi bi-trash"></i>
    </button>
</div>

@code {
    [Parameter] public FormFieldNode Field { get; set; }
    [Inject] private IEditorStateService EditorState { get; set; }
    
    private bool CanMoveUp => GetSiblingIndex() > 0;
    private bool CanMoveDown => GetSiblingIndex() < GetSiblingCount() - 1;
    
    private int GetSiblingIndex() { /* ... */ }
    private int GetSiblingCount() { /* ... */ }
    
    private void OpenSettings() => EditorState.SelectField(Field.Schema.Id);
    private void Duplicate() => EditorState.DuplicateField(Field.Schema.Id);
    private void MoveUp() => EditorState.MoveField(Field.Schema.Id, MoveDirection.Up);
    private void MoveDown() => EditorState.MoveField(Field.Schema.Id, MoveDirection.Down);
    private void Delete() => EditorState.DeleteField(Field.Schema.Id);
}
```

### Expected Output
1. FormCanvas.razor
2. CanvasField.razor
3. CanvasSection.razor
4. CanvasTextInput.razor
5. CanvasChoiceField.razor
6. CanvasDateField.razor
7. CanvasFileUpload.razor
8. CanvasDataGrid.razor
9. CanvasLabel.razor
10. CanvasGenericField.razor
11. FieldActions.razor
12. CSS styles for all canvas components

### CSS Classes
```css
.form-canvas { flex: 1; overflow: auto; padding: 24px; background: var(--bg-canvas); }
.form-canvas-content { max-width: 800px; margin: 0 auto; background: var(--bg-primary); border-radius: 12px; padding: 32px; box-shadow: var(--shadow-md); }
.canvas-header { margin-bottom: 24px; padding-bottom: 16px; border-bottom: 1px solid var(--border-color); }
.field-item { position: relative; margin-bottom: 16px; padding: 12px; border: 2px dashed transparent; border-radius: 8px; }
.field-item:hover { border-color: var(--primary-muted); }
.field-item.selected { border-color: var(--primary); background: var(--primary-light); }
.canvas-label { display: block; font-weight: 500; margin-bottom: 6px; }
.required-marker { color: var(--danger); }
.canvas-input { width: 100%; padding: 10px; border: 1px solid var(--border-color); border-radius: 6px; background: var(--bg-secondary); }
.canvas-section { border: 1px solid var(--border-color); border-radius: 8px; overflow: hidden; }
.section-header { padding: 12px 16px; background: var(--bg-secondary); font-weight: 600; display: flex; align-items: center; gap: 8px; }
.section-body { padding: 16px; min-height: 60px; }
.field-actions { position: absolute; top: 8px; right: 8px; display: flex; gap: 4px; background: var(--bg-primary); border-radius: 6px; box-shadow: var(--shadow-sm); padding: 4px; }
.action-btn { width: 28px; height: 28px; border: none; background: none; border-radius: 4px; cursor: pointer; }
.action-btn:hover { background: var(--bg-tertiary); }
.action-btn.danger:hover { background: var(--danger-light); color: var(--danger); }
.empty-canvas { text-align: center; padding: 60px 20px; color: var(--text-secondary); }
.empty-canvas i { font-size: 48px; opacity: 0.3; }
```
```

---

## Prompt 9: TypeConfig Editors

```markdown
# Task: Create TypeConfig Editor Modals for DynamicForms Visual Editor

## Context
Certain field types require additional configuration via TypeConfig. I need modal editors for each TypeConfig type.

## TypeConfig Classes
```csharp
public abstract record FieldTypeConfig { }

public record AutoCompleteConfig : FieldTypeConfig
{
    public required string DataSourceUrl { get; init; }
    public string QueryParameter { get; init; } = "q";
    public int MinCharacters { get; init; } = 3;
    public required string ValueField { get; init; }
    public required string DisplayField { get; init; }
    public string? ItemTemplate { get; init; }
}

public record DataGridConfig : FieldTypeConfig
{
    public bool AllowAdd { get; init; } = true;
    public bool AllowEdit { get; init; } = true;
    public bool AllowDelete { get; init; } = true;
    public int? MaxRows { get; init; }
    public string EditorMode { get; init; } = "Modal";  // "Modal" | "Inline"
    public FormFieldSchema[] Columns { get; init; } = [];  // NESTED!
}

public record FileUploadConfig : FieldTypeConfig
{
    public string[] AllowedExtensions { get; init; } = [];
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;
    public bool AllowMultiple { get; init; }
    public bool ScanRequired { get; init; } = true;
}

public record DateConfig : FieldTypeConfig
{
    public bool AllowFuture { get; init; } = true;
    public bool AllowPast { get; init; } = true;
    public string? MinDate { get; init; }  // ISO or "Now", "Now+30d"
    public string? MaxDate { get; init; }
}
```

## Requirements

### 1. TypeConfigModal.razor
Container modal that renders the appropriate editor.

```razor
@if (Field.FieldType == "AutoComplete")
{
    <AutoCompleteConfigEditor Config="@GetConfig<AutoCompleteConfig>()" OnSave="HandleSave" />
}
else if (Field.FieldType == "DataGrid")
{
    <DataGridConfigEditor Config="@GetConfig<DataGridConfig>()" OnSave="HandleSave" />
}
// etc.
```

### 2. AutoCompleteConfigEditor.razor

**Fields:**
- Data Source URL (text input, required)
- Query Parameter (text, default "q")
- Min Characters (number, default 3)
- Value Field (text, required)
- Display Field (text, required)
- Item Template (textarea, optional)

**Layout:**
- Two-column grid for URL and params
- Help text explaining each field

### 3. DataGridConfigEditor.razor

**Sections:**

**Behavior Settings:**
- Allow Add (checkbox)
- Allow Edit (checkbox)
- Allow Delete (checkbox)
- Max Rows (number, optional)
- Editor Mode (dropdown: Modal/Inline)

**Column Configuration:**
This is complex because columns are nested FormFieldSchema objects.

**Features:**
- List of existing columns
- Add Column button
- Edit column (opens nested field editor)
- Delete column
- Reorder columns

**Simplified Column Editor:**
For each column, show:
- ID (text)
- Label EN (text)
- Field Type (dropdown: TextBox, Number, Currency, DropDown, DatePicker, Checkbox)
- Required (checkbox)

### 4. FileUploadConfigEditor.razor

**Fields:**
- Allowed Extensions (chip input or textarea, comma-separated)
- Max File Size (number with MB/KB selector)
- Allow Multiple (checkbox)
- Require Virus Scan (checkbox)

**Preview:**
- Show list of allowed extensions as badges
- Show formatted file size

### 5. DateConfigEditor.razor

**Fields:**
- Allow Future Dates (checkbox)
- Allow Past Dates (checkbox)
- Minimum Date (date picker or special value selector)
- Maximum Date (date picker or special value selector)

**Special Values:**
- "Now" - current date
- "Now+7d" - 7 days from now
- "Now-30d" - 30 days ago
- Custom date

### Expected Output
1. TypeConfigModal.razor
2. AutoCompleteConfigEditor.razor
3. DataGridConfigEditor.razor
4. DataGridColumnEditor.razor
5. FileUploadConfigEditor.razor
6. DateConfigEditor.razor
7. Mutable model classes for editing
8. CSS styles

### Mutable Models
```csharp
public class AutoCompleteConfigModel
{
    public string DataSourceUrl { get; set; } = "";
    public string QueryParameter { get; set; } = "q";
    public int MinCharacters { get; set; } = 3;
    public string ValueField { get; set; } = "";
    public string DisplayField { get; set; } = "";
    public string? ItemTemplate { get; set; }
    
    public static AutoCompleteConfigModel From(AutoCompleteConfig? config);
    public AutoCompleteConfig ToConfig();
}
// Similar for other types
```
```

---

## Prompt 10: Dashboard Page

```markdown
# Task: Create Dashboard Page for DynamicForms Visual Editor

## Context
The Dashboard is the landing page showing recent workflows and quick-start templates.

## Requirements

### 1. Dashboard.razor (Page)

**Layout:**
- Page header with title and "New Workflow" button
- Templates section (horizontal scroll)
- Recent Workflows section (grid)

### 2. TemplateRow.razor
Horizontal scrolling row of template cards.

**Templates to show:**
- Blank Form
- Employee Onboarding
- Approval Workflow
- Expense Report
- Vendor Registration

### 3. TemplateCard.razor
Individual template card.

**Display:**
- Icon
- Title
- Brief description
- "Use Template" button or click whole card

### 4. WorkflowGrid.razor
Grid of recent workflow cards.

**Features:**
- Search/filter box
- Sort dropdown (Recent, Name, Modified)
- Grid of cards (responsive: 1-4 columns)
- "New Workflow" card with dashed border

### 5. WorkflowCard.razor
Card showing workflow summary.

**Display:**
- Title
- Description (truncated)
- Module count badge
- Status badge (Draft/Published)
- Last modified date
- Quick actions on hover (Edit, Duplicate, Delete)

### 6. Mock Data
Create sample workflows for display:
```csharp
private readonly WorkflowSummary[] _mockWorkflows = new[]
{
    new WorkflowSummary(1, "Employee Onboarding", "New hire onboarding process", 4, "draft", DateTime.Now.AddDays(-2)),
    new WorkflowSummary(2, "Expense Claim", "Submit and approve expenses", 3, "published", DateTime.Now.AddDays(-5)),
    new WorkflowSummary(3, "Vendor Registration", "New vendor setup", 2, "draft", DateTime.Now.AddDays(-10)),
};

public record WorkflowSummary(int Id, string Title, string Description, int ModuleCount, string Status, DateTime Modified);
```

### Expected Output
1. Dashboard.razor
2. TemplateRow.razor
3. TemplateCard.razor
4. WorkflowGrid.razor
5. WorkflowCard.razor
6. CSS styles
7. Mock data and models

### CSS Classes
```css
.dashboard { padding: 32px; }
.dashboard-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }
.section-title { font-size: 18px; font-weight: 600; margin-bottom: 16px; }
.templates-row { display: flex; gap: 16px; overflow-x: auto; padding-bottom: 16px; }
.template-card { min-width: 200px; padding: 20px; background: var(--bg-primary); border: 1px solid var(--border-color); border-radius: 12px; cursor: pointer; }
.template-card:hover { border-color: var(--primary); box-shadow: var(--shadow-md); }
.workflows-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 20px; }
.workflow-card { background: var(--bg-primary); border: 1px solid var(--border-color); border-radius: 12px; padding: 20px; cursor: pointer; }
.workflow-card:hover { box-shadow: var(--shadow-md); }
.new-workflow-card { border: 2px dashed var(--border-color); display: flex; flex-direction: column; align-items: center; justify-content: center; min-height: 180px; }
```
```

---

## Appendix A: Field Type to Icon Mapping

```csharp
public static class FieldTypeIcons
{
    public static readonly Dictionary<string, string> Icons = new()
    {
        ["TextBox"] = "bi-input-cursor-text",
        ["TextArea"] = "bi-textarea-t",
        ["Number"] = "bi-123",
        ["Currency"] = "bi-currency-dollar",
        ["DropDown"] = "bi-menu-button-wide",
        ["RadioGroup"] = "bi-ui-radios",
        ["CheckboxList"] = "bi-ui-checks",
        ["Checkbox"] = "bi-check-square",
        ["DatePicker"] = "bi-calendar-event",
        ["TimePicker"] = "bi-clock",
        ["DateTimePicker"] = "bi-calendar-week",
        ["FileUpload"] = "bi-cloud-upload",
        ["DataGrid"] = "bi-table",
        ["AutoComplete"] = "bi-search",
        ["Section"] = "bi-layout-three-columns",
        ["Panel"] = "bi-window",
        ["Divider"] = "bi-dash-lg",
        ["Label"] = "bi-fonts",
        ["Html"] = "bi-code-slash",
    };
    
    public static string Get(string fieldType) => Icons.GetValueOrDefault(fieldType, "bi-question-circle");
}
```

---

## Appendix B: Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+S | Save current module/workflow |
| Ctrl+Z | Undo |
| Ctrl+Y / Ctrl+Shift+Z | Redo |
| Ctrl+C | Copy selected field |
| Ctrl+V | Paste field |
| Ctrl+D | Duplicate selected field |
| Delete | Delete selected field |
| Escape | Close modal / Deselect |
| ↑ / ↓ | Navigate outline tree |
| Enter | Open properties for selected |

### Implementation
```javascript
// wwwroot/js/interop.js
window.registerKeyboardShortcuts = (dotNetRef) => {
    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey || e.metaKey) {
            switch (e.key.toLowerCase()) {
                case 's':
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('Save');
                    break;
                case 'z':
                    e.preventDefault();
                    if (e.shiftKey) {
                        dotNetRef.invokeMethodAsync('Redo');
                    } else {
                        dotNetRef.invokeMethodAsync('Undo');
                    }
                    break;
                case 'y':
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('Redo');
                    break;
                case 'c':
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('Copy');
                    break;
                case 'v':
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('Paste');
                    break;
                case 'd':
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('Duplicate');
                    break;
            }
        } else if (e.key === 'Delete') {
            dotNetRef.invokeMethodAsync('Delete');
        } else if (e.key === 'Escape') {
            dotNetRef.invokeMethodAsync('Escape');
        }
    });
};
```

---

## Appendix C: JSON Serialization Options

```csharp
public static class JsonSerializerOptionsProvider
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = 
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            // Note: Add polymorphic configuration here if not using attributes
        }
    };
    
    // NOTE: To support FieldTypeConfig polymorphism, you must ensure 
    // the base classes have the appropriate [JsonDerivedType] attributes 
    // as defined in the Schema Reference.
}
```

---

## Appendix D: Service Registration

```csharp
// Extensions/ServiceCollectionExtensions.cs

public static class EditorServiceCollectionExtensions
{
    public static IServiceCollection AddDynamicFormsEditor(this IServiceCollection services)
    {
        // Core V4 services
        services.AddDynamicFormsV4();
        
        // Editor services
        services.AddScoped<IEditorStateService, EditorStateService>();
        services.AddScoped<IUndoRedoService, UndoRedoService>();
        services.AddScoped<ISchemaValidationService, SchemaValidationService>();
        services.AddScoped<IJsonImportExportService, JsonImportExportService>();
        services.AddScoped<IWorkflowGraphService, WorkflowGraphService>();
        services.AddScoped<IClipboardService, ClipboardService>();
        services.AddScoped<IToastService, ToastService>();
        services.AddSingleton<IThemeService, ThemeService>();
        
        return services;
    }
}

// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDynamicFormsEditor();

var app = builder.Build();
// ... standard middleware
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

---

*End of Implementation Guide*

```