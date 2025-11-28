using DynamicForms.Core.Interfaces;
using DynamicForms.RazorPages.Extensions;
using DynamicForms.RazorPages.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicForms.RazorPages.Extensions;

/// <summary>
/// Service collection extensions for Razor Pages integration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add DynamicForms Razor Pages integration
    /// </summary>
    public static IServiceCollection AddDynamicFormsRazorPages(this IServiceCollection services)
    {
        // Register Razor Pages services if not already registered
        services.AddRazorPages(options =>
        {
            // Add any global conventions here
        });

        // Register tag helpers
        services.AddTransient<DynamicFieldTagHelper>();
        services.AddTransient<ValidationSummaryTagHelper>();
        services.AddTransient<FormProgressTagHelper>();
        services.AddTransient<ProgressStepperTagHelper>(); // Added new progress stepper
        services.AddTransient<DateRangePickerTagHelper>(); // Added new date range picker

        // Register view location expander if needed
        services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new DynamicFormsViewLocationExpander());
        });

        return services;
    }

    /// <summary>
    /// Add DynamicForms Razor Pages with runtime compilation (for development)
    /// </summary>
    public static IServiceCollection AddDynamicFormsRazorPagesWithRuntimeCompilation(this IServiceCollection services)
    {
        services.AddDynamicFormsRazorPages();
        
        // Add runtime compilation for development
        services.AddRazorPages()
                .AddRazorRuntimeCompilation();

        return services;
    }
}

/// <summary>
/// View location expander for DynamicForms views
/// </summary>
public class DynamicFormsViewLocationExpander : Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public IEnumerable<string> ExpandViewLocations(Microsoft.AspNetCore.Mvc.Razor.ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var additionalLocations = new[]
        {
            "/Views/DynamicForms/{1}/{0}.cshtml",
            "/Views/DynamicForms/Shared/{0}.cshtml",
            "/Areas/DynamicForms/Views/{1}/{0}.cshtml",
            "/Areas/DynamicForms/Views/Shared/{0}.cshtml"
        };

        return additionalLocations.Concat(viewLocations);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public void PopulateValues(Microsoft.AspNetCore.Mvc.Razor.ViewLocationExpanderContext context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        // No additional values needed
    }
}