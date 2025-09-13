using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Services;
using WebApp.ApiModels;
using WebApp.ApiModels.Mappers;
using WebApp.ApiModels.Playlists;
using WebApp.Utils;

namespace WebApp.Endpoints;

public static class PlaylistEndpoints
{
    public static void MapPlaylistEndpoints(this IEndpointRouteBuilder app)
    {
        var playlistsGroup = app.MapGroup("playlists").WithTags("playlists");

        playlistsGroup.MapPost("search", SearchPlaylistsAsync).HasApiVersion(1);

        playlistsGroup.MapGet("{playlistId:guid}", GetPlaylistAsync)
            .HasApiVersion(1);

        playlistsGroup.MapGet("{playlistId:guid}/items", GetPlaylistItemsAsync)
            .HasApiVersion(1);
    }

    private static async Task<Ok<PlaylistSearchResponseDtoV1>> SearchPlaylistsAsync(
        [FromServices] PlaylistPresentationService playlistPresentationService,
        [FromBody] PlaylistSearchDtoV1 search,
        HttpContext httpContext)
    {
        var response = await playlistPresentationService.SearchPlaylistsAsync(
            filter: new PlaylistPresentationService.PlaylistSearchParams
            {
                Name = search.Filter?.Name,
            },
            user: httpContext.User,
            paginationQuery: search);

        return TypedResults.Ok(new PlaylistSearchResponseDtoV1
        {
            Playlists = response.Result.Select(p => p.MapToPlaylistSimpleDtoV1(httpContext.GetBaseUrl())),
            PaginationResult = response.PaginationResult,
        });
    }

    private static async Task<Results<
        Ok<PlaylistSimpleDtoV1>,
        NotFound<ErrorResponseDto>
    >> GetPlaylistAsync(
        [FromServices] PlaylistPresentationService playlistPresentationService,
        [FromRoute] Guid playlistId,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var response = await playlistPresentationService.GetPlaylistByIdAsync(playlistId, ct);
        if (response is null)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = $"Playlist {playlistId} not found",
            });
        }

        // TODO: Separate detail DTO for this
        return TypedResults.Ok(response.MapToPlaylistSimpleDtoV1(httpContext.GetBaseUrl()));
    }

    private static async Task<Results<
        Ok<PlaylistItemsResponseDtoV1>,
        ForbidHttpResult
    >> GetPlaylistItemsAsync(
        [FromServices] PlaylistPresentationService playlistPresentationService,
        [FromRoute] Guid playlistId,
        [AsParameters] PaginationRequestOptionalDtoV1 paginationQuery,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var response = await playlistPresentationService.GetPlaylistItemsAsync(playlistId, paginationQuery, ct);

        return TypedResults.Ok(new PlaylistItemsResponseDtoV1
        {
            PlaylistItems = response.Result.Select(p => p.MapToPlaylistItemDtoV1(httpContext.GetBaseUrl())),
            PaginationResult = response.PaginationResult,
        });
    }
}
