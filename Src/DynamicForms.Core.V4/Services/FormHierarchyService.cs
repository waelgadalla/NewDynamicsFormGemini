using DynamicForms.Core.V4.Runtime;
using DynamicForms.Core.V4.Schemas;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Core.V4.Services;

/// <summary>
/// Implementation of hierarchy service for building and managing form field hierarchies
/// </summary>
public class FormHierarchyService : IFormHierarchyService
{
    private readonly ILogger<FormHierarchyService> _logger;
    private readonly ICodeSetProvider? _codeSetProvider;

    public FormHierarchyService(
        ILogger<FormHierarchyService> logger,
        ICodeSetProvider? codeSetProvider = null)
    {
        _logger = logger;
        _codeSetProvider = codeSetProvider;
    }

    /// <inheritdoc/>
    public async Task<FormModuleRuntime> BuildHierarchyAsync(
        FormModuleSchema schema,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Building hierarchy for module {ModuleId} '{Title}' with {FieldCount} fields",
            schema.Id, schema.TitleEn, schema.Fields.Length);

        var runtime = new FormModuleRuntime
        {
            Schema = schema
        };

        // Phase 1: Create all FormFieldNode instances
        foreach (var field in schema.Fields)
        {
            var node = new FormFieldNode { Schema = field };
            runtime.FieldNodes[field.Id] = node;
        }

        // Phase 2: Build parent-child relationships
        foreach (var node in runtime.FieldNodes.Values)
        {
            if (string.IsNullOrWhiteSpace(node.Schema.ParentId))
            {
                // Root field - add to root list
                runtime.RootFields.Add(node);
            }
            else
            {
                // Has a parent - establish relationship
                if (runtime.FieldNodes.TryGetValue(node.Schema.ParentId, out var parent))
                {
                    node.Parent = parent;
                    parent.Children.Add(node);
                }
                else
                {
                    // Parent doesn't exist - log warning and make it a root field
                    _logger.LogWarning(
                        "Field '{FieldId}' references non-existent parent '{ParentId}'. Making it a root field.",
                        node.Schema.Id, node.Schema.ParentId);
                    runtime.RootFields.Add(node);
                }
            }
        }

        // Phase 2.5: Resolve CodeSets if provider is available
        if (_codeSetProvider != null)
        {
            await ResolveCodeSetsAsync(runtime, cancellationToken);
        }

        // Phase 3: Sort root fields by Order
        runtime.RootFields.Sort((a, b) => a.Schema.Order.CompareTo(b.Schema.Order));

        // Sort children by Order (recursively)
        SortChildrenRecursive(runtime.RootFields);

        // Phase 4: Calculate metrics
        var metrics = CalculateMetrics(schema);
        runtime.GetType().GetProperty(nameof(FormModuleRuntime.Metrics))!
            .SetValue(runtime, metrics);

        _logger.LogDebug("Hierarchy built successfully: {TotalFields} fields, {RootFields} roots, max depth {MaxDepth}",
            metrics.TotalFields, metrics.RootFields, metrics.MaxDepth);

