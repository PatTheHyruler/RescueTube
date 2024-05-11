using MediatR;
using RescueTube.Core.Events.Events.Base;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Events.Events;

public class VideoAddedEvent : PlatformEntityAddedEvent, INotification
{
    public VideoAddedEvent(Guid id, EPlatform platform, string idOnPlatform) : base(id, platform, idOnPlatform)
    {
    }
}