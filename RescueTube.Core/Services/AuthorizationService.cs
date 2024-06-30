using System.Security.Claims;
using System.Security.Principal;
using LinqKit;
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
    public AuthorizationService(IServiceProvider services, ILogger<AuthorizationService> logger) : base(services,
        logger)
    {
    }

    public async Task AuthorizeVideoIfNotAuthorized(Guid userId, Guid videoId, CancellationToken ct = default)
    {
        if (!await DbCtx.EntityAccessPermissions.VideoPermissionExistsAsync(userId, videoId, ct))
        {
            DbCtx.EntityAccessPermissions.Add(new EntityAccessPermission
            {
                UserId = userId,
                VideoId = videoId,
            });
        }
    }

    public async Task AuthorizePlaylistIfNotAuthorized(Guid userId, Guid playlistId, CancellationToken ct = default)
    {
        if (!await DbCtx.EntityAccessPermissions.PlaylistPermissionExistsAsync(userId, playlistId, ct))
        {
            DbCtx.EntityAccessPermissions.Add(new EntityAccessPermission
            {
                UserId = userId,
                PlaylistId = playlistId,
            });
        }
    }

    public static bool IsAllowedToAccessAnyContentByRole(IPrincipal? user)
    {
        if (user == null)
        {
            return false;
        }

        return user.IsInRole(RoleNames.Admin) || user.IsInRole(RoleNames.SuperAdmin);
    }

    public async Task<bool> IsVideoAccessAllowed(Guid videoId, ClaimsPrincipal? user = null)
    {
        if (IsAllowedToAccessAnyContentByRole(user))
        {
            return true;
        }

        var userId = user?.GetUserIdIfExists();
        var videos = DbCtx.Videos
            .Where(v => v.Id == videoId)
            .AsExpandable().Where(DataUow.Permissions.IsUserAllowedToAccessVideoOrVideoIsPublic(userId, false));
        return await videos.AnyAsync();
    }
}