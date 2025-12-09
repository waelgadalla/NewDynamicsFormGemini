# D.3 JSON Preview - Implementation Plan

> **Task**: JSON Preview Component
> **Location**: `Src/VisualEditorOpus/Components/Preview/`
> **Priority**: High
> **Estimated Effort**: 4-5 hours
> **Delegation**: 80% AI

---

## Overview

The JSON Preview provides a syntax-highlighted, read-only (or editable) view of the form schema in JSON format. It supports features like search, copy, download, import, collapsible sections, validation feedback, and diff view for comparing changes.

---

## Components to Create

### JsonPreview.razor (Main Container)

```razor
@namespace VisualEditorOpus.Components.Preview
@inject IJSRuntime JS
@implements IAsyncDisposable

<div class="json-preview-container">
    <JsonToolbar
        ViewMode="@CurrentViewMode"
        IsValid="@IsJsonValid"
        Stats="@JsonStats"
        OnViewModeChange="SetViewMode"
        OnSearch="ToggleSearch"
        OnCopy="CopyJson"
        OnDownload="DownloadJson"
        OnImport="ImportJson"
        OnFormat="FormatJson" />

    @if (ShowSearch)
    {
        <JsonSearchBar
            SearchText="@SearchText"
            MatchCount="@SearchMatches.Count"
            CurrentMatch="@CurrentSearchIndex"
            OnSearchChange="HandleSearch"
            OnNext="NextMatch"
            OnPrevious="PreviousMatch"
            OnClose="() => ShowSearch = false" />
    }

    @if (ShowPathNavigator && CurrentPath.Any())
    {
        <PathNavigator
            Path="@CurrentPath"
            OnNavigate="NavigateToPath" />
    }

    @if (CurrentViewMode == JsonViewMode.Diff)
    {
        <JsonDiffView
            Original="@OriginalJson"
            Current="@CurrentJson" />
    }
    else
    {
        <div class="json-editor-area">
            <LineNumbers
                LineCount="@LineCount"
                ErrorLines="@ValidationErrorLines" />

            <div class="json-content @(IsEditable ? "editable" : "")"
                 @ref="jsonContentRef"
                 contenteditable="@IsEditable"
                 @oninput="HandleJsonEdit"
                 @onclick="HandleContentClick">
                @((MarkupString)HighlightedJson)
            </div>

            @if (ShowMinimap)
            {
                <JsonMinimap
                    Json="@CurrentJson"
                    ViewportPosition="@ScrollPosition"
                    OnNavigate="ScrollToPosition" />
            }
        </div>
    }

    @if (ValidationErrors.Any())
    {
        <ValidationPanel
            Errors="@ValidationErrors"
            OnErrorClick="NavigateToError"
            OnClose="() => ValidationErrors.Clear()" />
    }

    <JsonStatusBar
        IsValid="@IsJsonValid"
        LastModified="@LastModified"
        CursorPosition="@CursorPosition"
        Encoding="UTF-8"
        FileType="JSON" />
</div>

@code {
    [Parameter] public FormModuleSchema? Module { get; set; }
    [Parameter] public EventCallback<FormModuleSchema> OnModuleChanged { get; set; }
    [Parameter] public bool IsEditable { get; set; } = false;

    private ElementReference jsonContentRef;
    private DotNetObjectReference<JsonPreview>? objRef;

    private JsonViewMode CurrentViewMode { get; set; } = JsonViewMode.Formatted;
    private string CurrentJson { get; set; } = "";
    private string OriginalJson { get; set; } = "";
    private string HighlightedJson { get; set; } = "";
    private bool IsJsonValid { get; set; } = true;
    private int LineCount { get; set; } = 0;
    private JsonStats JsonStats { get; set; } = new();

    // Search
    private bool ShowSearch { get; set; } = false;
    private string SearchText { get; set; } = "";
    private List<SearchMatch> SearchMatches { get; set; } = new();
    private int CurrentSearchIndex { get; set; } = 0;

    // Navigation
    private bool ShowPathNavigator { get; set; } = false;
    private List<string> CurrentPath { get; set; } = new();
    private bool ShowMinimap { get; set; } = true;
    private double ScrollPosition { get; set; } = 0;

    // Validation
    private List<JsonValidationError> ValidationErrors { get; set; } = new();
    private HashSet<int> ValidationErrorLines => ValidationErrors.Select(e => e.Line).ToHashSet();

    // Status
    private DateTime LastModified { get; set; } = DateTime.Now;
    private (int Line, int Column) CursorPosition { get; set; } = (1, 1);

    protected override void OnParametersSet()
    {
        if (Module != null)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            CurrentJson = JsonSerializer.Serialize(Module, options);
            OriginalJson = CurrentJson;
            UpdateDisplay();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            objRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("initJsonPreview", objRef, jsonContentRef);
        }
    }

    private void UpdateDisplay()
    {
        var lines = CurrentJson.Split('\n');
        LineCount = lines.Length;
        HighlightedJson = SyntaxHighlight(CurrentJson);

        // Calculate stats
        JsonStats = new JsonStats
        {
            Size = Encoding.UTF8.GetByteCount(CurrentJson),
            Lines = LineCount,
            FieldCount = Module?.Fields?.Count ?? 0
        };

        ValidateJson();
    }

    private string SyntaxHighlight(string json)
    {
        if (string.IsNullOrEmpty(json)) return "";

        var escaped = System.Web.HttpUtility.HtmlEncode(json);

        // Apply syntax highlighting
        escaped = Regex.Replace(escaped, @"&quot;([^&]+)&quot;:",
            "<span class=\"json-key\">\"$1\"</span>:");
        escaped = Regex.Replace(escaped, @": &quot;([^&]*)&quot;",
            ": <span class=\"json-string\">\"$1\"</span>");
        escaped = Regex.Replace(escaped, @": (\d+)",
            ": <span class=\"json-number\">$1</span>");
        escaped = Regex.Replace(escaped, @": (true|false)",
            ": <span class=\"json-boolean\">$1</span>");
        escaped = Regex.Replace(escaped, @": (null)",
            ": <span class=\"json-null\">$1</span>");
        escaped = Regex.Replace(escaped, @"([{}\[\]])",
            "<span class=\"json-bracket\">$1</span>");

        return escaped;
    }

    private void ValidateJson()
    {
        ValidationErrors.Clear();
        try
        {
            JsonDocument.Parse(CurrentJson);
            IsJsonValid = true;
        }
        catch (JsonException ex)
        {
            IsJsonValid = false;
            ValidationErrors.Add(new JsonValidationError
            {
                Message = ex.Message,
                Line = (int)(ex.LineNumber ?? 1),
                Column = (int)(ex.BytePositionInLine ?? 1)
            });
        }
    }

    private void SetViewMode(JsonViewMode mode)
    {
        CurrentViewMode = mode;
    }

    private void ToggleSearch()
    {
        ShowSearch = !ShowSearch;
        if (!ShowSearch)
        {
            SearchText = "";
            SearchMatches.Clear();
        }
    }

    private void HandleSearch(string text)
    {
        SearchText = text;
        SearchMatches.Clear();
        CurrentSearchIndex = 0;

        if (string.IsNullOrEmpty(text)) return;

        var regex = new Regex(Regex.Escape(text), RegexOptions.IgnoreCase);
        var matches = regex.Matches(CurrentJson);

        foreach (Match match in matches)
        {
            var beforeMatch = CurrentJson[..match.Index];
            var line = beforeMatch.Count(c => c == '\n') + 1;
            var column = match.Index - beforeMatch.LastIndexOf('\n');

            SearchMatches.Add(new SearchMatch
            {
                Index = match.Index,
                Length = match.Length,
                Line = line,
                Column = column
            });
        }
    }

    private void NextMatch()
    {
        if (SearchMatches.Any())
        {
            CurrentSearchIndex = (CurrentSearchIndex + 1) % SearchMatches.Count;
        }
    }

    private void PreviousMatch()
    {
        if (SearchMatches.Any())
        {
            CurrentSearchIndex = CurrentSearchIndex == 0
                ? SearchMatches.Count - 1
                : CurrentSearchIndex - 1;
        }
    }

    private async Task CopyJson()
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", CurrentJson);
    }

    private async Task DownloadJson()
    {
        var fileName = $"{Module?.Name ?? "form"}-schema.json";
        await JS.InvokeVoidAsync("downloadJson", CurrentJson, fileName);
    }

    private async Task ImportJson()
    {
        await JS.InvokeVoidAsync("triggerFileInput", objRef);
    }

    [JSInvokable]
    public async Task HandleFileImport(string jsonContent)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var module = JsonSerializer.Deserialize<FormModuleSchema>(jsonContent, options);

            if (module != null)
            {
                await OnModuleChanged.InvokeAsync(module);
            }
        }
        catch (JsonException ex)
        {
            ValidationErrors.Add(new JsonValidationError
            {
                Message = $"Import failed: {ex.Message}",
                Line = 1,
                Column = 1
            });
        }
    }

    private void FormatJson()
    {
        try
        {
            var doc = JsonDocument.Parse(CurrentJson);
            var options = new JsonSerializerOptions { WriteIndented = true };
            CurrentJson = JsonSerializer.Serialize(doc.RootElement, options);
            UpdateDisplay();
        }
        catch { }
    }

    private void HandleJsonEdit(ChangeEventArgs e)
    {
        if (!IsEditable) return;

        CurrentJson = e.Value?.ToString() ?? "";
        LastModified = DateTime.Now;
        UpdateDisplay();
    }

    private void HandleContentClick(MouseEventArgs e)
    {
        // Update path navigator based on click position
        ShowPathNavigator = true;
    }

    private void NavigateToPath(List<string> path)
    {
        // Navigate to specific path in JSON
    }

    private void NavigateToError(JsonValidationError error)
    {
        // Scroll to error line
    }

    private void ScrollToPosition(double position)
    {
        ScrollPosition = position;
    }

    public async ValueTask DisposeAsync()
    {
        if (objRef != null)
        {
            await JS.InvokeVoidAsync("disposeJsonPreview");
            objRef.Dispose();
        }
    }
}
```