        return runtime;
    }

    /// <inheritdoc/>
    public HierarchyValidationResult ValidateHierarchy(FormModuleSchema schema)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        var fieldIds = new HashSet<string>();

        foreach (var field in schema.Fields)
        {
            // Check for duplicate IDs
            if (!fieldIds.Add(field.Id))
            {
                errors.Add($"Duplicate field ID: '{field.Id}'");
                continue;
            }

            // Check for self-referencing
            if (field.ParentId == field.Id)
            {
                errors.Add($"Field '{field.Id}' references itself as parent");
            }

            // Check for empty field ID
            if (string.IsNullOrWhiteSpace(field.Id))
            {
                errors.Add("Field has empty or null ID");
            }
        }

        // Check for orphaned fields (parent doesn't exist)
        foreach (var field in schema.Fields)
        {
            if (!string.IsNullOrWhiteSpace(field.ParentId) && !fieldIds.Contains(field.ParentId))
            {
                warnings.Add($"Field '{field.Id}' references non-existent parent '{field.ParentId}'");
            }
        }

        // Check for potential circular references (more complex check)
        foreach (var field in schema.Fields)
        {
            if (HasCircularReference(field, schema.Fields))
            {
                errors.Add($"Circular reference detected involving field '{field.Id}'");
            }
        }

        return new HierarchyValidationResult(errors, warnings);
    }

    /// <inheritdoc/>
    public FormModuleSchema FixHierarchyIssues(FormModuleSchema schema)
    {
        _logger.LogInformation("Attempting to fix hierarchy issues in module {ModuleId}", schema.Id);

        var validationResult = ValidateHierarchy(schema);
        if (validationResult.IsValid && !validationResult.Warnings.Any())
        {
            _logger.LogDebug("No issues to fix");
            return schema;
        }

        var fieldIds = new HashSet<string>(schema.Fields.Select(f => f.Id));
        var fixedFields = new List<FormFieldSchema>();

        foreach (var field in schema.Fields)
        {
            var fixedField = field;

            // Fix self-referencing
            if (field.ParentId == field.Id)
            {
                _logger.LogWarning("Fixing self-reference in field '{FieldId}'", field.Id);
                fixedField = fixedField with { ParentId = null };
            }

            // Fix orphaned parent references
            if (!string.IsNullOrWhiteSpace(field.ParentId) && !fieldIds.Contains(field.ParentId))
            {
                _logger.LogWarning("Clearing invalid parent reference '{ParentId}' from field '{FieldId}'",
                    field.ParentId, field.Id);
                fixedField = fixedField with { ParentId = null };
            }

            fixedFields.Add(fixedField);
        }

        return schema with { Fields = fixedFields.ToArray(), DateUpdated = DateTime.UtcNow };
    }

    /// <inheritdoc/>
    public HierarchyMetrics CalculateMetrics(FormModuleSchema schema)
    {
        if (schema.Fields.Length == 0)
        {
            return new HierarchyMetrics(0, 0, 0, 0, 0, 0);
        }

        // Build temporary hierarchy to calculate metrics
        var nodes = new Dictionary<string, FormFieldNode>();
        var rootNodes = new List<FormFieldNode>();

        // Create nodes
        foreach (var field in schema.Fields)
        {
            nodes[field.Id] = new FormFieldNode { Schema = field };
        }

        // Build relationships
        foreach (var node in nodes.Values)
        {
            if (string.IsNullOrWhiteSpace(node.Schema.ParentId))
            {
                rootNodes.Add(node);
            }
            else if (nodes.TryGetValue(node.Schema.ParentId, out var parent))
            {
                node.Parent = parent;
                parent.Children.Add(node);
            }
            else
            {
                rootNodes.Add(node); // Orphaned field becomes root
            }
        }

        // Calculate metrics
        var totalFields = schema.Fields.Length;
        var rootFields = rootNodes.Count;
        var maxDepth = rootNodes.Any() ? rootNodes.Max(r => GetMaxDepth(r)) : 0;
        var averageDepth = nodes.Values.Average(n => n.Level);
        var conditionalFields = schema.Fields.Count(f => f.ConditionalRules?.Length > 0);

        // Complexity score calculation
        // Factors: total fields, max depth, conditional fields, average children per parent
        var avgChildren = nodes.Values.Where(n => n.Children.Any()).Average(n => n.Children.Count);
        var complexityScore = (totalFields * 1.0) +
                            (maxDepth * 5.0) +
                            (conditionalFields * 3.0) +
                            (avgChildren * 2.0);

        return new HierarchyMetrics(
            totalFields,
            rootFields,
            maxDepth,
            Math.Round(averageDepth, 2),
            conditionalFields,
            Math.Round(complexityScore, 2)
        );
    }

    #region Private Helper Methods

    private void SortChildrenRecursive(List<FormFieldNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Children.Any())
            {
                node.Children.Sort((a, b) => a.Schema.Order.CompareTo(b.Schema.Order));
                SortChildrenRecursive(node.Children);
            }
        }
    }

    private int GetMaxDepth(FormFieldNode node)
    {
        if (!node.Children.Any())
            return node.Level;

        return node.Children.Max(c => GetMaxDepth(c));
    }

    private bool HasCircularReference(FormFieldSchema field, FormFieldSchema[] allFields)
    {
        var visited = new HashSet<string>();
        var current = field;

        while (current != null && !string.IsNullOrWhiteSpace(current.ParentId))
        {
            if (!visited.Add(current.Id))
            {
                // We've seen this ID before - circular reference
                return true;
            }

            current = allFields.FirstOrDefault(f => f.Id == current.ParentId);
        }

        return false;
    }

    private async Task ResolveCodeSetsAsync(FormModuleRuntime runtime, CancellationToken cancellationToken)
    {
        if (_codeSetProvider == null)
            return;

        // We need to check for RequiresCodeSetResolution, but that method was on FormFieldSchema in V2.
        // In V3, we need to ensure we check for CodeSetId presence.
        // Since we haven't ported the helper method to V3 Schema yet, we'll do the check inline.
        
        var fieldsNeedingResolution = runtime.FieldNodes.Values
            .Where(node => node.Schema.CodeSetId.HasValue && (node.Schema.Options == null || node.Schema.Options.Length == 0))
            .ToList();

        if (!fieldsNeedingResolution.Any())
        {
            _logger.LogDebug("No fields require CodeSet resolution");
            return;
        }

        _logger.LogDebug("Resolving CodeSets for {Count} fields", fieldsNeedingResolution.Count);

        foreach (var node in fieldsNeedingResolution)
        {
            try
            {
                var options = await _codeSetProvider.GetCodeSetAsFieldOptionsAsync(
                    node.Schema.CodeSetId!.Value, 
                    cancellationToken);

                if (options != null && options.Length > 0)
                {
                    node.ResolvedOptions = options;
                    _logger.LogDebug("Resolved CodeSet {CodeSetId} for field '{FieldId}': {OptionCount} options",
                        node.Schema.CodeSetId, node.Schema.Id, options.Length);
                }
                else
                {
                    _logger.LogWarning("CodeSet {CodeSetId} for field '{FieldId}' returned no options",
                        node.Schema.CodeSetId, node.Schema.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve CodeSet {CodeSetId} for field '{FieldId}'",
                    node.Schema.CodeSetId, node.Schema.Id);
            }
        }
    }

    #endregion
}
