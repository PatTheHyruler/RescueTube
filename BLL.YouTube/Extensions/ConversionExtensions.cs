using Domain.Entities;
using Domain.Enums;
using YoutubeDLSharp.Metadata;
using Url = BLL.YouTube.Utils.Url;

namespace BLL.YouTube.Extensions;

public static class ConversionExtensions
{
    public static EPrivacyStatus? ToPrivacyStatus(this Availability? availability)
    {
        return availability switch
        {
            Availability.Private => EPrivacyStatus.Private,
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
                    ValidAt = DateTime.UtcNow,
                }
            },

            PrivacyStatusOnPlatform = EPrivacyStatus.Public,
            IsAvailable = true,
            PrivacyStatus = EPrivacyStatus.Private,

            AddedToArchiveAt = DateTime.UtcNow,
        };
    }
    
    public static Author ToDomainAuthor(this CommentData commentData)
    {
        var domainAuthor = new Author
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = commentData.AuthorID,

            DisplayName = commentData.Author,

            IsAvailable = true,
            PrivacyStatus = EPrivacyStatus.Private,
            
            AuthorImages = new List<AuthorImage>
            {
                new()
                {
                    LastFetched = DateTime.UtcNow,
                    ImageType = EImageType.ProfilePicture,
                    Image = new Image
                    {
                        Platform = EPlatform.YouTube,
                        Url = commentData.AuthorThumbnail
                    }
                }
            },

            LastFetchUnofficial = DateTime.UtcNow,
            LastSuccessfulFetchUnofficial = DateTime.UtcNow,
            AddedToArchiveAt = DateTime.UtcNow,
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

            LastFetchUnofficial = DateTime.UtcNow,
            LastSuccessfulFetchUnofficial = DateTime.UtcNow,
            AddedToArchiveAt = DateTime.UtcNow
        };
    }
}