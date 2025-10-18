using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
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
                .HasMember(x => x.Status).EqualTo(DataFetchStatus.Started)
                .HasMember(x => x.VideoId).EqualTo(videoId);
        }
    }
}