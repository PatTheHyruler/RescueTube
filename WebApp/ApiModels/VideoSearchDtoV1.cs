using RescueTube.Core.DTO.Enums;
using RescueTube.Core.Utils.Pagination;

namespace WebApp.ApiModels;

public class VideoSearchDtoV1 : IPaginationQuery
{
    public string? NameQuery { get; set; }
    public string? AuthorQuery { get; set; }

    public EVideoSortingOptions SortingOptions { get; set; }
    public bool Descending { get; set; } = true;
    
    public int Page { get; set; }
    public int Limit { get; set; }
}