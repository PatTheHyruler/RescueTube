using RescueTube.Core.DTO.Entities;

namespace RescueTube.Core.Contracts;

public interface IPlatformVideoPresentationHandler
{
    public bool CanHandle(VideoSimple video);
    public void Handle(VideoSimple video);
}