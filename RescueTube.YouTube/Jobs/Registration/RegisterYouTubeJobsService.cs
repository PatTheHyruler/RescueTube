using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RescueTube.YouTube.Jobs.Registration;

public class RegisterYouTubeJobsService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RegisterYouTubeJobsService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateAsyncScope();

        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.RemoveIfExists(
            "yt-fetch-comments-recurring"); // Removing temporarily, TODO: comments job should probably be reworked
        // recurringJobManager.AddOrUpdate<FetchCommentsJob>(
        //     "yt-fetch-comments-recurring",
        //     x => x.QueueVideosForCommentFetch(default),
        //     Cron.Hourly);
        recurringJobManager.AddOrUpdate<EnqueueSubmissionsJob>(
            "yt-enqueue-submissions-recurring",
            x => x.RunAsync(),
            Cron.Hourly);
        recurringJobManager.AddOrUpdate<DownloadVideoJob>(
            "yt-download-non-downloaded-videos-recurring",
            x => x.DownloadNotDownloadedVideosAsync(default),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<UpdateYtDlpJob>(
            "yt-update-ytdlp-binary",
            x => x.UpdateYouTubeDlAsync(),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<FetchAuthorVideosJob>(
            "yt-fetch-author-videos-recurring",
            x => x.EnqueueAuthorVideoFetchesRecurring(default),
            "*/20 * * * *"); // Every 20th minute

        return Task.CompletedTask;
    }
}