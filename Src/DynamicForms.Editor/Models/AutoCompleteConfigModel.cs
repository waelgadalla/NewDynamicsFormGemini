using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Models;

public class AutoCompleteConfigModel
{
    public string DataSourceUrl { get; set; } = "";
    public string QueryParameter { get; set; } = "q";
    public int MinCharacters { get; set; } = 3;
    public string ValueField { get; set; } = "";
    public string DisplayField { get; set; } = "";
    public string? ItemTemplate { get; set; }

    public static AutoCompleteConfigModel From(AutoCompleteConfig? config)
    {
        if (config == null) return new AutoCompleteConfigModel();
        return new AutoCompleteConfigModel
        {
            DataSourceUrl = config.DataSourceUrl,
            QueryParameter = config.QueryParameter,
            MinCharacters = config.MinCharacters,
            ValueField = config.ValueField,
            DisplayField = config.DisplayField,
            ItemTemplate = config.ItemTemplate
        };
    }

    public AutoCompleteConfig ToConfig()
    {
        return new AutoCompleteConfig
        {
            DataSourceUrl = DataSourceUrl,
            QueryParameter = QueryParameter,
            MinCharacters = MinCharacters,
            ValueField = ValueField,
            DisplayField = DisplayField,
            ItemTemplate = ItemTemplate
        };
    }
}
