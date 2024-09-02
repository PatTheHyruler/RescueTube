using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;

namespace RescueTube.Core.Jobs;

public class DownloadVideoImagesJob
{
    private readonly IDataUow _dataUow;
    private readonly IBackgroundJobClient _backgroundJobs;

    public DownloadVideoImagesJob(IDataUow dataUow,
        IBackgroundJobClient backgroundJobs)
    {
        _dataUow = dataUow;
        _backgroundJobs = backgroundJobs;
    }

    [SkipConcurrent("core:download-video-images:{0}")]
    public async Task DownloadVideoImages(Guid videoId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var imageIds = _dataUow.Ctx.Images
            .Where(e => e.FailedFetchAttempts < 3 &&
                        e.VideoImages!.Any(vi =>
                            vi.VideoId == videoId && vi.ValidUntil == null))
            .Select(i => i.Id)
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var imageId in imageIds)
        {
            _backgroundJobs.Enqueue<DownloadImageJob>(x => x.DownloadImage(imageId, default));
        }
        transaction.Complete();
    }

    [RescheduleConcurrentExecution("core:download-all-not-downloaded-video-images")]
    [Queue(JobQueues.LowerPriority)]
    public async Task DownloadAllNotDownloadedVideoImages(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var imageIds = _dataUow.Ctx.VideoImages
            .Where(e => e.Image!.LocalFilePath == null &&
                        e.Image.FailedFetchAttempts < 3 &&
                        e.Image.Url != null)
            .Select(e => e.ImageId)
            .AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var imageId in imageIds)
        {
            _backgroundJobs.Enqueue<DownloadImageJob>(JobQueues.LowerPriority, x =>
                x.DownloadImage(imageId, default));
        }
        transaction.Complete();
    }
}