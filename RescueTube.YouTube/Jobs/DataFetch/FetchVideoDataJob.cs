using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchVideoDataJob : EntityDataFetchJobBase<Video>
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;

    public FetchVideoDataJob(IDataUow dataUow, YouTubeUow youTubeUow, ILogger<FetchVideoDataJob> logger)
        : base(dataUow, logger, JobDefinition.DataFetchDefinition)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
    }

    private static readonly DataFetchJobDefinition JobDefinition = new()
    {
        DataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.VideoPage,
        SuccessCutoffOffset = TimeSpan.FromDays(10),
        FailureCutoffOffset = TimeSpan.FromDays(12),
    };

    protected override Expression<Func<Video, bool>> FilterExpression =>
        DataUow.DataFetches.ShouldFetchVideoData(JobDefinition);

    protected override async Task FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.VideoService.UpdateVideoAsync(entityId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}