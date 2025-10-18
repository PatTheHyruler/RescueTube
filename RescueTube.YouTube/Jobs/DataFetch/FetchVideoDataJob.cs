using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchVideoDataJob : EntityDataFetchJobBase<Video>, IEntityDataFetchJobWithDefinition
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeServices _youTubeServices;

    public FetchVideoDataJob(IDataUow dataUow, YouTubeServices youTubeServices, ILogger<FetchVideoDataJob> logger)
        : base(dataUow, logger, JobDefinition.DataFetchDefinition)
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

    public override async Task FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        await _youTubeServices.VideoService.UpdateVideoAsync(entityId, ct);
        await _dataUow.SaveChangesAsync(ct);
    }
}