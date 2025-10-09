using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using RescueTube.Core;
using RescueTube.Core.Data;
using RescueTube.DAL.EF.Postgres;

namespace RescueTube.Tests.TestUtils;

public class BaseEfPostgresTest
{
    protected ServiceCollection ServiceCollection { get; } = new();

    protected FakeTimeProvider TimeProvider { get; } = new(new DateTimeOffset(2025, 10, 06, 12, 34, 56, TimeSpan.Zero));

    [ClassDataSource<PostgresFactory>(Shared = SharedType.PerTestSession)]
    public required PostgresFactory PostgresFactory { get; set; }    

    [Before(Test)]
    public async Task RegisterServicesAsync()
    {
        var connectionString = await PostgresFactory.CreateDatabaseAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new List<KeyValuePair<string, string?>>
            {
                new("ConnectionStrings:RescueTubePostgres", connectionString)
            })
            .Build();
        ServiceCollection.AddSingleton<TimeProvider>(TimeProvider);
        ServiceCollection.AddSingleton<IConfiguration>(config);
        ServiceCollection.AddLogging(b => b.AddConsole());
        ServiceCollection.AddDbPersistenceEfPostgres(config);
        ServiceCollection.AddBll();

        await using var sp = ServiceCollection.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    protected ServiceProvider BuildServiceProvider()
    {
        return ServiceCollection.BuildServiceProvider();
    }
}