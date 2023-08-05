using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Validation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOptionsFull<TOptions>(this IServiceCollection services,
        IConfiguration config) where TOptions : class
    {
        return services
            .AddOptionsRecursive<TOptions>(config)
            .ValidateOnStart()
            .Services;
    }

    public static IServiceCollection AddOptionsFull<TOptions>(this IServiceCollection services,
        string configSectionPath) where TOptions : class
    {
        return services
            .AddOptionsRecursive<TOptions>(configSectionPath)
            .ValidateOnStart()
            .Services;
    }

    public static OptionsBuilder<TOptions> AddOptionsRecursive<TOptions>(this IServiceCollection services,
        IConfiguration config) where TOptions : class
    {
        return services
            .AddOptionsRecursive<TOptions>()
            .Bind(config);
    }

    public static OptionsBuilder<TOptions> AddOptionsRecursive<TOptions>(this IServiceCollection services,
        string configSectionPath) where TOptions : class
    {
        return services
            .AddOptionsRecursive<TOptions>()
            .BindConfiguration(configSectionPath);
    }

    private static OptionsBuilder<TOptions> AddOptionsRecursive<TOptions>(this IServiceCollection services,
        Action<OptionsBuilder<TOptions>>? configureOptions = null) where TOptions : class
    {
        var result = services.AddOptions<TOptions>();
        configureOptions?.Invoke(result);
        return result
            .ValidateDataAnnotationsRecursively();
    }
}