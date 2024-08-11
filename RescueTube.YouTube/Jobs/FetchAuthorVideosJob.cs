using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs;

public class FetchAuthorVideosJob
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public FetchAuthorVideosJob(IDataUow dataUow, YouTubeUow youTubeUow, IBackgroundJobClient backgroundJobClient)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
        _backgroundJobClient = backgroundJobClient;
    }

    [DisableConcurrentExecution(10 * 60)]
    public async Task EnqueueAuthorVideoFetchesRecurring(CancellationToken ct)
    {
        var authorIds = _dataUow.Ctx.Authors
            .Where(AuthorService.AuthorHasNoTooRecentVideoFetches())
            .Where(AuthorService.AuthorIsActiveAndConfiguredForVideoArchival)
            .Select(a => a.Id)
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var authorId in authorIds)
        {
            _backgroundJobClient.Enqueue<FetchAuthorVideosJob>(x =>
                x.FetchAuthorVideos(authorId, false, default));
        }
    }

    [DisableConcurrentExecution(10 * 60)]
    public async Task FetchAuthorVideos(Guid authorId, bool force, CancellationToken ct)
    {
        await _youTubeUow.AuthorService.TryFetchAuthorVideosAsync(authorId: authorId, force: force, ct: ct);
        await _dataUow.SaveChangesAsync(ct);
    }
}