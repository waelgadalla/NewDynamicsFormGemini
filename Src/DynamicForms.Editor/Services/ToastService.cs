namespace DynamicForms.Editor.Services;

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public interface IToastService
{
    event Action<string, ToastLevel> OnShow;
    void ShowToast(string message, ToastLevel level);
    void ShowInfo(string message) => ShowToast(message, ToastLevel.Info);
    void ShowSuccess(string message) => ShowToast(message, ToastLevel.Success);
    void ShowWarning(string message) => ShowToast(message, ToastLevel.Warning);
    void ShowError(string message) => ShowToast(message, ToastLevel.Error);
}

public class ToastService : IToastService
{
    public event Action<string, ToastLevel>? OnShow;

    public void ShowToast(string message, ToastLevel level)
    {
        OnShow?.Invoke(message, level);
    }
}
