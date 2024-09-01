using Hangfire;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.Jobs;

public class FetchPlaylistDataJob
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public FetchPlaylistDataJob(IDataUow dataUow, YouTubeUow youTubeUow, IBackgroundJobClient backgroundJobClient)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
        _backgroundJobClient = backgroundJobClient;
    }

    [DisableConcurrentExecution(5 * 60)]
    public async Task EnqueuePlaylistDataFetches(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var playlistIds = _dataUow.Ctx.Playlists
            .AsExpandable()
            .Where(p => p.Platform == EPlatform.YouTube
                        && !p.DataFetches!.Any(d => _dataUow.DataFetches.IsTooRecent(
                            YouTubeConstants.FetchTypes.YtDlp.Source,
                            YouTubeConstants.FetchTypes.YtDlp.Playlist,
                            DateTimeOffset.UtcNow.AddDays(-5),
                            DateTimeOffset.UtcNow.AddDays(-1)
                        ).Invoke(d)))
            .Select(p => p.Id)
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var playlistId in playlistIds)
        {
            _backgroundJobClient.Enqueue<FetchPlaylistDataJob>(x =>
                x.FetchPlaylistData(playlistId, default));
        }
        transaction.Complete();
    }

    [SkipConcurrent(Key = "yt:fetch-playlist-data:{0}")]
    public async Task FetchPlaylistData(Guid playlistId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.PlaylistService.AddOrUpdatePlaylistAsync(playlistId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}