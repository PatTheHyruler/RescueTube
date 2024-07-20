using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Specifications;
using RescueTube.DAL.EF.MigrationUtils;
using RescueTube.DAL.EF.Specifications;

namespace RescueTube.DAL.EF.Postgres;

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
            o => o
                .UseNpgsql(connectionString)
                .WithExpressionExpanding()
        );
        services.AddScoped<AppDbContext>(GetPostgresAppDbContext);
        services.AddScoped<IAppDbContext>(GetPostgresAppDbContext);

        services.AddScoped<VideoSpecification>();
        services.AddScoped<IVideoSpecification>(s => s.GetRequiredService<VideoSpecification>());
        services.AddScoped<PlaylistSpecification>();
        services.AddScoped<IPlaylistSpecification>(s => s.GetRequiredService<PlaylistSpecification>());
        services.AddScoped<PermissionSpecification>();
        services.AddScoped<IPermissionSpecification>(s => s.GetRequiredService<PermissionSpecification>());
        services.AddScoped<ImageSpecification>();
        services.AddScoped<IImageSpecification>(s => s.GetRequiredService<ImageSpecification>());

        services.AddScoped<DataUow>();
        services.AddScoped<IDataUow>(s => s.GetRequiredService<DataUow>());

        services.AddDataMigrations<PostgresAppDbContext>();

        return services;
    }
}