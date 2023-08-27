using BLL.Base;
using DAL.EF.Extensions;
using Domain.Entities;

namespace BLL.Services;

public class AuthorizationService : BaseService
{
    public AuthorizationService(IServiceProvider services) : base(services)
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
}