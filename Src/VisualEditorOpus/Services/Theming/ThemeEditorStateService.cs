using System.Text.Json;
using DynamicForms.Models.Theming;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Implementation of theme editor state management.
/// Uses immutable state pattern with undo/redo support and debounced preview updates.
/// </summary>
public class ThemeEditorStateService : IThemeEditorStateService, IDisposable
{
    private const int MaxHistorySize = 50;
    private const int DebounceDelayMs = 50;

    private FormTheme _currentTheme;
    private FormTheme? _originalTheme;
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();

    private string? _activeSection;
    private bool _isLoading;
    private bool _isAdvancedMode;

    private Timer? _debounceTimer;
    private bool _pendingNotification;
    private readonly object _lock = new();

    public ThemeEditorStateService()
    {
        // Initialize with default theme
        _currentTheme = CreateDefaultTheme();
    }

    // === Current State ===

    public FormTheme CurrentTheme => _currentTheme;
    public FormTheme? OriginalTheme => _originalTheme;

    public bool IsDirty
    {
        get
        {
            if (_originalTheme is null) return true; // New unsaved theme
            return !ThemesAreEqual(_currentTheme, _originalTheme);
        }
    }

    public bool IsAdvancedMode => _isAdvancedMode;
    public bool IsLoading => _isLoading;
    public string? ActiveSection => _activeSection;

    // === Undo/Redo State ===

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    // === Events ===

    public event Action? OnThemeChanged;
    public event Action? OnDirtyStateChanged;
    public event Action? OnHistoryChanged;
    public event Action<bool>? OnAdvancedModeChanged;
    public event Action<string?>? OnSectionChanged;
    public event Action<bool>? OnLoadingChanged;

    // === Theme Operations ===

    public void LoadTheme(FormTheme theme)
    {
        var wasDirty = IsDirty;

        ClearHistory();
        _currentTheme = DeepClone(theme);
        _originalTheme = DeepClone(theme);

        NotifyThemeChanged();

        if (wasDirty != IsDirty)
        {
            OnDirtyStateChanged?.Invoke();
        }
    }

    public void CreateNewTheme(string name)
    {
        var wasDirty = IsDirty;

        ClearHistory();

        // Start with default preset
        var defaultPreset = ThemePresets.GetPreset("default");
        _currentTheme = defaultPreset is not null ? DeepClone(defaultPreset) : CreateDefaultTheme();
        _currentTheme.Id = Guid.NewGuid().ToString();
        _currentTheme.Name = name;
        _currentTheme.CreatedAt = DateTime.UtcNow;
        _currentTheme.ModifiedAt = DateTime.UtcNow;
        _originalTheme = null; // New theme is unsaved

        NotifyThemeChanged();

        if (wasDirty != IsDirty)
        {
            OnDirtyStateChanged?.Invoke();
        }
    }