### JsonToolbar.razor

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="json-toolbar">
    <div class="json-toolbar-left">
        <div class="view-toggle">
            <button class="view-toggle-btn @(ViewMode == JsonViewMode.Formatted ? "active" : "")"
                    @onclick="() => OnViewModeChange.InvokeAsync(JsonViewMode.Formatted)">
                Formatted
            </button>
            <button class="view-toggle-btn @(ViewMode == JsonViewMode.Raw ? "active" : "")"
                    @onclick="() => OnViewModeChange.InvokeAsync(JsonViewMode.Raw)">
                Raw
            </button>
            <button class="view-toggle-btn @(ViewMode == JsonViewMode.Diff ? "active" : "")"
                    @onclick="() => OnViewModeChange.InvokeAsync(JsonViewMode.Diff)">
                Diff
            </button>
        </div>
        <div class="json-stats">
            <span class="stat-item">
                <i class="bi bi-file-code"></i>
                <span class="stat-value">@FormatSize(Stats.Size)</span>
            </span>
            <span class="stat-item">
                <i class="bi bi-list-ol"></i>
                <span class="stat-value">@Stats.Lines lines</span>
            </span>
            <span class="stat-item">
                <i class="bi bi-diagram-3"></i>
                <span class="stat-value">@Stats.FieldCount fields</span>
            </span>
        </div>
    </div>
    <div class="json-toolbar-right">
        <button class="toolbar-btn" @onclick="OnSearch" title="Search (Ctrl+F)">
            <i class="bi bi-search"></i>
            Search
        </button>
        <div class="toolbar-divider"></div>
        <button class="toolbar-btn" @onclick="OnCopy" title="Copy JSON">
            <i class="bi bi-clipboard"></i>
            Copy
        </button>
        <button class="toolbar-btn" @onclick="OnDownload" title="Download JSON">
            <i class="bi bi-download"></i>
            Download
        </button>
        <button class="toolbar-btn primary" @onclick="OnImport" title="Import JSON">
            <i class="bi bi-upload"></i>
            Import
        </button>
    </div>
