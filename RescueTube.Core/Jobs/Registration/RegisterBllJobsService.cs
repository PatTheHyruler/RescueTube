using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RescueTube.Core.Jobs.Registration;

public class RegisterBllJobsService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RegisterBllJobsService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateAsyncScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<DownloadVideoImagesJob>(
            "download-all-not-downloaded-video-images", 
            x => x.DownloadAllNotDownloadedVideoImages(default),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<DownloadAuthorImagesJob>(
            "download-all-not-downloaded-author-images",
            x => x.DownloadAllNotDownloadedAuthorImages(default),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<DeleteExpiredRefreshTokensJob>("delete-expired-refresh-tokens",
            x => x.DeleteExpiredRefreshTokens(),
            Cron.Daily);
        return Task.CompletedTask;
    }
}