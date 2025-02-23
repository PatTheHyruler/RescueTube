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

public class FetchVideoDataJob
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IJobStorageAccessor _jobStorageAccessor;
    private readonly ILogger<FetchVideoDataJob> _logger;

    public FetchVideoDataJob(IDataUow dataUow, YouTubeUow youTubeUow, IBackgroundJobClient backgroundJobClient, IJobStorageAccessor jobStorageAccessor, ILogger<FetchVideoDataJob> logger)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
        _backgroundJobClient = backgroundJobClient;
        _jobStorageAccessor = jobStorageAccessor;
        _logger = logger;
    }

    [SkipConcurrent("yt:enqueue-video-data-fetches-recurring")]
    public async Task EnqueueVideoDataFetchesRecurring(CancellationToken ct)
    {
        const int targetConcurrentDataFetches = 5;
        using var transaction = TransactionUtils.NewTransactionScope();

        var currentlyProcessingVideoIds = await _jobStorageAccessor.GetActiveVideoFetchJobVideoIdsAsync(ct);

        var openProcessingSlots = targetConcurrentDataFetches - currentlyProcessingVideoIds.Count;
        if (openProcessingSlots <= 0)
        {
            _logger.LogInformation("Skipping recurring video data fetch enqueue, {JobCount} active jobs already exist", currentlyProcessingVideoIds.Count);
            transaction.Complete();
            return;
        }

        var videoIds = await _dataUow.Ctx.Videos
            .AsExpandable()
            .Where(v => v.Platform == EPlatform.YouTube
                        && !currentlyProcessingVideoIds.Contains(v.Id)
                        && !v.DataFetches!.Any(d => _dataUow.DataFetches.IsTooRecent(
                            YouTubeConstants.FetchTypes.YtDlp.Source,
                            YouTubeConstants.FetchTypes.YtDlp.VideoPage,
                            DateTimeOffset.UtcNow.AddDays(-10),
                            DateTimeOffset.UtcNow.AddHours(-12)
                        ).Invoke(d)))
            .OrderBy(v => v.Id) // TODO: Better thing to order by
            .Select(v => v.Id)
            .Take(targetConcurrentDataFetches)
            .ToListAsync(ct);
        _logger.LogInformation("Fetched {VideoIdsToEnqueueCount} video IDs to enqueue with {AlreadyProcessingVideoIdsCount} video IDs already processing", videoIds.Count, currentlyProcessingVideoIds.Count);

        foreach (var videoId in videoIds)
        {
            _backgroundJobClient.Enqueue<FetchVideoDataJob>(x =>
                x.FetchVideoData(videoId, default));
        }

        transaction.Complete();
    }

    [SkipConcurrent("yt:fetch-video-data:{0}")]
    public async Task FetchVideoData(Guid videoId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.VideoService.AddOrUpdateVideoAsync(videoId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}