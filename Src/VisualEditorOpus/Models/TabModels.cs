namespace VisualEditorOpus.Models;

/// <summary>
/// Represents the state of an open CodeSet tab
/// </summary>
public record TabState
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public int CodeSetId { get; init; }
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

/// <summary>
/// Event args for tab state changes
/// </summary>
public class TabChangedEventArgs : EventArgs
{
    public TabState Tab { get; }
    public TabChangeType ChangeType { get; }

    public TabChangedEventArgs(TabState tab, TabChangeType changeType)
    {
        Tab = tab;
        ChangeType = changeType;
    }
}

public enum TabChangeType
{
    Opened,
    Closed,
    Activated,
    Updated,
    Reordered,
    Pinned,
    Unpinned
}
