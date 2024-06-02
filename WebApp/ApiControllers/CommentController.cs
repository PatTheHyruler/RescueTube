using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Services;
using RescueTube.Core.Utils.Pagination;
using Swashbuckle.AspNetCore.Annotations;
using WebApp.ApiModels;
using WebApp.ApiModels.Mappers;
using WebApp.Utils;

namespace WebApp.ApiControllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/comments")]
public class CommentController : ControllerBase
{
    private readonly CommentService _commentService;
    private readonly AuthorizationService _authorizationService;

    public CommentController(CommentService commentService, AuthorizationService authorizationService)
    {
        _commentService = commentService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Get comments for video.
    /// </summary>
    /// <returns>List of comments on the video, grouped by root comment, including replies.</returns>
    /// <response code="200">Comments fetched successfully.</response>
    /// <response code="403">Permission denied.</response>
    /// <response code="404">Video not found.</response>
    [SwaggerErrorResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status403Forbidden)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(CommentRootsResponseDtoV1))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [AllowAnonymous]
    [HttpGet("~/api/v{version:apiVersion}/videos/{videoId:guid}/comments")]
    public async Task<ActionResult<CommentRootsResponseDtoV1>> CommentsSearch(
        [FromRoute] Guid videoId,
        [FromQuery] PaginationQuery paginationQuery)
    {
        if (!await _authorizationService.IsVideoAccessAllowed(videoId, User))
        {
            return Forbid();
        }

        var response = await _commentService.GetVideoComments(videoId, paginationQuery);
        if (response == null)
        {
            return NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = $"Video {videoId} not found",
            });
        }

        return Ok(new CommentRootsResponseDtoV1
        {
            Comments = response.Result.Comments.Select(c => c.MapComment(HttpContext.GetBaseUrl())),
            PaginationResult = response.PaginationResult,
        });
    }
}