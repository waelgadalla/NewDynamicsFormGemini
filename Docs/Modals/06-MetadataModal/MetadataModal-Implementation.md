# MetadataModal Component - Implementation Plan

> **Component**: `MetadataModal.razor`
> **Location**: `Src/VisualEditorOpus/Components/Editor/Modals/MetadataModal.razor`
> **Priority**: Medium
> **Estimated Effort**: 2 hours
> **Depends On**: ModalBase.razor

---

## Overview

MetadataModal allows editing of module-level metadata including bilingual titles, descriptions, instructions, and database configuration. It also displays read-only system information like ID, version, and timestamps.

---

## Core.V4 Schema Reference

### FormModuleSchema Properties to Edit
```csharp
public record FormModuleSchema
{
    // Read-only system info
    public required int Id { get; init; }
    public float Version { get; init; } = 1.0f;
    public DateTime DateCreated { get; init; }
    public DateTime? DateUpdated { get; init; }
    public string? CreatedBy { get; init; }
    public int? OpportunityId { get; init; }

    // Editable titles
    public required string TitleEn { get; init; }
    public string? TitleFr { get; init; }

    // Editable descriptions
    public string? DescriptionEn { get; init; }
    public string? DescriptionFr { get; init; }

    // Editable instructions
    public string? InstructionsEn { get; init; }
    public string? InstructionsFr { get; init; }

    // Database config
    public string? TableName { get; init; }
    public string? SchemaName { get; init; } = "dbo";
}
```

---

## Component API

### Parameters

```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

[Parameter] public FormModuleSchema Module { get; set; } = default!;
[Parameter] public EventCallback<FormModuleSchema> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

---

## UI Sections

### 1. System Information (Read-only)
- Module ID
- Version
- Created Date
- Last Modified Date
- Created By
- Opportunity ID (if applicable)

### 2. Titles
- Title (English) - Required
- Title (French) - Optional

### 3. Descriptions
- Description (English) - Optional textarea
- Description (French) - Optional textarea

### 4. Instructions
- Instructions (English) - Optional textarea
- Instructions (French) - Optional textarea

### 5. Database Configuration
- Table Name - Text input
- Schema Name - Dropdown (dbo, forms, app, custom)

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the MetadataModal component for my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Editor/Modals/
- Depends on: ModalBase.razor

## Schema Reference (DO NOT recreate)
```csharp
public record FormModuleSchema
{
    public required int Id { get; init; }
    public float Version { get; init; } = 1.0f;
    public DateTime DateCreated { get; init; }
    public DateTime? DateUpdated { get; init; }
    public string? CreatedBy { get; init; }
    public required string TitleEn { get; init; }
    public string? TitleFr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? DescriptionFr { get; init; }
    public string? InstructionsEn { get; init; }
    public string? InstructionsFr { get; init; }
    public string? TableName { get; init; }
    public string? SchemaName { get; init; } = "dbo";
}
```

## Files to Create

### MetadataModal.razor
Create modal with sections:
1. System Information (read-only display)
2. Titles (EN/FR text inputs in grid)
3. Descriptions (EN/FR textareas)
4. Instructions (EN/FR textareas)
5. Database Config (table name input, schema dropdown)

### MetadataModal.razor.cs
Parameters:
```csharp
[Parameter] public bool IsOpen { get; set; }
[Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
[Parameter] public FormModuleSchema Module { get; set; } = default!;
[Parameter] public EventCallback<FormModuleSchema> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

Create mutable EditModel class for binding:
```csharp
private class EditModel
{
    public string TitleEn { get; set; } = "";
    public string? TitleFr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionFr { get; set; }
    public string? InstructionsEn { get; set; }
    public string? InstructionsFr { get; set; }
    public string? TableName { get; set; }
    public string SchemaName { get; set; } = "dbo";
}
```

On save, create new FormModuleSchema with updated properties (using `with` keyword).

## UI Structure

```razor
<ModalBase @bind-IsOpen="IsOpen" Title="Module Metadata" Icon="bi-file-earmark-text" Size="ModalSize.Medium">
    <!-- System Info Section -->
    <div class="section-title"><i class="bi bi-info-circle"></i> System Information</div>
    <div class="info-row">
        <span class="info-label">Module ID</span>
        <span class="info-value">@Module.Id</span>
    </div>
    <!-- ... more info rows -->

    <!-- Titles Section -->
    <div class="section-title"><i class="bi bi-type"></i> Titles</div>
    <div class="form-grid">
        <div class="form-group">
            <label class="form-label">Title (English) *</label>
            <input type="text" class="form-input" @bind="model.TitleEn" required>
        </div>
        <div class="form-group">
            <label class="form-label">Title (French)</label>
            <input type="text" class="form-input" @bind="model.TitleFr">
        </div>
    </div>

    <!-- Similar sections for Descriptions, Instructions, Database Config -->

    <FooterContent>
        <button class="btn btn-ghost" @onclick="HandleCancel">Cancel</button>
        <button class="btn btn-primary" @onclick="HandleSave" disabled="@(!IsValid)">
            <i class="bi bi-check-lg"></i> Save Metadata
        </button>
    </FooterContent>
</ModalBase>
```

## Validation
- TitleEn is required (disable save if empty)

## CSS Classes
Use existing classes. Add scoped styles for:
```css
.section-title { /* Section header with icon */ }
.info-row { /* Key-value display row */ }
.info-label { /* Left label */ }
.info-value { /* Right value */ }
```

Please implement with complete, production-ready code.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `MetadataModal-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for opening MetadataModal
- System Information section verification (read-only display)
- Titles section testing (English required, French optional)
- Descriptions section testing (EN/FR textareas)
- Instructions section testing (EN/FR textareas)
- Database configuration testing (table name, schema dropdown)
- Validation testing (TitleEn required)
- Save with updated properties testing
- Cancel without saving testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Integration with module header or settings menu
- EditModel class setup for mutable binding
- FormModuleSchema update using `with` keyword
- Schema dropdown options configuration (dbo, forms, app, custom)
- CSS imports for section styling

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Modal opens with ModalBase (Medium size)
- [ ] System Information section displays read-only data
- [ ] Module ID displays correctly
- [ ] Version displays correctly
- [ ] Created Date displays formatted
- [ ] Last Modified Date displays formatted
- [ ] Created By displays correctly
- [ ] TitleEn input binds correctly
- [ ] TitleFr input binds correctly
- [ ] DescriptionEn textarea binds correctly
- [ ] DescriptionFr textarea binds correctly
- [ ] InstructionsEn textarea binds correctly
- [ ] InstructionsFr textarea binds correctly
- [ ] TableName input binds correctly
- [ ] SchemaName dropdown works with options
- [ ] Save button disabled when TitleEn is empty
- [ ] Save creates updated FormModuleSchema
- [ ] Cancel closes modal without saving
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Read-only fields display correctly
- [ ] All editable fields bind properly
- [ ] TitleEn validation works
- [ ] Save creates updated module schema
- [ ] Cancel closes without saving
- [ ] Dark mode works
