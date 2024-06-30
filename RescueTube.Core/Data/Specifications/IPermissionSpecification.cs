using System.Linq.Expressions;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IPermissionSpecification
{
    Expression<Func<Playlist, bool>> IsUserAllowedToAccessPlaylistOrPlaylistIsPublic(Guid? userId,
        bool requireIndexPermission);

    Expression<Func<Video, bool>> IsUserAllowedToAccessVideoOrVideoIsPublic(Guid? userId,
        bool requireIndexPermission);
}