using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public record RecognizedPlatformUrl(string Url, string IdOnPlatform, EPlatform Platform, EEntityType EntityType);