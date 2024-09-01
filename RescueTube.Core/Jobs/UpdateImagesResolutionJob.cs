using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;

namespace RescueTube.Core.Jobs;

public class UpdateImagesResolutionJob
{
    private readonly IDataUow _dataUow;
    private readonly ImageService _imageService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public UpdateImagesResolutionJob(IDataUow dataUow, ImageService imageService,
        IBackgroundJobClient backgroundJobClient)
    {
        _dataUow = dataUow;
        _imageService = imageService;
        _backgroundJobClient = backgroundJobClient;
    } 

    [RescheduleConcurrentExecution(Key = "core:update-images-resolution-enqueue")]
    public async Task EnqueueAsync(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var imageIds = _dataUow.Ctx.Images
            .Where(_dataUow.Images.ShouldAttemptResolutionUpdate)
            .Select(i => i.Id)
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var imageId in imageIds)
        {
            _backgroundJobClient.Enqueue<UpdateImagesResolutionJob>(
                x => x.UpdateResolutionAsync(imageId, default));
        }
        transaction.Complete();
    }

    [SkipConcurrent(Key = "core:update-author-images-resolutions:{0}")]
    public async Task UpdateAuthorImagesResolutionsAsync(Guid authorId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var images = _dataUow.Ctx.Images
            .Where(_dataUow.Images.ShouldAttemptResolutionUpdate)
            .Where(i => i.AuthorImages!.Any(ai => ai.AuthorId == authorId))
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var image in images)
        {
            await _imageService.TryUpdateResolutionFromFileAsync(image, ct);
        }

        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }

    [SkipConcurrent(Key = "core:update-video-images-resolutions:{0}")]
    public async Task UpdateVideoImagesResolutionsAsync(Guid videoId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var images = _dataUow.Ctx.Images
            .Where(_dataUow.Images.ShouldAttemptResolutionUpdate)
            .Where(i => i.VideoImages!.Any(vi => vi.VideoId == videoId))
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var image in images)
        {
            await _imageService.TryUpdateResolutionFromFileAsync(image, ct);
        }

        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }

    [SkipConcurrent(Key = "core:update-image-resolution:{0}")]
    public async Task UpdateResolutionAsync(Guid imageId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _imageService.TryUpdateResolutionFromFileAsync(imageId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}