</div>

@code {
    [Parameter] public JsonViewMode ViewMode { get; set; }
    [Parameter] public bool IsValid { get; set; }
    [Parameter] public JsonStats Stats { get; set; } = new();
    [Parameter] public EventCallback<JsonViewMode> OnViewModeChange { get; set; }
    [Parameter] public EventCallback OnSearch { get; set; }
    [Parameter] public EventCallback OnCopy { get; set; }
    [Parameter] public EventCallback OnDownload { get; set; }
    [Parameter] public EventCallback OnImport { get; set; }
    [Parameter] public EventCallback OnFormat { get; set; }

    private string FormatSize(int bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }
}
```

### JsonSearchBar.razor

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="search-bar">
    <div class="search-input-wrapper">
        <i class="bi bi-search"></i>
        <input type="text"
               class="search-input"
               placeholder="Search in JSON..."
               value="@SearchText"
               @oninput="e => OnSearchChange.InvokeAsync(e.Value?.ToString())"
               @onkeydown="HandleKeyDown" />
    </div>
    @if (MatchCount > 0)
    {
        <span class="search-count">@(CurrentMatch + 1) of @MatchCount matches</span>
    }
    else if (!string.IsNullOrEmpty(SearchText))
    {
        <span class="search-count no-matches">No matches</span>
    }
    <div class="search-nav">
        <button @onclick="OnPrevious" disabled="@(MatchCount == 0)" title="Previous (Shift+Enter)">
            <i class="bi bi-chevron-up"></i>
        </button>
        <button @onclick="OnNext" disabled="@(MatchCount == 0)" title="Next (Enter)">
            <i class="bi bi-chevron-down"></i>
        </button>
    </div>
    <button class="search-close" @onclick="OnClose">&times;</button>
</div>

@code {
    [Parameter] public string SearchText { get; set; } = "";
    [Parameter] public int MatchCount { get; set; }
    [Parameter] public int CurrentMatch { get; set; }
    [Parameter] public EventCallback<string> OnSearchChange { get; set; }
    [Parameter] public EventCallback OnNext { get; set; }
    [Parameter] public EventCallback OnPrevious { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (e.ShiftKey)
                await OnPrevious.InvokeAsync();
            else
                await OnNext.InvokeAsync();
        }
        else if (e.Key == "Escape")
        {
            await OnClose.InvokeAsync();
        }
    }
}
```

