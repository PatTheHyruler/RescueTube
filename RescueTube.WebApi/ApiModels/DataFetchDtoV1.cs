using RescueTube.Domain.Enums;

namespace RescueTube.WebApi.ApiModels;

public class DataFetchDtoV1
{
    public Guid Id { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public required DateTimeOffset? StatusUpdatedAt { get; init; }
    public required DataFetchStatus Status { get; init; }
    public required string Type { get; init; }
    public required string Source { get; init; }
    public required EPlatform Platform { get; init; }

    public required Guid? VideoId { get; init; }
    public required Guid? AuthorId { get; init; }
    public required Guid? PlaylistId { get; init; }
}