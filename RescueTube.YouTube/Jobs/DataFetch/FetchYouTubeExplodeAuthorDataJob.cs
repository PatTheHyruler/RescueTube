using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchYouTubeExplodeAuthorDataJob : EntityDataFetchJobBase<Author>, IEntityDataFetchJobWithDefinition
{
    private readonly YouTubeUow _youTubeUow;
    private readonly TimeProvider _timeProvider;

    public FetchYouTubeExplodeAuthorDataJob(YouTubeUow youTubeUow, IDataUow dataUow,
        ILogger<FetchYouTubeExplodeAuthorDataJob> logger, TimeProvider timeProvider)
        : base(dataUow, logger, JobDefinition.DataFetchDefinition)
    {
        _youTubeUow = youTubeUow;
        _timeProvider = timeProvider;
    }

    public static DataFetchJobDefinition JobDefinition { get; } = new()
    {
        DataFetchDefinition = YouTubeConstants.DataFetches.YouTubeExplode.Channel,
        SuccessCutoffOffset = TimeSpan.FromDays(10),
        FailureCutoffOffset = TimeSpan.FromDays(1),
    };

    protected override Expression<Func<Author, bool>> FilterExpression =>
        DataUow.DataFetches.ShouldFetchAuthorData(JobDefinition);

    public override async Task FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        if (AuthorService.LastYtExplodeRateLimitHit > _timeProvider.GetUtcNow().Subtract(TimeSpan.FromHours(1)))
        {
            Logger.LogDebug("Skipping YouTubeExplode data fetch, last rate limit was hit at {LastRateLimitHit}",
                AuthorService.LastYtExplodeRateLimitHit);
            return;
        }

        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.AuthorService.TryUpdateWithYouTubeExplodeDataAsync(entityId, ct);
        await DataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}