using BLL.DTO.Enums;

namespace WebApp.ViewModels;

public class VideoSearchQueryModel
{
    public string? NameQuery { get; set; }
    public string? AuthorQuery { get; set; }

    public int Page { get; set; } = 0;
    public int Limit { get; set; } = 50;

    public EVideoSortingOptions SortingOptions { get; set; }
    public bool Descending { get; set; } = true;
}