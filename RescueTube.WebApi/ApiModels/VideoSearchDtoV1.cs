using RescueTube.Core.DTO.Enums;
using RescueTube.Core.Utils.Pagination;

namespace RescueTube.WebApi.ApiModels;

public class VideoSearchDtoV1 : IPaginationQuery
{
    public VideoSearchFilterDtoV1? Filter { get; init; }
    public EVideoSortingOptions SortingOptions { get; init; }
    public bool Descending { get; init; } = true;

    public int Page { get; init; } = 0;
    public int Limit { get; init; } = 50;
}