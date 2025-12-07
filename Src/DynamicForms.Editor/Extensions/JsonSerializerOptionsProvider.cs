using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicForms.Editor.Extensions;

public static class JsonSerializerOptionsProvider
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            // NOTE: To support FieldTypeConfig polymorphism, you must ensure 
            // the base classes have the appropriate [JsonDerivedType] attributes 
            // as defined in the Schema Reference.
        }
    };
    
    // NOTE: To support FieldTypeConfig polymorphism, you must ensure 
    // the base classes have the appropriate [JsonDerivedType] attributes 
    // as defined in the Schema Reference.
}
