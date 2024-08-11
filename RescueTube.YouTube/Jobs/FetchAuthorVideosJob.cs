using Hangfire;
using RescueTube.Core.Data;

namespace RescueTube.YouTube.Jobs;

public class FetchAuthorVideosJob
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;

    public FetchAuthorVideosJob(IDataUow dataUow, YouTubeUow youTubeUow)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
    }
    
    // TODO: Regular fetches

    [DisableConcurrentExecution(10 * 60)]
    public async Task FetchAuthorVideos(Guid authorId, bool force, CancellationToken ct)
    {
        await _youTubeUow.AuthorService.TryFetchAuthorVideosAsync(authorId: authorId, force: force, ct: ct);
        await _dataUow.SaveChangesAsync(ct);
    }
}