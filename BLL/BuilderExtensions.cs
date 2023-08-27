using BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace BLL;

public static class BuilderExtensions
{
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        services.AddOptionsFull<AppPathOptions>(AppPathOptions.Section);

        services.AddScoped<ServiceUow>();

        services.AddScoped<SubmissionService>();
        services.AddScoped<AuthorizationService>();
        return services;
    }
}