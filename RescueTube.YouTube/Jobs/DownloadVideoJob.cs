using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Jobs.Filters;
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
    [SkipConcurrentSameArgExecution(Order = 0)]
    [DisableConcurrentExecution(60 * 60, Order = 1)]
    public async Task DownloadVideoAsync(Guid videoId, CancellationToken ct)
    {
        await _videoService.DownloadVideoAsync(videoId, ct);
        await _videoService.DataUow.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DownloadNotDownloadedVideosAsync(CancellationToken ct)
    {
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
            .AsAsyncEnumerable();

        await foreach (var videoId in videoIds)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            _backgroundJobClient.Enqueue<DownloadVideoJob>(x =>
                x.DownloadVideoAsync(videoId, default));
        }
    }
}