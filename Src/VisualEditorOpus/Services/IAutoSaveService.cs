namespace VisualEditorOpus.Services;

/// <summary>
/// Auto-save service that automatically persists changes using both:
/// 1. Timed auto-save: Every 30 seconds if there are unsaved changes
/// 2. On-change auto-save: 2-3 seconds after the last edit (debounced)
/// </summary>
public interface IAutoSaveService : IDisposable
{
    /// <summary>
    /// Whether auto-save is currently enabled
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Whether an auto-save is currently in progress
    /// </summary>
    bool IsSaving { get; }

    /// <summary>
    /// Last time auto-save ran successfully
    /// </summary>
    DateTime? LastAutoSaveTime { get; }

    /// <summary>
    /// Fired when auto-save state changes (saving started/completed)
    /// </summary>
    event Action? OnAutoSaveStateChanged;

    /// <summary>
    /// Enable auto-save functionality
    /// </summary>
    void Enable();

    /// <summary>
    /// Disable auto-save functionality
    /// </summary>
    void Disable();

    /// <summary>
    /// Start the auto-save service (call on component init)
    /// </summary>
    void Start();

    /// <summary>
    /// Stop the auto-save service (call on component dispose)
    /// </summary>
    void Stop();
}
