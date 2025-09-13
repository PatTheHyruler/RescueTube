using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Services;
using RescueTube.Core.Utils.Pagination;
using RescueTube.WebApi.ApiModels;
using RescueTube.WebApi.ApiModels.Mappers;
using RescueTube.WebApi.Utils;

namespace RescueTube.WebApi.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("videos/{videoId:guid}/comments", GetCommentsAsync)
            .AllowAnonymous()
            .WithTags("Comments")
            .HasApiVersion(1);
    }

    /// <summary>
    /// Get comments for video.
    /// </summary>
    /// <returns>List of comments on the video, grouped by root comment, including replies.</returns>
    /// <response code="200">Comments fetched successfully.</response>
    /// <response code="403">Permission denied.</response>
    /// <response code="404">Video not found.</response>
    private static async Task<Results<
        Ok<CommentRootsResponseDtoV1>, NotFound<ErrorResponseDto>, ForbidHttpResult
    >> GetCommentsAsync([FromRoute] Guid videoId, [AsParameters] PaginationQuery paginationQuery,
        [FromServices] CommentService commentService, [FromServices] AuthorizationService authorizationService,
        HttpContext httpContext, CancellationToken ct)
    {
        if (!await authorizationService.IsVideoAccessAllowedAsync(videoId, httpContext.User, ct))
        {
            return TypedResults.Forbid();
        }

        var response = await commentService.GetVideoComments(videoId, paginationQuery, ct);
        if (response == null)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = $"Video {videoId} not found",
            });
        }

        return TypedResults.Ok(new CommentRootsResponseDtoV1
        {
            Comments = response.Result.Comments.Select(c => c.MapComment(httpContext.GetBaseUrl())),
            PaginationResult = response.PaginationResult,
        });
    }
}