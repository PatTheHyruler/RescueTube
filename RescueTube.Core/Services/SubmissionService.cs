using System.Security.Claims;
using System.Security.Principal;
using MediatR;
using RescueTube.Core.Identity.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Contracts;
using RescueTube.Core.Events.Events;
using RescueTube.Core.Exceptions;
using RescueTube.Core.Identity;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class SubmissionService : BaseService
{
    private readonly IMediator _mediator;

    public SubmissionService(IServiceProvider services, ILogger<SubmissionService> logger, IMediator mediator)
        : base(services, logger)
    {
        _mediator = mediator;
    }

    private IEnumerable<IPlatformSubmissionHandler> SubmissionHandlers =>
        Services.GetRequiredService<IEnumerable<IPlatformSubmissionHandler>>();

    private static bool IsAllowedToAutoSubmit(IPrincipal user)
    {
        return RoleNames.AllowedToAutoSubmitRolesList.Any(user.IsInRole);
    }

    /// <exception cref="UnrecognizedUrlException">URL was not recognized and can't be archived.</exception>
    public Submission SubmitGenericLink(
        string url, ClaimsPrincipal user)
    {
        return SubmitGenericLink(url, user.GetUserId(), IsAllowedToAutoSubmit(user));
    }

    private Submission SubmitGenericLink(
        string url, Guid submitterId, bool autoSubmit)
    {
        foreach (var submissionHandler in SubmissionHandlers)
        {
            if (!submissionHandler.IsPlatformUrl(url, out var recognizedPlatformUrl)) continue;

            var submission = new Submission(recognizedPlatformUrl.IdOnPlatform, recognizedPlatformUrl.Platform,
                recognizedPlatformUrl.EntityType, submitterId, autoSubmit);
            DbCtx.Submissions.Add(submission);
            DataUow.RegisterSavedChangesCallbackRunOnce(() => _mediator.Publish(new SubmissionAddedEvent
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
}