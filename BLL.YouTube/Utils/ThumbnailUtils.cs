using Domain.Entities;
using Domain.Enums;
using YoutubeDLSharp.Metadata;
using YoutubeExplode.Common;

namespace BLL.YouTube.Utils;

public static class ThumbnailUtils
{
    public static VideoImage ToVideoImage(this ThumbnailData data)
    {
        Url.IsVideoThumbnailUrl(data.Url, out var quality, out var tag, out var ext);

        var image = new Image
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = data.ID,

            Key = tag?.Identifier,
            Quality = quality?.Name,
            Ext = ext,

            Url = data.Url,

            Width = quality?.Width,
            Height = quality?.Height,
        };

        var videoImage = new VideoImage
        {
            ImageType = EImageType.Thumbnail,
            Preference = data.Preference,

            Image = image,
        };

        return videoImage;
    }
}