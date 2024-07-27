using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using YoutubeDLSharp.Metadata;
using Url = RescueTube.YouTube.Utils.Url;

namespace RescueTube.YouTube.Extensions;

public static class ConversionExtensions
{
    public static EPrivacyStatus? ToPrivacyStatus(this Availability? availability)
    {
        return availability switch
        {
            Availability.Private => null, // We can't access private videos so if the status is private (the enum default value), it's likely incorrect
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

    public static Author ToDomainAuthor(this YoutubeExplode.Channels.Channel channel)
    {
        return new Author
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = channel.Id,

            UserName = Url.IsAuthorHandleUrl(channel.Url, out var handle) ? handle : null,
            DisplayName = channel.Title,

            PrivacyStatusOnPlatform = EPrivacyStatus.Public,
            PrivacyStatus = EPrivacyStatus.Private,

            AddedToArchiveAt = DateTimeOffset.UtcNow,

            DataFetches = new List<DataFetch>
            {
                new()
                {
                    Source = YouTubeConstants.FetchTypes.YtDlp.Source,
                    Type = YouTubeConstants.FetchTypes.YouTubeExplode.Channel,
                    OccurredAt = DateTimeOffset.UtcNow,
                    ShouldAffectValidity = true,
                    Success = true,
                },
            },
        };
    }

    public static Author ToDomainAuthor(this VideoData videoData, string fetchType)
    {
        return new Author
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = videoData.ChannelID,

            UserName = videoData.UploaderID ??
                       (Url.IsAuthorHandleUrl(videoData.ChannelUrl, out var handle) ? handle : null),
            DisplayName = videoData.Uploader ?? videoData.Channel,

            AuthorStatisticSnapshots = new List<AuthorStatisticSnapshot>
            {
                new()
                {
                    FollowerCount = videoData.ChannelFollowerCount,
                    ValidAt = DateTimeOffset.UtcNow,
                }
            },

            AuthorImages = videoData.Thumbnails?.Select(t => new AuthorImage
            {
                ImageType = EImageType.Thumbnail,
                LastFetched = DateTimeOffset.UtcNow,
                Image = new Image
                {
                    Width = t.Width,
                    Height = t.Height,
                    IdOnPlatform = t.ID,
                    Url = t.Url,
                }
            }).ToList(),

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
}