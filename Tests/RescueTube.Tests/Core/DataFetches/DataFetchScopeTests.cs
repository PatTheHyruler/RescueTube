using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Domain.Entities;
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
        // Arrange
        await using var serviceProvider = BuildServiceProvider();

        var definition = new DataFetchDefinition
        {
            EntityType = EEntityType.Video,
            Platform = EPlatform.YouTube,
            Source = "testsource",
            Type = "testtype",
        };

        var videoId = Guid.CreateVersion7();

        await using (var setupScope = serviceProvider.CreateAsyncScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var video = new Video
            {
                Id = videoId,
                IdOnPlatform = "test_123",
            };
            dbContext.Videos.Add(video);

            await dbContext.SaveChangesAsync(ct);
        }

        // Act
        await Assert.ThrowsAsync<CustomException>(async () =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dataFetchService = scope.ServiceProvider.GetRequiredService<DataFetchService>();

            await using var dataFetchScope = await dataFetchService.StartDataFetchAsync(definition, entityId: videoId, ct);
            throw new CustomException("An unhandled error occurred during the data fetch");
        });

        // Assert
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

    private sealed class CustomException(string message) : Exception(message);
}