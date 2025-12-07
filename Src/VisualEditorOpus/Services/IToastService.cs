namespace VisualEditorOpus.Services;

public interface IToastService
{
    event Action<ToastMessage>? OnToastAdded;
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInfo(string message);
}

public record ToastMessage(string Message, ToastType Type, DateTime CreatedAt)
{
    public string Id { get; } = Guid.NewGuid().ToString();
}

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}
