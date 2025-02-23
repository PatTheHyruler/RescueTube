using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Jobs;

namespace RescueTube.Jobs.DAL.Postgres;

public static class ServiceCollectionExtensions
{
    public static void AddHangfirePostgresStorageAccessor(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IJobStorageAccessor, PostgresJobStorageAccessor>(_ => new PostgresJobStorageAccessor(connectionString));
    }
}