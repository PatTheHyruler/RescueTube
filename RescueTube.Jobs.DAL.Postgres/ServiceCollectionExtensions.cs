using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RescueTube.Core.Jobs;

namespace RescueTube.Jobs.DAL.Postgres;

public static class ServiceCollectionExtensions
{
    public static void AddHangfirePostgresStorageAccessor(this IServiceCollection services, string connectionString)
    {
        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        services.AddKeyedSingleton(serviceKey: typeof(PostgresJobStorageAccessor), dataSource);
        services.AddScoped<IJobStorageAccessor, PostgresJobStorageAccessor>();
    }
}