using DynamicForms.Core.Entities;


namespace DynamicForms.Core.Entities.Data;

/// <summary>
/// Represents user-entered data for a JSON field
/// Independent implementation for the reusable DynamicForms library
/// </summary>
public class FormDataItem
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public FormDataItem()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Name = new TextClass();
        Values = new List<ControlValue>();
        ModalRecords = new List<ModalRecord>();
        Metadata = new Dictionary<string, object>();
        DateCreated = DateTime.UtcNow;
        DateUpdated = DateTime.UtcNow;
    }

    #region Core Properties
    /// <summary>
    /// GUID ID linking back to json metadata field
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for the field data
    /// </summary>
    public TextClass Name { get; set; }
    
    /// <summary>
    /// Simple field value (text, number, etc.)
    /// </summary>
    public string? Value { get; set; }
    
    /// <summary>
    /// Original value before any modifications (for audit trail)
    /// </summary>
    public string? OriginalValue { get; set; }
    #endregion

    #region Complex Data Types
    /// <summary>
    /// Collection of values for multi-select fields (checkboxes, multi-dropdown)
    /// </summary>
    public List<ControlValue> Values { get; set; }
    
    /// <summary>
    /// Complex data for specialized controls
    /// </summary>
    public SpeciesAutoCompleteData? SpeciesAutoCompleteData { get; set; }
    
    /// <summary>
    /// Data for modal/popup forms
    /// </summary>
    public List<ModalRecord> ModalRecords { get; set; }
    
    /// <summary>
    /// File upload information
    /// </summary>
    public FileUploadData? FileUploadData { get; set; }
    
    /// <summary>
    /// Address data for address lookup fields
    /// </summary>
    public AddressData? AddressData { get; set; }
    #endregion

    #region Metadata and Tracking
    /// <summary>
    /// Additional metadata for extensibility
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; }
    
    /// <summary>
    /// When this data item was created
    /// </summary>
    public DateTime DateCreated { get; set; }
    
    /// <summary>
    /// When this data item was last updated
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
    /// Validation status of this data item
    /// </summary>
    public bool IsValid { get; set; } = true;
    
    /// <summary>
    /// Validation error messages
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();
    #endregion

    #region Methods
    /// <summary>
    /// Returns comma-separated values for multi-select controls
    /// </summary>
    public string StringifyValues()
    {
        if (Values?.Count > 0)
        {
            return string.Join(",", Values.Select(v => v.Value));
        }
        return string.Empty;
    }
    
    /// <summary>
    /// Get display value based on field type and language
    /// </summary>
    public string GetDisplayValue(string language = "EN")
    {
        if (!string.IsNullOrEmpty(Value))
            return Value;
            
        if (Values?.Count > 0)
        {
            return string.Join(", ", Values.Select(v => v.Text.ToString(language)));
        }
        
        if (SpeciesAutoCompleteData != null)
            return SpeciesAutoCompleteData.GetDataForDisplay(language);
            
        if (FileUploadData != null)
            return FileUploadData.FileName ?? "File uploaded";
            
        if (AddressData != null)
#pragma warning disable CS8603 // Possible null reference return.
            return AddressData.FormattedAddress;
#pragma warning restore CS8603 // Possible null reference return.
            
        return string.Empty;
    }
    
    /// <summary>
    /// Check if the data item has any value
    /// </summary>
    public bool HasValue()
    {
        return !string.IsNullOrWhiteSpace(Value) ||
               (Values?.Count > 0) ||
               (SpeciesAutoCompleteData != null) ||
               (FileUploadData != null) ||
               (AddressData != null) ||
               (ModalRecords?.Count > 0);
    }
    
    /// <summary>
    /// Update the data and track changes
    /// </summary>
    public void UpdateValue(string? newValue, string? updatedBy = null)
    {
        if (Value != newValue)
        {
            OriginalValue = Value;
            Value = newValue;
            DateUpdated = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }
    
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
    }
    #endregion
}

