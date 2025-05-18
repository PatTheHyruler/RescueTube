using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Jobs.Registration;

namespace RescueTube.YouTube.Jobs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterYouTubeRecurringJobs(this IServiceCollection services)
    {
        services.Configure<HangfireRecurringJobRegistry>(jobRegistry =>
        {
            jobRegistry.RegisterJob<UpdateYtDlpJob>(
                "yt-update-ytdlp-binary",
                x => x.UpdateYouTubeDlAsync(),
                Cron.Daily());
        });

        return services;
    }
}