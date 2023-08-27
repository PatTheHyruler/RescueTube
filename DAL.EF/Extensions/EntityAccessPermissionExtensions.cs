using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF.Extensions;

public static class EntityAccessPermissionExtensions
{
    public static Task<bool> VideoPermissionExistsAsync(this DbSet<EntityAccessPermission> dbSet,
        Guid userId, Guid videoId) =>
        dbSet.AnyAsync(e => e.UserId == userId && e.VideoId == videoId);
}