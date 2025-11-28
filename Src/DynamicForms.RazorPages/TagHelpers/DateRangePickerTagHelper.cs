using DynamicForms.RazorPages.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace DynamicForms.RazorPages.TagHelpers;

/// <summary>
/// TagHelper for DateRangePicker control
/// Usage: <date-range-picker config="@Model.DateRangeConfig" value="@Model.DateRange" />
/// </summary>
[HtmlTargetElement("date-range-picker")]
public class DateRangePickerTagHelper : TagHelper
{
    /// <summary>
    /// Configuration for the date range picker
    /// </summary>
    [HtmlAttributeName("config")]
    public DateRangePickerConfiguration? Config { get; set; }

    /// <summary>
    /// Current date range value
    /// </summary>
    [HtmlAttributeName("value")]
    public DateRangeData? Value { get; set; }

    /// <summary>
    /// Field name for form submission
    /// </summary>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Field ID
    /// </summary>
    [HtmlAttributeName("field-id")]
    public string? FieldId { get; set; }

    /// <summary>
    /// Language for localization
    /// </summary>
    [HtmlAttributeName("language")]
    public string Language { get; set; } = "EN";

    /// <summary>
    /// Whether the field is required
    /// </summary>
    [HtmlAttributeName("required")]
    public bool Required { get; set; }

    /// <summary>
    /// Whether the field is read-only
    /// </summary>
    [HtmlAttributeName("readonly")]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// CSS classes to add
    /// </summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Label text
    /// </summary>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Help text
    /// </summary>
    [HtmlAttributeName("help-text")]
    public string? HelpText { get; set; }

    /// <summary>
    /// Validation error message
    /// </summary>
    [HtmlAttributeName("error-message")]
    public string? ErrorMessage { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override void Process(TagHelperContext context, TagHelperOutput output)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var config = Config ?? new DateRangePickerConfiguration();
        var containerClass = string.IsNullOrEmpty(CssClass) 
            ? "date-range-picker-container" 
            : $"date-range-picker-container {CssClass}";

        var fieldId = FieldId ?? $"daterange_{Guid.NewGuid():N}";
        var fieldName = Name ?? "dateRange";

        var html = GenerateDateRangePicker(config, fieldId, fieldName, containerClass);

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }

