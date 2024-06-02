using RescueTube.Core.Utils.Pagination;

namespace WebApp.ApiModels;

public class PaginationResultDtoV1 : IPaginationResult
{
    public required int Page { get; set; }
    public required int Limit { get; set; }
    public required int AmountOnPage { get; set; }
    public int? TotalResults { get; set; }

    public static implicit operator PaginationResultDtoV1(PaginationResult paginationResult) =>
        new()
        {
            Page = paginationResult.Page,
            Limit = paginationResult.Limit,
            AmountOnPage = paginationResult.AmountOnPage,
            TotalResults = paginationResult.TotalResults,
        };
}