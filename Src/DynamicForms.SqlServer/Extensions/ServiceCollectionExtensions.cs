using DynamicForms.Core.Interfaces;
using DynamicForms.SqlServer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DynamicForms.SqlServer.Extensions;

/// <summary>
/// Service collection extensions for SQL Server optimized implementations
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add SQL Server optimized repositories for DynamicForms
    /// Replaces Entity Framework repositories with high-performance Dapper implementations
    /// </summary>
    public static IServiceCollection AddDynamicFormsSqlServer(
        this IServiceCollection services,
        string connectionString)
    {
        // Replace repositories with SQL Server optimized versions
        services.AddScoped<IFormModuleRepository>(provider => 
            new SqlServerFormModuleRepository(connectionString, 
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqlServerFormModuleRepository>>()));
        
        services.AddScoped<IFormDataRepository>(provider => 
            new SqlServerFormDataRepository(connectionString,
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqlServerFormDataRepository>>()));
        
        return services;
    }

    /// <summary>
    /// Add SQL Server optimized repositories with connection string from configuration
    /// </summary>
    public static IServiceCollection AddDynamicFormsSqlServer(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");
            
        return services.AddDynamicFormsSqlServer(connectionString);
    }

    /// <summary>
    /// Add SQL Server optimized repositories (uses default DI to resolve connection string)
    /// Call this after AddDynamicFormsEntityFramework to override repositories
    /// </summary>
    public static IServiceCollection AddDynamicFormsSqlServer(this IServiceCollection services)
    {
        services.AddScoped<IFormModuleRepository, SqlServerFormModuleRepository>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("DefaultConnection connection string not found in configuration.");
                
            return new SqlServerFormModuleRepository(connectionString,
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqlServerFormModuleRepository>>());
        });
        
        services.AddScoped<IFormDataRepository, SqlServerFormDataRepository>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("DefaultConnection connection string not found in configuration.");
                
            return new SqlServerFormDataRepository(connectionString,
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqlServerFormDataRepository>>());
        });
        
        return services;
    }

    /// <summary>
    /// Add SQL Server performance monitoring and health checks
    /// Requires Microsoft.Extensions.Diagnostics.HealthChecks and AspNetCore.HealthChecks.SqlServer packages
    /// </summary>
    public static IServiceCollection AddDynamicFormsSqlServerMonitoring(
        this IServiceCollection services,
        string connectionString,
        string healthCheckName = "dynamicforms-sqlserver")
    {
        // Add health checks services first
        services.AddHealthChecks()
            .AddSqlServer(
                connectionString: connectionString,
                name: healthCheckName,
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "dynamicforms", "database", "sqlserver" });
            
        return services;
    }

    /// <summary>
    /// Add SQL Server health checks with configuration
    /// </summary>
    public static IServiceCollection AddDynamicFormsSqlServerMonitoring(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection",
        string healthCheckName = "dynamicforms-sqlserver")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");
            
        return services.AddDynamicFormsSqlServerMonitoring(connectionString, healthCheckName);
    }

    /// <summary>
    /// Add comprehensive SQL Server integration with repositories and health checks
    /// </summary>
    public static IServiceCollection AddDynamicFormsSqlServerWithMonitoring(
        this IServiceCollection services,
        string connectionString,
        string healthCheckName = "dynamicforms-sqlserver")
    {
        services.AddDynamicFormsSqlServer(connectionString);
        services.AddDynamicFormsSqlServerMonitoring(connectionString, healthCheckName);
        
        return services;
    }

    /// <summary>
    /// Add comprehensive SQL Server integration with configuration
    /// </summary>
    public static IServiceCollection AddDynamicFormsSqlServerWithMonitoring(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection",
        string healthCheckName = "dynamicforms-sqlserver")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");
            
        return services.AddDynamicFormsSqlServerWithMonitoring(connectionString, healthCheckName);
    }
}