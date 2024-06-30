using RescueTube.Core.Contracts;
using RescueTube.Core.DTO.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Utils;

namespace RescueTube.YouTube.Services;

public class PresentationHandler : IPlatformPresentationHandler
{
    public bool CanHandle(VideoSimple video)
    {
        return video.Platform == EPlatform.YouTube;
    }

    public void Handle(VideoSimple video)
    {
        video.Url = Url.ToVideoUrl(video.IdOnPlatform);
        video.EmbedUrl = Url.ToVideoEmbedUrl(video.IdOnPlatform);
        foreach (var author in video.Authors)
        {
            Handle(author);
        }
    }

    public bool CanHandle(PlaylistDto playlist)
    {
        return playlist.Platform == EPlatform.YouTube;
    }

    public void Handle(PlaylistDto playlist)
    {
        playlist.UrlOnPlatform = Url.ToPlaylistUrl(playlist.IdOnPlatform);
        if (playlist.Creator != null)
        {
            Handle(playlist.Creator);
        }
    }

    public void Handle(AuthorSimple author)
    {
        author.UrlOnPlatform = Url.ToAuthorUrl(author.IdOnPlatform);
    }
}