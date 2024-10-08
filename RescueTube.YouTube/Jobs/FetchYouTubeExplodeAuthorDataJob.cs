using Hangfire;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.Jobs;

public class FetchYouTubeExplodeAuthorDataJob
{
    private readonly ILogger<FetchYouTubeExplodeAuthorDataJob> _logger;
    private readonly YouTubeUow _youTubeUow;
    private readonly IDataUow _dataUow;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public FetchYouTubeExplodeAuthorDataJob(YouTubeUow youTubeUow, IDataUow dataUow,
        IBackgroundJobClient backgroundJobClient, ILogger<FetchYouTubeExplodeAuthorDataJob> logger)
    {
        _youTubeUow = youTubeUow;
        _dataUow = dataUow;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    [RescheduleConcurrentExecution("yt:enqueue-ytexplode-author-data-fetches-recurring")]
    public async Task EnqueueYouTubeExplodeAuthorDataFetchesRecurring(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var authorIds = _dataUow.Ctx.Authors
            .AsExpandable()
            .Where(
                a => a.Platform == EPlatform.YouTube
                     && !a.DataFetches!.Any(d =>
                         _dataUow.DataFetches.IsTooRecent(
                             YouTubeConstants.FetchTypes.YouTubeExplode.Source,
                             YouTubeConstants.FetchTypes.YouTubeExplode.Channel,
                             DateTimeOffset.UtcNow.AddDays(-10),
                             DateTimeOffset.UtcNow.AddDays(-1)
                         ).Invoke(d)
                     )
            )
            .Select(a => a.Id)
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var authorId in authorIds)
        {
            _backgroundJobClient.Enqueue<FetchYouTubeExplodeAuthorDataJob>(x =>
                x.FetchYouTubeExplodeAuthorData(authorId, default));
        }
        transaction.Complete();
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [1 * 3600, 2 * 3600, 4 * 3600])]
    [SkipConcurrent("yt:fetch-ytexplode-author-data:{0}")]
    [Queue(JobQueues.LowPriority)]
    public async Task FetchYouTubeExplodeAuthorData(Guid authorId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        if (_youTubeUow.AuthorService.LastYtExplodeRateLimitHit > DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)))
        {
            _backgroundJobClient.Schedule<FetchYouTubeExplodeAuthorDataJob>(
                x => x.FetchYouTubeExplodeAuthorData(authorId, default),
                _youTubeUow.AuthorService.LastYtExplodeRateLimitHit
                    .AddHours(1)
                    .AddMinutes(Random.Shared.Next(-10, 10)));
            _logger.LogInformation(
                "Skipping YouTubeExplode extra author data fetch for author {AuthorId}, last rate limit hit: {LastRateLimitHit}",
                authorId, _youTubeUow.AuthorService.LastYtExplodeRateLimitHit);
            transaction.Complete();
            return;
        }

        await _youTubeUow.AuthorService.TryUpdateWithYouTubeExplodeDataAsync(authorId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}