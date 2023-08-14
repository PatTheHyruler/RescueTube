using DAL.Contracts;
using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace DAL.EF;

public static class ServiceCollectionExtensions
{
    private static void AddDbLoggingOptions(this IServiceCollection services)
    {
        services.AddOptionsFull<DbLoggingOptions>(DbLoggingOptions.Section);
    }

    public static IServiceCollection AddDbPersistenceEf(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbLoggingOptions();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AbstractAppDbContext, PostgresAppDbContext>(
            o => o.UseNpgsql(connectionString));
        services.AddScoped<AbstractAppDbContext, PostgresAppDbContext>();
        return services;
    }
}