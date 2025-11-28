using DynamicForms.RazorPages.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace DynamicForms.RazorPages.TagHelpers;

/// <summary>
/// Enhanced progress stepper for multi-step forms
/// Usage: <progress-stepper config="@Model.StepperConfig" />
/// </summary>
[HtmlTargetElement("progress-stepper")]
public class ProgressStepperTagHelper : TagHelper
{
    /// <summary>
    /// Progress stepper configuration
    /// </summary>
    [HtmlAttributeName("config")]
    public ProgressStepperConfig? Config { get; set; }

    /// <summary>
    /// List of steps (alternative to config)
    /// </summary>
    [HtmlAttributeName("steps")]
    public List<StepInfo>? Steps { get; set; }

    /// <summary>
    /// Current active step (1-based)
    /// </summary>
    [HtmlAttributeName("current")]
    public int Current { get; set; } = 1;

    /// <summary>
    /// Language for localization
    /// </summary>
    [HtmlAttributeName("language")]
    public string Language { get; set; } = "EN";

    /// <summary>
    /// Display style: "horizontal", "vertical", "compact"
    /// </summary>
    [HtmlAttributeName("style")]
    public string Style { get; set; } = "horizontal";

    /// <summary>
    /// Show step numbers
    /// </summary>
    [HtmlAttributeName("show-numbers")]
    public bool ShowNumbers { get; set; } = true;

    /// <summary>
    /// Show progress percentage
    /// </summary>
    [HtmlAttributeName("show-percentage")]
    public bool ShowPercentage { get; set; } = true;

    /// <summary>
    /// Enable clickable steps (for navigation)
    /// </summary>
    [HtmlAttributeName("clickable")]
    public bool Clickable { get; set; } = false;

    /// <summary>
    /// Show step descriptions
    /// </summary>
    [HtmlAttributeName("show-descriptions")]
    public bool ShowDescriptions { get; set; } = false;

    /// <summary>
    /// CSS class for the container
    /// </summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override void Process(TagHelperContext context, TagHelperOutput output)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        // Build configuration from properties
        var config = Config ?? new ProgressStepperConfig
        {
            Steps = Steps ?? new List<StepInfo>(),
            CurrentStep = Current,
            Language = Language,
            Style = Style,
            ShowNumbers = ShowNumbers,
            ShowPercentage = ShowPercentage,
            Clickable = Clickable,
            ShowStepDescriptions = ShowDescriptions
        };

        if (config.Steps.Count == 0)
        {
            output.SuppressOutput();
            return;
        }

        // Update step states
        config.UpdateStepStates();

