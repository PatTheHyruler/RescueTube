using MediatR;
using RescueTube.Core.Events.Base;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Events;

public class AuthorAddedEvent : PlatformEntityAddedEvent, INotification
{
    public AuthorAddedEvent(Guid id, EPlatform platform, string idOnPlatform)
        : base(id, platform, idOnPlatform, EEntityType.Author)
    {
    }
}