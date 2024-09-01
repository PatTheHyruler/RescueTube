using Hangfire;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;

namespace RescueTube.Core.Jobs;

public class DownloadImageJob
{
    private readonly ImageService _imageService;

    public DownloadImageJob(ImageService imageService)
    {
        _imageService = imageService;
    }

    [SkipConcurrent(Key = "core:download-image:{0}")]
    [Queue(JobQueues.LowerPriority)]
    public async Task DownloadImage(Guid imageId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _imageService.UpdateImage(imageId, ct);
        await _imageService.DataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}