namespace RescueTube.Core.Utils.Pagination.Contracts;

public interface IPaginationQuery
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int? Total { get; set; }
}