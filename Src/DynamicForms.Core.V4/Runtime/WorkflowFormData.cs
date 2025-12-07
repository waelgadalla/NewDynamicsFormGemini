namespace DynamicForms.Core.V4.Runtime;

/// <summary>
/// Container for multi-module workflow form data.
/// Organizes data by module to enable cross-module field references and workflow-level rule evaluation.
/// </summary>
public class WorkflowFormData
{
    /// <summary>
    /// Dictionary of module data organized by module key.
    /// Key: Module key (can be numeric ID like "1" or named key like "PersonalInfo")
 /// Value: Dictionary of field values (FieldId => Value)
  /// 
    /// Example structure:
    /// {
    ///   "1": { "first_name": "John", "last_name": "Doe", "age": 25 },
    ///   "PersonalInfo": { "first_name": "John", "last_name": "Doe", "age": 25 },
    ///   "2": { "organization": "Acme Corp", "org_type": "Business" }
    /// }
    /// </summary>
    public Dictionary<string, Dictionary<string, object?>> Modules { get; set; } = new();

    /// <summary>
    /// Current active module key.
    /// Used as fallback when field references don't specify a module.
    /// Example: If CurrentModuleKey = "1", then Field="age" resolves to "1.age"
    /// </summary>
    public string? CurrentModuleKey { get; set; }

 /// <summary>
    /// Gets a field value with optional module scoping.
    /// </summary>
    /// <param name="moduleKey">Module key (null uses CurrentModuleKey)</param>
    /// <param name="fieldId">Field identifier</param>
    /// <returns>Field value or null if not found</returns>
    public object? GetFieldValue(string? moduleKey, string fieldId)
    {
        // Use specified module or fall back to current module
        var targetModule = moduleKey ?? CurrentModuleKey;

        if (targetModule != null && Modules.TryGetValue(targetModule, out var moduleData))
{
        return moduleData.GetValueOrDefault(fieldId);
        }

        return null;
    }

    /// <summary>
    /// Sets a field value in a specific module.
    /// </summary>
    /// <param name="moduleKey">Module key</param>
    /// <param name="fieldId">Field identifier</param>
    /// <param name="value">Value to set</param>
    public void SetFieldValue(string moduleKey, string fieldId, object? value)
    {
      if (!Modules.ContainsKey(moduleKey))
        {
            Modules[moduleKey] = new Dictionary<string, object?>();
        }

        Modules[moduleKey][fieldId] = value;
    }

    /// <summary>
    /// Checks if a module exists in the workflow data.
    /// </summary>
    /// <param name="moduleKey">Module key to check</param>
    /// <returns>True if module exists</returns>
    public bool HasModule(string moduleKey)
    {
  return Modules.ContainsKey(moduleKey);
    }

  /// <summary>
    /// Gets all field data for a specific module.
    /// </summary>
 /// <param name="moduleKey">Module key</param>
    /// <returns>Dictionary of field values or null if module not found</returns>
public Dictionary<string, object?>? GetModuleData(string moduleKey)
    {
  return Modules.GetValueOrDefault(moduleKey);
    }

    /// <summary>
    /// Sets all field data for a specific module.
    /// </summary>
    /// <param name="moduleKey">Module key</param>
    /// <param name="moduleData">Dictionary of field values</param>
  public void SetModuleData(string moduleKey, Dictionary<string, object?> moduleData)
    {
  Modules[moduleKey] = moduleData;
    }

    /// <summary>
    /// Gets the total number of modules with data.
    /// </summary>
    public int ModuleCount => Modules.Count;

    /// <summary>
    /// Gets all module keys that have data.
    /// </summary>
    public IEnumerable<string> ModuleKeys => Modules.Keys;

    /// <summary>
    /// Creates a WorkflowFormData instance from a single module's data.
    /// Useful for single-module scenarios or backward compatibility.
    /// </summary>
    /// <param name="moduleKey">Module key</param>
    /// <param name="fieldData">Field data dictionary</param>
    /// <returns>WorkflowFormData with single module</returns>
    public static WorkflowFormData FromSingleModule(string moduleKey, Dictionary<string, object?> fieldData)
    {
        return new WorkflowFormData
        {
          Modules = new Dictionary<string, Dictionary<string, object?>>
     {
  { moduleKey, fieldData }
      },
      CurrentModuleKey = moduleKey
        };
    }

    /// <summary>
    /// Creates an empty WorkflowFormData instance.
    /// </summary>
    /// <returns>Empty WorkflowFormData</returns>
    public static WorkflowFormData Empty()
    {
        return new WorkflowFormData();
  }
}
