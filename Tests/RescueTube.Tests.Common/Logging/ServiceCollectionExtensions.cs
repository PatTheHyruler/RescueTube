using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace RescueTube.Tests.Common.Logging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXunitLogging(this IServiceCollection services, ITestOutputHelper output)
    {
        services.AddLogging(builder =>
        {
            builder.Services.AddSingleton(output);
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, XUnitLoggerProvider>());
        });
        return services;
    }
}