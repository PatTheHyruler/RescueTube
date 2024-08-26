using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;

namespace RescueTube.Core.Jobs;

public class DownloadAuthorImagesJob
{
    private readonly IDataUow _dataUow;
    private readonly IBackgroundJobClient _backgroundJobs;

    public DownloadAuthorImagesJob(IBackgroundJobClient backgroundJobs, IDataUow dataUow)
    {
        _backgroundJobs = backgroundJobs;
        _dataUow = dataUow;
    }

    [SkipConcurrentSameArgExecution]
    public async Task DownloadAuthorImages(Guid authorId, CancellationToken ct)
    {
        var imageIds = _dataUow.Ctx.Images
            .Where(i => i.FailedFetchAttempts < 3 &&
                        i.AuthorImages!.Any(ai =>
                            ai.AuthorId == authorId && ai.ValidUntil == null))
            .Include(e => e.AuthorImages)
            .Select(x => x.Id)
            .AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var imageId in imageIds)
        {
            _backgroundJobs.Enqueue<DownloadImageJob>(x => x.DownloadImage(imageId, default));
        }
    }

    [DisableConcurrentExecution(10 * 60)]
    public async Task DownloadAllNotDownloadedAuthorImages(CancellationToken ct)
    {
        var imageIds = _dataUow.Ctx.AuthorImages
            .Where(e => e.Image!.LocalFilePath == null &&
                        e.Image.FailedFetchAttempts < 3 &&
                        e.Image.Url != null)
            .Select(e => e.ImageId)
            .AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var imageId in imageIds)
        {
            _backgroundJobs.Enqueue<DownloadImageJob>(x =>
                x.DownloadImage(imageId, default));
        }
    }
}