/// <summary>
/// Modal record containing multiple field values
/// </summary>
public class ModalRecord
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ModalRecord()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Values = new List<FormDataItem>();
        DateCreated = DateTime.UtcNow;
    }
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Id { get; set; } = Guid.NewGuid().ToString();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<FormDataItem> Values { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime DateCreated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CreatedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsDeleted { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
    /// <summary>
    /// Get display value for a specific field in this record
    /// </summary>
    public string GetFieldValue(string fieldId, string language = "EN")
    {
        var fieldData = Values.FirstOrDefault(v => v.Id == fieldId);
        return fieldData?.GetDisplayValue(language) ?? string.Empty;
    }
}

/// <summary>
/// Multilingual text class using the architecture
/// </summary>
public class TextClass : TextBase { }

/// <summary>
/// Control value for selections
/// </summary>
public class ControlValue
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ControlValue()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Text = new TextClass();
    }
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Value { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public TextClass Text { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsSelected { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public int Order { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, object>? Metadata { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string GetDisplayText(string language = "EN")
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return Text.ToString(language);
    }
}

/// <summary>
/// Species autocomplete data structure
/// </summary>
public class SpeciesAutoCompleteData
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public SpeciesAutoCompleteData()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Text = new TextClass();
        Cosewic = new SpeciesLabel();
        Sara = new SpeciesLabel();
        Funding = new SpeciesLabel();
        AdditionalProperties = new Dictionary<string, SpeciesLabel>();
    }
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public TextClass Text { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public SpeciesLabel Cosewic { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public SpeciesLabel Sara { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public SpeciesLabel Funding { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ScientificName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? CommonName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? TaxonomyId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, SpeciesLabel> AdditionalProperties { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string GetDataForDisplay(string language) => Text.ToString(language);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IDictionary<string, string> GetAdditionalDataForDisplay(FormField field, string language)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var sacf = field?.SpeciesAutoCompleteFields.OrderBy(s => s.Order);
        var vals = new Dictionary<string, string>();
        
        if (sacf != null)
        {
            foreach (var s in sacf)
            {
                if (!s.IsVisible) continue;

                var value = s.Key.ToLower() switch
                {
                    "cosewic" => Cosewic.Value.ToString(language),
                    "sara" => Sara.Value.ToString(language),
                    "funding" => Funding.Value.ToString(language),
                    "scientific" => ScientificName ?? string.Empty,
                    "common" => CommonName ?? string.Empty,
                    _ => AdditionalProperties.TryGetValue(s.Key, out var prop) ? prop.Value.ToString(language) : string.Empty
                };
                
                if (!string.IsNullOrEmpty(value))
                {
                    vals.Add(s.Name.ToString(language), value);
                }
            }
        }
        return vals;
    }
}

/// <summary>
/// Species label for classification data
/// </summary>
public class SpeciesLabel
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public SpeciesLabel()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        Name = new TextClass();
        Value = new TextClass();
    }
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public TextClass Name { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public TextClass Value { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Code { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// File upload data structure
/// </summary>
public class FileUploadData
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FileName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? OriginalFileName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ContentType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public long FileSize { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FilePath { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FileHash { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? UploadedBy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsVirusScanned { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool IsVirusClean { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Dictionary<string, object>? Metadata { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string GetFileExtension()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return Path.GetExtension(FileName ?? string.Empty);
    }
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string GetFormattedFileSize()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = FileSize;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// Address data structure for address lookup fields
/// </summary>
public class AddressData
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? StreetNumber { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? StreetName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? ApartmentUnit { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? City { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Province { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? PostalCode { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? Country { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double? Latitude { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public double? Longitude { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string? FormattedAddress { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string GetFullAddress()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(StreetNumber) && !string.IsNullOrEmpty(StreetName))
            parts.Add($"{StreetNumber} {StreetName}");
        else if (!string.IsNullOrEmpty(StreetName))
            parts.Add(StreetName);
            
        if (!string.IsNullOrEmpty(ApartmentUnit))
            parts.Add($"Unit {ApartmentUnit}");
            
        if (!string.IsNullOrEmpty(City))
            parts.Add(City);
            
        if (!string.IsNullOrEmpty(Province))
            parts.Add(Province);
            
        if (!string.IsNullOrEmpty(PostalCode))
            parts.Add(PostalCode);
            
        if (!string.IsNullOrEmpty(Country))
            parts.Add(Country);
            
        return string.Join(", ", parts);
    }
}