using System.Security.Claims;
using MediatR;
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
    private readonly IMediator _mediator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SubmissionService> _logger;

    public SubmissionService(AppDbContext dbContext, IEnumerable<IPlatformSubmissionHandler> submissionHandlers, IMediator mediator, TimeProvider timeProvider, ILogger<SubmissionService> logger)
    {
        _dbContext = dbContext;
        _submissionHandlers = submissionHandlers;
        _mediator = mediator;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <exception cref="UnrecognizedUrlException">URL was not recognized and can't be archived.</exception>
    public Submission SubmitGenericLink(string url, ClaimsPrincipal user)
    {
        return SubmitGenericLink(url, user.GetUserId(), autoSubmit: true);
    }

    private Submission SubmitGenericLink(string url, Guid submitterId, bool autoSubmit)
    {
        foreach (var submissionHandler in _submissionHandlers)
        {
            if (!submissionHandler.IsPlatformUrl(url, out var recognizedPlatformUrl))
            {
                continue;
            }

            var submission = new Submission(recognizedPlatformUrl, submitterId, autoSubmit);
            _dbContext.Submissions.Add(submission);
            _dbContext.RegisterSavedChangesCallbackRunOnce(() => _mediator.Publish(new SubmissionAddedEvent
            {
                EntityType = submission.EntityType,
                Platform = submission.Platform,
                SubmissionId = submission.Id,
                AutoSubmit = autoSubmit,
            }));
            return submission;
        }

        throw new UnrecognizedUrlException(url);
    }

    public async Task HandleSubmissionAsync(Submission submission, CancellationToken ct)
    {
        var submissionHandler = _submissionHandlers.FirstOrDefault(x => x.Platform == submission.Platform);
        if (submissionHandler is null)
        {
            _dbContext.SubmissionHandlingFailures.Add(new SubmissionHandlingFailure
            {
                Submission = submission,
                SubmissionId = submission.Id,
                OccurredAt = _timeProvider.GetUtcNow(),
                Reason = $"No submission handler for platform {submission.Platform}",
            });

            return;
        }

        try
        {
            await submissionHandler.HandleSubmissionAsync(submission, ct);
            submission.CompletedAt = _timeProvider.GetUtcNow();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred handling submission {SubmissionId}", submission.Id);
            _dbContext.SubmissionHandlingFailures.Add(new SubmissionHandlingFailure
            {
                Submission = submission,
                SubmissionId = submission.Id,
                OccurredAt = _timeProvider.GetUtcNow(),
                Reason = $"{e.GetType().FullName}: '{e.Message}'",
            });
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}