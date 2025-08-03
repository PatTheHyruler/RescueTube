using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Services;
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
    }

    private static async Task<Ok<PlaylistSearchResponseDtoV1>> SearchPlaylistsAsync(
        [FromServices] PlaylistPresentationService playlistPresentationService,
        [FromBody] PlaylistSearchDtoV1 search,
        HttpContext httpContext)
    {
        var response = await playlistPresentationService.SearchPlaylists(
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
}
