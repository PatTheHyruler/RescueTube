using Hangfire;
using Hangfire.Server;
using RescueTube.Core.JobOrchestration;
using RescueTube.YouTube.Services.External;

namespace RescueTube.YouTube.Jobs;

public class UpdateYtDlpJob : IJob
{
    public static string RecurringJobId => "yt:update-yt-dlp-binary";

    public static JobDefinition JobDefinition { get; } = new JobDefinition<UpdateYtDlpJob>
    {
        IsArchivalJob = true,
        DefaultSettings = new()
        {
            JobId = RecurringJobId,
            Cron = Cron.Daily(),
            IsEnabled = true,
        },
    };

    private readonly IYouTubeDlClient _youtubeDl;

    public UpdateYtDlpJob(IYouTubeDlClient youtubeDl)
    {
        _youtubeDl = youtubeDl;
    }

    [DisableConcurrentExecution("yt:update-yt-dlp-binary", timeoutSec: 5)]
    public Task RunAsync(PerformContext performContext, CancellationToken ct)
    {
        return _youtubeDl.RunUpdateAsync();
    }
}