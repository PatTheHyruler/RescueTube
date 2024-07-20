using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using YoutubeDLSharp.Metadata;

namespace RescueTube.YouTube.Utils;

public static class ThumbnailUtils
{
    private static Image ToImage(ThumbnailData data, ParsedThumbnailInfo? parsedThumbnailInfo = null)
    {
        return new Image
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = data.ID,

            Key = parsedThumbnailInfo?.Tag?.Identifier,
            Quality = parsedThumbnailInfo?.Quality?.Name,
            Ext = parsedThumbnailInfo?.Ext,

            Url = data.Url,

            Width = data.Width,
            Height = data.Height,
        };
    }

    public static VideoImage ToVideoImage(this ThumbnailData data)
    {
        Url.IsVideoThumbnailUrl(data.Url, out var parsedThumbnailInfo);
        var image = ToImage(data, parsedThumbnailInfo);

        var videoImage = new VideoImage
        {
            ImageType = EImageType.Thumbnail,
            Preference = data.Preference,
            LastFetched = DateTimeOffset.UtcNow,

            Image = image,
        };

        return videoImage;
    }

    public static PlaylistImage ToPlaylistImage(this ThumbnailData data)
    {
        Url.IsVideoThumbnailUrl(data.Url, out var parsedVideoThumbnailInfo);
        var image = ToImage(data, parsedVideoThumbnailInfo);

        var playlistImage = new PlaylistImage
        {
            ImageType = EImageType.Thumbnail,
            LastFetched = DateTimeOffset.UtcNow,

            Image = image,
        };

        return playlistImage;
    }
}