using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RescueTube.Core.JobOrchestration.Jobs;

namespace RescueTube.Core.Jobs.Registration;

public class RegisterBllJobsService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RegisterBllJobsService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        // TODO: Also remove old recurring jobs
        recurringJobManager.AddOrUpdate<EnqueueSubmissionsJob>(
            "enqueue-submissions-recurring",
            x => x.RunAsync(default),
            Cron.Hourly);
        recurringJobManager.AddOrUpdate<DownloadVideoImagesJob>(
            "download-all-not-downloaded-video-images", 
            x => x.DownloadAllNotDownloadedVideoImages(default),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<DownloadAuthorImagesJob>(
            "download-all-not-downloaded-author-images",
            x => x.DownloadAllNotDownloadedAuthorImages(default),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<DownloadVideoJob>(
            "download-non-downloaded-videos-recurring",
            x => x.DownloadNextNotDownloadedVideoAsync(CancellationToken.None),
            "*/15 * * * * *"); // Every 15th second
        recurringJobManager.AddOrUpdate<DeleteExpiredRefreshTokensJob>("delete-expired-refresh-tokens",
            x => x.DeleteExpiredRefreshTokens(),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<UpdateImagesResolutionJob>("update-images-resolution-from-file",
            x => x.EnqueueAsync(default),
            Cron.Daily);

        foreach (var workerIndex in Enumerable.Range(0, 10))
        {
            recurringJobManager.AddOrUpdate<ProcessNextPullingJob>(
                $"core:process-next-pulling-job:{workerIndex}",
                x => x.RunAsync(workerIndex, CancellationToken.None),
                "*/15 * * * * *"); // Every 15th second
        }
        recurringJobManager.AddOrUpdate<ClearOldJobExecutionStatsJob>(
            "clear-old-job-execution-stats-recurring",
            x => x.Run(CancellationToken.None),
            "*/15 * * * *"); // Every 15th minute
    }
}