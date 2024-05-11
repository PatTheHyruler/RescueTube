using BLL.Contracts;
using BLL.Utils.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.EF;

public static class ServiceCollectionExtensions
{
    public static void AddDbLoggingOptions(this IServiceCollection services)
    {
        services.AddOptionsFull<DbLoggingOptions>(DbLoggingOptions.Section);
    }
}