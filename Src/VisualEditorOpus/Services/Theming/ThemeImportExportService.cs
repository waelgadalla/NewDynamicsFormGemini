using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DynamicForms.Models.Theming;

namespace VisualEditorOpus.Services.Theming;

/// <summary>
/// Implementation of theme import/export functionality.
/// </summary>
public partial class ThemeImportExportService : IThemeImportExportService
{
    private readonly IThemeCssGeneratorService _cssGenerator;

    private static readonly JsonSerializerOptions PrettyPrintOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions ImportOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public ThemeImportExportService(IThemeCssGeneratorService cssGenerator)
    {
        _cssGenerator = cssGenerator;
    }

    #region Export Methods

    /// <inheritdoc />
    public string ExportToJson(FormTheme theme, bool prettyPrint = true)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var options = prettyPrint ? PrettyPrintOptions : CompactOptions;
        return JsonSerializer.Serialize(theme, options);
    }

    /// <inheritdoc />
    public byte[] ExportToJsonBytes(FormTheme theme)
    {
        var json = ExportToJson(theme, prettyPrint: true);
        return Encoding.UTF8.GetBytes(json);
    }

    /// <inheritdoc />
    public string ExportToCss(FormTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        return _cssGenerator.GenerateStylesheet(theme);
    }

    /// <inheritdoc />
    public byte[] ExportToCssBytes(FormTheme theme)
    {
        var css = ExportToCss(theme);
        return Encoding.UTF8.GetBytes(css);
    }

    /// <inheritdoc />
    public byte[] ExportToMinifiedCssBytes(FormTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        var css = _cssGenerator.GenerateMinifiedCss(theme);
        return Encoding.UTF8.GetBytes(css);
    }

    /// <inheritdoc />
    public string GetCssVariablesForClipboard(FormTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var variables = _cssGenerator.GetAllVariables(theme);
        var sb = new StringBuilder();

        sb.AppendLine("/* Theme: " + theme.Name + " */");
        sb.AppendLine(":root {");

        foreach (var (name, value) in variables.OrderBy(v => v.Key))
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.AppendLine($"  {name}: {value};");
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <inheritdoc />
    public string GetExportFilename(FormTheme theme, string extension)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var name = theme.Name ?? "theme";
        var sanitized = SanitizeFilename().Replace(name, "-");
        sanitized = sanitized.Trim('-');

        if (string.IsNullOrEmpty(sanitized))
            sanitized = "theme";

        var timestamp = DateTime.Now.ToString("yyyyMMdd");
        return $"{sanitized}_{timestamp}.{extension.TrimStart('.')}";
    }

    [GeneratedRegex(@"[^a-zA-Z0-9\-_]", RegexOptions.Compiled)]
    private static partial Regex SanitizeFilename();

    #endregion

    #region Import Methods

    /// <inheritdoc />
    public ThemeImportResult ImportFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return ThemeImportResult.Failed("JSON content is empty.");
        }

        var errors = new List<string>();
        var warnings = new List<string>();

        try
        {
            // First, validate JSON structure
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check for required properties
            if (!root.TryGetProperty("name", out _) && !root.TryGetProperty("Name", out _))
            {
                warnings.Add("Theme name is missing, a default name will be used.");
            }

            // Check for deprecated properties (future-proofing)
            CheckForDeprecatedProperties(root, warnings);

            // Deserialize the theme
            var theme = JsonSerializer.Deserialize<FormTheme>(json, ImportOptions);

            if (theme is null)
            {
                return ThemeImportResult.Failed("Failed to parse theme data.");
            }

            // Apply defaults for missing required values
            EnsureRequiredValues(theme, warnings);

            // Generate a new ID for imported themes to avoid conflicts
            theme.Id = Guid.NewGuid().ToString();
            theme.CreatedAt = DateTime.UtcNow;
            theme.ModifiedAt = DateTime.UtcNow;

            return ThemeImportResult.Succeeded(theme, warnings.Count > 0 ? warnings : null);
        }
        catch (JsonException ex)
        {
            errors.Add($"Invalid JSON format: {ex.Message}");
            return ThemeImportResult.Failed(errors);
        }
        catch (Exception ex)
        {
            errors.Add($"Unexpected error during import: {ex.Message}");
            return ThemeImportResult.Failed(errors);
        }
    }

    /// <inheritdoc />
    public async Task<ThemeImportResult> ImportFromJsonAsync(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            return ImportFromJson(json);
        }
        catch (Exception ex)
        {
            return ThemeImportResult.Failed($"Failed to read file: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> ValidateJson(string json)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            errors.Add("JSON content is empty.");
            return errors;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Validate that it's an object
            if (root.ValueKind != JsonValueKind.Object)
            {
                errors.Add("JSON must be an object, not an array or primitive value.");
                return errors;
            }

            // Check for basic theme structure
            var hasColors = root.TryGetProperty("colors", out _) || root.TryGetProperty("Colors", out _);
            var hasTypography = root.TryGetProperty("typography", out _) || root.TryGetProperty("Typography", out _);

            if (!hasColors && !hasTypography)
            {
                errors.Add("JSON does not appear to be a valid theme. Missing 'colors' and 'typography' sections.");
            }

            // Try to deserialize to check for type errors
            try
            {
                JsonSerializer.Deserialize<FormTheme>(json, ImportOptions);
            }
            catch (JsonException ex)
            {
                errors.Add($"Theme structure error: {ex.Message}");
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Invalid JSON format: {ex.Message}");
        }

        return errors;
    }

    #endregion

    #region Helper Methods

    private static void CheckForDeprecatedProperties(JsonElement root, List<string> warnings)
    {
        // Check for any deprecated property names (for future compatibility)
        var deprecatedProperties = new Dictionary<string, string>
        {
            { "primaryColor", "Use 'colors.primary' instead." },
            { "backgroundColor", "Use 'colors.background' instead." },
            { "fontFamily", "Use 'typography.fontFamily' instead." }
        };

        foreach (var (oldName, message) in deprecatedProperties)
        {
            if (root.TryGetProperty(oldName, out _))
            {
                warnings.Add($"Deprecated property '{oldName}': {message}");
            }
        }
    }

    private static void EnsureRequiredValues(FormTheme theme, List<string> warnings)
    {
        // Ensure name
        if (string.IsNullOrWhiteSpace(theme.Name))
        {
            theme.Name = "Imported Theme";
        }

        // Ensure colors
        theme.Colors ??= new ThemeColors();

        // Ensure typography
        theme.Typography ??= new ThemeTypography();

        // Ensure spacing
        theme.Spacing ??= new ThemeSpacing();

        // Ensure borders
        theme.Borders ??= new ThemeBorders();

        // Ensure shadows
        theme.Shadows ??= new ThemeShadows();

        // Ensure header
        theme.Header ??= new ThemeHeader();

        // Ensure background
        theme.Background ??= new ThemeBackground();

        // Ensure components
        theme.Components ??= new ThemeComponentStyles();

        // Ensure accessibility
        theme.Accessibility ??= new ThemeAccessibility();

        // Check for empty required color values
        if (string.IsNullOrEmpty(theme.Colors.Primary))
        {
            warnings.Add("Primary color was missing, using default value.");
            theme.Colors.Primary = "#6366F1";
        }

        if (string.IsNullOrEmpty(theme.Colors.Background))
        {
            warnings.Add("Background color was missing, using default value.");
            theme.Colors.Background = "#FFFFFF";
        }
    }

    #endregion
}
