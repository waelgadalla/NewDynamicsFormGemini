namespace DynamicForms.RazorPages.Models;

/// <summary>
/// Data structure for date range values
/// </summary>
public class DateRangeData
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime? StartDate { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime? EndDate { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public TimeSpan? StartTime { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public TimeSpan? EndTime { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
    /// <summary>
    /// Get total days in the range
    /// </summary>
    public int TotalDays => (EndDate - StartDate)?.Days ?? 0;
    
    /// <summary>
    /// Get total hours in the range (if time is included)
    /// </summary>
    public double TotalHours 
    {
        get
        {
            if (!StartDate.HasValue || !EndDate.HasValue) return 0;
            
            var start = StartDate.Value;
            var end = EndDate.Value;
            
            if (StartTime.HasValue) start = start.Add(StartTime.Value);
            if (EndTime.HasValue) end = end.Add(EndTime.Value);
            
            return (end - start).TotalHours;
        }
    }
    
    /// <summary>
    /// Get formatted date range string
    /// </summary>
    public string FormattedRange => StartDate.HasValue && EndDate.HasValue 
        ? $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}" 
        : string.Empty;
    
    /// <summary>
    /// Get localized formatted range
    /// </summary>
    public string GetFormattedRange(string language = "EN", string format = "yyyy-MM-dd")
    {
        if (!StartDate.HasValue || !EndDate.HasValue) return string.Empty;
        
        var startFormatted = StartDate.Value.ToString(format);
        var endFormatted = EndDate.Value.ToString(format);
        var separator = language == "FR" ? " au " : " to ";
        
        return $"{startFormatted}{separator}{endFormatted}";
    }
    
    /// <summary>
    /// Check if the date range is valid
    /// </summary>
    public bool IsValid => StartDate.HasValue && EndDate.HasValue && StartDate <= EndDate;
    
    /// <summary>
    /// Check if the range has any value
    /// </summary>
    public bool HasValue => StartDate.HasValue || EndDate.HasValue;
    
    /// <summary>
    /// Get ISO 8601 formatted range for JSON serialization
    /// </summary>
    public string ToIsoString()
    {
        if (!IsValid) return string.Empty;
        
        var start = StartDate!.Value;
        var end = EndDate!.Value;
        
        if (StartTime.HasValue) start = start.Add(StartTime.Value);
        if (EndTime.HasValue) end = end.Add(EndTime.Value);
        
        return $"{start:yyyy-MM-ddTHH:mm:ss}/{end:yyyy-MM-ddTHH:mm:ss}";
    }
    
    /// <summary>
    /// Parse from ISO 8601 string
    /// </summary>
    public static DateRangeData? FromIsoString(string isoString)
    {
        if (string.IsNullOrWhiteSpace(isoString)) return null;
        
        var parts = isoString.Split('/');
        if (parts.Length != 2) return null;
        
        var result = new DateRangeData();
        
        if (DateTime.TryParse(parts[0], out var start))
        {
            result.StartDate = start.Date;
            if (start.TimeOfDay != TimeSpan.Zero)
                result.StartTime = start.TimeOfDay;
        }
        
        if (DateTime.TryParse(parts[1], out var end))
        {
            result.EndDate = end.Date;
            if (end.TimeOfDay != TimeSpan.Zero)
                result.EndTime = end.TimeOfDay;
        }
        
        return result.HasValue ? result : null;
    }
}

/// <summary>
/// Configuration for DateRangePicker control
/// </summary>
public class DateRangePickerConfiguration
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime? MinDate { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime? MaxDate { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MaxRangeDays { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int? MinRangeDays { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IncludeTime { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool BusinessDaysOnly { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string[] DisabledDates { get; set; } = [];
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string DateFormat { get; set; } = "yyyy-MM-dd";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string TimeFormat { get; set; } = "HH:mm";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool AllowSameDay { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string DefaultRange { get; set; } = "custom"; // today, week, month, custom
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowPresets { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool ShowClearButton { get; set; } = true;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Theme { get; set; } = "bootstrap"; // bootstrap, material, custom
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
    /// <summary>
    /// Get localized preset options
    /// </summary>
    public Dictionary<string, string> GetPresets(string language = "EN")
    {
        return language == "FR" 
            ? new Dictionary<string, string>
            {
                ["today"] = "Aujourd'hui",
                ["tomorrow"] = "Demain",
                ["thisweek"] = "Cette semaine",
                ["nextweek"] = "Semaine prochaine",
                ["thismonth"] = "Ce mois",
                ["nextmonth"] = "Mois prochain",
                ["last7days"] = "7 derniers jours",
                ["last30days"] = "30 derniers jours",
                ["custom"] = "Personnalisé"
            }
            : new Dictionary<string, string>
            {
                ["today"] = "Today",
                ["tomorrow"] = "Tomorrow", 
                ["thisweek"] = "This Week",
                ["nextweek"] = "Next Week",
                ["thismonth"] = "This Month",
                ["nextmonth"] = "Next Month",
                ["last7days"] = "Last 7 Days",
                ["last30days"] = "Last 30 Days",
                ["custom"] = "Custom Range"
            };
    }
    
    /// <summary>
    /// Calculate date range for preset
    /// </summary>
    public DateRangeData? GetPresetRange(string preset)
    {
        var today = DateTime.Today;
        
        return preset.ToLower() switch
        {
            "today" => new DateRangeData { StartDate = today, EndDate = today },
            "tomorrow" => new DateRangeData { StartDate = today.AddDays(1), EndDate = today.AddDays(1) },
            "thisweek" => new DateRangeData 
            { 
                StartDate = today.AddDays(-(int)today.DayOfWeek),
                EndDate = today.AddDays(6 - (int)today.DayOfWeek)
            },
            "nextweek" => new DateRangeData 
            { 
                StartDate = today.AddDays(7 - (int)today.DayOfWeek),
                EndDate = today.AddDays(13 - (int)today.DayOfWeek)
            },
            "thismonth" => new DateRangeData 
            { 
                StartDate = new DateTime(today.Year, today.Month, 1),
                EndDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month))
            },
            "nextmonth" => new DateRangeData 
            { 
                StartDate = today.AddMonths(1).AddDays(1 - today.Day),
                EndDate = new DateTime(today.AddMonths(1).Year, today.AddMonths(1).Month, DateTime.DaysInMonth(today.AddMonths(1).Year, today.AddMonths(1).Month))
            },
            "last7days" => new DateRangeData { StartDate = today.AddDays(-7), EndDate = today },
            "last30days" => new DateRangeData { StartDate = today.AddDays(-30), EndDate = today },
            _ => null
        };
    }
}