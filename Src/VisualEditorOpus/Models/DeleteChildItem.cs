namespace VisualEditorOpus.Models;

/// <summary>
/// Represents a child item that will be deleted along with its parent.
/// Used in the ConfirmDeleteModal to display cascading deletes.
/// </summary>
/// <param name="Name">The display name of the item to be deleted</param>
/// <param name="Icon">Bootstrap icon class (e.g., "bi-file", "bi-input-cursor-text")</param>
public record DeleteChildItem(string Name, string Icon = "bi-file");
