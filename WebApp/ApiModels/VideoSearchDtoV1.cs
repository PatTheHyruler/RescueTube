using RescueTube.Core.DTO.Enums;
using RescueTube.Core.Utils.Pagination;

namespace WebApp.ApiModels;

public class VideoSearchDtoV1 : IPaginationQuery
{
    public string? NameQuery { get; init; }
    public string? AuthorQuery { get; init; }
    public Guid[]? AuthorIds { get; init; }

    public EVideoSortingOptions SortingOptions { get; init; }
    public bool Descending { get; init; } = true;

    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 50;
}