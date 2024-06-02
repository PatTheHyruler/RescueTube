using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Contracts;

public interface IPlatformEntity
{
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; }
}