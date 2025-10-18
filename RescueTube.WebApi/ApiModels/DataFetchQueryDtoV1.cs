using RescueTube.Domain.Enums;

namespace RescueTube.WebApi.ApiModels;

public sealed record DataFetchQueryDtoV1 : IPaginationQueryOptionalDtoV1
{
    public int? Page { get; init; }
    public int? Limit { get; init; }

    public int DefaultPage => 0;
    public int DefaultLimit => 50;

    public bool? OrderByDescending { get; init; }

    // TODO: Figure out how to use DateTimeRange for this
    // Currently nested DTOs don't seem to work for query params
    public DateTimeOffset? StartedAtFrom { get; init; }
    public DateTimeOffset? StartedAtTo { get; init; }

    public string? Type { get; init; }
    public string? Source { get; init; }

    public bool? Success { get; init; }

    public DataFetchStatus[]? Statuses { get; init; }
}