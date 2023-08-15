using System.Security.Claims;
using System.Security.Principal;
using BLL.Contracts;
using BLL.Contracts.Exceptions;
using BLL.DTO.Entities;
using BLL.Identity;
using BLL.Identity.Services;

namespace BLL.Services;

public class SubmissionService
{
    private readonly IEnumerable<IPlatformSubmissionHandler> _submissionHandlers;

    private static readonly string[] AllowedToAutoSubmitRoles = {
        RoleNames.SuperAdmin,
        RoleNames.Admin,
    };

    public SubmissionService(IEnumerable<IPlatformSubmissionHandler> submissionHandlers)
    {
        _submissionHandlers = submissionHandlers;
    }

    private static bool IsAllowedToAutoSubmit(IPrincipal user)
    {
        return AllowedToAutoSubmitRoles.Any(user.IsInRole);
    }

    public Task<LinkSubmissionResult> SubmitGenericLinkAsync(string url, ClaimsPrincipal user)
    {
        return SubmitGenericLinkAsync(url, user.GetUserId(), IsAllowedToAutoSubmit(user));
    }

    private async Task<LinkSubmissionResult> SubmitGenericLinkAsync(string url, Guid submitterId, bool autoSubmit)
    {
        foreach (var submissionHandler in _submissionHandlers)
        {
            if (submissionHandler.IsPlatformUrl(url))
            {
                return await submissionHandler.SubmitLink(url, submitterId, autoSubmit);
            }
        }

        throw new UnrecognizedUrlException(url);
    }
}