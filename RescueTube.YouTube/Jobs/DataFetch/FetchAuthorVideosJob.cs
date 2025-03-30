using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchAuthorVideosJob : EntityDataFetchJobBase<Author>
{
    private readonly YouTubeUow _youTubeUow;

    public FetchAuthorVideosJob(IDataUow dataUow, YouTubeUow youTubeUow, ILogger<FetchAuthorVideosJob> logger)
        : base(dataUow, logger, JobDefinition.DataFetchDefinition)
    {
        _youTubeUow = youTubeUow;
    }

    private static readonly DataFetchJobDefinition JobDefinition = new()
    {
        DataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.ChannelVideos,
        SuccessCutoffOffset = AuthorService.LatestAllowedVideosFetchOffset,
        FailureCutoffOffset = AuthorService.LatestAllowedVideosFetchOffset,
    };

    protected override Expression<Func<Author, bool>> FilterExpression =>
        DataUow.DataFetches.ShouldFetchAuthorData(JobDefinition);

    protected override async Task FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.AuthorService.TryFetchAuthorVideosAsync(authorId: entityId, force: false, ct: ct);
        await DataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}