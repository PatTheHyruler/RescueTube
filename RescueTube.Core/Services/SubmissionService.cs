using System.Security.Claims;
using System.Security.Principal;
using RescueTube.Core.Identity.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Contracts;
using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Exceptions;
using RescueTube.Core.Identity;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Services;

public class SubmissionService : BaseService
{
    private IEnumerable<IPlatformSubmissionHandler> SubmissionHandlers =>
        Services.GetRequiredService<IEnumerable<IPlatformSubmissionHandler>>();

    private static bool IsAllowedToAutoSubmit(IPrincipal user)
    {
        return RoleNames.AllowedToAutoSubmitRolesList.Any(user.IsInRole);
    }

    /// <exception cref="UnrecognizedUrlException">URL was not recognized and can't be archived.</exception>
    /// <exception cref="VideoNotFoundOnPlatformException">URL was not recognized and can't be archived.</exception>
    public Task<LinkSubmissionSuccessResult> SubmitGenericLinkAsync(
        string url, ClaimsPrincipal user, CancellationToken ct = default)
    {
        return SubmitGenericLinkAsync(url, user.GetUserId(), IsAllowedToAutoSubmit(user), ct);
    }

    private async Task<LinkSubmissionSuccessResult> SubmitGenericLinkAsync(
        string url, Guid submitterId, bool autoSubmit, CancellationToken ct = default)
    {
        foreach (var submissionHandler in SubmissionHandlers)
        {
            if (submissionHandler.IsPlatformUrl(url))
            {
                return await submissionHandler.SubmitLink(url, submitterId, autoSubmit, ct);
            }
        }

        throw new UnrecognizedUrlException(url);
    }

    public async Task<Submission> Add(Video video, Guid submitterId, bool autoSubmit, CancellationToken ct = default)
    {
        var submission = DbCtx.Submissions.Add(new Submission(video, submitterId, autoSubmit));
        if (autoSubmit) await ServiceUow.AuthorizationService.AuthorizeVideoIfNotAuthorized(submitterId, video.Id, ct);
        return submission.Entity;
    }

    public Submission Add(string idOnPlatform, EPlatform platform, EEntityType entityType, Guid submitterId,
        bool autoSubmit)
    {
        return DbCtx.Submissions.Add(new Submission(idOnPlatform, platform, entityType, submitterId, autoSubmit)).Entity;
    }

    public SubmissionService(IServiceProvider services, ILogger<SubmissionService> logger) : base(services, logger)
    {
    }
}