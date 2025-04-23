using RescueTube.Core.DTO.Entities;

namespace RescueTube.Core.Contracts;

public interface IPlatformPresentationHandler
{
    public bool CanHandle(VideoSimple video);
    public void Handle(VideoSimple video);

    public bool CanHandle(PlaylistDto playlist);
    public void Handle(PlaylistDto playlist);

    public bool CanHandle(AuthorSimple author);
    public void Handle(AuthorSimple author);
}