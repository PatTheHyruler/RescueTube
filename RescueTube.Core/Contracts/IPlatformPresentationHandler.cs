using RescueTube.Core.DTO.Entities;

namespace RescueTube.Core.Contracts;

public interface IPlatformPresentationHandler
{
    public bool CanHandle(VideoSimple video);
    public void Handle(VideoSimple video);

    public bool CanHandle(IPlaylistDto playlist);
    public void Handle(IPlaylistDto playlist);

    public bool CanHandle(AuthorSimple author);
    public void Handle(AuthorSimple author);
}