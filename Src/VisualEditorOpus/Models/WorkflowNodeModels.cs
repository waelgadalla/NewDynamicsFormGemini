namespace VisualEditorOpus.Models;

#region Enums

/// <summary>
/// Position of a connection handle on a workflow node
/// </summary>
public enum HandlePosition
{
    Top,
    Bottom,
    Left,
    Right
}

/// <summary>
/// Type of trigger that starts a workflow
/// </summary>
public enum TriggerType
{
    OnSubmit,
    OnSave,
    OnLoad,
    Manual,
    Scheduled
}

/// <summary>
/// Action to perform when workflow completes
/// </summary>
public enum CompletionAction
{
    Submit,
    Save,
    Redirect,
    ShowMessage,
    CallApi
}

/// <summary>
/// Type of automated action in a workflow
/// </summary>
public enum ActionType
{
    SendEmail,
    CallApi,
    SetFieldValue,
    CreateRecord,
    UpdateRecord,
    DeleteRecord,
    SendNotification,
    RunScript
}

/// <summary>
/// Type of workflow node
/// </summary>
public enum WorkflowNodeType
{
    Start,
    End,
    Step,
    Decision,
    Action
}

/// <summary>
/// Type of connection path rendering
/// </summary>
public enum ConnectionType
{
    Bezier,
    Step,
    SmoothStep,
    Straight
}

/// <summary>
/// Type of branch for decision connections
/// </summary>
public enum BranchType
{
    Default,
    Yes,
    No,
    Error
}

#endregion

#region Models

/// <summary>
/// Represents a point in 2D space
/// </summary>
public record WfPoint(double X, double Y)
{
    public static WfPoint Zero => new(0, 0);
}

/// <summary>
/// Represents a node in a workflow
/// </summary>
public record WorkflowNodeData
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "";
    public WorkflowNodeType Type { get; init; }
    public WfPoint Position { get; init; } = WfPoint.Zero;

    // Start node properties
    public TriggerType? TriggerType { get; init; }
    public string? ScheduleCron { get; init; }

    // End node properties
    public CompletionAction? CompletionAction { get; init; }
    public string? CompletionMessage { get; init; }
    public string? RedirectUrl { get; init; }

    // Step node properties
    public string? ModuleId { get; init; }
    public bool IsRequired { get; init; }
    public ConditionGroupModel? SkipCondition { get; init; }

    // Decision node properties
    public ConditionGroupModel? Condition { get; init; }
    public string? YesBranchLabel { get; init; }
    public string? NoBranchLabel { get; init; }

    // Action node properties
    public ActionType? ActionType { get; init; }
    public string? EmailRecipients { get; init; }
    public string? EmailSubject { get; init; }
    public string? EmailBody { get; init; }
    public string? ApiEndpoint { get; init; }
    public string? ApiMethod { get; init; }
    public string? TargetFieldId { get; init; }
    public string? FieldValue { get; init; }
    public string? TargetTable { get; init; }
    public string? NotificationMessage { get; init; }
    public string? ScriptContent { get; init; }
}

/// <summary>
/// Represents a connection between two workflow nodes
/// </summary>
public record WorkflowConnection
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string SourceNodeId { get; init; } = "";
    public HandlePosition SourceHandle { get; init; }
    public string TargetNodeId { get; init; } = "";
    public HandlePosition TargetHandle { get; init; }
    public string? Label { get; init; }
    public BranchType BranchType { get; init; } = BranchType.Default;
}

/// <summary>
/// Represents the position and dimensions of a node on the canvas
/// </summary>
public record NodePosition
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
}

#endregion
