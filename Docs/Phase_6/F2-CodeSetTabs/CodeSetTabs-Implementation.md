# F.2 CodeSetTabs - Implementation Guide

## Overview

The CodeSetTabs component provides a tabbed interface for managing multiple CodeSets simultaneously. Users can quickly switch between CodeSets, edit data inline, and view properties and bindings in a sidebar panel.

## Component Architecture

```
CodeSetTabs/
├── CodeSetTabs.razor                 # Main tabbed container
├── CodeSetTabs.razor.css             # Scoped styles
├── CodeSetTabBar.razor               # Tab navigation bar
├── CodeSetTabPanel.razor             # Individual tab content
├── CodeSetDataEditor.razor           # Inline data editing grid
├── CodeSetPropertiesSidebar.razor    # Properties panel
└── Services/
    └── TabStateService.cs            # Tab state management
```

## Data Models

### TabState.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Models;

/// <summary>
/// Represents the state of an open CodeSet tab
/// </summary>
public record TabState
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string CodeSetId { get; init; } = "";
    public string CodeSetName { get; init; } = "";
    public string Icon { get; init; } = "bi-collection";
    public int ItemCount { get; init; } = 0;
    public bool IsActive { get; init; } = false;
    public bool IsDirty { get; init; } = false;
    public bool IsPinned { get; init; } = false;
    public int Order { get; init; } = 0;

    // Editor state
    public string SearchTerm { get; init; } = "";
    public string SortColumn { get; init; } = "Order";
    public bool SortAscending { get; init; } = true;
    public List<string> SelectedItemIds { get; init; } = new();
    public int ScrollPosition { get; init; } = 0;
}
```

### TabStateService.cs

```csharp
namespace VisualEditorOpus.Components.CodeSet.Services;

public interface ITabStateService
{
    event EventHandler<TabState>? TabOpened;
    event EventHandler<string>? TabClosed;
    event EventHandler<TabState>? TabActivated;
    event EventHandler<TabState>? TabUpdated;

    IReadOnlyList<TabState> OpenTabs { get; }
    TabState? ActiveTab { get; }

    void OpenTab(CodeSet codeSet);
    void CloseTab(string tabId);
    void CloseAllTabs();
    void CloseOtherTabs(string keepTabId);
    void ActivateTab(string tabId);
    void UpdateTab(TabState tab);
    void ReorderTabs(List<string> tabIds);
    void PinTab(string tabId);
    void UnpinTab(string tabId);
}

public class TabStateService : ITabStateService
{
    private readonly List<TabState> _openTabs = new();
    private TabState? _activeTab;

    public event EventHandler<TabState>? TabOpened;
    public event EventHandler<string>? TabClosed;
    public event EventHandler<TabState>? TabActivated;
    public event EventHandler<TabState>? TabUpdated;

    public IReadOnlyList<TabState> OpenTabs => _openTabs.AsReadOnly();
    public TabState? ActiveTab => _activeTab;

