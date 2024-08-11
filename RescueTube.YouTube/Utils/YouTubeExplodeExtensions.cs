using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.Utils;

public static class YouTubeExplodeExtensions
{
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
}