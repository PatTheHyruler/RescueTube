using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;
using RescueTube.Domain;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs;

public class FetchPlaylistDataJob
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;
    private readonly ILogger<FetchPlaylistDataJob> _logger;

    public FetchPlaylistDataJob(IDataUow dataUow, YouTubeUow youTubeUow, ILogger<FetchPlaylistDataJob> logger)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
        _logger = logger;
    }

    private static readonly DataFetchJobDefinition JobDefinition = new(
        YouTubeConstants.DataFetches.YtDlp.Playlist,
        successCutoffOffset: TimeSpan.FromDays(5),
        failureCutoffOffset: TimeSpan.FromDays(1));

    [SkipConcurrent("yt:fetch-next-playlist-data")]
    public async Task FetchNextPlaylistDataAsync(CancellationToken ct)
    {
        var playlistId = await _dataUow.Ctx.Playlists
            .AsExpandable()
            .Where(_dataUow.DataFetches.ShouldFetchData<Playlist>(JobDefinition))
            .OrderBy(p => p.Id)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(ct);
        if (playlistId == Guid.Empty)
        {
            return;
        }

        await FetchPlaylistDataAsync(playlistId, ct);
    }

    private async Task FetchPlaylistDataAsync(Guid playlistId, CancellationToken ct)
    {
        using var logScope = _logger.BeginScope("Fetching playlist data for playlist {PlaylistId}", playlistId);
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.PlaylistService.UpdatePlaylistAsync(playlistId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}