### LineNumbers.razor

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="line-numbers">
    @for (int i = 1; i <= LineCount; i++)
    {
        var lineNum = i;
        <div class="line-number @(ErrorLines.Contains(lineNum) ? "error" : "")">
            @lineNum
        </div>
    }
</div>

@code {
    [Parameter] public int LineCount { get; set; }
    [Parameter] public HashSet<int> ErrorLines { get; set; } = new();
}
```

### JsonDiffView.razor

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="diff-view active">
    <div class="diff-pane">
        <div class="diff-header">
            <i class="bi bi-file-earmark-code"></i> Original (Saved)
        </div>
        <div class="diff-content">
            @foreach (var line in DiffLines)
            {
                <div class="diff-line @(line.Type == DiffLineType.Removed ? "removed" : "")">
                    @line.Content
                </div>
            }
        </div>
    </div>
    <div class="diff-pane">
        <div class="diff-header">
            <i class="bi bi-file-earmark-code-fill"></i> Current (Unsaved)
        </div>
        <div class="diff-content">
            @foreach (var line in DiffLines)
            {
                <div class="diff-line @(line.Type == DiffLineType.Added ? "added" : line.Type == DiffLineType.Modified ? "modified" : "")">
                    @line.NewContent
                </div>
            }
        </div>
    </div>
</div>

@code {
    [Parameter] public string Original { get; set; } = "";
    [Parameter] public string Current { get; set; } = "";

    private List<DiffLine> DiffLines { get; set; } = new();

    protected override void OnParametersSet()
    {
        ComputeDiff();
    }

    private void ComputeDiff()
    {
        DiffLines.Clear();

        var originalLines = Original.Split('\n');
        var currentLines = Current.Split('\n');

        // Simple line-by-line diff (for production, use a proper diff algorithm)
        var maxLines = Math.Max(originalLines.Length, currentLines.Length);

        for (int i = 0; i < maxLines; i++)
        {
            var origLine = i < originalLines.Length ? originalLines[i] : null;
            var currLine = i < currentLines.Length ? currentLines[i] : null;

            if (origLine == currLine)
            {
                DiffLines.Add(new DiffLine
                {
                    Content = origLine ?? "",
                    NewContent = currLine ?? "",
                    Type = DiffLineType.Unchanged
                });
            }
            else if (origLine == null)
            {
                DiffLines.Add(new DiffLine
                {
                    Content = "",
                    NewContent = currLine ?? "",
                    Type = DiffLineType.Added
                });
            }
            else if (currLine == null)
            {
                DiffLines.Add(new DiffLine
                {
                    Content = origLine,
                    NewContent = "",
                    Type = DiffLineType.Removed
                });
            }
            else
            {
                DiffLines.Add(new DiffLine
                {
                    Content = origLine,
                    NewContent = currLine,
                    Type = DiffLineType.Modified
                });
            }
        }
    }
}
```

