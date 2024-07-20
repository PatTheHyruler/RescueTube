using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Services;

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

    [DisableConcurrentExecution(10 * 60)]
    public async Task EnqueueAsync(CancellationToken ct)
    {
        var imageIds = _dataUow.Ctx.Images
            .Where(_dataUow.Images.ShouldAttemptResolutionUpdate)
            .Select(i => i.Id)
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var imageId in imageIds)
        {
            _backgroundJobClient.Enqueue<UpdateImagesResolutionJob>(
                x => x.UpdateResolutionAsync(imageId, default));
        }
    }

    [DisableConcurrentSameArgExecution(60)]
    public async Task UpdateAuthorImagesResolutionsAsync(Guid authorId, CancellationToken ct)
    {
        var images = _dataUow.Ctx.Images
            .Where(_dataUow.Images.ShouldAttemptResolutionUpdate)
            .Where(i => i.AuthorImages!.Any(ai => ai.AuthorId == authorId))
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var image in images)
        {
            await _imageService.TryUpdateResolutionFromFileAsync(image, ct);
        }

        await _dataUow.SaveChangesAsync(ct);
    }

    [DisableConcurrentSameArgExecution(60)]
    public async Task UpdateVideoImagesResolutionsAsync(Guid videoId, CancellationToken ct)
    {
        var images = _dataUow.Ctx.Images
            .Where(_dataUow.Images.ShouldAttemptResolutionUpdate)
            .Where(i => i.VideoImages!.Any(vi => vi.VideoId == videoId))
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var image in images)
        {
            await _imageService.TryUpdateResolutionFromFileAsync(image, ct);
        }

        await _dataUow.SaveChangesAsync(ct);
    }

    [DisableConcurrentSameArgExecution(60)]
    public async Task UpdateResolutionAsync(Guid imageId, CancellationToken ct)
    {
        await _imageService.TryUpdateResolutionFromFileAsync(imageId, ct);
        await _dataUow.SaveChangesAsync(ct);
    }
}