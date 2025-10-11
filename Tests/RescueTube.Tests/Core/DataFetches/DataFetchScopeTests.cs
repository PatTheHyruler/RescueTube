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
        const string idOnPlatform = "abcdef12345";

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dataFetchService = scope.ServiceProvider.GetRequiredService<DataFetchService>();

            await using var dataFetchScope = await dataFetchService.StartDataFetchAsync(definition, idOnPlatform, ct);
            throw new Exception("An unhandled error occurred during the data fetch");
        });

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dataFetch = await dbContext.DataFetches.SingleAsync(ct);
            await Assert.That(dataFetch)
                .HasMember(x => x.Status).EqualTo(DataFetchStatus.Failed)
                .HasMember(x => x.Platform).EqualTo(definition.Platform)
                .HasMember(x => x.VideoIdOnPlatform).EqualTo(idOnPlatform)
                .HasMember(x => x.Source).EqualTo(definition.Source)
                .HasMember(x => x.Type).EqualTo(definition.Type);
        }
    }
}