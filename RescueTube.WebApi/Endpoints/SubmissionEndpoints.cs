using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Pagination;
using RescueTube.Core.Exceptions;
using RescueTube.Core.Identity;
using RescueTube.Core.Identity.Services;
using RescueTube.Core.Services;
using RescueTube.Core.Utils.Pagination;
using RescueTube.Domain.Entities;
using RescueTube.WebApi.ApiModels;
using RescueTube.WebApi.ApiModels.Mappers;

namespace RescueTube.WebApi.Endpoints;

public static class SubmissionEndpoints
{
    public static void MapSubmissionEndpoints(this IEndpointRouteBuilder app)
    {
        var submissionsGroup = app.MapGroup("submissions").WithTags("Submissions");

        submissionsGroup.MapPost("create", CreateSubmissionAsync).HasApiVersion(1);
        submissionsGroup.MapGet("", GetSubmissionsAsync).HasApiVersion(1);
        submissionsGroup.MapPost("{submissionId:guid}/handle", HandleSubmissionAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
            .HasApiVersion(1);
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
            var submission = serviceUow.SubmissionService.SubmitGenericLink(input.Url, user);
            await serviceUow.SaveChangesAsync(ct);

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

    private static async Task<Ok<SubmissionSearchResponseDtoV1>> GetSubmissionsAsync(
        [FromServices] AppDbContext dbContext,
        [AsParameters] SubmissionSearchDtoV1 request,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct)
    {
        var isAdmin = claimsPrincipal.IsAdmin();
        var userId = claimsPrincipal.GetUserId();

        var paginationQuery = request.ToClamped();

        var submissionsQuery = dbContext.Submissions
            .Include(s => s.AddedBy)
            .Include(s => s.ApprovedBy)
            .Include(s => s.Failures!.OrderByDescending(f => f.OccurredAt))
            .Where(s => isAdmin || s.AddedById == userId)
            .Where(s => request.Completed == null || s.CompletedAt.HasValue == request.Completed);

        var count = await submissionsQuery.CountAsync(ct);

        var submissions = await submissionsQuery
            .OrderByWithConfiguration(request.OrderBy?.Value, SubmissionOrderingConfig)
            .Paginate(paginationQuery)
            .ToArrayAsync(ct);

        var result = new SubmissionSearchResponseDtoV1
        {
            PaginationResult = new PaginationResultDtoV1
            {
                AmountOnPage = submissions.Length,
                Limit = paginationQuery.Limit,
                Page = paginationQuery.Page,
                TotalResults = count,
            },
            Results = submissions.Select(ApiMapper.MapToSubmissionDtoV1),
        };

        return TypedResults.Ok(result);
    }

    private static readonly OrderingConfiguration<Submission> SubmissionOrderingConfig = new()
    {
        DefaultOrdering =
        [
            new OrderByPropertyDtoV1("addedAt", true),
            new OrderByPropertyDtoV1("id", true)
        ],
        RequiredProperty = new OrderByPropertyDtoV1("id", true),
        PropertyMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = static s => s.Id,
            ["addedAt"] = static s => s.AddedAt,
            ["approvedAt"] = static s => s.ApprovedAt ?? DateTimeOffset.MaxValue,
            ["completedAt"] = static s => s.CompletedAt ?? DateTimeOffset.MaxValue,
            ["platform"] = static s => s.Platform,
            ["entityType"] = static s => s.EntityType,
        }
    };

    private static async Task<Results<
        Ok,
        NotFound<ErrorResponseDto>,
        BadRequest<ErrorResponseDto>,
        InternalServerError<ErrorResponseDto>
    >> HandleSubmissionAsync(
        [FromRoute] Guid submissionId,
        ClaimsPrincipal claimsPrincipal,
        [FromServices] SubmissionService submissionService,
        [FromServices] AppDbContext dbContext,
        [FromServices] TimeProvider timeProvider,
        CancellationToken ct)
    {
        var userId = claimsPrincipal.GetUserId();

        var submission = await dbContext.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId, ct);

        if (submission is null)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = $"Submission {submissionId} not found",
            });
        }

        if (submission.CompletedAt is not null)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.GenericError,
                Message = $"Submission {submissionId} has already been completed at {submission.CompletedAt}",
            });
        }

        if (submission.ApprovedAt is null)
        {
            submission.ApprovedById = userId;
            submission.ApprovedAt = timeProvider.GetUtcNow();
        }

        var result = await submissionService.HandleSubmissionAsync(submission, ct);
        if (result.Success)
        {
            return TypedResults.Ok();
        }

        return TypedResults.InternalServerError(new ErrorResponseDto
        {
            ErrorType = EErrorType.GenericError,
            Message = $"Handling submission failed: {result.Error.Reason}",
        });
    }
}