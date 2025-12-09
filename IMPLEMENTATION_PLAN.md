# DynamicForms Editor - Implementation Plan

This plan outlines the remaining steps to bring the editor to a professional, release-ready state.

## Strategy
*   **Manual Work:** Focus on complex logic, architectural glue, and runtime safety (The "Brain").
*   **AI Delegation:** Focus on UI component boilerplate, distinct isolation logic, and unit tests (The "Hands").

---

## Phase 3.5: Finishing Advanced Properties (Immediate)

### 1. Accessibility Section (Properties Panel)
*   **Goal:** Replace the placeholder in `PropertiesPanel.razor`.
*   **AI Task (Claude/Gemini):**
    > "Create a `AccessibilitySection.razor` component that edits `AriaLabel`, `AriaRole`, and `TabIndex`. Use the existing `BilingualInput` for the labels. Bind it to `FormFieldSchema`."
*   **Manual Integration:** Ensure these properties are correctly serialized in the JSON export.

### 2. Cross-Field Validation Logic
*   **Goal:** Connect the `CrossFieldValidationModal` to the Schema.
*   **Manual Work:** Implement the logic in `EditorStateService` to add/update `ValidationRule` objects in the `FormModuleSchema` when the modal saves.

---

## Phase 4: Workflow Designer Logic (High Priority)

### 1. Module Management Integration
*   **Goal:** Implement the TODOs in `EditorStateService.cs`.
*   **Manual Work:**
    *   Implement `AddModuleToWorkflow(int moduleId)`: Should create a new `FormModuleSchema` and a corresponding `WorkflowVisualNode`.
    *   Implement `RemoveModuleFromWorkflow`: Should remove both the node and the schema definition, plus clean up any connections.

### 2. Graph Compilation Service
*   **Goal:** Convert visual SVG connections into executable `ConditionalRules`.
*   **Manual Work:**
    *   Develop the `CompileGraph(List<WorkflowVisualNode> nodes, List<WorkflowVisualConnection> connections)` method in `WorkflowGraphService`.
    *   *Logic:* If Node A connects to Node B via a "Yes" path, generate a rule: `IF (NodeA.Result == 'Yes') THEN GoTo(NodeB)`.

### 3. Workflow Simulation
*   **Goal:** Allow basic testing without leaving the editor.
*   **AI Task (Claude/Gemini):**
    > "Create a `WorkflowSimulator.razor` modal component. It should take a `FormWorkflowSchema` as input, render the current step's form (read-only), and allow clicking 'Next' buttons to traverse the rules defined in the schema. Highlight the active node in the `WorkflowCanvas`."

---

## Phase 5: CodeSet Intelligence

### 1. Usage Tracker
*   **Goal:** Show where CodeSets are used.
*   **AI Task (Claude/Gemini):**
    > "Write a service method `FindCodeSetUsages(FormWorkflowSchema workflow, string codeSetId)` that iterates through all modules and fields in the workflow and returns a list of `FieldReference` objects where `Field.CodeSetId == codeSetId`."

### 2. CSV Import Robustness
*   **Goal:** parsing CSVs for CodeSets.
*   **AI Task (Claude/Gemini):**
    > "Implement a `CsvParserService` that takes a raw string, detects headers (Code, LabelEn, LabelFr), and returns a list of `CodeSetItem` objects. Handle missing columns gracefully."

---

## Phase 6: Polish & Performance

### 1. Virtualization
*   **Goal:** Ensure 100+ field forms don't lag.
*   **Manual Work:** Replace standard `foreach` loops in `FormCanvas.razor` and `OutlineTree.razor` with `<Virtualize>` components if performance drops.

### 2. Keyboard Shortcuts & Accessibility
*   **Goal:** professional "Power User" feel.
*   **AI Task (Claude/Gemini):**
    > "Enhance `EditorStateService` to handle `Ctrl+Shift+Up/Down` for moving fields. Add keyboard navigation support to the `OutlineTree` component using `onkeydown` handlers."

---

## Summary of Work Allocation

| Task | Assignee | Complexity |
|------|----------|------------|
| Accessibility UI Component | 🤖 AI | Low |
| **Workflow Graph Logic** | 🧑‍💻 **Manual** | **High** |
| Workflow Simulation UI | 🤖 AI | Medium |
| CodeSet CSV Parsing | 🤖 AI | Low |
| **Cross-Field Validation Logic** | 🧑‍💻 **Manual** | **High** |
| Unit Tests (Services) | 🤖 AI | Low |
| **Performance Tuning** | 🧑‍💻 **Manual** | **Medium** |

## Next Step Recommendation
Start by implementing the **Accessibility Section** to clear the "In Progress" status of Phase 3, then move immediately to the **Workflow Graph Logic** (Phase 4), as that is the core differentiator of this tool.
