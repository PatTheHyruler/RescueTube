using RescueTube.Domain.Enums;

namespace RescueTube.WebApi.ApiModels;

public class DataFetchDtoV1
{
    public Guid Id { get; set; }
    public required DateTimeOffset OccurredAt { get; set; }
    public required DataFetchStatus Status { get; init; }
    public required string Type { get; set; }
    public required string Source { get; set; }
    public required EPlatform Platform { get; init; }

    public required string? VideoIdOnPlatform { get; init; }
    public required string? AuthorIdOnPlatform { get; init; }
    public required string? PlaylistIdOnPlatform { get; init; }
}