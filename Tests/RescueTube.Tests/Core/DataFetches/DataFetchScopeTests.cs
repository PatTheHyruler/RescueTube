using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Domain.Enums;
using RescueTube.Tests.TestUtils;

namespace RescueTube.Tests.Core.DataFetches;

public class DataFetchScopeTests : BaseEfPostgresTest
{
    public DataFetchScopeTests()
    {
        ServiceCollection.AddScoped<DataFetchService>();
    }

    [Test]
    public async Task Should_UpdateDataFetchStatusToFailed_When_UnhandledErrorOccursWithinScope(CancellationToken ct)
    {
        await using var serviceProvider = BuildServiceProvider();

        var definition = new DataFetchDefinition
        {
            EntityType = EEntityType.Video,
            Platform = EPlatform.YouTube,
            Source = "testsource",
            Type = "testtype",
        };

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dataFetchService = scope.ServiceProvider.GetRequiredService<DataFetchService>();

            await using var dataFetchScope = await dataFetchService.StartDataFetchAsync(definition, entityId: null, ct);
            throw new Exception("An unhandled error occurred during the data fetch");
        });

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dataFetch = await dbContext.DataFetches.SingleAsync(ct);
            await Assert.That(dataFetch)
                .Member(x => x.Status, x => x.EqualTo(DataFetchStatus.Failed))
                .And.Member(x => x.Platform, x => x.EqualTo(definition.Platform))
                .And.Member(x => x.Source, x => x.EqualTo(definition.Source))
                .And.Member(x => x.Type, x => x.EqualTo(definition.Type));
        }
    }
}