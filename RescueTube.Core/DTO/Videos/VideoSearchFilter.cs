using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Videos;

public record VideoSearchFilter
{
    public EPlatform? Platform { get; init; }
    public string? Name { get; init; }
    public string? Author { get; init; }
    public Guid[]? AuthorIds { get; init; }
}