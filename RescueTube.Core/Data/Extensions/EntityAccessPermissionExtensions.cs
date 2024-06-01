using Microsoft.EntityFrameworkCore;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Extensions;

public static class EntityAccessPermissionExtensions
{
    public static Task<bool> VideoPermissionExistsAsync(this DbSet<EntityAccessPermission> dbSet,
        Guid userId, Guid videoId, CancellationToken ct = default) =>
        dbSet.AnyAsync(e => e.UserId == userId && e.VideoId == videoId, ct);
}