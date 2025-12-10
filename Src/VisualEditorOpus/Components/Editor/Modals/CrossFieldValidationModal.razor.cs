using Microsoft.AspNetCore.Components;
using DynamicForms.Core.V4.Schemas;

namespace VisualEditorOpus.Components.Editor.Modals;

/// <summary>
/// Code-behind for the CrossFieldValidationModal component.
/// Allows creating validation rules that span multiple fields.
/// </summary>
public partial class CrossFieldValidationModal : ComponentBase
{
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
    /// Existing validation to edit (null for new).
    /// </summary>
    [Parameter]
    public FieldSetValidation? ExistingValidation { get; set; }

    /// <summary>
    /// The current module containing available fields.
    /// </summary>
    [Parameter]
    public FormModuleSchema CurrentModule { get; set; } = default!;

    /// <summary>
    /// Callback when the validation is saved.
    /// </summary>
    [Parameter]
    public EventCallback<FieldSetValidation> OnSave { get; set; }

    /// <summary>
    /// Callback when the modal is cancelled.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    // State
    private string selectedType = "AtLeastOne";
    private HashSet<string> selectedFieldIds = new();
    private string? errorMessageEn;
    private string? errorMessageFr;

    // Validation type definitions
    private static readonly List<ValidationTypeInfo> ValidationTypes = new()
    {
        new("AtLeastOne", "At Least One", "One or more fields must have a value", "bi-1-circle"),
        new("AllOrNone", "All or None", "Either all fields filled or all empty", "bi-check-all"),
        new("MutuallyExclusive", "Mutually Exclusive", "Only one field can have a value", "bi-x-circle")
    };

    private bool IsValid => selectedFieldIds.Count >= 2 && !string.IsNullOrEmpty(selectedType);

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
        if (ExistingValidation != null)
        {
            // Edit mode - load existing values
            selectedType = ExistingValidation.Type;
            selectedFieldIds = new HashSet<string>(ExistingValidation.FieldIds);
            errorMessageEn = ExistingValidation.ErrorMessageEn;
            errorMessageFr = ExistingValidation.ErrorMessageFr;
        }
        else
        {
            // New validation - reset to defaults
            selectedType = "AtLeastOne";
            selectedFieldIds = new HashSet<string>();
            errorMessageEn = null;
            errorMessageFr = null;
        }
    }

    private void SelectType(string typeId)
    {
        selectedType = typeId;
    }

    private void ToggleField(string fieldId)
    {
        if (selectedFieldIds.Contains(fieldId))
        {
            selectedFieldIds.Remove(fieldId);
        }
        else
        {
            selectedFieldIds.Add(fieldId);
        }
    }

    private void RemoveField(string fieldId)
    {
        selectedFieldIds.Remove(fieldId);
    }

    private string GetPreviewHtml()
    {
        var fields = selectedFieldIds.ToList();
        var fieldHtml = string.Join(", ", fields.Select(f => $"<span class=\"field\">{f}</span>"));

        return selectedType switch
        {
            "AtLeastOne" => $"<span class=\"type\">AT LEAST ONE</span> of the following fields must have a value:<br>{fieldHtml}",
            "AllOrNone" => $"<span class=\"type\">EITHER ALL OR NONE</span> of the following fields must have values:<br>{fieldHtml}",
            "MutuallyExclusive" => $"<span class=\"type\">ONLY ONE</span> of the following fields can have a value:<br>{fieldHtml}",
            _ => ""
        };
    }

    private string GetPreviewText()
    {
        var fields = string.Join(", ", selectedFieldIds);
        return selectedType switch
        {
            "AtLeastOne" => $"AT LEAST ONE of: {fields} must have a value",
            "AllOrNone" => $"Either ALL or NONE of: {fields} must have values",
            "MutuallyExclusive" => $"Only ONE of: {fields} can have a value",
            _ => ""
        };
    }

    private async Task HandleSave()
    {
        if (!IsValid) return;

        var validation = new FieldSetValidation
        {
            Type = selectedType,
            FieldIds = selectedFieldIds.ToArray(),
            ErrorMessageEn = string.IsNullOrWhiteSpace(errorMessageEn) ? null : errorMessageEn,
            ErrorMessageFr = string.IsNullOrWhiteSpace(errorMessageFr) ? null : errorMessageFr
        };

        await OnSave.InvokeAsync(validation);
        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await IsOpenChanged.InvokeAsync(false);
    }

    /// <summary>
    /// Information about a validation type.
    /// </summary>
    private record ValidationTypeInfo(string Id, string Name, string Description, string Icon);
}
