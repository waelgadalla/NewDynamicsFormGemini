using Microsoft.Extensions.Logging;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Service for managing CodeSet tab state
/// </summary>
public class TabStateService : ITabStateService
{
    private readonly List<TabState> _openTabs = new();
    private readonly ILogger<TabStateService> _logger;
    private TabState? _activeTab;

    public event EventHandler<TabState>? TabOpened;
    public event EventHandler<string>? TabClosed;
    public event EventHandler<TabState>? TabActivated;
    public event EventHandler<TabState>? TabUpdated;

    public IReadOnlyList<TabState> OpenTabs => _openTabs.AsReadOnly();
    public TabState? ActiveTab => _activeTab;

    public TabStateService(ILogger<TabStateService> logger)
    {
        _logger = logger;
    }

    public void OpenTab(ManagedCodeSet codeSet)
    {
        // Check if already open
        var existing = _openTabs.FirstOrDefault(t => t.CodeSetId == codeSet.Id);
        if (existing != null)
        {
            _logger.LogDebug("Tab for CodeSet {CodeSetId} already open, activating", codeSet.Id);
            ActivateTab(existing.Id);
            return;
        }

        var icon = GetIconForCodeSet(codeSet);
        var tab = new TabState
        {
            CodeSetId = codeSet.Id,
            CodeSetName = codeSet.NameEn,
            Icon = icon,
            ItemCount = codeSet.Items.Count,
            Order = _openTabs.Count,
            IsActive = true
        };

        // Deactivate current tab
        if (_activeTab != null)
        {
            var index = _openTabs.FindIndex(t => t.Id == _activeTab.Id);
            if (index >= 0)
            {
                _openTabs[index] = _activeTab with { IsActive = false };
            }
        }

        _openTabs.Add(tab);
        _activeTab = tab;

        _logger.LogInformation("Opened tab for CodeSet {CodeSetId}: {Name}", codeSet.Id, codeSet.NameEn);
        TabOpened?.Invoke(this, tab);
        TabActivated?.Invoke(this, tab);
    }

    public void CloseTab(string tabId)
    {
        var index = _openTabs.FindIndex(t => t.Id == tabId);
        if (index < 0) return;

        var tab = _openTabs[index];

        // Don't close pinned tabs
        if (tab.IsPinned)
        {
            _logger.LogDebug("Cannot close pinned tab {TabId}", tabId);
            return;
        }

        _openTabs.RemoveAt(index);
        _logger.LogInformation("Closed tab {TabId}", tabId);
        TabClosed?.Invoke(this, tabId);

        // If closing active tab, activate another
        if (_activeTab?.Id == tabId)
        {
            _activeTab = null;
            if (_openTabs.Any())
            {
                var newIndex = Math.Min(index, _openTabs.Count - 1);
                ActivateTab(_openTabs[newIndex].Id);
            }
        }
    }

    public void CloseAllTabs()
    {
        // Close non-pinned tabs in reverse order
        var tabsToClose = _openTabs
            .Where(t => !t.IsPinned)
            .Select(t => t.Id)
            .ToList();

        foreach (var id in tabsToClose)
        {
            var index = _openTabs.FindIndex(t => t.Id == id);
            if (index >= 0)
            {
                _openTabs.RemoveAt(index);
                TabClosed?.Invoke(this, id);
            }
        }

        _logger.LogInformation("Closed all non-pinned tabs ({Count} tabs)", tabsToClose.Count);

        // Update active tab
        if (_activeTab != null && !_openTabs.Any(t => t.Id == _activeTab.Id))
        {
            _activeTab = _openTabs.FirstOrDefault();
            if (_activeTab != null)
            {
                var idx = _openTabs.FindIndex(t => t.Id == _activeTab.Id);
                if (idx >= 0)
                {
                    _openTabs[idx] = _activeTab with { IsActive = true };
                    _activeTab = _openTabs[idx];
                    TabActivated?.Invoke(this, _activeTab);
                }
            }
        }
    }

    public void CloseOtherTabs(string keepTabId)
    {
        var tabsToClose = _openTabs
            .Where(t => t.Id != keepTabId && !t.IsPinned)
            .Select(t => t.Id)
            .ToList();

        foreach (var id in tabsToClose)
        {
            CloseTab(id);
        }

        // Activate the kept tab
        ActivateTab(keepTabId);

        _logger.LogInformation("Closed other tabs, keeping {TabId}", keepTabId);
    }

