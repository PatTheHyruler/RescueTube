using System.Text;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace RescueTube.YouTube.Utils;

public static class YtDlExtensions
{
    public static string? ErrorOutputToString<T>(this RunResult<T>? runResult) =>
        runResult?.ErrorOutput?.Aggregate("", (a, b) => $"'{a}', '{b}'");

    public static EPrivacyStatus? ToPrivacyStatus(this Availability? availability)
    {
        return availability switch
        {
            Availability
                    .Private =>
                null, // We can't access private videos so if the status is private (the enum default value), it's likely incorrect
            Availability.PremiumOnly => EPrivacyStatus.PremiumOnly,
            Availability.SubscriberOnly => EPrivacyStatus.SubscriberOnly,
            Availability.NeedsAuth => EPrivacyStatus.NeedsAuth,
            Availability.Unlisted => EPrivacyStatus.Unlisted,
            Availability.Public => EPrivacyStatus.Public,
            null => null,
            _ => throw new ArgumentException($"Unknown {typeof(Availability)} Enum value '{availability}'",
                nameof(availability)),
        };
    }

    private static Author ToAuthorBase(this VideoData videoData, string fetchType)
    {
        return new Author
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = videoData.ChannelID,

            UserName = videoData.UploaderID ??
                       (Url.IsAuthorHandleUrl(videoData.ChannelUrl, out var handle) ? handle : null),
            DisplayName = videoData.Uploader ?? videoData.Channel,

            AuthorStatisticSnapshots = videoData.ChannelFollowerCount == null
                ? null
                : new List<AuthorStatisticSnapshot>
                {
                    new()
                    {
                        FollowerCount = videoData.ChannelFollowerCount,
                        ValidAt = DateTimeOffset.UtcNow,
                    }
                },

            DataFetches = new List<DataFetch>
            {
                new()
                {
                    Source = YouTubeConstants.FetchTypes.YtDlp.Source,
                    Type = fetchType,
                    OccurredAt = DateTimeOffset.UtcNow,
                    ShouldAffectValidity = true,
                    Success = true,
                },
            },

            PrivacyStatusOnPlatform = EPrivacyStatus.Public,
            PrivacyStatus = EPrivacyStatus.Private,

            AddedToArchiveAt = DateTimeOffset.UtcNow,
        };
    }

    public static Author ToDomainAuthorFromChannel(this VideoData channelData, string fetchType)
    {
        var author = channelData.ToAuthorBase(fetchType);
        if (!string.IsNullOrWhiteSpace(channelData.Description))
        {
            author.Bio = new TextTranslationKey
            {
                Translations = new List<TextTranslation>
                {
                    new()
                    {
                        Content = channelData.Description,
                    },
                },
            };
        }

        if (channelData.Thumbnails is { Length: > 0 })
        {
            author.AuthorImages = channelData.Thumbnails.Select(t => new AuthorImage
                {
                    LastFetched = DateTimeOffset.UtcNow,

                    Image = new Image
                    {
                        Platform = EPlatform.YouTube,
                        IdOnPlatform = t.ID,

                        Url = t.Url,
                        Width = t.Width,
                        Height = t.Height,
                    },
                })
                .Select(ImageUtils.TrySetImageType)
                .ToList();
        }

        // TODO: Tags!

        return author;
    }

    public static Author ToDomainAuthorFromVideo(this VideoData videoData, string fetchType)
    {
        return videoData.ToAuthorBase(fetchType);
    }

    public static Author ToDomainAuthor(this CommentData commentData, string fetchType)
    {
        var domainAuthor = new Author
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = commentData.AuthorID,

            UserName = commentData.Author,

            PrivacyStatusOnPlatform = EPrivacyStatus.Public,
            PrivacyStatus = EPrivacyStatus.Private,

            AuthorImages = new List<AuthorImage>
            {
                new()
                {
                    LastFetched = DateTimeOffset.UtcNow,
                    ImageType = EImageType.ProfilePicture,
                    Image = new Image
                    {
                        Platform = EPlatform.YouTube,
                        Url = commentData.AuthorThumbnail
                    }
                }
            },

            DataFetches = new List<DataFetch>
            {
                new()
                {
                    Source = YouTubeConstants.FetchTypes.YtDlp.Source,
                    Type = fetchType,
                    OccurredAt = DateTimeOffset.UtcNow,
                    ShouldAffectValidity = true,
                    Success = true,
                }
            },
            AddedToArchiveAt = DateTimeOffset.UtcNow,
        };

        return domainAuthor;
    }

    public static Comment ToDomainComment(this CommentData commentData, string fetchType)
    {
        return new Comment
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = commentData.ID,

            Content = commentData.Text,

            CommentStatisticSnapshots = new List<CommentStatisticSnapshot>
            {
                new()
                {
                    LikeCount = commentData.LikeCount,
                    DislikeCount = commentData.DislikeCount,
                    IsFavorited = commentData.IsFavorited,
                }
            },

            AuthorIsCreator = commentData.AuthorIsUploader,

            CreatedAt = commentData.Timestamp.ToUniversalTime(),

            PrivacyStatus = EPrivacyStatus.Private,

            DataFetches = new List<DataFetch>
            {
                new()
                {
                    Source = YouTubeConstants.FetchTypes.YtDlp.Source,
                    Type = fetchType,
                    OccurredAt = DateTimeOffset.UtcNow,
                    ShouldAffectValidity = true,
                    Success = true,
                }
            },
            AddedToArchiveAt = DateTimeOffset.UtcNow
        };
    }

    public static Video ToDomainVideo(this VideoData videoData, string fetchType)
    {
        var liveStatus = videoData.GetLiveStatus();
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

                    ValidAt = DateTimeOffset.UtcNow,
                }
            },

            Captions = videoData.Subtitles?
                .SelectMany(kvp => kvp.Value.Select(subtitleData => new Caption
                {
                    Culture = kvp.Key,
                    Ext = subtitleData.Ext,
                    LastFetched = DateTimeOffset.UtcNow,
                    Name = subtitleData.Name,
                    Platform = EPlatform.YouTube,
                    Url = subtitleData.Url,
                })).ToList(),
            VideoImages = videoData.Thumbnails?.Select(e => e.ToVideoImage()).ToList(),
            VideoTags = videoData.Tags?.Select(e => new VideoTag
            {
                Tag = e,
                NormalizedTag = e
                    .ToUpper()
                    .Where(c => !char.IsPunctuation(c))
                    .Aggregate("", (current, c) => current + c)
                    .Normalize(NormalizationForm.FormKD),
            }).ToList(),

            LiveStatus = liveStatus,
            LiveStreamStartedAt =
                liveStatus.IsDefinitelyALiveStream()
                    ? videoData.ReleaseTimestamp
                    : null,

            CreatedAt = videoData.UploadDate,
            UpdatedAt = videoData.ModifiedTimestamp,
            PublishedAt = videoData.ReleaseTimestamp,

            PrivacyStatusOnPlatform = videoData.Availability.ToPrivacyStatus(),
            PrivacyStatus = EPrivacyStatus.Private,

            DataFetches =
            [
                new()
                {
                    Source = YouTubeConstants.FetchTypes.YtDlp.Source,
                    Type = fetchType,
                    OccurredAt = DateTimeOffset.UtcNow,
                    Success = true,
                    ShouldAffectValidity = true,
                }
            ],
            AddedToArchiveAt = DateTimeOffset.UtcNow,
            // TODO: Categories, etc
        };
        video.Type = video.TryGetVideoType();
        return video;
    }

    private static ELiveStatus GetLiveStatus(this VideoData videoData) =>
        videoData.LiveStatus switch
        {
            LiveStatus.IsLive => ELiveStatus.IsLive,
            LiveStatus.IsUpcoming => ELiveStatus.IsUpcoming,
            LiveStatus.WasLive => ELiveStatus.WasLive,
            LiveStatus.NotLive => ELiveStatus.NotLive,
            LiveStatus.PostLive => ELiveStatus.PostLive,
            LiveStatus.None => videoData.GetFallbackLiveStatus(),
            _ => videoData.GetFallbackLiveStatus(),
        };

    private static ELiveStatus GetFallbackLiveStatus(this VideoData videoData) =>
        (videoData.IsLive, videoData.WasLive) switch
        {
            (true, _) => ELiveStatus.IsLive,
            (_, true) => ELiveStatus.WasLive,
            _ => ELiveStatus.None,
        };

    private static EVideoType? TryGetVideoType(this Video video)
    {
        if (video.LiveStatus.IsDefinitelyALiveStream())
        {
            return EVideoType.Livestream;
        }

        return null;
    }
}