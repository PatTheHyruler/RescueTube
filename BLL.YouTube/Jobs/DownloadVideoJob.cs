using BLL.YouTube.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace BLL.YouTube.Jobs;

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
    public async Task DownloadVideo(Guid videoId, CancellationToken ct)
    {
        await _videoService.DownloadVideo(videoId, ct);
        await _videoService.Ctx.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DownloadNotDownloadedVideos(CancellationToken ct)
    {
        var addedToArchiveAtCutoff = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
        var videoIds = _videoService.Ctx.Videos
            .Where(v => v.VideoFiles!.Count == 0
                        && v.FailedDownloadAttempts < 3
                        && v.AddedToArchiveAt < addedToArchiveAtCutoff)
            .Select(v => v.Id)
            .AsAsyncEnumerable();

        await foreach (var videoId in videoIds)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }
            _backgroundJobClient.Enqueue<DownloadVideoJob>(x =>
                x.DownloadVideo(videoId, default));
        }
    }
}