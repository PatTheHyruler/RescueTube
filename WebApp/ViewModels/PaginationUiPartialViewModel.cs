using RescueTube.Core.Utils.Pagination;

namespace WebApp.ViewModels;

public class PaginationUiPartialViewModel : IPaginationResult
{
    public int Limit { get; set; }
    public int Page { get; set; }
    public int? TotalResults { get; set; }
    public int AmountOnPage { get; set; }
}