    private string GenerateDateRangePicker(DateRangePickerConfiguration config, string fieldId, string fieldName, string containerClass)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"<div class=\"{containerClass}\" data-field-id=\"{FieldId}\" data-field-type=\"daterangepicker\">");

        // Label
        if (!string.IsNullOrEmpty(Label))
        {
            sb.AppendLine($"  <label for=\"{fieldId}\" class=\"form-label\">");
            sb.AppendLine($"    {Label}");
            if (Required)
                sb.AppendLine("    <span class=\"text-danger ms-1\">*</span>");
            sb.AppendLine("  </label>");
        }

        // Date range input container
        sb.AppendLine("  <div class=\"date-range-input-container\">");

        // Main input container
        sb.AppendLine("    <div class=\"date-inputs d-flex gap-2 align-items-center\">");

        // Start date input
        sb.AppendLine("      <div class=\"date-input-group flex-grow-1\">");
        sb.AppendLine($"        <label class=\"form-label small text-muted\">{GetLocalizedText("StartDate")}</label>");
        sb.AppendLine($"        <input type=\"date\"");
        sb.AppendLine($"               id=\"{fieldId}_start\"");
        sb.AppendLine($"               name=\"{fieldName}.StartDate\"");
        sb.AppendLine($"               class=\"form-control start-date{(ReadOnly ? " readonly" : "")}\"");
        sb.AppendLine($"               value=\"{Value?.StartDate?.ToString("yyyy-MM-dd")}\"");
        
        if (config.MinDate.HasValue)
            sb.AppendLine($"               min=\"{config.MinDate.Value:yyyy-MM-dd}\"");
        if (config.MaxDate.HasValue)
            sb.AppendLine($"               max=\"{config.MaxDate.Value:yyyy-MM-dd}\"");
        if (Required)
            sb.AppendLine("               required");
        if (ReadOnly)
            sb.AppendLine("               readonly");
            
        sb.AppendLine("               data-role=\"start-date\" />");
        sb.AppendLine("      </div>");

        // Separator
        sb.AppendLine("      <div class=\"date-separator align-self-end mb-2\">");
        sb.AppendLine($"        <span class=\"text-muted\">{GetLocalizedText("To")}</span>");
        sb.AppendLine("      </div>");

        // End date input  
        sb.AppendLine("      <div class=\"date-input-group flex-grow-1\">");
        sb.AppendLine($"        <label class=\"form-label small text-muted\">{GetLocalizedText("EndDate")}</label>");
        sb.AppendLine($"        <input type=\"date\"");
        sb.AppendLine($"               id=\"{fieldId}_end\"");
        sb.AppendLine($"               name=\"{fieldName}.EndDate\"");
        sb.AppendLine($"               class=\"form-control end-date{(ReadOnly ? " readonly" : "")}\"");
        sb.AppendLine($"               value=\"{Value?.EndDate?.ToString("yyyy-MM-dd")}\"");
        
        if (config.MinDate.HasValue)
            sb.AppendLine($"               min=\"{config.MinDate.Value:yyyy-MM-dd}\"");
        if (config.MaxDate.HasValue)
            sb.AppendLine($"               max=\"{config.MaxDate.Value:yyyy-MM-dd}\"");
        if (Required)
            sb.AppendLine("               required");
        if (ReadOnly)
            sb.AppendLine("               readonly");
            
        sb.AppendLine("               data-role=\"end-date\" />");
        sb.AppendLine("      </div>");

        sb.AppendLine("    </div>");

        // Time inputs (if enabled)
        if (config.IncludeTime)
        {
            sb.AppendLine("    <div class=\"time-inputs d-flex gap-2 align-items-center mt-2\">");
            
            // Start time
            sb.AppendLine("      <div class=\"time-input-group flex-grow-1\">");
            sb.AppendLine($"        <label class=\"form-label small text-muted\">{GetLocalizedText("StartTime")}</label>");
            sb.AppendLine($"        <input type=\"time\"");
            sb.AppendLine($"               id=\"{fieldId}_start_time\"");
            sb.AppendLine($"               name=\"{fieldName}.StartTime\"");
            sb.AppendLine($"               class=\"form-control start-time{(ReadOnly ? " readonly" : "")}\"");
            sb.AppendLine($"               value=\"{Value?.StartTime?.ToString(@"hh\:mm")}\"");
            if (ReadOnly)
                sb.AppendLine("               readonly");
            sb.AppendLine("               data-role=\"start-time\" />");
            sb.AppendLine("      </div>");

            sb.AppendLine("      <div class=\"time-separator align-self-end mb-2\">");
            sb.AppendLine($"        <span class=\"text-muted\">{GetLocalizedText("To")}</span>");
            sb.AppendLine("      </div>");

            // End time
            sb.AppendLine("      <div class=\"time-input-group flex-grow-1\">");
            sb.AppendLine($"        <label class=\"form-label small text-muted\">{GetLocalizedText("EndTime")}</label>");
            sb.AppendLine($"        <input type=\"time\"");
            sb.AppendLine($"               id=\"{fieldId}_end_time\"");
            sb.AppendLine($"               name=\"{fieldName}.EndTime\"");
            sb.AppendLine($"               class=\"form-control end-time{(ReadOnly ? " readonly" : "")}\"");
            sb.AppendLine($"               value=\"{Value?.EndTime?.ToString(@"hh\:mm")}\"");
            if (ReadOnly)
                sb.AppendLine("               readonly");
            sb.AppendLine("               data-role=\"end-time\" />");
            sb.AppendLine("      </div>");

            sb.AppendLine("    </div>");
        }

        // Quick selection presets
        if (config.ShowPresets && !ReadOnly)
        {
            sb.AppendLine("    <div class=\"date-presets mt-2\">");
            sb.AppendLine("      <div class=\"btn-group btn-group-sm\" role=\"group\">");
            
            var presets = config.GetPresets(Language);
            foreach (var preset in presets.Take(6)) // Limit to 6 presets to avoid overflow
            {
                sb.AppendLine($"        <button type=\"button\" class=\"btn btn-outline-secondary preset-btn\" data-preset=\"{preset.Key}\">");
                sb.AppendLine($"          {preset.Value}");
                sb.AppendLine("        </button>");
            }
            
            sb.AppendLine("      </div>");
            
            if (config.ShowClearButton)
            {
                sb.AppendLine($"      <button type=\"button\" class=\"btn btn-sm btn-outline-danger ms-2 clear-btn\">");
                sb.AppendLine($"        <i class=\"fas fa-times me-1\"></i>{GetLocalizedText("Clear")}");
                sb.AppendLine("      </button>");
            }
            
            sb.AppendLine("    </div>");
        }

        // Range information display
        sb.AppendLine("    <div class=\"range-info mt-2\">");
        sb.AppendLine("      <small class=\"text-muted\">");
        sb.AppendLine($"        <span class=\"duration-display\">{GetDurationDisplay()}</span>");
        sb.AppendLine("      </small>");
        sb.AppendLine("    </div>");

        sb.AppendLine("  </div>");

        // Hidden field for complete range data (JSON)
        var jsonValue = Value?.ToIsoString() ?? string.Empty;
        sb.AppendLine($"  <input type=\"hidden\" name=\"{fieldName}\" value=\"{jsonValue}\" class=\"range-data\" />");

        // Help text
        if (!string.IsNullOrEmpty(HelpText))
        {
            sb.AppendLine($"  <div class=\"form-text\">{HelpText}</div>");
        }

        // Validation error
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            sb.AppendLine($"  <div class=\"invalid-feedback d-block\">{ErrorMessage}</div>");
        }

        sb.AppendLine("</div>");

        // Add CSS and JavaScript
        sb.AppendLine(GetDateRangePickerStyles());
        sb.AppendLine(GetDateRangePickerScript(fieldId, config));

        return sb.ToString();
    }

    private string GetLocalizedText(string key)
    {
        return Language == "FR" ? key switch
        {
            "StartDate" => "Date de début",
            "EndDate" => "Date de fin", 
            "StartTime" => "Heure de début",
            "EndTime" => "Heure de fin",
            "To" => "au",
            "Clear" => "Effacer",
            "Duration" => "Durée",
            "Days" => "jours",
            "Day" => "jour",
            "Hours" => "heures",
            "Hour" => "heure",
            _ => key
        } : key switch
        {
            "StartDate" => "Start Date",
            "EndDate" => "End Date",
            "StartTime" => "Start Time", 
            "EndTime" => "End Time",
            "To" => "to",
            "Clear" => "Clear",
            "Duration" => "Duration",
            "Days" => "days",
            "Day" => "day",
            "Hours" => "hours", 
            "Hour" => "hour",
            _ => key
        };
    }

    private string GetDurationDisplay()
    {
        if (Value?.IsValid == true)
        {
            var days = Value.TotalDays;
            var hours = Value.TotalHours;
            
            if (Config?.IncludeTime == true && hours > 0)
            {
                var hourText = Language == "FR" ? (hours == 1 ? "heure" : "heures") : (hours == 1 ? "hour" : "hours");
                return $"{GetLocalizedText("Duration")}: {hours:F1} {hourText}";
            }
            else if (days > 0)
            {
                var dayText = Language == "FR" ? (days == 1 ? "jour" : "jours") : (days == 1 ? "day" : "days"); 
                return $"{GetLocalizedText("Duration")}: {days} {dayText}";
            }
        }
        
        return string.Empty;
    }

    private string GetDateRangePickerStyles()
    {
        return @"
<style>
.date-range-picker-container {
    margin-bottom: 1rem;
}

.date-input-group, .time-input-group {
    position: relative;
}

.date-input-group .form-label, .time-input-group .form-label {
    margin-bottom: 0.25rem;
    font-size: 0.875rem;
}

.date-separator, .time-separator {
    padding: 0 0.5rem;
    font-weight: 500;
}

.date-presets .btn-group {
    flex-wrap: wrap;
}

.preset-btn {
    font-size: 0.8rem;
    padding: 0.25rem 0.5rem;
    border-radius: 0.25rem !important;
    margin-right: 0.25rem;
    margin-bottom: 0.25rem;
}

.range-info {
    min-height: 1.2rem;
}

.duration-display {
    font-weight: 500;
}

/* Validation states */
.date-range-picker-container .is-invalid {
    border-color: #dc3545;
}

.date-range-picker-container.has-error .form-control {
    border-color: #dc3545;
}

.date-range-picker-container.has-error .invalid-feedback {
    display: block;
}

/* Mobile responsiveness */
@@media (max-width: 576px) {
    .date-inputs {
        flex-direction: column !important;
        gap: 0.5rem !important;
    }
    
    .date-separator, .time-separator {
        display: none;
    }
    
    .date-presets .btn-group {
        flex-direction: column;
    }
    
    .preset-btn {
        margin-bottom: 0.25rem;
        border-radius: 0.25rem !important;
    }
}
</style>";
    }

    private string GetDateRangePickerScript(string fieldId, DateRangePickerConfiguration config)
    {
        var configJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            fieldId,
            includeTime = config.IncludeTime,
            minDate = config.MinDate?.ToString("yyyy-MM-dd"),
            maxDate = config.MaxDate?.ToString("yyyy-MM-dd"),
            maxRangeDays = config.MaxRangeDays,
            minRangeDays = config.MinRangeDays,
            allowSameDay = config.AllowSameDay,
            businessDaysOnly = config.BusinessDaysOnly,
            language = Language
        });

        return $@"
<script>
$(document).ready(function() {{
    DynamicForms.DateRangePicker.initialize('{fieldId}', {configJson});
}});
</script>";
    }
}