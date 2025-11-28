namespace DynamicForms.Core.Entities.Data;

/// <summary>
/// Represents complete module data including fields and modal data
/// Independent implementation for the DynamicForms library
/// </summary>
public class ModuleFormData
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ModuleFormData()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        DataItems = new List<FormDataItem>();
        Modals = new List<ModalFormData>();
        Metadata = new Dictionary<string, object>();
        DateCreated = DateTime.UtcNow;
        DateUpdated = DateTime.UtcNow;
    }

    #region Core Properties
    /// <summary>
    /// Associated opportunity/program identifier
    /// </summary>
    public int OpportunityId { get; set; }
    
    /// <summary>
    /// Module identifier this data belongs to
    /// </summary>
    public int ModuleId { get; set; }
    
    /// <summary>
    /// Application/submission identifier
    /// </summary>
    public int ApplicationId { get; set; }
    
    /// <summary>
    /// Collection of field data items
    /// </summary>
    public IEnumerable<FormDataItem> DataItems { get; set; }
    
    /// <summary>
    /// Collection of modal/popup data
    /// </summary>
    public IEnumerable<ModalFormData> Modals { get; set; }
    #endregion

    #region Metadata and Tracking
    /// <summary>
    /// Additional metadata for the module data
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; }
    
    /// <summary>
    /// Version of the module schema this data was created with
    /// </summary>
    public float SchemaVersion { get; set; } = 1.0f;
    
    /// <summary>
    /// Current status of the data submission
    /// </summary>
    public string Status { get; set; } = "Draft";
    
    /// <summary>
    /// When this module data was created
    /// </summary>
    public DateTime DateCreated { get; set; }
    
    /// <summary>
    /// When this module data was last updated
    /// </summary>
    public DateTime DateUpdated { get; set; }
    
    /// <summary>
    /// User who created this data
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// User who last updated this data
    /// </summary>
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Language the data was entered in
    /// </summary>
    public string Language { get; set; } = "EN";
    
    /// <summary>
    /// Whether the module data is complete
    /// </summary>
    public bool IsComplete { get; set; }
    
    /// <summary>
    /// Whether the module data is valid
    /// </summary>
    public bool IsValid { get; set; } = true;
    
    /// <summary>
    /// Module-level validation errors
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();
    #endregion

    #region Data Access Methods
    /// <summary>
    /// Get data item by field ID
    /// </summary>
    public FormDataItem? GetDataItem(string fieldId)
    {
        return DataItems.FirstOrDefault(d => d.Id == fieldId);
    }
    
    /// <summary>
    /// Get field value as string
    /// </summary>
    public string GetFieldValue(string fieldId, string language = "EN")
    {
        var dataItem = GetDataItem(fieldId);
        return dataItem?.GetDisplayValue(language) ?? string.Empty;
    }
    
    /// <summary>
    /// Set field value
    /// </summary>
    public void SetFieldValue(string fieldId, string? value, string? updatedBy = null)
    {
        var existingItem = DataItems.FirstOrDefault(d => d.Id == fieldId);
        if (existingItem != null)
        {
            existingItem.UpdateValue(value, updatedBy);
        }
        else
        {
            var newItem = new FormDataItem
            {
                Id = fieldId,
                Value = value,
                CreatedBy = updatedBy,
                UpdatedBy = updatedBy
            };
            ((List<FormDataItem>)DataItems).Add(newItem);
        }
        
        DateUpdated = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
    
    /// <summary>
    /// Check if a required field has a value
    /// </summary>
    public bool HasRequiredFieldValue(string fieldId)
    {
        var dataItem = GetDataItem(fieldId);
        return dataItem?.HasValue() ?? false;
    }
    
    /// <summary>
    /// Get all field values as dictionary
    /// </summary>
    public Dictionary<string, object?> GetAllFieldValues()
    {
        var values = new Dictionary<string, object?>();
        
        foreach (var item in DataItems)
        {
            if (item.Values?.Count > 0)
            {
                values[item.Id] = item.Values.Select(v => v.Value).ToList();
            }
            else if (item.SpeciesAutoCompleteData != null)
            {
                values[item.Id] = item.SpeciesAutoCompleteData;
            }
            else if (item.FileUploadData != null)
            {
                values[item.Id] = item.FileUploadData;
            }
            else if (item.ModalRecords?.Count > 0)
            {
                values[item.Id] = item.ModalRecords;
            }
            else
            {
                values[item.Id] = item.Value;
            }
        }
        
        return values;
    }
    
    /// <summary>
    /// Get modal data by modal ID
    /// </summary>
    public ModalFormData? GetModalData(string modalId)
    {
        return Modals.FirstOrDefault(m => m.ModalId == modalId);
    }
    
    /// <summary>
    /// Calculate completion percentage
    /// </summary>
    public double GetCompletionPercentage(IEnumerable<string> requiredFieldIds)
    {
        if (!requiredFieldIds.Any()) return 100.0;
        
        var completedFields = requiredFieldIds.Count(fieldId => HasRequiredFieldValue(fieldId));
        return (double)completedFields / requiredFieldIds.Count() * 100.0;
    }
    #endregion

    #region Validation Methods
    /// <summary>
    /// Add validation error
    /// </summary>
    public void AddValidationError(string error)
    {
        if (!ValidationErrors.Contains(error))
        {
            ValidationErrors.Add(error);
            IsValid = false;
        }
    }
    
    /// <summary>
    /// Clear validation errors
    /// </summary>
    public void ClearValidationErrors()
    {
        ValidationErrors.Clear();
        IsValid = true;
        
        // Also clear field-level errors
        foreach (var item in DataItems)
        {
            item.ClearValidationErrors();
        }
    }
    
    /// <summary>
    /// Get all validation errors (module and field level)
    /// </summary>
    public List<string> GetAllValidationErrors()
    {
        var allErrors = new List<string>(ValidationErrors);
        
        foreach (var item in DataItems.Where(d => !d.IsValid))
        {
            allErrors.AddRange(item.ValidationErrors);
        }
        
        return allErrors;
    }
    #endregion

    #region Export/Import Methods
    /// <summary>
    /// Export data to dictionary for serialization
    /// </summary>
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            ["OpportunityId"] = OpportunityId,
            ["ModuleId"] = ModuleId,
            ["ApplicationId"] = ApplicationId,
            ["Status"] = Status,
            ["Language"] = Language,
            ["IsComplete"] = IsComplete,
            ["IsValid"] = IsValid,
            ["DateCreated"] = DateCreated,
            ["DateUpdated"] = DateUpdated,
            ["CreatedBy"] = CreatedBy ?? string.Empty,
            ["UpdatedBy"] = UpdatedBy ?? string.Empty,
            ["SchemaVersion"] = SchemaVersion,
            ["DataItems"] = DataItems.ToList(),
            ["Modals"] = Modals.ToList(),
            ["ValidationErrors"] = ValidationErrors,
            ["Metadata"] = Metadata
        };
    }
    
    /// <summary>
    /// Create a summary for reporting
    /// </summary>
    public ModuleDataSummary GetSummary()
    {
        return new ModuleDataSummary
        {
            OpportunityId = OpportunityId,
            ModuleId = ModuleId,
            ApplicationId = ApplicationId,
            Status = Status,
            FieldCount = DataItems.Count(),
            ModalCount = Modals.Count(),
            IsComplete = IsComplete,
            IsValid = IsValid,
            DateCreated = DateCreated,
            DateUpdated = DateUpdated,
            CreatedBy = CreatedBy,
            UpdatedBy = UpdatedBy
        };
    }
    #endregion
}

