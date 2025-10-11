using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;
using YoutubeDLSharp.Metadata;

namespace RescueTube.YouTube.Services;

public class PlaylistService : BaseYouTubeService
{
    private readonly AppDbContext _dbCtx;
    private readonly ILogger<PlaylistService> _logger;
    private readonly EntityUpdateService _entityUpdateService;
    private readonly YouTubeServices _youTubeServices;
    private readonly DataFetchService _dataFetchService;

    public PlaylistService(AppDbContext dbCtx, ILogger<PlaylistService> logger, EntityUpdateService entityUpdateService, YouTubeServices youTubeServices, DataFetchService dataFetchService)
    {
        _dbCtx = dbCtx;
        _logger = logger;
        _entityUpdateService = entityUpdateService;
        _youTubeServices = youTubeServices;
        _dataFetchService = dataFetchService;
    }

    public async Task UpdatePlaylistAsync(Guid id, CancellationToken ct = default)
    {
        var idOnPlatform = await _dbCtx.Playlists
            .Where(p => p.Id == id && p.Platform == EPlatform.YouTube)
            .Select(p => p.IdOnPlatform)
            .FirstAsync(ct);
        if (await _dataFetchService.IsFetchingAsync(YouTubeConstants.DataFetches.YtDlp.Playlist, idOnPlatform, ct))
        {
            _logger.LogInformation("Already fetching {Platform} playlist {PlaylistIdOnPlatform}, skipping duplicate fetch", EPlatform.YouTube, idOnPlatform);
            return;
        }
        await AddOrUpdatePlaylistAsync(idOnPlatform, ct); // TODO: add failed data fetch
    }

    public async Task<Playlist?> AddOrUpdatePlaylistAsync(string idOnPlatform, CancellationToken ct = default)
    {
        await using var dataFetchScope = await _dataFetchService.StartDataFetchAsync(
            YouTubeConstants.DataFetches.YtDlp.Playlist, idOnPlatform, ct);
        dataFetchScope.ThrowIfAlreadyFetching();

        var dataFetch = dataFetchScope.DataFetch;

        var playlistResult = await _youTubeServices.YoutubeDl.RunVideoDataFetchAsync(Url.ToPlaylistUrl(idOnPlatform), ct);
        if (playlistResult is not { Success: true, Data: not null })
        {
            await _dataFetchService.UpdateDataFetchStatusAsync(dataFetch, DataFetchStatus.Failed,
                message: playlistResult?.ErrorOutputToString());
            return null;
        }

        _dataFetchService.CompleteDataFetch(dataFetch);

        return await AddOrUpdatePlaylistAsync(playlistResult.Data, dataFetch, ct);
    }

    private async Task<Playlist> AddOrUpdatePlaylistAsync(VideoData playlistData, DataFetch dataFetch,
        CancellationToken ct = default)
    {
        Expression<Func<PlaylistItem, bool>> playlistItemsFilter = pi => pi.RemovedAt == null;
        var playlist = await _dbCtx.Playlists
            .AsExpandable()
            .Where(p => p.Platform == EPlatform.YouTube && p.IdOnPlatform == playlistData.ID)
            .Include(p => p.Title!)
            .ThenInclude(t => t.Translations)
            .Include(p => p.Description!)
            .ThenInclude(t => t.Translations)
            .Include(p => p.PlaylistStatisticSnapshots)
            .Include(p => p.PlaylistImages!)
            .ThenInclude(pi => pi.Image)
            .Include(p => p.PlaylistItems!.Where(pi => playlistItemsFilter.Invoke(pi)))
            .ThenInclude(pi => pi.Video)
            .Include(p => p.PlaylistItems!.Where(pi => playlistItemsFilter.Invoke(pi)))
            .ThenInclude(pi => pi.PositionHistories!
                .OrderByDescending(ph => ph.ValidUntil)
                .Take(1))
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
        var isNew = playlist == null;
        playlist ??= new Playlist { IdOnPlatform = playlistData.ID, PlaylistItems = new List<PlaylistItem>() };

        _dbCtx.DataFetchResults.Add(new DataFetchResult { Playlist = playlist, DataFetch = dataFetch });

        var newPlaylistData = playlistData.ToDomainPlaylist();
        _entityUpdateService.UpdatePlaylist(playlist, newPlaylistData, isNew,
            EntityUpdateService.EImageUpdateOptions.OnlyAdd);

        await UpdatePlaylistItemsAsync(playlist, playlistData, isNew, dataFetch, ct);

        var author = await _youTubeServices.AuthorService.AddOrGetAuthor(playlistData, dataFetch, ct);
        playlist.Creator = author;
        playlist.CreatorId = author.Id;

        if (isNew)
        {
            _dbCtx.Playlists.Add(playlist);
        }

        return playlist;
    }

