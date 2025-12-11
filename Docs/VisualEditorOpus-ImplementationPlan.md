# VisualEditorOpus - Implementation Plan

> Comprehensive roadmap to production-ready visual form editor
> Created: December 10, 2025

---

## Executive Summary & Time Estimates

This implementation plan outlines the work required to complete VisualEditorOpus as the designated visual editor for form schemas. Time estimates factor in AI-assisted development (Claude Code).

| Metric | Value |
|--------|-------|
| **Total Duration (AI-Assisted)** | 3-4 weeks |
| **Total Duration (Manual)** | 6-8 weeks |
| **Major Phases** | 7 |
| **Total Tasks** | 35+ |
| **Time Saved with AI** | ~50% |

### AI Acceleration Factor

Using Claude Code for implementation can reduce development time by approximately 40-60%. AI excels at:
- Boilerplate code generation
- Component scaffolding
- CSS styling
- Documentation
- Repetitive patterns

Human oversight is still required for:
- Architecture decisions
- Complex business logic
- Testing
- Integration debugging

---

## Table of Contents

1. [Patterns from DynamicForms.Editor](#patterns-from-dynamicformseditor)
2. [Phase 1: Core Enhancements](#phase-1-core-service-enhancements)
3. [Phase 2: Missing Features](#phase-2-missing-field-types--features)
4. [Phase 3: Workflow Completion](#phase-3-workflow-designer-completion)
5. [Phase 4: Persistence Layer](#phase-4-persistence-layer)
6. [Phase 5: Preview & Runtime](#phase-5-form-preview--runtime)
7. [Phase 6: Polish & Testing](#phase-6-polish--testing)
8. [Phase 7: Documentation](#phase-7-documentation--cleanup)
9. [Timeline & Milestones](#timeline--milestones)

---

## Patterns from DynamicForms.Editor

The following patterns and features from DynamicForms.Editor's `EditorStateService.cs` (664 lines) should be incorporated into VisualEditorOpus:

### Key Patterns to Adopt

| Feature | DynamicForms.Editor | VisualEditorOpus | Action |
|---------|:-------------------:|:----------------:|--------|
| JS Keyboard Shortcuts | ✅ | ❌ | **Add** |
| Toast Feedback | ✅ | ❌ | **Add** |
| System Clipboard | ✅ | ❌ | **Add** |
| JSON Export/Download | ✅ | ⚠️ | Enhance |
| Workflow Layout Save | ✅ | ⚠️ | Enhance |
| Circular Ref Check | ✅ (with error msg) | ✅ (silent) | Improve |
| State Suppression | ✅ | ❌ | **Add** |
| Bilingual Copy Labels | ✅ | ⚠️ | Fix |

### Code Pattern 1: JS Invokable Keyboard Shortcuts

```csharp
// Add to EditorStateService.cs
[JSInvokable] public void OnSave() => _toastService.ShowInfo("Saving...");
[JSInvokable] public void OnUndo() => Undo();
[JSInvokable] public void OnRedo() => Redo();
[JSInvokable] public void OnDelete() { if (SelectedFieldId != null) DeleteField(SelectedFieldId); }
[JSInvokable] public void OnCopy() { if (SelectedFieldId != null) CopyField(SelectedFieldId); }
[JSInvokable] public void OnPaste() { if (HasClipboard) PasteField(SelectedField?.ParentId); }
[JSInvokable] public void OnDuplicate() { if (SelectedFieldId != null) DuplicateField(SelectedFieldId); }
[JSInvokable] public void OnEscape() => SelectField(null);
```

### Code Pattern 2: State Change Suppression for Batching

```csharp
private void SetState(EditorState newState, bool suppressStateChanged = false)
{
    _state = newState;
    if (!suppressStateChanged)
        OnStateChanged?.Invoke();
}

// Usage: Update multiple properties without triggering multiple renders
SetState(_state with { SelectedNodeId = null }, suppressStateChanged: true);
SaveWorkflowLayout(nodes, connections); // This will trigger the state change
```

### Code Pattern 3: Circular Reference Detection

```csharp
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
```

### Code Pattern 4: Workflow Layout Persistence

```csharp
public class WorkflowLayoutData
{
    public List<WorkflowVisualNode> Nodes { get; set; } = new();
    public List<WorkflowVisualConnection> Connections { get; set; } = new();
}

private JsonElement? SerializeWorkflowLayout(WorkflowLayoutData layout)
{
    var json = JsonSerializer.Serialize(layout, JsonSerializerOptionsProvider.Default);
    return JsonDocument.Parse(json).RootElement;
}

private WorkflowLayoutData DeserializeWorkflowLayout(JsonElement? jsonElement)
{
    if (jsonElement == null || jsonElement.Value.ValueKind == JsonValueKind.Null)
        return new WorkflowLayoutData();

    try
    {
        return JsonSerializer.Deserialize<WorkflowLayoutData>(jsonElement.Value)
            ?? new WorkflowLayoutData();
    }
    catch
    {
        // Fallback for old format
        try
        {
            var nodes = JsonSerializer.Deserialize<List<WorkflowVisualNode>>(jsonElement.Value);
            return new WorkflowLayoutData { Nodes = nodes ?? new() };
        }
        catch { return new WorkflowLayoutData(); }
    }
}
```

### Code Pattern 5: System Clipboard Integration

```csharp
public void CopyField(string fieldId)
{
    if (_state.Module is null) return;
    var fieldToCopy = _state.Module.Fields.FirstOrDefault(f => f.Id == fieldId);
    if (fieldToCopy is null) return;

    _clipboardService.CopyField(fieldToCopy);

    // Also copy JSON to system clipboard
    var json = JsonSerializer.Serialize(fieldToCopy, JsonSerializerOptionsProvider.Default);
    _clipboardService.CopyTextToSystemClipboardAsync(json); // Fire and forget

    OnStateChanged?.Invoke();
    _toastService.ShowInfo($"Copied field '{fieldId}'.");
}
```

---

## Phase 1: Core Service Enhancements

**Duration:** 3-4 days (AI-assisted)

### 1.1 EditorStateService Improvements (1.5 days)

- [ ] **Add IToastService integration** (2 hours) `HIGH`
  - Inject toast service and add feedback for all operations (add, delete, duplicate, copy, paste, undo, redo)

- [ ] **Add IClipboardService with system clipboard** (2 hours) `MEDIUM`
  - Create clipboard service that copies field JSON to system clipboard via JS interop

- [ ] **Add JS Invokable keyboard shortcuts** (3 hours) `HIGH`
  - Implement [JSInvokable] methods for OnSave, OnUndo, OnRedo, OnDelete, OnCopy, OnPaste, OnDuplicate, OnEscape

- [ ] **Add SetState with suppressStateChanged** (1 hour) `MEDIUM`
  - Implement state batching to prevent multiple re-renders during complex operations

- [ ] **Enhance circular reference check with toast error** (30 min) `LOW`
  - Show user-friendly error message when circular reference would be created

- [ ] **Add bilingual label support for copy/paste** (30 min) `LOW`
  - Use "Copy of" / "Copie de" and "Pasted" / "Collé" patterns

### 1.2 Import/Export Service (1 day)

- [ ] **Implement ExportModuleJsonAsync** (2 hours) `HIGH`
  - Serialize module to JSON and trigger browser file download

- [ ] **Implement ExportWorkflowJsonAsync** (2 hours) `HIGH`
  - Serialize workflow with node positions and trigger browser file download

- [ ] **Create download JS interop** (1 hour) `MEDIUM`
  - Add JavaScript function to create blob and trigger download

```javascript
// wwwroot/js/file-download.js
window.downloadFile = (fileName, content, mimeType) => {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
```

---

## Phase 2: Missing Field Types & Features

**Duration:** 2-3 days (AI-assisted)

### 2.1 Add Missing Field Types (1.5 days)

- [ ] **Add TimePicker field type** (2 hours) `MEDIUM`
  - Canvas component, palette entry, type config, validation

- [ ] **Add DateTimePicker field type** (2 hours) `MEDIUM`
  - Combined date + time picker with canvas and config

- [ ] **Add Html/RichText field type** (2 hours) `LOW`
  - Static HTML content display field

- [ ] **Add Currency field type** (2 hours) `MEDIUM`
  - Number field with currency formatting options

### 2.2 Field Palette Enhancements (0.5 days)

- [ ] **Add search/filter to field palette** (2 hours) `MEDIUM`
  - Quick search box to filter field types by name

- [ ] **Add collapsible categories** (1 hour) `LOW`
  - Allow users to collapse/expand field type categories

---

## Phase 3: Workflow Designer Completion

**Duration:** 3-4 days (AI-assisted)

### 3.1 Workflow State Management (1.5 days)

- [ ] **Implement workflow layout persistence** (3 hours) `HIGH`
  - Save/load node positions in ExtendedProperties as WorkflowLayoutData JSON

- [ ] **Add workflow node CRUD operations** (2 hours) `HIGH`
  - UpdateWorkflowNode, UpdateWorkflowNodePosition, RemoveNode methods

- [ ] **Add connection management** (2 hours) `HIGH`
  - AddConnection, RemoveConnection methods with duplicate prevention

- [ ] **Workflow undo/redo integration** (2 hours) `MEDIUM`
  - Save workflow state to undo stack before modifications

### 3.2 Workflow UI Enhancements (2 days)

- [ ] **Node drag functionality** (4 hours) `HIGH`
  - Draggable nodes on canvas with position persistence

- [ ] **Connection drawing UI** (4 hours) `HIGH`
  - Click-and-drag to create connections between node handles

- [ ] **Minimap interaction** (3 hours) `MEDIUM`
  - Click minimap to pan, drag viewport rectangle

- [ ] **Node properties panel** (4 hours) `HIGH`
  - Right sidebar properties for selected node (triggers, actions, conditions)

---

## Phase 4: Persistence Layer

**Duration:** 4-5 days (AI-assisted)

### 4.1 Repository Pattern Implementation (2 days)

- [ ] **Create IModuleRepository interface** (2 hours) `HIGH`
  ```csharp
  public interface IModuleRepository
  {
      Task<IEnumerable<FormModuleSchema>> GetAllAsync();
      Task<FormModuleSchema?> GetByIdAsync(int id);
      Task<FormModuleSchema> CreateAsync(FormModuleSchema module);
      Task<FormModuleSchema> UpdateAsync(FormModuleSchema module);
      Task DeleteAsync(int id);
  }
  ```

- [ ] **Create IWorkflowRepository interface** (2 hours) `HIGH`
  - GetAll, GetById, Create, Update, Delete for FormWorkflowSchema

- [ ] **Create ICodeSetRepository interface** (2 hours) `MEDIUM`
  - CRUD operations for CodeSet with caching support

- [ ] **Implement in-memory repositories** (3 hours) `HIGH`
  - Default implementations for development/testing

### 4.2 Database Integration (Optional) (2-3 days)

- [ ] **Entity Framework DbContext setup** (4 hours) `MEDIUM`
  - Create FormsDbContext with Module, Workflow, CodeSet entities

- [ ] **Implement EF repository classes** (4 hours) `MEDIUM`
  - EfModuleRepository, EfWorkflowRepository, EfCodeSetRepository

- [ ] **Add migrations** (2 hours) `MEDIUM`
  - Initial migration for Module, Workflow, CodeSet tables

- [ ] **Auto-save implementation** (4 hours) `LOW`
  - Debounced auto-save with conflict detection

---

## Phase 5: Form Preview & Runtime

**Duration:** 3-4 days (AI-assisted)

### 5.1 Form Preview Implementation (2.5 days)

- [ ] **Create FormPreview component** (6 hours) `HIGH`
  - Render form using DynamicForms.Core runtime with live data binding

- [ ] **Field renderers for preview** (6 hours) `HIGH`
  - Interactive renderers for each field type (TextBox, DropDown, etc.)

- [ ] **Device preview modes** (3 hours) `MEDIUM`
  - Desktop, Tablet, Mobile preview frames

- [ ] **Validation preview** (2 hours) `MEDIUM`
  - Show validation errors in preview mode when fields are invalid

### 5.2 JSON Schema View (1 day)

- [ ] **JSON schema viewer with syntax highlighting** (3 hours) `MEDIUM`
  - Read-only JSON view with copy button and download option

- [ ] **JSON diff view for changes** (4 hours) `LOW`
  - Show what changed between saves (optional)

---

## Phase 6: Polish & Testing

**Duration:** 3-4 days (AI-assisted)

### 6.1 UI/UX Polish (1.5 days)

- [ ] **Loading states and skeletons** (3 hours) `MEDIUM`
  - Add loading indicators for async operations

- [ ] **Error boundaries and fallbacks** (2 hours) `MEDIUM`
  - Graceful error handling for component failures

- [ ] **Keyboard navigation improvements** (3 hours) `LOW`
  - Arrow keys for field navigation, Enter for selection

- [ ] **Accessibility audit and fixes** (4 hours) `MEDIUM`
  - ARIA labels, focus management, screen reader support

### 6.2 Testing (2 days)

- [ ] **Unit tests for services** (4 hours) `HIGH`
  - EditorStateService, ValidationService, UndoRedoService tests

- [ ] **Component tests with bUnit** (4 hours) `MEDIUM`
  - Test key components: FormCanvas, PropertiesPanel, FieldPalette

- [ ] **Integration tests** (4 hours) `MEDIUM`
  - End-to-end flows: create module, add fields, save, export

- [ ] **Manual testing guide updates** (2 hours) `LOW`
  - Update testing guides for all new features

---

## Phase 7: Documentation & Cleanup

**Duration:** 1-2 days (AI-assisted)

### 7.1 Documentation (1 day)

- [ ] **API documentation** (3 hours) `MEDIUM`
  - Document all public service methods and interfaces

- [ ] **User guide** (3 hours) `MEDIUM`
  - How to use the editor: creating forms, workflows, CodeSets

- [ ] **Architecture overview** (2 hours) `LOW`
  - Component hierarchy, service responsibilities, data flow

### 7.2 Code Cleanup (0.5 days)

- [ ] **Remove TODO comments** (2 hours) `MEDIUM`
  - Address all remaining TODOs or convert to tracked issues

- [ ] **Code formatting and consistency** (1 hour) `LOW`
  - Run code formatter, fix warnings, ensure consistent patterns

- [ ] **Remove DynamicForms.Editor project** (30 min) `LOW`
  - Archive or remove the deprecated editor project

---

## Timeline & Milestones

### Phase Duration Summary

| Phase | Duration (AI) | Duration (Manual) | Deliverables |
|-------|---------------|-------------------|--------------|
| Phase 1: Core Enhancements | 3-4 days | 6-8 days | Keyboard shortcuts, toast feedback, export/import |
| Phase 2: Missing Features | 2-3 days | 5-6 days | 4 new field types, palette search |
| Phase 3: Workflow Completion | 3-4 days | 7-8 days | Full workflow designer functionality |
| Phase 4: Persistence | 4-5 days | 8-10 days | Repository pattern, database integration |
| Phase 5: Preview & Runtime | 3-4 days | 6-8 days | Form preview, JSON view |
| Phase 6: Polish & Testing | 3-4 days | 6-8 days | UI polish, unit tests, accessibility |
| Phase 7: Documentation | 1-2 days | 3-4 days | User guide, API docs, cleanup |
| **TOTAL** | **19-26 days (3-4 weeks)** | **41-52 days (6-8 weeks)** | Production-ready editor |

### Key Milestones

| Milestone | Target | Deliverable |
|-----------|--------|-------------|
| **M1: Core Complete** | Week 1 | Keyboard shortcuts, toast feedback, import/export working |
| **M2: Feature Parity** | Week 1.5 | All 18 field types, palette search, field operations complete |
| **M3: Workflow Ready** | Week 2.5 | Full workflow designer with drag-drop, connections, settings |
| **M4: Persistence** | Week 3 | Save/load forms and workflows from database |
| **M5: Preview** | Week 3.5 | Interactive form preview with validation |
| **M6: Production Ready** | Week 4 | Tests passing, docs complete, code cleaned up |

### Time Estimate Assumptions

**AI-Assisted (3-4 weeks):**
- Daily use of Claude Code for boilerplate, components, styling, and documentation
- Human oversight for architecture, testing, and debugging

**Manual (6-8 weeks):**
- Traditional development without AI assistance
- Single developer working full-time

**Variables:**
- Actual time may vary based on: existing codebase familiarity, database choice complexity, testing requirements, and feature scope changes

---

## Quick Reference: Files to Modify

### Services (Src/VisualEditorOpus/Services/)
- `EditorStateService.cs` - Add keyboard shortcuts, toast, clipboard, state suppression
- `IEditorStateService.cs` - Add new method signatures
- Create `IClipboardService.cs` + `ClipboardService.cs`
- Create `IJsonImportExportService.cs` + `JsonImportExportService.cs`

### JavaScript (Src/VisualEditorOpus/wwwroot/js/)
- Create `keyboard-shortcuts.js` - Register global keyboard handlers
- Create `file-download.js` - File download helper
- Create `clipboard.js` - System clipboard interop

### Components to Create
- `Components/Preview/FormPreview.razor` - Form preview mode
- `Components/Preview/FieldRenderers/` - Individual field renderers
- `Components/Workflow/NodePropertiesPanel.razor` - Node properties sidebar

### Models
- `Models/WorkflowLayoutData.cs` - Workflow persistence model

---

*Generated by Claude Code Analysis - December 10, 2025*
