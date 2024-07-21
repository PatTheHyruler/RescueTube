using RescueTube.Domain.Enums;

namespace RescueTube.Domain;

public record RecognizedPlatformUrl(string Url, string IdOnPlatform, EPlatform Platform, EEntityType EntityType)
{
    public string? IdType { get; init; }
};