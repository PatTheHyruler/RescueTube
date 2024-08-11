using Microsoft.Extensions.Logging;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
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

    public async Task<Playlist?> AddPlaylistAsync(string id, CancellationToken ct = default)
    {
        var playlistData = await FetchPlaylistDataYtdlAsync(id, ct);
        return playlistData == null
            ? null
            : await AddPlaylistAsync(playlistData, YouTubeConstants.FetchTypes.YtDlp.Playlist, ct);
    }

    private async Task<Playlist> AddPlaylistAsync(VideoData ytDlData, string fetchType, CancellationToken ct = default)
    {
        var playlist = new Playlist
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = ytDlData.ID,

            Title = new TextTranslationKey
            {
                Translations = new List<TextTranslation>
                {
                    new()
                    {
                        Content = ytDlData.Title,
                    }
                }
            },
            Description = new TextTranslationKey
            {
                Translations = new List<TextTranslation>
                {
                    new()
                    {
                        Content = ytDlData.Description,
                    }
                }
            },

            UpdatedAt = ytDlData.ModifiedTimestamp?.ToUniversalTime() ?? ytDlData.ModifiedDate?.ToUniversalTime(),
            PrivacyStatusOnPlatform = ytDlData.Availability.ToPrivacyStatus(),
            PrivacyStatus = EPrivacyStatus.Private,

            AddedToArchiveAt = DateTimeOffset.UtcNow,
            DataFetches = new List<DataFetch>
            {
                new()
                {
                    OccurredAt = DateTimeOffset.UtcNow,
                    Success = true,
                    Type = fetchType,
                    ShouldAffectValidity = true,
                    Source = YouTubeConstants.FetchTypes.YtDlp.Source,
                },
            },

            PlaylistItems = new List<PlaylistItem>(),
            PlaylistImages = ytDlData.Thumbnails.Select(e => e.ToPlaylistImage()).ToList(),
        };

        if (ytDlData.ViewCount == null && ytDlData.LikeCount == null && ytDlData.DislikeCount == null &&
            ytDlData.CommentCount == null)
        {
            playlist.PlaylistStatisticSnapshots = new List<PlaylistStatisticSnapshot>
            {
                new()
                {
                    ViewCount = ytDlData.ViewCount,
                    LikeCount = ytDlData.LikeCount,
                    DislikeCount = ytDlData.DislikeCount,
                    CommentCount = ytDlData.CommentCount,

                    ValidAt = DateTimeOffset.UtcNow,
                },
            };
        }

        for (uint index = 0; index < ytDlData.Entries.Length; index++)
        {
            var playlistEntry = ytDlData.Entries[index];
            if (playlistEntry == null)
            {
                continue;
            }

            var video = await YouTubeUow.VideoService.AddOrUpdateVideoAsync(playlistEntry,
                YouTubeConstants.FetchTypes.YtDlp.Playlist, ct);
            playlist.PlaylistItems.Add(new PlaylistItem
            {
                Position = index,
                VideoId = video.Id,
                Video = video,
            });
        }

        var author = await YouTubeUow.AuthorService.AddOrGetAuthor(ytDlData, YouTubeConstants.FetchTypes.YtDlp.Playlist, ct);
        playlist.Creator = author;
        playlist.CreatorId = author.Id;

        DbCtx.Playlists.Add(playlist);

        return playlist;
    }
}