        var html = config.Style.ToLower() switch
        {
            "vertical" => GenerateVerticalStepper(config),
            "compact" => GenerateCompactStepper(config),
            _ => GenerateHorizontalStepper(config)
        };

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }

    private string GenerateHorizontalStepper(ProgressStepperConfig config)
    {
        var sb = new StringBuilder();
        var totalSteps = config.Steps.Count;
        var percentage = config.GetProgressPercentage();
        var containerClass = string.IsNullOrEmpty(CssClass) ? "progress-stepper horizontal-stepper mb-4" : $"progress-stepper horizontal-stepper mb-4 {CssClass}";

        sb.AppendLine($"<div class=\"{containerClass}\">");

        // Progress bar and percentage
        if (config.ShowPercentage)
        {
            var percentText = config.Language == "FR" ? $"{percentage:F0}% terminé" : $"{percentage:F0}% complete";
            sb.AppendLine($"  <div class=\"stepper-progress-info text-center mb-2\">");
            sb.AppendLine($"    <small class=\"text-muted\">{percentText}</small>");
            sb.AppendLine($"  </div>");
        }

        sb.AppendLine("  <div class=\"stepper-progress-bar mb-3\">");
        sb.AppendLine($"    <div class=\"progress\" style=\"height: 4px;\">");
        sb.AppendLine($"      <div class=\"progress-bar bg-primary\" style=\"width: {percentage:F1}%\" role=\"progressbar\" aria-valuenow=\"{config.CurrentStep}\" aria-valuemin=\"0\" aria-valuemax=\"{totalSteps}\"></div>");
        sb.AppendLine($"    </div>");
        sb.AppendLine("  </div>");

        // Steps
        sb.AppendLine("  <div class=\"stepper-steps d-flex justify-content-between align-items-start\">");

        for (int i = 0; i < totalSteps; i++)
        {
            var step = config.Steps[i];
            var stepNumber = i + 1;
            var isActive = stepNumber == config.CurrentStep;
            var isCompleted = stepNumber < config.CurrentStep;
            var isPending = stepNumber > config.CurrentStep;

            var stepClass = isActive ? "active" : (isCompleted ? "completed" : "pending");
            var clickableClass = config.Clickable && step.IsAccessible ? "clickable" : "";

            sb.AppendLine($"    <div class=\"stepper-step {stepClass} {clickableClass}\" data-step=\"{stepNumber}\" data-step-url=\"{step.Url}\">");

            // Step circle with icon/number
            sb.AppendLine("      <div class=\"step-circle\">");
            if (isCompleted)
            {
                sb.AppendLine("        <i class=\"fas fa-check\" aria-hidden=\"true\"></i>");
            }
            else if (config.ShowNumbers)
            {
                sb.AppendLine($"        <span class=\"step-number\">{stepNumber}</span>");
            }
            else
            {
                sb.AppendLine($"        <span class=\"step-dot\"></span>");
            }
            sb.AppendLine("      </div>");

            // Step label and description
            if (config.ShowStepLabels)
            {
                var label = step.GetLabel(config.Language);
                var description = config.ShowStepDescriptions ? step.GetDescription(config.Language) : null;
                
                sb.AppendLine($"      <div class=\"step-label\">");
                sb.AppendLine($"        <div class=\"step-title\">{label}</div>");
                
                if (!string.IsNullOrEmpty(description))
                {
                    sb.AppendLine($"        <div class=\"step-description\">{description}</div>");
                }
                sb.AppendLine($"      </div>");
            }

            // Connector line (except for last step)
            if (i < totalSteps - 1)
            {
                var lineClass = stepNumber < config.CurrentStep ? "completed" : "pending";
                sb.AppendLine($"      <div class=\"step-connector {lineClass}\"></div>");
            }

            sb.AppendLine("    </div>");
        }

        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");

        // Add CSS styles
        sb.AppendLine(GetStepperStyles());

        return sb.ToString();
    }

    private string GenerateVerticalStepper(ProgressStepperConfig config)
    {
        var sb = new StringBuilder();
        var totalSteps = config.Steps.Count;
        var containerClass = string.IsNullOrEmpty(CssClass) ? "progress-stepper vertical-stepper mb-4" : $"progress-stepper vertical-stepper mb-4 {CssClass}";

        sb.AppendLine($"<div class=\"{containerClass}\">");

        for (int i = 0; i < totalSteps; i++)
        {
            var step = config.Steps[i];
            var stepNumber = i + 1;
            var isActive = stepNumber == config.CurrentStep;
            var isCompleted = stepNumber < config.CurrentStep;
            var isPending = stepNumber > config.CurrentStep;

            var stepClass = isActive ? "active" : (isCompleted ? "completed" : "pending");
            var clickableClass = config.Clickable && step.IsAccessible ? "clickable" : "";

            sb.AppendLine($"  <div class=\"stepper-step-vertical {stepClass} {clickableClass}\" data-step=\"{stepNumber}\" data-step-url=\"{step.Url}\">");
            sb.AppendLine("    <div class=\"step-content-vertical d-flex\">");

            // Step circle
            sb.AppendLine("      <div class=\"step-circle-vertical\">");
            if (isCompleted)
            {
                sb.AppendLine("        <i class=\"fas fa-check\" aria-hidden=\"true\"></i>");
            }
            else
            {
                sb.AppendLine($"        <span class=\"step-number\">{stepNumber}</span>");
            }
            sb.AppendLine("      </div>");

            // Step content
            if (config.ShowStepLabels)
            {
                sb.AppendLine("      <div class=\"step-info-vertical ms-3\">");
                var label = step.GetLabel(config.Language);
                sb.AppendLine($"        <h6 class=\"step-title-vertical mb-1\">{label}</h6>");
                
                if (config.ShowStepDescriptions)
                {
                    var description = step.GetDescription(config.Language);
                    if (!string.IsNullOrEmpty(description))
                    {
                        sb.AppendLine($"        <p class=\"step-description-vertical text-muted mb-0\">{description}</p>");
                    }
                }
                sb.AppendLine("      </div>");
            }

            sb.AppendLine("    </div>");

            // Vertical connector (except for last step)
            if (i < totalSteps - 1)
            {
                var lineClass = stepNumber < config.CurrentStep ? "completed" : "pending";
                sb.AppendLine($"    <div class=\"step-connector-vertical {lineClass}\"></div>");
            }

            sb.AppendLine("  </div>");
        }

        sb.AppendLine("</div>");
        sb.AppendLine(GetVerticalStepperStyles());

        return sb.ToString();
    }

    private string GenerateCompactStepper(ProgressStepperConfig config)
    {
        var sb = new StringBuilder();
        var totalSteps = config.Steps.Count;
        var percentage = config.GetProgressPercentage();
        var containerClass = string.IsNullOrEmpty(CssClass) ? "progress-stepper compact-stepper mb-4" : $"progress-stepper compact-stepper mb-4 {CssClass}";

        sb.AppendLine($"<div class=\"{containerClass}\">");
        sb.AppendLine("  <div class=\"d-flex align-items-center\">");

        // Compact progress bar
        sb.AppendLine("    <div class=\"flex-grow-1 me-3\">");
        sb.AppendLine($"      <div class=\"progress\" style=\"height: 6px;\">");
        sb.AppendLine($"        <div class=\"progress-bar\" style=\"width: {percentage:F1}%\" role=\"progressbar\" aria-valuenow=\"{config.CurrentStep}\" aria-valuemin=\"0\" aria-valuemax=\"{totalSteps}\"></div>");
        sb.AppendLine($"      </div>");
        sb.AppendLine("    </div>");

        // Current step info
        var currentStep = config.GetCurrentStep();
        if (currentStep != null)
        {
            var currentLabel = currentStep.GetLabel(config.Language);
            var stepText = config.Language == "FR" 
                ? $"Étape {config.CurrentStep} de {totalSteps}: {currentLabel}" 
                : $"Step {config.CurrentStep} of {totalSteps}: {currentLabel}";

            sb.AppendLine($"    <div class=\"step-info-compact\">");
            sb.AppendLine($"      <small class=\"text-muted\">{stepText}</small>");
            sb.AppendLine($"    </div>");
        }

        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");

        return sb.ToString();
    }

    private string GetStepperStyles()
    {
        return @"
<style>
.progress-stepper {
    --stepper-primary: #0d6efd;
    --stepper-success: #198754;
    --stepper-muted: #6c757d;
    --stepper-light: #f8f9fa;
}

.horizontal-stepper .stepper-steps {
    position: relative;
}

.stepper-step {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    position: relative;
    flex: 1;
    min-width: 120px;
}

.stepper-step.clickable {
    cursor: pointer;
    transition: transform 0.2s ease;
}

.stepper-step.clickable:hover {
    transform: translateY(-2px);
}

.step-circle {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;
    margin-bottom: 8px;
    transition: all 0.3s ease;
}

.stepper-step.completed .step-circle {
    background-color: var(--stepper-success);
    color: white;
    border: 2px solid var(--stepper-success);
}

.stepper-step.active .step-circle {
    background-color: var(--stepper-primary);
    color: white;
    border: 2px solid var(--stepper-primary);
    box-shadow: 0 0 0 4px rgba(13, 110, 253, 0.25);
}

.stepper-step.pending .step-circle {
    background-color: var(--stepper-light);
    color: var(--stepper-muted);
    border: 2px solid #dee2e6;
}

.step-label {
    min-height: 40px;
}

.step-title {
    font-weight: 500;
    font-size: 0.9rem;
    margin-bottom: 4px;
}

.stepper-step.completed .step-title {
    color: var(--stepper-success);
}

.stepper-step.active .step-title {
    color: var(--stepper-primary);
    font-weight: 600;
}

.stepper-step.pending .step-title {
    color: var(--stepper-muted);
}

.step-description {
    font-size: 0.75rem;
    color: var(--stepper-muted);
}

.step-connector {
    position: absolute;
    top: 20px;
    left: 50%;
    right: -50%;
    height: 2px;
    background-color: #dee2e6;
    z-index: -1;
}

.step-connector.completed {
    background-color: var(--stepper-success);
}

@media (max-width: 768px) {
    .stepper-step {
        min-width: 80px;
    }
    
    .step-circle {
        width: 32px;
        height: 32px;
        font-size: 0.8rem;
    }
    
    .step-title {
        font-size: 0.8rem;
    }
    
    .step-description {
        display: none;
    }
}
</style>";
    }

    private string GetVerticalStepperStyles()
    {
        return @"
<style>
.vertical-stepper .stepper-step-vertical {
    position: relative;
    padding-bottom: 1.5rem;
}

.stepper-step-vertical.clickable {
    cursor: pointer;
    transition: background-color 0.2s ease;
    padding: 0.5rem;
    border-radius: 0.375rem;
}

.stepper-step-vertical.clickable:hover {
    background-color: rgba(13, 110, 253, 0.05);
}

.step-circle-vertical {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;
    font-size: 0.85rem;
    flex-shrink: 0;
}

.stepper-step-vertical.completed .step-circle-vertical {
    background-color: var(--stepper-success);
    color: white;
}

.stepper-step-vertical.active .step-circle-vertical {
    background-color: var(--stepper-primary);
    color: white;
}

.stepper-step-vertical.pending .step-circle-vertical {
    background-color: var(--stepper-light);
    color: var(--stepper-muted);
    border: 2px solid #dee2e6;
}

.step-connector-vertical {
    position: absolute;
    left: 15px;
    top: 32px;
    bottom: -24px;
    width: 2px;
    background-color: #dee2e6;
}

.step-connector-vertical.completed {
    background-color: var(--stepper-success);
}

.step-title-vertical {
    margin-bottom: 0.25rem;
}

.stepper-step-vertical.active .step-title-vertical {
    color: var(--stepper-primary);
    font-weight: 600;
}

.step-description-vertical {
    font-size: 0.875rem;
}
</style>";
    }
}