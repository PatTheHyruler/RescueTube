using DAL.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Utils.Validation;

namespace DAL.EF;

public static class ServiceCollectionExtensions
{
    public static void AddDbLoggingOptions(this IServiceCollection services)
    {
        services.AddOptionsFull<DbLoggingOptions>(DbLoggingOptions.Section);
    }
}