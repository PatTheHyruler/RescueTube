using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Services;

namespace RescueTube.Core.Jobs;

public class DownloadImageJob
{
    private readonly ImageService _imageService;

    public DownloadImageJob(ImageService imageService)
    {
        _imageService = imageService;
    }

    [SkipConcurrentSameArgExecution]
    public async Task DownloadImage(Guid imageId, CancellationToken ct)
    {
        await _imageService.UpdateImage(imageId, ct);
        await _imageService.DataUow.SaveChangesAsync(ct);
    }
}