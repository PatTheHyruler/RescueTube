using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Jobs;
using RescueTube.Core.Utils;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchYouTubeExplodeAuthorDataJob : IEntityDataFetchJob
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

    public async Task ExecuteEntityDataFetchAsync(Guid entityId, CancellationToken ct)
    {
        if (AuthorService.LastYtExplodeRateLimitHit > _timeProvider.GetUtcNow().Subtract(TimeSpan.FromHours(1)))
        {
            _logger.LogDebug("Skipping YouTubeExplode data fetch, last rate limit was hit at {LastRateLimitHit}",
                AuthorService.LastYtExplodeRateLimitHit);
            return;
        }

        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.AuthorService.TryUpdateWithYouTubeExplodeDataAsync(entityId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}