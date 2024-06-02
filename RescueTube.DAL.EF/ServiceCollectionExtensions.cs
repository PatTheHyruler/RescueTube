using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Contracts;
using RescueTube.Core.Utils.Validation;

namespace RescueTube.DAL.EF;

public static class ServiceCollectionExtensions
{
    public static void AddDbLoggingOptions(this IServiceCollection services)
    {
        services.AddOptionsFull<DbLoggingOptions>(DbLoggingOptions.Section);
    }
}