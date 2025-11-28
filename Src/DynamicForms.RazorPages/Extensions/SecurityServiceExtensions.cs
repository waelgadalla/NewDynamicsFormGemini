using DynamicForms.RazorPages.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering DynamicForms security services
    /// </summary>
    public static class SecurityServiceExtensions
    {
        /// <summary>
        /// Add DynamicForms security services to the service collection
        /// </summary>
        public static IServiceCollection AddDynamicFormsSecurity(this IServiceCollection services)
        {
            services.AddScoped<InputSanitizationService>();
            
            return services;
        }

        /// <summary>
        /// Add DynamicForms security services with configuration
        /// </summary>
        public static IServiceCollection AddDynamicFormsSecurity(this IServiceCollection services, 
            Action<SecurityOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddScoped<InputSanitizationService>();
            
            return services;
        }
    }

    /// <summary>
    /// Configuration options for DynamicForms security
    /// </summary>
    public class SecurityOptions
    {
        /// <summary>
        /// Maximum allowed input length per field
        /// </summary>
        public int MaxInputLength { get; set; } = 10000;

        /// <summary>
        /// Whether to log security violations
        /// </summary>
        public bool LogSecurityViolations { get; set; } = true;

        /// <summary>
        /// Whether to block requests with security violations
        /// </summary>
        public bool BlockMaliciousRequests { get; set; } = true;

        /// <summary>
        /// Allowed HTML tags for rich text fields
        /// </summary>
        public string[] AllowedHtmlTags { get; set; } = 
        {
            "b", "i", "em", "strong", "p", "br", "ul", "ol", "li", "a", "h1", "h2", "h3", "h4", "h5", "h6"
        };

        /// <summary>
        /// Allowed URL schemes
        /// </summary>
        public string[] AllowedUrlSchemes { get; set; } = { "http", "https", "ftp" };
    }
}