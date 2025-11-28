namespace DynamicForms.RazorPages.Models;

/// <summary>
/// Information about a step in the progress stepper
/// </summary>
public class StepInfo
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string LabelEN { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string LabelFR { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? DescriptionEN { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? DescriptionFR { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsCompleted { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsActive { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsAccessible { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Url { get; set; } // For clickable navigation
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, object>? Metadata { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
    /// <summary>
    /// Get the step label for the specified language
    /// </summary>
    public string GetLabel(string language)
    {
        return language.ToUpper() switch
        {
            "FR" => !string.IsNullOrEmpty(LabelFR) ? LabelFR : LabelEN,
            _ => !string.IsNullOrEmpty(LabelEN) ? LabelEN : LabelFR
        };
    }
    
    /// <summary>
    /// Get the step description for the specified language
    /// </summary>
    public string? GetDescription(string language)
    {
        return language.ToUpper() switch
        {
            "FR" => !string.IsNullOrEmpty(DescriptionFR) ? DescriptionFR : DescriptionEN ?? Description,
            _ => !string.IsNullOrEmpty(DescriptionEN) ? DescriptionEN : DescriptionFR ?? Description
        };
    }
}

/// <summary>
/// Configuration for the progress stepper
/// </summary>
public class ProgressStepperConfig
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<StepInfo> Steps { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int CurrentStep { get; set; } = 1;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Language { get; set; } = "EN";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Style { get; set; } = "horizontal"; // horizontal, vertical, compact
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowNumbers { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowPercentage { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool Clickable { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowStepLabels { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowStepDescriptions { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
    /// <summary>
    /// Get progress percentage
    /// </summary>
    public double GetProgressPercentage()
    {
        if (Steps.Count == 0) return 0;
        return (double)CurrentStep / Steps.Count * 100;
    }
    
    /// <summary>
    /// Get current step info
    /// </summary>
    public StepInfo? GetCurrentStep()
    {
        if (CurrentStep <= 0 || CurrentStep > Steps.Count) return null;
        return Steps[CurrentStep - 1];
    }
    
    /// <summary>
    /// Update step states based on current step
    /// </summary>
    public void UpdateStepStates()
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            var stepNumber = i + 1;
            Steps[i].IsCompleted = stepNumber < CurrentStep;
            Steps[i].IsActive = stepNumber == CurrentStep;
            Steps[i].IsAccessible = stepNumber <= CurrentStep || Clickable;
        }
    }
}