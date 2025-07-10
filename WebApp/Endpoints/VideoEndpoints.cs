using System.Linq.Expressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Identity;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
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

        videosGroup.MapPatch("archival-settings/bulk", UpsertVideoArchivalSettingsBulkAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
            .HasApiVersion(1);
    }

    private static async Task<Ok<VideoSearchResponseDtoV1>> SearchVideosAsync(
        [FromBody] VideoSearchDtoV1 query, [FromServices] VideoPresentationService videoPresentationService,
        HttpContext httpContext, CancellationToken ct)
    {
        var response = await videoPresentationService.SearchVideosAsync(
            filter: query.Filter.MapToCoreVideoFilter(),
            user: httpContext.User,
            paginationQuery: query,
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

    private static async Task<Ok<int>> UpsertVideoArchivalSettingsBulkAsync(
        [FromBody] VideoArchivalSettingsBulkUpdateDtoV1 updateDto,
        [FromServices] IDataUow dataUow,
        CancellationToken ct)
    {
        Expression<Func<Video, bool>> videoIdFilter = updateDto switch
        {
            { SelectAll: true, VideoIds.Length: > 0 } => v => !updateDto.VideoIds.Contains(v.Id),
            { SelectAll: true, VideoIds: null or { Length: 0 } } => static v => true,
            { SelectAll: false, VideoIds.Length: > 0 } => v => updateDto.VideoIds.Contains(v.Id),
            { SelectAll: false, VideoIds: null or { Length: 0 } } => static v => false,
        };
        var videosQuery = dataUow.Ctx.Videos
            .Where(videoIdFilter);

        if (updateDto.Filter is not null)
        {
            videosQuery = videosQuery.Where(dataUow.Videos.FilterVideos(updateDto.Filter.MapToCoreVideoFilter()));
        }

        var updatedAmount = 0;
        await foreach (var video in videosQuery.AsAsyncEnumerable().WithCancellation(ct))
        {
            updatedAmount++;
            if (updateDto.Settings.ShouldRegularlyFetchVideoData.HasValue)
            {
                video.ArchivalSettings.ShouldRegularlyFetchVideoData = updateDto.Settings.ShouldRegularlyFetchVideoData.Value;
            }
        }

        await dataUow.SaveChangesAsync(ct);
        // TODO: Use ExecuteUpdate when complex filter bug is solved - https://github.com/npgsql/efcore.pg/issues/3573 https://github.com/dotnet/efcore/issues/36336
        // var updatedAmount = await videosQuery
        //     .ExecuteUpdateAsync(x =>
        //         x.SetProperty(
        //             static v => v.ArchivalSettings.ShouldRegularlyFetchVideoData,
        //             v => updateDto.Settings.ShouldRegularlyFetchVideoData.HasValue
        //                 ? updateDto.Settings.ShouldRegularlyFetchVideoData.Value
        //                 : v.ArchivalSettings.ShouldRegularlyFetchVideoData),
        //         ct);
        return TypedResults.Ok(updatedAmount);
    }
}