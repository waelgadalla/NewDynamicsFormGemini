namespace VisualEditorOpus.Services;

public class ToastService : IToastService
{
    public event Action<ToastMessage>? OnToastAdded;

    public void ShowSuccess(string message)
        => OnToastAdded?.Invoke(new ToastMessage(message, ToastType.Success, DateTime.UtcNow));

    public void ShowError(string message)
        => OnToastAdded?.Invoke(new ToastMessage(message, ToastType.Error, DateTime.UtcNow));

    public void ShowWarning(string message)
        => OnToastAdded?.Invoke(new ToastMessage(message, ToastType.Warning, DateTime.UtcNow));

    public void ShowInfo(string message)
        => OnToastAdded?.Invoke(new ToastMessage(message, ToastType.Info, DateTime.UtcNow));
}
