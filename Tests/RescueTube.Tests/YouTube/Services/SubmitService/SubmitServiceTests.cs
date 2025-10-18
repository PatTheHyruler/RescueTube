using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RescueTube.Core;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Services;
using RescueTube.Domain;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Identity;
using RescueTube.Domain.Enums;
using RescueTube.Tests.TestUtils;
using RescueTube.YouTube;
using RescueTube.YouTube.Services;
using RescueTube.YouTube.Services.External;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace RescueTube.Tests.YouTube.Services.SubmitService;

public class SubmitServiceTests : BaseEfPostgresTest
{
    private readonly IYouTubeDlClient _youTubeDlClient = Substitute.For<IYouTubeDlClient>();

    public SubmitServiceTests()
    {
        ServiceCollection.AddScoped<RescueTube.YouTube.Services.SubmitService>();

        ServiceCollection.AddScoped<YouTubeServices>();
        ServiceCollection.AddScoped<VideoService>();
        ServiceCollection.AddScoped<DataFetchService>();
        ServiceCollection.AddScoped<EntityUpdateService>();
        ServiceCollection.AddScoped<StatusChangeService>();
        ServiceCollection.AddMediatR(cfg =>
        {
            cfg
                .RegisterServicesFromAssemblyContaining<ICoreAssemblyMarker>()
                .RegisterServicesFromAssemblyContaining<IYouTubeAssemblyMarker>();
        });

        ServiceCollection.AddSingleton(_youTubeDlClient);
    }

    [Test]
    public async Task SubmitAsync_Should_SuccessfullyAddVideo_WhenValidVideoReturnedByYtDlp(CancellationToken ct)
    {
        // Arrange
        await using var provider = BuildServiceProvider();

        const string videoIdOnPlatform = "abcdefg1234";
        const string url = $"https://www.youtube.com/watch?v={videoIdOnPlatform}";

        Guid submissionId;

        await using (var setupScope = provider.CreateAsyncScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = new User();
            dbContext.Users.Add(user);

            var structuredUrl = new RecognizedPlatformUrl(
                url,
                videoIdOnPlatform,
                EPlatform.YouTube,
                EEntityType.Video);
            var submission = new Submission(
                structuredUrl, submitterId: user.Id, autoSubmit: true);
            dbContext.Submissions.Add(submission);

            await dbContext.SaveChangesAsync(ct);

            submissionId = submission.Id;
        }

        _youTubeDlClient.RunVideoDataFetchAsync(url, ct).Returns(new RunResult<VideoData?>(
            success: true,
            error: null,
            result: new VideoData
            {
                ID = videoIdOnPlatform,
                Title = "Test video title",
                Description = "Test video description",

                Duration = 123, // Seconds

                ViewCount = 4,
                LikeCount = 1,
                DislikeCount = null,
                CommentCount = 0,

                Subtitles = null,
                Thumbnails = null,
                Tags = null,
                LiveStatus = LiveStatus.None,

                UploadDate = new DateTime(2025, 01, 05),
                ModifiedTimestamp = new DateTime(2025, 01, 08),
                ReleaseTimestamp = new DateTime(2025, 01, 06),

                Availability = Availability.Public,
            }));

        await using (var scope = provider.CreateAsyncScope())
        {
            var sut = scope.ServiceProvider.GetRequiredService<RescueTube.YouTube.Services.SubmitService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var submission = await dbContext.Submissions.FirstAsync(x => x.Id == submissionId, ct);

            // Act
            await sut.HandleSubmissionAsync(submission, ct);
            await dbContext.SaveChangesAsync(ct);
        }

        // Assert
        await using (var scope = provider.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var video = await dbContext.Videos
                .Include(x => x.Title!.Translations!)
                .SingleAsync(ct);
            await Assert.That(video)
                .Member(x => x.IdOnPlatform, x => x.EqualTo(videoIdOnPlatform))
                .And.Member(x => x.Title!.Translations!.Single().Content, x => x.EqualTo("Test video title"));

            var dataFetch = await dbContext.DataFetches
                .Include(x => x.DataFetchResults)
                .SingleAsync(ct);
            await Assert.That(dataFetch)
                .Member(x => x.Platform, x => x.EqualTo(EPlatform.YouTube))
                .And.Member(x => x.VideoId, x => x.EqualTo(video.Id))
                .And.Member(x => x.Status, x => x.EqualTo(DataFetchStatus.Succeeded));
            await Assert.That(dataFetch.DataFetchResults.Single())
                .Member(x => x.VideoId, x => x.EqualTo(video.Id));
        }
    }
}