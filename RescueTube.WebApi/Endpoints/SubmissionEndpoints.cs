using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core;
using RescueTube.Core.Exceptions;
using RescueTube.Core.Utils;
using RescueTube.WebApi.ApiModels;

namespace RescueTube.WebApi.Endpoints;

public static class SubmissionEndpoints
{
    public static void MapSubmissionEndpoints(this IEndpointRouteBuilder app)
    {
        var optionsGroup = app.MapGroup("submissions").WithTags("Submissions");

        optionsGroup.MapPost("create", CreateSubmissionAsync).HasApiVersion(1);
    }

    /// <summary>
    /// Submit a URL to the archive.
    /// </summary>
    /// <returns>Details about the created submission, or error details if submission failed.</returns>
    /// <response code="200">Submission created successfully.</response>
    /// <response code="400">Submitted URL not recognized as a supported URL for archiving.</response>
    private static async Task<Results<Ok<LinkSubmissionResponseDtoV1>, BadRequest<ErrorResponseDto>>> CreateSubmissionAsync(
        [FromBody] LinkSubmissionRequestDtoV1 input, [FromServices] ServiceUow serviceUow,
        ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            using var transaction = TransactionUtils.NewTransactionScope();
            var submission = await serviceUow.SubmissionService.SubmitGenericLinkAsync(input.Url, user, ct);
            await serviceUow.SaveChangesAsync(ct);
            transaction.Complete();

            return TypedResults.Ok(new LinkSubmissionResponseDtoV1
            {
                SubmissionId = submission.Id,
                Type = submission.EntityType,
                Platform = submission.Platform,
                IdOnPlatform = submission.IdOnPlatform,
            });
        }
        catch (UnrecognizedUrlException e)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
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