### ValidationPanel.razor

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="validation-panel">
    <div class="validation-header">
        <span class="validation-title">
            <i class="bi bi-exclamation-circle-fill"></i>
            @Errors.Count Error@(Errors.Count != 1 ? "s" : "") Found
        </span>
        <button class="validation-close" @onclick="OnClose">&times;</button>
    </div>
    <div class="validation-list">
        @foreach (var error in Errors)
        {
            <div class="validation-error" @onclick="() => OnErrorClick.InvokeAsync(error)">
                <i class="bi bi-x-circle-fill"></i>
                <div class="validation-error-content">
                    <div class="validation-error-msg">@error.Message</div>
                    <div class="validation-error-loc">Line @error.Line, Column @error.Column</div>
                </div>
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public List<JsonValidationError> Errors { get; set; } = new();
    [Parameter] public EventCallback<JsonValidationError> OnErrorClick { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
}
```

### JsonStatusBar.razor

```razor
@namespace VisualEditorOpus.Components.Preview

<div class="json-status-bar">
    <div class="status-left">
        <span class="status-item @(IsValid ? "status-valid" : "status-invalid")">
            <i class="bi bi-@(IsValid ? "check-circle-fill" : "x-circle-fill")"></i>
            @(IsValid ? "Valid JSON" : "Invalid JSON")
        </span>
        <span class="status-item">
            <i class="bi bi-clock"></i>
            Last modified: @GetRelativeTime(LastModified)
        </span>
    </div>
    <div class="status-right">
        <span class="cursor-position">Ln @CursorPosition.Line, Col @CursorPosition.Column</span>
        <span class="status-item">@Encoding</span>
        <span class="status-item">@FileType</span>
    </div>
</div>

@code {
    [Parameter] public bool IsValid { get; set; }
    [Parameter] public DateTime LastModified { get; set; }
    [Parameter] public (int Line, int Column) CursorPosition { get; set; }
    [Parameter] public string Encoding { get; set; } = "UTF-8";
    [Parameter] public string FileType { get; set; } = "JSON";

    private string GetRelativeTime(DateTime time)
    {
        var diff = DateTime.Now - time;

        if (diff.TotalSeconds < 60) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hour(s) ago";
        return time.ToString("MMM d, yyyy");
    }
}
```

---

## Supporting Types

```csharp
// Enums/JsonViewMode.cs
public enum JsonViewMode
{
    Formatted,
    Raw,
    Diff
}

// Models/JsonStats.cs
public record JsonStats
{
    public int Size { get; init; }
    public int Lines { get; init; }
    public int FieldCount { get; init; }
}

// Models/JsonValidationError.cs
public record JsonValidationError
{
    public string Message { get; init; } = "";
    public int Line { get; init; }
    public int Column { get; init; }
}

// Models/SearchMatch.cs
public record SearchMatch
{
    public int Index { get; init; }
    public int Length { get; init; }
    public int Line { get; init; }
    public int Column { get; init; }
}

// Models/DiffLine.cs
public record DiffLine
{
    public string Content { get; init; } = "";
    public string NewContent { get; init; } = "";
    public DiffLineType Type { get; init; }
}

public enum DiffLineType
{
    Unchanged,
    Added,
    Removed,
    Modified
}
```

---

## CSS Styles

```css
/* ===== JSON PREVIEW CONTAINER ===== */
.json-preview-container {
    display: flex;
    flex-direction: column;
    height: 100%;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-lg);
    overflow: hidden;
}

/* ===== JSON TOOLBAR ===== */
.json-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 16px;
    background: var(--bg-tertiary);
    border-bottom: 1px solid var(--border-color);
}

.json-toolbar-left {
    display: flex;
    align-items: center;
    gap: 12px;
}

.json-stats {
    display: flex;
    align-items: center;
    gap: 16px;
}

.stat-item {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 12px;
    color: var(--text-secondary);
}

.stat-value {
    font-weight: 600;
    color: var(--text-primary);
}

.json-toolbar-right {
    display: flex;
    align-items: center;
    gap: 8px;
}

.toolbar-btn {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 8px 12px;
    border: 1px solid var(--border-color);
    background: var(--bg-primary);
    border-radius: var(--radius-md);
    font-size: 13px;
    color: var(--text-secondary);
    cursor: pointer;
    transition: all 0.15s;
}

.toolbar-btn:hover {
    border-color: var(--primary);
    color: var(--primary);
    background: var(--primary-light);
}

.toolbar-btn.primary {
    background: var(--primary);
    border-color: var(--primary);
    color: white;
}

