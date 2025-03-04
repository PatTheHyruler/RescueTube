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
        recurringJobManager.AddOrUpdate<EnqueueSubmissionsJob>(
            "yt-enqueue-submissions-recurring",
            x => x.RunAsync(default),
            Cron.Hourly);
        recurringJobManager.AddOrUpdate<UpdateYtDlpJob>(
            "yt-update-ytdlp-binary",
            x => x.UpdateYouTubeDlAsync(),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<FetchAuthorVideosJob>(
            "yt-fetch-author-videos-recurring",
            x => x.EnqueueAuthorVideoFetchesRecurring(default),
            "*/20 * * * *"); // Every 20th minute
        recurringJobManager.AddOrUpdate<FetchYouTubeExplodeAuthorDataJob>(
            "yt-fetch-ytexplode-author-data-recurring",
            x => x.EnqueueYouTubeExplodeAuthorDataFetchesRecurring(default),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<FetchVideoDataJob>(
            "yt-fetch-videos-data-recurring",
            x => x.EnqueueVideoDataFetchesRecurring(default),
            Cron.Minutely);
        recurringJobManager.AddOrUpdate<FetchPlaylistDataJob>(
            "yt-fetch-playlists-data-recurring",
            x => x.EnqueuePlaylistDataFetches(default),
            "*/10 * * * *"); // Every 10th minute

        return Task.CompletedTask;
    }
}