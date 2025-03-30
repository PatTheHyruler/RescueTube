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
        recurringJobManager.AddOrUpdate<UpdateYtDlpJob>(
            "yt-update-ytdlp-binary",
            x => x.UpdateYouTubeDlAsync(),
            Cron.Daily);

        return Task.CompletedTask;
    }
}