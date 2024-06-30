using System.Security.Claims;
using System.Security.Principal;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Identity.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Contracts;
using RescueTube.Core.Events;
using RescueTube.Core.Exceptions;
using RescueTube.Core.Identity;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

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

    public async Task SubmissionAddEntityAccessPermissionAsync(Guid submissionId, CancellationToken ct = default)
    {
        var submission = await DbCtx.Submissions
            .FirstOrDefaultAsync(e => e.Id == submissionId, cancellationToken: ct);

        submission = submission switch
        {
            null => throw new ApplicationException($"Submission {submissionId} not found"),
            { ApprovedAt: null } => throw new ApplicationException("Submission not approved"),
            _ => submission,
        };

        switch (submission.EntityType)
        {
            case EEntityType.Video:
                await ServiceUow.AuthorizationService.AuthorizeVideoIfNotAuthorized(
                    submission.AddedById,
                    submission.VideoId.AssertNotNull(), ct);
                break;
            case EEntityType.Playlist:
                await ServiceUow.AuthorizationService.AuthorizePlaylistIfNotAuthorized(
                    submission.AddedById,
                    submission.PlaylistId.AssertNotNull(), ct);
                break;
            default:
                throw new ApplicationException($"Unsupported entity type {submission.EntityType}");
        }
    }
}