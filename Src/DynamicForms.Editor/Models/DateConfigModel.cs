using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Models;

public class DateConfigModel
{
    public bool AllowFuture { get; set; } = true;
    public bool AllowPast { get; set; } = true;
    public string? MinDate { get; set; }
    public string? MaxDate { get; set; }

    public static DateConfigModel From(DateConfig? config)
    {
        if (config == null) return new DateConfigModel();
        return new DateConfigModel
        {
            AllowFuture = config.AllowFuture,
            AllowPast = config.AllowPast,
            MinDate = config.MinDate,
            MaxDate = config.MaxDate
        };
    }

    public DateConfig ToConfig()
    {
        return new DateConfig
        {
            AllowFuture = AllowFuture,
            AllowPast = AllowPast,
            MinDate = MinDate,
            MaxDate = MaxDate
        };
    }
}
