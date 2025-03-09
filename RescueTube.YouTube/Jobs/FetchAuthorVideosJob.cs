using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;
using RescueTube.Domain;
using RescueTube.Domain.Entities;
using RescueTube.YouTube.Services;

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

    private static readonly DataFetchJobDefinition JobDefinition = new(
        YouTubeConstants.DataFetches.YtDlp.ChannelVideos,
        AuthorService.LatestAllowedVideosFetchOffset,
        AuthorService.LatestAllowedVideosFetchOffset);

    [AutomaticRetry(Attempts = 0)]
    [SkipConcurrent("yt:fetch-next-playlist-data")]
    public async Task FetchNextChannelVideosAsync(CancellationToken ct)
    {
        var authorId = await _dataUow.Ctx.Authors
            .Where(_dataUow.DataFetches.ShouldFetchData<Author>(JobDefinition))
            .Where(AuthorService.AuthorIsActiveAndConfiguredForVideoArchival)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(ct);
        if (authorId == Guid.Empty)
        {
            return;
        }
        await FetchChannelVideos(authorId, false, ct);
    }

    private async Task FetchChannelVideos(Guid authorId, bool force, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.AuthorService.TryFetchAuthorVideosAsync(authorId: authorId, force: force, ct: ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}