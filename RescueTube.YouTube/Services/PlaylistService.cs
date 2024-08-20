using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public PlaylistService(IServiceProvider services, ILogger<BaseYouTubeService> logger) : base(services, logger)
    {
    }

    private async Task<VideoData?> FetchPlaylistDataYtdlAsync(string id, CancellationToken ct = default)
    {
        var playlistResult = await YouTubeUow.YoutubeDl.RunVideoDataFetch(Url.ToPlaylistUrl(id), ct);
        if (playlistResult is not { Success: true })
        {
            return null;
        }

        return playlistResult.Data;
    }

    public async Task<Playlist?> AddOrUpdatePlaylistAsync(Guid id, CancellationToken ct = default)
    {
        var idOnPlatform = await DbCtx.Playlists
            .Where(p => p.Id == id)
            .Select(p => p.IdOnPlatform)
            .FirstAsync(ct);
        return await AddOrUpdatePlaylistAsync(idOnPlatform, ct);
    }

    public async Task<Playlist?> AddOrUpdatePlaylistAsync(string id, CancellationToken ct = default)
    {
        var playlistData = await FetchPlaylistDataYtdlAsync(id, ct);
        return playlistData == null
            ? null
            : await AddOrUpdatePlaylistAsync(playlistData, YouTubeConstants.FetchTypes.YtDlp.Playlist, ct);
    }

    private async Task<Playlist> AddOrUpdatePlaylistAsync(VideoData playlistData, string fetchType,
        CancellationToken ct = default)
    {
        Expression<Func<PlaylistItem, bool>> playlistItemsFilter = pi => pi.RemovedAt == null;
        var playlist = await DbCtx.Playlists
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
            .FirstOrDefaultAsync(ct);
        var isNew = playlist == null;
        playlist ??= new Playlist { IdOnPlatform = playlistData.ID, PlaylistItems = new List<PlaylistItem>() };

        var newPlaylistData = playlistData.ToDomainPlaylist(fetchType);
        ServiceUow.EntityUpdateService.UpdatePlaylist(playlist, newPlaylistData, isNew,
            EntityUpdateService.EImageUpdateOptions.OnlyAdd);

        var fetchTime = newPlaylistData.DataFetches?.Where(df =>
                df.Source == YouTubeConstants.FetchTypes.YtDlp.Source
                && df.Type == fetchType
                && df.Success)
            .Select(df => df.OccurredAt)
            .OrderDescending()
            .FirstOrDefault() ?? DateTimeOffset.UtcNow;
        await UpdatePlaylistItems(playlist, playlistData, isNew, fetchTime, ct);

        var author = await YouTubeUow.AuthorService.AddOrGetAuthor(
            playlistData, YouTubeConstants.FetchTypes.YtDlp.Playlist, ct);
        playlist.Creator = author;
        playlist.CreatorId = author.Id;

        if (isNew)
        {
            DbCtx.Playlists.Add(playlist);
        }

        return playlist;
    }

    private async Task UpdatePlaylistItems(Playlist playlist, VideoData playlistData,
        bool isNew, DateTimeOffset fetchTime, CancellationToken ct)
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

            var video = await YouTubeUow.VideoService.AddOrUpdateVideoAsync(playlistEntry,
                YouTubeConstants.FetchTypes.YtDlp.Playlist, ct);
            var newPlaylistItem = new PlaylistItem
            {
                Position = index,
                VideoId = video.Id,
                Video = video,
                AddedAt = fetchTime,
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
                DbCtx.PlaylistItems.Add(newPlaylistItem);
            }
        }

        foreach (var playlistItem in playlist.PlaylistItems
                     .AssertNotNull($"PlaylistItems not loaded for YouTube playlist {playlistData.ID}")
                     .Where(pi => !playlistItemOccurrences.TryGetValue(
                         pi.Video
                             .AssertNotNull($"Video {pi.VideoId} not loaded for PlaylistItem {pi.Id}")
                             .IdOnPlatform, out var occurrences) || occurrences == 0))
        {
            playlistItem.RemovedAt = fetchTime;
        }
    }

    private void UpdatePlaylistItem(PlaylistItem existingPlaylistItem, PlaylistItem newPlaylistItem)
    {
        var previousPositionHistory = existingPlaylistItem.PositionHistories
            .AssertNotNull($"PositionHistories not loaded for PlaylistItem {existingPlaylistItem.Id}")
            .MaxBy(p => p.ValidUntil);
        DbCtx.PlaylistItemPositionHistories.Add(new PlaylistItemPositionHistory
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