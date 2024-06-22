using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace RescueTube.DAL.EF.MigrationUtils;

public static class MigrationUtils
{
    private const string MigrationsNamespace = "Migrations";

    public static async Task MigrateDbAsync<TDbContext>(this IServiceProvider serviceProvider)
        where TDbContext : DbContext
    {
        await using var dbContext = serviceProvider.GetRequiredService<TDbContext>();

        var assembly = dbContext.GetType().Assembly;

        var migrations = await dbContext.Database.GetPendingMigrationsAsync();

        var migrator = dbContext.Database.GetService<IMigrator>();

        foreach (var migration in migrations)
        {
            var migrationTypeName = migration[(migration.IndexOf('_') + 1)..];
            var fullTypeName = $"{assembly.GetName().Name}.{MigrationsNamespace}.{migrationTypeName}";
            var migrationType = assembly.GetType(fullTypeName);
            var attribute = migrationType?.GetCustomAttribute(typeof(DataMigrationAttribute));
            Console.WriteLine(
                $"Migration: {migration}, FullMigrationTypeName: {fullTypeName}, MigrationType: {migrationType}, Attribute: {attribute}, AttributeType: {attribute?.GetType()}");
            if (attribute is DataMigrationAttribute dataMigrationAttribute)
            {
                var dataMigrationType = dataMigrationAttribute.DataMigrationType;
                var dataMigrationObject = serviceProvider.GetRequiredService(dataMigrationType);
                if (dataMigrationObject is not IDataMigration dataMigration)
                {
                    throw new Exception();
                }

                var dbConnection = dbContext.Database.GetDbConnection();
                await dbConnection.OpenAsync();
                try
                {
                    await using var transaction = await dbConnection.BeginTransactionAsync();
                    await dataMigration.MigrateAsync(dbConnection);
                    await transaction.CommitAsync();
                }
                finally
                {
                    await dbConnection.CloseAsync();
                }
            }
            await migrator.MigrateAsync(migration);
        }
    }

    public static void AddDataMigrations<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        var assembly = typeof(TDbContext).Assembly;

        IEnumerable<Type> types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(t => t != null).Select(t => t!);
        }

        var interfaceType = typeof(IDataMigration);
        types = types
            .Where(interfaceType.IsAssignableFrom)
            .Where(t => !(t == interfaceType));

        foreach (var type in types)
        {
            services.AddScoped(type);
        }
    }
}