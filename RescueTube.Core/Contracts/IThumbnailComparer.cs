using RescueTube.Domain.Enums;

namespace RescueTube.Core.Contracts;

public interface IThumbnailComparer
{
    public string?[]? Qualities { get; }
    public string?[]? Keys { get; }
    public EPlatform Platform { get; }
}