.toolbar-btn.primary:hover {
    background: var(--primary-hover);
}

.toolbar-divider {
    width: 1px;
    height: 24px;
    background: var(--border-color);
}

/* ===== VIEW MODE TOGGLE ===== */
.view-toggle {
    display: flex;
    gap: 2px;
    padding: 3px;
    background: var(--bg-secondary);
    border-radius: var(--radius-md);
}

.view-toggle-btn {
    padding: 6px 12px;
    border: none;
    background: transparent;
    border-radius: var(--radius-sm);
    font-size: 12px;
    font-weight: 500;
    color: var(--text-secondary);
    cursor: pointer;
    transition: all 0.15s;
}

.view-toggle-btn.active {
    background: var(--bg-primary);
    color: var(--primary);
    box-shadow: var(--shadow-sm);
}

/* ===== JSON EDITOR AREA ===== */
.json-editor-area {
    flex: 1;
    display: flex;
    overflow: hidden;
}

/* Line Numbers */
.line-numbers {
    width: 50px;
    background: var(--bg-tertiary);
    border-right: 1px solid var(--border-color);
    padding: 16px 0;
    overflow: hidden;
    user-select: none;
}

.line-number {
    height: 24px;
    line-height: 24px;
    font-size: 12px;
    font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', monospace;
    color: var(--text-muted);
    text-align: right;
    padding-right: 12px;
}

.line-number.error {
    background: var(--error-light);
    color: var(--error);
}

/* JSON Content */
.json-content {
    flex: 1;
    padding: 16px;
    overflow: auto;
    font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', monospace;
    font-size: 13px;
    line-height: 24px;
    white-space: pre;
}

.json-content.editable {
    white-space: pre-wrap;
    word-break: break-all;
    outline: none;
}

.json-content.editable:focus {
    background: var(--bg-secondary);
}

/* JSON Syntax Highlighting */
.json-key {
    color: #0550AE;
}

.json-string {
    color: #0A3069;
}

.json-number {
    color: #0550AE;
}

.json-boolean {
    color: #CF222E;
}

.json-null {
    color: #8250DF;
}

.json-bracket {
    color: #24292F;
}

[data-theme="dark"] .json-key {
    color: #79C0FF;
}

[data-theme="dark"] .json-string {
    color: #A5D6FF;
}

[data-theme="dark"] .json-number {
    color: #79C0FF;
}

[data-theme="dark"] .json-boolean {
    color: #FF7B72;
}

[data-theme="dark"] .json-null {
    color: #D2A8FF;
}

[data-theme="dark"] .json-bracket {
    color: #C9D1D9;
}

/* ===== SEARCH BAR ===== */
.search-bar {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 16px;
    background: var(--bg-secondary);
    border-bottom: 1px solid var(--border-color);
}

.search-input-wrapper {
    flex: 1;
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 12px;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
}

.search-input-wrapper:focus-within {
    border-color: var(--primary);
    box-shadow: 0 0 0 3px var(--primary-light);
}

.search-input {
    flex: 1;
    border: none;
    background: transparent;
    font-size: 13px;
    color: var(--text-primary);
    outline: none;
}

.search-count {
    font-size: 12px;
    color: var(--text-muted);
    padding: 0 8px;
}

.search-count.no-matches {
    color: var(--error);
}

.search-nav {
    display: flex;
    gap: 4px;
}

.search-nav button {
    padding: 4px 8px;
    border: 1px solid var(--border-color);
    background: var(--bg-primary);
    border-radius: var(--radius-sm);
    cursor: pointer;
    color: var(--text-secondary);
}

.search-nav button:hover:not(:disabled) {
    background: var(--bg-tertiary);
}

.search-nav button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.search-close {
    padding: 4px 8px;
    border: none;
    background: transparent;
    cursor: pointer;
    color: var(--text-muted);
    font-size: 18px;
}

/* ===== STATUS BAR ===== */
.json-status-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 8px 16px;
    background: var(--bg-tertiary);
    border-top: 1px solid var(--border-color);
    font-size: 12px;
}

.status-left,
.status-right {
    display: flex;
    align-items: center;
    gap: 16px;
}

.status-item {
    display: flex;
    align-items: center;
    gap: 6px;
    color: var(--text-secondary);
}

.status-valid {
    color: var(--success);
}

.status-invalid {
    color: var(--error);
}

.cursor-position {
    color: var(--text-muted);
}

