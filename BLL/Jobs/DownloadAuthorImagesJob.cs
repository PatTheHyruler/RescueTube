using BLL.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace BLL.Jobs;

public class DownloadAuthorImagesJob
{
    private readonly AuthorService _authorService;
    private readonly IBackgroundJobClient _backgroundJobs;

    public DownloadAuthorImagesJob(AuthorService authorService,
        IBackgroundJobClient backgroundJobs)
    {
        _authorService = authorService;
        _backgroundJobs = backgroundJobs;
    }

    public async Task DownloadAuthorImages(Guid authorId, CancellationToken ct)
    {
        await _authorService.DownloadAuthorImages(authorId, ct);
        await _authorService.Ctx.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DownloadAllNotDownloadedAuthorImages(CancellationToken ct)
    {
        var imageIds = _authorService.Ctx.AuthorImages
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