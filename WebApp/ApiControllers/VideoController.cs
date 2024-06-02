using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core;
using Swashbuckle.AspNetCore.Annotations;
using WebApp.ApiModels;
using WebApp.ApiModels.Mappers;
using WebApp.Utils;

namespace WebApp.ApiControllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/videos")]
public class VideoController : ControllerBase
{
    private readonly ServiceUow _serviceUow;

    public VideoController(ServiceUow serviceUow)
    {
        _serviceUow = serviceUow;
    }

    /// <summary>
    /// Search for videos
    /// </summary>
    /// <param name="query">Filters, pagination, ordering</param>
    /// <response code="200">List of videos matching the query</response>
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(VideoSearchResponseDtoV1))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("[action]")]
    public async Task<ActionResult> Search([FromBody] VideoSearchDtoV1 query)
    {
        var response = await _serviceUow.VideoPresentationService.SearchVideosAsync(
            platformQuery: null, nameQuery: query.NameQuery, authorQuery: query.AuthorQuery,
            categoryIds: null,
            user: User, userAuthorId: null,
            query,
            sortingOptions: query.SortingOptions, descending: query.Descending
        );

        return Ok(new VideoSearchResponseDtoV1
        {
            Videos = response.Result.Select(v => v.MapToVideoSimpleDtoV1(HttpContext.GetBaseUrl())),
            PaginationResult = response.PaginationResult,
        });
    }

    /// <summary>
    /// Get video data.
    /// </summary>
    /// <param name="videoId">Video ID</param>
    /// <returns>Video data</returns>
    /// <response code="200">Video details fetched successfully.</response>
    /// <response code="403">Access to video forbidden.</response>
    /// <response code="404">Video not found.</response>
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(VideoSimpleDtoV1))]
    [SwaggerResponse(StatusCodes.Status403Forbidden)]
    [SwaggerErrorResponse(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [AllowAnonymous]
    [HttpGet("{videoId:guid}")]
    public async Task<ActionResult<VideoSimpleDtoV1>> Index([FromRoute] Guid videoId)
    {
        if (!await _serviceUow.AuthorizationService.IsVideoAccessAllowed(videoId, User))
        {
            return Forbid();
        }

        var response = await _serviceUow.VideoPresentationService.GetVideoSimple(videoId);
        if (response == null)
        {
            return NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = $"Video {videoId} not found",
            });
        }

        return Ok(response.MapToVideoSimpleDtoV1(HttpContext.GetBaseUrl()));
    }
}