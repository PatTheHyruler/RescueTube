using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.JobOrchestration.Jobs;

namespace RescueTube.Core.Jobs.Registration;

public static class RegisterBllJobsServiceCollectionExtensions
{
    public static IServiceCollection RegisterBllRecurringJobs(this IServiceCollection services)
    {
        services.Configure<HangfireRecurringJobRegistry>(jobRegistry =>
        {
            jobRegistry.RegisterJob<EnqueueSubmissionsJob>(
                "enqueue-submissions-recurring",
                x => x.RunAsync(default),
                Cron.Hourly());
            jobRegistry.RegisterJob<DownloadVideoJob>(
                "download-non-downloaded-videos-recurring",
                x => x.DownloadNextNotDownloadedVideoAsync(CancellationToken.None),
                "*/15 * * * * *"); // Every 15th second

            jobRegistry.RegisterJob<DeleteExpiredRefreshTokensJob>(
                "delete-expired-refresh-tokens",
                x => x.DeleteExpiredRefreshTokens(),
                Cron.Daily(),
                isArchivalJob: false);
            jobRegistry.RegisterJob<UpdateImagesResolutionJob>(
                "update-images-resolution-from-file",
                x => x.EnqueueAsync(default),
                Cron.Daily(),
                isArchivalJob: false);

            foreach (var workerIndex in Enumerable.Range(0, 10))
            {
                jobRegistry.RegisterJob<ProcessNextPullingJob>(
                    $"core:process-next-pulling-job:{workerIndex}",
                    x => x.RunAsync(workerIndex, CancellationToken.None),
                    "*/15 * * * * *"); // Every 15th second
            }
            jobRegistry.RegisterJob<ClearOldJobExecutionStatsJob>(
                "clear-old-job-execution-stats-recurring",
                x => x.Run(CancellationToken.None),
                "*/15 * * * *",
                isArchivalJob: false); // Every 15th minute
        });

        return services;
    }
}