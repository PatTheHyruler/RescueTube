using System.Linq.Expressions;
using LinqKit;
using RescueTube.Core.Data.Specifications;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.DAL.EF.Specifications;

public class PermissionSpecification : BaseDbService, IPermissionSpecification
{
    public PermissionSpecification(IServiceProvider services) : base(services)
    {
    }

    private EPrivacyStatus[] GetAllowedPrivacyStatuses(bool requireIndexPermission) =>
        requireIndexPermission
            ? [EPrivacyStatus.Public]
            : [EPrivacyStatus.Public, EPrivacyStatus.Unlisted];

    private Expression<Func<TEntity, bool>> IsEntityAccessibleByStatus<TEntity>(bool requireIndexPermission)
        where TEntity : IInternalPrivacyEntity
    {
        var statuses = GetAllowedPrivacyStatuses(requireIndexPermission);
        return e => statuses.Contains(e.PrivacyStatus);
    }

    private Expression<Func<Guid, bool>> IsUserAllowedToAccessPlaylistId(Guid? userId)
    {
        if (userId == null)
        {
            return _ => false;
        }

        return playlistId => Ctx.EntityAccessPermissions.Any(p =>
            p.PlaylistId == playlistId && p.UserId == userId);
    }

    private Expression<Func<Playlist, bool>> IsUserAllowedToAccessPlaylist(Guid? userId)
    {
        if (userId == null)
        {
            return _ => false;
        }

        return pl => IsUserAllowedToAccessPlaylistId(userId).Invoke(pl.Id);
    }

    public Expression<Func<Playlist, bool>> IsUserAllowedToAccessPlaylistOrPlaylistIsPublic(Guid? userId,
        bool requireIndexPermission)
    {
        var expression = PredicateBuilder.New(IsEntityAccessibleByStatus<Playlist>(requireIndexPermission));

        if (userId != null)
        {
            expression = expression.Or(IsUserAllowedToAccessPlaylist(userId));
        }

        return expression;
    }

    public Expression<Func<Video, bool>> IsUserAllowedToAccessVideoOrVideoIsPublic(Guid? userId,
        bool requireIndexPermission)
    {
        var expression = PredicateBuilder.New(IsEntityAccessibleByStatus<Video>(requireIndexPermission));
        if (userId != null)
        {
            expression = expression.Or(v =>
                Ctx.EntityAccessPermissions.Any(p =>
                    p.VideoId == v.Id && p.UserId == userId));

            expression = expression.Or(v => Ctx.PlaylistItems.Any(pv =>
                pv.VideoId == v.Id &&
                IsUserAllowedToAccessPlaylistOrPlaylistIsPublic(userId, true)
                    .Invoke(pv.Playlist!)
            ));
        }

        return expression;
    }
}