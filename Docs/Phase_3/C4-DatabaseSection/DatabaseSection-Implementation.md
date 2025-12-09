# C.4 Database Section - Implementation Plan

> **Task**: Database Section for Property Panel
> **Location**: `Src/VisualEditorOpus/Components/PropertyPanel/Sections/`
> **Priority**: High
> **Estimated Effort**: 2-3 hours
> **Delegation**: 90% AI

---

## Overview

The Database Section manages SQL column mapping for form fields. It allows configuration of column name, data type, constraints, and provides a preview of the generated DDL.

---

## Schema Reference

From `DynamicForms.Core.V4`:

```csharp
// FormFieldSchema.cs
public string? ColumnName { get; init; }
public string? ColumnType { get; init; }

// FormModuleSchema.cs
public string? TableName { get; init; }
public string? SchemaName { get; init; } = "dbo";
```

---

## Component to Create

### DatabaseSection.razor

```razor
@namespace VisualEditorOpus.Components.PropertyPanel.Sections

<div class="property-section">
    <div class="section-header @(IsExpanded ? "expanded" : "")" @onclick="ToggleExpanded">
        <div class="section-header-left">
            <i class="bi bi-database"></i>
            <span>Database Mapping</span>
        </div>
        <i class="bi bi-chevron-down section-chevron"></i>
    </div>

    @if (IsExpanded)
    {
        <div class="section-content">
            @* Status Badge *@
            <div class="db-status @(IsConfigured ? "configured" : "not-configured")">
                <div class="db-status-icon">
                    <i class="bi bi-@(IsConfigured ? "check-lg" : "database-dash")"></i>
                </div>
                <div class="db-status-content">
                    <div class="db-status-title">@(IsConfigured ? "Column Configured" : "No Column Mapping")</div>
                    <div class="db-status-desc">@StatusDescription</div>
                </div>
            </div>

            @* Column Name *@
            <div class="form-group">
                <label class="form-label">
                    <i class="bi bi-type"></i>
                    Column Name
                </label>
                <input type="text"
                       class="form-input monospace"
                       value="@Field.ColumnName"
                       @oninput="e => UpdateColumnName(e.Value?.ToString())"
                       placeholder="e.g., FirstName" />
                <div class="help-text">
                    <i class="bi bi-info-circle"></i>
                    <span>@(IsConfigured ? "Name of the column in the database table" : "Leave empty if field shouldn't be stored")</span>
                </div>
            </div>

            @if (IsConfigured)
            {
                @* Column Type Grid *@
                <div class="form-group">
                    <label class="form-label">
                        <i class="bi bi-braces"></i>
                        Column Type
                    </label>
                    <div class="column-type-grid">
                        @foreach (var type in ColumnTypes)
                        {
                            <div class="column-type-option @(SelectedColumnType == type.Value ? "selected" : "")"
                                 @onclick="() => SelectColumnType(type.Value)">
                                <div class="column-type-option-icon">@type.Icon</div>
                                <div class="column-type-option-label">@type.Label</div>
                                <div class="column-type-option-type">@type.SqlType</div>
                            </div>
                        }
                    </div>
                </div>

                @* Max Length (for text types) *@
                @if (ShowMaxLength)
                {
                    <div class="form-group">
                        <label class="form-label">
                            <i class="bi bi-rulers"></i>
                            Max Length
                        </label>
                        <div class="input-with-suffix">
                            <input type="number"
                                   class="form-input has-suffix"
                                   value="@MaxLength"
                                   @oninput="e => UpdateMaxLength(e.Value?.ToString())"
                                   placeholder="255" />
                            <span class="input-suffix">characters</span>
                        </div>
                    </div>
                }

                @* Precision and Scale (for decimal types) *@
                @if (ShowPrecision)
                {
                    <div class="form-group">
                        <label class="form-label">
                            <i class="bi bi-rulers"></i>
                            Precision & Scale
                        </label>
                        <div class="input-row">
                            <input type="number"
                                   class="form-input"
                                   value="@Precision"
                                   @oninput="e => UpdatePrecision(e.Value?.ToString())"
                                   placeholder="18" />
                            <input type="number"
                                   class="form-input"
                                   value="@Scale"
                                   @oninput="e => UpdateScale(e.Value?.ToString())"
                                   placeholder="2" />
                        </div>
                        <div class="help-text">
                            <i class="bi bi-info-circle"></i>
                            <span>Precision (total digits) and Scale (decimal places)</span>
                        </div>
                    </div>
                }

                @* Constraints *@
                <div class="form-group">
                    <label class="form-label">
                        <i class="bi bi-sliders"></i>
                        Constraints
                    </label>
                    <div class="toggle-row">
                        <div class="toggle-label">
                            <div class="toggle-icon">
                                <i class="bi bi-asterisk"></i>
                            </div>
                            <div>
                                <div class="toggle-text">Nullable</div>
                                <div class="toggle-desc">Allow NULL values in column</div>
                            </div>
                        </div>
                        <div class="toggle-switch @(IsNullable ? "active" : "")"
                             @onclick="ToggleNullable"></div>
                    </div>
                    <div class="toggle-row">
                        <div class="toggle-label">
                            <div class="toggle-icon">
                                <i class="bi bi-key"></i>
                            </div>
                            <div>
                                <div class="toggle-text">Primary Key</div>
                                <div class="toggle-desc">Column is part of primary key</div>
                            </div>
                        </div>
                        <div class="toggle-switch @(IsPrimaryKey ? "active" : "")"
                             @onclick="TogglePrimaryKey"></div>
                    </div>
                </div>

                @* Default Value *@
                <div class="form-group">
                    <label class="form-label">
                        <i class="bi bi-input-cursor"></i>
                        Default Value
                    </label>
                    <input type="text"
                           class="form-input monospace"
                           value="@DefaultValue"
                           @oninput="e => UpdateDefaultValue(e.Value?.ToString())"
                           placeholder="e.g., GETDATE() or 'Unknown'" />
                </div>

                @* SQL Preview *@
                <div class="sql-preview">
                    <div class="sql-preview-title">
                        <i class="bi bi-code-slash"></i>
                        Generated DDL
                    </div>
                    <div class="sql-preview-code">@((MarkupString)GeneratedDdl)</div>
                </div>
            }

            @* Quick Actions *@
            <div class="quick-actions">
                @if (IsConfigured)
                {
                    <button class="quick-action-btn" @onclick="AutoDetect">
                        <i class="bi bi-magic"></i>
                        Auto-Detect
                    </button>
                    <button class="quick-action-btn" @onclick="CopyDdl">
                        <i class="bi bi-clipboard"></i>
                        Copy DDL
                    </button>
                }
                else
                {
                    <button class="quick-action-btn" @onclick="GenerateFromFieldName">
                        <i class="bi bi-magic"></i>
                        Generate from Field Name
                    </button>
                }
            </div>
        </div>
    }
</div>

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = default!;
    [Parameter] public FormModuleSchema? Module { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private bool IsExpanded { get; set; } = true;
    private bool IsConfigured => !string.IsNullOrWhiteSpace(Field.ColumnName);

    // Parsed from ColumnType
    private string SelectedColumnType { get; set; } = "NVARCHAR";
    private int MaxLength { get; set; } = 255;
    private int Precision { get; set; } = 18;
    private int Scale { get; set; } = 2;
    private bool IsNullable { get; set; } = true;
    private bool IsPrimaryKey { get; set; }
    private string? DefaultValue { get; set; }

    private bool ShowMaxLength => SelectedColumnType is "NVARCHAR" or "VARCHAR" or "CHAR";
    private bool ShowPrecision => SelectedColumnType is "DECIMAL" or "NUMERIC";

    private string StatusDescription => IsConfigured
        ? $"Maps to {Module?.SchemaName ?? "dbo"}.{Module?.TableName ?? "Table"}.{Field.ColumnName}"
        : "Field won't be persisted to database";

    private static readonly ColumnTypeOption[] ColumnTypes = new[]
    {
        new ColumnTypeOption("Aa", "Text", "NVARCHAR", "NVARCHAR"),
        new ColumnTypeOption("123", "Number", "INT", "INT"),
        new ColumnTypeOption("1.5", "Decimal", "DECIMAL", "DECIMAL"),
        new ColumnTypeOption("📅", "Date", "DATETIME2", "DATETIME2"),
        new ColumnTypeOption("✓", "Boolean", "BIT", "BIT"),
        new ColumnTypeOption("📝", "Long Text", "NVARCHAR(MAX)", "NVARCHAR(MAX)")
    };

    protected override void OnParametersSet()
    {
        ParseColumnType();
    }

    private void ToggleExpanded() => IsExpanded = !IsExpanded;

    private void ParseColumnType()
    {
        if (string.IsNullOrWhiteSpace(Field.ColumnType))
        {
            SelectedColumnType = GetDefaultType(Field.Type);
            return;
        }

        var type = Field.ColumnType.ToUpper();

        // Parse NVARCHAR(255) or DECIMAL(18,2)
        if (type.Contains('('))
        {
            var baseType = type.Substring(0, type.IndexOf('('));
            var args = type.Substring(type.IndexOf('(') + 1).TrimEnd(')');

            SelectedColumnType = baseType;

            if (args.Contains(','))
            {
                var parts = args.Split(',');
                Precision = int.TryParse(parts[0], out var p) ? p : 18;
                Scale = int.TryParse(parts[1], out var s) ? s : 2;
            }
            else if (args.ToUpper() != "MAX")
            {
                MaxLength = int.TryParse(args, out var l) ? l : 255;
            }
        }
        else
        {
            SelectedColumnType = type;
        }

        // Parse constraints from extended properties if stored
        // For now, default values
        IsNullable = true;
        IsPrimaryKey = false;
    }

    private string GetDefaultType(FieldType fieldType) => fieldType switch
    {
        FieldType.Text or FieldType.Email or FieldType.Phone => "NVARCHAR",
        FieldType.Number => "INT",
        FieldType.Date or FieldType.DateTime => "DATETIME2",
        FieldType.Time => "TIME",
        FieldType.Checkbox or FieldType.Toggle => "BIT",
        FieldType.TextArea or FieldType.RichText => "NVARCHAR(MAX)",
        FieldType.Select or FieldType.Radio => "NVARCHAR",
        FieldType.MultiSelect => "NVARCHAR(MAX)",
        FieldType.File or FieldType.Image or FieldType.Signature => "VARBINARY(MAX)",
        FieldType.Calculated => "NVARCHAR",
        _ => "NVARCHAR"
    };

    private string GeneratedDdl
    {
        get
        {
            var columnName = Field.ColumnName ?? "ColumnName";
            var typeSpec = BuildTypeSpec();
            var nullable = IsNullable ? "NULL" : "NOT NULL";
            var pk = IsPrimaryKey ? " PRIMARY KEY" : "";
            var defaultSpec = !string.IsNullOrEmpty(DefaultValue) ? $" DEFAULT {DefaultValue}" : "";

            return $"[<span class=\"value\">{columnName}</span>] <span class=\"type\">{typeSpec}</span> <span class=\"constraint\">{nullable}</span>{pk}{defaultSpec}";
        }
    }

    private string BuildTypeSpec()
    {
        return SelectedColumnType switch
        {
            "NVARCHAR" => MaxLength > 0 ? $"NVARCHAR({MaxLength})" : "NVARCHAR(255)",
            "VARCHAR" => MaxLength > 0 ? $"VARCHAR({MaxLength})" : "VARCHAR(255)",
            "NVARCHAR(MAX)" => "NVARCHAR(MAX)",
            "DECIMAL" or "NUMERIC" => $"DECIMAL({Precision},{Scale})",
            _ => SelectedColumnType
        };
    }

    private async Task UpdateColumnName(string? value)
    {
        var updated = Field with { ColumnName = value };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task SelectColumnType(string type)
    {
        SelectedColumnType = type;
        await UpdateColumnType();
    }

    private async Task UpdateMaxLength(string? value)
    {
        MaxLength = int.TryParse(value, out var l) ? l : 255;
        await UpdateColumnType();
    }

    private async Task UpdatePrecision(string? value)
    {
        Precision = int.TryParse(value, out var p) ? p : 18;
        await UpdateColumnType();
    }

    private async Task UpdateScale(string? value)
    {
        Scale = int.TryParse(value, out var s) ? s : 2;
        await UpdateColumnType();
    }

    private async Task UpdateColumnType()
    {
        var typeSpec = BuildTypeSpec();
        var updated = Field with { ColumnType = typeSpec };
        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task ToggleNullable()
    {
        IsNullable = !IsNullable;
        await UpdateColumnType();
    }

    private async Task TogglePrimaryKey()
    {
        IsPrimaryKey = !IsPrimaryKey;
        if (IsPrimaryKey) IsNullable = false;
        await UpdateColumnType();
    }

    private async Task UpdateDefaultValue(string? value)
    {
        DefaultValue = value;
        // Default value could be stored in extended metadata
        StateHasChanged();
    }

    private async Task AutoDetect()
    {
        // Auto-detect type based on field type
        SelectedColumnType = GetDefaultType(Field.Type);

        // Set sensible defaults
        MaxLength = Field.Type switch
        {
            FieldType.Email => 320,
            FieldType.Phone => 20,
            _ => 255
        };

        await UpdateColumnType();
    }

    private async Task GenerateFromFieldName()
    {
        // Generate column name from field label/ID
        var name = Field.LabelEn ?? Field.Id;

        // Convert to PascalCase column name
        var columnName = ToPascalCase(name);

        var updated = Field with
        {
            ColumnName = columnName,
            ColumnType = GetDefaultType(Field.Type)
        };

        await OnFieldChanged.InvokeAsync(updated);
    }

    private async Task CopyDdl()
    {
        var ddl = $"[{Field.ColumnName}] {BuildTypeSpec()} {(IsNullable ? "NULL" : "NOT NULL")}";
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", ddl);
    }

    private static string ToPascalCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Remove special characters and split
        var words = System.Text.RegularExpressions.Regex.Split(text, @"[\s_\-]+");

        return string.Concat(words.Select(w =>
            char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));
    }

    private record ColumnTypeOption(string Icon, string Label, string SqlType, string Value);
}
```

