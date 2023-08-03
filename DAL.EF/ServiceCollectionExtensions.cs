using DAL.Contracts;
using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.EF;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddDbLoggingOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DbLoggingOptions>().Bind(configuration.GetSection(DbLoggingOptions.Section));
        return services;
    }

    public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbLoggingOptions(configuration);
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AbstractAppDbContext, PostgresAppDbContext>(o => o.UseNpgsql(connectionString));
        services.AddScoped<AbstractAppDbContext, PostgresAppDbContext>();
        return services;
    }
}