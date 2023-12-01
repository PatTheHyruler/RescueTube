using Domain.Enums;

namespace BLL.Events.Events.Base;

public abstract class PlatformEntityAddedEvent
{
    protected PlatformEntityAddedEvent(Guid id, EPlatform platform, string idOnPlatform)
    {
        Platform = platform;
        IdOnPlatform = idOnPlatform;
        Id = id;
    }

    public Guid Id { get; set; }
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; }
}