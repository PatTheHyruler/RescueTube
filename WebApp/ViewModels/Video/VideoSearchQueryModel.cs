using RescueTube.Core.DTO.Enums;
using RescueTube.Core.Utils.Pagination;

namespace WebApp.ViewModels.Video;

public class VideoSearchQueryModel : IPaginationQuery
{
    public string? NameQuery { get; set; }
    public string? AuthorQuery { get; set; }

    public int Page { get; set; }
    public int Limit { get; set; } = 50;
    public uint? Total { get; set; }

    public EVideoSortingOptions SortingOptions { get; set; }
    public bool Descending { get; set; } = true;
}