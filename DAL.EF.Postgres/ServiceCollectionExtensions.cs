using BLL.Data;
using BLL.Data.Repositories;
using DAL.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.EF.Postgres;

public static class ServiceCollectionExtensions
{
    private static PostgresAppDbContext GetPostgresAppDbContext(IServiceProvider s) =>
        s.GetRequiredService<PostgresAppDbContext>();

    public static IServiceCollection AddDbPersistenceEfPostgres(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbLoggingOptions();
        var connectionString = configuration.GetConnectionString("RescueTubePostgres");
        services.AddDbContext<PostgresAppDbContext>(
            o => o.UseNpgsql(connectionString));
        services.AddScoped<AppDbContext>(GetPostgresAppDbContext);
        services.AddScoped<IAppDbContext>(GetPostgresAppDbContext);

        services.AddScoped<VideoRepository>();
        services.AddScoped<IVideoRepository>(s => s.GetRequiredService<VideoRepository>());

        services.AddScoped<DataUow>();
        services.AddScoped<IDataUow>(s => s.GetRequiredService<DataUow>());
        return services;
    }
}