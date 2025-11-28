namespace DynamicForms.Core.Entities;

/// <summary>
/// Base class for multilingual text support in the architecture
/// Provides consistent multilingual text handling across all components
/// </summary>
public abstract class TextBase
{
    /// <summary>
    /// English text content
    /// </summary>
    public string? EN { get; set; }
    
    /// <summary>
    /// French text content
    /// </summary>
    public string? FR { get; set; }

    /// <summary>
    /// Get text in the specified language with fallback logic
    /// </summary>
    /// <param name="language">Language code (EN/FR)</param>
    /// <returns>Text in requested language or fallback</returns>
    public string ToString(string language)
    {
        return language?.ToLower() switch
        {
            "fr" or "french" => FR ?? EN ?? string.Empty,
            "en" or "english" => EN ?? FR ?? string.Empty,
            _ => EN ?? FR ?? string.Empty
        };
    }

    /// <summary>
    /// Check if text is available in the specified language
    /// </summary>
    /// <param name="language">Language code (EN/FR)</param>
    /// <returns>True if text is available in the specified language</returns>
    public bool HasText(string language)
    {
        return language?.ToLower() switch
        {
            "fr" or "french" => !string.IsNullOrWhiteSpace(FR),
            "en" or "english" => !string.IsNullOrWhiteSpace(EN),
            _ => !string.IsNullOrWhiteSpace(EN) || !string.IsNullOrWhiteSpace(FR)
        };
    }
    
    /// <summary>
    /// Check if any text content is available
    /// </summary>
    /// <returns>True if either EN or FR text is available</returns>
    public bool HasAnyText()
    {
        return !string.IsNullOrWhiteSpace(EN) || !string.IsNullOrWhiteSpace(FR);
    }
    
    /// <summary>
    /// Get the default text (preferring EN, then FR)
    /// </summary>
    /// <returns>Default text content</returns>
    public override string ToString()
    {
        return EN ?? FR ?? string.Empty;
    }
    
    /// <summary>
    /// Create a copy of this text instance
    /// </summary>
    /// <returns>New instance with same content</returns>
    public virtual TextClass Clone()
    {
        return new TextClass
        {
            EN = this.EN,
            FR = this.FR
        };
    }
    
    /// <summary>
    /// Set text for both languages
    /// </summary>
    /// <param name="en">English text</param>
    /// <param name="fr">French text (optional, defaults to EN if not provided)</param>
    public void SetText(string en, string? fr = null)
    {
        EN = en;
        FR = fr ?? en;
    }
    
    /// <summary>
    /// Clear all text content
    /// </summary>
    public void Clear()
    {
        EN = null;
        FR = null;
    }
}

/// <summary>
/// Concrete implementation of multilingual text for the architecture
/// Used throughout the system for consistent text handling
/// </summary>
public class TextClass : TextBase
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public TextClass() { }
    
    /// <summary>
    /// Constructor with initial English text
    /// </summary>
    /// <param name="englishText">Initial English text</param>
    public TextClass(string englishText)
    {
        EN = englishText;
        FR = englishText; // Default to same value
    }
    
    /// <summary>
    /// Constructor with both English and French text
    /// </summary>
    /// <param name="englishText">English text</param>
    /// <param name="frenchText">French text</param>
    public TextClass(string englishText, string frenchText)
    {
        EN = englishText;
        FR = frenchText;
    }
    
    /// <summary>
    /// Implicit conversion from string to TextClass
    /// </summary>
    /// <param name="text">Text to convert</param>
    public static implicit operator TextClass(string text)
    {
        return new TextClass(text);
    }
    
    /// <summary>
    /// Implicit conversion from TextClass to string
    /// </summary>
    /// <param name="textClass">TextClass to convert</param>
    public static implicit operator string(TextClass textClass)
    {
        return textClass?.ToString() ?? string.Empty;
    }
    
    /// <summary>
    /// Equality comparison
    /// </summary>
    /// <param name="obj">Object to compare</param>
    /// <returns>True if content is equal</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not TextClass other) return false;
        return string.Equals(EN, other.EN, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(FR, other.FR, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Get hash code based on content
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(EN?.GetHashCode() ?? 0, FR?.GetHashCode() ?? 0);
    }
}