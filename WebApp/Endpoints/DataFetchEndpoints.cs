using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Pagination;
using RescueTube.Core.Identity;
using RescueTube.Core.Utils.Pagination;
using WebApp.ApiModels;
using WebApp.ApiModels.Mappers;

namespace WebApp.Endpoints;

public static class DataFetchEndpoints
{
    public static void MapDataFetchEndpoints(this IEndpointRouteBuilder app)
    {
        var dataFetchesGroup = app.MapGroup("data-fetches").WithTags("Data Fetches");

        dataFetchesGroup.MapGet("", GetDataFetchesAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
            .HasApiVersion(1);
    }

    private static async Task<Ok<DataFetchesResponseDtoV1>> GetDataFetchesAsync(
        [AsParameters] DataFetchQueryDtoV1 request,
        [FromServices] IDataUow dataUow,
        CancellationToken ct)
    {
        var query = dataUow.Ctx.DataFetches
            .Where(x => request.Source == null || x.Source == request.Source)
            .Where(x => request.Type == null || x.Type == request.Type)
            .Where(x => request.OccurredAtFrom == null || x.OccurredAt >= request.OccurredAtFrom)
            .Where(x => request.OccurredAtTo == null || x.OccurredAt <= request.OccurredAtTo)
            .Where(x => request.Success == null || x.Success == request.Success);

        var orderedQuery = (request.OrderByDescending ?? true) switch
        {
            true => query.OrderByDescending(x => x.OccurredAt),
            false => query.OrderBy(x => x.OccurredAt),
        };
        var dataFetches = await orderedQuery
            .Paginate(request)
            .ToListAsync(ct);

        var paginationResult = request.ToPaginationResult(dataFetches.Count, await query.CountAsync(ct));
        var result = new DataFetchesResponseDtoV1
        {
            DataFetches = dataFetches.Select(ApiMapper.MapDataFetchDtoV1).ToArray(),
            Limit = paginationResult.Limit,
            Page = paginationResult.Page,
            TotalResults = paginationResult.TotalResults,
            AmountOnPage = paginationResult.AmountOnPage,
        };
        return TypedResults.Ok(result);
    }
}