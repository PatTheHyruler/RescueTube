using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchVideoDataJob : EntityDataFetchJobBase<Video, FetchVideoDataJob>, IJob
{
    public static string RecurringJobId => "yt:fetch-video-data";

    private static readonly DataFetchDefinition DataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.VideoPage;

    public static JobDefinition JobDefinition { get; } = new JobDefinition<FetchVideoDataJob>
    {
        IsArchivalJob = true,
        DefaultSettings = new()
        {
            JobId = RecurringJobId,
            Cron = "*/10 * * * *", // Every 10th minute
            IsEnabled = true,
            DataFetchJobSettings = new()
            {
                SuccessCutoffOffset = TimeSpan.FromDays(10),
                FailureCutoffOffset = TimeSpan.FromDays(12),
            },
        },
        DataFetchDefinition = DataFetchDefinition,
    };

    private readonly IDataUow _dataUow;
    private readonly YouTubeServices _youTubeServices;

    public FetchVideoDataJob(IDataUow dataUow, YouTubeServices youTubeServices, ILogger<FetchVideoDataJob> logger, IBackgroundJobClientV2 backgroundJobClient, IRecurringJobsService recurringJobsService)
        : base(dataUow, logger, DataFetchDefinition, backgroundJobClient, recurringJobsService)
    {
        _dataUow = dataUow;
        _youTubeServices = youTubeServices;
    }

    protected override Expression<Func<Video, bool>> GetFilterExpression(DataFetchJobSettings dataFetchJobSettings)
        => DataUow.DataFetches.ShouldFetchVideoData(DataFetchDefinition, dataFetchJobSettings,
            v => v.ArchivalSettings.ShouldRegularlyFetchVideoData);

    public override async Task<EntityDataFetchResult> FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        await _youTubeServices.VideoService.UpdateVideoAsync(entityId, ct);
        await _dataUow.SaveChangesAsync(ct);
        return EntityDataFetchResult.Completed;
    }
}