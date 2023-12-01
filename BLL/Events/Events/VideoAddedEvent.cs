using BLL.Events.Events.Base;
using Domain.Enums;
using MediatR;

namespace BLL.Events.Events;

public class VideoAddedEvent : PlatformEntityAddedEvent, INotification
{
    public VideoAddedEvent(Guid id, EPlatform platform, string idOnPlatform) : base(id, platform, idOnPlatform)
    {
    }
}