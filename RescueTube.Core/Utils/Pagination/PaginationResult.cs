namespace RescueTube.Core.Utils.Pagination;

public record PaginationResult : IPaginationResult
{
    public required int Page { get; init; }
    public required int Limit { get; init; }
    public required int AmountOnPage { get; init; }
    public int? TotalResults { get; init; }
}