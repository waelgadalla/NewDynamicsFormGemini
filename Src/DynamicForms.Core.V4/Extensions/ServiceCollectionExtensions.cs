using DynamicForms.Core.V4.Services;
using DynamicForms.Core.V4.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicForms.Core.V4.Extensions;

/// <summary>
/// Extension methods for registering DynamicForms.Core.V4 services with dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all DynamicForms.Core.V4 services to the service collection.
    /// Registers hierarchy service, validation service, condition evaluator, and built-in validation rules.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDynamicFormsV4(this IServiceCollection services)
    {
        // Register core services as singletons (they are stateless and thread-safe)
        services.AddSingleton<IFormHierarchyService, FormHierarchyService>();
        services.AddSingleton<IFormValidationService, FormValidationService>();

        // Register conditional rules engine
        services.AddSingleton<IConditionEvaluator, ConditionEvaluator>();

        // Register CodeSet provider
        services.AddSingleton<ICodeSetProvider, InMemoryCodeSetProvider>();

        // Register built-in validation rules as singletons
        services.AddSingleton<IValidationRule, RequiredFieldRule>();
        services.AddSingleton<IValidationRule, LengthValidationRule>();
        services.AddSingleton<IValidationRule, PatternValidationRule>();
        services.AddSingleton<IValidationRule, EmailValidationRule>();

        return services;
    }

    /// <summary>
    /// Adds DynamicForms.Core.V4 services with the option to register a custom repository implementation.
    /// Use this overload when you want to provide your own IFormModuleRepository implementation.
    /// </summary>
    /// <typeparam name="TRepository">The repository implementation type</typeparam>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDynamicFormsV4<TRepository>(this IServiceCollection services)
        where TRepository : class, IFormModuleRepository
    {
        // Add core services
        services.AddDynamicFormsV4();

        // Register custom repository
        services.AddScoped<IFormModuleRepository, TRepository>();

        return services;
    }

    /// <summary>
    /// Adds DynamicForms.Core.V4 services with a custom CodeSet provider.
    /// Use this when you want to provide your own ICodeSetProvider implementation.
    /// </summary>
    /// <typeparam name="TCodeSetProvider">The CodeSet provider implementation type</typeparam>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDynamicFormsV4WithCodeSetProvider<TCodeSetProvider>(this IServiceCollection services)
        where TCodeSetProvider : class, ICodeSetProvider
    {
        // Register core services
        services.AddSingleton<IFormHierarchyService, FormHierarchyService>();
        services.AddSingleton<IFormValidationService, FormValidationService>();
        services.AddSingleton<IConditionEvaluator, ConditionEvaluator>();

        // Register custom CodeSet provider
        services.AddSingleton<ICodeSetProvider, TCodeSetProvider>();

        // Register built-in validation rules
        services.AddSingleton<IValidationRule, RequiredFieldRule>();
        services.AddSingleton<IValidationRule, LengthValidationRule>();
        services.AddSingleton<IValidationRule, PatternValidationRule>();
        services.AddSingleton<IValidationRule, EmailValidationRule>();

        return services;
    }
}
