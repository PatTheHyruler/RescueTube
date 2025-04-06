using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;

namespace RescueTube.Core.Jobs;

public class DownloadImageJob : IJob
{
    private readonly ImageService _imageService;
    private readonly IDataUow _dataUow;

    public DownloadImageJob(ImageService imageService, IDataUow dataUow)
    {
        _imageService = imageService;
        _dataUow = dataUow;
    }

    public async Task<JobExecutionResult> RunAsync(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();

        var images = await _dataUow.Ctx.Images
            .Where(i =>
                    i.LocalFilePath == null
                    && i.FailedFetchAttempts < 3
                    && i.Url != null)
            // TODO: Filter by entity image validity and entity settings?
            .Include(i => i.AuthorImages)
            .Include(i => i.VideoImages)
            .Include(i => i.PlaylistImages)
            .AsSingleQuery()
            .Take(2)
            .ToArrayAsync(ct);
        if (images is not [var image, .. var nextImages])
        {
            return JobExecutionResult.NothingToProcess;
        }

        await _imageService.DownloadImageAsync(image, ct);

        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();

        return nextImages.Length != 0
            ? JobExecutionResult.HasMoreToProcess
            : JobExecutionResult.Succeeded;
    }
}