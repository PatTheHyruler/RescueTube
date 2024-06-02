using RescueTube.Core.Contracts;
using RescueTube.Core.DTO.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Utils;

namespace RescueTube.YouTube.Services;

public class PresentationHandler : IPlatformVideoPresentationHandler
{
    public bool CanHandle(VideoSimple video)
    {
        return video.Platform == EPlatform.YouTube;
    }

    public void Handle(VideoSimple video)
    {
        video.Url = Url.ToVideoUrl(video.IdOnPlatform);
        video.EmbedUrl = Url.ToVideoEmbedUrl(video.IdOnPlatform);
        var thumbnails = video.Thumbnails
            .OrderByDescending(i => i.Quality, new ThumbnailQualityComparer())
            .ThenByDescending(i => i.Key, new ThumbnailTagComparer());
        video.Thumbnail = thumbnails.FirstOrDefault();
        foreach (var author in video.Authors)
        {
            Handle(author);
        }
    }

    public void Handle(AuthorSimple author)
    {
        author.UrlOnPlatform = Url.ToAuthorUrl(author.IdOnPlatform);
    }
}