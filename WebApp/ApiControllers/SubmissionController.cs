using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core;
using RescueTube.Core.Exceptions;
using RescueTube.Core.Utils;
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
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<ActionResult<LinkSubmissionResponseDtoV1>> Create([FromBody] LinkSubmissionRequestDtoV1 input, CancellationToken ct = default)
    {
        try
        {
            using var transaction = TransactionUtils.NewTransactionScope();
            var submission = await _serviceUow.SubmissionService.SubmitGenericLink(input.Url, User, ct);
            await _serviceUow.SaveChangesAsync(ct);
            transaction.Complete();
            return new LinkSubmissionResponseDtoV1
            {
                SubmissionId = submission.Id,
                Type = submission.EntityType,
                Platform = submission.Platform,
                IdOnPlatform = submission.IdOnPlatform,
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
    }
}