using MediatR;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Events.Base;

public abstract class PlatformEntityAddedEvent : INotification
{
    protected PlatformEntityAddedEvent(Guid id, EPlatform platform, string idOnPlatform, EEntityType entityType)
    {
        Platform = platform;
        IdOnPlatform = idOnPlatform;
        Id = id;
        EntityType = entityType;
    }

    public Guid Id { get; set; }
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; }
    public EEntityType EntityType { get; set; }
}