/// <summary>
/// Represents data for modal/popup forms within a module
/// </summary>
public class ModalFormData
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ModalFormData()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        DataItems = new List<FormDataItem>();
        Metadata = new Dictionary<string, object>();
        DateCreated = DateTime.UtcNow;
    }

    /// <summary>
    /// Unique identifier for this modal data record
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Associated opportunity identifier
    /// </summary>
    public int OpportunityId { get; set; }
    
    /// <summary>
    /// Module identifier
    /// </summary>
    public int ModuleId { get; set; }
    
    /// <summary>
    /// Modal/popup identifier from the schema
    /// </summary>
    public string ModalId { get; set; } = string.Empty;
    
    /// <summary>
    /// Application identifier
    /// </summary>
    public string ApplicationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Collection of field data for this modal record
    /// </summary>
    public IEnumerable<FormDataItem> DataItems { get; set; }
    
    /// <summary>
    /// Display order within the modal
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Whether this modal record is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Record-level metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; }
    
    /// <summary>
    /// When this modal data was created
    /// </summary>
    public DateTime DateCreated { get; set; }
    
    /// <summary>
    /// User who created this modal data
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Get field value from this modal record
    /// </summary>
    public string GetFieldValue(string fieldId, string language = "EN")
    {
        var dataItem = DataItems.FirstOrDefault(d => d.Id == fieldId);
        return dataItem?.GetDisplayValue(language) ?? string.Empty;
    }
    
    /// <summary>
    /// Get a summary representation of this modal record
    /// </summary>
    public string GetSummary(string? primaryFieldId = null, string language = "EN")
    {
        if (!string.IsNullOrEmpty(primaryFieldId))
        {
            return GetFieldValue(primaryFieldId, language);
        }
        
        // Return the first non-empty field value as summary
        var firstValue = DataItems.FirstOrDefault(d => d.HasValue());
        return firstValue?.GetDisplayValue(language) ?? $"Record {Id[..8]}";
    }
}

/// <summary>
/// Summary information for module data (for reporting and lists)
/// </summary>
public class ModuleDataSummary
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int OpportunityId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ModuleId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ApplicationId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Status { get; set; } = string.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int FieldCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int ModalCount { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsComplete { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsValid { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime DateCreated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime DateUpdated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CreatedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? UpdatedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}