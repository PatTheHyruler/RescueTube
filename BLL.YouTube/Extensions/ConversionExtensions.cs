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
}