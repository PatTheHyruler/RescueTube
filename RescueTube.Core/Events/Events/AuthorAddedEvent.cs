using MediatR;
using RescueTube.Core.Events.Events.Base;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Events.Events;

public class AuthorAddedEvent : PlatformEntityAddedEvent, INotification
{
    public AuthorAddedEvent(Guid id, EPlatform platform, string idOnPlatform) : base(id, platform, idOnPlatform)
    {
    }
}