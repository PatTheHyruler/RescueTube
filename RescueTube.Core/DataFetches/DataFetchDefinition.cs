using RescueTube.Domain.Enums;

namespace RescueTube.Core.DataFetches;

public record DataFetchDefinition
{
    public required string Type { get; init; }
    public required string Source { get; init; }

    public required EPlatform Platform { get; init; }
    public required EEntityType EntityType { get; init; }
}