using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Components.Shared;

/// <summary>
/// Foundation modal component that provides consistent styling, animations,
/// accessibility features, and behavior across the application.
/// </summary>
public partial class ModalBase : ComponentBase, IDisposable
{
    private readonly string titleId = $"modal-title-{Guid.NewGuid():N}";
    private ElementReference backdropRef;
    private ElementReference modalRef;
    private bool isVisible;
    private bool isDisposed;

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
    /// Event callback invoked when the modal is closed.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// The modal title displayed in the header.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Modal";

    /// <summary>
    /// Optional Bootstrap icon class (e.g., "bi-gear") displayed before the title.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Optional badge text displayed after the title.
    /// </summary>
    [Parameter]
    public string? Badge { get; set; }

    /// <summary>
    /// The size of the modal dialog.
    /// </summary>
    [Parameter]
    public ModalSize Size { get; set; } = ModalSize.Medium;

    /// <summary>
    /// Whether clicking the backdrop closes the modal.
    /// </summary>
    [Parameter]
    public bool CloseOnBackdropClick { get; set; } = true;

    /// <summary>
    /// Whether pressing the Escape key closes the modal.
    /// </summary>
    [Parameter]
    public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Whether to show the close button in the header.
    /// </summary>
    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// The main content of the modal body.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content to render in the modal footer.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Additional content to render in the header (e.g., extra buttons).
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderExtra { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the modal.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Whether the modal is in a loading state.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Gets whether the modal is visible (for animation purposes).
    /// </summary>
    protected bool IsVisible => isVisible;

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (IsOpen && !isVisible)
        {
            // Trigger animation on open
            await Task.Delay(10);
            isVisible = true;
            StateHasChanged();
        }
        else if (!IsOpen && isVisible)
        {
            isVisible = false;
        }
    }

    /// <summary>
    /// Handles clicking on the backdrop.
    /// </summary>
    private async Task HandleBackdropClick(MouseEventArgs args)
    {
        if (CloseOnBackdropClick)
        {
            await CloseAsync();
        }
    }

    /// <summary>
    /// Handles keyboard events for the modal.
    /// </summary>
    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape" && CloseOnEscape)
        {
            await CloseAsync();
        }
    }

    /// <summary>
    /// Handles the close button click.
    /// </summary>
    private async Task HandleClose()
    {
        await CloseAsync();
    }

    /// <summary>
    /// Closes the modal and notifies parent components.
    /// </summary>
    public async Task CloseAsync()
    {
        isVisible = false;
        StateHasChanged();

        // Allow animation to complete
        await Task.Delay(200);

        await IsOpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
    }

    /// <summary>
    /// Opens the modal.
    /// </summary>
    public async Task OpenAsync()
    {
        await IsOpenChanged.InvokeAsync(true);
    }

    /// <summary>
    /// Gets the CSS class for the current modal size.
    /// </summary>
    private string GetSizeClass()
    {
        return Size switch
        {
            ModalSize.Small => "modal-sm",
            ModalSize.Medium => "modal-md",
            ModalSize.Large => "modal-lg",
            ModalSize.ExtraLarge => "modal-xl",
            ModalSize.Fullscreen => "modal-fullscreen",
            _ => "modal-md"
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
        }
    }
}
