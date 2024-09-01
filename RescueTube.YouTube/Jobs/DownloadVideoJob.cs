using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs;

public class DownloadVideoJob
{
    private readonly VideoService _videoService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public DownloadVideoJob(VideoService videoService, IBackgroundJobClient backgroundJobClient)
    {
        _videoService = videoService;
        _backgroundJobClient = backgroundJobClient;
    }

    [AutomaticRetry(Attempts = 0)]
    [SkipConcurrent(Key = "yt:download-video:{0}")]
    [RescheduleConcurrentExecution(Key = "yt:download-video")]
    public async Task DownloadVideoAsync(Guid videoId, CancellationToken ct)
    {
        var result = await _videoService.DownloadVideoAsync(videoId, ct);

        using var transaction = TransactionUtils.NewTransactionScope();
        await _videoService.PersistVideoDownloadResultAsync(result.Result, result.Video, ct);

        await _videoService.DataUow.SaveChangesAsync(CancellationToken.None);
        transaction.Complete();
    }

    public async Task DownloadNotDownloadedVideosAsync(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var addedToArchiveAtCutoff = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1));
        var videoIds = _videoService.DbCtx.Videos
            .Where(v =>
                    v.VideoFiles!.Count == 0
                    && v.AddedToArchiveAt < addedToArchiveAtCutoff
                    && v.DataFetches!
                        .Where(d =>
                            d.Source == YouTubeConstants.FetchTypes.YtDlp.Source
                            && d.Type == YouTubeConstants.FetchTypes.YtDlp.VideoFileDownload)
                        .OrderByDescending(x => x.OccurredAt)
                        .Take(3)
                        .Count(x => !x.Success) < 3 // TODO: Make sure this compiles to SQL
            )
            .Select(v => v.Id)
            .AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var videoId in videoIds)
        {
            _backgroundJobClient.Enqueue<DownloadVideoJob>(x =>
                x.DownloadVideoAsync(videoId, default));
        }

        transaction.Complete();
    }
}