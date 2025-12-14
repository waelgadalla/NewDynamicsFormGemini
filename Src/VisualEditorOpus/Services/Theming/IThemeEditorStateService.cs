using DynamicForms.Models.Theming;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Central state management service for the theme editor.
/// Handles current theme, undo/redo, dirty state, and change notifications.
/// </summary>
public interface IThemeEditorStateService
{
    // === Current State ===

    /// <summary>
    /// The currently loaded theme being edited
    /// </summary>
    FormTheme CurrentTheme { get; }

    /// <summary>
    /// The original theme state (for dirty comparison and reset)
    /// </summary>
    FormTheme? OriginalTheme { get; }

    /// <summary>
    /// Whether the current theme has unsaved changes
    /// </summary>
    bool IsDirty { get; }

    /// <summary>
    /// Whether advanced editing mode is enabled (shows all sections)
    /// </summary>
    bool IsAdvancedMode { get; }

    /// <summary>
    /// Whether the editor is in loading state
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// The currently active section in the editor sidebar
    /// </summary>
    string? ActiveSection { get; }

    // === Events ===

    /// <summary>
    /// Fired when any theme property changes
    /// </summary>
    event Action? OnThemeChanged;

    /// <summary>
    /// Fired when dirty state changes
    /// </summary>
    event Action? OnDirtyStateChanged;

    /// <summary>
    /// Fired when undo/redo availability changes
    /// </summary>
    event Action? OnHistoryChanged;

    /// <summary>
    /// Fired when advanced mode is toggled
    /// </summary>
    event Action<bool>? OnAdvancedModeChanged;

    /// <summary>
    /// Fired when a specific section is activated
    /// </summary>
    event Action<string?>? OnSectionChanged;

    /// <summary>
    /// Fired when loading state changes
    /// </summary>
    event Action<bool>? OnLoadingChanged;

    // === Theme Operations ===

    /// <summary>
    /// Load an existing theme for editing
    /// </summary>
    void LoadTheme(FormTheme theme);

    /// <summary>
    /// Create a new theme with default values
    /// </summary>
    void CreateNewTheme(string name);

    /// <summary>
    /// Apply a preset to the current theme
    /// </summary>
    void ApplyPreset(string presetId);

    /// <summary>
    /// Reset current theme to original state
    /// </summary>
    void Reset();

    /// <summary>
    /// Clear the current theme
    /// </summary>
    void ClearTheme();

    /// <summary>
    /// Mark current state as saved (clears dirty flag)
    /// </summary>
    void MarkClean();

    // === Property Updates ===

    /// <summary>
    /// Update theme name
    /// </summary>
    void UpdateName(string name);

    /// <summary>
    /// Set theme mode (Light/Dark/Auto)
    /// </summary>
    void SetMode(ThemeMode mode);

    /// <summary>
    /// Update colors section
    /// </summary>
    void UpdateColors(Action<ThemeColors> update);

    /// <summary>
    /// Update typography section
    /// </summary>
    void UpdateTypography(Action<ThemeTypography> update);

    /// <summary>
    /// Update spacing section
    /// </summary>
    void UpdateSpacing(Action<ThemeSpacing> update);

    /// <summary>
    /// Update borders section
    /// </summary>
    void UpdateBorders(Action<ThemeBorders> update);

    /// <summary>
    /// Update shadows section
    /// </summary>
    void UpdateShadows(Action<ThemeShadows> update);

    /// <summary>
    /// Update header section
    /// </summary>
    void UpdateHeader(Action<ThemeHeader> update);

    /// <summary>
    /// Update background section
    /// </summary>
    void UpdateBackground(Action<ThemeBackground> update);

    /// <summary>
    /// Update accessibility section
    /// </summary>
    void UpdateAccessibility(Action<ThemeAccessibility> update);

    /// <summary>
    /// Update component styles section
    /// </summary>
    void UpdateComponentStyles(Action<ThemeComponentStyles> update);

    // === Undo/Redo ===

    /// <summary>
    /// Whether undo is available
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Whether redo is available
    /// </summary>
    bool CanRedo { get; }

    /// <summary>
    /// Undo the last change
    /// </summary>
    void Undo();

    /// <summary>
    /// Redo the last undone change
    /// </summary>
    void Redo();

    /// <summary>
    /// Clear all undo/redo history
    /// </summary>
    void ClearHistory();

    // === UI State ===

    /// <summary>
    /// Toggle advanced editing mode
    /// </summary>
    void ToggleAdvancedMode();

    /// <summary>
    /// Set advanced editing mode
    /// </summary>
    void SetAdvancedMode(bool enabled);

    /// <summary>
    /// Set the active section in the sidebar
    /// </summary>
    void SetActiveSection(string? sectionId);

    /// <summary>
    /// Set loading state
    /// </summary>
    void SetLoading(bool isLoading);

    // === Legacy support ===

    /// <summary>
    /// Reset theme to last saved state (alias for Reset)
    /// </summary>
    void ResetTheme();

    /// <summary>
    /// Update theme mode (alias for SetMode)
    /// </summary>
    void UpdateMode(ThemeMode mode);

    /// <summary>
    /// Generic theme update
    /// </summary>
    void UpdateTheme(Action<FormTheme> updateAction);
}
