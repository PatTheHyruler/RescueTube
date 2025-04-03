using RescueTube.Domain.Enums;

namespace RescueTube.Core.Contracts;

public interface IPlatformService
{
    public EPlatform Platform { get; }
}