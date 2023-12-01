using System.Security.Claims;
using System.Security.Principal;
using BLL.Base;
using BLL.Identity;
using DAL.EF.Extensions;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using BLL.Identity.Services;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class AuthorizationService : BaseService
{
    public AuthorizationService(IServiceProvider services, ILogger<AuthorizationService> logger) : base(services, logger)
    {
    }

    public async Task AuthorizeVideoIfNotAuthorized(Guid userId, Guid videoId)
    {
        if (await Ctx.EntityAccessPermissions.VideoPermissionExistsAsync(userId, videoId)) return;
        Ctx.EntityAccessPermissions.Add(new EntityAccessPermission
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

        return await Ctx.Videos
            .Where(v => v.Id == videoId)
            .WhereUserIsAllowedToAccessVideoOrVideoIsPublic(
                Ctx, user?.GetUserId())
            .AnyAsync();
    }
}