using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs;

public class DownloadVideoJob
{
    private readonly VideoDownloadService _videoDownloadService;
    private readonly ILogger<DownloadVideoJob> _logger;
    private readonly StorageLimitService _storageLimitService;

    private static readonly ConcurrentDictionary<Guid, DateTimeOffset> DownloadingVideoIds = new();

    public DownloadVideoJob(
        VideoDownloadService videoDownloadService,
        ILogger<DownloadVideoJob> logger,
        StorageLimitService storageLimitService
    )
    {
        _videoDownloadService = videoDownloadService;
        _storageLimitService = storageLimitService;
        _logger = logger;
    }

    private async Task DownloadVideoAsync(Guid videoId, CancellationToken ct)
    {
        if (!DownloadingVideoIds.IsEmpty || !DownloadingVideoIds.TryAdd(videoId, DateTimeOffset.UtcNow))
        {
            return;
        }

        if (VideoDownloadService.LatestThrottlingAssessment?.ShouldSkipDownloading() ?? false)
        {
            _logger.LogInformation("Skipping video download due to likely throttling.");
            return;
        }

        if (await _storageLimitService.IsVideoDownloadForbidden(EPlatform.YouTube, ct))
        {
            _logger.LogWarning("Skipping video download, storage limit reached.");
            return;
        }

        try
        {
            var result = await _videoDownloadService.DownloadVideoAsync(videoId, ct);

            using var transaction = TransactionUtils.NewTransactionScope();
            await _videoDownloadService.PersistVideoDownloadResultAsync(result.Result, result.Video, ct);

            await _videoDownloadService.DataUow.SaveChangesAsync(CancellationToken.None);
            transaction.Complete();
        }
        finally
        {
            DownloadingVideoIds.TryRemove(videoId, out _);
        }
    }

    [SkipConcurrent("yt:download-not-downloaded-video-recurring")]
    public async Task DownloadNotDownloadedVideoAsync(CancellationToken ct)
    {
        if (!DownloadingVideoIds.IsEmpty)
        {
            return;
        }

        var videoId = await _videoDownloadService.DbCtx.Videos
            .Where(v =>
                v.VideoFiles!.Count == 0
                && v.DataFetches!
                    .Where(d =>
                        d.Source == YouTubeConstants.FetchTypes.YtDlp.Source
                        && d.Type == YouTubeConstants.FetchTypes.YtDlp.VideoFileDownload)
                    .OrderByDescending(x => x.OccurredAt)
                    .Take(3)
                    .Count(x => !x.Success) < 3
            )
            .Select(v => v.Id)
            .FirstOrDefaultAsync(ct);

        if (videoId == default)
        {
            return;
        }

        if (!DownloadingVideoIds.IsEmpty)
        {
            return;
        }

        await DownloadVideoAsync(videoId, ct);
    }
}