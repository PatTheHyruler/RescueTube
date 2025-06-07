using RescueTube.Core.Utils.Pagination;

namespace WebApp.ApiModels;

public sealed record DataFetchQueryDtoV1 : IPaginationQuery
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 50;
}