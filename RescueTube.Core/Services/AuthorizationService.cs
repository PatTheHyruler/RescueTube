using System.Security.Claims;
using System.Security.Principal;
using RescueTube.Core.Data.Extensions;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Identity.Services;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Base;
using RescueTube.Core.Identity;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class AuthorizationService : BaseService
{
    public AuthorizationService(IServiceProvider services, ILogger<AuthorizationService> logger) : base(services, logger)
    {
    }

    public async Task AuthorizeVideoIfNotAuthorized(Guid userId, Guid videoId)
    {
        if (await DbCtx.EntityAccessPermissions.VideoPermissionExistsAsync(userId, videoId)) return;
        DbCtx.EntityAccessPermissions.Add(new EntityAccessPermission
        {
            UserId = userId,
            VideoId = videoId,
        });
    }

    public static bool IsAllowedToAccessVideoByRole(IPrincipal? user) {
        if (user == null)
        {
            return false;
        }
        return user.IsInRole(RoleNames.Admin) || user.IsInRole(RoleNames.SuperAdmin);
    }

    public async Task<bool> IsAllowedToAccessVideo(ClaimsPrincipal? user, Guid videoId)
    {
        if (IsAllowedToAccessVideoByRole(user))
        {
            return true;
        }

        var videos = DbCtx.Videos.Where(v => v.Id == videoId);
        videos = DataUow.VideoRepo.WhereUserIsAllowedToAccessVideoOrVideoIsPublic(videos, user?.GetUserIdIfExists());
        return await videos.AnyAsync();
    }
}