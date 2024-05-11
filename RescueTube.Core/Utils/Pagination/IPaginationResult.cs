namespace RescueTube.Core.Utils.Pagination;

public interface IPaginationResult : IPaginationBase
{
    public int AmountOnPage { get; }
    public int? TotalResults { get; }
}