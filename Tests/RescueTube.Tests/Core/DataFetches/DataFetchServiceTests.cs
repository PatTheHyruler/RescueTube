using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.Tests.TestUtils;

namespace RescueTube.Tests.Core.DataFetches;

public class DataFetchServiceTests : BaseEfPostgresTest
{
    public DataFetchServiceTests()
    {
        ServiceCollection.AddScoped<DataFetchService>();
    }

    [Test]
    public async Task StartDataFetchAsync_Should_SuccessfullyStartDataFetch(CancellationToken ct)
    {
        // Arrange
        await using var provider = BuildServiceProvider();

        var dataFetchDefinition = new DataFetchDefinition
        {
            Type = "video",
            Source = "custom-test-source",
            EntityType = EEntityType.Video,
            Platform = EPlatform.Other,
        };

        var videoId = Guid.CreateVersion7();

        await using (var setupScope = provider.CreateAsyncScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var video = new Video
            {
                Id = videoId,
                IdOnPlatform = "example_123",
            };
            dbContext.Videos.Add(video);

            await dbContext.SaveChangesAsync(ct);
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var sut = scope.ServiceProvider.GetRequiredService<DataFetchService>();

            // Act
            await using var dataFetchScope = await sut.StartDataFetchAsync(dataFetchDefinition, videoId, ct);

            // Assert
            await Assert.That(dataFetchScope).IsNotNull();
            await Assert.That(dataFetchScope!.DataFetch)
                .Member(x => x.Status, x => x.EqualTo(DataFetchStatus.Started))
                .And.Member(x => x.VideoId, x => x.EqualTo(videoId));
        }
    }

    [Test]
    public async Task UpdateDataFetchStatusAsync_Should_UpdateDataFetchStatusAndMessage(CancellationToken ct)
    {
        // Arrange
        await using var provider = BuildServiceProvider();

        var dataFetchDefinition = new DataFetchDefinition
        {
            Type = "video",
            Source = "custom-test-source",
            EntityType = EEntityType.Video,
            Platform = EPlatform.Other,
        };

        var videoId = Guid.CreateVersion7();

        await using (var setupScope = provider.CreateAsyncScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var video = new Video
            {
                Id = videoId,
                IdOnPlatform = "example_123",
            };
            dbContext.Videos.Add(video);

            await dbContext.SaveChangesAsync(ct);
        }

        const string customErrorMessage = "Test error message";

        // Act
        await using (var scope = provider.CreateAsyncScope())
        {
            var sut = scope.ServiceProvider.GetRequiredService<DataFetchService>();

            await using var dataFetchScope = await sut.StartDataFetchAsync(dataFetchDefinition, videoId, ct);

            dataFetchScope.AssertNotNull();
            await sut.UpdateDataFetchStatusAsync(dataFetchScope.DataFetch, DataFetchStatus.Failed, customErrorMessage);
        }

        // Assert
        await using (var scope = provider.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var dataFetch = await dbContext.DataFetches.SingleAsync(ct);

            await Assert.That(dataFetch)
                .Member(x => x.Status, x => x.EqualTo(DataFetchStatus.Failed))
                .And.Member(x => x.Message, x => x.EqualTo(customErrorMessage));
        }
    }
}