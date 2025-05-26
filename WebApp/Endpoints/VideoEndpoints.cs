using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Identity;
using RescueTube.Core.Services;
using WebApp.ApiModels;
using WebApp.ApiModels.Mappers;
using WebApp.Utils;

namespace WebApp.Endpoints;

public static class VideoEndpoints
{
    public static void MapVideoEndpoints(this IEndpointRouteBuilder app)
    {
        var videosGroup = app.MapGroup("videos").WithTags("Videos");

        videosGroup.MapPost("search", SearchVideosAsync).HasApiVersion(1);
        videosGroup.MapGet("{videoId:guid}", GetVideoAsync)
            .AllowAnonymous()
            .HasApiVersion(1);

        videosGroup.MapGet("{videoId:guid}/archival-settings", GetVideoArchivalSettingsAsync)
            .HasApiVersion(1);

        videosGroup.MapPut("{videoId:guid}/archival-settings", UpsertVideoArchivalSettingsAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
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
        if (!await authorizationService.IsVideoAccessAllowedAsync(videoId, httpContext.User, ct))
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

    private static async Task<Results<Ok<VideoArchivalSettingsDtoV1>, NotFound<ErrorResponseDto>>>
        GetVideoArchivalSettingsAsync([FromRoute] Guid videoId, [FromServices] IDataUow dataUow, CancellationToken ct)
    {
        var videoSettings = await dataUow.Ctx.Videos
            .Where(v => v.Id == videoId)
            .Select(v => v.ArchivalSettings)
            .FirstOrDefaultAsync(ct);
        if (videoSettings is null)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = "Video not found",
            });
        }

        return TypedResults.Ok(videoSettings.MapToVideoArchivalSettingsDtoV1());
    }

    private static async Task<Results<Ok, NotFound<ErrorResponseDto>>> UpsertVideoArchivalSettingsAsync(
        [FromRoute] Guid videoId, [FromBody] VideoArchivalSettingsDtoV1 settingsDto,
        [FromServices] IDataUow dataUow, CancellationToken ct)
    {
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
        var video = await dataUow.Ctx.Videos.FirstOrDefaultAsync(v => v.Id == videoId, ct);
        if (video is null)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = "Video not found",
            });
        }

        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        video.ArchivalSettings.ShouldRegularlyFetchVideoData = settingsDto.ShouldRegularlyFetchVideoData;

        await dataUow.SaveChangesAsync(ct);

        return TypedResults.Ok();
    }
}