    private async Task UpdatePlaylistItemsAsync(Playlist playlist, VideoData playlistData,
        bool isNew, DataFetch dataFetch, CancellationToken ct)
    {
        var previousPlaylistItems =
            isNew
                ? []
                : playlist.PlaylistItems.AssertNotNull(
                    $"PlaylistItems not loaded for YouTube playlist {playlistData.ID}");

        var playlistItemOccurrences = new Dictionary<string, int>();

        for (uint index = 0; index < playlistData.Entries.Length; index++)
        {
            var playlistEntry = playlistData.Entries[index];
            if (playlistEntry == null)
            {
                continue;
            }

            playlistItemOccurrences.TryAdd(playlistEntry.ID, 0);
            var occurrences = playlistItemOccurrences[playlistEntry.ID]++;

            var existingPlaylistItem = previousPlaylistItems
                .Where(pi => pi.Video
                    .AssertNotNull($"Video {pi.VideoId} not loaded for PlaylistItem {pi.Id}")
                    .IdOnPlatform == playlistEntry.ID)
                .Skip(occurrences) // Attempting to behave reasonably if playlist has/had multiple entries for the same video
                .FirstOrDefault();

            var video = await _youTubeServices.VideoService.AddOrUpdateVideoAsync(playlistEntry, dataFetch, ct);
            var newPlaylistItem = new PlaylistItem
            {
                Position = index,
                VideoId = video.Id,
                Video = video,
                AddedAt = dataFetch.StartedAt,
                Playlist = playlist,
                PlaylistId = playlist.Id,
            };
            if (existingPlaylistItem != null)
            {
                if (existingPlaylistItem.Position != newPlaylistItem.Position)
                {
                    UpdatePlaylistItem(existingPlaylistItem, newPlaylistItem);
                }
            }
            else
            {
                playlist.PlaylistItems
                    .AssertNotNull($"PlaylistItems was null for YouTube playlist {playlist.IdOnPlatform}")
                    .Add(newPlaylistItem);
                _dbCtx.PlaylistItems.Add(newPlaylistItem);
            }
        }

        foreach (var playlistItem in playlist.PlaylistItems
                     .AssertNotNull($"PlaylistItems not loaded for YouTube playlist {playlistData.ID}")
                     .Where(pi => !playlistItemOccurrences.TryGetValue(
                         pi.Video
                             .AssertNotNull($"Video {pi.VideoId} not loaded for PlaylistItem {pi.Id}")
                             .IdOnPlatform, out var occurrences) || occurrences == 0))
        {
            playlistItem.RemovedAt = dataFetch.StartedAt;
        }
    }

    private void UpdatePlaylistItem(PlaylistItem existingPlaylistItem, PlaylistItem newPlaylistItem)
    {
        var previousPositionHistory = existingPlaylistItem.PositionHistories
            .AssertNotNull($"PositionHistories not loaded for PlaylistItem {existingPlaylistItem.Id}")
            .MaxBy(p => p.ValidUntil);
        _dbCtx.PlaylistItemPositionHistories.Add(new PlaylistItemPositionHistory
        {
            PlaylistItemId = existingPlaylistItem.Id,
            PlaylistItem = existingPlaylistItem,
            Position = existingPlaylistItem.Position,
            ValidUntil = DateTimeOffset.UtcNow,
            ValidSince = previousPositionHistory?.ValidUntil ?? existingPlaylistItem.AddedAt
        });
        existingPlaylistItem.Position = newPlaylistItem.Position;
    }
}