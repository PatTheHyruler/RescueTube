using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core;
using RescueTube.Core.Exceptions;
using Swashbuckle.AspNetCore.Annotations;
using WebApp.ApiModels;

namespace WebApp.ApiControllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/submissions/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SubmissionController : ControllerBase
{
    private readonly ServiceUow _serviceUow;

    public SubmissionController(ServiceUow serviceUow)
    {
        _serviceUow = serviceUow;
    }

    /// <summary>
    /// Submit a URL to the archive.
    /// </summary>
    /// <returns>Details about the created submission, or error details if submission failed.</returns>
    /// <response code="200">Submission created successfully.</response>
    /// <response code="400">Submitted URL not recognized as a supported URL for archiving.</response>
    /// <response code="404">Entity matching submitted URL not found on platform</response>
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpPost]
    public async Task<ActionResult<LinkSubmissionResponseDtoV1>> Create([FromBody] LinkSubmissionRequestDtoV1 input, CancellationToken ct = default)
    {
        try
        {
            var successResult = await _serviceUow.SubmissionService.SubmitGenericLinkAsync(input.Url, User, ct);
            await _serviceUow.SaveChangesAsync(ct);
            return new LinkSubmissionResponseDtoV1
            {
                SubmissionId = successResult.SubmissionId,
                Type = successResult.Type,
                Platform = successResult.Platform,
                IdOnPlatform = successResult.IdOnPlatform,
                AlreadyAdded = successResult.AlreadyAdded,
                EntityId = successResult.EntityId,
            };
        }
        catch (UnrecognizedUrlException e)
        {
            return BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.UnrecognizedUrl,
                Message = e.Message,
                Details = new
                {
                    input.Url,
                },
            });
        }
        catch (VideoNotFoundOnPlatformException e)
        {
            return NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.SubmissionEntityNotFound,
                Message = e.Message,
                Details =
                new {
                    input.Url,
                },
            });
        }
    }
}