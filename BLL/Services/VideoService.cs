using BLL.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class VideoService : BaseService
{
    public VideoService(IServiceProvider services, ILogger<VideoService> logger) : base(services, logger)
    {
    }

    public async Task DownloadVideoImages(Guid videoId, CancellationToken ct = default)
    {
        var images = Ctx.Images
            .Where(e => e.FailedFetchAttempts < 3 &&
                        e.VideoImages!.Any(vi =>
                            vi.VideoId == videoId && vi.ValidUntil == null))
            .Include(e => e.VideoImages)
            .AsAsyncEnumerable();
        // var images = Ctx.VideoImages
        //     .Where(e => e.VideoId == videoId && e.ValidUntil != null)
        //     .Select(e => e.Image!)
        //     .Include(e => e.VideoImages)
        //     .AsAsyncEnumerable();

        await foreach (var image in images)
        {
            await ServiceUow.ImageService.UpdateImage(image, ct);
        }
    }
}