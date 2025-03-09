using Hangfire;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;
using RescueTube.Domain;
using RescueTube.Domain.Entities;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs;

public class FetchYouTubeExplodeAuthorDataJob
{
    private readonly ILogger<FetchYouTubeExplodeAuthorDataJob> _logger;
    private readonly YouTubeUow _youTubeUow;
    private readonly IDataUow _dataUow;
    private readonly TimeProvider _timeProvider;

    public FetchYouTubeExplodeAuthorDataJob(YouTubeUow youTubeUow, IDataUow dataUow, ILogger<FetchYouTubeExplodeAuthorDataJob> logger, TimeProvider timeProvider)
    {
        _youTubeUow = youTubeUow;
        _dataUow = dataUow;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    private static readonly DataFetchJobDefinition JobDefinition = new(
        YouTubeConstants.DataFetches.YouTubeExplode.Channel,
        successCutoffOffset: TimeSpan.FromDays(10),
        failureCutoffOffset: TimeSpan.FromDays(1));

    [SkipConcurrent("yt:fetch-next-ytexplode-author-data")]
    [Queue(JobQueues.LowPriority)]
    public async Task FetchNextYouTubeExplodeAuthorDataAsync(CancellationToken ct)
    {
        if (AuthorService.LastYtExplodeRateLimitHit > _timeProvider.GetUtcNow().Subtract(TimeSpan.FromHours(1)))
        {
            _logger.LogDebug("Skipping data fetch {DataFetchDefinition}, last rate limit was hit at {LastRateLimitHit}",
                JobDefinition.DataFetchDefinition, AuthorService.LastYtExplodeRateLimitHit);
            return;
        }
        var authorId = await _dataUow.Ctx.Authors
            .AsExpandable()
            .Where(_dataUow.DataFetches.ShouldFetchData<Author>(JobDefinition))
            .OrderBy(a => a.Id)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(ct);
        await FetchYouTubeExplodeAuthorDataAsync(authorId, ct);
    }

    private async Task FetchYouTubeExplodeAuthorDataAsync(Guid authorId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.AuthorService.TryUpdateWithYouTubeExplodeDataAsync(authorId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}