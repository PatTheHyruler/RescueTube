using Utils.Pagination.Contracts;

namespace WebApp.ViewModels;

public class PaginationUiPartialViewModel : IPaginationQuery
{
    public int Limit { get; set; }
    public int Page { get; set; }
    public int? Total { get; set; }
    public int AmountOnPage { get; set; }
}