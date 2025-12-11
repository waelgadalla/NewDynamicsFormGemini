using Microsoft.Extensions.Logging;

namespace VisualEditorOpus.Services;

/// <summary>
/// Auto-save service implementation with dual triggers:
/// 1. Timed: Every 30 seconds if dirty
/// 2. Debounced: 2.5 seconds after last change
/// </summary>
public class AutoSaveService : IAutoSaveService
{
    private readonly IEditorStateService _editorState;
    private readonly IEditorPersistenceService _persistence;
    private readonly ILogger<AutoSaveService> _logger;

    private Timer? _timedSaveTimer;
    private Timer? _debounceTimer;
    private bool _isEnabled = true;
    private bool _isSaving;
    private bool _isStarted;
    private DateTime? _lastAutoSaveTime;

    // Configuration
    private const int TimedIntervalMs = 30_000;    // 30 seconds
    private const int DebounceDelayMs = 2_500;     // 2.5 seconds

    public bool IsEnabled => _isEnabled;
    public bool IsSaving => _isSaving;
    public DateTime? LastAutoSaveTime => _lastAutoSaveTime;

    public event Action? OnAutoSaveStateChanged;

    public AutoSaveService(
        IEditorStateService editorState,
        IEditorPersistenceService persistence,
        ILogger<AutoSaveService> logger)
    {
        _editorState = editorState;
        _persistence = persistence;
        _logger = logger;
    }

    public void Start()
    {
        if (_isStarted) return;
        _isStarted = true;

        // Subscribe to dirty state changes for debounced save
        _editorState.OnDirtyStateChanged += OnDirtyStateChanged;

        // Start periodic timer (30 seconds)
        _timedSaveTimer = new Timer(
            callback: _ => _ = TriggerTimedSaveAsync(),
            state: null,
            dueTime: TimedIntervalMs,
            period: TimedIntervalMs);

        _logger.LogInformation("AutoSaveService started - Timed: {TimedInterval}s, Debounce: {DebounceDelay}s",
            TimedIntervalMs / 1000, DebounceDelayMs / 1000);
    }

    public void Stop()
    {
        if (!_isStarted) return;
        _isStarted = false;

        _editorState.OnDirtyStateChanged -= OnDirtyStateChanged;

        _timedSaveTimer?.Dispose();
        _timedSaveTimer = null;

        _debounceTimer?.Dispose();
        _debounceTimer = null;

        _logger.LogInformation("AutoSaveService stopped");
    }

    public void Enable()
    {
        _isEnabled = true;
        _logger.LogDebug("AutoSave enabled");
    }

    public void Disable()
    {
        _isEnabled = false;
        CancelDebounce();
        _logger.LogDebug("AutoSave disabled");
    }

    private void OnDirtyStateChanged()
    {
        if (!_isEnabled || !_isStarted) return;

        // Only trigger debounce when dirty (not when marking clean)
        if (_editorState.IsDirty)
        {
            StartDebounce();
        }
        else
        {
            CancelDebounce();
        }
    }

    private void StartDebounce()
    {
        // Cancel existing debounce timer and restart
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(
            callback: _ => _ = TriggerDebouncedSaveAsync(),
            state: null,
            dueTime: DebounceDelayMs,
            period: Timeout.Infinite);
    }

    private void CancelDebounce()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = null;
    }

    private async Task TriggerTimedSaveAsync()
    {
        if (!_isEnabled || !_isStarted || _isSaving) return;
        if (!_editorState.IsDirty) return;

        _logger.LogDebug("Timed auto-save triggered");
        await ExecuteSaveAsync("timed");
    }

    private async Task TriggerDebouncedSaveAsync()
    {
        if (!_isEnabled || !_isStarted || _isSaving) return;
        if (!_editorState.IsDirty) return;

        _logger.LogDebug("Debounced auto-save triggered");
        await ExecuteSaveAsync("debounced");
    }

    private async Task ExecuteSaveAsync(string trigger)
    {
        if (_editorState.CurrentModule is null)
        {
            _logger.LogDebug("Auto-save skipped - no module loaded");
            return;
        }

        try
        {
            _isSaving = true;
            OnAutoSaveStateChanged?.Invoke();

            var result = await _persistence.SaveModuleAsync(_editorState.CurrentModule);

            if (result.Success)
            {
                _editorState.MarkClean();
                _lastAutoSaveTime = DateTime.UtcNow;
                _logger.LogInformation("Auto-save ({Trigger}) completed for module {ModuleId}",
                    trigger, _editorState.CurrentModule.Id);
            }
            else
            {
                _logger.LogWarning("Auto-save ({Trigger}) failed: {Error}", trigger, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-save ({Trigger}) error", trigger);
        }
        finally
        {
            _isSaving = false;
            OnAutoSaveStateChanged?.Invoke();
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
