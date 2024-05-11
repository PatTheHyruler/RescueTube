using BLL.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class AuthorService : BaseService
{
    public AuthorService(IServiceProvider services, ILogger<AuthorService> logger) : base(services, logger)
    {
    }

    public async Task DownloadAuthorImages(Guid authorId, CancellationToken ct = default)
    {
        var images = DbCtx.Images
            .Where(i => i.FailedFetchAttempts < 3 &&
                        i.AuthorImages!.Any(ai =>
                            ai.AuthorId == authorId && ai.ValidUntil == null))
            .Include(e => e.AuthorImages)
            .AsAsyncEnumerable();
        // var images = Ctx.AuthorImages
        //     .Where(e => e.AuthorId == authorId && e.ValidUntil != null)
        //     .Select(e => e.Image!)
        //     .Include(e => e.AuthorImages)
        //     .AsAsyncEnumerable();

        await foreach (var image in images)
        {
            await ServiceUow.ImageService.UpdateImage(image, ct);
        }
    }
}