---

## CSS Styles

Add to `database-section.css`:

```css
/* ===== DATABASE SECTION ===== */

/* Status Badge */
.db-status {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 14px 16px;
    background: var(--bg-secondary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    margin-bottom: 16px;
}

.db-status.configured {
    background: var(--success-light);
    border-color: rgba(16, 185, 129, 0.2);
}

.db-status.not-configured {
    background: var(--bg-tertiary);
}

.db-status-icon {
    width: 40px;
    height: 40px;
    border-radius: var(--radius-md);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 18px;
}

.db-status.configured .db-status-icon {
    background: var(--success);
    color: white;
}

.db-status.not-configured .db-status-icon {
    background: var(--bg-tertiary);
    color: var(--text-muted);
}

.db-status-content { flex: 1; }

.db-status-title {
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary);
}

.db-status-desc {
    font-size: 12px;
    color: var(--text-muted);
}

/* Column Type Grid */
.column-type-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 8px;
}

.column-type-option {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    padding: 12px;
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    cursor: pointer;
    transition: all 0.15s;
}

.column-type-option:hover {
    border-color: var(--primary);
    background: var(--bg-secondary);
}

.column-type-option.selected {
    border-color: var(--primary);
    background: var(--primary-light);
}

.column-type-option-icon {
    font-size: 20px;
    color: var(--text-muted);
}

.column-type-option.selected .column-type-option-icon {
    color: var(--primary);
}

.column-type-option-label {
    font-size: 12px;
    font-weight: 500;
    color: var(--text-primary);
}

.column-type-option-type {
    font-size: 10px;
    font-family: monospace;
    color: var(--text-muted);
}

/* Input Row */
.input-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
}

/* Input with Suffix */
.input-with-suffix {
    position: relative;
}

.input-suffix {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 12px;
    color: var(--text-muted);
    font-family: monospace;
}

.form-input.has-suffix {
    padding-right: 80px;
}

/* SQL Preview */
.sql-preview {
    background: var(--bg-tertiary);
    border-radius: var(--radius-md);
    padding: 14px;
    margin-top: 16px;
}

.sql-preview-title {
    font-size: 11px;
    font-weight: 600;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    margin-bottom: 10px;
    display: flex;
    align-items: center;
    gap: 6px;
}

.sql-preview-code {
    font-family: 'Fira Code', 'Cascadia Code', monospace;
    font-size: 12px;
    color: var(--text-primary);
    background: var(--bg-primary);
    padding: 12px 14px;
    border-radius: var(--radius-sm);
    overflow-x: auto;
    white-space: pre;
}

.sql-preview-code .keyword { color: #2563EB; font-weight: 600; }
.sql-preview-code .type { color: var(--primary); }
.sql-preview-code .constraint { color: var(--warning); }
.sql-preview-code .value { color: var(--success); }

/* Monospace Input */
.form-input.monospace {
    font-family: 'Fira Code', 'Cascadia Code', monospace;
}
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the Database Section component for the property panel in my Blazor application.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/PropertyPanel/Sections/
- Schema: DynamicForms.Core.V4 (FormFieldSchema has ColumnName and ColumnType)

## Component: DatabaseSection.razor

This section manages SQL Server column mapping for form fields.

### Features Required:

1. **Status Badge**
   - Configured: Shows green badge with table.column path
   - Not Configured: Shows gray badge, "Field won't be persisted"

2. **Column Name Input**
   - Monospace font
   - Placeholder: "e.g., FirstName"
   - Help text based on state

3. **Column Type Grid** (when configured)
   - 3x2 grid of type options
   - Types: Text (NVARCHAR), Number (INT), Decimal (DECIMAL), Date (DATETIME2), Boolean (BIT), Long Text (NVARCHAR(MAX))
   - Selected state styling

4. **Max Length Input** (for text types)
   - Number input with "characters" suffix
   - Only shown for NVARCHAR/VARCHAR

5. **Precision & Scale** (for decimal types)
   - Two inputs side by side
   - Only shown for DECIMAL/NUMERIC

6. **Constraints Toggles**
   - Nullable toggle (allow NULL)
   - Primary Key toggle

7. **Default Value Input**
   - Supports SQL expressions like GETDATE()

8. **SQL Preview**
   - Shows generated DDL
   - Syntax highlighted: type (purple), constraint (yellow), value (green)

9. **Quick Actions**
   - Auto-Detect: Set type based on field type
   - Copy DDL: Copy to clipboard
   - Generate from Field Name (when not configured)

### Parameters:
```csharp
[Parameter] public FormFieldSchema Field { get; set; }
[Parameter] public FormModuleSchema? Module { get; set; }
[Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }
```

### Schema Reference:
```csharp
public string? ColumnName { get; init; }
public string? ColumnType { get; init; }  // e.g., "NVARCHAR(255)" or "DECIMAL(18,2)"
```

### Key Logic:
- Parse ColumnType to extract base type, length, precision, scale
- Build type spec string from selections
- Default type mapping based on FieldType
- PascalCase conversion for column name generation

Please implement complete, production-ready code with proper CSS styling.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `DatabaseSection-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing unconfigured state display
- Column name input testing
- Column type grid selection testing (all 6 types)
- Max length input testing (for text types)
- Precision & scale inputs testing (for decimal types)
- Nullable toggle testing
- Primary Key toggle testing (should disable nullable)
- Default value input testing
- SQL preview update verification
- Auto-Detect quick action testing
- Copy DDL to clipboard testing
- Generate from Field Name testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Integration with RightSidebar property panel
- Module context provision for table/schema name display
- IJSRuntime injection for clipboard copy
- ColumnType parsing logic verification
- PascalCase conversion testing
- CSS imports for database-section.css

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Unconfigured state shows "No Column Mapping"
- [ ] Configured state shows green badge with table path
- [ ] Column name input uses monospace font
- [ ] Column type grid displays all 6 options
- [ ] Type selection updates SelectedColumnType
- [ ] Max Length input appears for NVARCHAR/VARCHAR
- [ ] Precision/Scale inputs appear for DECIMAL
- [ ] Inputs hidden for other types
- [ ] Nullable toggle works correctly
- [ ] Primary Key toggle works and disables nullable
- [ ] Default value accepts SQL expressions
- [ ] SQL preview updates in real-time
- [ ] DDL syntax highlighting works
- [ ] Auto-Detect sets appropriate type from FieldType
- [ ] Copy DDL copies correct string to clipboard
- [ ] Generate from Field Name creates PascalCase column
- [ ] Section expands/collapses correctly
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] Status shows correctly for configured/unconfigured
- [ ] Column name saves correctly
- [ ] Type grid selection works
- [ ] Max length input appears for text types
- [ ] Precision/scale inputs appear for decimal types
- [ ] Nullable toggle works
- [ ] Primary Key toggle works (and disables nullable)
- [ ] Default value input works
- [ ] SQL preview updates in real-time
- [ ] Auto-Detect sets appropriate type
- [ ] Copy DDL copies to clipboard
- [ ] Generate from Field Name works
- [ ] Dark mode styling correct
- [ ] Section collapses/expands

---

## Notes

- ColumnType stores the full type spec (e.g., "NVARCHAR(255)")
- Consider adding foreign key support in future
- Consider adding index configuration
- Module context provides table/schema name for status display
- Default values should be SQL expressions (quoted for strings)
