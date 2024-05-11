namespace RescueTube.Core.Utils.Pagination;

public interface IPaginationBase
{
    public int Page { get; }
    public int Limit { get; }
}