    public void ActivateTab(string tabId)
    {
        var tab = _openTabs.FirstOrDefault(t => t.Id == tabId);
        if (tab == null) return;

        // Deactivate current
        if (_activeTab != null && _activeTab.Id != tabId)
        {
            var currentIndex = _openTabs.FindIndex(t => t.Id == _activeTab.Id);
            if (currentIndex >= 0)
            {
                _openTabs[currentIndex] = _activeTab with { IsActive = false };
            }
        }

        // Activate new
        var newIndex = _openTabs.FindIndex(t => t.Id == tabId);
        if (newIndex >= 0)
        {
            var activated = tab with { IsActive = true };
            _openTabs[newIndex] = activated;
            _activeTab = activated;

            _logger.LogDebug("Activated tab {TabId}: {Name}", tabId, tab.CodeSetName);
            TabActivated?.Invoke(this, activated);
        }
    }

    public void UpdateTab(TabState tab)
    {
        var index = _openTabs.FindIndex(t => t.Id == tab.Id);
        if (index >= 0)
        {
            _openTabs[index] = tab;
            if (_activeTab?.Id == tab.Id)
            {
                _activeTab = tab;
            }
            TabUpdated?.Invoke(this, tab);
        }
    }

    public void ReorderTabs(List<string> tabIds)
    {
        var reordered = tabIds
            .Select((id, index) => (_openTabs.FirstOrDefault(t => t.Id == id), index))
            .Where(x => x.Item1 != null)
            .Select(x => x.Item1! with { Order = x.index })
            .ToList();

        _openTabs.Clear();
        _openTabs.AddRange(reordered.OrderBy(t => t.Order));

        _logger.LogDebug("Reordered {Count} tabs", reordered.Count);
    }

    public void PinTab(string tabId)
    {
        var index = _openTabs.FindIndex(t => t.Id == tabId);
        if (index >= 0)
        {
            var tab = _openTabs[index] with { IsPinned = true };
            _openTabs[index] = tab;
            if (_activeTab?.Id == tabId)
            {
                _activeTab = tab;
            }

            _logger.LogInformation("Pinned tab {TabId}", tabId);
            TabUpdated?.Invoke(this, tab);
        }
    }

    public void UnpinTab(string tabId)
    {
        var index = _openTabs.FindIndex(t => t.Id == tabId);
        if (index >= 0)
        {
            var tab = _openTabs[index] with { IsPinned = false };
            _openTabs[index] = tab;
            if (_activeTab?.Id == tabId)
            {
                _activeTab = tab;
            }

            _logger.LogInformation("Unpinned tab {TabId}", tabId);
            TabUpdated?.Invoke(this, tab);
        }
    }

    public void MarkDirty(string tabId)
    {
        var index = _openTabs.FindIndex(t => t.Id == tabId);
        if (index >= 0 && !_openTabs[index].IsDirty)
        {
            var tab = _openTabs[index] with { IsDirty = true };
            _openTabs[index] = tab;
            if (_activeTab?.Id == tabId)
            {
                _activeTab = tab;
            }
            TabUpdated?.Invoke(this, tab);
        }
    }

    public void MarkClean(string tabId)
    {
        var index = _openTabs.FindIndex(t => t.Id == tabId);
        if (index >= 0 && _openTabs[index].IsDirty)
        {
            var tab = _openTabs[index] with { IsDirty = false };
            _openTabs[index] = tab;
            if (_activeTab?.Id == tabId)
            {
                _activeTab = tab;
            }
            TabUpdated?.Invoke(this, tab);
        }
    }

    private static string GetIconForCodeSet(ManagedCodeSet codeSet)
    {
        // Determine icon based on name/category patterns
        var name = codeSet.NameEn.ToLowerInvariant();
        var category = codeSet.Category?.ToLowerInvariant() ?? "";

        return (name, category) switch
        {
            var (n, c) when n.Contains("country") || n.Contains("nation") || c == "geography" => "bi-globe",
            var (n, _) when n.Contains("state") || n.Contains("province") => "bi-geo-alt",
            var (n, c) when n.Contains("department") || n.Contains("team") || c == "organization" => "bi-building",
            var (n, _) when n.Contains("status") => "bi-flag",
            var (n, _) when n.Contains("priority") => "bi-exclamation-triangle",
            var (n, _) when n.Contains("category") => "bi-folder",
            var (n, _) when n.Contains("type") => "bi-tag",
            var (_, c) when c == "system" => "bi-gear",
            _ => "bi-collection"
        };
    }
}
