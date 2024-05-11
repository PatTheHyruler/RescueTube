namespace RescueTube.Core.Utils.Pagination;

public class PaginationResponse<T>
{
    public required T Result { get; init; }
    public required IPaginationResult PaginationResult { get; init; }
}