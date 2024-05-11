namespace BLL.Utils.Pagination.Contracts;

public class PaginationQuery : IPaginationQuery
{
    public int Page { get; set; } = 0;
    public int Limit { get; set; } = 50;
    public int? Total { get; set; }
}