    public void OpenTab(CodeSet codeSet)
    {
        // Check if already open
        var existing = _openTabs.FirstOrDefault(t => t.CodeSetId == codeSet.Id);
        if (existing != null)
        {
            ActivateTab(existing.Id);
            return;
        }

        var icon = GetIconForCodeSet(codeSet);
        var tab = new TabState
        {
            CodeSetId = codeSet.Id,
            CodeSetName = codeSet.Name,
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

        TabOpened?.Invoke(this, tab);
        TabActivated?.Invoke(this, tab);
    }

    public void CloseTab(string tabId)
    {
        var index = _openTabs.FindIndex(t => t.Id == tabId);
        if (index < 0) return;

        var tab = _openTabs[index];
        _openTabs.RemoveAt(index);

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
        var tabIds = _openTabs.Select(t => t.Id).ToList();
        foreach (var id in tabIds)
        {
            CloseTab(id);
        }
    }

    public void CloseOtherTabs(string keepTabId)
    {
        var tabsToClose = _openTabs.Where(t => t.Id != keepTabId).Select(t => t.Id).ToList();
        foreach (var id in tabsToClose)
        {
            CloseTab(id);
        }
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
    }

    public void PinTab(string tabId)
    {
        var index = _openTabs.FindIndex(t => t.Id == tabId);
        if (index >= 0)
        {
            var tab = _openTabs[index] with { IsPinned = true };
            _openTabs[index] = tab;
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
            TabUpdated?.Invoke(this, tab);
        }
    }

    private string GetIconForCodeSet(CodeSet codeSet)
    {
        // Determine icon based on name patterns
        var name = codeSet.Name.ToLowerInvariant();
        return name switch
        {
            var n when n.Contains("country") || n.Contains("nation") => "bi-globe",
            var n when n.Contains("state") || n.Contains("province") => "bi-geo-alt",
            var n when n.Contains("department") || n.Contains("team") => "bi-building",
            var n when n.Contains("status") => "bi-flag",
            var n when n.Contains("priority") => "bi-exclamation-triangle",
            var n when n.Contains("category") => "bi-folder",
            var n when n.Contains("type") => "bi-tag",
            _ => "bi-collection"
        };
    }
}
```

## Blazor Components

### CodeSetTabs.razor

```razor
@namespace VisualEditorOpus.Components.CodeSet
@inject ITabStateService TabStateService
@inject ICodeSetService CodeSetService
@implements IDisposable

<div class="codeset-tabs-container">
    <CodeSetTabBar Tabs="TabStateService.OpenTabs.ToList()"
                   ActiveTabId="@TabStateService.ActiveTab?.Id"
                   OnTabSelect="HandleTabSelect"
                   OnTabClose="HandleTabClose"
                   OnTabAdd="HandleTabAdd"
                   OnTabReorder="HandleTabReorder"
                   OnTabContextMenu="HandleTabContextMenu" />

