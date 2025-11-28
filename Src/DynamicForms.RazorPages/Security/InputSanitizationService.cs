using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Html;

namespace DynamicForms.RazorPages.Security
{
    /// <summary>
    /// Input sanitization service for form data security
    /// </summary>
    public class InputSanitizationService
    {
        private static readonly Regex ScriptTagRegex = new(@"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", 
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        private static readonly Regex HtmlTagRegex = new(@"<[^>]+>", 
            RegexOptions.Compiled);
        
        private static readonly Regex SqlInjectionRegex = new(
            @"('|(\\')|(;)|(\s*(union|select|insert|delete|update|create|alter|exec|execute|drop|truncate)\s+))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ILogger<InputSanitizationService> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public InputSanitizationService(ILogger<InputSanitizationService> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            _logger = logger;
        }

        /// <summary>
        /// Sanitize form data dictionary
        /// </summary>
        public Dictionary<string, object> SanitizeFormData(Dictionary<string, object> formData)
        {
            var sanitizedData = new Dictionary<string, object>();

            foreach (var kvp in formData)
            {
                sanitizedData[kvp.Key] = SanitizeValue(kvp.Value, kvp.Key);
            }

            return sanitizedData;
        }

        /// <summary>
        /// Sanitize individual value based on context
        /// </summary>
        public object SanitizeValue(object? value, string fieldName = "")
        {
            if (value == null) return string.Empty;

            var stringValue = value.ToString();
            if (string.IsNullOrEmpty(stringValue)) return string.Empty;

            try
            {
                // Log potentially dangerous input
                if (ContainsSuspiciousContent(stringValue))
                {
                    _logger.LogWarning("Potentially dangerous input detected in field '{FieldName}': {Input}", 
                        fieldName, stringValue.Length > 100 ? stringValue[..100] + "..." : stringValue);
                }

                // Apply sanitization based on field type/name
                var sanitized = ApplySanitization(stringValue, GetFieldType(fieldName));
                
                return sanitized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing input for field '{FieldName}'", fieldName);
                return string.Empty; // Safe fallback
            }
        }

        /// <summary>
        /// HTML encode for display purposes
        /// </summary>
        public string HtmlEncode(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return HttpUtility.HtmlEncode(input);
        }

        /// <summary>
        /// Create safe HTML string for Razor rendering
        /// </summary>
        public HtmlString CreateSafeHtml(string input)
        {
            var sanitized = SanitizeHtmlContent(input);
            return new HtmlString(sanitized);
        }

        /// <summary>
        /// Validate input against security policies
        /// </summary>
        public ValidationResult ValidateInput(string input, string fieldName)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(input))
                return result;

            // Check for script tags
            if (ScriptTagRegex.IsMatch(input))
            {
                result.AddError($"Script content not allowed in field '{fieldName}'");
            }

            // Check for SQL injection patterns
            if (SqlInjectionRegex.IsMatch(input))
            {
                result.AddError($"Potentially dangerous SQL content detected in field '{fieldName}'");
            }

            // Check length limits
            if (input.Length > GetMaxLength(fieldName))
            {
                result.AddError($"Input too long for field '{fieldName}'. Maximum {GetMaxLength(fieldName)} characters allowed.");
            }

            // Check for excessive HTML tags
            var htmlMatches = HtmlTagRegex.Matches(input);
            if (htmlMatches.Count > 10) // Configurable limit
            {
                result.AddError($"Too many HTML tags in field '{fieldName}'");
            }

            return result;
        }

        #region Private Methods

        private string ApplySanitization(string input, FieldSanitizationType fieldType)
        {
            return fieldType switch
            {
                FieldSanitizationType.PlainText => SanitizePlainText(input),
                FieldSanitizationType.Email => SanitizeEmail(input),
                FieldSanitizationType.Url => SanitizeUrl(input),
                FieldSanitizationType.Html => SanitizeHtmlContent(input),
                FieldSanitizationType.Number => SanitizeNumber(input),
                FieldSanitizationType.Phone => SanitizePhone(input),
                _ => SanitizePlainText(input)
            };
        }

        private string SanitizePlainText(string input)
        {
            // Remove script tags
            var sanitized = ScriptTagRegex.Replace(input, string.Empty);
            
            // HTML encode dangerous characters
            sanitized = HttpUtility.HtmlEncode(sanitized);
            
            // Trim whitespace
            sanitized = sanitized.Trim();
            
            return sanitized;
        }

