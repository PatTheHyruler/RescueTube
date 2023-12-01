using BLL.Events.Events.Base;
using Domain.Enums;
using MediatR;

namespace BLL.Events.Events;

public class AuthorAddedEvent : PlatformEntityAddedEvent, INotification
{
    public AuthorAddedEvent(Guid id, EPlatform platform, string idOnPlatform) : base(id, platform, idOnPlatform)
    {
    }
}