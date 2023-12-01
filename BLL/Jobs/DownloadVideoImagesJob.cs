using BLL.Services;
using DAL.EF.DbContexts;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace BLL.Jobs;

public class DownloadVideoImagesJob
{
    private readonly VideoService _videoService;
    private readonly AbstractAppDbContext _dbCtx;
    private readonly IBackgroundJobClient _backgroundJobs;

    public DownloadVideoImagesJob(VideoService videoService, AbstractAppDbContext dbCtx,
        IBackgroundJobClient backgroundJobs)
    {
        _videoService = videoService;
        _dbCtx = dbCtx;
        _backgroundJobs = backgroundJobs;
    }

    public async Task DownloadVideoImages(Guid videoId, CancellationToken ct)
    {
        await _videoService.DownloadVideoImages(videoId, ct);
        await _videoService.Ctx.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DownloadAllNotDownloadedVideoImages(CancellationToken ct)
    {
        var imageIds = _dbCtx.VideoImages
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