    <div class="tab-content">
        @if (TabStateService.ActiveTab != null)
        {
            <CodeSetTabPanel TabState="TabStateService.ActiveTab"
                             CodeSet="ActiveCodeSet"
                             Items="ActiveItems"
                             OnItemAdd="HandleItemAdd"
                             OnItemUpdate="HandleItemUpdate"
                             OnItemDelete="HandleItemDelete"
                             OnSearch="HandleSearch"
                             OnSort="HandleSort"
                             OnCodeSetUpdate="HandleCodeSetUpdate" />
        }
        else
        {
            <div class="empty-state">
                <div class="empty-icon">
                    <i class="bi bi-collection"></i>
                </div>
                <h3>No CodeSet Open</h3>
                <p>Open a CodeSet from the sidebar or create a new one</p>
                <button class="btn btn-primary" @onclick="HandleTabAdd">
                    <i class="bi bi-plus-lg"></i>
                    Create CodeSet
                </button>
            </div>
        }
    </div>
</div>

@if (_showContextMenu)
{
    <div class="context-menu" style="top: @(_contextMenuY)px; left: @(_contextMenuX)px;">
        <button class="context-menu-item" @onclick="CloseTab">
            <i class="bi bi-x"></i> Close
        </button>
        <button class="context-menu-item" @onclick="CloseOtherTabs">
            <i class="bi bi-x-lg"></i> Close Others
        </button>
        <button class="context-menu-item" @onclick="CloseAllTabs">
            <i class="bi bi-x-circle"></i> Close All
        </button>
        <div class="context-menu-divider"></div>
        @if (_contextMenuTab?.IsPinned == true)
        {
            <button class="context-menu-item" @onclick="UnpinTab">
                <i class="bi bi-pin-angle"></i> Unpin
            </button>
        }
        else
        {
            <button class="context-menu-item" @onclick="PinTab">
                <i class="bi bi-pin"></i> Pin
            </button>
        }
    </div>
    <div class="context-menu-backdrop" @onclick="CloseContextMenu"></div>
}

@code {
    private CodeSet? ActiveCodeSet { get; set; }
    private List<CodeSetItem> ActiveItems { get; set; } = new();

    private bool _showContextMenu = false;
    private double _contextMenuX = 0;
    private double _contextMenuY = 0;
    private TabState? _contextMenuTab;

    protected override void OnInitialized()
    {
        TabStateService.TabActivated += OnTabActivated;
        TabStateService.TabUpdated += OnTabUpdated;
    }

    private async void OnTabActivated(object? sender, TabState tab)
    {
        await LoadActiveCodeSet(tab.CodeSetId);
        await InvokeAsync(StateHasChanged);
    }

    private void OnTabUpdated(object? sender, TabState tab)
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task LoadActiveCodeSet(string codeSetId)
    {
        ActiveCodeSet = await CodeSetService.GetCodeSetByIdAsync(codeSetId);
        if (ActiveCodeSet != null)
        {
            ActiveItems = await CodeSetService.GetItemsAsync(codeSetId);
        }
    }

    private void HandleTabSelect(string tabId)
    {
        TabStateService.ActivateTab(tabId);
    }

    private void HandleTabClose(string tabId)
    {
        var tab = TabStateService.OpenTabs.FirstOrDefault(t => t.Id == tabId);
        if (tab?.IsDirty == true)
        {
            // Show confirmation dialog
            // For now, just close
        }
        TabStateService.CloseTab(tabId);
    }

    private async Task HandleTabAdd()
    {
        var newCodeSet = new CodeSet
        {
            Name = "New CodeSet",
            Description = ""
        };
        var created = await CodeSetService.CreateCodeSetAsync(newCodeSet);
        TabStateService.OpenTab(created);
    }

    private void HandleTabReorder(List<string> tabIds)
    {
        TabStateService.ReorderTabs(tabIds);
    }

    private void HandleTabContextMenu((TabState Tab, double X, double Y) args)
    {
        _contextMenuTab = args.Tab;
        _contextMenuX = args.X;
        _contextMenuY = args.Y;
        _showContextMenu = true;
    }

    private void CloseContextMenu()
    {
        _showContextMenu = false;
        _contextMenuTab = null;
    }

    private void CloseTab()
    {
        if (_contextMenuTab != null)
            TabStateService.CloseTab(_contextMenuTab.Id);
        CloseContextMenu();
    }

    private void CloseOtherTabs()
    {
        if (_contextMenuTab != null)
            TabStateService.CloseOtherTabs(_contextMenuTab.Id);
        CloseContextMenu();
    }

    private void CloseAllTabs()
    {
        TabStateService.CloseAllTabs();
        CloseContextMenu();
    }

    private void PinTab()
    {
        if (_contextMenuTab != null)
            TabStateService.PinTab(_contextMenuTab.Id);
        CloseContextMenu();
    }

    private void UnpinTab()
    {
        if (_contextMenuTab != null)
            TabStateService.UnpinTab(_contextMenuTab.Id);
        CloseContextMenu();
    }

    // Editor event handlers
    private async Task HandleItemAdd()
    {
        if (ActiveCodeSet == null) return;

        var newItem = new CodeSetItem
        {
            Code = "",
            DisplayName = "New Item",
            Order = ActiveItems.Count
        };
        var added = await CodeSetService.AddItemAsync(ActiveCodeSet.Id, newItem);
        ActiveItems.Add(added);
        MarkDirty();
    }

    private async Task HandleItemUpdate(CodeSetItem item)
    {
        if (ActiveCodeSet == null) return;
        await CodeSetService.UpdateItemAsync(ActiveCodeSet.Id, item);
        MarkDirty();
    }

    private async Task HandleItemDelete(CodeSetItem item)
    {
        if (ActiveCodeSet == null) return;
        await CodeSetService.DeleteItemAsync(ActiveCodeSet.Id, item.Id);
        ActiveItems.Remove(item);
        MarkDirty();
    }

    private async Task HandleSearch(string searchTerm)
    {
        if (ActiveCodeSet == null || TabStateService.ActiveTab == null) return;

        var updated = TabStateService.ActiveTab with { SearchTerm = searchTerm };
        TabStateService.UpdateTab(updated);

        ActiveItems = await CodeSetService.GetFilteredItemsAsync(
            ActiveCodeSet.Id,
            searchTerm: searchTerm);
    }

    private void HandleSort((string Column, bool Ascending) sort)
    {
        if (TabStateService.ActiveTab == null) return;

        var updated = TabStateService.ActiveTab with
        {
            SortColumn = sort.Column,
            SortAscending = sort.Ascending
        };
        TabStateService.UpdateTab(updated);

        ActiveItems = sort.Column switch
        {
            "Code" => sort.Ascending
                ? ActiveItems.OrderBy(i => i.Code).ToList()
                : ActiveItems.OrderByDescending(i => i.Code).ToList(),
            "DisplayName" => sort.Ascending
                ? ActiveItems.OrderBy(i => i.DisplayName).ToList()
                : ActiveItems.OrderByDescending(i => i.DisplayName).ToList(),
            "Order" => sort.Ascending
                ? ActiveItems.OrderBy(i => i.Order).ToList()
                : ActiveItems.OrderByDescending(i => i.Order).ToList(),
            _ => ActiveItems
        };
    }

    private async Task HandleCodeSetUpdate(CodeSet codeSet)
    {
        await CodeSetService.UpdateCodeSetAsync(codeSet);
        ActiveCodeSet = codeSet;
        MarkDirty();
    }

    private void MarkDirty()
    {
        if (TabStateService.ActiveTab == null) return;
        var updated = TabStateService.ActiveTab with { IsDirty = true };
        TabStateService.UpdateTab(updated);
    }

    public void Dispose()
    {
        TabStateService.TabActivated -= OnTabActivated;
        TabStateService.TabUpdated -= OnTabUpdated;
    }
}
```

### CodeSetTabs.razor.css

```css
.codeset-tabs-container {
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-color, #e5e7eb);
    border-radius: 12px;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    height: 100%;
}

.tab-content {
    flex: 1;
    overflow: hidden;
}

.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    padding: 3rem;
    text-align: center;
}