        private string SanitizeEmail(string input)
        {
            // Basic email sanitization
            var sanitized = input.Trim().ToLowerInvariant();
            
            // Remove script content
            sanitized = ScriptTagRegex.Replace(sanitized, string.Empty);
            
            // Validate email format
            if (!IsValidEmail(sanitized))
            {
                _logger.LogWarning("Invalid email format detected: {Email}", sanitized);
                return string.Empty;
            }
            
            return sanitized;
        }

        private string SanitizeUrl(string input)
        {
            var sanitized = input.Trim();
            
            // Remove script content
            sanitized = ScriptTagRegex.Replace(sanitized, string.Empty);
            
            // Validate URL format and scheme
            if (Uri.TryCreate(sanitized, UriKind.Absolute, out var uri))
            {
                // Only allow safe schemes
                if (uri.Scheme == "http" || uri.Scheme == "https" || uri.Scheme == "ftp")
                {
                    return uri.ToString();
                }
            }
            
            _logger.LogWarning("Invalid or unsafe URL detected: {Url}", input);
            return string.Empty;
        }

        private string SanitizeHtmlContent(string input)
        {
            // Allow safe HTML tags only
            var allowedTags = new[] { "b", "i", "em", "strong", "p", "br", "ul", "ol", "li", "a" };
            
            // Simple implementation - for production, use a library like HtmlSanitizer
            var sanitized = input;
            
            // Remove script tags
            sanitized = ScriptTagRegex.Replace(sanitized, string.Empty);
            
            // Remove dangerous attributes like onclick, onload, etc.
            sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*[""'][^""']*[""']", string.Empty, RegexOptions.IgnoreCase);
            
            // Remove javascript: links
            sanitized = Regex.Replace(sanitized, @"javascript:", string.Empty, RegexOptions.IgnoreCase);
            
            return sanitized;
        }

        private string SanitizeNumber(string input)
        {
            // Remove all non-numeric characters except decimal point and minus sign
            var sanitized = Regex.Replace(input, @"[^0-9.-]", string.Empty);
            
            // Validate as number
            if (decimal.TryParse(sanitized, out _))
            {
                return sanitized;
            }
            
            return "0";
        }

        private string SanitizePhone(string input)
        {
            // Remove all non-numeric characters except +, -, (, ), and spaces
            var sanitized = Regex.Replace(input, @"[^0-9+\-() ]", string.Empty);
            return sanitized.Trim();
        }

        private bool ContainsSuspiciousContent(string input)
        {
            // Check for common attack patterns
            var suspiciousPatterns = new[]
            {
                "<script", "javascript:", "vbscript:", "onload=", "onclick=", "onerror=",
                "eval(", "expression(", "alert(", "document.cookie", "window.location",
                "union select", "drop table", "exec(", "execute("
            };

            return suspiciousPatterns.Any(pattern => 
                input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private FieldSanitizationType GetFieldType(string fieldName)
        {
            // Determine field type based on name or metadata
            fieldName = fieldName.ToLowerInvariant();
            
            if (fieldName.Contains("email")) return FieldSanitizationType.Email;
            if (fieldName.Contains("url") || fieldName.Contains("website")) return FieldSanitizationType.Url;
            if (fieldName.Contains("phone") || fieldName.Contains("telephone")) return FieldSanitizationType.Phone;
            if (fieldName.Contains("number") || fieldName.Contains("amount")) return FieldSanitizationType.Number;
            if (fieldName.Contains("description") || fieldName.Contains("content")) return FieldSanitizationType.Html;
            
            return FieldSanitizationType.PlainText;
        }

        private int GetMaxLength(string fieldName)
        {
            // Return appropriate max lengths based on field type
            fieldName = fieldName.ToLowerInvariant();
            
            if (fieldName.Contains("email")) return 254;
            if (fieldName.Contains("url")) return 2048;
            if (fieldName.Contains("phone")) return 20;
            if (fieldName.Contains("description")) return 4000;
            if (fieldName.Contains("content")) return 8000;
            
            return 255; // Default
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Field sanitization types
    /// </summary>
    public enum FieldSanitizationType
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        PlainText,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Email,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Url,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Html,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Number,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Phone
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Validation result for input checking
    /// </summary>
    public class ValidationResult
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<string> Errors { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool IsValid => !Errors.Any();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void AddError(string error)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Errors.Add(error);
        }
    }
}