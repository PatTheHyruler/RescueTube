using Hangfire;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;

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

    // TODO: Recurring job for re-fetching this data regularly?

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [1 * 3600, 2 * 3600, 4 * 3600])]
    [DisableConcurrentSameArgExecution(60)]
    public async Task FetchYouTubeExplodeAuthorData(Guid authorId, CancellationToken ct)
    {
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
            return;
        }

        await _youTubeUow.AuthorService.TryUpdateWithYouTubeExplodeDataAsync(authorId, ct);
        await _dataUow.SaveChangesAsync(ct);
    }
}