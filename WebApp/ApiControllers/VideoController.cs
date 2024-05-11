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
[Route("api/v{version:apiVersion}/videos/[action]")]
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
    [HttpPost]
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
}