using System.Security.Claims;
using System.Security.Principal;
using BLL.Base;
using BLL.Contracts;
using BLL.Contracts.Exceptions;
using BLL.DTO.Entities;
using BLL.Identity;
using BLL.Identity.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class SubmissionService : BaseService
{
    private IEnumerable<IPlatformSubmissionHandler> SubmissionHandlers =>
        Services.GetRequiredService<IEnumerable<IPlatformSubmissionHandler>>();

    private static bool IsAllowedToAutoSubmit(IPrincipal user)
    {
        return RoleNames.AllowedToAutoSubmitRolesList.Any(user.IsInRole);
    }

    /// <exception cref="UnrecognizedUrlException">URL was not recognized and can't be archived.</exception>
    public Task<LinkSubmissionSuccessResult> SubmitGenericLinkAsync(string url, ClaimsPrincipal user)
    {
        return SubmitGenericLinkAsync(url, user.GetUserId(), IsAllowedToAutoSubmit(user));
    }

    private async Task<LinkSubmissionSuccessResult> SubmitGenericLinkAsync(string url, Guid submitterId, bool autoSubmit)
    {
        foreach (var submissionHandler in SubmissionHandlers)
        {
            if (submissionHandler.IsPlatformUrl(url))
            {
                return await submissionHandler.SubmitLink(url, submitterId, autoSubmit);
            }
        }

        throw new UnrecognizedUrlException(url);
    }

    public async Task<Submission> Add(Video video, Guid submitterId, bool autoSubmit)
    {
        var submission = Ctx.Submissions.Add(new Submission(video, submitterId, autoSubmit));
        if (autoSubmit) await ServiceUow.AuthorizationService.AuthorizeVideoIfNotAuthorized(submitterId, video.Id);
        return submission.Entity;
    }

    public Submission Add(string idOnPlatform, EPlatform platform, EEntityType entityType, Guid submitterId,
        bool autoSubmit)
    {
        return Ctx.Submissions.Add(new Submission(idOnPlatform, platform, entityType, submitterId, autoSubmit)).Entity;
    }

    public SubmissionService(IServiceProvider services, ILogger<SubmissionService> logger) : base(services, logger)
    {
    }
}