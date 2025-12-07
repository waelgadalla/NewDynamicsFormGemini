# DynamicForms V4 Schema & Services Reference

> **Purpose**: This document provides a complete reference for the DynamicForms V4 codebase to enable AI assistants to understand the system and continue development work without requiring all source files.
>
> **Last Updated**: December 4, 2025
>
> **Document Version**: 1.0 (Complete - all runtime classes included)
>
> **Project Goal**: Build a visual schema editor (Blazor Server) that allows users to author, edit, import, export, clone, and save form schemas.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Schema Classes](#schema-classes)
3. [Enums](#enums)
4. [Services & Interfaces](#services--interfaces)
5. [Builders](#builders)
6. [Validation System](#validation-system)
7. [Runtime Classes](#runtime-classes)
8. [Key Patterns & Conventions](#key-patterns--conventions)
9. [JSON Serialization Notes](#json-serialization-notes)
10. [Visual Editor Requirements](#visual-editor-requirements)

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                      FormWorkflowSchema                         │
│  (Orchestrates multiple modules with navigation & branching)    │
├─────────────────────────────────────────────────────────────────┤
│  - Id, Version, TitleEn/Fr, DescriptionEn/Fr                    │
│  - ModuleIds[] (sequence of modules)                            │
│  - WorkflowRules[] (ConditionalRule for navigation)             │
│  - Navigation settings, Workflow settings                       │
└───────────────────────────┬─────────────────────────────────────┘
                            │ references
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      FormModuleSchema                           │
│  (A complete form with fields and validation rules)             │
├─────────────────────────────────────────────────────────────────┤
│  - Id, OpportunityId, Version, TitleEn/Fr                       │
│  - Fields[] (flat array, hierarchy via ParentId)                │
│  - CrossFieldValidations[], CustomValidationRules[]             │
│  - TableName, SchemaName (database mapping)                     │
└───────────────────────────┬─────────────────────────────────────┘
                            │ contains
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      FormFieldSchema                            │
│  (Individual field with all configuration)                      │
├─────────────────────────────────────────────────────────────────┤
│  - Id, FieldType, Order, ParentId, Relationship                 │
│  - Labels, Descriptions, Placeholders (En/Fr)                   │
│  - Validation, Accessibility, ConditionalRules[]                │
│  - Options[] or CodeSetId, TypeConfig (polymorphic)             │
│  - ComputedValue, Layout settings, Database mapping             │
└─────────────────────────────────────────────────────────────────┘
```

### Namespace Structure

```
DynamicForms.Core.V4.Schemas    - All schema record classes
DynamicForms.Core.V4.Enums      - Enumerations
DynamicForms.Core.V4.Services   - Service interfaces and implementations
DynamicForms.Core.V4.Builders   - Fluent builder classes
DynamicForms.Core.V4.Validation - Validation rules and results
DynamicForms.Core.V4.Runtime    - Runtime objects (FormFieldNode, FormModuleRuntime, WorkflowFormData, etc.)
DynamicForms.Core.V4.Extensions - DI extension methods (ServiceCollectionExtensions)
```

---

## Schema Classes

### FormWorkflowSchema

Top-level container for multi-module workflows.

```csharp
public record FormWorkflowSchema
{
    // Identity
    public required int Id { get; init; }
    public int? OpportunityId { get; init; }
    public float Version { get; init; } = 1.0f;
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;

    // Multilingual
    public required string TitleEn { get; init; }
    public string? TitleFr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? DescriptionFr { get; init; }

    // Module Sequence
    public int[] ModuleIds { get; init; } = Array.Empty<int>();

    // Workflow Logic
    public ConditionalRule[]? WorkflowRules { get; init; }  // Navigation/branching rules

    // Settings
    public WorkflowNavigation Navigation { get; init; } = new();
    public WorkflowSettings Settings { get; init; } = new();

    // Extensibility
    public JsonElement? ExtendedProperties { get; init; }
}

public record WorkflowNavigation(
    bool AllowStepJumping = false,
    bool ShowProgress = true,
    bool ShowStepNumbers = true
);

public record WorkflowSettings(
    bool RequireAllModulesComplete = true,
    bool AllowModuleSkipping = false,
    int AutoSaveIntervalSeconds = 300
);
```

### FormModuleSchema

A complete form containing fields and validation rules.

```csharp
public record FormModuleSchema
{
    // Identity
    public required int Id { get; init; }
    public int? OpportunityId { get; init; }
    public float Version { get; init; } = 1.0f;
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
    public DateTime? DateUpdated { get; init; }
    public string? CreatedBy { get; init; }

    // Multilingual Metadata
    public required string TitleEn { get; init; }
    public string? TitleFr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? DescriptionFr { get; init; }
    public string? InstructionsEn { get; init; }
    public string? InstructionsFr { get; init; }

    // Fields (FLAT ARRAY - hierarchy built at runtime via ParentId)
    public FormFieldSchema[] Fields { get; init; } = Array.Empty<FormFieldSchema>();

    // Validation
    public FieldSetValidation[]? CrossFieldValidations { get; init; }
    public ModuleValidationRule[]? CustomValidationRules { get; init; }

    // Database Mapping
    public string? TableName { get; init; }
    public string? SchemaName { get; init; } = "dbo";

    // Extensibility
    public JsonElement? ExtendedProperties { get; init; }

    // Factory Method
    public static FormModuleSchema Create(int id, string titleEn, string? titleFr = null, int? opportunityId = null);
}

public record ModuleValidationRule(
    string RuleId,
    string RuleType,
    JsonElement Configuration
);
```

### FormFieldSchema

The core building block - individual field definition.

```csharp
public record FormFieldSchema
{
    // === Core Identity ===
    public required string Id { get; init; }           // Unique within module
    public required string FieldType { get; init; }    // TextBox, DropDown, Section, DatePicker, FileUpload, etc.
    public int Order { get; init; } = 1;               // Display order (lower = first)
    public float Version { get; init; } = 1.0f;

    // === Hierarchy ===
    public string? ParentId { get; init; }             // null for root fields
    public RelationshipType Relationship { get; init; } = RelationshipType.Container;

    // === Multilingual Text ===
    public string? LabelEn { get; init; }
    public string? LabelFr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? DescriptionFr { get; init; }
    public string? HelpEn { get; init; }
    public string? HelpFr { get; init; }
    public string? PlaceholderEn { get; init; }
    public string? PlaceholderFr { get; init; }

    // === Validation ===
    public FieldValidationConfig? Validation { get; init; }

    // === Accessibility (WCAG) ===
    public AccessibilityConfig? Accessibility { get; init; }

    // === Logic & Computation ===
    public ConditionalRule[]? ConditionalRules { get; init; }
    public ComputedFormula? ComputedValue { get; init; }

    // === Data Source (for dropdowns, radios, etc.) ===
    public int? CodeSetId { get; init; }               // Reference to CodeSetSchema
    public FieldOption[]? Options { get; init; }       // Inline options (used if CodeSetId is null)

    // === Layout & Styling ===
    public int? WidthClass { get; init; }              // CSS grid width (12 = full, 6 = half)
    public string? CssClasses { get; init; }
    public bool IsVisible { get; init; } = true;
    public bool IsReadOnly { get; init; }

    // === Database Mapping ===
    public string? ColumnName { get; init; }           // null = use Id
    public string? ColumnType { get; init; }           // nvarchar, int, datetime, etc.

    // === Type-Specific Configuration (Polymorphic) ===
    public FieldTypeConfig? TypeConfig { get; init; }

    // === Extensibility ===
    public JsonElement? ExtendedProperties { get; init; }

    // Factory Methods
    public static FormFieldSchema CreateTextField(string id, string labelEn, string? labelFr = null, bool isRequired = false, int order = 1);
    public static FormFieldSchema CreateSection(string id, string titleEn, string? titleFr = null, int order = 1);
    public static FormFieldSchema CreateDropDown(string id, string labelEn, FieldOption[] options, string? labelFr = null, bool isRequired = false, int order = 1);
}
```

### FieldTypeConfig (Polymorphic)

Base class for type-specific configuration with JSON polymorphic serialization.

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(AutoCompleteConfig), typeDiscriminator: "autocomplete")]
[JsonDerivedType(typeof(DataGridConfig), typeDiscriminator: "datagrid")]
[JsonDerivedType(typeof(FileUploadConfig), typeDiscriminator: "fileupload")]
[JsonDerivedType(typeof(DateConfig), typeDiscriminator: "date")]
public abstract record FieldTypeConfig { }

public record AutoCompleteConfig : FieldTypeConfig
{
    public required string DataSourceUrl { get; init; }    // API endpoint
    public string QueryParameter { get; init; } = "q";
    public int MinCharacters { get; init; } = 3;
    public required string ValueField { get; init; }       // Property for saved value
    public required string DisplayField { get; init; }     // Property for display
    public string? ItemTemplate { get; init; }             // Handlebars-style template
}

public record DataGridConfig : FieldTypeConfig
{
    public bool AllowAdd { get; init; } = true;
    public bool AllowEdit { get; init; } = true;
    public bool AllowDelete { get; init; } = true;
    public int? MaxRows { get; init; }
    public string EditorMode { get; init; } = "Modal";     // "Modal" | "Inline"
    public FormFieldSchema[] Columns { get; init; } = Array.Empty<FormFieldSchema>();  // NESTED FIELDS!
}

public record FileUploadConfig : FieldTypeConfig
{
    public string[] AllowedExtensions { get; init; } = Array.Empty<string>();
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;  // 10MB
    public bool AllowMultiple { get; init; }
    public bool ScanRequired { get; init; } = true;
}

public record DateConfig : FieldTypeConfig
{
    public bool AllowFuture { get; init; } = true;
    public bool AllowPast { get; init; } = true;
    public string? MinDate { get; init; }                  // ISO 8601 or "Now", "Now+30d"
    public string? MaxDate { get; init; }
}
```

### Supporting Schema Classes

```csharp
// === Field Options (for dropdowns, radios, checkboxes) ===
public record FieldOption(
    string Value,
    string LabelEn,
    string? LabelFr = null,
    bool IsDefault = false,
    int Order = 0
);

// === Validation Configuration ===
public record FieldValidationConfig
{
    public bool IsRequired { get; init; }
    public string? RequiredMessageEn { get; init; }
    public string? RequiredMessageFr { get; init; }
    public int? MinLength { get; init; }
    public int? MaxLength { get; init; }
    public string? Pattern { get; init; }                  // Regex
    public string? PatternMessageEn { get; init; }
    public string? PatternMessageFr { get; init; }
    public double? MinValue { get; init; }
    public double? MaxValue { get; init; }
    public string[]? CustomRuleIds { get; init; }
}

// === Accessibility Configuration ===
public record AccessibilityConfig
{
    public string? AriaLabelEn { get; init; }
    public string? AriaLabelFr { get; init; }
    public string? AriaDescribedBy { get; init; }
    public string? AriaRole { get; init; }
    public bool AriaLive { get; init; }
}

// === Cross-Field Validation ===
public record FieldSetValidation
{
    public required string Type { get; init; }             // "AtLeastOne", "AllOrNone", "MutuallyExclusive"
    public required string[] FieldIds { get; init; }
    public string? ErrorMessageEn { get; init; }
    public string? ErrorMessageFr { get; init; }
}

// === Computed Values ===
public record ComputedFormula
{
    public required string Expression { get; init; }       // e.g., "Quantity * Price"
    public string[]? DependentFieldIds { get; init; }
}
```

### Conditional Logic Classes

```csharp
// === Conditional Rule ===
public record ConditionalRule
{
    public required string Id { get; init; }
    public string? Description { get; init; }

    // Targets (one of these based on action type)
    public string? TargetFieldId { get; init; }            // For field actions
    public int? TargetStepNumber { get; init; }            // For workflow actions (1-based)
    public string? TargetModuleKey { get; init; }          // For module actions

    // Action to perform when condition is true
    // Field: "show", "hide", "enable", "disable", "setRequired", "setOptional"
    // Workflow: "skipStep", "goToStep", "completeWorkflow"
    public required string Action { get; init; }

    // The condition to evaluate
    public required Condition Condition { get; init; }

    // Execution control
    public int Priority { get; init; } = 100;              // Lower = higher priority
    public bool IsActive { get; init; } = true;

    // Organization
    public string? Category { get; init; }
    public string[]? Tags { get; init; }
}

// === Condition (Recursive Structure) ===
public record Condition
{
    // === Simple Condition (Leaf Node) ===
    public string? Field { get; init; }                    // Field ID or "ModuleKey.FieldId"
    public ConditionOperator? Operator { get; init; }
    public object? Value { get; init; }

    // === Complex Condition (Branch Node) ===
    public LogicalOperator? LogicalOp { get; init; }       // And, Or, Not
    public Condition[]? Conditions { get; init; }          // Child conditions

    // Helpers
    [JsonIgnore] public bool IsSimpleCondition => !string.IsNullOrWhiteSpace(Field) && Operator.HasValue;
    [JsonIgnore] public bool IsComplexCondition => LogicalOp.HasValue && Conditions?.Length > 0;
}
```

### CodeSet Classes

```csharp
public record CodeSetSchema
{
    // Identity
    public required int Id { get; init; }
    public required string Code { get; init; }             // e.g., "PROVINCES_CA"
    public float Version { get; init; } = 1.0f;
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
    public DateTime? DateUpdated { get; init; }

    // Multilingual
    public required string NameEn { get; init; }
    public string? NameFr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? DescriptionFr { get; init; }

    // Items
    public CodeSetItem[] Items { get; init; } = Array.Empty<CodeSetItem>();

    // Metadata
    public string? Category { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsSystemManaged { get; init; }
    public string[]? Tags { get; init; }

    // Helper Methods
    public FieldOption[] ToFieldOptions();
    public CodeSetItem? GetItem(string value);
}

public record CodeSetItem
{
    public required string Value { get; init; }
    public required string TextEn { get; init; }
    public string? TextFr { get; init; }
    public bool IsDefault { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; } = true;
    public string? Description { get; init; }
    public string? ParentValue { get; init; }              // For hierarchical CodeSets
    public string? CssClass { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}
```

### Workflow Module Info

```csharp
public record WorkflowModuleInfo
{
    public required int ModuleId { get; init; }
    public string? TitleEn { get; init; }
    public string? TitleFr { get; init; }
    public required int Order { get; init; }               // 0-based position
    public ConditionalBranch? Branch { get; init; }
    public bool IsRequired { get; init; } = true;
    public bool IsSkippable { get; init; } = false;
}

public record ConditionalBranch(
    string ConditionFieldId,
    string Operator,
    string Value,
    int? NextModuleIdIfTrue = null,
    int? NextModuleIdIfFalse = null
);
```

---

## Enums

```csharp
// === Condition Operators ===
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConditionOperator
{
    // Equality
    Equals, NotEquals,

    // Numeric/Date Comparison
    GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual,

    // String/Collection
    Contains, NotContains, StartsWith, EndsWith, In, NotIn,

    // Null/Empty Checks
    IsNull, IsNotNull, IsEmpty, IsNotEmpty
}

// === Logical Operators ===
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogicalOperator
{
    And,    // All sub-conditions must be true
    Or,     // At least one must be true
    Not     // Negates the first sub-condition
}

// === Relationship Types ===
public enum RelationshipType
{
    Container = 0,      // Section/Group containing children
    Conditional = 1,    // Parent value controls child visibility
    Cascade = 2,        // Parent selection filters child options
    Validation = 3      // Parent affects child validation
}
```

---

## Services & Interfaces

### IFormHierarchyService

Transforms flat field arrays into navigable tree structures.

```csharp
public interface IFormHierarchyService
{
    // Build runtime hierarchy from schema
    Task<FormModuleRuntime> BuildHierarchyAsync(FormModuleSchema schema, CancellationToken ct = default);

    // Validate hierarchy structure
    HierarchyValidationResult ValidateHierarchy(FormModuleSchema schema);

    // Auto-fix common issues (orphans, circular refs)
    FormModuleSchema FixHierarchyIssues(FormModuleSchema schema);

    // Calculate complexity metrics
    HierarchyMetrics CalculateMetrics(FormModuleSchema schema);
}

// Implementation: FormHierarchyService
// - Phase 1: Create FormFieldNode for each field
// - Phase 2: Build parent-child relationships via ParentId
// - Phase 2.5: Resolve CodeSets if provider available
// - Phase 3: Sort by Order (root and children recursively)
// - Phase 4: Calculate metrics
```

### IConditionEvaluator

Evaluates conditional logic for visibility, validation, and workflow navigation.

```csharp
public interface IConditionEvaluator
{
    // Multi-module evaluation (workflow scenarios)
    bool Evaluate(Condition condition, WorkflowFormData workflowData);

    // Single-module evaluation (backward compatibility)
    bool Evaluate(Condition condition, Dictionary<string, object?> fieldData, string? moduleKey = null);

    // Evaluate a rule and get the action to perform
    RuleEvaluationResult EvaluateRule(ConditionalRule rule, WorkflowFormData workflowData);

    // Evaluate multiple rules (returns triggered rules in priority order)
    RuleEvaluationResult[] EvaluateRules(ConditionalRule[] rules, WorkflowFormData workflowData);

    // Parse "ModuleKey.FieldId" references
    (string? moduleKey, string fieldId) ParseFieldReference(string fieldReference);
}

public record RuleEvaluationResult
{
    public required ConditionalRule Rule { get; init; }
    public bool IsTriggered { get; init; }
    public string? ActionToPerform => IsTriggered ? Rule.Action : null;
    public string? TargetFieldId => Rule.TargetFieldId;
    public int? TargetStepNumber => Rule.TargetStepNumber;
    public string? TargetModuleKey => Rule.TargetModuleKey;
    public string? ErrorMessage { get; init; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
}

// Implementation: ConditionEvaluator
// - Supports all ConditionOperator values
// - Recursive evaluation for complex (And/Or/Not) conditions
// - Cross-module field references via dot notation
```

### IFormValidationService

Validates form data against field and module rules.

```csharp
public interface IFormValidationService
{
    // Validate entire module
    Task<ValidationResult> ValidateModuleAsync(FormModuleRuntime module, Dictionary<string, object?> formData, CancellationToken ct = default);

    // Validate single field
    Task<ValidationResult> ValidateFieldAsync(FormFieldNode field, object? value, Dictionary<string, object?> formData, CancellationToken ct = default);

    // Register custom validation rules
    void RegisterRule(string ruleId, IValidationRule rule);
}

// Implementation: FormValidationService
// - Built-in rules: required, length, pattern, email
// - Cross-field validations: AtLeastOne, AllOrNone, MutuallyExclusive
// - Extensible via RegisterRule()
```

### ICodeSetProvider

Abstraction for loading CodeSets from various sources.

```csharp
public interface ICodeSetProvider
{
    Task<CodeSetSchema?> GetCodeSetAsync(int codeSetId, CancellationToken ct = default);
    Task<CodeSetSchema?> GetCodeSetByCodeAsync(string code, CancellationToken ct = default);
    Task<CodeSetSchema[]> GetAllCodeSetsAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<CodeSetSchema[]> GetCodeSetsByCategoryAsync(string category, CancellationToken ct = default);
    Task<CodeSetItem[]> GetCodeSetItemsAsync(int codeSetId, CancellationToken ct = default);
    Task<FieldOption[]> GetCodeSetAsFieldOptionsAsync(int codeSetId, CancellationToken ct = default);
    Task<bool> CodeSetExistsAsync(int codeSetId, CancellationToken ct = default);
}

// Implementation: InMemoryCodeSetProvider
// - Dictionary-based storage
// - RegisterCodeSet(), UnregisterCodeSet(), Clear()
// - GetStats() for monitoring
```

### IFormModuleRepository

Persistence abstraction for module schemas.

```csharp
public interface IFormModuleRepository
{
    Task<bool> SaveAsync(FormModuleSchema schema, CancellationToken ct = default);
    Task<FormModuleSchema?> GetByIdAsync(int moduleId, int? opportunityId = null, CancellationToken ct = default);
    Task<FormModuleSchema[]> GetByIdsAsync(int[] moduleIds, int? opportunityId = null, CancellationToken ct = default);
    Task<bool> DeleteAsync(int moduleId, int? opportunityId = null, CancellationToken ct = default);
    Task<bool> ExistsAsync(int moduleId, int? opportunityId = null, CancellationToken ct = default);
}

public record ModuleVersionInfo(int ModuleId, int? OpportunityId, float Version, DateTime DateCreated, string? CreatedBy, bool IsCurrent, int TotalFields, string TitleEn);
public record ModuleSearchCriteria(int? OpportunityId = null, string? SearchText = null, DateTime? CreatedAfter = null, DateTime? CreatedBefore = null, int PageSize = 20, int PageNumber = 1);
public record ModuleSearchResult(FormModuleSchema[] Modules, int TotalCount, int PageNumber, int PageSize);
```

---

## Builders

### FormFieldBuilder

Fluent builder for creating FormFieldSchema instances.

```csharp
public class FormFieldBuilder
{
    public FormFieldBuilder Id(string id);
    public FormFieldBuilder Type(string type);
    public FormFieldBuilder Label(string en, string? fr = null);
    public FormFieldBuilder Required(bool required = true);
    public FormFieldBuilder Length(int? min, int? max);
    public FormFieldBuilder Parent(string parentId);
    public FormFieldBuilder Order(int order);
    public FormFieldBuilder WithAria(string role, string? labelEn = null, string? labelFr = null);
    public FormFieldBuilder AddOption(string value, string textEn, string? textFr = null);
    public FormFieldSchema Build();
}
```

### FormModuleBuilder

Fluent builder for creating FormModuleSchema instances.

```csharp
public class FormModuleBuilder
{
    public static FormModuleBuilder Create(int id, string titleEn);
    public FormModuleBuilder WithTitle(string en, string? fr = null);
    public FormModuleBuilder WithDescription(string en, string? fr = null);
    public FormModuleBuilder WithInstructions(string en, string? fr = null);
    public FormModuleBuilder ForOpportunity(int opportunityId);
    public FormModuleBuilder AddField(Func<FormFieldBuilder, FormFieldBuilder> builder);
    public FormModuleBuilder AddSection(string id, string titleEn, Action<SectionBuilder>? childBuilder = null);
    public FormModuleBuilder RequireOneOf(string[] fieldIds, string? messageEn = null, string? messageFr = null);
    public FormModuleSchema Build();
}

public class SectionBuilder
{
    public SectionBuilder AddText(string id, string labelEn, bool required = false);
    public SectionBuilder AddField(Func<FormFieldBuilder, FormFieldBuilder> builder);
    public IEnumerable<FormFieldSchema> Build();
}
```

---

## Validation System

### IValidationRule

```csharp
public interface IValidationRule
{
    string RuleId { get; }
    Task<ValidationResult> ValidateAsync(FormFieldNode field, object? value, Dictionary<string, object?> formData, CancellationToken ct = default);
}
```

### Built-in Rules

```csharp
// RequiredFieldRule ("required")
// - Checks Validation.IsRequired
// - Uses RequiredMessageEn/Fr or generates default

// LengthValidationRule ("length")
// - Checks MinLength/MaxLength
// - Generates bilingual error messages

// PatternValidationRule ("pattern")
// - Validates against Validation.Pattern regex
// - Uses PatternMessageEn/Fr or generates default

// EmailValidationRule ("email")
// - Regex: ^[^@\s]+@[^@\s]+\.[^@\s]+$
// - Bilingual error messages
```

### Validation Results

```csharp
public record ValidationError(
    string FieldId,
    string ErrorCode,      // "REQUIRED", "MIN_LENGTH", "MAX_LENGTH", "PATTERN_MISMATCH", "INVALID_EMAIL"
    string Message,        // English
    string? MessageFr = null
);

public record ValidationResult(bool IsValid, List<ValidationError> Errors)
{
    public static ValidationResult Success();
    public static ValidationResult Failure(params ValidationError[] errors);
}
```

---

## Runtime Classes

These classes are built at runtime from the schema and provide navigable hierarchy structures.

### FormFieldNode

Runtime representation of a field with hierarchy navigation.

```csharp
namespace DynamicForms.Core.V4.Runtime;

public class FormFieldNode
{
    /// <summary>
    /// The immutable schema definition for this field
    /// </summary>
    public required FormFieldSchema Schema { get; init; }

    /// <summary>
    /// Reference to the parent node in the hierarchy (null for root fields)
    /// </summary>
    public FormFieldNode? Parent { get; set; }

    /// <summary>
    /// Collection of child nodes in the hierarchy
    /// </summary>
    public List<FormFieldNode> Children { get; } = new();

    /// <summary>
    /// Resolved options from CodeSet (if field uses CodeSetId).
    /// Populated during hierarchy building when CodeSet is resolved.
    /// </summary>
    public FieldOption[]? ResolvedOptions { get; set; }

    /// <summary>
    /// Gets the effective options for this field (resolved CodeSet or inline options)
    /// </summary>
    public FieldOption[]? GetEffectiveOptions()
    {
        return ResolvedOptions ?? Schema.Options;
    }

    /// <summary>
    /// Computed depth level in the hierarchy (0 = root, 1 = first level child, etc.)
    /// </summary>
    public int Level => Parent?.Level + 1 ?? 0;

    /// <summary>
    /// Computed full path from root to this node (e.g., "section1.group1.field1")
    /// </summary>
    public string Path => Parent != null ? $"{Parent.Path}.{Schema.Id}" : Schema.Id;

    /// <summary>
    /// Recursively gets all descendant nodes (children, grandchildren, etc.)
    /// </summary>
    public IEnumerable<FormFieldNode> GetAllDescendants()
    {
        foreach (var child in Children)
        {
            yield return child;
            foreach (var descendant in child.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Gets all ancestor nodes (parent, grandparent, etc.) from immediate parent to root
    /// </summary>
    public IEnumerable<FormFieldNode> GetAllAncestors()
    {
        var current = Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public override string ToString() => $"{Schema.FieldType} [{Schema.Id}] at Level {Level}";
}
```

### FormModuleRuntime

Runtime representation of a module with built hierarchy.

```csharp
namespace DynamicForms.Core.V4.Runtime;

public class FormModuleRuntime
{
    /// <summary>
    /// The original immutable schema for this module
    /// </summary>
    public required FormModuleSchema Schema { get; init; }

    /// <summary>
    /// Dictionary of all field nodes indexed by field ID for O(1) lookup
    /// </summary>
    public Dictionary<string, FormFieldNode> FieldNodes { get; init; } = new();

    /// <summary>
    /// List of root-level fields (fields with no parent)
    /// </summary>
    public List<FormFieldNode> RootFields { get; init; } = new();

    /// <summary>
    /// Calculated metrics about the hierarchy structure
    /// </summary>
    public HierarchyMetrics Metrics { get; init; } = new(0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Gets a field node by its ID
    /// </summary>
    public FormFieldNode? GetField(string fieldId)
    {
        return FieldNodes.TryGetValue(fieldId, out var node) ? node : null;
    }

    /// <summary>
    /// Gets all fields in depth-first traversal order
    /// </summary>
    public IEnumerable<FormFieldNode> GetFieldsInOrder()
    {
        foreach (var rootField in RootFields.OrderBy(f => f.Schema.Order))
        {
            yield return rootField;
            foreach (var descendant in rootField.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    public override string ToString() =>
        $"Module '{Schema.TitleEn}' - {Metrics.TotalFields} fields, {Metrics.RootFields} roots, max depth {Metrics.MaxDepth}";
}
```

### WorkflowFormData

Container for multi-module workflow form data, enabling cross-module field references.

```csharp
namespace DynamicForms.Core.V4.Runtime;

public class WorkflowFormData
{
    /// <summary>
    /// Dictionary of module data organized by module key.
    /// Key: Module key (can be numeric ID like "1" or named key like "PersonalInfo")
    /// Value: Dictionary of field values (FieldId => Value)
    /// 
    /// Example structure:
    /// {
    ///   "1": { "first_name": "John", "last_name": "Doe", "age": 25 },
    ///   "PersonalInfo": { "first_name": "John", "last_name": "Doe", "age": 25 },
    ///   "2": { "organization": "Acme Corp", "org_type": "Business" }
    /// }
    /// </summary>
    public Dictionary<string, Dictionary<string, object?>> Modules { get; set; } = new();

    /// <summary>
    /// Current active module key.
    /// Used as fallback when field references don't specify a module.
    /// </summary>
    public string? CurrentModuleKey { get; set; }

    /// <summary>
    /// Gets a field value with optional module scoping.
    /// </summary>
    public object? GetFieldValue(string? moduleKey, string fieldId)
    {
        var targetModule = moduleKey ?? CurrentModuleKey;
        if (targetModule != null && Modules.TryGetValue(targetModule, out var moduleData))
        {
            return moduleData.GetValueOrDefault(fieldId);
        }
        return null;
    }

    /// <summary>
    /// Sets a field value in a specific module.
    /// </summary>
    public void SetFieldValue(string moduleKey, string fieldId, object? value)
    {
        if (!Modules.ContainsKey(moduleKey))
        {
            Modules[moduleKey] = new Dictionary<string, object?>();
        }
        Modules[moduleKey][fieldId] = value;
    }

    public bool HasModule(string moduleKey) => Modules.ContainsKey(moduleKey);

    public Dictionary<string, object?>? GetModuleData(string moduleKey) => Modules.GetValueOrDefault(moduleKey);

    public void SetModuleData(string moduleKey, Dictionary<string, object?> moduleData)
    {
        Modules[moduleKey] = moduleData;
    }

    public int ModuleCount => Modules.Count;
    public IEnumerable<string> ModuleKeys => Modules.Keys;

    /// <summary>
    /// Creates a WorkflowFormData instance from a single module's data.
    /// Useful for single-module scenarios or backward compatibility.
    /// </summary>
    public static WorkflowFormData FromSingleModule(string moduleKey, Dictionary<string, object?> fieldData)
    {
        return new WorkflowFormData
        {
            Modules = new Dictionary<string, Dictionary<string, object?>>
            {
                { moduleKey, fieldData }
            },
            CurrentModuleKey = moduleKey
        };
    }

    public static WorkflowFormData Empty() => new WorkflowFormData();
}
```

### HierarchyMetrics

Statistical metrics about a form module's hierarchy structure.

```csharp
namespace DynamicForms.Core.V4.Runtime;

public record HierarchyMetrics(
    int TotalFields,
    int RootFields,
    int MaxDepth,
    double AverageDepth,
    int ConditionalFields,
    double ComplexityScore
);
```

### HierarchyValidationResult

Result of validating a form module's hierarchy structure.

```csharp
namespace DynamicForms.Core.V4.Runtime;

public record HierarchyValidationResult(
    List<string> Errors,
    List<string> Warnings
)
{
    public bool IsValid => !Errors.Any();

    public static HierarchyValidationResult Success() => new(new(), new());

    public static HierarchyValidationResult WithErrors(params string[] errors)
        => new(new List<string>(errors), new());

    public static HierarchyValidationResult WithWarnings(params string[] warnings)
        => new(new(), new List<string>(warnings));
}

---

## Dependency Injection Setup

### ServiceCollectionExtensions

```csharp
namespace DynamicForms.Core.V4.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all DynamicForms.Core.V4 services to the service collection.
    /// </summary>
    public static IServiceCollection AddDynamicFormsV4(this IServiceCollection services)
    {
        // Core services (stateless, thread-safe singletons)
        services.AddSingleton<IFormHierarchyService, FormHierarchyService>();
        services.AddSingleton<IFormValidationService, FormValidationService>();
        services.AddSingleton<IConditionEvaluator, ConditionEvaluator>();
        services.AddSingleton<ICodeSetProvider, InMemoryCodeSetProvider>();

        // Built-in validation rules
        services.AddSingleton<IValidationRule, RequiredFieldRule>();
        services.AddSingleton<IValidationRule, LengthValidationRule>();
        services.AddSingleton<IValidationRule, PatternValidationRule>();
        services.AddSingleton<IValidationRule, EmailValidationRule>();

        return services;
    }

    /// <summary>
    /// Adds services with a custom repository implementation.
    /// </summary>
    public static IServiceCollection AddDynamicFormsV4<TRepository>(this IServiceCollection services)
        where TRepository : class, IFormModuleRepository
    {
        services.AddDynamicFormsV4();
        services.AddScoped<IFormModuleRepository, TRepository>();
        return services;
    }

    /// <summary>
    /// Adds services with a custom CodeSet provider.
    /// </summary>
    public static IServiceCollection AddDynamicFormsV4WithCodeSetProvider<TCodeSetProvider>(this IServiceCollection services)
        where TCodeSetProvider : class, ICodeSetProvider
    {
        services.AddSingleton<IFormHierarchyService, FormHierarchyService>();
        services.AddSingleton<IFormValidationService, FormValidationService>();
        services.AddSingleton<IConditionEvaluator, ConditionEvaluator>();
        services.AddSingleton<ICodeSetProvider, TCodeSetProvider>();

        services.AddSingleton<IValidationRule, RequiredFieldRule>();
        services.AddSingleton<IValidationRule, LengthValidationRule>();
        services.AddSingleton<IValidationRule, PatternValidationRule>();
        services.AddSingleton<IValidationRule, EmailValidationRule>();

        return services;
    }
}
```

### Usage in Program.cs

```csharp
// Basic setup
builder.Services.AddDynamicFormsV4();

// With custom repository
builder.Services.AddDynamicFormsV4<SqlFormModuleRepository>();

// With custom CodeSet provider
builder.Services.AddDynamicFormsV4WithCodeSetProvider<DatabaseCodeSetProvider>();
```

---

## Key Patterns & Conventions

### 1. Immutable Records

All schema classes are C# `record` types with `init` properties for immutability. Use `with` expressions to create modified copies.

### 2. Bilingual Support

Every user-facing text has En/Fr variants:
- `TitleEn` / `TitleFr`
- `LabelEn` / `LabelFr`
- `DescriptionEn` / `DescriptionFr`
- etc.

### 3. Flat Storage, Runtime Hierarchy

Fields are stored as flat arrays in `FormModuleSchema.Fields`. Hierarchy is established via `ParentId` references and built at runtime by `IFormHierarchyService`.

### 4. Polymorphic Type Config

`FieldTypeConfig` uses System.Text.Json polymorphic serialization with `$type` discriminator:
- `"autocomplete"` → `AutoCompleteConfig`
- `"datagrid"` → `DataGridConfig`
- `"fileupload"` → `FileUploadConfig`
- `"date"` → `DateConfig`

### 5. Cross-Module Field References

Conditions can reference fields in other modules using dot notation:
- `"age"` - field in current module
- `"Step1.age"` - field in module with key "Step1"
- `"1.age"` - field in module with numeric key "1"

### 6. Recursive Conditions

`Condition` is a recursive structure:
- Simple: `{ Field, Operator, Value }`
- Complex: `{ LogicalOp, Conditions[] }`

### 7. Extensibility via JsonElement

`ExtendedProperties` allows arbitrary JSON data without schema changes.

---

## JSON Serialization Notes

### Polymorphic TypeConfig

```json
{
  "Id": "species",
  "FieldType": "AutoComplete",
  "TypeConfig": {
    "$type": "autocomplete",
    "DataSourceUrl": "/api/species/search",
    "ValueField": "Id",
    "DisplayField": "Name"
  }
}
```

### Nested Conditions

```json
{
  "LogicalOp": "And",
  "Conditions": [
    { "Field": "age", "Operator": "LessThan", "Value": 18 },
    { "Field": "province", "Operator": "Equals", "Value": "ON" }
  ]
}
```

### DataGrid with Nested Fields

```json
{
  "Id": "lineItems",
  "FieldType": "DataGrid",
  "TypeConfig": {
    "$type": "datagrid",
    "AllowAdd": true,
    "MaxRows": 10,
    "Columns": [
      { "Id": "description", "FieldType": "TextBox", "LabelEn": "Description" },
      { "Id": "quantity", "FieldType": "Number", "LabelEn": "Qty" },
      { "Id": "price", "FieldType": "Currency", "LabelEn": "Price" }
    ]
  }
}
```

---

## Visual Editor Requirements

### Core Features (MVP)

1. **Schema Navigation**
   - Tree view of Workflow → Modules → Fields (hierarchical)
   - Expand/collapse sections
   - Visual indicators for field types

2. **Property Editing**
   - Edit all field properties
   - Bilingual text editors (En/Fr tabs or side-by-side)
   - Validation config editor
   - Accessibility config editor

3. **Field Management**
   - Add/remove fields
   - Drag-drop reordering
   - Change parent (move in hierarchy)
   - Clone fields

4. **Options Editor**
   - Inline options for dropdowns/radios
   - CodeSet selection and preview

5. **Import/Export**
   - Load from JSON
   - Export to JSON
   - Pretty-print formatting

6. **Save/Persist**
   - Save to repository
   - Version tracking

### Advanced Features (Post-MVP)

1. **Condition Builder UI**
   - Visual builder for simple conditions
   - Nested condition groups (AND/OR/NOT)
   - Cross-module field picker

2. **TypeConfig Editors**
   - Specialized editors for each FieldTypeConfig type
   - AutoComplete: API endpoint tester
   - DataGrid: Column editor with nested field schemas
   - FileUpload: Extension/size configuration
   - Date: Min/max date picker

3. **Computed Formula Editor**
   - Expression builder
   - Field reference picker
   - Formula validation

4. **Workflow Editor**
   - Module sequencing (drag-drop)
   - Workflow rule builder
   - Navigation settings

5. **Schema Validation**
   - Real-time validation feedback
   - Hierarchy validation
   - Circular reference detection

6. **Undo/Redo**
   - Command pattern implementation
   - History navigation

---

## Appendix: Field Types Reference

Based on the schema, these field types are expected:

| FieldType | TypeConfig | Notes |
|-----------|------------|-------|
| TextBox | - | Basic text input |
| TextArea | - | Multi-line text |
| Number | - | Numeric input |
| Currency | - | Money values |
| DropDown | - | Single select (Options or CodeSetId) |
| RadioGroup | - | Single select visual |
| CheckboxList | - | Multi-select |
| Checkbox | - | Single boolean |
| DatePicker | DateConfig | Date selection |
| DateTimePicker | DateConfig | Date + time |
| TimePicker | - | Time only |
| FileUpload | FileUploadConfig | File attachments |
| AutoComplete | AutoCompleteConfig | API-driven lookup |
| DataGrid | DataGridConfig | Repeating rows |
| Section | - | Container/grouping |
| Panel | - | Visual container |
| Divider | - | Visual separator |
| Label | - | Display-only text |
| Html | - | Rich text display |

---

## Sample JSON Schemas

> **NOTE**: Add example JSON schemas here for testing and validation.

### Example: Simple Contact Form Module

```json
{
  "Id": 1,
  "TitleEn": "Contact Information",
  "TitleFr": "Coordonnées",
  "Version": 1.0,
  "Fields": [
    {
      "Id": "contactSection",
      "FieldType": "Section",
      "LabelEn": "Contact Details",
      "LabelFr": "Détails du contact",
      "Order": 1
    },
    {
      "Id": "firstName",
      "FieldType": "TextBox",
      "LabelEn": "First Name",
      "LabelFr": "Prénom",
      "ParentId": "contactSection",
      "Order": 1,
      "Validation": {
        "IsRequired": true,
        "MaxLength": 50
      }
    },
    {
      "Id": "lastName",
      "FieldType": "TextBox",
      "LabelEn": "Last Name",
      "LabelFr": "Nom de famille",
      "ParentId": "contactSection",
      "Order": 2,
      "Validation": {
        "IsRequired": true,
        "MaxLength": 50
      }
    },
    {
      "Id": "email",
      "FieldType": "TextBox",
      "LabelEn": "Email Address",
      "LabelFr": "Adresse courriel",
      "ParentId": "contactSection",
      "Order": 3,
      "Validation": {
        "IsRequired": true,
        "Pattern": "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$",
        "PatternMessageEn": "Please enter a valid email address"
      }
    },
    {
      "Id": "province",
      "FieldType": "DropDown",
      "LabelEn": "Province",
      "LabelFr": "Province",
      "ParentId": "contactSection",
      "Order": 4,
      "CodeSetId": 1,
      "Validation": {
        "IsRequired": true
      }
    }
  ]
}
```

### Example: Field with Conditional Rule

```json
{
  "Id": "parentConsent",
  "FieldType": "Checkbox",
  "LabelEn": "I have parental consent",
  "Order": 5,
  "IsVisible": false,
  "ConditionalRules": [
    {
      "Id": "show-if-minor",
      "Description": "Show consent checkbox if applicant is under 18",
      "TargetFieldId": "parentConsent",
      "Action": "show",
      "Condition": {
        "Field": "age",
        "Operator": "LessThan",
        "Value": 18
      },
      "Priority": 10,
      "IsActive": true
    }
  ]
}
```

### Example: Complex Nested Condition

```json
{
  "Id": "rule-complex",
  "Action": "show",
  "TargetFieldId": "specialField",
  "Condition": {
    "LogicalOp": "And",
    "Conditions": [
      {
        "Field": "age",
        "Operator": "GreaterThanOrEqual",
        "Value": 18
      },
      {
        "LogicalOp": "Or",
        "Conditions": [
          { "Field": "province", "Operator": "Equals", "Value": "ON" },
          { "Field": "province", "Operator": "Equals", "Value": "QC" }
        ]
      }
    ]
  }
}
```

### Example: DataGrid with Nested Columns

```json
{
  "Id": "lineItems",
  "FieldType": "DataGrid",
  "LabelEn": "Line Items",
  "TypeConfig": {
    "$type": "datagrid",
    "AllowAdd": true,
    "AllowEdit": true,
    "AllowDelete": true,
    "MaxRows": 20,
    "EditorMode": "Modal",
    "Columns": [
      {
        "Id": "description",
        "FieldType": "TextBox",
        "LabelEn": "Description",
        "Order": 1,
        "Validation": { "IsRequired": true, "MaxLength": 200 }
      },
      {
        "Id": "quantity",
        "FieldType": "Number",
        "LabelEn": "Quantity",
        "Order": 2,
        "Validation": { "IsRequired": true, "MinValue": 1 }
      },
      {
        "Id": "unitPrice",
        "FieldType": "Currency",
        "LabelEn": "Unit Price",
        "Order": 3,
        "Validation": { "IsRequired": true, "MinValue": 0 }
      }
    ]
  }
}
```

---

## Questions for Wael (To Complete This Document)

1. **Sample JSON**: Can you provide more complete example JSON schemas from your actual usage?

2. **UI Framework**: Are you planning to use a specific Blazor component library (MudBlazor, Radzen, Syncfusion, etc.)?

3. **Existing Work**: Have you started any Blazor components for the editor?

4. **Database**: What storage mechanism will you use for the repository implementation?

5. **Additional Field Types**: Are there other FieldType values beyond the ones listed that you use?

---

*End of Reference Document*
