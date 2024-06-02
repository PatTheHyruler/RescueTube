namespace RescueTube.Core.Utils.Pagination;

public record PaginationQuery : IPaginationQuery
{
    public int Page { get; init; } = 0;
    public int Limit { get; init; } = 50;

    public static PaginationQuery Default = new();
}