/* ===== VALIDATION PANEL ===== */
.validation-panel {
    background: var(--bg-secondary);
    border-top: 1px solid var(--border-color);
    max-height: 150px;
    overflow: auto;
}

.validation-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 8px 16px;
    background: var(--error-light);
    border-bottom: 1px solid var(--border-color);
}

.validation-title {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 13px;
    font-weight: 600;
    color: var(--error);
}

.validation-close {
    background: transparent;
    border: none;
    cursor: pointer;
    color: var(--text-muted);
    font-size: 16px;
}

.validation-list {
    padding: 8px 16px;
}

.validation-error {
    display: flex;
    align-items: flex-start;
    gap: 8px;
    padding: 8px;
    border-radius: var(--radius-sm);
    cursor: pointer;
}

.validation-error:hover {
    background: var(--bg-tertiary);
}

.validation-error i {
    color: var(--error);
    margin-top: 2px;
}

.validation-error-content {
    flex: 1;
}

.validation-error-msg {
    font-size: 13px;
    color: var(--text-primary);
}

.validation-error-loc {
    font-size: 11px;
    color: var(--text-muted);
    margin-top: 2px;
}

/* ===== DIFF VIEW ===== */
.diff-view {
    display: none;
    flex: 1;
    overflow: hidden;
}

.diff-view.active {
    display: flex;
}

