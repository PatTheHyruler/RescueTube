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

    public static Author ToDomainAuthor(this VideoData videoData)
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

            PrivacyStatusOnPlatform = EPrivacyStatus.Public,
            IsAvailable = true,
            PrivacyStatus = EPrivacyStatus.Private,

            AddedToArchiveAt = DateTimeOffset.UtcNow,
        };
    }
    
    public static Author ToDomainAuthor(this CommentData commentData)
    {
        var domainAuthor = new Author
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = commentData.AuthorID,

            UserName = commentData.Author,

            IsAvailable = true,
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

            LastFetchUnofficial = DateTimeOffset.UtcNow,
            LastSuccessfulFetchUnofficial = DateTimeOffset.UtcNow,
            AddedToArchiveAt = DateTimeOffset.UtcNow,
        };

        return domainAuthor;
    }
    
    public static Comment ToDomainComment(this CommentData commentData)
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

            IsAvailable = true,
            PrivacyStatus = EPrivacyStatus.Private,

            LastFetchUnofficial = DateTimeOffset.UtcNow,
            LastSuccessfulFetchUnofficial = DateTimeOffset.UtcNow,
            AddedToArchiveAt = DateTimeOffset.UtcNow
        };
    }
}