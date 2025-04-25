using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Services;
using WebApp.ApiModels;
using WebApp.ApiModels.Mappers;
using WebApp.Utils;

namespace WebApp.Endpoints;

public static class VideoEndpoints
{
    public static void MapVideosEndpoints(this IEndpointRouteBuilder app)
    {
        var videosGroup = app.MapGroup("videos").WithTags("Videos");

        videosGroup.MapGet("search", SearchVideosAsync).HasApiVersion(1);
        videosGroup.MapGet("{videoId:guid}", GetVideoAsync)
            .AllowAnonymous()
            .HasApiVersion(1);
    }

    private static async Task<Ok<VideoSearchResponseDtoV1>> SearchVideosAsync(
        [FromBody] VideoSearchDtoV1 query, [FromServices] VideoPresentationService videoPresentationService,
        HttpContext httpContext, CancellationToken ct)
    {
        var response = await videoPresentationService.SearchVideosAsync(
            platformQuery: null, nameQuery: query.NameQuery, authorQuery: query.AuthorQuery,
            categoryIds: null,
            user: httpContext.User, userAuthorId: null,
            query,
            sortingOptions: query.SortingOptions, descending: query.Descending,
            ct
        );

        return TypedResults.Ok(new VideoSearchResponseDtoV1
        {
            Videos = response.Result.Select(v => v.MapToVideoSimpleDtoV1(httpContext.GetBaseUrl())),
            PaginationResult = response.PaginationResult,
        });
    }

    private static async Task<Results<
        Ok<VideoSimpleDtoV1>, NotFound<ErrorResponseDto>, ForbidHttpResult
    >> GetVideoAsync(
        [FromRoute] Guid videoId,
        [FromServices] AuthorizationService authorizationService,
        [FromServices] VideoPresentationService videoPresentationService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (!await authorizationService.IsVideoAccessAllowed(videoId, httpContext.User))
        {
            return TypedResults.Forbid();
        }

        var response = await videoPresentationService.GetVideoSimpleAsync(videoId, ct);
        if (response == null)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = $"Video {videoId} not found",
            });
        }

        return TypedResults.Ok(response.MapToVideoSimpleDtoV1(httpContext.GetBaseUrl()));
    }
}