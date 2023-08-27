using System.Text;
using BLL.YouTube.Base;
using BLL.YouTube.Extensions;
using BLL.YouTube.Utils;
using Domain.Entities;
using Domain.Entities.Localization;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using YoutubeDLSharp.Metadata;

namespace BLL.YouTube.Services;

public class VideoService : BaseYouTubeService
{
    public VideoService(IServiceProvider services) : base(services)
    {
    }

    public async Task<VideoData?> FetchVideoDataYtdl(string id, bool fetchComments, CancellationToken ct = default)
    {
        var videoResult = await YouTubeUow.YoutubeDl.RunVideoDataFetch(
            Url.ToVideoUrl(id), fetchComments: fetchComments, ct: ct);
        if (videoResult is not { Success: true })
        {
            ct.ThrowIfCancellationRequested();

            // TODO: Add status change if video exists in archive

            return null;
        }

        return videoResult.Data;
    }

    public async Task<Video?> AddVideo(string id, CancellationToken ct = default)
    {
        var videoData = await FetchVideoDataYtdl(id, false, ct);
        return videoData == null ? null : await AddVideo(videoData);
    }

    public async Task<Video> AddVideo(VideoData videoData)
    {
        var video = new Video
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = videoData.ID,

            Title = new TextTranslationKey
            {
                Translations = new List<TextTranslation>
                {
                    new()
                    {
                        Content = videoData.Title,
                    }
                }
            },
            Description = new TextTranslationKey
            {
                Translations = new List<TextTranslation>
                {
                    new()
                    {
                        Content = videoData.Description,
                    }
                }
            },

            Duration = videoData.Duration != null ? TimeSpan.FromSeconds(videoData.Duration.Value) : null,

            VideoStatisticSnapshots = new List<VideoStatisticSnapshot>
            {
                new()
                {
                    ViewCount = videoData.ViewCount,
                    LikeCount = videoData.LikeCount,
                    DislikeCount = videoData.DislikeCount,
                    CommentCount = videoData.CommentCount,

                    ValidAt = DateTime.UtcNow,
                }
            },

            Captions = videoData.Subtitles
                .SelectMany(kvp => kvp.Value.Select(subtitleData => new Caption
                {
                    Culture = kvp.Key,
                    Ext = subtitleData.Ext,
                    LastFetched = DateTime.UtcNow,
                    Name = subtitleData.Name,
                    Platform = EPlatform.YouTube,
                    Url = subtitleData.Url,
                })).ToList(),
            VideoImages = videoData.Thumbnails.Select(e => e.ToVideoImage()).ToList(),
            VideoTags = videoData.Tags.Select(e => new VideoTag
            {
                Tag = e,
                NormalizedTag = e
                    .ToUpper()
                    .Where(c => !char.IsPunctuation(c))
                    .Aggregate("", (current, c) => current + c)
                    .Normalize(NormalizationForm.FormKD),
            }).ToList(),

            IsLiveStreamRecording = videoData.WasLive ?? videoData.IsLive,
            LiveStreamStartedAt = (videoData.WasLive ?? videoData.IsLive ?? false) ? videoData.ReleaseTimestamp : null,

            CreatedAt = videoData.UploadDate,
            UpdatedAt = videoData.ModifiedTimestamp,
            PublishedAt = videoData.ReleaseTimestamp,

            PrivacyStatusOnPlatform = videoData.Availability.ToPrivacyStatus(),
            IsAvailable = videoData.Availability.ToPrivacyStatus().IsAvailable(),
            PrivacyStatus = EPrivacyStatus.Private,

            LastFetchUnofficial = DateTime.UtcNow,
            LastSuccessfulFetchUnofficial = DateTime.UtcNow,
            AddedToArchiveAt = DateTime.UtcNow,
        };

        try
        {
            await YouTubeUow.AuthorService.AddAndSetAuthor(video, videoData);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to add author for YouTube video {VideoId}, Author ID {AuthorId} ({AuthorName})", videoData.ID, videoData.ChannelID, videoData.Channel);
        }

        Ctx.Videos.Add(video);

        Ctx.RegisterVideoAddedCallback(new PlatformEntityAddedEventArgs(EPlatform.YouTube, videoData.ID));
        // TODO: Comments callback subscribe
        // TODO: Image downloader callback subscribe
        // TODO: Captions downloader callback subscribe

        // TODO: Categories, Games

        return video;
    }
}