.diff-pane {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.diff-pane:first-child {
    border-right: 1px solid var(--border-color);
}

.diff-header {
    padding: 8px 16px;
    background: var(--bg-tertiary);
    border-bottom: 1px solid var(--border-color);
    font-size: 12px;
    font-weight: 600;
    color: var(--text-secondary);
}

.diff-content {
    flex: 1;
    overflow: auto;
    padding: 16px;
    font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', monospace;
    font-size: 13px;
    line-height: 24px;
}

.diff-line {
    padding: 0 8px;
    margin: 0 -8px;
}

.diff-line.added {
    background: var(--success-light);
}

.diff-line.removed {
    background: var(--error-light);
}

.diff-line.modified {
    background: var(--warning-light);
}
```

---

## JavaScript Interop

```javascript
// wwwroot/js/json-preview.js

let dotNetRef = null;
let contentElement = null;

window.initJsonPreview = (objRef, element) => {
    dotNetRef = objRef;
    contentElement = element;

    // Setup keyboard shortcuts
    document.addEventListener('keydown', handleKeyDown);

    // Setup scroll sync
    if (contentElement) {
        contentElement.addEventListener('scroll', handleScroll);
    }
};

window.disposeJsonPreview = () => {
    document.removeEventListener('keydown', handleKeyDown);
    if (contentElement) {
        contentElement.removeEventListener('scroll', handleScroll);
    }
    dotNetRef = null;
    contentElement = null;
};

function handleKeyDown(e) {
    if (e.ctrlKey && e.key === 'f') {
        e.preventDefault();
        dotNetRef?.invokeMethodAsync('ToggleSearch');
    }
}

function handleScroll() {
    const scrollPosition = contentElement.scrollTop / contentElement.scrollHeight;
    dotNetRef?.invokeMethodAsync('UpdateScrollPosition', scrollPosition);
}

window.downloadJson = (content, filename) => {
    const blob = new Blob([content], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
};

window.triggerFileInput = (objRef) => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';
    input.onchange = async (e) => {
        const file = e.target.files[0];
        if (file) {
            const content = await file.text();
            objRef.invokeMethodAsync('HandleFileImport', content);
        }
    };
    input.click();
};
```

---

## Claude Implementation Prompt

### PROMPT START

```
I need you to implement the JSON Preview component for my Blazor form editor.

## Project Context
- Project: VisualEditorOpus (Blazor Server, .NET 9.0)
- Location: Src/VisualEditorOpus/Components/Preview/

## Components to Create:

### 1. JsonPreview.razor
Main container with:
- Toolbar with view mode toggle (Formatted/Raw/Diff)
- JSON stats (size, lines, field count)
- Search functionality (Ctrl+F)
- Copy/Download/Import buttons
- Syntax highlighting
- Line numbers
- Validation feedback

### 2. JsonToolbar.razor
Controls for:
- View mode toggle (Formatted/Raw/Diff)
- JSON stats display
- Search, Copy, Download, Import buttons

### 3. JsonSearchBar.razor
Search interface with:
- Search input
- Match count
- Previous/Next navigation
- Close button

### 4. LineNumbers.razor
Line number display with error highlighting

### 5. JsonDiffView.razor
Side-by-side diff comparison:
- Original (saved) vs Current (unsaved)
- Added/Removed/Modified line highlighting

### 6. ValidationPanel.razor
Error display panel:
- Error count header
- Clickable errors with line/column
- Navigate to error line

### 7. JsonStatusBar.razor
Status display:
- Valid/Invalid status
- Last modified time
- Cursor position
- Encoding/File type

### Features:
- Syntax highlighting for JSON
- Real-time validation with error feedback
- Search with highlight and navigation
- Copy to clipboard
- Download as .json file
- Import from file
- Diff view for changes
- Line numbers with error indicators

Please implement complete, production-ready code with CSS.

## Required Deliverable: Testing & Manual Implementation Guide

After completing the implementation, create an HTML document named `JsonPreview-TestingGuide.html` that includes:

### Section 1: Manual Testing Steps
Provide a detailed checklist of manual testing steps I need to perform to verify the implementation:
- Step-by-step instructions for JSON syntax highlighting testing
- Line numbers display testing
- View mode toggle testing (Formatted/Raw/Diff)
- Search bar toggle (Ctrl+F) testing
- Search find and highlight testing
- Previous/Next match navigation testing
- Copy to clipboard testing
- Download JSON file testing
- Import JSON file testing
- Import validation error display testing
- Diff view comparison testing
- Validation error panel testing
- Click error to navigate testing
- Status bar information testing
- Dark mode appearance verification

### Section 2: Manual Implementation Tasks
List any tasks that require manual implementation by me:
- Supporting type files (JsonStats, JsonValidationError, SearchMatch, DiffLine)
- JavaScript file import (json-preview.js)
- Optional: JsonMinimap component
- Optional: PathNavigator component
- Proper diff algorithm library for production
- CSS file imports

### Section 3: Verification Checklist
Create an interactive checklist (with checkboxes) that I can use to verify:
- [ ] JSON displays with proper indentation
- [ ] JSON keys colored (blue)
- [ ] JSON strings colored
- [ ] JSON numbers colored
- [ ] JSON booleans colored (red)
- [ ] JSON null colored (purple)
- [ ] JSON brackets colored
- [ ] Line numbers display correctly
- [ ] Error lines highlighted in line numbers
- [ ] Formatted view mode works
- [ ] Raw view mode works
- [ ] Diff view mode works
- [ ] Ctrl+F toggles search bar
- [ ] Search finds matches in JSON
- [ ] Match count displays correctly
- [ ] Previous button navigates matches
- [ ] Next button navigates matches
- [ ] ESC closes search bar
- [ ] Copy button copies JSON to clipboard
- [ ] Download button saves .json file
- [ ] Import button opens file picker
- [ ] Import validates JSON
- [ ] Invalid JSON shows validation errors
- [ ] Diff view shows Original vs Current
- [ ] Added lines highlighted green
- [ ] Removed lines highlighted red
- [ ] Modified lines highlighted yellow
- [ ] Validation panel shows errors
- [ ] Clicking error navigates to line
- [ ] Status bar shows Valid/Invalid
- [ ] Status bar shows last modified
- [ ] Status bar shows cursor position
- [ ] Dark mode syntax colors correct

Format this as a standalone HTML file with clean styling that I can open in a browser to track my testing progress.
```

### PROMPT END

---

## Testing Checklist

- [ ] JSON renders with proper syntax highlighting
- [ ] Line numbers display correctly
- [ ] Error lines highlighted in line numbers
- [ ] Search bar toggles with Ctrl+F
- [ ] Search finds and highlights matches
- [ ] Previous/Next navigation works
- [ ] Copy to clipboard works
- [ ] Download creates valid JSON file
- [ ] Import reads JSON file correctly
- [ ] Import shows validation errors for invalid JSON
- [ ] View mode toggle works (Formatted/Raw/Diff)
- [ ] Diff view shows changes correctly
- [ ] Validation panel shows errors
- [ ] Clicking error navigates to line
- [ ] Status bar shows correct info
- [ ] Dark mode styling correct

---

## Notes

- Use System.Text.Json for serialization/deserialization
- Consider adding Monaco Editor for advanced editing
- Consider adding JSON schema validation
- Consider adding collapsible sections for large objects
- Consider adding path breadcrumb navigation
- Consider adding minimap for large files
- For diff, consider using a proper diff library for production