    public void ApplyPreset(string presetId)
    {
        var preset = ThemePresets.GetPreset(presetId);
        if (preset is null) return;

        // Save current state for undo
        PushUndoState();

        // Preserve theme ID, name, and metadata while applying preset styling
        var currentId = _currentTheme.Id;
        var currentName = _currentTheme.Name;
        var createdAt = _currentTheme.CreatedAt;

        _currentTheme = DeepClone(preset);
        _currentTheme.Id = currentId;
        _currentTheme.Name = currentName;
        _currentTheme.CreatedAt = createdAt;
        _currentTheme.ModifiedAt = DateTime.UtcNow;

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void Reset()
    {
        if (_originalTheme is null) return;

        var wasDirty = IsDirty;

        // Save current state for undo
        PushUndoState();

        _currentTheme = DeepClone(_originalTheme);

        NotifyThemeChanged();

        if (wasDirty != IsDirty)
        {
            OnDirtyStateChanged?.Invoke();
        }
    }

    public void ResetTheme() => Reset();

    public void ClearTheme()
    {
        var wasDirty = IsDirty;

        ClearHistory();
        _currentTheme = CreateDefaultTheme();
        _originalTheme = null;
        _activeSection = null;

        NotifyThemeChanged();

        if (wasDirty)
        {
            OnDirtyStateChanged?.Invoke();
        }
    }

    public void MarkClean()
    {
        var wasDirty = IsDirty;
        _originalTheme = DeepClone(_currentTheme);

        if (wasDirty)
        {
            OnDirtyStateChanged?.Invoke();
        }
    }

    // === Property Updates ===

    public void UpdateName(string name)
    {
        if (_currentTheme.Name == name) return;

        PushUndoState();
        _currentTheme.Name = name;
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void SetMode(ThemeMode mode)
    {
        if (_currentTheme.Mode == mode) return;

        PushUndoState();
        _currentTheme.Mode = mode;
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateMode(ThemeMode mode) => SetMode(mode);

    public void UpdateTheme(Action<FormTheme> updateAction)
    {
        PushUndoState();
        updateAction(_currentTheme);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateColors(Action<ThemeColors> update)
    {
        PushUndoState();
        update(_currentTheme.Colors);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateTypography(Action<ThemeTypography> update)
    {
        PushUndoState();
        update(_currentTheme.Typography);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateSpacing(Action<ThemeSpacing> update)
    {
        PushUndoState();
        update(_currentTheme.Spacing);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateBorders(Action<ThemeBorders> update)
    {
        PushUndoState();
        update(_currentTheme.Borders);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateShadows(Action<ThemeShadows> update)
    {
        PushUndoState();
        update(_currentTheme.Shadows);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateHeader(Action<ThemeHeader> update)
    {
        PushUndoState();
        update(_currentTheme.Header);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateBackground(Action<ThemeBackground> update)
    {
        PushUndoState();
        update(_currentTheme.Background);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateAccessibility(Action<ThemeAccessibility> update)
    {
        PushUndoState();
        update(_currentTheme.Accessibility);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    public void UpdateComponentStyles(Action<ThemeComponentStyles> update)
    {
        PushUndoState();
        update(_currentTheme.Components);
        _currentTheme.Touch();

        NotifyThemeChangedDebounced();
        NotifyDirtyStateIfChanged();
    }

    // === Undo/Redo ===

    public void Undo()
    {
        if (!CanUndo) return;

        var currentState = SerializeTheme(_currentTheme);
        _redoStack.Push(currentState);

        var previousState = _undoStack.Pop();
        _currentTheme = DeserializeTheme(previousState);

        NotifyThemeChanged();
        NotifyDirtyStateIfChanged();
        OnHistoryChanged?.Invoke();
    }

    public void Redo()
    {
        if (!CanRedo) return;

        var currentState = SerializeTheme(_currentTheme);
        _undoStack.Push(currentState);

        var nextState = _redoStack.Pop();
        _currentTheme = DeserializeTheme(nextState);

        NotifyThemeChanged();
        NotifyDirtyStateIfChanged();
        OnHistoryChanged?.Invoke();
    }

    public void ClearHistory()
    {
        var hadHistory = CanUndo || CanRedo;

        _undoStack.Clear();
        _redoStack.Clear();

        if (hadHistory)
        {
            OnHistoryChanged?.Invoke();
        }
    }

    // === UI State ===

    public void ToggleAdvancedMode()
    {
        SetAdvancedMode(!_isAdvancedMode);
    }

    public void SetAdvancedMode(bool enabled)
    {
        if (_isAdvancedMode == enabled) return;

        _isAdvancedMode = enabled;
        OnAdvancedModeChanged?.Invoke(enabled);
    }

    public void SetActiveSection(string? sectionId)
    {
        if (_activeSection == sectionId) return;

        _activeSection = sectionId;
        OnSectionChanged?.Invoke(sectionId);
    }

    public void SetLoading(bool isLoading)
    {
        if (_isLoading == isLoading) return;

        _isLoading = isLoading;
        OnLoadingChanged?.Invoke(isLoading);
    }

    // === Private Helpers ===

    private void PushUndoState()
    {
        var state = SerializeTheme(_currentTheme);
        _undoStack.Push(state);

        // Clear redo stack when new change is made
        _redoStack.Clear();

        // Limit history size
        while (_undoStack.Count > MaxHistorySize)
        {
            // Remove oldest items (convert to array, take recent, rebuild)
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            foreach (var item in items.Take(MaxHistorySize).Reverse())
            {
                _undoStack.Push(item);
            }
        }

        OnHistoryChanged?.Invoke();
    }

    private void NotifyThemeChanged()
    {
        // Cancel any pending debounced notification
        lock (_lock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
            _pendingNotification = false;
        }

        OnThemeChanged?.Invoke();
    }

    private void NotifyThemeChangedDebounced()
    {
        lock (_lock)
        {
            _pendingNotification = true;

            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(_ =>
            {
                lock (_lock)
                {
                    if (_pendingNotification)
                    {
                        _pendingNotification = false;
                        OnThemeChanged?.Invoke();
                    }
                }
            }, null, DebounceDelayMs, Timeout.Infinite);
        }
    }

    private bool _lastDirtyState;

    private void NotifyDirtyStateIfChanged()
    {
        var currentDirty = IsDirty;
        if (_lastDirtyState != currentDirty)
        {
            _lastDirtyState = currentDirty;
            OnDirtyStateChanged?.Invoke();
        }
    }

    private static FormTheme CreateDefaultTheme()
    {
        return new FormTheme
        {
            Id = Guid.NewGuid().ToString(),
            Name = "New Theme",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }

    private static bool ThemesAreEqual(FormTheme a, FormTheme b)
    {
        // Compare by serialization (ignoring timestamps)
        var jsonA = SerializeThemeForComparison(a);
        var jsonB = SerializeThemeForComparison(b);
        return jsonA == jsonB;
    }

    private static string SerializeTheme(FormTheme theme)
    {
        return JsonSerializer.Serialize(theme, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    private static string SerializeThemeForComparison(FormTheme theme)
    {
        // Create a copy without timestamps for comparison
        var copy = DeepClone(theme);
        copy.CreatedAt = default;
        copy.ModifiedAt = default;
        return JsonSerializer.Serialize(copy, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    private static FormTheme DeserializeTheme(string json)
    {
        return JsonSerializer.Deserialize<FormTheme>(json) ?? CreateDefaultTheme();
    }

    private static FormTheme DeepClone(FormTheme theme)
    {
        var json = SerializeTheme(theme);
        return DeserializeTheme(json);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }
    }
}
