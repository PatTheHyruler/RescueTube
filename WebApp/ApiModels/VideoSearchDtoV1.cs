using RescueTube.Core.DTO.Enums;
using RescueTube.Core.Utils.Pagination;

namespace WebApp.ApiModels;

public class VideoSearchDtoV1 : VideoSearchFilterDtoV1, IPaginationQuery
{
    public EVideoSortingOptions SortingOptions { get; init; }
    public bool Descending { get; init; } = true;

    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 50;
}