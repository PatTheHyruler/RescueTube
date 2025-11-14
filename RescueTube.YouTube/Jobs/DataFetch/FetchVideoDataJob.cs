using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchVideoDataJob : EntityDataFetchJobBase<Video, FetchVideoDataJob>, IEntityDataFetchJobWithDefinition, IJob
{
    public static string RecurringJobId => "yt:fetch-video-data";

    private readonly IDataUow _dataUow;
    private readonly YouTubeServices _youTubeServices;

    public FetchVideoDataJob(IDataUow dataUow, YouTubeServices youTubeServices, ILogger<FetchVideoDataJob> logger, IBackgroundJobClientV2 backgroundJobClient)
        : base(dataUow, logger, JobDefinition.DataFetchDefinition, backgroundJobClient)
    {
        _dataUow = dataUow;
        _youTubeServices = youTubeServices;
    }

    public static DataFetchJobDefinition JobDefinition { get; } = new()
    {
        DataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.VideoPage,
        SuccessCutoffOffset = TimeSpan.FromDays(10),
        FailureCutoffOffset = TimeSpan.FromDays(12),
    };

    protected override Expression<Func<Video, bool>> FilterExpression =>
        DataUow.DataFetches.ShouldFetchVideoData(JobDefinition, v =>
            v.ArchivalSettings.ShouldRegularlyFetchVideoData);

    public override async Task<EntityDataFetchResult> FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        await _youTubeServices.VideoService.UpdateVideoAsync(entityId, ct);
        await _dataUow.SaveChangesAsync(ct);
        return EntityDataFetchResult.Completed;
    }
}