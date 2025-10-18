using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.Core.Events;
using RescueTube.Core.Exceptions;
using RescueTube.Core.Identity.Services;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class SubmissionService
{
    private readonly AppDbContext _dbContext;
    private readonly IEnumerable<IPlatformSubmissionHandler> _submissionHandlers;
    private readonly ILogger<SubmissionService> _logger;
    private readonly IMediator _mediator;
    private readonly TimeProvider _timeProvider;

    public SubmissionService(AppDbContext dbContext, IEnumerable<IPlatformSubmissionHandler> submissionHandlers, ILogger<SubmissionService> logger, IMediator mediator, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _submissionHandlers = submissionHandlers;
        _logger = logger;
        _mediator = mediator;
        _timeProvider = timeProvider;
    }

    /// <exception cref="UnrecognizedUrlException">URL was not recognized and can't be archived.</exception>
    public async Task<Submission> SubmitGenericLinkAsync(
        string url, ClaimsPrincipal user, CancellationToken ct = default)
    {
        return await SubmitGenericLinkAsync(url, user.GetUserId(), autoSubmit: true, ct);
    }

    private async Task<Submission> SubmitGenericLinkAsync(
        string url, Guid submitterId, bool autoSubmit, CancellationToken ct = default)
    {
        foreach (var submissionHandler in _submissionHandlers)
        {
            if (!submissionHandler.IsPlatformUrl(url, out var recognizedPlatformUrl))
            {
                continue;
            }

            var submission = new Submission(recognizedPlatformUrl, submitterId, autoSubmit);
            _dbContext.Submissions.Add(submission);
            await _mediator.Publish(new SubmissionAddedEvent
            {
                EntityType = submission.EntityType,
                Platform = submission.Platform,
                SubmissionId = submission.Id,
                AutoSubmit = autoSubmit,
            }, ct);
            return submission;
        }

        throw new UnrecognizedUrlException(url);
    }

    public async Task HandleSubmissionAsync(Guid submissionId, CancellationToken ct)
    {
        var submission = await _dbContext.Submissions
            .Where(s => s.Id == submissionId)
            .FirstAsync(cancellationToken: ct);

        if (submission.ApprovedAt is null)
        {
            throw new InvalidOperationException($"Submission {submissionId} not approved");
        }

        if (submission.CompletedAt != null)
        {
            _logger.LogInformation("Submission {SubmissionId} already handled at {CompletedAt}, skipping",
                submissionId, submission.CompletedAt);
            return;
        }

        var submissionHandler = _submissionHandlers.FirstOrDefault(x => x.Platform == submission.Platform);
        if (submissionHandler is null)
        {
            throw new NotSupportedException($"No submission handler for platform {submission.Platform}");
        }

        await submissionHandler.HandleSubmissionAsync(submission, ct);

        submission.CompletedAt = _timeProvider.GetUtcNow();

        await _dbContext.SaveChangesAsync(ct);
    }
}