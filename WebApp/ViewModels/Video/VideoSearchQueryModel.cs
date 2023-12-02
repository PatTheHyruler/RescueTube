using BLL.DTO.Enums;
using Utils.Pagination.Contracts;

namespace WebApp.ViewModels.Video;

public class VideoSearchQueryModel : IPaginationQuery
{
    public string? NameQuery { get; set; }
    public string? AuthorQuery { get; set; }

    public int Page { get; set; }
    public int Limit { get; set; } = 50;
    public int? Total { get; set; }

    public EVideoSortingOptions SortingOptions { get; set; }
    public bool Descending { get; set; } = true;
}