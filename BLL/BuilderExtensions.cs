using BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class BuilderExtensions
{
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        services.AddScoped<ServiceUow>();

        services.AddScoped<SubmissionService>();
        return services;
    }
}