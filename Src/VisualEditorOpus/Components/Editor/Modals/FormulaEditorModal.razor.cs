using Microsoft.AspNetCore.Components;
using System.Data;
using System.Text.RegularExpressions;
using DynamicForms.Core.V4.Schemas;

namespace VisualEditorOpus.Components.Editor.Modals;

/// <summary>
/// Code-behind for the FormulaEditorModal component.
/// Provides a formula editor for creating computed field expressions.
/// </summary>
public partial class FormulaEditorModal : ComponentBase
{
    private static readonly string[] Operators = { "+", "-", "*", "/", "%", "(", ")" };
    private static readonly string[] QuickFunctions = { "IF", "SUM", "AVG", "ROUND" };

    private static readonly HashSet<string> AllowedFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "SUM", "AVG", "MIN", "MAX", "ROUND", "FLOOR", "CEIL", "ABS",
        "IF", "CONCAT", "DATEDIFF", "NOW", "COALESCE", "ISNULL"
    };

    private static readonly List<FunctionInfo> Functions = new()
    {
        new("SUM", "SUM(a, b)", "Add multiple values"),
        new("AVG", "AVG(a, b)", "Calculate average"),
        new("MIN", "MIN(a, b)", "Get minimum value"),
        new("MAX", "MAX(a, b)", "Get maximum value"),
        new("ROUND", "ROUND(value, decimals)", "Round to decimal places"),
        new("FLOOR", "FLOOR(value)", "Round down"),
        new("CEIL", "CEIL(value)", "Round up"),
        new("ABS", "ABS(value)", "Absolute value"),
        new("IF", "IF(condition, trueVal, falseVal)", "Conditional value"),
        new("CONCAT", "CONCAT(str1, str2)", "Join strings"),
        new("DATEDIFF", "DATEDIFF(date1, date2)", "Days between dates"),
        new("NOW", "NOW()", "Current date/time"),
        new("COALESCE", "COALESCE(val1, val2)", "First non-null value"),
        new("ISNULL", "ISNULL(value)", "Check if null")
    };

    private ElementReference editorRef;
    private string formulaText = string.Empty;
    private string fieldSearchQuery = string.Empty;
    private bool isValid;
    private string? validationError;
    private string? previewResult;
    private string previewType = "Number";
    private string[] dependencies = Array.Empty<string>();

    /// <summary>
    /// Gets or sets whether the modal is open.
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>
    /// Event callback for two-way binding of IsOpen.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsOpenChanged { get; set; }

    /// <summary>
    /// Existing formula to edit (null for new formula).
    /// </summary>
    [Parameter]
    public ComputedFormula? ExistingFormula { get; set; }

    /// <summary>
    /// Callback when the formula is saved.
    /// </summary>
    [Parameter]
    public EventCallback<ComputedFormula> OnSave { get; set; }

    /// <summary>
    /// Callback when the modal is cancelled.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// The current module for field references.
    /// </summary>
    [Parameter]
    public FormModuleSchema CurrentModule { get; set; } = default!;

    /// <summary>
    /// Sample data for preview evaluation.
    /// </summary>
    [Parameter]
    public Dictionary<string, object?>? SampleData { get; set; }

    private bool CanSave => isValid && !string.IsNullOrWhiteSpace(formulaText);

    private IEnumerable<FormFieldSchema> FilteredFields =>
        CurrentModule?.Fields?
            .Where(f => string.IsNullOrEmpty(fieldSearchQuery) ||
                        f.Id.Contains(fieldSearchQuery, StringComparison.OrdinalIgnoreCase))
        ?? Enumerable.Empty<FormFieldSchema>();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (IsOpen)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        if (ExistingFormula != null)
        {
            formulaText = ExistingFormula.Expression;
        }
        else
        {
            formulaText = string.Empty;
        }

        fieldSearchQuery = string.Empty;
        Validate();
        UpdatePreview();
    }

    private void InsertText(string text)
    {
        // Simple append - in a more advanced implementation, we'd use JS interop
        // to insert at cursor position
        formulaText += text;
        OnFormulaChanged();
    }

    private void InsertField(string fieldId)
    {
        InsertText(fieldId);
    }

    private void ClearFormula()
    {
        formulaText = string.Empty;
        OnFormulaChanged();
    }

    private void OnFormulaChanged()
    {
        Validate();
        ExtractDependencies();
        UpdatePreview();
        StateHasChanged();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(formulaText))
        {
            isValid = false;
            validationError = "Formula cannot be empty";
            return;
        }

        // Check balanced parentheses
        int parenCount = 0;
        foreach (char c in formulaText)
        {
            if (c == '(') parenCount++;
            if (c == ')') parenCount--;
            if (parenCount < 0)
            {
                isValid = false;
                validationError = "Unmatched closing parenthesis";
                return;
            }
        }
        if (parenCount != 0)
        {
            isValid = false;
            validationError = "Unmatched opening parenthesis";
            return;
        }

        // Validate field references
        var fieldIds = CurrentModule?.Fields?.Select(f => f.Id).ToHashSet() ?? new HashSet<string>();
        var referencedFields = ExtractFieldReferences(formulaText);
        var unknownFields = referencedFields.Except(fieldIds).ToList();
        if (unknownFields.Any())
        {
            isValid = false;
            validationError = $"Unknown field(s): {string.Join(", ", unknownFields)}";
            return;
        }

        // Validate function names
        var usedFunctions = ExtractFunctionNames(formulaText);
        var unknownFunctions = usedFunctions.Where(f => !AllowedFunctions.Contains(f)).ToList();
        if (unknownFunctions.Any())
        {
            isValid = false;
            validationError = $"Unknown function(s): {string.Join(", ", unknownFunctions)}";
            return;
        }

        isValid = true;
        validationError = null;
    }

    private string[] ExtractFieldReferences(string formula)
    {
        var fieldIds = CurrentModule?.Fields?.Select(f => f.Id).ToHashSet() ?? new HashSet<string>();
        var regex = new Regex(@"\b([A-Za-z_][A-Za-z0-9_]*)\b");

        return regex.Matches(formula)
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(fieldIds.Contains)
            .Distinct()
            .ToArray();
    }

    private string[] ExtractFunctionNames(string formula)
    {
        var regex = new Regex(@"\b([A-Za-z]+)\s*\(");
        return regex.Matches(formula)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value.ToUpper())
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToArray();
    }

    private void ExtractDependencies()
    {
        dependencies = ExtractFieldReferences(formulaText);
    }

    private void UpdatePreview()
    {
        if (!isValid || SampleData == null || !SampleData.Any())
        {
            previewResult = null;
            return;
        }

        try
        {
            var result = EvaluatePreview(formulaText, SampleData);
            if (result != null)
            {
                previewResult = FormatResult(result);
                previewType = result.GetType().Name;
            }
            else
            {
                previewResult = null;
            }
        }
        catch
        {
            previewResult = null;
        }
    }

    private object? EvaluatePreview(string formula, Dictionary<string, object?> sampleData)
    {
        try
        {
            // Replace field references with sample values
            var expression = formula;
            foreach (var (field, value) in sampleData)
            {
                expression = Regex.Replace(
                    expression,
                    $@"\b{Regex.Escape(field)}\b",
                    value?.ToString() ?? "0"
                );
            }

            // Simple evaluation using DataTable.Compute for basic math
            // Note: This won't handle custom functions like SUM, AVG etc.
            // In a production environment, use a proper expression evaluator like NCalc
            var table = new DataTable();
            var result = table.Compute(expression, null);
            return result;
        }
        catch
        {
            return null;
        }
    }

    private static string FormatResult(object value)
    {
        return value switch
        {
            decimal d => d.ToString("N2"),
            double d => d.ToString("N2"),
            float f => f.ToString("N2"),
            int i => i.ToString("N0"),
            long l => l.ToString("N0"),
            _ => value.ToString() ?? string.Empty
        };
    }

    private void TestFormula()
    {
        // Force preview update
        UpdatePreview();
        StateHasChanged();
    }

    private async Task HandleSave()
    {
        if (!CanSave) return;

        var formula = new ComputedFormula
        {
            Expression = formulaText,
            DependentFieldIds = dependencies.Length > 0 ? dependencies : null
        };

        await OnSave.InvokeAsync(formula);
        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await IsOpenChanged.InvokeAsync(false);
    }

    private static string GetFieldTypeIcon(string fieldType)
    {
        return fieldType switch
        {
            "Number" => "bi-123",
            "Currency" => "bi-currency-dollar",
            "Date" => "bi-calendar",
            "DateTime" => "bi-calendar-event",
            "Text" => "bi-fonts",
            "TextArea" => "bi-text-paragraph",
            "Checkbox" => "bi-check-square",
            "DropDown" => "bi-list",
            "Radio" => "bi-circle",
            "Email" => "bi-envelope",
            "Phone" => "bi-telephone",
            "Url" => "bi-link",
            _ => "bi-input-cursor"
        };
    }

    private record FunctionInfo(string Name, string Syntax, string Description);
}
