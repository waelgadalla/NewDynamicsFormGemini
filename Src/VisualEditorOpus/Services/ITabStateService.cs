using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Service interface for managing CodeSet tab state
/// </summary>
public interface ITabStateService
{
    /// <summary>
    /// Event fired when a tab is opened
    /// </summary>
    event EventHandler<TabState>? TabOpened;

    /// <summary>
    /// Event fired when a tab is closed
    /// </summary>
    event EventHandler<string>? TabClosed;

    /// <summary>
    /// Event fired when a tab is activated
    /// </summary>
    event EventHandler<TabState>? TabActivated;

    /// <summary>
    /// Event fired when a tab is updated
    /// </summary>
    event EventHandler<TabState>? TabUpdated;

    /// <summary>
    /// Gets the list of open tabs
    /// </summary>
    IReadOnlyList<TabState> OpenTabs { get; }

    /// <summary>
    /// Gets the currently active tab
    /// </summary>
    TabState? ActiveTab { get; }

    /// <summary>
    /// Opens a new tab for a CodeSet or activates existing tab
    /// </summary>
    void OpenTab(ManagedCodeSet codeSet);

    /// <summary>
    /// Closes a tab by ID
    /// </summary>
    void CloseTab(string tabId);

    /// <summary>
    /// Closes all tabs
    /// </summary>
    void CloseAllTabs();

    /// <summary>
    /// Closes all tabs except the specified one
    /// </summary>
    void CloseOtherTabs(string keepTabId);

    /// <summary>
    /// Activates a tab by ID
    /// </summary>
    void ActivateTab(string tabId);

    /// <summary>
    /// Updates a tab's state
    /// </summary>
    void UpdateTab(TabState tab);

    /// <summary>
    /// Reorders tabs by ID list
    /// </summary>
    void ReorderTabs(List<string> tabIds);

    /// <summary>
    /// Pins a tab
    /// </summary>
    void PinTab(string tabId);

    /// <summary>
    /// Unpins a tab
    /// </summary>
    void UnpinTab(string tabId);

    /// <summary>
    /// Marks a tab as dirty (has unsaved changes)
    /// </summary>
    void MarkDirty(string tabId);

    /// <summary>
    /// Marks a tab as clean (no unsaved changes)
    /// </summary>
    void MarkClean(string tabId);
}
