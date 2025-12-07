using DynamicForms.Core.V4.Schemas;

namespace DynamicForms.Editor.Models;

public class FileUploadConfigModel
{
    public string AllowedExtensions { get; set; } = "";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public bool AllowMultiple { get; set; }
    public bool ScanRequired { get; set; } = true;

    public static FileUploadConfigModel From(FileUploadConfig? config)
    {
        if (config == null) return new FileUploadConfigModel();
        return new FileUploadConfigModel
        {
            AllowedExtensions = string.Join(", ", config.AllowedExtensions),
            MaxFileSizeBytes = config.MaxFileSizeBytes,
            AllowMultiple = config.AllowMultiple,
            ScanRequired = config.ScanRequired
        };
    }

    public FileUploadConfig ToConfig()
    {
        var extensions = AllowedExtensions.Split(',')
            .Select(e => e.Trim())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToArray();

        return new FileUploadConfig
        {
            AllowedExtensions = extensions,
            MaxFileSizeBytes = MaxFileSizeBytes,
            AllowMultiple = AllowMultiple,
            ScanRequired = ScanRequired
        };
    }
}
