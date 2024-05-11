using BLL.DTO.Entities;

namespace BLL.Contracts;

public interface IPlatformVideoPresentationHandler
{
    public bool CanHandle(VideoSimple video);
    public void Handle(VideoSimple video);
}