.empty-icon {
    width: 5rem;
    height: 5rem;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--bg-secondary, #f3f4f6);
    border-radius: 50%;
    margin-bottom: 1.5rem;
}

.empty-icon i {
    font-size: 2rem;
    color: var(--text-muted, #9ca3af);
}

.empty-state h3 {
    font-size: 1.25rem;
    font-weight: 600;
    margin-bottom: 0.5rem;
}

.empty-state p {
    color: var(--text-secondary, #6b7280);
    margin-bottom: 1.5rem;
}

/* Context Menu */
.context-menu {
    position: fixed;
    z-index: 1001;
    background: var(--bg-primary, #ffffff);
    border: 1px solid var(--border-color, #e5e7eb);
    border-radius: 8px;
    box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
    padding: 0.25rem;
    min-width: 160px;
}

.context-menu-item {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    width: 100%;
    padding: 0.5rem 0.75rem;
    background: none;
    border: none;
    border-radius: 4px;
    font-size: 0.8125rem;
    color: var(--text-primary, #1f2937);
    cursor: pointer;
    text-align: left;
}

.context-menu-item:hover {
    background: var(--bg-secondary, #f3f4f6);
}

.context-menu-divider {
    height: 1px;
    background: var(--border-color, #e5e7eb);
    margin: 0.25rem 0;
}

.context-menu-backdrop {
    position: fixed;
    inset: 0;
    z-index: 1000;
}

/* Dark Mode */
:global([data-theme="dark"]) .codeset-tabs-container {
    background: var(--bg-primary, #1f2937);
    border-color: var(--border-color, #4b5563);
}

:global([data-theme="dark"]) .context-menu {
    background: var(--bg-secondary, #374151);
    border-color: var(--border-color, #4b5563);
}
```

### CodeSetTabBar.razor

```razor
@namespace VisualEditorOpus.Components.CodeSet

<div class="tab-bar" @ondragover="HandleDragOver" @ondrop="HandleDrop">
    @foreach (var tab in GetOrderedTabs())
    {
        <button class="tab-item @(tab.IsActive ? "active" : "") @(tab.IsDirty ? "dirty" : "") @(tab.IsPinned ? "pinned" : "")"
                @onclick="() => OnTabSelect.InvokeAsync(tab.Id)"
                @oncontextmenu="e => HandleContextMenu(e, tab)"
                @oncontextmenu:preventDefault
                draggable="true"
                @ondragstart="() => HandleDragStart(tab)"
                @ondragend="HandleDragEnd">
            @if (tab.IsPinned)
            {
                <i class="bi bi-pin-fill pin-icon"></i>
            }
            <i class="bi @tab.Icon tab-icon"></i>
            <span class="tab-name">@tab.CodeSetName</span>
            <span class="tab-badge">@tab.ItemCount</span>
            @if (!tab.IsPinned)
            {
                <button class="tab-close"
                        @onclick="() => OnTabClose.InvokeAsync(tab.Id)"
                        @onclick:stopPropagation>
                    <i class="bi bi-x"></i>
                </button>
            }
            @if (tab.IsDirty)
            {
                <span class="dirty-indicator"></span>
            }
        </button>
    }
    <button class="tab-add" @onclick="OnTabAdd" title="Add CodeSet">
        <i class="bi bi-plus"></i>
    </button>
</div>

@code {
    [Parameter] public List<TabState> Tabs { get; set; } = new();
    [Parameter] public string? ActiveTabId { get; set; }
    [Parameter] public EventCallback<string> OnTabSelect { get; set; }
    [Parameter] public EventCallback<string> OnTabClose { get; set; }
    [Parameter] public EventCallback OnTabAdd { get; set; }
    [Parameter] public EventCallback<List<string>> OnTabReorder { get; set; }
    [Parameter] public EventCallback<(TabState Tab, double X, double Y)> OnTabContextMenu { get; set; }

    private TabState? _draggingTab;

    private IEnumerable<TabState> GetOrderedTabs()
    {
        // Pinned tabs first, then by order
        return Tabs.OrderByDescending(t => t.IsPinned).ThenBy(t => t.Order);
    }

    private void HandleDragStart(TabState tab)
    {
        if (tab.IsPinned) return;
        _draggingTab = tab;
    }

    private void HandleDragEnd()
    {
        _draggingTab = null;
    }

    private void HandleDragOver(DragEventArgs e)
    {
        e.DataTransfer.DropEffect = "move";
    }

    private async Task HandleDrop(DragEventArgs e)
    {
        if (_draggingTab == null) return;

        // Simple reorder - in production, calculate position from mouse
        var tabIds = Tabs.Select(t => t.Id).ToList();
        await OnTabReorder.InvokeAsync(tabIds);
    }

    private void HandleContextMenu(MouseEventArgs e, TabState tab)
    {
        OnTabContextMenu.InvokeAsync((tab, e.ClientX, e.ClientY));
    }
}
```

### CodeSetTabBar.razor.css

```css
.tab-bar {
    display: flex;
    align-items: center;
    gap: 0.25rem;
    padding: 0.75rem 1rem;
    background: var(--bg-secondary, #f9fafb);
    border-bottom: 1px solid var(--border-color, #e5e7eb);
    overflow-x: auto;
}

.tab-bar::-webkit-scrollbar {
    height: 4px;
}

.tab-bar::-webkit-scrollbar-track {
    background: transparent;
}

.tab-bar::-webkit-scrollbar-thumb {
    background: var(--border-color, #e5e7eb);
    border-radius: 2px;
}

.tab-item {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background: transparent;
    border: none;
    border-radius: 6px;
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--text-secondary, #6b7280);
    cursor: pointer;
    white-space: nowrap;
    transition: all 0.15s ease;
    position: relative;
}

.tab-item:hover {
    background: var(--bg-tertiary, #f3f4f6);
    color: var(--text-primary, #1f2937);
}

.tab-item.active {
    background: var(--bg-primary, #ffffff);
    color: var(--primary, #6366f1);
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}

.tab-item.dirty::after {
    content: '';
    position: absolute;
    top: 0.5rem;
    right: 0.5rem;
    width: 6px;
    height: 6px;
    background: var(--warning, #f59e0b);
    border-radius: 50%;
}

.tab-item.pinned {
    padding-left: 0.625rem;
}

.pin-icon {
    font-size: 0.625rem;
    color: var(--text-muted, #9ca3af);
}

.tab-icon {
    font-size: 1rem;
}

.tab-name {
    max-width: 120px;
    overflow: hidden;
    text-overflow: ellipsis;
}

.tab-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 1.25rem;
    height: 1.25rem;
    padding: 0 0.375rem;
    background: var(--bg-tertiary, #e5e7eb);
    border-radius: 9999px;
    font-size: 0.6875rem;
    font-weight: 600;
    color: var(--text-muted, #9ca3af);
}

.tab-item.active .tab-badge {
    background: var(--primary-light, #eef2ff);
    color: var(--primary, #6366f1);
}

.tab-close {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 1.25rem;
    height: 1.25rem;
    background: transparent;
    border: none;
    border-radius: 4px;
    color: var(--text-muted, #9ca3af);
    cursor: pointer;
    opacity: 0;
    transition: all 0.15s ease;
}

.tab-item:hover .tab-close,
.tab-item.active .tab-close {
    opacity: 1;
}

.tab-close:hover {
    background: var(--danger-light, #fee2e2);
    color: var(--danger, #ef4444);
}

.tab-add {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 2rem;
    height: 2rem;
    background: transparent;
    border: 1px dashed var(--border-color, #d1d5db);
    border-radius: 6px;
    color: var(--text-muted, #9ca3af);
    cursor: pointer;
    transition: all 0.15s ease;
    flex-shrink: 0;
}

.tab-add:hover {
    border-color: var(--primary, #6366f1);
    color: var(--primary, #6366f1);
    background: var(--primary-light, #eef2ff);
}

/* Dark Mode */
:global([data-theme="dark"]) .tab-bar {
    background: var(--bg-tertiary, #111827);
    border-color: var(--border-color, #4b5563);
}

:global([data-theme="dark"]) .tab-item.active {
    background: var(--bg-secondary, #374151);
}
```

### CodeSetDataEditor.razor

```razor
@namespace VisualEditorOpus.Components.CodeSet

<div class="data-editor">
    <div class="editor-toolbar">
        <div class="search-box">
            <i class="bi bi-search"></i>
            <input type="text"
                   placeholder="Search..."
                   value="@SearchTerm"
                   @oninput="HandleSearch" />
        </div>
        <div class="toolbar-actions">
            <button class="btn-icon" @onclick="OnImport" title="Import">
                <i class="bi bi-upload"></i>
            </button>
            <button class="btn-icon" @onclick="OnExport" title="Export">
                <i class="bi bi-download"></i>
            </button>
            <button class="btn-icon danger"
                    @onclick="DeleteSelected"
                    disabled="@(!SelectedItems.Any())"
                    title="Delete Selected">
                <i class="bi bi-trash"></i>
            </button>
        </div>
    </div>

    <div class="data-grid">
        <table>
            <thead>
                <tr>
                    <th style="width: 40px;"></th>
                    <th class="sortable @(SortColumn == "Code" ? "sorted" : "")"
                        @onclick="() => Sort(\"Code\")" style="width: 100px;">
                        Code
                        <i class="bi bi-caret-@(SortAscending ? "up" : "down")-fill sort-icon"></i>
                    </th>
                    <th class="sortable @(SortColumn == "DisplayName" ? "sorted" : "")"
                        @onclick="() => Sort(\"DisplayName\")">
                        Display Name
                        <i class="bi bi-caret-@(SortAscending ? "up" : "down")-fill sort-icon"></i>
                    </th>
                    <th>Description</th>
                    <th class="sortable @(SortColumn == "Order" ? "sorted" : "")"
                        @onclick="() => Sort(\"Order\")" style="width: 80px;">
                        Order
                        <i class="bi bi-caret-@(SortAscending ? "up" : "down")-fill sort-icon"></i>
                    </th>
                    <th style="width: 60px;"></th>
                </tr>
            </thead>
            <tbody>
                @foreach (var item in Items)
                {
                    <tr class="@(SelectedItems.Contains(item.Id) ? "selected" : "")">
                        <td>
                            <i class="bi bi-grip-vertical drag-handle"
                               draggable="true"
                               @ondragstart="() => StartDrag(item)"
                               @ondragend="EndDrag"></i>
                        </td>
                        <td>
                            <input type="text"
                                   class="inline-input code-input"
                                   value="@item.Code"
                                   @onchange="e => UpdateCode(item, e.Value?.ToString())" />
                        </td>
                        <td>
                            <input type="text"
                                   class="inline-input"
                                   value="@item.DisplayName"
                                   @onchange="e => UpdateDisplayName(item, e.Value?.ToString())" />
                        </td>
                        <td>
                            <input type="text"
                                   class="inline-input"
                                   value="@item.Description"
                                   @onchange="e => UpdateDescription(item, e.Value?.ToString())" />
                        </td>
                        <td>
                            <input type="number"
                                   class="inline-input order-input"
                                   value="@item.Order"
                                   @onchange="e => UpdateOrder(item, e.Value?.ToString())" />
                        </td>
                        <td>
                            <div class="row-actions">
                                <button class="row-action" @onclick="() => EditItem(item)" title="Edit">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="row-action danger" @onclick="() => OnItemDelete.InvokeAsync(item)" title="Delete">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
        <button class="add-row" @onclick="OnItemAdd">
            <i class="bi bi-plus-lg"></i>
            Add New Item
        </button>
    </div>
</div>

@code {
    [Parameter] public List<CodeSetItem> Items { get; set; } = new();
    [Parameter] public string SearchTerm { get; set; } = "";
    [Parameter] public string SortColumn { get; set; } = "Order";
    [Parameter] public bool SortAscending { get; set; } = true;
    [Parameter] public EventCallback OnItemAdd { get; set; }
    [Parameter] public EventCallback<CodeSetItem> OnItemUpdate { get; set; }
    [Parameter] public EventCallback<CodeSetItem> OnItemDelete { get; set; }
    [Parameter] public EventCallback<string> OnSearch { get; set; }
    [Parameter] public EventCallback<(string, bool)> OnSort { get; set; }
    [Parameter] public EventCallback OnImport { get; set; }
    [Parameter] public EventCallback OnExport { get; set; }

    private HashSet<string> SelectedItems { get; set; } = new();
    private CodeSetItem? _draggingItem;

    private async Task HandleSearch(ChangeEventArgs e)
    {
        await OnSearch.InvokeAsync(e.Value?.ToString() ?? "");
    }

    private async Task Sort(string column)
    {
        var ascending = SortColumn == column ? !SortAscending : true;
        await OnSort.InvokeAsync((column, ascending));
    }

    private async Task UpdateCode(CodeSetItem item, string? value)
    {
        if (value != null && value != item.Code)
        {
            var updated = item with { Code = value };
            await OnItemUpdate.InvokeAsync(updated);
        }
    }

    private async Task UpdateDisplayName(CodeSetItem item, string? value)
    {
        if (value != null && value != item.DisplayName)
        {
            var updated = item with { DisplayName = value };
            await OnItemUpdate.InvokeAsync(updated);
        }
    }

    private async Task UpdateDescription(CodeSetItem item, string? value)
    {
        var updated = item with { Description = value };
        await OnItemUpdate.InvokeAsync(updated);
    }

    private async Task UpdateOrder(CodeSetItem item, string? value)
    {
        if (int.TryParse(value, out var order) && order != item.Order)
        {
            var updated = item with { Order = order };
            await OnItemUpdate.InvokeAsync(updated);
        }
    }

    private void EditItem(CodeSetItem item)
    {
        // Open edit modal or expand row
    }

    private async Task DeleteSelected()
    {
        foreach (var id in SelectedItems.ToList())
        {
            var item = Items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                await OnItemDelete.InvokeAsync(item);
            }
        }
        SelectedItems.Clear();
    }

    private void StartDrag(CodeSetItem item)
    {
        _draggingItem = item;
    }

    private void EndDrag()
    {
        _draggingItem = null;
    }
}
```

## Usage Example

```razor
@page "/codeset-manager"
@inject ICodeSetService CodeSetService
@inject ITabStateService TabStateService

<div class="codeset-manager-layout">
    <aside class="sidebar">
        <CodeSetList CodeSets="CodeSets"
                     OnCodeSetSelect="OpenCodeSet"
                     OnCodeSetCreate="CreateCodeSet" />
    </aside>

    <main class="main-content">
        <CodeSetTabs />
    </main>
</div>

@code {
    private List<CodeSet> CodeSets { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        CodeSets = await CodeSetService.GetAllCodeSetsAsync();
    }

    private void OpenCodeSet(CodeSet codeSet)
    {
        TabStateService.OpenTab(codeSet);
    }

    private async Task CreateCodeSet()
    {
        var newCodeSet = new CodeSet { Name = "New CodeSet" };
        var created = await CodeSetService.CreateCodeSetAsync(newCodeSet);
        CodeSets.Add(created);
        TabStateService.OpenTab(created);
    }
}
```

## Claude Prompt for Implementation

```
Implement the CodeSetTabs component for managing multiple CodeSets in a tabbed interface.

Requirements:
1. Tab bar with draggable, closable tabs
2. Tab state management with active/dirty/pinned states
3. Context menu for tab operations (close, close others, pin)
4. Inline data editing grid with drag-to-reorder rows
5. Search and sort functionality
6. Properties sidebar showing CodeSet info and field bindings
7. Keyboard navigation support
8. Dark mode support

Use the existing design system:
- CSS variables for colors (--primary: #6366f1, etc.)
- Consistent tab styling with badges and close buttons
- Inline input fields for data editing
- Bootstrap Icons for iconography

Tab state should persist across component re-renders.
Support opening the same CodeSet only once (activate existing tab).

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `CodeSetTabs-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for testing tab creation
- Tab activation and switching testing
- Tab close functionality testing
- Dirty state indicator testing
- Pin/unpin tab functionality testing
- Tab drag-and-drop reordering testing
- Context menu operations (Close, Close Others, Close All)
- Inline data editing in tab content
- Search and sort within active tab
- Tab state persistence verification
- Opening same CodeSet activates existing tab

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- TabState model file creation
- ITabStateService interface and implementation
- Service registration in DI as singleton
- CSS file imports
- Component registration in _Imports.razor
- Tab state persistence to local storage (optional)
- Keyboard shortcut handlers (Ctrl+W, Ctrl+Tab)
- Integration with CodeSetList sidebar component

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] Tab bar displays open tabs
- [ ] Clicking tab activates it
- [ ] Tab shows icon and name
- [ ] Tab badge shows item count
- [ ] Close button closes tab
- [ ] Dirty indicator shows for modified tabs
- [ ] Pin icon appears for pinned tabs
- [ ] Pinned tabs cannot be closed
- [ ] Context menu appears on right-click
- [ ] Close Others closes all except clicked tab
- [ ] Close All closes all tabs
- [ ] Tab reordering via drag works
- [ ] Opening existing CodeSet activates its tab
- [ ] Search filters items in active tab
- [ ] Sort changes order in active tab
- [ ] Inline editing marks tab as dirty
- [ ] Dark mode styling correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

## Integration Notes

1. **Tab Persistence**: Consider saving tab state to local storage
2. **Dirty State**: Track changes and prompt before closing dirty tabs
3. **Keyboard Shortcuts**: Ctrl+W to close, Ctrl+Tab to switch tabs
4. **Performance**: Virtualize large item lists in the data grid
5. **Drag and Drop**: Use Sortable.js or similar for row reordering
