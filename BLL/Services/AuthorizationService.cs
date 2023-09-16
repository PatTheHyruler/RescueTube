using System.Security.Principal;
using BLL.Base;
using BLL.Identity;
using DAL.EF.Extensions;
using Domain.Entities;
using Microsoft.Extensions.Logging;

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

    public static bool IsAllowedToAccessVideoByRole(IPrincipal user) =>
        user.IsInRole(RoleNames.Admin) || user.IsInRole(RoleNames.SuperAdmin);
}