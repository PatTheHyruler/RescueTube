using DAL.Contracts;
using DAL.EF.DbContexts;
using DAL.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.EF;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddDbLoggingOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<DbLoggingOptions>().Bind(configuration.GetSection(DbLoggingOptions.Section));
        return services;
    }

    public static IServiceCollection AddDbPersistenceEf(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbLoggingOptions(configuration)
            .AddEfRepositories();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AbstractAppDbContext, PostgresAppDbContext>(
            o => o.UseNpgsql(connectionString));
        services.AddScoped<AbstractAppDbContext, PostgresAppDbContext>();
        services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();
        return services;
    }
}