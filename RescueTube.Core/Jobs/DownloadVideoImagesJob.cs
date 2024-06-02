using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Services;

namespace RescueTube.Core.Jobs;

public class DownloadVideoImagesJob
{
    private readonly VideoService _videoService;
    private readonly IDataUow _dataUow;
    private readonly IBackgroundJobClient _backgroundJobs;

    public DownloadVideoImagesJob(VideoService videoService, IDataUow dataUow,
        IBackgroundJobClient backgroundJobs)
    {
        _videoService = videoService;
        _dataUow = dataUow;
        _backgroundJobs = backgroundJobs;
    }

    public async Task DownloadVideoImages(Guid videoId, CancellationToken ct)
    {
        await _videoService.DownloadVideoImages(videoId, ct);
        await _videoService.DataUow.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DownloadAllNotDownloadedVideoImages(CancellationToken ct)
    {
        var imageIds = _dataUow.Ctx.VideoImages
            .Where(e => e.Image!.LocalFilePath == null &&
                        e.Image.FailedFetchAttempts < 3 &&
                        e.Image.Url != null)
            .Select(e => e.ImageId)
            .AsAsyncEnumerable();

        await foreach (var imageId in imageIds)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            _backgroundJobs.Enqueue<DownloadImageJob>(x =>
                x.DownloadImage(imageId, default));
        }
    }
}