using DAL.Contracts;
using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.Validation;

namespace DAL.EF.Postgres;

public static class ServiceCollectionExtensions
{
    private static void AddDbLoggingOptions(this IServiceCollection services)
    {
        services.AddOptionsFull<DbLoggingOptions>(DbLoggingOptions.Section);
    }
    
    public static IServiceCollection AddDbPersistenceEfPostgres(this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDbLoggingOptions();
        var connectionString = configuration.GetConnectionString("RescueTubePostgres");
        services.AddDbContext<AppDbContext, PostgresAppDbContext>(
            o => o.UseNpgsql(connectionString));
        services.AddScoped<AppDbContext, PostgresAppDbContext>();
        return services;
    }
}