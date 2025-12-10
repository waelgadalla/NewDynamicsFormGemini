using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VisualEditorOpus.Models;

namespace VisualEditorOpus.Services;

/// <summary>
/// Loads CodeSet items from various data sources (API, File)
/// </summary>
public class CodeSetLoader : ICodeSetLoader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CodeSetLoader> _logger;

    public CodeSetLoader(HttpClient httpClient, ILogger<CodeSetLoader> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ManagedCodeSetItem>> LoadAsync(CodeSetSource source)
    {
        return source.Type switch
        {
            CodeSetSourceType.Api => await LoadFromApiAsync(source),
            CodeSetSourceType.File => await LoadFromFileAsync(source),
            CodeSetSourceType.Static => new List<ManagedCodeSetItem>(),
            _ => new List<ManagedCodeSetItem>()
        };
    }

    public async Task<CodeSetLoaderResult> TestSourceAsync(CodeSetSource source)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var items = await LoadAsync(source);
            stopwatch.Stop();

            return new CodeSetLoaderResult
            {
                Success = true,
                Items = items.Take(10).ToList(), // Return sample
                TotalCount = items.Count,
                LoadTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to test source: {Type}", source.Type);

            return new CodeSetLoaderResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                LoadTime = stopwatch.Elapsed
            };
        }
    }

    private async Task<List<ManagedCodeSetItem>> LoadFromApiAsync(CodeSetSource source)
    {
        if (string.IsNullOrEmpty(source.ApiEndpoint))
        {
            _logger.LogWarning("API endpoint not specified");
            return new List<ManagedCodeSetItem>();
        }

        try
        {
            var request = new HttpRequestMessage(
                new HttpMethod(source.HttpMethod),
                source.ApiEndpoint);

            // Add custom headers
            foreach (var header in source.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Add request body if POST
            if (source.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(source.RequestBody))
            {
                request.Content = new StringContent(
                    source.RequestBody,
                    Encoding.UTF8,
                    "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // Extract data using response path if specified
            if (!string.IsNullOrEmpty(source.ResponsePath))
            {
                var doc = JsonDocument.Parse(json);
                var element = NavigateJsonPath(doc.RootElement, source.ResponsePath);
                json = element.GetRawText();
            }

            var items = JsonSerializer.Deserialize<List<ManagedCodeSetItem>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ManagedCodeSetItem>();

            _logger.LogInformation("Loaded {Count} items from API: {Endpoint}",
                items.Count, source.ApiEndpoint);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load CodeSet from API: {Endpoint}", source.ApiEndpoint);
            return new List<ManagedCodeSetItem>();
        }
    }

    private async Task<List<ManagedCodeSetItem>> LoadFromFileAsync(CodeSetSource source)
    {
        if (string.IsNullOrEmpty(source.FilePath))
        {
            _logger.LogWarning("File path not specified");
            return new List<ManagedCodeSetItem>();
        }

        try
        {
            var extension = Path.GetExtension(source.FilePath).ToLowerInvariant();

            return extension switch
            {
                ".json" => await LoadFromJsonFileAsync(source.FilePath),
                ".csv" => await LoadFromCsvFileAsync(source.FilePath),
                _ => throw new NotSupportedException($"File type {extension} not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load CodeSet from file: {Path}", source.FilePath);
            return new List<ManagedCodeSetItem>();
        }
    }

    private async Task<List<ManagedCodeSetItem>> LoadFromJsonFileAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<ManagedCodeSetItem>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new List<ManagedCodeSetItem>();
    }

    private async Task<List<ManagedCodeSetItem>> LoadFromCsvFileAsync(string filePath)
    {
        var items = new List<ManagedCodeSetItem>();

        using var reader = new StreamReader(filePath);

        // Read header
        var header = await reader.ReadLineAsync();
        if (header == null) return items;

        var columns = header.Split(',')
            .Select(c => c.Trim().ToLowerInvariant())
            .ToArray();

        var codeIndex = Array.IndexOf(columns, "code");
        var displayEnIndex = Array.IndexOf(columns, "displaynameen");
        var displayFrIndex = Array.IndexOf(columns, "displaynamefr");
        var descriptionIndex = Array.IndexOf(columns, "description");
        var statusIndex = Array.IndexOf(columns, "status");
        var orderIndex = Array.IndexOf(columns, "order");

        if (codeIndex < 0) codeIndex = 0;
        if (displayEnIndex < 0) displayEnIndex = 1;

        int order = 0;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line)) continue;

            var values = ParseCsvLine(line);

            var item = new ManagedCodeSetItem
            {
                Id = Guid.NewGuid().ToString(),
                Code = GetValue(values, codeIndex),
                DisplayNameEn = GetValue(values, displayEnIndex),
                DisplayNameFr = displayFrIndex >= 0 ? GetValue(values, displayFrIndex) : null,
                Description = descriptionIndex >= 0 ? GetValue(values, descriptionIndex) : null,
                Status = ParseStatus(statusIndex >= 0 ? GetValue(values, statusIndex) : "active"),
                Order = orderIndex >= 0 && int.TryParse(GetValue(values, orderIndex), out var o) ? o : order++
            };

            items.Add(item);
        }

        _logger.LogInformation("Loaded {Count} items from CSV: {Path}", items.Count, filePath);
        return items;
    }

    private static string GetValue(string[] values, int index)
    {
        return index >= 0 && index < values.Length ? values[index] : "";
    }

    private static CodeSetItemStatus ParseStatus(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "inactive" => CodeSetItemStatus.Inactive,
            "deprecated" => CodeSetItemStatus.Deprecated,
            _ => CodeSetItemStatus.Active
        };
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = new StringBuilder();

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString().Trim());

        return result.ToArray();
    }

    private static JsonElement NavigateJsonPath(JsonElement element, string path)
    {
        var parts = path.Split('.');
        foreach (var part in parts)
        {
            if (element.TryGetProperty(part, out var child))
                element = child;
            else
                break;
        }
        return element;
    }
}
