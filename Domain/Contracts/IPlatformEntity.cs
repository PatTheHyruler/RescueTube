using Domain.Enums;

namespace Domain.Contracts;

public interface IPlatformEntity
{
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; }
}