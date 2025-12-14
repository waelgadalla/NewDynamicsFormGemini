using Microsoft.AspNetCore.Components;
using DynamicForms.Core.V4.Schemas;
using VisualEditorOpus.Services.Theming;

namespace VisualEditorOpus.Components.Editor.Modals;

/// <summary>
/// Code-behind for the MetadataModal component.
/// Allows editing of module-level metadata including titles, descriptions, instructions, and database configuration.
/// </summary>
public partial class MetadataModal : ComponentBase
{
    [Inject] private IThemePersistenceService ThemePersistence { get; set; } = default!;

    private EditModel model = new();
    private List<ThemeSummary> _availableThemes = new();
    private bool _themesLoaded;

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
    /// The module to edit metadata for.
    /// </summary>
    [Parameter]
    public FormModuleSchema Module { get; set; } = default!;

    /// <summary>
    /// Callback when the metadata is saved.
    /// </summary>
    [Parameter]
    public EventCallback<FormModuleSchema> OnSave { get; set; }

    /// <summary>
    /// Callback when the modal is cancelled.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    private bool IsValid => !string.IsNullOrWhiteSpace(model.TitleEn);

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (IsOpen && Module != null)
        {
            Initialize();
            await LoadThemesAsync();
        }
    }

    private void Initialize()
    {
        model = new EditModel
        {
            TitleEn = Module.TitleEn,
            TitleFr = Module.TitleFr,
            DescriptionEn = Module.DescriptionEn,
            DescriptionFr = Module.DescriptionFr,
            InstructionsEn = Module.InstructionsEn,
            InstructionsFr = Module.InstructionsFr,
            TableName = Module.TableName,
            SchemaName = Module.SchemaName ?? "dbo",
            ThemeId = Module.ThemeId
        };
    }

    private async Task LoadThemesAsync()
    {
        if (_themesLoaded) return;

        try
        {
            _availableThemes = (await ThemePersistence.ListThemesAsync()).ToList();
            _themesLoaded = true;
        }
        catch
        {
            _availableThemes = new List<ThemeSummary>();
        }
    }

    private async Task HandleSave()
    {
        if (!IsValid) return;

        var updatedModule = Module with
        {
            TitleEn = model.TitleEn,
            TitleFr = string.IsNullOrWhiteSpace(model.TitleFr) ? null : model.TitleFr,
            DescriptionEn = string.IsNullOrWhiteSpace(model.DescriptionEn) ? null : model.DescriptionEn,
            DescriptionFr = string.IsNullOrWhiteSpace(model.DescriptionFr) ? null : model.DescriptionFr,
            InstructionsEn = string.IsNullOrWhiteSpace(model.InstructionsEn) ? null : model.InstructionsEn,
            InstructionsFr = string.IsNullOrWhiteSpace(model.InstructionsFr) ? null : model.InstructionsFr,
            TableName = string.IsNullOrWhiteSpace(model.TableName) ? null : model.TableName,
            SchemaName = model.SchemaName,
            ThemeId = string.IsNullOrWhiteSpace(model.ThemeId) ? null : model.ThemeId,
            DateUpdated = DateTime.UtcNow
        };

        await OnSave.InvokeAsync(updatedModule);
        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
        await IsOpenChanged.InvokeAsync(false);
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("MMM d, yyyy 'at' h:mm tt");
    }

    /// <summary>
    /// Mutable model for binding to form inputs.
    /// </summary>
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
        public string? ThemeId { get; set; }
    }
}
